using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReServer.Core.Responses
{
    public class NotFoundResponse : RSResponse
    {
        public NotFoundResponse()
        {
            StatusCode = System.Net.HttpStatusCode.NotFound;
            Satisfied = true;
        }
    }
}
