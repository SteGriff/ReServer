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

        public RSRequest(HttpListenerRequest httpRequest)
        {
            //(Uri requestUri, NameValueCollection headers, string contentType, CookieCollection cookies)
            //_request.Url, _request.Headers, _request.ContentType, _request.Cookies

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

        public bool PathEndsInSlash()
        {
            return LocalPath[LocalPath.Length - 1] == '/';
        }

    }
}
