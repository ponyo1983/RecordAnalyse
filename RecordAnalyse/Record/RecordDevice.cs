using System;
using System.Collections.Generic;
using System.Text;
using RecordAnalyse.Utils;

namespace RecordAnalyse.Record
{
    class RecordDevice
    {

        string Tag = "";

        DeviceInfo devInfo;
        public RecordDevice(DiskUtil disk)
        {
            this.IsValid = false;
            byte[] data = new byte[512];
            disk.Read(data, 0, 512);

            this.Tag = Encoding.ASCII.GetString(data, 0, 4);


            if (this.Tag != " dev") return;

            this.IsValid = true;

            this.Time = new DateTime(data[4] +2000, data[5], data[6], data[7], data[8], data[9]);

            this.devInfo = new DeviceInfo(data, 10);

            this.UsedSector = BitConverter.ToInt32(data, 138);
            this.UsedRate = BitConverter.ToInt16(data, 142);
            this.Overflow = BitConverter.ToInt32(data, 144);
        }

        public bool IsValid
        {
            get;
            private set;
        }

        public DateTime Time
        {
            get;
            private set;

        }

        public int UsedSector
        {
            get;
            private set;
        }
        public int UsedRate
        {
            get;
            private set;
        }

        public int Overflow
        {
            get;
            private set;

        }



    }
}
