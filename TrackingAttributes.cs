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
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class DontTrackAttribute : MethodDontWrapAttribute
    {

    }

    /// <summary>
    /// Determines the <see cref="TrackingEngine"/> used to track events
    /// </summary>
    [Serializable]
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly)]
    public abstract class TrackingEngineAttributeBase : Attribute
    {
        public TrackingEngineAttributeBase()
        {
            
        }

        protected abstract TrackingEngine CreateEngine();


        private TrackingEngine _engine;
        /// <summary>
        /// The name of the application being tracked
        /// </summary>
        public TrackingEngine Engine
        {
            get
            {
                if (_engine == null)
                    _engine = CreateEngine();
                return _engine;
            }
        }
    }

    /// <summary>
    /// Describes the application being tracked to the <see cref="TrackingEngine"/>
    /// </summary>
    [Serializable]
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly)]
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

    [AttributeUsage(AttributeTargets.Method)]
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

    [AttributeUsage(AttributeTargets.Method)]
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
