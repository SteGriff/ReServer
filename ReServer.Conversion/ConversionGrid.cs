using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReServer.Extensions;

namespace ReServer.Conversion
{
    public static class ConversionGrid
    {
        // Possible conversions from each type
        // Breaking these down by type makes it possible to define AllConversions in a less verbose way later

        private static List<string> ConversionsFromJson = new List<string> { "application/xml", "text/xml", "text/html" };
        private static List<string> ConversionsFromMarkdown = new List<string> { "text/html", "text/plain" };

        /// <summary>
        /// A table containing all possible conversions as a dictionary on fromType
        /// "from-type-A" => ["to-type-1", "to-type-2"...]
        /// "from-type-B" => ["to-type-2", "to-type-3"...]
        /// Accessed via CanConvert() method
        /// </summary>
        private static Dictionary<string, List<string>> AllConversions = new Dictionary<string, List<string>>
        {
            {"application/json", ConversionsFromJson},
            {"text/json", ConversionsFromJson},
            {"text/markdown", ConversionsFromMarkdown}
        };


        private static Dictionary<string, string> ConverterFullTypeNames = new Dictionary<string, string>
        {
            {"json-xml", typeof(JsonToXmlConverter).AssemblyQualifiedName},
            {"json-html", typeof(JsonToHtmlConverter).AssemblyQualifiedName},
            {"markdown-html", typeof(MarkdownToHtmlConverter).AssemblyQualifiedName}
        };


        /// <summary>
        /// Check whether a document in one ContentType has a converter to change it to another
        /// </summary>
        /// <param name="fromType">The present MIME ContentType string of the document</param>
        /// <param name="toType">The target MIME ContentType for the document</param>
        /// <returns></returns>
        public static bool CanConvert(string fromType, string toType)
        {
            return AllConversions.Any(conv => conv.Key == fromType.ToLower() && conv.Value.Any(to => to.ToLower() == toType));
        }

        public static string ConverterFullTypeName(string fromType, string toType)
        {
            string converterTypeName = String.Format("{0}-{1}",
                            fromType.ContentTypeLastPart().ToLower(),
                            toType.ContentTypeLastPart().ToLower()
                            );

            return ConverterFullTypeNames[converterTypeName];
        }
    }
}
