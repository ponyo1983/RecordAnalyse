using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;

namespace UltraChart
{
    public class CurveGroup : ChartObject
    {

        public const int MarginLeft = 60;
        public const int MarginRight = 25;
        public const int MarginTop = 20;
        public const int MarginBottom = 30;

        public const int ScaleSize = 40;
        const float BarCurveHight = 28f;
        const float LineCurveHight = 100f;

        /// <summary>
        ///  提示时间的显示格式
        /// </summary>
        public String ValueTimeTimeFormat = "";
    

        private List<ChartObject> curveList = new List<ChartObject>();
        private Dictionary<String, ChartObject> curveDict = new Dictionary<String, ChartObject>();

        public Dictionary<String, ChartObject> ChartObjectDict
        {
            get { return curveDict; }
        }
        private XAxes xAxes;

        Bitmap bitmap = null;
        RectangleF rectMap = new RectangleF();

        public RectangleF RectClient
        {
            get;
            private set;
        }

        private static readonly long[] ScaleList = new long[] { 
        10L,20L,50L,100L,200L,500L,
        1L*1000L,2L*1000L,5L*1000L,10L*1000L,20L*1000L,50L*1000L,
        100L*1000L,200L*1000L,500L*1000L,
        1L*1000000L,2L*1000000L,5L*1000000L,10L*1000000L,20L*1000000L,30L*1000000L,
        1L*60L*1000000L,2L*60L*1000000L,5L*60L*1000000L,10L*60L*1000000L,20L*60L*1000000L,30L*60L*1000000L,
        1L*60L*60L*1000000L,2L*60L*60L*1000000L,5L*60L*60L*1000000L,10L*60L*60L*1000000L,20L*60L*60L*1000000L,
        1L*24L*60L*60L*1000000L,2L*24L*60L*60L*1000000L,5L*24L*60L*60L*1000000L,10L*24L*60L*60L*1000000L,20L*24L*60L*60L*1000000L,
        };

        private static readonly long[] FineScaleList = new long[] { 
        10L,20L,50L,100L,200L,500L,
        1L*1000L,2L*1000L,5L*1000L,10L*1000L,20L*1000L,50L*1000L,
        100L*1000L,200L*1000L,500L*1000L,
        1L*1000000L,1100000L,1200000L,1500000L,
        2L*1000000L,2100000L,2200000L,2500000L,
        3L*1000000L,3100000L,3200000L,3500000L,
        4L*1000000L,4100000L,4200000L,4500000L,
        5L*1000000L,10L*1000000L,20L*1000000L,30L*1000000L,
        1L*60L*1000000L,2L*60L*1000000L,5L*60L*1000000L,10L*60L*1000000L,20L*60L*1000000L,30L*60L*1000000L,
        1L*60L*60L*1000000L,2L*60L*60L*1000000L,5L*60L*60L*1000000L,10L*60L*60L*1000000L,20L*60L*60L*1000000L,
        1L*24L*60L*60L*1000000L,2L*24L*60L*60L*1000000L,5L*24L*60L*60L*1000000L,10L*24L*60L*60L*1000000L,20L*24L*60L*60L*1000000L,
        };


        public CurveGroup(ChartGraph chart)
        {
            base.Chart = chart;
            this.xAxes = new XAxes();
            this.CursorPoint = new Point(0, 0);
            this.ShowCursor = true;
        }

        /// <summary>
        /// 时间跨度
        /// </summary>
        public long TimeSpan
        {
            get
            {
                long span = 0;
                for (int i = 0; i < curveList.Count; i++)
                {
                    LineArea la = curveList[i] as LineArea;
                    if (la != null)
                    {
                        for(int j=0;j<la.Lines.Count;j++)
                        {
                            IList<LinePoint> pts = la.Lines[j].Points;

                            if (pts.Count >= 2)
                            {
                                long lspn = pts[pts.Count - 1].Time - pts[0].Time;
                                if (lspn > span)
                                {
                                    span = lspn;
                                }
                            }
                        }
                       
                    }
                }
                return span;
            }
        }

        public long GetLargerScale(long scale)
        {
            long[] SelScaleList = Chart.UseFineScale ? FineScaleList : ScaleList;
            for (int i = 0; i < SelScaleList.Length; i++)
            {
                if (SelScaleList[i] >= scale) return SelScaleList[i];
            }
            return scale;
        }

