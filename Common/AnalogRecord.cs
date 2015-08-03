using System;
using System.Collections.Generic;
using System.Text;

namespace Common
{
  public  class AnalogRecord
    {
      public AnalogRecord(DateTime time,float value)
      {
          this.Time = time;
          this.Value = value;
      }

      public DateTime Time
      {
          get;
          private set;
      }

      public float Value
      {
          get;
          private set;
      }
    }
}
