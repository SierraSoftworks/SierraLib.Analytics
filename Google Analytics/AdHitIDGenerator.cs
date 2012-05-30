using System;
using System.Collections.Generic;
using System.Text;

namespace SierraLib.Analytics.GoogleAnalytics
{
    public class AdHitIDGenerator
    {
        private bool adsEnabled = false;

        public AdHitIDGenerator(bool enableAds)
        {
            adsEnabled = enableAds;
        }

        public int AdHitID
        {
            get
            {
                if (!adsEnabled)
                    return 0;
                return AdMobInfo.Instance.GenerateHitID();
            }
        }
    }


}
