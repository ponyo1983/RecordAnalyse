using System;
using System.Collections.Generic;
using System.Text;

namespace RecordAnalyse.DSP
{
    /// <summary>
    /// 工具类 
    /// </summary>
    class DSPUtil
    {

        #region 滤波系数

        static readonly float[] A = new float[] { 1, -2.686157396548f, 2.419655110966f, -0.7301653453057f }; //A[0]始终为1
        static readonly float[] B = new float[] { 0.0004165461390757f, 0.001249638417227f, 0.001249638417227f, 0.0004165461390757f };

        #endregion


        /// <summary>
        /// 寻找极大值 
        /// </summary>
        /// <param name="ampl">信号幅度</param>
        /// <param name="peaks">极值大小</param>
        /// <param name="index">极值所在位置</param>
        public void FindPeaks(float[] ampl, float[] peaks, int[] index)
        {
            int length = ampl.Length;
            int peakCnt = Math.Min(peaks.Length, index.Length);

            for (int i = 0; i < index.Length; i++)// -1表示没发现极值
            {
                index[i] = -1;
            }
            float peakVal = 0;
            int findPeakCnt = 0;
            for (int i = 1; i < length; i++)
            {
                if (ampl[i] > ampl[i - 1])
                {
                    peakVal = ampl[i];
                    findPeakCnt = 1;
                }
                else if (ampl[i] < ampl[i - 1])
                {
                    if (findPeakCnt > 0) //找到极值
                    {
                        for (int j = 0; j < peakCnt; j++)
                        {
                            if (index[j] < 0)
                            {
                                peaks[j] = peakVal;
                                index[j] = i - (findPeakCnt + 1) / 2;
                                break;
                            }
                            else if (peakVal > peaks[j])
                            { //找到插入的位置
                                for (int k = peakCnt - 2; k >= j; k--) //移动
                                {
                                    peaks[k + 1] = peaks[k];
                                    index[k + 1] = index[k];
                                }
                                peaks[j] = peakVal;
                                index[j] = i - (findPeakCnt + 1) / 2;
                                break;
                            }
                        }
                    }
                    findPeakCnt = 0;
                }
                else if (findPeakCnt > 0)
                { //相等
                    findPeakCnt++;
                }
            }

        }
        /// <summary>
        /// 寻找极值
        /// </summary>
        /// <param name="ampl">信号幅度</param>
        /// <param name="index">开始查找位置</param>
        /// <param name="offset">信号的误差范围(在此范围内认为相同)</param>
        /// <param name="peakAmpl">极值</param>
        /// <param name="peakIndex">极值位置</param>
        public void FindPeaks(float[] ampl, int[] index, float offset, float[] peakAmpl, float[] peakIndex)
        {


            for (int i = 0; i < peakIndex.Length; i++)
            {
                peakIndex[i] = -1;
            }
            // 判断数据有效长度
            int dataLength = 0;
            for (int i = 0; i < index.Length; i++)
            {
                if (index[i] < 0)
                    break;
                dataLength++;
            }
            if (dataLength < 1)
            {
                return;
            }
            // 先排序索引
            for (int i = 0; i < dataLength - 1; i++)
            {
                int minIndex = i;
                for (int j = i + 1; j < dataLength; j++)
                {
                    if (index[j] < index[minIndex])
                    {
                        minIndex = j;
                    }
                }
                if (minIndex != i)
                {
                    float t = ampl[minIndex];
                    int tIndex = index[minIndex];

                    ampl[minIndex] = ampl[i];
                    index[minIndex] = index[i];

                    ampl[i] = t;
                    index[i] = tIndex;
                }

            }

            int peakCnt = 0;
            float peakVal = 0;
            for (int i = 1; i < dataLength; i++)
            {
                if (peakCnt > 0)
                {

                    if (ampl[i] > (1 + offset) * peakVal)
                    {
                        peakCnt = 1;
                        peakVal = ampl[i]; // 找到新的可能的极点

                    }
                    else if (ampl[i] < (1 - offset) * peakVal)
                    {
                        // 找到新的极点

                        for (int j = 0; j < peakIndex.Length; j++)
                        {
                            if (peakIndex[j] < 0)
                            {
                                peakAmpl[j] = peakVal;
                                peakIndex[j] = (index[i - 1] + index[i - peakCnt]) / 2f; // ??
                                break;
                            }
                            else if (peakVal > peakAmpl[j])
                            { // 找到插入的位置
                                for (int k = peakIndex.Length - 2; k >= j; k--) // 移动
                                {
                                    peakAmpl[k + 1] = peakAmpl[k];
                                    peakIndex[k + 1] = peakIndex[k];
                                }
                                peakAmpl[j] = peakVal;
                                peakIndex[j] = (index[i - 1] + index[i - peakCnt]) / 2f;
                                break;
                            }
                        }
                        peakCnt = 0;
                    }
                    else
                    {
                        peakCnt++;
                    }
                }
                else if (ampl[i] > ampl[i - 1])
                {

                    peakCnt = 1;
                    peakVal = ampl[i]; // 找到新的可能的极点
                }
                else
                {
                    peakCnt = 0;
                }

            }

        }

