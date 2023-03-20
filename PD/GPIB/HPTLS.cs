using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace PD.GPIB
{
    /// <summary>
    /// tunable laser regulated path
    /// </summary>
    //public enum TLS_PATH
    //{
    //    /// <summary>
    //    /// Use Default
    //    /// </summary>
    //    NONE,
    //    /// <summary>
    //    /// The High Power output is regulated.
    //    /// </summary>
    //    HIGHpower,
    //    /// <summary>
    //    /// The Low SSE output is regulated.
    //    /// </summary>
    //    LOWSse,
    //    /// <summary>
    //    /// Both outputs are active but only the High Power output is Regulated.
    //    /// </summary>
    //    HIGHpowerLow,
    //    /// <summary>
    //    /// Both outputs are active but only the Low SSE output is Regulated.
    //    /// </summary>
    //    LOWSseHigh
    //}

    public class HPTLS : HPBase
    {
        public override void init()
        {
            if (protocol == 0)
                SendCommand("*CLS;DISP:ENAB ON;AM:STATE OFF");
        }

        public void SetWL(double WL)
        {
            SendCommand("WAVE " + Convert.ToString(WL) + "NM");
        }

        public void setUnit(int units)
        {
            if (units == 1)
                SendCommand("SOUR:POW:UNIT DBM");
            else
                SendCommand("SOUR:POW:UNIT W");

            // [N777] :sour0:pow:unit w
        }

        /// <summary>
        /// Set tunable laser active, protocol 0: HP, protocol 1: N777
        /// </summary>
        /// <param name="state">active</param>
        public void SetActive(bool state)
        {
            if (protocol == 0)
            {
                if (state)
                    SendCommand("OUTPUT ON");
                else
                    SendCommand("OUTPUT OFF");
            }
            else if (protocol == 1)
            {
                if (state)
                    SendCommand("sour0:pow:stat 1");
                else
                    SendCommand("sour0:pow:stat 0");
            }
            // [N777] :sour0:pow:stat 1, :sour0:pow:stat 0
        }

        public void SetPower(double pow)
        {
            SendCommand("POWER:UNIT DBM;:POWER  " + Convert.ToString(pow) + " DBM");
            
            // [N777] :sour0:pow 5mW
        }
		
		// Set Att of TLS - added by Warren 20160904
		public void SetAtt(double att)
        {
            SendCommand("sour0:pow:att " + Convert.ToString(att));
			Thread.Sleep(100);
			
			//mGPIBSession.Write("sour0:pow:att " + aAttenuation.ToString)
			//Thread.Sleep(100)
        }

        public double ReadWL()
        {
            string strRead;

            SendCommand("WAVE?");
            strRead = Read();
            return Convert.ToDouble(strRead) * Math.Pow(10, 9);
        }

      

        /// <summary>
        /// Get/Read Min WL of TLS - added by Warren 20140219
        /// </summary>
        public double ReadWL_Min()
        {
            string strRead;

            SendCommand("WAVE? MIN");
            strRead = Read();
            return Convert.ToDouble(strRead) * Math.Pow(10, 9);
        }

        /// <summary>
        /// Get/Read Max WL of TLS - added by Warren 20140219
        /// </summary>
        public double ReadWL_Max()
        {
            string strRead;

            SendCommand("WAVE? MAX");
            strRead = Read();
            return Convert.ToDouble(strRead) * Math.Pow(10, 9);
        }
		
		// Copied from Lxx - added by Warren 20160904
		public HPTLS GetHPTLS(HPTLS hptls, int iboard, int iaddr, double power, int idbunit)
		{
		  hptls.BoardNumber = iboard;
		  hptls.Addr = iaddr;
		  hptls.Open();
		  hptls.init();
		  hptls.SetActive(true);
		  hptls.SetPower(power);
		  hptls.setUnit(idbunit);
		  return hptls;
		}
    }
}
