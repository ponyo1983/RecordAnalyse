using System;
using System.Collections.Generic;
using System.Text;

namespace Common
{
   public class StationCurve
    {

        float[] pts;



        public StationCurve(DateTime time, int sampleRate, float[] data)
        {
            this.OccurTime = time;
            this.SampleRate = sampleRate;
            this.pts = data;
        }

        public DateTime OccurTime
        {
            get;
            private set;
        }

        public float[] Points
        {
            get
            {
                return pts;
            }
        }

        public byte Dir
        {
            get;
            private set;
        }

        public int SampleRate
        {
            get;
            private set;
        }
    }
}
