using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace RecordAnalyse
{
    public partial class FormMain : Form
    {

        FormDayCurve formDayCurve;
        FormSwitchCurve formSwitchCurve;
        FormRawData formRawData;
        public FormMain()
        {
            InitializeComponent();

            DataStorage.DatabaseModule.GetInstance().Start();
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            using (FormDataWizard form = new FormDataWizard())
            {
                form.ShowDialog();
            }
        }

        private void simpleButton2_Click(object sender, EventArgs e)
        {
            if (formDayCurve == null || formDayCurve.IsDisposed)
            {
                formDayCurve = new FormDayCurve();
                //formDayCurve.Owner = this;
                formDayCurve.Show();
            }
            else
            {
                formDayCurve.WindowState = FormWindowState.Maximized;
            }
        }

        private void simpleButton3_Click(object sender, EventArgs e)
        {
            if (formSwitchCurve == null || formSwitchCurve.IsDisposed)
            {
                formSwitchCurve = new FormSwitchCurve();
                //formDayCurve.Owner = this;
                formSwitchCurve.Show();
            }
            else
            {
                formSwitchCurve.WindowState = FormWindowState.Maximized;
            }
        }

        private void simpleButton4_Click(object sender, EventArgs e)
        {
            if (formRawData == null || formRawData.IsDisposed)
            {
                formRawData = new FormRawData();
                //formDayCurve.Owner = this;
                formRawData.Show();
            }
            else
            {
                formRawData.WindowState = FormWindowState.Maximized;
            }
        }
    }
}
