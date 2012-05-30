using System;
using System.Collections.Generic;
using System.Text;

using SierraLib.Databases.SQLite;
using SierraLib.Databases;
using System.Diagnostics;
using SierraLib.Databases.Databases;
using System.IO;

namespace SierraLib.Analytics.GoogleAnalytics
{
    class SQLiteHitStore : IHitStore
    {

        #region Constants

        const int MAX_HITS = 1000;

        //Constants used for Event table
        const string STORE_ID = "store_id";
        const string EVENT_ID = "event_id";
        const string SCREEN_WIDTH = "screen_width";
        const string SCREEN_HEIGHT = "screen_height";
        const string VALUE = "value";
        const string LABEL = "label";
        const string ACTION = "action";
        const string CATEGORY = "category";
        const string VISITS = "visits";
        const string TIMESTAMP_CURRENT = "timestamp_current";
        const string TIMESTAMP_PREVIOUS = "timestamp_previous";
        const string TIMESTAMP_FIRST = "timestamp_first";
        const string RANDOM_VAL = "random_val";
        const string ACCOUNT_ID = "account_id";
        const string USER_ID = "user_id";


        //Constants used for Referrer table
        const string REFERRER_COLUMN = "referrer";
        const string REFERRER = "referrer";
        const string TIMESTAMP_REFERRER = "timestamp_referrer";
        const string REFERRER_VISIT = "referrer_visit";
        const string REFERRER_INDEX = "referrer_index";
        const string REFERRER_URL = "referrer_url";


        //Constants used for CustomVars table
        const string CUSTOMVAR_ID = "cv_id";
        const string CUSTOMVAR_INDEX = "cv_index";
        const string CUSTOMVAR_NAME = "cv_name";
        const string CUSTOMVAR_VALUE = "cv_value";
        const string CUSTOMVAR_SCOPE = "cv_scope";
        const string CUSTOMVAR_COLUMN_TYPE = "CHAR(64)";


        //Constants used for Transactions table
        const string TRANSACTION_ID = "tran_id";
        const string ORDER_ID = "order_id";
        const string STORE_NAME = "tran_storename";
        const string TOTAL_COST = "tran_totalcost";
        const string TOTAL_TAX = "tran_totaltax";
        const string SHIPPING_COST = "tran_shippingcost";


        //Constants used for Item table
        const string ITEM_ID = "item_id";
        const string ITEM_SKU = "item_sku";
        const string ITEM_NAME = "item_name";
        const string ITEM_CATEGORY = "item_category";
        const string ITEM_PRICE = "item_price";
        const string ITEM_COUNT = "item_count";
        

        //Constants used for Hit table
        const string HIT_ID = "hit_id";
        const string HIT_STRING = "hit_string";
        const string HIT_TIMESTAMP = "hit_timestamp";


        //Database constants
        const string DefaultDBFile = "%AppData%\\Sierra Softworks\\Analytics\\trackingStore.db";
        const int AnalyticsDBVersion = 5;

        #endregion

        #region Private Variables

        static SQLiteDatabase database = null;

        bool sessionStarted = false;

        #endregion

        #region Properties

        public int StoreID
        {
            get;
            private set;
        }

        public string VisitorID
        {
            get;
            private set;
        }

        public string SessionID
        {
            get;
            private set;
        }

        public Referrer Referrer
        {
            get;
            private set;
        }

        public bool AnonymizeIP
        {
            get;
            set;
        }

        public int SampleRate
        {
            get;
            set;
        }

        public bool UseStoredVisitorVariables
        { get; set; }

        public CustomVariableBuffer VisitorVariableCache
        { get; private set; }

        public int TimestampFirst
        { get; private set; }

        public int TimestampPrevious
        { get; private set; }

        public int TimestampCurrent
        { get; private set; }

        public int VisitsCount
        { get; private set; }

        #endregion


        public SQLiteHitStore() :
            this(Environment.ExpandEnvironmentVariables(DefaultDBFile))
        { }