        public bool AddChartObject(ChartObject obj)
        {
            if (obj is LineArea)
            {
                LineArea la = obj as LineArea;
                la.CurveGroup = this;
                curveList.Add(obj);
                if (!String.IsNullOrEmpty(la.Name))
                {
                    curveDict[la.Name] = la;
                }
                obj.Chart = this.Chart;
            }
            else if (obj is BarCurve)
            {
                BarCurve bc = obj as BarCurve;
                bc.CurveGroup = this;
                int index = 0;
                
                for (int i = 0; i < curveList.Count; i++)
                {
                    if (curveList[i] is BarCurve)
                    {
                        index ++;
                    }
                }
                this.curveList.Insert(index, obj);
                if (!String.IsNullOrEmpty(bc.Name))
                {
                    curveDict[bc.Name] = bc;
                }
            }
            return true;
        }

        public bool HaveChartObject(String chartObjName)
        {
           return  curveDict.ContainsKey(chartObjName);  
        }

        public ChartObject this[String chartObjName]
        {
            get 
            {
                if (curveDict.ContainsKey(chartObjName))
                {
                    return curveDict[chartObjName];
                }
                else
                {
                    return null;
                }
            }
        }

        public bool AutoFitSize()
        {
            return true;
        }

        public void ClearChartObject()
        {
            
            this.curveList.Clear();
            this.curveDict.Clear();
        }

        public bool GetCursorTimePos(ref long timePos, ref int timeSegment)
        {
            return true;
        }

        public bool RemoveChartObject(ChartObject obj)
        {
            this.curveList.Remove(obj);
            if (this.curveDict.ContainsKey(obj.Name))
            {
                this.curveDict.Remove(obj.Name);
            }
            return true;
        }

        public bool SetCursorTimePos(long timePos, int timeSegment)
        {
            return true;
        }

        public bool SetTipTimeFormat(string timeFormat, TipTimeType type)
        {
            return true;
        }

        public bool SetValueTipTimeFormat(string timeFormat, TipTimeType type)
        {
            this.ValueTimeTimeFormat = timeFormat;
            return true;
        }

        public void MoveHorizontal(int distance)
        {
            this.xAxes.OriginalTime += distance*this.xAxes.TimeScale / ScaleSize;
        }

        public bool RefreshAll
        {
            get;
            set;
        }


        public Point CursorPoint
        {
            get;
            set;
        }


        public bool ShowCursor
        {
            get;
            set;
        }


