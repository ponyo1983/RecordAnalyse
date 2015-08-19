using System;
using System.Collections.Generic;
using System.Text;
using ConfigManager.HHFormat.Analog;
using Common;
using ConfigManager.HHFormat.Curve;
using System.ComponentModel;

namespace ConfigManager.HHFormat.Device
{
    public class HHDeviceProperty
    {






        private HHDeviceProperty() { }

        public HHDeviceProperty(IniDocument ini, HHDeviceGrp grp, int index)
        {
            this.DevGrp = grp;
            string sectionName = grp.Name + "\\属性" + (index + 1);
            this.Type = ini.GetString(sectionName, "类型");
            if (string.IsNullOrEmpty(Type)) return;
            this.DisplayName = ini.GetString(sectionName, "显示名称");
            if (string.IsNullOrEmpty(DisplayName)) return;
            this.Name = ini.GetString(sectionName, "名称");
            if (string.IsNullOrEmpty(Name)) return;
            this.DataSrc = ini.GetString(sectionName, "数据来源");
            if (string.IsNullOrEmpty(DataSrc)) return;
            string monitorType = ini.GetString(sectionName, "室外监测类型");
            if (string.IsNullOrEmpty(monitorType)) return;
            this.MonitorType = SignalType.SignalAC;
            switch (monitorType)
            {
                case "直流":
                    this.MonitorType = SignalType.SignalDC;
                    break;
                case "载频":
                    this.MonitorType = SignalType.SignalCarrier;
                    break;
                case "低频":
                    this.MonitorType = SignalType.SignalLow;
                    break;
                case "直流道岔":
                    this.MonitorType = SignalType.SignalDCCurve;
                    break;
                case "交流道岔":
                    this.MonitorType = SignalType.SignalACCurve;
                    break;
                case "相位角":
                    this.MonitorType = SignalType.SignalAngle;
                    break;
            }

            string monitorGrp = ini.GetString(sectionName, "室外监测数据源");
            if (string.IsNullOrEmpty(monitorGrp)) return;

            string[] grps = monitorGrp.Split(new char[]{'-'},StringSplitOptions.RemoveEmptyEntries);
            this.GroupIndex = int.Parse(grps[0]);
            this.GroupCount = 1;
            if (grps.Length > 1)
            {
                this.GroupCount = int.Parse(grps[1]);
            }


            this.Unit = ini.GetString(sectionName, "单位");

            this.IsValid = true;
        }


        public HHDeviceGrp DevGrp
        {
            get;
            private set;
        }


        public bool Selected
        {
            get;
            set;
        }
        public bool IsValid
        {
            get;
            private set;
        }

        public string Type
        {
            get;
            private set;
        }
        public string DisplayName
        {
            get;
            private set;
        }

        public string Name
        {
            get;
            private set;
        }

        public string DataSrc
        {
            get;
            private set;
        }

        public SignalType MonitorType
        {
            get;
            private set;
        }

        public string Unit
        {
            get;
            private set;
        }

        public DevAnalog Analog
        {
            get;
            private set;
        }

        [Browsable(false)]
        public List<DevCurve> Curves
        {
            get;
            private set;
        }

        public int GroupIndex
        {
            get;
            private set;
        }

        public int GroupCount
        {
            get;
            private set;
        }

        public HHDeviceProperty Bind(string bindName)
        {

            HHDeviceProperty prop = new HHDeviceProperty();
            prop.DataSrc = this.DataSrc;
            prop.DisplayName = this.DisplayName;
            prop.Name = this.Name;
            prop.Type = this.Type;
            prop.GroupIndex = this.GroupIndex;
            prop.GroupCount = this.GroupCount;
          
            if (string.IsNullOrEmpty(bindName) == false)
            {
                switch (this.Type)
                {
                    case "模拟量":
                        prop.Analog = AnalogManager.GetInstance().GetAnalog(this.DataSrc, bindName);
                        if (prop.Analog != null)
                        {
                            prop.IsBind = true;
                        }
                        break;
                    case "曲线":
                        prop.Curves = CurveManager.GetInstance().GetCurves(this.DataSrc, bindName);
                        if (prop.Curves != null)
                        {
                            prop.IsBind = true;
                        }
                        break;
                }
            }

            return prop;



        }

        public bool IsBind
        {
            get;
            private set;
        }

        public override string ToString()
        {
            return this.DisplayName;
        }


    }
}
