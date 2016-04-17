using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.IO;
using UniLinq;

public interface IDateTimeFormatter
{
    string PrintTimeLong(double time);
    string PrintTimeStamp(double time, bool days = false, bool years = false);
    string PrintTimeStampCompact(double time, bool days = false, bool years = false);
    string PrintTime(double time, int valuesOfInterest, bool explicitPositive);
    string PrintTimeCompact(double time, bool explicitPositive);
    string PrintDateDelta(double time, bool includeTime, bool includeSeconds, bool useAbs);
    string PrintDateDeltaCompact(double time, bool includeTime, bool includeSeconds, bool useAbs);
    string PrintDate(double time, bool includeTime, bool includeSeconds = false);
    string PrintDateNew(double time, bool includeTime);
    string PrintDateCompact(double time, bool includeTime, bool includeSeconds = false);

    int Minute { get; }
    int Hour { get; }
    int Day { get; }
    int Year { get; }
}

public static class KSPUtil
{
    public class DefaultDateTimeFormatter : IDateTimeFormatter
    {
        static string AddUnits(int val, string singular, string plural)
        {
            return val.ToString() + (val == 1 ? singular : plural);
        }
        static string IsBadNum (double time)
        {
            if (double.IsNaN(time))
            {
                return "NaN";
            }
            if (double.IsPositiveInfinity(time))
            {
                return "+Inf";
            }
            if (double.IsNegativeInfinity(time))
            {
                return "-Inf";
            }
            return null;
        }
        public string PrintTimeLong(double time)
        {
            string badStr = IsBadNum(time);
            if (badStr != null)
            {
                return badStr;
            }
            int[] timeIntervals = GetDateFromUT(time);
            string timeStr = AddUnits(timeIntervals[4], "Year", "Years");
            timeStr += ", " + AddUnits(timeIntervals[3], "Day", "Days");
            timeStr += ", " + AddUnits(timeIntervals[2], "Hour", "Hours");
            timeStr += ", " + AddUnits(timeIntervals[1], "Min", "Mins");
            timeStr += ", " + AddUnits(timeIntervals[0], "Sec", "Secs");
            return timeStr;
        }
        public string PrintTimeStamp(double time, bool days = false, bool years = false)
        {
            string badStr = IsBadNum(time);
            if (badStr != null)
            {
                return badStr;
            }
            int[] timeIntervals = GetDateFromUT(time);
            string timeStr = "";
            if (years)
            {
                timeStr += "Year " + timeIntervals[4] + ", ";
            }
            if (days)
            {
                timeStr += "Day " + timeIntervals[3] + " - ";
            }
            timeStr += timeIntervals[2].ToString("00");
            timeStr += ":" + timeIntervals[1].ToString("00");
            if (timeIntervals[4] < 10)
                timeStr += ":" + timeIntervals[0].ToString("00");
            return timeStr;
        }
        public string PrintTimeStampCompact(double time, bool days = false, bool years = false)
        {
            string badStr = IsBadNum(time);
            if (badStr != null)
            {
                return badStr;
            }
            int[] timeIntervals = GetDateFromUT(time);
            string timeStr = "";
            if (years)
            {
                timeStr += timeIntervals[4].ToString() + "y, ";
            }
            if (days)
            {
                timeStr += timeIntervals[3].ToString() + "d, ";
            }
            timeStr += timeIntervals[2].ToString("00");
            timeStr += ":" + timeIntervals[1].ToString("00");
            if (timeIntervals[4] < 10)
                timeStr += ":" + timeIntervals[0].ToString("00");
            return timeStr;
        }
        public string PrintTime(double time, int valuesOfInterest, bool explicitPositive)
        {
            string badStr = IsBadNum(time);
            if (badStr != null)
            {
                return badStr;
            }
            bool isNegative = time < 0;
            int[] timeIntervals = GetDateFromUT(time);

            string[] intervalCaptions = new string[]
                {
                    "s", "m", "h", "d", "y"
                };

            string timeString = isNegative ? "- " : (explicitPositive ? "+ " : "");

            // find first non-zero value, checking backwards
            for (int i = timeIntervals.Length - 1; i >= 0; i--)
            {
                if (timeIntervals[i] != 0)
                {
                    for (int j = i; j > Mathf.Max(i - valuesOfInterest, -1); j--)
                    {
                        timeString += Math.Abs(timeIntervals[j]) + intervalCaptions[j] + (j - 1 > Mathf.Max(i - valuesOfInterest, -1) ? ", " : "");
                    }
                    break;
                }
            }

            return timeString;
        }

