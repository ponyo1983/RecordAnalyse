using System;
using System.Collections.Generic;
using System.Text;

namespace ConfigManager.Device
{
    public class DeviceType
    {
        const int MaxSelectNum = 3;

        List<DeviceProperty> listProperty = new List<DeviceProperty>();

        List<DeviceProperty> listAnalogProperty = new List<DeviceProperty>();
        List<DeviceProperty> listDCProperty = new List<DeviceProperty>();


        Dictionary<int, SourceGroup> dicSource = new Dictionary<int, SourceGroup>();

        List<int> listSel = new List<int>();

        public DeviceType(string name,int typeId)
        {
            this.Name = name;
            this.TypeID = typeId;
        }
        public string Name
        {
            get;
            private set;
        }
        public int TypeID
        {
            get;
            private set;
        }


        public void AddProperty(DeviceProperty prop)
        {
            listProperty.Add(prop);
            if (prop.Type == 1)
            {
                listAnalogProperty.Add(prop);
            }
            else if (prop.Type == 2)
            {
                listDCProperty.Add(prop);
            }
        }


        public void BuildGroup()
        {
            dicSource.Clear();

            for (int i = 0; i < listProperty.Count; i++)
            {
                int sigIndex=listProperty[i].GroupIndex;
                if (dicSource.ContainsKey(sigIndex)==false)
                {
                    dicSource.Add(sigIndex, new SourceGroup(sigIndex));
                }
                dicSource[sigIndex].AddProperty(listProperty[i]);

            }
        }


        public List<SourceGroup> SourceGroups
        {

            get
            {
                List<SourceGroup> listGrp = new List<SourceGroup>();
                foreach (SourceGroup grp in dicSource.Values)
                {
                    listGrp.Add(grp);
                }

                return listGrp;
            }
        }

        public List<DeviceProperty> AnalogProperties
        {
            get
            {
                return listAnalogProperty;
            }
        }

        public List<DeviceProperty> DCProperties
        {
            get
            {
                return listDCProperty;
            }
        }

        public void SelectProperty(int index,bool select)
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

    }
}
