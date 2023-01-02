using RestSharp;
using System;
using System.Reflection;

namespace SierraLib.Analytics.Implementation
{
    /// <summary>
    /// Provides information about an application which is being tracked.
    /// </summary>
    public interface ITrackingApplication
    {
        /// <summary>
        /// Gets the <see cref="Name"/> associated with the application
        /// being tracked.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the <see cref="Version"/> of the application being tracked.
        /// </summary>
        string Version { get; }
    }

    /// <summary>
    /// Base interface for all tracking modules which provide
    /// tracker specific structure and functionality handling.
    /// </summary>
    public interface ITrackingModule
    {
        /// <summary>
        /// Called by the <see cref="TrackingEngine"/> to enable the tracking
        /// module to perform pre-processing on a request.
        /// </summary>
        /// <param name="request">The <see cref="RestRequest"/> which is being sent.</param>
        void PreProcess(RestRequest request);
    }

    /// <summary>
    /// Indicates that a tracking module needs to perform some kind
    /// of post-processing on the request before it is queued for sending.
    /// </summary>
    /// <remarks>
    /// This can be used to handle protocol requirements which need certain
    /// other modules to have been used (or not) for this module to be
    /// used.
    /// </remarks>
    public interface ITrackingPostProcess : ITrackingModule
    {
        /// <summary>
        /// Called by the <see cref="TrackingEngine"/> to enable the tracking
        /// module to perform post-processing on a request before it is enqueued
        /// for sending.
        /// </summary>
        /// <param name="request">The <see cref="RestRequest"/> which is being enqueued.</param>
        void PostProcess(RestRequest request);
    }

    /// <summary>
    /// Indicates that a tracking module needs to perform some kind of
    /// finalization on the request before it is sent, but which needs
    /// to be executed just before the request is sent.
    /// </summary>
    public interface ITrackingFinalize : ITrackingModule
    {
        /// <summary>
        /// Called by the <see cref="TrackingEngine"/> to enable the tracking
        /// module to perform some finalization on a request just prior to
        /// sending.
        /// </summary>
        /// <param name="request">The <see cref="RestRequest"/> which is being sent.</param>
        void FinalizeRequest(RestRequest request);
    }
}