        public string PrintTimeCompact(double time, bool explicitPositive)
        {
            string badStr = IsBadNum(time);
            if (badStr != null)
            {
                return badStr;
            }
            bool isNegative = time < 0;
            int[] timeIntervals = GetDateFromUT(time);

            string timeString = isNegative ? "T- " : (explicitPositive ? "T+ " : "");

            timeString += (timeIntervals[3] > 0 ? Math.Abs(timeIntervals[3]).ToString() + ":" : "") +
                Math.Abs(timeIntervals[2]).ToString("00") + ":" +
                Math.Abs(timeIntervals[1]).ToString("00") + ":" +
                Math.Abs(timeIntervals[0]).ToString("00");

            // find first non-zero value, checking backwards
            /*for (int i = timeIntervals.Length - 1; i >= 0; i--)
            {
                if (timeIntervals[i] != 0)
                {
                    for (int j = i; j > Mathf.Max(i - valuesOfInterest, -1); j--)
                    {
                        timeString += (j==3 ? Math.Abs(timeIntervals[j]).ToString() : Math.Abs(timeIntervals[j]).ToString("00")) + (j - 1 > Mathf.Max(i - valuesOfInterest, -1) ? ":" : "");
                    }
                    break;
                }
            }
            */
            return timeString;
        }

        public int[] GetDateFromUT(double time)
        {
            return GameSettings.KERBIN_TIME ? GetKerbinDateFromUT(time) : GetEarthDateFromUT(time);
        }

        static int[] get_date_from_UT (double time, int year_len, int day_len)
        {
            int years = (int) (time / year_len);
            time -= (double)years * (double)year_len;
            int seconds = (int) time;
            int minutes = (seconds / 60) % 60;
            int hours = (seconds / 3600) % (day_len / 3600);
            int days = seconds / day_len;
            int[] timeIntervals = new int[] {
                seconds % 60,
                minutes,
                hours,
                days,
                years
            };

            return timeIntervals;
        }

        public int[] GetEarthDateFromUT(double time)
        {
            return get_date_from_UT (time, EarthYear, EarthDay);
        }

        public int[] GetKerbinDateFromUT(double time)
        {
            return get_date_from_UT (time, KerbinYear, KerbinDay);
        }

        public string PrintDateDelta(double time, bool includeTime, bool includeSeconds, bool useAbs)
        {
            string badStr = IsBadNum(time);
            if (badStr != null)
            {
                return badStr;
            }
            if (useAbs && time < 0d)
                time = -time;

            string date = "";

            int[] saveDate = GetDateFromUT(time);

            if (saveDate[4] > 1)
            {
                date += saveDate[4].ToString() + " years";
            }
            else if (saveDate[4] == 1)
            {
                date += saveDate[4].ToString() + " year";
            }

            if (saveDate[3] > 1)
            {
                if (date != "")
                    date += ", ";

                date += saveDate[3].ToString() + " days";
            }
            else if (saveDate[3] == 1)
            {
                if (date != "")
                    date += ", ";

                date += saveDate[3].ToString() + " day";
            }

            if (includeTime)
            {
                if (saveDate[2] > 1)
                {
                    if (date != "")
                        date += ", ";

                    date += saveDate[2].ToString() + " hours";
                }
                else if (saveDate[2] == 1)
                {
                    if (date != "")
                        date += ", ";

                    date += saveDate[2].ToString() + " hour";
                }

                if (saveDate[1] > 1)
                {
                    if (date != "")
                        date += ", ";

                    date += saveDate[1].ToString() + " minutes";
                }
                else if (saveDate[1] == 1)
                {
                    if (date != "")
                        date += ", ";

                    date += saveDate[1].ToString() + " minute";
                }

                if (includeSeconds)
                {
                    if (saveDate[0] > 1)
                    {
                        if (date != "")
                            date += ", ";

                        date += saveDate[0].ToString() + " seconds";
                    }
                    else if (saveDate[0] == 1)
                    {
                        if (date != "")
                            date += ", ";

                        date += saveDate[0].ToString() + " second";
                    }
                }
            }

            if (string.IsNullOrEmpty(date))
                date = includeTime ? (includeSeconds ? "0 seconds" : "0 minutes") : "0 days";

            return date;
        }

