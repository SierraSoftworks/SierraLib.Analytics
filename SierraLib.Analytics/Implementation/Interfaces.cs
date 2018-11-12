using RestSharp;

namespace SierraLib.Analytics.Implementation
{
    public interface ITrackingApplication
    {
        string Name { get; }
        string Version { get; }
    }

    /// <summary>
    /// Base interface for all tracking modules which provide
    /// tracker specific structure and functionality handling.
    /// </summary>
    public interface ITrackingModule
    {
        void PreProcess(IRestRequest request);
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
        void PostProcess(IRestRequest request);
    }

    /// <summary>
    /// Indicates that a tracking module needs to perform some kind of
    /// finalization on the request before it is sent, but which needs
    /// to be executed just before the request is sent.
    /// </summary>
    public interface ITrackingFinalize : ITrackingModule
    {
        void FinalizeRequest(IRestRequest request);
    }
}
