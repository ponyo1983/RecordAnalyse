using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Common;

namespace DataStorage.Curve
{
    class CurveRequestBlock:IDataProcessBlock
    {
        ManualResetEvent eventFinished = new ManualResetEvent(false);

        public CurveRequestBlock(CurveFile file,int curveType,int curveIndex,DateTime time)
        {
            this.File = file;
            this.CurveType = curveType;
            this.CurveIndex = curveIndex;
            this.CurveTime = time;
        }

        public CurveFile File
        {
            get;
            private set;
        }
        public int CurveType
        {
            get;
            private set;
        }
        public int CurveIndex
        {
            get;
            private set;
        }
        public DateTime CurveTime
        {
            get;
            private set;
        }


        public List<StationCurve> Result
        {
            get;
            private set;
        }


        public bool Wait(int millsecond)
        {
           return eventFinished.WaitOne(millsecond, false);
        }

        #region IDataProcessBlock 成员

        public bool Process()
        {
            this.Result = this.File.GetCurveHistory(this.CurveType, this.CurveIndex, this.CurveTime);
            eventFinished.Set();
            return true;
        }

        #endregion
    }
}
