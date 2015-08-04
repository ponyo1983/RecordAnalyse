namespace RecordAnalyse
{
    partial class FormMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.simpleButton1 = new DevExpress.XtraEditors.SimpleButton();
            this.imageCollection1 = new DevExpress.Utils.ImageCollection(this.components);
            this.simpleButton2 = new DevExpress.XtraEditors.SimpleButton();
            this.simpleButton3 = new DevExpress.XtraEditors.SimpleButton();
            this.simpleButton4 = new DevExpress.XtraEditors.SimpleButton();
            this.simpleButton5 = new DevExpress.XtraEditors.SimpleButton();
            this.simpleButton6 = new DevExpress.XtraEditors.SimpleButton();
            ((System.ComponentModel.ISupportInitialize)(this.imageCollection1)).BeginInit();
            this.SuspendLayout();
            // 
            // simpleButton1
            // 
            this.simpleButton1.AllowFocus = false;
            this.simpleButton1.Appearance.Font = new System.Drawing.Font("Tahoma", 16F, System.Drawing.FontStyle.Bold);
            this.simpleButton1.Appearance.ForeColor = System.Drawing.Color.DimGray;
            this.simpleButton1.Appearance.Options.UseFont = true;
            this.simpleButton1.Appearance.Options.UseForeColor = true;
            this.simpleButton1.ImageIndex = 3;
            this.simpleButton1.ImageList = this.imageCollection1;
            this.simpleButton1.ImageLocation = DevExpress.XtraEditors.ImageLocation.TopCenter;
            this.simpleButton1.Location = new System.Drawing.Point(12, 9);
            this.simpleButton1.Name = "simpleButton1";
            this.simpleButton1.Size = new System.Drawing.Size(246, 200);
            this.simpleButton1.TabIndex = 0;
            this.simpleButton1.Text = "1:导入数据";
            this.simpleButton1.ToolTip = "导入数据";
            this.simpleButton1.Click += new System.EventHandler(this.simpleButton1_Click);
            // 
            // imageCollection1
            // 
            this.imageCollection1.ImageSize = new System.Drawing.Size(128, 128);
            this.imageCollection1.ImageStream = ((DevExpress.Utils.ImageCollectionStreamer)(resources.GetObject("imageCollection1.ImageStream")));
            this.imageCollection1.Images.SetKeyName(0, "raw_data.png");
            this.imageCollection1.Images.SetKeyName(1, "vector_curve.png");
            this.imageCollection1.Images.SetKeyName(2, "vector_day.png");
            this.imageCollection1.Images.SetKeyName(3, "vector_usb.png");
            this.imageCollection1.Images.SetKeyName(4, "export_data.png");
            this.imageCollection1.Images.SetKeyName(5, "About.png");
            // 
            // simpleButton2
            // 
            this.simpleButton2.AllowFocus = false;
            this.simpleButton2.Appearance.Font = new System.Drawing.Font("Tahoma", 16F, System.Drawing.FontStyle.Bold);
            this.simpleButton2.Appearance.ForeColor = System.Drawing.Color.DimGray;
            this.simpleButton2.Appearance.Options.UseFont = true;
            this.simpleButton2.Appearance.Options.UseForeColor = true;
            this.simpleButton2.ImageIndex = 2;
            this.simpleButton2.ImageList = this.imageCollection1;
            this.simpleButton2.ImageLocation = DevExpress.XtraEditors.ImageLocation.TopCenter;
            this.simpleButton2.Location = new System.Drawing.Point(278, 9);
            this.simpleButton2.Name = "simpleButton2";
            this.simpleButton2.Size = new System.Drawing.Size(246, 200);
            this.simpleButton2.TabIndex = 0;
            this.simpleButton2.Text = "2:日曲线";
            this.simpleButton2.ToolTip = "日曲线";
            this.simpleButton2.Click += new System.EventHandler(this.simpleButton2_Click);
            // 
            // simpleButton3
            // 
            this.simpleButton3.AllowFocus = false;
            this.simpleButton3.Appearance.Font = new System.Drawing.Font("Tahoma", 16F, System.Drawing.FontStyle.Bold);
            this.simpleButton3.Appearance.ForeColor = System.Drawing.Color.DimGray;
            this.simpleButton3.Appearance.Options.UseFont = true;
            this.simpleButton3.Appearance.Options.UseForeColor = true;
            this.simpleButton3.ImageIndex = 1;
            this.simpleButton3.ImageList = this.imageCollection1;
            this.simpleButton3.ImageLocation = DevExpress.XtraEditors.ImageLocation.TopCenter;
            this.simpleButton3.Location = new System.Drawing.Point(278, 225);
            this.simpleButton3.Name = "simpleButton3";
            this.simpleButton3.Size = new System.Drawing.Size(246, 200);
            this.simpleButton3.TabIndex = 0;
            this.simpleButton3.Text = "5:道岔曲线";
            this.simpleButton3.ToolTip = "道岔曲线";
            this.simpleButton3.Click += new System.EventHandler(this.simpleButton3_Click);
            // 
            // simpleButton4
            // 
            this.simpleButton4.AllowFocus = false;
            this.simpleButton4.Appearance.Font = new System.Drawing.Font("Tahoma", 16F, System.Drawing.FontStyle.Bold);
            this.simpleButton4.Appearance.ForeColor = System.Drawing.Color.DimGray;
            this.simpleButton4.Appearance.Options.UseFont = true;
            this.simpleButton4.Appearance.Options.UseForeColor = true;
            this.simpleButton4.ImageIndex = 0;
            this.simpleButton4.ImageList = this.imageCollection1;
            this.simpleButton4.ImageLocation = DevExpress.XtraEditors.ImageLocation.TopCenter;
            this.simpleButton4.Location = new System.Drawing.Point(544, 9);
            this.simpleButton4.Name = "simpleButton4";
            this.simpleButton4.Size = new System.Drawing.Size(246, 200);
            this.simpleButton4.TabIndex = 0;
            this.simpleButton4.Text = "3:原始曲线";
            this.simpleButton4.ToolTip = "原始曲线";
            this.simpleButton4.Click += new System.EventHandler(this.simpleButton4_Click);
            // 
            // simpleButton5
            // 
            this.simpleButton5.AllowFocus = false;
            this.simpleButton5.Appearance.Font = new System.Drawing.Font("Tahoma", 16F, System.Drawing.FontStyle.Bold);
            this.simpleButton5.Appearance.ForeColor = System.Drawing.Color.DimGray;
            this.simpleButton5.Appearance.Options.UseFont = true;
            this.simpleButton5.Appearance.Options.UseForeColor = true;
            this.simpleButton5.ImageIndex = 4;
            this.simpleButton5.ImageList = this.imageCollection1;
            this.simpleButton5.ImageLocation = DevExpress.XtraEditors.ImageLocation.TopCenter;
            this.simpleButton5.Location = new System.Drawing.Point(12, 225);
            this.simpleButton5.Name = "simpleButton5";
            this.simpleButton5.Size = new System.Drawing.Size(246, 200);
            this.simpleButton5.TabIndex = 0;
            this.simpleButton5.Text = "4:导出数据";
            this.simpleButton5.ToolTip = "导出数据";
            this.simpleButton5.Click += new System.EventHandler(this.simpleButton5_Click);
            // 
            // simpleButton6
            // 
            this.simpleButton6.AllowFocus = false;
            this.simpleButton6.Appearance.Font = new System.Drawing.Font("Tahoma", 16F, System.Drawing.FontStyle.Bold);
            this.simpleButton6.Appearance.ForeColor = System.Drawing.Color.DimGray;
            this.simpleButton6.Appearance.Options.UseFont = true;
            this.simpleButton6.Appearance.Options.UseForeColor = true;
            this.simpleButton6.ImageIndex = 5;
            this.simpleButton6.ImageList = this.imageCollection1;
            this.simpleButton6.ImageLocation = DevExpress.XtraEditors.ImageLocation.TopCenter;
            this.simpleButton6.Location = new System.Drawing.Point(544, 225);
            this.simpleButton6.Name = "simpleButton6";
            this.simpleButton6.Size = new System.Drawing.Size(246, 200);
            this.simpleButton6.TabIndex = 0;
            this.simpleButton6.Text = "6:关于程序";
            this.simpleButton6.ToolTip = "关于程序";
            this.simpleButton6.Click += new System.EventHandler(this.simpleButton6_Click);
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(803, 434);
            this.Controls.Add(this.simpleButton6);
            this.Controls.Add(this.simpleButton4);
            this.Controls.Add(this.simpleButton3);
            this.Controls.Add(this.simpleButton2);
            this.Controls.Add(this.simpleButton5);
            this.Controls.Add(this.simpleButton1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "FormMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "移动监测数据分析";
            ((System.ComponentModel.ISupportInitialize)(this.imageCollection1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraEditors.SimpleButton simpleButton1;
        private DevExpress.XtraEditors.SimpleButton simpleButton2;
        private DevExpress.XtraEditors.SimpleButton simpleButton3;
        private DevExpress.XtraEditors.SimpleButton simpleButton4;
        private DevExpress.Utils.ImageCollection imageCollection1;
        private DevExpress.XtraEditors.SimpleButton simpleButton5;
        private DevExpress.XtraEditors.SimpleButton simpleButton6;
    }
}