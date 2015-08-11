using System;
using System.Collections.Generic;
using System.Text;
using ConfigManager.HHFormat.Device;
using System.IO;

namespace ConfigManager.HHDevice
{
   public class HHDeviceManager
    {
        Dictionary<string, HHDeviceGrp> dicDevGrp = new Dictionary<string, HHDeviceGrp>();


        List<HHDeviceGrp> listGrp = new List<HHDeviceGrp>();


        private HHDeviceManager() { }

        static HHDeviceManager manager = null;

        public static HHDeviceManager GetInstance()
        {
            if (manager == null)
            {
                manager = new HHDeviceManager();
                string fieName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Config\设备.rhhcfg");
                manager.Load(fieName);
            }
            return manager;
        }

        private void Load(string fileName)
        {
            if (File.Exists(fileName) == false) return;
            IniDocument ini = new IniDocument();
            ini.Load(fileName);
            int num = ini.GetInt("设备", "数目", 0);

            for (int i = 0; i < num; i++)
            {
                HHDeviceGrp devGrp = new HHDeviceGrp(ini,i);
                if (devGrp.IsValid)
                {
                    listGrp.Add(devGrp);
                }

            }

        }

    }
}