        public void FindComplexPeaks(float[] signal, float[] peaks, int[] index)
        {
            FindComplexPeaks(signal, signal.Length / 2, peaks, index);
        }

        public void FindComplexPeaks(float[] signal, int signalLength, float[] peaks, int[] index)
        {

            int length = signalLength / 2;
            float[] ampl = new float[length];
            for (int i = 0; i < length; i++)
            {
                ampl[i] = signal[2 * i] * signal[2 * i] + signal[2 * i + 1] * signal[2 * i + 1];

            }

            FindPeaks(ampl, peaks, index);

        }
        public void FindComplexPeaks(float[] signal, int from, int to, float[] peaks, int[] index)
        {


            float[] ampl = new float[to - from];
            for (int i = from; i < to; i++)
            {
                ampl[i - from] = signal[2 * i] * signal[2 * i] + signal[2 * i + 1] * signal[2 * i + 1];

            }
            FindPeaks(ampl, peaks, index);

        }

        /// <summary>
        /// 信号搬移
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="freq"></param>
        /// <param name="sampleRate"></param>
        public void ShiftSignal(float[] signal, float freq, int sampleRate)
        {
            int length = signal.Length / 2;

            for (int i = 0; i < length; i++)
            {
                double alpha = 2 * Math.PI * freq * i / sampleRate;
                double real = Math.Cos(alpha);
                double img = Math.Sin(alpha);

                double rReal = signal[2 * i] * real + signal[2 * i + 1] * img;
                double rImg = signal[2 * i + 1] * real - signal[2 * i] * img;

                signal[2 * i] = (float)rReal;
                signal[2 * i + 1] = (float)rImg;
            }
        }

        /// <summary>
        /// 信号低频滤波
        /// </summary>
        /// <param name="orignal"></param>
        /// <param name="filterData"></param>
        public void ComplexFilter(float[] orignal, float[] filterData)
        {

            if (orignal == null || orignal.Length <= 1) return;
            if (filterData.Length < orignal.Length) return;
            int length = orignal.Length / 2;

            int order = A.Length - 1;
            int paddingLenth = length + order; //补零
            float[] xData = new float[paddingLenth * 2];
            float[] yData = new float[paddingLenth * 2];

            Array.Clear(yData, 0, yData.Length);
            Array.Clear(xData, 0, order * 2);


            Array.Copy(orignal, 0, xData, order * 2, orignal.Length);




            for (int i = order; i < paddingLenth; i++)
            {
                for (int j = 0; j <= order; j++)
                {
                    yData[i * 2] += B[j] * xData[(i - j) * 2];
                    yData[i * 2 + 1] += B[j] * xData[(i - j) * 2 + 1];
                    if (j > 0)
                    {
                        yData[i * 2] -= A[j] * yData[(i - j) * 2];
                        yData[i * 2 + 1] -= A[j] * yData[(i - j) * 2 + 1];
                    }
                }
            }

            Array.Copy(yData, 2 * order, filterData, 0, filterData.Length);

        }

        public void UnderSample(float[] signal, int sampleRate)
        {
            int length = signal.Length / 2;
            int cnt = length / sampleRate;
            for (int i = 1; i < cnt; i++)
            {
                signal[i * 2] = signal[i * 2 * sampleRate];
                signal[i * 2 + 1] = signal[i * 2 * sampleRate + 1];
            }

            for (int i = 2 * cnt; i < signal.Length; i++)
            {
                signal[i] = 0;
            }
        }


        public float CalAmpl(float[] data, int offset, int cnt)
        {
            float sum = 0;
            for (int i = 0; i < cnt; i++)
            {
                sum += (data[offset + i] * data[offset + i]);
            }

            return (float)Math.Sqrt(sum / cnt);
        }

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

        public void CalDCACAmpl(float[] data, int offset, int cnt, float[] result)
        {
            float dcAmpl = CalDCAmpl(data, offset, cnt);
            result[0] = dcAmpl;
            float sum = 0;
            for (int i = 0; i < cnt; i++)
            {
                sum += ((data[offset + i] - dcAmpl) * (data[offset + i] - dcAmpl));
            }

            result[1] = (float)Math.Sqrt(sum / cnt);
        }


    }
}
