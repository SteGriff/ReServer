using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ReServer.Models
{
    public class User
    {
        [XmlElement("name")]
        public string Name { get; set; }

        [XmlElement("password_crypt")]
        public string PasswordCrypt { get; set; }
    }
}
