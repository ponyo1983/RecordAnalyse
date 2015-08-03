using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using Common;

namespace ConfigManager.Device
{
  public  class DeviceTypeManager
    {


        List<DeviceType> listDeviceType = new List<DeviceType>();

        Dictionary<int, DeviceType> dicDevType = new Dictionary<int, DeviceType>();
        static DeviceTypeManager manager = null;

        private DeviceTypeManager() { }

        public static DeviceTypeManager GetInstance()
        {
            if (manager == null)
            {
                manager = new DeviceTypeManager();

                string fileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Config/DeviceType.xml");
                if (File.Exists(fileName))
                {
                    manager.LoadXml(fileName);
                }
            }

            return manager;
        }

        private void LoadXml(string fileName)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(fileName);

            XmlNode nodeDevices = doc.SelectSingleNode("Devices");

            

            foreach(XmlNode nodeDevice in nodeDevices.ChildNodes)
            {
     
                string name=nodeDevice.ChildNodes[0].InnerText;
                int typeId = int.Parse(nodeDevice.ChildNodes[1].InnerText);
                DeviceType devType = new DeviceType(name,typeId);

                int propIndex = 0;
                foreach (XmlNode nodeProp in nodeDevice.ChildNodes[2])
                {

                    string propName = nodeProp.ChildNodes[0].InnerText;
                    int type = int.Parse(nodeProp.ChildNodes[1].InnerText);

                    string[] srcSignal = nodeProp.ChildNodes[2].InnerText.Split('@');

                    string unit = nodeProp.ChildNodes[3].InnerText;
                    string[] rangeTxt = nodeProp.ChildNodes[5].InnerText.Split('-');
                    int grpIndex = 0;
                    int grpCnt = 1;
                    string[] grpVals = srcSignal[0].Split('-');
                    grpIndex = int.Parse(grpVals[0]);
                    if (grpVals.Length > 1)
                    {
                        grpCnt = int.Parse(grpVals[1]);
                    }
                    SignalType sigType = (SignalType)int.Parse(srcSignal[1]);

                    float rangeLow = float.Parse(rangeTxt[0]);
                    float rangeHigh = float.Parse(rangeTxt[1]);



                    DeviceProperty devProp = new DeviceProperty(propName, type,unit, grpIndex,grpCnt, propIndex,sigType,rangeLow,rangeHigh);
                    propIndex++;
                    devType.AddProperty(devProp);
                }

                devType.BuildGroup();
                devType.SelectProperty(0, true);
                listDeviceType.Add(devType);
                if (dicDevType.ContainsKey(typeId) == false)
                {
                    dicDevType.Add(typeId, devType);
                }

            }
        }

        public IList<DeviceType> DeviceTypes
        {
            get
            {
                return listDeviceType.AsReadOnly();
            }
        }

        public DeviceType GetType(int type)
        {
            if (dicDevType.ContainsKey(type))
            {
                return dicDevType[type];
            }

            return null;
        }

    }
}
