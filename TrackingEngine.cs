using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SierraLib.Analytics.Implementation;
using RestSharp.Serializers;
using RestSharp.Deserializers;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;

using Akavache;
using ReactiveUI;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Concurrency;
using System.Diagnostics;

namespace SierraLib.Analytics
{
    /// <summary>
    /// Defines the behaviour of a tracking engine. This class is designed to be
    /// the base class for custom tracking engine implementations.
    /// </summary>
    public abstract class TrackingEngine
    {
        static TrackingEngine()
        {
            BlobCache.ApplicationName = @"Sierra Softworks\Analytics";

            RequestQueue.Subscribe(x =>
                Interlocked.Increment(ref ProcessingRequests));
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

            RequestQueue.Where(x => x.Engine == this).ObserveOn(TaskPoolScheduler.Default).Subscribe(ProcessRequest);
        }

        #region Storage

        static readonly IBlobCache KeyStore = BlobCache.UserAccount;

        #endregion

        #region Engine Caching

        [NonSerialized]
        static volatile bool CreatingEngine = false;
        [NonSerialized]
        static readonly object CreatingEngineLock = new object();
        [NonSerialized]
        static Dictionary<string, TrackingEngine> EngineCache = new Dictionary<string, TrackingEngine>();

        /// <summary>
        /// Creates a new instance of the requested <see cref="TrackingEngine"/> type or
        /// returns an existing instance if one has already been instantiated.
        /// </summary>
        /// <typeparam name="TEngine">The type of <see cref="TrackingEngine"/> being instantiated</typeparam>
        /// <param name="account">A unique ID differentiating this particular tracking engine from others, should give the same value as <see cref="TrackingEngine.GetEngineID"/></param>
        /// <param name="initializer">A function to create a new instance of the <typeparamref name="TEngine"/> if one is not yet present</param>
        /// <returns>Returns an instance of the requested <typeparamref name="TEngine"/></returns>
        public static TEngine Create<TEngine>(string account, Func<string, TEngine> initializer) where TEngine : TrackingEngine
        {
            if (EngineCache.ContainsKey(account)) return (TEngine)EngineCache[account];
            else
            {
                lock (CreatingEngineLock) //Prevent multiple threads from instantiating engines at the same time to prevent race conditions on CreatingEngine
                {
                    CreatingEngine = true;
                    EngineCache.Add(account, initializer(account));
                    CreatingEngine = false;
                    if (Default == null)
                        return (TEngine)(Default = EngineCache[account]);
                    else
                        return (TEngine)EngineCache[account];
                }
            }
        }

        #endregion

        #region Engine Access

        /// <summary>
        /// Sets this as the default <see cref="TrackingEngine"/> used when calls are made to
        /// the <see cref="TrackingEngine.TrackDefault"/> methods.
        /// </summary>
        public void SetDefault()
        {
            Default = this;
        }

        /// <summary>
        /// Gets the current default <see cref="TrackingEngine"/>
        /// </summary>
        /// <remarks>
        /// The default <see cref="TrackingEngine"/> can be set by calling
        /// <see cref="TrackingEngine.SetDefault()"/> on the instance of the
        /// engine you wish to set as the <see cref="Default"/>.
        /// </remarks>
        public static TrackingEngine Default
        { get; private set; }
        
        /// <summary>
        /// Gets the <see cref="TrackingEngine"/> attached to the current item.
        /// </summary>
        /// <typeparam name="T">The type of the object on which the engine is attached</typeparam>
        /// <param name="target">An expression returning the target for which the engine should be retrieved</param>
        /// <returns>Returns the <see cref="TrackingEngine"/> attached to the <paramref name="target"/> method</returns>
        public static TrackingEngine GetEngine(Expression<Action> target)
        {
            return target.GetMemberInfo().GetCustomAttribute<TrackingEngineAttributeBase>(true).Engine;
        }

        /// <summary>
        /// Gets the <see cref="TrackingEngine"/> attached to the current item.
        /// </summary>
        /// <typeparam name="T">The type of the object on which the engine is attached</typeparam>
        /// <param name="target">An expression returning the target for which the engine should be retrieved</param>
        /// <returns>Returns the <see cref="TrackingEngine"/> attached to the <paramref name="target"/> method</returns>
        public static TrackingEngine GetEngine<T>(Expression<Action<T>> target)
        {
            return target.GetMemberInfo().GetCustomAttribute<TrackingEngineAttributeBase>(true).Engine;
        }

