using System.IO;
using System.Xml;

namespace Display.Helper.Encode
{
    internal class XamlHelper
    {
        internal static string ConvertXmlToString(XmlDocument xmlDoc)
        {
            using var stream = ConvertXmlToStream(xmlDoc);
            using var sr = new StreamReader(stream);

            return sr.ReadToEnd();
        }

        internal static Stream ConvertXmlToStream(XmlDocument xmlDoc)
        {
            var stream = new MemoryStream();

            var settings = new XmlWriterSettings
            {
                OmitXmlDeclaration = true,
                Indent = true
            };
            var writer = XmlWriter.Create(stream, settings);
            xmlDoc.Save(writer);
            stream.Position = 0;

            return stream;
        }
    }
}
