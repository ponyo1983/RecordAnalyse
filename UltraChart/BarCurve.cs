using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace UltraChart
{
   public class BarCurve : Curve
    {
        private float PaddingHeight = 2.5f;
        const int ScaleSize = CurveGroup.ScaleSize;

        Dictionary<int, Color> dicColor = new Dictionary<int, Color>();

        public BarCurve(ChartGraph chart, string name, int maxPointCount)
            : base(chart)
        {
            this.Name = name;
            this.MaxPointCount = maxPointCount;
        }

        public bool SetBarColor(int val, Color color)
        {
            
            if (dicColor.ContainsKey(val))
            {
                dicColor[val] = color;
            }
            else
            {
                dicColor.Add(val, color);
            }

            return true;
        }

       /// <summary>
       /// 设置指定值的颜色
       /// 颜色使用数组的原因为显示信号机状态时可能显示为两种颜色，如绿黄，红黄等
       /// 更理想的情况应该是一个类，除了显示颜色，还要显示器对应的宽度比例
       /// </summary>
       /// <param name="val"></param>
       /// <param name="colors"></param>
       /// <returns></returns>
        public bool SetBarColor(ushort val, Color[] colors)
        {
            if (colors == null || colors.Length <= 0) return false;
            if (dicColor.ContainsKey(val))
            {
                dicColor[val] = colors[0];
            }
            else
            {
                dicColor.Add(val, colors[0]);
            }

            return true;
        }

        public CurveGroup CurveGroup
        {
            get;
            set;
        }

        private RectangleF BorderRect
        {
            get
            {
                RectangleF rectBorder = new RectangleF(this.Rectangle.X, this.Rectangle.Y + PaddingHeight, this.Rectangle.Width, this.Rectangle.Height - PaddingHeight * 2);

                return rectBorder;
            }
        }

        public RectangleF BorderRectPrint
        {
            get
            {
                RectangleF rectBorder = new RectangleF(this.PrintRectangle.X, this.PrintRectangle.Y + PaddingHeight, this.PrintRectangle.Width, this.PrintRectangle.Height - PaddingHeight * 2);

                return rectBorder;
            }
        }


        private Color GetColor(int val)
        {
            if (dicColor.ContainsKey(val))
            {
                return dicColor[val];
            }
            return Color.Black;
        }
        private void DrawBarValue(int barValue, long startTime, long endTime, Graphics g, SolidBrush brush,bool print)
        {
            XAxes xAxes = this.CurveGroup.XAxes;
            RectangleF rectBorder =print?this.BorderRectPrint:this.BorderRect;
            if (barValue >= 0 && endTime > startTime)
            {
                float posX1 = (startTime - xAxes.OriginalTime) * ScaleSize * 1f / xAxes.TimeScale;
                float posX2 = (endTime - xAxes.OriginalTime) * ScaleSize * 1f / xAxes.TimeScale;

                if (posX1 < rectBorder.Width && posX2 > 0)
                {
                    if (posX1 < 0)
                    {
                        posX1 = 0;
                    }
                    if (posX2 > rectBorder.Width)
                    {
                        posX2 = rectBorder.Width;
                    }
                    brush.Color = GetColor(barValue);
                    g.FillRectangle(brush, posX1 + rectBorder.X, rectBorder.Y, posX2 - posX1, rectBorder.Height);

                }
            }
        }
        private void DrawBar(Graphics g, RectangleF rect,bool print)
        {
            RectangleF rectBorder =print?this.BorderRectPrint: this.BorderRect;
            XAxes xAxes = this.CurveGroup.XAxes;

            int pCnt = listBarPoint.Count;

            int barValue = -1;
            long startTime = 0;
            long endTime = 0;

            using (SolidBrush brush = new SolidBrush(Color.Black))
            {
                for (int i = 0; i < pCnt; i++)
                {
                    BarPoint bp = listBarPoint[i];
                    if (barValue < 0) //无效点
                    {
                        barValue = bp.Value;
                        startTime = bp.Time;
                        endTime = startTime;
                    }
                    else if (bp.Time < endTime)
                    {
                        //开始绘制
                        DrawBarValue(barValue, startTime, endTime, g, brush,print);
                        barValue = bp.Value;
                        startTime = bp.Time;
                        endTime = bp.Time;
                    }
                    else if (bp.Value != barValue)
                    {

                        endTime = bp.Time;
                        //开始绘制
                        DrawBarValue(barValue, startTime, endTime, g, brush,print);
                        barValue = bp.Value;
                        startTime = bp.Time;

                    }
                    else
                    {
                        endTime = bp.Time;
                    }

                }
                DrawBarValue(barValue, startTime, endTime, g, brush,print);

            }

        }

        private void DrawBorder(Graphics g, RectangleF rect,bool print)
        {
            RectangleF rectBorder =print?this.BorderRectPrint: this.BorderRect;
            using (Pen penBorder = new Pen(Color.FromArgb(74, 176, 72), 1.5f))
            {
                g.DrawRectangle(penBorder, rectBorder.X, rectBorder.Y, rectBorder.Width, rectBorder.Height);
            }
        }

        private void DrawName(Graphics g, RectangleF rect,bool print)
        {
            RectangleF rectBorder =print?this.BorderRectPrint: this.BorderRect;
            StringFormat sf = new StringFormat();
            sf.Alignment = StringAlignment.Center;
            sf.LineAlignment = StringAlignment.Center;
            using (SolidBrush brush = new SolidBrush(NameColor))
            {
                using (Font font = new Font("雅黑", 9f,FontStyle.Bold))
                {
                   
                    g.DrawString(this.Name, font, brush, rectBorder, sf);
                }
            }
        }

        private void Paint(Graphics g, RectangleF rect, bool print)
        {
            if (this.Visible == false) return;
            DrawBar(g, rect, print);

            DrawBorder(g, rect, print);
            DrawName(g, rect, print);
        }

        public override void Print(Graphics g, RectangleF rect)
        {
            Paint(g, rect, true);
        }
        public override void Draw(Graphics g, RectangleF rect)
        {
            Paint(g, rect,false);
        }

        public override void DrawStage2(Graphics g, RectangleF rect)
        {
            if (this.CurveGroup.ShowCursor == false) return;
            Point pCursor = CurveGroup.CursorPoint;
            RectangleF borderRect = this.BorderRect;
            using (Pen pen = new Pen(Color.LightGray, 1))
            {
                pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Custom;
                pen.DashPattern = new float[] { 4.5f, 4.5f };
                if ((pCursor.X >= borderRect.Left) && (pCursor.X <= borderRect.Right))
                {
                    g.DrawLine(pen, pCursor.X, borderRect.Top, pCursor.X, borderRect.Bottom);
                }
                if (pCursor.Y >= borderRect.Top && pCursor.Y <= borderRect.Bottom)
                {
                    g.DrawLine(pen, borderRect.Left, pCursor.Y, borderRect.Right, pCursor.Y);
                }

            }
        }
        public override void ClearData()
        {

            listBarPoint.Clear();

         
        }
    }
    
}
