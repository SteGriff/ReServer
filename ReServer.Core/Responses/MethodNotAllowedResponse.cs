using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ReServer.Core.Responses
{
    public class MethodNotAllowedResponse : RSResponse
    {
        public MethodNotAllowedResponse()
        {
            StatusCode = HttpStatusCode.MethodNotAllowed;
            Satisfied = true;

            Type = RSResponseType.Text;
            Text = StatusDescription();
        }
    }
}
