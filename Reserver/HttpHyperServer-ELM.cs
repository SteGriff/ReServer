using ReServer.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;

namespace ReServer.Core
{
    public class HttpHyperServer
    {
        // Continue the listen loop?
        private bool _listen = true;

        // Declare listener and attached domains
        private HttpListener _listener;
        private IList<Uri> _uris;

        /// <summary>
        /// Construct a HyperServer and set its debug output mode
        /// </summary>
        public HttpHyperServer()
        {
            Output.OutputMode = OutputModeEnum.Console;
            Output.Write("HttpHyperServer created");
        }

        /// <summary>
        /// Start the server. This is the main wait loop which handles clients
        /// and creates a new handler thread for each request
        /// </summary>
        public void Start()
        {
            Output.Write("HttpHyperServer started");
            _listen = true;

            // Start the thread which allows the user to quit by pressing 'x'
            ThreadPool.QueueUserWorkItem(new WaitCallback(AcceptUserInterrupt));

            //Initialise the config and check that it worked
            var initialised = Config.Initialise();

            if (!initialised)
            {
                Output.Write(Config.Error);
                return;
            }

            //Instantiate the listener
            _listener = new HttpListener();


            // Bind the listener to all of the remote web addresses
            // specified in the config
            _uris = Config.Server.Sites.SelectMany(s => s.RemoteAddresses).ToList();

            Output.Write("Binding remote URLs:");
            foreach (Uri u in _uris)
            {
                Output.Write("\t" + u.AbsoluteUri);
                _listener.Prefixes.Add(u.AbsoluteUri);
            }
            
            //Start listening for requests!
            _listener.Start();

            while (_listen)
            {
                Output.Write("HttpHyperServer waiting");

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

                    //End the program
                    return;
                }
           
            }

        }

        /// <summary>
        /// Runs on its own thread; allows the user to quit the program
        /// </summary>
        /// <param name="state">Not used</param>
        private void AcceptUserInterrupt(object state)
        {
            Output.Write("Press 'x' key to stop server");

            // Loop until x key is received
            // (writing to the console feeds ReadKey with a \0 character)

            ConsoleKeyInfo key = Console.ReadKey();
            while(key.KeyChar != 'x')
            {
               key = Console.ReadKey();
            }

            //Received x key
            _listener.Abort();
        }

        /// <summary>
        /// Container method for starting a new HttpServerThread
        /// </summary>
        /// <param name="httpContext">The HttpListenerContext to pass in to the thread</param>
        private void ProvisionNewThread(object httpContext)
        {
            var handler = new HttpServerThread((HttpListenerContext) httpContext);
            handler.HandleRequest();
        }



    }
}
