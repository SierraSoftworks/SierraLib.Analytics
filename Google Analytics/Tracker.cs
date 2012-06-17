using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using SierraLib.Databases;
using SierraLib.Databases.SQLite;

namespace SierraLib.Analytics.GoogleAnalytics
{
    /// <summary>
    /// This class provides Google Analytics tracking similar to that
    /// provided by the official Google Analytics libraries.
    /// </summary>
    public class Tracker
    {
        #region Static Variables


        public const string Product = "GoogleAnalytics";

        /// <summary>
        /// This represents the version of the Google Analytics Mobile SDK
        /// library that this is modelled on.
        /// </summary>
        public const string LibraryVersion = "1.4.2";

        /// <summary>
        /// This represents the version of the Google Analytics API that
        /// this library uses. The "ma" at the end represents Mobile Analytics
        /// </summary>
        public const string APIVersion = "5.2.2";

        /// <summary>
        /// This represents the current instance of the Google Analytics tracker
        /// which is being used by the running application.
        /// </summary>
        private static Tracker instance;

        /// <summary>
        /// Gets the current instance of this application which can be used for
        /// tracking the current application.
        /// </summary>
        /// <remarks>
        /// This value is lazily initialized to improve performance when not used
        /// in applications. This means the value will only be populated when it
        /// is first used. This can improve startup performance in your applications.
        /// </remarks>
        public static Tracker CurrentInstance
        { 
            get 
            {
                if (instance == null)
                    instance = new Tracker();
                return instance; 
            } 
        }


        #endregion

        #region Private Variables

        IDispatcher dispatcher = null;
        DateTime lastDispatch = DateTime.MinValue;

        Dictionary<string, Transaction> transactionQue;
        Dictionary<string, List<Item>> itemQue;

        Timer timer;
        
        /// <summary>
        /// Gets the HitStore used to store events that should be transmitted to Google Analytics
        /// </summary>
        internal IHitStore HitStore
        { get; set; }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or Sets a value indicating whether or not Google Analytics
        /// tracking will be enabled
        /// </summary>
        /// <remarks>
        /// Setting this to <c>false</c> will cause calls to 
        /// <see cref="TrackEvent"/>
        /// <see cref="TrackPageView"/>
        /// <see cref="TrackTransaction"/>
        /// to do nothing
        /// </remarks>
        public bool Enabled
        { get; set; }

        /// <summary>
        /// Gets or Sets the name of the product which is currently running
        /// </summary>
        public string ProductName
        { get; set; }

        /// <summary>
        /// Gets or Sets the version string for the product which is currently running
        /// </summary>
        public string ProductVersion
        { get; set; }

        /// <summary>
        /// Determines whether or not the library will output debug information
        /// to the terminal.
        /// </summary>
        public bool Debug
        { get; set; }

        /// <summary>
        /// Determines whether or not the library will send data to the Analytics
        /// servers. If set to true, no data will be sent.
        /// </summary>
        public bool DryRun
        { get; set; }

        /// <summary>
        /// Determines whether or not IP addresses are modified to anonymize them.
        /// </summary>
        /// <remarks>
        /// This is done by dropping the last part of the IP address. This allows
        /// you to receive somewhat accurate analytics while still providing anonymity
        /// for your users.
        /// </remarks>
        public bool AnonymizeIP
        { get; set; }

        /// <summary>
        /// Determines whether or not the application will allow the Google Analytics
        /// servers to determine the time at which the event is sent.
        /// </summary>
        public bool UseServerTime
        { get; set; }

        /// <summary>
        /// Determines the sample rate which is used to randomly select users
        /// for analytics. This is a range from 0 to 100 percent.
        /// </summary>
        public int SampleRate
        { get; set; }

        /// <summary>
        /// Gets or Sets the Google Analytics account ID used to determine
        /// where tracking events are routed on Google's servers.
        /// </summary>
        public string AccountID
        { get; set; }

        /// <summary>
        /// Determines whether or not SSL connections are used when requesting
        /// data from Google Analytics.
        /// </summary>
        public bool UseSecureConnections
        { get; set; }