        public string PrintDateDeltaCompact(double time, bool includeTime, bool includeSeconds, bool useAbs)
        {
            string badStr = IsBadNum(time);
            if (badStr != null)
            {
                return badStr;
            }
            if (useAbs && time < 0d)
                time = -time;

            string date = "";

            int[] saveDate = GetDateFromUT(time);

            if (saveDate[4] > 0)
            {
                date += saveDate[4].ToString() + "y";
            }

            if (saveDate[3] > 0)
            {
                if (date != "")
                    date += ", ";

                date += saveDate[3].ToString() + "d";
            }

            if (includeTime)
            {
                if (saveDate[2] > 0)
                {
                    if (date != "")
                        date += ", ";

                    date += saveDate[2].ToString() + "h";
                }
                if (saveDate[1] > 0)
                {
                    if (date != "")
                        date += ", ";

                    date += saveDate[1].ToString() + "m";
                }

                if (includeSeconds)
                {
                    if (saveDate[0] > 0)
                    {
                        if (date != "")
                            date += ", ";

                        date += saveDate[0].ToString() + "s";
                    }
                }
            }

            if (string.IsNullOrEmpty(date))
                date = includeTime ? (includeSeconds ? "0s" : "0m") : "0d";

            return date;
        }

        public string PrintDate(double time, bool includeTime, bool includeSeconds = false)
        {
            string badStr = IsBadNum(time);
            if (badStr != null)
            {
                return badStr;
            }
            string date = "";

            int[] saveDate = GetDateFromUT(time);

            date += "Year " + (saveDate[4] + 1) + ", Day " + (saveDate[3] + 1);

            if (includeTime)
            {
                date += " - " + saveDate[2] + "h, " + saveDate[1] + "m";
            }
            if (includeSeconds)
            {
                date += ", " + saveDate[0] + "s";
            }

            return date;
        }

        public string PrintDateNew(double time, bool includeTime)
        {
            string badStr = IsBadNum(time);
            if (badStr != null)
            {
                return badStr;
            }
            string date = "";

            int[] saveDate = GetDateFromUT(time);

            date += "Year " + (saveDate[4] + 1) + ", Day " + (saveDate[3] + 1);

            if (includeTime)
            {
                date += " - " + saveDate[2].ToString("D2") + ":" + saveDate[1].ToString("D2") + ":" + saveDate[0].ToString("D2");
            }

            return date;
        }

        public string PrintDateCompact(double time, bool includeTime, bool includeSeconds = false)
        {
            string badStr = IsBadNum(time);
            if (badStr != null)
            {
                return badStr;
            }
            string date = "";

            int[] saveDate = GetDateFromUT(time);

            date += "Y" + (saveDate[4] + 1) + ", D" + (saveDate[3] + 1).ToString("00");

            if (includeTime)
            {
                date += ", " + saveDate[2] + ":" + saveDate[1].ToString("00");
            }
            if (includeSeconds)
            {
                date += ":" + saveDate[0].ToString("00");
            }

            return date;
        }

        public int Minute { get { return 60; } }
        public int Hour { get { return 3600; } }
        public int Day { get { return GameSettings.KERBIN_TIME ? KerbinDay : EarthDay; } }
        public int Year { get { return GameSettings.KERBIN_TIME ? KerbinYear : EarthYear; } }

        public int KerbinDay { get { return 21600; } }
        public int KerbinYear { get { return 9201600; } }
        public int EarthDay { get { return 86400; } }
        public int EarthYear { get { return 31536000; } }
    }

    static IDateTimeFormatter _dateTimeFormatter;
    public static IDateTimeFormatter dateTimeFormatter
    {
        get
        {
            if (_dateTimeFormatter == null)
            {
                _dateTimeFormatter = new DefaultDateTimeFormatter();
            }
            return _dateTimeFormatter;
        }
        set
        {
            _dateTimeFormatter = value;
        }
    }

    /// <summary>
    /// Returns a comma-separated string for the given vector
    /// </summary>
    public static string WriteVector(Vector2 vector)
    {
        //if (vector == null) return "";
        return vector.x.ToString() + "," + vector.y.ToString();
    }
    /// <summary>
    /// Returns a comma-separated string for the given vector
    /// </summary>
    public static string WriteVector(Vector3 vector)
    {
        //if (vector == null) return "";
        return vector.x.ToString() + "," + vector.y.ToString() + "," + vector.z.ToString();
    }
    /// <summary>
    /// Returns a comma-separated string for the given vector
    /// </summary>
    public static string WriteVector(Vector3d vector)
    {
        //if (vector == null) return "";
        return vector.x.ToString() + "," + vector.y.ToString() + "," + vector.z.ToString();
    }
    /// <summary>
    /// Returns a comma-separated string for the given vector
    /// </summary>
    public static string WriteVector(Vector4 vector)
    {
        //if (vector == null) return "";
        return vector.x.ToString() + "," + vector.y.ToString() + "," + vector.z.ToString() + "," + vector.w.ToString();
    }
    /// <summary>
    /// Returns a comma-separated string for the given quaternion
    /// </summary>
    public static string WriteQuaternion(Quaternion quaternion)
    {
        //if (vector == null) return "";
        return quaternion.x.ToString() + "," + quaternion.y.ToString() + "," + quaternion.z.ToString() + "," + quaternion.w.ToString();
    }
    /// <summary>
    /// Returns a comma-separated string for the given quaternion
    /// </summary>
    public static string WriteQuaternion(QuaternionD quaternion)
    {
        //if (vector == null) return "";
        return quaternion.x.ToString() + "," + quaternion.y.ToString() + "," + quaternion.z.ToString() + "," + quaternion.w.ToString();
    }

