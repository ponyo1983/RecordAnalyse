using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace DataStorage.Curve
{
    class CurveIndexFile
    {
        const int MaxIndexNum = 1024 * 100; //每天最多1024条*100天
        const int MaxFileSize=100*1024*1024;
        List<CurveIndex> listIndex = new List<CurveIndex>();

        Dictionary<int, int> dicDataIndex = new Dictionary<int, int>(); //每个数据文件的最近的记录索引 0:文件记录大小 1:记录索引

        CurveFile curveFile;

        FileStream fileStream;

        public CurveIndexFile(CurveFile curveFile)
        {
            this.curveFile = curveFile;
            byte[] buffer = new byte[MaxIndexNum * CurveIndex.IndexSize];
            Array.Clear(buffer, 0, buffer.Length);
            try
            {
                string indexName = Path.Combine(curveFile.DataManager.StoreDir,  "Curve.idx");
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
            for (int i = 0; i < MaxIndexNum; i++)
            {
                CurveIndex indexRecord = new CurveIndex(i, buffer, i * CurveIndex.IndexSize);
                listIndex.Add(indexRecord);
            }
            listIndex.Sort();

            //TODO:必须加入对重叠的Index处理
            for (int i = 0; i < listIndex.Count; i++)
            {
                if (listIndex[i].IsValid == false) break;

                for (int j = i + 1; j < listIndex.Count; j++)
                {
                    if (listIndex[j].IsValid && listIndex[j].ConflictWith(listIndex[i]))
                    {
                        for (int k = j; k < listIndex.Count; k++)
                        {
                            listIndex[k].FileIndex = 0; //设置为无效
                        }
                        break;

                    }
                }

            }

            for (int i = listIndex.Count - 1; i >= 0; i--)
            {
                if (listIndex[i].IsValid == false) continue;

                int recordSize = listIndex[i].RecordLength;

                if (dicDataIndex.ContainsKey(recordSize) == false)
                {
                    dicDataIndex.Add(recordSize, listIndex[i].CurveFileIndex);
                }
                else
                {
                    dicDataIndex[recordSize]= listIndex[i].CurveFileIndex;
                }
                
            }

        }

        public CurveIndex LastIndex
        {
            get
            {
                return listIndex[0];
            }

        }

        public IList<CurveIndex> AllIndex
        {
            get
            {
                return listIndex.AsReadOnly();
            }
        }

        public CurveIndex NewRecord(int curvePoint)
        {

            int length = CurveIndex.IndexSize + curvePoint * 4;
            int needSize = 1;
            for (int i = 1; i <= 20; i++)
            {
                needSize = 1 << i;
                if (needSize >= length) break;
            }
            int dataRecordIndex = 0;
            if (dicDataIndex.ContainsKey(needSize))
            {
                dataRecordIndex = dicDataIndex[needSize];
                dataRecordIndex += 1;
                if (dataRecordIndex * needSize >= MaxFileSize) //返回
                {
                    dataRecordIndex = 0;
                }
                dicDataIndex[needSize] = dataRecordIndex;
            }
            else
            {
                dicDataIndex.Add(needSize, 0);
            }
            if (needSize < length)
            {
                curvePoint = (needSize - CurveIndex.IndexSize) / 4;
            }

            CurveIndex recordLast = listIndex[listIndex.Count - 1];
            CurveIndex recordFirst = listIndex[0];
           
            recordLast.FileIndex = recordFirst.FileIndex + 1; //文件记录索引
            recordLast.CurvePoint = curvePoint;
            recordLast.CurveFileIndex = dataRecordIndex;
            recordLast.RecordLength = needSize; //记录长度
           
            listIndex.Remove(recordLast);
            listIndex.Insert(0, recordLast);
            return recordLast;
        }


        public void Store(CurveIndex record)
        {
            if (record == null) return;
            if (fileStream != null)
            {
                byte[] data = record.GetBytes();

                try
                {
                    fileStream.Position = record.Index * CurveIndex.IndexSize;
                    fileStream.Write(data, 0, data.Length);
                    fileStream.Flush();
                }
                catch (Exception) { }
            }
        }
    }
}
