using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace RecordAnalyse.Signal
{
    public class ADBlock
    {

        Queue<byte> queueAd = new Queue<byte>();


        byte[] dataSample;

        AutoResetEvent adEvent = new AutoResetEvent(false);


        int slideNum = 0;

        public ADBlock(int window, int slide)
        {
            this.Window = window;
            this.Slide = slide;
            this.dataSample = new byte[window];
        }

        public ADBlock()
        {
        }

        public int Window
        {
            get;
            private set;
        }

        public int Slide
        {
            get;
            private set;
        }



        public void PutAdData(byte[] data, int offset, int length)
        {

            while (slideNum > 0)
            {
                if (queueAd.Count <= 0) break;

                queueAd.Dequeue();

                slideNum--;
            }
            for (int i = 0; i < length; i++)
            {
                if (slideNum > 0)
                {
                    slideNum--;

                }
                else
                {
                    queueAd.Enqueue(data[offset + i]);
                    if (queueAd.Count == Window)
                    {
                        dataSample = queueAd.ToArray();

                        adEvent.Set();

                        slideNum = this.Slide;
                    }
                }
            }
        }

        public float[] GetAdData(int timeout)
        {
            if (timeout == 0)
            {
                byte[] ad= (byte[])dataSample.Clone();
                float[] data=new float[ad.Length];
                for (int i = 0; i < ad.Length; i++)
                {
                    data[i] = ad[i];
                }

                return data;
            }
            if (timeout > 0)
            {
                if (adEvent.WaitOne(timeout, false) == false)
                {
                    return null;
                }
            }
            else
            {
                adEvent.WaitOne();
            }
            return GetAdData(0);
        }


    }
}
