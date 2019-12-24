using System.Windows.Controls;
using System;
using System.Windows.Input;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.ComponentModel;
using System.Collections.ObjectModel;
using GPIB_utility;
using DiCon.UCB;
using DiCon.UCB.Communication;
using OxyPlot;
using OxyPlot.Series;

using PD.Functions;
using PD.AnalysisModel;
using PD.NavigationPages;
using PD.Functions;
using DiCon.Instrument.HP;

namespace PD.ViewModel
{
    public class ComViewModel : NotifyBase
    {
        public System.Timers.Timer timer1 = new System.Timers.Timer();
        public System.Timers.Timer timer2 = new System.Timers.Timer();
        public System.Timers.Timer timer3 = new System.Timers.Timer();
        public static SetupIniIP ini = new SetupIniIP();

        ControlCmd cmd = new ControlCmd();

        string ini_path = @"D:\PD\Instrument.ini";

        ICommunication icomm;
        DiCon.UCB.Communication.RS232.RS232 rs232;
        DiCon.UCB.MTF.IMTFCommand tf;

        //ICommand
        #region ICommand
        public ICommand BearTestCommand { get { return new Delegatecommand(BearTest); } }

        public ICommand Cmd_Test { get { return new Delegatecommand(cmd_test); } }
        #endregion

        //Commands
        #region Commands
        private void BearTest()
        {
            DataPoint ddpp = new DataPoint(1548.4, -3);
            List<DataPoint> dataPoint = new List<DataPoint>() { ddpp, ddpp, ddpp, ddpp, ddpp, ddpp, ddpp };

            list_SN = new List<string>() { "29A0JA300200", "29A0JA300201", "29A0JA300202", "29A0JA300203", "29A0JA300204", "29A0JA300205", "29A0JA300206", "29A0JA300207" };

            //test();

            #region Get Board Name
            //int board_count = _list_Board_Setting.Count;
            //for (int idz = 0; idz < board_count; idz++)
            //{
            //    if (!string.IsNullOrEmpty(_list_Board_Setting[idz][1]))
            //    {
            //        rs232 = new DiCon.UCB.Communication.RS232.RS232(_list_Board_Setting[idz][1]);
            //        rs232.OpenPort();
            //        icomm = (ICommunication)rs232;

            //        tf = new DiCon.UCB.MTF.RS232.RS232(icomm);

            //        //txtSN[idz].Text = tf.ReadSN();
            //        try
            //        {
            //            list_Board_Setting[idz][0] = tf.ReadSN();
            //            Thread.Sleep(500);
            //            rs232.ClosePort();
            //        }
            //        catch { }
            //    }
            //}
            #endregion

            bool dac_volt;
            if (bool.TryParse(Ini_Read("Connection", "DACorVolt"), out dac_volt))
                isDACorVolt = dac_volt;

            List<List<string>> lls = new List<List<string>>();
            lls.Add(new List<string>() { "1530.33", "-1.561" });
            lls.Add(new List<string>() { "1531.58", "-1.528", "29.3" });
            lls.Add(new List<string>() { "1530.33", "-1.561", "28.9" });
            lls.Add(new List<string>() { "1531.58", "-1.528", "29.3" });
            lls.Add(new List<string>() { "1530.33", "-1.561", "28.9" });
            lls.Add(new List<string>() { "1531.58", "-1.528", "29.3" });
            lls.Add(new List<string>() { "1530.33", "-1.561", "28.9" });
            lls.Add(new List<string>() { "1531.58", "-1.528", "29.3" });
            List_bear_say = new List<List<string>>(lls);

            Collection_bear_say.Add(lls);
            bear_say_all++;
            bear_say_now = bear_say_all;

            lls = new List<List<string>>();
            lls.Add(new List<string>() { "1550.33", "-1.336", "27.9" });
            lls.Add(new List<string>() { "1551.58", "-1.358", "28.3" });
            lls.Add(new List<string>() { "1550.33", "-1.336", "27.9" });
            lls.Add(new List<string>() { "1551.58", "-1.358", "28.3" });
            lls.Add(new List<string>() { "1550.33", "-1.336", "27.9" });
            lls.Add(new List<string>() { "1551.58", "-1.358", "28.3" });
            lls.Add(new List<string>() { "1550.33", "-1.336", "27.9" });
            lls.Add(new List<string>() { "1551.58", "-1.358", "28.3" });
            List_bear_say = new List<List<string>>(lls);

            Collection_bear_say.Add(lls);
            bear_say_all++;
            bear_say_now = bear_say_all;

            
        }

        private Task<string> testing = Task<string>.Factory.StartNew(() =>
        {
            return "AAA";
        });

        private void test()
        {
            MessageBox.Show(testing.Result);
        }

        private void cmd_test()
        {
            MessageBox.Show("Test");
        }

        public string ini_exist()
        {
            if (Directory.Exists(@"D:"))
                ini_path = @"D:\PD\Instrument.ini";
            else
                ini_path = System.Environment.CurrentDirectory + @"\Instrument.ini";

            return ini_path;
        }
        
        public void Convert_ReadPower_to_UIGauge(double power_PM, int ch)
        {
            Value_PD.Clear();
            Float_PD.Clear();

            ch++;

            float y = Convert.ToSingle(power_PM);
            float z = (y * 300 / -64 - 150) * -1;
            z = z != 1350 ? z : 150;

            if (z < -150)
                z = -150;

            if (ch < 9) //Switch mode  1~8
            {
                if (Gauge_Page_now == 1)
                {
                    Value_PD = new List<float>() { -150, -150, -150, -150, -150, -150, -150, -150 };
                    Float_PD = Analysis.ListDefault<double>(8);

                    Value_PD[ch - 1] = z;
                    Float_PD[ch - 1] = y;
                    Value_PD = new List<float>(Value_PD);

                    Str_PD = Analysis.ListDefault<string>(8);
                    Str_PD[ch - 1] = (Math.Round(y, 4)).ToString();
                    Str_PD = new List<string>(Str_PD);
                }
                else
                    Str_PD = new List<string>();
            }
            else if (this.ch < 13 && this.ch >= 9)  //Switch mode  9~12
            {
                if (Gauge_Page_now == 2)
                {
                    Value_PD = new List<float>() { -150, -150, -150, -150, -150, -150, -150, -150 };
                    Float_PD = Analysis.ListDefault<double>(8);

                    Value_PD[ch - 9] = z;
                    Float_PD[ch - 9] = y;
                    Value_PD = new List<float>(Value_PD);

                    Str_PD = Analysis.ListDefault<string>(8);
                    Str_PD[ch - 9] = (Math.Round(y, 4)).ToString();
                    Str_PD = new List<string>(Str_PD);
                }
                else
                    Str_PD = new List<string>();
            }
            else  //Normal mode
            {
                Value_PD.Add(z);  //-150~150 degree, for gauge binding
                Float_PD.Add(Math.Round(y,4));  //list 0~-64dBm in float type

                Str_PD = new List<string>() { (Math.Round(y, 4)).ToString() };
            }

            Value_PD = new List<float>(Value_PD);
            Float_PD = new List<double>(Float_PD);
        }
        
        public void Show_Bear_Window(object bear_say, bool _is_txt_reshow, string type)
        {
            if (Winbear != null)
                if (Winbear.IsLoaded)
                    Winbear.Close();
            
            Winbear = new Window_Bear(this, _is_txt_reshow, type);

            if (bear_say.GetType().Name == "String")
                Str_bear_say = (string)bear_say;

            Winbear.Show();
        }

        public void Show_Bear_Window(object bear_say, bool _is_txt_reshow, string type, bool BearBtn_show)
        {
            if (Winbear != null)
                if (Winbear.IsLoaded)
                    Winbear.Close();
            
            if (BearBtn_show)
            {
                BearBtn_visibility = Visibility.Visible;
                BearSay_RowSpan = 1;
            }
            else
            {
                BearBtn_visibility = Visibility.Collapsed;
                BearSay_RowSpan = 3;
            }

            Winbear = new Window_Bear(this, _is_txt_reshow, type);

            Winbear.Top = 0;
            Winbear.Left = 0;

            if (bear_say.GetType().Name == "String")
                Str_bear_say = (string)bear_say;

            Winbear.Show();
        }