        private int dispatchPeriod = 0;
        /// <summary>
        /// Gets or Sets the dispatch period, or the amount of time between
        /// dispatches in seconds.
        /// </summary>
        public int DispatchPeriod
        {
            get { return dispatchPeriod; }
            set
            {
                dispatchPeriod = value;
                if (value <= 0)
                {
                    if(timer != null)
                        timer.Change(Timeout.Infinite, 0);
                    maybeScheduleNextDispatch();
                }
                else
                {
                    if (dispatcher != null)
                        dispatcher.Stop();
                    if (timer != null)
                        timer.Change(0, value * 1000);
                }
            }
        }

        /// <summary>
        /// Gets or Sets a value indicating whether or not the library should
        /// run in Power Save mode.
        /// </summary>
        public bool PowerSaveMode
        { get; set; }

        /// <summary>
        /// Gets a value indicating whether or not the dispatcher is currently busy
        /// </summary>
        public bool DispatcherBusy
        {
            get
            {
                if (dispatcher != null)
                    return dispatcher.Busy;
                return false;
            }
        }

        /// <summary>
        /// Gets the <see cref="AdHitIDGenerator"/> used for generating
        /// AdHitIDs for AdWord tracking.
        /// </summary>
        public AdHitIDGenerator AdHitGenerator
        { get; private set; }

        /// <summary>
        /// Allows you to set custom reporting variables to be sent with your events
        /// </summary>
        public CustomVariableBuffer CustomVariables
        { get; set; }

        /// <summary>
        /// Gets or Sets the current location of the tracking database file
        /// </summary>
        public string DatabaseLocation
        { get; set; }

        /// <summary>
        /// Gets or Sets the default domain to which pages will belong.
        /// </summary>
        /// <remarks>Set this to <c>null</c> to disable the domain
        /// functionality and revert to the default FAKE_DOMAIN_HASH</remarks>
        public string Domain
        { get; set; }

        /// <summary>
        /// Gets or Sets the referrer to be bundled with future pageviews
        /// </summary>
        public Referrer Referrer
        {
            get
            {
                if (HitStore != null)
                    return HitStore.Referrer;
                return new Referrer();
            }
            set
            {
                if (HitStore == null)
                    throw new InvalidOperationException("Call StartSession before attempting to set a referrer");
                if (value != null)
                    HitStore.SetReferrer(value);
                else
                    HitStore.SetReferrer(new Referrer());
            }
        }

        /// <summary>
        /// Gets or Sets the current language code in &lt;lang&gt;-&lt;country&gt;
        /// format.
        /// </summary>
        public string CurrentLanguage
        {
            get;
            set;
        }

        #endregion

        /// <summary>
        /// Constructor which initializes default values for the current instance.
        /// </summary>
        internal Tracker()
        {
            Enabled = true;
            timer = new Timer(new TimerCallback((o) =>
                {
                    maybeScheduleNextDispatch();
                }), null, DispatchPeriod > 0 ? DispatchPeriod * 1000 : Timeout.Infinite, DispatchPeriod > 0 ? DispatchPeriod * 1000 : 0);

            SampleRate = 100;
            CustomVariables = new CustomVariableBuffer();
            PowerSaveMode = false;

            ProductName = Product;
            ProductVersion = LibraryVersion;

            CurrentLanguage = Thread.CurrentThread.CurrentCulture.Name;

            HitStore = null;

            transactionQue = new Dictionary<string, Transaction>();
            itemQue = new Dictionary<string, List<Item>>();

            AccountID = null;

        }



        #region Start Functions
        
        /// <summary>
        /// Starts a new tracking session
        /// </summary>
        /// <param name="accountCode">Google Analytics account code (UA-xxxxxxxx-xx)</param>
        /// <param name="databaseFile">The location of the database file to be used for storing persistent tracking information</param>
        /// <param name="startNewVisit">Should a new visit be started automatically?</param>
        /// <param name="dispatchInterval">The new dispatch interval to be used</param>
        public void StartSession(string accountCode, string databaseFile, bool startNewVisit = true, int dispatchInterval = -1)
        {
            AccountID = accountCode;
            DispatchPeriod = dispatchInterval;

            DatabaseLocation = databaseFile;

            
            HitStore = new SQLiteHitStore(DatabaseLocation);
            HitStore.AnonymizeIP = AnonymizeIP;
            HitStore.SampleRate = SampleRate;
            HitStore.GetAndUpdateReferrer();
            HitStore.LoadExistingSession();
            
            if (dispatcher == null)
            {
                dispatcher = new NetworkDispatcher(ProductName, ProductVersion);
                dispatcher.HitDispatched += new HitDispatchedEventHandler(dispatcher_HitDispatched);
            }

            if (AdHitGenerator == null)
                AdHitGenerator = new AdHitIDGenerator(true);

            if (startNewVisit)
                HitStore.StartNewVisit();

            dispatcher.Initialize();
        }

