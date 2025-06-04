namespace HikvisionHelpers.Helpers
{
    public class ImageStorage
    {
        public static string SaveBinaryDataAsync(byte[] data, string outputDirectory, string plateNumber)
        {
            string fileName = $"{DateTime.Now:yyyyMMdd-HHmmss}-{plateNumber}.jpg";

            if (!Directory.Exists(outputDirectory))
                Directory.CreateDirectory(outputDirectory);

            string fullPath = Path.Combine(outputDirectory, fileName);

            if (data != null)
            {
                using (FileStream fs = File.Open(fullPath, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    fs.Write(data, 0, data.Length);
                    fs.Close();
                }
            }
            return fullPath;
        }
    }
}
