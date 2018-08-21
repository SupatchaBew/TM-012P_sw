using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;
using System.Threading;
using System.Windows.Forms;

namespace Modbus_Poll_CS
{
    class modbus
    {
        private SerialPort sp = new SerialPort();
        public string modbusStatus;
        int pulltime = 500;

        // ------------------------------------------------------------------------
        /// <summary>Constant for exception illegal function.</summary>
        public const byte excIllegalFunction = 1;
        /// <summary>Constant for exception illegal data address.</summary>
        public const byte excIllegalDataAdr = 2;
        /// <summary>Constant for exception illegal data value.</summary>
        public const byte excIllegalDataVal = 3;
        /// <summary>Constant for exception slave device failure.</summary>
        public const byte excSlaveDeviceFailure = 4;
        /// <summary>Constant for exception acknowledge.</summary>
        public const byte excAck = 5;
        /// <summary>Constant for exception slave is busy/booting up.</summary>
        public const byte excSlaveIsBusy = 6;
        /// <summary>Constant for exception gate path unavailable.</summary>
        public const byte excGatePathUnavailable = 10;
        /// <summary>Constant for exception not connected.</summary>
        public const byte excExceptionNotConnected = 253;
        /// <summary>Constant for exception connection lost.</summary>
        public const byte excExceptionConnectionLost = 254;
        /// <summary>Constant for exception response timeout.</summary>
        public const byte excExceptionTimeout = 255;
        /// <summary>Constant for exception wrong offset.</summary>
        private const byte excExceptionOffset = 128;
        /// <summary>Constant for exception send failt.</summary>
        private const byte excSendFailt = 100;

        // ------------------------------------------------------------------------
        #region Constructor / Deconstructor
        public modbus()
        {
        }
        ~modbus()
        {
        }
        #endregion

        #region Open / Close Procedures
        public bool Open(string portName, int baudRate, int databits, Parity parity, StopBits stopBits)
        {
            //Ensure port isn't already opened:
            if (!sp.IsOpen)
            {
                //Assign desired settings to the serial port:
                sp.PortName = portName;
                sp.BaudRate = baudRate;
                sp.DataBits = databits;
                sp.Parity = parity;
                sp.StopBits = stopBits;
                //These timeouts are default and cannot be editted through the class at this point:
                sp.ReadTimeout = 5000;
                sp.WriteTimeout = 5000;

                try
                {
                    sp.Open();
                }
                catch (Exception err)
                {
                    modbusStatus = "Error opening " + portName + ": " + err.Message;
                    return false;
                }
                modbusStatus = portName + " opened successfully";
                return true;
            }
            else
            {
                modbusStatus = portName + " already opened";
                return false;
            }
        }
        public bool Close()
        {
            //Ensure port is opened before attempting to close:
            if (sp.IsOpen)
            {
                try
                {
                    sp.Close();
                }
                catch (Exception err)
                {
                    modbusStatus = "Error closing " + sp.PortName + ": " + err.Message;
                    return false;
                }
                modbusStatus = sp.PortName + " closed successfully";
                return true;
            }
            else
            {
                modbusStatus = sp.PortName + " is not open";
                return false;
            }
        }
        #endregion

        #region CRC Computation
        private void GetCRC(byte[] message, ref byte[] CRC)
        {
            //Function expects a modbus message of any length as well as a 2 byte CRC array in which to 
            //return the CRC values:

            ushort CRCFull = 0xFFFF;
            byte CRCHigh = 0xFF, CRCLow = 0xFF;
            char CRCLSB;

            for (int i = 0; i < (message.Length) - 2; i++)
            {
                CRCFull = (ushort)(CRCFull ^ message[i]);

                for (int j = 0; j < 8; j++)
                {
                    CRCLSB = (char)(CRCFull & 0x0001);
                    CRCFull = (ushort)((CRCFull >> 1) & 0x7FFF);

                    if (CRCLSB == 1)
                        CRCFull = (ushort)(CRCFull ^ 0xA001);
                }
            }
            CRC[1] = CRCHigh = (byte)((CRCFull >> 8) & 0xFF);
            CRC[0] = CRCLow = (byte)(CRCFull & 0xFF);
        }
        #endregion

