using System;
using System.Collections.Generic;
using System.Text;
using ConfigManager.HHFormat.Device;
using System.IO;
using System.Reflection;

namespace ConfigManager.HHDevice
{
   public class HHDeviceManager
    {
        Dictionary<string, HHDeviceGrp> dicDevGrp = new Dictionary<string, HHDeviceGrp>();


        List<HHDeviceGrp> listGrp = new List<HHDeviceGrp>();


        List<HHDeviceGrp> listGrpSort = new List<HHDeviceGrp>();

        List<HHDeviceGrp> listGrpUnsort = new List<HHDeviceGrp>();

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
            if (File.Exists(fileName) == false)
            {
                if (File.Exists(fileName) == false)
                {
                    Assembly assembly = this.GetType().Assembly;
                    System.IO.Stream smEmbeded = assembly.GetManifestResourceStream("ConfigManager.Config.设备.rhhcfg");


                    byte[] data = new byte[smEmbeded.Length];

                    smEmbeded.Read(data, 0, data.Length);


                    //建立目录

                    string dir = Path.GetDirectoryName(fileName);
                    if (Directory.Exists(dir) == false)
                    {
                        Directory.CreateDirectory(dir);
                    }


                    File.WriteAllBytes(fileName, data);


                    smEmbeded.Close();
                }
            }
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

            //区间轨道电路

            List<HHDeviceGrp> grpsQJ = new List<HHDeviceGrp>();
            HHDeviceGrp grpQJ = null;

            List<HHDeviceGrp> grpsZN = new List<HHDeviceGrp>();
            HHDeviceGrp grpZN = null;

            for (int i = 0; i < listGrp.Count; i++)
            {
                int devType=listGrp[i].DevType;
                switch (devType)
                {
                    case 8: //区间轨道电路
                        grpQJ = listGrp[i];
                        listGrpSort.Add(grpQJ);
                        break;
                    case 10: //无绝缘移频
                        grpsQJ.Add(listGrp[i]);
                        break;
                    case 30: //有绝缘移频
                        grpsQJ.Add(listGrp[i]);
                        break;
                    case 208: //ZPW2000
                        grpsQJ.Add(listGrp[i]);
                        break;
                    case 4: //站内轨道电路
                        grpZN = listGrp[i];
                        listGrpSort.Add(grpZN);
                        break;
                    case 11: //25HZ轨道电路
                        grpsZN.Add(listGrp[i]);
                        break;
                    case 16: //480轨道电路
                        grpsZN.Add(listGrp[i]);
                        break;
                    case 17: //高压不对称
                        grpsZN.Add(listGrp[i]);
                        break;
                    case 25: //驼峰
                        grpsZN.Add(listGrp[i]);
                        break;
                    default:
                        listGrpSort.Add(listGrp[i]);
                        break;
                        
                }
                if (devType != 4 && devType != 8)
                {
                    listGrpUnsort.Add(listGrp[i]);
                }
               
            }

            if (grpQJ != null)
            {
                grpQJ.AddGrp(grpsQJ);
            }
            else
            {
                listGrpSort.AddRange(grpsQJ);
            }
            if (grpZN != null)
            {
                grpZN.AddGrp(grpsZN);
            }
            else
            {
                listGrpSort.AddRange(grpsZN);
            }




        }


        public IList<HHDeviceGrp> DeviceGroups
        {
            get
            {
                return listGrpSort.AsReadOnly();
            }
        }

        public IList<HHDeviceGrp> DeviceGroupsUnsort
        {
            get { return listGrpUnsort.AsReadOnly(); }
        }

    }
}
