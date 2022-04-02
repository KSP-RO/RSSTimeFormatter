using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace RSSTimeFormatter
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class DTReplacer : MonoBehaviour
    {
        public static string GetUniqueStringFromUniqueNode(string name, string node)
        {
            var configs = GameDatabase.Instance.GetConfigs(node);
            if (configs.Length > 1)
            {
                Debug.LogError($"[RSSDT] Multiple `{node}` configurations, falling back to default");
            }
            else if (configs.Length == 1)
            {
                ConfigNode config = configs[0].config;
                string[] formats = config.GetValues(name);
                if (formats.Length > 1)
                {
                    Debug.LogError($"[RSSDT] `{node}` configuration has multiple `{name}` entries, falling back to default");
                }
                else if (formats.Length == 1)
                {
                    // Unescaping similar to that performed in KSP localization
                    // strings (because we need curly braces), though contrary
                    // to KSP we reject unpaired surrogates and instead accept
                    // codepoints outside the BMP with the \U syntax.
                    return new Regex(
                        @"\\(?:u([0-9A-F]{4})|U([0-9A-F]{5}))").Replace(
                        formats[0].Replace("｢", "{").Replace("｣", "}"),
                        match => char.ConvertFromUtf32(
                            int.Parse(
                                match.Value.Substring(2),
                                System.Globalization.NumberStyles.HexNumber)));
                }
            }
            return null;
        }

        public void Start()
        {
            string dateFormat = KSP.Localization.Localizer.GetStringByTag("#RSSTimeFormatter_CLDR_mediumDate");
            string timeFormat = KSP.Localization.Localizer.GetStringByTag("#RSSTimeFormatter_CLDR_mediumTime");
            string dateTimeFormat = KSP.Localization.Localizer.GetStringByTag("#RSSTimeFormatter_CLDR_mediumDateTime");
            string customDateFormat = GetUniqueStringFromUniqueNode("dateFormat", "RSSTimeFormatter");
            string customTimeFormat = GetUniqueStringFromUniqueNode("timeFormat", "RSSTimeFormatter");
            string customDateTimeFormat = GetUniqueStringFromUniqueNode("dateTimeFormat", "RSSTimeFormatter");
            // Validate any custom patterns.
            if (customDateFormat != null)
            {
                try
                {
                    string.Format("{0:" + customDateFormat + "}", new DateTime(1957, 10, 04));
                    dateFormat = customDateFormat;
                }
                catch (FormatException)
                {
                    Debug.LogError("[RSSDT] Invalid date format " + customDateFormat);
                }
            }
            if (customTimeFormat != null)
            {
                try
                {
                    string.Format("{0:" + customTimeFormat + "}", new DateTime(1957, 10, 04));
                    timeFormat = customTimeFormat;
                }
                catch (FormatException)
                {
                    Debug.LogError("[RSSDT] Invalid time format " + customTimeFormat);
                }
            }
            if (customDateTimeFormat != null)
            {
                try
                {
                    string.Format(customDateTimeFormat, "time", "date");
                    dateTimeFormat = customDateTimeFormat;
                }
                catch (FormatException)
                {
                    Debug.LogError("[RSSDT] Invalid date and time combiner format " + customDateTimeFormat);
                }
            }

            DateTime epoch = new DateTime(1951, 01, 01);
            string customEpoch = GetUniqueStringFromUniqueNode("epoch", "RSSTimeFormatter");
            if (customEpoch != null && !DateTime.TryParse(customEpoch, out epoch))
            {
                Debug.LogError("[RSSDT] Invalid epoch " + customEpoch);
            }

            KSPUtil.dateTimeFormatter = new RealDateTimeFormatter(dateFormat, timeFormat, dateTimeFormat, epoch);
        }
    }
}