        /// <summary>
        /// Starts a new tracking session
        /// </summary>
        /// <param name="accountCode">Google Analytics account code (UA-xxxxxxxx-xx)</param>
        /// <param name="database">The SQLiteDatabase which contains the tracking records</param>
        /// <param name="startNewVisit">Should a new visit be started automatically?</param>
        /// <param name="dispatchInterval">The new dispatch interval to be used</param>
        public void StartSession(string accountCode, SQLiteDatabase database, bool startNewVisit = true, int dispatchInterval = -1)
        {
            AccountID = accountCode;
            DispatchPeriod = dispatchInterval;

            DatabaseLocation = database.Path;

            HitStore = new SQLiteHitStore(database);
            HitStore.AnonymizeIP = AnonymizeIP;
            HitStore.SampleRate = SampleRate;
            HitStore.GetAndUpdateReferrer();
            HitStore.LoadExistingSession();
            if (dispatcher == null)
            {
                dispatcher = new NetworkDispatcher(ProductName, ProductVersion);
                dispatcher.HitDispatched += new HitDispatchedEventHandler(dispatcher_HitDispatched);
            }

            if (AdHitGenerator == null)
                AdHitGenerator = new AdHitIDGenerator(true);

            if (startNewVisit)
                HitStore.StartNewVisit();

            dispatcher.Initialize();
        }


        void dispatcher_HitDispatched(object sender, Hit e)
        {
            HitStore.RemoveHit(e.HitID);
        }

        #endregion

        #region Tracking Functions
        
        /// <summary>
        /// Sets a new custom variable at the given index
        /// </summary>
        /// <param name="index">The index of the variable from 1 to 5</param>
        /// <param name="name">The name of the custom variable</param>
        /// <param name="value">The value for the custom variable</param>
        /// <param name="scope">The <see cref="CustomVariable.Scopes"/> of the variable</param>
        public void SetCustomVariable(int index, string name, string value, CustomVariable.Scopes scope = CustomVariable.Scopes.Page)
        {
            CustomVariables[index] = new CustomVariable(index, name, value, scope);
        }

        /// <summary>
        /// Tracks a new event with any set custom variables
        /// </summary>
        /// <param name="category">The category to which the even belongs</param>
        /// <param name="action">The action performed to trigger the event</param>
        /// <param name="label">The label describing the event</param>
        /// <param name="value">The value of the event</param>
        public void TrackEvent(string category, string action, string label = "", int value = 0)
        {
            if (!Enabled)
                return;
            if (HitStore == null)
                return;


            if (category == null || category.Trim().Length == 0)
                throw new ArgumentNullException("category", "Category cannot be null or empty");
            if (action == null || action.Trim().Length == 0)
                throw new ArgumentNullException("action", "Action cannot be null or empty");
            
            HitStore.PushEvent(createEvent(AccountID, category, action, label, value));
        }

        /// <summary>
        /// Tracks a pageview with any set custom variables
        /// </summary>
        /// <param name="page">The path to the page which should be tracked</param>
        /// <param name="pageTitle">An optional title for the page</param>
        public void TrackPageView(string page, string pageTitle = null)
        {
            if (!Enabled)
                return;
            if (HitStore == null)
                return;

            HitStore.PushEvent(createEvent(AccountID, Event.PageViewEventCategory, page, pageTitle, 0));
        }
        
