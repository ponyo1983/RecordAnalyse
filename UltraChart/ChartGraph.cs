using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Printing;

namespace UltraChart
{
    public partial class ChartGraph : UserControl
    {

        private List<CurveGroup> grpList = new List<CurveGroup>();

        Point pointHit = new Point(0, 0);

        public const long SecondTicks = 1000000L;

        public ChartGraph()
        {
            InitializeComponent();
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.UserPaint, true);
            this.GridColor = Color.FromArgb(50, 50, 50);
         
            this.BackColor = Color.Black;
            this.ForeColor = Color.FromArgb(252, 210, 10);
            this.UseFineScale = false;
            this.ShowInvalidPoint = true;
           
        }

        public bool ShowInvalidPoint
        {
            get;
            set;
        }


        public bool UseFineScale
        {
            get;
            set;
        }

        public void AutoSetXScale()
        {
            if (this.XAxes.XAxesMode == XAxesMode.Relative)
            {
                for (int i = 0; i < grpList.Count; i++)
                {
                    if (grpList[i].TimeSpan > 0)
                    {
                        long scale = grpList[i].TimeSpan * CurveGroup.ScaleSize / (this.Width - CurveGroup.MarginLeft - CurveGroup.MarginRight);
                        this.XAxes.TimeScale = grpList[i].GetLargerScale(scale);
                        break;
                    }
                }
            }
        }

        public static DateTime ChartTime2DateTime(long t)
        {
            DateTime time = new DateTime(1970, 1, 1, 0, 0, 0);
            return time.AddSeconds((double)(t / 1000000)).AddMilliseconds((double)((t % 1000000) / 1000));
        }


        public static long DateTime2ChartTime(DateTime t)
        {
            DateTime time = new DateTime(1970, 1, 1, 0, 0, 0);
            if (t < time)
            {
                return 0;
            }
            TimeSpan span = (TimeSpan)(t - time);
            return Convert.ToInt64((double)(span.TotalMilliseconds * 1000.0));
        }

        public bool Create() {
            return true;
        }

        public bool SaveAsFile(string fileName)
        {
            foreach (var grp in grpList)
            {
               return grp.SaveAsFile(fileName);
                
            }
            return false;
        }
       
        public CurveGroup AddNewGroup()
        {
            CurveGroup item = new CurveGroup(this);
            this.grpList.Add(item);
            return item;
        }

        public void SetAllGroupDefautSize()
        { }

        public IList<CurveGroup> GroupList
        {
            get { return grpList.AsReadOnly(); }
        }

        public Color GridColor
        {
            get;
            set;
        }
        public XAxes XAxes
        {
            get {
                if (grpList.Count > 0)
                {
                    return grpList[0].XAxes;
                }
                return new XAxes();
            }
        }

        public void Draw()
        {
            foreach (var grp in grpList)
            {
                grp.RefreshAll = true;
            }
            this.Invalidate();
        }

        private PrintAction printAction;
        private PrintDocument printDocument;
        private string info;
        public void Print(string info)
        {
            this.info = info;
            using (PrintDialog pd = new PrintDialog())
            {
                this.printDocument = new PrintDocument();
                this.printDocument.DefaultPageSettings.Landscape = true;
                this.printAction = PrintAction.PrintToPrinter;
               
                pd.Document = printDocument;

                if (pd.ShowDialog() == DialogResult.OK)
                {
                    printDocument.PrintPage += new PrintPageEventHandler(pd_PrintPage);
                    try
                    {
                        printDocument.Print();
                    }
                    catch
                    { 
                    }

                   
                }
                

            }
            

            
        }

        void pd_PrintPage(object sender, PrintPageEventArgs e)
        {


            Graphics g = e.Graphics;
            RectangleF marginBounds = e.MarginBounds;
            RectangleF printableArea = e.PageSettings.PrintableArea;

            if (printAction == PrintAction.PrintToPreview)
                g.TranslateTransform(printableArea.X, printableArea.Y);

         
            int availableWidth = (int)Math.Floor(printDocument.OriginAtMargins ? marginBounds.Width : (e.PageSettings.Landscape ? printableArea.Height : printableArea.Width));
            int availableHeight = (int)Math.Floor(printDocument.OriginAtMargins ? marginBounds.Height : (e.PageSettings.Landscape ? printableArea.Width : printableArea.Height));


            if (!string.IsNullOrEmpty(info))
            {
                using (Font font = new Font("雅黑", 9f))
                {
                    g.DrawString(info, font, Brushes.Black, new PointF(CurveGroup.MarginLeft, 0));
                }
            }
            foreach (CurveGroup grp in grpList)
            {
                grp.Print(e.Graphics, new RectangleF(0,0,availableWidth-1,availableHeight-1));
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            e.Graphics.FillRectangle(Brushes.Black, e.ClipRectangle);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            foreach (CurveGroup grp in grpList)
            {
                grp.Draw(e.Graphics, this.ClientRectangle);
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            this.Focus();
            if (e.Button == MouseButtons.Left)
            {
                pointHit = e.Location;
                this.Capture = true;
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;

            foreach (var grp in grpList)
            {
                grp.RefreshAll = true;
                if (grp.HitTest(e.Location))
                {
                    
                    grp.Action();
                }
            }
            this.Capture = false;
            this.Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {

           

            #region 光标设置
            if (e.Button == MouseButtons.None)
            {
                Point p = e.Location;

                foreach (CurveGroup grp in grpList)
                {
                    if (grp.HitTest(p))
                    {
                        if (this.Cursor != Cursors.Hand)
                        {
                            this.Cursor = Cursors.Hand;
                        }
                    }
                    else if (this.Cursor == Cursors.Hand)
                    {
                        this.Cursor = Cursors.Arrow;
                    }
                }
            }
           
            #endregion

            #region 时间轴移动

            if (e.Button == MouseButtons.Left)
            {
                bool needFresh = false;
                if (e.Location != pointHit)
                {
                    if (pointHit.X != e.X)
                    {
                        foreach (var grp in grpList)
                        {
                            grp.MoveHorizontal(pointHit.X - e.X);
                            needFresh = true;
                            grp.CursorPoint = e.Location;
                        }
                    }
                    pointHit = e.Location;
                }
                if (needFresh)
                {
                    foreach (var grp in grpList)
                    {
                        grp.RefreshAll = true;
                    }
                    this.Invalidate();
                }
            }
            else if (e.Button == MouseButtons.None)
            {
                foreach (var grp in grpList)
                {
                    grp.CursorPoint = e.Location;
                }
                this.Invalidate();
            }
           
            #endregion


        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            foreach (var grp in grpList)
            {
                grp.Zoom(e.X, e.Delta);
                grp.RefreshAll = true;
            }

            this.Invalidate();

        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                foreach (var grp in grpList)
                {
                    grp.ShowCursor = !grp.ShowCursor;
                }
                this.Invalidate();
            }
        }

        public void ClearData()
        {
            foreach (var grp in grpList)
            {
                grp.ClearData();
            }
        }
    }
}
