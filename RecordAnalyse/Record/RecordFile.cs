using System;
using System.Collections.Generic;
using System.Text;
using RecordAnalyse.Utils;
using System.Threading;
using RecordAnalyse.Signal;

namespace RecordAnalyse.Record
{
   public class RecordFile
    {
        RecordDevice device;

        List<SignalChannel> listChannel = new List<SignalChannel>();

        EventWaitHandle[] eventWaits;

        DiskUtil disk;


        public event EventHandler<SignalArgs> SignalArgsChanged;

        public RecordFile(DiskUtil disk, long offset)
        {
            this.disk = disk;
            this.Offset = offset;
            disk.Position = offset;
            device = new RecordDevice(disk);
            if (device.IsValid == false) return;

            byte[] data = new byte[512];

            disk.Read(data, 0, 512);

            string tag = ASCIIEncoding.ASCII.GetString(data, 0, 4);

            if (tag != " chn") return;

            int channelNum = data[4];
            eventWaits = new EventWaitHandle[channelNum];

            for (int i = 0; i < channelNum; i++)
            {
                AutoResetEvent autoEvent = new AutoResetEvent(false);
                SignalChannel info = new SignalChannel(this,autoEvent, data, 5 + 128 * i);
                eventWaits[i] = autoEvent;
                listChannel.Add(info);
            }



        }


        public DiskUtil Disk
        {
            get
            {
                return disk;
            }
        }

        public long Offset
        {
            get;
            private set;
        }


        public long Length
        {

            get
            {

                if (device.IsValid)
                {
                    return (device.UsedSector) * 512;
                }
                return 0;
            }

        }

        public long ExportLength
        {
            get;
            private set;
        }

        public IList<SignalChannel> Channels
        {
            get { return listChannel.AsReadOnly(); }
        }



        private bool WaitDSP()
        {
            return EventWaitHandle.WaitAll(eventWaits, 1000);


        }

        public DateTime BeginTime
        {
            get
            {
                return device.Time;
            }
        }

        public DateTime EndTime
        {
            get
            {
                int channelNum = listChannel.Count;

                if (channelNum <= 0) return DateTime.MinValue;

                int sampleRate = listChannel[0].SampleRate;


                double seconds = (this.Length-512*2)*1f /(channelNum * sampleRate); //去掉最后一秒的数据

                return this.device.Time.AddSeconds(seconds);
            }
        }

        /// <summary>
        /// 导出数据时的当前时间
        /// </summary>
        public DateTime TimeExport
        {
            get;
            private set;

        }




        public float[] GetData(int channel, DateTime timeBegin, int secLen)
        {

            int channelNum = listChannel.Count;

            if (channelNum <= 0) return null;

           int startSec =(int)((timeBegin - this.BeginTime).TotalSeconds);

            int sampleRate = listChannel[0].SampleRate;
            int bytesPerSec = channelNum * sampleRate;
            int secTotal = (int)((this.Length - 512 * 2) * 1f / (channelNum * sampleRate)); //去掉最后一秒的数据
            if (startSec + secLen > secTotal) return null;

            int startRd = (int)(this.Offset + 512 * 2 + startSec * sampleRate * channelNum-511)/512*512;
            int rdOffset = (int)(this.Offset + 512 * 2 + startSec * sampleRate * channelNum - startRd);
            int endRd = (int)(this.Offset + 512 * 2+sampleRate*(startSec +secLen) * channelNum + 511) / 512 * 512;
            disk.Position = startRd;

          
            byte[] adData = new byte[endRd-startRd];

            float[] rdVal = new float[sampleRate * secLen];
            int length=disk.Read(adData, 0, adData.Length);

            for (int i = 0; i < sampleRate*secLen; i++)
            {
                rdVal[i] = adData[i * channelNum + channel+rdOffset];
            }

            return rdVal;



        }


        /// <summary>
        /// 导出数据
        /// </summary>
        public void Export()
        {

            int channelNum = listChannel.Count;

            if (channelNum <= 0) return;

            disk.Position = 512 * 2 + this.Offset;


            this.TimeExport = device.Time;

            int sampleRate = listChannel[0].SampleRate;
            int bytesPerSec = listChannel[0].SampleRate*channelNum;

            int readOffset = 512 * 2;
            long recordSize = this.Length -   bytesPerSec; //去掉最后一秒的数据

            byte[] adData = new byte[sampleRate];

         


            byte[] data = new byte[1024*1024]; //每次读取1MB数据


            for (int i = 0; i < listChannel.Count; i++)
            {
                listChannel[i].Start();
            }

            List<byte> dataList = new List<byte>();

            int refChannelIndex = -1;
            int diffChannelIndex = -1;

            if (SignalArgsChanged != null)
            {
                for (int i = 0; i < listChannel.Count; i++)
                {
                    if (listChannel[i].IsReference)
                    {
                        refChannelIndex = i;
                    }
                    else if (listChannel[i].DecodeAngle)
                    {
                        diffChannelIndex = i;
                    }
                }
            }
            
            while (readOffset < recordSize)
            {

              
                int length = disk.Read(data, 0, data.Length);

                dataList.AddRange(data);

                int num = dataList.Count / bytesPerSec;

                for (int i = 0; i < num; i++)
                {
                    for (int j = 0; j < channelNum; j++)
                    {
                        for (int k = 0; k < adData.Length; k++)
                        {
                            adData[k] = dataList[i * bytesPerSec + j + k * channelNum];
                        }
                        listChannel[j].PutADData(adData, 0, adData.Length);
                    }
                    readOffset += bytesPerSec;
                    if (WaitDSP())
                    {
                        if (diffChannelIndex >= 0 && refChannelIndex >= 0)
                        {
                            float diffAngle = listChannel[diffChannelIndex].Angle - listChannel[refChannelIndex].Angle;
                           
                            if (diffAngle < 0)
                            {
                                diffAngle += 360;
                            }

                            if (SignalArgsChanged != null)
                            {
                                SignalArgsChanged(this, new SignalArgs(this.TimeExport, diffAngle));
                            }
                        }
                    }
                    this.TimeExport = this.TimeExport.AddSeconds(1);
                    this.ExportLength = readOffset;
                    if (readOffset >= recordSize)
                    {
                        break;
                    }
                }

                dataList.RemoveRange(0, num * bytesPerSec);




            }

            for (int i = 0; i < listChannel.Count; i++)
            {
                listChannel[i].Stop();
            }

        }

    }
}
