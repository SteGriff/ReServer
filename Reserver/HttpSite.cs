using ReServer.Core.Configuration;
using ReServer.Models;
using ReServer.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;

namespace ReServer.Core
{
    public class HttpSite : RSThread
    {
        // Continue the listen loop?
        private bool _listen = true;

        // The object which listens for requests...
        private HttpListener _listener;

        // ...to these URIs...
        private IList<Uri> _uris;

        // ...which belong to this website:
        private Site _website;

        /// <summary>
        /// Construct a HyperServer and set its debug output mode
        /// </summary>
        public HttpSite(Site website)
        {
            Output.OutputMode = OutputModeEnum.Console;
            _website = website;
            Output.Write("HttpSite created for " + _website.Name);
        }

        public void Stop()
        {
            Output.Write("Stopping " + _website.Name + "...");
            _listener.Abort();
        }

        /// <summary>
        /// Start the server. This is the main wait loop which handles clients
        /// and creates a new handler thread for each request
        /// </summary>
        public void Start()
        {
            Output.Write(_website.Name + " ready on " + this.Name);

            //Instantiate the listener
            _listener = new HttpListener();

            //Set up client authentication
            //_listener.AuthenticationSchemes = AuthenticationSchemes.Basic;
            //_listener.Realm = 

            // Bind the listener to all of the remote web addresses
            // specified in the config
            _uris = _website.RemoteAddresses.ToList();

            foreach (Uri u in _uris)
            {
                Output.Write("Binding " + _website.Name + " to " + u.AbsoluteUri);
                _listener.Prefixes.Add(u.AbsoluteUri);
            }

            //Start listening for requests!
            _listener.Start();

            while (_listen)
            {
                Output.Write(_website.Name + " ready");

                HttpListenerContext context;

                // Wait for a request (blocking)
                try
                {
                    context = _listener.GetContext();

                    // When a request has been received
                    //  Start a handler on a new thread, with the current HTTP context
                    ThreadPool.QueueUserWorkItem(new WaitCallback(ProvisionNewThread), context);
                }
                catch (HttpListenerException)
                {
                    // We drop in here if the listener is Aborted during use
                    // (i.e. when the user presses 'x' to quit)
                    _listen = false;

                    //End the site
                    return;
                }

            }

        }

        /// <summary>
        /// Container method for starting a new HttpServerThread
        /// </summary>
        /// <param name="httpContext">The HttpListenerContext to pass in to the thread</param>
        private void ProvisionNewThread(object httpContext)
        {
            var handler = new HttpServerThread((HttpListenerContext)httpContext);
            handler.HandleRequest();
        }

    }
}
