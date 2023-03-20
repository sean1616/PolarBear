using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NationalInstruments.NI4882;
using System.Windows.Forms;
using Ivi.Visa.Interop;

namespace PD.GPIB
{
    public abstract class HPBase
    {
        // For HP tls protocol
        private Device device;
        private int _addr;
        private int _boardnumber;

        // For keysight tls protocol
        private ResourceManager rm;
        private FormattedIO488 instrument;
        private string _addr_tcpip;
        
        // 0: GPIB, 1:TCPIP for usb (ex: "TCPIP0::100.65.8.113::5025::SOCKET")
        // Default 0, you can use HPOSA, HPPDL ... etc.
        public int protocol = 0; 

        public abstract void init();

        public int Addr
        {
            set
            {
                _addr = value;
            }
        }

        public string Addr_TCPIP
        {
            set 
            {
                _addr_tcpip = value;
            }
        }

        public int BoardNumber
        {
            set
            {
                _boardnumber = value;
            }
        }

        public void setAddr(int addr)
        {
            _addr = addr;
        }

        public void setAddr(string addr)
        {
            _addr_tcpip = addr;
        }

        public int getAddr()
        {
            return _addr;
        }

        public void setBoardNo(int boardNo)
        {
            _boardnumber = boardNo;
        }

        public int getBoardNo()
        {
            return _boardnumber;
        }

        public bool Open()
        {
            try
            {
                if (protocol == 0)
                {
                    //device = new Device(_boardnumber, (byte)_addr, (byte)0);
                    byte adr = Convert.ToByte(_addr);
                    device = new Device(_boardnumber, adr);
                }
                else if (protocol == 1)
                {
                    rm = new ResourceManager();
                    instrument = new FormattedIO488();

                    // TCPIP0::100.65.8.113::5025::SOCKET
                    var instrAddress = "TCPIP" + _boardnumber + "::" + _addr_tcpip + "::SOCKET";
                    instrument.IO = (IMessage)rm.Open(instrAddress, AccessMode.NO_LOCK, 0, "");
                    instrument.IO.TerminationCharacterEnabled = true;
                    instrument.IO.Timeout = 3000;
                }
                return true;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        public bool Open(int BoardNumber, int Addr)
        {
            try
            {
                byte addr = Convert.ToByte(Addr);
                device = new Device(BoardNumber, addr);
                this.BoardNumber = BoardNumber;
                this.Addr = Addr;
                return true;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }


        public void Close()
        {
            try
            {
                if (protocol == 0)
                {
                    device.Dispose();
                }
                else if (protocol == 1)
                {
                    instrument.IO.Close();
                    instrument.IO = null;
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(instrument);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(rm);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        // Added by Warren 20150306
        public void GoToLocal()
        {
            try
            {
                device.GoToLocal();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void SendCommand(string strCMD)
        {
            try
            {
                if (protocol == 0)
                {
                    device.Write(strCMD);
                }
                else if (protocol == 1)
                {
                    instrument.WriteString(strCMD, true);
                }
            }
            catch (Exception)
            {
                //MessageBox.Show(ex.Message);
            }
        }

        public string Read()
        {
            try
            {
                if (protocol == 0)
                {
                    return device.ReadString();
                }
                else
                {
                    return instrument.ReadString();
                }
            }
            catch (Exception)
            {
                //MessageBox.Show(ex.Message);
                //return string.Empty;
                return "-100";
            }
            finally
            {

            }
        }

        public string Read(int ileng)
        {
            try
            {
                if (protocol == 0)
                {
                    if(ileng > 0)
                        return device.ReadString(ileng);
                    else
                        return device.ReadString();
                }
                else
                {
                    return instrument.ReadString();
                }
            }
            catch (Exception)
            {
                return "-100";
            }
            finally
            {

            }
        }

        /// <summary>
        /// Read Byte Array - added by Warren 20160907
        /// </summary>
        public byte[] ReadByteArray()
        {
            byte[] a = new byte[] { 0 };
            try
            {
                return device.ReadByteArray();
            }
            catch (Exception)
            {
                //MessageBox.Show(ex.Message);
                //return string.Empty;
                return a;
            }
            finally
            {

            }
        }
    }
}
