using System;
using System.Collections.Generic;
using System.Text;

namespace DataStorage
{
    class Utility
    {
        public static readonly DateTime TimeBase = new DateTime(1970, 1, 1, 8, 0, 0);
        public static readonly DateTime TimeMAX = TimeBase.AddSeconds(UInt32.MaxValue);
        public static DateTime Unix2DateTime(uint unixTime)
        {
            return TimeBase.AddSeconds(unixTime);
        }
        public static UInt32 DateTime2Unix(DateTime time)
        {
            if (time <= TimeBase) return 0;
            if (time >= TimeMAX) return UInt32.MaxValue;
            TimeSpan ts = time - TimeBase;
            return (uint)ts.TotalSeconds;
        }
    }
}
