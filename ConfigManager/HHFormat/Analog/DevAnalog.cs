using System;
using System.Collections.Generic;
using System.Text;

namespace ConfigManager.HHFormat.Analog
{
    public class DevAnalog
    {
       
        public DevAnalog(IniDocument ini,DevAnalogGrp grp, int index)
        {
            this.Group = grp;
            this.Index = index;

            this.Name = ini.GetString(grp.Name + "\\" + (index + 1), "设备名称");
            if (string.IsNullOrEmpty(this.Name)) return;
            if (this.Name.ToUpper() == "DUMMY") return;

            string min = ini.GetString(grp.Name + "\\" + (index + 1), "AD最小");
            string max = ini.GetString(grp.Name + "\\" + (index + 1), "AD最大");

            float val = 0;

            float.TryParse(min, out val);
            this.ADMin = val;
            float.TryParse(max, out val);
            this.ADMax = val;

            this.IsValid = true;



        }

        public int Index
        {
            get;
            private set;
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

        public DevAnalogGrp Group
        {
            get;
            private set;
        }

        public float ADMin
        {
            get;
            private set;
        }

        public float ADMax
        {
            get;
            private set;
        }
    }
}
