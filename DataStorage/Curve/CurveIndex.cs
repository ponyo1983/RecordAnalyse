using System;
using System.Collections.Generic;
using System.Text;

namespace DataStorage.Curve
{
    class CurveIndex : IComparable
    {
        public const int IndexSize = 32;

        public CurveIndex(int index, byte[] buffer, int offset)
        {
            this.Index = index;

            FileIndex = BitConverter.ToInt32(buffer, offset);

            uint time = BitConverter.ToUInt32(buffer, offset + 4); //开始时间
            this.CurveTime = Utility.Unix2DateTime(time);

            this.CurveType = BitConverter.ToInt16(buffer, offset + 8);
            this.CrvIndex = BitConverter.ToInt16(buffer, offset + 10);

            this.CurvePhase = buffer[offset + 12];
            this.Direction = buffer[offset + 13];
            this.CurveMark = buffer[offset + 14];
            this.SampleRate = BitConverter.ToInt16(buffer, offset + 15);

            this.CurvePoint = BitConverter.ToInt16(buffer, offset + 17);
            this.RecordLength = BitConverter.ToInt32(buffer, offset + 19);
            this.CurveFileIndex = BitConverter.ToInt32(buffer, offset + 23);
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

        public bool ConflictWith(CurveIndex other)
        {
            if (other.RecordLength != this.RecordLength) return false;

            if (other.CurveFileIndex != this.CurveFileIndex) return false;

            return true;
        }

        /// <summary>
        /// Unix格式
        /// </summary>
        public DateTime CurveTime
        {
            get;
            set;
        }
        /// <summary>
        /// 曲线类型
        /// </summary>
        public Int16 CurveType
        {
            get;
            set;
        }
        /// <summary>
        /// 曲线索引
        /// </summary>
        public Int16 CrvIndex
        {
            get;
            set;
        }
        /// <summary>
        /// 相位
        /// </summary>
        public byte CurvePhase
        {
            get;
            set;
        }
        /// <summary>
        /// 转换方向
        /// </summary>
        public byte Direction
        {
            get;
            set;
        }
        /// <summary>
        /// 摩擦 参考标记
        /// </summary>
        public byte CurveMark
        {
            get;
            set;
        }
        public Int16 SampleRate
        {
            get;
            set;
        }
        public int CurvePoint
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
        /// 在数据文件中的记录索引
        /// </summary>
        public int CurveFileIndex
        {
            get;
            set;
        }

        

        public byte[] GetBytes()
        {
            byte[] data = new byte[IndexSize];
            //记录ID
            BitConverter.GetBytes(this.FileIndex).CopyTo(data, 0);
            //时间
            uint time = Utility.DateTime2Unix(this.CurveTime);
            BitConverter.GetBytes(time).CopyTo(data, 4);
            //类型
            BitConverter.GetBytes(this.CurveType).CopyTo(data, 8);
            BitConverter.GetBytes(this.CrvIndex).CopyTo(data, 10);
            data[12] = this.CurvePhase;
            data[13] = this.Direction;
            data[14] = this.CurveMark;
            BitConverter.GetBytes(this.SampleRate).CopyTo(data, 15);
            BitConverter.GetBytes(this.CurvePoint).CopyTo(data, 17);
            BitConverter.GetBytes(this.RecordLength).CopyTo(data, 19);
            BitConverter.GetBytes(this.CurveFileIndex).CopyTo(data, 23);
            return data;
        }

        #region IComparable 成员

        public int CompareTo(object obj)
        {
            CurveIndex record = obj as CurveIndex;
            if (record.FileIndex == this.FileIndex)
            {
                return record.Index - this.Index;
            }
            else
            {
                return record.FileIndex - this.FileIndex;
            }
        }

        #endregion
    }
}
