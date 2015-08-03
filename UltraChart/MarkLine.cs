using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace UltraChart
{
   public class MarkLine : ChartObject
    {
       public MarkLine(ChartGraph chart, long timePos, int width, Color color)
       {
           this.TimePos = timePos;
           this.LineWidth = width;
           this.LineColor = color;
       }

       public int LineWidth
       {
           get;
           set;
       }

       public Color LineColor
       {
           get;
           set;
       }

       public long TimePos
       {
           get;
           set;
       }
    }
}
