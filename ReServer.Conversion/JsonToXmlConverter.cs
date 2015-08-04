using JsonXslt;
using Newtonsoft.Json.Linq;
using ReServer.Core.Responses;
using System.Xml.Linq;

namespace ReServer.Conversion
{
    public class JsonToXmlConverter : IConverter
    {
        private string JsonText;
        private string XmlText;

        public JsonToXmlConverter()
        { }

        public RSResponse Convert(RSResponse original)
        {
            JsonText = original.StringValue;
            PopulateXmlFromJson();

            var newResponse = new RSResponse()
            {
                Type = RSResponse.RSResponseType.Converted,
                Text = XmlText,
                Satisfied = true,
                ContentType = new System.Net.Mime.ContentType("text/xml")
            };

            return newResponse;
        }

        private void PopulateXmlFromJson()
        {
            var jsonObject = JObject.Parse(JsonText);
            XmlText = XDocument.Load(new JsonXPathNavigator(jsonObject).ReadSubtree()).ToString();
        }
    }
}