        public bool SaveAsFile(string fileName)
        {
            string dir = Path.GetDirectoryName(fileName);
            string ext = Path.GetExtension(fileName);
            if (!Directory.Exists(dir)) return false;


            try
            {
                if (ext == ".txt")
                {

                    using (StreamWriter sw = new StreamWriter(new FileStream(fileName, FileMode.Create)))
                    {
                        foreach (ChartObject charObj in curveList)
                        {
                            if (charObj is LineArea)
                            {
                                IList<LineCurve> lines = (charObj as LineArea).Lines;
                                foreach (LineCurve lc in lines)
                                {
                                    sw.Write(lc.Name + ":");
                                    List<LinePoint> lps = lc.Points;
                                    for (int i = 0; i < lps.Count; i++)
                                    {
                                        sw.Write(lps[i].Value.ToString("0.00") + ",");
                                    }
                                    sw.WriteLine();
                                }
                            }
                        }
                        sw.Close();
                    }


                }
                else
                {
                    Bitmap bitmap = new Bitmap((int)this.RectClient.Width, (int)this.RectClient.Height);

                    Graphics g = Graphics.FromImage(bitmap);

                    this.Draw(g, this.RectClient);
                    if (ext == ".bmp")
                    {
                        bitmap.Save(fileName, System.Drawing.Imaging.ImageFormat.Bmp);
                    }
                    else if (ext == ".png")
                    {
                        bitmap.Save(fileName, System.Drawing.Imaging.ImageFormat.Png);
                    }
                }
            }
            catch
            {
                return false;
            }
            

          

            return true;

        }
        public void Zoom(int center, int dir)
        {
            if (center < RectClient.Left + MarginLeft) return;
            if (center > RectClient.Right - MarginRight) return;
            long[] SelScaleList = Chart.UseFineScale ? FineScaleList : ScaleList;
            if (dir < 0) //ZoomOut
            {
                long scaleSel = xAxes.TimeScale;
                for (int i = 0; i < SelScaleList.Length; i++)
                {
                    if (SelScaleList[i] > scaleSel)
                    {
                        scaleSel = SelScaleList[i];
                        break;
                    }
                }

                if ((scaleSel > xAxes.TimeScale) && (scaleSel <=xAxes.MaxScale))
                {
                    long timeCursor = (long)(xAxes.OriginalTime + (double)(center - MarginLeft - RectClient.Left) * xAxes.TimeScale / ScaleSize);
                    this.xAxes.OriginalTime = (long)(timeCursor - (double)(center - MarginLeft - RectClient.Left) * scaleSel / ScaleSize);
                    this.xAxes.TimeScale = scaleSel;
                }
            }
            else if (dir > 0) //ZoomIn
            {
                long scaleSel = xAxes.TimeScale;
                for (int i = SelScaleList.Length - 1; i >= 0; i--)
                {
                    if (SelScaleList[i] < scaleSel)
                    {
                        scaleSel = SelScaleList[i];
                        break;
                    }
                }

                if ((scaleSel < xAxes.TimeScale) && (scaleSel >=xAxes.MinScale))
                {
                    long timeCursor = (long)(xAxes.OriginalTime + (double)(center - MarginLeft - RectClient.Left) * xAxes.TimeScale / ScaleSize);
                    this.xAxes.OriginalTime = (long)(timeCursor - (double)(center - MarginLeft - RectClient.Left) * scaleSel / ScaleSize);
                    this.xAxes.TimeScale = scaleSel;
                }
            }
        }

        public CurveCursorType CursorType
        {
            get;
            set;
        }

        public List<ChartObject> CurveList
        {
            get
            {
                return this.curveList;
            }
        }

        public long DrawPointFlagXAxesScale
        {
             get;
             set;
        }

        public XAxes XAxes
        {
            get
            {
                return this.xAxes;
            }
        }


        /// <summary>
        /// 计算各个部件的大小
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="print">是否为打印</param>
        private void CalculateShape(RectangleF rect,bool print)
        {
            
            float hight = rect.Height - MarginTop - MarginBottom;
            int LineExpandNum = 0;
            foreach (ChartObject obj in curveList)
            {
                if (obj.Visible == false) continue;
                
                if (obj is BarCurve)
                {
                    hight -= BarCurveHight;
                }
                else if (obj is LineArea)
                {
                    if (obj.IsFold)
                    {
                        hight -= LineCurveHight;
                    }
                    else
                    {
                        LineExpandNum++;
                    }
                }
            }

            float startY = MarginTop;
            foreach (ChartObject obj in curveList)
            {
                if (obj.Visible == false) continue;
                if (obj is BarCurve)
                {
                    if (print)
                    {
                        obj.PrintRectangle = new RectangleF(MarginLeft, rect.Top + startY, rect.Width - MarginLeft - MarginRight, BarCurveHight);
                    }
                    else
                    {
                        obj.Rectangle = new RectangleF(MarginLeft, rect.Top + startY, rect.Width - MarginLeft - MarginRight, BarCurveHight);
                    }
                    startY += BarCurveHight;
                }
                else if(obj is LineArea)
                {
                    if (obj.IsFold)
                    {
                        if (print)
                        {
                            obj.PrintRectangle = new RectangleF(MarginLeft, rect.Top + startY, rect.Width - MarginLeft - MarginRight, LineCurveHight);
                        }
                        else
                        {
                            obj.Rectangle = new RectangleF(MarginLeft, rect.Top + startY, rect.Width - MarginLeft - MarginRight, LineCurveHight);
                        }
                        
                        startY += LineCurveHight;
                    }
                    else
                    {
                        if (print)
                        {
                            obj.PrintRectangle = new RectangleF(MarginLeft, rect.Top + startY, rect.Width - MarginLeft - MarginRight, (hight / LineExpandNum));
                        }
                        else
                        {
                            obj.Rectangle = new RectangleF(MarginLeft, rect.Top + startY, rect.Width - MarginLeft - MarginRight, (hight / LineExpandNum));
                        }
                        
                        startY += (hight / LineExpandNum);
                    }
                }
            }


            

        }

