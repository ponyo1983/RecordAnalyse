using System;
using System.Collections.Generic;
using System.Text;

namespace UltraChart
{
  public  class YAxes
    {

      
        public YAxesMode Mode
        {
            get;
            set;
        }

        public YAxesMode YAxesMode
        {
            get { return this.Mode; }
            set { this.Mode = value; }
        }

        public float YAxesMin
        {
            get;
            set;
        }
        public float YAxesMax
        {
            get;
            set;
        }

      
        public string UnitString
        {
            get;
            set;
        }
        public int Precision
        {
            get;
            set;
        }


    }
}
