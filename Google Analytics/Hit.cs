using System;
using System.Collections.Generic;
using System.Text;

namespace SierraLib.Analytics.GoogleAnalytics
{
    public class Hit
    {
        public string HitString
        { get; private set; }

        public long HitID
        { get; private set; }

        public Hit(string hitString, long hitID)
        {
            HitString = hitString;
            HitID = hitID;
        }
    }
}
