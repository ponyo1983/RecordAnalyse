using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;

namespace ConfigManager.Device
{
   public class DeviceManager
    {

       Dictionary<int, List<Device>> dicDevice = new Dictionary<int, List<Device>>();

       List<Device> listDevice = new List<Device>();

        private DeviceManager() { }

        static DeviceManager manager = null;

        public static DeviceManager GetInstance()
        {
            if (manager == null)
            {
                manager = new DeviceManager();

                string fileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Config/Devices.xml");
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



            foreach (XmlNode nodeDevice in nodeDevices.ChildNodes)
            {

              string name=  nodeDevice.Attributes["Name"].Value;
              int type = int.Parse(nodeDevice.Attributes["Type"].Value);


              if (dicDevice.ContainsKey(type))
              {
                  int devIndex = dicDevice[type].Count;
                  Device dev = new Device(name, type, devIndex);

                  dicDevice[type].Add(dev);
              }
              else
              {
                  Device dev = new Device(name, type, 0);

                  dicDevice.Add(type, new List<Device>());
                  dicDevice[type].Add(dev);
              }


                
            }

            
        }


        public Device GetDevice(string name, int type, bool forceNew)
        {
            Device dev = null;

            if (dicDevice.ContainsKey(type))
            {
                List<Device> list = dicDevice[type];

                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i].Name == name)
                    {
                        dev = list[i];
                        break;
                    }
                }
            }

            if (dev == null && forceNew)
            {
                if (dicDevice.ContainsKey(type))
                {
                    dev = new Device(name, type, 0);
                    dicDevice[type].Add(dev);
                }
                else
                {
                    dev = new Device(name, type, 0);
                    List<Device> list = new List<Device>();
                    list.Add(dev);
                    dicDevice.Add(type, list);
                }
            }

            return dev;
        }


        public List<Device> GetDevices(int type)
        {
            if (dicDevice.ContainsKey(type))
            {
                return dicDevice[type];
            }
            return new List<Device>();
        }
    }
}
