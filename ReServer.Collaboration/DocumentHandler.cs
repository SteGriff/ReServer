using ReServer.Core;
using ReServer.Core.Requests;
using ReServer.Core.Responses;
using ReServer.Extensions;
using System;
using System.IO;
using System.Net;

namespace ReServer.Collaboration
{
    public static class DocumentHandler
    {

        public static RSResponse StoreFile(RSRequest rsRequest, Stream entity)
        {
            string extensionToAdd = "";

            //If file extension was not specified, make a good guess
            if (Path.GetExtension(rsRequest.LocalPath) == String.Empty)
            {
                extensionToAdd = rsRequest.ContentType.GetFileExtensionCandidates()[0];
            }

            //Add extension to the target file name (or don't)
            string newFileName = rsRequest.LocalPath + extensionToAdd;
                    
            try
            {
                using (FileStream fs = File.OpenWrite(newFileName))
                {
                    entity.CopyTo(fs);
                }
            }
            catch (Exception ex)
            {
                return new StatusCodeResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

            return new StatusCodeResponse(HttpStatusCode.Created);
        }

        public static RSResponse DeleteFile(string location, string contentType)
        {
            try
            {
                location = MatchingFileFinder.BestMatchingFile(location, contentType);

                if (File.Exists(location))
                {
                    File.Delete(location);
                }
                else
                {
                    return new StatusCodeResponse(HttpStatusCode.NotFound);
                }
            }
            catch (Exception ex)
            {
                return new StatusCodeResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

            return new StatusCodeResponse(HttpStatusCode.NoContent);
        }

        public static RSResponse DeleteDirectory(string location)
        {
            try
            {
                Directory.Delete(location);
            }
            catch (Exception ex)
            {
                return new StatusCodeResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

            return new StatusCodeResponse(HttpStatusCode.NoContent);
        }

    }
}
