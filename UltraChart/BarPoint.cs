using System;
using System.Collections.Generic;
using System.Text;

namespace UltraChart
{
    public struct BarPoint
    {
        private int value;
        private long time;
        private int type;
        private int id;
        public BarPoint(long time,int value)
        {
            this.value = value;
            this.time = time;
            this.type = 0;
            this.id = 0;
        }

        public int Value
        {
            get { return value; }
            set { this.value = value; }
        }
        public long Time
        {
            get { return time; }
            set { this.time = value; }
        }

        public int PointType
        {
            get { return type; }
            set { this.type = value; }
        }
        public int RecordId
        {
            get { return id; }
            set { this.id = value; }
        }
    }
}