        public async Task AccessDelayAsync(int delayTime)
        {
            await Task.Delay(delayTime);
        }

        public async Task TLS_SetActive(bool status)
        {
            if (PD_or_PM == true && IsGoOn == true)
            {
                await PM_Stop();
            }
            await Task.Delay(150);
            tls.SetActive(status);
            await Task.Delay(150);
            if (PD_or_PM == true && IsGoOn == true)
            {
                PM_GO();
            }
        }
        
        public string Ini_Read(string Section, string key)
        {
            string _ini_read;
            if (File.Exists(ini_path))
            {
                _ini_read = ini.IniReadValue(Section, key, ini_path);
            }
            else
                _ini_read = "";
            
            return _ini_read;
        }

        public void Ini_Write(string Section, string key, string value)
        {            
            if (!File.Exists(ini_path))
                Directory.CreateDirectory(System.IO.Directory.GetParent(ini_path).ToString());  //建立資料夾
            ini.IniWriteValue(Section, key, value, ini_path);  //創建ini file並寫入基本設定
        }

        public void Clean_Chart()
        {
            Save_PD_Value = new List<DataPoint>();
            Save_All_PD_Value = new List<List<DataPoint>>()
            {
                new List<DataPoint>(),
                new List<DataPoint>(),
                new List<DataPoint>(),
                new List<DataPoint>(),
                new List<DataPoint>(),
                new List<DataPoint>(),
                new List<DataPoint>(),
                new List<DataPoint>()
            };
        }
        
        public async Task Port_ReOpen(string comport)
        {
            if (!_pd_or_pm)  //PD type
            {
                if (_isGoOn)
                {
                    //timer2.Stop();
                    await PD_Stop();
                    await AccessDelayAsync(Int_Read_Delay);
                }
            }

            try
            {
                if (port_PD != null)
                {
                    if (port_PD.IsOpen)
                    {
                        port_PD.DiscardInBuffer();       // RX
                        port_PD.DiscardOutBuffer();      // TX
                        port_PD.Close();                        
                    }
                }
            }
            catch (Exception ex) { }

            try
            {
                if (!string.IsNullOrEmpty(comport))
                {
                    port_PD = new SerialPort(comport, 115200, Parity.None, 8, StopBits.One);
                    port_PD.Open();
                }
                else
                {
                    Str_cmd_read = "Comport is Null";
                    cmd.Save_Log_Message("Connection", Str_cmd_read, DateTime.Now.ToLongTimeString());
                }
                
            }
            catch { Str_cmd_read = "Port Open Error"; cmd.Save_Log_Message("Connection", Str_cmd_read, DateTime.Now.ToLongTimeString());  }
        }
        
        public List<SerialPort> List_Port = new List<SerialPort>(); 
        public void Multi_Port_Setting()
        {
            try
            {
                List_Port = new List<SerialPort>();
                if (list_Board_Setting.Count > 0)
                {
                    foreach(List<string> board_setting in list_Board_Setting)
                    {
                        if (!string.IsNullOrEmpty(board_setting[1]))
                        {
                            SerialPort port = new SerialPort(board_setting[1], 115200, Parity.None, 8, StopBits.One);
                            List_Port.Add(port);
                        }
                        else
                            List_Port.Add(new SerialPort());
                    }

                    foreach (SerialPort port in List_Port)
                    {
                        port.Open();
                    }
                }
                
            }
            catch { Str_cmd_read = "Port Open Error"; }
        }
                
        public async Task Port_Switch_ReOpen()
        {
            try
            {
                if (port_Switch != null)
                {
                    if (port_Switch.IsOpen)
                    {
                        port_Switch.DiscardInBuffer();       // RX
                        port_Switch.DiscardOutBuffer();      // TX
                        port_Switch.Close();
                    }
                }
            }
            catch { }

            try
            {
                if (comport_switch > 0)
                {
                    port_Switch = new SerialPort("COM" + comport_switch.ToString(), 115200, Parity.None, 8, StopBits.One);
                    port_Switch.Open();
                }
            }
            catch
            {
                Str_cmd_read = "Switch Port Open Error";
                return;
            }
        }

        public async Task<string> PD_GO()
        {
            try
            {
                if (port_PD != null)
                {
                    Clean_Chart();

                    Str_comment = "P0?";

                    await Port_ReOpen(_Selected_Comport);

                    timer2.Start();
                }
            }
            catch { Str_cmd_read = "Port is closed"; }
            await AccessDelayAsync(1);
            return Str_cmd_read;
        }

        public async Task PD_Stop()
        {
            try
            {
                // 清空 serial port 的緩存
                port_PD.DiscardInBuffer();       // RX
                port_PD.DiscardOutBuffer();      // TX

                timer2.Stop();

                port_PD.Close();

                Clean_Chart();

                timer2_count = 0;
            }
            catch { Str_cmd_read = "---"; }
            await AccessDelayAsync(1);
        }

        public void PM_GO()
        {
            if (_pd_or_pm)  //PM mode
            {
                try { timer3.Start(); }
                catch { Str_cmd_read = "GPIB error"; }                
            }
            else  //PD mode
            {
                try { timer2.Start(); }
                catch { Str_cmd_read = "PD error"; }
            }
            
        }

        public async Task PM_Stop()
        {
            try
            {
                timer3.Stop();

                timer3_count = 0;
            }
            catch
            {
                Str_cmd_read = "GPIB error";
            }
            await AccessDelayAsync(_int_Read_Delay*2);
        }

        public async void WriteDac(string ch, string TF_or_VOA, string DAC)
        {
            if(_station_type=="Hermetic Test")
                await Port_ReOpen(list_Board_Setting[int.Parse(ch)-1][1]);
            else
                await Port_ReOpen(_Selected_Comport);

            if (PD_or_PM == false)  //PD mode
                Str_comment = TF_or_VOA + ch.ToString() + " " + DAC;  //Write Dac
            else  //PM mode
            {
                Str_comment = TF_or_VOA + 1.ToString() + " " + DAC;  //Write Dac. For PM, always D1 XXX
            }

            try
            {
                port_PD.Write(Str_comment + "\r");
            }
            catch { Str_cmd_read = "Write Dac Error"; }
            await Task.Delay(Int_Write_Delay);
            
            //Gauge keep going on
            if (IsGoOn)
            {                
                try
                {
                    if (port_PD != null)
                    {                        
                        Str_comment = "P0?";

                        if (_pd_or_pm == false)
                            timer2.Start();
                        else
                            timer3.Start();
                    }
                }
                catch
                {
                    Str_cmd_read = "Port is closed";
                }
                await Task.Delay(1);
            }
        }
        #endregion

        public SerialPort port_PD, port_Switch;

        public BackgroundWorker bw1 = new BackgroundWorker();

        public void bw_setting()
        {
            bw1.WorkerSupportsCancellation = true;
            bw1.WorkerReportsProgress = true;

            bw1.DoWork += bw_dowork;
            bw1.ProgressChanged += bw_duringwork;
            bw1.RunWorkerCompleted += bw_afterwork;

            Str_cmd_read = "1";
            bw1.RunWorkerAsync();
        }

        private void bw_dowork(object sender, DoWorkEventArgs e)
        {
            Thread.Sleep(300);
            Str_cmd_read = "2";

            try
            {
                port_Switch = new SerialPort("COM" + Comport_Switch.ToString(), 115200, Parity.None, 8, StopBits.One);
                port_Switch.Open();
            }
            catch { }
        }

        private void bw_duringwork(object sender, ProgressChangedEventArgs e)
        {

        }

        private void bw_afterwork(object sender, RunWorkerCompletedEventArgs e)
        {

        }

        public HPPM pm = new HPPM();

        public List<string> _write_line { get; set; }

        public bool is_BearSay_History_Loaded { get; set; }