        public SQLiteHitStore(string databaseFile)
        {
            if (!Directory.Exists(Path.GetDirectoryName(databaseFile)))
            {
                if (Tracker.CurrentInstance.Debug)
                    Debug.WriteLine("Specified database file folder does not exist, creating: " + Path.GetDirectoryName(databaseFile));
                Directory.CreateDirectory(Path.GetDirectoryName(databaseFile));
            }
            if(database == null)
                database = new SQLiteDatabase(databaseFile, true);

            SampleRate = 100;

            VisitorVariableCache = new CustomVariableBuffer();

            //Initialize database
            CreateEventsTable();
            CreateInstallReferrerTable();
            CreateSessionTable();

            Referrer = new Referrer();

            if (AnalyticsDBVersion > 1)
            {
                CreateCustomVariablesTable();
                CreateCustomVariableCacheTable();
            }
            if (AnalyticsDBVersion > 2)
            {
                CreateTransactionEventsTable();
                CreateItemEventsTable();
            }
            if (AnalyticsDBVersion > 3)
            {
                CreateHitsTable();
                CreateReferrerTable();
            }
        }

        public SQLiteHitStore(DatabaseBase db)
        {
            database = (SQLiteDatabase)db;
            SampleRate = 100;

            VisitorVariableCache = new CustomVariableBuffer();

            //Initialize database
            CreateEventsTable();
            CreateInstallReferrerTable();
            CreateSessionTable();

            Referrer = new Referrer();

            if (AnalyticsDBVersion > 1)
            {
                CreateCustomVariablesTable();
                CreateCustomVariableCacheTable();
            }
            if (AnalyticsDBVersion > 2)
            {
                CreateTransactionEventsTable();
                CreateItemEventsTable();
            }
            if (AnalyticsDBVersion > 3)
            {
                CreateHitsTable();
                CreateReferrerTable();
            }
        }


        #region Public Functions


        public bool PushEvent(Event e)
        {
            if (HitCount >= MAX_HITS)
                return false;

            if(SampleRate != 100)
                if ((e.UserID == -1 ? StoreID : e.UserID) % 1000 >= SampleRate * 100)
                {
                    Debug.WriteLine("User has been sampled out, aborting hit.");
                    return false;
                }

            lock (database)
            {
                if (!sessionStarted)
                    storeUpdatedSession();

                if (!e.SessionInitialized)
                {
                    e.RandomValue = new Random().Next(int.MaxValue - 1);
                    e.TimestampFirst = TimestampFirst;
                    e.TimestampPrevious = TimestampPrevious;
                    e.TimestampCurrent = TimestampCurrent;
                    e.VisitCount = VisitsCount;
                }

                e.AnonymizeIP = AnonymizeIP;
                if (e.UserID == -1)
                    e.UserID = StoreID;

                storeCustomVariables(e);

                Referrer referrer = GetAndUpdateReferrer();

                string[] accountIDs = e.AccountID.Split(',');
                if (accountIDs.Length == 1)
                    storeEvent(e, referrer, true);
                else
                    foreach (string str in accountIDs)
                        storeEvent(new Event(e, str), referrer, true);
            }

            return true;
        }