        /// <summary>
        /// Gets the <see cref="TrackingEngine"/> attached to the current item.
        /// </summary>
        /// <typeparam name="T">The type of the object on which the engine is attached</typeparam>
        /// <param name="target">An expression returning the target for which the engine should be retrieved</param>
        /// <returns>Returns the <see cref="TrackingEngine"/> attached to the <paramref name="target"/> method</returns>
        public static TrackingEngine GetEngine<T>(Expression<Func<T>> target)
        {
            return target.GetMemberInfo().GetCustomAttribute<TrackingEngineAttributeBase>(true).Engine;
        }

        /// <summary>
        /// Gets the <see cref="TrackingEngine"/> attached to the given type.
        /// </summary>
        /// <typeparam name="T">The type of the object on which the engine is attached</typeparam>
        /// <param name="target">An expression returning the target for which the engine should be retrieved</param>
        /// <returns>Returns the <see cref="TrackingEngine"/> attached to the <typeparamref name="T"/>ype</returns>
        public static TrackingEngine GetEngine<T>()
        {
            return typeof(T).GetCustomAttribute<TrackingEngineAttributeBase>(true).Engine;
        }

        /// <summary>
        /// Gets the <see cref="TrackingEngine"/> which reports to the given ID (usually the account code)
        /// </summary>
        /// <param name="engineID">The unique ID used by the engine to identify itself</param>
        /// <returns>Returns the engine which reports the given <paramref name="engineID"/> or <c>null</c> if no such engine was found</returns>
        public static TrackingEngine GetEngineByID(string engineID)
        {
            if (!EngineCache.ContainsKey(engineID))
                return null;
            return EngineCache[engineID];
        }

        private static void CheckDefaultSet()
        {
            if (Default == null)
                throw new InvalidOperationException("You must have a tracking engine attribute present or call <TrackingEngine>.SetDefault before attempting to use this method");
        }

        #endregion

        #region Static Tracking

        /// <summary>
        /// Tracks the given <paramref name="modules"/> for the current <paramref name="application"/> using the <see cref="Default"/> <see cref="TrackingEngine"/>
        /// </summary>
        /// <param name="application">The details of the application making the tracking request</param>
        /// <param name="modules">The <see cref="ITrackingModule"/>s being used to generate the request</param>
        public static void TrackDefault(ITrackingApplication application, params ITrackingModule[] modules)
        {
            CheckDefaultSet();
            Default.Track(application, modules);
        }

        /// <summary>
        /// Tracks the given <paramref name="modules"/> for the current <paramref name="application"/> using the <see cref="Default"/> <see cref="TrackingEngine"/>
        /// </summary>
        /// <param name="application">The details of the application making the tracking request</param>
        /// <param name="modules">The <see cref="ITrackingModule"/>s being used to generate the request</param>
        public static void TrackDefault(ITrackingApplication application, IEnumerable<ITrackingModule> modules)
        {
            CheckDefaultSet();
            Default.Track(application, modules);
        }

        /// <summary>
        /// Tracks the <paramref name="modules"/> for the <see cref="triggerMethod"/> using the <see cref="Default"/> <see cref="TrackingEngine"/>
        /// or the inherited <see cref="TrackingEngineAttributeBase"/> attribute's value if present.
        /// </summary>
        /// <param name="triggerMethod">The method which is being tracked</param>
        /// <param name="triggerType">The point in the method which is being tracked</param>
        /// <param name="modules">The additional <see cref="ITrackingModule"/>s to add to those tracked for the method</param>
        /// <remarks>
        /// This method will attempt to use the <see cref="TrackingEngine"/> specified by an attached attribute on either the
        /// method or one of its ancestors as the tracking engine. If no such attribute is found, then the <see cref="Default"/>
        /// <see cref="TrackingEngine"/> will be used instead.
        /// </remarks>
        public static void TrackDefault(Expression<Action> triggerMethod, TrackOn triggerType = TrackOn.Entry, params ITrackingModule[] modules)
        {
            var method = triggerMethod.GetMemberInfo();
            var engineAttributes = method.GetCustomAttributes<TrackingEngineAttributeBase>(true);
            if (engineAttributes.Any())
                engineAttributes.First().Engine.Track(triggerMethod, triggerType, modules);
            else
            {
                CheckDefaultSet();
                Default.Track(triggerMethod, triggerType, modules);
            }
        }

