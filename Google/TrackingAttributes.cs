using RestSharp.Serializers;
using SierraLib.Analytics.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SierraLib.Analytics.Google
{
    /// <summary>
    /// Provides support for tracking application usage through Google's Universal Analytics tracking platform.
    /// </summary>
    public sealed class UniversalAnalyticsAttribute : TrackingEngineAttributeBase
    {
        public UniversalAnalyticsAttribute(string trackingID)
        {
            TrackingID = trackingID;
        }

        /// <summary>
        /// Gets or Sets the Tracking ID used to associate data reported
        /// by this application with your account. The format is UA-XXXX-Y
        /// </summary>
        [NotNull]
        public string TrackingID
        { get; private set; }



        static Dictionary<string, TrackingEngine> EngineCache = new Dictionary<string, TrackingEngine>();

        protected override TrackingEngine CreateEngine()
        {
            if (EngineCache.ContainsKey(TrackingID)) return EngineCache[TrackingID];
            else
            {
                var engine = new UniversalAnalytics(TrackingID);
                EngineCache.Add(TrackingID, engine);
                return engine;
            }
        }
    }

    #region Session Control

    public sealed class StartSessionAttribute : TrackingModuleAttributeBase
    {
        public override void PreProcess(RestSharp.IRestRequest request)
        {
            request.AddParameterExclusive("sc", "start");
        }
    }

    public sealed class EndSessionAttribute : TrackingModuleAttributeBase
    {
        public override void PreProcess(RestSharp.IRestRequest request)
        {
            request.AddParameterExclusive("sc", "end");
        }
    }

    #endregion

    #region Hit Types

    public sealed class EventAttribute : TrackingModuleAttributeBase
    {
        public EventAttribute(string category = null, string action = null, string label = null)
        {
            Category = category;
            Action = action;
            Label = label;
            Value = 0;
        }

        public string Category
        { get; set; }

        public string Action
        { get; set; }

        public string Label
        { get; set; }

        public int Value
        { get; set; }

        public override void PreProcess(RestSharp.IRestRequest request)
        {
            request.AddParameterExclusive("t", "event");

            if (!string.IsNullOrEmpty(Category))
                request.AddParameterExclusive("ec", Category.Truncate(150));
            if (!string.IsNullOrEmpty(Action))
                request.AddParameterExclusive("ea", Action.Truncate(500));
            if (!string.IsNullOrEmpty(Label))
                request.AddParameterExclusive("el", Label.Truncate(500));
            if (Value != 0)
                request.AddParameterExclusive("ev", Value);
        }
    }
    
    public sealed class AppViewAttribute : TrackingModuleAttributeBase
    {
        public AppViewAttribute(string description)
        {
            Description = description;
        }

        public string Description
        { get; set; }

        public override void PreProcess(RestSharp.IRestRequest request)
        {
            if (!Description.IsNullOrWhitespace())
                request.AddParameterExclusive("cd", Description.Truncate(500));
        }
    }

    public sealed class PageViewAttribute : TrackingModuleAttributeBase
    {
        public PageViewAttribute(string path)
        {
            Path = path;
        }

        public string Path
        { get; set; }

        public override void PreProcess(RestSharp.IRestRequest request)
        {
            if (!Path.IsNullOrWhitespace())
                request.AddParameterExclusive("dp", Path.Truncate(2048));
        }
    }


    #endregion

    #region Exceptions

    public sealed class OnExceptionAttribute : TrackOnExceptionAttributeBase
    {
        public OnExceptionAttribute()
        {
            TrackParameters = true;
        }

        public bool TrackParameters
        { get; set; }

        public override void PreProcess(RestSharp.IRestRequest request)
        {
            request.AddParameterExclusive("exd", string.Format("{0}: {1} in {2}.{3}", Exception.GetType().Name, Exception.Message, Exception.TargetSite.DeclaringType.FullName, Exception.TargetSite.Name));
            request.AddParameterExclusive("exf", 1);


            var parametersValues = "";
            if (TrackParameters)
            {
                var serializer = new JsonSerializer();

                for (int i = 0; i < Parameters.Length; i++)
                    parametersValues += string.Format("arg{0}: {1}\n", i, serializer.Serialize(Parameters[i]));

            }

            if (!string.IsNullOrWhiteSpace(parametersValues))
                request.AddParameterExclusive("cd", string.Format("Parameters: {0}\n\nStack Trace:\n{1}", Exception.StackTrace, parametersValues).Truncate(2048));
            else
                request.AddParameterExclusive("cd", string.Format("Stack Trace:\n {0}", Exception.StackTrace).Truncate(2048));
        }
    }

    #endregion
}
