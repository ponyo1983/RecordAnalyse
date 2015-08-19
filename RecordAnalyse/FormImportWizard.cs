using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ConfigManager.Device;
using System.Collections;
using DevExpress.XtraTab;
using RecordAnalyse.GUI;
using RecordAnalyse.Record;
using System.Threading;
using ConfigManager.HHDevice;
using ConfigManager.HHFormat.Device;
using ConfigManager.HHFormat.Analog;
using ConfigManager.HHFormat.Curve;

namespace RecordAnalyse
{
    public partial class FormImportWizard : Form
    {

        
        const int PropChannelNum = 3; //每个属性(包括道岔)最多3个采集通道

        const int MaxSrcs = PropChannelNum*30;

        HHDeviceManager hhDevManager = HHDeviceManager.GetInstance();


        HHDevice selectDevice;

        SelectChannel[] selectChannels = new SelectChannel[MaxSrcs]; //最大
        TextBox[] txtBoxes = new TextBox[MaxSrcs];

        Button[] buttons = new Button[MaxSrcs];



        long exportTotal = 0;
        long exportedSize = 0;

        int exportSrcIndex = 0;

        RecordFile exportFile;


        bool exportOK=false;

        public FormImportWizard()
        {
            InitializeComponent();


            IList<HHDeviceGrp> listTypes = hhDevManager.DeviceGroups;
            for (int i = 0; i < listTypes.Count; i++)
            {
                comboBoxEdit1.Properties.Items.Add(listTypes[i]);
            }
            if (comboBoxEdit1.Properties.Items.Count > 0)
            {
                comboBoxEdit1.SelectedIndex = 0;
            }

            

        }

        private void InitDevList(HHDeviceGrp devGrp)
        {
            comboBoxEdit2.Properties.Items.Clear();

            for (int i = 0; i < MaxSrcs; i++)
            {
                selectChannels[i] = null;
                txtBoxes[i] = null;
                buttons[i] = null;
            }
            comboBoxEdit2.Text = "";

            IList<HHDevice> devices = devGrp.Devices;
            for (int i = 0; i < devices.Count; i++)
            {
                comboBoxEdit2.Properties.Items.Add(devices[i]);
            }

            List<HHSourceGroup> grps = devGrp.SourceGroups;

            xtraTabControl1.TabPages.Clear();


            for (int i = 0; i < grps.Count; i++)
            {
                XtraTabPage tabPage = new XtraTabPage();
                tabPage.Name = "Signal" + i;
                tabPage.Text = "信号源" + (i + 1);

                int height = 15;

                int grpCnt = 1;

                for (int j = 0; j < grps[i].Properties.Count; j++)
                {
                    string propName = grps[i].Properties[j].Name;
                    Label label = new Label();
                    label.AutoSize = true;
                    label.Font = new Font("宋体", 11, FontStyle.Bold);
                    label.Text = (j + 1) + ":" + propName;
                    label.Location = new Point(15, height);

                    height += 30;



                    tabPage.Controls.Add(label);

                    grpCnt = grps[i].Properties[j].GroupCount;

                }

                string[][] txtList=new string[][]{
                new string[]{"选取"},
                new string[]{"参考","选取"},
                new string[]{"A相(或单相)","B相","C相"},
                };
                for (int j = 0; j < grpCnt; j++)
                {
                    TextBox textBox = new TextBox();
                    txtBoxes[i * PropChannelNum + j] = textBox;
                    textBox.Location = new Point(15, height);
                    textBox.Size = new Size(150, 25);
                    textBox.ReadOnly = true;
                    textBox.BackColor = Color.Pink;
                    tabPage.Controls.Add(textBox);


                    Button bt = new Button();
                    bt.Tag = i * PropChannelNum + j;
                    bt.Location = new Point(textBox.Left + textBox.Width, height);
                    bt.Size = new Size(80, 25);


                    bt.Text = txtList[grpCnt - 1][j];
                    tabPage.Controls.Add(bt);

                    bt.Click += new EventHandler(bt_Click);

                    height += 30;
                }



                xtraTabControl1.TabPages.Add(tabPage);
            }
        }

