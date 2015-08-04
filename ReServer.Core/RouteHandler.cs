using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ReServer.Core
{
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

        public RouteHandler(string method)
        {
            Method = method.ToUpper();

            if (!HTTP_METHODS.Contains(method))
            {
                InvalidHttpMethod(method);
            }
        }

        public RSResponse HandleDefaultFileRoute(string localPath, IList<MediaTypeWithQualityHeaderValue> acceptableTypes)
        {
            switch (Method)
            {
                case GET:
                    return new DefaultFileResponse(localPath, acceptableTypes);

                case POST:
                    return new PostResultResponse(HttpStatusCode.Created);

                default:
                    InvalidHttpMethod(Method);
                    break;
            }

            return new MethodNotAllowedResponse();
        }

        private void InvalidHttpMethod(string method)
        {
            throw new InvalidOperationException(method + " is not a valid HTTP method");
        }

    }
}
