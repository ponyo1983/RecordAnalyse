using System;
using System.Collections.Generic;
using System.Text;
using RecordAnalyse.Utils;
using System.Windows.Forms;

namespace RecordAnalyse.Record
{
    class RecordDisk
    {

        RecordControl recordControl;
        List<RecordFile> listFile = new List<RecordFile>();

        DiskUtil disk;

        public RecordDisk(string driverName)
        {
            disk = new DiskUtil(driverName);

            recordControl = new RecordControl(disk);

            if (recordControl.IsValid == false) return;

            long offset = 512;
            while (true)
            {
                RecordFile file = new RecordFile(disk, offset);
                if (file.Length == 0) break;


                listFile.Add(file);
                offset += file.Length;
            }

        }


        public IList<RecordFile> RecordList
        {
            get
            {
                return listFile.AsReadOnly();
            }
        }

    }
}
