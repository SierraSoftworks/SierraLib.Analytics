using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace SierraLib.Analytics.GoogleAnalytics
{
    static class RequestBuilder
    {
        const string GOOGLE_ANALYTICS_GIF_PATH = "/__utm.gif";

        const string FAKE_DOMAIN_HASH = "1";

        const int X10_PROJECT_NAMES = 8;
        const int X10_PROJECT_VALUES = 9;
        const int X10_PROJECT_SCOPES = 11;

        private static string Encode(string text)
        {
            return Utils.Encode(text).Replace("+", "%20");
        }

        private static string constructPageviewRequestPath(Event e, Referrer referrer)
        {
            string str1 = "";
            if (e.Action != null)
                str1 = e.Action;
            if (!str1.StartsWith("/"))
                str1 = "/" + str1;
            str1 = Encode(str1);

            string str2 = getCustomVariableParams(e);
            string locale = Tracker.CurrentInstance.CurrentLanguage;

            StringBuilder str = new StringBuilder();
            str.Append(GOOGLE_ANALYTICS_GIF_PATH);
            str.Append("?utmwv=" + Tracker.APIVersion);
            str.Append("&utmn=").Append(e.RandomValue);

            if (str2 != null && str2.Length > 0)
                str.Append("&utme=").Append(str2);

            str.Append("&umtcs=").Append(e.CharacterSet);
            str.Append("&utmsr=").Append(e.ScreenWidth).Append("x").Append(e.ScreenHeight);
#if !WINDOWS_PHONE
            str.Append("&utmsc=").Append(new Microsoft.VisualBasic.Devices.Computer().Screen.BitsPerPixel + "-bit");
#endif
            str.Append("&utmul=").Append(locale);
            str.Append("&utmp=").Append(str1);
            
            if(Tracker.CurrentInstance.Domain != null)
                str.Append("&utmhn=").Append(Tracker.CurrentInstance.Domain);


            if(e.Label != null)
                str.Append("&utmdt=").Append(Encode(e.Label));

            if (referrer != null)
                str.Append("&utmr=").Append(referrer.URL);

            if (e.AdHitID != 0)
                str.Append("&utmhid=").Append(e.AdHitID);

            if (e.IPAddress != null)
                str.Append("&utmip=").Append(e.IPAddress);

            str.Append("&utmac=").Append(e.AccountID);
            str.Append("&utmcc=").Append(getEscapedCookieString(e, referrer));
            return str.ToString();

        }

        private static string constructEventRequestPath(Event e, Referrer r)
        {
            string locale = Thread.CurrentThread.CurrentCulture.Name;

            StringBuilder str1 = new StringBuilder();
            StringBuilder str2 = new StringBuilder();

            str2.Append(String.Format("5({0}*{1}", Encode(e.Category), Encode(e.Action)));

            if (e.Label != null)
                str2.Append("*").Append(Encode(e.Label));

            str2.Append(")");
            if (e.Value != 0)
                str2.Append(String.Format("({0})", e.Value.ToString()));

            str1.Append(GOOGLE_ANALYTICS_GIF_PATH);
            str1.Append("?utmwv=").Append(Tracker.APIVersion);
            str1.Append("&utmn=").Append(e.RandomValue);
            str1.Append("&utmt=event");
            str1.Append("&utme=").Append(str2.ToString());
            str1.Append("&utmcs=").Append(e.CharacterSet);
            str1.Append(String.Format("&utmsr={0}x{1}", e.ScreenWidth, e.ScreenHeight));
            str1.Append("&utmul=").Append(locale);
            str1.Append("&utmac=").Append(e.AccountID);
            str1.Append("&utmcc=").Append(getEscapedCookieString(e, r));

            if (e.IPAddress != null)
                str1.Append("&utmip=").Append(e.IPAddress);

            if (e.AdHitID != 0)
                str1.Append("&utmhid=").Append(e.AdHitID);
            return str1.ToString();        
        }

        private static void appendStringValue(StringBuilder str, string key, string value)
        {
            str.Append(key).Append("=");
            if (value != null && value.Trim().Length > 0)
                str.Append(Encode(value));            
        }

        private static void appendCurrencyValue(StringBuilder str, string key, double value)
        {
            str.Append(key).Append("=");
            double d = Math.Floor(value * 1000000 + 0.5) / 1000000;
            if (d != 0)
                str.Append(d);
        }

        private static string constructTransactionRequestPath(Event e, Referrer r)
        {
            StringBuilder str = new StringBuilder();
            //.Replace("+","%20")

            str.Append(GOOGLE_ANALYTICS_GIF_PATH);
            str.Append("?utmwv=").Append(Tracker.APIVersion);
            str.Append("&utmn=").Append(e.RandomValue);
            str.Append("&utmt=tran");

            if (e.Transaction != null)
            {
                Transaction t = e.Transaction;
                appendStringValue(str, "&utmtid", t.OrderID);
                appendStringValue(str, "&utmtst", t.StoreName);
                appendCurrencyValue(str, "&utmtto", t.TotalCost);
                appendCurrencyValue(str, "&utmttx", t.TotalTax);
                appendCurrencyValue(str, "&utmtsp", t.ShippingCost);
                appendStringValue(str, "&utmtci", "");
                appendStringValue(str, "&utmtrg", "");
                appendStringValue(str, "&utmtco", "");
            }
            
            if (e.IPAddress != null)
                str.Append("&utmip=").Append(e.IPAddress);

            str.Append("&utmac=").Append(e.AccountID);
            str.Append("&utmcc=").Append(getEscapedCookieString(e, r));
            return str.ToString();
        }

        private static string constructItemRequestPath(Event e, Referrer r)
        {
            StringBuilder str = new StringBuilder();
            //.Replace("+","%20")

            str.Append(GOOGLE_ANALYTICS_GIF_PATH);
            str.Append("?utmwv=").Append(Tracker.APIVersion);
            str.Append("&utmn=").Append(e.RandomValue);
            str.Append("&utmt=item");

            if (e.Item != null)
            {
                Item i = e.Item;
                appendStringValue(str, "&utmtid", i.OrderID);
                appendStringValue(str, "&utmipc", i.ItemSKU);
                appendStringValue(str, "&utmipn", i.ItemName);
                appendStringValue(str, "&utmiva", i.ItemCategory);
                appendCurrencyValue(str, "&utmipr", i.ItemPrice);
                str.Append("&utmiqt=");
                if (i.ItemCount != 0)
                    str.Append(i.ItemCount);
            }

            str.Append("&utmac=").Append(e.AccountID);
            str.Append("&utmcc=").Append(getEscapedCookieString(e, r));
            return str.ToString();
        }

        public static string HitRequestPath(Event eventData, Referrer referrer)
        {
            StringBuilder str = new StringBuilder();
            if (eventData.Category == "__##GOOGLEPAGEVIEW##__")
                str.Append(constructPageviewRequestPath(eventData, referrer));
            else if (eventData.Category == "__##GOOGLEITEM##__")
                str.Append(constructItemRequestPath(eventData, referrer));
            else if (eventData.Category == "__##GOOGLETRANSACTION##__")
                str.Append(constructTransactionRequestPath(eventData, referrer));
            else
                str.Append(constructEventRequestPath(eventData, referrer));

            if (eventData.AnonymizeIP)
                str.Append("&aip=1");
            if (!eventData.UseServerTime)
                str.Append(String.Format("&utmht={0}", (long)DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds));

            return str.ToString();
        }



        public static string AddQueTimeParameter(string hitString)
        {
            long time = (long)DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;

            Dictionary<string, string> hitParams = Utils.getRequestParams(hitString);

            if (!hitParams.ContainsKey("utmht") || hitParams["utmht"] == null)
                return hitString;

            try
            {
                long hitTime = Convert.ToInt64(hitParams["utmht"]);
                return hitString + "&utmqt=" + (time - hitTime);
            }
            catch
            {
                return hitString;
            }
        }

        private static string DomainHash
        {
            get
            {
                string PageDomain = Tracker.CurrentInstance.Domain;
                if (PageDomain != null)
                {
                    // converted from the google domain hash code listed here:
                    // http://www.google.com/support/forum/p/Google+Analytics/thread?tid=626b0e277aaedc3c&hl=en
                    int a = 1;
                    int c = 0;
                    int h;
                    char chrCharacter;
                    int intCharacter;

                    a = 0;
                    for (h = PageDomain.Length - 1; h >= 0; h--)
                    {
                        chrCharacter = char.Parse(PageDomain.Substring(h, 1));
                        intCharacter = (int)chrCharacter;
                        a = (a << 6 & 268435455) + intCharacter + (intCharacter << 14);
                        c = a & 266338304;
                        a = c != 0 ? a ^ c >> 21 : a;
                    }

                    return a.ToString();
                }
                return FAKE_DOMAIN_HASH;
            }
        }

        public static string getEscapedCookieString(Event e, Referrer referrer)
        {
            StringBuilder str = new StringBuilder();
            str.Append("__utma=");
            str.Append(DomainHash).Append(".");
            str.Append(e.UserID).Append(".");
            str.Append(e.TimestampFirst).Append(".");
            str.Append(e.TimestampPrevious).Append(".");
            str.Append(e.TimestampCurrent).Append(".");
            str.Append(e.VisitCount).Append(";");

            if (referrer != null)
            {
                str.Append("+__utmz=");
                str.Append(DomainHash).Append(".");
                str.Append(referrer.Timestamp).Append(".");
                str.Append(referrer.Visit).Append(".");
                str.Append(referrer.Index).Append(".");
                str.Append(referrer.CookieString).Append(";");
            }

            return Encode(str.ToString());
        }

        public static string getCustomVariableParams(Event e)
        {
            if (e.CustomVariables == null || !e.CustomVariables.HasVariables)
                return "";
            
            StringBuilder str = new StringBuilder();
            createX10Project(e.CustomVariables, str, X10_PROJECT_NAMES);
            createX10Project(e.CustomVariables, str, X10_PROJECT_VALUES);
            createX10Project(e.CustomVariables, str, X10_PROJECT_SCOPES);
            return str.ToString();
        }

        private static void createX10Project(CustomVariableBuffer buffer, StringBuilder str, int X10ProjectType)
        {
            int i = 1;
            str.Append(X10ProjectType).Append("(");
            for (int i1 = 1; i1 <= buffer.Variables.Length; i1++)
            {
                if (buffer[i1] == null)
                    continue;
                if (i == 0)
                    str.Append("*");
                else i = 0;

                str.Append(buffer[i1].Index).Append("!");
                switch(X10ProjectType)
                {
                    case X10_PROJECT_NAMES:
                        str.Append(x10Escape(Encode(buffer[i1].Name)));
                        break;
                    case X10_PROJECT_VALUES:
                        str.Append(x10Escape(Encode(buffer[i1].Value)));
                        break;
                    case X10_PROJECT_SCOPES:
                        str.Append((int)buffer[i1].Scope);
                        break;
                }
            }
            str.Append(")");
        }

        private static string x10Escape(string value)
        {
            return value.Replace("'", "'0").Replace(")", "'1").Replace("*", "'2").Replace("!", "'3");
        }
    }
}
