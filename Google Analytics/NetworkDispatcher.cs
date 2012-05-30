using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net;
using System.Diagnostics;
using SierraLib.Analytics.NetworkInfo;

namespace SierraLib.Analytics.GoogleAnalytics
{
    class NetworkDispatcher : IDispatcher
    {
        const string GoogleAnalyticsHostname = "http://www.google-analytics.com";
        const string GoogleAnalyticsSecureHostname = "https://www.google-analytics.com";
        const int MaxGETLength = 2036;
        const int MaxPOSTLength = 8196;
        const string UserAgentTemplate = "{0}/{1} ({2}; U; {3}; {4}) SierraLib.Analytics/{5} (GoogleAnalytics, like com.google.analytics)";

        public string UserAgent
        {
            get;
            private set;
        }

        public string Hostname
        { get; private set; }
        

        public NetworkDispatcher() : 
            this(Tracker.Product, Tracker.LibraryVersion)
        { }

        public NetworkDispatcher(string productName, string productVersion)
            : this(productName, productVersion, GoogleAnalyticsHostname)
        { }

        public NetworkDispatcher(string productName, string productVersion, string analyticsHost)
        {
            Hostname = analyticsHost;
            UserAgent = String.Format(UserAgentTemplate,
                productName, productVersion,
                OSPlatform(), Environment.OSVersion.Version.ToString(),
                Thread.CurrentThread.CurrentCulture.Name, Tracker.LibraryVersion
                );
        }

        private string OSPlatform()
        {
            switch(Environment.OSVersion.Platform)
            {
                case PlatformID.Unix:
                    return "Linux";
                case PlatformID.MacOSX:
                    return "MacOSX";
                case PlatformID.Win32NT:
                    return "Windows NT " + Environment.OSVersion.Version.ToString(2);
                case PlatformID.Win32Windows:
                case PlatformID.Win32S:
                    return "Windows";
                case PlatformID.WinCE:
                    return "Windows CE";
                case PlatformID.Xbox:
                    return "XBox";
                default:
                    return "Unknown";
            }
        }
        
        bool dispatchBusy = false;
        bool exitDispatch = false;
        object lockObject = new object();
        int dispatchWait = 0;


        WebClient client = null;

        private void sendDispatches(object context)
        {
            Hit[] currentDispatches = (Hit[])context;

            if (dispatchWait > 0)
            {
                if(Tracker.CurrentInstance.Debug)
                    Debug.WriteLine("NetworkDispatcher waiting for " + dispatchWait + " milliseconds");
                Thread.Sleep(dispatchWait);
            }
            dispatchWait = 0;

            dispatchBusy = true;
            exitDispatch = false;

            //Check whether there is an active internet connection, this only works on Vista or greater
            if (Environment.OSVersion.Version.Major >= 6)
            {
                if (!NetworkListManager.IsConnectedToInternet)
                {
                    Debug.WriteLine("No internet connection present, aborting");
                    dispatchBusy = false;
                    return;
                }
            }

            lock (lockObject)
            {
                 

                //Here we lock the dispatch que while we copy the current dispatches into an array
                //for processing
                //lock (dispatchQue) currentDispatches = dispatchQue.ToArray();

                int count = 0;
                foreach (Hit hit in currentDispatches)
                {
                    if (exitDispatch)
                    {
                        //lock (dispatchQue) dispatchQue.RemoveRange(0, count);
                        return;
                    }

                    count++;
                    string requestText = RequestBuilder.AddQueTimeParameter(hit.HitString);
                    if (requestText.Length < MaxGETLength)
                    {
                        //Send the request as an HTTP GET request
                        try
                        {
                            string result = null;
                            if(!Tracker.CurrentInstance.DryRun)
                                result = client.DownloadString(requestText);
                            
                            if(Tracker.CurrentInstance.Debug)
                            {
                                Debug.WriteLine("Dispatched Hit [GET]:");
                                Debug.Indent();
                                Debug.WriteLine(requestText);
                                Debug.Unindent();

                                Debug.WriteLine("Received Response [GET]");
                            }
                            

                            if (HitDispatched != null)
                                HitDispatched(this, hit);

                        }
                        catch(Exception ex)
                        {
                            Debug.WriteLine("Failed to dispatch hit: " + ex.Message);
                        }
                    }
                    else if (requestText.Length < MaxPOSTLength)
                    {
                        try
                        {
                            string result = null;
                            client.Headers.Set(HttpRequestHeader.ContentType, "text/plain");
                            if (!Tracker.CurrentInstance.DryRun)
                                result = client.UploadString("/p", requestText);

                            if (Tracker.CurrentInstance.Debug)
                            {
                                Debug.WriteLine("Dispatched Hit [POST]:");
                                Debug.Indent();
                                Debug.WriteLine(requestText);
                                Debug.Unindent();
                                
                                Debug.WriteLine("Received Response [POST]");
                            }

                            if (HitDispatched != null)
                                HitDispatched(this, hit);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("Failed to dispatch hit: " + ex.Message);
                        }
                    }
                    else
                    {
                        if (Tracker.CurrentInstance.Debug)
                            Debug.WriteLine("Hit was too long (" + requestText.Length + " bytes), not sent");
                        continue;
                    }
                }


                //Upon completing the dispatch, we should remove these items from the dispatch que
                //lock (dispatchQue) dispatchQue.RemoveRange(0,currentDispatches.Length);

                if (DispatchFinished != null)
                    DispatchFinished(this, new EventArgs());

            }
            dispatchBusy = false;
        }




        public void DispatchHits(Hit[] hits, int wait = 0)
        {
            //dispatchQue.AddRange(hits);
            dispatchWait = wait;
            ThreadPool.QueueUserWorkItem(sendDispatches, hits);
        }

        public void Initialize()
        {
            client = new WebClient();
            client.BaseAddress = (Tracker.CurrentInstance.UseSecureConnections ?
                                GoogleAnalyticsSecureHostname : GoogleAnalyticsHostname);
            client.Headers.Set(HttpRequestHeader.UserAgent, UserAgent);
            client.Encoding = Encoding.UTF8;

            if (Tracker.CurrentInstance.Debug)
            {
                Debug.WriteLine("NetworkDispatcher initialized");
                Debug.Indent();
                Debug.WriteLine("Base Address: " + client.BaseAddress);
                Debug.WriteLine("UserAgent: " + UserAgent);
                Debug.Unindent();
            }
        }

        public void Stop()
        {
            exitDispatch = true;
        }

        public bool Busy
        {
            get { return dispatchBusy; }
        }

        /// <summary>
        /// [THREADED] This event is raised each time a hit is dispatched to the Google servers
        /// </summary>
        public event HitDispatchedEventHandler HitDispatched = null;

        /// <summary>
        /// [THREADED] This event is raised upon completion of a dispatch batch
        /// </summary>
        public event EventHandler DispatchFinished = null;
    }
}
