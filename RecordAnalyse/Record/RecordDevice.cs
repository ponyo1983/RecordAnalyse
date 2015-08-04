using System;
using System.Collections.Generic;
using System.Text;
using RecordAnalyse.Utils;
using System.IO;

namespace RecordAnalyse.Record
{
    class RecordDevice
    {

        string Tag = "";

        DeviceInfo devInfo;
        byte[] sectorData = new byte[512];

        public RecordDevice(DiskUtil disk)
        {
            this.IsValid = false;
            disk.Read(sectorData, 0, 512);

            this.Tag = Encoding.ASCII.GetString(sectorData, 0, 4);


            if (this.Tag != " dev") return;

            this.IsValid = true;

            this.Time = new DateTime(sectorData[4] +2000, sectorData[5], sectorData[6], sectorData[7], sectorData[8], sectorData[9]);

            this.devInfo = new DeviceInfo(sectorData, 10);

            this.UsedSector = BitConverter.ToInt32(sectorData, 138);
            this.UsedRate = BitConverter.ToInt16(sectorData, 142);
            this.Overflow = BitConverter.ToInt32(sectorData, 144);
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

        public void Write(FileStream fs)
        {
            fs.Write(sectorData, 0, 512);
        }

    }
}