        /// <summary>
        /// Tracks the <paramref name="modules"/> for the <see cref="triggerMethod"/> using the <see cref="Default"/> <see cref="TrackingEngine"/>
        /// or the inherited <see cref="TrackingEngineAttributeBase"/> attribute's value if present.
        /// </summary>
        /// <param name="triggerMethod">The method which is being tracked</param>
        /// <param name="triggerType">The point in the method which is being tracked</param>
        /// <param name="modules">The additional <see cref="ITrackingModule"/>s to add to those tracked for the method</param>
        /// <remarks>
        /// This method will attempt to use the <see cref="TrackingEngine"/> specified by an attached attribute on either the
        /// method or one of its ancestors as the tracking engine. If no such attribute is found, then the <see cref="Default"/>
        /// <see cref="TrackingEngine"/> will be used instead.
        /// </remarks>
        public static void TrackDefault<T>(Expression<Action<T>> triggerMethod, TrackOn triggerType = TrackOn.Entry, params ITrackingModule[] modules)
        {
            var method = triggerMethod.GetMemberInfo();
            var engineAttributes = method.GetCustomAttributes<TrackingEngineAttributeBase>(true);
            if (engineAttributes.Any())
                engineAttributes.First().Engine.Track(triggerMethod, triggerType, modules);
            else
            {
                CheckDefaultSet();
                Default.Track(triggerMethod, triggerType, modules);
            }
        }

        /// <summary>
        /// Tracks the <paramref name="modules"/> for the <see cref="triggerMethod"/> using the <see cref="Default"/> <see cref="TrackingEngine"/>
        /// or the inherited <see cref="TrackingEngineAttributeBase"/> attribute's value if present.
        /// </summary>
        /// <param name="triggerMethod">The method which is being tracked</param>
        /// <param name="triggerType">The point in the method which is being tracked</param>
        /// <param name="modules">The additional <see cref="ITrackingModule"/>s to add to those tracked for the method</param>
        /// <remarks>
        /// This method will attempt to use the <see cref="TrackingEngine"/> specified by an attached attribute on either the
        /// method or one of its ancestors as the tracking engine. If no such attribute is found, then the <see cref="Default"/>
        /// <see cref="TrackingEngine"/> will be used instead.
        /// </remarks>
        public static void TrackDefault<T>(Expression<Func<T>> triggerMethod, TrackOn triggerType = TrackOn.Entry, params ITrackingModule[] modules)
        {
            var method = triggerMethod.GetMemberInfo();
            var engineAttributes = method.GetCustomAttributes<TrackingEngineAttributeBase>(true);
            if (engineAttributes.Any())
                engineAttributes.First().Engine.Track(triggerMethod, triggerType, modules);
            else
            {
                CheckDefaultSet();
                Default.Track(triggerMethod, triggerType, modules);
            }
        }

        #endregion
        
        #region Standard Tracking
        
        /// <summary>
        /// Tracks the given <paramref name="modules"/> for the current <paramref name="application"/> using the current <see cref="TrackingEngine"/>
        /// </summary>
        /// <param name="application">The details of the application making the tracking request</param>
        /// <param name="modules">The <see cref="ITrackingModule"/>s being used to generate the request</param>
        public void Track(ITrackingApplication application, params ITrackingModule[] modules)
        {
            Track(application, modules as IEnumerable<ITrackingModule>);
        }

        /// <summary>
        /// Tracks the given <paramref name="modules"/> for the current <paramref name="application"/> using the current <see cref="TrackingEngine"/>
        /// </summary>
        /// <param name="application">The details of the application making the tracking request</param>
        /// <param name="modules">The <see cref="ITrackingModule"/>s being used to generate the request</param>
        public void Track(ITrackingApplication application, IEnumerable<ITrackingModule> modules)
        {
            //Check that we have a valid UserAgent string, if not then load a default one
            if (UserAgent.IsNullOrWhitespace())            
                UpdateUserAgent(application.Name, application.Version);
            
            var request = CreateRequest(application);
            PreProcess(request);

            List<ITrackingFinalize> requiringFinalization = new List<ITrackingFinalize>();
            
            foreach(var module in modules)
            {
                module.PreProcess(request);
                if (module is ITrackingFinalize)
                    requiringFinalization.Add(module as ITrackingFinalize);
            }

            PostProcess(request);

            var preparedRequest = PrepareRequest(request, requiringFinalization);

            OnRequestPrepared(preparedRequest);
        }