        private int _BoardTable_V123 = 1;  //V1
        public int BoardTable_V123
        {
            get { return _BoardTable_V123; }
            set { _BoardTable_V123 = value; }
        }

        private string _BoardTable_SelectedBoard;
        public string BoardTable_SelectedBoard
        {
            get { return _BoardTable_SelectedBoard; }
            set { _BoardTable_SelectedBoard = value; }
        }

        private int _BoardTable_SelectedIndex = 0;
        public int BoardTable_SelectedIndex
        {
            get { return _BoardTable_SelectedIndex; }
            set
            {
                _BoardTable_SelectedIndex = value;
                OnPropertyChanged("BoardTable_SelectedIndex");
            }
        }

        private List<List<string>> _List_BoardTable = new List<List<string>>();
        public List<List<string>> List_BoardTable
        {
            get { return _List_BoardTable; }
            set { _List_BoardTable = value; }
        }      

        private string[] _txt_No = new string[] { "Ch 1", "Ch 2", "Ch 3", "Ch 4", "Ch 5", "Ch 6", "Ch 7", "Ch 8" };
        public string[] txt_No
        {
            get { return _txt_No; }
            set
            {
                _txt_No = value;
                OnPropertyChanged("txt_No");
            }
        }

        private string[] _txt_Column_Description = new string[] { "WL", "IL" };
        public string[] txt_Column_Description
        {
            get { return _txt_Column_Description; }
            set
            {
                _txt_Column_Description = value;
                OnPropertyChanged("txt_Column_Description");
            }
        }

        private string _txt_ref_path = @"D:\Ref";
        public string txt_ref_path
        {
            get { return _txt_ref_path; }
            set
            {
                _txt_ref_path = value;
                OnPropertyChanged("txt_ref_path");
            }
        }

        private string _txt_board_table_path = @"\\192.168.2.3\tff\Data\BoardCalibration\UFA\";
        public string txt_board_table_path
        {
            get { return _txt_board_table_path; }
            set
            {
                _txt_board_table_path = value;
                OnPropertyChanged("txt_board_table_path");
            }
        }

        private string _txt_save_wl_data_path = @"\\192.168.2.5\wdm_data\UFA HT\";
        public string txt_save_wl_data_path
        {
            get { return _txt_save_wl_data_path; }
            set
            {
                _txt_save_wl_data_path = value;
                OnPropertyChanged("txt_save_wl_data_path");
            }
        }

        private List<DataPoint> _Save_PD_Value = new List<DataPoint>();
        public List<DataPoint> Save_PD_Value
        {
            get { return _Save_PD_Value; }
            set
            {
                _Save_PD_Value = value;
                OnPropertyChanged("Save_PD_Value");
            }
        }

        private List<List<DataPoint>> _Save_All_PD_Value = new List<List<DataPoint>>(Analysis.ListDefault<List<DataPoint>>(8));
        public List<List<DataPoint>> Save_All_PD_Value
        {
            get { return _Save_All_PD_Value; }
            set
            {
                _Save_All_PD_Value = value;
                OnPropertyChanged("Save_All_PD_Value");
            }
        }

        private double[] _mainWin_size;
        public double[] mainWin_size
        {
            get { return _mainWin_size; }
            set
            {
                _mainWin_size = value;
                OnPropertyChanged("mainWin_size");
            }
        }

        private Point _mainWin_point;
        public Point mainWin_point
        {
            get { return _mainWin_point; }
            set
            {
                _mainWin_point = value;
                OnPropertyChanged("mainWin_point");
            }
        }

        private List<bool> _List_switchBox_ischeck = new List<bool>();
        public List<bool> List_switchBox_ischeck
        {
            get { return _List_switchBox_ischeck; }
            set
            {
                _List_switchBox_ischeck = value;
                OnPropertyChanged("List_switchBox_ischeck");
            }
        }

        public Analysis analysis { get; set; }

        private int _Switch_Number = 13;
        public int ch
        {
            get { return _Switch_Number; }
            set { _Switch_Number = value; }
        }

        private int progressbar_value = 0;
        public int Progressbar_Value
        {
            get { return progressbar_value; }
            set { progressbar_value = value; }
        }

        private int _control_board_type = 0;  //0: UFV, 1: V, ...
        public int Control_board_type
        {
            get { return _control_board_type; }
            set
            {
                _control_board_type = value;
                OnPropertyChanged("Control_board_type");
            }
        }

        private int comport_switch = 1;
        public int Comport_Switch
        {
            get { return comport_switch; }
            set
            {
                comport_switch = value;
                Ini_Write("Connection", "Switch_Comport", value.ToString());  //創建ini file並寫入基本設定
                OnPropertyChanged("Comport_Switch");
            }
        }

        private List<string> _dateTime;
        public List<string> dateTime
        {
            get { return _dateTime; }
            set
            {
                _dateTime = value;
                OnPropertyChanged("dateTime");
            }
        }

        private Window_Bear winbear;
        public Window_Bear Winbear
        {
            get { return winbear; }
            set { winbear = value; }
        }
        
        private Window_Password winPass;
        public Window_Password WinPass
        {
            get { return winPass; }
            set { winPass = value; }
        }

        private Window_Switch_Box winswitch;
        public Window_Switch_Box WinSwitch
        {
            get { return winswitch; }
            set
            {
                winswitch = value;
            }
        }

        private GridLength _BearBtnSize_Height = new GridLength(1, GridUnitType.Star);
        public GridLength BearBtnSize_Height
        {
            get { return _BearBtnSize_Height; }
            set
            {
                _BearBtnSize_Height = value;
                OnPropertyChanged("BearBtnSize_Height");
            }
        }

        private GridLength _GaugeSize_Height = new GridLength(6, GridUnitType.Star);
        public GridLength GaugeSize_Height
        {
            get { return _GaugeSize_Height; }
            set
            {
                _GaugeSize_Height = value;
                OnPropertyChanged("GaugeSize_Height");
            }
        }

        private GridLength _GaugeTxtSize_Height = new GridLength(1, GridUnitType.Star);
        public GridLength GaugeTxtSize_Height
        {
            get { return _GaugeTxtSize_Height; }
            set
            {
                _GaugeTxtSize_Height = value;
                OnPropertyChanged("GaugeTxtSize_Height");
            }
        }

        private List<GridLength> _GaugeTxtSize_Column = new List<GridLength>() { new GridLength(2.5, GridUnitType.Star) , new GridLength(5, GridUnitType.Star) ,
            new GridLength(5, GridUnitType.Star) , new GridLength(3, GridUnitType.Star) , new GridLength(2.5, GridUnitType.Star) };
        public List<GridLength> GaugeTxtSize_Column
        {
            get { return _GaugeTxtSize_Column; }
            set
            {
                _GaugeTxtSize_Column = value;
                OnPropertyChanged("GaugeTxtSize_Column");
            }
        }

        private List<List<string>> _list_Board_Setting = new List<List<string>>();
        public List<List<string>> list_Board_Setting
        {
            get { return _list_Board_Setting; }
            set
            {
                _list_Board_Setting = value;
                OnPropertyChanged("list_Board_Setting");
            }
        }

        private List<List<string>> _list_D_All = new List<List<string>>();
        public List<List<string>> list_D_All
        {
            get { return _list_D_All; }
            set
            {
                _list_D_All = value;
                OnPropertyChanged("list_D_All");
            }
        }

        private List<int> _list_V12 = new List<int>();
        public List<int> List_V12
        {
            get { return _list_V12; }
            set
            {
                _list_V12 = value;
            }
        }

        private List<List<int>> _list_V3_dac = new List<List<int>>();
        public List<List<int>> List_V3_dac
        {
            get { return _list_V3_dac; }
            set { _list_V3_dac = value; }
        }

        private List<double> _list_V3_Voltage = new List<double>();
        public List<double> List_V3_Voltage
        {
            get { return _list_V3_Voltage; }
            set { _list_V3_Voltage = value; }
        }

