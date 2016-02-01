using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using SierraLib.Analytics.Google;
using System.Threading;
using Akavache;
using System.Diagnostics;
using RestSharp;
using System.IO;

namespace SierraLib.Analytics.Test
{
    [UniversalAnalytics(GoogleAnalytics.Account)]
    [TrackingApplication(Name = "SierraLib.Analytics.Tests", Version = "1.0.0")]
    [TestClass]
    public class GoogleAnalytics
    {
        public const string Account = "UA-40095482-1";

        #region Core Logic Tests

        [TestMethod, TestCategory("Core Logic")]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Tracking_DirectConstructorCall()
        {
            var engine = new UniversalAnalytics(Account);
        }

        [TestMethod, TestCategory("Core Logic")]
        public void Tracking_CreateEngine()
        {
            var engine = TrackingEngine.Create(Account, x => new UniversalAnalytics(x));

            Assert.IsNotNull(engine);
            Assert.IsInstanceOfType(engine, typeof(UniversalAnalytics));
            Assert.AreEqual(Account, engine.TrackingID);
        }

        [TestMethod, TestCategory("Core Logic")]
        public void Tracking_GetInheritedEngine()
        {
            var engine = TrackingEngine.GetEngine(() => Tracking_GetInheritedEngine());
            Assert.IsNotNull(engine);
            Assert.IsInstanceOfType(engine, typeof(UniversalAnalytics));
        }
        
        [TestMethod, TestCategory("Core Logic")]
        public void Tracking_SetUserAgent()
        {
            var engine = TrackingEngine.GetEngine(() => Tracking_SetUserAgent());
            Assert.IsNotNull(engine);
            Assert.IsInstanceOfType(engine, typeof(UniversalAnalytics));

            var oldUserAgent = engine.UserAgent;
            
            engine.UpdateUserAgent("SierraLib.Analytics.Test", "1.0.0", "CustomUA");
            Assert.IsTrue(engine.UserAgent.IndexOf("CustomUA") != -1, "Custom UserAgent not propagated");
            engine.UserAgent = oldUserAgent;
            Assert.AreEqual(oldUserAgent, engine.UserAgent);
        }

        #endregion

        #region Tracking Tests

        [TestMethod, TestCategory("Tracking")]
        [AppView("BasicAppView")]
        public void Tracking_BasicAppView()
        {
            TrackingEngine.TrackDefault(() => Tracking_BasicAppView(), TrackOn.Entry);

            TrackingEngine.WaitForActive(TimeSpan.FromSeconds(30));
        }

        [TestMethod, TestCategory("Tracking")]
        [Event("Tests", "MethodEntry", "TestEvent()", Filter = TrackOn.Entry)]
        public void Tracking_BasicEvent()
        {
            TrackingEngine.TrackDefault(() => Tracking_BasicEvent(), TrackOn.Entry);
        }

        #endregion

        #region Request Serialization Tests

        [TestMethod]
        public void Tracking_Serialization()
        {
            var engine = TrackingEngine.GetEngine(() => Tracking_Serialization()) as UniversalAnalytics;
            var request = new RestRequest() {
                Resource = "/collect",
                Method = Method.POST
            };
            request.AddParameter("q", "Test", ParameterType.GetOrPost);
            request.AddParameter("r", "Yargh!", ParameterType.GetOrPost);
            var pendingRequest = new PreparedTrackingRequest(engine, request, new Implementation.ITrackingFinalize[0]);
            PreparedTrackingRequest deserialized = null;
            using (var ms = new MemoryStream(pendingRequest.Serialize()))            
                deserialized = (PreparedTrackingRequest)ms.ToArray().ToPreparedTrackingRequest();

            Assert.IsNotNull(deserialized);
            Assert.IsNotNull(deserialized.Request);
            Assert.AreEqual(pendingRequest.Generated, deserialized.Generated);
            Assert.AreEqual(pendingRequest.Engine, deserialized.Engine);

            Assert.AreEqual(pendingRequest.Request.Parameters.Count, request.Parameters.Count);
            for (int i = 0; i < pendingRequest.Request.Parameters.Count; i++)
            {
                var got = pendingRequest.Request.Parameters[i];
                var expected = request.Parameters[i];
                Assert.AreEqual(expected.Type, got.Type);
                Assert.AreEqual(expected.Name, got.Name);
                Assert.AreEqual(expected.Value, got.Value);
            }
        }

        #endregion

        //[TestMethod]
        //public void TestCustomVariables()
        //{
        //    Tracker tracker = Tracker.CurrentInstance;

        //    tracker.CustomVariables[1] = new CustomVariable(1, "AppName", "SierraLib.Analytics.Test");
        //    tracker.CustomVariables[2] = new CustomVariable(2, "AppVersion", "1.4.2");
        //    tracker.CustomVariables[3] = new CustomVariable(3, "Build", "Debug");

        //    Assert.IsNotNull(tracker.CustomVariables[1]);
        //    Assert.IsNotNull(tracker.CustomVariables[2]);
        //    Assert.IsNotNull(tracker.CustomVariables[3]);
        //    Assert.IsNull(tracker.CustomVariables[4]);
        //    Assert.IsNull(tracker.CustomVariables[5]);

        //    tracker.TrackEvent("Analytics Tests", "Events", "TestEvent");
        //    Assert.IsTrue(tracker.DispatcherBusy || tracker.Dispatch());

        //    Assert.IsNull(tracker.CustomVariables[1]);
        //    Assert.IsNull(tracker.CustomVariables[2]);
        //    Assert.IsNull(tracker.CustomVariables[3]);
        //    Assert.IsNull(tracker.CustomVariables[4]);
        //    Assert.IsNull(tracker.CustomVariables[5]);

        //    tracker.Dispatch();
        //    tracker.WaitForDispatch();
        //}

    }
}
