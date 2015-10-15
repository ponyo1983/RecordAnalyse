using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.IO;

namespace RecordAnalyse
{
    public partial class FormAbout : Form
    {
        public FormAbout()
        {
            InitializeComponent();

            Assembly assembly = GetType().Assembly;
            System.IO.Stream streamSmall = assembly.GetManifestResourceStream("RecordAnalyse.master");
            StreamReader sr = new StreamReader(streamSmall);
            string sha1 = sr.ReadToEnd();

            sr.Close();

            labelControl4.Text = "SHA-1: " + sha1;

        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