        private List<List<float>> _list_PDvalue_byV12 = new List<List<float>>(new List<float>[8]);
        public List<List<float>> List_PDvalue_byV12
        {
            get { return _list_PDvalue_byV12; }
            set
            {
                _list_PDvalue_byV12 = value;
                OnPropertyChanged("List_PDvalue_byV12");
            }
        }

        private List<float> _list_PMvalue_byV12 = new List<float>();
        public List<float> List_PMvalue_byV12
        {
            get { return _list_PMvalue_byV12; }
            set
            {
                _list_PMvalue_byV12 = value;
                OnPropertyChanged("List_PMvalue_byV12");
            }
        }

        private List<int> _list_V3 = new List<int>();
        public List<int> List_V3
        {
            get { return _list_V3; }
            set
            {
                _list_V3 = value;
                OnPropertyChanged("List_V3");
            }
        }

        private List<List<float>> _list_PDvalue_byV3 = new List<List<float>>(new List<float>[8]);
        public List<List<float>> List_PDvalue_byV3
        {
            get { return _list_PDvalue_byV3; }
            set
            {
                _list_PDvalue_byV3 = value;
                OnPropertyChanged("List_PDvalue_byV3");
            }
        }

        private List<Visibility> _lineseries_visibility = new List<Visibility>(new Visibility[8]);
        public List<Visibility> LineSeries_Visible
        {
            get { return _lineseries_visibility; }
            set
            {
                _lineseries_visibility = value;
                OnPropertyChanged("LineSeries_Visible");
            }
        }                

        private string _station_type = "Testing";
        public string station_type
        {
            get { return _station_type; }
            set
            {
                _station_type = value;
                Ini_Write("Connection", "Station", value);
                if (value == "Hermetic Test")
                {
                    ch_count = 12;
                    //Bool_Gauge = new bool[] { true, true, true, true, true, true, true, true, true, true, true, true };
                    Bool_Gauge = new bool[] { false, false, false, false, false, false, false, false, false, false, false, false };
                    bo_temp_gauge = new bool[] { true, true, true, true, true, true, true, true, true, true, true, true };

                   

                    Is_switch_mode = true;

                    GaugeText_visible = Visibility.Hidden;
                    GaugeTabEnable = true;
                    GaugeSize_Height = new GridLength(6, GridUnitType.Star);
                    GaugeTxtSize_Height = new GridLength(1, GridUnitType.Star);

                    GaugeGrid3_visible = Visibility.Visible;
                }
                else
                {
                    ch_count = 8;
                    Is_switch_mode = false;
                    GaugeText_visible = Visibility.Visible;
                    GaugeTabEnable = false;
                    GaugeSize_Height = new GridLength(7, GridUnitType.Star);
                    GaugeTxtSize_Height = new GridLength(0, GridUnitType.Star);

                    GaugeGrid3_visible = Visibility.Collapsed;
                }
                    
                OnPropertyChanged("station_type");
            }
        }

        public double center_WL { get; set; }

        private string _Selected_Comport;
        public string Selected_Comport
        {
            get { return _Selected_Comport; }
            set
            {
                _Selected_Comport = value;
                OnPropertyChanged("Selected_Comport");
            }
        }

        private string _product_type;
        public string product_type
        {
            get { return _product_type; }
            set
            {
                _product_type = value;          
                OnPropertyChanged("product_type");
            }
        }

        private string _waterPrint1 = "Command";
        public string waterPrint1
        {
            get { return _waterPrint1; }
            set
            {
                _waterPrint1 = value;
                OnPropertyChanged("waterPrint1");
            }
        }

        private int _ErrorCode = 0;
        public int ErrorCode
        {
            get { return _ErrorCode; }
            set
            {
                _ErrorCode = value;

                string error_msg = "";
                switch (value)
                {
                    case 1:
                        error_msg = "Comport連線異常";
                        break;
                    case 21:
                        error_msg = "Curfitting資料點數小於1";
                        break;
                    case 22:
                        error_msg = "Curfitting結果異常";
                        break;
                }

                using (System.IO.StreamWriter file =
                new System.IO.StreamWriter(@"D:\PD\Log.txt", true))
                {
                    string str = "";
                    DateTime dt = DateTime.Now;
                    string d = dt.ToString("yyyy/MM/dd HH:mm:ss");
                    str = string.Concat(error_msg, "  ,  ", d);
                    file.WriteLine(str);
                }
            }
        }

        private int _ch_count = 8;
        public int ch_count
        {
            get { return _ch_count; }
            set { _ch_count = value; }
        }

        private List<string> _list_combox_TLS_WL_Range_items =
            new List<string>() { "C Band", "L Band" };
        public List<string> list_combox_TLS_WL_Range_items
        {
            get { return _list_combox_TLS_WL_Range_items; }
            set
            {
                _list_combox_TLS_WL_Range_items = value;
                OnPropertyChanged("list_combox_TLS_WL_Range_items");
            }
        }

        private List<string> _list_combox_Working_Table_Type_items =
            new List<string>() { "Testing", "Hermetic Test", "Chamber(S)", "BR" };
        public List<string> list_combox_Working_Table_Type_items
        {
            get { return _list_combox_Working_Table_Type_items; }
            set
            {
                _list_combox_Working_Table_Type_items = value;                
                OnPropertyChanged("list_combox_Working_Table_Type_items");
            }
        }

        private List<string> _list_combox_Control_Board_Type_items =
            new List<string>() { "UFV", "V" };
        public List<string> list_combox_Control_Board_Type_items
        {
            get { return _list_combox_Control_Board_Type_items; }
            set
            {
                _list_combox_Control_Board_Type_items = value;                
                OnPropertyChanged("list_combox_Control_Board_Type_items");
            }
        }

        private List<string> _list_combox_Laser_Type_items =
            new List<string>() { "Agilent", "GoLight" };
        public List<string> list_combox_Laser_Type_items
        {
            get { return _list_combox_Laser_Type_items; }
            set
            {
                _list_combox_Laser_Type_items = value;
                OnPropertyChanged("list_combox_Laser_Type_items");
            }
        }

        private List<string> _list_combox_K_WL_Type_items =
            new List<string>() { "ALL Range", "Human Like" };
        public List<string> list_combox_K_WL_Type_items
        {
            get { return _list_combox_K_WL_Type_items; }
            set
            {
                _list_combox_K_WL_Type_items = value;
                OnPropertyChanged("list_combox_K_WL_Type_items");
            }
        }

        private string _selected_K_WL_Type;
        public string selected_K_WL_Type
        {
            get { return _selected_K_WL_Type; }
            set
            {
                _selected_K_WL_Type = value;                
                OnPropertyChanged("selected_K_WL_Type");
            }
        }

        private List<string> _list_combox_Calibration_items = 
            new List<string>() { "Calibration", "VOA -> 0", "TF -> 0", "K VOA", "K TF", "K TF (Rough)", "K TF (Detail)", "Curve Fitting", "K WL" };
        public List<string> list_combox_Calibration_items
        {
            get { return _list_combox_Calibration_items; }
            set
            {
                _list_combox_Calibration_items = value;
                OnPropertyChanged("list_combox_Calibration_items");
            }
        }            

        private int _switch_index = 0;
        public int switch_index
        {
            get { return _switch_index; }
            set
            {
                _switch_index = value;
                OnPropertyChanged("switch_index");
            }
        }

        public List<string> _list_combox_switch_items = new List<string>()
            {
                "Switch?", "Switch-Ch1", "Switch-Ch2", "Switch-Ch3", "Switch-Ch4", "Switch-Ch5", "Switch-Ch6" ,
                "Switch-Ch7", "Switch-Ch8", "Switch-Ch9", "Switch-Ch10", "Switch-Ch11", "Switch-Ch12", "Switch Mode Off"
            };
        public List<string> list_combox_switch_items
        {
            get { return _list_combox_switch_items; }
            set
            {
                _list_combox_switch_items = value;
                OnPropertyChanged("list_combox_switch_items");
            }
        }

