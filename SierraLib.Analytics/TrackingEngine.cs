using Akavache;
using RestSharp;
using SierraLib.Analytics.Implementation;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using ReactiveUI;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace SierraLib.Analytics
{
    /// <summary>
    /// Defines the behaviour of a tracking engine. This class is designed to be
    /// the base class for custom tracking engine implementations.
    /// </summary>
    public abstract partial class TrackingEngine
    {
        static TrackingEngine()
        {
            BlobCache.ApplicationName = BlobCache.ApplicationName ?? @"Sierra Softworks\Analytics";
        }

        /// <summary>
        /// Instantiates a <see cref="TrackingEngine"/> instance
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the user attempts to instantiate a <see cref="TrackingEngine"/> type directly
        /// instead of making a call to <see cref="Create"/>. This is to prevent poor programming
        /// practices and having multiple engines active at once with the same ID.
        /// </exception>
        protected TrackingEngine()
        {
            if (!CreatingEngine)
                throw new InvalidOperationException("TrackingEngine instances must be created with TrackingEngine.Create(...).");

            UpdateUserAgent("SierraLib.Analytics", AssemblyInformation.GetAssemblyVersion().ToString(3), "UniversalAnalytics");
            QueueLifeSpan = new TimeSpan(7, 0, 0, 0);
            RetryInterval = new TimeSpan(0, 1, 0);
            Enabled = true;

            RequestQueue.Where(x => x.Engine == this).ObserveOn(TaskPoolScheduler.Default).Subscribe(ProcessRequest);
        }

        #region Storage

        static readonly IBlobCache KeyStore = BlobCache.UserAccount;

        #endregion

        #region UserAgent

        /// <summary>
        /// Updates the UserAgent to use the given values, making use of the standard format
        /// and the <see cref="GetSystemInformationString"/> and <see cref="GetTrackerPlatformString"/>
        /// functions.
        /// </summary>
        /// <param name="product">The name of the product for which the UserAgent is being generated</param>
        /// <param name="version">The version of the product for which the UserAgent is being generated</param>
        /// <param name="features">A number of possible features available within the product</param>
        public void UpdateUserAgent(string product, string version, params string[] features)
        {
            UserAgent = string.Format("{0}/{1} ({2}) {3} {4}", product, version, GetSystemInformationString(), GetTrackerPlatformString(), features.Aggregate((x, y) => x + ' ' + y));
        }

        /// <summary>
        /// Gets a string representation for the System and Browser Details component of the UserAgent string.
        /// http://en.wikipedia.org/wiki/User_agent#Format
        /// </summary>
        /// <returns>System </returns>
        protected virtual string GetSystemInformationString()
        {
            var cpuType = "";
            if (Environment.Is64BitProcess && Environment.Is64BitOperatingSystem) cpuType = "x86_64";
            else if (Environment.Is64BitOperatingSystem) cpuType = "WOW64";
            else cpuType = "x86";

            var osPlatform = "";

            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32Windows:
                    osPlatform = "Windows";
                    break;
                case PlatformID.Win32NT:
                    osPlatform = "Windows NT";
                    break;
                case PlatformID.Win32S:
                    osPlatform = "Windows";
                    break;
                case PlatformID.MacOSX:
                    osPlatform = "Macintosh";
                    break;
                case PlatformID.Unix:
                    osPlatform = "X11; Linux";
                    break;
                case PlatformID.WinCE:
                    osPlatform = "Windows CE";
                    break;
            }

            return string.Format("{0} {1}; {2}; {3}", osPlatform, Environment.OSVersion.Version.ToString(2), cpuType, Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName);
        }

        /// <summary>
        /// Gets a string representation of the tracking platform being used for the Platform and Platform Details components of the UserAgent string.
        /// http://en.wikipedia.org/wiki/User_agent#Format
        /// </summary>
        /// <returns>SierraLib.Analytics/1.0.0 (YourAnalyticsTracker)</returns>
        protected virtual string GetTrackerPlatformString()
        {
            return string.Format("SierraLib.Analytics/{0} ({1})", AssemblyInformation.GetAssemblyVersion().ToString(3), this.GetType().Name);
        }

        #endregion

        #region Properties

        private string _UserAgent = null;
        /// <summary>
        /// Gets or sets the UserAgent used by this <see cref="TrackingEngine"/> for communication
        /// with the tracking server.
        /// </summary>
        public string UserAgent
        {
            get { return _UserAgent; }
            set
            {
                _UserAgent = value;
                if (_restClient != null)
                {
                    _restClient.DefaultParameters.RemoveParameter("User-Agent");
                    _restClient.AddDefaultParameter(new HeaderParameter("User-Agent", _UserAgent));
                }
            }
        }

        /// <summary>
        /// Gets or Sets the <see cref="TimeSpan"/> representing the amount of time before items which
        /// are in the queue will be considered stale and discarded.
        /// </summary>
        /// <remarks>
        /// This is useful in cases where tracking requests might not be sent immediately 
        /// due to network connection restrictions, or because the user closed the application.
        /// By setting this value you can help ensure that you only receive up-to-date information
        /// and can also help keep the cache size down somewhat.
        /// </remarks>
        public TimeSpan QueueLifeSpan
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or Sets the <see cref="TimeSpan"/> representing the amount of time to wait between
        /// repeated attempts to submit failed analytics requests.
        /// </summary>
        public static TimeSpan RetryInterval
        { get; set; }

        /// <summary>
        /// Determines whether or not the current <see cref="TrackingEngine"/> will handle tracking
        /// requests.
        /// </summary>
        /// <remarks>
        /// This allows you to very easily disable tracking for a specific engine (for example, if the
        /// user opts-out of tracking) without requiring any modifications to your existing tracking code.
        /// </remarks>
        public bool Enabled
        { get; set; } = true;

        /// <summary>
        /// Determines whether or not tracking is enabled for any engines.
        /// </summary>
        /// <remarks>
        /// This allows you to very easily disable tracking for all engines (for example, if the user
        /// opts-out of tracking) without requiring any modifications to your existing tracking code.
        /// </remarks>
        public static bool GlobalEnabled
        { get; set; } = true;

        static bool _process = true;
        /// <summary>
        /// Determines whether or not new requests are processed as they come in
        /// </summary>
        /// <remarks>
        /// This allows you to postpone processing of tracking requests until a certain
        /// point or environment state is reached.
        /// </remarks>
        public static bool Process
        {
            get { return _process; }
            set
            {
                _process = value;
                PauseQueue.OnNext(value);
            }
        }

        #endregion

        #region Queue Management

        static readonly Subject<bool> PauseQueue = new Subject<bool>();
        static readonly Subject<PreparedTrackingRequest> RequestQueue = new Subject<PreparedTrackingRequest>();

        /// <summary>
        /// Loads and processes any tracking requests which were not sent
        /// in a previous application session.
        /// </summary>
        /// <remarks>
        /// This is used to transmit requests which failed to send previously and
        /// were added to the persistent store instead. It should ideally be called
        /// before any other tracking requests are made or when no other requests
        /// are in the queue.
        /// </remarks>
        public static void ProcessStoredRequests()
        {
            var activeRequestQueue = RequestQueue.ToEnumerable();
            KeyStore.GetAllObjects<PreparedTrackingRequest>()
                .SubscribeOn(TaskPoolScheduler.Default)
                .Select(x => x.ToObservable())
                .Switch()
                .Where(x => !activeRequestQueue.Any(y => y.RequestID == x.RequestID))
                .Subscribe(request =>
                {
                    request.Engine.OnStoredRequestLoaded(request);
                }, ex =>
                {
                    Debug.WriteLine($"Failed to extract pending tracking requests: {ex.Message}");
                });
        }

        #endregion

        #region Request Processing

        private void ProcessRequest(PreparedTrackingRequest request)
        {
            foreach (var finalizer in request.RequiredFinalizations)
                finalizer.FinalizeRequest(request.Request);

            var response = request.Engine.NetworkClient.Execute(request.Request);

            if (response.ResponseStatus != ResponseStatus.Completed)
                OnRequestFailed(request, response);
            else if (response.StatusCode != System.Net.HttpStatusCode.OK)
                OnRequestFailed(request, response);
            else
                OnRequestTransmitted(request);
        }

        private void OnRequestFailed(PreparedTrackingRequest request, RestResponse response)
        {
            if (response.ResponseStatus != ResponseStatus.Completed || response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                ThreadPool.RegisterWaitForSingleObject(new ManualResetEvent(false), (completed, state) =>
                {
                    RequestQueue.OnNext(request); //Re-queue the item for sending
                }, null, RetryInterval, true);
            }

            Debug.WriteLine("Analytics Request Failed");
            Debug.Indent();
            Debug.WriteLine("{0} {1} {2}", response.ResponseStatus, (int)response.StatusCode, response.StatusCode);
            Debug.WriteLine("{0}", response.Request.Parameters.Select(x => x.Name + "=" + x.Value).Aggregate((x, y) => x + "&" + y));
            if (!response.Content.IsNullOrEmpty())
                Debug.WriteLine("{0}", response.Content);
            Debug.Unindent();
            KeyStore.Invalidate(string.Format("PreparedTrackingRequest_{0}", request.RequestID.ToString()));
        }

        private void OnRequestPrepared(PreparedTrackingRequest request)
        {
            KeyStore.InsertObject($"PreparedTrackingRequest_{request.RequestID}", request, new DateTimeOffset(DateTime.Now.Add(QueueLifeSpan)));

            RequestQueue.OnNext(request);
        }

        private void OnStoredRequestLoaded(PreparedTrackingRequest request)
        {
            RequestQueue.OnNext(request);
        }

        private void OnRequestTransmitted(PreparedTrackingRequest request)
        {
            KeyStore.Invalidate(string.Format("PreparedTrackingRequest_{0}", request.RequestID.ToString()));
        }

        #endregion

        #region Object Implementation

        /// <summary>
        /// Determines whether or not two <see cref="TrackingEngine"/> represent the same tracker
        /// </summary>
        public bool Equals(TrackingEngine obj)
        {
            return GetTrackerID().Equals(obj.GetTrackerID());
        }

        /// <summary>
        /// Determines whether or not two <see cref="TrackingEngine"/> represent the same tracker
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj is TrackingEngine)
                return GetTrackerID().Equals(((TrackingEngine)obj).GetTrackerID());
            return false;
        }

        /// <summary>
        /// Gets a unique hash for this engine instance which is determined by the
        /// account it pushes requests to.
        /// </summary>
        public override int GetHashCode()
        {
            return GetTrackerID().GetHashCode();
        }

        /// <summary>
        /// Returns a string representation of the current tracker
        /// </summary>
        public override string ToString()
        {
            return GetTrackerID();
        }

        #endregion
    }
}
