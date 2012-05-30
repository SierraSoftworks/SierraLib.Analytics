using System;
using System.Collections.Generic;
using System.Text;

namespace SierraLib.Analytics.GoogleAnalytics
{
    class Event
    {
        public const string PageViewEventCategory = "__##GOOGLEPAGEVIEW##__";
        public const string InstallEventCategory = "__##GOOGLEINSTALL##__";
        public const string TransactionCategory = "__##GOOGLETRANSACTION##__";
        public const string ItemCategory = "__##GOOGLEITEM##__";

        public long EventID
        { get; private set; }

        public string AccountID
        { get; private set; }

        public int RandomValue
        { get; set; }

        public int AdHitID
        { get; set; }

        public int TimestampFirst
        { get; set; }

        public int TimestampPrevious
        { get; set; }

        public int TimestampCurrent
        { get; set; }

        public int VisitCount
        { get; set; }

        public int UserID
        { get; set; }

        public bool AnonymizeIP
        { get; set; }

        public bool UseServerTime
        { get; set; }

        public string Category
        { get; private set; }

        public string Action
        { get; private set; }

        public string Label
        { get; private set; }

        public int Value
        { get; private set; }

        public int ScreenWidth
        { get; private set; }

        public int ScreenHeight
        { get; private set; }

        public string CharacterSet
        {
            get
            {
                return "UTF-8";
            }
        }

        public string IPAddress
        {
            get
            {
                //Only send IP with Mobile Account codes
                if (!AccountID.StartsWith("MO"))
                    return null;

                foreach (System.Net.IPAddress ip in System.Net.Dns.GetHostAddresses(System.Net.Dns.GetHostName()))
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        return ip.ToString();
                return null;
            }
        }

        private Transaction _transaction;

        public Transaction Transaction
        {
            get { return _transaction; }
            set 
            {
                if (Category != "__##GOOGLETRANSACTION##__")
                    throw new InvalidOperationException("Attempted to add a transaction to an event of type " + Category);
                _transaction = value;
            }
        }


        private Item _item;

        public Item Item
        {
            get { return _item; }
            set
            {
                if (Category != "__##GOOGLEITEM##__")
                    throw new InvalidOperationException("Attempted to add an item to an event of type " + Category);

                _item = value;
            }
        }
        
        public CustomVariableBuffer CustomVariables
        { get; set; }

        public Event(Event baseEvent, String accountCode)
            : this(baseEvent.EventID, accountCode, baseEvent.RandomValue,
            baseEvent.TimestampFirst, baseEvent.TimestampPrevious, baseEvent.TimestampCurrent,
            baseEvent.VisitCount, baseEvent.Category, baseEvent.Action, baseEvent.Label,
            baseEvent.Value, baseEvent.ScreenHeight, baseEvent.ScreenWidth)
        {
            AdHitID = baseEvent.AdHitID;
            UserID = baseEvent.UserID;
            AnonymizeIP = baseEvent.AnonymizeIP;
            UseServerTime = baseEvent.UseServerTime;
            
            CustomVariables = baseEvent.CustomVariables;
            this.Transaction = baseEvent.Transaction;
            this.Item = baseEvent.Item;
             
        }

        public Event(long eventID, string accountCode,
            int randomValue, int timestampFirst, int timestampPrevious, int timestampCurrent,
            int visitCount, string category, string action, string label, int value,
            int screenHeight, int screenWidth)
        {
            EventID = eventID;
            AccountID = accountCode;
            RandomValue = randomValue;
            TimestampFirst = timestampFirst;
            TimestampPrevious = timestampPrevious;
            TimestampCurrent = timestampCurrent;
            VisitCount = visitCount;
            Category = category;
            Action = action;
            Label = label;
            Value = value;
            ScreenHeight = screenHeight;
            ScreenWidth = screenWidth;
            UserID = -1;
            UseServerTime = false;
        }

        public Event(string accountCode, string category, string action, string label, int value,
            int screenHeight, int screenWidth)
            : this(-1, accountCode, -1, -1, -1, -1, -1, category, action, label, value, screenHeight, screenWidth)
        {

        }


        public bool SessionInitialized
        { get { return TimestampFirst != -1; } }


        public override string ToString()
        {
            return "id:" + EventID + " random:" + RandomValue + " timestampCurrent:" + TimestampCurrent +
                " timestampFirst:" + TimestampFirst + " timestampPrevious:" + TimestampPrevious +
                " visits:" + VisitCount + " category:" + Category + " action:" + Action +
                " label:" + Label + " value:" + Value + " width:" + ScreenWidth + " height:" + ScreenHeight;
        }
    }
}
