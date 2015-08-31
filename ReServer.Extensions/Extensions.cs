using System.Net.Mime;
using System.Web;
using System.Linq;
using System.Collections.Generic;

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

        public static ContentType ToContentType(this string ContentTypeString)
        {
            return new ContentType(ContentTypeString);
        }

        public static string ContentTypeLastPart(this string ContentType)
        {
            return ContentType.Split(new[] { '/' }).Last().ToString();
        }

        public static string Capitalise(this string s)
        {
            switch (s.Length)
            {
                case 0:
                    return "";
                case 1:
                    return s.ToUpper();
                default:
                    return s[0].ToString().ToUpper() + s.Substring(1).ToLower();
            }
        }

        public static string RemoveSubtypes(this string contentTypeString)
        {
            return contentTypeString.Split(new char[] { ';' })[0];
        }

    }
}
