using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ReServer.Core.Responses
{
    public class PostResultResponse : RSResponse
    {
        public PostResultResponse(HttpStatusCode statusCode)
        {
            StatusCode = statusCode;
        }
        public PostResultResponse(HttpStatusCode statusCode, string location)
        {
            StatusCode = statusCode;
            AdditionalHeaders.Add("Location: " + location);
        }
    }
}
