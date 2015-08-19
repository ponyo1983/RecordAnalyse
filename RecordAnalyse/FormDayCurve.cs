using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ConfigManager.Device;
using UltraChart;
using Common;
using DataStorage;
using ConfigManager.HHDevice;
using ConfigManager.HHFormat.Device;

namespace RecordAnalyse
{
    public partial class FormDayCurve : Form
    {

        UltraChart.CurveGroup dayCurveGroup;


        HHDevice selectDev = null;


      static readonly  Color[] colorList = new Color[] { Color.Red, Color.Lime, Color.Yellow };

        public FormDayCurve()
        {
            InitializeComponent();


            this.dayCurveGroup = this.chart.AddNewGroup();
            this.dayCurveGroup.XAxes.MaxScale = 60000000*60L;
            this.dayCurveGroup.XAxes.MinScale = 250000;
            this.dayCurveGroup.XAxes.SetOrgTime(ChartGraph.DateTime2ChartTime(DateTime.Now), 0);
            this.dayCurveGroup.CursorType = CurveCursorType.CURSOR_CROSSLINE;
            this.dayCurveGroup.DrawPointFlagXAxesScale = 5000000;


            IList<HHDeviceGrp> types = HHDeviceManager.GetInstance().DeviceGroupsUnsort;


            foreach (HHDeviceGrp type in types)
            {
                if (type.AnalogProperties.Count > 0)
                {
                   comboBoxEdit1.Properties.Items.Add(type);
                
                 
                }
            }
            if (comboBoxEdit1.Properties.Items.Count > 0)
            {
                comboBoxEdit1.SelectedIndex = 0;
            }

            dateEdit1.DateTime = DateTime.Now.Date;
        }

        private void FormDayCurve_Load(object sender, EventArgs e)
        {

        }

        private void comboBoxEdit1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxEdit1.SelectedIndex >= 0)
            {


                HHDeviceGrp devType = comboBoxEdit1.SelectedItem as HHDeviceGrp;


                IList<HHDevice> listDev = devType.Devices;

                comboBoxEdit2.Properties.Items.Clear();


                comboBoxEdit2.Text = "";
                for (int i = 0; i < listDev.Count; i++)
                {
                    comboBoxEdit2.Properties.Items.Add(listDev[i]);
                }

                if (listDev.Count > 0)
                {
                    comboBoxEdit2.SelectedIndex = 0;
                }

                gridControl1.DataSource = devType.AnalogProperties;
            }
        }

        private void comboBoxEdit2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxEdit1.SelectedIndex >= 0 && comboBoxEdit2.SelectedIndex>=0)
            {
                selectDev = comboBoxEdit2.SelectedItem as HHDevice;

                DrawCurve();
            }
        }

        private void gridView1_CellValueChanging(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            if (e.Column.FieldName == "Selected")
            {

                HHDeviceGrp devType = comboBoxEdit1.SelectedItem as HHDeviceGrp;

                devType.SelectProperty(e.RowHandle, (bool)e.Value);


                int num = devType.AnalogProperties.Count;

                for (int i = 0; i < num; i++)
                {
                    gridView1.RefreshRow(i);
                }

                DrawCurve();
            }
        }

        public void DrawCurve()
        {
            dayCurveGroup.ClearChartObject();
            DateTime time = dateEdit1.DateTime.Date;
            this.dayCurveGroup.XAxes.SetOrgTime(ChartGraph.DateTime2ChartTime(time), 0);
          
            IList<HHDeviceProperty> listProp = selectDev.DevGroup.AnalogProperties;

            int colorIndex = -1;

            DateTime drawTime = DateTime.MinValue;

            for (int i = 0; i < listProp.Count; i++)
            {
                if (listProp[i].Selected == false) continue;
                colorIndex++;
                HHDeviceProperty devProp = selectDev.GetProperty(listProp[i]);
                int analogIndex = devProp.Analog.Index;
                LineArea la = new LineArea(chart, listProp[i].Name, false);
                la.YAxes.YAxesMin = 0;
                la.YAxes.YAxesMax = 200;
                LineCurve lc = new LineCurve(chart, listProp[i].Name, 0);
               
                lc.LineColor = colorList[colorIndex%colorList.Length];
                la.AddLine(lc);
                lc.YAxes.Mode = YAxesMode.Manual;
                lc.YAxes.YAxesMin = devProp.Analog.ADMin;
                lc.YAxes.YAxesMax = devProp.Analog.ADMax;

                List<AnalogRecordGroup> r = DatabaseModule.GetInstance().QueryAnalogHistory(devProp.Analog.Group.Type, analogIndex, time, time.AddDays(1));


                if (r != null && r.Count > 0)
                {
                    List<AnalogRecord> records = r[0].Records;

                    for (int j = 0; j < records.Count; j++)
                    {
                        if (drawTime == DateTime.MinValue)
                        {
                            drawTime = records[j].Time;
                        }
                        else if (drawTime > records[j].Time)
                        {
                            drawTime = records[j].Time;
                        }
                        long tm = ChartGraph.DateTime2ChartTime(records[j].Time);
                        lc.AddPoint(new LinePoint(tm, records[j].Value));
                    }
                }

                dayCurveGroup.AddChartObject(la);

            }
            if (drawTime == DateTime.MinValue)
            {
                drawTime = time;
            }
            this.dayCurveGroup.XAxes.SetOrgTime(ChartGraph.DateTime2ChartTime(drawTime), 0);
            chart.Draw();

            




            
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            DrawCurve();
        }

        private void simpleButton4_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void simpleButton2_Click(object sender, EventArgs e)
        {
            if (this.comboBoxEdit2.SelectedItem== null)
            {
                MessageBox.Show("没有可打印的曲线设备！", "提示");
                return;
            }
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

        private String GetName()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("日曲线_{0}{1}_{2:yyyy年MM月dd日}",
               comboBoxEdit1.Text,
                comboBoxEdit2.Text,
                dateEdit1.DateTime.Date
                );
            return sb.ToString();
        }

        private void dateEdit1_EditValueChanged(object sender, EventArgs e)
        {
            chart.Focus();
            DrawCurve();
        }
       
    }
}
