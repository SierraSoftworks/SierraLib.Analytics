using System;
using System.Threading;
using System.Threading.Tasks;
using Akavache;
using SierraLib.Analytics;

namespace SierraLib.Analytics.Example
{
    [Google.UniversalAnalytics("UA-123456789-1")]
    [TrackingApplication(Name = "SierraLib.Analytics.Example", Version = "1.0.0-dev")]
    public class Program
    {
        public static void Main(string[] args)
        {
            // Setup the name of your application (used to persist events across sessions)
            BlobCache.ApplicationName = "SierraLib.Analytics.Example";

            Console.WriteLine("Sending events from previous session");
            TrackingEngine.ProcessStoredRequests();

            new Program().Run();
        }

        [Google.Title("Example Startup")]
        [Google.PageView("/")]
        [TrackOnEntry]
        private void Run()
        {
            Console.WriteLine("Running Application");

            try
            {
                ThrowAnException();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An exception was thrown, but we reported it ({ex})");
            }
        }

        [Google.TrackOnException]
        private void ThrowAnException()
        {
            throw new Exception("This is an example exception");
        }
    }
}
