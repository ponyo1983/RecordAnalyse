using System;
using System.Collections.Generic;
using System.Text;

namespace ConfigManager.HHFormat.Device
{
    class HHDeviceGrp
    {

        List<HHDeviceGrp> listGrp = new List<HHDeviceGrp>();

        public HHDeviceGrp(IniDocument ini, int index)
        {
            string name = ini.GetString("设备", (index + 1).ToString());
            if (string.IsNullOrEmpty(name)) return;

            this.DevTypeName = name;

            int propNum = ini.GetInt(name, "属性数目", 0);
            if (propNum <= 0) return;

            int devNum = ini.GetInt(name, "设备数目", 0);
            if (devNum <= 0) return;

            
           
        }

        public string DevTypeName
        {
            get;
            private set;
        }

        /// <summary>
        /// 树形的高度
        /// </summary>
        public int Level
        {
            get;
            private set;
        }

        public bool IsValid
        {
            get;
            private set;
        }
    }
}
