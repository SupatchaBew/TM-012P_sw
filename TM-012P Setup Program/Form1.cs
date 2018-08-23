using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using Modbus_Poll_CS;
using System.Threading;
using System.IO;
using System.Windows.Forms.DataVisualization.Charting;
using TM_012P_Setup_Program.Properties;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace TM_012P_Setup_Program
{
    public partial class Form1 : Form
    {
        private static modbus mb = new modbus();
        public static Thread sampleThread1;
        public static int timecount = 0;

        private decimal _decimalp;
        private static int device_model = 0; //0=TC. 1=R
        Byte pv1;
        Byte pv2;
        Byte pv3;
        Byte pv4;
        Byte[] pvByte;
        private static short[] values = new short[16];
        private static bool _connected = false;
        private static bool _deviceisset = true;
        private static bool _mbissent = false;
        private static bool _mbisread = false;

        short inputselect;
        short pvgainselect;
        short pvadjselect;
        short pvfilselect;
        short cfselect;
        int sllselect;
        int slhselect;
        short invselect;
        int inlselect;
        int inhselect;
        short defaultset;

        bool input_change = false;
        bool pvgain_change = false;
        bool pvadj_change = false;
        bool pvfil_change = false;
        bool cf_change = false;
        bool sll_change = false;
        bool slh_change = false;
        bool inv_change = false;
        bool inl_change = false;
        bool inh_change = false;

        Decimal pvgain_min = 0.90M;
        Decimal pvgain_max = 1.20M;
        Decimal pvadj_min = -199.9M;
        Decimal pvadj_max = 999.9M;
        Decimal pvfil_min = 0M;
        Decimal pvfil_max = 32M;
        Decimal sll_min;
        Decimal sll_max;
        Decimal slh_min;
        Decimal slh_max;
        Decimal inl_min = 4.00M;
        Decimal inl_max = 20.00M;
        Decimal inh_min = 4.00M;
        Decimal inh_max = 20.00M;

        Int32 pv;
        Int16 input;
        Int16 sll;
        Int16 slh;
        Int16 inv;
        Int16 inl;
        Int16 inh;

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd,
                         int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();

        public Form1()
        {
            InitializeComponent();
            Init_settingBox();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
        void Get_SerialPort()
        {
            deviceBox.Items.Clear();
            String[] devicePort = SerialPort.GetPortNames();
            deviceBox.Items.AddRange(devicePort);
        }
        void Init_settingBox()
        {
            pleaseclickLabel.Text = "Please click connect....";
            //initial input type 
            System.Object[] input_type = new System.Object[9];
            input_type[0] = "TC-K";
            input_type[1] = "TC-J";
            input_type[2] = "TC-R";
            input_type[3] = "TC-T";
            input_type[4] = "TC-N";
            input_type[5] = "TC-S";
            input_type[6] = "TC-E";
            input_type[7] = "PT100";
            input_type[8] = "5K\u2126";
            inputBox.Items.AddRange(input_type);
            //initial input setting limit
            sllnumud.Maximum = 9999.9M;
            sllnumud.Minimum = -999.9M;
            slhnumud.Maximum = 9999.9M;
            slhnumud.Minimum = -999.9M;
            //initial output limit
            inlnumud.Maximum = 100.00M;
            inlnumud.Minimum = 0.00M;
            inhnumud.Maximum = 100.00M;
            inhnumud.Minimum = 0.00M;

            //initial picbox
            wiringpicbox.Visible = false;

        }
        public static bool connected
        {
            get { return _connected; }
        }
        public static bool deviceisset
        {
            get { return _deviceisset; }
        }
        public bool mbissent
        {
            get { return _mbissent; }
        }
        public bool mbisread
        {
            get { return _mbisread; }
        }
        #region Click-->>Connect device
        private void connectButton_Click(object sender, EventArgs e)
        {
            short[] values = new short[16];
            if (connectButton.Text == "Connect")
            {
                if (deviceBox.Text == "")
                {
                    MyMsgBox.Showmsg("Please select device com port", "Can't connect device", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    if (mb.Open(deviceBox.Text, 9600, 8, Parity.None, StopBits.One))
                    {
                        Enable_Panel();
                        connectButton.Text = "Disconnect";
                        pleaseclickLabel.Text = "";
                        actionTextBox.Text += "TM-012P is connected\r\n";
                        pvLabel.ForeColor = Color.Turquoise;
                        outputLabel.ForeColor = Color.LightBlue;
                        cojLabel.ForeColor = Color.Turquoise;
                        deviceBox.Enabled = false;
                        _connected = true;
                        threadstart();
                        Verifyuser_config();
                    }
                    else
                    {
                        MyMsgBox.Showmsg("Failed to connect device due to this issue \r\n - Com port is not availble\r\n" +
                            " - Device is not powered up", "Communication failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                //device_serialPort.Close();
                if (mb.Close())
                {
                    Disble_Panel();
                    _connected = false;
                    _mbisread = false;
                    //threadstart();
                    prodIDLabel.Text = "";
                    connectButton.Text = "Connect";
                    pleaseclickLabel.Text = "Please click connect....";
                    actionTextBox.Text += "TM-012P is disconnected\r\n";
                    swLabel.Text = "";
                    pvLabel.ForeColor = Color.DarkGray;
                    outputLabel.ForeColor = Color.DarkGray;
                    cojLabel.ForeColor = Color.DarkGray;
                    deviceBox.Enabled = true;
                }
                else
                {
                    MyMsgBox.Showmsg("Can not disconnect, device might be unplugged", "Communication failed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
        private void Enable_Panel()
        {
            wiringpicbox.Visible = true;
            inputBox.Enabled = true;
            deg_crdb.Enabled = true;
            deg_frdb.Enabled = true;
            pvgainnumud.Enabled = true;
            pvadjnumud.Enabled = true;
            pvfilnumud.Enabled = true;
            slhnumud.Enabled = true;
            sllnumud.Enabled = true;
            invertrdb.Enabled = true;
            noninvertrdb.Enabled = true;
            inhnumud.Enabled = true;
            inlnumud.Enabled = true;
        }
        private void Disble_Panel()
        {
            wiringpicbox.Visible = false;
            inputBox.Enabled = false;
            deg_crdb.Enabled = false;
            deg_frdb.Enabled = false;
            pvgainnumud.Enabled = false;
            pvadjnumud.Enabled = false;
            pvfilnumud.Enabled = false;
            slhnumud.Enabled = false;
            sllnumud.Enabled = false;
            invertrdb.Enabled = false;
            noninvertrdb.Enabled = false;
            inhnumud.Enabled = false;
            inlnumud.Enabled = false;
        }
        #endregion

        #region Click-->>Link
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                VisitLink();
            }
            catch (Exception ex)
            {
                MyMsgBox.Showmsg("Unable to open Primus website", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void VisitLink()
        {
            // Change the color of the link text by setting LinkVisited   
            // to true.  
            linkLabel1.LinkVisited = true;
            //Call the Process.Start method to open the default browser   
            //with a URL:  
            System.Diagnostics.Process.Start("https://www.primusthai.com/");
        }
        #endregion

        #region Click-->>Load config file
        private void loadButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog configfile = new OpenFileDialog();
            configfile.Filter = "binary files (*.bin)|*.bin|All files (*.*)|*.*";
            configfile.InitialDirectory = @"C:\";
            configfile.Title = "Please select a config file.";
            //-----load setting value when select file-----//
            if (configfile.ShowDialog() == DialogResult.OK)
            {
                
                if (Path.GetExtension(configfile.FileName) == ".bin")
                {
                    actionTextBox.Text += "loading file" + configfile.FileName + "\r\n";
                    Disable_event();
                    _mbissent = true;
                    configloadprgbar.Increment(20);
                    Thread.Sleep(200);
                    configloadprgbar.Increment(20);
                    FileStream configreadfs = new FileStream(configfile.FileName, FileMode.Open, FileAccess.Read);
                    BinaryReader br = new BinaryReader(configreadfs);
                    configloadprgbar.Increment(20);
                    //-----read number from binary file-----//  
                    //read binary to check device model
                    if(device_model != br.ReadInt32())
                    {
                        MyMsgBox.Showmsg("You are loading config file that contains wrong device model, please select new file.", "Failed loading file!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        actionTextBox.Text += "Failed loading\r\n";
                    }
                    else
                    {
                        inputBox.SelectedIndex = br.ReadInt32();
                        mb.SendFc6(1, 3, (short)inputBox.SelectedIndex);
                        configloadprgbar.Increment(20);
                        switch (br.ReadInt32())
                        {
                            case 0:
                                if (device_model == 0)
                                {
                                    deg_crdb.Checked = true;
                                    inunitLabel.Text = "°C";
                                    mb.SendFc6(1, 8, 0);
                                }
                                break;
                            case 1:
                                if (device_model == 0)
                                {
                                    deg_frdb.Checked = true;
                                    inunitLabel.Text = "°F";
                                    mb.SendFc6(1, 8, 1);
                                }
                                break;
                        }
                        pvgainselect = (short)(br.ReadInt32());
                        pvgainnumud.Value = pvgainselect / 100.00M;
                        mb.SendFc6(1, 5, pvgainselect);

                        pvadjselect = (short)(br.ReadInt32());
                        pvadjnumud.Value = pvadjselect / _decimalp;
                        mb.SendFc6(1, 6, pvadjselect);
                        pvfilnumud.Value = br.ReadInt32();
                        mb.SendFc6(1, 7, (short)pvfilnumud.Value);

                        sllselect = (int)br.ReadInt32();
                        slhselect = (int)br.ReadInt32();

                        sllnumud.Value = sllselect / _decimalp;
                        mb.SendFc6(1, 10, sllselect);

                        slhnumud.Value = slhselect / _decimalp;
                        mb.SendFc6(1, 9, slhselect);

                        inlselect = (int)br.ReadInt32();
                        inlnumud.Value = inlselect / 100.0M;
                        mb.SendFc6(1, 13, inlselect);

                        inhselect = (int)br.ReadInt32();
                        inhnumud.Value = inhselect / 100.0M;
                        mb.SendFc6(1, 12, inhselect);
                        switch (br.ReadInt32())
                        {
                            case 0:
                                noninvertrdb.Checked = true;
                                polarityLabel.Text = "Noninverting";
                                mb.SendFc6(1, 11, 0);
                                break;
                            case 1:
                                invertrdb.Checked = true;
                                polarityLabel.Text = "Inverting";
                                mb.SendFc6(1, 11, 1);
                                break;
                        }
                    }
                    Thread.Sleep(200);
                    mb.SendFc4(1, 0, 16, ref values);
                    SetText(values);
                    Thread.Sleep(200);
                    Enable_event();
                    _mbissent = false;               
                    configloadprgbar.Increment(10);
                    Saveuser_config();
                    configloadprgbar.Increment(10);
                    MyMsgBox.Showmsg("Loading config file is succeeded!", "Done!", MessageBoxButtons.OK, MessageBoxIcon.None);
                    actionTextBox.Text +=  "Done loading\r\n";
                    configloadprgbar.Value = 0;
                    //threadstart();
                }
                else
                {
                    MyMsgBox.Showmsg("Wrong file type was selected, Please selecte again", "Wrong file type!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    actionTextBox.Text += "Failed loading\r\n";
                }

            }
        }
        #endregion

        #region Click--> Save config file
        private void saveButton_Click(object sender, EventArgs e)
        {
            SaveFileDialog scf = new SaveFileDialog();
            scf.Filter = "binary files (*.bin)|*.bin|All files (*.*)|*.*";
            scf.InitialDirectory = @"C:\";
            scf.Title = "Please select location to save a config file.";
            scf.FileName = "*.bin";
            if (scf.ShowDialog() == DialogResult.OK)
            {
                if (Path.GetExtension(scf.FileName) != ".bin")
                {
                    MyMsgBox.Showmsg("Warning you're saving with wrong file type, this file can not be used for configuration. " +
                        "Please save it again with .bin type", "Wrong file type!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                FileStream configfs = new FileStream(scf.FileName, FileMode.Create);
                BinaryWriter bw = new BinaryWriter(configfs);
                actionTextBox.Text += "Saving file :" + scf.FileName + "\r\n";
                //-----write number to binary file-----//
                bw.Write(Convert.ToInt32(device_model));
                bw.Write(Convert.ToInt32(inputBox.SelectedIndex));
                if (deg_crdb.Checked == true)
                {
                    bw.Write(Convert.ToInt32(0));
                }
                else
                {
                    if(device_model == 0)
                    {
                        bw.Write(Convert.ToInt32(1));
                    }
                    else
                    {
                        bw.Write(Convert.ToInt32(0));
                    }
                }
                bw.Write(Convert.ToInt32(pvgainnumud.Value * 100.00M));
                bw.Write(Convert.ToInt32(pvadjnumud.Value * _decimalp));
                bw.Write(Convert.ToInt32(pvfilnumud.Value));
                bw.Write(Convert.ToInt32(sllnumud.Value * _decimalp));
                bw.Write(Convert.ToInt32(slhnumud.Value * _decimalp));
                bw.Write(Convert.ToInt32(inlnumud.Value * 100.0M));
                bw.Write(Convert.ToInt32(inhnumud.Value * 100.0M));
                if (invertrdb.Checked == true)
                {
                    bw.Write(Convert.ToInt32(0));
                }
                else
                {
                    bw.Write(Convert.ToInt32(1));
                }

                bw.Close();
                configfs.Close();
                actionTextBox.Text += "Done saving\r\n";
                MyMsgBox.Showmsg("Saving config file is succeeded!", "Done!", MessageBoxButtons.OK, MessageBoxIcon.None);
                //MyMsgBox.Showmsg("Saving config file is succeed!", "Done");

            }
        }
        #endregion

        #region Thread --> read device all register
        private void threadstart()
        {
            Thread.Sleep(200);
            Thread read_device_Thread = new Thread(new ThreadStart(Read_device_Thread));
            if (connected)
            {
                try
                {
                    Disable_event();
                    mb.SendFc4(1, 265, 1, ref values);
                    switch (values[0])
                    {
                        default:
                        case 0:
                            prodIDLabel.Text = "TM-012P-TC";
                            device_model = 0;
                            break;
                        case 1:
                            prodIDLabel.Text = "TM-012P-R";
                            device_model = 1;
                            break;
                    }
                    mb.SendFc4(1, 0, 16, ref values);
                    SetText(values);
                    Enable_event();
                    //Thread.Sleep(200);
                    read_device_Thread.Start();
                }
                catch
                {
                    MyMsgBox.Showmsg("Can not read device's registers, please check wiring and reconnect device ", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                read_device_Thread.Abort();
            }
        }
        public void Read_device_Thread()
        {
            while (connected)
            {
                if(!mbissent)
                {
                    mb.SendFc4(1, 0, 16, ref values);
                    Showdata(values);
                }
            }
        }
        delegate void Callback(short[] values);
        private void Showdata(short[] values)
        {

            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.InvokeRequired)
            {
                Callback d = new Callback(Showdata);
                this.Invoke(d, new object[] { values });
            }
            else
            {
                //convert short pv to byte then reverse
                pv1 = (byte)((values[0] & 0xFF00) >> 8);
                pv2 = (byte)(values[0] & 0xFF);
                pv3 = (byte)((values[1] & 0xFF00) >> 8);
                pv4 = (byte)(values[1] & 0xFF);
                pvByte = BitConverter.GetBytes((pv1 << 24) + (pv2 << 16) + (pv3 << 8) + pv4);
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(pvByte);
                pv = BitConverter.ToInt32(pvByte, 0);

                //put recieve data into components
                if (pv < sll || pv > slh)
                {
                    this.pvLabel.Text = "----";
                }
                else
                {
                    this.pvLabel.Text = (pv).ToString("0");
                    this.pvLabel.Text = ((pv) / _decimalp).ToString("0.0");
                }
                this.outputLabel.Text = (((float)values[2]) / 100.00).ToString("0.00");
                if(device_model == 0)
                {
                    this.cojLabel.Text = (((float)values[4]) / 10.0).ToString("0.0");
                }
                else
                {
                    this.cojLabel.Text = "---";
                }
            }
        }
        private void SetText(short[] values)
        {
            try
            {
                Select_input_unit();
                inputBox.SelectedIndex = values[3];

                switch (device_model)
                {
                    default:
                    case 0:
                        switch (values[8])
                        {
                            default:
                            case 0:
                                
                                this.deg_crdb.Checked = true;
                                this.inunitLabel.Text = "°C";
                                break;
                            case 1:
                                this.pvadjnumud.Value = (decimal)values[6];
                                this.deg_frdb.Checked = true;
                                this.inunitLabel.Text = "°F";
                                break;
                        }
                        _decimalp = 10.0M;
                        pvadjnumud.DecimalPlaces = 1;
                        sllnumud.DecimalPlaces = 1;
                        slhnumud.DecimalPlaces = 1;
                        pvadjnumud.Increment = 0.1M;
                        sllnumud.Increment = 0.1M;
                        slhnumud.Increment = 0.1M;
                        inputBox.Enabled = true;
                        panel6.Enabled = true;
                        break;
                    case 1:
                        _decimalp = 1.0M;
                        inputBox.Enabled = false;
                        pvadjnumud.DecimalPlaces = 0;
                        sllnumud.DecimalPlaces = 0;
                        slhnumud.DecimalPlaces = 0;
                        pvadjnumud.Increment = 1M;
                        sllnumud.Increment = 1M;
                        slhnumud.Increment = 1M;
                        this.inunitLabel.Text = "\u2126";
                        panel6.Enabled = false;
                        break;
                }
                this.pvgainnumud.Value = (decimal)values[5] / 100.00M;
                this.pvadjnumud.Value = (decimal)values[6] / _decimalp;
                this.pvfilnumud.Value = values[7];
                slh = values[9];
                sll = values[10];

                this.slhnumud.Value = (decimal)slh / _decimalp;
                this.slhvalueLabel.Text = (slh / _decimalp).ToString("0.0");
                this.sllnumud.Value = (decimal)sll / _decimalp;
                this.sllvalueLabel.Text = (sll / _decimalp).ToString("0.0");

                inv = values[11];
                switch (inv)
                {
                    case 0:
                        this.noninvertrdb.Checked = true;
                        this.polarityLabel.Text = "Non-inverting";
                        break;
                    case 1:
                        this.invertrdb.Checked = true;
                        this.polarityLabel.Text = "Inverting";
                        break;
                }
                inh = values[12];
                inl = values[13];
                this.inhnumud.Value = (decimal)inh / 100.0M;
                this.inhvalueLabel.Text = (inh / 100.00).ToString("0.00");
                this.inlnumud.Value = (decimal)inl / 100.0M;
                this.inlvalueLabel.Text = (inl / 100.00).ToString("0.00");
                this.swLabel.Text = (values[14] / 100.00M).ToString("0.00");
            }
            catch
            {
                MyMsgBox.Showmsg("Can not read device's register, Please check wiring and reconnect device", "Communication problem", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region user.config checking
        private void Saveuser_config()
        {
            Settings.Default.input = (short)inputBox.SelectedIndex;
            if(deg_crdb.Checked && !deg_frdb.Checked)
            {
                Settings.Default.c_f = 0;
            }
            else
            {
                Settings.Default.c_f = 1;
            }
            Settings.Default.pvgain = pvgainnumud.Value;
            Settings.Default.pvadj = pvadjnumud.Value;
            Settings.Default.pvfil = pvfilnumud.Value;
            Settings.Default.slh = slhnumud.Value;
            Settings.Default.sll = sllnumud.Value;
            if (noninvertrdb.Checked && !invertrdb.Checked)
            {
                Settings.Default.inv = 0;
            }
            else
            {
                Settings.Default.inv = 1;
            }
            Settings.Default.inh = inhnumud.Value;
            Settings.Default.inl = inlnumud.Value;
            Settings.Default.swver = "1.00";
            Settings.Default.Save();
        }
        private void Verifyuser_config()
        {
            if(device_model == Settings.Default.model)
            {
                int degc_f;
                if (deg_crdb.Checked && !deg_frdb.Checked)
                {
                    degc_f = 0;
                }
                else
                {
                    degc_f = 1;
                }
                int _invrbt;
                if (noninvertrdb.Checked && !invertrdb.Checked)
                {
                    _invrbt = 0;
                }
                else
                {
                    _invrbt = 1;
                }
                if (Settings.Default.input != inputBox.SelectedIndex
                    || Settings.Default.c_f != degc_f
                    || Settings.Default.pvgain != pvgainnumud.Value
                    || Settings.Default.pvadj != pvadjnumud.Value
                    || Settings.Default.pvfil != pvfilnumud.Value
                    || Settings.Default.slh != slhnumud.Value
                    || Settings.Default.sll != sllnumud.Value
                    || Settings.Default.inv != _invrbt
                    || Settings.Default.inh != inhnumud.Value
                    || Settings.Default.inl != inlnumud.Value
                    || Settings.Default.swver != swLabel.Text)
                {
                    if (MyMsgBox.Showmsg("Click 'Yes' to config device with last setting", "This device is not set to last config", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                    {
                        _mbissent = true;
                        Disable_event();
                        configloadprgbar.Increment(25);
                        Thread.Sleep(500);
                        configloadprgbar.Increment(25);
                        if (Settings.Default.input != inputBox.SelectedIndex)
                        {
                            mb.SendFc6(1, 3, Settings.Default.input);
                        }
                        if (Settings.Default.pvgain != pvgainnumud.Value)
                        {
                            mb.SendFc6(1, 5, (short)Settings.Default.pvgain);
                        }
                        if (Settings.Default.pvadj != pvadjnumud.Value)
                        {
                            mb.SendFc6(1, 6, (short)Settings.Default.pvadj);
                        }
                        if (Settings.Default.pvfil != pvfilnumud.Value)
                        {
                            mb.SendFc6(1, 7, (short)Settings.Default.pvfil);
                        }
                        if (Settings.Default.c_f != degc_f)
                        {
                            mb.SendFc6(1, 8, (short)Settings.Default.c_f);
                        }
                        if (Settings.Default.sll != sllnumud.Value)
                        {
                           mb.SendFc6(1, 10, (int)(Settings.Default.sll * _decimalp));
                        }
                        if (Settings.Default.slh != slhnumud.Value)
                        {
                            mb.SendFc6(1, 9, (int)(Settings.Default.slh * _decimalp));
                        }
                        if (Settings.Default.inv != _invrbt)
                        {
                            mb.SendFc6(1, 11, (short)Settings.Default.inv);
                        }
                        if (Settings.Default.inl != inlnumud.Value)
                        {
                            mb.SendFc6(1, 13, (int)(Settings.Default.inl * 100.0M));
                        }
                        if (Settings.Default.inh != inhnumud.Value)
                        {
                            mb.SendFc6(1, 12, (int)(Settings.Default.inh * 100.0M));
                        }
                        configloadprgbar.Increment(25);
                        mb.SendFc4(1, 0, 16, ref values);
                        SetText(values);
                        Thread.Sleep(200);
                        Enable_event();
                        _mbissent = false;
                        //threadstart();
                        configloadprgbar.Increment(25);
                        actionTextBox.AppendText("Done Applying last config \r\n");
                        configloadprgbar.Value = 0;
                        //sampleThread1.Start();
                    }
                    Saveuser_config();
                }
            }
        }
        #endregion

        #region setting box change value -->> send modbus function6
        private void inputBox_SelectionChangeCommitted(object sender, EventArgs e)
        {
            Select_input_unit();
            inputselect = (short)inputBox.SelectedIndex;
            input_change = true;
        }
        private void Select_input_unit()
        {
            if(device_model == 1)
            {
                UpdatesettingLimit(0.0M, 5000.0M); // actual is 5000 ohms
            }
            else
            {
                ///re-init sll, slh, 
                if (deg_crdb.Checked && !deg_frdb.Checked)
                {
                    switch (inputBox.SelectedIndex)
                    {
                        case 0: //tc-k
                            UpdatesettingLimit(-200.0M, 1372.0M);
                            break;
                        case 1: //tc-j
                            UpdatesettingLimit(-200.0M, 1200.0M);
                            break;
                        case 2: //tc-n
                            UpdatesettingLimit(-200.0M, 1300.0M);
                            break;
                        case 3: //tc-e
                            UpdatesettingLimit(-200.0M, 1000.0M);
                            break;
                        case 4: //tc-t
                            UpdatesettingLimit(-200.0M, 400.0M);
                            break;
                        case 5: //tc-r
                            UpdatesettingLimit(-50.0M, 1768.0M);
                            break;
                        case 6: //tc-s
                            UpdatesettingLimit(-50.0M, 1768.0M);
                            break;
                        case 7: // pt-100
                            UpdatesettingLimit(-200.0M, 850.0M);
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    switch (inputBox.SelectedIndex)
                    {
                        case 0: //tc-k
                            UpdatesettingLimit(-328.0M, 2501.6M);
                            break;
                        case 1: //tc-j
                            UpdatesettingLimit(-328.0M, 2192.0M);
                            break;
                        case 2: //tc-n
                            UpdatesettingLimit(-328.0M, 2372.0M);
                            break;
                        case 3: //tc-e
                            UpdatesettingLimit(-328.0M, 1832.0M);
                            break;
                        case 4: //tc-t
                            UpdatesettingLimit(-328.0M, 752.0M);
                            break;
                        case 5: //tc-r
                            UpdatesettingLimit(-58.0M, 3214.4M);
                            break;
                        case 6: //tc-s
                            UpdatesettingLimit(-58.0M, 3214.4M);
                            break;
                        case 7: // pt-100
                            UpdatesettingLimit(-328.0M, 1562.0M);
                            break;
                        default:
                            break;
                    }
                }
            }
 
        }
        private void UpdatesettingLimit(decimal min, decimal max)
        {
            sll_max = max;
            sll_min = min;
            //sllnumud.Value = min;
            slh_max = max;
            slh_min = min;
            //slhnumud.Value = max;
        }
        private void pvgainnumud_ValueChanged(object sender, EventArgs e)
        {
            pvgainselect = (short)(pvgainnumud.Value*100);
            pvgain_change = true;   
        }

        private void pvadjnumud_ValueChanged(object sender, EventArgs e)
        {
            pvadjselect = (short)(pvadjnumud.Value*_decimalp);
            pvadj_change = true;
        }

        private void pvfilnumud_ValueChanged(object sender, EventArgs e)
        {
            pvfilselect = (short)pvfilnumud.Value;
            pvfil_change = true;
        }

        private void degreeradioButtons_CheckedChanged(object sender, EventArgs e)
        {
            Select_input_unit();
            if (deg_crdb.Checked == true)
            {
                cfselect = 0;
            }
            else
            {
                cfselect = 1;
            }
            cf_change = true;
        }

        private void sllnumud_ValueChanged(object sender, EventArgs e)
        {
            sllselect = (int)(sllnumud.Value * _decimalp);
            sll_change = true;
        }

        private void slhnumud_ValueChanged(object sender, EventArgs e)
        {
            slhselect = (int)(slhnumud.Value * _decimalp);
            slh_change = true;
        }

        private void polarrdb_CheckedChanged(object sender, EventArgs e)
        {
            if(invertrdb.Checked == true)
            {
                invselect = 1;
            }
            else
            {
                invselect = 0;
            }
            inv_change = true;
        }

        private void inlnumud_ValueChanged(object sender, EventArgs e)
        {
            inlselect = (int)(inlnumud.Value*100);
            inl_change = true;
        }

        private void inhnumud_ValueChanged(object sender, EventArgs e)
        {
            inhselect = (int)(inhnumud.Value*100);
            inh_change = true;
        }
        #endregion
        private void setdefButton_Click(object sender, EventArgs e)
        {
            if(MyMsgBox.Showmsg("Your current configuration might be lost. Are you sure you want to restore default settings?", "Restore Device!", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                _mbissent = true;
                configloadprgbar.Increment(25);
                //default_set = true;  
                Thread.Sleep(500);
                Disable_event();

                configloadprgbar.Increment(25);
                mb.SendFc6(1, 15, 0);
                Enable_event();
                Thread.Sleep(200);

                configloadprgbar.Increment(25);
                mb.SendFc4(1, 0, 16, ref values);
                SetText(values);
                _mbissent = false;

                configloadprgbar.Increment(24);
                Thread.Sleep(100);
                Saveuser_config();
                if (deviceisset != false)
                {
                    actionTextBox.AppendText("Done Restoring device\r\n");
                    MyMsgBox.Showmsg("Restoring device is succeeded", "Done!", MessageBoxButtons.OK, MessageBoxIcon.None);
                }
                else
                {
                    actionTextBox.AppendText("Failed restoring device \r\n");
                    _deviceisset = true;
                }
                configloadprgbar.Increment(1);
                configloadprgbar.Value = 0;
                //threadstart();
            }
            else
            {
                //default_set = false;
            }
            
        }
        private void applyButton_Click(object sender, EventArgs e)
        {
            _mbissent = true;
            Disable_event();
            configloadprgbar.Increment(25);
            Thread.Sleep(500);
            configloadprgbar.Increment(25);
            if (connected)
            {
                if (input_change)
                {
                    if(inputBox.SelectedItem == inputBox.Items)
                    {
                        mb.SendFc6(1, 3, inputselect);
                        actionTextBox.Text += "changing input " + inputBox.Text + "\r\n";
                    }
                    else
                    {
                        MyMsgBox.Showmsg("Selected Input Type is incorrect, Please select agian", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        //MyMsgBox.Showmsg("Selected Input type is incorrect, Please Select again", "Wrong type of selected input");
                        _deviceisset = false;
                    }
                    input_change = false;
                }
                if (pvgain_change)
                {
                    if(pvgainselect/100.0M >= pvgain_min && pvgainselect/100.0M <= pvgain_max)
                    {
                        mb.SendFc6(1, 5, pvgainselect);
                        actionTextBox.Text += "changing pvgain " + pvgainselect + "\r\n";
                    }
                    else
                    {
                        MyMsgBox.Showmsg("Selected PV Gain is out of range, Please select again", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        //MyMsgBox.Showmsg("Selected PV Gain is out of range, Please select again", "Selected PV Gain is out of range", MessageBoxButtons.OK);
                        _deviceisset = false;
                    }
                    pvgain_change = false;
                }
                if (pvadj_change)
                {
                    if (pvadjselect/_decimalp >= pvadj_min && pvadjselect/_decimalp <= pvadj_max)
                    {
                        mb.SendFc6(1, 6, pvadjselect);
                        actionTextBox.Text += "adjusting pv " + pvadjselect + "\r\n";   
                    }
                    else
                    {
                        MyMsgBox.Showmsg("Selected PV Adjust is out of range, Please select again", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        _deviceisset = false;
                    }
                    pvadj_change = false;
                }
                if (pvfil_change)
                {
                    if (pvfilselect >= pvfil_min && pvfilselect <= pvfil_max)
                    {
                        mb.SendFc6(1, 7, pvfilselect);
                        actionTextBox.Text += "changing pv filter " + pvfilselect + "\r\n";                       
                    }
                    else
                    {
                        MyMsgBox.Showmsg("Selected PV Filter is out of range, Please select again", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        _deviceisset = false;
                    }
                    pvfil_change = false;
                }
                if(cf_change)
                {
                    if(device_model == 0)
                    {
                        mb.SendFc6(1, 8, cfselect);
                        if (cfselect == 1)
                        {
                            actionTextBox.Text += "changing degree C to F \r\n";
                        }
                        else
                        {
                            actionTextBox.Text += "changing degree F to C \r\n";
                        }
                    }               
                    cf_change = false;
                }
                if(sll_change || slh_change)
                {
                    if(sllnumud.Value > slhnumud.Value)
                    {
                        sll_change = false;
                        slh_change = false;
                        MyMsgBox.Showmsg("Setting limit low must be lower than Setting limit high, Please select again", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        //MyMsgBox.Showmsg("Setting limit low must not be higher than Setting limit high, Please select again", "sll is higher than slh", MessageBoxButtons.OK);
                        _deviceisset = false;
                    }
                }
                if (sll_change)
                {
                    if ((sllselect/_decimalp) >= sll_min && (sllselect/_decimalp) <= sll_max)
                    {
                        mb.SendFc6(1, 10, sllselect);
                        actionTextBox.Text += "changing setting limit low =" + sllselect + "\r\n";                        
                    }
                    else
                    {
                        MyMsgBox.Showmsg("Selected Setting limit low is out of range, Please select again", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        _deviceisset = false;
                    }
                    sll_change = false;
                }
                if (slh_change)
                {
                    if ((slhselect/_decimalp) >= slh_min && (slhselect/_decimalp) <= slh_max)
                    {
                        mb.SendFc6(1, 9, slhselect);
                        actionTextBox.Text += "changing setting limit high =" + slhselect + "\r\n";                        
                    }
                    else
                    {
                        MyMsgBox.Showmsg("Selected Setting limit high is out of range, Please select again", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        _deviceisset = false;
                    }
                    slh_change = false;
                }
                if (inv_change)
                {
                    mb.SendFc6(1, 11, invselect);
                    inv_change = false;
                }
                if (inl_change || inh_change)
                {
                    if (inlnumud.Value > inhnumud.Value)
                    {
                        inl_change = false;
                        inh_change = false;
                        MyMsgBox.Showmsg("Output low limit must be lower than Output high limit, Please select again", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        _deviceisset = false;
                    }
                }
                if (inl_change)
                {
                    if ((inlselect/100.00M) >= inl_min && (inlselect/100.00M) <= inl_max)
                    {
                        mb.SendFc6(1, 13, inlselect);
                        actionTextBox.Text += "changing output limit low =" + inlselect + "\r\n";                        
                    }
                    else
                    {
                        MyMsgBox.Showmsg("Selected Output low limit is out of range, Please select again", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        _deviceisset = false;
                    }
                    inl_change = false;
                }
                if (inh_change)
                {
                    if ((inhselect/100.00M) >= inh_min && (inhselect/100.00M) <= inh_max)
                    {
                        mb.SendFc6(1, 12, inhselect);
                        actionTextBox.Text += "changing output limit high =" + inhselect + "\r\n";                        
                    }
                    else
                    {
                        MyMsgBox.Showmsg("Selected Output high limit is out of range, Please select again", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        _deviceisset = false;
                    }
                    inh_change = false;
                }
                
                configloadprgbar.Increment(25);
                mb.SendFc4(1, 0, 16, ref values);
                SetText(values);
                Thread.Sleep(200);
                Enable_event();
                _mbissent = false;
                //threadstart();
                configloadprgbar.Increment(24);
                Saveuser_config();
                configloadprgbar.Increment(1);
                if(deviceisset != false)
                {
                    actionTextBox.AppendText("Done Applying \r\n");
                    MyMsgBox.Showmsg("Applying device is succeeded", "Done!", MessageBoxButtons.OK, MessageBoxIcon.None);
                }
                else
                {
                    actionTextBox.AppendText("Failed Applying \r\n");
                    _deviceisset = true;
                }
                configloadprgbar.Value = 0;
                //sampleThread1.Start();
            }
        }

        void Disable_event()
        {
            this.inputBox.SelectionChangeCommitted -= new System.EventHandler(inputBox_SelectionChangeCommitted);
            this.deg_crdb.CheckedChanged -= new EventHandler(degreeradioButtons_CheckedChanged);
            this.deg_frdb.CheckedChanged -= new EventHandler(degreeradioButtons_CheckedChanged);
            this.pvgainnumud.ValueChanged -= new System.EventHandler(pvgainnumud_ValueChanged);
            this.pvadjnumud.ValueChanged -= new System.EventHandler(pvadjnumud_ValueChanged);
            this.pvfilnumud.ValueChanged -= new System.EventHandler(pvfilnumud_ValueChanged);
            this.invertrdb.CheckedChanged -= new System.EventHandler(polarrdb_CheckedChanged);
            this.noninvertrdb.CheckedChanged -= new System.EventHandler(polarrdb_CheckedChanged);
            this.sllnumud.ValueChanged -= new System.EventHandler(sllnumud_ValueChanged);
            this.slhnumud.ValueChanged -= new System.EventHandler(slhnumud_ValueChanged);
            this.inlnumud.ValueChanged -= new System.EventHandler(inlnumud_ValueChanged);
            this.inhnumud.ValueChanged -= new System.EventHandler(inhnumud_ValueChanged);
        }
        void Enable_event()
        {
            this.inputBox.SelectionChangeCommitted += new System.EventHandler(inputBox_SelectionChangeCommitted);
            this.deg_crdb.CheckedChanged += new EventHandler(degreeradioButtons_CheckedChanged);
            this.deg_frdb.CheckedChanged += new EventHandler(degreeradioButtons_CheckedChanged);
            this.pvgainnumud.ValueChanged += new System.EventHandler(pvgainnumud_ValueChanged);
            this.pvadjnumud.ValueChanged += new System.EventHandler(pvadjnumud_ValueChanged);
            this.pvfilnumud.ValueChanged += new System.EventHandler(pvfilnumud_ValueChanged);
            this.invertrdb.CheckedChanged += new System.EventHandler(polarrdb_CheckedChanged);
            this.noninvertrdb.CheckedChanged += new System.EventHandler(polarrdb_CheckedChanged);
            this.sllnumud.ValueChanged += new System.EventHandler(sllnumud_ValueChanged);
            this.slhnumud.ValueChanged += new System.EventHandler(slhnumud_ValueChanged);
            this.inlnumud.ValueChanged += new System.EventHandler(inlnumud_ValueChanged);
            this.inhnumud.ValueChanged += new System.EventHandler(inhnumud_ValueChanged);
        }

        #region Click --> exit, minimize
        private void exitbtn_Click(object sender, EventArgs e)
        {
            if (MyMsgBox.Showmsg("Are you sure you want to exit this software?", "Exit!", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.OK)
            {
                _mbissent = true;
                Thread.Sleep(200);
                mb.Close();               
                Disable_event();
                Thread.Sleep(200);
                Application.Exit();
            }
        }

        private void minimizebtn_Click(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
            {
                WindowState = FormWindowState.Minimized;
            }
        }
        #endregion
        private void deviceBox_Click(object sender, EventArgs e)
        {
            Get_SerialPort();
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void specllb_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            //Convert The resource Data into Byte[] 

            byte[] PDF = Properties.Resources.TM_012P_SPEC;
            MemoryStream ms = new MemoryStream(PDF);
            //Create PDF File From Binary of resources folders help.pdf
            FileStream f = new FileStream("TM-012P_SERIES_SPEC.pdf", FileMode.OpenOrCreate);
            //Write Bytes into Our Created help.pdf
            ms.WriteTo(f);
            f.Close();
            ms.Close();
            // Finally Show the Created PDF from resources 
            Process.Start("TM-012P_SERIES_SPEC.pdf");
        }


        /* close form 
private void Form1_FormClosing(object sender, FormClosingEventArgs e)
{
   if (e.CloseReason == CloseReason.UserClosing)
   {
       DialogResult result = MyMsgBox.Showmsg("Are you sure to exit this program?", "Exit TM-012 software setup", MessageBoxButtons.OKCancel);
       if (result == DialogResult.Yes)
       {
           Application.Exit();
       }
       else
       {
           e.Cancel = true;
       }
  }
   else
   {
       e.Cancel = true;
   }
}
*/
    }
}
