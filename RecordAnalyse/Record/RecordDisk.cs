using System;
using System.Collections.Generic;
using System.Text;
using RecordAnalyse.Utils;
using System.Windows.Forms;
using System.IO;

namespace RecordAnalyse.Record
{
  public  class RecordDisk
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
            int index = 0;
            while (true)
            {
                RecordFile file = new RecordFile(disk, offset,index++);
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


        public bool ExportOK
        {
            get;
          private  set;
        }


        int exportFileIndex = 0;
        long exportedSize = 0;
        public float CurrentFileRate
        {
            get
            {
                return listFile[exportFileIndex].ExportLength*1f/listFile[exportFileIndex].Length;
            }
        }

        public float ExportRate
        {
            get
            {
                long totalSize = 0;
                for (int i = 0; i < listFile.Count; i++)
                {
                    RecordFile file = listFile[i];

                    if (file.Select)
                    {
                        totalSize += file.Length;
                    }
                }
                if (totalSize == 0) return 1;


                return (listFile[exportFileIndex].ExportLength + exportedSize) * 1f / totalSize;
            }
        }

       

        public void Export(FileStream fs)
        {
            this.ExportOK = false;
            recordControl.Write(fs);

            for (int i = 0; i < listFile.Count; i++)
            {
                RecordFile file = listFile[i];

                if (file.Select)
                {
                    exportFileIndex = i;
                    file.Write(fs);
                    exportedSize += file.Length;
                }
            }
            this.ExportOK = true;
        }

    }
}
