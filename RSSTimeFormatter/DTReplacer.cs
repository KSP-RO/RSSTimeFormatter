using System;
using UnityEngine;

namespace RSSTimeFormatter
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class DTReplacer : MonoBehaviour
    {
        public static string GetUniqueStringFromUniqueNode(string name, string node)
        {
            var configs = GameDatabase.Instance.GetConfigs(node);
            if (configs.Length > 1) {
                Debug.LogError(
                    "[RSSDT] Multiple `" + node + "` configurations, falling back to default");
            }
            else if (configs.Length == 1) {
                ConfigNode config = configs[0].config;
                var formats = config.GetValues(name);
                if (formats.Length > 1) {
                    Debug.LogError(
                        "[RSSDT] `" + node + "` configuration has multiple `" + name + "` entries, falling back to default");
                }
                else if (formats.Length == 1) {
                    return formats[0];
                }
            }
            return null;
        }

        public void Start()
        {
            // Since Unity overrides the CurrentCulture, we cannot rely on it to
            // format dates in a way that the user will understand, see
            // https://github.com/KSP-RO/RSSTimeFormatter/issues/2.
            // This default is an international standard, namely ISO 8601 extended
            // format.  It is chosen (and was designed) to avoid ambiguities on the
            // order of month and day that are inevitable with formats using
            // slashes.
            string dateFormat = "yyyy-MM-dd";
            string customDateFormat = GetUniqueStringFromUniqueNode("dateFormat", "RSSTimeFormatter");
            if (customDateFormat != null) {
                // Validate the format string.
                try {
                    string.Format("{0:" + customDateFormat + "}", new DateTime(1957, 10, 04));
                    dateFormat = customDateFormat;
                }
                catch (FormatException) {
                    Debug.LogError("[RSSDT] Invalid date format " + customDateFormat);
                }
            }

            DateTime epoch = new DateTime(1951, 01, 01);
            string customEpoch = GetUniqueStringFromUniqueNode("epoch", "RSSTimeFormatter");
            if (customEpoch != null) {
                if (!DateTime.TryParse(customEpoch, out epoch)) {
                    Debug.LogError("[RSSDT] Invalid epoch " + customEpoch);
                }
            }

            KSPUtil.dateTimeFormatter = new RealDateTimeFormatter(dateFormat, epoch);
        }
    }
}