        public Event[] GetEvents(int maxEvents, bool useCustomVariables)
        {
            List<Event> events = new List<Event>();

            RecordCollection records = database["events"].Records;

            foreach (DatabaseRecordBase record in records)
            {
                Event newEvent = new Event(
                        record[EVENT_ID].ForceConvertTo<long>(),
                        record[ACCOUNT_ID].ToString(),
                        record[RANDOM_VAL].ConvertTo<int>(),
                        record[TIMESTAMP_FIRST].ConvertTo<int>(),
                        record[TIMESTAMP_PREVIOUS].ConvertTo<int>(),
                        record[TIMESTAMP_CURRENT].ConvertTo<int>(),
                        record[VISITS].ConvertTo<int>(),
                        record[CATEGORY].ToString(),
                        record[ACTION].ToString(),
                        record[LABEL].ToString(),
                        record[VALUE].ConvertTo<int>(),
                        record[SCREEN_HEIGHT].ConvertTo<int>(),
                        record[SCREEN_WIDTH].ConvertTo<int>()
                    );

                newEvent.UserID = record[USER_ID].ConvertTo<int>();

                if (newEvent.Category == "__##GOOGLETRANSACTION##__")
                {
                    //Load the transaction as well
                    try
                    {
                        DatabaseRecordBase trans = database["transaction_events"][EVENT_ID, newEvent.EventID.ToString()][0];

                        newEvent.Transaction = new Transaction(
                                trans[ORDER_ID].ToString(),
                                trans[TOTAL_COST].ForceConvertTo<double>(),
                                trans[STORE_NAME].ToString(),
                                trans[TOTAL_TAX].ForceConvertTo<double>(),
                                trans[SHIPPING_COST].ForceConvertTo<double>()
                            );
                    }
                    catch
                    {
                        Debug.WriteLine("Failed to load transaction for event: " + newEvent.EventID);
                        newEvent.Transaction = null;
                    }
                }
                else if (newEvent.Category == "__##GOOGLEITEM##__")
                {
                    //Load item
                    try
                    {
                        DatabaseRecordBase item = database["item_events"][EVENT_ID, newEvent.EventID.ToString()][0];

                        newEvent.Item = new Item(
                                item[ORDER_ID].ToString(),
                                item[ITEM_SKU].ToString(),
                                item[ITEM_PRICE].ForceConvertTo<double>(),
                                item[ITEM_COUNT].ForceConvertTo<long>(),
                                item[ITEM_NAME].ToString(),
                                item[ITEM_CATEGORY].ToString()
                            );
                    }
                    catch
                    {
                        Debug.WriteLine("Failed to load item for event: " + newEvent.EventID);
                        newEvent.Item = null;
                    }
                }
                else
                {
                    //Load custom variables
                    newEvent.CustomVariables = (useCustomVariables ? getCustomVariables(newEvent.EventID) : new CustomVariableBuffer());
                }
            }

            return events.ToArray();
        }


        public Hit[] GetHits()
        {
            return GetHits(MAX_HITS);
        }

        public Hit[] GetHits(int maxResults)
        {
            List<Hit> hits = new List<Hit>();
            int count = 0;

            try
            {
                RecordCollection records = database["hits"].Records;

                foreach (DatabaseRecordBase record in records)
                {
                    if (count > maxResults)
                        break;

                    hits.Add(new Hit(record[HIT_STRING].ToString(), record[HIT_ID].ForceConvertTo<long>()));
                }
            }
            catch
            {
                return new Hit[] { };
            }

            return hits.ToArray();
        }

        public void RemoveHit(long hitID)
        {
            RecordCollection collection = database["hits"][HIT_ID, hitID.ToString()];
            if(collection == null || collection.Count == 0)
                Debug.WriteLine("Failed to find hit in database: " + hitID);
            else if(!database["hits"].RemoveRecord(collection[0]))
                Debug.WriteLine("Failed to remove hit from database: " + hitID);
        }

        public int HitCount
        {
            get { return database["hits"].Count; }
        }







        public bool SetReferrer(string url)
        {
            string newUrl = formatReferrer(url);
            if (newUrl == null)
                return false;

            Referrer newRef = new Referrer(url, newUrl, Referrer != null ? Referrer.Timestamp : 0, Referrer != null ? Referrer.Visit : 0, 
                Referrer.Index + (Referrer != null && Referrer.Timestamp > 0 ? 1 : 0));

            if (storeReferrer(newRef))
            {
                StartNewVisit();
                return true;
            }
            return false;
        }

        public bool SetReferrer(Referrer referrer)
        {
            if (storeReferrer(referrer))
            {
                StartNewVisit();
                return true;
            }
            return false;
        }

        public void ClearReferrer()
        {
            Referrer = null;
            database["referrer"].Clear();
        }

