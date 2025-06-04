using System.Xml.Serialization;

namespace HikvisionHelpers.Helpers
{
    public static class XmlHelper
    {
        public static T DeserializeXml<T>(string xmlContent)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(T));
                using (var reader = new StringReader(xmlContent))
                {
                    return (T)serializer.Deserialize(reader);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("couldn't deserialize XML structure");
            }
        }
    }
}
