using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using RecordAnalyse.Utils;
using RecordAnalyse.Record;
using DataStorage;
using UltraChart;
using Common;
using ConfigManager.Device;

namespace RecordAnalyse
{
    public partial class Form1 : Form
    {
        UltraChart.CurveGroup dayCurveGroup;
        public Form1()
        {
            InitializeComponent();


            this.dayCurveGroup = this.chart.AddNewGroup();
            this.dayCurveGroup.XAxes.MaxScale = 60000000;
            this.dayCurveGroup.XAxes.MinScale = 250000;
            this.dayCurveGroup.XAxes.SetOrgTime(ChartGraph.DateTime2ChartTime(DateTime.Now), 0);
            this.dayCurveGroup.CursorType = CurveCursorType.CURSOR_CROSSLINE;
            this.dayCurveGroup.DrawPointFlagXAxesScale = 5000000;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            float tt = 0.12f;

            UInt16[] data = new ushort[1];
            float[] ret = new float[1];

            ret[0] = 0.12f;
            HalfFloat.Singles2Halfp(data, ret, 1);

            Console.WriteLine();

            HalfFloat.Halfp2Singles(ret, data, 1);

            Console.WriteLine();




        }

        private void button2_Click(object sender, EventArgs e)
        {

            RecordDisk file = new RecordDisk("G:");

            file.RecordList[0].Channels[1].SignalArgsChanged += new EventHandler<RecordAnalyse.Signal.SignalArgs>(Form1_SignalArgsChanged);
            file.RecordList[0].Channels[1].DecodeFM = true;
            file.RecordList[0].Export();

            Console.WriteLine();


        }

        void Form1_SignalArgsChanged(object sender, RecordAnalyse.Signal.SignalArgs e)
        {

            DatabaseModule.GetInstance().AddAnalog(12, 2, e.LowFreq, e.Time);


        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            dayCurveGroup.ClearChartObject();

            LineArea la = new LineArea(chart, "日曲线", false);

            LineArea laCarrier = new LineArea(chart, "日曲线", false);

            la.YAxes.YAxesMin = 0;
            la.YAxes.YAxesMax = 30;


            laCarrier.YAxes.YAxesMin = 0;
            laCarrier.YAxes.YAxesMax = 3000;

            LineCurve curveLow = new LineCurve(chart, "信号低频", 0);
            LineCurve curveCarrier = new LineCurve(chart,"信号载频",0);

            la.AddLine(curveLow);
            laCarrier.AddLine(curveCarrier);


            curveLow.LineColor = Color.Red;
            DateTime timeBegin = new DateTime(2015, 7, 21);
            this.dayCurveGroup.XAxes.SetOrgTime(ChartGraph.DateTime2ChartTime(timeBegin), 0);

            List<AnalogRecordGroup> r = DatabaseModule.GetInstance().QueryAnalogHistory(12, 2, timeBegin, timeBegin.AddDays(1));


            if (r != null && r.Count > 0)
            {
                List<AnalogRecord> records = r[0].Records;

                for (int i = 0; i < records.Count; i++)
                {
                    long tm=ChartGraph.DateTime2ChartTime(records[i].Time);
                    curveLow.AddPoint(new LinePoint(tm, records[i].Value));
                }
            }

            r = DatabaseModule.GetInstance().QueryAnalogHistory(12, 1, timeBegin, timeBegin.AddDays(1));


            if (r != null && r.Count > 0)
            {
                List<AnalogRecord> records = r[0].Records;

                for (int i = 0; i < records.Count; i++)
                {
                    long tm = ChartGraph.DateTime2ChartTime(records[i].Time);
                    curveCarrier.AddPoint(new LinePoint(tm, records[i].Value));
                }
            }

            dayCurveGroup.AddChartObject(la);

            dayCurveGroup.AddChartObject(laCarrier);




        }


        DSP.APFFT fft = new RecordAnalyse.DSP.APFFT(8000);

       const int N = 8000;
        float[] data = new float[N * 2];
        float[] data1 = new float[N * 2];
      const  float ph1 = 45.86f;
      const float ph2 = ph1 - 330.258f;



        private void button4_Click(object sender, EventArgs e)
        {
           
             for (int i = 0; i < data.Length; i++)
             {
                 data[i] = (float)(5*Math.Sin(2 * Math.PI * 25* (i) / 8000 + ph1*Math.PI/180+123));
             }

             for (int i = 0; i < data.Length; i++)
             {
                 data1[i] = (float)(5 * Math.Sin(2 * Math.PI * 25 * ( i) / 8000 + ph2 * Math.PI / 180+123));
             }

            double angle= fft.CalAngle(data)*180/Math.PI;
            double angle1 = fft.CalAngle(data1) * 180 / Math.PI;
            Console.WriteLine(angle-angle1);
        }

        private void button5_Click(object sender, EventArgs e)
        {
           double ff= Math.Atan(1);

           Console.WriteLine();
        }
    }
}
