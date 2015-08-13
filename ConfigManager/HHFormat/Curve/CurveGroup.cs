using System;
using System.Collections.Generic;
using System.Text;

namespace ConfigManager.HHFormat.Curve
{
   public class CurveGroup
    {

        Dictionary<string, List<DevCurve>> dicCurve = new Dictionary<string,List< DevCurve>>();

        public CurveGroup(IniDocument docIni, int index)
        {
           this.Name = docIni.GetString("记录曲线类型", (index + 1).ToString());
            if (string.IsNullOrEmpty(Name)) return;
       
            this.Type = docIni.GetInt(this.Name, "类型", 0);
            if (this.Type <= 0) return;
   
   
            int curveNum = docIni.GetInt(this.Name, "数目", 0);
            for (int i = 0; i < curveNum; i++)
            {
                DevCurve curve = new DevCurve(docIni, this, i);
                if (curve.IsValid)
                {
                    if (dicCurve.ContainsKey(curve.Name) == false)
                    {
                        List<DevCurve> listCurve = new List<DevCurve>();
                        listCurve.Add(curve);
                        dicCurve.Add(curve.Name, listCurve);
                    }
                    else
                    {
                        dicCurve[curve.Name].Add(curve);
                    }
                }
            }
            if (dicCurve.Count <= 0) return;

            this.IsValid = true;
        }

        public bool IsValid
        {
            get;
            private set;
        }

        public string Name
        {
            get;
            private set;
        }

        public int Type
        {
            get;
            private set;
        }

        public List<DevCurve> GetCurves(string name)
        {
            if (dicCurve.ContainsKey(name))
            {
                return dicCurve[name];
            }
            return null;
        }
    }
}