        /// <summary>
        /// Tracks the <paramref name="modules"/> for the <see cref="triggerMethod"/> using the current <see cref="TrackingEngine"/>
        /// or the inherited <see cref="TrackingEngineAttributeBase"/> attribute's value if present.
        /// </summary>
        /// <param name="triggerMethod">The method which is being tracked</param>
        /// <param name="triggerType">The point in the method which is being tracked</param>
        /// <param name="modules">The additional <see cref="ITrackingModule"/>s to add to those tracked for the method</param>
        /// <remarks>
        /// This method will attempt to use the <see cref="TrackingEngine"/> specified by an attached attribute on either the
        /// method or one of its ancestors as the tracking engine. If no such attribute is found, then the current
        /// <see cref="TrackingEngine"/> will be used instead.
        /// </remarks>
        public void Track(Expression<Action> triggerMethod, TrackOn triggerType = TrackOn.Entry, params ITrackingModule[] modules)
        {
            Track(triggerMethod.GetMemberInfo(), triggerType, modules);
        }

        /// <summary>
        /// Tracks the <paramref name="modules"/> for the <see cref="triggerMethod"/> using the current <see cref="TrackingEngine"/>
        /// or the inherited <see cref="TrackingEngineAttributeBase"/> attribute's value if present.
        /// </summary>
        /// <param name="triggerMethod">The method which is being tracked</param>
        /// <param name="triggerType">The point in the method which is being tracked</param>
        /// <param name="modules">The additional <see cref="ITrackingModule"/>s to add to those tracked for the method</param>
        /// <remarks>
        /// This method will attempt to use the <see cref="TrackingEngine"/> specified by an attached attribute on either the
        /// method or one of its ancestors as the tracking engine. If no such attribute is found, then the current
        /// <see cref="TrackingEngine"/> will be used instead.
        /// </remarks>
        public void Track<T>(Expression<Action<T>> triggerMethod, TrackOn triggerType = TrackOn.Entry, params ITrackingModule[] modules)
        {
            Track(triggerMethod.GetMemberInfo(), triggerType, modules);
        }

        /// <summary>
        /// Tracks the <paramref name="modules"/> for the <see cref="triggerMethod"/> using the current <see cref="TrackingEngine"/>
        /// or the inherited <see cref="TrackingEngineAttributeBase"/> attribute's value if present.
        /// </summary>
        /// <param name="triggerMethod">The method which is being tracked</param>
        /// <param name="triggerType">The point in the method which is being tracked</param>
        /// <param name="modules">The additional <see cref="ITrackingModule"/>s to add to those tracked for the method</param>
        /// <remarks>
        /// This method will attempt to use the <see cref="TrackingEngine"/> specified by an attached attribute on either the
        /// method or one of its ancestors as the tracking engine. If no such attribute is found, then the current
        /// <see cref="TrackingEngine"/> will be used instead.
        /// </remarks>
        public void Track<T>(Expression<Func<T>> triggerMethod, TrackOn triggerType = TrackOn.Entry, params ITrackingModule[] modules)
        {
            Track(triggerMethod.GetMemberInfo(), triggerType, modules);
        }


        private void Track(MemberInfo method, TrackOn triggerType, ITrackingModule[] modules)
        {
            var engineAttributes = method.GetCustomAttributes<TrackingEngineAttributeBase>(true);

            var application = method.GetCustomAttribute<TrackingApplicationAttribute>(true) as ITrackingApplication;
            var dataBundle = method.GetCustomAttributes<TrackingModuleAttributeBase>(true).Where(x => x.Filter.HasFlag(triggerType)).Concat(modules).ToArray();

            if (engineAttributes.Any())
                engineAttributes.First().Engine.Track(application, dataBundle);
            else
                Track(application, dataBundle);
        }

        #endregion

