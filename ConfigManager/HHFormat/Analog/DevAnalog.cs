using System;
using System.Collections.Generic;
using System.Text;

namespace ConfigManager.HHFormat.Analog
{
    public class DevAnalog
    {
        DevAnalogGrp grp;
        public DevAnalog(IniDocument ini,DevAnalogGrp grp, int index)
        {
            this.grp = grp;
            this.Index = index;

            this.Name = ini.GetString(grp.Name + "\\" + (index + 1), "设备名称");
            if (string.IsNullOrEmpty(this.Name)) return;
            if (this.Name.ToUpper() == "DUMMY") return;

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
    }
}
