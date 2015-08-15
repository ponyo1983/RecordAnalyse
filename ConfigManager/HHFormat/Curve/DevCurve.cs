using System;
using System.Collections.Generic;
using System.Text;
using Common;

namespace ConfigManager.HHFormat.Curve
{
   public class DevCurve
    {
       public DevCurve(IniDocument ini, CurveGroup grp, int index)
       {
           this.Group = grp;
           this.Index = index;
           string section = grp.Name + "\\" + (index + 1);
           this.Name = ini.GetString(section, "设备名称");
           if (string.IsNullOrEmpty(this.Name)) return;
           if (this.Name.ToUpper() == "DUMMY") return;

           this.ADMax = grp.ADMax;
           this.ADMin = grp.ADMin;

           float limit = ini.GetFloat(section, "AD最小", float.NaN);
           if (float.IsNaN(limit) == false)
           {
               this.ADMin = limit;
           }
           limit = ini.GetFloat(section, "AD最大", float.NaN);
           if (float.IsNaN(limit) == false)
           {
               this.ADMax = limit;
           }

           this.Tag = ini.GetInt(grp.Name + "\\" + (index + 1), "标志", 0);

           this.TimeInterval = grp.TimeInterval;
           float interval = ini.GetFloat(grp.Name + "\\" + (index + 1), "时间间隔", float.NaN);
           string monitor = ini.GetString(grp.Name + "\\" + (index + 1), "室外监测类型");
           this.MonitorType = SignalType.SignalACCurve;
           if (monitor == "直流道岔")
           {
               this.MonitorType = SignalType.SignalDCCurve;
           }
           if (float.IsNaN(interval) == false)
           {
               this.TimeInterval = interval;
           }



           this.IsValid = true;
       }


        public SignalType MonitorType
        {
            get;
            private set;
        }

        public int Index
        {
            get;
            private set;
        }
        public string Name
        {
            get;
            private set;
        }
        public bool IsValid
        {
            get;
            private set;
        }

        public int Tag
        {
            get;
            private set;

        }
        public float ADMax
        {
            get;
            private set;
        }

        public float ADMin
        {
            get;
            private set;
        }

        public float TimeInterval
        {
            get;
            private set;
        }

        public CurveGroup Group
        {
            get;
            private set;
        }
    }
}
