using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace UltraChart
{
    public class LineCurve:Curve
    {
        public LineArea LineArea { get; internal set; }


        List<AlarmLevel> listLevel = new List<AlarmLevel>();

        long PrevPointTime = 0;
        List<LinePoint> currentSeg = null;
        List<List<LinePoint>> listSegment = new List<List<LinePoint>>();


        public bool TimeDayAlign
        {
            get;
            set;
        }


        float maxVal = float.NaN;
        float minVal = float.NaN;

     

        

        public IList<AlarmLevel> AlarmLevels
        {
            get {
              return  listLevel.AsReadOnly();
            }
        }

        /// <summary>
        /// 选择框
        /// </summary>
        public RectangleF RectSel { get; set; }

        /// <summary>
        /// 名字框
        /// </summary>
        public RectangleF RectName { get; set; }


        public List<LinePoint> CursorPoints
        {
            get;
            set;
        }

        /// <summary>
        /// 光标处数值
        /// </summary>
        public float CursorValue
        {
            get;
            set;
        }

        public List<LinePoint> Points
        {
            get { return listLinePoint; }
        }
        /// <summary>
        /// 数据点的时间
        /// </summary>
        public long CursorTime
        {
            get;
            set;
        }

        public bool LastHitTest
        {
            get;
            set;
        }
        private YAxes yAxes;


        public LineCurve(ChartGraph chart, string name, int maxPointCount)
            : base(chart)
        {
            this.Name = name;
            this.LineWidth = 1;
            this.LineColor = Color.Lime;
            this.yAxes = new YAxes();
            this.TimeDayAlign = false;
        }

        private void MarkMaxMin(LinePoint linePoint)
        {
            float val = linePoint.Value;
            
            if ((!float.IsInfinity(val)) && (!float.IsNaN(val)) && (!float.IsNegativeInfinity(val)) && (!float.IsPositiveInfinity(val)))
            {
                if (float.IsNaN(maxVal))
                {
                    maxVal = val;
                    minVal = val;
                }
                else
                {
                    if (val > maxVal)
                    {
                        maxVal = val;
                    }
                    else if (val < minVal)
                    {
                        minVal = val;
                    }
                }
            }
        }

        private void MakeYAxes()
        {
            if (yAxes.Mode == YAxesMode.Auto)
            {
                if (float.IsNaN(maxVal))
                {
                    yAxes.YAxesMax = 0;
                    yAxes.YAxesMin = 0;
                }
                else
                {
                    if (maxVal - minVal < 1)
                    {
                        yAxes.YAxesMax = maxVal + 5;
                        yAxes.YAxesMin = minVal - 5;
                    }
                    else
                    {
                        yAxes.YAxesMax = (maxVal+minVal)/2 + (maxVal-minVal)*0.7f;
                        yAxes.YAxesMin = (maxVal + minVal) / 2 - (maxVal - minVal) * 0.7f;
                    }
                }

                YAxes shareYAxes = this.LineArea.SharedYAxes;
                if (shareYAxes.YAxesMax <= shareYAxes.YAxesMin) //无效坐标
                {
                    shareYAxes.YAxesMax = yAxes.YAxesMax;
                    shareYAxes.YAxesMin = yAxes.YAxesMin;
                }
                else
                {
                    if (shareYAxes.YAxesMax < yAxes.YAxesMax)
                    {
                        shareYAxes.YAxesMax = yAxes.YAxesMax;
                    }
                    if (shareYAxes.YAxesMin > yAxes.YAxesMin)
                    {
                        shareYAxes.YAxesMin = yAxes.YAxesMin;
                    }
                }
                
            }
        }


        public LinePoint GetCursorPoint(bool print, List<LinePoint> lineSegment)
        {
            LinePoint pt = new LinePoint(0, float.NaN);
            RectangleF disRect = print ? this.LineArea.PrintRectangle : this.LineArea.Rectangle;
            Point pCursor = this.LineArea.CurveGroup.CursorPoint;
            long time = (long)((pCursor.X - disRect.X) / CurveGroup.ScaleSize * this.LineArea.CurveGroup.XAxes.TimeScale) + this.LineArea.CurveGroup.XAxes.OriginalTime;

            if (this.TimeDayAlign)
            {

                List<LinePoint> points = lineSegment;

                int pCnt = points.Count;

                if (pCnt > 0)
                {
                    int startIndex = 0;
                    int endIndex = pCnt - 1;
                    if (points[0].DayAlignTime > time) return pt;
                    if (points[endIndex].DayAlignTime < time)
                    {
                        return points[endIndex];
                    }
                    int midIndex = -1;
                    while (startIndex + 1 < endIndex)
                    {
                        midIndex = (startIndex + endIndex) / 2;
                        if (points[midIndex].DayAlignTime < time)
                        {
                            startIndex = midIndex;
                        }
                        else if (points[midIndex].DayAlignTime > time)
                        {
                            endIndex = midIndex;
                        }
                        else
                        {
                            for (int j = midIndex; j >= 0; j--)
                            {
                                float val = points[j].Value;
                                if (float.IsInfinity(val) || float.IsNaN(val) || float.IsNegativeInfinity(val) || float.IsPositiveInfinity(val)) continue;
                                return points[j];
                            }
                            return points[0];
                        }
                    }
                    if (midIndex > 0)
                    {
                        for (int j = startIndex; j >= 0; j--)
                        {
                            float val = points[j].Value;
                            if (float.IsInfinity(val) || float.IsNaN(val) || float.IsNegativeInfinity(val) || float.IsPositiveInfinity(val)) continue;
                            return points[j];
                        }
                        return points[0];
                    }
                    else
                    {
                        return points[0];
                    }
                }


            }
            else
            {

                List<LinePoint> points = lineSegment;

                int pCnt = points.Count;

                if (pCnt > 0)
                {
                    int startIndex = 0;
                    int endIndex = pCnt - 1;
                    if (points[0].Time > time) return pt;
                    if (points[endIndex].Time < time)
                    {
                        return points[endIndex];
                    }
                    int midIndex = -1;
                    while (startIndex + 1 < endIndex)
                    {
                        midIndex = (startIndex + endIndex) / 2;
                        if (points[midIndex].Time < time)
                        {
                            startIndex = midIndex;
                        }
                        else if (points[midIndex].Time > time)
                        {
                            endIndex = midIndex;
                        }
                        else
                        {
                            for (int j = midIndex; j >= 0; j--)
                            {
                                float val = points[j].Value;
                                if (float.IsInfinity(val) || float.IsNaN(val) || float.IsNegativeInfinity(val) || float.IsPositiveInfinity(val)) continue;
                                return points[j];
                            }
                            return points[0];
                        }
                    }
                    if (midIndex > 0)
                    {
                        for (int j = startIndex; j >= 0; j--)
                        {
                            float val = points[j].Value;
                            if (float.IsInfinity(val) || float.IsNaN(val) || float.IsNegativeInfinity(val) || float.IsPositiveInfinity(val)) continue;
                            return points[j];
                        }
                        return points[0];
                    }
                    else
                    {
                        return points[0];
                    }
                }


            }
            return pt;

        }

        public List<LinePoint> GetCursorPoint(bool print)
        {
            List<LinePoint> listPoint = new List<LinePoint>();
            for (int i = 0; i < listSegment.Count; i++)
            {
                listPoint.Add(GetCursorPoint(print, listSegment[i]));
            }
            return listPoint;
        }
        public override bool AddPoint(LinePoint linePoint)
        {
            MarkMaxMin(linePoint);
            MakeYAxes();

            if (currentSeg == null)
            {
                currentSeg = new List<LinePoint>();
                listSegment.Add(currentSeg);
            }
            else if(linePoint.Time<PrevPointTime-10*LineArea.TicksPerSecond)
            {
                currentSeg = new List<LinePoint>();
                listSegment.Add(currentSeg);
            }

            currentSeg.Add(linePoint);

            PrevPointTime = linePoint.Time;

            return base.AddPoint(linePoint);
        }

        public override void ClearPoints()
        {
            currentSeg = null;
            maxVal = float.NaN;
            minVal = float.NaN;
            listSegment.Clear();
            base.ClearPoints();
            MarkMaxMin();
        }

        private void MarkMaxMin()
        {
            for (int i = 0; i < listLevel.Count; i++)
            {
                float val = listLevel[i].Value;

                if (float.IsInfinity(val) || float.IsNaN(val) || float.IsNegativeInfinity(val) || float.IsPositiveInfinity(val)) continue;

                if (float.IsNaN(maxVal))
                {
                    maxVal = val;
                    minVal = val;
                }
                else
                {
                    if (val > maxVal)
                    {
                        maxVal = val;
                    }
                    else if (val < minVal)
                    {
                        minVal = val;
                    }
                }

            }
        }

      
        public void AddThresholdValue(LineThresholdType type, float val, long time)
        {
            AlarmLevel level = new AlarmLevel(type, time, val);
            ClearThresholdValue(type);
            listLevel.Add(level);
            MarkMaxMin();
        }

        public void AddThresholdValue(LineThresholdType type, float val, string name)
        {
            AlarmLevel level = new AlarmLevel(type, name, val);
            ClearThresholdValue(type);
            listLevel.Add(level);
            MarkMaxMin();
        }

        public bool ClearThresholdValue(LineThresholdType type)
        {
            List<AlarmLevel> rmList=new List<AlarmLevel>();
            for (int i = 0; i < listLevel.Count; i++)
            {
                if (listLevel[i].Type == type)
                {
                    rmList.Add(listLevel[i]);
                }
            }
            for (int i = 0; i < rmList.Count; i++)
            {
                listLevel.Remove(rmList[i]);
            }
            return true;
        }

        public bool IsExistThresholdLine(LineThresholdType type)
        {
            return true;
        }

        public bool IsShowThresholdLine(LineThresholdType type)
        {
            return true;
        }

        public bool RemoveThresholdLine(LineThresholdType type)
        {
            return true;
        }

        public void ShowThresholdLine(LineThresholdType type, bool isShow)
        {
            for (int i = 0; i < listLevel.Count; i++)
            {
                if (listLevel[i].Type == type)
                {
                    listLevel[i].Show = isShow;
                }
            }
        }

        public Color LineColor
        {
            get;
            set;
        }

        public int LineWidth
        {
            get;
            set;
        }
        public YAxes YAxes
        {
            get { return this.LineArea.YAxes; }
        }

        private void DrawLine(Graphics g, Pen pen, RectangleF rect, PointF p1, PointF p2)
        {
            if ((p1.X < rect.X) && (p2.X < rect.X)) return;
            if ((p1.X > rect.Right) && (p2.X > rect.Right)) return;
            if ((p1.Y > rect.Bottom) && (p2.Y > rect.Bottom)) return;
            if ((p1.Y < rect.Top) && (p2.Y < rect.Top)) return;

            if (rect.Contains(p1) && rect.Contains(p2)) //两点都在矩形框内
            {
                g.DrawLine(pen, p1, p2);
            }
            else if ((p1.X == p2.X) && (p1.X >= rect.Left) && (p1.X <= rect.Right)) //垂直直线
            {
                float y1 = p1.Y;
                if (y1 < rect.Top)
                {
                    y1 = rect.Top;
                }
                else if (y1 > rect.Bottom)
                {
                    y1 = rect.Bottom;
                }
                float y2 = p2.Y;
                if (y2 < rect.Top)
                {
                    y2 = rect.Top;
                }
                else if (y2 > rect.Bottom)
                {
                    y2 = rect.Bottom;
                }
                if (y1 != y2)
                {
                    g.DrawLine(pen, p1.X, y1, p1.X, y2);
                }

            }
            else if ((p1.Y == p2.Y) && (p1.Y >= rect.Top) && (p1.Y <= rect.Bottom)) //水平直线
            {

                float x1 = p1.X;
                if (x1 < rect.Left)
                {
                    x1 = rect.Left;
                }
                else if (x1 > rect.Right)
                {
                    x1 = rect.Right;
                }
                float x2 = p2.X;
                if (x2 < rect.Left)
                {
                    x2 = rect.Left;
                }
                else if (x2 > rect.Right)
                {
                    x2 = rect.Right;
                }
                if (x1 != x2)
                {
                    g.DrawLine(pen, x1, p1.Y, x2, p1.Y);
                }

            }
            else
            {
                float k = (p1.Y - p2.Y) / (p1.X - p2.X);
                float b = p1.Y - k * p1.X;
                float YH = p1.Y > p2.Y ? p1.Y : p2.Y;
                float YL = p1.Y > p2.Y ? p2.Y : p1.Y;
                float XH = p1.X > p2.X ? p1.X : p2.X;
                float XL = p1.X > p2.X ? p2.X : p1.X;

                float x1 = float.NaN;
                float y1 = float.NaN;
                if (rect.Contains(p1))
                {
                    x1 = p1.X;
                    y1 = p1.Y;
                }
                else if (rect.Contains(p2))
                {
                    x1 = p2.X;
                    y1 = p2.Y;
                }
                //
                float x = rect.X;
                float y = k * x + b;
                if (y >= YL && y <= YH && y >= rect.Top && y <= rect.Bottom)
                {
                    if (float.IsNaN(x1))
                    {
                        x1 = x;
                        y1 = y;
                    }
                    else
                    {
                        g.DrawLine(pen, x1, y1, x, y);
                        return;
                    }
                }
                //
                y = rect.Top;
                x = (y - b) / k;
                if (x >= XL && x <= XH && x >= rect.Left && x <= rect.Right)
                {
                    if (float.IsNaN(x1))
                    {
                        x1 = x;
                        y1 = y;
                    }
                    else
                    {
                        g.DrawLine(pen, x1, y1, x, y);
                        return;
                    }
                }
                x = rect.Right;
                y = k * x + b;
                if (y >= YL && y <= YH && y >= rect.Top && y <= rect.Bottom)
                {
                    if (float.IsNaN(x1))
                    {
                        x1 = x;
                        y1 = y;
                    }
                    else
                    {
                        g.DrawLine(pen, x1, y1, x, y);
                        return;
                    }
                }
                y = rect.Bottom;
                x = (y - b) / k;
                if (x >= XL && x <= XH && x >= rect.Left && x <= rect.Right)
                {
                    if (float.IsNaN(x1))
                    {
                        x1 = x;
                        y1 = y;
                    }
                    else
                    {
                        g.DrawLine(pen, x1, y1, x, y);
                        return;
                    }
                }

            }

        }


        public int SearchPoint(List<LinePoint> points,long time)
        {
            if (points == null) return 0;
            int pCnt = points.Count;
            if (pCnt >=2)
            {
                int startIndex = 0;
                int endIndex = pCnt - 1;
                if (points[startIndex].Time >= time) return startIndex;
                if (points[endIndex].Time <= time) return endIndex;
                int midIndex=0;
                while (startIndex+1 < endIndex )
                {
                    midIndex = (startIndex + endIndex) / 2;
                    if (points[midIndex].Time < time)
                    {
                        startIndex = midIndex;
                    }
                    else if (points[midIndex].Time > time)
                    {
                        endIndex = midIndex;
                    }
                    else
                    {
                        return midIndex;
                    }
                }
                return (startIndex + endIndex) / 2;
            }
            return 0;
        }

        private void DrawLine(Graphics g, RectangleF rect, List<LinePoint> points, bool print)
        {

            if (this.Visible == false) return;
            RectangleF disRect = print ? this.LineArea.PrintRectangle : this.LineArea.Rectangle;
           // RectangleF borderRect = new RectangleF(disRect.X, disRect.Top + LineArea.CaptionHeight, disRect.Width, disRect.Height - LineArea.CaptionHeight - LineArea.XScaleHeight);

            RectangleF borderRect = disRect;


            
            float lastValue = float.NaN;
            bool prevValid = false;
            long prevTime = -1;
            long prevX = -1;
            float prevStart = 0;
            float prevEnd = 0;
            float prevMax = float.MinValue;
            float prevMin = float.MaxValue;
            XAxes xAxes = this.Chart.XAxes;

            long timeEnd = xAxes.OriginalTime + (long)((borderRect.Width + CurveGroup.MarginRight) * 1d / CurveGroup.ScaleSize * xAxes.TimeScale);

            bool lookbackPoint = false;
            int lookbackNum = 0;
            using (Pen pen = new Pen(this.LineColor, this.LineWidth))
            using (SolidBrush brush = new SolidBrush(this.LineColor))
            {
                if (print)
                {
                    pen.Color = Color.Lime;
                    brush.Color = Color.Lime;
                }
                int pCnt = points.Count;
                int startIndex = SearchPoint(points, xAxes.OriginalTime);
                startIndex -= 10;
                if (startIndex < 0) { startIndex = 0; }
                for (int i = startIndex; i >= 0; i--)
                {
                    float val = points[i].Value;
                    if (float.IsInfinity(val) || float.IsNaN(val) || float.IsNegativeInfinity(val) || float.IsPositiveInfinity(val)) continue;
                    startIndex = i;
                    break;
                }
                List<PointF> listCurveFit = new List<PointF>();

                double pixelTime = this.LineArea.CurveGroup.XAxes.TimeScale*1d / CurveGroup.ScaleSize;

                bool curveFit = false;
                if (pixelTime < 1000000d / 8000)
                {
                    curveFit = true;
                }

                for (int i = startIndex; i < pCnt; i++)
                {
                    lookbackPoint = false;
                    LinePoint pOrig = points[i];
                    LinePoint p = pOrig;
                    if (this.TimeDayAlign)
                    {
                        p.Time = p.DayAlignTime;
                    }

                    if (i > 1 && i < pCnt)
                    {
                        if ((points[i-2].Time > timeEnd) && (points[i-1].Time > timeEnd) && (points[i].Time > timeEnd))
                        {
                            break;
                        }
                    }

                    if (float.IsNaN(pOrig.Value))
                    {
                        p = new LinePoint(pOrig.Time, lastValue);
                    }
                    if (float.IsNaN(p.Value) || float.IsInfinity(p.Value) || float.IsNegativeInfinity(p.Value) || float.IsPositiveInfinity(p.Value))
                    {
                        if (prevValid && (prevX >= 0) && (prevX < borderRect.Width) && (prevMax > prevMin))
                        {
                            if (prevMax > 0 && prevMin < borderRect.Height)
                            {
                                float max = Math.Min(prevMax, borderRect.Height);
                                float min = Math.Max(prevMin, 0);
                                if (!curveFit)
                                {
                                    g.DrawLine(pen, prevX + borderRect.X, borderRect.Bottom - min, prevX + borderRect.X, borderRect.Bottom - max);
                                }
                                
                            }
                        }
                        lastValue = float.NaN;
                        prevValid = false;
                    }
                    else
                    {
                        lastValue = p.Value;
                        long posX = (p.Time - xAxes.OriginalTime) * CurveGroup.ScaleSize / xAxes.TimeScale;

                        float posY = 0;

                        if (this.YAxes.Mode == YAxesMode.Auto)
                        {
                            posY = (p.Value - this.LineArea.SharedYAxes.YAxesMin) / (this.LineArea.SharedYAxes.YAxesMax - this.LineArea.SharedYAxes.YAxesMin) * borderRect.Height;
                        }
                        else
                        {
                            posY = (p.Value - this.YAxes.YAxesMin) / (this.YAxes.YAxesMax - this.YAxes.YAxesMin) * borderRect.Height;
                        }


                        if (prevX + borderRect.X <= this.LineArea.CurveGroup.CursorPoint.X)
                        {
                            this.CursorValue = p.Value;
                        }
                        if (prevValid && (posX == prevX))
                        {
                            prevEnd = posY;
                            if (posY > prevMax)
                            {
                                prevMax = posY;
                            }
                            if (posY < prevMin)
                            {
                                prevMin = posY;
                            }
                        }
                        else
                        {
                            if (posX != prevX)
                            {
                                if (prevValid && (prevX >= 0) && (prevX < borderRect.Width) && (prevMax > prevMin))
                                {
                                    if (prevMax > 0 && prevMin < borderRect.Height)
                                    {
                                        float max = Math.Min(prevMax, borderRect.Height);
                                        float min = Math.Max(prevMin, 0);
                                       
                                        if(!curveFit)
                                        {
                                            g.DrawLine(pen, prevX + borderRect.X, borderRect.Bottom - min, prevX + borderRect.X, borderRect.Bottom - max);
                                        }
                                        
                                    }
                                }
                            }

                            if (prevValid && (posX > prevX))
                            {
                                if (i > 0)
                                {
                                    if (!curveFit)
                                    {
                                        PointF p1 = new PointF(prevX + borderRect.X, borderRect.Bottom - prevEnd);
                                        PointF p2 = new PointF(posX + borderRect.X, borderRect.Bottom - posY);
                                        RectangleF rectBig = new RectangleF(borderRect.X - 1, borderRect.Y - 1, borderRect.Width + 2, borderRect.Height + 2);
                                        DrawLine(g, pen, rectBig, p1, p2);
                                    }
                                
                                }
                            }
                            //if (prevValid && (posX < prevX))
                            if (prevValid && (prevTime - p.Time) > 10L * 1000000L)
                            {
                                lookbackPoint = true;
                                lookbackNum++;
                            }
                            prevX = posX;
                            prevStart = posY;
                            prevMax = posY;
                            prevMin = posY;
                            prevEnd = posY;
                            prevValid = true;
                        }
                        //绘制点
                        PointF pDot = new PointF(borderRect.X + posX, borderRect.Bottom - posY);
                        if (curveFit)
                        {
                            listCurveFit.Add(pDot);
                        }
                        if (this.LineArea.CurveGroup.DrawPointFlagXAxesScale >= this.LineArea.CurveGroup.XAxes.TimeScale)
                        {
                           
                            //把边框扩大1个像素
                            RectangleF rectBig = new RectangleF(borderRect.X - 1, borderRect.Y - 1, borderRect.Width + 2, borderRect.Height + 2);
                            if (rectBig.Contains(pDot))
                            {
                                if (lookbackPoint)
                                {
                                    g.FillRectangle(brush, pDot.X - 7f, pDot.Y - 7f, 14, 14);
                                    using (Font font = new Font("雅黑", 8))
                                    {
                                        StringFormat sf = new StringFormat();
                                        sf.Alignment = StringAlignment.Near;
                                        sf.LineAlignment = StringAlignment.Center;
                                        g.DrawString(lookbackNum.ToString(), font, Brushes.Black, new RectangleF(pDot.X - 7f, pDot.Y - 7f, 14, 14), sf);
                                    }
                                }
                                else
                                {
                                    DrawLinePointDot(g, brush, ref pOrig, ref pDot);

                                }

                            }
                        }
                    }
                    prevTime = p.Time;
                }
                if (curveFit)
                {
                    g.DrawCurve(pen, listCurveFit.ToArray(), 0.85f);
                }
            }
        }

        private void DrawLinePointDot(Graphics g, SolidBrush brush, ref LinePoint pOrig, ref PointF pDot)
        {
            if (float.IsNaN(pOrig.Value))
            {
                if (Chart.ShowInvalidPoint
                    && this.YAxes.Mode != YAxesMode.Digital)
                {
                    g.FillRectangle(Brushes.Black, pDot.X - 4f, pDot.Y - 4f, 8, 8);
                }
            }
            else
            {
                g.FillEllipse(brush, pDot.X - 2.5f, pDot.Y - 2.5f, 5, 5);
            }
        }


        public override void Draw(Graphics g, RectangleF rect)
        {
            for (int i = 0; i < listSegment.Count; i++)
            {
                DrawLine(g, rect,listSegment[i], false);
            }
                
        }

        public override void Print(Graphics g, RectangleF rect)
        {
            for (int i = 0; i < listSegment.Count; i++)
            {
                DrawLine(g, rect, listSegment[i], true);
            }
        }

    }


    public class AlarmLevel
    {
        LineThresholdType type;
        long time;
        float value;
        string name;
        public AlarmLevel(LineThresholdType type, long time, float value)
        {
            this.type = type;
            this.time = time;
            this.value = value;
            this.Show = true;
        }

        public AlarmLevel(LineThresholdType type, string name, float value)
        {
            this.type = type;
            this.name = name;
            this.value = value;
            this.Show = true;
        }

        public bool Show
        {
            get;
            set;
        }
        public LineThresholdType Type
        {
            get { return type; }
        }

        public string Name
        {
            get { return name; }
        }
        public long Time
        {
            get { return time; }
        }

        public float Value
        {
            get { return value; }
        }
    }
}
