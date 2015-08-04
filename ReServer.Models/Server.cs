using System.Collections.Generic;
using System.Xml.Serialization;

namespace ReServer.Models
{
    [XmlRoot("reserver")]
    public class Server
    {
        [XmlArray("sites")]
        [XmlArrayItem("site")]
        public List<Site> Sites { get; set; }

        //[XmlArray("defaults")]
        //[XmlArrayItem("default")]
        //public List<KeyValue> Defaults { get; set; }
    }
}
