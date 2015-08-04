using ReServer.Extensions;
using System.Collections.Generic;
using System.Net.Http.Headers;

namespace ReServer.Core.Responses
{
    public class FileResponse : RSResponse
    {
        public FileResponse(string localPath, IList<MediaTypeWithQualityHeaderValue> acceptableTypes)
        {
            Type = RSResponseType.File;

            string matchingFile = MatchingFileFinder.BestMatchingFile(localPath, acceptableTypes);

            if (matchingFile != null)
            {
                FilePath = matchingFile;
                ContentType = matchingFile.GetContentType();
                Satisfied = true;
                HasContent = true;
            }

        }

    }
}
