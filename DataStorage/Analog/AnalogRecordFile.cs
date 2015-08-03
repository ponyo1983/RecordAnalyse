using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace DataStorage.Analog
{
    class AnalogRecordFile
    {
        public const int IncreaseSize = 4*1024 * 1024;
        FileStream fileStream = null;
        AnalogFile fileAnalog;
        string dataName="";
        public AnalogRecordFile(AnalogFile analog)
        {
            this.fileAnalog = analog;
            try
            {
                 dataName = Path.Combine(analog.DataManager.StoreDir, analog.Type.ToString("X2") + "H-" + analog.Index.ToString("000") + ".dat");
                fileStream = new FileStream(dataName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            }
            catch
            {
                fileStream = null;
            }
        }
      
        public void Store(AnalogIndex index, byte[] data, int length)
        {
            long offset = index.BeginOffset;
            long fileSize=fileStream.Length;
            long needSize = offset + index.RecordLength + length;
            long wrPos = (index.RecordLength + offset)%fileAnalog.MaxFileSize;
            if (needSize <= fileAnalog.MaxFileSize)//直接写入
            {
                if (fileSize < needSize)
                {
                    fileStream.SetLength(fileSize + IncreaseSize);
                }
                fileStream.Position = wrPos;
                fileStream.Write(data, 0, length);
                fileStream.Flush();
            }
            else
            {
            
                long leftSize = fileSize - wrPos;

                int size1 = (int)(leftSize < length ? leftSize : length);
                int size2 = length - size1;

                if (size1 > 0)
                {
                    fileStream.Position = wrPos;
                    fileStream.Write(data, 0, size1);
                }
                if (size2 > 0)
                {
                    fileStream.Position = 0;
                    fileStream.Write(data, size1, size2);
                }
                fileStream.Flush();
            }

            index.RecordLength = index.RecordLength + length;

        }

        public byte[] GetRecord(AnalogIndex indexRecord)
        {
            
            if (indexRecord.RecordLength > 10 * 1024 * 1024 || indexRecord.RecordLength<8) return null; //超过10M的记录或者不存在一条记录
           
            byte[] data=new byte[indexRecord.RecordLength];

            long fileSize = fileStream.Length;
            fileStream.Position = indexRecord.BeginOffset;

            if (fileSize >= indexRecord.BeginOffset + indexRecord.RecordLength)
            {
                fileStream.Read(data, 0, data.Length);
            }
            else
            {
                int size1=(int)(fileSize-indexRecord.BeginOffset);
                if(size1>0)
                {
                    fileStream.Read(data, 0, size1);
                }
                int size2 = indexRecord.RecordLength - size1;
                if (size2 > 0)
                {
                    fileStream.Position = 0;
                    fileStream.Read(data, size1, size2);
                }
               
            }
            
            return data;
            
        }
    }
}
