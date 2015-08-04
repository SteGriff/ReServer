using ReServer.Core.Configuration;
using ReServer.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ReServer.Core.Requests
{
    public class RSRequest
    {
        public Uri RequestUri { get; private set; }
        public string LocalPath { get; private set; }
        public IList<MediaTypeWithQualityHeaderValue> AcceptTypes { get; private set; }
        public NameValueCollection Headers {get;private set;}
        public Site Website { get; private set; }

        public RSRequest(Uri requestUri)
        {
            //Get the correct Website instance from URI mapping
            Website = Config.Server.Sites
                .Where(s => s.IsMappedToUriDomain(requestUri))
                .FirstOrDefault();

            string requestedPath = requestUri.AbsolutePath;
            LocalPath = Website.Local + requestedPath;

            //Parse the client's Accept types
            AcceptTypes = AcceptableTypes();
            
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
