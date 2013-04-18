using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SierraLib.Analytics.Implementation;

namespace SierraLib.Analytics.Google
{
    /// <summary>
    /// Provides support for tracking application usage through Google's Universal Analytics tracking platform.
    /// </summary>
    public sealed class UniversalAnalytics : TrackingEngine
    {
        /// <summary>
        /// Creates a new Google Analytics tracker with the given <paramref name="trackingID"/>
        /// </summary>
        /// <param name="trackingID">The Tracking ID associated with the Google Analytics account where tracking data should be sent</param>
        public UniversalAnalytics(string trackingID)
        {
            TrackingID = trackingID;
        }

        #region Properties

        private bool _Secure = true;
        /// <summary>
        /// Determines whether or not the secure portal is used
        /// </summary>
        public bool Secure
        {
            get { return _Secure; }
            set
            {
                _Secure = value;
                RequestNewClient();
            }
        }

        /// <summary>
        /// Gets or Sets the Tracking ID used to associate data reported
        /// by this application with your account. The format is UA-XXXX-Y
        /// </summary>
        [NotNull]
        public string TrackingID { get; set; }

        /// <summary>
        /// The version of the Measurement Protocol used by this library
        /// </summary>
        public const byte ProtocolVersion = 1;

        /// <summary>
        /// Determines whether or not the client's IP address will be anonymized
        /// when being sent to Google's servers.
        /// </summary>
        public bool AnonymizeIP { get; set; }
        
        #endregion

        #region Tracking Engine

        protected override string GetTrackerID()
        {
            return TrackingID;
        }

        protected override Implementation.PreparedTrackingRequest PrepareRequest(RestSharp.IRestRequest request, IEnumerable<Implementation.ITrackingFinalize> finalizationQueue)
        {
            return new PreparedTrackingRequest(this, request, finalizationQueue);
        }

        protected override RestSharp.IRestClient CreateNetworkClient(string userAgent)
        {
            if(Secure)
                return new RestSharp.RestClient("https://ssl.google-analytics.com/") { UserAgent = userAgent };
            return new RestSharp.RestClient("http://www.google-analytics.com") { UserAgent = userAgent };
        }

        protected override string CreateNewClientID(ITrackingApplication application)
        {
            return Guid.NewGuid().ToString().ToLower();
        }

        protected override void PreProcess(RestSharp.IRestRequest request)
        {
            // Add protocol version
            // https://developers.google.com/analytics/devguides/collection/protocol/v1/parameters
            request.AddParameterExclusive("v", ProtocolVersion);

            // Add account tracking ID
            // https://developers.google.com/analytics/devguides/collection/protocol/v1/parameters
            request.AddParameterExclusive("tid", TrackingID);

            // Anonymize IP flag
            // https://developers.google.com/analytics/devguides/collection/protocol/v1/parameters
            if (AnonymizeIP)
                request.AddParameterExclusive("aip", 1);

            // Screen resolution
            // https://developers.google.com/analytics/devguides/collection/protocol/v1/parameters
            var screenArea = new Microsoft.VisualBasic.Devices.Computer().Screen.Bounds;
            request.AddParameterExclusive("sr", string.Format("{0}x{1}", screenArea.Width, screenArea.Height));

            // Document encoding
            // https://developers.google.com/analytics/devguides/collection/protocol/v1/parameters
            request.AddParameterExclusive("de", Encoding.Default.BodyName);

            // User language
            // https://developers.google.com/analytics/devguides/collection/protocol/v1/parameters
            request.AddParameterExclusive("ul", System.Threading.Thread.CurrentThread.CurrentCulture.Name);
        }

        protected override void PostProcess(RestSharp.IRestRequest request)
        {
            // Hit type (default)
            // https://developers.google.com/analytics/devguides/collection/protocol/v1/parameters
            request.AddParameterExclusive("t", "appview"); //If we haven't set a type before this point
        }

        protected override RestSharp.IRestRequest CreateRequest(Implementation.ITrackingApplication application)
        {
            var request = new RestSharp.RestRequest("/collect", RestSharp.Method.POST);

            // Client ID
            // https://developers.google.com/analytics/devguides/collection/protocol/v1/parameters
            request.AddParameterExclusive("cid", GetClientID(application));

            // Add application name and version parameters to request
            // https://developers.google.com/analytics/devguides/collection/protocol/v1/devguide
            request.AddParameter("an", application.Name.Truncate(150));
            request.AddParameter("av", application.Version.Truncate(150));
            
            return request;
        }

        #endregion

    }
}
