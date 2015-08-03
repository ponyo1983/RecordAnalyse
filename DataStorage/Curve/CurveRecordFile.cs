using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace DataStorage.Curve
{
    class CurveRecordFile
    {

        public const int IncreaseSize = 1024 * 1024;

        CurveFile curveFile;

        FileStream fileStream;

        public CurveRecordFile(CurveFile curveFile, int recordSize)
        {
            this.curveFile = curveFile;
            this.RecordSize = recordSize;
            try
            {
                string fileName = Path.Combine(curveFile.DataManager.StoreDir, "Curve"+recordSize+".dat");
                fileStream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            }
            catch
            {
                fileStream = null;
            }
        }

        public int RecordSize
        {
            get;
            private set;
        }

        public void Store(CurveIndex indexRecord, float[] points)
        {
            if (fileStream != null)
            {
                int ptNum = indexRecord.CurvePoint;

                List<byte> list = new List<byte>();
                list.AddRange(indexRecord.GetBytes());

                for (int i = 0; i < ptNum; i++)
                {
                    list.AddRange(BitConverter.GetBytes(points[i]));
                }

                int leftNum = this.RecordSize - list.Count;
                for (int i = 0; i < leftNum; i++)
                {
                    list.Add(0);
                }

                byte[] buffer = list.ToArray();
                try
                {
                    int offset=indexRecord.CurveFileIndex * this.RecordSize;
                    if (fileStream.Length <= offset)
                    {
                        fileStream.SetLength(fileStream.Length + IncreaseSize);
                    }
                    fileStream.Position = indexRecord.CurveFileIndex * this.RecordSize;

                    fileStream.Write(buffer, 0, this.RecordSize);
                    fileStream.Flush();
                }
                catch (Exception)
                {
 
                }
            }
        }

        public float[] GetCurvePoint(CurveIndex record)
        {
            int offset = record.CurveFileIndex * record.RecordLength;
            byte[] buffer = null;
            if (fileStream != null)
            {
                try
                {
                    if (fileStream.Length >= offset + record.RecordLength)
                    {
                        fileStream.Position = offset;

                        byte[] data = new byte[record.RecordLength];

                        fileStream.Read(data, 0, data.Length);

                        buffer = data;

                    }
                }
                catch (Exception) { }
            }
            if (buffer != null)
            {
                List<float> listPt = new List<float>();

                for (int i = 0; i < record.CurvePoint; i++)
                {
                    listPt.Add(BitConverter.ToSingle(buffer, CurveIndex.IndexSize + i * 4));
                }

                return listPt.ToArray();
            }

            return null;
        }


    }
}
