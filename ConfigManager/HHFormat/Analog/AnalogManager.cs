using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;

namespace ConfigManager.HHFormat.Analog
{
   public class AnalogManager
    {

        Dictionary<string, DevAnalogGrp> dicAnalogGrp = new Dictionary<string, DevAnalogGrp>();


        private AnalogManager() { }


        private static AnalogManager manager = null;
        public static AnalogManager GetInstance()
        {
            if (manager == null)
            {
                manager = new AnalogManager();
                string fieName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Config\模拟量.rhhcfg");
              
                manager.Load(fieName);
            }
            return manager;
        }


        public DevAnalog GetAnalog(string src, string devName)
        {
            if (dicAnalogGrp.ContainsKey(src))
            {
                return dicAnalogGrp[src].GetAnalog(devName);
            }
            return null;
        }

        private void Load(string fileName)
        {
            if (File.Exists(fileName) == false)
            {
                Assembly assembly = this.GetType().Assembly;
                System.IO.Stream smEmbeded = assembly.GetManifestResourceStream("ConfigManager.Config.模拟量.rhhcfg");


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
            IniDocument ini = new IniDocument();
            ini.Load(fileName);
            int num = ini.GetInt("模拟量类型", "数目", 0);
            for (int i = 0; i < num; i++)
            {
                DevAnalogGrp grp = new DevAnalogGrp(ini, i);
                if (grp.IsValid==false) continue;

                if (dicAnalogGrp.ContainsKey(grp.Name) == false)
                {
                    dicAnalogGrp.Add(grp.Name, grp);
                }
            }


        }


    }
}
