using AndreasReitberger.API.Print3dServer.Core.Enums;
using System;

namespace AndreasReitberger.API.Print3dServer.Core.Utilities
{
    public static class TimeBaseConvertHelper
    {
        #region Methods

        public static TimeSpan? ConvertFrom<T>(GcodeTimeBaseTarget target, T value, bool withMiliSeconds = false)
        {
            TimeSpan? ts = target switch
            {
                GcodeTimeBaseTarget.DoubleHoursUnix or GcodeTimeBaseTarget.DoubleHours => (TimeSpan?)FromUnixDoubleHours(value as double?, withMiliSeconds),
                GcodeTimeBaseTarget.DoubleSeconds or GcodeTimeBaseTarget.DoubleSecondsUnix => (TimeSpan?)FromDoubleSeconds(value as double?, withMiliSeconds),
                GcodeTimeBaseTarget.LongSeconds or GcodeTimeBaseTarget.LongSecondsUnix => (TimeSpan?)FromLongSeconds(value as long?, withMiliSeconds),
                _ => (TimeSpan?)TimeSpan.Zero,
            };
            return ts;
        }

        public static TimeSpan FromUnixDoubleHours(double? hours, bool withMiliSeconds = false)
        {
            try
            {
                TimeSpan ts = TimeSpan.FromHours(Convert.ToDouble(hours));
                if (!withMiliSeconds)
                    ts = new TimeSpan(ts.Days, ts.Hours, ts.Minutes, ts.Seconds);
                return ts;
            }
            catch (Exception)
            {
                return TimeSpan.Zero;
            }
        }

        public static TimeSpan FromDoubleHours(double? hours, bool withMiliSeconds = false) => FromUnixDoubleHours(hours, withMiliSeconds);

        public static TimeSpan FromDoubleSeconds(double? seconds, bool withMiliSeconds = false)
        {
            try
            {
                TimeSpan ts = TimeSpan.FromSeconds(Convert.ToDouble(seconds));
                if (!withMiliSeconds)
                    ts = new TimeSpan(ts.Days, ts.Hours, ts.Minutes, ts.Seconds);
                
                return ts;
            }
            catch (Exception)
            {
                return TimeSpan.Zero;
            }
        }
        public static TimeSpan FromLongSeconds(long? seconds, bool withMiliSeconds = false) => FromDoubleSeconds(Convert.ToDouble(seconds), withMiliSeconds);
        public static TimeSpan FromLongSeconds(double? seconds, bool withMiliSeconds = false) => FromDoubleSeconds(seconds, withMiliSeconds);

        public static DateTime FromUnixDoubleHoursToNow(double? hours, bool withMiliSeconds = false)
        {
            try
            {
                TimeSpan ts = TimeSpan.FromSeconds(Convert.ToDouble(hours));
                if(!withMiliSeconds)
                    ts = new TimeSpan(ts.Days, ts.Hours, ts.Minutes, ts.Seconds);
                DateTime eta = DateTime.Now.Add(ts);
                return eta;
            }
            catch (Exception)
            {
                return DateTime.Now;
            }
        }

        public static DateTime FromDoubleSecondsToNow(double? seconds, bool withMiliSeconds = false)
        {
            try
            {
                TimeSpan ts = TimeSpan.FromSeconds(Convert.ToDouble(seconds));
                if (!withMiliSeconds)               
                    ts = new TimeSpan(ts.Days, ts.Hours, ts.Minutes, ts.Seconds);         
                DateTime eta = DateTime.Now.Add(ts);
                return eta;
            }
            catch (Exception)
            {
                return DateTime.Now;
            }
        }

        public static DateTime FromUnixDate(double? unixDate)
        {
            try
            {
                DateTime dt = DateTime.MinValue;
                TimeSpan ts = TimeSpan.FromSeconds(Convert.ToDouble(unixDate));
                dt = new DateTime(1970, 1, 1, 0, 0, 0, 0).Add(ts).ToLocalTime();

                return dt;
            }
            catch (Exception)
            {
                return DateTime.MinValue;
            }
        }
        public static DateTime FromUnixDoubleMiliseconds(double? miliSeconds)
        {
            try
            {
                DateTime dt = DateTime.MinValue;
                TimeSpan ts = TimeSpan.FromMilliseconds(Convert.ToInt64(miliSeconds));
                dt = new DateTime(1970, 1, 1, 0, 0, 0, 0).Add(ts).ToLocalTime();

                return dt;
            }
            catch (Exception)
            {
                return DateTime.MinValue;
            }
        }

        public static DateTime FromDouble(double? value)
        {
            try
            {
                DateTime dt = DateTime.MinValue;
                TimeSpan ts = TimeSpan.FromMilliseconds(Convert.ToDouble(value));
                dt = new DateTime(1970, 1, 1, 0, 0, 0, 0).Add(ts);

                return dt;
            }
            catch (Exception)
            {
                return DateTime.MinValue;
            }
        }

        public static DateTime FromLong(long? ms)
        {
            try
            {
                DateTime dt = DateTime.MinValue;
                TimeSpan ts = TimeSpan.FromMilliseconds(Convert.ToDouble(ms));
                dt = new DateTime(1970, 1, 1, 0, 0, 0, 0).Add(ts);

                return dt;
            }
            catch (Exception)
            {
                return DateTime.MinValue;
            }
        }
        public static DateTime FromInt(int? ms) => FromLong(ms);

        #endregion
    }
}
