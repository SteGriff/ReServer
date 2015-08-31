using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace ReServer.Models
{
    public class Site
    {
        [XmlElement("name")]
        public string Name { get; set; }

        [XmlElement("local")]
        public string Local { get; set; }

        [XmlElement("remote")]
        public List<string> RemoteAddressStrings { get; set; }

        [XmlElement("protect")]
        public Protectorate ProtectedActions { get; set; }

        [XmlArray("users")]
        [XmlArrayItem("user")]
        public List<User> Users { get; set; }
        
        [XmlIgnore]
        private List<Uri> _remoteAddresses;

        [XmlIgnore]
        /// <summary>
        /// Returns the list of addresses from the config file as Uri objects
        /// </summary>
        public List<Uri> RemoteAddresses
        {
            get
            {
                if (_remoteAddresses == null)
                {
                    _remoteAddresses = RemoteAddressStrings.Select(a => new Uri(a)).ToList();
                }
                return _remoteAddresses;
            }
        }

        public bool RequiresAuthentication
        {
            get
            {
                return Users.Count > 0;
            }
        }

        /// <summary>
        /// Check whether this website is mapped to the host domain of the given Uri
        /// </summary>
        /// <param name="uri"></param>
        /// <returns>True if the site has a mapping the host domain</returns>
        public bool IsMappedToUriDomain(Uri uri)
        {
            string domain = uri.Host;
            return _remoteAddresses.Where(a => a.Host == domain).FirstOrDefault() != null;
        }

    }
}
