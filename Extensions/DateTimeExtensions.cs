using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FileManage.Extensions.DateTimeEx
{
    /// <summary>
    /// 数据操作
    /// </summary>
    public static class DateTimeHelper
    {
        /// <summary>
        /// 单位微秒
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static long ToTimestampMilliseconds(DateTime value)
        {
            TimeSpan span = (value - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime());
            return (long)span.TotalMilliseconds;
        }
        /// <summary>
        /// 单位微秒
        /// </summary>
        /// <returns></returns>
        public static long GetTimeStampMilliseconds()
        {
            DateTime startTime = new System.DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime();
            DateTime nowTime = DateTime.Now;
            long unixTime = (long)System.Math.Round((nowTime - startTime).TotalMilliseconds, MidpointRounding.AwayFromZero);
            return unixTime;
        }
        /// <summary>
        /// 单位微秒
        /// </summary>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        public static DateTime ConvertTimestampMilliseconds(long timestamp)
        {
            DateTime converted = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            DateTime newDateTime = converted.AddMilliseconds(timestamp);
            return newDateTime.ToLocalTime();
        }


        /// <summary>
        /// 单位秒
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static long ToTimestampSeconds(DateTime value)
        {
            TimeSpan span = (value - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime());
            return (long)span.TotalSeconds;
        }
        /// <summary>
        /// 单位秒
        /// </summary>
        /// <returns></returns>
        public static long GetTimeStampSeconds()
        {
            DateTime startTime = new System.DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime();
            DateTime nowTime = DateTime.Now;
            long unixTime = (long)System.Math.Round((nowTime - startTime).TotalSeconds, MidpointRounding.AwayFromZero);
            return unixTime;
        }
        /// <summary>
        /// 单位秒
        /// </summary>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        public static DateTime ConvertTimestampSeconds(long timestamp)
        {
            DateTime converted = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            DateTime newDateTime = converted.AddSeconds(timestamp);
            return newDateTime.ToLocalTime();
        }
    }
}
