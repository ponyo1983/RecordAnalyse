using System;
using System.Collections.Generic;
using System.Text;

namespace RecordAnalyse.Record
{
  public  class DeviceInfo
    {

        public DeviceInfo(byte[] data, int offset)
        {
            this.DevType = data[offset];
           // this.DeliveryDate = new DateTime(data[offset + 1] + 2000, data[offset + 2], data[offset + 3]);
           
            this.Person = ASCIIEncoding.ASCII.GetString(data, offset + 6, 8).TrimEnd(new char[]{'\0'}); //操作人员
            this.OperationTime = new DateTime(data[offset + 14] + 2000, data[offset + 15], data[offset + 16]); //操作时间

            this.DeliveryDate = new DateTime(data[offset + 17] + 2000, data[offset + 18], data[offset + 19]); //出厂日期

            int idLen = data[offset + 20];
            this.ID = ASCIIEncoding.ASCII.GetString(data, offset + 21, idLen);

            int typeLen = data[offset + 20+idLen+1];
            this.DevCode = ASCIIEncoding.ASCII.GetString(data, offset + 20 + idLen + 2, typeLen);
        }

        public byte DevType
        {
            get;
            private set;
        }

        public DateTime DeliveryDate
        {
            get;
            private set;
        }

    

        public string Person
        {
            get;
            private set;
        }

        public DateTime OperationTime
        {
            get;
            private set;
        }

        public string ID
        {
            get;
            private set;
        }

        /// <summary>
        /// 设备型号编码
        /// </summary>
        public string DevCode
        {
            get;
            private set;
        }
    }

}
