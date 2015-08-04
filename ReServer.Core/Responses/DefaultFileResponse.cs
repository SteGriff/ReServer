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
                string matchingFile = MatchingFileFinder.BestMatchingFile(tryPath, acceptableTypes);

                if (matchingFile != null)
                {
                    FilePath = matchingFile;
                    ContentType = matchingFile.GetContentType();
                    Satisfied = true;
                    HasContent = true;

                    //Search no more defaults
                    break;
                }

            }
        }
    }

}
