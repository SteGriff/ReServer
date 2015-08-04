using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace ReServer.Core.Responses
{
    public class StatusCodeResponse : RSResponse
    {
        public StatusCodeResponse(HttpStatusCode statusCode)
            : base()
        {
            Initialise(statusCode);
            Text = StatusDescription();
        }

        public StatusCodeResponse(HttpStatusCode statusCode, string message)
            : base()
        {
            Initialise(statusCode);

            //Set custom message
            Text = StatusDescription(message);
        }

        public StatusCodeResponse(HttpStatusCode statusCode, string message, string location)
            : base()
        {
            Initialise(statusCode);

            //Set custom message
            Text = StatusDescription(message);

            //Set location of redirected/created resource
            AdditionalHeaders.Add("Location", location);
        }

        private void Initialise(HttpStatusCode statusCode)
        {
            StatusCode = statusCode;
            Satisfied = true;
            HasContent = false;

            Type = RSResponseType.Text;
            ContentType = new ContentType("text/plain");
        }
    }
}
