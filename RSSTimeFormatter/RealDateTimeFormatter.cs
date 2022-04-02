using System;
using System.Text;

namespace RSSTimeFormatter
{
    public class RealDateTimeFormatter : IDateTimeFormatter
    {
        private readonly string dateFormat;
        private readonly string timeFormat;
        private readonly string dateTimeFormat;
        private readonly DateTime epoch;
        private readonly System.Globalization.CultureInfo culture;

        public RealDateTimeFormatter()
        {
        }

        public RealDateTimeFormatter(string dateFormat, string timeFormat, string dateTimeFormat, DateTime epoch)
        {
            this.dateFormat = dateFormat;
            this.timeFormat = timeFormat;
            this.dateTimeFormat = dateTimeFormat;
            this.epoch = epoch;
            culture = new System.Globalization.CultureInfo(KSP.Localization.Localizer.CurrentLanguage);
        }

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
                , span.Days != 0 ? span.Days.ToString() : ""
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
            return PrintDateDeltaCompact(time, includeTime, includeSeconds, useAbs, 5);
        }

        public string PrintDateDeltaCompact(double time, bool includeTime, bool includeSeconds, bool useAbs, int interestedPlaces)
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

            // interestedPlaces cheat sheet:
            // 5 - seconds
            // 4 - minutes
            // 3 - hours
            // 2 - days
            // 1 - years
            // Note: Printing out the difference in years isn't supported. Instead for levels 1 and 2 we will always print Days.

            StringBuilder sb = StringBuilderCache.Acquire(256);
            if (span.Days > 0 && interestedPlaces >= 1)
            {
                sb.AppendFormat("{0}d", span.Days);
            }

            if (includeTime)
            {
                if (span.Hours > 0 && interestedPlaces >= 3)
                {
                    if (sb.Length != 0)
                        sb.Append(" ");
                    sb.AppendFormat("{0}h", span.Hours);
                }

                if (span.Minutes > 0 && interestedPlaces >= 4)
                {
                    if (sb.Length != 0)
                        sb.Append(" ");
                    sb.AppendFormat("{0}m", span.Minutes);
                }

                if (includeSeconds && span.Seconds > 0 && interestedPlaces >= 5)
                {
                    if (sb.Length != 0)
                        sb.Append(" ");
                    sb.AppendFormat("{0}s", span.Seconds);
                }
            }

            if (sb.Length == 0)
                sb.AppendFormat("0{0}", includeTime ? (includeSeconds ? "s" : "m") : "d");

            return sb.ToStringAndRelease();
        }

        private string FormatDate(DateTime t)
        {
            return string.Format(culture, "{0:" + dateFormat + "}", t);
        }

        private string FormatTime(DateTime t)
        {
            return string.Format(culture, "{0:" + timeFormat + "}", t);
        }

        private string FormatDateTime(DateTime t)
        {
            return string.Format(culture, dateTimeFormat, FormatTime(t), FormatDate(t));
        }

        public string PrintDate(double time, bool includeTime, bool includeSeconds = false)
        {
            if (IsInvalidTime(time))
                return InvalidTimeStr(time);

            DateTime epoch = GetEpoch();
            DateTime target = epoch.AddSeconds(time);
            return includeTime ? FormatDateTime(target) : FormatDate(target);
        }

        public string PrintDateNew(double time, bool includeTime)
        {
            if (IsInvalidTime(time))
                return InvalidTimeStr(time);

            DateTime epoch = GetEpoch();
            DateTime target = epoch.AddSeconds(time);
            return includeTime ? FormatDateTime(target) : FormatDate(target);
        }

        // This is chiefly used by the MET display in flight view.
        public string PrintDateCompact(double time, bool includeTime, bool includeSeconds = false)
        {
            if (IsInvalidTime(time))
                return InvalidTimeStr(time);

            DateTime epoch = GetEpoch();
            DateTime target = epoch.AddSeconds(time);

            // TODO(egg): This format should probably be configurable too, even
            // if we keep this default.
            return string.Format("{0:yyyy-MM-dd} {1}{2}" // Always use ISO-8601 (stock is yyy-dd).
                , target
                , includeTime ? string.Format("{0:D2}:{1:D2}", target.Hour, target.Minute) : ""
                , includeTime && includeSeconds ? string.Format(":{0:D2}", target.Second) : ""
            );
        }

        public int Minute => 60;
        public int Hour => 3600;
        public int Day => 86400;
        public int Year => 31536000;
        #endregion

        protected bool IsInvalidTime(double time)
        {
            return double.IsNaN(time) || double.IsPositiveInfinity(time) || double.IsNegativeInfinity(time);
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
    }
}
