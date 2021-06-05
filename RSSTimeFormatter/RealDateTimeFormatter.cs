using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RSSTimeFormatter
{
	public class RealDateTimeFormatter : IDateTimeFormatter
	{
		private string dateFormat;
		private DateTime epoch;

		#region IDateTimeFormatter implementation
		public string PrintTimeLong(double time)
		{
			// short-circuit if invalid time passed
			if (IsInvalidTime(time))
				return InvalidTimeStr(time);
			DateTime epoch = GetEpoch();
			DateTime target = epoch.AddSeconds(time);
			TimeSpan span = target - epoch;
			return string.Format("{0}{1}, {2}{3}, {4}{5}, {6}{7}"
				, span.Days, span.Days == 1 ? "day" : "days"
				, span.Hours, span.Hours == 1 ? "hour" : "hours"
				, span.Minutes, span.Minutes == 1 ? "minute" : "minutes"
				, span.Seconds, span.Seconds == 1 ? "second" : "seconds"
			);
		}
		public string PrintTimeStamp(double time, bool days = false, bool years = false)
		{
			// short-circuit if invalid time passed
			if (IsInvalidTime(time))
				return InvalidTimeStr(time);
			DateTime epoch = GetEpoch();
			DateTime target = epoch.AddSeconds(time);
			TimeSpan span = target - epoch;
			return string.Format("{0}{1:D2}:{2:D2}:{3:D2}"
				, days ? string.Format("Day {0} - ", span.Days) : ""
				, span.Hours
				, span.Minutes
				, span.Seconds
			);
		}
		public string PrintTimeStampCompact(double time, bool days = false, bool years = false)
		{
			// short-circuit if invalid time passed
			if (IsInvalidTime(time))
				return InvalidTimeStr(time);
			DateTime epoch = GetEpoch();
			DateTime target = epoch.AddSeconds(time);
			TimeSpan span = target - epoch;
			int dNum = span.Days;
			int yNum = dNum / 365;
			int subDays = dNum - yNum * 365;
			return string.Format("{0}{1}{2:D2}:{3:D2}:{4:D2}"
				, years ? string.Format("{0}y, ", yNum) : ""
				, days ? string.Format("{0}d, ", ((years && subDays != 0) ? subDays : dNum)) : ""
				, span.Hours
				, span.Minutes
				, span.Seconds
			);
		}
		public string PrintTime(double time, int valuesOfInterest, bool explicitPositive)
		{
			// This is a downright strange and confusing method but as I understand it
			// what it is saying is give it the time in the following format:
			// 1y, 1d, 1h, 1m, 1s
			// But the param 'valuesOfInterest' is used to control how 'deep' it goes
			// IOW a valueofInterest of say 3 would only give you hours, minutes, and seconds
			// Why it isn't more straightforward is beyond me
			// short-circuit if invalid time passed
			if (IsInvalidTime(time))
				return InvalidTimeStr(time);
			bool isNegativeTime = false;
			if (time < 0) {
				time = Math.Abs(time);
				isNegativeTime = true;
			}
			DateTime epoch = GetEpoch();
			DateTime target = epoch.AddSeconds(time);
			TimeSpan span = target - epoch;
			return string.Format("{0}{1}{2}{3}{4}"
				, isNegativeTime ? "- " : (explicitPositive ? "+ " : "")
				, (valuesOfInterest >= 3 && span.Days != 0) ? string.Format("{0}d, ", span.Days) : ""
				, (valuesOfInterest >= 2 && span.Hours != 0) ? string.Format("{0}h, ", span.Hours) : ""
				, (valuesOfInterest >= 1 && span.Minutes != 0) ? string.Format("{0}m, ", span.Minutes) : ""
				, valuesOfInterest >= 0 ? string.Format("{0}s", span.Seconds) : ""
			);
		}

		public string PrintTime(double time, int valuesOfInterest, bool explicitPositive, bool logEnglish)
		{
			return PrintTime(time, valuesOfInterest, explicitPositive);
		}

		public string PrintTimeCompact(double time, bool explicitPositive)
		{
			if (IsInvalidTime(time))
				return InvalidTimeStr(time);
			bool isNegativeTime = false;
			if (time < 0) {
				time = Math.Abs(time);
				isNegativeTime = true;
			}
			DateTime epoch = GetEpoch();
			DateTime target = epoch.AddSeconds(time);
			TimeSpan span = target - epoch;
			return string.Format("{0}{1}{2:D2}:{3:D2}:{4:D2}"
				, isNegativeTime ? "- " : (explicitPositive ? "+ " : "")
				, (span.Days != 0 ? span.Days.ToString() : "")
				, span.Hours
				, span.Minutes
				, span.Seconds
			);
		}
		public string PrintDateDelta(double time, bool includeTime, bool includeSeconds, bool useAbs)
		{
			if (IsInvalidTime(time))
				return InvalidTimeStr(time);
			if (time < 0 && useAbs)
				time = Math.Abs(time);
			if (time == 0d)
				return string.Format("0 {0}", includeTime ? (includeSeconds ? "seconds" : "minutes") : "days");

			DateTime epoch = GetEpoch();
			DateTime target = epoch.AddSeconds(time);
			TimeSpan span = target - epoch;

			return string.Format("{0}{1}{2}{3}"
				, span.Days > 0 ? string.Format("{0} {1} ", span.Days, span.Days == 1 ? "day" : "days") : ""
				, span.Hours > 0 && includeTime ? string.Format("{0} {1} ", span.Hours, span.Hours == 1 ? "hour" : "hours") : ""
				, span.Minutes > 0 && includeTime ? string.Format("{0} {1} ", span.Minutes, span.Minutes == 1 ? "minute" : "minutes") : ""
				, span.Seconds > 0 && includeTime && includeSeconds ? string.Format("{0} {1}", span.Seconds, span.Seconds == 1 ? "second" : "seconds") : ""
			);
		}
		public string PrintDateDeltaCompact(double time, bool includeTime, bool includeSeconds, bool useAbs)
		{
			if (IsInvalidTime(time))
				return InvalidTimeStr(time);
			if (time < 0 && useAbs)
				time = Math.Abs(time);
			if (time == 0d)
				return string.Format("0{0}", includeTime ? (includeSeconds ? "s" : "m") : "d");

			DateTime epoch = GetEpoch();
			DateTime target = epoch.AddSeconds(time);
			TimeSpan span = target - epoch;

			return string.Format("{0}{1}{2}{3}"
				, span.Days > 0 ? string.Format("{0}{1} ", span.Days, "d") : ""
				, span.Hours > 0 && includeTime ? string.Format("{0}{1} ", span.Hours, "h") : ""
				, span.Minutes > 0 && includeTime ? string.Format("{0}{1} ", span.Minutes, "m") : ""
				, span.Seconds > 0 && includeTime && includeSeconds ? string.Format("{0}{1}", span.Seconds, "s") : ""
			);
		}
		public string PrintDate(double time, bool includeTime, bool includeSeconds = false)
		{
			if (IsInvalidTime(time))
				return InvalidTimeStr(time);

			DateTime epoch = GetEpoch();
			DateTime target = epoch.AddSeconds(time);
			return string.Format("{0:" + dateFormat + "} {1}"
				, target
				, includeTime ? string.Format("{0:D2}:{1:D2}:{2:D2}", target.Hour, target.Minute, target.Second) : ""
			);
		}
		public string PrintDateNew(double time, bool includeTime)
		{
			if (IsInvalidTime(time))
				return InvalidTimeStr(time);

			DateTime epoch = GetEpoch();
			DateTime target = epoch.AddSeconds(time);

			return string.Format("{0:" + dateFormat + "} {1}"
				, target
				, includeTime ? string.Format("{0:D2}:{1:D2}:{2:D2}", target.Hour, target.Minute, target.Second) : ""
			);
		}
		public string PrintDateCompact(double time, bool includeTime, bool includeSeconds = false)
		{
			if (IsInvalidTime(time))
				return InvalidTimeStr(time);

			DateTime epoch = GetEpoch();
			DateTime target = epoch.AddSeconds(time);

			return string.Format("{0}-{1} {2}{3}"
				, target.Year
				, target.DayOfYear
				, includeTime ? string.Format("{0:D2}:{1:D2}", target.Hour, target.Minute) : ""
				, includeTime && includeSeconds ? string.Format(":{0:D2}", target.Second) : ""
			);
		}
		public int Minute {
			get {
				return 60;
			}
		}
		public int Hour {
			get {
				return 3600;
			}
		}
		public int Day {
			get {
				return 86400;
			}
		}
		public int Year {
			get {
				return 31536000;
			}
		}
		#endregion

		protected bool IsInvalidTime(double time)
		{
			if (double.IsNaN(time) || double.IsPositiveInfinity(time) || double.IsNegativeInfinity(time))
				return true;
			else
				return false;
		}
		protected string InvalidTimeStr(double time)
		{
			if (double.IsNaN(time)) {
				return "NaN";
			}
			if (double.IsPositiveInfinity(time)) {
				return "+Inf";
			}
			if (double.IsNegativeInfinity(time)) {
				return "-Inf";
			}
			return null;
		}

		protected DateTime DateFromUT(double time)
		{
			return GetEpoch().AddSeconds(time);
		}

		protected DateTime GetEpoch()
		{
			return epoch;
		}

		public RealDateTimeFormatter()
		{
		}

		public RealDateTimeFormatter(string dateFormat, DateTime epoch)
		{
			this.dateFormat = dateFormat;
			this.epoch = epoch;
		}
	}
}