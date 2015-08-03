using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace UltraChart
{
    public class ChartObject
    {


        public ChartObject()
        {
            this.Visible = true;
        }

        public ChartGraph Chart { get; set; }

        /// <summary>
        /// 窗体绘制框
        /// </summary>
        public RectangleF Rectangle { get; set; }


        /// <summary>
        /// 打印绘制框
        /// </summary>
        public RectangleF PrintRectangle { get;set;}


       

        public bool IsFold
        {
            get;
            set;
        }

        public bool IsShowFoldFlag
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public virtual bool Visible
        {
            get;
            set;
        }

        public virtual void Print(Graphics g, RectangleF rect) { }
        public virtual void Draw(Graphics g, RectangleF rect) { }

        public virtual void DrawStage2(Graphics g, RectangleF rect) { }

        public virtual bool HitTest(Point p)
        {
            return false;
        }

        public virtual void Action()
        { 
        }


        public virtual void ClearData()
        {
            
        }
    }
}
