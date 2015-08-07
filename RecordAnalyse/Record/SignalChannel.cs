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

        public bool DecodeCurve
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

            this.DecodeCurve = false;
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
                if (freq < 0.1)
                {
                    return calList[minIndex].CoeffK * adVal + calList[minIndex].CoeffB;
                }
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


        private void ProcDSP()
        {
            DSPUtil util = new DSPUtil();
            FFTPlan fftPlan = null;

            float[] data1 = null;
            float[] data2 = null;
            float[] fftData = null;
            float[] dataSpectrum = null;

            float[] peakVal = new float[5];
            int[] peakIndexLeft = new int[5];
            int[] peakIndexRight = new int[5];
            float[] dcacAmpl = new float[2];

            float[] amplDenseAC = new float[25];
            float[] amplDenseDC = new float[25];



            float dcAmpl = 0;
            float acAmpl = 0;
            float prevCarrier = -1;
            float prevLow = -1;
            int diffCnt = 0;
            float carrierFreq = 0;
            float lowFreq = 0;
            SignalArgs args;

            List<float> curveList = new List<float>();

            bool curveStart = false;



            Queue<float> queueACCurve = new Queue<float>();

            int maxCurveCnt = 0;

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
                    if (DecodeCurve) //计算道岔曲线 ，不计算载频和低频
                    {

                        int amplCnt = this.SampleRate / 25; //40ms一个点

                        for (int i = 0; i < 25; i++)
                        {
                            amplDenseAC[i] = util.CalACAmpl(adData, i * amplCnt, amplCnt);
                            amplDenseAC[i] = CalRealVal(amplDenseAC[i], 50); //道岔电流都是交流50Hz

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
                                                maxCurveCnt = 40 * 25;
                                                break;
                                            }
                                        }
                                    }

                                }

                            }
                            else
                            {
                                curveList.Add(amplDenseAC[i]);

                                if (amplDenseAC[i] < 0.2f)
                                {
                                    maxCurveCnt = curveList.Count + 5;
                                }
                                else
                                {
                                    if (queueACCurve.Count >= 2)
                                    {
                                        float[] prevVals = queueACCurve.ToArray();

                                        for (int j = 1; j < prevVals.Length; j++)
                                        {
                                            if (prevVals[j] > 0.01f)
                                            {
                                                float rate = (prevVals[0] - prevVals[j]) / prevVals[j];
                                                if (rate > 5)
                                                {
                                                    maxCurveCnt = curveList.Count + 5;
                                                    break;
                                                }
                                            }
                                        }

                                    }
                                }

                                if (curveList.Count >= maxCurveCnt) //最大40秒
                                {
                                    args = new SignalArgs(recordFile.TimeExport, curveList.ToArray());
                                    SignalArgsChanged(this, args);
                                    curveStart = false;
                                    maxCurveCnt = 0;
                                    curveList.Clear();
                                }

                            }
                            if (queueACCurve.Count >= MaxPt)
                            {
                                queueACCurve.Dequeue();
                            }
                            queueACCurve.Enqueue(amplDenseAC[i]);


                            amplDenseDC[i] = util.CalDCAmpl(adData, i * amplCnt, amplCnt);
                            amplDenseDC[i] = CalRealVal(amplDenseDC[i], 0); //直流幅度

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
                    int ignoreLow = 5;
                    util.FindComplexPeaks(data2, ignoreLow, this.SampleRate / 2, peakVal, peakIndexLeft); //忽略直流信息

                   // int freqCenter=  util.CalFreqCenter(dataSpectrum, 400);


                    if (peakIndexLeft[0] < 0)
                    {
                        args = new SignalArgs(recordFile.TimeExport, dcAmpl, acAmpl, 0, 0);
                        SignalArgsChanged(this, args);
                        eventDSP.Set();
                        continue; //不存在极点
                    }


                    float freq = peakIndexLeft[0] + ignoreLow;
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
                            if ((peakIndexLeft[i] + ignoreLow < 1600 - 200) || (peakIndexLeft[i] + ignoreLow > 2600 + 200))
                            {
                                matchUM71 = false;
                            }
                            if ((peakIndexLeft[i] + ignoreLow < 550 - 100) || (peakIndexLeft[i] + ignoreLow > 850 + 100))
                            {
                                matchYP = false;
                            }
                        }
                        if ((matchYP == false) && (matchUM71 == false))
                        {
                            carrierFreq = 0;
                            lowFreq = 0;
                        }

                        if (matchYP)
                        {
                            float tmpShiftDiff = float.MaxValue;

                            for (int i = 0; i < peakIndexLeft.Length - 2; i++)
                            {
                                for (int j = i + 1; j < peakIndexLeft.Length - 1; j++)
                                {
                                    float tmpShift = (peakIndexLeft[i] + peakIndexLeft[j] + 2 * ignoreLow) / 2f;
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
                            underSampleCount = 30;
                        }
                        if (matchUM71)
                        {
                            freqShift = peakIndexLeft[0] + ignoreLow - 40;
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
                            lowFreq = -1; //Math.Abs(peakIndex[0] - peakIndex[1]) * 1f / underSampleCount;


                            for (int j = 0; j < peakIndexLeft.Length - 1; j++)
                            {
                                for (int k = j; k < peakIndexLeft.Length - 1; k += 2)
                                {
                                    float tmpLow = Math.Abs(peakIndexLeft[k] - peakIndexLeft[k + 1]) * 1f / underSampleCount;
                                    if (tmpLow > 7 && tmpLow < 28)
                                    {
                                        lowFreq = tmpLow;
                                        break;
                                    }
                                }
                                if (lowFreq > 0) break;
                            }



                            util.FindComplexPeaks(data1, signalLength / 2, signalLength, peakVal, peakIndexRight);



                            float[] carrierList = new float[] { 550, 650, 750, 850, };

                            float diffrate = float.MaxValue;

                            for (int i = 0; i < 3; i++)
                            {
                                for (int j = 0; j < 3; j++)
                                {
                                    float indexLeft = peakIndexLeft[i];
                                    float indexRight = signalLength / 2 - peakIndexRight[j];

                                    float tmpCarrier = freqShift + (indexRight - indexLeft + 2) / 2f / underSampleCount;

                                    for (int k = 0; k < carrierList.Length; k++)
                                    {
                                        float rate = Math.Abs(tmpCarrier - carrierList[k]) / carrierList[k];
                                        if (rate < diffrate)
                                        {
                                            diffrate = rate;
                                            carrierFreq = tmpCarrier;
                                        }
                                    }


                                }
                            }

                        }
                        if (matchUM71)
                        {
                            util.FindComplexPeaks(data1, peakVal, peakIndexLeft);
                            carrierFreq = freqShift + peakIndexLeft[0] * 1f / underSampleCount;
                            lowFreq = Math.Abs(peakIndexLeft[1] - peakIndexLeft[2]) / 2f / underSampleCount;
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
