using System;
using System.Collections.Generic;
using System.Text;
using Common;


namespace ConfigManager.Device
{
  public  class DeviceProperty
    {


        public DeviceProperty(string name,int type,string unit,int grpIndex, int grpCnt,int propIndex, SignalType sigType,float rangeLow,float rangeHigh)
        {
            this.Name = name;
            this.Type = type;
            this.Unit = unit;
            this.GroupIndex = grpIndex;
            this.GroupCount = grpCnt;
            this.SignalType = sigType;
            this.PropertyIndex = propIndex;
            this.RangeLow = rangeLow;
            this.RangeHigh = rangeHigh;
        
        }

        public string Unit
        {
            get;
            private set;
        }
        public float RangeLow
        {
            get;
            private set;
        }

        public float RangeHigh
        {
            get;
            private set;
        }

        public string Name
        {
            get;
            private set;
        }

        public int Type
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


        public int PropertyIndex
        {
            get;
            private set;
        }

        public SignalType SignalType
        {
            get;
            private set;
        }

        public bool Selected
        {
            get;
            set;
        }

      

    }
}
