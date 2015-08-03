using System;
using System.Collections.Generic;
using System.Text;

namespace RecordAnalyse.Signal
{
   public class SignalArgs:EventArgs
    {

        float[] curve;


        public SignalArgs(DateTime time, float angleDiff)
        {
            this.Time = time;
            this.AngleDiff = angleDiff;
        }

        public SignalArgs(DateTime time,float[] curve)
        {
            this.Time = time;
            if (curve != null)
            {
                this.curve = (float[])curve.Clone();

            }
        }

        public SignalArgs(DateTime time, float dc, float ac, float carrier, float low)
        {
            this.Time = time;
            this.DCAmpl = dc;
            this.ACAmpl = ac;
            this.CarrierFreq = carrier;
            this.LowFreq = low;
        }


        public float AngleDiff
        {
            get;
            private set;
        }

        public float ACAmpl
        {
            get;
            private set;
        }

        public float DCAmpl
        {
            get;
            private set;
        }

        public float CarrierFreq
        {
            get;
            private set;
        }

        public float LowFreq
        {
            get;
            private set;
        }

        public float[] ACCurve
        {
            get
            {
                return curve;
            }
        }

        public DateTime Time
        {
            get;
            private set;
        }
        

    }
}
