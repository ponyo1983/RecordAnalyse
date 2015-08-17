using System;
using System.Collections.Generic;
using System.Text;
using RecordAnalyse.Utils;
using RecordAnalyse.Signal;
using RecordAnalyse.DSP;
using System.Threading;

namespace RecordAnalyse.Record
{
    public class SignalChannel
    {

        RecordFile recordFile;
        List<CalcItem> calList = new List<CalcItem>();

        ADBlock adBlock;

        AutoResetEvent eventDSP;

        public event EventHandler<SignalArgs> SignalArgsChanged;


        Thread threadDSP;

        const int MaxPt = 3;


        private static readonly string[] ChannelNames = new string[] { "轨道电路", "交流道岔", "直流道岔", "交流信号机", "直流信号机", "综合电流采集", "综合电压采集" };

        public IList<CalcItem> CalcList
        {
            get
            {
                return calList.AsReadOnly();
            }
        }


        public bool DecodeFM
        {
            get;
            set;
        }

        public int DecodeCurve
        {
            get;
            set;
        }

        public bool DecodeAngle
        {
            get;
            set;
        }


        public bool IsReference
        {
            get;
            set;
        }


        public SignalChannel(RecordFile recordFile, AutoResetEvent autoEvent, byte[] data, int offset)
        {
            this.recordFile = recordFile;
            this.eventDSP = autoEvent;

            this.DecodeCurve = 0;
            this.DecodeFM = false;
            this.DecodeAngle = false;
            UInt16 crc = CRC16.ComputeCRC16(data, offset, 124 - 2);


            UInt16 crcCal = BitConverter.ToUInt16(data, offset + 124 - 2);


            if (crc != crcCal) return;

            // this.ChannnelNum = data[0 + offset];
            this.State = (byte)(data[1 + offset] >> 7);
            this.SampleRate = data[0 + offset] + ((data[1 + offset] & 0x1f) << 8);
          
            this.ChannelType = data[2 + offset];
            //时间
            int year = data[3 + offset] + 2000;
            int month = data[4 + offset];
            int day = data[5 + offset];
            this.Time = new DateTime(year, month, day);

            //名字
            int num = data[6 + offset];

            if (num > 0)
            {
                num = num > 8 ? 8 : num;

                this.Person = ASCIIEncoding.UTF8.GetString(data, 7 + offset, num);
            }
            else
            {

                this.Person = "";
            }


            for (int i = 0; i < 21; i++)
            {
                CalcItem calItm = new CalcItem(data, 15 + i * 5 + offset);
                if (calItm.IsValid)
                {
                    calList.Add(calItm);
                }
            }
            this.TimeInterval = 40; //默认是40毫秒计算一个点
        }


       

        public void AddCalcItem(CalcItem itm)
        {
            calList.Add(itm);
        }
        public void ReplaceCalcItem(int m, CalcItem itm)
        {
            calList[m] = itm;
        }

        public byte ChannnelNum
        {
            get;
            set;
        }

        public byte State
        {
            get;
            set;
        }

        public int SampleRate
        {
            get;
            set;
        }
        public byte ChannelType
        {
            get;
            set;
        }

        public string ChannelName
        {
            get
            {

                //先将10进制变成16进制

                int channel = this.ChannelType / 10 * 16 + (this.ChannelType % 10);

                int type = (channel >> 4) & 0x0f;
                int index = channel & 0x0f;
                if (type >= 0 && type < ChannelNames.Length)
                {
                    return ChannelNames[type] + ":通道"+index;
                }
                return "";
            }
        }


        public DateTime Time
        {
            get;
            set;
        }


        public string Person
        {
            get;
            set;
        }




        public void PutADData(byte[] data, int offset, int cnt)
        {
            if (adBlock != null)
            {
                adBlock.PutAdData(data, offset, cnt);
            }
        }



        public void Start()
        {
            if (threadDSP == null || threadDSP.IsAlive == false)
            {
                if (DecodeAngle)
                {
                    threadDSP = new Thread(new ThreadStart(ProcDSPAngle));
                    threadDSP.IsBackground = true;
                    threadDSP.Start();
                }
                else
                {
                    threadDSP = new Thread(new ThreadStart(ProcDSP));
                    threadDSP.IsBackground = true;
                    threadDSP.Start();
                }
                
            }
        }

        public void Stop()
        {
            if (threadDSP != null && threadDSP.IsAlive)
            {
                threadDSP.Abort();
            }

        }



