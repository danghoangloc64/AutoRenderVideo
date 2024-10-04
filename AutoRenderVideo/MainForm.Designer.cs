namespace AutoRenderVideo
{
    partial class MainForm
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
            this.txtAmNen = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtSoundNhac = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtHinhAnh = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.txtSoLuongToiDa = new System.Windows.Forms.TextBox();
            this.btnChonAmNen = new System.Windows.Forms.Button();
            this.btnChonSoundNhac = new System.Windows.Forms.Button();
            this.btnChonHinhAnh = new System.Windows.Forms.Button();
            this.btnStart = new System.Windows.Forms.Button();
            this.txtSoPhutDauRa = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.rtbLog = new System.Windows.Forms.RichTextBox();
            this.txtSoLuongFileRender = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.btnStop = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // txtAmNen
            // 
            this.txtAmNen.Location = new System.Drawing.Point(188, 13);
            this.txtAmNen.Name = "txtAmNen";
            this.txtAmNen.ReadOnly = true;
            this.txtAmNen.Size = new System.Drawing.Size(519, 20);
            this.txtAmNen.TabIndex = 0;
            this.txtAmNen.TextChanged += new System.EventHandler(this.txtAmNen_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(170, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Âm nền (ghép vào đoạn đầu tiên):";
            // 
            // txtSoundNhac
            // 
            this.txtSoundNhac.Location = new System.Drawing.Point(188, 42);
            this.txtSoundNhac.Name = "txtSoundNhac";
            this.txtSoundNhac.ReadOnly = true;
            this.txtSoundNhac.Size = new System.Drawing.Size(519, 20);
            this.txtSoundNhac.TabIndex = 2;
            this.txtSoundNhac.TextChanged += new System.EventHandler(this.txtSoundNhac_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 46);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(68, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Sound nhạc:";
            // 
            // txtHinhAnh
            // 
            this.txtHinhAnh.Location = new System.Drawing.Point(188, 71);
            this.txtHinhAnh.Name = "txtHinhAnh";
            this.txtHinhAnh.ReadOnly = true;
            this.txtHinhAnh.Size = new System.Drawing.Size(519, 20);
            this.txtHinhAnh.TabIndex = 4;
            this.txtHinhAnh.TextChanged += new System.EventHandler(this.txtHinhAnh_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 75);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 13);
            this.label3.TabIndex = 1;
            this.label3.Text = "Hình ảnh:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 104);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(82, 13);
            this.label4.TabIndex = 1;
            this.label4.Text = "Số luồng tối đa:";
            // 
            // txtSoLuongToiDa
            // 
            this.txtSoLuongToiDa.Location = new System.Drawing.Point(188, 100);
            this.txtSoLuongToiDa.Name = "txtSoLuongToiDa";
            this.txtSoLuongToiDa.Size = new System.Drawing.Size(106, 20);
            this.txtSoLuongToiDa.TabIndex = 6;
            this.txtSoLuongToiDa.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.txtSoLuongToiDa.TextChanged += new System.EventHandler(this.txtSoLuongToiDa_TextChanged);
            this.txtSoLuongToiDa.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtSoLuongToiDa_KeyPress);
            // 
            // btnChonAmNen
            // 
            this.btnChonAmNen.Location = new System.Drawing.Point(713, 12);
            this.btnChonAmNen.Name = "btnChonAmNen";
            this.btnChonAmNen.Size = new System.Drawing.Size(75, 23);
            this.btnChonAmNen.TabIndex = 1;
            this.btnChonAmNen.Text = "Chọn";
            this.btnChonAmNen.UseVisualStyleBackColor = true;
            this.btnChonAmNen.Click += new System.EventHandler(this.btnChonAmNen_Click);
            // 
            // btnChonSoundNhac
            // 
            this.btnChonSoundNhac.Location = new System.Drawing.Point(713, 41);
            this.btnChonSoundNhac.Name = "btnChonSoundNhac";
            this.btnChonSoundNhac.Size = new System.Drawing.Size(75, 23);
            this.btnChonSoundNhac.TabIndex = 3;
            this.btnChonSoundNhac.Text = "Chọn";
            this.btnChonSoundNhac.UseVisualStyleBackColor = true;
            this.btnChonSoundNhac.Click += new System.EventHandler(this.btnChonSoundNhac_Click);
            // 
            // btnChonHinhAnh
            // 
            this.btnChonHinhAnh.Location = new System.Drawing.Point(713, 70);
            this.btnChonHinhAnh.Name = "btnChonHinhAnh";
            this.btnChonHinhAnh.Size = new System.Drawing.Size(75, 23);
            this.btnChonHinhAnh.TabIndex = 5;
            this.btnChonHinhAnh.Text = "Chọn";
            this.btnChonHinhAnh.UseVisualStyleBackColor = true;
            this.btnChonHinhAnh.Click += new System.EventHandler(this.btnChonHinhAnh_Click);
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(12, 575);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(776, 23);
            this.btnStart.TabIndex = 11;
            this.btnStart.Text = "Chạy";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // txtSoPhutDauRa
            // 
            this.txtSoPhutDauRa.Location = new System.Drawing.Point(430, 127);
            this.txtSoPhutDauRa.Name = "txtSoPhutDauRa";
            this.txtSoPhutDauRa.Size = new System.Drawing.Size(106, 20);
            this.txtSoPhutDauRa.TabIndex = 8;
            this.txtSoPhutDauRa.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.txtSoPhutDauRa.TextChanged += new System.EventHandler(this.txtSoPhutDauRa_TextChanged);
            this.txtSoPhutDauRa.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtSoPhutDauRa_KeyPress);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(343, 131);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(81, 13);
            this.label5.TabIndex = 1;
            this.label5.Text = "Số phút đầu ra:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(542, 131);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(34, 13);
            this.label6.TabIndex = 1;
            this.label6.Text = "(phút)";
            // 
            // rtbLog
            // 
            this.rtbLog.Location = new System.Drawing.Point(12, 153);
            this.rtbLog.Name = "rtbLog";
            this.rtbLog.ReadOnly = true;
            this.rtbLog.Size = new System.Drawing.Size(776, 416);
            this.rtbLog.TabIndex = 9;
            this.rtbLog.Text = "";
            this.rtbLog.TextChanged += new System.EventHandler(this.rtbLog_TextChanged);
            // 
            // txtSoLuongFileRender
            // 
            this.txtSoLuongFileRender.Location = new System.Drawing.Point(188, 127);
            this.txtSoLuongFileRender.Name = "txtSoLuongFileRender";
            this.txtSoLuongFileRender.Size = new System.Drawing.Size(106, 20);
            this.txtSoLuongFileRender.TabIndex = 7;
            this.txtSoLuongFileRender.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.txtSoLuongFileRender.TextChanged += new System.EventHandler(this.txtSoLuongFileRender_TextChanged);
            this.txtSoLuongFileRender.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtSoLuongFileRender_KeyPress);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(12, 131);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(130, 13);
            this.label7.TabIndex = 1;
            this.label7.Text = "Số lượng file muốn render:";
            // 
            // btnStop
            // 
            this.btnStop.Location = new System.Drawing.Point(12, 604);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(776, 23);
            this.btnStop.TabIndex = 12;
            this.btnStop.Text = "Dừng";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 639);
            this.Controls.Add(this.rtbLog);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.btnChonHinhAnh);
            this.Controls.Add(this.btnChonSoundNhac);
            this.Controls.Add(this.btnChonAmNen);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtSoPhutDauRa);
            this.Controls.Add(this.txtSoLuongFileRender);
            this.Controls.Add(this.txtSoLuongToiDa);
            this.Controls.Add(this.txtHinhAnh);
            this.Controls.Add(this.txtSoundNhac);
            this.Controls.Add(this.txtAmNen);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Auto Render Video - v3";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtAmNen;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtSoundNhac;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtHinhAnh;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtSoLuongToiDa;
        private System.Windows.Forms.Button btnChonAmNen;
        private System.Windows.Forms.Button btnChonSoundNhac;
        private System.Windows.Forms.Button btnChonHinhAnh;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.TextBox txtSoPhutDauRa;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.RichTextBox rtbLog;
        private System.Windows.Forms.TextBox txtSoLuongFileRender;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button btnStop;
    }
}

