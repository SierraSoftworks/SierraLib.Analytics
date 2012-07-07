using System;
using System.Collections.Generic;
using System.Text;

namespace SierraLib.Analytics.GoogleAnalytics
{
    interface IHitStore
    {
        bool PushEvent(Event paramEvent);

        Hit[] GetHits();
        Hit[] GetHits(int maxResults);

        void RemoveHit(long hitID);

        int HitCount { get; }

        int StoreID { get; }

        string VisitorID { get; }

        string SessionID { get; }

        Referrer Referrer { get; }

        bool SetReferrer(string url);

        bool SetReferrer(Referrer referrer);

        Referrer GetAndUpdateReferrer();

        void ClearReferrer();

        void LoadExistingSession();

        void StartNewVisit();

        string GetCustomVar(int varID);

        bool AnonymizeIP { get; set; }

        int SampleRate { get; set; }
    }
}
