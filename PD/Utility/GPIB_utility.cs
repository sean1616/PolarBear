using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Windows.Forms;
using System.Globalization;
using System.Threading;

using NationalInstruments.NI4882;

namespace GPIB_utility
{
 

   class GPIB_CMD 
   {
        private Device device;

         public GPIB_CMD(int board, byte address)
		{
            try {
                Address myAddress = new NationalInstruments.NI4882.Address(address);
                device = new Device(board, myAddress);
            }
            catch { }
		}
		public void Write( byte[] chars )
		{
            try
            {
                if (device != null)
                    device.Write(chars);
            }
            catch { }
		}
  		public string Read()
		{
            //device.ReadByteArray();
            try
            {
                string readstring = "";
                if (device != null)
                    readstring = device.ReadString();

                return (readstring);
            }
            catch { return null; }
        }
        public byte[] ReadByteArray()
        {
            byte[] readbytearray = new byte[] { };
            if (device != null)
                readbytearray = device.ReadByteArray();

            return ( readbytearray );
        }
    }

}