using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace sselFinOps.AppCode
{
    public enum DateFormat
    {
        RCF3339 = 1
    }

    public static class DateUtility
    {
        public static string Format(DateTime date, DateFormat format)
        {
            switch (format)
            {
                case DateFormat.RCF3339:
                    TimeSpan span = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now);
                    string tzoffset = string.Format("{0}{1:hh':'mm}", (span < TimeSpan.Zero) ? "-" : "", span);
                    string datestr = string.Format("{0:M/d/yyyy HH:mm:ss}{1}", date, tzoffset);
                    return DateTime.Parse(datestr).ToString("yyyy-MM-dd'T'HH:mm:ssK");
                default:
                    throw new ArgumentException("format");
            }
        }

        public static DateTime Parse(string value, string format)
        {
            return DateTime.ParseExact(value, format, CultureInfo.InvariantCulture);
        }
    }
}
