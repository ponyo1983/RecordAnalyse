using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using RecordAnalyse.GUI;
using UltraChart;
using RecordAnalyse.DSP;

namespace RecordAnalyse
{
    public partial class FormRawData : Form
    {


        SelectChannel selChannel;


        List<DateTime> listTimes = new List<DateTime>();

        UltraChart.CurveGroup dayCurveGroup;
        UltraChart.CurveGroup freqCurveGroup;

        int secNum = 5;

        FFTPlan fftPlan;
        float[] fftIn;
        float[] fftOut;

        DateTime timeQuery = DateTime.Now;

        public FormRawData()
        {
            InitializeComponent();

            this.dayCurveGroup = this.chart.AddNewGroup();
            this.dayCurveGroup.XAxes.MaxScale = 100000000L;
            this.dayCurveGroup.XAxes.MinScale = 100;
            this.dayCurveGroup.XAxes.SetScale(100000);
            this.dayCurveGroup.XAxes.SetOrgTime(ChartGraph.DateTime2ChartTime(DateTime.Now), 0);
            this.dayCurveGroup.CursorType = CurveCursorType.CURSOR_CROSSLINE;
            dayCurveGroup.XAxes.XAxesMode = XAxesMode.Relative;

            this.dayCurveGroup.DrawPointFlagXAxesScale = 500;


            this.freqCurveGroup = this.chart1.AddNewGroup();
            this.freqCurveGroup.XAxes.MaxScale = 100000000L;
            this.freqCurveGroup.XAxes.MinScale = 100;
            this.freqCurveGroup.XAxes.SetScale(100000);
            this.freqCurveGroup.XAxes.SetOrgTime(ChartGraph.DateTime2ChartTime(DateTime.Now), 0);
            this.freqCurveGroup.CursorType = CurveCursorType.CURSOR_CROSSLINE;
            freqCurveGroup.XAxes.XAxesMode = XAxesMode.Relative;

            this.freqCurveGroup.DrawPointFlagXAxesScale = 500;

            fftPlan = new FFTPlan(secNum * 8000);
            fftIn = new float[secNum * 8000 * 2];
            fftOut = new float[secNum * 8000 * 2];
        }

        private void simpleButton5_Click(object sender, EventArgs e)
        {
            using (FormSignalSelect formSel = new FormSignalSelect())
            {
                if (formSel.ShowDialog() == DialogResult.OK)
                {
                   selChannel= formSel.Channel;
                   textEdit1.Text = selChannel.ToString();
                   trackBarControl1.Properties.Maximum = selChannel.RecordSec;

                   int days = selChannel.RecordSec / (3600 * 24)+1;

                   comboBoxEdit1.Properties.Items.Clear();
                   listTimes.Clear();
                   for (int i = 0; i < days; i++)
                   {
                       DateTime time=selChannel.File.BeginTime.Date.AddDays(i);
                       comboBoxEdit1.Properties.Items.Add(time.ToString("yyyy/MM/dd"));
                       listTimes.Add(time);
                   }
                   if (days > 0)
                   {
                       comboBoxEdit1.SelectedIndex = 0;
                   }

                   QueryData();



                }
            }
        }

