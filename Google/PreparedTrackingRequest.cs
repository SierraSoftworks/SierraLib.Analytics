using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Imp = SierraLib.Analytics.Implementation;

namespace SierraLib.Analytics.Google
{
    public sealed class PreparedTrackingRequest : Imp.PreparedTrackingRequest
    {
        internal PreparedTrackingRequest(UniversalAnalytics engine, IRestRequest request, 
                                        IEnumerable<Imp.ITrackingFinalize> requiredFinalizations) 
            : base(engine, request, requiredFinalizations)
        {
            Generated = DateTime.UtcNow;
        }

        /// <summary>
        /// The time when the request was generated, prior to insertion into the queue for submission.
        /// </summary>
        public DateTime Generated
        { get; private set; }

        public override void Finalize()
        {
            //Queue Time https://developers.google.com/analytics/devguides/collection/protocol/v1/parameters#
            Request.AddParameter("qt", DateTime.UtcNow.Subtract(Generated).TotalMilliseconds);

            //Cache Buster https://developers.google.com/analytics/devguides/collection/protocol/v1/parameters#
            Request.AddParameter("z", new Random().Next());
        }
    }
}
