using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace RecordAnalyse.Record
{
    class DiskManager
    {

        Dictionary<string, RecordDisk> dicDisk = new Dictionary<string, RecordDisk>();
        private DiskManager()
        { }
        static DiskManager manager;
        public static DiskManager GetInstance()
        {
            if (manager == null)
            {
                manager = new DiskManager();
            }
            return manager;
        }
        public RecordDisk GetDisk(string drive)
        {
            if (dicDisk.ContainsKey(drive)==false)
            {
                dicDisk.Add(drive, new RecordDisk(drive));
            }
            return dicDisk[drive];
        }

        public void Reset()
        {
            foreach (RecordDisk disk in dicDisk.Values)
            {
                disk.Close();
            }

            dicDisk = new Dictionary<string, RecordDisk>();

        }
    }
}