        public void LoadExistingSession()
        {
            if (Tracker.CurrentInstance.Debug)
                Debug.WriteLine("Attempting to load existing session");
            try
            {
                RecordCollection records = database["session"].Records;
                if (records == null || records.Count == 0)
                {
                    sessionStarted = false;
                    UseStoredVisitorVariables = false;
                    StoreID = new Random().Next() & 0x7FFFFFFF;

                    SQLiteDatabaseField[] fields = new SQLiteDatabaseField[] {
                    new SQLiteDatabaseField(database["session"].Columns[TIMESTAMP_FIRST], 0),
                    new SQLiteDatabaseField(database["session"].Columns[TIMESTAMP_PREVIOUS], 0),
                    new SQLiteDatabaseField(database["session"].Columns[TIMESTAMP_CURRENT], 0),
                    new SQLiteDatabaseField(database["session"].Columns[VISITS], 0),
                    new SQLiteDatabaseField(database["session"].Columns[STORE_ID], StoreID)
                };
                    database["session"].AddRecord(new SQLiteDatabaseRecord(fields));
                }
                else
                {
                    TimestampFirst = records[0][TIMESTAMP_FIRST].ConvertTo<int>();
                    TimestampPrevious = records[0][TIMESTAMP_PREVIOUS].ConvertTo<int>();
                    TimestampCurrent = records[0][TIMESTAMP_CURRENT].ConvertTo<int>();
                    VisitsCount = records[0][VISITS].ConvertTo<int>();
                    StoreID = records[0][STORE_ID].ConvertTo<int>();
                    Referrer = readCurrentReferrer();
                    sessionStarted = (TimestampFirst != 0 && (Referrer == null || Referrer.Timestamp != 0));
                }
            }
            catch
            {
                Debug.WriteLine("Failed to load existing session");
            }
        }

        public void StartNewVisit()
        {
            sessionStarted = false;
            UseStoredVisitorVariables = true;
        }

        public string GetCustomVar(int varID)
        {
            if (VisitorVariableCache[varID] == null || VisitorVariableCache[varID].Value == null)
                return null;
            return VisitorVariableCache[varID].Value;
        }


        #endregion

        #region Private Functions

        #region Create Table Functions


        private void CreateEventsTable()
        {
            List<DatabaseColumnBase> columns = new List<DatabaseColumnBase>()
            {
                new SQLiteDatabaseColumn(EVENT_ID, "INTEGER", ColumnFlags.PrimaryKey | ColumnFlags.NotNull | ColumnFlags.AutoIncrement),
                new SQLiteDatabaseColumn(USER_ID, "INTEGER", ColumnFlags.NotNull),
                new SQLiteDatabaseColumn(ACCOUNT_ID, "CHAR(256)", ColumnFlags.NotNull),
                new SQLiteDatabaseColumn(RANDOM_VAL, "INTEGER", ColumnFlags.NotNull),
                new SQLiteDatabaseColumn(TIMESTAMP_FIRST, "INTEGER", ColumnFlags.NotNull),
                new SQLiteDatabaseColumn(TIMESTAMP_PREVIOUS, "INTEGER", ColumnFlags.NotNull),
                new SQLiteDatabaseColumn(TIMESTAMP_CURRENT, "INTEGER", ColumnFlags.NotNull),
                new SQLiteDatabaseColumn(VISITS, "INTEGER", ColumnFlags.NotNull),
                new SQLiteDatabaseColumn(CATEGORY, "CHAR(256)", ColumnFlags.NotNull),
                new SQLiteDatabaseColumn(ACTION, "CHAR(256)", ColumnFlags.NotNull),
                new SQLiteDatabaseColumn(LABEL, "CHAR(256)", ColumnFlags.None),
                new SQLiteDatabaseColumn(VALUE, "INTEGER", ColumnFlags.None),
                new SQLiteDatabaseColumn(SCREEN_WIDTH, "INTEGER", ColumnFlags.None),
                new SQLiteDatabaseColumn(SCREEN_HEIGHT, "INTEGER", ColumnFlags.None)
            };

            database.CreateTable("events", Databases.CreationParameters.Skip, columns);
        }

