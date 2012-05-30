using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace SierraLib.Analytics.GoogleAnalytics
{
    static class Utils
    {
        static Regex urlParser = new Regex("(?<url>(?<path>[^ ?]*)(?:\\?(?<params>[^ ]*))?)", RegexOptions.Compiled);
        static Regex encodingParser = new Regex("%([A-Za-z0-9]{2})", RegexOptions.Compiled);

        public static string Decode(string httptext, bool convertPlusToSpace = true)
        {
            MatchCollection matches = encodingParser.Matches(httptext);
            string temp = httptext;

            if (convertPlusToSpace)
                temp = temp.Replace("+", " ");

            while (true)
            {
                Match mtch = encodingParser.Match(temp);
                if (!mtch.Success)
                    break;
                temp = temp.Replace(mtch.Value, "" + (char)int.Parse(mtch.Groups[1].Value, System.Globalization.NumberStyles.HexNumber));
            }

            return temp;

        }

        public static string Encode(string httpText, bool convertSpaceToPlus = true)
        {
            return httpText
                .Replace("%", "%25")
                .Replace("+", "%2B")
                .Replace(" ", convertSpaceToPlus ? "+" : "%20")
                .Replace("<", "%3C")
                .Replace(">", "%3E")
                .Replace("#", "%23")
                .Replace("{", "%7B")
                .Replace("}", "%7D")
                .Replace("|", "%7C")
                .Replace("\\", "%5C")
                .Replace("^", "%5E")
                .Replace("~", "%7E")
                .Replace("[", "%5B")
                .Replace("]", "%5D")
                .Replace("`", "%60")
                .Replace(";", "%3B")
                .Replace("/", "%2F") //We don't want to escape these as they are part of the path...
                .Replace("?", "%3F")
                .Replace(":", "%3A")
                .Replace("@", "%40")
                .Replace("=", "%3D")
                .Replace("&", "%26")
                .Replace("$", "%24")
                .Replace("\n", "%0A")
                .Replace("\"", "%22");
        }

        public static string getRequestPath(string request)
        {
            return urlParser.Match(request).Groups["path"].Value;
        }

        public static Dictionary<string, string> getRequestParams(string request)
        {

            string paramsString = urlParser.Match(request).Groups["params"].Value;

            Dictionary<string, string> result = new Dictionary<string, string>();

            string[] pairs = paramsString.Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string pair in pairs)
            {
                try
                {
                    string[] vals = pair.Split(new char[] { '=' }, 2);
                    if (vals.Length == 2)
                        result.Add(Decode(vals[0], false), Decode(vals[1], false));
                    else
                        result.Add(Decode(vals[0], false), "");
                }
                catch { }
            }

            return result;
        }

    }
}
