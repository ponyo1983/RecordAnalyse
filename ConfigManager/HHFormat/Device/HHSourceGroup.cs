using System;
using System.Collections.Generic;
using System.Text;
using Common;

namespace ConfigManager.HHFormat.Device
{
    public class HHSourceGroup
    {

        List<HHDeviceProperty> listProp = new List<HHDeviceProperty>();

        public HHSourceGroup(int sourceIndex)
        {
 
        }

        public void AddProperty(HHDeviceProperty prop)
        {
            listProp.Add(prop);
        }

        public IList<HHDeviceProperty> Properties
        {
            get
            {

                return listProp.AsReadOnly();
            }
        }


        public bool AllowFM
        {
            get
            {
                bool allow=false;
                foreach (HHDeviceProperty prop in listProp)
                {
                    if (prop.MonitorType == SignalType.SignalCarrier)
                    {
                        allow = true;
                        break;
                    }
                    if (prop.MonitorType == SignalType.SignalLow)
                    {
                        allow = true;
                        break;
                    }
                }

                return allow;
            }

        }

        public int AllowCurve
        {
            get
            {
                int allow = 0;
                foreach (HHDeviceProperty prop in listProp)
                {
                    if (prop.MonitorType == SignalType.SignalACCurve)
                    {
                        allow = 1;
                        break;
                    }
                    if (prop.MonitorType == SignalType.SignalDCCurve)
                    {
                        allow = 2;
                        break;
                    }
                }

                return allow;
            }
        }

        public bool AllowAngle
        {
            get
            {
                bool allow = false;
                foreach (HHDeviceProperty prop in listProp)
                {
                    if (prop.MonitorType == SignalType.SignalAngle)
                    {
                        allow = true;
                        break;
                    }
                }

                return allow;
            }
        }
        
    }
}
