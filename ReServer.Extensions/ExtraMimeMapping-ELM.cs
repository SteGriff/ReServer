using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Web;

namespace ReServer.Extensions
{
    static class ExtraMimeMapping
    {
        private static Dictionary<string, string> _extraMimeMappings =
            new Dictionary<string, string>{
                {".json", "application/json"},
                {".xml", "application/xml"},
                {".markdown", "text/markdown"},
                {".md", "text/markdown"},
                {".htm", "text/html"},
                {".html", "text/html"}
            };

        private static ILookup<object, string> _systemMappings;
        private static ILookup<object, string> SystemMappings
        {
            get
            {
                if (_systemMappings == null)
                {
                    //Get Mime->Extension mappings from windows registry
                    // CC-BY-SA 3.0 - Courtesy of user 'L.B' http://stackoverflow.com/a/21330460 
                    _systemMappings = Microsoft.Win32.Registry.ClassesRoot.GetSubKeyNames()
                        .Select(key => new
                        {
                            Key = key,
                            ContentType = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(key).GetValue("Content Type")
                        })
                        .Where(x => x.ContentType != null)
                        .ToLookup(x => x.ContentType, x => x.Key);
                }
                return _systemMappings;
            }
        }

        public static ContentType GetContentType(string fileName)
        {
            //Check my overrides first
            string mime = GetContentTypeOverride(fileName);
            if (mime == null)
            {
                //Not found in overrides; use Microsoft mappings
                mime = MimeMapping.GetMimeMapping(fileName);
            }

            return new ContentType(mime);
        }

        private static string GetContentTypeOverride(string fileName)
        {
            string fileExtension = Path.GetExtension(fileName);

            var contentType = _extraMimeMappings.Where(mm => mm.Key == fileExtension).FirstOrDefault();

            if (contentType.Value == null)
            {
                return null;
            }
            else
            {
                return contentType.Value;
            }
        }

        public static IList<string> GetFileExtensionCandidates(string contentType)
        {
            //Check my overrides first

            List<string> extensions = _extraMimeMappings
                .Where(mm => mm.Value == contentType)
                .Select(mm => mm.Key)
                .Distinct()
                .ToList();

            if (extensions.Count == 0)
            {
                //Not found; use Registry keys
                extensions = GetSystemFileExtensionsFor(contentType);
            }

            return extensions;
        }

        private static List<string> GetSystemFileExtensionsFor(string contentType)
        {
            return SystemMappings[contentType].ToList();
        }
    }
}
