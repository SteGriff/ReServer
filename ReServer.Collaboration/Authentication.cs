using ReServer.Core.Requests;
using ReServer.Core.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ReServer.Collaboration
{
    public static class Authentication
    {
        /// <summary>
        /// Pass the user through an authentication gate.
        /// If the correct auth cookie is present, returns null
        /// otherwise returns a 401 login prompt
        /// </summary>
        /// <param name="rsRequest"></param>
        /// <returns>null if authenticated, StatusCodeResponse(401) if not</returns>
        public static RSResponse RequireAuthentication(RSRequest rsRequest)
        {
            if (CookiePresent(rsRequest))
            {
                return null;
            }
            else
            {
                var loginResponse = new StatusCodeResponse(HttpStatusCode.Unauthorized, "Log in");
                loginResponse.AdditionalHeaders.Add("WWW-Authenticate",  "Basic realm=\"Log in to " + rsRequest.Website.Name + "\"");
                return loginResponse;
            }
        }

        private static string CookieName(RSRequest rsRequest)
        {
            return "rs-collab-" + rsRequest.Website.Name;
        }

        private static bool CookiePresent(RSRequest rsRequest)
        {
            string requiredCookie = CookieName(rsRequest);
            return (rsRequest.Cookies[requiredCookie] != null);
        }

    }
}