        private void CreateSessionTable()
        {
            List<DatabaseColumnBase> columns = new List<DatabaseColumnBase>()
            {
                new SQLiteDatabaseColumn(TIMESTAMP_FIRST, "INTEGER", ColumnFlags.PrimaryKey | ColumnFlags.NotNull),
                new SQLiteDatabaseColumn(TIMESTAMP_PREVIOUS, "INTEGER", ColumnFlags.NotNull),
                new SQLiteDatabaseColumn(TIMESTAMP_CURRENT, "INTEGER", ColumnFlags.NotNull),
                new SQLiteDatabaseColumn(VISITS, "INTEGER", ColumnFlags.NotNull),
                new SQLiteDatabaseColumn(STORE_ID, "INTEGER", ColumnFlags.NotNull)
            };

            database.CreateTable("session", Databases.CreationParameters.Skip, columns);
        }

        private void CreateInstallReferrerTable()
        {
            List<DatabaseColumnBase> columns = new List<DatabaseColumnBase>()
            {
                new SQLiteDatabaseColumn(REFERRER, "TEXT", ColumnFlags.PrimaryKey | ColumnFlags.NotNull)
                
            };

            database.CreateTable("install_referrer", Databases.CreationParameters.Skip, columns);
        }

        private void CreateReferrerTable()
        {
            List<DatabaseColumnBase> columns = new List<DatabaseColumnBase>()
            {
                new SQLiteDatabaseColumn(REFERRER, "TEXT", ColumnFlags.PrimaryKey | ColumnFlags.NotNull),
                new SQLiteDatabaseColumn(REFERRER_URL,"TEXT", ColumnFlags.NotNull),
                new SQLiteDatabaseColumn(TIMESTAMP_REFERRER, "INTEGER", ColumnFlags.NotNull),
                new SQLiteDatabaseColumn(REFERRER_VISIT, "INTEGER", ColumnFlags.NotNull),
                new SQLiteDatabaseColumn(REFERRER_INDEX, "INTEGER", ColumnFlags.NotNull)
            };

            database.CreateTable("referrer", Databases.CreationParameters.Skip, columns);
        }

        private void CreateCustomVariablesTable()
        {
            List<DatabaseColumnBase> columns = new List<DatabaseColumnBase>()
            {
                new SQLiteDatabaseColumn(CUSTOMVAR_ID, "INTEGER", ColumnFlags.PrimaryKey | ColumnFlags.NotNull | ColumnFlags.AutoIncrement),
                new SQLiteDatabaseColumn(EVENT_ID, "INTEGER", ColumnFlags.NotNull),
                new SQLiteDatabaseColumn(CUSTOMVAR_INDEX, "INTEGER", ColumnFlags.NotNull),
                new SQLiteDatabaseColumn(CUSTOMVAR_NAME, CUSTOMVAR_COLUMN_TYPE, ColumnFlags.NotNull),
                new SQLiteDatabaseColumn(CUSTOMVAR_VALUE, CUSTOMVAR_COLUMN_TYPE, ColumnFlags.NotNull),
                new SQLiteDatabaseColumn(CUSTOMVAR_SCOPE, "INTEGER", ColumnFlags.NotNull)
            };

            database.CreateTable("custom_variables", Databases.CreationParameters.Skip, columns);
        }

        private void CreateCustomVariableCacheTable()
        {
            List<DatabaseColumnBase> columns = new List<DatabaseColumnBase>()
            {
                new SQLiteDatabaseColumn(CUSTOMVAR_ID, "INTEGER", ColumnFlags.PrimaryKey | ColumnFlags.NotNull | ColumnFlags.AutoIncrement),
                new SQLiteDatabaseColumn(EVENT_ID, "INTEGER", ColumnFlags.NotNull),
                new SQLiteDatabaseColumn(CUSTOMVAR_INDEX, "INTEGER", ColumnFlags.NotNull),
                new SQLiteDatabaseColumn(CUSTOMVAR_NAME, CUSTOMVAR_COLUMN_TYPE, ColumnFlags.NotNull),
                new SQLiteDatabaseColumn(CUSTOMVAR_VALUE, CUSTOMVAR_COLUMN_TYPE, ColumnFlags.NotNull),
                new SQLiteDatabaseColumn(CUSTOMVAR_SCOPE, "INTEGER", ColumnFlags.NotNull)
            };

            database.CreateTable("custom_var_cache", Databases.CreationParameters.Skip, columns);
        }

