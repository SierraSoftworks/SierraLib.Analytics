using AspectInjector.Broker;
using RestSharp;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SierraLib.Analytics.Implementation
{
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

    /// <summary>
    /// Causes a tracking event to be submitted before this method is executed.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = true)]
    [Injection(typeof(MethodInvokeAspect), Inherited = true)]
    public class TrackOnAttributeBase : Attribute, IMethodInvokeTrigger
    {
        public virtual void OnEntry(MethodBase method, object[] parameters)
        {
            // Do nothing
        }

        public virtual void OnException(MethodBase method, object[] parameters, Exception exception)
        {
            // Do nothing
        }

        public virtual void OnExit(MethodBase method, object[] parameters, object result)
        {
            // Do nothing
        }
    }
}
