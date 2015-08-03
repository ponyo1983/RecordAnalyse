using System;
using System.Collections.Generic;
using System.Text;
using Common;

namespace ConfigManager.Device
{
    public class SourceGroup
    {

        List<DeviceProperty> listProp = new List<DeviceProperty>();

        public SourceGroup(int sourceIndex)
        {
 
        }

        public void AddProperty(DeviceProperty prop)
        {
            listProp.Add(prop);
        }

        public IList<DeviceProperty> Properties
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
                foreach(DeviceProperty prop in listProp)
                {
                    if (prop.SignalType == SignalType.SignalCarrier)
                    {
                        allow = true;
                        break;
                    }
                    if (prop.SignalType == SignalType.SignalLow)
                    {
                        allow = true;
                        break;
                    }
                }

                return allow;
            }

        }

        public bool AllowCurve
        {
            get
            {
                bool allow = false;
                foreach (DeviceProperty prop in listProp)
                {
                    if (prop.SignalType == SignalType.SignalACCurve)
                    {
                        allow = true;
                        break;
                    }
                    if (prop.SignalType == SignalType.SignalDCCurve)
                    {
                        allow = true;
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
                foreach (DeviceProperty prop in listProp)
                {
                    if (prop.SignalType == SignalType.SignalAngle)
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
