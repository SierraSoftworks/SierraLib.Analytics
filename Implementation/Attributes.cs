using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace SierraLib.Analytics.Implementation
{
    /// <summary>
    /// When used in conjunction with Afterthought, allows methods marked with implementations of this attribute
    /// to automatically handle exceptions generated therein by sending them to the tracking server.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = true, Inherited = true)]
    public abstract class TrackOnExceptionAttributeBase : MethodWrapperAttribute, ITrackingModule
    {
        public Type ExceptionFilter
        { get; set; }

        public abstract void PreProcess(IRestRequest request);

        protected Exception Exception { get; private set; }
        protected object[] Parameters { get; private set; }

        public override void OnException(MethodBase method, Exception ex, object[] parameters)
        {
            if (ExceptionFilter != null && !ex.GetType().IsSubclassOf(ExceptionFilter))
                return;

            var engine = method.GetCustomAttribute<TrackingEngineAttributeBase>(true).Engine;
            var application = method.GetCustomAttribute<TrackingApplicationAttribute>(true) as ITrackingApplication;
            var dataBundle = method.GetCustomAttributes<TrackingModuleAttributeBase>(true).Where(x => (x.Filter & TrackOn.Exception) != 0).Append((ITrackingModule)this).ToArray();

            Exception = ex;
            Parameters = parameters;

            engine.Track(application, dataBundle);
        }
    }
    
    /// <summary>
    /// Provides the base implementation for all tracking attributes which wish to support
    /// filtering.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly, Inherited = true, AllowMultiple = true)]
    public abstract class TrackingModuleAttributeBase : Attribute, ITrackingModule
    {
        public TrackingModuleAttributeBase()
        {
            Filter = TrackOn.All;
        }

        public abstract void PreProcess(IRestRequest request);

        /// <summary>
        /// Gets or sets the types of event which will cause this attribute to be included in the tracked data bundle
        /// </summary>
        public TrackOn Filter
        { get; set; }
    }
}
