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
			TimeSpan span = TimeSpan.FromSeconds(time);
			return SpanAsLongFormDateString(span);
		}

		public string PrintTimeStamp(double time, bool days = false, bool years = false)
		{
			// short-circuit if invalid time passed
			if (IsInvalidTime(time))
				return InvalidTimeStr(time);
			TimeSpan span = TimeSpan.FromSeconds(time);
			return string.Format("{0}{1}"
				, days ? $"Day {span.Days} - " : ""
				, SpanAs24HWithSeconds(span)
			);
		}

		public string PrintTimeStampCompact(double time, bool days = false, bool years = false)
		{
			// short-circuit if invalid time passed
			if (IsInvalidTime(time))
				return InvalidTimeStr(time);
			TimeSpan span = TimeSpan.FromSeconds(time);
			int totalYears = (int)(span.TotalDays / 365);
			return string.Format("{0}{1}{2}"
				, years ? $"{totalYears}y, " : ""
				, days ? $"{span.Days}d, " : ""
				, SpanAs24HWithSeconds(span)
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
			bool isTimeNegative = time < 0;
			time = Math.Abs(time);
			TimeSpan span = TimeSpan.FromSeconds(time);
			int[] data = DataFromTimeSpan(span);
			string[] intervalCaptions = {"y", "d", "h", "m", "s"};
			string result = isTimeNegative ? "- " : (explicitPositive ? "+ " : "");
			for (int i = 0; i <= data.Length - 1; i++) {
				if (data[i] != 0) {
					for (int j = 0; j < valuesOfInterest; j++) {
						if (j < data.Length - 1) {
							result += $"{data[i + j]}{intervalCaptions[i + j]}";
						}
						if (j != valuesOfInterest - 1 && j != data.Length - 1) {
							result += ", ";
						}
					}
					break;
				}
			}
			if (result == "") {
				return "0s";
			}
			return result;
		}

		public string PrintTime(double time, int valuesOfInterest, bool explicitPositive, bool logEnglish)
		{
			return PrintTime(time, valuesOfInterest, explicitPositive);
		}

		public string PrintTimeCompact(double time, bool explicitPositive)
		{
			if (IsInvalidTime(time))
				return InvalidTimeStr(time);
			bool isTimeNegative = time < 0;
			time = Math.Abs(time);
			TimeSpan span = TimeSpan.FromSeconds(time);
			return string.Format("{0}{1}{2}"
				, isTimeNegative ? "- " : (explicitPositive ? "+ " : "")
				, (span.Days != 0 ? span.Days.ToString() : "")
				, SpanAs24HWithSeconds(span)
			);
		}

		public string PrintDateDelta(double time, bool includeTime, bool includeSeconds, bool useAbs)
		{
			if (IsInvalidTime(time))
				return InvalidTimeStr(time);
            if (useAbs) time = Math.Abs(time);
			if (time == 0d)
				return string.Format("0 {0}", includeTime ? (includeSeconds ? "seconds" : "minutes") : "days");
			TimeSpan span = TimeSpan.FromSeconds(time);
			return SpanAsLongFormDateString(span, " ");
		}

		public string PrintDateDeltaCompact(double time, bool includeTime, bool includeSeconds, bool useAbs)
		{
			if (IsInvalidTime(time))
				return InvalidTimeStr(time);
            if (useAbs) time = Math.Abs(time);
            if (time == 0d)
				return string.Format("0{0}", includeTime ? (includeSeconds ? "s" : "m") : "d");
			TimeSpan span = TimeSpan.FromSeconds(time);
			return SpanAsShortFormDateString(span, " ");
		}

		public string PrintDate(double time, bool includeTime, bool includeSeconds = false)
		{
			if (IsInvalidTime(time))
				return InvalidTimeStr(time);
			DateTime target = GetEpoch().AddSeconds(time);
			return string.Format("{0:" + dateFormat + "}{1}"
				, target
				, includeTime ? $" {DateTimeAs24H(target, includeSeconds)}" : ""
			);
		}

		public string PrintDateNew(double time, bool includeTime)
		{
			return PrintDate(time, includeTime, true);
		}

		public string PrintDateCompact(double time, bool includeTime, bool includeSeconds = false)
		{
			if (IsInvalidTime(time))
				return InvalidTimeStr(time);

			DateTime target = GetEpoch().AddSeconds(time);

			return string.Format("{0}-{1}{2}"
				, target.Year
				, target.DayOfYear
				, includeTime ? $" {DateTimeAs24H(target, includeSeconds)}" : ""
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
		
		private int[] DataFromTimeSpan(TimeSpan span)
		{
			return new int[]
			{
				(int)(span.TotalDays / 365),
				span.Days,
				span.Hours,
				span.Minutes,
				span.Seconds
			};
		}

		private string SpanAs24H(TimeSpan span, bool includeSeconds)
		{
			if (includeSeconds)
				return SpanAs24HWithSeconds(span);
			return SpanAs24H(span);
		}

		private string SpanAs24H(TimeSpan span)
		{
			return $"{span.Hours:D2}:{span.Minutes:D2}";
		}

		private string SpanAs24HWithSeconds(TimeSpan span)
		{
			return $"{span.Hours:D2}:{span.Minutes:D2}:{span.Seconds:D2}";
		}

		private string DateTimeAs24H(DateTime time, bool includeSeconds)
		{
			if (includeSeconds)
				return DateTimeAs24HWithSeconds(time);
			return DateTimeAs24H(time);
		}

		private string DateTimeAs24H(DateTime time)
		{
			return $"{time.Hour:D2}:{time.Minute:D2}";
		}

		private string DateTimeAs24HWithSeconds(DateTime time)
		{
			return $"{time.Hour:D2}:{time.Minute:D2}:{time.Second:D2}";
		}

		private string SpanAsDateString(TimeSpan span, bool longForm, string seperator = ", ")
		{
			if (longForm) return SpanAsLongFormDateString(span, seperator);
			return SpanAsShortFormDateString(span, seperator);
		}

		private string SpanAsShortFormDateString(TimeSpan span, string seperator = ", ")
		{
			return string.Format("{1}d{0}{2}h{0}{3}m{0}{4}s"
				, seperator
				, span.Days
				, span.Hours
				, span.Minutes
				, span.Seconds
			);
		}

		private string SpanAsLongFormDateString(TimeSpan span, string seperator = ", ")
		{
			return string.Format("{1}{2}{0}{3}{4}{0}{5}{6}{0}{7}{8}"
				, seperator
				, span.Days, span.Days == 1 ? "day" : "days"
				, span.Hours, span.Hours == 1 ? "hour" : "hours"
				, span.Minutes, span.Minutes == 1 ? "minute" : "minutes"
				, span.Seconds, span.Seconds == 1 ? "second" : "seconds"
			);
		}

		private string DateTimeAsDateString(DateTime time, bool longForm, string seperator = ", ")
		{
			if (longForm) return DateTimeAsLongFormDateString(time, seperator);
			return DateTimeAsShortFormDateString(time, seperator);
		}

		private string DateTimeAsShortFormDateString(DateTime time, string seperator = ", ")
		{
			return string.Format("{1}d{0}{2}h{0}{3}m{0}{4}s"
				, seperator
				, time.Day
				, time.Hour
				, time.Minute
				, time.Second
			);
		}

		private string DateTimeAsLongFormDateString(DateTime time, string seperator = ", ")
		{
			return string.Format("{1}{2}{0}{3}{4}{0}{5}{6}{0}{7}{8}"
				, seperator
				, time.Day, time.Day == 1 ? "day" : "days"
				, time.Hour, time.Hour == 1 ? "hour" : "hours"
				, time.Minute, time.Minute == 1 ? "minute" : "minutes"
				, time.Second, time.Second == 1 ? "second" : "seconds"
			);
		}

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