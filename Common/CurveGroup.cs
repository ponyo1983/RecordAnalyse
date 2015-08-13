using System;
using System.Collections.Generic;
using System.Text;

namespace Common
{
    public class CurveGroup
    {
        List<StationCurve> listCurve = new List<StationCurve>();


        public CurveGroup(int type, int index,DateTime time, int sampleRate, float[] data)
        {
            this.Type = type;
            this.Index = index;

            StationCurve curve = new StationCurve(time, sampleRate, data);
            listCurve.Add(curve);
        }

        public CurveGroup(int phaseNum, int type, int index)
        {
            this.Type = type;
            this.Index = index;
            for (int i = 0; i < phaseNum; i++)
            {
                listCurve.Add(null);
            }
        }

        public void AddCurve(int phase, DateTime time, int sampleRate, float[] data)
        {
            listCurve[phase] = new StationCurve(time, sampleRate, data);
        }


        public int Type
        {
            get;
            private set;
        }

        public int Index
        {
            get;
            private set;
        }

        public List<StationCurve> Curves
        {
            get
            {
                return listCurve;
            }
        }
    }
}
