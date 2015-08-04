using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using RecordAnalyse.Record;
using RecordAnalyse.GUI;

namespace RecordAnalyse
{
    public partial class FormSignalSelect : Form
    {
        public FormSignalSelect()
        {
            InitializeComponent();
        }

        private void FormSignalSelect_Load(object sender, EventArgs e)
        {
            DriveInfo[] allDrives = DriveInfo.GetDrives();

            foreach (DriveInfo info in allDrives)
            {
                if (info.DriveType == DriveType.Removable)
                {
                    comboBox1.Items.Add(info.Name.TrimEnd(new char[]{'\\'}));
                }
            }

            if (comboBox1.Items.Count > 0)
            {
                comboBox1.SelectedIndex = 0;
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string drive = comboBox1.Text;
            if (string.IsNullOrEmpty(drive))
            {
                gridControl1.DataSource = null;
                return;
            }
            RecordDisk recordDisk = DiskManager.GetInstance().GetDisk(drive);// new RecordDisk(drive);


            DiskChannel diskChannel = new DiskChannel(recordDisk);

            gridControl1.DataSource = diskChannel.Channels;
           

        }

        private void gridView1_CellValueChanging(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            if (e.Column.FieldName == "Select")
            {

                if ((bool)e.Value)
                {
                    IList<SelectChannel> channels = (IList<SelectChannel>)gridControl1.DataSource;

                    for (int i = 0; i < channels.Count; i++)
                    {

                        if (i != e.RowHandle)
                        {
                            channels[i].Select = false;

                        }
                        gridView1.RefreshRow(i);
                    }

                }

            }
           
        }

        private void button1_Click(object sender, EventArgs e)
        {
              IList<SelectChannel> channels = (IList<SelectChannel>)gridControl1.DataSource;

              for (int i = 0; i < channels.Count; i++)
              {
                  if (channels[i].Select)
                  {
                      this.Channel = channels[i];
                      break;
                  }
              }
            this.DialogResult = DialogResult.OK;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }


        public SelectChannel Channel
        {
            get;
            private set;
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "数据文件(*.dat)|*.dat";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    comboBox1.Items.Add(ofd.FileName);
                    comboBox1.SelectedIndex = comboBox1.Items.Count - 1;
                }
            }
        }

    

       
    }
}
