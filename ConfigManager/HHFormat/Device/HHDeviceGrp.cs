using System;
using System.Collections.Generic;
using System.Text;
using ConfigManager.HHFormat.Analog;

namespace ConfigManager.HHFormat.Device
{
  public  class HHDeviceGrp
    {

        List<HHDeviceGrp> listGrp = new List<HHDeviceGrp>();

        List<HHDevice> listDevice = new List<HHDevice>();
        List<HHDeviceProperty> listProperty = new List<HHDeviceProperty>();

        List<HHDeviceProperty> listAnalogProperty = new List<HHDeviceProperty>();
        Dictionary<int, HHSourceGroup> dicSource = new Dictionary<int, HHSourceGroup>();

        List<int> listSel = new List<int>();
        const int MaxSelectNum = 3;

        public HHDeviceGrp(IniDocument ini, int index)
        {
            string name = ini.GetString("设备", (index + 1).ToString());
            if (string.IsNullOrEmpty(name)) return;

            this.DevType = ini.GetInt(name, "设备类型", 0);
            if (this.DevType <= 0) return;

            this.Name = name;

            int propNum = ini.GetInt(name, "属性数目", 0);
            if (propNum <= 0) return;

            int devNum = ini.GetInt(name, "设备数目", 0);
            if (devNum <= 0) return;

            for (int i = 0; i < propNum; i++)
            {

                HHDeviceProperty prop = new HHDeviceProperty(ini, name, i);
                if (prop.IsValid)
                {
                    listProperty.Add(prop);
                    if (prop.Type == "模拟量")
                    {
                        listAnalogProperty.Add(prop);
                    }
                }
            }
            if (this.DevType != 4 && this.DevType != 8)
            {
                if (listProperty.Count <= 0) return;
            }
            if (listAnalogProperty.Count > 0)
            {
                SelectProperty(0, true);
            }
            for (int i = 0; i < devNum; i++)
            {
                HHDevice device = new HHDevice(ini, i, this);

                if (device.IsValid)
                {
                    listDevice.Add(device);
                }


            }

            this.BuildGroup();
            this.Level = 1;
            this.IsValid = true;
        }

        public string Name
        {
            get;
            private set;
        }

        public int DevType
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
        public void AddGrp(List<HHDeviceGrp> grp)
        {
            this.listGrp = grp;
            this.Level = 2;
        }

        public void BuildGroup()
        {
            dicSource.Clear();

            for (int i = 0; i < listProperty.Count; i++)
            {
                int sigIndex = listProperty[i].GroupIndex;
                if (dicSource.ContainsKey(sigIndex) == false)
                {
                    dicSource.Add(sigIndex, new HHSourceGroup(sigIndex));
                }
                dicSource[sigIndex].AddProperty(listProperty[i]);

            }
        }


        public IList<HHDeviceGrp> ChildGrp
        {
            get
            {
                return listGrp.AsReadOnly();

            }
        }


        public IList<HHDevice> Devices
        {
            get
            {
                return listDevice.AsReadOnly();
            }
        }

        public IList<HHDeviceProperty> Properties
        {
            get
            {
                return listProperty.AsReadOnly();
            }
        }

        public List<HHSourceGroup> SourceGroups
        {
            get
            {
                List<HHSourceGroup> listGrp = new List<HHSourceGroup>();
                foreach (HHSourceGroup grp in dicSource.Values)
                {
                    listGrp.Add(grp);
                }

                return listGrp;
            }
        }


        public IList<HHDeviceProperty> AnalogProperties
        {
            get
            {
                return listAnalogProperty.AsReadOnly();
            }
        }

        public void SelectProperty(int index, bool select)
        {
            if (listSel.Contains(index))
            {
                if (select == false)
                {
                    listSel.Remove(index);
                    listAnalogProperty[index].Selected = false;
                }
            }
            else
            {
                if (select)
                {
                    if (listSel.Count >= MaxSelectNum)
                    {
                        int preIndex = listSel[0];
                        listSel.RemoveAt(0);
                        listAnalogProperty[preIndex].Selected = false;
                    }
                    listSel.Add(index);
                    if (listAnalogProperty.Count > 0)
                    {
                        listAnalogProperty[index].Selected = true;
                    }
                }
            }
        }


        public override string ToString()
        {
            return this.Name;
        }
             
    }
}