        #region Build Message
        private void BuildMessage(byte address, byte type, ushort start, int registers, ref byte[] message)
        {
            //Array to receive CRC bytes:
            byte[] CRC = new byte[2];

            message[0] = address;
            message[1] = type;
            message[2] = (byte)(start >> 8);
            message[3] = (byte)start;
            message[4] = (byte)(registers >> 8);
            message[5] = (byte)registers;

            GetCRC(message, ref CRC);
            message[message.Length - 2] = CRC[0];
            message[message.Length - 1] = CRC[1];
        }
        #endregion

        #region Check Response
        private bool CheckResponse(byte[] response)
        {
            //Perform a basic CRC check:
            byte[] CRC = new byte[2];
            GetCRC(response, ref CRC);
            if (CRC[0] == response[response.Length - 2] && CRC[1] == response[response.Length - 1])
                return true;
            else
                return false;
        }
        #endregion

        #region Get Response
        private void GetResponse(ref byte[] response)
        {
            Thread.Sleep(pulltime);
            //There is a bug in .Net 2.0 DataReceived Event that prevents people from using this
            //event as an interrupt to handle data (it doesn't fire all of the time).  Therefore
            //we have to use the ReadByte command for a fixed length as it's been shown to be reliable.
            for (int i = 0; i < response.Length; i++)
            {
                response[i] = (byte)(sp.ReadByte());
            }
        }
        #endregion

        #region Check Exception
        private void MBmaster_OnException(byte exception)
        {
            string exc = "Modbus says error: ";
            switch (exception)
            {
                case modbus.excIllegalFunction: exc += "Illegal function!"; break;
                case modbus.excIllegalDataAdr: exc += "Illegal data adress!"; break;
                case modbus.excIllegalDataVal: exc += "Illegal data value!"; break;
                case modbus.excSlaveDeviceFailure: exc += "Slave device failure!"; break;
                case modbus.excAck: exc += "Acknoledge!"; break;
                case modbus.excGatePathUnavailable: exc += "Gateway path unavailbale!"; break;
                case modbus.excExceptionTimeout: exc += "Slave timed out!"; break;
                case modbus.excExceptionConnectionLost: exc += "Connection is lost!"; break;
                case modbus.excExceptionNotConnected: exc += "Not connected!"; break;
            }

            MessageBox.Show(exc, "Modbus slave exception");
        }
        #endregion

