using ReServer.Extensions;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Headers;

namespace ReServer.Core.Responses
{
    public class FileResponse : RSResponse
    {
        public FileResponse(string localPath, IList<MediaTypeWithQualityHeaderValue> acceptableTypes)
        {
            Type = RSResponseType.File;
            Satisfied = File.Exists(localPath);

            if (Satisfied)
            {
                // File found verbatim, i.e.
                // reserver.local/site/file.json matched C:\reserver\file.json

                FilePath = localPath;

                //TODO File access safety here? Don't let remote users open something outside of the website path
                //response.Bytes = File.ReadAllBytes(FilePath);
                ContentType = localPath.GetContentType();
            }
            else
            {
                // Not found yet
                // Client may be implying the file extension.
                // Match a file, prioritising by matching exts of the Accept types
                string matchingFile = MatchingFileFinder.BestMatchingFile(localPath, acceptableTypes);

                if (matchingFile != null)
                {
                    FilePath = matchingFile;
                    ContentType = matchingFile.GetContentType();
                    Satisfied = true;
                }
            }

        }

    }
}