        private List<bool> _ischeck = new List<bool>() { false, true, false, false, false, false, false, false };
        public List<bool> IsCheck
        {
            get { return _ischeck; }
            set
            {
                _ischeck = value;
                List<Visibility> v_list = new List<Visibility>();
                foreach (bool b in _ischeck)
                {
                    if (b)
                        v_list.Add(Visibility.Visible);
                    else
                        v_list.Add(Visibility.Hidden);
                }
                LineSeries_Visible = new List<Visibility>(v_list);
                OnPropertyChanged("IsCheck");
            }
        }
        
        private Visibility _mainfunction_visibility = Visibility.Hidden;
        public Visibility Mainfunction_visibility
        {
            get { return _mainfunction_visibility; }
            set
            {
                _mainfunction_visibility = value;
                OnPropertyChanged("Mainfunction_visibility");
            }
        }

        private Visibility _BearBtn_visibility = Visibility.Visible;
        public Visibility BearBtn_visibility
        {
            get { return _BearBtn_visibility; }
            set
            {
                _BearBtn_visibility = value;
                OnPropertyChanged("BearBtn_visibility");
            }
        }

        private List<List<string>> _board_read = new List<List<string>>();
        public List<List<string>> board_read
        {
            get { return _board_read; }
            set
            {
                _board_read = value;
                OnPropertyChanged("board_read");
            }
        }

        private bool _isConnected = false;
        public bool isConnected
        {
            get { return _isConnected; }
            set
            {
                _isConnected = value;
                OnPropertyChanged("isConnected");
            }
        }

        private bool _isLaserActive = false;
        public  bool isLaserActive
        {
            get { return _isLaserActive; }
            set
            {
                
                _isLaserActive = value;
                bool _isgoon_saved = _isGoOn;
                if (PD_or_PM == true && IsGoOn == true)
                {
                    IsGoOn = false;
                    timer3.Stop();
                    timer3.Close();
                }
                tls.SetActive(value);
                //if(PD_or_PM==true && _isgoon_saved)
                //{
                //    IsGoOn = true;
                //    timer3.Start();
                //}
                OnPropertyChanged("isLaserActive");
            }
        }

        private void OnTimedEvent(object source, System.Timers.ElapsedEventArgs e)
        {
            //timer3.Stop();
        }

        private List<string> _list_SN = new List<string>() { "", "" , "" , "" , "" , "" , "" , "" , "" , "" , "" , "" };
        public List<string> list_SN
        {
            get { return _list_SN; }
            set
            {
                _list_SN = value;
                OnPropertyChanged("list_SN");
            }
        }

        private bool _isDACorVolt = false;  //False is Dac, True is Volt
        public bool isDACorVolt
        {
            get { return _isDACorVolt; }
            set
            {
                _isDACorVolt = value;
                if (_isDACorVolt)
                    DacType = "V3 Voltage";
                else
                    DacType = "V3 Dac";
                OnPropertyChanged("isDACorVolt");
                //OnPropertyChanged("DacType");
                Ini_Write("Connection", "DACorVolt", value.ToString());
            }
        }

        private string _DacType = "V3 Dac";
        public string DacType
        {
            get { return _DacType; }
            set
            {
                _DacType = value;
                OnPropertyChanged("DacType");
            }
        }

        private bool _isEnglish = false;
        public bool isEnglish
        {
            get { return _isEnglish; }
            set
            {
                _isEnglish = value;
                OnPropertyChanged("isEnglish");
            }
        }

        private bool _dB_or_dBm = true; //false is "dBm"
        public bool dB_or_dBm
        {
            get { return _dB_or_dBm; }
            set
            {
                _dB_or_dBm = value;
                OnPropertyChanged("dB_or_dBm");
            }
        }

        private bool _Auto_Connect_TLS = true; 
        public bool Auto_Connect_TLS
        {
            get { return _Auto_Connect_TLS; }
            set
            {
                _Auto_Connect_TLS = value;
                ini.IniWriteValue("Connection", "Auto_Connect_TLS", value.ToString(), ini_path);
                OnPropertyChanged("Auto_Connect_TLS");
            }
        }

        private List<double> _float_WL_Ref = new List<double>();  //目前波長的Reference值
        public List<double> float_WL_Ref
        {
            get { return _float_WL_Ref; }
            set
            {
                _float_WL_Ref = value;
                OnPropertyChanged("float_WL_Ref");
            }
        }

        private List<double> _list_wl = new List<double>();
        public List<double> list_wl
        {
            get { return _list_wl; }
            set
            {
                _list_wl = value;
            }
        }

        private List<List<double>> _list_WL_Ref;
        public List<List<double>> list_WL_Ref
        {
            get { return _list_WL_Ref; }
            set
            {
                _list_WL_Ref = value;
                //OnPropertyChanged("list_WL_Ref");
            }
        }

        private double _float_WL_Scan_Start = 1526;
        public double float_WL_Scan_Start
        {
            get { return _float_WL_Scan_Start; }
            set
            {
                _float_WL_Scan_Start = value;
                OnPropertyChanged("float_WL_Scan_Start");
            }
        }

        private double _float_WL_Scan_End = 1528;
        public double float_WL_Scan_End
        {
            get { return _float_WL_Scan_End; }
            set
            {
                _float_WL_Scan_End = value;
                OnPropertyChanged("float_WL_Scan_End");
            }
        }

        private double _float_WL_Scan_Gap = 0.6;
        public double float_WL_Scan_Gap
        {
            get { return _float_WL_Scan_Gap; }
            set
            {
                _float_WL_Scan_Gap = value;
                OnPropertyChanged("float_WL_Scan_Gap");
            }
        }

        private float[] _float_TLS_WL_Range = { 1523, 1573 };
        public float[] float_TLS_WL_Range
        {
            get { return _float_TLS_WL_Range; }
            set
            {
                _float_TLS_WL_Range = value;
                OnPropertyChanged("float_TLS_WL_Range");
            }
        }

        int index = 0;
        private double _double_Laser_Wavelength;
        public double Double_Laser_Wavelength
        {
            get { return _double_Laser_Wavelength; }
            set
            {
                _double_Laser_Wavelength = value;
                
                index = list_wl.IndexOf((float)Math.Round(value,2));
                if (index >= 0)
                {
                    float_WL_Ref = new List<double>();
                    if (!_pd_or_pm)  //PD mode
                    {
                        if (_list_WL_Ref.Count == 8)
                        {
                            for (int ch = 0; ch < 8; ch++)
                            {
                                float_WL_Ref.Add(list_WL_Ref[ch][index]);
                            }
                        }
                    }
                    else  //pm
                    {
                        if (index < 0)
                        {
                            float_WL_Ref.Add(0);
                            return;
                        }

                        if (_list_WL_Ref.Count >= 1)
                        {
                            if (station_type != "Hermetic Test")
                            {
                                for (int ch = 0; ch < 8; ch++)
                                {
                                    float_WL_Ref.Add(list_WL_Ref[ch][index]);
                                }
                            }
                            else
                            {
                                for (int ch = 0; ch < 8; ch++)
                                {
                                    float_WL_Ref.Add(list_WL_Ref[ch][index]);
                                }
                            }
                        }
                    }
                    
                }
                OnPropertyChanged("Double_Laser_Wavelength");
            }
        }

        private int _BearSay_RowSpan = 3;
        public int BearSay_RowSpan
        {
            get { return _BearSay_RowSpan; }
            set
            {
                _BearSay_RowSpan = value;
                OnPropertyChanged("BearSay_RowSpan");
            }
        }

        private int _int_Dac_cmd = 0;
        public int int_Dac_cmd
        {
            get { return _int_Dac_cmd; }
            set
            {
                _int_Dac_cmd = value;
                OnPropertyChanged("int_Dac_cmd");
            }
        }

        private int _int_Dac_min = 0;
        public int int_Dac_min
        {
            get { return _int_Dac_min; }
            set
            {
                _int_Dac_min = value;
                OnPropertyChanged("int_Dac_min");
            }
        }

        private string _UserID = "";
        public string UserID
        {
            get { return _UserID; }
            set
            {
                _UserID = value;
                OnPropertyChanged("UserID");
            }
        }

