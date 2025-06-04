using System.Net;
using System.Text;

namespace HikvisionHelpers.Helpers
{
    public class RequestProcessing
    {
        private readonly AnprSettings _anprSettings;
        private string fileName = string.Empty;


        public RequestProcessing(AnprSettings anprSettings)
        {
            _anprSettings = anprSettings;

        }

        public async Task ProcessRequestAsync(HttpListenerContext context)
        {
            try
            {
                var request = context.Request;
                var contentType = request.ContentType;

                if (!contentType.StartsWith("multipart/form-data"))
                {
                    context.Response.StatusCode = 400;
                    await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes("Invalid Content-Type"));
                    context.Response.Close();
                    return;
                }

                // Extraer boundary
                string boundary = GetBoundary(contentType);
                if (string.IsNullOrEmpty(boundary))
                {
                    context.Response.StatusCode = 400;
                    await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes("Missing boundary"));
                    context.Response.Close();
                    return;
                }

                var result = await ProcessMultipartRequest(request.InputStream, boundary);

                Console.WriteLine($"[OK] Recibido: {result?.ANPR.LicensePlate}, confiabilidad: {result.ANPR.ConfidenceLevel}");

                // Respuesta
                context.Response.StatusCode = 200;
                byte[] buffer = Encoding.UTF8.GetBytes("200 OK");
                context.Response.ContentLength64 = buffer.Length;
                await context.Response.OutputStream.WriteAsync(buffer);
                context.Response.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] {ex.Message}");
                context.Response.StatusCode = 500;
                await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes("Internal Server Error"));
                context.Response.Close();
            }
        }

        private async Task<EventNotificationAlert> ProcessMultipartRequest(Stream stream, string boundary)
        {
            EventNotificationAlert eventNotification = new EventNotificationAlert();
            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    await stream.CopyToAsync(memoryStream);
                    memoryStream.Position = 0;


                    using (var reader = new BinaryReader(memoryStream))
                    {
                        bool isBinary = false;
                        MemoryStream binaryData = null;
                        StringBuilder xmlData = null;
                        int contentLength = 0;
                        byte[] boundaryBytesFinal = Encoding.UTF8.GetBytes(boundary); // + "--");

                        while (reader.BaseStream.Position < reader.BaseStream.Length)
                        {
                            string line = ReadLineBinary(reader);

                            if (line.Contains(boundary))
                            {
                                // Procesar el XML si existe
                                if (xmlData != null)
                                {
                                    eventNotification = ProcessXmlData(xmlData.ToString());
                                    xmlData = null;
                                }

                                // Guardar imagen si existe
                                if (binaryData != null)
                                {
                                    binaryData = null;
                                }

                                // Leer headers
                                while (reader.BaseStream.Position < reader.BaseStream.Length)
                                {
                                    line = ReadLineBinary(reader);
                                    if (string.IsNullOrWhiteSpace(line)) break;

                                    if (line.StartsWith("Content-Type: image/jpeg"))
                                    {
                                        isBinary = true;
                                        binaryData = new MemoryStream();
                                    }
                                    else if (line.StartsWith("Content-Type: application/xml"))
                                    {
                                        isBinary = false;
                                        xmlData = new StringBuilder();
                                    }
                                    else if (line.StartsWith("Content-Length:"))
                                    {
                                        contentLength = int.Parse(line.Split(':')[1].Trim());
                                    }
                                    if (isBinary && contentLength > 0 && line.StartsWith("Content-Length:"))
                                    {
                                        if (!(eventNotification.ANPR.LicensePlate == "NOPLATE"))
                                        {
                                            string blankLine = ReadLineBinary(reader);
                                        }
                                        break;
                                    }
                                }
                            }
                            else if (isBinary && binaryData != null)
                            {
                                List<byte> bufferList = new List<byte>();
                                byte[] startBytes = new byte[] { 0xFF, 0xD8 };

                                bool started = false;

                                while (reader.BaseStream.Position < reader.BaseStream.Length)
                                {
                                    byte b = reader.ReadByte();
                                    bufferList.Add(b);

                                    // Detectar inicio de imagen (���� / SOI JPEG)
                                    if (!started && bufferList.Count >= startBytes.Length)
                                    {
                                        byte[] lastBytes = bufferList.Skip(bufferList.Count - startBytes.Length).ToArray();
                                        if (lastBytes.SequenceEqual(startBytes))
                                        {
                                            started = true;
                                            bufferList.Clear();
                                            bufferList.AddRange(lastBytes);
                                        }
                                    }

                                    // Verificar si llegamos al boundary
                                    if (started && bufferList.Count >= boundaryBytesFinal.Length)
                                    {
                                        byte[] lastBytes = bufferList.Skip(bufferList.Count - boundaryBytesFinal.Length).ToArray();
                                        if (lastBytes.SequenceEqual(boundaryBytesFinal))
                                        {
                                            bufferList.RemoveRange(bufferList.Count - boundaryBytesFinal.Length, boundaryBytesFinal.Length);
                                            break;
                                        }
                                    }
                                }

                                binaryData.Write(bufferList.ToArray(), 0, bufferList.Count);
                            }
                            else
                            {
                                xmlData?.AppendLine(line);
                            }
                        }

                        if (xmlData != null)
                            eventNotification = ProcessXmlData(xmlData.ToString());

                        if (binaryData != null)
                        {
                            string outputDirectory = _anprSettings.Path;
                            string plateNumber = eventNotification?.ANPR?.LicensePlate ?? "NOPLATE";
                            fileName = ImageStorage.SaveBinaryDataAsync(binaryData.ToArray(), outputDirectory, plateNumber);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error: " + ex.Message);
            }

            return eventNotification;
        }

        private string GetBoundary(string contentType)
        {
            string boundary = null;
            string boundaryPrefix = "boundary=";
            int boundaryIndex = contentType.IndexOf(boundaryPrefix);
            if (boundaryIndex >= 0)
            {
                boundary = contentType.Substring(boundaryIndex + boundaryPrefix.Length);
                if (boundary.StartsWith("\"") && boundary.EndsWith("\""))
                {
                    boundary = boundary.Substring(1, boundary.Length - 2);
                }
                return "--" + boundary; // asegúrate de que tenga los dos guiones
            }
            return null;
        }

        private string ReadLineBinary(BinaryReader reader)
        {
            List<byte> lineBytes = new List<byte>();

            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                byte b = reader.ReadByte();
                if (b == '\n') break; // Fin de línea
                if (b != '\r') lineBytes.Add(b); // Ignorar '\r' en CRLF
            }

            return Encoding.UTF8.GetString(lineBytes.ToArray());
        }

        private EventNotificationAlert ProcessXmlData(string xmlContent)
        {
            var eventNotification = XmlHelper.DeserializeXml<EventNotificationAlert>(xmlContent);
            return eventNotification;
        }

        //private static async Task<(string XmlContent, byte[] ImageBytes)> ParseMultipartAsync(Stream stream, string boundary)
        //{
        //    var reader = new StreamReader(stream);
        //    string line;
        //    string xml = null;
        //    MemoryStream imageData = null;

        //    while ((line = await reader.ReadLineAsync()) != null)
        //    {
        //        if (line.StartsWith(boundary))
        //        {
        //            // Headers
        //            string contentDisposition = await reader.ReadLineAsync(); // Content-Disposition
        //            string contentType = await reader.ReadLineAsync(); // Content-Type
        //            await reader.ReadLineAsync(); // Empty line

        //            if (contentType.Contains("text/xml"))
        //            {
        //                var sb = new StringBuilder();
        //                while ((line = await reader.ReadLineAsync()) != null && !line.StartsWith(boundary))
        //                {
        //                    sb.AppendLine(line);
        //                }
        //                xml = sb.ToString().Trim();
        //                if (line == null) break; // EOF
        //                continue;
        //            }
        //            else if (contentType.Contains("image/jpeg"))
        //            {
        //                imageData = new MemoryStream();
        //                var boundaryBytes = Encoding.UTF8.GetBytes("\r\n" + boundary);
        //                var buffer = new byte[4096];
        //                int bytesRead;
        //                var matchBuffer = new List<byte>();

        //                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
        //                {
        //                    imageData.Write(buffer, 0, bytesRead);
        //                    if (imageData.Length >= boundaryBytes.Length)
        //                    {
        //                        byte[] tail = imageData.ToArray()[(int)(imageData.Length - boundaryBytes.Length)..];
        //                        if (boundaryBytes.SequenceEqual(tail))
        //                        {
        //                            imageData.SetLength(imageData.Length - boundaryBytes.Length);
        //                            break;
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    return (xml, imageData?.ToArray());
        //}

    }
}
