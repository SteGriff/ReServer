using ReServer.Models;
using System;
using System.Configuration;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace ReServer.Core.Configuration
{
    public static class Config
    {
        private static string ReserverConfigLocation = "reserverConfigLocation";

        public static string Error { get; private set; }
        public static Server Server;

        /// <summary>
        /// Initialise the ReServer configuration class
        /// </summary>
        /// <returns>True on success</returns>
        public static bool Initialise()
        {
            Error = "";

            //Use app.config to load ReserverConfig file location
            string configLocation = ConfigurationManager.AppSettings[ReserverConfigLocation];

            //Check it exists and set error if not
            if (File.Exists(configLocation))
            {
                //Load the file and try to parse it
                var fileContent = File.ReadAllText(configLocation);

                try
                {
                    Server = Parse(fileContent);
                }
                catch (Exception ex)
                {
                    Error = "Reserver config file malformed: " + ex.Message;
                    return false;
                }
                
                return true;
            }
            else
            {
                Error = "Could not find Reserver config file at the address specified by app.config, '" + configLocation + "'";
                return false;
            }
        }

        private static Server Parse(string fileContent)
        {
            MemoryStream memStream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent));

            //Make a serializer, and deserialize using config model
            XmlSerializer serializer = new XmlSerializer(typeof(Server));
            return (Server)serializer.Deserialize(memStream);
        }
    }
}
