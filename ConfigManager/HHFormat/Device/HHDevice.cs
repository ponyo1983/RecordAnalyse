using System;
using System.Collections.Generic;
using System.Text;

namespace ConfigManager.HHFormat.Device
{
  public  class HHDevice
    {

        List<HHDeviceProperty> listProp = new List<HHDeviceProperty>();

        public HHDevice(IniDocument ini, int index, HHDeviceGrp devGrp)
        {
            this.DevGroup = devGrp;
            string sectionName = devGrp.Name + "\\设备" + (index + 1);
            this.Name = ini.GetString(sectionName, "设备名称");
            if (string.IsNullOrEmpty(this.Name)) return;
            IList<HHDeviceProperty> props = devGrp.Properties;
            for (int i = 0; i < props.Count; i++)
            {
                string propName = props[i].Name;

                string bindName = ini.GetString(sectionName, propName);


                listProp.Add(props[i].Bind(bindName));

            }

            for (int i = 0; i < listProp.Count; i++)
            {
                if (listProp[i].IsBind)
                {
                    this.IsValid = true;
                    break;
                }
            }


        }

        public HHDeviceProperty GetProperty(HHDeviceProperty prop)
        {
            HHDeviceProperty devProp = null;
            for (int i = 0; i < listProp.Count; i++)
            {
                if (listProp[i].Name == prop.Name)
                {
                    devProp = listProp[i];
                    break;
                }
            }

            return devProp;
        }

        public HHDeviceGrp DevGroup
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

        public override string ToString()
        {
            return this.Name;
        }
        
    }
}