        private void comboBoxEdit1_SelectedIndexChanged(object sender, EventArgs e)
        {

            HHDeviceGrp devGrp = comboBoxEdit1.SelectedItem as HHDeviceGrp;
            if (devGrp == null) return;

            if (devGrp.Level == 2)
            {
                IList<HHDeviceGrp> childGrp = devGrp.ChildGrp;

                comboBoxEdit3.Properties.Items.Clear();
                for (int i = 0; i < childGrp.Count; i++)
                {
                    comboBoxEdit3.Properties.Items.Add(childGrp[i]);
                }
                comboBoxEdit1.Width = 200;
                labelControl5.Visible = true;
                comboBoxEdit3.Visible = true;
                if (childGrp.Count > 0)
                {
                    comboBoxEdit3.SelectedIndex = 0;
                }

             
            }
            else
            {
                labelControl5.Visible = false;
                comboBoxEdit3.Visible = false;
                comboBoxEdit1.Width = comboBoxEdit2.Width;

                InitDevList(devGrp);

            }


        }

        void bt_Click(object sender, EventArgs e)
        {
            Button bt=(Button)sender;
            using (FormSignalSelect form = new FormSignalSelect())
            {
                if(form.ShowDialog()==DialogResult.OK)
                {
                    TextBox txtBox = txtBoxes[(int)bt.Tag];
                    if (form.Channel != null)
                    {
                        selectChannels[(int)bt.Tag] = form.Channel;
                        txtBox.Text = form.Channel.ToString();
                        if (comboBoxEdit2.Text != "")
                        {
                            wizardPage1.AllowNext = true;
                        }
                    }

               
                }
            }
        }

        private void welcomeWizardPage1_PageCommit(object sender, EventArgs e)
        {
            wizardPage1.AllowBack = false;
            wizardPage1.AllowNext = false;
        }

        private void comboBoxEdit2_EditValueChanged(object sender, EventArgs e)
        {
            if (comboBoxEdit2.Text != "")
            {
                for (int i = 0; i < selectChannels.Length; i++)
                {
                    if (selectChannels[i] != null)
                    {
                        wizardPage1.AllowNext = true;
                        return;
                    }

                }
                wizardPage1.AllowNext = false;

            }
            else
            {
                wizardPage1.AllowNext = false;
            }
        }

        private void wizardPage1_PageCommit(object sender, EventArgs e)
        {
            wizardPage2.AllowBack = false;
            wizardPage2.AllowNext = false;


            selectDevice = comboBoxEdit2.SelectedItem as HHDevice;

            if (selectDevice != null)
            {
                Thread threadExport = new Thread(new ThreadStart(ExportProc));
                threadExport.IsBackground = true;
                threadExport.Start();

                timer1.Start();
            }
        }