        public override bool HitTest(Point p)
        {
            for (int i = 0; i < curveList.Count; i++)
            {
                if (curveList[i].HitTest(p)) return true;
            }

            return false;
        }

        public override void Action()
        {
            foreach (var curve in curveList)
            {
                curve.Action();
            }
        }

        private void DrawTimeTip(Graphics g, RectangleF rect)
        {
            DateTime time = ChartGraph.ChartTime2DateTime(this.xAxes.OriginalTime);
            StringFormat sf = new StringFormat();
            sf.Alignment = StringAlignment.Near;
            sf.LineAlignment = StringAlignment.Near;
            using (Font font = new Font("雅黑", 10f))
            {
                using (Brush brush = new SolidBrush(Color.FromArgb(223,34,39)))
                {
                    RectangleF rectTxt = new RectangleF(rect.X + MarginLeft, rect.Bottom - MarginBottom + 5, 300, 60);
                    StringBuilder sb = new StringBuilder(time.ToString("yyyy-MM-dd HH:mm:ss") + "    每格:");
                    if (this.xAxes.TimeScale >= LineArea.TicksPerDay)
                    {
                        sb.Append(this.xAxes.TimeScale / LineArea.TicksPerDay + "天");
                    }
                    else if (this.xAxes.TimeScale >= LineArea.TicksPerHour)
                    {
                        sb.Append(this.xAxes.TimeScale / LineArea.TicksPerHour + "小时");
                    }
                    else if (this.xAxes.TimeScale >= LineArea.TicksPerMinute)
                    {
                        sb.Append(this.xAxes.TimeScale / LineArea.TicksPerMinute + "分钟");
                    }
                    else if (this.xAxes.TimeScale >= LineArea.TicksPerSecond)
                    {
                        if (Chart.UseFineScale)
                        {
                            sb.Append((this.xAxes.TimeScale*1d / LineArea.TicksPerSecond).ToString("0.0") + "秒");
                        }
                        else
                        {
                            sb.Append(this.xAxes.TimeScale / LineArea.TicksPerSecond + "秒");
                        }
                      
                    }
                    else if (this.xAxes.TimeScale >= LineArea.TicksPerMiliSec)
                    {
                        sb.Append(this.xAxes.TimeScale / LineArea.TicksPerMiliSec + "毫秒");
                    }
                    else
                    {
                        sb.Append(this.xAxes.TimeScale + "微妙");
                    }


                    g.DrawString(sb.ToString(), font, brush, rectTxt, sf);
                }
            }
        }

        public void Print(Graphics g, RectangleF rect)
        {
            this.PrintRectangle = rect;
            CalculateShape(rect, true);

            foreach (ChartObject curve in curveList)
            {
                curve.Print(g, rect);
            }
            DrawTimeTip(g, rect);
        }

        public override void Draw(Graphics g, RectangleF rect)
        {
            this.RectClient = rect;
            CalculateShape(rect,false);

            if (this.rectMap != RectClient)
            {
                if(bitmap!=null)
                {
                    bitmap.Dispose();
                }
                bitmap = new Bitmap((int)RectClient.Width, (int)RectClient.Height);
                this.RefreshAll = true;
                rectMap = RectClient;
            }

            Graphics gMap = Graphics.FromImage(bitmap);
            if (RefreshAll)
            {
                using (Brush brush = new SolidBrush(this.Chart.BackColor))
                {
                    gMap.FillRectangle(brush, RectClient);
                }
                foreach (ChartObject curve in curveList)
                {
                    curve.Draw(gMap, rect);
                }
                DrawTimeTip(gMap, rect);
            }
            g.DrawImage(bitmap, 0, 0);

            foreach (ChartObject curve in curveList)
            {
                curve.DrawStage2(g, rect);
            }

            
            RefreshAll = false;
        }

        internal void ClearData()
        {
            foreach (var curve in curveList)
            {
                curve.ClearData();
            }
        }
    }
}
