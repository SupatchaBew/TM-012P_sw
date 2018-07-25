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

namespace TM_012P_Setup_Program
{
    public partial class Form1 : Form
    {
        private static modbus mb = new modbus();
        public static Thread sampleThread1;
        public static int timecount = 0;
        Int16 tdomain;
        Byte pv1;
        Byte pv2;
        Byte pv3;
        Byte pv4;
        Byte[] pvByte;
        private static short[] values = new short[16];
        private static bool _connected = false;
        private static bool _mbissent = false;
        private static bool _mbisread = false;

        short[] inputselect = new short[1];
        short[] pvgainselect = new short[1];
        short[] pvadjselect = new short[1];
        short[] pvfilselect = new short[1];
        short[] cfselect = new short[1];
        short[] sllselect = new short[1];
        short[] slhselect = new short[1];
        short[] invselect = new short[1];
        short[] inlselect = new short[1];
        short[] inhselect = new short[1];
        short[] defaultset = new short[1];

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
        bool default_set = false;

        Int32 pv;
        Int16 input;
        Int16 sll;
        Int16 slh;
        Int16 inv;
        Int16 inl;
        Int16 inh;

        public Form1()
        {
            InitializeComponent();
            Get_SerialPort();
            Init_settingBox();
            Initchart();
            deg_crdb.CheckedChanged += new EventHandler(degreeradioButtons_CheckedChanged);
            deg_frdb.CheckedChanged += new EventHandler(degreeradioButtons_CheckedChanged);
            pvgainnumud.ValueChanged += new System.EventHandler(pvgainnumud_ValueChanged);
            pvadjnumud.ValueChanged += new System.EventHandler(pvadjnumud_ValueChanged);
            pvfilnumud.ValueChanged += new System.EventHandler(pvfilnumud_ValueChanged);
            invertrdb.CheckedChanged += new System.EventHandler(polarrdb_CheckedChanged);
            noninvertrdb.CheckedChanged += new System.EventHandler(polarrdb_CheckedChanged);
            sllnumud.ValueChanged += new System.EventHandler(sllnumud_ValueChanged);
            slhnumud.ValueChanged += new System.EventHandler(slhnumud_ValueChanged);
            inlnumud.ValueChanged += new System.EventHandler(inlnumud_ValueChanged);
            inhnumud.ValueChanged += new System.EventHandler(inhnumud_ValueChanged);
        }

