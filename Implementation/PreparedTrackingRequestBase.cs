using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RestSharp;

namespace SierraLib.Analytics.Implementation
{
    /// <summary>
    /// Represents a tracking request that is waiting to be sent to the server
    /// </summary>
    /// <remarks>
    /// This is generally stored in a queue while waiting to be sent to the
    /// tracking server, allowing the <see cref="TrackingManager"/> to worry
    /// about the process of offline queuing etc.
    /// </remarks>
    [Serializable]
    public class PreparedTrackingRequest
    {
        /// <summary>
        /// Creates a new <see cref="PreparedTrackingRequest"/> which performs no additional
        /// finalization on a request object.
        /// </summary>
        /// <param name="engine">The <see cref="TrackingEngine"/> managing the tracking request</param>
        /// <param name="request">The <see cref="IRestRequest"/> representing the current request</param>
        /// <param name="requiredFinalizations">The <see cref="ITrackingModule"/>s which require finalization of the request before sending</param>
        public PreparedTrackingRequest(TrackingEngine engine, IRestRequest request, IEnumerable<ITrackingFinalize> requiredFinalizations)
        {
            Engine = engine;
            RequestID = Guid.NewGuid();
            RequiredFinalizations = requiredFinalizations;
            Request = request;
        }

        /// <summary>
        /// Gets the <see cref="TrackingEngine"/> which generated this request
        /// </summary>
        public TrackingEngine Engine
        { get; private set; }

        /// <summary>
        /// Gets a unique ID used to identify this request locally.
        /// </summary>
        public Guid RequestID
        { get; private set; }

        /// <summary>
        /// A number of <see cref="ITrackingModule"/>s which require the ability to
        /// finalize a request just before it is sent to the server.
        /// </summary>
        public IEnumerable<ITrackingFinalize> RequiredFinalizations
        { get; private set; }

        /// <summary>
        /// Gets the <see cref="IRestRequest"/> containing the tracking information
        /// </summary>
        public IRestRequest Request
        { get; private set; }

        /// <summary>
        /// Performs any final housekeeping for the request, and returns a <see cref="IRestRequest"/>
        /// to submit the request to the tracking server.
        /// </summary>
        /// <returns></returns>
        public virtual void Finalize()
        {

        }
    }
}