        private string _selected_band;
        public string selected_band
        {
            get { return _selected_band; }
            set
            {
                _selected_band = value;               
                OnPropertyChanged("selected_band");
            }
        }

        private double _double_Laser_Power = 0;
        public double Double_Laser_Power
        {
            get { return _double_Laser_Power; }
            set
            {
                _double_Laser_Power = value;
                OnPropertyChanged("Double_Laser_Power");
            }
        }

        private SolidColorBrush[] _ref_Color = new SolidColorBrush[8]; 
        public SolidColorBrush[] ref_Color
        {
            get { return _ref_Color; }
            set
            {
                _ref_Color = value;
                OnPropertyChanged("ref_Color");
            }
        }

        //#FF6B6D78  Level 0 : 底灰
        //#FF68FCDF  Level 1 : 淺綠
        //#FF10E2C4  Level 2 : 中綠
        //#FF10E2C4  Level 3 : 深綠
        //#FF36836E  Level 4 : 深深綠
        //#FFD0F6FF  Level 1 : 淺藍
        private SolidColorBrush _main_Color = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF33D3C4"));
        public SolidColorBrush Main_Color
        {
            get { return _main_Color; }
            set
            {
                _main_Color = value;
                OnPropertyChanged("Main_Color");
            }
        }

        private SolidColorBrush _complement_Color = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF10E2C4"));
        public SolidColorBrush Complement_Color
        {
            get { return _complement_Color; }
            set
            {
                _complement_Color = value;
                OnPropertyChanged("Complement");
            }
        }

        //private double _double_Laser_Power;
        //public double Double_Laser_Power
        //{
        //    get { return _double_Laser_Power; }
        //    set
        //    {
        //        _double_Laser_Power = value;
        //        OnPropertyChanged("Double_Laser_Power");
        //    }
        //}

        private HPTLS _tls = new HPTLS();
        public HPTLS tls
        {
            get { return _tls; }
            set
            {
                _tls = value;
                OnPropertyChanged("tls");
            }
        }

        private bool _isStop = false;
        public bool isStop
        {
            get { return _isStop; }
            set
            {
                _isStop = value;
                OnPropertyChanged("isStop");
            }
        }

        private bool _isDeltaILModeOn = false;
        public bool isDeltaILModeOn
        {
            get { return _isDeltaILModeOn; }
            set
            {
                _isDeltaILModeOn = value;
                OnPropertyChanged("isDeltaILModeOn");
            }
        }

        private List<double> _savedPower_for_deltaMode = new List<double>();
        public List<double> savedPower_for_deltaMode
        {
            get { return _savedPower_for_deltaMode; }
            set
            {
                _savedPower_for_deltaMode = value;
                OnPropertyChanged("savedPower_for_deltaMode");
            }
        }

        private List<double> _double_Maxdelta = new List<double>() { 0, 0, 0, 0, 0, 0, 0, 0 };
        public List<double> double_Maxdelta
        {
            get { return _double_Maxdelta; }
            set
            {
                _double_Maxdelta = value;
                OnPropertyChanged("double_Maxdelta");
            }
        }

        private List<double> _double_MaxIL_for_DeltaMode = new List<double>();
        public List<double> double_MaxIL_for_DeltaMode
        {
            get { return _double_MaxIL_for_DeltaMode; }
            set
            {
                _double_MaxIL_for_DeltaMode = value;
                OnPropertyChanged("double_MaxIL_for_DeltaMode");
            }
        }
        
        private List<double> _double_MinIL_for_DeltaMode = new List<double>();
        public List<double> double_MinIL_for_DeltaMode
        {
            get { return _double_MinIL_for_DeltaMode; }
            set
            {
                _double_MinIL_for_DeltaMode = value;
                OnPropertyChanged("double_MinIL_for_DeltaMode");
            }
        }

        private bool _is_switch_mode = false;
        public bool Is_switch_mode
        {
            get { return _is_switch_mode; }
            set
            {
                _is_switch_mode = value;
                OnPropertyChanged("Is_switch_mode");
            }
        }

        private int _switch_selected = 0;
        public int switch_selected_index
        {
            get { return _switch_selected; }
            set { _switch_selected = value; }
        }
        
        private bool[] _bo_temp_gauge = new bool[12];
        public bool[] bo_temp_gauge
        {
            get { return _bo_temp_gauge; }
            set
            {
                _bo_temp_gauge = value;
                OnPropertyChanged("bo_temp_gauge");
            }
        }

        private int _Gauge_Page_now = 1;
        public int Gauge_Page_now
        {
            get { return _Gauge_Page_now; }
            set { _Gauge_Page_now = value; }
        }

        private bool[] _bool_Page1 = new bool[8];
        public bool[] Bool_Page1
        {
            get { return _bool_Page1; }
            set { _bool_Page1 = value; }
        }

        private bool[] _bool_Page2 = new bool[4];
        public bool[] Bool_Page2
        {
            get { return _bool_Page2; }
            set { _bool_Page2 = value; }
        }

        private bool[] _bool_gauge = new bool[] { false, false, false, false, false, false, false, false, false, false, false, false };
        public bool[] Bool_Gauge
        {
            get { return _bool_gauge; }
            set { _bool_gauge = value; }
        }

        private bool[] _bool_gauge_show = new bool[] { false, false, false, false, false, false, false, false };
        public bool[] Bool_Gauge_Show
        {
            get { return _bool_gauge_show; }
            set
            {
                _bool_gauge_show = value;                
                OnPropertyChanged("Bool_Gauge_Show");
            }
        }

        private List<int> list_curfit_resultDac = new List<int>();
        public List<int> List_curfit_resultDac
        {
            get { return list_curfit_resultDac; }
            set
            {
                list_curfit_resultDac = value;
                OnPropertyChanged("List_curfit_resultDac");
            }
        }

        private int list_curfit_resultDac_single = 0;
        public int List_curfit_resultDac_single
        {
            get { return list_curfit_resultDac_single; }
            set
            {
                list_curfit_resultDac_single = value;
                OnPropertyChanged("List_curfit_resultDac_single");
            }
        }

        private List<double> list_curfit_resultWL = new List<double>();
        public List<double> List_curfit_resultWL
        {
            get { return list_curfit_resultWL; }
            set
            {
                list_curfit_resultWL = value;
                //OnPropertyChanged("List_curfit_resultWL");
            }
        }

        public double List_curfit_resultWL_single { get; set; }

        private int _int_fontsize = 17;
        public int Int_Fontsize
        {
            get { return _int_fontsize; }
            set
            {
                _int_fontsize = value;
                OnPropertyChanged("Int_Fontsize");
            }
        }

        private int _int_V3_scan_start = 40000;
        public int int_V3_scan_start
        {
            get { return _int_V3_scan_start; }
            set
            {
                _int_V3_scan_start = value;
                Ini_Write("Productions", "V3_scan_start", value.ToString());  //創建ini file並寫入基本設定
                OnPropertyChanged("int_V3_scan_start");
            }
        }

        private int _int_V3_scan_end = 65500;
        public int int_V3_scan_end
        {
            get { return _int_V3_scan_end; }
            set
            {
                _int_V3_scan_end = value;
                Ini_Write("Productions", "V3_scan_end", value.ToString());  //創建ini file並寫入基本設定
                OnPropertyChanged("int_V3_scan_end");
            }
        }

        private int _int_V3_scan_gap = 3600;
        public int int_V3_scan_gap
        {
            get { return _int_V3_scan_gap; }
            set
            {
                _int_V3_scan_gap = value;
                Ini_Write("Productions", "V3_Scan_Gap", value.ToString());  //創建ini file並寫入基本設定
                OnPropertyChanged("int_V3_scan_gap");
            }
        }

        private int _int_rough_scan_gap = 1000;
        public int int_rough_scan_gap
        {
            get { return _int_rough_scan_gap; }
            set
            {
                _int_rough_scan_gap = value;
                Ini_Write("Productions", "Rough_Scan_Gap", value.ToString());  //創建ini file並寫入基本設定
                OnPropertyChanged("int_rough_scan_gap");
            }
        }