    /// <summary>
    /// Parses a Vector2 from a comma-separated string of XYZ values
    /// </summary>
    public static Vector2 ParseVector2(string vectorString)
    {
        string[] vectorData = vectorString.Split(',');
        if (vectorData.Length < 2)
        { Debug.Log("WARNING: Vector2 entry is nor formatted properly! proper format for Vector2s is x,y"); return Vector2.zero; }

        return new Vector2(float.Parse(vectorData[0]), float.Parse(vectorData[1]));
    }

    public static Vector2 ParseVector2(string x, string y)
    {
        return new Vector2(float.Parse(x), float.Parse(y));
    }

    /// <summary>
    /// Parses a Vector3 from a comma-separated string of XYZ values
    /// </summary>
    public static Vector3 ParseVector3(string vectorString)
    {
        string[] vectorData = vectorString.Split(',');
        if (vectorData.Length < 3)
        { Debug.Log("WARNING: Vector3 entry is nor formatted properly! proper format for Vector3s is x,y,z"); return Vector3.zero; }

        return new Vector3(float.Parse(vectorData[0]), float.Parse(vectorData[1]), float.Parse(vectorData[2]));
    }

    public static Vector3 ParseVector3(string x, string y, string z)
    {
        return new Vector3(float.Parse(x), float.Parse(y), float.Parse(z));
    }

    /// <summary>
    /// Parses a Vector3 from a comma-separated string of XYZ values
    /// </summary>
    public static Vector3d ParseVector3d(string vectorString)
    {
        string[] vectorData = vectorString.Split(',');
        if (vectorData.Length < 3)
        { Debug.Log("WARNING: Vector3d entry is nor formatted properly! proper format for Vector3s is x,y,z"); return Vector3d.zero; }

        return new Vector3d(double.Parse(vectorData[0]), double.Parse(vectorData[1]), double.Parse(vectorData[2]));
    }

    public static Vector3d ParseVector3d(string x, string y, string z)
    {
        return new Vector3d(double.Parse(x), double.Parse(y), double.Parse(z));
    }

    /// <summary>
    /// Parses a Vector4 from a comma-separated string of XYZ values
    /// </summary>
    public static Vector4 ParseVector4(string vectorString)
    {
        string[] vectorData = vectorString.Split(',');
        if (vectorData.Length < 4)
        { Debug.Log("WARNING: Vector4 entry is nor formatted properly! proper format for Vector4s is x,y,z,w"); return Vector4.zero; }

        return new Vector4(float.Parse(vectorData[0]), float.Parse(vectorData[1]), float.Parse(vectorData[2]), float.Parse(vectorData[3]));
    }
    public static Vector4 ParseVector4(string x, string y, string z, string w)
    {
        return new Vector4(float.Parse(x), float.Parse(y), float.Parse(z), float.Parse(w));
    }

    public static Quaternion ParseQuaternion(string quaternionString)
    {
        string[] quatData = quaternionString.Split(',');
        if (quatData.Length < 4)
        {
            Debug.Log("WARNING: Quaternion entry is nor formatted properly! proper format for Quaternion is x,y,z,w");
            return Quaternion.identity;
        }

        return new Quaternion(float.Parse(quatData[0]), float.Parse(quatData[1]), float.Parse(quatData[2]), float.Parse(quatData[3]));

    }

    public static Quaternion ParseQuaternion(string x, string y, string z, string w)
    {
        return new Quaternion(float.Parse(x), float.Parse(y), float.Parse(z), float.Parse(w));
    }

    public static QuaternionD ParseQuaternionD(string quaternionString)
    {
        string[] quatData = quaternionString.Split(',');
        if (quatData.Length < 4)
        { Debug.Log("WARNING: QuaternionD entry is nor formatted properly! proper format for QuaternionD is x,y,z,w"); return QuaternionD.identity; }

        return new QuaternionD(double.Parse(quatData[0]), double.Parse(quatData[1]), double.Parse(quatData[2]), double.Parse(quatData[3]));

    }

