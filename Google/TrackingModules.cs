using SierraLib.Analytics.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SierraLib.Analytics.Google
{

    #region Session Control

    /// <summary>
    /// Causes a new session to be started with this tracking request
    /// </summary>
    public sealed class GASessionStart : ITrackingModule
    {
        public void PreProcess(RestSharp.IRestRequest request)
        {
            request.AddParameterExclusive("sc", "start");
        }
    }

    /// <summary>
    /// Causes the current session to be terminated with this tracking request
    /// </summary>
    public sealed class GASessionEnd : ITrackingModule
    {
        public void PreProcess(RestSharp.IRestRequest request)
        {
            request.AddParameterExclusive("sc", "end");
        }
    }

    #endregion

    #region Traffic Sources

    public sealed class GAReferrer : ITrackingModule
    {
        public GAReferrer(string referrer)
        {
            Value = referrer;
        }

        public string Value
        { get; set; }

        public void PreProcess(RestSharp.IRestRequest request)
        {
            request.AddParameterExclusive("dr", Value.Truncate(2048));
        }
    }

    public sealed class GATitle : ITrackingModule
    {
        public GATitle(string value)
        {
            Value = value;
        }

        public string Value
        { get; set; }
                
        public void PreProcess(RestSharp.IRestRequest request)
        {
            request.AddParameterExclusive("dt", Value.Truncate(1500));
        }
    }

    public sealed class GAHostName : ITrackingModule
    {
        public GAHostName(string value)
        {
            Value = value;
        }

        public string Value
        { get; set; }

        public void PreProcess(RestSharp.IRestRequest request)
        {
            request.AddParameterExclusive("dh", Value.Truncate(100));
        }
    }

    public sealed class GAPath : ITrackingModule
    {
        public GAPath(string value)
        {
            Value = value;
        }

        public string Value
        { get; set; }

        public void PreProcess(RestSharp.IRestRequest request)
        {
            request.AddParameterExclusive("dp", Value.Truncate(2048));
        }
    }

    public sealed class GALocationUri : ITrackingModule
    {
        public GALocationUri(string value)
        {
            Value = value;
        }

        public string Value
        { get; set; }

        public void PreProcess(RestSharp.IRestRequest request)
        {
            request.AddParameterExclusive("dl", Value.Truncate(2048));
        }
    }

    public sealed class GADescription : ITrackingModule
    {
        public GADescription(string value)
        {
            Value = value;
        }

        public string Value
        { get; set; }

        public void PreProcess(RestSharp.IRestRequest request)
        {
            request.AddParameterExclusive("cd", Value.Truncate(2048));
        }
    }

    #endregion

    #region Campaigns

    public sealed class GACampaign : ITrackingModule
    {
        public GACampaign()
        {

        }

        public string Name
        { get; set; }

        public string Source
        { get; set; }

        public string Medium
        { get; set; }

        public string Keyword
        { get; set; }

        public string Content
        { get; set; }

        public string ID
        { get; set; }
                
        public void PreProcess(RestSharp.IRestRequest request)
        {
            if (!Name.IsNullOrWhitespace())
                request.AddParameterExclusive("cn", Name.Truncate(100));
            if (!Source.IsNullOrWhitespace())
                request.AddParameterExclusive("cs", Source.Truncate(100));
            if (!Medium.IsNullOrWhitespace())
                request.AddParameterExclusive("cm", Medium.Truncate(50));
            if (!Keyword.IsNullOrWhitespace())
                request.AddParameterExclusive("ck", Keyword.Truncate(500));
            if (!Content.IsNullOrWhitespace())
                request.AddParameterExclusive("cc", Content.Truncate(500));
            if (!ID.IsNullOrWhitespace())
                request.AddParameterExclusive("ci", ID.Truncate(100));
        }
    }

    public sealed class GAAdWords : ITrackingModule
    {
        public GAAdWords(string id)
        {
            ID = id;
        }

        [NotNull]
        public string ID
        { get; set; }

        public void PreProcess(RestSharp.IRestRequest request)
        {
            request.AddParameterExclusive("gclid", ID);
        }
    }

    public sealed class GADisplayAds : ITrackingModule
    {
        public GADisplayAds(string id)
        {
            ID = id;
        }

        [NotNull]
        public string ID
        { get; set; }

        public void PreProcess(RestSharp.IRestRequest request)
        {
            request.AddParameterExclusive("dclid", ID);
        }
    }

    #endregion

    #region Exception Management

    public sealed class GAException : ITrackingModule, ITrackingPostProcess
    {
        public GAException(Exception ex, bool fatal = true)
        {
            Exception = ex;
            Fatal = fatal;
        }

        public Exception Exception
        { get; set; }

        public bool Fatal
        { get; set; }

        public void PreProcess(RestSharp.IRestRequest request)
        {
            request.AddParameterExclusive("exd", 
                string.Format("{0}: '{1}' in {2}.{3}", Exception.GetType().Name, Exception.Message, Exception.TargetSite.DeclaringType.FullName, Exception.TargetSite.Name)
                .Truncate(150));
            request.AddParameterExclusive("exf", Fatal ? 1 : 0);
        }

        public void PostProcess(RestSharp.IRestRequest request)
        {
            TrackingModuleHelpers.RequiresParameter(this, request, "t", "exception");
        }
    }

    #endregion

    #region Hit Types

    public sealed class GAAppView : ITrackingModule
    {
        public void PreProcess(RestSharp.IRestRequest request)
        {
            request.AddParameterExclusiveOrThrow("t", "appview");
        }
    }

    public sealed class GAPageView : ITrackingModule, ITrackingPostProcess
    {
        public void PreProcess(RestSharp.IRestRequest request)
        {
            request.AddParameterExclusiveOrThrow("t", "pageview");
        }

        public void PostProcess(RestSharp.IRestRequest request)
        {
            if (!request.Parameters.Any(x => x.Name == "dp"))
                throw new InvalidOperationException("GAPageView requests require the GAPath module to be used");
        }
    }

    public sealed class GAEvent : ITrackingModule
    {
        public GAEvent(string category = null, string action = null, string label = null, int value = 0)
        {
            Category = category;
            Action = action;
            Label = label;
            Value = value;
        }

        public string Category
        { get; set; }

        public string Action
        { get; set; }

        public string Label
        { get; set; }

        public int Value
        { get; set; }

        public void PreProcess(RestSharp.IRestRequest request)
        {
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

    public sealed class GASocial : ITrackingModule
    {
        public GASocial(string socialNetwork, string action, string target)
        {
            SocialNetwork = socialNetwork;
            Action = action;
            Target = target;
        }

        [NotNull]
        public string SocialNetwork
        { get; set; }

        [NotNull]
        public string Action
        { get; set; }

        [NotNull]
        public string Target
        { get; set; }

        public void PreProcess(RestSharp.IRestRequest request)
        {
            request.AddParameterExclusive("sn", SocialNetwork.Truncate(50));
            request.AddParameterExclusive("sa", Action.Truncate(50));
            request.AddParameterExclusive("st", Target.Truncate(2048));
        }
    }

    public sealed class GAUserTiming : ITrackingModule
    {
        public GAUserTiming(string timingVariable, int milliseconds, string timingCategory = null, string timingLabel = null)
        {
            Category = timingCategory;
            Variable = timingVariable;
            Label = timingLabel;
            Milliseconds = milliseconds;
        }

        public string Category
        { get; set; }

        [NotNull]
        public string Variable
        { get; set; }

        public string Label
        { get; set; }

        public int Milliseconds
        { get; set; }

        public void PreProcess(RestSharp.IRestRequest request)
        {
            if(!Category.IsNullOrEmpty())
                request.AddParameterExclusive("utc", Category.Truncate(150));
            if (!Label.IsNullOrEmpty())
                request.AddParameterExclusive("utl", Label.Truncate(500));
            request.AddParameterExclusive("utv", Variable.Truncate(500));
            request.AddParameterExclusive("utt", Milliseconds);
        }
    }

    public sealed class GANonInteractive : ITrackingModule
    {
        public void PreProcess(RestSharp.IRestRequest request)
        {
            request.AddParameterExclusive("ni", 1);
        }
    }

    #endregion

    #region Custom Dimensions

    public sealed class GACustomDimension : ITrackingModule
    {
        public GACustomDimension(byte index, string value)
        {
            Index = index;
            Value = value;
        }

        byte _index = 1;
        public byte Index
        {
            get { return _index; }
            set
            {
                if (value < 1 || value > 200)
                    throw new ArgumentOutOfRangeException("Custom Dimension indices must be between 1 and 200 inclusive");
                _index = value;
            }
        }

        public string Value
        { get; set; }
                
        public void PreProcess(RestSharp.IRestRequest request)
        {
            if(!Value.IsNullOrWhitespace())
                request.AddParameterExclusiveOrThrow(string.Format("cd{0}", Index), Value.Truncate(150));
        }
    }

    public sealed class GACustomMetric : ITrackingModule
    {
        public GACustomMetric(byte index, int value)
        {
            Index = index;
            Value = value;
        }

        byte _index = 1;
        public byte Index
        {
            get { return _index; }
            set
            {
                if (value < 1 || value > 200)
                    throw new ArgumentOutOfRangeException("Custom Metric indices must be between 1 and 200 inclusive");
                _index = value;
            }
        }

        public int Value
        { get; set; }

        public void PreProcess(RestSharp.IRestRequest request)
        {
            request.AddParameterExclusiveOrThrow(string.Format("cm{0}", Index), Value);
        }
    }

    #endregion

    #region System Information

    public sealed class GAViewport : ITrackingModule
    {
        public GAViewport(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public int Width
        { get; set; }

        public int Height
        { get; set; }
                
        public void PreProcess(RestSharp.IRestRequest request)
        {
            request.AddParameterExclusive("vp", string.Format("{0}x{1}", Width, Height));
        }
    }

    #endregion
}
