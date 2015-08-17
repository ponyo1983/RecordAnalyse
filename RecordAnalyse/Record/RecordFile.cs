using System;
using System.Collections.Generic;
using System.Text;
using RecordAnalyse.Utils;
using System.Threading;
using RecordAnalyse.Signal;
using System.IO;

namespace RecordAnalyse.Record
{
   public class RecordFile
    {
        RecordDevice device;

        List<SignalChannel> listChannel = new List<SignalChannel>();

        EventWaitHandle[] eventWaits;

        DiskUtil disk;


        public event EventHandler<SignalArgs> SignalArgsChanged;

        byte[] dataChannel = new byte[512];
        public RecordFile(DiskUtil disk, long offset,int index)
        {
            this.disk = disk;
            this.Offset = offset;
            disk.Position = offset;
            device = new RecordDevice(disk);
            if (device.IsValid == false) return;

         

            disk.Read(dataChannel, 0, 512);

            string tag = ASCIIEncoding.ASCII.GetString(dataChannel, 0, 4);

            if (tag != " chn") return;

            int channelNum = dataChannel[4];
            eventWaits = new EventWaitHandle[channelNum];

            for (int i = 0; i < channelNum; i++)
            {
                AutoResetEvent autoEvent = new AutoResetEvent(false);
                SignalChannel info = new SignalChannel(this,autoEvent, dataChannel, 5 + 128 * i);
                eventWaits[i] = autoEvent;
                listChannel.Add(info);
            }


            this.FileIndex = index+1;
        }

        public bool Select
        {
            get;
            set;
        }

        public int FileIndex
        {
            get;
            private set;
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
                    return (device.UsedSector) * 512L;
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
        public string BeginTimeTxt
        {
            get
            {
                return this.BeginTime.ToString("yyyy/MM/dd HH:mm:ss");
            }
        }

        public string TimeLength
        {
            get
            {
              int  seconds = (int)((this.EndTime - this.BeginTime).TotalSeconds);
              
                int h = seconds / (3600);
                int m = (seconds - h * (3600)) / 60;
                int s = seconds - h * (3600) - m * 60;

               return h.ToString("00") + ":" + m.ToString("00") + ":" + s.ToString("00");
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


            if (timeBegin > this.EndTime) return null;

            if (timeBegin.AddSeconds(secLen) < this.BeginTime) return null;

           long startSec =(long)((timeBegin - this.BeginTime).TotalSeconds);

            int sampleRate = listChannel[0].SampleRate;
            int bytesPerSec = channelNum * sampleRate;
            long secTotal = (long)((this.Length - 512 * 2) * 1f / (channelNum * sampleRate)); //去掉最后一秒的数据


            if (startSec + secLen > secTotal)
            {
                secLen = (int)(secTotal - startSec);
            }

            long startRd = (this.Offset + 512 * 2 + startSec * sampleRate * channelNum-511)/512L*512;
            long rdOffset = (this.Offset + 512 * 2 + startSec * sampleRate * channelNum - startRd);
            long endRd = (this.Offset + 512 * 2+sampleRate*(startSec +secLen) * channelNum + 511) / 512L * 512;
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


        public void Write(FileStream fs)
        {
            device.Write(fs);
            fs.Write(dataChannel, 0, 512);


            //原始数据
            disk.Position = 512 * 2 + this.Offset;

            byte[] data=new byte[256*512];

            int readSector = 0;

            while (readSector < device.UsedSector-2)
            {
                int length = disk.Read(data, 0, data.Length);
                int sector = length / 512;
                if (sector + readSector > device.UsedSector - 2)
                {
                    sector = device.UsedSector - 2 - readSector;
                }
                readSector += sector;
                fs.Write(data, 0, sector * 512);
                this.ExportLength = (readSector+2)*512L;
                

            }

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

            long readOffset = 512 * 2;
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
