using ReServer.Conversion;
using ReServer.Core.Configuration;
using ReServer.Core.Requests;
using ReServer.Core.Responses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Threading;

namespace ReServer.Core
{
    public class HttpServerThread
    {
        private HttpListenerRequest _request;
        private HttpListenerResponse _response;
        private RSRequest rsRequest;

        /// <summary>
        /// The name of the ServerThread for use in logging
        /// </summary>
        public string Name
        {
            get
            {
                return "Thread-" + Thread.CurrentThread.ManagedThreadId;
            }
        }

        /// <summary>
        /// Construct new ServerThread, using HttpListenerContext to populate request and response objects
        /// </summary>
        /// <param name="context">The HttpListenerContext of the current request</param>
        public HttpServerThread(HttpListenerContext context)
        {
            _request = context.Request;
            _response = context.Response;

            Output.Write(Name + " initialised");
        }

        /// <summary>
        /// Based on request URI, method, and headers; form an appropriate response and carry out necessary actions on the server
        /// </summary>
        public void HandleRequest()
        {
            Output.Write(Name + " handling '" + _request.Url.AbsoluteUri + "'");

            rsRequest = new RSRequest(_request.Url);
            
            //ReServer (RS) response object
            // Class used to build the raw output
            // which will be fed into the HttpResponse OutputStream 
            RSResponse rsResponse = new RSResponse();

            // Check first whether it is a dir
            if (Directory.Exists(rsRequest.LocalPath))
            {
                Output.Write("Directory");

                // localPath is a directory:
                // Check it ends with a slash
                if (rsRequest.PathEndsInSlash())
                {
                    Output.Write("Getting default file");

                    // Folder root requested:
                    // Search configured list of default pages
                    rsResponse = new DefaultFileResponse(rsRequest.LocalPath, rsRequest.AcceptTypes);
                }
                else
                {
                    // Re-target the request to the directory itself
                    // to give correct behaviour when child files are requested

                    Output.Write("    Lacks terminating slash; redirecting...");

                    _response.Redirect(_request.Url.AbsoluteUri + '/');
                    rsResponse = new RedirectionResponse();
                }

            }
            else
            {
                //Return the requested file
                rsResponse = new FileResponse(rsRequest.LocalPath, rsRequest.AcceptTypes);
            }

            //If not found, return error or debug info
            if (!rsResponse.Satisfied)
            {
                //This is kind of temporary
                rsResponse = new TextResponse(rsRequest.Website.Local, _request, HttpStatusCode.NotFound);
            }

            //Convert the response to a client-preferred Content-Type
            // UNLESS this is a redirect scenario
            if (rsResponse.Type != RSResponse.RSResponseType.Redirect)
            {
                rsResponse = TryConvertToAcceptableContentType(rsResponse);
            }

            //Set headers
            // Logical headers like content type
            _response.ContentType = rsResponse.ContentType.ToString();

            // Additional headers added by the RSResponse
            foreach (var h in rsResponse.AdditionalHeaders)
            {
                _response.Headers.Add(h);
            }
            
            // Feed the bytes using outputstream and close
            _response.ContentLength64 = rsResponse.Bytes.Length;

            Stream output = _response.OutputStream;
            output.Write(rsResponse.Bytes, 0, rsResponse.Bytes.Length);
            output.Close();
        }

        /// <summary>
        /// Convert the response to a client-preferred Content-Type
        /// </summary>
        /// <param name="rsResponse">The response to convert</param>
        /// <returns>A new RSResponse with type RSResponseType.Converted
        /// and Text containing the converted data</returns>
        private RSResponse TryConvertToAcceptableContentType(RSResponse rsResponse)
        {
            //Process the accept types in descending client preference
            foreach (var clientSpecifiedContentTypeOption in rsRequest.AcceptTypes)
            {
                //If the response is in that type already, or can be
                // converted to it, use the type. Otherwise, try next.

                string fromType = rsResponse.ContentType.MediaType;
                string toType = clientSpecifiedContentTypeOption.MediaType;

                if (fromType == toType)
                {
                    Output.Write(String.Format("    Matched {0} (Quality: {1})",
                        toType.ToString(),
                        clientSpecifiedContentTypeOption.Quality.ToString()));

                    _response.ContentType = rsResponse.ContentType.ToString();
                    break;
                }
                else
                {
                    //Try to convert

                    bool canConvert = ConversionGrid.CanConvert(fromType, toType);

                    if (canConvert)
                    {
                        string converterTypeName = ConversionGrid.ConverterFullTypeName(fromType, toType);
                        Type converterType = Type.GetType(converterTypeName);

                        if (converterType == null)
                        {
                            //Converter not found/implemented
                            // Try next Accept preference
                            continue;
                        }

                        try
                        {
                            //Create a converter which is a JsonToXmlConverter (or whatever)
                            // and call its convert method.
                            IConverter converter = (IConverter)Activator.CreateInstance(converterType);
                            rsResponse = converter.Convert(rsResponse);
                            Output.Write("    Converted by " + converterType);
                        }
                        catch (Exception ex)
                        {
                            Output.Write("    Conversion by " + converterType + " failed: " + ex.Message);
                        }

                        //Finished; don't check any more Acceptable types
                        break;
                    }

                }
            }
            return rsResponse;
        }

    }
}
