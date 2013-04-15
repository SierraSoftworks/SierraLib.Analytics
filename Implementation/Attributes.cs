using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace SierraLib.Analytics.Implementation
{
    [AttributeUsage(AttributeTargets.Method)]
    public abstract class TrackOnExceptionAttributeBase : MethodWrapperAttribute, ITrackingModule
    {
        public abstract void PreProcess(IRestRequest request);

        protected Exception Exception { get; private set; }
        protected object[] Parameters { get; private set; }

        public override void OnException(MethodBase method, Exception ex, object[] parameters)
        {
            var engine = method.GetCustomAttribute<TrackingEngineAttributeBase>(true).Engine;
            var application = method.GetCustomAttribute<TrackingApplicationAttribute>(true) as ITrackingApplication;
            var dataBundle = method.GetCustomAttributes<TrackingModuleAttributeBase>(true).Where(x => (x.Filter & TrackOn.Exception) != 0).Append((ITrackingModule)this).ToArray();

            Exception = ex;
            Parameters = parameters;

            engine.Track(application, dataBundle);
        }
    }
    
    [AttributeUsage(AttributeTargets.Method)]
    public abstract class TrackingModuleAttributeBase : Attribute, ITrackingModule
    {
        public TrackingModuleAttributeBase()
        {
            Filter = TrackOn.Entry;
        }

        public abstract void PreProcess(IRestRequest request);

        /// <summary>
        /// Gets or sets the types of event which will cause this attribute to be included in the tracked data bundle
        /// </summary>
        public TrackOn Filter
        { get; set; }
    }
}
