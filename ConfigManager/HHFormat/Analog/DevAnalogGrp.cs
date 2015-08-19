using System;
using System.Collections.Generic;
using System.Text;

namespace ConfigManager.HHFormat.Analog
{
   public class DevAnalogGrp
    {
        Dictionary<string, DevAnalog> dicAnalog = new Dictionary<string, DevAnalog>();

        public DevAnalog GetAnalog(string devName)
        {
            if (dicAnalog.ContainsKey(devName))
            {
                return dicAnalog[devName];
            }
            return null;
        }

        public DevAnalogGrp(IniDocument docIni, int index)
        {

            string name = docIni.GetString("模拟量类型", (index+1).ToString());
            if (string.IsNullOrEmpty(name)) return;
            this.Name = name;
            int type = docIni.GetInt(name, "类型",0);
            if (type <= 0) return;
            this.Type = type;
            this.Unit = docIni.GetString(name, "单位");
            int analogNum = docIni.GetInt(name, "数目", 0);

            float val = docIni.GetFloat(name, "AD最小", float.NaN);
            this.ADMin = float.IsNaN(val) ? 0 : val;
            val = docIni.GetFloat(name, "AD最大", float.NaN);
            this.ADMax = float.IsNaN(val) ? 0 : val;

            for (int i = 0; i < analogNum; i++)
            {
                DevAnalog analog = new DevAnalog(docIni,this,i);
                if (analog.IsValid)
                {
                    if (dicAnalog.ContainsKey(analog.Name) == false)
                    {
                        dicAnalog.Add(analog.Name, analog);
                    }
                }
            }
            if (dicAnalog.Count <= 0) return;

            this.IsValid = true;

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

        public int Type
        {
            get;
            private set;
        }
        public string Unit
        {
            get;
            private set;
        }


        public int AnalogNum
        {
            get;
            private set;
        }

        public float ADMin
        {
            get;
            private set;
        }

        public float ADMax
        {
            get;
            private set;
        }


    }
}
