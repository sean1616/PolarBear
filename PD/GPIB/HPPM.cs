using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Text.RegularExpressions;

namespace PD.GPIB
{
    public class HPPM : HPBase
    {
        private int _slot;

        public int Slot
        {
            set
            {
                _slot = value;
            }
        }

        public override void init()
        {
            SendCommand("*CLS;*CLS;*SRE 0;*ESE 0");
        }
		
        public void SetWL(double WL)
        {
            SendCommand("SENS" + Convert.ToString(_slot) + ":POW:WAVE " + Convert.ToString(WL) + "NM");
        }

        public void setUnit(int units)
        {
            if (units == 1)
                SendCommand("SENS" + Convert.ToString(_slot) + ":POW:UNIT DBM");
            else
                SendCommand("SENS" + Convert.ToString(_slot) + ":POW:UNIT W");
        }
		
		// Enable Max Min function of PM - Added by Warren 20160904 
		public void enableMaxMinFunc(string mode, int delayTime)
        {
            SendCommand("SENS" + Convert.ToString(_slot) + ":FUNC:PAR:MINM " + mode + ",100");  // 100: the number of data points    cmd: WIND ??
			Thread.Sleep(delayTime);  // 1000
        }

        // Start Max Min function of PM - Added by Warren 20160904 
		public void startMaxMinFunc()
        {
            SendCommand("SENS" + Convert.ToString(_slot) + ":FUNC:STAT MINM,STAR");
			// Set measure time xx sec
			//Thread.Sleep(sec*1000);
        }
		
		// Stop Max Min function of PM - Added by Warren 20160904
		public void stopMaxMinFunc(int delayTime)
        {
            SendCommand("SENS" + Convert.ToString(_slot) + ":FUNC:STAT MINM,STOP");
			Thread.Sleep(delayTime);  // 100
        }

        // Get the result of Abs(Max - Min) of PM power - Added by Warren 20160908 xxx
        public double getMaxMinDelta(int pmIdx)
        {
            ////double[] values = new double[2];
            double min = 0, max = 0;
            double deltaValue = 0;
            string input;
            byte[] results;
            ////string pattern = @"^#[0-9]{3}[\s]*Min:([\s]*[0-9\.E-]*),[\s]*Max:[\s]*([\s]*[0-9\.E-]*),[\s]*Act:[\s]*[\s]*[0-9\.E-]*";
            ////Regex rgx = new Regex(pattern, RegexOptions.IgnoreCase); 

            SendCommand("SENS" + Convert.ToString(_slot) + ":FUNC:RES?");  // Get MIN and MAX results of PM 
            results = ReadByteArray();  // MUST!! Read the result using binary

            input = System.Text.Encoding.ASCII.GetString(results);   // byte[] -> ASCII and 'input' will look like: 
            // #255 Min: 7.24079E-04, Max: 7.24252E-04, Act: 7.24155E-04  

            string[] split = input.Split(',');
            min = Convert.ToDouble(split[0].Split(':')[1]);
            max = Convert.ToDouble(split[1].Split(':')[1]);

            deltaValue = Math.Round(Math.Abs(max - min), 3);
            return deltaValue;

            ////MatchCollection matches = rgx.Matches(input);
            ////Console.WriteLine("{0} ({1} matches):", input, matches.Count);
            ////if (matches.Count > 0)
            ////{
            ////    Console.WriteLine("{0} ({1} matches):", input, matches.Count);

            ////    int idx = 0;
            ////    foreach (Match match in matches)
            ////    {
            ////        values[idx] = Convert.ToDouble(match.Value.Trim());
            ////        Console.WriteLine("values " + (idx+1).ToString() + ": " + match.Value);
            ////        idx++;
            ////    }

            ////    deltaValue = Math.Abs(values[1] - values[0]) ;
            ////    Console.WriteLine("PM" + pmIdx.ToString() + ", delta result:" + deltaValue.ToString());
            ////}
        }
		
		public double ReadPower()
        {
            //string strRead;
            double Pow;

            SendCommand("READ" + Convert.ToString(_slot) + ":POW?");
            //strRead = Read();
            Pow = Convert.ToDouble(Read());
            if (Pow > 1000000)
                return -99; //1000
            else
                return Pow;
        }
		
		// Copied from Lxx - added by Warren 20160904
		public double ReadPower(int icount)
		{
		  int num1 = 0;
		  this.SendCommand("READ" + Convert.ToString(this._slot) + ":POW?");
		  double num2 = Convert.ToDouble(this.Read());
		  while (num2 > 1000000.0)
		  {
			this.SendCommand("READ" + Convert.ToString(this._slot) + ":POW?");
			num2 = Convert.ToDouble(this.Read());
			Thread.Sleep(200);
			++num1;
			if (num1 > icount)
			  return -99.0;
		  }
		  return num2;
		}
		