    public static QuaternionD ParseQuaternionD(string x, string y, string z, string w)
    {
        return new QuaternionD(double.Parse(x), double.Parse(y), double.Parse(z), double.Parse(w));
    }


    public static string WriteArray<T>(T[] array) where T : IConvertible
    {
        string s = "";

        for (int i = 0; i < array.Length; i++)
        {
            s += array[i].ToString();

            if (i < array.Length - 1)
            {
                s += "; ";
            }
        }

        return s;
    }

    public static T[] ParseArray<T>(string arrayString, ParserMethod<T> parser)
    {
        string[] ss = arrayString.Split(';');

        T[] ts = new T[ss.Length];

        for (int i = 0; i < ss.Length; i++)
        {
            ts[i] = parser(ss[i].Trim());
        }

        return ts;
    }



    public static Transform FindInPartModel(Transform part, string childName)
    {
        Transform obj = part.FindChild("model").FindChild(childName);

        if (!obj) return recurseModels(part.FindChild("model"), childName);
        else return obj;
    }

    private static Transform recurseModels(Transform obj, string childName)
    {
        Transform c = null;
        foreach (Transform t in obj)
        {
            if (t.name == childName) return t;
            else
            {
                c = recurseModels(t, childName);
                if (c != null) break;
            }
        }
        return c;
    }

    public static string PrintCoordinates(double latitude, double longitude, bool singleLine)
    {
        return PrintLatitude(latitude) +
            (singleLine ? ", " : "\n") +
            PrintLongitude(longitude);
    }

    public static string PrintLatitude(double latitude)
    {
        int latSeconds = (int)Math.Round(latitude * 3600);
        int latDegrees = latSeconds / 3600;
        latSeconds = Math.Abs(latSeconds % 3600);
        int latMinutes = latSeconds / 60;
        latSeconds %= 60;
        return Math.Abs(latDegrees) + "Â° " + latMinutes + "' " + latSeconds + "\" " + (latitude >= 0 ? "N" : "S");
    }

    public static string PrintLongitude(double longitude)
    {
        int lonSeconds = (int)Math.Round(longitude * 3600);
        int lonDegrees = lonSeconds / 3600;
        lonSeconds = Math.Abs(lonSeconds % 3600);
        int lonMinutes = lonSeconds / 60;
        lonSeconds %= 60;
        return Math.Abs(lonDegrees) + "Â° " + lonMinutes + "' " + lonSeconds + "\" " + (longitude >= 0 ? "E" : "W");
    }

    public static string PrintTimeLong(double time)
    {
        return dateTimeFormatter.PrintTimeLong(time);
    }

    public static string PrintTimeStamp(double time, bool days = false, bool years = false)
    {
        return dateTimeFormatter.PrintTimeStamp(time, days, years);
    }

    public static string PrintTimeStampCompact(double time, bool days = false, bool years = false)
    {
        return dateTimeFormatter.PrintTimeStampCompact(time, days, years);
    }

    public static string PrintTime(double time, int valuesOfInterest, bool explicitPositive)
    {
        return dateTimeFormatter.PrintTime(time, valuesOfInterest, explicitPositive);
    }

    public static string PrintTimeCompact(double time, bool explicitPositive)
    {
        return dateTimeFormatter.PrintTimeCompact(time, explicitPositive);
    }

    public static string PrintDateDelta(double time, bool includeTime, bool includeSeconds = false, bool useAbs = false)
    {
        return dateTimeFormatter.PrintDateDelta(time, includeTime, includeSeconds, useAbs);
    }

    public static string PrintDateDeltaCompact(double time, bool includeTime, bool includeSeconds, bool useAbs = false)
    {
        return dateTimeFormatter.PrintDateDeltaCompact(time, includeTime, includeSeconds, useAbs);
    }

    public static string PrintDate(double time, bool includeTime, bool includeSeconds = false)
    {
        return dateTimeFormatter.PrintDate(time, includeTime, includeSeconds);
    }

    public static string PrintDateNew(double time, bool includeTime)
    {
        return dateTimeFormatter.PrintDateNew(time, includeTime);
    }

    public static string PrintDateCompact(double time, bool includeTime, bool includeSeconds = false)
    {
        return dateTimeFormatter.PrintDateCompact(time, includeTime, includeSeconds);
    }

