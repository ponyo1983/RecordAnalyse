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
using ConfigManager.HHDevice;
using ConfigManager.HHFormat.Device;
using ConfigManager.HHFormat.Curve;

namespace RecordAnalyse
{
    public partial class FormSwitchCurve : Form
    {
        static readonly Color[] ColorList = new Color[] { Color.Red, Color.Lime, Color.Yellow };
    


        HHDevice selectDev;


        public FormSwitchCurve()
        {
            InitializeComponent();


            UltraChart.CurveGroup grp = chart.AddNewGroup();
            //chart.Attach(hWnd);
            grp.SetValueTipTimeFormat("", TipTimeType.MICROSECOND);
            grp.CursorType = CurveCursorType.CURSOR_CROSSLINE;
            grp.XAxes.MaxScale = 4000000;
            grp.XAxes.MinScale = 20000;
            grp.XAxes.XAxesMode = XAxesMode.Relative;
            grp.DrawPointFlagXAxesScale = 200000;

  
            IList<HHDeviceGrp> devGrps = HHDeviceManager.GetInstance().DeviceGroupsUnsort;

            foreach (HHDeviceGrp type in devGrps)
            {
                for (int i = 0; i < type.CurveProperties.Count; i++)
                {
                    comboBoxEdit1.Properties.Items.Add(type.CurveProperties[i]);
                }

            }
            if (comboBoxEdit1.Properties.Items.Count > 0)
            {
                comboBoxEdit1.SelectedIndex = 0;
            }
        }


        private void DrawCurve()
        {
            HHDevice device = comboBoxEdit2.SelectedItem as HHDevice;

            HHDeviceProperty devProp = comboBoxEdit1.SelectedItem as HHDeviceProperty;
            DateTime timeSel = DateTime.Now;
            if (comboBoxEdit3.SelectedIndex >= 0)
            {
                timeSel = (DateTime)comboBoxEdit3.SelectedItem;
            }




            HHDeviceProperty devBindProp = device.GetProperty(devProp);


            List<DevCurve> curves = devBindProp.Curves;

            UltraChart.CurveGroup grp = chart.GroupList[0];
            grp.ClearChartObject();

            LineArea area = new LineArea(chart, "道岔曲线", true);
            area.IsShowFoldFlag = false;
            area.IsFold = false;
            area.YAxes.Precision = 3;
            area.YAxes.UnitString = "";
            grp.AddChartObject(area);
            grp.XAxes.SetOrgTime(ChartGraph.DateTime2ChartTime(timeSel), 0);
            chart.AutoSetXScale();


            List<StationCurve> listCurve = DataStorage.DatabaseModule.GetInstance().QueryCurveHistory(curves[0].Group.Type, curves[0].Index, timeSel);


            string[][] curveNames = new string[][] { 
            new string[]{"曲线"},
            new string[]{"曲线1","曲线2"},
            new string[]{"A相","B相","C相"},
            };

            for (int i = 0; i < curves.Count; i++)
            {
                LineCurve line = new LineCurve(chart, curveNames[curves.Count-1][i], 0);
                line.LineColor = ColorList[i % ColorList.Length];
                area.AddLine(line);
                if (listCurve != null && listCurve.Count > 0 && listCurve[i] != null)
                {


                    for (int j = 0; j < listCurve[i].Points.Length; j++)
                    {
                        DateTime time = timeSel.AddMilliseconds(curves[i].Group.TimeInterval * j); //40毫秒
                        LinePoint point = new LinePoint();
                        point.Value = listCurve[i].Points[j];
                        point.Time = ChartGraph.DateTime2ChartTime(time);
                        line.AddPoint(point);
                    }

                }

            }

            chart.Draw();
        }
        private void comboBoxEdit1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxEdit1.SelectedIndex >= 0)
            {


                HHDeviceProperty devProp = comboBoxEdit1.SelectedItem as HHDeviceProperty;


                IList<HHDevice> listDev = devProp.DevGrp.Devices;

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

            }
        }

        private void comboBoxEdit2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxEdit1.SelectedIndex >= 0 && comboBoxEdit2.SelectedIndex >= 0)
            {

                HHDeviceProperty prop = comboBoxEdit1.SelectedItem as HHDeviceProperty;

                HHDevice dev = comboBoxEdit2.SelectedItem as HHDevice;

                HHDeviceProperty devProp = dev.GetProperty(prop);


                List<DevCurve> curves = devProp.Curves;


                int curveIndex = 0;


               List<DateTime>  listTime = DataStorage.DatabaseModule.GetInstance().QueryCurveTimeList(curves[0].Group.Type, curves[0].Index);

              
                comboBoxEdit3.Properties.Items.Clear();
                comboBoxEdit3.Text = "";
                for (int i = 0; i < listTime.Count; i++)
                {
                    comboBoxEdit3.Properties.Items.Add(listTime[i]);
                }

                if (comboBoxEdit3.Properties.Items.Count > 0)
                {
                    comboBoxEdit3.SelectedIndex = 0;
                }
                else
                {
                    DrawCurve();
                }

            }

        }

        private void comboBoxEdit3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxEdit1.SelectedIndex < 0) return;
            if (comboBoxEdit2.SelectedIndex < 0) return;
            if (comboBoxEdit3.SelectedIndex < 0) return;

            DrawCurve();
            
        }

       

        private void simpleButton2_Click(object sender, EventArgs e)
        {

        }

        private void simpleButton3_Click(object sender, EventArgs e)
        {

        }

        private void simpleButton4_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
