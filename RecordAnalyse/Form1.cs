using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ConfigManager.HHFormat.Analog;
using ConfigManager.HHDevice;
using ConfigManager.HHFormat.Curve;

namespace RecordAnalyse
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            HHDeviceManager.GetInstance();


        }

        private void button2_Click(object sender, EventArgs e)
        {
            CurveManager.GetInstance();
        }
    }
}
