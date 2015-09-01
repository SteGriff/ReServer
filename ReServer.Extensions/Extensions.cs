using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;

namespace ReServer.Extensions
{
    public static class Extensions
    {
        public static ContentType GetContentType(this string FileName)
        {
            return ExtraMimeMapping.GetContentType(FileName);
        }

        public static IList<string> GetFileExtensionCandidates(this string ContentType)
        {
            //Checks overrides and then system extensions
            return ExtraMimeMapping.GetFileExtensionCandidates(ContentType);
        }

        public static string ContentTypeLastPart(this string ContentType)
        {
            return ContentType.Split(new[] { '/' }).Last().ToString();
        }

        public static string RemoveSubtypes(this string contentTypeString)
        {
            return contentTypeString.Split(new char[] { ';' })[0];
        }

    }
}