    /// <summary>
    /// returns a string with an auto-generated pretty name for a module, given its class name-
    /// </summary>
    /// <returns></returns>
    public static string PrintModuleName(string moduleName)
    {
        string name = moduleName;

        if (name.StartsWith("Module"))
        {
            name = moduleName.Remove(0, ("Module").Length);
        }

        for (int i = 0; i < name.Length; ++i)
        {
            if (char.IsUpper(name[i]) && i != 0 && !char.IsUpper(name[i - 1]))
            {
                name = name.Insert(i, " ");
                i++;
            }
        }

        return name;
    }

    public static string PrintSpacedStringFromCamelcase(string s)
    {
        if (string.IsNullOrEmpty(s))
            return string.Empty;
        string name = s;
        for (int i = 0; i < name.Length; i++)
        {
            if (i == 0 && !char.IsUpper(name[i]))
            {
                name = char.ToUpper(s[0]) + s.Substring(1);
            }
            else if (i != 0 && char.IsUpper(name[i]) && !char.IsUpper(name[i - 1]))
            {
                name = name.Insert(i, " ");
                i++;
            }
        }
        return name;
    }


    /// <summary>
    /// Returns a filepath string that points to the game's root folder (regardless of platform)
    /// </summary>
    public static string ApplicationRootPath
    {
        get
        {
            return Application.platform == RuntimePlatform.OSXPlayer ? Application.dataPath + "/../../" : Application.dataPath + "/../";
        }
    }

    /// <summary>
    /// Returns a filepath that points to the given folders inside the game's root folder (will create it if one isn't found)
    /// </summary>
    /// <param name="relPath">the path, relative to the game's .exe location</param>
    /// <returns>the full filepath</returns>
    public static string GetOrCreatePath(string relPath)
    {
        if (!Directory.Exists(ApplicationRootPath + relPath))
            Directory.CreateDirectory(ApplicationRootPath + relPath);

        return ApplicationRootPath + relPath;
    }



    public static string GetTransformPathToRoot(Transform t, Transform root)
    {
        // error checking
        if (!t)
        {
            Debug.LogError("KSPUtil.GetTransformPathToRoot() passed null t!");
            return "";
        }

        if (t == root)
        {
            return "";
        }
        //if (!root)
        //{
        //    Debug.LogError("KSPUtil.GetTransformPathToRoot() passed null root!");
        //    return "";
        //}

        // the path to return, beginning with t Transform
        string path = t.name;

        // the first parent
        Transform c_parent = t.parent;

        // go through each parent up the tree until we reach the specified root
        while (c_parent != null && c_parent != root)
        {
            path = path.Insert(0, c_parent.name + "/");
            c_parent = c_parent.parent;
        }

        // return the path
        return path;
    }

    public static string GetTransformIndexPathToRoot(Transform t, Transform root)
    {
        // error checking
        if (!t)
        {
            Debug.LogError("KSPUtil.GetTransformPathToRoot() passed null t!");
            return "";
        }

        if (t == root)
        {
            return "";
        }

        //if (!root)
        //{
        //    Debug.LogError("KSPUtil.GetTransformPathToRoot() passed null root!");
        //    return "";
        //}

        // the path to return, beginning with t Transform
        string path = t.GetSiblingIndex().ToString();

        // the first parent
        Transform c_parent = t.parent;

        // go through each parent up the tree until we reach the specified root
        while (c_parent != null && c_parent != root)
        {
            path = path.Insert(0, c_parent.GetSiblingIndex().ToString() + "/");
            c_parent = c_parent.parent;
        }

        // return the path
        return path;
    }

    public static Transform FindTransformAtIndexPath(string indexPath, Transform root)
    {
        if (string.IsNullOrEmpty(indexPath))
        {
            return root;
        }

        int tIndex;
        Transform t;
        string rPath;
        if (indexPath.Contains('/'))
        {
            string[] pathData = indexPath.Split('/');
            tIndex = int.Parse(pathData[0]);
            rPath = KSPUtil.PrintCollection(pathData.Skip(1), "/");


            t = root.GetChild(tIndex);
            return FindTransformAtIndexPath(rPath, t);
        }
        else
        {
            tIndex = int.Parse(indexPath);
            return root.GetChild(tIndex);
        }


    }

    public static string StripFileExtension(FileInfo file)
    {
        return file.Name.Substring(0, file.Name.Length - file.Extension.Length);
    }

