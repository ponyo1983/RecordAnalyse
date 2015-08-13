using System;
using System.Collections.Generic;
using System.Text;

namespace ConfigManager.HHFormat.Curve
{
   public class DevCurve
    {
        public DevCurve(IniDocument ini, CurveGroup grp, int index)
        {
            this.Group = grp;
            this.Index = index;

            this.Name = ini.GetString(grp.Name + "\\" + (index + 1), "设备名称");
            if (string.IsNullOrEmpty(this.Name)) return;
            if (this.Name.ToUpper() == "DUMMY") return;

            this.Tag = ini.GetInt(grp.Name + "\\" + (index + 1), "标志", 0);
            this.IsValid = true;
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
        public CurveGroup Group
        {
            get;
            private set;
        }
    }
}