        private void ExportProc()
        {
            try
            {
                exportTotal = 0;
                exportedSize = 0;
                for (int i = 0; i < MaxSrcs; i++)
                {
                    if (selectChannels[i] != null)
                    {
                        exportTotal += selectChannels[i].File.Length;
                    }
                }
                for (int i = 0; i < MaxSrcs; i += PropChannelNum)
                {
                    
                    if (selectChannels[i] != null)
                    {
                        exportFile = selectChannels[i].File;
                        if (i > 0 && (selectChannels[i - PropChannelNum] != null))
                        {
                            exportedSize += selectChannels[i - PropChannelNum].File.Length;
                        }
                        SignalChannel sigChannel = selectChannels[i].File.Channels[selectChannels[i].Channel-1];

                        int srcIndex = i / PropChannelNum;


                        sigChannel.DecodeFM = false;
                        sigChannel.DecodeCurve = 0;
                        sigChannel.DecodeAngle = false;

                        sigChannel.DecodeFM=selectDevice.DevGroup.SourceGroups[srcIndex].AllowFM;
                        int allowCurve=selectDevice.DevGroup.SourceGroups[srcIndex].AllowCurve;
                        sigChannel.DecodeCurve = allowCurve;
                        bool allowAngle = selectDevice.DevGroup.SourceGroups[srcIndex].AllowAngle;
                        sigChannel.DecodeAngle = allowAngle;
                        if (allowAngle && selectChannels[i + 1] != null) //参考相位
                        {
                            SignalChannel sigChannel1 = selectChannels[i + 1].File.Channels[selectChannels[i + 1].Channel - 1];
                            sigChannel1.DecodeAngle = true;
                            sigChannel1.IsReference = true;
                            selectChannels[i + 1].File.SignalArgsChanged += new EventHandler<RecordAnalyse.Signal.SignalArgs>(FormDataWizard_SignalArgsChanged);
                        }
                        else if (allowCurve>0) //解码道岔曲线
                        {
                           HHDeviceProperty devBindProp= selectDevice.GetProperty(selectDevice.DevGroup.SourceGroups[srcIndex].Properties[0]);
                            DevCurve devCurve= devBindProp.Curves[0];
                            sigChannel.TimeInterval = devCurve.TimeInterval;

                            if (devCurve.MonitorType == Common.SignalType.SignalDCCurve)
                            {
                                sigChannel.DecodeCurve = 2;
                            }
                            else
                            {
                                sigChannel.DecodeCurve = 1;
                            }

                            if ((selectChannels[i + 1] == null) && (selectChannels[i + 2] == null)) //单相
                            {
                                sigChannel.SignalArgsChanged += new EventHandler<RecordAnalyse.Signal.SignalArgs>(FormDataWizard_SignalArgsChanged);
                            }
                            else
                            {
                                curveGrp = new Common.CurveGroup(3, devCurve.Group.Type, devCurve.Index);


                                sigChannel.SignalArgsChanged += new EventHandler<RecordAnalyse.Signal.SignalArgs>(FormDataWizard_SignalArgsChangedA);

                                if (selectChannels[i + 1] != null)
                                {
                                    selectChannels[i + 1].File.Channels[selectChannels[i + 1].Channel - 1].SignalArgsChanged += new EventHandler<RecordAnalyse.Signal.SignalArgs>(FormDataWizard_SignalArgsChangedB); //B相
                                }
                                if (selectChannels[i + 2] != null)
                                {
                                    selectChannels[i + 2].File.Channels[selectChannels[i + 2].Channel - 1].SignalArgsChanged += new EventHandler<RecordAnalyse.Signal.SignalArgs>(FormDataWizard_SignalArgsChangedC); //C相
                                }
                            }
                        }
                        else
                        {
                            sigChannel.SignalArgsChanged += new EventHandler<RecordAnalyse.Signal.SignalArgs>(FormDataWizard_SignalArgsChanged);
                        }
                        exportSrcIndex = srcIndex;
                        selectChannels[i].File.Export();
                        if (allowAngle && selectChannels[i + 1] != null) //参考相位
                        {
                            SignalChannel sigChannel1 = selectChannels[i + 1].File.Channels[selectChannels[i + 1].Channel - 1];
                            sigChannel1.DecodeAngle = true;
                            sigChannel1.SignalArgsChanged -= new EventHandler<RecordAnalyse.Signal.SignalArgs>(FormDataWizard_SignalArgsChanged);
                        }
                        else if ((allowCurve>0) && ((selectChannels[i + 1] != null) || (selectChannels[i + 2] != null))) //有2条以上曲线
                        {
                            HHDeviceProperty devBindProp = selectDevice.GetProperty(selectDevice.DevGroup.SourceGroups[srcIndex].Properties[0]);
                            DevCurve devCurve = devBindProp.Curves[0];
                          

                            sigChannel.SignalArgsChanged -= new EventHandler<RecordAnalyse.Signal.SignalArgs>(FormDataWizard_SignalArgsChangedA);
                            if (selectChannels[i + 1] != null)
                            {
                                selectChannels[i + 1].File.Channels[selectChannels[i + 1].Channel - 1].SignalArgsChanged -= new EventHandler<RecordAnalyse.Signal.SignalArgs>(FormDataWizard_SignalArgsChangedB); //B相
                            }
                            if (selectChannels[i + 2] != null)
                            {
                                selectChannels[i + 2].File.Channels[selectChannels[i + 2].Channel - 1].SignalArgsChanged -= new EventHandler<RecordAnalyse.Signal.SignalArgs>(FormDataWizard_SignalArgsChangedC); //C相
                            }

                        }
                        else
                        {
                            sigChannel.SignalArgsChanged -= new EventHandler<RecordAnalyse.Signal.SignalArgs>(FormDataWizard_SignalArgsChanged);
                        }
                        
                        
                    }
                }

                exportOK = true;
            }
            catch (Exception)
            {
 
            }
        }


        Common.CurveGroup curveGrp = null;

        System.Threading.Timer theadTimer=null;




        void timer_tick(object obj)
        {
            if (curveGrp != null)
            {
                DataStorage.DatabaseModule.GetInstance().AddCurve(curveGrp);
            }
            theadTimer.Dispose();
            theadTimer = null;
        }

