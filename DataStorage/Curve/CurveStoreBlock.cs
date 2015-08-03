using System;
using System.Collections.Generic;
using System.Text;

namespace DataStorage.Curve
{
    class CurveStoreBlock:IDataProcessBlock
    {


        CurveFile file;

        CurveIndex indexRecord;

        float[] points;



        public CurveStoreBlock(CurveFile file, CurveIndex indexRecord, float[] points)
        {
            this.file = file;
            this.indexRecord = indexRecord;
            this.points = points;
        }



        #region IDataProcessBlock 成员

        public bool Process()
        {
            file.Stroe(indexRecord, points);
            return true;
        }
        public bool Wait(int milliSec)
        {
            return true;
        }

        #endregion
    }
}
