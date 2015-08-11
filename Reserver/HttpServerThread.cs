using ReServer.Conversion;
using ReServer.Core;
using ReServer.Core.Requests;
using ReServer.Core.Responses;
using System;
using System.IO;
using System.Net;
using System.Threading;

namespace ReServer.Server
{
    public class HttpServerThread : RSThread
    {
        private HttpListenerRequest _request;
        private HttpListenerResponse _response;
        private RSRequest rsRequest;

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

            var handler = new RouteHandler(_request.HttpMethod);
            rsRequest = new RSRequest(_request);
            
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
                    // and perform correct action
                    rsResponse = handler.HandleDefaultFileRoute(rsRequest.LocalPath, rsRequest.AcceptTypes);
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
                // or perform actions on it as a resource (depending on HttpMethod)
                rsResponse = handler.HandleFileRoute(rsRequest, _request.InputStream);

            }

            //If request was good and statuscode has not been assigned, assign 200
            if (rsResponse.Satisfied)
            {
                if ((int)rsResponse.StatusCode == 0)
                {
                    rsResponse.StatusCode = HttpStatusCode.OK;
                }
            }
            else
            {
                //If not found, return error or debug info
                //TODO change to 404 response?
                rsResponse = new TextResponse(rsRequest.Website.Local, _request, HttpStatusCode.NotFound);
            }

            //Convert the response to a client-preferred Content-Type
            // if it actually has content (i.e. not a redirect or error message)
            if (rsResponse.HasContent)
            {
                rsResponse = TryConvertToAcceptableContentType(rsResponse);
                _response.ContentType = rsResponse.ContentType.ToString();
            }

            /*
                Copy details to the real HTTP Response object
            */

            // Response code (if set)
            if (rsResponse.StatusCode != 0)
            {
                _response.StatusCode = (int)rsResponse.StatusCode;
            }

            // Additional headers added by the RSResponse
            if (rsResponse.AdditionalHeaders != null)
            {
                foreach (var h in rsResponse.AdditionalHeaders)
                {
                    _response.AppendHeader(h.Key, h.Value);
                }
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
