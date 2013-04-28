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

    /// <summary>
    /// Specifies the source from which the current page was reached
    /// </summary>
    public sealed class Referrer : ITrackingModule
    {
        /// <summary>
        /// Specifies the source from which the current page was reached. Max length: 2048
        /// </summary>
        public Referrer(Uri referrer)
        {
            Value = referrer;
        }

        /// <summary>
        /// A valid <see cref="Uri"/> representing the source from which this page was reached. Max length: 2048
        /// </summary>
        public Uri Value
        { get; set; }

        public void PreProcess(RestSharp.IRestRequest request)
        {
            request.AddParameterExclusive("dr", Value.ToString().Truncate(2048));
        }
    }

    /// <summary>
    /// Specifies the title of the current page. Max length: 1500
    /// </summary>
    public sealed class Title : ITrackingModule
    {
        public Title(string value)
        {
            Value = value;
        }

        /// <summary>
        /// Specifies the title of the current page. Max length: 1500
        /// </summary>
        public string Value
        { get; set; }
                
        public void PreProcess(RestSharp.IRestRequest request)
        {
            request.AddParameterExclusive("dt", Value.Truncate(1500));
        }
    }

    /// <summary>
    /// Specifies the Host name of the current page. Max length: 100
    /// </summary>
    public sealed class HostName : ITrackingModule
    {
        /// <summary>
        /// Specifies the Host name of the current page. Max length: 100
        /// </summary>
        public HostName(string value)
        {
            Value = value;
        }

        /// <summary>
        /// Specifies the Host name of the current page. Max length: 100
        /// </summary>
        public string Value
        { get; set; }

        public void PreProcess(RestSharp.IRestRequest request)
        {
            request.AddParameterExclusive("dh", Value.Truncate(100));
        }
    }

    /// <summary>
    /// Specifies the path of the current page
    /// </summary>
    public sealed class Path : ITrackingModule
    {
        /// <summary>
        /// Specifies the path of the current page
        /// </summary>
        public Path(string value)
        {
            Value = value;
        }

        /// <summary>
        /// Specifies the path of the current page
        /// </summary>
        public string Value
        { get; set; }

        public void PreProcess(RestSharp.IRestRequest request)
        {
            request.AddParameterExclusive("dp", Value.Truncate(2048));
        }
    }

    /// <summary>
    /// Specifies the location of the current page
    /// </summary>
    public sealed class LocationUri : ITrackingModule
    {
        /// <summary>
        /// Specifies the location of the current page
        /// </summary>
        public LocationUri(string value)
        {
            Value = value;
        }

        /// <summary>
        /// Specifies the location of the current page
        /// </summary>
        public string Value
        { get; set; }

        public void PreProcess(RestSharp.IRestRequest request)
        {
            request.AddParameterExclusive("dl", Value.Truncate(2048));
        }
    }

    /// <summary>
    /// Specifies a description for the current page
    /// </summary>
    public sealed class Description : ITrackingModule
    {
        /// <summary>
        /// Specifies a description for the current page
        /// </summary>
        public Description(string value)
        {
            Value = value;
        }

        /// <summary>
        /// Specifies a description for the current page
        /// </summary>
        public string Value
        { get; set; }

        public void PreProcess(RestSharp.IRestRequest request)
        {
            request.AddParameterExclusive("cd", Value.Truncate(2048));
        }
    }

    #endregion

    #region Campaigns

    /// <summary>
    /// Allows an advertising campaign to be tracked
    /// </summary>
    public sealed class Campaign : ITrackingModule
    {
        public Campaign()
        {

        }

        /// <summary>
        /// Gets or sets the name of the advertising campaign which generated the hit. Max length: 100
        /// </summary>
        public string Name
        { get; set; }
        
        /// <summary>
        /// Gets or sets the source from which the hit was generated. Max length: 100
        /// </summary>
        public string Source
        { get; set; }

        /// <summary>
        /// Gets or sets the medium through which the hit was generated. Max length: 50
        /// </summary>
        public string Medium
        { get; set; }

        /// <summary>
        /// Gets or sets the keyword which resulted in the hit. Max length: 500
        /// </summary>
        public string Keyword
        { get; set; }

        /// <summary>
        /// Gets or sets the content of the advertising campaign. Max length: 500
        /// </summary>
        public string Content
        { get; set; }

        /// <summary>
        /// Gets or sets the ID of the campaign. Max length: 100
        /// </summary>
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

    /// <summary>
    /// Allows tracking of Google AdWords campaigns
    /// </summary>
    public sealed class AdWords : ITrackingModule
    {
        public AdWords(string id)
        {
            ID = id;
        }

        /// <summary>
        /// Gets or sets the AdWords advert ID
        /// </summary>
        [NotNull]
        public string ID
        { get; set; }

        public void PreProcess(RestSharp.IRestRequest request)
        {
            request.AddParameterExclusive("gclid", ID);
        }
    }

    /// <summary>
    /// Allows tracking of Google Display Ads (DoubleClick ads)
    /// </summary>
    public sealed class DisplayAds : ITrackingModule
    {
        public DisplayAds(string id)
        {
            ID = id;
        }

        /// <summary>
        /// Gets or sets the Google Display Ads ID
        /// </summary>
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

    /// <summary>
    /// Allows tracking of exceptions within your application
    /// </summary>
    public sealed class TrackedException : ITrackingModule, ITrackingPostProcess
    {
        public TrackedException(Exception ex, bool fatal = true)
        {
            Exception = ex;
            Fatal = fatal;
        }

        /// <summary>
        /// Gets or sets the <see cref="Exception"/> which you'd like to track
        /// </summary>
        public Exception Exception
        { get; set; }

        /// <summary>
        /// Gets or sets whether or not the exception resulted in an application crash
        /// </summary>
        public bool Fatal
        { get; set; }

        public void PreProcess(RestSharp.IRestRequest request)
        {
            request.AddParameterExclusive("exd", 
                string.Format("{1}.{2}: {3}", Exception.TargetSite.DeclaringType.FullName, Exception.TargetSite.Name, Exception.Message)
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

    /// <summary>
    /// Tracks the current item as an AppView hit type.
    /// </summary>
    /// <remarks>
    /// Requires that your <see cref="UniversalAnalytics.TrackingID"/> corresponds
    /// to a profile configured for App Tracking.
    /// </remarks>
    public sealed class AppView : ITrackingModule
    {
        public void PreProcess(RestSharp.IRestRequest request)
        {
            request.AddParameterExclusiveOrThrow("t", "appview");
        }
    }

    /// <summary>
    /// Tracks the current item as a PageView hit type
    /// </summary>
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

    /// <summary>
    /// Tracks the current item as an Event hit type
    /// </summary>
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

    /// <summary>
    /// Indicates that an interaction was performed in the 
    /// background without a user's active request.
    /// </summary>
    public sealed class NonInteractive : ITrackingModule
    {
        public void PreProcess(RestSharp.IRestRequest request)
        {
            request.AddParameterExclusive("ni", 1);
        }
    }

    #endregion

    #region Custom Dimensions

    /// <summary>
    /// Tracks a specific custom dimension for the current hit
    /// </summary>
    public sealed class CustomDimension : ITrackingModule
    {
        /// <summary>
        /// Tracks a specific custom dimension for the current hit
        /// </summary>
        /// <param name="index">The unique index for this dimension. Value between 1 and 20 inclusive</param>
        /// <param name="value">The value of this custom dimension. Max length: 150</param>
        public CustomDimension(byte index, string value)
        {
            Index = index;
            Value = value;
        }

        byte _index = 1;
        /// <summary>
        /// Specifies the custom dimension index. Must be between 1 and 20 inclusive
        /// </summary>
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

        /// <summary>
        /// Specifies the custom dimension's value. Max length: 150
        /// </summary>
        public string Value
        { get; set; }
                
        public void PreProcess(RestSharp.IRestRequest request)
        {
            if(!Value.IsNullOrWhitespace())
                request.AddParameterExclusiveOrThrow(string.Format("cd{0}", Index), Value.Truncate(150));
        }
    }

    /// <summary>
    /// Tracks a custom metric for the current hit
    /// </summary>
    public sealed class CustomMetric : ITrackingModule
    {
        public CustomMetric(byte index, int value)
        {
            Index = index;
            Value = value;
        }

        byte _index = 1;
        /// <summary>
        /// Specifies the index for the custom metric. Must be between 1 and 20 inclusive
        /// </summary>
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

        /// <summary>
        /// Specifies the value for the custom metric
        /// </summary>
        public int Value
        { get; set; }

        public void PreProcess(RestSharp.IRestRequest request)
        {
            request.AddParameterExclusiveOrThrow(string.Format("cm{0}", Index), Value);
        }
    }

    #endregion

    #region System Information

    /// <summary>
    /// Tracks the current viewport's dimensions
    /// </summary>
    public sealed class Viewport : ITrackingModule
    {
        /// <summary>
        /// Tracks the current viewport's dimensions
        /// </summary>
        /// <param name="width">The width of the viewport in pixels</param>
        /// <param name="height">The height of the viewport in pixels</param>
        public Viewport(int width, int height)
        {
            Width = width;
            Height = height;
        }

        /// <summary>
        /// The width of the viewport in pixels
        /// </summary>
        public int Width
        { get; set; }

        /// <summary>
        /// The height of the viewport in pixels
        /// </summary>
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