        #region Methods
        
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
            UserAgent = string.Format("{0}/{1} ({2}) {3} {4}", product, version, GetSystemInformationString(), GetTrackerPlatformString(), features.Aggregate((x,y) => x + ' ' + y));
        }

        /// <summary>
        /// Gets a string representation for the System and Browser Details component of the UserAgent string.
        /// http://en.wikipedia.org/wiki/User_agent#Format
        /// </summary>
        /// <returns>System </returns>
        protected virtual string GetSystemInformationString()
        {
            var system = "";

            var cpuType = "";

            var osPlatform =    
                (Environment.OSVersion.Platform & (PlatformID.Win32Windows | PlatformID.Win32S | PlatformID.Win32NT | PlatformID.WinCE)) != 0 ? "Windows" :
                (Environment.OSVersion.Platform & PlatformID.Unix) != 0 ? "Unix" :
                (Environment.OSVersion.Platform == PlatformID.MacOSX) ? "OS" : "Other";
                       
            return string.Format("{0}; {1} {2} {3}; {4}", 
                system, cpuType, osPlatform, Environment.OSVersion.Version.ToString(), Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName);
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
                    _restClient.UserAgent = _UserAgent;
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

        #endregion
        
        #region Network

        private IRestClient _restClient = null;
        /// <summary>
        /// Gets the <see cref="IRestClient"/> used to send requests
        /// to the tracking server.
        /// </summary>
        protected IRestClient NetworkClient
        {
            get
            {
                if (_restClient == null)
                {
                    _restClient = CreateNetworkClient(UserAgent);
                }

                return _restClient;
            }
        }

        /// <summary>
        /// Creates a <see cref="IRestClient"/> which will be used
        /// by the <see cref="TrackingManager"/> for tracking packet
        /// submissions.
        /// </summary>
        /// <param name="userAgent">The default UserAgent string to use for the client</param>
        /// <returns>
        /// Returns a <see cref="IRestClient"/> configured for sending
        /// tracking data for this <see cref="TrackingEngine"/> instance.
        /// </returns>
        protected abstract IRestClient CreateNetworkClient(string userAgent);

        /// <summary>
        /// Requests that a new <see cref="NetworkClient"/> is created for this
        /// engine, allowing changes to be made to the client used by subsequent requests.
        /// </summary>
        protected void RequestNewClient()
        {
            _restClient = null;
        }

        #endregion

        #region Request Preperation

        /// <summary>
        /// Creates a <see cref="IRestRequest"/> for the given application context
        /// which will be used to generate the tracking request for the engine.
        /// </summary>
        /// <param name="application">The application context which generated the tracking request</param>
        /// <returns></returns>
        protected abstract IRestRequest CreateRequest(ITrackingApplication application);

        /// <summary>
        /// Allows the engine to pre-process a tracking request before it is handed
        /// off to the <see cref="ITrackingModule"/>s for population.
        /// </summary>
        /// <param name="request">The request being used for the current tracking hit</param>
        protected virtual void PreProcess(IRestRequest request)
        {
            
        }

        /// <summary>
        /// Allows the engine to post-process a tracking request after it has been
        /// populated by all <see cref="ITrackingModule"/>s.
        /// </summary>
        /// <param name="request">The request being used for the current tracking hit</param>
        protected virtual void PostProcess(IRestRequest request)
        {
            
        }

        /// <summary>
        /// Generates the <see cref="PreparedTrackingRequest"/> object which represents
        /// a tracking request which is ready for queuing.
        /// </summary>
        /// <param name="request">The populated and post-processed request waiting to be sent</param>
        /// <param name="finalizationQueue">A number of modules which require the ability to finalize a request prior to transmission</param>
        /// <returns>Returns a <see cref="PreparedTrackingRequest"/> object tailored to the engine's specific requirements</returns>
        protected abstract PreparedTrackingRequest PrepareRequest(IRestRequest request, IEnumerable<ITrackingFinalize> finalizationQueue);

        #endregion
        
        #region Client Identification

        /// <summary>
        /// Gets a unique identifier for this tracker instance - determined by the account it
        /// submits its data to.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// This is used to link disk queued tracking requests to their respective engines.
        /// </remarks>
        protected abstract string GetTrackerID();

        /// <summary>
        /// Gets a unique identifier for the current user
        /// </summary>
        /// <param name="application">The application for which the client ID should be retrieved</param>
        /// <returns>Returns the unique identifier representing the currently active user</returns>
        /// <remarks>
        /// Each client ID is unique to a computer, user account, <see cref="ITrackingApplication"/>, <see cref="TrackingEngine"/> and
        /// tracker ID given by <see cref="GetTrackerID()"/> (provided that the <see cref="CreateNewClientID"/> function returns
        /// unique values).
        /// 
        /// If a client ID is not found for the current combination of the above then one will be generated by making a call to
        /// <see cref="CreateNewClientID"/>.
        /// </remarks>
        protected string GetClientID(ITrackingApplication application)
        {
            var clientIDKey = string.Format("{0}:{1}:{2}", this.GetType().FullName, application.Name, GetTrackerID());
            return KeyStore.GetOrFetchObject<string>(clientIDKey, () => Task<string>.FromResult(CreateNewClientID(application))).First();
        }

        protected abstract string CreateNewClientID(ITrackingApplication application);

        #endregion

        #region Queue Management

        static readonly Subject<PreparedTrackingRequest> RequestQueue = new Subject<PreparedTrackingRequest>();
        static int ProcessingRequests = 0;


        /// <summary>
        /// Determines whether or not the tracking engine is currently busy processing requests.
        /// </summary>
        /// <remarks>
        /// This property can be used to wait for the global request queue to become empty, however
        /// it is generally easier to just call <see cref="TrackingEngine.WaitForPending"/>.
        /// </remarks>
        public static bool PendingRequests
        {
            get { return ProcessingRequests > 0; }
        }


        /// <summary>
        /// Waits for any <see cref="PendingRequests"/> to complete before returning
        /// </summary>
        public static void WaitForPending()
        {
            while (PendingRequests)
                Thread.Sleep(10);
        }

        /// <summary>
        /// Waits for any <see cref="PendingRequests"/> to complete before returning
        /// </summary>
        /// <param name="timeout">
        /// The amount of time to wait for <see cref="PendingRequests"/> to process
        /// before giving up.
        /// </param>
        public static void WaitForPending(TimeSpan timeout)
        {
            var finalTime = DateTime.Now.Add(timeout);
            while (PendingRequests && DateTime.Now < finalTime)
                Thread.Sleep(10);
        }

        private void ProcessRequest(PreparedTrackingRequest request)
        {
            foreach (var finalizer in request.RequiredFinalizations)
                finalizer.Finalize(request.Request);
            
            var response = request.Engine.NetworkClient.Execute(request.Request);

            if (response.ResponseStatus != ResponseStatus.Completed)
                OnRequestFailed(request, response);
            else if (response.StatusCode != System.Net.HttpStatusCode.OK)
                OnRequestFailed(request, response);
            else
                OnRequestTransmitted(request);

            Interlocked.Decrement(ref ProcessingRequests);
        }

        private void OnRequestFailed(PreparedTrackingRequest request, IRestResponse response)
        {
            if (response.ResponseStatus != ResponseStatus.Completed)
            {
                Thread.Sleep(RetryInterval);
                RequestQueue.OnNext(request); //Re-queue the item for sending
            }

            Debug.WriteLine("Analytics Request Failed");
            Debug.Indent();
            Debug.WriteLine("{0} {1} {2}", response.ResponseStatus, (int)response.StatusCode, response.StatusCode);
            Debug.WriteLine("{0}", response.Request.Parameters.Select(x => x.Name + "=" + x.Value).Aggregate((x, y) => x + "&" + y));
            if (!response.Content.IsNullOrEmpty())
                Debug.WriteLine("{0}", response.Content);
            Debug.Unindent();
            KeyStore.Invalidate(request.RequestID.ToString());
        }

        private void OnRequestPrepared(PreparedTrackingRequest request)
        {
            KeyStore.Insert(request.RequestID.ToString(), request.Serialize(), new DateTimeOffset(DateTime.Now.Add(QueueLifeSpan)));

            RequestQueue.OnNext(request);
        }

        private void OnRequestTransmitted(PreparedTrackingRequest request)
        {
            KeyStore.Invalidate(request.RequestID.ToString());
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
            if(obj is TrackingEngine)
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