        private int _int_detail_scan_gap = 200;
        public int int_detail_scan_gap
        {
            get { return _int_detail_scan_gap; }
            set
            {
                _int_detail_scan_gap = value;
                Ini_Write("Productions", "Detail_Scan_Gap", value.ToString());  //創建ini file並寫入基本設定
                OnPropertyChanged("int_detail_scan_gap");
            }
        }

        private int _int_rough_scan_start = -65500;
        public int int_rough_scan_start
        {
            get { return _int_rough_scan_start; }
            set
            {
                _int_rough_scan_start = value;
                OnPropertyChanged("int_rough_scan_start");
            }
        }

        private int _int_rough_scan_stop = 65500;
        public int int_rough_scan_stop
        {
            get { return _int_rough_scan_stop; }
            set
            {
                _int_rough_scan_stop = value;
                OnPropertyChanged("int_rough_scan_stop");
            }
        }

        private int _int_Read_Delay = 125;
        public int Int_Read_Delay
        {
            get { return _int_Read_Delay; }
            set
            {
                _int_Read_Delay = value;
                timer2.Interval = value;
                timer3.Interval = value;
                OnPropertyChanged("Int_Read_Delay");

                Ini_Write("Connection", "RS232_Delay_Time", value.ToString());
            }
        }

        private int _int_Write_Delay = 50;
        public int Int_Write_Delay
        {
            get { return _int_Write_Delay; }
            set
            {
                _int_Write_Delay = value;
                OnPropertyChanged("Int_Write_Delay");
            }
        }

        private int _int_Set_WL_Delay = 200;
        public int Int_Set_WL_Delay
        {
            get { return _int_Set_WL_Delay; }
            set
            {
                _int_Set_WL_Delay = value;
                OnPropertyChanged("Int_Set_WL_Delay");
            }
        }

        private string _chart_x_title = "Time(s)";
        public string Chart_x_title
        {
            get { return _chart_x_title; }
            set
            {
                _chart_x_title = value;
                OnPropertyChanged("Chart_x_title");
            }
        }

        private string _chart_y_title = "Power(dBm)";
        public string Chart_y_title
        {
            get { return _chart_y_title; }
            set
            {
                _chart_y_title = value;
                OnPropertyChanged("Chart_y_title");
            }
        }

        private string _str_status = "Status : --";
        public string Str_Status
        {
            get { return _str_status; }
            set
            {
                _str_status = "Status : " + value;
                OnPropertyChanged("Str_Status");
            }
        }

        private string _str_Unit = "dB";
        public string str_Unit
        {
            get { return _str_Unit; }
            set
            {
                _str_Unit = value;
                OnPropertyChanged("str_Unit");
            }
        }

        private string _str_Go_content = "GO";
        public string Str_Go_Content
        {
            get { return _str_Go_content; }
            set
            {
                _str_Go_content = value;
                OnPropertyChanged("Str_Go_Content");
            }
        }

        private bool _isGoOn = false;
        public bool IsGoOn
        {
            get { return _isGoOn; }
            set
            {
                _isGoOn = value;
                Str_Go_Content = value == false ? "Go" : "Stop";
                OnPropertyChanged("IsGoOn");
            }
        }

        private bool _pd_or_pm = false;  //False is PD, True is PM
        public bool PD_or_PM
        {
            get { return _pd_or_pm; }
            set
            {
                _pd_or_pm = value;
                if (_pd_or_pm)
                {                    
                    Main_Color = (SolidColorBrush)(new BrushConverter().ConvertFrom("#0085CA"));
                    Ini_Write("Connection", "PD_or_PM", "PM");
                }
                else
                {
                    Main_Color = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF33D3C4"));
                    Ini_Write("Connection", "PD_or_PM", "PD");
                }
                    
                OnPropertyChanged("PD_or_PM");
            }
        }

        private int _timer2_count = 0;
        public int timer2_count
        {
            get { return _timer2_count; }
            set
            {
                _timer2_count = value;
                OnPropertyChanged("timer2_count");
            }
        }

        private int _timer3_count = 0;
        public int timer3_count
        {
            get { return _timer3_count; }
            set
            {
                _timer3_count = value;
                OnPropertyChanged("timer3_count");
            }
        }

        private int _tls_BoardNumber = 0;
        public int tls_BoardNumber
        {
            get { return _tls_BoardNumber; }
            set
            {
                _tls_BoardNumber = value;
                OnPropertyChanged("tls_BoardNumber");
            }
        }

        private int _tls_Addr = 24;
        public int tls_Addr
        {
            get { return _tls_Addr; }
            set
            {
                _tls_Addr = value;
                OnPropertyChanged("tls_Addr");
            }
        }

        private int _PM_slot = 1;
        public int PM_slot
        {
            get { return _PM_slot; }
            set
            {
                _PM_slot = value;
                OnPropertyChanged("PM_slot");
            }
        }

        private double _window_bear_width = 500;  //[width, height]
        public double window_bear_width
        {
            get { return _window_bear_width; }
            set
            {
                _window_bear_width = value;
                OnPropertyChanged("window_bear_width");
            }
        }

        private double _window_bear_heigh = 250;  //[width, height]
        public double window_bear_heigh
        {
            get { return _window_bear_heigh; }
            set
            {
                _window_bear_heigh = value;
                OnPropertyChanged("window_bear_heigh");
            }
        }

        private List<DataPoint> _chart_datapoints_temp = new List<DataPoint>();
        public List<DataPoint> Chart_DataPoints_temp
        {
            get { return _chart_datapoints_temp; }
            set
            {
                _chart_datapoints_temp = value;
                OnPropertyChanged("Chart_DataPoints_temp");
            }
        }
        
        private List<List<List<DataPoint>>> _chart_all_datapoints_history = new List<List<List<DataPoint>>>();
        public List<List<List<DataPoint>>> Chart_All_Datapoints_History
        {
            get { return _chart_all_datapoints_history; }
            set
            {
                _chart_all_datapoints_history = value;
                OnPropertyChanged("Chart_All_Datapoints_History");
            }
        }

        private int _int_chart_count = 0;
        public int int_chart_count
        {
            get { return _int_chart_count; }
            set
            {
                _int_chart_count = value;
                OnPropertyChanged("int_chart_count");
            }
        }

        private int _int_chart_now = 0;
        public int int_chart_now
        {
            get { return _int_chart_now; }
            set
            {
                _int_chart_now = value;
                OnPropertyChanged("int_chart_now");
            }
        }

        private List<List<DataPoint>> _chart_all_datapoints = new List<List<DataPoint>>() { new List<DataPoint>(8)};
        public List<List<DataPoint>> Chart_All_DataPoints
        {
            get { return _chart_all_datapoints; }
            set
            {
                _chart_all_datapoints = value;
                OnPropertyChanged("Chart_All_DataPoints");
            }
        }

        private List<DataPoint> _chart_datapoints = new List<DataPoint>();
        public List<DataPoint> Chart_DataPoints
        {
            get { return _chart_datapoints; }
            set
            {
                _chart_datapoints = value;
                OnPropertyChanged("Chart_DataPoints");
            }
        }

        private List<DataPoint> _chart_datapoints_ref = new List<DataPoint>();
        public List<DataPoint> Chart_DataPoints_ref
        {
            get { return _chart_datapoints_ref; }
            set
            {
                _chart_datapoints_ref = value;
                OnPropertyChanged("Chart_DataPoints_ref");
            }
        }

        private List<List<DataPoint>> _chart_all_datapoints_ref = new List<List<DataPoint>>() { new List<DataPoint>(8) };
        public List<List<DataPoint>> Chart_All_DataPoints_ref
        {
            get { return _chart_all_datapoints_ref; }
            set
            {
                _chart_all_datapoints_ref = value;
                OnPropertyChanged("Chart_All_DataPoints_ref");
            }
        }

        private List<float> _value_PD = new List<float>(8); //-150~150 degree, for gauge binding
        public List<float> Value_PD
        {
            get { return _value_PD; }
            set
            {
                _value_PD = value;
                OnPropertyChanged("Value_PD");
            }
        }

