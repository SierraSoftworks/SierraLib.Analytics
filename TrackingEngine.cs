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

namespace SierraLib.Analytics
{
    /// <summary>
    /// Defines the behaviour of a tracking engine. This class is designed to be
    /// the base class for custom tracking engine implementations.
    /// </summary>
    public abstract class TrackingEngine
    {
        public TrackingEngine()
        {
            UserAgent = string.Format("SierraLib.Analytics/v{0}", AssemblyInformation.GetAssemblyVersion().ToString(3));
        }

        #region Static Tracking

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

        private static void CheckDefaultSet()
        {
            if (Default == null)
                throw new InvalidOperationException("You must have a tracking engine attribute present or call <TrackingEngine>.SetDefault before attempting to use this method");
        }

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
            var dataBundle = method.GetCustomAttributes<TrackingModuleAttributeBase>(true).Where(x => (x.Filter & triggerType) != 0).Concat(modules).ToArray();

            if (engineAttributes.Any())
                engineAttributes.First().Engine.Track(application, dataBundle);
            else
                Track(application, dataBundle);
        }

        #endregion

        #region Methods
        
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

        #endregion
        
        #region Network

        private IRestClient _restClient = null;
        /// <summary>
        /// Gets the <see cref="IRestClient"/> used to send requests
        /// to the tracking server.
        /// </summary>
        public IRestClient NetworkClient
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

        protected string GetClientID(ITrackingApplication application)
        {
            var storeFile = new FileInfo(Environment.ExpandEnvironmentVariables(@"%AppData%\Sierra Softworks\Analytics\Store.json"));
            if (storeFile.Exists)
            {
                var deserializer = new JsonDeserializer();

                var storeJSON = "";
                using (var fileData = storeFile.OpenText())
                    storeJSON = fileData.ReadToEnd();

                var store = deserializer.Deserialize<TrackingStore>(new RestResponse() { Content = storeJSON });
                if (store.ClientIDs.Any(x => x.Key == application.Name))
                    return store.ClientIDs.First(x => x.Key == application.Name).Value;
                else
                {
                    var newClientID = CreateNewClientID(application);
                    store.ClientIDs.Add(new TrackingPair()
                    {
                        Key = application.Name,
                        Value = newClientID
                    });

                    var serializer = new JsonSerializer();
                    storeJSON = serializer.Serialize(store);

                    storeFile.Delete();
                    using (var file = storeFile.OpenWrite())
                    using (var fileData = new StreamWriter(file))
                        fileData.Write(storeJSON);

                    return newClientID;
                }
            }
            else
            {
                var store = new TrackingStore();

                var newClientID = CreateNewClientID(application);
                store.ClientIDs.Add(new TrackingPair()
                {
                    Key = application.Name,
                    Value = newClientID
                });

                var serializer = new JsonSerializer();
                var storeJSON = serializer.Serialize(store);

                storeFile.Delete();
                using (var file = storeFile.OpenWrite())
                using (var fileData = new StreamWriter(file))
                    fileData.Write(storeJSON);

                return newClientID;
            }
        }

        protected abstract string CreateNewClientID(ITrackingApplication application);

        #endregion

        #region Queue Management

        private void OnRequestPrepared(PreparedTrackingRequest request)
        {
            
        }

        private void OnRequestTransmitted(PreparedTrackingRequest request)
        {

        }

        #endregion
    }

    [Serializable]
    class TrackingStore
    {
        bool Enabled = true;
        public List<TrackingPair> ClientIDs = new List<TrackingPair>();
        public List<TrackingPair> Settings = new List<TrackingPair>();
    }

    [Serializable]
    class TrackingPair
    {
        public string Key;
        public string Value;
    }
}
