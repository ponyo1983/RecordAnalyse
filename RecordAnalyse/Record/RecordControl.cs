using System;
using System.Collections.Generic;
using System.Text;
using RecordAnalyse.Utils;
using System.IO;

namespace RecordAnalyse.Record
{
    class RecordControl
    {

        string Tag = "";

        byte[] data=new byte[512];
        public RecordControl(DiskUtil disk)
        {
            this.IsValid = false;

            disk.Read(data, 0, data.Length);

            this.Tag = Encoding.ASCII.GetString(data, 0, 4);

            if (this.Tag != "ctrl") return;

            this.IsValid = true;

            this.TotalSector = BitConverter.ToInt32(data, 4);
            this.UsedSector = BitConverter.ToInt32(data, 8);


        }

        public bool IsValid
        {
            get;
            private set;
        }


        public int TotalSector
        {
            get;
            private set;
        }

        public int UsedSector
        {
            get;
            private set;
        }

        public void Write(FileStream fs)
        {
            fs.Write(data, 0, data.Length);
        }
    }
}
