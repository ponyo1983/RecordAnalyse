using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace DataStorage.Analog
{
    /// <summary>
    /// 模拟量文件记录的索引
    /// </summary>
    class AnalogIndexFile
    {
        AnalogFile fileAnalog;
        const int IndexCount = 512; //索引文件大小 512*32Bytes=16KB
        List<AnalogIndex> listIndex = new List<AnalogIndex>(); //按文件记录索引排序
        FileStream fileStream = null;


        public AnalogIndexFile(AnalogFile analog)
        {
            this.fileAnalog = analog;
            byte[] buffer = new byte[IndexCount * 32];
            Array.Clear(buffer, 0, buffer.Length);
            try
            {
                string indexName = Path.Combine(analog.DataManager.StoreDir, analog.Type.ToString("X2") + "H-" + analog.Index.ToString("000") + ".idx");
                fileStream = new FileStream(indexName, FileMode.OpenOrCreate, FileAccess.ReadWrite);


                if (fileStream.Length < buffer.Length)
                {
                    fileStream.SetLength(buffer.Length); //默认建立16KB的文件
                }

                fileStream.Read(buffer, 0, buffer.Length);
            }
            catch
            {
                fileStream = null;
            }
            for (int i = 0; i < IndexCount; i++)
            {
                AnalogIndex indexRecord = new AnalogIndex(i, buffer, i * 32);
                listIndex.Add(indexRecord);
            }
            listIndex.Sort();

            //TODO:必须加入对重叠的Index处理
            for (int i = 0; i < listIndex.Count; i++)
            {
                if (listIndex[i].IsValid == false) break;

                for (int j = i + 1; j < listIndex.Count; j++)
                {
                    if (listIndex[j].IsValid && listIndex[j].ConflictWith(listIndex[i], this.fileAnalog.MaxFileSize))
                    {
                        for (int k = j; k < listIndex.Count; k++)
                        {
                            listIndex[k].FileIndex = 0; //设置为无效
                        }
                        break;

                    }
                }

            }

        }
           
        

        public AnalogIndex LastIndex
        {
            get
            {
                return listIndex[0];
            }

        }

        public IList<AnalogIndex> AllIndex
        {
            get
            {
                return listIndex.AsReadOnly();
            }
        }

        public AnalogIndex NewIndex()
        {

            AnalogIndex recordLast = listIndex[listIndex.Count - 1];
            AnalogIndex recordFirst = listIndex[0];
            recordLast.BeginOffset = 0;
            if (recordFirst.IsValid) //有效
            {
                recordLast.BeginOffset = (uint)((recordFirst.BeginOffset + recordFirst.RealLength) % fileAnalog.MaxFileSize);
            }
            recordLast.FileIndex = recordFirst.FileIndex + 1; //文件记录索引
            recordLast.RecordLength = 0; //记录长度
            recordLast.RealLength = 0;
            recordLast.BeginTime = DateTime.MinValue;
            recordLast.EndTime = DateTime.MinValue;

            lock (listIndex)
            {
                listIndex.Remove(recordLast);
                listIndex.Insert(0, recordLast);
            }
            return recordLast;
        }


        public void Store(AnalogIndex record)
        {
            if (fileStream != null)
            {
                byte[] data = record.GetBytes();
                
                fileStream.Position = record.Index * AnalogIndex.IndexSize;
                fileStream.Write(data, 0, data.Length);
                fileStream.Flush();
            }
        }

        public List<DateTime> GetAnalogTimes()
        {
            List<DateTime> times = new List<DateTime>();


            DateTime timeLast = DateTime.MinValue;

            lock (listIndex)
            {
                for (int i = 0; i < listIndex.Count; i++)
                {
                    if (listIndex[i].IsValid == false) break;

                    DateTime date = listIndex[i].BeginTime.Date;
                    if (date != timeLast)
                    {
                        times.Add(date);
                        timeLast = date;
                    }
                }
            }

            times.Sort();
            return times;
        }

    }
}
