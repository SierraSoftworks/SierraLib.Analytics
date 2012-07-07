using System;
using System.Collections.Generic;
using System.Text;

namespace SierraLib.Analytics.GoogleAnalytics
{
    delegate void HitDispatchedEventHandler(object sender, Hit e);
    interface IDispatcher
    {
        void DispatchHits(Hit[] hits, int wait = 0);

        void Initialize();

        void Stop();
        
        bool Busy { get; }

        event HitDispatchedEventHandler HitDispatched;

        event EventHandler DispatchFinished;
    }
}
