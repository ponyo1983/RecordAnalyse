using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace UltraChart
{
    public class LineArea : ChartObject
    {
       public const float CaptionHeight = 28;
       public const float XScaleHeight = 15;
        const float FoldBoxSize = 12;
        const int ScaleSize = CurveGroup.ScaleSize;

        const int SelectBoxWidth = 40;

        public const long TicksPerDay = 24L * 60L * 60L * 1000L * 1000L;
        public const long TicksPerHour = 1L * 60L * 60L * 1000L * 1000L;
        public const long TicksPerMinute = 1L * 60L * 1000L * 1000L;
        public const long TicksPerSecond = 1L * 1000L * 1000L;
        public const long TicksPerMiliSec = 1000L;
     
        ChartGraph chart;

        bool visibal = false;

        private List<LineCurve> lines = new List<LineCurve>();


        RectangleF rectFold = new RectangleF(0, 0, 0, 0);
        bool rectFoldHit = false;

        public LineArea(ChartGraph chart, string name, bool shareY)
        {
            this.chart = chart;
            this.Name = name;
            this.IsShareYAxes = shareY;
            this.YAxes = new YAxes();
            this.SharedYAxes = new YAxes();
            this.XFreqScale = -1;

        }

        public CurveGroup CurveGroup
        {
            get;
            set;
        }
        public override bool Visible
        {
            get
            {
                return this.visibal;
            }
            set
            {
                if (this.visibal != value)
                {
                    this.visibal = value;
                    if (chart != null)
                    {
                        chart.Draw();
                    }
                }
               
               
            }
        }
        public bool AddLine(LineCurve line)
        {
            this.lines.Add(line);
            line.LineArea = this;
            return true;
        }

        public bool IsShareYAxes
        {
            get;
            private set;
        }

        public List<LineCurve> LineList
        {
            get
            {
                return this.lines;
            }
        }

        public YAxes SharedYAxes
        {
            get;
            set;
        }



        public YAxes YAxes
        {
            get;
            private set;
        }

        public YAxes YAxesValid
        {
            get
            {
                if (lines.Count > 0)
                {
                    if (lines[0].YAxes.Mode == YAxesMode.Auto)
                    {
                        return SharedYAxes;
                    }
                }
                return this.YAxes;
            }
        }


        public IList<LineCurve> Lines
        {
            get { return lines; }
        }

        public override bool HitTest(Point p)
        {
            if (this.visibal==false) return false;
            rectFoldHit = false;
            if (rectFold.Contains(p))
            {
                rectFoldHit = true;
                return true;
            }
            for (int i = 0; i < lines.Count; i++)
            {
                lines[i].LastHitTest = false;
            }
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].RectSel.Contains(p))
                {
                    lines[i].LastHitTest = true;
                    return true;
                }
            }
            return false;
        }

        public override void Action()
        {
            if (rectFoldHit)
            {
                this.IsFold = !this.IsFold;
            }
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].LastHitTest)
                {
                    lines[i].Visible = !lines[i].Visible;
                }
            }
        }

        private void DrawYScale(Graphics g, RectangleF rect,bool print)
        {
            if (this.IsFold) return;

            RectangleF disRect =print?this.PrintRectangle: this.Rectangle;
         //   RectangleF borderRect = new RectangleF(disRect.X, disRect.Top + CaptionHeight, disRect.Width, disRect.Height - CaptionHeight - XScaleHeight);
            RectangleF borderRect = disRect;

            float yScaleHor = borderRect.Bottom - ScaleSize;
            StringFormat sf = new StringFormat();
            sf.Alignment = StringAlignment.Far;
            sf.LineAlignment = StringAlignment.Center;
            using (Font font = new Font("雅黑", 8f))
            {
                using (SolidBrush brush = new SolidBrush(this.chart.ForeColor))
                {
                    if (print)
                    {
                        brush.Color = Color.Black;
                    }
                    using (Pen pen = new Pen(this.chart.GridColor, 1f))
                    {
                        if (print)
                        {
                            pen.Color = Color.LightGray;
                        }
                        pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Custom;
                        pen.DashPattern = new float[] { 3, 3 };

                        if (this.YAxes.Mode == YAxesMode.Digital)
                        {

                            DrawDigitYScale(g, borderRect, yScaleHor, sf, font, brush, pen);
                        }
                        else
                        {
                            StringBuilder valFmt = new StringBuilder("0");
                            if (this.YAxes.Precision > 0)
                            {
                                valFmt.Append(".");
                                for (int i = 0; i < this.YAxes.Precision; i++)
                                {
                                    valFmt.Append("0");
                                }
                            }

                            RectangleF rectTxt;
                            while (yScaleHor > borderRect.Top)
                            {
                                g.DrawLine(pen, borderRect.X, yScaleHor, borderRect.Right, yScaleHor);
                                float yValue = (borderRect.Bottom - yScaleHor) * (this.YAxesValid.YAxesMax - this.YAxesValid.YAxesMin) / borderRect.Height + this.YAxesValid.YAxesMin;
                                rectTxt = new RectangleF(borderRect.X - 100, yScaleHor - 20, 100 - 2, 40);
                                if (yScaleHor - 20 > borderRect.Top )
                                {
                                    g.DrawString(yValue.ToString(valFmt.ToString()) + this.YAxes.UnitString, font, brush, rectTxt, sf);
                                }
                                yScaleHor -= ScaleSize;
                            }
                            rectTxt = new RectangleF(borderRect.X - 100, borderRect.Bottom - 22, 100 - 2, 40);
                            g.DrawString(this.YAxesValid.YAxesMin.ToString(valFmt.ToString()) + this.YAxes.UnitString, font, brush, rectTxt, sf);
                            rectTxt = new RectangleF(borderRect.X - 100, borderRect.Top-10, 100 - 2, 40);
                            g.DrawString(this.YAxesValid.YAxesMax.ToString(valFmt.ToString()) + this.YAxes.UnitString, font, brush, rectTxt, sf);
                         
                        }
                       
                    }
                }
            }

        }

        private void DrawDigitYScale(Graphics g, RectangleF borderRect, float yScaleHor, StringFormat sf, Font font, SolidBrush brush, Pen pen)
        {
            float[] digitVal = new float[] { 1, 0, -1 };
            string[] digitName = new string[] { "吸起", "落下", "未知" };

            for (int i = 0; i < digitVal.Length; i++)
            {
                float scaleY = borderRect.Bottom - (digitVal[i] - this.YAxesValid.YAxesMin) * borderRect.Height / (this.YAxesValid.YAxesMax - this.YAxesValid.YAxesMin);
                g.DrawLine(pen, borderRect.X, scaleY, borderRect.Right, scaleY);
                float yValue = (borderRect.Bottom - yScaleHor) * (this.YAxesValid.YAxesMax - this.YAxesValid.YAxesMin) / borderRect.Height + this.YAxesValid.YAxesMin;
                RectangleF rectTxt = new RectangleF(borderRect.X - 100, scaleY - 20, 100 - 2, 40);

                g.DrawString(digitName[i], font, brush, rectTxt, sf);
            }
            
        }


        /// <summary>
        /// 时间基准
        /// </summary>
        public long RelativeTime
        {
            get
            {
                long relativeTime = CurveGroup.XAxes.OriginalTime;
                if (CurveGroup.XAxes.XAxesMode == XAxesMode.Relative)
                {
                    bool isValid = false;
                    long relVal = 0;
                    for (int i = 0; i < lines.Count; i++)
                    {
                        List<LinePoint> points = lines[i].Points;
                        for (int j = 0; j < points.Count; j++)
                        {
                            if (!isValid)
                            {
                                relVal = points[j].Time;
                                isValid = true;
                            }
                            else if (points[j].Time < relVal)
                            {
                                relVal = points[j].Time;
                            }
                        }
                    }
                    if (isValid)
                    {
                        relativeTime = relVal;
                    }
                }
                return relativeTime;
            }
        }

        public RectangleF BorderRect
        {
            get
            {
                RectangleF disRect = this.Rectangle;
                RectangleF borderRect = new RectangleF(disRect.X, disRect.Top + CaptionHeight, disRect.Width, disRect.Height - CaptionHeight - XScaleHeight);

                return borderRect;
            }
        }


        public float XFreqScale
        {
            get;
            set;
        }

        private void DrawXScale(Graphics g, RectangleF rect,bool print)
        {
            if (this.IsFold) return;
            RectangleF disRect = print ? this.PrintRectangle : this.Rectangle;
            RectangleF borderRect = disRect;


            long timeStart = this.CurveGroup.XAxes.OriginalTime;
            long timeScale = this.CurveGroup.XAxes.TimeScale;
            this.CurveGroup.XAxes.DisTimeLen = (int)(rect.Width  * timeScale/ ScaleSize);
            this.CurveGroup.Chart.XAxes.DisTimeLen = this.CurveGroup.XAxes.DisTimeLen;

            long leftTime = timeStart % timeScale;


            StringFormat sf = new StringFormat();
            sf.Alignment = StringAlignment.Center;
            sf.LineAlignment = StringAlignment.Near;
            using (Font font = new Font("雅黑", 8f))
            {
                using (SolidBrush brush = new SolidBrush(this.chart.ForeColor/*Color.FromArgb(252,210,10)*/))
                {
                    if (print)
                    {
                        brush.Color = Color.Black;
                    }
                    using (Pen pen = new Pen(this.chart.GridColor, 1f))
                    {
                        if (print)
                        {
                            pen.Color = Color.LightGray;
                        }
                        pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Custom;
                        pen.DashPattern = new float[] { 3, 3 };


                        RectangleF rectTxt = new RectangleF(borderRect.X - 100, borderRect.Bottom - 20, 100 - 2, 40);

                        float xScale = 0;
                        long scaleTime = timeStart;
                        if (leftTime > 0)
                        {
                            scaleTime += (timeScale - leftTime);
                            xScale += ((timeScale - leftTime) * ScaleSize * 1f) / timeScale;
                        }
                        long relativeTime = this.RelativeTime;
                        while (xScale < borderRect.Width)
                        {

                            g.DrawLine(pen, borderRect.X + xScale, borderRect.Top, borderRect.X + xScale, borderRect.Bottom);

                            DateTime time = ChartGraph.ChartTime2DateTime(scaleTime);
                            rectTxt = new RectangleF(borderRect.X + xScale - 50, borderRect.Bottom -14, 100, 40);
                            if (CurveGroup.XAxes.XAxesMode == XAxesMode.Absolute)
                            {
                                if (timeScale >= TicksPerDay)
                                {
                                    g.DrawString(time.ToString("MM-dd"), font, brush, rectTxt, sf);
                                }
                                else if (timeScale >= TicksPerHour)
                                {
                                    if ((scaleTime % TicksPerDay) == 0)
                                    {
                                        g.DrawString(time.ToString("MM-dd"), font, brush, rectTxt, sf);
                                    }
                                    else
                                    {
                                        g.DrawString(time.ToString("HH"), font, brush, rectTxt, sf);
                                    }
                                }
                                else if (timeScale >= TicksPerMinute)
                                {
                                    if ((scaleTime % (TicksPerHour / 2)) == 0)
                                    {
                                        g.DrawString(time.ToString("HH:mm"), font, brush, rectTxt, sf);
                                    }
                                    else
                                    {
                                        g.DrawString(time.ToString("mm"), font, brush, rectTxt, sf);
                                    }
                                }
                                else if (timeScale >= TicksPerSecond)
                                {
                                    if ((scaleTime % TicksPerMinute) == 0)
                                    {
                                        g.DrawString(time.ToString("HH:mm"), font, brush, rectTxt, sf);
                                    }
                                    else
                                    {
                                        g.DrawString(time.ToString("ss"), font, brush, rectTxt, sf);
                                    }
                                }
                                else
                                {
                                    if ((scaleTime % TicksPerSecond) == 0)
                                    {
                                        g.DrawString(time.ToString("mm:ss.fff"), font, brush, rectTxt, sf);
                                    }
                                    else
                                    {
                                        g.DrawString(time.ToString("fff"), font, brush, rectTxt, sf);
                                    }
                                }
                            }
                            else
                            {
                                
                                if (this.XFreqScale > 0)
                                {
                                    double relaVal = (scaleTime - relativeTime) * 1d;
                                    relaVal = relaVal * XFreqScale;
                                    g.DrawString(relaVal.ToString("0.0"), font, brush, rectTxt, sf);
                                }
                                else
                                {
                                    double relaVal = (scaleTime - relativeTime) * 1d / 1000000L;
                                    if (timeScale < 1000L)
                                    {
                                        g.DrawString((relaVal * 1000000).ToString("0."), font, brush, rectTxt, sf);
                                    }
                                    else if (timeScale < 1000L * 1000L)
                                    {
                                        g.DrawString((relaVal * 1000).ToString("0."), font, brush, rectTxt, sf);

                                    }
                                    else
                                    {
                                        if (Chart.UseFineScale)
                                        {
                                            g.DrawString(relaVal.ToString("0.0"), font, brush, rectTxt, sf);
                                        }
                                        else
                                        {
                                            g.DrawString(relaVal.ToString("0."), font, brush, rectTxt, sf);
                                        }

                                    }
                                }
                               
                            }
                           

                            scaleTime += timeScale;
                            xScale += ScaleSize;

                        }

                    }
                }
            }

        }


        private void DrawAlarmLevel(Graphics g, RectangleF rect,bool print)
        {
           // RectangleF borderRect = print ? this.PrintRectangle : this.BorderRect;
            RectangleF disRect = print ? this.PrintRectangle : this.Rectangle;
          //  RectangleF borderRect = new RectangleF(disRect.X, disRect.Top + CaptionHeight, disRect.Width, disRect.Height - CaptionHeight - XScaleHeight);
            RectangleF borderRect = disRect;
            StringBuilder valFmt = new StringBuilder("0");
            if (this.YAxes.Precision > 0)
            {
                valFmt.Append(".");
                for (int i = 0; i < this.YAxes.Precision; i++)
                {
                    valFmt.Append("0");
                }
            }
            StringFormat sf = new StringFormat();

            using(Font font=new Font("雅黑",8f))
            {
                for (int i = 0; i < lines.Count; i++)
                {
                    LineCurve lc = lines[i];

                    IList<AlarmLevel> alarms = lc.AlarmLevels;

                    for (int j = 0; j < alarms.Count; j++)
                    {
                        if (!alarms[j].Show) continue;
                        
                        float val = alarms[j].Value;
                        if (float.IsNaN(val) || float.IsInfinity(val) || float.IsNegativeInfinity(val) || float.IsPositiveInfinity(val)) continue;
                        if (this.YAxesValid.YAxesMax <= this.YAxesValid.YAxesMin) continue;
                        if (val >= this.YAxesValid.YAxesMax) continue;
                        if (val <= this.YAxesValid.YAxesMin) continue;
                        float posY = (val - this.YAxesValid.YAxesMin) / (this.YAxesValid.YAxesMax - this.YAxesValid.YAxesMin) * borderRect.Height;

                        if (posY > borderRect.Height || posY < 0) continue;

                        g.DrawLine(print?Pens.Gray: Pens.Red, borderRect.X, borderRect.Bottom- posY, borderRect.Right,borderRect.Bottom- posY);
                        StringBuilder sb = new StringBuilder();
                        if (string.IsNullOrEmpty(alarms[j].Name) == false)
                        {
                            if ((alarms[j].Type == LineThresholdType.UpLine0) || (alarms[j].Type == LineThresholdType.UpLine1))
                            {
                                sb.Append(alarms[j].Name + "上限:  ");
                            }
                            else
                            {
                                sb.Append(alarms[j].Name + "下限:  ");
                            }
                            
                        }
                        sb.Append(val.ToString(valFmt.ToString()));
                        sb.Append(YAxes.UnitString);
                        if ((alarms[j].Type == LineThresholdType.UpLine0) || (alarms[j].Type == LineThresholdType.UpLine1))
                        {
                            sf.LineAlignment = StringAlignment.Far;
                            sf.Alignment = StringAlignment.Far;
                            g.DrawString(sb.ToString(), font, Brushes.Red, new RectangleF(borderRect.Right - 100, borderRect.Bottom - posY - 50, 100, 50), sf);
                        }
                        else
                        {
                            sf.LineAlignment = StringAlignment.Near;
                            sf.Alignment = StringAlignment.Far;
                            g.DrawString(sb.ToString(), font, Brushes.Red, new RectangleF(borderRect.Right - 100, borderRect.Bottom - posY, 100, 50), sf);
                        }
                    }

                }
            }
        }
        

        private void DrawBorder(Graphics g, RectangleF rect,bool print)
        {
            RectangleF disRect = print ? this.PrintRectangle : this.Rectangle;
            RectangleF borderRect = this.BorderRect;

            g.FillRectangle(Brushes.Black, borderRect.X - CurveGroup.MarginLeft, borderRect.Y, CurveGroup.MarginLeft, disRect.Height);
            g.FillRectangle(Brushes.Black, borderRect.Right , borderRect.Y, CurveGroup.MarginRight, disRect.Height);
            using (Pen penBorder = new Pen(Color.FromArgb(74, 176, 72), 1.5f))
            {
                if (print)
                {
                    penBorder.Color = Color.Black;
                }
                g.DrawRectangle(penBorder, disRect.X, disRect.Y, disRect.Width, disRect.Height);
                //绘制折叠按钮
                rectFold = new RectangleF(disRect.Right + 5, disRect.Y , FoldBoxSize, FoldBoxSize);
                g.DrawRectangle(penBorder, rectFold.X, rectFold.Y, rectFold.Width, rectFold.Height);
                g.DrawLine(penBorder, disRect.Right + 7, disRect.Y +  FoldBoxSize / 2, disRect.Right + 7 + FoldBoxSize - 4, disRect.Y  + FoldBoxSize / 2);
                if (this.IsFold)
                {
                    g.DrawLine(penBorder, disRect.Right + 5 + FoldBoxSize / 2, disRect.Y + 2, disRect.Right + 5 + FoldBoxSize / 2, disRect.Y  + FoldBoxSize - 2);
                }
               
            }
            using (Pen pen = new Pen(Color.FromArgb(74, 220, 72), 2.0f))
            {
                g.DrawLine(pen, 20, disRect.Bottom, disRect.Right, disRect.Bottom);
            }
        }

        private void DrawCaption(Graphics g, RectangleF rect,bool print)
        {

            if (lines.Count <= 0) return;
            RectangleF disRect = print ? this.PrintRectangle : this.Rectangle;
            RectangleF borderRect = new RectangleF(disRect.X, disRect.Top + CaptionHeight, disRect.Width, disRect.Height - CaptionHeight - XScaleHeight);

            List<float> listTxtWidth = new List<float>();
            float sumWidth = 0;
            using (Font font = new Font("雅黑", 9f))
            {
                for (int i = 0; i < lines.Count; i++)
                {
                    SizeF size = g.MeasureString(lines[i].Name, font);
                    listTxtWidth.Add(size.Width);
                    sumWidth += size.Width;
                }


                if (sumWidth + SelectBoxWidth * lines.Count > borderRect.Width)
                {
                    float widthLeft = borderRect.Width - lines.Count * SelectBoxWidth;

                    float offset = 5;
                    for (int i = 0; i < lines.Count; i++)
                    {
                        lines[i].RectSel = new RectangleF(borderRect.X + offset, borderRect.Y - 20, 16, 16);
                        lines[i].RectName = new RectangleF(borderRect.X + offset + 20, borderRect.Y - 20, listTxtWidth[i] * widthLeft / sumWidth, 15);

                        offset += (SelectBoxWidth + listTxtWidth[i] * widthLeft / sumWidth);
                    }

                }
                else
                {
                    float offset = 5;
                    for (int i = 0; i < lines.Count; i++)
                    {
                        lines[i].RectSel = new RectangleF(borderRect.X + offset, borderRect.Y - 20, 16, 16);
                        lines[i].RectName = new RectangleF(borderRect.X + offset + 20, borderRect.Y - 20, listTxtWidth[i], 20);

                        offset += (SelectBoxWidth + listTxtWidth[i]);
                    }

                }
                StringFormat sf = new StringFormat();
                sf.Trimming = StringTrimming.EllipsisCharacter;
                for (int i = 0; i < lines.Count; i++)
                {
                    using (Pen pen = new Pen(lines[i].LineColor, 1.5f))
                    {
                        if (print)
                        {
                            pen.Color = Color.Black;
                        }
                        RectangleF rectBox=lines[i].RectSel;
                        g.DrawRectangle(pen, rectBox.X, rectBox.Y, rectBox.Width, rectBox.Height);
                    }
                    using(SolidBrush brush=new SolidBrush(lines[i].LineColor))
                    {
                        if (print)
                        {
                            brush.Color = Color.Black;
                        }
                        if (lines[i].Visible)
                        {
                            RectangleF rectSolid = lines[i].RectSel;
                            rectSolid = new RectangleF(rectSolid.X + 3.5f, rectSolid.Y + 3.5f, rectSolid.Width - 3.5f*2, rectSolid.Height - 3.5f*2);
                            g.FillRectangle(brush, rectSolid);
                        }
                        g.DrawString(lines[i].Name, font, brush, lines[i].RectName, sf);
                    }
                    
                }
            }

        }


      
        private void DrawCurves(Graphics g, RectangleF rect, bool print)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                if (print)
                {
                    lines[i].Print(g, rect);
                }
                else
                {
                    lines[i].Draw(g, rect);
                }
                
            }
        }


        public void Paint(Graphics g, RectangleF rect,bool print)
        {
            if (!this.Visible) return;
            //绘制曲线
            DrawCurves(g, rect, print);
            //绘制外框
            DrawBorder(g, rect, print);
            //绘制Y刻度
            DrawYScale(g, rect, print);
            //绘制X刻度
            DrawXScale(g, rect, print);
            //绘制上下限
            DrawAlarmLevel(g, rect, print);
          
            //绘制标题
            DrawCaption(g, rect, print);
           
        }

        public override void Print(Graphics g, RectangleF rect)
        {
            Paint(g, rect, true);
        }
        public override void Draw(Graphics g, RectangleF rect)
        {
            Paint(g, rect, false);
        }

        private string GetTimeText(long time)
        {
            XAxes xAxes = CurveGroup.XAxes;

            if (xAxes.XAxesMode == XAxesMode.Absolute)
            {

                DateTime timeTips = ChartGraph.ChartTime2DateTime(time);
                //if (xAxes.TimeScale >= TicksPerDay)
                //{
                //    return timeTips.ToString("yyyy-MM-dd");
                //}
                //else if (xAxes.TimeScale >= TicksPerHour)
                //{
                //    return timeTips.ToString("MM-dd HH:mm");
                //}
                //else
                {
                    return timeTips.ToString("HH:mm:ss");
                }
            }
            else if (xAxes.XAxesMode == XAxesMode.Relative)
            {
                double time1 = (double)(time - this.RelativeTime) / 1000000L;
                if (this.XFreqScale < 0)
                {
                    if (xAxes.TimeScale >= TicksPerSecond)
                    {
                        return time1.ToString("0.000") + "秒";
                    }
                    else if (xAxes.TimeScale >= TicksPerMiliSec)
                    {
                        return (time1 * 1000).ToString("0.") + "毫秒";
                    }
                    else
                    {
                        return (time1 * 1000000).ToString("0.") + "微秒";
                    }
                }
                else
                {
                    return (time1*1000000*this.XFreqScale).ToString("0.00") + "HZ";
                }

             
            }
            return "";
        }

        
        public override void DrawStage2(Graphics g, RectangleF rect)
        {
            if (!this.Visible) return;
            if (!this.CurveGroup.ShowCursor) return;
            //绘制光标
            RectangleF disRect = this.Rectangle;
           // RectangleF borderRect = new RectangleF(disRect.X, disRect.Top + CaptionHeight, disRect.Width, disRect.Height - CaptionHeight - XScaleHeight);
            RectangleF borderRect = disRect;
            Point pCursor = CurveGroup.CursorPoint;
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

            if (borderRect.Contains(pCursor))
            {
                XAxes xAxes = CurveGroup.XAxes;

                List<string> listTipsValue = new List<string>();
                List<string> listTipsTime = new List<string>();
                List<Color> listColor = new List<Color>();

                long cursorTime = 0;


                if (xAxes.XAxesMode == XAxesMode.Absolute)
                {
                    long timeAbs = xAxes.OriginalTime + (long)((pCursor.X - borderRect.X) * xAxes.TimeScale / ScaleSize);
                    DateTime timeTips = ChartGraph.ChartTime2DateTime(timeAbs);
                    cursorTime = timeAbs;
                    if (xAxes.TimeScale >= TicksPerDay)
                    {
                        listTipsValue.Add("时间:" + timeTips.ToString("yyyy-MM-dd"));
                    }
                    else if (xAxes.TimeScale >= TicksPerHour)
                    {
                        listTipsValue.Add("时间:" + timeTips.ToString("MM-dd HH:mm"));

                    }
                    else
                    {
                        listTipsValue.Add("时间:" + timeTips.ToString("HH:mm:ss"));

                    }
                    listTipsTime.Add("");
                }
                else if (xAxes.XAxesMode == XAxesMode.Relative)
                {
                    double time = ((pCursor.X - borderRect.X) * xAxes.TimeScale / ScaleSize - (this.RelativeTime - xAxes.OriginalTime)) / 1000000L;
                    cursorTime = (long)(this.RelativeTime + (pCursor.X - borderRect.X) * xAxes.TimeScale / ScaleSize);
                    if (XFreqScale > 0)
                    {
                       
                        {
                            listTipsValue.Add("频率:" + (time * 1000000*XFreqScale).ToString("0.00") + "Hz");
                        }
                       
                    }
                    else
                    {
                        if (xAxes.TimeScale >= LineArea.TicksPerSecond)
                        {
                            listTipsValue.Add("时间:" + time.ToString("0.000") + "秒");
                        }
                        else if (xAxes.TimeScale >= LineArea.TicksPerMiliSec)
                        {
                            listTipsValue.Add("时间:" + (time * 1000).ToString("0.") + "毫秒");
                        }
                        else
                        {
                            listTipsValue.Add("时间:" + (time * 1000000).ToString("0.") + "微妙");
                        }
                    }
                 
                 
                    listTipsTime.Add("");
                }
                listColor.Add(Color.White);
                for (int i = 0; i < lines.Count; i++)
                {
                    lines[i].CursorValue = float.NaN;
                    if (lines[i].Visible == false) continue;
                    List<LinePoint> cPoints = lines[i].GetCursorPoint(false);
                    StringBuilder sb = new StringBuilder(lines[i].Name + ":");
                    string timeTxt = "";
                    if (cPoints.Count == 1)
                    {
                        if (float.IsNaN(cPoints[0].Value) == false)
                        {
                            if (Math.Abs(cursorTime - cPoints[0].Time) < LineArea.TicksPerDay)
                            {
                                sb.Append(cPoints[0].Value.ToString("0.000") + (string.IsNullOrEmpty(this.YAxes.UnitString) ? lines[i].YAxes.UnitString : this.YAxes.UnitString));
                                timeTxt = (this.XFreqScale > 0 ? "频率" : "时间:")+ GetTimeText(cPoints[0].Time);
                            }
                        }
                    }
                    else if(cPoints.Count>1)
                    {
                        for (int k = 0; k < cPoints.Count; k++)
                        {
                            sb.Append("("+(k+1)+"):");
                            if (float.IsNaN(cPoints[k].Value) == false)
                            {
                                if (Math.Abs(cursorTime - cPoints[k].Time) < LineArea.TicksPerDay)
                                {
                                    timeTxt =(this.XFreqScale>0?"频率": "时间:") + GetTimeText(cPoints[k].Time);
                                    sb.Append(cPoints[k].Value.ToString("0.000") + (string.IsNullOrEmpty(this.YAxes.UnitString) ? lines[i].YAxes.UnitString : this.YAxes.UnitString) + "," + timeTxt);
                                    timeTxt = "";
                                }
                                
                            }
                        }
                    }

                    listTipsValue.Add(sb.ToString());
                    listTipsTime.Add(timeTxt);
                    listColor.Add(lines[i].LineColor);
                }



                using (Font font = new Font("雅黑", 8f))
                {

                    float txtLineHeight = 20;
                    float txtMaxWidth = 0;
                    float txtDateMaxWidth = 0;
                    float txtValueMaxWidth = 0;
                    for (int i = 0; i < listTipsValue.Count; i++)
                    {
                        SizeF txtSize = g.MeasureString(listTipsValue[i], font);
                        if (txtSize.Width > txtValueMaxWidth)
                        {
                            txtValueMaxWidth = txtSize.Width;
                        }
                        txtSize = g.MeasureString(listTipsTime[i], font);
                        if (txtSize.Width > txtDateMaxWidth)
                        {
                            txtDateMaxWidth = txtSize.Width;
                        }
                    }
                    txtMaxWidth = txtDateMaxWidth + txtValueMaxWidth;
                    txtMaxWidth += 10f;
                    RectangleF rectTxt = new RectangleF();
                    rectTxt.Width = txtMaxWidth;
                    rectTxt.Height = listTipsValue.Count * txtLineHeight;

                    if ((pCursor.X > rect.Width / 2) && (pCursor.Y > rect.Height / 2)) //第一象限
                    {
                        rectTxt.X = pCursor.X - txtMaxWidth;
                        rectTxt.Y = pCursor.Y - txtLineHeight * listTipsValue.Count;

                    }
                    else if ((pCursor.X < rect.Width / 2) && (pCursor.Y > rect.Height / 2))
                    {
                        rectTxt.X = pCursor.X;
                        rectTxt.Y = pCursor.Y - rectTxt.Height;
                    }
                    else if ((pCursor.X < rect.Width / 2) && (pCursor.Y < rect.Height / 2))
                    {
                        rectTxt.X = pCursor.X + 12;
                        rectTxt.Y = pCursor.Y + 15;
                    }
                    else
                    {
                        rectTxt.X = pCursor.X - rectTxt.Width;
                        rectTxt.Y = pCursor.Y;
                    }


                    using (SolidBrush brush = new SolidBrush(Color.FromArgb(150, 88, 119, 211)))
                    {

                        g.FillRectangle(brush, rectTxt.X, rectTxt.Y, rectTxt.Width, rectTxt.Height);

                        for (int i = 0; i < listTipsValue.Count; i++)
                        {
                            brush.Color = listColor[i];
                            g.DrawString(listTipsValue[i], font, brush, new PointF(rectTxt.X + 5, rectTxt.Y + i * txtLineHeight));
                            if (listTipsTime[i] != "")
                            {
                                g.DrawString(listTipsTime[i], font, brush, new PointF(rectTxt.X + 5 + txtValueMaxWidth, rectTxt.Y + i * txtLineHeight));
                            }
                        }
                    }

                }

            }


        }

        public override void ClearData()
        {
            for (int i = 0; i < lines.Count; i++)
            {
                lines[i].ClearPoints();
                
            }
        }
    }
}
