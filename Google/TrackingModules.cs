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
    public sealed class StartSession : ITrackingModule
    {
        public void PreProcess(RestSharp.IRestRequest request)
        {
            request.AddParameterExclusive("sc", "start");
        }
    }

    /// <summary>
    /// Causes the current session to be terminated with this tracking request
    /// </summary>
    public sealed class EndSession : ITrackingModule
    {
        public void PreProcess(RestSharp.IRestRequest request)
        {
            request.AddParameterExclusive("sc", "end");
        }
    }

    #endregion

    #region Traffic Sources

    public sealed class Referrer : ITrackingModule
    {
        public Referrer(string referrer)
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

    public sealed class Title : ITrackingModule
    {
        public Title(string value)
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

    public sealed class HostName : ITrackingModule
    {
        public HostName(string value)
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

    public sealed class Path : ITrackingModule
    {
        public Path(string value)
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

    public sealed class LocationUri : ITrackingModule
    {
        public LocationUri(string value)
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

    public sealed class Description : ITrackingModule
    {
        public Description(string value)
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

    public sealed class Campaign : ITrackingModule
    {
        public Campaign()
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

    public sealed class TrackedException : ITrackingModule, ITrackingPostProcess
    {
        public TrackedException(Exception ex, bool fatal = true)
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

    public sealed class AppView : ITrackingModule
    {
        public void PreProcess(RestSharp.IRestRequest request)
        {
            request.AddParameterExclusiveOrThrow("t", "appview");
        }
    }

    public sealed class PageView : ITrackingModule, ITrackingPostProcess
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

    public sealed class Event : ITrackingModule
    {
        public Event(string category = null, string action = null, string label = null, int value = 0)
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

    public sealed class Social : ITrackingModule
    {
        public Social(string socialNetwork, string action, string target)
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

    public sealed class UserTiming : ITrackingModule
    {
        public UserTiming(string timingVariable, int milliseconds, string timingCategory = null, string timingLabel = null)
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

    public sealed class NonInteractive : ITrackingModule
    {
        public void PreProcess(RestSharp.IRestRequest request)
        {
            request.AddParameterExclusive("ni", 1);
        }
    }

    #endregion

    #region Custom Dimensions

    public sealed class CustomDimension : ITrackingModule
    {
        public CustomDimension(byte index, string value)
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

    public sealed class CustomMetric : ITrackingModule
    {
        public CustomMetric(byte index, int value)
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

    public sealed class Viewport : ITrackingModule
    {
        public Viewport(int width, int height)
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

    #region E-Commerce

    public sealed class Item : ITrackingModule
    {
        public Item(string transactionID, string itemName)
        {
            TransactionID = transactionID;
            Name = itemName;
        }

        public string TransactionID
        { get; set; }

        public string Name
        { get; set; }

        public double? Price
        { get; set; }

        public int? Quantity
        { get; set; }

        public string Code
        { get; set; }

        public string Category
        { get; set; }

        public string CurrencyCode
        { get; set; }

        public void PreProcess(RestSharp.IRestRequest request)
        {
            request.AddParameterExclusiveOrThrow("t", "item");
            request.AddParameterExclusive("ti", TransactionID.Truncate(500));
            request.AddParameterExclusive("in", Name.Truncate(500));
            if (Price.HasValue)
                request.AddParameterExclusive("ip", Price.Value);
            if (Quantity.HasValue)
                request.AddParameterExclusive("iq", Quantity.Value);
            if (!Code.IsNullOrWhitespace())
                request.AddParameterExclusive("ic", Code.Truncate(500));
            if (!Category.IsNullOrWhitespace())
                request.AddParameterExclusive("iv", Category.Truncate(500));
            if (!CurrencyCode.IsNullOrWhitespace())
                request.AddParameterExclusive("cu", CurrencyCode.Truncate(10));
        }
    }

    public sealed class Transaction : ITrackingModule
    {
        public Transaction(string transactionID)
        {
            TransactionID = transactionID;
        }

        public string TransactionID
        { get; set; }

        public string Affiliation
        { get; set; }

        public double? Revenue
        { get; set; }

        public double? Shipping
        { get; set; }

        public double? Tax
        { get; set; }

        public void PreProcess(RestSharp.IRestRequest request)
        {
            request.AddParameterExclusiveOrThrow("t", "transaction");
            request.AddParameterExclusive("ti", TransactionID.Truncate(500));

            if (!Affiliation.IsNullOrWhitespace())
                request.AddParameterExclusive("ta", Affiliation.Truncate(500));
            if (Revenue.HasValue)
                request.AddParameterExclusive("tr", Revenue.Value);
            if (Shipping.HasValue)
                request.AddParameterExclusive("ts", Shipping.Value);
            if (Tax.HasValue)
                request.AddParameterExclusive("tt", Tax.Value);
        }
    }

    #endregion
}
