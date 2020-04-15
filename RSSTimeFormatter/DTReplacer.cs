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
			if (configs.Length == 1) {
				ConfigNode config = configs[0].config;
				var formats = config.GetValues(name);
				if (formats.Length == 1) {
					return formats[0];
				} else {
					Debug.LogError($"`{node}` configuration has multiple `{name}` entries, falling back to default");
				}
			} else {
				Debug.LogError($"Multiple `{node}` configurations, falling back to default");
			}
			return null;
		}

		public void Start()
		{
			Debug.Log("Replacing DateTime formatter");
			KSPUtil.dateTimeFormatter = new RealDateTimeFormatter(GetDateFormat(), GetEpoch());
		}

		private string GetDateFormat()
		{
			// Since Unity overrides the CurrentCulture, we cannot rely on it to
			// format dates in a way that the user will understand, see
			// https://github.com/KSP-RO/RSSTimeFormatter/issues/2.
			// This default is an international standard, namely ISO 8601 extended
			// format.  It is chosen (and was designed) to avoid ambiguities on the
			// order of month and day that are inevitable with formats using
			// slashes.
			string format = GetUniqueStringFromUniqueNode("dateFormat", "RSSTimeFormatter");
			if (format != null) {
				try {
					string.Format("{0:" + format + "}", new DateTime(1957, 10, 04));
				} catch (FormatException) {
					Debug.LogError($"Invalid date format {format} using default");
				}
				return format;
			}
			return "yyyy-MM-dd";
		}

		private DateTime GetEpoch()
		{
			DateTime result;
			string customEpoch = GetUniqueStringFromUniqueNode("epoch", "RSSTimeFormatter");
			if (customEpoch != null) {
				if (DateTime.TryParse(customEpoch, out result)) {
					return result;                                        
				}
			}
			return new DateTime(1951, 01, 01);
		}
	}
}

