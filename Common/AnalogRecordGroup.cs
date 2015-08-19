using System;
using System.Collections.Generic;
using System.Text;

namespace Common
{
   public class AnalogRecordGroup:IComparable
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



       #region IComparable 成员

       public int CompareTo(object obj)
       {
           AnalogRecordGroup grp = obj as AnalogRecordGroup;

           if (grp.records.Count <= 0 || this.records.Count <= 0) return 0;

           return this.records[0].Time.CompareTo(grp.records[0].Time);

       }

       #endregion
    }
}
