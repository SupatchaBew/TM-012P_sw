﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace TM_012P_Setup_Program
{
    public partial class MsgBoxYesNo : Form
    {
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd,
                         int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();

        public MsgBoxYesNo()
        {
            InitializeComponent();
        }

        public Image MessageIcon
        {
            get { return msgicon.Image; }
            set { msgicon.Image = value; }
        }

        public string Message
        {
            get { return msgtextbox.Text; }
            set { msgtextbox.Text = value; }
        }

        public string Title
        {
            get { return msgtitleLabel.Text; }
            set { msgtitleLabel.Text = value; }
        }

        public Color formcolor
        {
            get { return msgPanel.BackColor; }
            set { msgPanel.BackColor = value; msgtextbox.BackColor = value; }
        }

        public Color btncolor
        {
            get { return okbtn.BackColor; }
            set { okbtn.BackColor = value; }
        }

        public string btntext
        {
            get { return okbtn.Text; }
            set { okbtn.Text = value; }
        }

        private void MsgBoxOK_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }
    }
}
