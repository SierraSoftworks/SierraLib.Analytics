using System;
using System.Collections.Generic;
using System.Text;

namespace SierraLib.Analytics.GoogleAnalytics
{
    public class Referrer
    {
        public string URL { get; private set; }

        public string CookieString { get; private set; }

        public long Timestamp { get; private set; }

        public int Visit { get; private set; }

        public int Index { get; private set; }

        public Referrer(string referrer, string cookieString, long timestamp, int visit, int index)
        {
            URL = referrer;
            CookieString = cookieString;
            Timestamp = timestamp;
            Visit = visit;
            Index = index;
        }

        public Referrer(string url, string referralURL, string campaign, string medium, long timestamp, int visit, int index)
            : this(url,"utmcsr=" + referralURL + "|utmccn=" + campaign + "|utmcmd=" + medium, timestamp, visit, index)
        {

        }

        public Referrer()
            : this("-", "(direct)","(direct)","(none)",(long)DateTime.Now.Subtract(new DateTime(1970,1,1)).TotalMilliseconds,1,1)
        {

        }

    }
}
