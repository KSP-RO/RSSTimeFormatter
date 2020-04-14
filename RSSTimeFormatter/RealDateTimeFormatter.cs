using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RSSTimeFormatter
{
    public class RealDateTimeFormatter : IDateTimeFormatter {
        private string dateFormat;
        private DateTime epoch;

        #region IDateTimeFormatter implementation
        public string PrintTimeLong(double time) {
            // short-circuit if invalid time passed
            if (IsInvalidTime(time))
                return InvalidTimeStr(time);
            TimeSpan span = TimeSpan.FromSeconds(time);
            return spanAsLongFormDateString(span);
        }
        public string PrintTimeStamp(double time, bool days = false, bool years = false) {
            // short-circuit if invalid time passed
            if (IsInvalidTime(time))
                return InvalidTimeStr(time);
            TimeSpan span = TimeSpan.FromSeconds(time);
            return string.Format("{0}{1}"
                , days ? string.Format("Day {0} - ", span.Days) : ""
                , spanAs24HWithSeconds(span)
            );
        }
        public string PrintTimeStampCompact(double time, bool days = false, bool years = false) {
            // short-circuit if invalid time passed
            if (IsInvalidTime(time))
                return InvalidTimeStr(time);
            TimeSpan span = TimeSpan.FromSeconds(time);
            int totalYears = (int)(span.TotalDays / 365);
            return string.Format("{0}{1}{2}"
                , years ? string.Format("{0}y, ", totalYears) : ""
                , days ? string.Format("{0}d, ", span.Days) : ""
                , spanAs24HWithSeconds(span)
            );
        }
        public string PrintTime(double time, int valuesOfInterest, bool explicitPositive) {
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
            int years = (int)(span.TotalDays / 365);
            return string.Format("{0}{1}{2}{3}{4}{5}"
                , isTimeNegative ? "- " : (explicitPositive ? "+ " : "")
                , years > 0 ? string.Format("{0}y, ", years) : ""
                , (valuesOfInterest >= 1 && span.Days != 0) ? string.Format("{0}d, ", span.Days) : ""
                , (valuesOfInterest >= 2 && span.Hours != 0) ? string.Format("{0}h, ", span.Hours) : ""
                , (valuesOfInterest >= 3 && span.Minutes != 0) ? string.Format("{0}m, ", span.Minutes) : ""
                , valuesOfInterest >= 4 ? string.Format("{0}s", span.Seconds) : ""
            );
        }

        public string PrintTime(double time, int valuesOfInterest, bool explicitPositive, bool logEnglish) {
            return PrintTime(time, valuesOfInterest, explicitPositive);
        }
        
        public string PrintTimeCompact(double time, bool explicitPositive) {
            if (IsInvalidTime(time))
                return InvalidTimeStr(time);
            bool isTimeNegative = time < 0;
            time = Math.Abs(time);
            TimeSpan span = TimeSpan.FromSeconds(time);
            return string.Format("{0}{1}{2}"
                , isTimeNegative ? "- " : (explicitPositive ? "+ " : "")
                , (span.Days != 0 ? span.Days.ToString() : "")
                , spanAs24HWithSeconds(span)
            );
        }
        public string PrintDateDelta(double time, bool includeTime, bool includeSeconds, bool useAbs) {
            if (IsInvalidTime(time))
                return InvalidTimeStr(time);
            time = useAbs ? Math.Abs(time) : time;
            if (time == 0d)
                return string.Format("0 {0}", includeTime ? (includeSeconds ? "seconds" : "minutes") : "days");
            TimeSpan span = TimeSpan.FromSeconds(time);       
            return spanAsLongFormDateString(span, " ");
        }
        public string PrintDateDeltaCompact(double time, bool includeTime, bool includeSeconds, bool useAbs) {
            if (IsInvalidTime(time))
                return InvalidTimeStr(time);
            time = useAbs ? Math.Abs(time) : time;
            if (time == 0d)
                return string.Format("0{0}", includeTime ? (includeSeconds ? "s" : "m") : "d");
            TimeSpan span = TimeSpan.FromSeconds(time);
            return spanAsShortFormDateString(span, " ");
        }
        public string PrintDate(double time, bool includeTime, bool includeSeconds = false) {
            if (IsInvalidTime(time))
                return InvalidTimeStr(time);
            DateTime target = GetEpoch().AddSeconds(time);
            return string.Format("{0:" + dateFormat + "}{1}"
                , target
                , includeTime ? (" " + dateTimeAs24H(target, includeSeconds)) : ""
            );
        }
        public string PrintDateNew(double time, bool includeTime) {
            return PrintDate(time, includeTime, true);
        }

        public string PrintDateCompact(double time, bool includeTime, bool includeSeconds = false) {
            if (IsInvalidTime(time))
                return InvalidTimeStr(time);

            DateTime target = GetEpoch().AddSeconds(time);

            return string.Format("{0}-{1}{2}"
                , target.Year
                , target.DayOfYear
                , includeTime ? (" " + dateTimeAs24H(target, includeSeconds)): ""
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

        private string spanAs24H(TimeSpan span, bool includeSeconds) {
            if (includeSeconds)
                return spanAs24HWithSeconds(span);
            return spanAs24H(span);        
        }
        private string spanAs24H(TimeSpan span) {
            return string.Format("{0:D2}:{1:D2}",
                span.Hours, 
                span.Minutes);
        }
        private string spanAs24HWithSeconds(TimeSpan span) {
            return string.Format("{0:D2}:{1:D2}:{2:D2}",
                span.Hours,
                span.Minutes,
                span.Seconds);
        }

        private string dateTimeAs24H(DateTime time, bool includeSeconds) {
            if (includeSeconds)
                return dateTimeAs24HWithSeconds(time);
            return dateTimeAs24H(time);
        }
        private string dateTimeAs24H(DateTime time) {
            return string.Format("{0:D2}:{1:D2}",
                time.Hour,
                time.Minute);
        }
        private string dateTimeAs24HWithSeconds(DateTime time) {
            return string.Format("{0:D2}:{1:D2}:{2:D2}",
                time.Hour,
                time.Minute,
                time.Second);
        }

        private string spanAsDateString(TimeSpan span, bool longForm, string seperator = ", ") {
            if (longForm) return spanAsLongFormDateString(span, seperator);
            return spanAsShortFormDateString(span, seperator);
        }
        private string spanAsShortFormDateString(TimeSpan span, string seperator = ", ") {
            return string.Format("{1}d{0}{2}h{0}{3}m{0}{4}s"
                , seperator
                , span.Days
                , span.Hours
                , span.Minutes
                , span.Seconds
            );
        }
        private string spanAsLongFormDateString(TimeSpan span, string seperator = ", ") {
            return string.Format("{1}{2}{0}{3}{4}{0}{5}{6}{0}{7}{8}"
                , seperator
                , span.Days, span.Days == 1 ? "day" : "days"
                , span.Hours, span.Hours == 1 ? "hour" : "hours"
                , span.Minutes, span.Minutes == 1 ? "minute" : "minutes"
                , span.Seconds, span.Seconds == 1 ? "second" : "seconds"
            );
        }
        private string dateTimeAsDateString(DateTime time, bool longForm, string seperator = ", ") {
            if (longForm) return dateTimeAsLongFormDateString(time, seperator);
            return dateTimeAsShortFormDateString(time, seperator);
        }
        private string dateTimeAsShortFormDateString(DateTime time, string seperator = ", ") {
            return string.Format("{1}d{0}{2}h{0}{3}m{0}{4}s"
                , seperator
                , time.Day
                , time.Hour
                , time.Minute
                , time.Second
            );
        }
        private string dateTimeAsLongFormDateString(DateTime time, string seperator = ", ") {
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