        /// <summary>
        /// Submits a transaction set for tracking
        /// </summary>
        /// <remarks>
        /// This function should be used after calls to <see cref="AddTransaction"/>
        /// and/or <see cref="AddItem"/>
        /// </remarks>
        public void TrackTransactions()
        {
            if (!Enabled)
                return;
            if (HitStore == null)
                return;
                
            foreach (Transaction transaction in transactionQue.Values)
            {
                Event e = createEvent(AccountID, Event.TransactionCategory, "","",0);
                e.Transaction = transaction;
                HitStore.PushEvent(e);

                if (itemQue.ContainsKey(transaction.OrderID))
                {
                    foreach (Item item in itemQue[transaction.OrderID])
                    {
                        Event itemEvent = createEvent(AccountID, Event.ItemCategory, "", "", 0);
                        itemEvent.Item = item;
                        HitStore.PushEvent(itemEvent);
                    }
                }
            }

            transactionQue.Clear();
            itemQue.Clear();
        }

        /// <summary>
        /// Clears any existing transaction items from the buffer
        /// </summary>
        public void ClearTransactions()
        {
            transactionQue.Clear();
            itemQue.Clear();
        }

        #endregion

        #region Transaction Functions

        /// <summary>
        /// Adds a new transaction to the transaction buffer
        /// </summary>
        /// <param name="transaction">The <see cref="Transaction"/> to add to the buffer</param>
        public void AddTransaction(Transaction transaction)
        {
            transactionQue.Add(transaction.OrderID, transaction);
        }

        /// <summary>
        /// Adds a new item to the transaction buffer
        /// </summary>
        /// <param name="item">
        /// The <see cref="Item"/> to add to the buffer
        /// </param>
        public void AddItem(Item item)
        {
            if (!transactionQue.ContainsKey(item.OrderID))
                AddTransaction(new Transaction(item.OrderID, item.ItemPrice));
            if (!itemQue.ContainsKey(item.OrderID))
                itemQue.Add(item.OrderID, new List<Item>() { item });
            else
                itemQue[item.OrderID].Add(item);
        }

        #endregion

        #region Other Functions

        /// <summary>
        /// Blocks the currently executing thread until the dispatcher
        /// completes any pending operations
        /// </summary>
        /// <returns>
        /// Returns <c>true</c> if the current thread was blocked or
        /// <c>false</c> if the dispatcher was not busy and no hits
        /// were pending.
        /// </returns>
        public bool WaitForDispatch(int timeout = 5000)
        {
            int time = 0;
            if (!DispatcherBusy && HitStore.HitCount == 0)
                return false;
            while (DispatcherBusy || HitStore.HitCount > 0)
            {
                Thread.CurrentThread.Join(50);
                time += 50;
                if (time > timeout)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Forces a manual dispatch of any pending records in the
        /// database.
        /// </summary>
        /// <returns>
        /// Returns <c>true</c> if a new dispatch was started or <c>false</c>
        /// if a dispatch was in progress.
        /// </returns>
        public bool Dispatch()
        {
            if (DispatcherBusy)
            {
                maybeScheduleNextDispatch();
                return false;
            }

            if (HitStore.HitCount == 0)
                return false;

            else
            {
                dispatcher.DispatchHits(HitStore.GetHits());
                maybeScheduleNextDispatch();
                return true;
            }
        }

        /// <summary>
        /// Stops a currently running dispatch as soon as possible
        /// </summary>
        [Obsolete("This function is obsolete and should not be used")]
        public void Stop()
        {
            if (dispatcher != null)
                dispatcher.Stop();
        }

        #endregion

        #region Utility Functions

        private Event createEvent(string accountCode, string category, string action, string label, int value)
        {
            var screen = new Microsoft.VisualBasic.Devices.Computer().Screen.Bounds;
            Event e = new Event(accountCode, category, action, label,
                value, screen.Height, screen.Width);
            e.CustomVariables = CustomVariables;
            e.AdHitID = AdHitGenerator != null ? AdHitGenerator.AdHitID : 0;
            e.UseServerTime = UseServerTime;
            CustomVariables = new CustomVariableBuffer();
            return e;
        }

        private void maybeScheduleNextDispatch()
        {
            if (DispatchPeriod < 0)
                return;

            int waitTime = (int)(DateTime.Now.AddSeconds(DispatchPeriod) > lastDispatch ? 
                DateTime.Now.AddSeconds(DispatchPeriod).Subtract(lastDispatch).TotalMilliseconds :
                0);

            if(dispatcher != null && HitStore != null)
                dispatcher.DispatchHits(HitStore.GetHits(), waitTime);
        }

        #endregion
    }
}
