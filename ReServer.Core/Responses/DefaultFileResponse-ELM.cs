using ReServer.Extensions;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Headers;

namespace ReServer.Core.Responses
{
    public class DefaultFileResponse : RSResponse
    {
        public DefaultFileResponse(string localPath, IList<MediaTypeWithQualityHeaderValue> acceptableTypes)
        {
            Type = RSResponseType.File;

            //TODO Get from config
            var defaultPages = new[] { "index" };

            foreach (var d in defaultPages)
            {
                string tryPath = localPath + d;
                Satisfied = File.Exists(tryPath);

                if (Satisfied)
                {
                    // File found verbatim, i.e.
                    // reserver.local/site/file.json matched C:\reserver\file.json

                    FilePath = tryPath;
                    ContentType = tryPath.GetContentType();
                    break;
                }
                else
                {
                    // Not found yet
                    // Client may be implying the file extension.
                    // Match a file, prioritising by matching exts of the Accept types
                    string matchingFile = MatchingFileFinder.BestMatchingFile(tryPath, acceptableTypes);

                    if (matchingFile != null)
                    {
                        FilePath = matchingFile;
                        ContentType = matchingFile.GetContentType();
                        Satisfied = true;
                        break;
                    }
                }

            }


        }
    }

}
