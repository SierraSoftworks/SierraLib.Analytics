using AspectInjector.Broker;
using RestSharp;
using SierraLib.Analytics.Implementation;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SierraLib.Analytics
{
    #region Generic Tracking Attributes

    /// <summary>
    /// Indicates that attached elements shouldn't be tracked by linked
    /// tracking engines
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Method, Inherited = true)]
    public sealed class DontTrackAttribute : Attribute
    {

    }

    /// <summary>
    /// Determines the <see cref="TrackingEngine"/> used to track events
    /// </summary>
    [Serializable]
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly, Inherited = true)]
    public abstract class TrackingEngineAttributeBase : Attribute
    {
        public TrackingEngineAttributeBase()
        {

        }

        /// <summary>
        /// Used to create a new <see cref="TrackingEngine"/> instance if one hasn't yet
        /// been created.
        /// </summary>
        /// <returns>
        /// Returns a new <see cref="TrackingEngine"/> instance which will be cached for
        /// future <see cref="TrackingEngineAttrbuteBase"/> instances.
        /// </returns>
        protected abstract TrackingEngine CreateEngine();

        /// <summary>
        /// Gets an identifier which should be unique to each different tracking engine
        /// configuration within your application, should give the same value as <see cref="TrackingEngine.GetEngineID"/>.
        /// </summary>
        /// <returns>Returns an identifier for the engine described by the current <see cref="TrackingEngineAttributeBase"/> implementation</returns>
        protected abstract string GetEngineID();


        private TrackingEngine _engine = null;

        /// <summary>
        /// The name of the application being tracked
        /// </summary>
        public TrackingEngine Engine
        {
            get
            {
                if (_engine == null)
                    _engine = TrackingEngine.Create(GetEngineID(), x => CreateEngine());
                return _engine;
            }
        }
    }

    /// <summary>
    /// Describes the application being tracked to the <see cref="TrackingEngine"/>
    /// </summary>
    [Serializable]
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly, Inherited = true)]
    public sealed class TrackingApplicationAttribute : Attribute, ITrackingApplication
    {
        public TrackingApplicationAttribute()
        {
            var a = Assembly.GetEntryAssembly();

            if (a != null)
            {
                Name = AssemblyInformation.GetAssemblyTitle(a);
                Version = AssemblyInformation.GetAssemblyVersion(a).ToString(3);
            }
        }


        /// <inheritdoc/>
        public string Name
        { get; set; }

        /// <inheritdoc/>
        public string Version
        { get; set; }
    }

    #endregion

    #region Tracking Trigger Attributes

    /// <summary>
    /// Causes a tracking event to be submitted before this method is executed.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = true)]
    public class TrackOnEntryAttribute : TrackOnAttributeBase
    {
        /// <inheritdoc/>
        public override void OnEntry(MethodBase method, object[] parameters)
        {
            var engine = method.GetTrackingEngine();
            var application = method.GetCustomAttribute<TrackingApplicationAttribute>(true) as ITrackingApplication;
            var dataBundle = method.GetCustomAttributes<TrackingModuleAttributeBase>(true).Where(x => (x.Filter & TrackOn.Entry) != 0).ToArray();

            engine?.TrackAsync(application, dataBundle);
        }
    }

    /// <summary>
    /// Causes a tracking event to be submitted after this method is executed.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = true)]
    public class TrackOnExitAttribute : TrackOnAttributeBase
    {
        /// <inheritdoc/>
        public override void OnExit(MethodBase method, object[] parameters, object result)
        {
            var engine = method.GetTrackingEngine();
            var application = method.GetCustomAttribute<TrackingApplicationAttribute>(true) as ITrackingApplication;
            var dataBundle = method.GetCustomAttributes<TrackingModuleAttributeBase>(true).Where(x => (x.Filter & TrackOn.Exit) != 0).ToArray();

            engine?.TrackAsync(application, dataBundle);
        }
    }

    /// <summary>
    /// Causes a tracking event to be generated each time an exception is thrown within this method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = true, Inherited = true)]
    public class TrackOnExceptionAttribute : TrackOnAttributeBase
    {
        /// <summary>
        /// Gets or sets the <see cref="Type"/> of exceptions which should be reported by this tracker.
        /// </summary>
        public Type ExceptionFilter
        { get; set; }

        /// <inheritdoc/>
        public override void OnException(MethodBase method, object[] parameters, Exception ex)
        {
            if (ExceptionFilter != null && !ex.GetType().IsSubclassOf(ExceptionFilter))
                return;

            var engine = method.GetTrackingEngine();
            var application = method.GetCustomAttribute<TrackingApplicationAttribute>(true) as ITrackingApplication;
            var dataBundle = method.GetCustomAttributes<TrackingModuleAttributeBase>(true).Where(x => (x.Filter & TrackOn.Exception) != 0)
                .Cast<ITrackingModule>()
                .Append(new CallbackTrackingModule(request => this.PreProcessRequest(request, method, parameters, ex))).ToArray();

            engine?.TrackAsync(application, dataBundle);
        }

        /// <summary>
        /// Overridden in a derived class to customize how the exception is reported.
        /// </summary>
        /// <param name="request">The <see cref="IRestRequest"/> responsible for reporting the exception.</param>
        /// <param name="method">The method within which the exception was thrown.</param>
        /// <param name="parameters">The parameters which were passed to the method.</param>
        /// <param name="ex">The <see cref="Exception"/> which was thrown.</param>
        protected virtual void PreProcessRequest(IRestRequest request, MethodBase method, object[] parameters, Exception ex)
        {

        }

        private class CallbackTrackingModule : ITrackingModule
        {
            private readonly Action<IRestRequest> preProcess;

            public CallbackTrackingModule(Action<IRestRequest> preProcess)
            {
                this.preProcess = preProcess;
            }

            public void PreProcess(IRestRequest request)
            {
                this.preProcess?.Invoke(request);
            }
        }
    }

    #endregion
}