        private void Form1_Load(object sender, EventArgs e)
        {


        }
        void Get_SerialPort()
        {
            String[] devicePort = SerialPort.GetPortNames();
            deviceBox.Items.AddRange(devicePort);
        }
        void Init_settingBox()
        {
            //initial input type 
            System.Object[] input_type = new System.Object[8];
            input_type[0] = "TC-K";
            input_type[1] = "TC-J";
            input_type[2] = "TC-R";
            input_type[3] = "TC-T";
            input_type[4] = "TC-N";
            input_type[5] = "TC-S";
            input_type[6] = "TC-E";
            input_type[7] = "PT100";
            inputBox.Items.AddRange(input_type);
            //initial input setting limit
            sllnumud.Maximum = 9999.9M;
            sllnumud.Minimum = -999.9M;
            slhnumud.Maximum = 9999.9M;
            slhnumud.Minimum = -999.9M;
            //initial output limit
            inlnumud.Maximum = 20.00M;
            inlnumud.Minimum = 4.00M;
            inhnumud.Maximum = 20.00M;
            inhnumud.Minimum = 4.00M;

            //initial timer

        }
        public static bool connected
        {
            get { return _connected; }
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
                    MessageBox.Show("Please select device com port");
                }
                else
                {
                    if (mb.Open(deviceBox.Text, 9600, 8, Parity.None, StopBits.One))
                    {
                        connectButton.Text = "Disconnect";
                        pvLabel.ForeColor = Color.Turquoise;
                        outputLabel.ForeColor = Color.LightBlue;
                        cojLabel.ForeColor = Color.Turquoise;
                        _connected = true;
                        timerchart.Enabled = true;
                        threadstart();


                    }
                    else
                    {
                        MessageBox.Show("Failed to connect device or device com port not availble", "Communication failed", MessageBoxButtons.OK);
                    }
                }
            }
            else
            {
                //device_serialPort.Close();
                if (mb.Close())
                {
                    _connected = false;
                    _mbisread = false;
                    timerchart.Enabled = false;
                    //threadstart();
                    connectButton.Text = "Connect";
                    swLabel.Text = "";
                    pvLabel.ForeColor = Color.DarkGray;
                    outputLabel.ForeColor = Color.DarkGray;
                    cojLabel.ForeColor = Color.DarkGray;
                }
                else
                {
                    MessageBox.Show("Can not close communication, device might be unplugged", "Communication failed", MessageBoxButtons.OK);
                }
            }
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
                MessageBox.Show("Unable to open link that was clicked.");
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
                    Disable_event();
                    _mbissent = true;
                    configloadprgbar.Increment(20);
                    Thread.Sleep(200);
                    configloadprgbar.Increment(20);
                    FileStream configreadfs = new FileStream(configfile.FileName, FileMode.Open, FileAccess.Read);
                    BinaryReader br = new BinaryReader(configreadfs);
                    configloadprgbar.Increment(20);
                    //-----read number from binary file-----//             
                    inputBox.SelectedIndex = br.ReadInt32();
                    mb.SendFc6(1, 3, (short)inputBox.SelectedIndex);
                    configloadprgbar.Increment(20);
                    switch (br.ReadInt32())
                    {
                        case 0:
                            deg_crdb.Checked = true;
                            inunitLabel.Text = "°C";
                            mb.SendFc6(1, 8, 0);
                            break;
                        case 1:
                            deg_frdb.Checked = true;
                            inunitLabel.Text = "°F";
                            mb.SendFc6(1, 8, 1);
                            break;
                    }
                    pvgainnumud.Value = br.ReadInt32();
                    mb.SendFc6(1, 5, (short)pvgainnumud.Value);
                    pvfilnumud.Value = br.ReadInt32();
                    mb.SendFc6(1, 6, (short)pvfilnumud.Value);
                    pvadjnumud.Value = br.ReadInt32();
                    mb.SendFc6(1, 7, (short)pvadjnumud.Value);

                    sllselect[0] = (short)br.ReadInt32();
                    sllnumud.Value = sllselect[0] / 10.0M;
                    mb.SendFc6(1, 10, sllselect[0]);

                    slhselect[0] = (short)br.ReadInt32();
                    slhnumud.Value = slhselect[0] / 10.0M;
                    mb.SendFc6(1, 9, slhselect[0]);

                    inlselect[0] = (short)br.ReadInt32();
                    inlnumud.Value = inlselect[0] / 100.0M;
                    mb.SendFc6(1, 13, inlselect[0]);

