using System;
using System.Collections.Generic;
using System.Text;
using RecordAnalyse.Record;

namespace RecordAnalyse.GUI
{
    public class SelectChannel
    {

        RecordFile file;
        int channel;
        public SelectChannel(RecordFile file, int channel, int index)
        {
            this.file = file;
            this.channel = channel;
            this.RecordIndex = index;
            this.BeginTime = file.BeginTime.ToString("yyyy-MM-dd HH:mm:ss");
            this.EndTime = file.EndTime.ToString("yyyy-MM-dd HH:mm:ss");

            int seconds = 0;
            if (file.EndTime > file.BeginTime)
            {
                seconds = (int)((file.EndTime - file.BeginTime).TotalSeconds);
                this.RecordSec = seconds;
                int h = seconds / (3600);
                int m = (seconds - h * (3600)) / 60;
                int s = seconds - h * (3600) - m * 60;

                this.TimeLength = h.ToString("00") + ":" + m.ToString("00") + ":" + s.ToString("00");

            }
            else
            {
                this.TimeLength = "---";
            }


            this.Channel = channel;
        }


        public int RecordSec
        {
            get;
            private set;
        }
        public bool Select
        {
            get;
            set;
        }

        public int RecordIndex
        {
            get;
            private set;

        }

        public string BeginTime
        {
            get;
            private set;
        }

        public string EndTime
        {
            get;
            private set;
        }

        public string TimeLength
        {
            get;
            private set;
        }

        public int Channel
        {
            get;
            private set;
        }


        public RecordFile File
        {
            get
            {
                return file;
            }
        }

        public string ToString()
        {
            return file.Disk.DriveName + "\\" + RecordIndex + ":" + channel;
            
        }
    }
}