        void FormDataWizard_SignalArgsChangedA(object sender, RecordAnalyse.Signal.SignalArgs e)
        {
            if (curveGrp != null)
            {
                curveGrp.AddCurve(0, e.Time, 25, e.ACCurve);
            }
            if (theadTimer == null)
            {
                theadTimer = new System.Threading.Timer(new TimerCallback(timer_tick), null, 5000, System.Threading.Timeout.Infinite);
            }
          
        }
        void FormDataWizard_SignalArgsChangedB(object sender, RecordAnalyse.Signal.SignalArgs e)
        {
            if (curveGrp != null)
            {
                curveGrp.AddCurve(1, e.Time, 25, e.ACCurve);
            }
            if (theadTimer == null)
            {
                theadTimer = new System.Threading.Timer(new TimerCallback(timer_tick), null, 5000, System.Threading.Timeout.Infinite);
            }
        }
        void FormDataWizard_SignalArgsChangedC(object sender, RecordAnalyse.Signal.SignalArgs e)
        {
            if (curveGrp != null)
            {
                curveGrp.AddCurve(2, e.Time, 25, e.ACCurve);
            }
            if (theadTimer == null)
            {
                theadTimer = new System.Threading.Timer(new TimerCallback(timer_tick), null, 5000, System.Threading.Timeout.Infinite);
            }
        }

        void FormDataWizard_SignalArgsChanged(object sender, RecordAnalyse.Signal.SignalArgs e)
        {
            if (exportSrcIndex >= 0 && selectDevice!=null)
            {

                HHSourceGroup srcGrp = selectDevice.DevGroup.SourceGroups[exportSrcIndex];

                foreach (HHDeviceProperty prop in srcGrp.Properties)
                {

                    HHDeviceProperty devProp = selectDevice.GetProperty(prop);
                    DevAnalog devAnalog = devProp.Analog;
                    List<DevCurve> devCurves = devProp.Curves;
                    switch (prop.MonitorType)
                    {
                        case Common.SignalType.SignalAC:
                            if (devAnalog != null)
                            {
                                DataStorage.DatabaseModule.GetInstance().AddAnalog(devAnalog.Group.Type, devAnalog.Index, e.ACAmpl, e.Time);
                            }
                            break;
                        case Common.SignalType.SignalDC:
                            if (devAnalog != null)
                            {
                                DataStorage.DatabaseModule.GetInstance().AddAnalog(devAnalog.Group.Type, devAnalog.Index, e.DCAmpl, e.Time);
                            }
                            break;
                        case Common.SignalType.SignalCarrier:
                            if (e.LowFreq > 0 && devAnalog!=null )
                            {
                                DataStorage.DatabaseModule.GetInstance().AddAnalog(devAnalog.Group.Type, devAnalog.Index, e.CarrierFreq, e.Time);
                            }
                            break;
                        case Common.SignalType.SignalLow:
                            if (e.LowFreq > 0 && devAnalog!=null )
                            {
                                DataStorage.DatabaseModule.GetInstance().AddAnalog(devAnalog.Group.Type, devAnalog.Index, e.LowFreq, e.Time);
                            }
                            break;
                        case Common.SignalType.SignalACCurve:
                            if (devCurves != null && devCurves.Count > 0)
                            {
                                DataStorage.DatabaseModule.GetInstance().AddCurve(devCurves[0].Group.Type, devCurves[0].Index, e.Time, 25, e.ACCurve);
                            }
                            break;
                        case Common.SignalType.SignalDCCurve:
                            break;
                        case Common.SignalType.SignalAngle:
                            if (devAnalog != null)
                            {
                                DataStorage.DatabaseModule.GetInstance().AddAnalog(devAnalog.Group.Type, devAnalog.Index, e.AngleDiff, e.Time);
                            }
                            break;
                    }


                }

                


                

            }
        }



        private void wizardControl1_CancelClick(object sender, CancelEventArgs e)
        {
            
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (exportFile != null)
            {
                float rate = (exportedSize + exportFile.ExportLength)*1f / exportTotal;
                progressBar2.Value = (int)(progressBar2.Maximum * rate);
                progressBar1.Value = (int)(exportFile.ExportLength * progressBar1.Maximum * 1f / exportFile.Length);
            }
            if (exportOK)
            {
                wizardPage2.AllowNext = true;
                progressBar2.Value = progressBar2.Maximum ;
                progressBar1.Value = progressBar1.Maximum;
                timer1.Enabled = false;
            }
        }

        private void wizardPage2_PageCommit(object sender, EventArgs e)
        {
            completionWizardPage1.AllowCancel = false;
            completionWizardPage1.AllowBack = false;
        }

        private void wizardControl1_FinishClick(object sender, CancelEventArgs e)
        {
            this.Close();
        }

        private void comboBoxEdit3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxEdit3.Visible && comboBoxEdit3.SelectedItem!=null)
            {
                HHDeviceGrp devGrp = comboBoxEdit3.SelectedItem as HHDeviceGrp;
                if (devGrp != null)
                {
                    InitDevList(devGrp);
                }
            }
        }
    }
}