		// Copied from Lxx - added by Warren 20160904
		public double ReadPower(int icount, int idelay)
		{
		  int num1 = 0;
		  this.SendCommand("READ" + Convert.ToString(this._slot) + ":POW?");
		  double num2 = Convert.ToDouble(this.Read());
		  while (num2 > 1000000.0)
		  {
			this.SendCommand("READ" + Convert.ToString(this._slot) + ":POW?");
			num2 = Convert.ToDouble(this.Read());
			Thread.Sleep(idelay);
			++num1;
			if (num1 > icount)
			  return -99.0;
		  }
		  return num2;
		}

        public void aveTime(int avgT)
        {
            SendCommand("SENS" + Convert.ToString(_slot) + ":POW:ATIME " + Convert.ToString(avgT) + "MS");
        }
		
		// Added by Warren 20160904
		public void setAverageTime(int time)
        {
            SendCommand("SENS" + Convert.ToString(_slot) + ":POW:ATIME " + Convert.ToString(time) + "MS");
        }

        public void AutoRange(bool States)
        {
            if (States)
                SendCommand("SENS" + Convert.ToString(_slot) + ":POW:RANG:AUTO ON");
            else
                SendCommand("SENS" + Convert.ToString(_slot) + ":POW:RANG:AUTO OFF");
        }

        public double ReadWL()
        {
            string strRead = string.Empty;
            SendCommand("SENS" + Convert.ToString(_slot) + ":POW:WAVE?");
            
            strRead = Read();
            return Convert.ToDouble(strRead) * Math.Pow(10, 9);
        }

        /// <summary>
        /// Get/Read Min WL of PM - added by Warren 20140219
        /// </summary>
        public double ReadWL_Min()
        {
            string strRead = string.Empty;
            SendCommand("SENS" + Convert.ToString(_slot) + ":POW:WAVE? MIN");
            
            strRead = Read();
            return Convert.ToDouble(strRead) * Math.Pow(10, 9);
        }

        /// <summary>
        /// Get/Read Max WL of PM - added by Warren 20140219 
        /// </summary>
        public double ReadWL_Max()
        {
            string strRead = string.Empty;
            SendCommand("SENS" + Convert.ToString(_slot) + ":POW:WAVE? MAX");

            strRead = Read();
            return Convert.ToDouble(strRead) * Math.Pow(10, 9);
        }

        public double ReadReference(int iref)
        {
            string strRead=string.Empty;
            switch (iref)
            {
                case 0:
                    SendCommand("SENS" + Convert.ToString(_slot) + ":POW:REF? TOA");
                    break;
                case 1:
                    SendCommand("SENS" + Convert.ToString(_slot) + ":POW:REF? TOB");
                    break;
                case 2:
                    SendCommand("SENS" + Convert.ToString(_slot) + ":POW:REF? TOREF");
                    break;
            }

            strRead = Read();
            return Convert.ToDouble(strRead);
        }

        public void SetReference(int iref, double refvalue)
        {
            string strCmd="SENS" + Convert.ToString(_slot) + ":POW:REF";
            switch (iref)
            {
                case 0:
                    SendCommand(strCmd + " TOA," + refvalue.ToString() +"DB");
                    break;
                case 1:
                    SendCommand(strCmd + " TOB," + refvalue.ToString() + "DB");
                    break;
                case 2:
                    SendCommand(strCmd + " TOREF," + refvalue.ToString() + "DBM");
                    break;
            }
        }

        //public void SetReferenceStatus(int iref)
        //{
        //    if (iref== 0)
        //        SendCommand("SENS" + Convert.ToString(_slot) + ":POW:REF:STATE OFF");
        //    else if (iref == 1)
        //        SendCommand("SENS" + Convert.ToString(_slot) + ":POW:REF:STATE ON");
        //}
		
		// Copied from Lxx - added by Warren 20160904
		public void SetReferenceStatus(int iref)
		{
		  if (iref == 0)
		  {
			this.SendCommand("SENS" + Convert.ToString(this._slot) + ":POW:REF:STATE OFF");
		  }
		  else
		  {
			if (iref != 1)
			  return;
			this.SendCommand("SENS" + Convert.ToString(this._slot) + ":POW:REF:STATE ON");
		  }
		}

        public int ReadReferenceStatus()
        {
            string strRead = string.Empty;
            SendCommand("SENS" + Convert.ToString(_slot) + ":POW:REF:STATE?");
            //SendCommand("WAVE?");
            strRead = Read();
            return Convert.ToInt32(strRead);
        }
		
		// Copied from Lxx - added by Warren 20160904
		public HPPM GetPM(HPPM hppm, int iboard, int iaddr, int islot)
		{
		  hppm.BoardNumber = iboard;
		  hppm.Addr = iaddr;
		  hppm.Slot = islot;
		  hppm.Open();
		  hppm.init();
		  hppm.setUnit(1);
		  hppm.aveTime(20);
		  return hppm;
		}
    }
}