    /// <summary>
    /// Strips out nasty characters for mac/pc filename creation
    /// </summary>
    /// <param name="original">The original string</param>
    /// <param name="replacementChar">The character to use to replace nasty ones</param>
    /// <returns></returns>
    public static string SanitizeString(string originalString, char replacementChar, bool replaceEmpty)
    {
        originalString = originalString.Replace('\\', replacementChar);
        originalString = originalString.Replace('/', replacementChar);
        originalString = originalString.Replace('.', replacementChar);
        originalString = originalString.Replace(':', replacementChar);
        originalString = originalString.Replace('|', replacementChar);
        originalString = originalString.Replace('*', replacementChar);
        originalString = originalString.Replace('?', replacementChar);

        originalString = originalString.Replace('{', replacementChar);
        originalString = originalString.Replace('}', replacementChar);

        originalString = originalString.Replace('<', replacementChar);
        originalString = originalString.Replace('>', replacementChar);
        originalString = originalString.Replace('\"', replacementChar);

        originalString = originalString.Trim();

        if (replaceEmpty && originalString == "")
            originalString = "Unnamed";

        return originalString;
    }


    /// <summary>
    /// Strips out nasty characters for mac/pc filename creation
    /// </summary>
    /// <param name="originalFilename"></param>
    /// <returns></returns>
    public static string SanitizeFilename(string originalFilename)
    {
        return SanitizeString(originalFilename, '_', true);
    }




    public static VersionCompareResult CheckVersion(string versionString, int lastMajor, int lastMinor, int lastRev)
    {
        string[] versionData = versionString.Split('.');

        if (versionData.Length != 3) return VersionCompareResult.INVALID;

        int version_major = int.Parse(versionData[0]);
        int version_minor = int.Parse(versionData[1]);
        int version_revision = int.Parse(versionData[2]);

        return CheckVersion(version_major, version_minor, version_revision, lastMajor, lastMinor, lastRev);

    }
    public static VersionCompareResult CheckVersion(int version_major, int version_minor, int version_revision, int lastMajor, int lastMinor, int lastRev)
    {
        bool compatible = false;

        // allow for backward compatibility up to the last backward-compatible version
        if (version_major > lastMajor)
        {
            compatible = true;
        }
        else if (version_major == lastMajor)
        {
            if (version_minor > lastMinor)
            {
                compatible = true;
            }
            else if (version_minor == lastMinor)
            {
                if (version_revision >= lastRev)
                    compatible = true;
            }
        }

        // don't allow files of later versions to be loaded on earlier ones.
        if (version_major > Versioning.version_major ||
            (version_major == Versioning.version_major &&
                version_minor > Versioning.version_minor) ||
            (version_major == Versioning.version_major &&
                version_minor == Versioning.version_minor &&
                version_revision > Versioning.Revision))
        {
            return VersionCompareResult.INCOMPATIBLE_TOO_LATE;
        }

        if (compatible) return VersionCompareResult.COMPATIBLE;
        else return VersionCompareResult.INCOMPATIBLE_TOO_EARLY;
    }

    /// <summary>
    /// Returns true if the src transform is a hierarchical descendant of the ancestor transform
    /// </summary>
    public static bool HasAncestorTransform(Transform src, Transform ancestor)
    {
        if (ancestor == src) return true;
        else if (src.parent != null) return HasAncestorTransform(src.parent, ancestor);
        else return false;
    }

    /// <summary>
    /// Returns true if the src transform has the child transform as a descendant on its hierarchy
    /// </summary>
    public static bool HasDescendantTransform(Transform src, Transform child)
    {
        return HasAncestorTransform(child, src);
    }

    /// <summary>
    /// Takes a Rect r and moves it inside the screen space
    /// </summary>
    public static Rect ClampRectToScreen(Rect r)
    {
        r.x = Mathf.Clamp(r.x, 0, Screen.width - r.width);
        r.y = Mathf.Clamp(r.y, 0, Screen.height - r.height);
        return r;
    }

    [System.Serializable]
    public class StringReplacement
    {
        public string badString;
        public string replacement;
    }

    public static string ReplaceString(string src, params StringReplacement[] replacements)
    {
        foreach (StringReplacement sr in replacements)
        {
            src = src.Replace(sr.badString, sr.replacement);
        }

        return src;

    }



    public static List<T> FindComponentsImplementing<T>(GameObject go, bool returnInactive)
    {
        return go.GetComponents<MonoBehaviour>().Where(mb => mb is T && (returnInactive || mb.enabled)).Cast<T>().ToList();
    }



    /// <summary>
    /// Returns an angle from 0 to 2PI between the two vectors, based on the up axis to tell left from right
    /// </summary>
    /// <param name="v1"></param>
    /// <param name="v2"></param>
    /// <param name="upAxis"></param>
    /// <returns></returns>
    public static float HeadingRadians(Vector3 v1, Vector3 v2, Vector3 upAxis)
    {
        if (v1 == v2)
        {
            return 0f;
        }
        return UtilMath.WrapAround(Mathf.Acos(Vector3.Dot(v1, v2)) * Mathf.Sign(Vector3.Dot(Vector3.Cross(v1, v2), upAxis)), 0f, Mathf.PI * 2.0f);
    }


