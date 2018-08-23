namespace TM_012P_Setup_Program
{
    partial class MsgBoxOK
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
            this.okbtn = new System.Windows.Forms.Button();
            this.msgPanel = new System.Windows.Forms.Panel();
            this.msgtitleLabel = new System.Windows.Forms.Label();
            this.msgicon = new System.Windows.Forms.PictureBox();
            this.msgtextbox = new System.Windows.Forms.TextBox();
            this.msgPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.msgicon)).BeginInit();
            this.SuspendLayout();
            // 
            // okbtn
            // 
            this.okbtn.BackColor = System.Drawing.Color.SeaGreen;
            this.okbtn.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okbtn.FlatAppearance.BorderSize = 0;
            this.okbtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.okbtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.okbtn.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.okbtn.Location = new System.Drawing.Point(250, 218);
            this.okbtn.Name = "okbtn";
            this.okbtn.Size = new System.Drawing.Size(115, 35);
            this.okbtn.TabIndex = 0;
            this.okbtn.Text = "&OK";
            this.okbtn.UseVisualStyleBackColor = false;
            // 
            // msgPanel
            // 
            this.msgPanel.BackColor = System.Drawing.Color.SeaGreen;
            this.msgPanel.Controls.Add(this.msgtextbox);
            this.msgPanel.Controls.Add(this.msgtitleLabel);
            this.msgPanel.Controls.Add(this.msgicon);
            this.msgPanel.Location = new System.Drawing.Point(-2, -1);
            this.msgPanel.Name = "msgPanel";
            this.msgPanel.Size = new System.Drawing.Size(604, 198);
            this.msgPanel.TabIndex = 2;
            // 
            // msgtitleLabel
            // 
            this.msgtitleLabel.AutoSize = true;
            this.msgtitleLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 16.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.msgtitleLabel.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.msgtitleLabel.Location = new System.Drawing.Point(189, 42);
            this.msgtitleLabel.Name = "msgtitleLabel";
            this.msgtitleLabel.Size = new System.Drawing.Size(99, 32);
            this.msgtitleLabel.TabIndex = 1;
            this.msgtitleLabel.Text = "label1";
            // 
            // msgicon
            // 
            this.msgicon.Image = global::TM_012P_Setup_Program.Properties.Resources.Attention;
            this.msgicon.Location = new System.Drawing.Point(29, 42);
            this.msgicon.Name = "msgicon";
            this.msgicon.Size = new System.Drawing.Size(124, 117);
            this.msgicon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.msgicon.TabIndex = 0;
            this.msgicon.TabStop = false;
            // 
            // msgtextbox
            // 
            this.msgtextbox.BackColor = System.Drawing.Color.SeaGreen;
            this.msgtextbox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.msgtextbox.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.msgtextbox.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.msgtextbox.Location = new System.Drawing.Point(195, 77);
            this.msgtextbox.Multiline = true;
            this.msgtextbox.Name = "msgtextbox";
            this.msgtextbox.Size = new System.Drawing.Size(374, 82);
            this.msgtextbox.TabIndex = 3;
            // 
            // MsgBoxOK
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(50)))), ((int)(((byte)(60)))));
            this.ClientSize = new System.Drawing.Size(600, 274);
            this.Controls.Add(this.msgPanel);
            this.Controls.Add(this.okbtn);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "MsgBoxOK";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MsgBox";
            this.TopMost = true;
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.MsgBoxOK_MouseDown);
            this.msgPanel.ResumeLayout(false);
            this.msgPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.msgicon)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button okbtn;
        private System.Windows.Forms.Panel msgPanel;
        private System.Windows.Forms.Label msgtitleLabel;
        private System.Windows.Forms.PictureBox msgicon;
        private System.Windows.Forms.TextBox msgtextbox;
    }
}