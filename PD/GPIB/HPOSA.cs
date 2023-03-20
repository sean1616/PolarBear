using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace PD.GPIB
{
    public class HPOSA:HPBase
    {
        public override void init()
        {
            SendCommand("*RST");
            SendCommand("DISP:WIND:TRAC:STAT TRB,ON");
            SendCommand("INIT:CONT OFF");
            //SendCommand("DISP:WIND:TRAC:ALL:SCAL AUTO");
            //SendCommand("CAL:ALIG:MARK1");
            SendCommand("*OPC?");
            while (Convert.ToInt32(Read()) != 1)
            {
                Thread.Sleep(200);
            }
        }

        public void StartTrace(double swl, double ewl)
        {
            SendCommand("SENS:WAV:START " + swl.ToString() + "NM;STOP " + ewl.ToString() + "NM;");
            // set the sensitivity
            //SendCommand("SENS:POW:RANG:LOW " + "-78" + "DBM;");
            //SendCommand("sens:swe:poin 1001");
            //set the display & active trace
            SendCommand("TRAC:FEED:CONT TRA,ALW");
            //set the SRQ

            //'rc1 = m_OSA.SendCommand("STAT:PRES;*SRE " & Str(OSA_RQS))
            //'tSTB = m_OSA.SerialPoll ' clear the status byte include bit 4
            //' start sweep
            SendCommand("INIT:IMM");
            //SendCommand("*OPC?");
            //while (Convert.ToInt32(Read()) != 1)
            //{
            //    Thread.Sleep(200);
            //}
            SendCommand("DISP:WIND:TRAC:STAT TRA,ON");
            SendCommand("TRAC:DATA:Y? TRA");
            //Thread.Sleep(2000);

        }

        /// <summary>
        /// Set wavelength range
        /// </summary>
        /// <param name="start_WL">start wavelength</param>
        /// <param name="end_WL">end wavelength</param>
        /// <param name="_sensitive">sensitivity</param>
        /// <param name="_resolution">step</param>
        /// <param name="IsSetResolution">Is set Res?</param>
        /// <returns>string power:WL1, IL1, WL2, IL2,...</returns>
        public string StartTrace(double start_WL, double end_WL, double _sensitive, double _resolution, bool IsSetResolution)
        {
            if (IsSetResolution)
            {
                SendCommand("SENS:BAND:RES " + _resolution.ToString() + "NM");
                WaitCMDCompleted();
            }

            int points = Convert.ToInt32((end_WL - start_WL) / _resolution + 1);

            SendCommand("INIT:CONT OFF");
            WaitCMDCompleted();
            // Setting trace range
            SendCommand("SENS:WAV:START " + start_WL.ToString("#0.00") + "NM;STOP " + end_WL.ToString("#0.00") + "NM;");
            WaitCMDCompleted();

            // Setting sensitivity
            SendCommand("sens:pow:rang:low " + _sensitive.ToString() + "dbm");
            WaitCMDCompleted();
            // Setting points
            SendCommand(string.Format("sens:swe:poin {0}", points));
            WaitCMDCompleted();

            // Setting the display & active trace
            SendCommand("TRAC:FEED:CONT TRA,ALW");
            WaitCMDCompleted();

            // Take sweep
            SendCommand("INIT:IMM");
            //if (!getVISA())
            //    WaitCMDCompleted();
            //else WaitCMDCompleted(90);

            WaitCMDCompleted(90);

            // View Trace A
            SendCommand("DISP:WIND:TRAC:STAT TRA,ON");
            WaitCMDCompleted();

            // Query data
            SendCommand("TRAC:DATA:Y? TRA");
            //WaitCMDCompleted();

            string rawdata = Read(17 * points); // set length of geting attenuation, 1 raw = 17 byte

            //int counter = 0;
            //while (rawdata == "-100" && counter < 10)
            //{

            //    SendCommand("TRAC:DATA:Y? TRA");
            //    rawdata = Read(17 * points);
            //    counter++;
            //}
            //if (rawdata == "-100") return "-999,Time Limit Exceeded to complete operation";
            //double osastep = _resolution;// ((ewl - swl) / 1000); // each wavelength gets count
            //string result = GetAllPower(rawdata, osastep, start_WL, end_WL);

            return rawdata;
        }

        /// <summary>
        /// Set wavelength range
        /// </summary>
        /// <param name="start_WL">start wavelength</param>
        /// <param name="end_WL">end wavelength</param>
        /// <param name="gap_WL">gap wavelength</param>
        /// <param name="_sensitive">sensitivity</param>
        /// <param name="_resolutionBW">RBW</param>
        /// <returns>string power:WL1, IL1, WL2, IL2,...</returns>
        public string StartTrace(double start_WL, double end_WL, double gap_WL, double _sensitive, double _resolutionBW)
        {
            //SendCommand("SENS:BAND:RES " + _resolution.ToString() + "NM");
            SendCommand("sens:bwid:res " + _resolutionBW.ToString("f2") + "nm");
            WaitCMDCompleted();

            int points = Convert.ToInt32((end_WL - start_WL) / gap_WL + 1);

            SendCommand("INIT:CONT OFF");
            WaitCMDCompleted();
            // Setting trace range
            SendCommand("SENS:WAV:START " + start_WL.ToString("#0.00") + "NM;STOP " + end_WL.ToString("#0.00") + "NM;");
            WaitCMDCompleted();

            // Setting sensitivity
            SendCommand("sens:pow:rang:low " + _sensitive.ToString() + "dbm");
            WaitCMDCompleted();
            // Setting points
            SendCommand(string.Format("sens:swe:poin {0}", points));
            WaitCMDCompleted();

            // Setting the display & active trace
            SendCommand("TRAC:FEED:CONT TRA,ALW");
            WaitCMDCompleted();

            // Take sweep
            SendCommand("INIT:IMM");
            WaitCMDCompleted(90);

            // View Trace A
            SendCommand("DISP:WIND:TRAC:STAT TRA,ON");
            WaitCMDCompleted();

            // Query data
            SendCommand("TRAC:DATA:Y? TRA");

            string rawdata = Read(17 * points); // set length of geting attenuation, 1 raw = 17 byte

            //int counter = 0;
            //while (rawdata == "-100" && counter < 10)
            //{

            //    SendCommand("TRAC:DATA:Y? TRA");
            //    rawdata = Read(17 * points);
            //    counter++;
            //}
            //if (rawdata == "-100") return "-999,Time Limit Exceeded to complete operation";
            //double osastep = _resolution;// ((ewl - swl) / 1000); // each wavelength gets count
            //string result = GetAllPower(rawdata, osastep, start_WL, end_WL);

            return rawdata;
        }

        private void WaitCMDCompleted(int _cnt = 30)
        {
            int times = 0;
            if (_cnt == 30)
            {
                do
                {
                    Thread.Sleep(100);
                    SendCommand("*OPC?");
                    times++;
                } while (Convert.ToInt32(Read()) != 1 && times < _cnt);

            }
            else
            {
                do
                {
                    Thread.Sleep(100);
                    times++;
                } while (times < _cnt);
            }
        }

        public void SetSpan(double ispan)
        {
            SendCommand("sens:wav:span " +ispan.ToString()+"nm");
        }

        public void SetResolution(double ires)
        {
            SendCommand("sens:bwid:res "+ires.ToString("f2")+"nm");
        }

        public void SetVideoBW(double ires)
        {
            SendCommand("sens:bwid:vid " + ires.ToString() + "khz");
        }

        public void SetRange(double irange)
        {      
            SendCommand("sens:pow:rang:low "+irange.ToString()+"dbm");
        }

        public void SetTracePoint(int ipoint)
        {
            SendCommand("sens:swe:poin "+ipoint.ToString());
        }

        /// <summary>
        /// Read All power in exact wavelength format
        /// </summary>
        /// <param name="rawdata">raw data</param>
        /// <param name="osastep">wavelength step</param>
        /// <param name="swl">start wavelength</param>
        /// <param name="ewl">end wavelength</param>
        /// <returns>string power:WL1, IL1, WL2, IL2,...</returns>
        public string GetAllPower(string rawdata, double osastep, double swl, double ewl)
        {
            //string rawdata = Read(data_count * 17); // set length of geting attenuation, 1 raw = 17 byte
            string[] eachdata = rawdata.Split(',');
            double[] osawl = new double[eachdata.Length]; // save rawdata

            string result = string.Empty;
            double wl_temp = swl;
            // set each wavelength, ex: 1290, 1290.3, 1290.6, 1290.9, ..., 1620, 1620.3, 1620.6 ...
            for (int idx = 0; idx < eachdata.Length; idx++)
            {
                wl_temp = Math.Round(wl_temp, 3);
                osawl[idx] = wl_temp;
                wl_temp += osastep;
            }
            // filter wavelength with step 1 nm, ex: 1290, 1291, 1292, ..., 1620.
            for (int wlidx = 0; wlidx < eachdata.Length; wlidx++)
            {
                if (swl == ewl + osastep)
                    break;
                else if (swl <= osawl[wlidx])
                {
                    double att = Math.Round(Convert.ToDouble(eachdata[wlidx]), 3);
                    result += string.Format("{0},{1},", swl.ToString(), att.ToString());
                    swl += osastep;
                    swl = Math.Round(swl, 3);
                }
            }
            return result;
        }

        public void GetTotalPower(ref double[] power, double startwl, double endwl)
        {
            GetTotalPower(ref power, startwl, endwl, 10, 5);

        }

        public void GetTotalPower(ref double[] power,double startwl, double endwl, double ispan, double ires)
        {
            SendCommand("*RST");
            SendCommand("disp:wind:trac:all:scal:auto");
            SetSpan(ispan);
            SetResolution(ires);
            SendCommand("init:imm");
            SendCommand("calc1:tpow:stat 1");
            SendCommand("calc1:tpow:data?");
            power[0] = Convert.ToDouble(Read());
            SendCommand("calc1:tpow:iran:low "+startwl.ToString() +"nm");
            SendCommand("calc1:tpow:iran:upp " + endwl.ToString() + "nm");
            SendCommand("calc1:tpow:data?");
            power[1] = Convert.ToDouble(Read());
        }

        public double GetSpanPower(double centerwl, double ispan, double ires, double irange)
        {
            SendCommand("sens:wav:cent " + centerwl.ToString() + "nm");
            SetSpan(ispan);
            SetResolution(ires);
            //SetVideoBW(ivbw);
            SendCommand("sens:pow:rang:low "+irange.ToString()+"dbm");
            SendCommand("init:imm");
            Thread.Sleep(1500);
            SendCommand("calc1:tpow:stat 1");
            SendCommand("calc1:tpow:data?");
            return Convert.ToDouble(Read());
        }

        public double GetSpanPower(double centerwl, double iRange, double ispan)
        {
            double halfRange = iRange / 2;
            double halfSpan = ispan / 2;
            SendCommand("sens:wav:star " + (centerwl-halfRange).ToString() + "nm");
            SendCommand("sens:wav:stop " + (centerwl + halfRange).ToString() + "nm");
            Thread.Sleep(1000);
             SendCommand("calc1:tpow:iran:low "+(centerwl-halfSpan).ToString() +"nm");
             SendCommand("calc1:tpow:iran:upp " + (centerwl + halfSpan).ToString() + "nm");
            SendCommand("calc1:tpow:stat 1");
            SendCommand("calc1:tpow:data?");
            return Convert.ToDouble(Read());
        }

        public double GetPower(double centerwl)
        {
            SendCommand("sens:wav:cent " + centerwl.ToString() + "nm");
         
            SendCommand("init:imm");
            Thread.Sleep(2000);
            SendCommand("calc1:tpow:stat 1");
            SendCommand("calc1:tpow:data?");
            return Convert.ToDouble(Read());
        }

        public double GetPeakWL()
        {
            SendCommand("calc1:mark1:max");
            Thread.Sleep(200);
            SendCommand("calc1:mark1:x?");
            
            return Convert.ToDouble(Read());
        }

        // Copied from Lxx - added by Warren 20160904		
		public double GetPeakWL2()
		{
		  this.SendCommand("calc1:mark1:x?");
		  return Convert.ToDouble(this.Read());
		}

		// Copied from Lxx - added by Warren 20160904
		public double GetPeakPower()
		{
		  this.SendCommand("calc1:mark1:y?");
		  return Convert.ToDouble(this.Read());
		}

        #region BTI Member
        public void SetWLStart(double wl)
        {
            SendCommand("sens:wav:star "+wl.ToString()+"nm");
        }

        public void SetWLStop(double wl)
        {
            SendCommand("sens:wav:stop " + wl.ToString() + "nm");
        }

        public void MarkRefLevel()
        {
            SendCommand("calc1:mark1:srl");
        }

        public void MarkToCenter()
        {
            SendCommand("calc1:mark1:scen");
        }

        public void Scan()
        {
            SendCommand("init:imm");
        }

        public void SetPeak()
        {
            SendCommand("calc1:mark1:max");
        }

        public void SetBandWidthdB(double idB)
        {
            SendCommand("calc1:mark1:func:bwid:ndb "+idB.ToString()+" db");
        }

        public double ReadBandWidthWL(int idx)
        {
            SendCommand("calc1:mark1:func:bwid:int on");// ! Enable bw marker interpolation
            SendCommand("calc1:mark1:func:bwid:read wav");// ! Set the BW unit of measurement to
            //WL
            SendCommand("calc1:mark1:func:bwid:stat on");//
            switch (idx)
            {
                case -1:
                    SendCommand("calc1:mark1:func:bwid:x:left?");
                    break;
                case 0:
                    SendCommand("calc1:mark1:func:bwid:x:cent?");
                    break;
                case 1:
                    SendCommand("calc1:mark1:func:bwid:x:righ?");
                    break;
            }
            return Convert.ToDouble(Read());
        }

        public double ReadBandWidth()//double swl,double ewl,double ires, double ispan, double idB)
        {
            //SetWLStart(swl);
            //SetWLStop(ewl);
            ////Scan();
            //SetPeak(); //Marker to peak
            //MarkRefLevel();//Marker to reference level
            //MarkToCenter();//Marker to center

            //SetResolution(ires);
            //SetSpan(ispan);
            //Scan();
            //Thread.Sleep(2000);
            //SetPeak();
            //SetBandWidthdB(idB);
            SendCommand("calc1:mark1:func:bwid:int on");// ! Enable bw marker interpolation
            SendCommand("calc1:mark1:func:bwid:read wav");// ! Set the BW unit of measurement to
            //WL
            SendCommand("calc1:mark1:func:bwid:stat on");// ! Enable bandwidth marker

            SendCommand("calc1:mark1:func:bwid:res?");// ! Return axis values between markers
            return Convert.ToDouble(Read());
        }
        #endregion

    }
}
