﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResourceShared.Utils
{
    public class DateTimeHelper
    {
        public static long ConvertToUnixTimestamp(DateTime dateTime)
        {
            DateTimeOffset dateTimeOffset = new DateTimeOffset(dateTime);
            return dateTimeOffset.ToUnixTimeSeconds();
        }
        public static long GetDateTimeNowInUnix()
        {
            DateTimeOffset dateTimeOffset = new DateTimeOffset(DateTime.Now);
            return dateTimeOffset.ToUnixTimeSeconds();
        }
    }
}
