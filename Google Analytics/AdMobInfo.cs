using System;
using System.Collections.Generic;
using System.Text;

namespace SierraLib.Analytics.GoogleAnalytics
{
    public class AdMobInfo
    {
        private static AdMobInfo _instance = null;
        public static AdMobInfo Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new AdMobInfo();
                return _instance;
            }
        }

        public int AdHitID
        { get; set; }

        public string JoinID
        {
            get
            {
                if (AdHitID == 0)
                    return null;

                Tracker tracker = Tracker.CurrentInstance;

                string vid = tracker.HitStore.VisitorID;
                string sid = tracker.HitStore.SessionID;

                if (vid == null || sid == null)
                    return null;

                return String.Format("A,{0},{1},{2}", vid, sid, AdHitID);
            }
        }

        Random random = new Random();

        public int GenerateHitID()
        {
            AdHitID = random.Next();
            return AdHitID;
        }
    }
}
