using System;
using System.Collections.Generic;
using System.Text;

namespace UltraChart
{
    public struct LinePoint
    {
        long time;
        float value;
        int id;
        int type;
        
        public LinePoint(long time, float value)
        {
            this.time = time;
            this.value = value;
            this.type = 0;
            this.id = 0;
        }

        public LinePoint(DateTime time, float value)
            : this(ChartGraph.DateTime2ChartTime(time), value)
        { 
        }

        public int PointType
        {
            get { return type; }
            set { type = value; }
        }

        public int RecordId
        {
            get { return id; }
            set { this.id = value; }
        }

        public long Time
        {
            get { return time; }
            set { this.time = value; }
        }
        public long DayAlignTime
        {
            get
            {
                return (this.Time) / LineArea.TicksPerDay * LineArea.TicksPerDay;
            }
        }

        public float Value
        {
            get { return value; }
            set { this.value = value; }
        }
    }
}