        #region Function 16 - Write Multiple Registers
        public bool SendFc16(byte address, ushort start, short registers, short[] values)
        {
            //Ensure port is open:
            if (sp.IsOpen)
            {
                //Clear in/out buffers:
                sp.DiscardOutBuffer();
                sp.DiscardInBuffer();
                //Message is 1 addr + 1 fcn + 2 start + 2 reg + 1 count + 2 * reg vals + 2 CRC
                byte[] message = new byte[9 + 2 * registers];
                //Function 16 response is fixed at 8 bytes
                byte[] response = new byte[8];

                //Add bytecount to message:
                message[6] = (byte)(registers * 2);
                //Put write values into message prior to sending:
                for (int i = 0; i < registers; i++)
                {
                    message[7 + 2 * i] = (byte)(values[i] >> 8);
                    message[8 + 2 * i] = (byte)(values[i]);
                }
                //Build outgoing message:
                BuildMessage(address, (byte)16, start, registers, ref message);
                
                //Send Modbus message to Serial Port:
                try
                {
                    sp.Write(message, 0, message.Length);
                    GetResponse(ref response);
                }
                catch (Exception err)
                {
                    modbusStatus = "Error in write event: " + err.Message;
                    return false;
                }
                //Evaluate message:
                if (CheckResponse(response))
                {
                    modbusStatus = "Write successful";
                    return true;
                }
                else
                {
                    modbusStatus = "CRC error";
                    return false;
                }
            }
            else
            {
                modbusStatus = "Serial port not open";
                return false;
            }
        }
        #endregion
        #region Function 6 - Write single Registers
        public bool SendFc6(byte address, ushort start, int values)
        {
            //Ensure port is open:
            if (sp.IsOpen)
            {
                //Clear in/out buffers:
                sp.DiscardOutBuffer();
                sp.DiscardInBuffer();
                //Message is 1 addr + 1 fcn + 2 start + 2 reg + 1 count + 2 * reg vals + 2 CRC
                byte[] message = new byte[8];
                //Function 16 response is fixed at 8 bytes
                byte[] response = new byte[8];

                //Build outgoing message:
                BuildMessage(address, (byte)6, start, values, ref message);

                //Send Modbus message to Serial Port:
                try
                {
                    sp.Write(message, 0, message.Length);
                    GetResponse(ref response);
                }
                catch (Exception err)
                {
                    // Response data is slave exception
                    if ((response != null) && (response[1] > excExceptionOffset))
                    {
                        response[1] -= excExceptionOffset;
                        MBmaster_OnException(response[2]);
                        return false;
                    }
                    else
                    {
                        modbusStatus = "Error in write event: " + err.Message;
                        MessageBox.Show($"Error in read event: + {err.Message}");
                        return false;
                    }

                }
                //Evaluate message:
                if (CheckResponse(response))
                {
                    // Response data is regular data
                    if (response != null)
                    {
                        modbusStatus = "Write successful";
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    modbusStatus = "CRC error";
                    return false;
                }
            }
            else
            {
                modbusStatus = "Serial port not open";
                return false;
            }
        }
        #endregion
        #region Function 3 - Read Registers
        public bool SendFc3(byte address, ushort start, short registers, ref short[] values)
        {
            //Ensure port is open:
            if (sp.IsOpen)
            {
                //Clear in/out buffers:
                sp.DiscardOutBuffer();
                sp.DiscardInBuffer();
                //Function 3 request is always 8 bytes:
                byte[] message = new byte[8];
                //Function 3 response buffer:
                byte[] response = new byte[5 + 2 * registers];
                //Build outgoing modbus message:
                BuildMessage(address, (byte)3, start, registers, ref message);
                //Send modbus message to Serial Port:
                try
                {
                    sp.Write(message, 0, message.Length);
                    GetResponse(ref response);
                }
                catch (Exception err)
                {
                    modbusStatus = "Error in read event: " + err.Message;
                    return false;
                }
                //Evaluate message:
                if (CheckResponse(response))
                {
                    //Return requested register values:
                    for (int i = 0; i < (response.Length - 5) / 2; i++)
                    {
                        values[i] = response[2 * i + 3];
                        values[i] <<= 8;
                        values[i] += response[2 * i + 4];
                    }
                    modbusStatus = "Read successful";
                    return true;
                }
                else
                {
                    modbusStatus = "CRC error";
                    return false;
                }
            }
            else
            {
                modbusStatus = "Serial port not open";
                return false;
            }

        }
        #endregion
        #region Function 4 - Read Multiple Registers
        public bool SendFc4(byte address, ushort start, short registers, ref short[] values)
        {
            //Ensure port is open:
            if (sp.IsOpen)
            {
                //Clear in/out buffers:
                sp.DiscardOutBuffer();
                sp.DiscardInBuffer();
                //Function 4 request is 512 bytes:
                byte[] message = new byte[8];
                //Function 4 response buffer:
                byte[] response = new byte[5 + 2 * registers];
                //Build outgoing modbus message:
                BuildMessage(address, (byte)4, start, registers, ref message);
                //Send modbus message to Serial Port:
                try
                {
                    sp.Write(message, 0, message.Length);
                    GetResponse(ref response);
                }
                catch (Exception err)
                {
                    modbusStatus = "Error in read event: " + err.Message;

                    return false;
                }
                //Evaluate message:
                if (CheckResponse(response))
                {
                    //Return requested register values:
                    for (int i = 0; i < (response.Length - 5) / 2; i++)
                    {
                        values[i] = response[2 * i + 3];
                        values[i] <<= 8;
                        values[i] += response[2 * i + 4];
                    }
                    modbusStatus = "Read successful";
                    return true;
                }
                else
                {
                    modbusStatus = "CRC error";
                    return false;
                }
            }
            else
            {
                modbusStatus = "Serial port not open";
                return false;
            }

        }
        #endregion


    }
}
