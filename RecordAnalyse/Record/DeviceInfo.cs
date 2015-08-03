using System;
using System.Collections.Generic;
using System.Text;

namespace RecordAnalyse.Record
{
    class DeviceInfo
    {

        public DeviceInfo(byte[] data, int offset)
        {
            this.DevType = data[offset];
            this.DeliveryDate = new DateTime(data[offset + 1] + 2000, data[offset + 2], data[offset + 3]);
            this.ID = BitConverter.ToInt16(data, offset + 4);
            this.Person = ASCIIEncoding.ASCII.GetString(data, offset + 6, 8).TrimEnd(new char[]{'\0'});
            this.OperationTime = new DateTime(data[offset + 14] + 2000, data[offset + 15], data[offset + 16]);

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

        public Int16 ID
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
    }

}
