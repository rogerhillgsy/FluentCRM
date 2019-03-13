using System;
using System.IO;
using System.Xml.Serialization;

namespace FluentCRM.Utility
{
    public class XMLUtilities
    {
        public static void SaveClassToXml<T>(T target, string fileName) where T : class
        {
            var serializer = new XmlSerializer(typeof(T));

            var path = System.IO.Path.ChangeExtension(fileName, "xml");

            using (var file = System.IO.File.Create(path))
            {
                serializer.Serialize(file, target);
            }
        }

        /// <summary>
        /// Read the serialized form of a class from a file.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static T LoadClassfromXml<T>(string fileName) where T : class
        {
            var reader = new FileStream(fileName, FileMode.Open);
            var serializer = new XmlSerializer(typeof(T));
            var response = serializer.Deserialize(reader) as T;
            return response;
        }

        /// <summary>
        /// Convert class to XML
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <returns></returns>
        public static string ToString<T>(T target) where T: class
        {
            var serializer = new XmlSerializer(typeof(T));

            using (var sww = new StringWriter())
            {
                serializer.Serialize(sww, target);
                return sww.ToString();
            }
        }
    }
}