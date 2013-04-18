using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;
using System.Linq.Expressions;
using SierraLib.Analytics.Implementation;

namespace SierraLib.Analytics
{
    #region Generic Tracking Attributes
    
    /// <summary>
    /// Indicates that attached elements shouldn't be tracked by linked
    /// tracking engines
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Method, Inherited = true)]
    public sealed class DontTrackAttribute : MethodDontWrapAttribute
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


        private TrackingEngine _engine;
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

        /// <summary>
        /// The name of the application being tracked
        /// </summary>
        public string Name
        { get; set; }

        /// <summary>
        /// The version of the application being tracked
        /// </summary>
        public string Version
        { get; set; }
    }
    
    #endregion

    #region Tracking Trigger Attributes

    [AttributeUsage(AttributeTargets.Method, Inherited = true)]
    public class TrackOnEntryAttribute : MethodWrapperAttribute
    {
        protected object[] Parameters { get; private set; }

        public override void OnEntry(MethodBase method, object[] parameters)
        {
            var engine = method.GetCustomAttribute<TrackingEngineAttributeBase>(true).Engine;
            var application = method.GetCustomAttribute<TrackingApplicationAttribute>(true) as ITrackingApplication;
            var dataBundle = method.GetCustomAttributes<TrackingModuleAttributeBase>(true).Where(x => (x.Filter & TrackOn.Entry) != 0).ToArray();

            Parameters = parameters;

            engine.Track(application, dataBundle);
        }
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = true)]
    public class TrackOnExitAttribute : MethodWrapperAttribute
    {
        protected object[] Parameters { get; private set; }
        protected object Result { get; private set; }

        public override void OnExit(MethodBase method, object[] parameters, object result)
        {
            var engine = method.GetCustomAttribute<TrackingEngineAttributeBase>(true).Engine;
            var application = method.GetCustomAttribute<TrackingApplicationAttribute>(true) as ITrackingApplication;
            var dataBundle = method.GetCustomAttributes<TrackingModuleAttributeBase>(true).Where(x => (x.Filter & TrackOn.Exit) != 0).ToArray();

            Parameters = parameters;
            Result = result;

            engine.Track(application, dataBundle);
        }
    }

    #endregion
}
