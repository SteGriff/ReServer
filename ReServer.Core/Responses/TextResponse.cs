using System.Net;
using System.Net.Mime;

namespace ReServer.Core.Responses
{
    public class TextResponse : RSResponse
    {
        public TextResponse(string siteLocalMapping, HttpListenerRequest request)
        {
            setText(siteLocalMapping, request);
        }
        
        public TextResponse(string siteLocalMapping, HttpListenerRequest request, HttpStatusCode statusCode)
        {
            StatusCode = statusCode;
            setText(siteLocalMapping, request);
        }

        private void setText(string siteLocalMapping, HttpListenerRequest request)
        {
            string response = "Welcome to ReServer!\r\n\r\n";
            response += "You requested: '" + request.Url.AbsoluteUri + "'\r\n";
            response += "This maps to: '" + siteLocalMapping + "'\r\n";
            response += "You accept:\r\n";

            foreach (string t in request.AcceptTypes)
            {
                response += " - " + t + "\r\n";
            }

            response += "END";

            Type = RSResponseType.Text;
            Satisfied = true;
            HasContent = true;

            ContentType = new ContentType("text/plain");
            Text = response;
        }
    }
}
