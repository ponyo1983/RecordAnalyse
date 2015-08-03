using System;
using System.Collections.Generic;
using System.Text;

namespace UltraChart
{
    public class XAxes
    {
        public long OriginalTime
        {
            get;
            set;
        }

        public long TimeScale
        {
            get;
            set;
        }

        public XAxes()
        {
            this.TimeScale = 1000000L;
        }

        public bool GetOrgTime(ref long xOrgTime, ref int timeSegment)
        {
            xOrgTime = this.OriginalTime;
            timeSegment = (int)this.TimeScale;
            return true;
        }

        public long GetScaleTimeLen(int scaleIndex)
        {
            return 0;
        }

        public bool GetTimeSegment(int segmentIndex, ref long beginTime, ref long endTime)
        {
            return true;
        }

        public int GetTimeSegmentCount()
        {
            return 0;
        }

        public bool SetOrgTime(long xOrgTime, int timeSegment)
        {
            this.OriginalTime = xOrgTime;
            return true;
        }

        public bool SetOrgTime(DateTime time)
        {
            this.SetOrgTime(ChartGraph.DateTime2ChartTime(time), 0);
            return true;
        }
        public int SetScale(long scale)
        {
            this.TimeScale = scale;
            return 0;
        }

        public int CurScaleIndex
        {
            get;
            set;
        }

        public long DisTimeLen
        {
            get;
            set;
        }

     

        public long MaxScale
        {
            get;
            set;
        }

        public long MinScale
        {
            get;
            set;
        }

        public int ScaleCount
        {
            private set;
            get;
        }

        public XAxesMode XAxesMode
        {
            get;
            set;
        }
    }
}
