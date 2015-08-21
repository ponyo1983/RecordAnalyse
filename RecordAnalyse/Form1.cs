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
using RecordAnalyse.Utils;

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

            float[] ff = new float[1] ;

            HalfFloat.Halfp2Singles(ff,new ushort[]{(ushort)0x33cd},1);

            Console.WriteLine();

        }

        private void button2_Click(object sender, EventArgs e)
        {
            CurveManager.GetInstance();
        }
    }
}
