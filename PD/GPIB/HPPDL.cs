using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PD.GPIB
{
    public class HPPDL:HPBase
    {
        public override void init()
        {
            SendCommand("*CLS;*RST");
        }

        public void scanRate(int irate)
        {
            SendCommand("SCAN:RATE " + Convert.ToString(irate));
        }

        public void startPolarizationScan()
        {
            SendCommand("INIT:IMM");
        }

        public void stopPolarizationScan()
        {
            SendCommand("ABOR");
        }
		
		// Copied from Lxx - added by Warren 20160905
		public HPPDL GetHPPDL(HPPDL hppdl, int iboard, int iaddr, int iscanrate)
		{
		  hppdl.BoardNumber = iboard;
		  hppdl.Addr = iaddr;
		  hppdl.Open();
		  hppdl.init();
		  hppdl.scanRate(iscanrate);
		  return hppdl;
		}
    }
}
