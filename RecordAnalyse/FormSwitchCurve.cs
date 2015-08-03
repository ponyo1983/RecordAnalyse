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

namespace RecordAnalyse
{
    public partial class FormSwitchCurve : Form
    {
        static readonly Color[] ColorList = new Color[] { Color.Red, Color.Lime, Color.Yellow };
        List<DeviceType> listTypes = new List<DeviceType>();
        List<DateTime> listTime = new List<DateTime>();
        Device selectDev;

      
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

            IList<DeviceType> types = DeviceTypeManager.GetInstance().DeviceTypes;


            foreach (DeviceType type in types)
            {
                if (type.DCProperties.Count > 0)
                {
                    comboBoxEdit1.Properties.Items.Add(type.Name);
                    listTypes.Add(type);

                }
            }
            if (comboBoxEdit1.Properties.Items.Count > 0)
            {
                comboBoxEdit1.SelectedIndex = 0;
            }
        }

        private void comboBoxEdit1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxEdit1.SelectedIndex >= 0)
            {


                DeviceType devType = listTypes[comboBoxEdit1.SelectedIndex];


                List<Device> listDev = DeviceManager.GetInstance().GetDevices(devType.TypeID);

                comboBoxEdit2.Properties.Items.Clear();


                comboBoxEdit2.Text = "";
                for (int i = 0; i < listDev.Count; i++)
                {
                    comboBoxEdit2.Properties.Items.Add(listDev[i].Name);
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

                DeviceType devType = listTypes[comboBoxEdit1.SelectedIndex];


                List<Device> listDev = DeviceManager.GetInstance().GetDevices(devType.TypeID);

                selectDev = listDev[comboBoxEdit2.SelectedIndex];


                int curveIndex = devType.DCProperties[0].PropertyIndex + selectDev.Index * 20;


                 listTime = DataStorage.DatabaseModule.GetInstance().QueryCurveTimeList(devType.TypeID, curveIndex);

              
                comboBoxEdit3.Properties.Items.Clear();
                comboBoxEdit3.Text = "";
                for (int i = 0; i < listTime.Count; i++)
                {
                    comboBoxEdit3.Properties.Items.Add(listTime[i].ToString("yyyy/MM/dd HH:mm:ss"));
                }

                if (listTime.Count > 0)
                {
                    comboBoxEdit3.SelectedIndex = 0;
                }

            }

        }

        private void comboBoxEdit3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxEdit1.SelectedIndex >= 0 && comboBoxEdit2.SelectedIndex >= 0 && comboBoxEdit3.SelectedIndex >= 0)
            {
                DeviceType devType = listTypes[comboBoxEdit1.SelectedIndex];

                DateTime timeSel = listTime[comboBoxEdit3.SelectedIndex];

                List<Device> listDev = DeviceManager.GetInstance().GetDevices(devType.TypeID);
                selectDev = listDev[comboBoxEdit2.SelectedIndex];

                List<DeviceProperty> dcProperties = devType.DCProperties;

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


                for (int i = 0; i < dcProperties.Count; i++)
                {
                    int curveIndex = devType.DCProperties[i].PropertyIndex + selectDev.Index * 20;
                    List<StationCurve> listCurve = DataStorage.DatabaseModule.GetInstance().QueryCurveHistory(devType.TypeID, curveIndex, timeSel);

                    LineCurve line = new LineCurve(chart, dcProperties[i].Name, 0);
                    line.LineColor = ColorList[i % ColorList.Length];
                    area.AddLine(line);
                    if (listCurve != null && listCurve.Count > 0 && listCurve[0]!=null)
                    {
                      

                        for (int j = 0; j < listCurve[0].Points.Length; j++)
                        {
                            DateTime time = timeSel.AddMilliseconds(40*j); //40毫秒
                            LinePoint point = new LinePoint();
                            point.Value=listCurve[0].Points[j];
                            point.Time = ChartGraph.DateTime2ChartTime(time);
                            line.AddPoint(point);
                        }

                    }

                }



            }
        }

        private void DrawCurve()
        {
 
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