        private void QueryData()
        {
            if (selChannel == null) return;
            int selIndex = comboBoxEdit1.SelectedIndex;
            if (selIndex < 0) return;

            DateTime timeSel = listTimes[selIndex];

             timeQuery = timeSel.AddSeconds((timeEdit1.Time - timeEdit1.Time.Date).TotalSeconds);

            if (timeQuery < selChannel.File.BeginTime)
            {
                MessageBox.Show("查询时间小于记录开始时间!","查询",MessageBoxButtons.OK,MessageBoxIcon.Warning);
                return;
            }
            if (timeQuery > selChannel.File.EndTime)
            {
                MessageBox.Show("查询时间大于记录开始时间!", "查询", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }


            int sec = (int)((timeQuery - selChannel.File.BeginTime).TotalSeconds);
            trackBarControl1.ValueChanged -= new EventHandler(trackBarControl1_ValueChanged);
            trackBarControl1.Value = sec;
            trackBarControl1.ValueChanged += new EventHandler(trackBarControl1_ValueChanged);

            float[] data = selChannel.File.GetData(selChannel.Channel - 1, timeQuery, secNum);

            dayCurveGroup.ClearChartObject();
            freqCurveGroup.ClearChartObject();
            this.dayCurveGroup.XAxes.SetOrgTime(ChartGraph.DateTime2ChartTime(timeQuery), 0);
            this.freqCurveGroup.XAxes.SetOrgTime(ChartGraph.DateTime2ChartTime(timeQuery), 0);
            //幅度谱
            LineArea laAmpl = new LineArea(chart, "原始波形", false);
            laAmpl.YAxes.YAxesMin = 0;
            laAmpl.YAxes.YAxesMax = 200;
            LineCurve lcAmpl = new LineCurve(chart, "原始波形", 0);

            lcAmpl.LineColor = Color.Lime;
            laAmpl.AddLine(lcAmpl);
            //lc.YAxes.Mode = YAxesMode.Manual;
            lcAmpl.YAxes.YAxesMin = 0;
            lcAmpl.YAxes.YAxesMax = 100;

            if (data != null)
            {
                long startTm = ChartGraph.DateTime2ChartTime(timeQuery);
                for (int j = 0; j < data.Length; j++)
                {

                    long tm = startTm +j* 1000000L / 8000;
                   // var tm = timeQuery.AddMilliseconds(j / 8.0);
                    lcAmpl.AddPoint(new LinePoint(tm, data[j]));
                }
            }


            //频率谱
            LineArea laFreq = new LineArea(chart1, "频率谱", false);
            laFreq.YAxes.YAxesMin = 0;
            laFreq.YAxes.YAxesMax = 200;
            laFreq.XFreqScale = 8000f / (secNum *1000000);
            LineCurve lcFreq = new LineCurve(chart1, "频率谱", 0);

            lcFreq.LineColor = Color.Red;
            laFreq.AddLine(lcFreq);
            lcFreq.YAxes.Mode = YAxesMode.Manual;
            lcFreq.YAxes.YAxesMin = -1;
            lcFreq.YAxes.YAxesMax = 15;

            if (data != null)
            {
                if (fftPlan.FFTNum != secNum * 8000)
                {
                    fftPlan.Dispose();
                    fftPlan = new FFTPlan(secNum * 8000);
                    fftIn=new float[secNum*16000];
                    fftOut=new float[secNum*16000];

                }
              
                for (int i = 0; i < data.Length; i++)
                {
                    fftIn[i * 2] = data[i];
                    fftIn[i * 2 + 1] = 0;
                }
                fftPlan.FFTForward(fftIn, fftOut);

                long startTm = ChartGraph.DateTime2ChartTime(timeQuery);
                float maxAmpl = 0;
                for (int j = 0; j < data.Length/2; j++)
                {

                    long tm = startTm + j * 1000000L / 8000;
                    // var tm = timeQuery.AddMilliseconds(j / 8.0);
                    float amplVal = 0;
                    if (j > 2) //不显示直流
                    {
                        amplVal = (float)(Math.Sqrt(fftOut[2 * j] * fftOut[2 * j] + fftOut[2 * j + 1] * fftOut[2 * j + 1]) * 2 / (8000 * secNum));
                    }
                    if (amplVal > maxAmpl)
                    {
                        maxAmpl = amplVal;
                    }
                    lcFreq.AddPoint(new LinePoint(tm, amplVal));
                }

                lcFreq.YAxes.YAxesMax = maxAmpl*1.2f;

            }

            dayCurveGroup.AddChartObject(laAmpl);
            freqCurveGroup.AddChartObject(laFreq);
            chart.Draw();
            chart1.Draw();

        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            QueryData();
        }

        private void comboBoxEdit1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selIndex = comboBoxEdit1.SelectedIndex;
            if (selIndex < 0) return;

            DateTime timeSel = listTimes[selIndex];

            DateTime timeSet = timeSel.AddSeconds((timeEdit1.Time - timeEdit1.Time.Date).TotalSeconds);

            if (timeSet < selChannel.File.BeginTime || timeSet > selChannel.File.EndTime)
            {
                if (selIndex == 0)
                {
                    timeEdit1.Time = selChannel.File.BeginTime; 
                }
                else
                {
                    timeEdit1.Time = timeSel;
                }
            }
            timeSet = timeSel.AddSeconds((timeEdit1.Time - timeEdit1.Time.Date).TotalSeconds);
            int sec = (int)((timeSet - selChannel.File.BeginTime).TotalSeconds);
            trackBarControl1.ValueChanged-=new EventHandler(trackBarControl1_ValueChanged);
            trackBarControl1.Value = sec;
            trackBarControl1.ValueChanged+=new EventHandler(trackBarControl1_ValueChanged);

        }

        private void trackBarControl1_ValueChanged(object sender, EventArgs e)
        {
            if (selChannel == null) return;

            int secs = trackBarControl1.Value;
            DateTime time = selChannel.File.BeginTime.AddSeconds(secs);

            timeEdit1.Time = time;

            int index=(int)((time.Date-selChannel.File.BeginTime.Date).TotalDays);
            comboBoxEdit1.SelectedIndex=index;


            if (MouseButtons != MouseButtons.Left)
            {
                QueryData();
            }

        }

        private void trackBarControl1_MouseUp(object sender, MouseEventArgs e)
        {
            QueryData();
        }

        private void simpleButton4_Click(object sender, EventArgs e)
        {
            this.Close();
        }



        private String GetName()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("原始波形_{0}{1:yyyy年MM月dd日}",
               comboBoxEdit1.Text,
               timeQuery
                );
            return sb.ToString();
        }

        private void simpleButton2_Click(object sender, EventArgs e)
        {
            if (this.dayCurveGroup.CurveList.Count <= 0)
            {
                MessageBox.Show("没有可打印的曲线！", "提示");
                return;
            }
            this.chart.Print(GetName());
        }

        private void simpleButton3_Click(object sender, EventArgs e)
        {

            if (this.dayCurveGroup.CurveList.Count == 0)
            {
                MessageBox.Show("没有可导出的曲线！", "提示");
                return;
            }

            SaveFileDialog savefile = new SaveFileDialog();
            savefile.OverwritePrompt = true;
            savefile.Filter = "*.png|*.png|*.bmp|*.bmp";
            savefile.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            savefile.AddExtension = true;
            savefile.DefaultExt = ".png";
            String str = GetName();

            string fileName = str;
            savefile.FileName = fileName.Replace('/', '-');
            if (savefile.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            if (chart.SaveAsFile(savefile.FileName))
            {
                MessageBox.Show("曲线导出成功!", "曲线导出", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void FormRawData_Shown(object sender, EventArgs e)
        {
            //splitContainerControl1.Panel1.Height = splitContainerControl1.Height / 2;
            //groupBox1.Height = splitContainerControl1.Height / 2;
        }

        private void spinEdit1_EditValueChanged(object sender, EventArgs e)
        {
            this.secNum = (int)spinEdit1.Value;
        }


    }
}
