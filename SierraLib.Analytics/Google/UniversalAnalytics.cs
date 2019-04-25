using SierraLib.Analytics.Implementation;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SierraLib.Analytics.Google
{
    /// <summary>
    /// Provides support for tracking application usage through Google's Universal Analytics tracking platform.
    /// </summary>
    public sealed class UniversalAnalytics : TrackingEngine
    {
        /// <summary>
        /// Creates a new Google Analytics tracker with the given <paramref name="trackingID"/>.
        /// You should avoid using this method directly as it doesn't provide memo-ization support
        /// for <see cref="TrackingEngine"/> instances.
        /// </summary>
        /// <seealso cref="TrackingEngine.Create"/>
        /// <param name="trackingID">The Tracking ID associated with the Google Analytics account where tracking data should be sent</param>
        public UniversalAnalytics(string trackingID)
        {
            TrackingID = trackingID;
        }

        #region Properties

#if DEBUG
		private bool _Secure = false;
#else
        private bool _Secure = true;
#endif
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

        protected async override Task<Implementation.PreparedTrackingRequest> PrepareRequestAsync(RestSharp.IRestRequest request, IEnumerable<Implementation.ITrackingFinalize> finalizationQueue)
        {
            await Task.Yield();
            return new PreparedTrackingRequest(this, request, finalizationQueue);
        }

        protected override RestSharp.IRestClient CreateNetworkClient(string userAgent)
        {
            if (Secure)
                return new RestSharp.RestClient("https://ssl.google-analytics.com/") { UserAgent = userAgent };
            return new RestSharp.RestClient("http://www.google-analytics.com") { UserAgent = userAgent };
        }

        protected async override Task<string> CreateNewClientIDAsync(ITrackingApplication application)
        {
            await Task.Yield();
            return Guid.NewGuid().ToString().ToLower();
        }

        protected async override Task PreProcessAsync(RestSharp.IRestRequest request)
        {
            await Task.Yield();
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
            var screenArea = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
            request.AddParameterExclusive("sr", string.Format("{0}x{1}", screenArea.Width, screenArea.Height));

            // Document encoding
            // https://developers.google.com/analytics/devguides/collection/protocol/v1/parameters
            request.AddParameterExclusive("de", Encoding.Default.BodyName);

            // User language
            // https://developers.google.com/analytics/devguides/collection/protocol/v1/parameters
            request.AddParameterExclusive("ul", System.Threading.Thread.CurrentThread.CurrentCulture.Name);
        }

        protected async override Task PostProcessAsync(RestSharp.IRestRequest request)
        {
            await Task.Yield();
            // Hit type (default)
            // https://developers.google.com/analytics/devguides/collection/protocol/v1/parameters
            request.AddParameterExclusive("t", "appview"); //If we haven't set a type before this point
        }

        protected async override Task<RestSharp.IRestRequest> CreateRequestAsync(Implementation.ITrackingApplication application)
        {
            await Task.Yield();
            var request = new RestSharp.RestRequest("/collect", RestSharp.Method.POST);

            // Client ID
            // https://developers.google.com/analytics/devguides/collection/protocol/v1/parameters
            request.AddParameterExclusive("cid", await GetClientIDAsync(application));

            // Add application name and version parameters to request
            // https://developers.google.com/analytics/devguides/collection/protocol/v1/devguide
            request.AddParameter("an", application.Name.Truncate(150));
            request.AddParameter("av", application.Version.Truncate(150));

            return request;
        }

        #endregion

    }
}
