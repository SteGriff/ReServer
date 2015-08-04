using JsonXslt;
using Newtonsoft.Json.Linq;
using ReServer.Core.Responses;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace ReServer.Conversion
{
    public class JsonToHtmlConverter : IConverter
    {
        private string JsonText;
        private string HtmlText;

        public JsonToHtmlConverter()
        { }

        public RSResponse Convert(RSResponse original)
        {
            JsonText = original.StringValue;
            PopulateHtmlFromJson();

            var newResponse = new RSResponse()
            {
                Type = RSResponse.RSResponseType.Converted,
                Text = HtmlText,
                Satisfied = true,
                ContentType = new System.Net.Mime.ContentType("text/html")
            };

            return newResponse;
        }

        private void PopulateHtmlFromJson()
        {
            var jsonObject = JObject.Parse(JsonText);

            //Get the stylesheet href and remove it from the data document
            string stylesheetLink = "";
            
            try 
	        {
                stylesheetLink = jsonObject.GetValue(Conventions.JsonStylesheetSpecifier).ToString();
                jsonObject.Remove(Conventions.JsonStylesheetSpecifier);
	        }
	        catch (Exception)
	        {
                //No stylesheet link - it's ok
	        }

            // Load the JSON data as an XML doc
            // with root node named 'body'
            var xmlDoc = XDocument.Load(new JsonXPathNavigator(jsonObject, "body").ReadSubtree());
            var xmlRoot = xmlDoc.Root;

            string ConversionBinPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            //TODO Change to configurable path
            var html = File.ReadAllText("C:/ReServer/Templates/DefaultHtmlTemplate.htm");
            var htmlDoc = XDocument.Parse(html);

            var headSection = htmlDoc.Descendants("head").FirstOrDefault();

            if (headSection != null)
            {
                if (stylesheetLink != "")
                {
                    var linkTag = XElement.Parse("<link rel=\"stylesheet\" href=\"" + stylesheetLink + "\" />");
                    headSection.Add(linkTag);
                }
            }
            
            //Add the xml result as child of html
            htmlDoc.Root.Add(xmlRoot);

            //Set the HtmlText
            // (The doctype is added last because the XML parser would mess it up)
            HtmlText = "<!DOCTYPE HTML>\r\n" + htmlDoc.ToString();
        }
    }
}
