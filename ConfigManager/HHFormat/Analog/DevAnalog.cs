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

            string section = grp.Name + "\\" + (index + 1);

            this.Name = ini.GetString(section, "设备名称");
            if (string.IsNullOrEmpty(this.Name)) return;
            if (this.Name.ToUpper() == "DUMMY") return;

    

            float val=  ini.GetFloat(section, "AD最小", float.NaN);

            this.ADMin = float.IsNaN(val) ? grp.ADMin : val;
           

            val = ini.GetFloat(section, "AD最大", float.NaN);

            this.ADMax = float.IsNaN(val) ? grp.ADMax : val;

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
