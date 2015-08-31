using ReServer.Core.Configuration;
using ReServer.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;

namespace ReServer.Core.Requests
{
    public class RSRequest
    {
        public Uri RequestUri { get; private set; }
        public string LocalPath { get; private set; }
        public Site Website { get; private set; }
        public string ContentType { get; private set; }

        public IList<MediaTypeWithQualityHeaderValue> AcceptTypes { get; private set; }
        public NameValueCollection Headers { get; private set; }
        public CookieCollection Cookies { get; private set; }
        public HttpListenerBasicIdentity Identity { get; private set; }

        private string Method;

        public RSRequest(HttpListenerContext context)
        {
            //Just for terseness
            var httpRequest = context.Request;

            //Store the method (for authorisation checks)
            Method = context.Request.HttpMethod;

            //Get the correct Website instance from URI mapping
            Website = Config.Server.Sites
                .Where(s => s.IsMappedToUriDomain(httpRequest.Url))
                .FirstOrDefault();

            string requestedPath = httpRequest.Url.AbsolutePath;
            LocalPath = Website.Local + requestedPath;

            Headers = httpRequest.Headers;
            ContentType = httpRequest.ContentType;

            //Parse the client's Accept types
            AcceptTypes = AcceptableTypes();

            Cookies = httpRequest.Cookies;

            //Get the username and password
            Identity = ((HttpListenerBasicIdentity)context.User.Identity);
        }

        /// <summary>
        /// Get an enumerable of the types listed in the client's Accept header
        /// ordered by preference
        /// </summary>
        /// <returns>List of Accept types in descending preference order</returns>
        private IList<MediaTypeWithQualityHeaderValue> AcceptableTypes()
        {

            if (String.IsNullOrWhiteSpace(Headers["Accept"]))
            {
                //return Config.Server.
                //TODO get from config
                return new[] { new MediaTypeWithQualityHeaderValue("text/html") };
            }

            //Split the request header and form it into objects
            IList<string> values = Headers["Accept"].Split(new[] { ',' });

            return values
                .Select(a => SafeMediaTypeWithQuality(a))
                .OrderByDescending(a => a.Quality)
                .ToList();
        }

        private MediaTypeWithQualityHeaderValue SafeMediaTypeWithQuality(string headerValue)
        {
            MediaTypeWithQualityHeaderValue mt;
            bool valid = MediaTypeWithQualityHeaderValue.TryParse(headerValue, out mt);

            if (valid)
            {
                if (mt.Quality == null)
                {
                    mt.Quality = 1.0;
                }
                return mt;
            }

            return null;
        }

        public bool IsAuthorised
        {
            get
            {
                if (RequiresAuthorisation)
                {
                    var user = Website.Users.Where(u => u.Name == Identity.Name).FirstOrDefault();
                    if (user != null)
                    {
                        //User exists
                        string usernameSalt = Identity.Name.ToLower();
                        string enteredPassword = PasswordCrypt.Encrypt(Identity.Password, usernameSalt);
                        if (user.PasswordCrypt == enteredPassword)
                        {
                            //Correct password: allow
                            return true;
                        }
                    }

                    //All other cases, i.e. no user or wrong password: reject
                    return false;
                }
                else
                {
                    //Auth not required: allow
                    return true;
                }
            }
        }

        private bool RequiresAuthorisation
        {
            get
            {
                if (Website.ProtectedActions == Protectorate.none)
                {
                    //Nothing is protected; doesn't require auth
                    return false;
                }
                else if (Website.ProtectedActions == Protectorate.all)
                {
                    //All protected; all require auth
                    return true;
                }
                else
                {
                    //Write is protected
                    // Return true if the attempted action is a protected one
                    return Website.ProtectedActions == TargetAction;
                }
            }
        }

        private Protectorate TargetAction
        {
            get
            {
                switch (Method)
                {
                    case "POST":
                    case "DELETE":
                    case "PUT":
                    case "PATCH":
                        return Protectorate.write;
                    default:
                        return Protectorate.all;
                }
            }
        }

        public bool PathEndsInSlash
        {
            get
            {
                return LocalPath[LocalPath.Length - 1] == '/';
            }
        }

    }
}
