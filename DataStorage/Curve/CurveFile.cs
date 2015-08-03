using System;
using System.Collections.Generic;
using System.Text;
using Common;

namespace DataStorage.Curve
{
    class CurveFile
    {

        private CurveManager manager = null;

        CurveIndexFile fileIndex;

        Dictionary<int, CurveRecordFile> dicFileData = new Dictionary<int, CurveRecordFile>();

        public CurveFile(CurveManager manager)
        {
            this.manager = manager;

            this.fileIndex = new CurveIndexFile(this);

        }

        public CurveManager DataManager
        {
            get
            {
                return manager;
            }
        }


        public void AddCurves(CurveGroup grp)
        {

            List<StationCurve> listCurve = grp.Curves;
            if ((listCurve == null) || (listCurve.Count <= 0)) return;

            DateTime tmCurve = DateTime.Now;
            for (int i = 0; i < listCurve.Count; i++)
            {
                if (listCurve[i] != null)
                {
                    tmCurve = listCurve[i].OccurTime;
                    break;
                }
            }
            for (int i = 0; i < listCurve.Count; i++)
            {
                int ptNum = (listCurve[i] == null) ? 0 : listCurve[i].Points.Length;
                CurveIndex indexRecord = fileIndex.NewRecord(ptNum);

                indexRecord.CurveTime = tmCurve;
                indexRecord.CurveType = (Int16)grp.Type;
                indexRecord.CrvIndex = (Int16)grp.Index;
                indexRecord.CurvePhase = (byte)((listCurve.Count == 1) ? 0 : (i + 1));
                indexRecord.Direction = (listCurve[i] == null) ? (byte)0 : listCurve[i].Dir;
                indexRecord.CurveMark = 0;
                indexRecord.SampleRate = (listCurve[i] == null) ? (Int16)0 : (Int16)listCurve[i].SampleRate;


                CurveStoreBlock blk = new CurveStoreBlock(this, indexRecord,  (listCurve[i] == null) ? null:listCurve[i].Points);

                manager.AddBlock(blk);


            }






        }

        public void Stroe(CurveIndex indexRecord, float[] points)
        {
            int size = indexRecord.RecordLength;

            if (dicFileData.ContainsKey(size)==false)
            {
                dicFileData.Add(size, new CurveRecordFile(this, size));
            }

            dicFileData[size].Store(indexRecord, points); //保存数据
            fileIndex.Store(indexRecord); //保存索引

        }

        public List<DateTime> GetCurveTimeList(int type,int index)
        {
            List<DateTime> listTime = new List<DateTime>();
            IList<CurveIndex> listRecord = fileIndex.AllIndex;

            //按最新的时间添加
            for (int i = 0; i < listRecord.Count; i++)
            {
                if (listRecord[i].IsValid == false) continue;
                if ((listRecord[i].CurveType == type) && (listRecord[i].CrvIndex == index) && (listRecord[i].CurvePhase <= 1)) //取第一相的时间
                {
                    listTime.Add(listRecord[i].CurveTime);
                }

            }
            return listTime;
        }

        public List<StationCurve> GetCurveHistory(int curveType, int curveindex, DateTime time)
        {
            List<StationCurve> list = new List<StationCurve>();
            //排列顺序为动作曲线(A,B,C)+参考曲线定位到反位(A,B,C)+参考曲线反位到定位(A,B,C)+摩擦曲线定位到X位(A,B,C)+摩擦曲线反位到X位(A,B,C)
            for (int i = 0; i < 3 ; i++)
            {
                list.Add(null);
            }

            IList<CurveIndex> listRecord = fileIndex.AllIndex;

            for (int i = listRecord.Count - 1; i >= 0; i--)
            {
                CurveIndex rcd = listRecord[i];
                byte mark = rcd.CurveMark;
                byte phase = rcd.CurvePhase;
                if (phase > 3) { phase = 3; }//不允许超过3个相位
                if ((rcd.CurveType == curveType) && (rcd.CrvIndex == curveindex)) //曲线类型匹配
                {
                    
                    if (Math.Abs((rcd.CurveTime-time).TotalSeconds)<2)
                    {
                        float[] pt = this.GetCurvePoint(rcd);
                        int index = (phase == 0) ? 0 : phase - 1;
                        //CurveRecord cr = new CurveRecord(time, "", McType.None, curveType, curveindex, phase, rcd.Direction, rcd.SampleRate, pt);

                        StationCurve cr = new StationCurve(rcd.CurveTime,rcd.SampleRate,pt);
                        list[index] = cr;

                    }
                 
                }
            }


            return list;
        }


        private float[] GetCurvePoint(CurveIndex record)
        {
            int recordSize = record.RecordLength;
            if (dicFileData.ContainsKey(recordSize) == false)
            {
                CurveRecordFile fileData = new CurveRecordFile(this, recordSize);
                dicFileData.Add(recordSize, fileData);
            }

            return dicFileData[recordSize].GetCurvePoint(record);
        }


      
    }
}