        private float CalRealVal(float adVal, float freq)
        {

            float minFreq = float.MaxValue;
            int minIndex = -1;
            for (int i = 0; i < calList.Count; i++)
            {
                float diff = Math.Abs(calList[i].Freq - freq);
                if (diff < minFreq)
                {
                    minFreq = diff;
                    minIndex = i;
                }
            }

            if (minIndex >= 0)
            {
               
                return calList[minIndex].CoeffK * adVal + calList[minIndex].CoeffB;
            }
            return adVal;
        }



        float CalAmplByFreq(float[] data, int freq)
        {

            int fftNum = data.Length / 2;

            if (freq == 0) return data[0] / fftNum;

            int startIndex = freq;
            int endIndex = freq;
            if (freq < 450)
            {
                startIndex = freq - 2 > 0 ? freq - 2 : 0;
                endIndex = freq + 2;
            }
            else if (freq < 1000)
            {
                startIndex = freq - 150 > 0 ? freq - 150 : 0;
                endIndex = freq + 150;
            }
            else if (freq < 3000)
            {
                startIndex = freq - 100 > 0 ? freq - 100 : 0;
                endIndex = freq + 100;
            }


            float ampl = 0;
            for (int i = startIndex; i <= endIndex; i++)
            {
                ampl += (data[i * 2] * data[i * 2] + data[i * 2 + 1] * data[i * 2 + 1]);
            }
            ampl = (float)(Math.Sqrt(ampl) * 2 / fftNum / Math.Sqrt(2f));
            return ampl;
        }


        public float Angle
        {
            get;
            private set;
        }

        private void ProcDSPAngle()
        {
            this.adBlock = new ADBlock(SampleRate*2, SampleRate);
            APFFT apfft = new APFFT(this.SampleRate);
            try
            {
                while (true)
                {
                    float[] adData = adBlock.GetAdData(-1);

                    
                    if (adData == null) continue;

                    this.Angle = apfft.CalAngle(adData);
                    eventDSP.Set();
                }
            }
            catch (Exception)
            {
 
            }
        }


        public float TimeInterval
        {
            get;
            set;
        }

