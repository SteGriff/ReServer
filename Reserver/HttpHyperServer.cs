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
    public class HttpHyperServer
    {
        // Continue the listen loop?
        private bool _listen = true;

        // The listening site objects
        private List<HttpSite> _sites = new List<HttpSite>();

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

            Output.Write("Starting sites...");
            foreach (Site s in Config.Server.Sites)
            {
                HttpSite httpSite = new HttpSite(s);

                // Update internal list of HttpSites
                _sites.Add(httpSite);

                // Start the HttpSite, on its own thread
                ThreadPool.QueueUserWorkItem(new WaitCallback(StartSiteThread), httpSite);                
            }

            Output.Write("All sites up!");

            // We do no work in this thread from here on,
            // so let's reduce priority
            Thread.CurrentThread.Priority = ThreadPriority.Lowest;

            while (_listen)
            {
                // We're waiting for an 'x' key press (to quit)
                // Reduce duty cycle by sleeping
                Thread.Sleep(1000);
            }

            // We have quit - kill every HttpSite
            foreach (HttpSite s in _sites)
            {
                s.Stop();
            }

            Output.Write("All sites stopped.");
            Output.Write("You can close ReServer.");

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
            _listen = false;
        }

        /// <summary>
        /// Start the listener for the passed-in HttpSite.
        /// This is a blocking method; run it on its own Thread to provide availability
        /// </summary>
        /// <param name="httpSite">The HttpSite to Start</param>
        private void StartSiteThread(object httpSite)
        {
            var theSite = (HttpSite)httpSite;
            theSite.Start();
        }

    }
}
