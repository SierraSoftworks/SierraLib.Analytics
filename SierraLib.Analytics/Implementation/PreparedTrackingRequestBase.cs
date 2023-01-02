using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;

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
    public class PreparedTrackingRequest : ISerializable
    {
        static PreparedTrackingRequest()
        {
            Serializer.Error += OnSerializerError;
        }

        /// <summary>
        /// Creates a new <see cref="PreparedTrackingRequest"/> which performs no additional
        /// finalization on a request object.
        /// </summary>
        /// <param name="engine">The <see cref="TrackingEngine"/> managing the tracking request</param>
        /// <param name="request">The <see cref="RestRequest"/> representing the current request</param>
        /// <param name="requiredFinalizations">The <see cref="ITrackingModule"/>s which require finalization of the request before sending</param>
        public PreparedTrackingRequest(TrackingEngine engine, RestRequest request, IEnumerable<ITrackingFinalize> requiredFinalizations)
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
        /// Gets the <see cref="RestRequest"/> containing the tracking information
        /// </summary>
        public RestRequest Request
        { get; private set; }

        /// <summary>
        /// Performs any final housekeeping for the <see cref="Request"> prior to it being
        /// sent to the server.
        /// </summary>
        /// <returns></returns>
        public virtual void FinalizeRequest()
        {

        }


        #region Serialization

        static readonly JsonSerializer Serializer = new JsonSerializer();

        protected void SerializeRequest(RestRequest request, SerializationInfo info, StreamingContext context)
        {
            using (var sw = new StringWriter())
            {
                Serializer.Serialize(sw, request);
                info.AddValue("Request", sw.ToString());
            }
        }

        static void OnSerializerError(object sender, Newtonsoft.Json.Serialization.ErrorEventArgs e)
        {
            e.ErrorContext.Handled = true;
        }

        protected RestRequest DeserializeRequest(SerializationInfo info, StreamingContext context)
        {
            var request = new RestRequest();

            var serialized = info.GetString("Request");

            Serializer.Populate(new StringReader(serialized), request);
            return request;
        }

        /// <summary>
        /// Creates a <see cref="PreparedTrackingRequest"/> from a serialized <see cref="PreparedTrackingRequest"/> object.
        /// </summary>
        public PreparedTrackingRequest(SerializationInfo info, StreamingContext context)
        {
            Engine = TrackingEngine.GetEngineByID(info.GetString("Engine"));
            RequestID = (Guid)info.GetValue("RequestID", typeof(Guid));
            RequiredFinalizations = (IEnumerable<ITrackingFinalize>)info.GetValue("RequiredFinalizations", typeof(IEnumerable<ITrackingFinalize>));

            Request = DeserializeRequest(info, context);
        }

        /// <summary>
        /// Serializes the current <see cref="PreparedTrackingRequest"/> object to a data stream
        /// </summary>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Engine", Engine.ToString());
            info.AddValue("RequestID", RequestID);
            info.AddValue("RequiredFinalizations", RequiredFinalizations);
            SerializeRequest(Request, info, context);
        }

        #endregion
    }
}
