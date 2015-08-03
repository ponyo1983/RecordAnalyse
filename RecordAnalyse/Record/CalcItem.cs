using System;
using System.Collections.Generic;
using System.Text;
using RecordAnalyse.Utils;

namespace RecordAnalyse.Record
{
  public  class CalcItem
    {

        static readonly int[] FreqList = new int[] { 0, 25, 50, 100, 200, 550, 650, 750, 850, 1700, 2000, 2300, 2600 };

        public CalcItem(byte[] data, int offset)
        {
            this.IsValid = false;
            if (data[offset] == 0xff) return;
            this.Gear = data[offset] & 0x03; //档位
            int freqIndex = (data[offset] >> 4) & 0x0f;
            this.Freq=0;
            if (freqIndex < FreqList.Length)
            {
                this.Freq = FreqList[freqIndex];
            }
            float[] fd=new float[1];
            ushort[] sd = new ushort[] { BitConverter.ToUInt16(data, offset + 1) };
            HalfFloat.Halfp2Singles(fd, sd, 1);
            this.CoeffK = fd[0];
            sd = new ushort[] { BitConverter.ToUInt16(data, offset + 3) };
            HalfFloat.Halfp2Singles(fd, sd, 1);
            this.CoeffB = fd[0];

            this.IsValid = true;

        }
        public bool IsValid
        {
            get;
            private set;
        }


        public int Freq
        {
            get;
            private set;
        }

        public int Gear
        {
            get;
            private set;
        }

        public float CoeffK
        {
            get;
            private set;
        }

        public float CoeffB
        {
            get;
            private set;
        }

       
    }
}
