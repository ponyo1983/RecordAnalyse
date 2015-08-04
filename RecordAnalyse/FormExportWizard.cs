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
using System.IO;

namespace RecordAnalyse
{
    public partial class FormExportWizard : Form
    {


        string driveName = "";
        public FormExportWizard()
        {
            InitializeComponent();

        }

     
        private void welcomeWizardPage1_PageCommit(object sender, EventArgs e)
        {
            wizardPage1.AllowBack = false;
            wizardPage1.AllowNext = false;
        }

      

     


        private void wizardControl1_CancelClick(object sender, CancelEventArgs e)
        {
            
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

        private void FormExportWizard_Load(object sender, EventArgs e)
        {
            DriveInfo[] allDrives = DriveInfo.GetDrives();

            foreach (DriveInfo info in allDrives)
            {
                if (info.DriveType == DriveType.Removable)
                {
                    comboBox1.Items.Add(info.Name.TrimEnd(new char[] { '\\' }));
                }
            }

            if (comboBox1.Items.Count > 0)
            {
                comboBox1.SelectedIndex = 0;
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            driveName = comboBox1.Text;
            if (string.IsNullOrEmpty(driveName))
            {
                gridControl1.DataSource = null;
                return;
            }
            RecordDisk recordDisk = DiskManager.GetInstance().GetDisk(driveName);// new RecordDisk(drive);


        

            gridControl1.DataSource = recordDisk.RecordList;

            JudgeSelect();
        }


        private void JudgeSelect()
        {
            IList<RecordFile> files = (IList<RecordFile>)gridControl1.DataSource;
            if (files == null)
            {
                wizardPage1.AllowNext = false;
                return;
            }
            bool selectOne = false;

            for (int i = 0; i < files.Count; i++)
            {
                RecordFile f = files[i];
                selectOne = f.Select;

                if (selectOne) break;
            }
            if (string.IsNullOrEmpty(textEdit1.Text))
            {
                wizardPage1.AllowNext = false;
            }
            else
            {
                wizardPage1.AllowNext = selectOne;
            }
           
        }
     

        private void gridView1_CellValueChanging(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            if (e.Column.FieldName == "Select")
            {

                IList<RecordFile> files = (IList<RecordFile>)gridControl1.DataSource;

                bool selectOne = false;
               
                for (int i=0;i<files.Count;i++)
                {
                    RecordFile f = files[i];
                
                    if (i == e.RowHandle)
                    {
                        selectOne = (bool)e.Value;
                    }
                    else
                    {
                        selectOne = f.Select;
                    }

                    if (selectOne) break;
                }
                if (string.IsNullOrEmpty(textEdit1.Text))
                {
                    wizardPage1.AllowNext = false;
                }
                else
                {
                    wizardPage1.AllowNext = selectOne;
                }
           
               

            }
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "数据文件(*.dat)|*.dat";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    this.textEdit1.Text = sfd.FileName;

                    JudgeSelect();

                }
            }
        }

        private void wizardPage1_PageCommit(object sender, EventArgs e)
        {
            wizardPage2.AllowNext = false;
            wizardPage2.AllowBack = false;

            try
            {
                FileStream fs = new FileStream(textEdit1.Text, FileMode.OpenOrCreate);
                Thread threadExport = new Thread(new ParameterizedThreadStart(ProcExport));
                threadExport.IsBackground = true;
                threadExport.Start(fs);
                timer1.Start();
            }
            catch (Exception)
            {
 
            }

         
        }

        private void ProcExport(object obj)
        {
            try
            {
              
                RecordDisk recordDisk = DiskManager.GetInstance().GetDisk(driveName);// new RecordDisk(drive);

                FileStream fs = (FileStream)obj;
                
                recordDisk.Export(fs);
                fs.Close();
            }
            catch (Exception)
            {
 
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            RecordDisk recordDisk = DiskManager.GetInstance().GetDisk(driveName);
            if (recordDisk.ExportOK)
            {
                wizardPage2.AllowNext = true;
                progressBar1.Value = progressBar1.Maximum;
                progressBar2.Value = progressBar2.Maximum;
                timer1.Enabled = false;
            }
            else
            {
                progressBar1.Value = (int)(progressBar1.Maximum*recordDisk.CurrentFileRate);
                progressBar2.Value = (int)(progressBar2.Maximum * recordDisk.ExportRate);
            }
        }
    }
}
