using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace TM_012P_Setup_Program
{
    public static class MyMsgBox
    {
        public static System.Windows.Forms.DialogResult Showmsg(string message, string caption, System.Windows.Forms.MessageBoxButtons button, System.Windows.Forms.MessageBoxIcon icon)
        {
            System.Windows.Forms.DialogResult dlgResult = System.Windows.Forms.DialogResult.None;
            switch(button)
            {
                case System.Windows.Forms.MessageBoxButtons.OK:
                    using (MsgBoxOK msgOK = new MsgBoxOK())
                    {
                        msgOK.Message = message;
                        msgOK.Title = caption;
                        switch(icon)
                        {
                            case System.Windows.Forms.MessageBoxIcon.Warning:
                                msgOK.MessageIcon = TM_012P_Setup_Program.Properties.Resources.Attention;
                                msgOK.formcolor = Color.LightSteelBlue;
                                msgOK.btncolor = Color.Silver;
                                msgOK.btntext = "Close";
                                break;
                            case System.Windows.Forms.MessageBoxIcon.None:
                                msgOK.MessageIcon = TM_012P_Setup_Program.Properties.Resources.Succeed;
                                msgOK.formcolor = Color.SeaGreen;
                                msgOK.btncolor = Color.SeaGreen;
                                msgOK.btntext = "OK";
                                break;
                            case System.Windows.Forms.MessageBoxIcon.Error:
                                msgOK.MessageIcon = TM_012P_Setup_Program.Properties.Resources.Error;
                                msgOK.formcolor = Color.Tomato;
                                msgOK.btncolor = Color.Silver;
                                msgOK.btntext = "Close";
                                break;
                            case System.Windows.Forms.MessageBoxIcon.Information:
                                msgOK.MessageIcon = TM_012P_Setup_Program.Properties.Resources.Question;
                                msgOK.formcolor = Color.Silver;
                                msgOK.btncolor = Color.Silver;
                                msgOK.btntext = "OK";
                                break;
                        }
                        dlgResult = msgOK.ShowDialog();
                    }
                    break;
                case System.Windows.Forms.MessageBoxButtons.YesNo:
                    using (MsgBoxYesNo msgYesNo = new MsgBoxYesNo())
                    {
                        msgYesNo.Message = message;
                        msgYesNo.Title = caption;
                        switch (icon)
                        {
                            case System.Windows.Forms.MessageBoxIcon.Warning:
                                msgYesNo.MessageIcon = TM_012P_Setup_Program.Properties.Resources.Attention;
                                msgYesNo.formcolor = Color.LightSteelBlue;
                                break;
                            case System.Windows.Forms.MessageBoxIcon.None:
                                msgYesNo.MessageIcon = TM_012P_Setup_Program.Properties.Resources.Succeed;
                                msgYesNo.formcolor = Color.SeaGreen;
                                break;
                            case System.Windows.Forms.MessageBoxIcon.Error:
                                msgYesNo.MessageIcon = TM_012P_Setup_Program.Properties.Resources.Error;
                                msgYesNo.formcolor = Color.Tomato;
                                break;
                            case System.Windows.Forms.MessageBoxIcon.Information:
                                msgYesNo.MessageIcon = TM_012P_Setup_Program.Properties.Resources.Question;
                                msgYesNo.formcolor = Color.Silver;
                                break;
                        }
                        dlgResult = msgYesNo.ShowDialog();
                    }
                    break;
            }
            return dlgResult;
        }
    }
}
