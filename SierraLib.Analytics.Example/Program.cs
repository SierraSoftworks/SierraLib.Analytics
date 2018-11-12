using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SierraLib.Analytics;

namespace SierraLib.Analytics.Example
{
    [Google.UniversalAnalytics("UA-123456789-1")]
    public class Program
    {
        public void Main(string[] args)
        {
            Console.WriteLine("Sending events from previous session");
            TrackingEngine.ProcessStoredRequests();

            TrackingEngine.WaitForActive();
            Console.WriteLine("Previous session events sent");
        }

        [Google.Title("Example Startup")]
        [Google.PageView("/")]
        [TrackOnEntry]
        private static void Run()
        {
            Console.WriteLine("Running Application");
        }
    }
}
