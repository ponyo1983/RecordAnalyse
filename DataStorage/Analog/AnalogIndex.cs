using System;
using System.Collections.Generic;
using System.Text;

namespace DataStorage.Analog
{
    class AnalogIndex:IComparable
    {
        public const int IndexSize = 32;
        public AnalogIndex(int index, byte[] buffer, int offset)
        {
            this.Index = index;

            FileIndex = BitConverter.ToInt32(buffer, offset);

            uint time = BitConverter.ToUInt32(buffer, offset + 4); //开始时间
            this.BeginTime = Utility.Unix2DateTime(time);
            time = BitConverter.ToUInt32(buffer, offset + 8); //结束时间
            this.EndTime = Utility.Unix2DateTime(time);

            this.BeginOffset = BitConverter.ToUInt32(buffer, offset + 12);
            this.RecordLength = BitConverter.ToInt32(buffer, offset + 16);
            this.RealLength = RecordLength;

            this.Type = (FallbackType)buffer[offset + 20];
        }
        public int Index
        {
            get;
            private set;
        }

        /// <summary>
        /// 文件记录的索引
        /// </summary>
        public int FileIndex
        {
            get;
            set;
        }

        public bool IsValid
        {
            get
            {
                return (this.FileIndex > 0) ? true : false;
            }
        }

        public bool ConflictWith(AnalogIndex other,long maxFileSize)
        {
            long[] sect1 = new long[4];
            long[] sect2 = new long[4];

            long leftLen=maxFileSize-this.BeginOffset;

            sect1[0] = this.BeginOffset;
            sect1[1] = leftLen > this.RecordLength ? this.RecordLength : leftLen;
            sect1[2] = 0;
            sect1[3] = this.RecordLength-leftLen;

            leftLen = maxFileSize - other.BeginOffset;
            sect2[0] = other.BeginOffset;
            sect2[1] = leftLen > other.RecordLength ? other.RecordLength : leftLen;
            sect2[2] = 0;
            sect2[3] = other.RecordLength - leftLen;

            for (int i = 0; i < sect1.Length/2; i++)
            {
                long sec1Size = sect1[i * 2 + 1];
                if (sec1Size <= 0) continue;
                long sec1Begin = sect1[i * 2];
                for (int j = 0; j < sect2.Length / 2; j++)
                {
                    long sec2Size = sect2[j * 2 + 1];
                    if (sec2Size <= 0) continue;
                    long sec2Begin = sect2[j * 2];

                    if (sec2Begin >= sec1Begin + sec1Size) continue;
                    if (sec1Begin >= sec2Begin + sec2Size) continue;

                    return true;
                }

            }

            return false;
        }

        /// <summary>
        /// Unix格式
        /// </summary>
        public DateTime BeginTime
        {
            get;
            set;
        }
        public DateTime EndTime
        {
            get;
            set;
        }

        public uint BeginOffset
        {
            get;
            set;
        }

        /// <summary>
        /// 写到磁盘上的长度
        /// </summary>
        public int RecordLength
        {
            get;
            set;
        }

        /// <summary>
        /// 实际接收数据的长度
        /// </summary>
        public int RealLength
        {
            get;
            set;
        }

        /// <summary>
        /// 记录的类型 0：时间连续 1：时间回溯
        /// </summary>
        public FallbackType Type
        {
            get;
            set;
        }

        public byte[] GetBytes()
        {
            byte[] data = new byte[IndexSize];
            BitConverter.GetBytes(this.FileIndex).CopyTo(data, 0);
            uint time = Utility.DateTime2Unix(this.BeginTime);
            BitConverter.GetBytes(time).CopyTo(data, 4);
            time = Utility.DateTime2Unix(this.EndTime);
            BitConverter.GetBytes(time).CopyTo(data, 8);
            BitConverter.GetBytes(this.BeginOffset).CopyTo(data, 12);
            BitConverter.GetBytes(this.RecordLength).CopyTo(data, 16);
            data[20] = (byte)this.Type;
            return data;
        }

        #region IComparable 成员

        public int CompareTo(object obj)
        {
            AnalogIndex record = obj as AnalogIndex;
            if (record.FileIndex == this.FileIndex)
            {
                return  record.Index-this.Index;
            }
            else
            {
                return record.FileIndex-this.FileIndex;
            }
        }

        #endregion
    }
}
