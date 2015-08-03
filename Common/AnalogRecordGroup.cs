using System;
using System.Collections.Generic;
using System.Text;

namespace Common
{
   public class AnalogRecordGroup
    {

       List<AnalogRecord> records;

       public AnalogRecordGroup(int index,List<AnalogRecord> records)
       {
           this.Index = index;
           this.records = records;
       }

       public int Index
       {
           get;
           private set;
       }

       public List<AnalogRecord> Records
       {
           get
           {
               return records;
           }
       }


    }
}
