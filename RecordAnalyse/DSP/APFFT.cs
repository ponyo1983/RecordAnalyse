using System;
using System.Collections.Generic;
using System.Text;

namespace RecordAnalyse.DSP
{
    class APFFT
    {

        double[] hanning;
        double[] convHann;

        FFTPlan fftPlan;
        DSPUtil util;
        double[] dotData;
        float[] fftData1;
        float[] fftData2;

        float[] peakVal=new float[2];
        int[] peakIndex=new int[2];
        public APFFT(int N)
        {
            hanning=new double[N];
            for (int i = 0; i < N; i++)
            {
                hanning[i] = 0.5 - 0.5 * Math.Cos(2 * (i+1) * Math.PI / N);
            }
            convHann=new double[2*N-1];

            for (int i = 0; i < 2 * N - 1; i++)
            {
                double sum = 0;

                for (int j = 0; j < N; j++)
                {
                    int index = i - j;
                    if (index >= 0 && index < N)
                    {
                        sum += hanning[j] * hanning[index];
                    }
                }
                convHann[i] = sum;

            }

            double sumHann = 0;

            for (int i = 0; i < 2 * N - 1; i++)
            {
                sumHann += convHann[i];
            }


            for (int i = 0; i < 2 * N - 1; i++)
            {
                convHann[i] = convHann[i] / sumHann;
            }

            dotData = new double[2 * N - 1];

            fftPlan = new FFTPlan(N);
            this.NUM = N;
            fftData1 = new float[N * 2];
            fftData2 = new float[N * 2];
            util = new DSPUtil();
        }

        public int NUM
        {
            get;
            private set;
        }

        private float Angle(float r, float i)
        {
            double angle = 0;
            if (r > 0 && i >= 0)
            {
                angle = Math.Atan(i / r);
            }
            else if (r < 0 && i >= 0)
            {
                angle = Math.PI - Math.Atan(i / -r);
            }
            else if (r > 0 && i < 0)
            {
                angle = 2 * Math.PI - Math.Atan(-i / r);
            }
            else if (r < 0 && i < 0)
            {
                angle = Math.PI + Math.Atan(-i / -r);
            }
            return (float)angle;

        }


        public float CalAngle(float[] data)
        {
            if (data == null || data.Length < NUM * 2) return 0;

            for (int i = 0; i < 2 * NUM - 1; i++)
            {
                dotData[i] = data[i] * convHann[i];
            }


            for (int i = 0; i < NUM; i++)
            {
                double tmp = (i > 0 ? dotData[i - 1] : 0) + dotData[NUM - 1 + i];
                fftData1[i * 2] = (float)(tmp); 
                fftData1[i * 2 + 1] = 0;
            }

            fftPlan.FFTForward(fftData1, fftData2);

            util.FindComplexPeaks(fftData2, +5, this.NUM / 2, peakVal, peakIndex); //忽略直流信息

            float angle = 0;
            int index=peakIndex[0]+5;
            if (index> 0)
            {
                angle=Angle(fftData2[index*2],fftData2[index*2+1]);
            }



            return (float)(angle*180/Math.PI);

        }
        
    }
}
