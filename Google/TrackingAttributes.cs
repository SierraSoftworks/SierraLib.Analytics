using RestSharp.Serializers;
using SierraLib.Analytics.Implementation;
using System;

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
        
        protected override TrackingEngine CreateEngine()
        {
            return new UniversalAnalytics(TrackingID);
        }

        protected override string GetEngineID()
        {
            return TrackingID;
        }
    }

    #region Session Control

    /// <summary>
    /// Causes a new session to be started with this tracking request
    /// </summary>
    public sealed class StartSessionAttribute : TrackingModuleAttributeBase
    {
        public override void PreProcess(RestSharp.IRestRequest request)
        {
            request.AddParameterExclusive("sc", "start");
        }
    }

    /// <summary>
    /// Causes the current session to be terminated with this tracking request
    /// </summary>
    public sealed class EndSessionAttribute : TrackingModuleAttributeBase
    {
        public override void PreProcess(RestSharp.IRestRequest request)
        {
            request.AddParameterExclusive("sc", "end");
        }
    }

    #endregion

    #region Hit Types

    /// <summary>
    /// Tracks the current item as an AppView hit type.
    /// </summary>
    /// <remarks>
    /// Requires that your <see cref="UniversalAnalytics.TrackingID"/> corresponds
    /// to a profile configured for App Tracking.
    /// </remarks>
    public sealed class AppViewAttribute : TrackingModuleAttributeBase
    {
        public AppViewAttribute(string description)
        {
            Description = description;
        }

        /// <summary>
        /// The description of the page being viewed
        /// </summary>
        public string Description
        { get; set; }

        public override void PreProcess(RestSharp.IRestRequest request)
        {
            if (!Description.IsNullOrWhitespace())
                request.AddParameterExclusive("cd", Description.Truncate(500));
        }
    }

    /// <summary>
    /// Tracks the current item as an Event hit type
    /// </summary>
    public sealed class EventAttribute : TrackingModuleAttributeBase
    {
        /// <summary>
        /// Tracks the current item an an Event hit type
        /// </summary>
        /// <param name="category">The category of the event</param>
        /// <param name="action">The action of the event</param>
        /// <param name="label">The label of the event</param>
        public EventAttribute(string category = null, string action = null, string label = null)
        {
            Category = category;
            Action = action;
            Label = label;
            Value = 0;
        }

        /// <summary>
        /// Specifies the event category
        /// </summary>
        public string Category
        { get; set; }

        /// <summary>
        /// Specifies the event action
        /// </summary>
        public string Action
        { get; set; }

        /// <summary>
        /// Specifies the event label
        /// </summary>
        public string Label
        { get; set; }

        /// <summary>
        /// Specifies the event value, must be non-negative
        /// </summary>
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
    
    /// <summary>
    /// Tracks the current item as a PageView hit type
    /// </summary>
    public sealed class PageViewAttribute : TrackingModuleAttributeBase
    {
        /// <summary>
        /// Tracks the current item as a PageView hit type
        /// </summary>
        /// <param name="path">The path of the page being tracked</param>
        public PageViewAttribute(string path)
        {
            Path = path;
        }

        /// <summary>
        /// Specifies the page's path
        /// </summary>
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

    /// <summary>
    /// Tracks an exception within the marked method when used in conjunction with Afterthought
    /// </summary>
    public sealed class OnExceptionAttribute : TrackOnExceptionAttributeBase
    {
        public OnExceptionAttribute()
        {
            TrackParameters = true;
        }

        /// <summary>
        /// Specifies whether or not call parameters are tracked with the request
        /// </summary>
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