        private void CreateTransactionEventsTable()
        {
            List<DatabaseColumnBase> columns = new List<DatabaseColumnBase>()
            {
                new SQLiteDatabaseColumn(TRANSACTION_ID, "INTEGER", ColumnFlags.PrimaryKey | ColumnFlags.NotNull | ColumnFlags.AutoIncrement),
                new SQLiteDatabaseColumn(EVENT_ID, "INTEGER", ColumnFlags.NotNull),
                new SQLiteDatabaseColumn(ORDER_ID, "TEXT", ColumnFlags.NotNull),
                new SQLiteDatabaseColumn(STORE_NAME, "TEXT", ColumnFlags.None),
                new SQLiteDatabaseColumn(TOTAL_COST, "TEXT", ColumnFlags.NotNull),
                new SQLiteDatabaseColumn(TOTAL_TAX, "TEXT", ColumnFlags.None),
                new SQLiteDatabaseColumn(SHIPPING_COST, "TEXT", ColumnFlags.None)
            };

            database.CreateTable("transaction_events", CreationParameters.Skip, columns);
        }

        private void CreateItemEventsTable()
        {
            List<DatabaseColumnBase> columns = new List<DatabaseColumnBase>()
            {
                new SQLiteDatabaseColumn(ITEM_ID, "INTEGER", ColumnFlags.PrimaryKey | ColumnFlags.NotNull | ColumnFlags.AutoIncrement),
                new SQLiteDatabaseColumn(EVENT_ID, "INTEGER", ColumnFlags.NotNull),
                new SQLiteDatabaseColumn(ORDER_ID, "TEXT", ColumnFlags.NotNull),
                new SQLiteDatabaseColumn(ITEM_SKU, "TEXT", ColumnFlags.NotNull),
                new SQLiteDatabaseColumn(ITEM_NAME, "TEXT", ColumnFlags.None),
                new SQLiteDatabaseColumn(ITEM_CATEGORY, "TEXT", ColumnFlags.None),
                new SQLiteDatabaseColumn(ITEM_PRICE, "TEXT", ColumnFlags.NotNull),
                new SQLiteDatabaseColumn(ITEM_COUNT, "TEXT", ColumnFlags.NotNull)
            };

            database.CreateTable("item_events", CreationParameters.Skip, columns);
        }

        private void CreateHitsTable()
        {
            List<DatabaseColumnBase> columns = new List<DatabaseColumnBase>()
            {
                new SQLiteDatabaseColumn(HIT_ID, "INTEGER", ColumnFlags.PrimaryKey | ColumnFlags.NotNull | ColumnFlags.AutoIncrement),
                new SQLiteDatabaseColumn(HIT_STRING, "TEXT", ColumnFlags.NotNull),
                new SQLiteDatabaseColumn(HIT_TIMESTAMP, "INTEGER", ColumnFlags.NotNull)                
            };

            database.CreateTable("hits", CreationParameters.Skip, columns);
        }

        #endregion

        private CustomVariableBuffer getCustomVariables(long eventID)
        {
            RecordCollection records = database["custom_variables"][EVENT_ID, eventID.ToString()];

            CustomVariableBuffer buffer = new CustomVariableBuffer();

            foreach (DatabaseRecordBase record in records)
            {
                CustomVariable variable = new CustomVariable(
                    record[CUSTOMVAR_INDEX].ConvertTo<int>(),
                    record[CUSTOMVAR_NAME].ToString(),
                    record[CUSTOMVAR_VALUE].ToString(),
                    (CustomVariable.Scopes)record[CUSTOMVAR_SCOPE].ConvertTo<int>());

                buffer[variable.Index] = variable;
            }

            return buffer;
        }

