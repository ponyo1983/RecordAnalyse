using System;
using System.Collections.Generic;
using System.Text;

namespace DataStorage.Analog
{
    struct AnalogPoint
    {
        DateTime time;
        float analogValue;
        byte digitValue;

        public AnalogPoint(DateTime timeBase,byte[] data, int offset)
        {
            uint timeDiff = BitConverter.ToUInt32(data,offset);
            digitValue = (byte)(timeDiff & 0x0f);
            this.time = timeBase.AddMilliseconds(timeDiff >> 4);
            analogValue = BitConverter.ToSingle(data, offset + 4);
        }

        public AnalogPoint(DateTime time, float analog, byte digit)
        {
            this.time = time;
            this.analogValue = analog;
            this.digitValue = digit;
        }

        public DateTime Time
        {
            get { return time; }
        }

        public float AnalogValue
        {
            get { return analogValue; }
        }

        public byte DigitValue
        {
            get { return digitValue; }
        }
    }
}