                    inhselect[0] = (short)br.ReadInt32();
                    inhnumud.Value = inhselect[0] / 100.0M;
                    mb.SendFc6(1, 12, inhselect[0]);
                    switch (br.ReadInt32())
                    {
                        case 0:
                            invertrdb.Checked = true;
                            polarityLabel.Text = "Noninverting";
                            mb.SendFc6(1, 11, 0);
                            break;
                        case 1:
                            noninvertrdb.Checked = true;
                            polarityLabel.Text = "Inverting";
                            mb.SendFc6(1, 11, 1);
                            break;
                    }
                    Thread.Sleep(200);
                    Enable_event();
                    _mbissent = false;               
                    configloadprgbar.Increment(20);
                    statusLabel.ForeColor = Color.Turquoise;
                    statusLabel.Text = "Done Loading";
                    configloadprgbar.Value = 0;
                    //threadstart();
                }
                else
                {
                    MessageBox.Show("Wrong file type was selected, Please selecte again", "Wrong file type", MessageBoxButtons.OK);
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
                    MessageBox.Show("Warning you're saving with wrong file type, this file can not be used for configuration. " +
                        "Please save it again with .bin type", "Wrong file type", MessageBoxButtons.OK);
                }
                FileStream configfs = new FileStream(scf.FileName, FileMode.Create);
                BinaryWriter bw = new BinaryWriter(configfs);
                //-----write number to binary file-----//
                bw.Write(Convert.ToInt32(inputBox.SelectedIndex));
                if (deg_crdb.Checked == true)
                {
                    bw.Write(Convert.ToInt32(0));
                }
                else
                {
                    bw.Write(Convert.ToInt32(1));
                }
                bw.Write(Convert.ToInt32(pvgainnumud.Value));
                bw.Write(Convert.ToInt32(pvfilnumud.Value));
                bw.Write(Convert.ToInt32(pvadjnumud.Value));
                bw.Write(Convert.ToInt32(sllnumud.Value * 10.0M));
                bw.Write(Convert.ToInt32(slhnumud.Value * 10.0M));
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
                statusLabel.ForeColor = Color.Turquoise;
                statusLabel.Text = "Done Saving file!";

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
                inputBox.SelectionChangeCommitted -= new System.EventHandler(inputBox_SelectionChangeCommitted);
                deg_crdb.CheckedChanged -= new EventHandler(degreeradioButtons_CheckedChanged);
                deg_frdb.CheckedChanged -= new EventHandler(degreeradioButtons_CheckedChanged);
                pvgainnumud.ValueChanged -= new System.EventHandler(pvgainnumud_ValueChanged);
                pvadjnumud.ValueChanged -= new System.EventHandler(pvadjnumud_ValueChanged);
                pvfilnumud.ValueChanged -= new System.EventHandler(pvfilnumud_ValueChanged);
                invertrdb.CheckedChanged -= new System.EventHandler(polarrdb_CheckedChanged);
                noninvertrdb.CheckedChanged -= new System.EventHandler(polarrdb_CheckedChanged);
                sllnumud.ValueChanged -= new System.EventHandler(sllnumud_ValueChanged);
                slhnumud.ValueChanged -= new System.EventHandler(slhnumud_ValueChanged);
                inlnumud.ValueChanged -= new System.EventHandler(inlnumud_ValueChanged);
                inhnumud.ValueChanged -= new System.EventHandler(inhnumud_ValueChanged);
                mb.SendFc4(1, 0, 16, ref values);
                SetText(values);
                inputBox.SelectionChangeCommitted += new System.EventHandler(inputBox_SelectionChangeCommitted);
                deg_crdb.CheckedChanged += new EventHandler(degreeradioButtons_CheckedChanged);
                deg_frdb.CheckedChanged += new EventHandler(degreeradioButtons_CheckedChanged);
                pvgainnumud.ValueChanged += new System.EventHandler(pvgainnumud_ValueChanged);
                pvadjnumud.ValueChanged += new System.EventHandler(pvadjnumud_ValueChanged);
                pvfilnumud.ValueChanged += new System.EventHandler(pvfilnumud_ValueChanged);
                invertrdb.CheckedChanged += new System.EventHandler(polarrdb_CheckedChanged);
                noninvertrdb.CheckedChanged += new System.EventHandler(polarrdb_CheckedChanged);
                sllnumud.ValueChanged += new System.EventHandler(sllnumud_ValueChanged);
                slhnumud.ValueChanged += new System.EventHandler(slhnumud_ValueChanged);
                inlnumud.ValueChanged += new System.EventHandler(inlnumud_ValueChanged);
                inhnumud.ValueChanged += new System.EventHandler(inhnumud_ValueChanged);
                //Thread.Sleep(200);
                read_device_Thread.Start();
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
                    this.pvLabel.Text = ((pv) / 10.0).ToString("0.0");
                }

                this.outputLabel.Text = (((float)values[2]) / 100.00).ToString("0.00");
                this.Plotchart((float)values[2] / 100.00);
  
                this.cojLabel.Text = (((float)values[4]) / 10.0).ToString("0.0");


            }
        }
        private void SetText(short[] values)
        {
            inputBox.SelectedIndex = values[3];
            this.pvgainnumud.Value = values[5];
            this.pvadjnumud.Value = values[6];
            this.pvfilnumud.Value = values[7];
            switch (values[8])
            {
                case 0:
                    this.deg_crdb.Checked = true;
                    this.inunitLabel.Text = "°C";
                    break;
                case 1:
                    this.deg_frdb.Checked = true;
                    this.inunitLabel.Text = "°F";
                    break;
            }
            slh = values[9];
            sll = values[10];
            this.slhnumud.Value = (decimal)slh / 10.0M;
            this.slhvalueLabel.Text = (slh / 10.0).ToString("0.0");
            this.sllnumud.Value = (decimal)sll / 10.0M;
            this.sllvalueLabel.Text = (sll / 10.0).ToString("0.0");

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
            this.swLabel.Text = $"Firmware version : " + (values[14]/100.00M).ToString("0.00") + " (latest ver.)";

        }
        #endregion

        private void Plotchart(double yplot)
        {
            if (timecount >= 1)
            {
                timecount = 0;
                tdomain++;
                if (tdomain > 10)
                {
                    outputChart.ChartAreas[0].AxisX.Maximum = tdomain;
                    outputChart.ChartAreas[0].AxisX.Minimum = tdomain - 10;
                    outputChart.ChartAreas[0].AxisX.ScaleView.Zoom(tdomain - 10, tdomain);
                    outputChart.Series[0].Points.AddXY(tdomain, yplot);

                }
                else
                {
                    outputChart.Series[0].Points.AddXY(tdomain, yplot);
                }
            }
        }
        private void Initchart()
        {
            //initial chart
            outputChart.ChartAreas[0].AxisY.ScaleView.Zoom(4.00, 20.00);
            outputChart.ChartAreas[0].AxisX.ScaleView.Zoom(0, 10);
            outputChart.ChartAreas[0].CursorX.IsUserEnabled = true;
            outputChart.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
            outputChart.ChartAreas[0].AxisX.ScaleView.Zoomable = false;
            outputChart.ChartAreas[0].AxisX.Maximum = 10;
            outputChart.ChartAreas[0].AxisX.Minimum = 0;
            outputChart.ChartAreas[0].AxisY.Maximum = 20.00;
            outputChart.ChartAreas[0].AxisY.Minimum = 4.00;
            outputChart.ChartAreas[0].AxisY.Interval = 4.00;
            outputChart.ChartAreas[0].AxisX.ScrollBar.Enabled = false;
            outputChart.ChartAreas[0].AxisY.ScrollBar.Enabled = false;

            //StripLine sl0 = new StripLine();
            //sl0.BackColor = Color.FromArgb(64, Color.LightSeaGreen);
            //sl0.StripWidth = 4;
            //sl0.IntervalOffset = 8;

            //StripLine sl1 = new StripLine();
            //sl1.BackColor = Color.FromArgb(64, Color.LightGoldenrodYellow);
            //sl1.StripWidth = 4;
            //sl1.IntervalOffset = 12;

            //StripLine sl2 = new StripLine();
            //sl2.BackColor = Color.FromArgb(64, Color.LightSalmon);
            //sl2.StripWidth = 4;
            //sl2.IntervalOffset = 16;

            //outputChart.ChartAreas[0].AxisY.StripLines.Add(sl0);
            //outputChart.ChartAreas[0].AxisY.StripLines.Add(sl1);
            //outputChart.ChartAreas[0].AxisY.StripLines.Add(sl2);

            outputChart.Series[0].Points.AddXY(0, 4);
        }
        private void runButton_Click(object sender, EventArgs e)
        {
            sllnumud.ValueChanged -= new System.EventHandler(sllnumud_ValueChanged);
            slhnumud.ValueChanged -= new System.EventHandler(slhnumud_ValueChanged);
            mb.SendFc4(1, 0, 16, ref values);
            Showdata(values);
            sllnumud.ValueChanged += new System.EventHandler(sllnumud_ValueChanged);
            slhnumud.ValueChanged += new System.EventHandler(slhnumud_ValueChanged);
        }

        #region setting box change value -->> send modbus function6
        private void inputBox_SelectionChangeCommitted(object sender, EventArgs e)
        {
            inputselect[0] = (short)inputBox.SelectedIndex;
            input_change = true;
        }

        private void pvgainnumud_ValueChanged(object sender, EventArgs e)
        {
            pvgainselect[0] = (short)pvgainnumud.Value;
            pvgain_change = true;      
        }

        private void pvadjnumud_ValueChanged(object sender, EventArgs e)
        {
            pvadjselect[0] = (short)pvadjnumud.Value;
            pvadj_change = true;
        }

        private void pvfilnumud_ValueChanged(object sender, EventArgs e)
        {
            pvfilselect[0] = (short)pvfilnumud.Value;
            pvfil_change = true;
        }

        private void degreeradioButtons_CheckedChanged(object sender, EventArgs e)
        {
            if(deg_crdb.Checked == true)
            {
                cfselect[0] = 0;
            }
            else
            {
                cfselect[0] = 1;
            }
            cf_change = true;
        }

        private void sllnumud_ValueChanged(object sender, EventArgs e)
        {
            sllselect[0] = (short)(sllnumud.Value*10);
            sll_change = true;
        }

        private void slhnumud_ValueChanged(object sender, EventArgs e)
        {
            slhselect[0] = (short)(slhnumud.Value * 10);
            slh_change = true;
        }

        private void polarrdb_CheckedChanged(object sender, EventArgs e)
        {
            if(invertrdb.Checked == true)
            {
                invselect[0] = 1;
            }
            else
            {
                invselect[0] = 0;
            }
            inv_change = true;
        }

        private void inlnumud_ValueChanged(object sender, EventArgs e)
        {
            inlselect[0] = (short)(inlnumud.Value*100);
            inl_change = true;
        }

        private void inhnumud_ValueChanged(object sender, EventArgs e)
        {
            inhselect[0] = (short)(inhnumud.Value*100);
            inh_change = true;
        }
        #endregion
        private void setdefButton_Click(object sender, EventArgs e)
        {
            if(MessageBox.Show("Do you want to restore default settings?", "Restore default settings", MessageBoxButtons.YesNo) == DialogResult.Yes)
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

                configloadprgbar.Increment(25);
                Thread.Sleep(100);
                statusLabel.ForeColor = Color.Turquoise;
                statusLabel.Text = "Done Restoring device";
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
                    mb.SendFc6(1, 3, inputselect[0]);
                    input_change = false;
                }
                if (pvgain_change)
                {
                    mb.SendFc6(1, 5, pvgainselect[0]);
                    pvgain_change = false;
                }
                if (pvadj_change)
                {
                    mb.SendFc6(1, 6, pvadjselect[0]);
                    pvadj_change = false;
                }
                if (pvfil_change)
                {
                    mb.SendFc6(1, 7, pvfilselect[0]);
                    pvfil_change = false;
                }
                if(cf_change)
                {
                    mb.SendFc6(1, 8, cfselect[0]);
                    cf_change = false;
                }
                if (sll_change)
                {
                    mb.SendFc6(1, 10, sllselect[0]);
                    sll_change = false;
                }
                if (slh_change)
                {
                    mb.SendFc6(1, 9, slhselect[0]);
                    slh_change = false;
                }
                if (inv_change)
                {
                    mb.SendFc6(1, 11, invselect[0]);
                    inv_change = false;
                }
                if (inl_change)
                {
                    mb.SendFc6(1, 13, inlselect[0]);
                    inl_change = false;
                }
                if (inh_change)
                {
                    mb.SendFc6(1, 12, inhselect[0]);
                    inh_change = false;
                }
                configloadprgbar.Increment(25);
                mb.SendFc4(1, 0, 16, ref values);
                SetText(values);
                Thread.Sleep(200);
                Enable_event();
                _mbissent = false;
                //threadstart();
                configloadprgbar.Increment(25);
                statusLabel.ForeColor = Color.Turquoise;
                statusLabel.Text = "Done Applying";
                configloadprgbar.Value = 0;
                //sampleThread1.Start();
            }
        }

        private void timerchart_Tick(object sender, EventArgs e)
        {
            timecount++;
        }

        private void clrchartbtn_Click(object sender, EventArgs e)
        {
            outputChart.Series[0].Points.Clear();
            outputChart.ChartAreas[0].AxisX.Maximum = 10;
            outputChart.ChartAreas[0].AxisX.Minimum = 0;
            tdomain = 0;
            timecount = 0;
            outputChart.Series[0].Points.AddXY(0, 4);
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

        private void exitbtn_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure to exit this program?", "Exit TM-012 software setup", MessageBoxButtons.OKCancel) == DialogResult.OK)
            {
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
        /* close form 
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                DialogResult result = MessageBox.Show("Are you sure to exit this program?", "Exit TM-012 software setup", MessageBoxButtons.OKCancel);
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