        void storeUpdatedSession()
        {
            database["session"].Clear();

            if (TimestampFirst == 0)
            {
                if (Tracker.CurrentInstance.Debug)
                    Debug.WriteLine("Creating New Session");
                int time = (int)(DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds / 1000);
                TimestampFirst = time;
                TimestampPrevious = time;
                TimestampCurrent = time;
                VisitsCount = 1;
            }
            else
            {
                TimestampPrevious = TimestampCurrent;
                TimestampCurrent = (int)(DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds / 1000);
                while (TimestampCurrent <= TimestampPrevious)
                    TimestampCurrent++;
                VisitsCount++;
            }

            SQLiteDatabaseField[] fields = new SQLiteDatabaseField[] {
                new SQLiteDatabaseField(database["session"].Columns[TIMESTAMP_FIRST], TimestampFirst),
                new SQLiteDatabaseField(database["session"].Columns[TIMESTAMP_PREVIOUS], TimestampPrevious),
                new SQLiteDatabaseField(database["session"].Columns[TIMESTAMP_CURRENT], TimestampCurrent),
                new SQLiteDatabaseField(database["session"].Columns[VISITS], VisitsCount),
                new SQLiteDatabaseField(database["session"].Columns[STORE_ID], StoreID)
            };

            if (Tracker.CurrentInstance.Debug)            
                Debug.WriteLine("Storing Updated Session: TS_FIRST:" + TimestampFirst + " TS_PREV:" + TimestampPrevious + " TS_NOW:" + TimestampCurrent
                    + " VCount:" + VisitsCount + " STORE_ID:" + StoreID);
            

            database["session"].AddRecord(new SQLiteDatabaseRecord(fields));
            sessionStarted = true;
        }

        void storeCustomVariables(Event e)
        {
            if (e.Category == Event.ItemCategory || e.Category == Event.TransactionCategory)
                return;

            try
            {
                if (UseStoredVisitorVariables)
                {
                    if (e.CustomVariables == null)
                    {
                        e.CustomVariables = new CustomVariableBuffer();
                    }

                    for (int i = 1; i <= e.CustomVariables.Variables.Length; i++)
                    {
                        if (e.CustomVariables[i] == null && VisitorVariableCache[i] != null)
                            e.CustomVariables[i] = VisitorVariableCache[i];
                    }
                    UseStoredVisitorVariables = false;
                }

                if (e.CustomVariables != null)
                {
                    for (int i = 1; i < e.CustomVariables.Variables.Length; i++)
                    {
                        if (e.CustomVariables.IndexAvailable(i))
                            continue;

                        SQLiteDatabaseField[] fields = new SQLiteDatabaseField[]
                        {
                            new SQLiteDatabaseField(database["custom_var_cache"].Columns[EVENT_ID],0),
                            new SQLiteDatabaseField(database["custom_var_cache"].Columns[CUSTOMVAR_INDEX],e.CustomVariables[i].Index),
                            new SQLiteDatabaseField(database["custom_var_cache"].Columns[CUSTOMVAR_NAME],e.CustomVariables[i].Name),
                            new SQLiteDatabaseField(database["custom_var_cache"].Columns[CUSTOMVAR_SCOPE],(int)e.CustomVariables[i].Scope),
                            new SQLiteDatabaseField(database["custom_var_cache"].Columns[CUSTOMVAR_VALUE],e.CustomVariables[i].Value),
                        };

                        RecordCollection records = database["custom_var_cache"][CUSTOMVAR_INDEX, e.CustomVariables[i].Index.ToString()];
                        DatabaseRecordBase oldRecord = records != null ? records[0] : null;
                        if (oldRecord == null)
                            database["custom_var_cache"].AddRecord(new SQLiteDatabaseRecord(fields));
                        else
                            database["custom_var_cache"].UpdateRecord(oldRecord, new SQLiteDatabaseRecord(fields));

                        if (e.CustomVariables[i].Scope == CustomVariable.Scopes.Visitor)
                            VisitorVariableCache[i] = e.CustomVariables[i];
                        else
                            VisitorVariableCache.ClearVariableAt(i);
                    }
                }
            }
            catch
            {

            }
        }