        private void ProcDSP()
        {
            DSPUtil util = new DSPUtil();
            FFTPlan fftPlan = null;

            float[] data1 = null;
            float[] data2 = null;
            float[] fftData = null;
            float[] dataSpectrum = null;

            int PeakNum = 6;
            float[] ypDiff = new float[PeakNum - 1];
            float[] peakVal = new float[PeakNum];
            int[] peakIndexLeft = new int[PeakNum];
            int[] peakIndexLeftTmp = new int[PeakNum];
            int[] peakIndexRight = new int[PeakNum];
            float[] dcacAmpl = new float[2];

            float[] amplDense = new float[250];
           // float[] amplDenseDC = new float[25];



            float dcAmpl = 0;
            float acAmpl = 0;
            float prevCarrier = -1;
            float prevLow = -1;
            int diffCnt = 0;
            float carrierFreq = 0;
            float lowFreq = 0;
            float standard = 0;
            SignalArgs args;

            List<float> curveList = new List<float>();

            bool curveStart = false;



            float[] ypList = new float[] { 8.5f, 9.0f, 9.5f, 11.0f, 12.5f, 13.5f, 15.0f, 16.5f, 17.5f, 18.5f, 20.0f, 21.5f, 22.5f, 23.5f, 24.5f, 26.0f };

            Queue<float> queueACCurve = new Queue<float>();

            int maxCurveCnt = 0;
            int MAXPT=40*25;
            this.adBlock = new ADBlock(SampleRate, SampleRate);
            try
            {
                while (true)
                {
                    float[] adData = adBlock.GetAdData(-1);

                    if (SignalArgsChanged == null)
                    {
                        eventDSP.Set();
                        continue;
                    }
                    if (adData == null) continue;
                    if (DecodeCurve>0) //计算道岔曲线 ，不计算载频和低频
                    {
                        int calCnt = (int)(1000 / this.TimeInterval);
                        int amplCnt = this.SampleRate / calCnt; //40ms一个点

                        for (int i = 0; i < calCnt; i++)
                        {
                            if (DecodeCurve == 1)
                            {
                                amplDense[i] = util.CalACAmpl(adData, i * amplCnt, amplCnt);
                                amplDense[i] = CalRealVal(amplDense[i], 50); //道岔电流都是交流50Hz
                            }
                            else if (DecodeCurve == 2)
                            {
                                amplDense[i] = util.CalDCAmpl(adData, i * amplCnt, amplCnt);
                                amplDense[i] = CalRealVal(amplDense[i], 0); 
                            }
                            if (curveStart == false)
                            {
                                if (queueACCurve.Count >= 2)
                                {
                                    float[] prevVals = queueACCurve.ToArray();

                                    for (int j = 0; j < prevVals.Length - 1; j++)
                                    {
                                        if (prevVals[j] > 0.01f)
                                        {
                                            float rate = (prevVals[prevVals.Length - 1] - prevVals[j]) / prevVals[j];
                                            if (rate > 5)
                                            {
                                                curveStart = true;

                                                curveList.AddRange(prevVals);
                                                maxCurveCnt = MAXPT;
                                                break;
                                            }
                                        }
                                    }

                                }

                            }
                            else
                            {
                                curveList.Add(amplDense[i]);

                                if (amplDense[i] < 0.1f)
                                {
                                    if (maxCurveCnt >= MAXPT)
                                    {
                                        maxCurveCnt = curveList.Count + 5;
                                    }
                                }

                                if (curveList.Count >= maxCurveCnt) //最大40秒
                                {
                                    args = new SignalArgs(recordFile.TimeExport.AddMilliseconds(-1 * this.TimeInterval * curveList.Count + i *1000f/ calCnt), curveList.ToArray());
                                    SignalArgsChanged(this, args);
                                    curveStart = false;
                                    maxCurveCnt = 0;
                                    curveList.Clear();
                                    queueACCurve.Clear();
                                }

                            }
                            if (queueACCurve.Count >= MaxPt)
                            {
                                queueACCurve.Dequeue();
                            }
                            queueACCurve.Enqueue(amplDense[i]);


                          

                        }

                        eventDSP.Set();
                        continue;


                    }
                    if (fftPlan == null)
                    {
                        fftPlan = new FFTPlan(SampleRate);
                        data1 = new float[SampleRate * 2];
                        data2 = new float[SampleRate * 2];
                        fftData = new float[SampleRate * 2];
                        dataSpectrum = new float[SampleRate];

                    }
                    //将实数变为复数
                    for (int i = 0; i < adData.Length; i++)
                    {
                        data1[i * 2] = adData[i];
                        data1[i * 2 + 1] = 0;
                    }
                    fftPlan.FFTForward(data1, fftData);
                    Array.Copy(fftData, data2, data2.Length);

                    //计算频谱
                    int spectrumLen = adData.Length / 2;
                    for (int i = 0; i < spectrumLen; i++)
                    {
                        dataSpectrum[i] = (float)Math.Sqrt(data2[i * 2] * data2[i * 2] + data2[i * 2 + 1] * data2[i * 2 + 1]);
                    }


                    //计算交直流幅度
                    util.CalDCACAmpl(adData, 0, this.SampleRate, dcacAmpl);

                    dcAmpl = CalRealVal(dcacAmpl[0], 0);
                    acAmpl = CalRealVal(dcacAmpl[1], 0);
                    int ignoreLow =DecodeFM?400: 5;
                    int ignoreHigh = DecodeFM ? 3000 : this.SampleRate / 2;
                    util.FindComplexPeaks(data2, ignoreLow, ignoreHigh, peakVal, peakIndexLeft); //忽略直流信息

                   // int freqCenter=  util.CalFreqCenter(dataSpectrum, 400);


                    if (peakIndexLeft[0] < 0)
                    {
                        args = new SignalArgs(recordFile.TimeExport, dcAmpl, acAmpl, 0, 0);
                        SignalArgsChanged(this, args);
                        eventDSP.Set();
                        continue; //不存在极点
                    }


                    float freq = peakIndexLeft[0];
                    acAmpl = CalAmplByFreq(fftData, (int)freq);
                    acAmpl = CalRealVal(acAmpl, freq);
                    if (DecodeFM == false)
                    {
                        args = new SignalArgs(recordFile.TimeExport, dcAmpl, acAmpl, 0, 0);
                        SignalArgsChanged(this, args);

                        eventDSP.Set();
                        continue;
                    }
                  
                    if (DecodeFM)
                    {
                        if (acAmpl < 0.1)
                        {
                            eventDSP.Set();
                            continue;
                        }
                        bool matchYP = true;
                        bool matchUM71 = true;
                        float freqShift = 0;
                        int underSampleCount = 1;
                        
                        for (int i = 0; i < peakIndexLeft.Length; i++)
                        {
                            if ((peakIndexLeft[i]  < 1600 - 200) || (peakIndexLeft[i]  > 2600 + 200))
                            {
                                matchUM71 = false;
                            }
                            if ((peakIndexLeft[i]  < 550 - 100) || (peakIndexLeft[i]  > 850 + 100))
                            {
                                matchYP = false;
                            }
                        }
                        if ((matchYP == false) && (matchUM71 == false))
                        {
                            carrierFreq = peakIndexLeft[0];
                            lowFreq = 0;
                        }

                        if (matchYP)
                        {
                            float tmpShiftDiff = float.MaxValue;

                            for (int i = 0; i < peakIndexLeft.Length - 2; i++)
                            {
                                for (int j = i + 1; j < peakIndexLeft.Length - 1; j++)
                                {
                                    float tmpShift = (peakIndexLeft[i] + peakIndexLeft[j] ) / 2f;
                                    if (Math.Abs(tmpShift - 550) < tmpShiftDiff)
                                    {
                                        freqShift = tmpShift;
                                        tmpShiftDiff = Math.Abs(tmpShift - 550);
                                    }
                                    if (Math.Abs(tmpShift - 650) < tmpShiftDiff)
                                    {
                                        freqShift = tmpShift;
                                        tmpShiftDiff = Math.Abs(tmpShift - 650);
                                    }
                                    if (Math.Abs(tmpShift - 750) < tmpShiftDiff)
                                    {
                                        freqShift = tmpShift;
                                        tmpShiftDiff = Math.Abs(tmpShift - 750);
                                    }
                                    if (Math.Abs(tmpShift - 850) < tmpShiftDiff)
                                    {
                                        freqShift = tmpShift;
                                        tmpShiftDiff = Math.Abs(tmpShift - 850);
                                    }
                                    
                                }
                            }

                            util.FindComplexPeaks(data2, (int)freqShift, ignoreHigh, peakVal, peakIndexLeft);
                            standard = Math.Abs(peakIndexLeft[0] - peakIndexLeft[1]);
                            underSampleCount = 30;

                        }
                        if (matchUM71)
                        {
                            freqShift = peakIndexLeft[0]  - 40;
                            underSampleCount = 40;
                        }

                        util.ShiftSignal(data1, freqShift, this.SampleRate); //频谱搬移

                        util.ComplexFilter(data1, data2); //滤波

                        util.UnderSample(data2, underSampleCount); //欠采样

                        fftPlan.FFTForward(data2, data1); //频谱分析



                        if (matchYP)
                        {
                            int signalLength = data1.Length / 2;
                            util.FindComplexPeaks(data1, 0, signalLength / 2, peakVal, peakIndexLeft);
          

                            float diffMinYp = 1.5f; //最大不能差1.5

                            lowFreq = -1;
                            for (int j = 0; j < peakIndexLeft.Length - 1; j++)
                            {
                                for (int k = j+1; k < peakIndexLeft.Length; k++)
                                {
                                    float tmpLow = Math.Abs(peakIndexLeft[k] - peakIndexLeft[j]) * 1f / underSampleCount;
                                    if (tmpLow > 8 && tmpLow < 28)
                                    {
                                        if (Math.Abs(tmpLow - standard) < diffMinYp)
                                        {
                                            lowFreq = tmpLow;
                                            diffMinYp = Math.Abs(tmpLow - standard);
                                        }
                                    }
                                }
                               if (lowFreq > 0) break;
                            }

                   
                            util.FindComplexPeaks(data1, signalLength / 2, signalLength, peakVal, peakIndexRight);


                            carrierFreq = freqShift;

                        }
                        if (matchUM71)
                        {
                            util.FindComplexPeaks(data1, peakVal, peakIndexLeft);
                            carrierFreq = freqShift + peakIndexLeft[0] * 1f / underSampleCount;
                            lowFreq = Math.Abs(peakIndexLeft[1] - peakIndexLeft[2]) / 2f / underSampleCount;
                            if (lowFreq < 9 || lowFreq > 31)
                            {
                                lowFreq = -1;
                            }
                        }

                    }

                    acAmpl = CalAmplByFreq(fftData, (int)carrierFreq);
                    acAmpl = CalRealVal(acAmpl, carrierFreq);
                 
                    if (Math.Abs(carrierFreq - prevCarrier) > 10 || Math.Abs(lowFreq - prevLow) > 1)
                    {
                        diffCnt++;
                        if (diffCnt >= 2)
                        {
                            prevCarrier = carrierFreq;
                            prevLow = lowFreq;
                            diffCnt = 0;
                        }
                        else
                        {
                            carrierFreq = prevCarrier;
                            lowFreq = prevLow;
                        }
                    }
                    else
                    {
                        prevCarrier = carrierFreq;
                        prevLow = lowFreq;
                        diffCnt = 0;
                    }

                  
                    args = new SignalArgs(recordFile.TimeExport, dcAmpl, acAmpl, carrierFreq, lowFreq);
                    SignalArgsChanged(this, args);

                    eventDSP.Set();
                }
            }
            catch (Exception)
            {
                if (fftPlan != null)
                {
                    fftPlan.Dispose();
                }

            }


        }






    }
}
