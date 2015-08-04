using ReServer.Collaboration;
using ReServer.Core.Requests;
using ReServer.Core.Responses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;

namespace ReServer.Server
{
    /// <summary>
    /// Based on request information, marshals the correct action and response 
    /// using DocumentHandler and by producing an RSResponse object of some kind
    /// </summary>
    public class RouteHandler
    {
        public string Method { get; set; }

        private const string GET = "GET";
        private const string HEAD = "HEAD";
        private const string POST = "POST";
        private const string PUT = "PUT";
        private const string DELETE = "DELETE";
        private const string OPTIONS = "OPTIONS";
        private const string TRACE = "TRACE";
        private const string CONNECT = "CONNECT";

        private string[] HTTP_METHODS = new string[] { GET, HEAD, POST, PUT, DELETE, OPTIONS, TRACE, CONNECT };

        /// <summary>
        /// Instantiate a RouteHandler with the specified HTTP Verb/Method
        /// </summary>
        /// <param name="method"></param>
        public RouteHandler(string method)
        {
            Method = method.ToUpper();

            //If method is not valid, throw exception
            if (!HTTP_METHODS.Contains(method))
            {
                InvalidHttpMethod(method);
            }
        }

        /// <summary>
        /// Find the appropriate default file in a certain directory 
        /// and act upon it or create the right kind of response
        /// </summary>
        /// <param name="localPath">Path of the directory</param>
        /// <param name="acceptableTypes">The client's Accept types</param>
        /// <returns>RSResponse for the server thread to feed to the client</returns>
        public RSResponse HandleDefaultFileRoute(string localPath, IList<MediaTypeWithQualityHeaderValue> acceptableTypes)
        {
            switch (Method)
            {
                case GET:
                    //Retrieve and return the best default file (based on accept types)
                    return new DefaultFileResponse(localPath, acceptableTypes);

                case POST:
                    //Return error status code 405 MethodNotAllowed
                    return new StatusCodeResponse(HttpStatusCode.MethodNotAllowed, "Cannot target directory with POST");

                case PUT:
                    //Return error status code 405 MethodNotAllowed
                    return new StatusCodeResponse(HttpStatusCode.MethodNotAllowed, "Cannot target directory with PUT");

                case DELETE:
                    //Delete the directory
                    return DocumentHandler.DeleteDirectory(localPath);

                default:
                    //If method is not handled, return error
                    InvalidHttpMethod(Method);
                    break;
            }

            //(This is here defensively)
            return new StatusCodeResponse(HttpStatusCode.MethodNotAllowed);
        }

        /// <summary>
        /// Retrieve or perform an action on the specified file
        /// </summary>
        /// <param name="rsRequest">The original request object</param>
        /// <param name="entityStream">The request body stream (for storage with PUT if necessary)</param>
        /// <returns>RSResponse for the server thread to feed to the client</returns>
        public RSResponse HandleFileRoute(RSRequest rsRequest, Stream entityStream)
        {
            switch (Method)
            {
                case GET:
                    //Retrieve the file
                    return new FileResponse(rsRequest.LocalPath, rsRequest.AcceptTypes);

                case POST:
                    //TODO Decide behaviour for post to file; Append resource??
                    //return new StatusCodeResponse(HttpStatusCode.Created);

                case PUT:
                    //Store the file at the specified path
                    return DocumentHandler.StoreFile(rsRequest, entityStream);

                case DELETE:
                    //Delete the file
                    return DocumentHandler.DeleteFile(rsRequest.LocalPath, rsRequest.ContentType);

                default:
                    //Return error if not handled
                    InvalidHttpMethod(Method);
                    break;
            }

            //(Defensive) return error if not handled
            return new StatusCodeResponse(HttpStatusCode.MethodNotAllowed);
        }

        private void InvalidHttpMethod(string method)
        {
            throw new InvalidOperationException(method + " is not a valid HTTP method");
        }

    }
}
