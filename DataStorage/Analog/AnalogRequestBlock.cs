using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace DataStorage.Analog
{
    class AnalogRequestBlock:IDataProcessBlock
    {

        AnalogFile fileAnalog;
        ManualResetEvent finishedEvent = new ManualResetEvent(false);

        List<List<AnalogPoint>> listAllPoit=new List<List<AnalogPoint>>();

         DateTime timeBegin;
         DateTime timeEnd;


         public AnalogRequestBlock(AnalogFile fileAnalog,DateTime timeBegin,DateTime timeEnd)
         {
             this.fileAnalog = fileAnalog;
             if (timeBegin < timeEnd)
             {
                 this.timeBegin = timeBegin;
                 this.timeEnd = timeEnd;
             }
             else
             {
                 this.timeBegin = timeEnd;
                 this.timeEnd = timeBegin;
             }
         }


         public bool Wait(int timeout)
         {
             if (timeout < 0)
                 return finishedEvent.WaitOne();
             return finishedEvent.WaitOne(timeout, false);
         }

         public List<List<AnalogPoint>> Result
         {
             get
             {
                 return listAllPoit;
             }
         }

        #region IDataProcessBlock 成员

        public bool Process()
        {
            List<AnalogPoint> listDayPoint = new List<AnalogPoint>();
            IList<AnalogIndex> indexAll = fileAnalog.IndexFile.AllIndex;
            for (int i = indexAll.Count-1; i >= 0; i--)
            {
                AnalogIndex recordIndex=indexAll[i];
                if (recordIndex.IsValid == false) continue;
                if (recordIndex.BeginTime > this.timeEnd) continue;
                if (recordIndex.EndTime < this.timeBegin) continue;

                if (recordIndex.Type == FallbackType.Fallback) //回溯数据
                {
                    if (listDayPoint.Count > 0)
                    {
                        listAllPoit.Add(listDayPoint);
                        listDayPoint = new List<AnalogPoint>();
                    }
                }
                //读取全部数据
                byte[] data = fileAnalog.DataFile.GetRecord(recordIndex);

                if (data == null) continue;

                int cnt = data.Length / 8;
                 DateTime timeBase = recordIndex.BeginTime.Date;
                if ((recordIndex.BeginTime >= this.timeBegin) && (recordIndex.EndTime <= this.timeEnd))
                {
                   
                    for (int j = 0; j < cnt; j++)
                    {
                        AnalogPoint pt = new AnalogPoint(timeBase, data, j * 8);
                        listDayPoint.Add(pt);
                    }
                }
                else
                {
                    for (int j = 0; j < cnt; j++)
                    {
                        uint timeDiff = BitConverter.ToUInt32(data, j * 8);
                        byte digit = (byte)(timeDiff & 0x0f);

                        DateTime time = timeBase.AddMilliseconds(timeDiff >> 4);
                        if (time < this.timeBegin) continue;
                        if (time > this.timeEnd) break;

                        float analogValue = BitConverter.ToSingle(data, j * 8 + 4);

                        AnalogPoint pt = new AnalogPoint(time, analogValue, digit);
                        listDayPoint.Add(pt);
                    }
                   
                }

            }
            if (listDayPoint.Count > 0)
            {
                listAllPoit.Add(listDayPoint);
            }

            finishedEvent.Set();

            return true;
        }

        #endregion
    }
}
