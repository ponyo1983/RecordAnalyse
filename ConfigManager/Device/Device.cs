using System;
using System.Collections.Generic;
using System.Text;

namespace ConfigManager.Device
{
   public class Device
    {


        public Device(string name,int type, int index)
        {
            this.Name = name;
            this.Index = index;
            this.DevType = DeviceTypeManager.GetInstance().GetType(type);
        }

        public DeviceType DevType
        {
            get;
            private set;
        }

        public string Name
        {
            get;
            private set;
        }

        public int Index
        {
            get;
            private set;
        }
    }
}
