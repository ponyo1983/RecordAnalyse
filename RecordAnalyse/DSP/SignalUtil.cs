using System;
using System.Collections.Generic;
using System.Text;

namespace RecordAnalyse.DSP
{
    class SignalUtil
    {
        public float CalDCAmpl(float[] data, int offset, int cnt)
        {
            float sum = 0;
            for (int i = 0; i < cnt; i++)
            {
                sum += data[offset + i];
            }
            return sum / cnt;
        }

        public float CalACAmpl(float[] data, int offset, int cnt)
        {
            float dcAmpl = CalDCAmpl(data, offset, cnt);
            float sum = 0;
            for (int i = 0; i < cnt; i++)
            {
                sum += ((data[offset + i] - dcAmpl) * (data[offset + i] - dcAmpl));
            }

            return (float)Math.Sqrt(sum / cnt);

        }
    }
}
