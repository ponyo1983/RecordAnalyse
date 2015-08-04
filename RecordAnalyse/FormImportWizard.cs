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

namespace RecordAnalyse
{
    public partial class FormImportWizard : Form
    {

        const int MaxSrcs = 40;
        DeviceTypeManager devTypeManager = DeviceTypeManager.GetInstance();


        Device selectDevice;






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


            IList<DeviceType> types = devTypeManager.DeviceTypes;


            foreach (DeviceType type in types)
            {
                comboBoxEdit1.Properties.Items.Add(type.Name);
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
                IList<DeviceType> types = devTypeManager.DeviceTypes;

                DeviceType devType = types[comboBoxEdit1.SelectedIndex];


                List<Device> listDev = DeviceManager.GetInstance().GetDevices(devType.TypeID);

                comboBoxEdit2.Properties.Items.Clear();

                for (int i = 0; i < MaxSrcs; i++)
                {
                    selectChannels[i] = null;
                    txtBoxes[i] = null;
                    buttons[i] = null;
                }
                comboBoxEdit2.Text = "";
                for (int i = 0; i < listDev.Count; i++)
                {
                    comboBoxEdit2.Properties.Items.Add(listDev[i].Name);
                }

                List<SourceGroup> grps = devType.SourceGroups;

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
                        label.Font = new Font("宋体", 11, FontStyle.Bold);
                        label.Text = (j + 1) + ":" + propName;
                        label.Location = new Point(15, height);

                        height += 30;



                        tabPage.Controls.Add(label);

                        grpCnt = grps[i].Properties[j].GroupCount;

                    }

                    for (int j = 0; j < grpCnt; j++)
                    {
                        TextBox textBox = new TextBox();
                        txtBoxes[i*2+j] = textBox;
                        textBox.Location = new Point(15, height);
                        textBox.Size = new Size(150, 25);
                        textBox.ReadOnly = true;
                        textBox.BackColor = Color.Pink;
                        tabPage.Controls.Add(textBox);


                        Button bt = new Button();
                        bt.Tag = i*2+j;
                        bt.Location = new Point(textBox.Left + textBox.Width, height);
                        bt.Size = new Size(50, 25);
                        bt.Text =(j==0&&grpCnt>1)?"参考": "选取";
                        tabPage.Controls.Add(bt);

                        bt.Click += new EventHandler(bt_Click);

                        height += 30;
                    }

                   

                    xtraTabControl1.TabPages.Add(tabPage);
                }

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
            DeviceType type = devTypeManager.DeviceTypes[comboBoxEdit1.SelectedIndex];
          
            
            selectDevice = DeviceManager.GetInstance().GetDevice(comboBoxEdit2.Text, type.TypeID, true);

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
                for (int i = 0; i < MaxSrcs; i+=2)
                {
                    
                    if (selectChannels[i] != null)
                    {
                        exportFile = selectChannels[i].File;
                        if (i > 0 && (selectChannels[i-2]!=null))
                        {
                            exportedSize += selectChannels[i-2].File.Length;
                        }
                        SignalChannel sigChannel = selectChannels[i].File.Channels[selectChannels[i].Channel-1];

                        int srcIndex=i/2;

                       
                        sigChannel.DecodeFM=selectDevice.DevType.SourceGroups[srcIndex].AllowFM;
                        sigChannel.DecodeCurve = selectDevice.DevType.SourceGroups[srcIndex].AllowCurve;
                        bool allowAngle = selectDevice.DevType.SourceGroups[srcIndex].AllowAngle;
                        sigChannel.DecodeAngle = allowAngle;
                        if (allowAngle && selectChannels[i + 1] != null) //参考相位
                        {
                            SignalChannel sigChannel1 = selectChannels[i + 1].File.Channels[selectChannels[i + 1].Channel - 1];
                            sigChannel1.DecodeAngle = true;
                            sigChannel1.IsReference = true;
                            selectChannels[i + 1].File.SignalArgsChanged += new EventHandler<RecordAnalyse.Signal.SignalArgs>(FormDataWizard_SignalArgsChanged);
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
                        else
                        {
                            sigChannel.SignalArgsChanged -= new EventHandler<RecordAnalyse.Signal.SignalArgs>(FormDataWizard_SignalArgsChanged);
                        }
                        sigChannel.DecodeFM = false;
                        sigChannel.DecodeCurve = false;
                        sigChannel.DecodeAngle = false;
                        
                    }
                }

                exportOK = true;
            }
            catch (Exception)
            {
 
            }
        }

        void FormDataWizard_SignalArgsChanged(object sender, RecordAnalyse.Signal.SignalArgs e)
        {
            if (exportSrcIndex >= 0 && selectDevice!=null)
            {

                SourceGroup srcGrp = selectDevice.DevType.SourceGroups[exportSrcIndex];

                int analogIndex=selectDevice.Index*20;

                foreach (DeviceProperty prop in srcGrp.Properties)
                {

                    switch (prop.SignalType)
                    {
                        case Common.SignalType.SignalAC:
                            DataStorage.DatabaseModule.GetInstance().AddAnalog(selectDevice.DevType.TypeID, analogIndex + prop.PropertyIndex, e.ACAmpl, e.Time);
                            break;
                        case Common.SignalType.SignalDC:
                            DataStorage.DatabaseModule.GetInstance().AddAnalog(selectDevice.DevType.TypeID, analogIndex + prop.PropertyIndex, e.DCAmpl, e.Time);
                            break;
                        case Common.SignalType.SignalCarrier:
                            if (e.LowFreq > 0)
                            {
                                DataStorage.DatabaseModule.GetInstance().AddAnalog(selectDevice.DevType.TypeID, analogIndex + prop.PropertyIndex, e.CarrierFreq, e.Time);
                            }
                            break;
                        case Common.SignalType.SignalLow:
                            if (e.LowFreq > 0)
                            {
                                DataStorage.DatabaseModule.GetInstance().AddAnalog(selectDevice.DevType.TypeID, analogIndex + prop.PropertyIndex, e.LowFreq, e.Time);
                            }
                            break;
                        case Common.SignalType.SignalACCurve:
                            DataStorage.DatabaseModule.GetInstance().AddCurve(selectDevice.DevType.TypeID, analogIndex + prop.PropertyIndex, e.Time, 25, e.ACCurve);
                            break;
                        case Common.SignalType.SignalDCCurve:
                            break;
                        case Common.SignalType.SignalAngle:
                            DataStorage.DatabaseModule.GetInstance().AddAnalog(selectDevice.DevType.TypeID, analogIndex + prop.PropertyIndex, e.AngleDiff, e.Time);
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
    }
}
