using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SierraLib.Analytics
{
    /// <summary>
    /// Determines what triggers will cause the attached <see cref="TrackingModuleAttributeBase"/>
    /// to be included in the tracked data bundle.
    /// </summary>
    [Flags]
    public enum TrackOn
    {
        /// <summary>
        /// Tracking will occur on entry into the marked method
        /// </summary>
        Entry = 0x1,

        /// <summary>
        /// Tracking will occur on exit from the marked method
        /// </summary>
        Exit = 0x2,

        /// <summary>
        /// Tracking will occur when an exception is generated within the marked method
        /// </summary>
        Exception = 0x4
    }
}
