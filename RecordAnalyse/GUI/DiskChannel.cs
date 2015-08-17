using System;
using System.Collections.Generic;
using System.Text;
using RecordAnalyse.Record;
using System.ComponentModel;

namespace RecordAnalyse.GUI
{
    class DiskChannel
    {
        BindingList<SelectChannel> channels = new BindingList<SelectChannel>();

        public DiskChannel(RecordDisk disk)
        {
            
            for (int i = 0; i < disk.RecordList.Count; i++)
            {
                RecordFile file = disk.RecordList[i];
                for (int j = 0; j < file.Channels.Count; j++)
                {
                    SelectChannel channel = new SelectChannel(disk, j + 1, i + 1);
                    channels.Add(channel);
                }
            }
        }

        public BindingList<SelectChannel> Channels
        {
            get { return channels; }
        }
    }
}
