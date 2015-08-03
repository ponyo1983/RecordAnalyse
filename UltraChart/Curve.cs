using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace UltraChart
{
    public class Curve:ChartObject
    {
      protected  List<BarPoint> listBarPoint = new List<BarPoint>();
      protected List<LinePoint> listLinePoint = new List<LinePoint>();

       
        public Curve(ChartGraph chart)
        {
            this.Chart = chart;
        }

        public virtual bool AddPoint(BarPoint barPoint)
        {
            listBarPoint.Add(barPoint);
            return true;
        }

        public virtual bool AddPoint(LinePoint linePoint)
        {
            listLinePoint.Add(linePoint);
            return true;
        }

        public virtual void ClearPoints()
        {
            listBarPoint.Clear();
            listLinePoint.Clear();
        }

        public bool GetPoint(int pointIndex, ref BarPoint barPoint)
        {
            return true;
        }

        public bool GetPoint(int pointIndex, ref LinePoint linePoint)
        {
            return true;
        }

        protected bool SetCurveColor(ushort val, Color[] colors)
        {
            if (colors == null)
            {
                return false;
            }
            return true;
        }

        public int MaxPointCount
        {
            get;
            set;
        }

        public Color NameColor
        {
            get;
            set;
        }

        public int PointCount
        {
            get
            {
                return listLinePoint.Count;
            }
        }
    }
}