        void storeEvent(Event e, Referrer referrer, bool storeHitTime)
        {
            string hitString = RequestBuilder.HitRequestPath(e,referrer);
            
            SQLiteDatabaseField[] fields = new SQLiteDatabaseField[] {
                new SQLiteDatabaseField(database["hits"].Columns[HIT_STRING],hitString),
                new SQLiteDatabaseField(database["hits"].Columns[HIT_TIMESTAMP], (storeHitTime ? (int)Math.Round(DateTime.Now.Subtract(new DateTime(1970,1,1)).TotalMilliseconds) : 0))
            };

            database["hits"].AddRecord(new SQLiteDatabaseRecord(fields));
        }

        static string formatReferrer(string referrerUrl)
        {
            if (referrerUrl == null)
                return null;

            if (!referrerUrl.Contains("="))            
                if (referrerUrl.Contains("%3D"))
                    referrerUrl = Utils.Decode(referrerUrl, false);
                else
                    return null;

            Dictionary<string, string> p = Utils.getRequestParams(referrerUrl);

            if (!p.ContainsKey("gclid") && 
                (!p.ContainsKey("utm_campaign") || !p.ContainsKey("utm_medium") || !p.ContainsKey("utm_source")))
            {
                Debug.WriteLine("Badly formatted referrer missing campaign, medium and source, or click ID");
                return null;
            }

            string[][] newReferrer = new string[][] {
                new string[] { "utmcid", p["utm_id"] },
                new string[] { "utmcsr", p["utm_source"] },
                new string[] { "utmgclid", p["gclid"] },
                new string[] { "utmccn", p["utm_campaign"] },
                new string[] { "utmcmd", p["utm_medium"] },
                new string[] { "utmctr", p["utm_term"] },
                new string[] { "utmcct", p["utm_content"] }
            };

            string str = "";
            for (int i = 0; i < newReferrer.Length; i++)
            {
                if (newReferrer[i][1] == null)
                    continue; //Don't add values which we don't have

                if (i != 0)
                    str += "|";
                str += newReferrer[i][0] + "=" + newReferrer[i][1].Replace("+", "%20").Replace(" ", "%20");
            }

            return str;
        }

        Referrer readCurrentReferrer()
        {
            if (database["referrer"].Count == 0)
                return null;


            DatabaseRecordBase record = database["referrer"].Records[0];

            if (record == null)
            {
                Referrer = null;
                return null;
            }

            Referrer newRef = new Referrer(
                record[REFERRER_URL].ToString(),
                record[REFERRER].ToString(),
                record[TIMESTAMP_REFERRER].ForceConvertTo<long>(),
                record[REFERRER_VISIT].ConvertTo<int>(),
                record[REFERRER_INDEX].ConvertTo<int>());
            Referrer = newRef;
            return newRef;
        }

        bool storeReferrer(Referrer r)
        {
            database["referrer"].Clear();

            SQLiteDatabaseField[] fields = new SQLiteDatabaseField[] {
                new SQLiteDatabaseField(database["referrer"].Columns[REFERRER_URL], r.URL),
                new SQLiteDatabaseField(database["referrer"].Columns[REFERRER_COLUMN],r.CookieString),
                new SQLiteDatabaseField(database["referrer"].Columns[TIMESTAMP_REFERRER],r.Timestamp),
                new SQLiteDatabaseField(database["referrer"].Columns[REFERRER_VISIT],r.Visit),
                new SQLiteDatabaseField(database["referrer"].Columns[REFERRER_INDEX],r.Index)
            };

            return database["referrer"].AddRecord(new SQLiteDatabaseRecord(fields));
        }

        public Referrer GetAndUpdateReferrer()
        {
            Referrer = readCurrentReferrer();

            if (Referrer == null)
                return null;

            if (Referrer.Timestamp != 0)
                return Referrer;

            Referrer newRef = new Referrer(Referrer.URL, Referrer.CookieString, TimestampCurrent, VisitsCount, Referrer.Index);
            if (storeReferrer(newRef))
                return newRef;
            return null;
        }

        #endregion
    }
}
