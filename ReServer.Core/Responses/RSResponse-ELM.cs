using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mime;
using System.Text;

namespace ReServer.Core.Responses
{
    public class RSResponse
    {
        public bool Satisfied { get; set; }
        public ContentType ContentType { get; set; }
        public RSResponseType Type { get; set; }
        public HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// Headers to be added to the HttpResponse
        /// </summary>
        public List<string> AdditionalHeaders { get; set; }

        public string FilePath { get; set; }
        public string Text { get; set; }

        public string StringValue
        {
            get
            {
                if (Type == RSResponseType.File)
                {
                    return File.ReadAllText(FilePath);
                }
                else if (Type == RSResponseType.Text || Type == RSResponseType.Converted)
                {
                    return Text;
                }
                else
                {
                    return "";
                }
            }
        }

        public byte[] Bytes
        {
            get
            {
                if (Type == RSResponseType.File)
                {
                    return File.ReadAllBytes(FilePath);
                }
                else if (Type == RSResponseType.Text || Type == RSResponseType.Converted)
                {
                    return Encoding.UTF8.GetBytes(Text);
                }
                else
                {
                    //Could be a redirect, just return nothing
                    return new[]{(byte) 0x00};
                }
            }
        }

        public RSResponse()
        {
            Type = RSResponseType.None;
            Satisfied = false;
            ContentType = null;
        }

        protected string StatusDescription()
        {
            return ((int)StatusCode).ToString() + " " + StatusCode.ToString();
        }

        public enum RSResponseType
        {
            None,
            Redirect,
            File,
            Text,
            Converted
        }

    }
}