        private int _Int_Process_Schedule = 0; //0~100
        public int Int_Process_Schedule
        {
            get { return _Int_Process_Schedule; }
            set
            {
                _Int_Process_Schedule = value;
                OnPropertyChanged("Int_Process_Schedule");
            }
        }

        private List<double> _float_PD = new List<double>(); //0~-64dBm
        public List<double> Float_PD
        {
            get { return _float_PD; }
            set
            {
                _float_PD = value;
                OnPropertyChanged("Float_PD");
            }
        }

        private List<string> _str_PD = new List<string>();
        public List<string> Str_PD
        {
            get { return _str_PD; }
            set
            {
                _str_PD = value;
                OnPropertyChanged("Str_PD");
            }
        }

        private List<string> _str_channel = new List<string>() { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12" };
        public List<string> Str_Channel
        {
            get { return _str_channel; }
            set
            {
                _str_channel = value;
                OnPropertyChanged("Str_Channel");
            }
        }

        private List<Visibility> _channel_visible = new List<Visibility>() { };
        public List<Visibility> Channel_visible
        {
            get { return _channel_visible; }
            set
            {
                _channel_visible = value;
                OnPropertyChanged("Channel_visible");
            }
        }

        private Visibility _GaugeText_visible = Visibility.Visible;
        public Visibility GaugeText_visible
        {
            get { return _GaugeText_visible; }
            set
            {
                _GaugeText_visible = value;
                OnPropertyChanged("GaugeText_visible");
                OnPropertyChanged("GaugeText_visible_Reverse");
            }
        }

        private Visibility _GaugeGrid3_visible = Visibility.Visible;
        public Visibility GaugeGrid3_visible
        {
            get { return _GaugeGrid3_visible; }
            set
            {
                _GaugeGrid3_visible = value;
                OnPropertyChanged("GaugeGrid3_visible");
            }
        }

        public Visibility GaugeText_visible_Reverse
        {
            get
            {
                if (GaugeText_visible == Visibility.Visible)
                    return Visibility.Hidden;
                else
                    return Visibility.Visible;
            }
        }
               
        private double _GaugePage_Width=1200;
        public double GaugePage_Width
        {
            get { return _GaugePage_Width; }
            set
            {
                _GaugePage_Width = value;
                OnPropertyChanged("GaugePage_Width");
            }
        }

        private double _Gauge_Width = 300;
        public double Gauge_Width
        {
            get
            {
                _Gauge_Width = _GaugePage_Width / 4;
                return _Gauge_Width;
            }
            set
            {
                _Gauge_Width = value;
                OnPropertyChanged("Gauge_Width");
            }
        }

        //private double _GaugePage_Height = 800;
        //public double GaugePage_Height
        //{
        //    get { return _GaugePage_Height; }
        //    set
        //    {
        //        _GaugePage_Height = value;
        //        OnPropertyChanged("GaugePage_Height");
        //        OnPropertyChanged("Gauge_Height");
        //    }
        //}

        //private double _Gauge_Height = 300;
        //public double Gauge_Height
        //{
        //    get
        //    {
        //        _Gauge_Height = _GaugePage_Height / 2;
        //        return _Gauge_Height;
        //    }
        //    set
        //    {
        //        _Gauge_Height = value;
        //        OnPropertyChanged("Gauge_Height");
        //    }
        //}

        //private List<int> _GaugeTabOrder = new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7 };
        //public List<int> GaugeTabOrder
        //{
        //    get { return _GaugeTabOrder; }
        //    set
        //    {
        //        _GaugeTabOrder = value;
        //        OnPropertyChanged("GaugeTabOrder");
        //    }
        //}

        private bool _GaugeTabEnable;
        public bool GaugeTabEnable
        {
            get { return _GaugeTabEnable; }
            set
            {
                _GaugeTabEnable = value;
                OnPropertyChanged("GaugeTabEnable");
            }
        }

        private bool _is_Gauge_ContinueSelect = false;
        public bool is_Gauge_ContinueSelect
        {
            get { return _is_Gauge_ContinueSelect; }
            set
            {
                _is_Gauge_ContinueSelect = value;
                OnPropertyChanged("is_Gauge_ContinueSelect");
            }
        }

        private List<string> _str_K_WL_result = new List<string>();
        public List<string> Str_K_WL_result
        {
            get { return _str_K_WL_result; }
            set
            {
                _str_K_WL_result = value;
                OnPropertyChanged("Str_K_WL_result");
            }
        }

        private string _str_comment = " ";
        public string Str_comment
        {
            get { return _str_comment; }
            set
            {
                _str_comment = value;
                OnPropertyChanged("Str_comment");
            }
        }

        private string _msg = "";
        public string msg
        {
            get { return _msg; }
            set
            {
                _msg = value;
                OnPropertyChanged("msg");
            }
        }

        private string _str_cmd_read = "---";
        public string Str_cmd_read
        {
            get { return _str_cmd_read; }
            set
            {
                _str_cmd_read = value;
                OnPropertyChanged("Str_cmd_read");
            }
        }

        private string _str_bear_say = ". . . .";
        public string Str_bear_say
        {
            get { return _str_bear_say; }
            set
            {
                _str_bear_say = value;
                //OnPropertyChanged("Str_bear_say");
            }
        }

        private List<string> _list_bear_say_DataLabel = new List<string>() { "K WL", "WL", "IL" };
        public List<string> List_bear_say_DataLabel
        {
            get { return _list_bear_say_DataLabel; }
            set
            {
                _list_bear_say_DataLabel = value;
                OnPropertyChanged("List_bear_say_DataLabel");
            }
        }

        private List<List<string>> _list_bear_say ;
        public List<List<string>> List_bear_say
        {
            get { return _list_bear_say; }
            set
            {
                _list_bear_say = value;
                OnPropertyChanged("List_bear_say");
            }
        }

        private int _bear_say_all = 0;
        public int bear_say_all
        {
            get { return _bear_say_all; }
            set
            {
                _bear_say_all = value;
                OnPropertyChanged("bear_say_all");
            }
        }
        
        private int _bear_say_now = 0;
        public int bear_say_now
        {
            get { return _bear_say_now; }
            set
            {
                _bear_say_now = value;
                OnPropertyChanged("bear_say_now");
            }
        }

        private List<List<List<string>>> _Collection_bear_say = new List<List<List<string>>>();
        public List<List<List<string>>> Collection_bear_say
        {
            get { return _Collection_bear_say; }
            set
            {
                _Collection_bear_say = value;
                OnPropertyChanged("Collection_bear_say");
            }
        }

        private List<List<string>> _list_D0_value = new List<List<string>>();
        public List<List<string>> List_D0_value
        {
            get { return _list_D0_value; }
            set
            {
                _list_D0_value = value;
                OnPropertyChanged("List_D0_value");
            }
        }

        private string[,] _str_D0_value = new string[8,3];
        public string[,] Str_D0_value
        {
            get { return _str_D0_value; }
            set
            {
                _str_D0_value = value;
                OnPropertyChanged("Str_D0_value");
            }
        }

        private string[] _str_D1_value;
        public string[] Str_D1_value
        {
            get { return _str_D1_value; }
            set
            {
                _str_D1_value = value;
                OnPropertyChanged("Str_D1_value");
            }
        }

        private string _str_read = "---";
        public string Str_read
        {
            get { return _str_read; }
            set
            {
                _str_read = value;
                OnPropertyChanged("Str_read");
            }
        }

        private string _title = "UFA Fast Calibration v1.1.0";
        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                OnPropertyChanged("Title");
            }
        }

        private ItemsControl _combox_items ;
        public ItemsControl Combox_items
        {
            get { return _combox_items; }
            set
            {
                _combox_items = value;
                OnPropertyChanged("Combox_items");
            }
        }

        //public int Int_Dac_min { get => _int_Dac_min; set => _int_Dac_min = value; }

        //public int Int_Read_Delay { get => _int_Read_Delay; set => _int_Read_Delay = value; }
    }
}
