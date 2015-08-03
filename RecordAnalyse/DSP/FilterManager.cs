using System;
using System.Collections.Generic;
using System.Text;

namespace RecordAnalyse.DSP
{
    class FilterManager
    {
        static FilterManager singleton;
        public static FilterManager Instance
        {
            get
            {
                if (singleton == null) { singleton = new FilterManager(); }
                return singleton;
            }

        }
        private FilterManager() { }


        public IFilter CreateLowpassFilter()
        {
            return new ButterFilter();
        }

        public IFilter CreateBandpassFilter(float Fc)
        {
            return new BandpassFilter(Fc);
        }

        class ButterFilter : IFilter
        {


            static readonly float[] A = new float[] { 1, -2.686157396548f, 2.419655110966f, -0.7301653453057f }; //第一个数值始终为1
            static readonly float[] B = new float[] { 0.0004165461390757f, 0.001249638417227f, 0.001249638417227f, 0.0004165461390757f };

            #region IFilter 成员

            public float[] Process(float[] origSignal, int offset, int length)
            {
                if (origSignal == null || origSignal.Length <= 0) return null;

                //int length = origSignal.Length;

                int order = A.Length - 1;
                int paddingLenth = length + order; //补零
                float[] xData = new float[paddingLenth];
                float[] yData = new float[paddingLenth];
                Array.Clear(xData, 0, order);
                Array.Copy(origSignal, 0, xData, order, length);
                Array.Clear(yData, 0, paddingLenth);

                for (int i = order; i < paddingLenth; i++)
                {
                    for (int j = 0; j <= order; j++)
                    {
                        yData[i] += B[j] * xData[i - j];
                        if (j > 0)
                        {
                            yData[i] -= A[j] * yData[i - j];
                        }
                    }
                }
                float[] filterSignal = new float[length];
                Array.Copy(yData, order, filterSignal, 0, length);
                return filterSignal;
            }

            #endregion
        }

        /// <summary>
        /// 这个滤波器是按8000HZ的采样率进行设计的
        /// </summary>
        class BandpassFilter : IFilter
        {


            int filterIndex = -1; //选择的滤波器
            public const int Order = 6; //6阶

            float[] orderX = new float[Order];
            float[] orderY = new float[Order];

            float[] xData = new float[Order];
            float[] yData = new float[Order];

            public static readonly float[][] A = new float[][] { 
                new float[]{1,   -1.270195723f,    2.914719343f,   -2.111337185f,    2.366715431f,   -0.8332899213f,   0.5320753455f}, //1700
                new float[]{1,-6.621495835e-16f,    2.374094725f,-1.356818985e-15f,    1.929355621f, -6.430251148e-16f,   0.5320753455f}, //2000
                new float[]{1,    1.270195723f,    2.914719343f,    2.111337185f,    2.366715431f,    0.8332899213f,   0.5320753455f}, //2300
                new float[]{ 1,      2.4702003f,    4.418744087f,     4.52286768f,    3.583455563f,     1.620532036f,   0.5320753455f}, //2600

                new float[]{ 1,  -5.310338974f,     12.2460556f,   -15.62362957f,    11.62108135f,     -4.782167912f,   0.8546014428f}, //550
                new float[]{1,   -5.101892948f,     11.5223484f,   -14.59983158f,    10.93431854f,     -4.594454765f,   0.8546014428f},//650
                new float[]{1,   -4.861992359f,    10.72529697f,   -13.48244953f,    10.17795658f,     -4.378414631f,   0.8546014428f},//750
                new float[]{1,   -4.592116356f,    9.874526978f,   -12.29969311f,    9.370617867f,     -4.135380745f,   0.8546014428f}, //850
            };

            public static readonly float[][] B = new float[][] { 
                new float[]{0.002898194594f, 0, -0.00869458355f,  0,  0.00869458355f,  0,-0.002898194594f},
                new float[]{0.002898194594f, 0, -0.00869458355f,  0,  0.00869458355f,  0,-0.002898194594f},
                new float[]{0.002898194594f, 0, -0.00869458355f,  0,  0.00869458355f,  0,-0.002898194594f},
                new float[]{0.002898194594f, 0, -0.00869458355f,  0,  0.00869458355f,  0,-0.002898194594f},

                new float[]{5.607011553e-05f,              0,-0.0001682103466f,              0,0.0001682103466f,                0,-5.607011553e-05f},
                new float[]{5.607011553e-05f,              0,-0.0001682103466f,              0,0.0001682103466f,                0,-5.607011553e-05f},
                new float[]{5.607011553e-05f,              0,-0.0001682103466f,              0,0.0001682103466f,                0,-5.607011553e-05f},
                new float[]{5.607011553e-05f,              0,-0.0001682103466f,              0,0.0001682103466f,                0,-5.607011553e-05f},
            };

            public static readonly int[] freqCenter = new int[] { 1700, 2000, 2300, 2600, 550, 650, 750, 850 };
            /// <summary>
            /// 中心频率
            /// </summary>
            /// <param name="freqCenter"></param>
            public BandpassFilter(float Fc)
            {
                for (int i = 0; i < freqCenter.Length; i++)
                {
                    if (Math.Abs(Fc - freqCenter[i]) < 0.1f)
                    {
                        filterIndex = i;
                        break;
                    }
                }
            }

            #region IFilter 成员

            public float[] Process(float[] origSignal, int offset, int length)
            {

                float[] filter = new float[length];
                if (filterIndex >= 0) //存在适当的滤波器
                {
                    //需要重新扩展
                    int paddingLenth = Order + length;
                    if (xData.Length < paddingLenth)
                    {
                        Array.Copy(xData, 0, orderX, 0, Order);
                        Array.Copy(yData, 0, orderY, 0, Order);
                        xData = new float[paddingLenth];
                        yData = new float[paddingLenth];

                        Array.Copy(orderX, 0, xData, 0, Order);
                        Array.Copy(orderY, 0, yData, 0, Order);
                    }


                    Array.Copy(origSignal, offset, xData, Order, length);
                    for (int i = Order; i < paddingLenth; i++)
                    {
                        yData[i] = 0;
                        for (int j = 0; j <= Order; j++)
                        {
                            yData[i] += B[filterIndex][j] * xData[i - j];
                            if (j > 0)
                            {
                                yData[i] -= A[filterIndex][j] * yData[i - j];
                            }
                        }
                    }
                    Array.Copy(yData, Order, filter, 0, length);
                    for (int i = 0; i < Order; i++)
                    {
                        xData[i] = xData[length + i];
                        yData[i] = yData[length + i];
                    }

                }
                else
                {
                    Array.Copy(origSignal, offset, filter, 0, length); //不进行滤波，数据原样返回
                }




                return filter;

            }

            #endregion
        }



    }
}
