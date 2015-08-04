using System;
using System.Linq;

namespace ReServer.Core.Responses
{
    public class RedirectionResponse : RSResponse
    {
        public RedirectionResponse()
        {
            Type = RSResponseType.Redirect;
            Satisfied = true;
        }
    }
}