    /// <summary>
    /// Returns an angle from 0 to 360Â° between the two vectors, based on the up axis to tell left from right
    /// </summary>
    /// <param name="v1"></param>
    /// <param name="v2"></param>
    /// <param name="upAxis"></param>
    /// <returns></returns>
    public static float HeadingDegrees(Vector3 v1, Vector3 v2, Vector3 upAxis)
    {
        if (v1 == v2)
        {
            return 0f;
        }
        return UtilMath.WrapAround(Mathf.Acos(Vector3.Dot(v1, v2)) * Mathf.Sign(Vector3.Dot(Vector3.Cross(v1, v2), upAxis)) * Mathf.Rad2Deg, 0f, 360f);
    }


    /// <summary>
    /// Returns an angle from -PI to PI between the two vectors, based on the up axis to tell left from right
    /// </summary>
    /// <param name="v1"></param>
    /// <param name="v2"></param>
    /// <param name="upAxis"></param>
    /// <returns></returns>
    public static float BearingRadians(Vector3 v1, Vector3 v2, Vector3 upAxis)
    {
        if (v1 == v2)
        {
            return 0f;
        }
        return Mathf.Acos(Vector3.Dot(v1, v2)) * Mathf.Sign(Vector3.Dot(Vector3.Cross(v1, v2), upAxis));
    }


    /// <summary>
    /// Returns an angle from -180Â° to 180Â° between the two vectors, based on the up axis to tell left from right
    /// </summary>
    /// <param name="v1"></param>
    /// <param name="v2"></param>
    /// <param name="upAxis"></param>
    /// <returns></returns>
    public static float BearingDegrees(Vector3 v1, Vector3 v2, Vector3 upAxis)
    {
        if (v1 == v2)
        {
            return 0f;
        }
        return Mathf.Acos(Vector3.Dot(v1, v2)) * Mathf.Sign(Vector3.Dot(Vector3.Cross(v1, v2), upAxis)) * Mathf.Rad2Deg;
    }


    /// <summary>
    /// Produces a string combining the string values (given by .ToString()) of all the elements in a collection
    /// </summary>
    public static string PrintCollection<T>(IEnumerable<T> collection, string separator = ", ")
    {
        return PrintCollection(collection, separator, c => c.ToString());
    }

    /// <summary>
    /// Produces a string combining the string values (given by stringAccessor) of all the elements in a collection
    /// </summary>
    public static string PrintCollection<T>(IEnumerable<T> collection, string separator, Func<T, string> stringAccessor)
    {
        string output = "";
        IEnumerator<T> enumr = collection.GetEnumerator();
        int count = collection.Count();

        for (int i = 0; i < count; i++)
        {
            enumr.MoveNext();
            output += stringAccessor(enumr.Current);

            if (i != count - 1)
            {
                output += separator;
            }
        }

        return output;
    }

    /// <summary>
    /// Appends a new string to the given source string if source string does not already contain it. Does not modify input values.
    /// </summary>
    /// <param name="s0">The source string</param>
    /// <param name="val">The new string to append</param>
    /// <param name="separator">A separator char to separate the appended section from the src string</param>
    /// <returns>The resulting string</returns>
    public static string AppendValueToString(string s0, string val, char separator)
    {
        string s = string.Copy(s0);

        if (string.IsNullOrEmpty(s0))
        {
            s = val;
        }
        else if (!s.Contains(val))
        {
            s += separator + " " + val;
        }

        return s;
    }

    /// <summary>
    /// Find all tags contained in the sub-object hierarchy
    /// </summary>
    /// <param name="tags">A non-null list of strings to hold the tags</param>
    /// <param name="trf">The transform to start recursing from</param>
    public static void FindTagsInChildren(List<string> tags, Transform trf)
    {
        tags.AddUnique(trf.gameObject.tag);
        foreach (Transform t in trf)
        {
            FindTagsInChildren(tags, t);
        }
    }

    public static string AddSpacesOnCaps(this string str)
    {
        for (int i = 1; i < str.Length; ++i)
        {
            if (char.IsUpper(str[i]))
            {
                str = str.Insert(i, " ");
                ++i;
            }
        }

        return str;
    }


    /// <summary>
    /// Strips out nas


}

public enum VersionCompareResult
{
    INCOMPATIBLE_TOO_EARLY,
    COMPATIBLE,
    INCOMPATIBLE_TOO_LATE,
    INVALID
}

public delegate T ParserMethod<T>(string value);
