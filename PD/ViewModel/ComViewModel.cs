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
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System.Reflection;

using OxyPlot;

using PD.Functions;
using PD.Models;
using PD.AnalysisModel;
using PD.NavigationPages;
using DiCon.Instrument.HP;

namespace PD.ViewModel
{
    public class ComViewModel : NotifyBase
    {
        #region Timers
        //public System.Timers.Timer timer1 = new System.Timers.Timer();
        public System.Timers.Timer timer2 = new System.Timers.Timer();
        public System.Timers.Timer timer3 = new System.Timers.Timer();
        public System.Timers.Timer timer_arduino_AdRead = new System.Timers.Timer();

        //宣告Timer
        public DispatcherTimer _timer = new DispatcherTimer();

        public void Timer_Command_Setting()
        {
            timer_command.Interval = Int_Set_WL_Delay;
            timer_command.Elapsed += Timer_command_Elapsed;
        }

        public System.Timers.Timer timer_command = new System.Timers.Timer();
        private void Timer_command_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (ComMembers.Count == 0) return;

            ComMember comMember = ComMembers[0];
        }

        public System.Diagnostics.Stopwatch watch { get; set; }
        #endregion

        public static SetupIniIP ini = new SetupIniIP();

        #region List_Members
        public List<ScriptModel> list_scriptModels = new List<ScriptModel>();
        public CmdMsgModel cmdMsg = new CmdMsgModel();

        private ChannelModel _PD_A_ChannelModel = new ChannelModel()
        {
            Board_Port = "COM1",
            BautRate = 115200
        };
        public ChannelModel PD_A_ChannelModel
        {
            get { return _PD_A_ChannelModel; }
            set
            {
                _PD_A_ChannelModel = value;
                OnPropertyChanged_Normal("PD_A_ChannelModel");
            }
        }

        private ChannelModel _PD_B_ChannelModel = new ChannelModel()
        {
            Board_Port = "COM2",
            BautRate = 115200
        };
        public ChannelModel PD_B_ChannelModel
        {
            get { return _PD_B_ChannelModel; }
            set
            {
                _PD_B_ChannelModel = value;
                OnPropertyChanged_Normal("PD_B_ChannelModel");


            }
        }

        private ChannelModel _Golight_ChannelModel = new ChannelModel()
        {
            Board_Port = "COM3",
            BautRate = 115200
        };
        public ChannelModel Golight_ChannelModel
        {
            get { return _Golight_ChannelModel; }
            set
            {
                _Golight_ChannelModel = value;
                OnPropertyChanged_Normal("Golight_ChannelModel");
            }
        }

        private ObservableCollection<ChannelModel> _list_ChannelModels = new ObservableCollection<ChannelModel>() {
            new ChannelModel() { channel="Ch 1", BautRate=115200, Board_Port="COM1"} };
        public ObservableCollection<ChannelModel> list_ChannelModels
        {
            get { return _list_ChannelModels; }
            set
            {
                _list_ChannelModels = value;
                OnPropertyChanged_Normal("list_ChannelModels");
            }
        }

        private ObservableCollection<VariableModel> _list_VariableModels = new ObservableCollection<VariableModel>();
        public ObservableCollection<VariableModel> list_VariableModels
        {
            get { return _list_VariableModels; }
            set
            {
                _list_VariableModels = value;
                OnPropertyChanged_Normal("list_VariableModels");
            }
        }

        private ObservableCollection<StringModel> _list_StringModels = new ObservableCollection<StringModel>();
        public ObservableCollection<StringModel> list_StringModels
        {
            get { return _list_StringModels; }
            set
            {
                _list_StringModels = value;
                OnPropertyChanged_Normal("list_StringModels");
            }
        }

        private ObservableCollection<VariableModel> _list_VarBoolModels = new ObservableCollection<VariableModel>();
        public ObservableCollection<VariableModel> list_VarBoolModels
        {
            get { return _list_VarBoolModels; }
            set
            {
                _list_VarBoolModels = value;
                OnPropertyChanged_Normal("list_VarBoolModels");
            }
        }

        private ObservableCollection<ChartModel> _list_ChartModels = new ObservableCollection<ChartModel>() { };
        public ObservableCollection<ChartModel> list_ChartModels
        {
            get { return _list_ChartModels; }
            set
            {
                _list_ChartModels = value;
                OnPropertyChanged("list_ChartModels");
            }
        }

        private ObservableCollection<Chart_UI_Model> _list_Chart_UI_Models = new ObservableCollection<Chart_UI_Model>() { };
        public ObservableCollection<Chart_UI_Model> list_Chart_UI_Models
        {
            get { return _list_Chart_UI_Models; }
            set
            {
                _list_Chart_UI_Models = value;
                OnPropertyChanged("list_Chart_UI_Models");
            }
        }

        public List<KModel> kModels { get; set; } = new List<KModel>();

        private ObservableCollection<GaugeModel> _list_GaugeModels = new ObservableCollection<GaugeModel>() { new GaugeModel() { GaugeChannel = "1", GaugeMode = Visibility.Collapsed } };
        public ObservableCollection<GaugeModel> list_GaugeModels
        {
            get { return _list_GaugeModels; }
            set
            {
                _list_GaugeModels = value;
                OnPropertyChanged("list_GaugeModels");
            }
        }

        //public OC_GaugeModels OC_Gauges { get; set; }

        private List<ObservableCollection<GaugeModel>> _list_collection_GaugeModels = new List<ObservableCollection<GaugeModel>>();
        public List<ObservableCollection<GaugeModel>> list_collection_GaugeModels
        {
            get { return _list_collection_GaugeModels; }
            set
            {
                _list_collection_GaugeModels = value;
            }
        }

        //public Dictionary<int, ObservableCollection<GaugeModel>> DT_GaugeModels { get; set; } = new Dictionary<int, ObservableCollection<GaugeModel>>();

        private ObservableCollection<BoardTable_Members> _memberBoardDatas = new ObservableCollection<BoardTable_Members>();
        public ObservableCollection<BoardTable_Members> memberBoardDatas
        {
            get { return _memberBoardDatas; }
            set
            {
                _memberBoardDatas = value;
                OnPropertyChanged("memberBoardDatas");
            }
        }

        public ObservableCollection<LogMember> LogMembers = new ObservableCollection<LogMember>();
        public ObservableCollection<SN_Member> SNMembers;

        public ObservableCollection<DeltaILMember> DeltaILMembers { get; set; } = new ObservableCollection<DeltaILMember>();

        private static readonly object key = new object();
        private ObservableCollection<ComMember> _ComMembers = new ObservableCollection<ComMember>();
        public ObservableCollection<ComMember> ComMembers
        {
            get
            {
                lock (key)
                    return _ComMembers;
            }
            set
            {
                lock (key)
                    _ComMembers = value;
            }
        }



        //public ComMember comMember_copy { get; set; }

        private static readonly object key_ComMembers_Spare = new object();
        private ObservableCollection<ComMember> _ComMembers_Spare = new ObservableCollection<ComMember>();
        public ObservableCollection<ComMember> ComMembers_Spare
        {
            get
            {
                lock (key_ComMembers_Spare)
                {
                    return _ComMembers_Spare;
                }
            }
            set
            {
                lock (key_ComMembers_Spare)
                {
                    _ComMembers_Spare = value;
                }
            }
        }
        #endregion

        #region Calibration Model
        private ChartModel _ChartNowModel = new ChartModel(8);
        public ChartModel ChartNowModel
        {
            get { return _ChartNowModel; }
            set
            {
                _ChartNowModel = value;
                OnPropertyChanged_Normal("ChartNowModel");
            }
        }

        public OpticalPropertyModel opModel_1 { get; set; } = new OpticalPropertyModel();
        public PropertyInfo[] props_opModel;

        public KModel kModel { get; set; } = new KModel();
        public GetPowerSettingModel GetPWSettingModel = new GetPowerSettingModel();
        #endregion

        ControlCmd cmd;

        string ini_path = @"D:\PD\Instrument.ini";
        public string CurrentPath { get; set; } = Directory.GetCurrentDirectory();

        //ICommunication icomm;
        //DiCon.UCB.Communication.RS232.RS232 rs232;
        //DiCon.UCB.MTF.IMTFCommand tf;

        //ICommand
        #region ICommand

        public ICommand BearTestCommand { get { return new Delegatecommand(BearTest); } }

        public ICommand Cmd_Test { get { return new Delegatecommand(cmd_test); } }
        #endregion

        //Commands
        #region Commands
        private async void CommandListCycle()
        {
            while (IsGoOn && !isStop)
            {
                try
                {
                    #region Auto Add/Remove Command
                    if (ComMembers.Count < 4)   //資料若大於N則不再增加資料
                    {
                        if (!PD_or_PM)  //PD mode
                        {
                            Save_Command(Cmd_Count++, "P0?");
                            Save_cmd(new ComMember() { No = Cmd_Count++.ToString(), Command = "P0?" });
                            await AccessDelayAsync(20);
                        }
                        else  //PM mode
                        {
                            Save_cmd(new ComMember() { YN = true, No = Cmd_Count++.ToString(), Command = "GETPOWER", Type = "PM", Channel = "1" });
                            await AccessDelayAsync(20);
                        }
                    }

                    if (isDeleteCmd || ComMembers.Count >= 4)
                    {
                        int a = ComMembers.Count;
                        if (a > 2)
                            ComMembers.RemoveAt(ComMembers.Count - 1);
                        isDeleteCmd = false;
                    }
                    #endregion
                }
                catch { }
            }
        }
        //Chamber_S_16ch
        public async void CommandListCycle_16ch()
        {
            while (IsGoOn && !isStop)
            {
                try
                {
                    if (ComMembers.Count < 4)   //資料若大於N則不再增加資料
                    {
                        Save_cmd(new ComMember() { Command = "P0?", No = (Cmd_Count + 1).ToString(), Comport = PD_A_ChannelModel.Board_Port, Type = "PD", Channel = "16" });
                        Save_cmd(new ComMember() { Command = "P0?", No = (Cmd_Count + 1).ToString(), Comport = PD_B_ChannelModel.Board_Port, Type = "PD", Channel = "16" });
                        await AccessDelayAsync(Int_Read_Delay + 20);
                    }

                    if (isDeleteCmd || ComMembers.Count >= 4)
                    {
                        int a = ComMembers.Count;
                        if (a > 2)
                            ComMembers.RemoveAt(ComMembers.Count - 1);
                        isDeleteCmd = false;
                    }
                }
                catch
                {

                }
            }
        }

        public string[] Get_BoardRatio_Database(string board_no, string dataBase, string server_IP, int channel)
        {
            try
            {
                string connstring = "User ID=" + "opticomm_pe" + ";" +
                                            "Password=" + "opticomm_pe!@#456" + ";" +
                                            "Trusted_Connection=false;" +
                                            "Server=" + "OPTICOMM-MFG" + ";" +
                                            "Data Source=" + server_IP + ";" +
                                            "Initial Catalog=" + dataBase + ";Pooling=false;Connection Timeout=2";  //DataBase： "UFA", "CTF"為不同的資料庫

                string tableName = "Board_V";
                string boardSN = board_no;   //板號 ex: "U4V35"
                string sql = "SELECT [Board_SN],[V1],[V2] FROM [dbo]." + tableName + " WHERE [Board_SN]= '" + boardSN + "'";

                DataSet ds = new DataSet();
                SqlConnection connection = new SqlConnection(connstring);
                SqlDataAdapter dataAdapter = new SqlDataAdapter(sql, connection);
                Console.WriteLine("ConnectionTimeout: {0}",
            connection.ConnectionTimeout);
                connection.Open();
                dataAdapter.Fill(ds, tableName);
                connection.Close();
                connection = null;

                if (ds.Tables[0].Rows.Count > 0)
                {
                    //vm.BoardTable_Dictionary.Clear();
                    for (int i = 0; i < ds.Tables[0].Rows.Count;)
                    {
                        string board_SN, V1_Ratio, V2_Ratio;
                        board_SN = ds.Tables[0].Rows[i]["Board_SN"].ToString().Trim();
                        V1_Ratio = ds.Tables[0].Rows[i]["V1"].ToString().Trim();
                        V2_Ratio = ds.Tables[0].Rows[i]["V2"].ToString().Trim();

                        return new string[] { V1_Ratio, V2_Ratio };

                        //vm.BoardTable_Dictionary.Add(board_SN,
                        //    new List<string>() { V1_Ratio, V2_Ratio });
                    }
                }
            }
            catch
            {
                Save_Log(new LogMember()
                {
                    Status = "Get_BoardRatio_Database",
                    Message = "SqlConnection error",
                    isShowMSG = false,
                    Channel = channel.ToString()
                });
            }

            return new string[] { "0.00068665598", "0.00068665598" };  //若資料庫中無此板號
        }

        public void Set_StationType(string station_type)
        {
            if (string.IsNullOrEmpty(station_type)) return;
            Ini_Write("Connection", "Station", station_type);
            if (station_type.Equals("Hermetic_Test") || station_type.Equals("Hermetic Test"))
            {
                if (int.TryParse(Ini_Read("Connection", "Hermetic_ch_count"), out int i))
                    ch_count = i;
                else ch_count = 12;

                BoudRate = 115200;
                Bool_Gauge = new bool[] { false, false, false, false, false, false, false, false, false, false, false, false };
                bo_temp_gauge = new bool[] { true, true, true, true, true, true, true, true, true, true, true, true };

                GaugeText_visible = Visibility.Hidden;
                GaugeTabEnable = true;
                GaugeSize_Height = new GridLength(6, GridUnitType.Star);
                GaugeTxtSize_Height = new GridLength(1, GridUnitType.Star);

                GaugeChart_visible = Visibility.Collapsed;

                PD_or_PM = true;
                Is_switch_mode = true;

                foreach (GaugeModel gm in _list_GaugeModels)
                {
                    gm.SN_Row = 1;
                    gm.GaugeMode = Visibility.Visible;
                }

                is_update_chart = false;
            }
            else if (station_type.Equals("Testing") || station_type.Equals("TF2"))
            {
                PD_or_PM = true;
                BoudRate = 115200;
                ch_count = 1;
                switch_index = 1;
                Is_switch_mode = false;
                GaugeText_visible = Visibility.Visible;
                GaugeTabEnable = false;
                GaugeSize_Height = new GridLength(7, GridUnitType.Star);
                GaugeTxtSize_Height = new GridLength(0, GridUnitType.Star);

                GaugeGrid1_visible = Visibility.Collapsed;  //Only Channel 1 will Show!!
                GaugeGrid2_visible = Visibility.Collapsed;
                GaugeGrid3_visible = Visibility.Collapsed;

                GaugeChart_visible = Visibility.Visible;

                foreach (GaugeModel gm in _list_GaugeModels)
                {
                    gm.SN_Row = 2;
                    gm.GaugeMode = Visibility.Collapsed;
                }

                is_update_chart = true;
            }
            else if (station_type.Equals("UV_Curing") || station_type.Equals("UV Curing"))
            {
                PD_or_PM = true;
                //BoudRate = 9600;
                ch_count = 1;
                switch_index = 1;
                Is_switch_mode = false;
                GaugeText_visible = Visibility.Visible;
                GaugeTabEnable = false;
                GaugeSize_Height = new GridLength(7, GridUnitType.Star);
                GaugeTxtSize_Height = new GridLength(0, GridUnitType.Star);

                GaugeGrid1_visible = Visibility.Collapsed;  //Only Channel 1 will Show!!
                GaugeGrid2_visible = Visibility.Collapsed;
                GaugeGrid3_visible = Visibility.Collapsed;

                GaugeChart_visible = Visibility.Visible;

                ComMembers.Clear();
                cmd_SelectedSource = new ObservableCollection<string>() { "SetPower", "SetTimer", "Start", "Stop" };

                Cmd_Count = 0;

                Save_Command(Cmd_Count++, "UV_Curing", "", Selected_Comport, "1", "UVSETPOW", "40", "", "", "", " ", "Set CH1 Power to 40%");
                Save_Command(Cmd_Count++, "UV_Curing", "", Selected_Comport, "2", "UVSETPOW", "30", "", "", "", " ", "Set CH2 Power to 30%");
                Save_Command(Cmd_Count++, "UV_Curing", "", Selected_Comport, "1", "UVSETTIMER", "5", "", "", "", " ", "Set CH1 Timer to 5S");
                Save_Command(Cmd_Count++, "UV_Curing", "", Selected_Comport, "2", "UVSETTIMER", "5", "", "", "", " ", "Set CH2 Timer to 5S");

                Save_Command(Cmd_Count++, "UV_Curing", "", Selected_Comport, "0", "UVSTART", "", "", "", "", " ", "Start all channel");
                Save_Command(Cmd_Count++, "UV_Curing", "", Selected_Comport, "0", "UVSTOP", "", "", "", "", " ", "Stop all channel");

                foreach (GaugeModel gm in _list_GaugeModels)
                {
                    gm.SN_Row = 2;
                    gm.GaugeMode = Visibility.Collapsed;
                }

                is_update_chart = true;
            }
            else if (station_type.Equals("Chamber_S_16ch"))
            {
                PD_or_PM = false;
                BoudRate = 115200;
                ch_count = 16;
                PD_A_ChannelModel.Board_Port = Ini_Read("Connection", "COM_PD_A");
                PD_B_ChannelModel.Board_Port = Ini_Read("Connection", "COM_PD_B");

                if (!string.IsNullOrEmpty(PD_B_ChannelModel.Board_Port))
                    port_PD_B = new SerialPort(PD_B_ChannelModel.Board_Port, 115200, Parity.None, 8, StopBits.One);

                Save_Log("Get COM A", PD_A_ChannelModel.Board_Port, false);
                Save_Log("Get COM B", PD_B_ChannelModel.Board_Port, false);
                Is_switch_mode = false;
                GaugeText_visible = Visibility.Visible;
                GaugeTabEnable = false;
                GaugeSize_Height = new GridLength(7, GridUnitType.Star);
                GaugeTxtSize_Height = new GridLength(0, GridUnitType.Star);

                GaugeGrid1_visible = Visibility.Visible;
                GaugeGrid2_visible = Visibility.Visible;
                GaugeGrid3_visible = Visibility.Collapsed;

                GaugeChart_visible = Visibility.Collapsed;

                foreach (GaugeModel gm in _list_GaugeModels)
                {
                    gm.SN_Row = 2;
                    gm.GaugeMode = Visibility.Collapsed;
                }

                is_update_chart = true;
            }
            else if (station_type == "Fast_Calibration")
            {
                PD_or_PM = false;
                BoudRate = 115200;
                ch_count = 8;
                Is_switch_mode = false;
                GaugeText_visible = Visibility.Visible;
                GaugeTabEnable = false;
                GaugeSize_Height = new GridLength(7, GridUnitType.Star);
                GaugeTxtSize_Height = new GridLength(0, GridUnitType.Star);

                GaugeGrid1_visible = Visibility.Visible;
                GaugeGrid2_visible = Visibility.Visible;
                GaugeGrid3_visible = Visibility.Collapsed;

                GaugeChart_visible = Visibility.Collapsed;

                foreach (GaugeModel gm in _list_GaugeModels)
                {
                    gm.SN_Row = 2;
                    gm.GaugeMode = Visibility.Collapsed;
                }

                is_update_chart = true;
            }
            else
            {
                PD_or_PM = false;
                BoudRate = 115200;
                ch_count = 8;
                Is_switch_mode = false;
                GaugeText_visible = Visibility.Visible;
                GaugeTabEnable = false;
                GaugeSize_Height = new GridLength(7, GridUnitType.Star);
                GaugeTxtSize_Height = new GridLength(0, GridUnitType.Star);

                GaugeGrid1_visible = Visibility.Visible;
                GaugeGrid2_visible = Visibility.Visible;
                GaugeGrid3_visible = Visibility.Collapsed;

                GaugeChart_visible = Visibility.Collapsed;

                foreach (GaugeModel gm in _list_GaugeModels)
                {
                    gm.SN_Row = 2;
                    gm.GaugeMode = Visibility.Collapsed;
                }

                is_update_chart = true;
            }
        }

        private void BearTest()
        {
            #region Get Board Name
            //rs232 = new DiCon.UCB.Communication.RS232.RS232(Selected_Comport);
            //rs232.OpenPort();
            //icomm = (ICommunication)rs232;

            //tf = new DiCon.UCB.MTF.RS232.RS232(icomm);

            //string str_ID = string.Empty;
            //try
            //{
            //    str_ID = tf.ReadSN();
            //    Str_cmd_read = str_ID;               
            //    rs232.ClosePort();
            //}
            //catch { }
            #endregion

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

            //Collection_bear_say.Add(lls);
            bear_say_all++;
            bear_say_now = bear_say_all;

            TraversalRequest tRequest = new TraversalRequest(FocusNavigationDirection.Next);
            System.Windows.UIElement keyboardFocus = Keyboard.FocusedElement as System.Windows.UIElement;

            if (keyboardFocus != null)
            {
                keyboardFocus.MoveFocus(tRequest);
            }
        }

        private Task<string> testing = Task<string>.Factory.StartNew(() =>
        {
            return "AAA";
        });

        private void test()
        {
            MessageBox.Show(testing.Result);
        }

        public void Save_Log<T>(T msg)
        {
            LogMembers.Add(new LogMember()
            {
                Message = msg.ToString()
            });
            Str_cmd_read = msg.ToString();
        }

        public void Save_Log(LogMember lm)
        {
            if (lm.isShowMSG)
            {
                Str_Status = lm.Status;
                Str_cmd_read = lm.Message.ToString();

                if (lm.TimeSpan != null)
                    msgModel.msg_3 = lm.TimeSpan;

                Show_Bear_Window(Str_cmd_read);
            }

            LogMembers.Add(lm);
        }

        public void Save_Log<T>(string status, T msg, bool showMSG)
        {
            if (showMSG)
                Str_cmd_read = msg.ToString();
            LogMembers.Add(new LogMember()
            {
                Status = status,
                Message = msg.ToString(),
                Date = DateTime.Now.Date.ToShortDateString(),
                Time = DateTime.Now.ToLongTimeString()
            });
        }

        public void Save_Log<T>(string status, T ch, T msg)
        {
            LogMembers.Add(new LogMember()
            {
                Status = status,
                Channel = ch.ToString(),
                Message = msg.ToString(),
                Date = DateTime.Now.Date.ToShortDateString(),
                Time = DateTime.Now.ToLongTimeString()
            });
        }

        public void Save_Log<T>(string status, T ch, T msg, T result)
        {
            LogMembers.Add(new LogMember()
            {
                Status = status,
                Channel = ch.ToString(),
                Message = msg.ToString(),
                Result = result.ToString(),
                Date = DateTime.Now.Date.ToShortDateString(),
                Time = DateTime.Now.ToLongTimeString()
            });
        }

        public int Cmd_Count { get; set; } = 0;

        public bool BoolAllGauge { get; set; } = true;

        public void Save_Spare_Command<T>(string status, string type, string command, string comport, T No)
        {
            ComMembers_Spare.Insert(0, new ComMember()
            {
                YN = true,
                No = No.ToString(),
                Status = status,
                Type = type,
                Command = command,
                Comport = comport
            });
        }

        public void Save_cmd(ComMember cm)
        {
            ComMembers.Insert(0, cm);
        }

        public void Save_Command<T>(T No, string command)
        {
            if (station_type == "UV_Curing")
            {
                ComMembers.Add(new ComMember()
                {
                    YN = true,
                    No = No.ToString(),
                    Command = command
                });
            }
            else
                ComMembers.Insert(0, new ComMember()
                {
                    YN = true,
                    No = No.ToString(),
                    Command = command
                });
        }

        public void Save_Command<T>(string status, string type, string command, string comport)
        {
            ComMembers.Insert(0, new ComMember()
            {
                YN = true,
                Status = status,
                Type = type,
                Command = command,
                Comport = comport
            });
        }

        public void Save_Command<T>(string status, string type, string command, string comport, T No)
        {
            if (station_type == "UV_Curing")
            {
                ComMembers.Add(new ComMember()
                {
                    YN = true,
                    No = No.ToString(),
                    Status = status,
                    Type = type,
                    Command = command,
                    Comport = comport

                });
            }
            else
            {
                ComMembers.Insert(0, new ComMember()
                {
                    YN = true,
                    No = No.ToString(),
                    Status = status,
                    Type = type,
                    Command = command,
                    Comport = comport
                });
            }
        }

        public void Save_Command<T>(string status, string type, string command, string comport, T No, string description)
        {
            if (station_type == "UV_Curing")
            {
                ComMembers.Add(new ComMember()
                {
                    YN = true,
                    No = No.ToString(),
                    Status = status,
                    Type = type,
                    Command = command,
                    Comport = comport,
                    Description = description
                });
            }
            else
            {
                ComMembers.Insert(0, new ComMember()
                {
                    YN = true,
                    No = No.ToString(),
                    Status = status,
                    Type = type,
                    Command = command,
                    Comport = comport,
                    Description = description
                });
            }
        }

        public void Save_Command(int No, string status, string type, string comport, string Ch, string command, string value1, string value2, string value3, string value4, string read, string description)
        {
            if (station_type == "UV_Curing")
            {
                ComMembers.Add(new ComMember()
                {
                    YN = true,
                    No = No.ToString(),
                    Status = status,
                    Type = type,
                    Comport = comport,
                    Channel = Ch.ToString(),
                    Command = command,
                    Value_1 = value1.ToString(),
                    Value_2 = value2.ToString(),
                    Value_3 = value3.ToString(),
                    Value_4 = value4.ToString(),
                    Read = read,
                    Description = description
                });
            }
            else
            {
                try
                {
                    ComMembers.Insert(0, new ComMember()
                    {
                        YN = true,
                        No = No.ToString(),
                        Status = status,
                        Type = type,
                        Comport = comport,
                        Channel = Ch.ToString(),
                        Command = command,
                        Value_1 = value1.ToString(),
                        Value_2 = value2.ToString(),
                        Value_3 = value3.ToString(),
                        Value_4 = value4.ToString(),
                        Read = read,
                        Description = description
                    });
                }
                catch { Save_Log("Add command to list error"); }
            }
        }

        public void Save_Command_Read(string read)
        {
            ComMembers.Insert(0, new ComMember()
            {
                YN = true,
                Read = read
            });
        }

        public void Save_Command_Read<T>(string command, T no, string read)
        {
            ComMembers.Insert(0, new ComMember()
            {
                YN = true,
                No = no.ToString(),
                Command = command,
                Read = read
            });
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
            Double_Powers.Clear();

            ch++;

            float y = Convert.ToSingle(power_PM);
            float z = (y * 300 / -64 - 150) * -1;
            z = z != 1350 ? z : 150;

            if (z < -150)
                z = -150;

            if (ch < 13) //Switch mode  1~12
            {
                Value_PD = new List<float>() { -150, -150, -150, -150, -150, -150, -150, -150, -150, -150, -150, -150 };
                Double_Powers = Analysis.ListDefault<double>(_ch_count);

                Value_PD[ch - 1] = z;
                Double_Powers[ch - 1] = y;
                Value_PD = new List<float>(Value_PD);

                Str_PD = Analysis.ListDefault<string>(_ch_count);
                Str_PD[ch - 1] = (Math.Round(y, 4)).ToString();
                Str_PD = new List<string>(Str_PD);
            }
            else  //Normal mode
            {
                Value_PD.Add(z);  //-150~150 degree, for gauge binding
                Double_Powers.Add(Math.Round(y, 4));  //list 0~-64dBm in float type

                Str_PD = new List<string>() { (Math.Round(y, 4)).ToString() };
            }

            Value_PD = new List<float>(Value_PD);
            Double_Powers = new List<double>(Double_Powers);
        }

        public void Show_Bear_Window(string msg)
        {
            Winbear = new Window_Bear(this, false, "String");
            BearBtn_visibility = Visibility.Collapsed;
            BearSay_RowSpan = 3;
            Str_bear_say = msg;
            Winbear.Show();
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

            if (bear_say.GetType().Name.Equals("String"))
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
            try
            {
                if (port_PD != null)
                {
                    if (port_PD.IsOpen)
                    {
                        if (port_PD.PortName != comport)
                        {
                            port_PD.DiscardInBuffer();       // RX
                            port_PD.DiscardOutBuffer();      // TX
                            port_PD.Close();
                        }
                        else return;
                    }
                }
                else port_PD = new SerialPort(comport, _BoudRate, Parity.None, 8, StopBits.One);
            }
            catch { }

            try
            {
                if (port_PD.PortName != comport || port_PD.BaudRate != _BoudRate)
                    port_PD = new SerialPort(comport, _BoudRate, Parity.None, 8, StopBits.One);


                port_PD.Open();

                await Task.Delay(10);  //100ms
            }
            catch (Exception ex)
            {
                Str_cmd_read = "Port Open Error";
                throw ex;
            }
        }

        public async Task Port_ReOpen(SerialPort port, string comport, int boudRate)
        {
            try
            {
                if (port.PortName != comport)
                    port = new SerialPort(comport, boudRate, Parity.None, 8, StopBits.One);

                if (!port.IsOpen)
                    port_PD.Open();

                await Task.Delay(100);
            }
            catch (Exception ex) { Str_cmd_read = "Port Open Error"; cmd.Save_Log_Message("Connection", Str_cmd_read, DateTime.Now.ToLongTimeString()); throw ex; }

        }

        public async Task Port_PD_B_ReOpen(string comport)
        {
            try
            {
                if (port_PD_B != null)
                {
                    if (port_PD_B.IsOpen)
                    {
                        if (port_PD_B.PortName != comport)
                        {
                            port_PD_B.DiscardInBuffer();       // RX
                            port_PD_B.DiscardOutBuffer();      // TX
                            port_PD_B.Close();
                        }
                        else return;
                    }
                }
                else port_PD_B = new SerialPort(comport, _BoudRate, Parity.None, 8, StopBits.One);
            }
            catch { }

            try
            {
                if (port_PD_B.PortName != comport || port_PD_B.BaudRate != _BoudRate)
                    port_PD_B = new SerialPort(comport, _BoudRate, Parity.None, 8, StopBits.One);

                port_PD_B.Open();

                await Task.Delay(100);
            }
            catch (Exception ex) { Str_cmd_read = "Port Open Error"; cmd.Save_Log_Message("Connection", Str_cmd_read, DateTime.Now.ToLongTimeString()); throw ex; }
        }

        public List<SerialPort> List_Port = new List<SerialPort>();
        public void Multi_Port_Setting()
        {
            try
            {
                List_Port = new List<SerialPort>();
                if (list_Board_Setting.Count > 0)
                {
                    foreach (List<string> board_setting in list_Board_Setting)
                    {
                        if (!string.IsNullOrEmpty(board_setting[1]))
                        {
                            SerialPort port = new SerialPort(board_setting[1], _BoudRate, Parity.None, 8, StopBits.One);
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
                        if (!port_Switch.PortName.Equals(comport_switch))
                        {
                            port_Switch.DiscardInBuffer();       // RX
                            port_Switch.DiscardOutBuffer();      // TX

                            port_Switch.Close();

                            await Task.Delay(30);

                            port_Switch.PortName = comport_switch;
                            port_Switch.BaudRate = _Switch_BoudRate;
                            port_Switch.Parity = Parity.None;
                            port_Switch.StopBits = StopBits.One;

                            port_Switch.Open();
                        }

                        return;
                    }
                    else
                    {

                        port_Switch.PortName = comport_switch;
                        port_Switch.BaudRate = _Switch_BoudRate;
                        port_Switch.Parity = Parity.None;
                        port_Switch.StopBits = StopBits.One;
                        port_Switch.Open();
                    }
                }
                else
                {
                    try
                    {
                        port_Switch = new SerialPort("COM" + comport_switch.ToString(), _BoudRate, Parity.None, 8, StopBits.One);
                        port_Switch.Open();
                    }
                    catch
                    {
                        Str_cmd_read = "Switch Port Open Error";
                        return;
                    }
                }
            }
            catch
            {
                port_Switch = new SerialPort(comport_switch, _BoudRate, Parity.None, 8, StopBits.One);
                port_Switch.Open();
            }
        }

        public async Task<string> PD_GO()
        {
            try
            {
                if (port_PD != null)
                {
                    Clean_Chart();

                    Str_Command = "P0?";

                    if (station_type == "Chamber_S_16ch")
                    {
                        await Port_ReOpen(PD_A_ChannelModel.Board_Port);
                        await Port_ReOpen(PD_B_ChannelModel.Board_Port);
                    }
                    else
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
                Save_All_PD_Value = new List<List<DataPoint>>() { new List<DataPoint>() };
                try { timer3.Start(); }
                catch { Save_Log("Timer Start", "GPIB error", true); }
            }
        }

        public async Task PM_Stop()
        {
            try
            {
                timer3.Stop();

                await Task.Delay(200);

                timer3_count = 0;
            }
            catch
            {
                Str_cmd_read = "GPIB error";
            }
            await Task.Delay(_int_Read_Delay * 2);
        }

        public async Task WriteDac<T>(string ch, string TF_or_VOA, T DAC)
        {
            string cmd = "";
            if (_station_type.Equals("Hermetic_Test"))
                await Port_ReOpen(list_Board_Setting[int.Parse(ch) - 1][1]);
            else
            {
                if (!PD_or_PM && _isGoOn)
                {

                }
                else await Port_ReOpen(_Selected_Comport);
            }

            if (PD_or_PM == false)  //PD mode
                cmd = TF_or_VOA + ch + " " + DAC;  //Write Dac
            else  //PM mode
            {
                string sdac = DAC.ToString();

                string[] array_dac = sdac.Split(',');

                if (array_dac.Length == 1)
                {
                    int dac = int.Parse(sdac);

                    if (TF_or_VOA == "D")
                    {
                        if (!Control_board_type.Equals(1))
                        {
                            if (dac >= 0)
                                cmd = "D1 " + DAC + ",0,0";  //Write Dac. For PM, always D1 XXX
                            else
                                cmd = "D1 0," + Math.Abs(dac) + ",0";  //Write Dac. For PM, always D1 XXX
                        }
                        else
                        {
                            if (dac >= 0)
                                cmd = "D1 " + DAC + ",0";  //Write Dac. For PM, always D1 XXX
                            else
                                cmd = "D1 0," + Math.Abs(dac);  //Write Dac. For PM, always D1 XXX
                        }
                    }
                    else
                        cmd = "D1 0,0," + Math.Abs(dac);  //Write Dac. For PM, always D1 XXX
                }
                else
                {
                    cmd = "D1 " + DAC;  //Write Dac. For PM, always D1 XXX
                }
            }

            try
            {
                if (!PD_or_PM && _isGoOn)
                {
                    Save_Command(Cmd_Count++, cmd);
                    //Save_Command(Cmd_Count++, "D" + ch + "?");
                }
                else
                {
                    if (!port_PD.IsOpen)
                        port_PD.Open();

                    port_PD.Write(cmd + "\r");
                    //port_PD.Write("D1 5000\r");

                    await Task.Delay(Int_Write_Delay);

                    //port_PD.Close();
                }
            }
            catch
            {
                Save_Log("Write Dac", "Write Dac Error", true);
            }

            //Gauge keep going on
            if (IsGoOn)
            {
                try
                {
                    if (_pd_or_pm)
                    {
                        if (port_PD != null) Str_Command = "P0?";
                        timer3.Start();
                    }
                }
                catch
                {
                    Save_Log("Write Dac", "Port is closed", true);
                }
                await Task.Delay(1);
            }
        }

        public async void Cmd_WriteDac(string ch, string DAC1, string DAC2, string DAC3)
        {
            try
            {

                if (string.IsNullOrEmpty(DAC1) && string.IsNullOrEmpty(DAC2) && string.IsNullOrEmpty(DAC3))
                    return;

                int channel = int.Parse(ch);
                if (_station_type.Equals("Hermetic_Test"))
                    await Port_ReOpen(list_Board_Setting[int.Parse(ch) - 1][1]);
                else if (_station_type.Equals("Chamber_S_16ch"))
                {
                    if (int.Parse(ch) < 9)
                    {
                        if (!port_PD.IsOpen)
                            await Port_ReOpen(_PD_A_ChannelModel.Board_Port);
                    }
                    else if (int.Parse(ch) < 17 && int.Parse(ch) > 8)
                    {
                        channel -= 8;
                        if (!port_PD_B.IsOpen)
                            await Port_PD_B_ReOpen(_PD_B_ChannelModel.Board_Port);
                    }
                }
                else
                {
                    if (!port_PD.IsOpen)
                        await Port_ReOpen(_Selected_Comport);
                }

                if (!PD_or_PM)  //PD mode
                {
                    if(int.TryParse(DAC2, out int d2))
                    {
                        d2 = -1 * Math.Abs(d2);
                        Str_Command = "D" + channel.ToString() + " " + d2.ToString();  //Write Dac
                    }
                    else if(!string.IsNullOrEmpty(DAC1))
                    {
                        Str_Command = "D" + channel.ToString() + " " + DAC1;  //Write Dac
                    }

                    if (DAC3 != null)
                    {
                        string cd = "VOA" + channel.ToString() + " " + DAC3;  //Write Dac

                        if (_station_type.Equals("Chamber_S_16ch"))
                        {
                            if (int.Parse(ch) < 9)
                            {
                                port_PD.Write(cd + "\r");
                            }
                            else if (int.Parse(ch) < 17 && int.Parse(ch) > 8)
                            {
                                port_PD_B.Write(cd + "\r");
                            }
                        }
                        else
                            port_PD.Write(cd + "\r");

                        await Task.Delay(Int_Write_Delay);
                    }
                }
                else  //PM mode
                {
                    if (string.IsNullOrEmpty(DAC2)) DAC2 = "0";
                    if (string.IsNullOrEmpty(DAC3)) DAC3 = "0";
                    Str_Command = string.Concat("D", ch, " ", DAC1, ",", DAC2, ",", DAC3);  //Write Dac. For PM, always D1 XXX  
                }

                try
                {
                    if (_station_type.Equals("Chamber_S_16ch"))
                    {
                        if (int.Parse(ch) < 9)
                        {
                            port_PD.Write(Str_Command + "\r");
                        }
                        else if (int.Parse(ch) < 17 && int.Parse(ch) > 8)
                        {
                            port_PD_B.Write(Str_Command + "\r");
                        }
                    }
                    else
                        port_PD.Write(Str_Command + "\r");

                    //await Task.Delay(Int_Write_Delay);

                    Str_Status = "Set Dac Ch" + ch;
                    Str_cmd_read = Str_Command;
                }
                catch
                {
                    Save_Log("Write Dac", "Write Dac Error", false);
                }

            }
            catch { }
        }
        #endregion


        public SerialPort port_PD, port_PD_B, _port_Switch, port_TLS_Filter;

        private object key_port_Switch = new object();


        public SerialPort port_Switch
        {
            get
            {
                lock (key_port_Switch) return _port_Switch;
            }
            set
            {
                lock (key_port_Switch) _port_Switch = value;
            }
        }

        #region BackgroundWorkers
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
                port_Switch = new SerialPort(Comport_Switch, _BoudRate, Parity.None, 8, StopBits.One);
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
        #endregion

        public HPPM pm = new HPPM();

        public ComMember lastCMD = new ComMember();

        public int CMDStartIndex { get; set; } = 0;

        public List<string> _write_line { get; set; }

        public List<double> list_ch_power { get; set; } = new List<double>();
        public List<double> list_ch_X { get; set; } = new List<double>();

        public Page_Command _Page_Command { get; set; }
        public string pageName_LogCmd { get; set; } = "page_commandList";
        public bool isDeleteCmd { get; set; } = false;
        public double max_Y { get; set; } = 0;
        public int loop_start_index { get; set; }
        public int loop_end_index { get; set; }
        public int loop_total_cycle { get; set; }
        public int loop_cycle_count { get; set; }

        private int _CmdSelected_Index = 0;
        public int CmdSelected_Index
        {
            get { return _CmdSelected_Index; }
            set
            {
                _CmdSelected_Index = value;
                OnPropertyChanged("CmdSelected_Index");
            }
        }

        public string LastScript_Path { get; set; }
      
        public bool is_BearSay_History_Loaded { get; set; }

        public MsgModel msgModel = new MsgModel() { msg_1 = "", msg_2 = "", msg_3 = "" };

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

        public Dictionary<string, List<string>> BoardTable_Dictionary { get; set; } = new Dictionary<string, List<string>>();
        public Dictionary<string, List<string>> Dictionary_Product_WL_Setting { get; set; } = new Dictionary<string, List<string>>();

        public string Station_ID { get; set; } = "S00";

        private ObservableCollection<FastCalibrationStatusModel> _List_FastCalibration_Status =
            new ObservableCollection<FastCalibrationStatusModel>() { new FastCalibrationStatusModel() };
        public ObservableCollection<FastCalibrationStatusModel> List_FastCalibration_Status
        {
            get { return _List_FastCalibration_Status; }
            set
            {
                _List_FastCalibration_Status = value;
                OnPropertyChanged("List_FastCalibration_Status");
            }
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

        private string _csv_product_wl_setting_path = @"D:\Product_WL";
        public string csv_product_wl_setting_path
        {
            get { return _csv_product_wl_setting_path; }
            set
            {
                _csv_product_wl_setting_path = value;
                OnPropertyChanged("csv_product_wl_setting_path");
            }
        }

        private string _csv_product_TF2_wl_setting_path = @"D:\Product_TF2_WL";
        public string csv_product_TF2_wl_setting_path
        {
            get { return _csv_product_TF2_wl_setting_path; }
            set
            {
                _csv_product_TF2_wl_setting_path = value;
                OnPropertyChanged("csv_product_TF2_wl_setting_path");
            }
        }

        private string _txt_board_table_path = @"\\192.168.2.3\tff\Data\BoardCalibration\UFA\";
        public string txt_board_table_path
        {
            get { return _txt_board_table_path; }
            set
            {
                _txt_board_table_path = value;
                ini.IniWriteValue("Connection", "Control_Board_Table_Path", value.ToString(), ini_path);
                OnPropertyChanged("txt_board_table_path");
            }
        }

        private string _txt_save_wl_data_path = @"\\192.168.2.3\wdm_data\UFA HT\";
        public string txt_save_wl_data_path
        {
            get { return _txt_save_wl_data_path; }
            set
            {
                _txt_save_wl_data_path = value;
                ini.IniWriteValue("Connection", "Save_Hermetic_Data_Path", value.ToString(), ini_path);
                OnPropertyChanged("txt_save_wl_data_path");
            }
        }

        

        private string _txt_save_TF2_wl_data_path = @"\\192.168.2.4\OptiComm\tff\Data\TF2\data\";
        public string txt_save_TF2_wl_data_path
        {
            get { return _txt_save_TF2_wl_data_path; }
            set
            {
                _txt_save_TF2_wl_data_path = value;
                ini.IniWriteValue("Connection", "Save_TF2_Data_Path", value.ToString(), ini_path);
                OnPropertyChanged("txt_save_TF2_wl_data_path");
            }
        }

        private string _txt_Auto_Update_Path = @"\\192.168.2.3\shared\SeanWu\PB";
        public string txt_Auto_Update_Path
        {
            get { return _txt_Auto_Update_Path; }
            set
            {
                _txt_Auto_Update_Path = value;
                ini.IniWriteValue("Connection", "Auto_Update_Path", value.ToString(), ini_path);
                OnPropertyChanged("txt_Auto_Update_Path");
            }
        }

        private string _txt_Chamber_Status_Path = @"\\192.168.2.3\shared\SeanWu\ChamberStatus";
        public string txt_Chamber_Status_Path
        {
            get { return _txt_Chamber_Status_Path; }
            set
            {
                _txt_Chamber_Status_Path = value;
                ini.IniWriteValue("Connection", "Chamber_Status_Path", value.ToString(), ini_path);
                OnPropertyChanged("txt_Chamber_Status_Path");
            }
        }

        private string _txt_Equip_Setting_Path = "";
        public string txt_Equip_Setting_Path
        {
            get { return _txt_Equip_Setting_Path; }
            set
            {
                _txt_Equip_Setting_Path = value;
                ini.IniWriteValue("Connection", "Equip_Setting_Path", value.ToString(), ini_path);
                OnPropertyChanged("txt_Equip_Setting_Path");
            }
        }

        private string _Server_IP = @"172.16.10.108";
        public string Server_IP
        {
            get { return _Server_IP; }
            set
            {
                _Server_IP = value;
                ini.IniWriteValue("Connection", "Server_IP", value.ToString(), ini_path);
                OnPropertyChanged("Server_IP");
            }
        }

        private string _txt_now_version = "5.5.5";
        public string txt_now_version
        {
            get { return _txt_now_version; }
            set
            {
                _txt_now_version = value;
                OnPropertyChanged("txt_now_version");
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

        private List<List<DataPoint>> _Save_All_PD_Value = new List<List<DataPoint>>(Analysis.ListDefault<List<DataPoint>>(16));
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

        private string comport_switch;
        public string Comport_Switch
        {
            get { return comport_switch; }
            set
            {
                comport_switch = value;
                Ini_Write("Connection", "Comport_Switch", value.ToString()); 
                OnPropertyChanged("Comport_Switch");

                if (port_Switch != null)
                {
                    if (!port_Switch.IsOpen)
                    {
                        port_Switch.PortName = Comport_Switch;
                    }
                }
            }
        }

        private int _Switch_BoudRate = 115200;
        public int Switch_BoudRate
        {
            get { return _Switch_BoudRate; }
            set
            {
                _Switch_BoudRate = value;
                OnPropertyChanged("Switch_BoudRate");
            }
        }

        private string comport_TLS_Filter;
        public string Comport_TLS_Filter
        {
            get { return comport_TLS_Filter; }
            set
            {
                comport_TLS_Filter = value;
                Ini_Write("Connection", "Comport_TLS_Filter", value.ToString());  //創建ini file並寫入基本設定
                OnPropertyChanged("Comport_TLS_Filter");

                if (port_TLS_Filter != null)
                {
                    if (!port_TLS_Filter.IsOpen)
                    {
                        port_TLS_Filter.PortName = Comport_TLS_Filter;
                    }
                }
            }
        }

        private int _TLS_Filter_BoudRate = 115200;
        public int TLS_Filter_BoudRate
        {
            get { return _TLS_Filter_BoudRate; }
            set
            {
                _TLS_Filter_BoudRate = value;
                OnPropertyChanged("TLS_Filter_BoudRate");
            }
        }

        private string _Comport_chamber;
        public string Comport_chamber
        {
            get { return _Comport_chamber; }
            set
            {
                _Comport_chamber = value;
                Ini_Write("Connection", "Comport_chamber", value.ToString());  //創建ini file並寫入基本設定
                OnPropertyChanged("Comport_chamber");
            }
        }

        private int _Chamber_BoudRate = 115200;
        public int Chamber_BoudRate
        {
            get { return _Chamber_BoudRate; }
            set
            {
                _Chamber_BoudRate = value;
                OnPropertyChanged("Chamber_BoudRate");
            }
        }

        //private List<string> _dateTime;
        //public List<string> dateTime
        //{
        //    get { return _dateTime; }
        //    set
        //    {
        //        _dateTime = value;
        //        OnPropertyChanged("dateTime");
        //    }
        //}

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

        //private double _BW_05 = 0;
        //public double BW_05
        //{
        //    get { return _BW_05; }
        //    set
        //    {
        //        _BW_05 = value;
        //        OnPropertyChanged("BW_05");
        //    }
        //}

        //private double _BW_1_5 = 0;
        //public double BW_1_5
        //{
        //    get { return _BW_1_5; }
        //    set
        //    {
        //        _BW_1_5 = value;
        //        OnPropertyChanged("BW_1_5");
        //    }
        //}

        //private double _BW_3 = 0;
        //public double BW_3
        //{
        //    get { return _BW_3; }
        //    set
        //    {
        //        _BW_3 = value;
        //        OnPropertyChanged("BW_3");
        //    }
        //}

        //private double _BW_20 = 0;
        //public double BW_20
        //{
        //    get { return _BW_20; }
        //    set
        //    {
        //        _BW_20 = value;
        //        OnPropertyChanged("BW_20");
        //    }
        //}

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

        private static readonly object key_list_D_All = new object();
        private ObservableCollection<ObservableCollection<string>> _list_D_All = new ObservableCollection<ObservableCollection<string>>();
        public ObservableCollection<ObservableCollection<string>> list_D_All
        {
            get { lock (key_list_D_All) { return _list_D_All; } }
            set
            {
                lock (key_list_D_All)
                {
                    _list_D_All = value;
                    //OnPropertyChanged("list_D_All");
                }
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

        //private ObservableCollection<Visibility> _lineseries_visibility = new ObservableCollection<Visibility>(new Visibility[12]);
        //public ObservableCollection<Visibility> LineSeries_Visible
        //{
        //    get { return _lineseries_visibility; }
        //    set
        //    {
        //        _lineseries_visibility = value;
        //        OnPropertyChanged("LineSeries_Visible");
        //    }
        //}

        private ObservableCollection<string> _list_ch_title = new ObservableCollection<string>() { "ch1", "ch2", "ch3", "ch4", "ch5", "ch6", "ch7", "ch8" };
        public ObservableCollection<string> list_ch_title
        {
            get { return _list_ch_title; }
            set
            {
                _list_ch_title = value;
                OnPropertyChanged("list_ch_title");
            }
        }

        private ObservableCollection<string> _cmd_SelectedSource = new ObservableCollection<string>();
        public ObservableCollection<string> cmd_SelectedSource
        {
            get { return _cmd_SelectedSource; }
            set
            {
                _cmd_SelectedSource = value;
                OnPropertyChanged_Normal("cmd_SelectedSource");
            }
        }

        private System.Windows.Visibility _typeGPIBVis = System.Windows.Visibility.Visible;
        public System.Windows.Visibility typeGPIBVis
        {
            get { return _typeGPIBVis; }
            set
            {
                _typeGPIBVis = value;
                OnPropertyChanged("typeGPIBVis");
            }
        }

        private string _Laser_type = "Agilent";
        public string Laser_type
        {
            get { return _Laser_type; }
            set
            {
                _Laser_type = value;
                OnPropertyChanged("Laser_type");
                //Ini_Write("Connection", "Laser_type", value);
            }
        }

        private string _station_type = "Testing";
        public string station_type
        {
            get { return _station_type; }
            set
            {
                _station_type = value;

                Set_StationType(value);

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
                if (!string.IsNullOrEmpty(value))
                {
                    if (value.Split(' ').Length > 1)
                    {
                        list_combox_comports.Remove(value);

                        _Selected_Comport = value.Split(' ')[0];

                        list_combox_comports.Add(_Selected_Comport);
                    }
                    else _Selected_Comport = value;

                    OnPropertyChanged("Selected_Comport");
                }
            }
        }

        private string _Selected_Arduino_Comport;
        public string Selected_Arduino_Comport
        {
            get { return _Selected_Arduino_Comport; }
            set
            {
                _Selected_Arduino_Comport = value;
                OnPropertyChanged("Selected_Arduino_Comport");
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


        private string _TF2_station_type = "Alignment";
        public string TF2_station_type
        {
            get { return _TF2_station_type; }
            set
            {
                _TF2_station_type = value;
                ini.IniWriteValue("Connection", "TF2_station_type", value.ToString(), ini_path);
                OnPropertyChanged("TF2_station_type");
            }
        }

        private ObservableCollection<string> _list_TF2_station_type = new ObservableCollection<string>() { "Alignment", "PreTest", "Test", "FinalTest" };
        public ObservableCollection<string> list_TF2_station_type
        {
            get { return _list_TF2_station_type; }
            set
            {
                _list_TF2_station_type = value;
                OnPropertyChanged("list_TF2_station_type");
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
            set
            {
                value = value < 1 ? 1 : value;
                if (value < 1) value = 1;
                _ch_count = value;
                
                if (station_type == "Hermetic_Test")
                {
                    if (value <= 4)
                    {
                        GaugeGrid1_visible = Visibility.Visible;
                        GaugeGrid2_visible = Visibility.Collapsed;
                        GaugeGrid3_visible = Visibility.Collapsed;
                    }
                    else if (value <= 8 && value > 4)
                    {
                        GaugeGrid1_visible = Visibility.Visible;
                        GaugeGrid2_visible = Visibility.Visible;
                        GaugeGrid3_visible = Visibility.Collapsed;
                    }
                    else
                    {
                        GaugeGrid1_visible = Visibility.Visible;
                        GaugeGrid2_visible = Visibility.Visible;
                        GaugeGrid3_visible = Visibility.Visible;
                    }

                    ini.IniWriteValue("Connection", "Hermetic_ch_count", value.ToString(), ini_path);
                }

                OnPropertyChanged("ch_count");

                try
                {
                    maxIL.Clear();
                    minIL.Clear();
                    list_GaugeModels.Clear();
                    Save_All_PD_Value.Clear();
                    Chart_All_DataPoints.Clear();
                    Double_Powers.Clear();
                    ChartNowModel.list_dataPoints.Clear();
                    ChartNowModel.list_delta_IL.Clear();
                    list_ChannelModels.Clear();
                    //IsCheck.Clear();
                    list_Chart_UI_Models.Clear();
                    board_read.Clear();

                    list_Board_Setting.Clear();

                    //(SolidColorBrush)(new BrushConverter().ConvertFrom("#FF858585"))
                    SolidColorBrush[] M_brushes = new SolidColorBrush[]
                        {
                        Brushes.Green, Brushes.Red, Brushes.Blue, Brushes.Orange,
                        Brushes.YellowGreen, Brushes.Purple, Brushes.Gray, Brushes.Chocolate,
                        Brushes.LightSeaGreen, Brushes.MediumVioletRed, Brushes.Coral, Brushes.BurlyWood,
                        Brushes.DarkKhaki, Brushes.DarkCyan, Brushes.MediumPurple, Brushes.OrangeRed,
                        };

                    for (int i = 0; i < _ch_count; i++)
                    {
                        list_GaugeModels.Add(new GaugeModel()
                        {
                            GaugeChannel = (i + 1).ToString(),
                            GaugeUnit = _dB_or_dBm ? "dB" : "dBm",
                            GaugeContinueSelect = false,
                            SN_Row = 1,
                            GaugeMode = Visibility.Visible
                        });
                        board_read.Add(new List<string>());
                        maxIL.Add(0);
                        minIL.Add(0);
                        Save_All_PD_Value.Add(new List<DataPoint>());
                        Chart_All_DataPoints.Add(new List<DataPoint>());
                        Double_Powers.Add(0);
                        ChartNowModel.list_dataPoints.Add(new List<DataPoint>());
                        ChartNowModel.list_delta_IL.Add(0);

                        string Board_ID = "Board_ID_" + (i + 1).ToString();
                        string Board_COM = "Board_COM_" + (i + 1).ToString();
                        list_Board_Setting.Add(new List<string>() { Ini_Read("Board_ID", Board_ID), Ini_Read("Board_Comport", Board_COM) });

                        if (list_Board_Setting.Count >= i)
                        {
                            list_ChannelModels.Add(new ChannelModel()
                            {
                                channel = string.Format("ch {0}", (i + 1)),
                                BautRate = 115200,
                                Board_ID = list_Board_Setting[i][0],
                                Board_Port = list_Board_Setting[i][1]
                            });
                        }
                        else
                        {
                            list_ChannelModels.Add(new ChannelModel()
                            {
                                channel = string.Format("ch {0}", (i + 1)),
                                BautRate = 115200
                            });
                        }


                        #region Get Board V1 V2 Ratio
                        if (!string.IsNullOrEmpty(list_GaugeModels[i].chModel.Board_ID))
                        {
                            string[] v12_ratio = Get_BoardRatio_Database(list_GaugeModels[i].chModel.Board_ID, "CTF", "172.16.10.108", i + 1);
                            if (v12_ratio.Length >= 2)
                            {
                                list_GaugeModels[i].chModel.Board_V1_Ratio
                                    = double.Parse(v12_ratio[0]);
                                list_GaugeModels[i].chModel.Board_V2_Ratio
                                    = double.Parse(v12_ratio[1]);
                            }
                        }
                        else
                        {
                            list_GaugeModels[i].chModel.Board_V1_Ratio = 0.00068665598;
                            list_GaugeModels[i].chModel.Board_V2_Ratio = 0.00068665598;
                        }


                        //if (!BoardTable_Dictionary.ContainsKey(list_GaugeModels[i].chModel.Board_Port))   //若資料庫中無此板號
                        //{
                        //    list_GaugeModels[i].chModel.Board_V1_Ratio = 0.00068665598;
                        //    list_GaugeModels[i].chModel.Board_V2_Ratio = 0.00068665598;
                        //}

                        #endregion

                        #region Get Board Table
                        int k = 0;
                        foreach (List<string> board_info in list_Board_Setting)
                        {
                            if (string.IsNullOrEmpty(board_info[0]))
                            {
                                k++; continue;
                            }

                            string board_id = board_info[0];
                            string path = Path.Combine(txt_board_table_path, board_id + "-boardtable.txt");

                            if (!File.Exists(path))
                            {
                                Str_cmd_read = "UFV Board table is not exist";
                                Save_Log("Get Board Table", (k + 1).ToString(), Str_cmd_read);
                                k++;
                                continue;
                            }

                            StreamReader str = new StreamReader(path);

                            while (true)  //Read board v3 data
                            {
                                string readline = str.ReadLine();

                                if (string.IsNullOrEmpty(readline)) break;

                                board_read[k].Add(readline);
                            }
                            str.Close(); //(關閉str)

                            k++;
                        }
                        #endregion

                        bool check = false;
                        if (i == 0)
                        {
                            //IsCheck.Add(true);
                            check = true;
                        }
                        //else
                        //{
                        //IsCheck.Add(false);
                        //}
                        //IsCheck = new List<bool>(IsCheck);

                        //list_Chart_UI_Models[i].Button_IsChecked = check;

                        list_Chart_UI_Models.Add(new Chart_UI_Model()
                        {
                            Button_Color = i < M_brushes.Count() ? M_brushes[i] : M_brushes[i - M_brushes.Count()],
                            Button_Channel = (i + 1),
                            Button_Content = string.Format("Ch {0}", (i + 1)),
                            Button_IsChecked = check,
                            Button_IsVisible = Visibility.Visible,
                            Button_Tag = "../../Resources/right-arrow.png"
                        });
                    }

                    for (int i = _ch_count; i < 16; i++)
                    {
                        list_Chart_UI_Models.Add(new Chart_UI_Model()
                        {
                            Button_Color = i < M_brushes.Count() ? M_brushes[i] : M_brushes[i - M_brushes.Count()],
                            Button_Channel = (i + 1),
                            Button_Content = string.Format("Ch {0}", (i + 1)),
                            Button_IsChecked = false,
                            Button_IsVisible = Visibility.Collapsed,
                            Button_Tag = "../../Resources/right-arrow.png"
                        });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.StackTrace.ToString());
                }

            }
        }

        private static ObservableCollection<string> _cmdBox = new ObservableCollection<string>() { "Delay", "WriteDac", "LOOP", "LOOPE", "GETPOWER", "MESSAGEBOX", "MAXPOWER", "SaveChart" };
        public static ObservableCollection<string> cmdBox
        {
            get { return _cmdBox; }
            set
            {
                _cmdBox = value;
            }
        }

        private List<string> _list_combox_TLS_WL_Range_items =
            new List<string>() { "Auto", "C Band", "L Band", "C+L Band", "O Band", "E Band", "S Band", "U Band" };
        public List<string> list_combox_TLS_WL_Range_items
        {
            get { return _list_combox_TLS_WL_Range_items; }
            set
            {
                _list_combox_TLS_WL_Range_items = value;
                OnPropertyChanged("list_combox_TLS_WL_Range_items");
            }
        }

        public enum_station e_stations = new enum_station();


        private List<string> _list_combox_Working_Table_Type_items =
            new List<string>() { "Testing", "Hermetic_Test", "Chamber_S", "Chamber_S_16ch", "BR", "UV_Curing", "Fast_Calibration", "TF2" };
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
            new List<string>() { "Agilent", "Golight" };
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

        private string _selected_K_WL_Type = "ALL Range";
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
            new List<string>() { "Calibration", "DAC -> 0", "VOA -> 0", "TF -> 0", "K VOA", "K TF", "K TF (Rough)", "K TF (Detail)", "Curve Fitting", "K WL" };
        public List<string> list_combox_Calibration_items
        {
            get { return _list_combox_Calibration_items; }
            set
            {
                _list_combox_Calibration_items = value;
                OnPropertyChanged("list_combox_Calibration_items");
            }
        }

        private ObservableCollection<string> _list_combox_comports = new ObservableCollection<string>() { };
        public ObservableCollection<string> list_combox_comports
        {
            get { return _list_combox_comports; }
            set
            {
                _list_combox_comports = value;
                OnPropertyChanged("list_combox_comports");
            }
        }

        private List<string> _list_combox_Product_items =
            new List<string>() { "CTF", "UFA", "UFA-T", "UFA(H)", "UTF", "UTF300(H)", "UTF400", "UTF450", "UTF500", "UTF550", "MTF" };
        public List<string> list_combox_Product_items
        {
            get { return _list_combox_Product_items; }
            set
            {
                _list_combox_Product_items = value;
                OnPropertyChanged("list_combox_Product_items");
            }
        }
       

        private List<string> _list_combox_PowerMeterType_items =
            new List<string>() { "GPIB", "RS232" };
        public List<string> list_combox_PowerMeterType_items
        {
            get { return _list_combox_PowerMeterType_items; }
            set
            {
                _list_combox_PowerMeterType_items = value;
                OnPropertyChanged("list_combox_PowerMeterType_items");
            }
        }

        private int _switch_index = 1;
        public int switch_index
        {
            get { return _switch_index; }
            set
            {
                _switch_index = value;
                OnPropertyChanged_Normal("switch_index");
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

        //private List<bool> _ischeck = new List<bool>();
        //public List<bool> IsCheck
        //{
        //    get { return _ischeck; }
        //    set
        //    {
        //        _ischeck = value;
        //        List<Visibility> v_list = new List<Visibility>();
        //        foreach (bool b in _ischeck)
        //        {
        //            if (b)
        //                v_list.Add(Visibility.Visible);
        //            else
        //                v_list.Add(Visibility.Hidden);
        //        }
        //        if (v_list.Count > 0)
        //        {
        //            if (v_list.Count < 12)
        //            {
        //                for (int j = v_list.Count; j < LineSeries_Visible.Count; j++)
        //                {
        //                    v_list.Add(Visibility.Collapsed);
        //                }
        //            }

        //            LineSeries_Visible = new ObservableCollection<Visibility>(v_list);
        //        }
        //        else
        //        {
        //            for (int i = 0; i < 12; i++)
        //            {
        //                v_list.Add(Visibility.Hidden);
        //            }
        //            LineSeries_Visible = new ObservableCollection<Visibility>(v_list);
        //        }

        //        OnPropertyChanged("IsCheck");
        //    }
        //}

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
        public bool isLaserActive
        {
            get { return _isLaserActive; }
            set
            {

                _isLaserActive = value;

                if (_isGoOn)
                {
                    Save_cmd(new ComMember() { YN = true, Command = "TLSACT", Type = "Agilent", Value_1 = value.ToString(), No = Cmd_Count.ToString() });
                }
                //else
                //    Laser_Connect_Active(value);

                OnPropertyChanged("isLaserActive");
            }
        }

        private void OnTimedEvent(object source, System.Timers.ElapsedEventArgs e)
        {
            //timer3.Stop();
        }

        private List<string> _list_SN = new List<string>() { "", "", "", "", "", "", "", "", "", "", "", "" };
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
                    DacType = "Voltage";
                else
                    DacType = "Dac";
                OnPropertyChanged("isDACorVolt");
                //OnPropertyChanged("DacType");
                Ini_Write("Connection", "DACorVolt", value.ToString());
            }
        }

        private string _DacType = "Dac";
        public string DacType
        {
            get { return _DacType; }
            set
            {
                _DacType = value;
                OnPropertyChanged("DacType");
            }
        }

        private string[] _Save_Product_Info = new string[2];
        public string[] Save_Product_Info
        {
            get { return _Save_Product_Info; }
            set
            {
                _Save_Product_Info = value;
                OnPropertyChanged("Save_Product_Info");
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

        private bool _Auto_Update = true;
        public bool Auto_Update
        {
            get { return _Auto_Update; }
            set
            {
                _Auto_Update = value;
                ini.IniWriteValue("Connection", "Auto_Update", value.ToString(), ini_path);
                OnPropertyChanged("Auto_Update");
            }
        }



        private bool _SN_Judge = true;
        public bool SN_Judge
        {
            get { return _SN_Judge; }
            set
            {
                _SN_Judge = value;
                ini.IniWriteValue("Productions", "SN_Judge", value.ToString(), ini_path);
                OnPropertyChanged("SN_Judge");
            }
        }

        private bool _SN_AutoTab = false;
        public bool SN_AutoTab
        {
            get { return _SN_AutoTab; }
            set
            {
                _SN_AutoTab = value;
                ini.IniWriteValue("Productions", "SN_AutoTab", value.ToString(), ini_path);
                OnPropertyChanged("SN_AutoTab");
            }
        }

        private bool _is_TLS_Filter = true;
        public bool is_TLS_Filter
        {
            get { return _is_TLS_Filter; }
            set
            {
                _is_TLS_Filter = value;
                ini.IniWriteValue("Connection", "is_TLS_Filter", value.ToString(), ini_path);
                OnPropertyChanged("is_TLS_Filter");
            }
        }

        private bool _PM_sync = true;
        public bool PM_sync
        {
            get { return _PM_sync; }
            set
            {
                _PM_sync = value;
                OnPropertyChanged("PM_sync");
            }
        }

        private List<Dictionary<double, double>> _Ref_dictionaries = new List<System.Collections.Generic.Dictionary<double, double>>();
        public List<Dictionary<double, double>> Ref_Dictionaries
        {
            get { return _Ref_dictionaries; }
            set
            {
                _Ref_dictionaries = value;

                OnPropertyChanged("Ref_Dictionaries");
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

        //private bool _HermeticData_Save_or_Read = false;
        //public bool HermeticData_Save_or_Read
        //{
        //    get { return _HermeticData_Save_or_Read; }
        //    set { _HermeticData_Save_or_Read = value; }
        //}

        public List<double> wl_list { get; set; } = new List<double>();
        public int wl_list_index { get; set; } = 0;

        private double _float_WL_Scan_Start = 1526;
        public double float_WL_Scan_Start
        {
            get { return _float_WL_Scan_Start; }
            set
            {
                _float_WL_Scan_Start = value;
                OnPropertyChanged("float_WL_Scan_Start");

                Ini_Write("Scan", "WL_Scan_Start", value.ToString());
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
                Ini_Write("Scan", "WL_Scan_End", value.ToString());
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
                Ini_Write("Scan", "WL_Scan_Gap", value.ToString());
            }
        }

        private float[] _float_TLS_WL_Range = { 1520, 1580 };
        public float[] float_TLS_WL_Range
        {
            get { return _float_TLS_WL_Range; }
            set
            {
                _float_TLS_WL_Range = value;
                OnPropertyChanged("float_TLS_WL_Range");
            }
        }

        private ObservableCollection<float> _List_Fix_WL = new ObservableCollection<float>() { 1530, 1548, 1565, 1550 };
        public ObservableCollection<float> List_Fix_WL
        {
            get { return _List_Fix_WL; }
            set
            {
                _List_Fix_WL = value;
                OnPropertyChanged("List_Fix_WL");
            }
        }

        //int index = 0;
        private double _double_Laser_Wavelength;
        public double Double_Laser_Wavelength
        {
            get { return _double_Laser_Wavelength; }
            set
            {
                _double_Laser_Wavelength = Math.Round(value, 2);

                float_WL_Ref = new List<double>();  //目前波長的Ref值

                //從字典找Ref值
                if (_Ref_dictionaries != null)
                    if (_Ref_dictionaries.Count >= ch_count)
                    {
                        for (int ch = 0; ch < ch_count; ch++)
                        {
                            float_WL_Ref.Add(_Ref_dictionaries[ch].ContainsKey(_double_Laser_Wavelength) ? _Ref_dictionaries[ch][_double_Laser_Wavelength] : 0);
                        }
                    }
                OnPropertyChanged("Double_Laser_Wavelength");
            }
        }

        private double _double_PM_Wavelength;
        public double Double_PM_Wavelength
        {
            get { return _double_PM_Wavelength; }
            set
            {
                _double_PM_Wavelength = value;
                OnPropertyChanged("Double_PM_Wavelength");
            }
        }

        private double _double_PM_Ref = 0;
        public double Double_PM_Ref
        {
            get { return _double_PM_Ref; }
            set
            {
                _double_PM_Ref = value;
                OnPropertyChanged("Double_PM_Ref");
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

        private int _int_Dac_max = 65535;
        public int int_Dac_max
        {
            get { return _int_Dac_max; }
            set
            {
                _int_Dac_max = value;
                OnPropertyChanged("int_Dac_max");
            }
        }

        private string _UserID = "";
        public string UserID
        {
            get { return _UserID; }
            set
            {
                if (value.Length < 5 && value.Length > 0)
                {
                    char head = value[0];
                    string str_Nums = value.Substring(1);

                    string compensation = "";

                    if (!int.TryParse(head.ToString(), out int h1) && int.TryParse(str_Nums, out int Nums1))
                    {
                        for (int i = value.Length; i < 5; i++)
                            compensation += "0";
                        _UserID = $"{head}{compensation}{str_Nums}".ToUpper();
                    }
                    else if (int.TryParse(head.ToString(), out int h2) && int.TryParse(str_Nums, out int Nums2))
                    {
                        for (int i = value.Length; i < 4; i++)
                            compensation += "0";
                        _UserID = $"P{compensation}{value}".ToUpper();
                    }
                    else
                    {
                        MessageBox.Show("Please check ID format");
                        return;
                    }
                }
                else if (value.Length == 5)
                    _UserID = value;
                else
                {
                    MessageBox.Show("Please check ID format");
                    return;
                }

                OnPropertyChanged("UserID");
            }
        }

        private string _selected_band = "Auto";
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

        private SolidColorBrush[] _ref_Color = new SolidColorBrush[16];
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

        private DiCon.Instrument.HP.GLTLS _tls_GL = new DiCon.Instrument.HP.GLTLS();
        public DiCon.Instrument.HP.GLTLS tls_GL
        {
            get { return _tls_GL; }
            set
            {
                _tls_GL = value;
                OnPropertyChanged("tls_GL");
            }
        }

        //private DiCon.Instrument.HP.GLTLS _tls_GL = new DiCon.Instrument.HP.GLTLS();
        //public DiCon.Instrument.HP.GLTLS tls_GL
        //{
        //    get { return _tls_GL; }
        //    set
        //    {
        //        _tls_GL = value;
        //        OnPropertyChanged("tls_GL");
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

        private bool _is_k_WL_manual_setting = false;
        public bool Is_k_WL_manual_setting
        {
            get { return _is_k_WL_manual_setting; }
            set
            {
                _is_k_WL_manual_setting = value;
                ini.IniWriteValue("Scan", "is_k_WL_manual_setting", value.ToString(), ini_path);
                OnPropertyChanged("Is_k_WL_manual_setting");
            }
        }

        private bool _is_FastScan_Mode = false;
        public bool Is_FastScan_Mode
        {
            get { return _is_FastScan_Mode; }
            set
            {
                _is_FastScan_Mode = value;
                ini.IniWriteValue("Scan", "Is_FastScan_Mode", value.ToString(), ini_path);
                OnPropertyChanged("Is_FastScan_Mode");
            }
        }

        public string IL_ALL { get; set; } = "";

        private bool _is_update_chart = true;
        public bool is_update_chart
        {
            get { return _is_update_chart; }
            set
            {
                _is_update_chart = value;
                OnPropertyChanged("is_update_chart");
            }
        }

        //private int _switch_selected = 1;
        //public int switch_selected_index
        //{
        //    get { return _switch_selected; }
        //    set { _switch_selected = value; }
        //}

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

        //private int _Gauge_Page_now = 1;
        //public int Gauge_Page_now
        //{
        //    get { return _Gauge_Page_now; }
        //    set { _Gauge_Page_now = value; }
        //}

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
            set
            {
                _bool_gauge = value;
                OnPropertyChanged("Bool_Gauge");
            }
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

        private bool _bool_isCurfitting = false;
        public bool Bool_isCurfitting
        {
            get { return _bool_isCurfitting; }
            set
            {
                _bool_isCurfitting = value;
                OnPropertyChanged("Bool_isCurfitting");
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

        private int _int_fontsize = 12;
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
                Ini_Write("Scan", "V3_scan_start", value.ToString());  //創建ini file並寫入基本設定
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
                Ini_Write("Scan", "V3_scan_end", value.ToString());  //創建ini file並寫入基本設定
                OnPropertyChanged("int_V3_scan_end");
            }
        }

        private int _int_V3_scan_gap = 2000;
        public int int_V3_scan_gap
        {
            get { return _int_V3_scan_gap; }
            set
            {
                _int_V3_scan_gap = value;
                Ini_Write("Scan", "V3_Scan_Gap", value.ToString());  //創建ini file並寫入基本設定
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
                //Ini_Write("Productions", "Rough_Scan_Gap", value.ToString());  //創建ini file並寫入基本設定
                OnPropertyChanged("int_rough_scan_gap");
            }
        }

        //private int _int_detail_scan_gap = 200;
        //public int int_detail_scan_gap
        //{
        //    get { return _int_detail_scan_gap; }
        //    set
        //    {
        //        _int_detail_scan_gap = value;
        //        //Ini_Write("Productions", "Detail_Scan_Gap", value.ToString());  //創建ini file並寫入基本設定
        //        OnPropertyChanged("int_detail_scan_gap");
        //    }
        //}

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

        private bool _isTimerOn = false;
        public bool isTimerOn
        {
            get { return _isTimerOn; }
            set { _isTimerOn = value; }
        }

        private int _int_timer_timespan = int.MaxValue;
        public int int_timer_timespan
        {
            get { return _int_timer_timespan; }
            set
            {
                _int_timer_timespan = value;
            }
        }

        private int _int_timer_sec = 60;
        public int int_timer_sec
        {
            get { return _int_timer_sec; }
            set
            {
                _int_timer_sec = value;
                int_timer_sec_angle = 6 * value;
                OnPropertyChanged("int_timer_sec");
            }
        }

        private int _int_timer_min = 0;
        public int int_timer_min
        {
            get { return _int_timer_min; }
            set
            {
                _int_timer_min = value;
                int_timer_min_angle = 6 * value;
                OnPropertyChanged("int_timer_min");
            }
        }

        private int _int_timer_hrs = 0;
        public int int_timer_hrs
        {
            get { return _int_timer_hrs; }
            set
            {
                _int_timer_hrs = value;
                int_timer_hrs_angle = 15 * value;
                OnPropertyChanged("int_timer_hrs");
            }
        }

        private int _int_timer_sec_angle = 00;
        public int int_timer_sec_angle
        {
            get { return _int_timer_sec_angle; }
            set
            {
                _int_timer_sec_angle = value;
                OnPropertyChanged("int_timer_sec_angle");
            }
        }

        private int _int_timer_min_angle = 0;
        public int int_timer_min_angle
        {
            get { return _int_timer_min_angle; }
            set
            {
                _int_timer_min_angle = value;
                OnPropertyChanged("int_timer_min_angle");
            }
        }

        private int _int_timer_hrs_angle = 0;
        public int int_timer_hrs_angle
        {
            get { return _int_timer_hrs_angle; }
            set
            {
                _int_timer_hrs_angle = value;
                OnPropertyChanged("int_timer_hrs_angle");
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

                GetPWSettingModel.DelayTime = _int_Read_Delay;
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

                Ini_Write("Connection", "RS232_Write_DelayTime", value.ToString());
            }
        }

        private int _int_Set_WL_Delay = 280;
        public int Int_Set_WL_Delay
        {
            get { return _int_Set_WL_Delay; }
            set
            {
                _int_Set_WL_Delay = value;
                OnPropertyChanged("Int_Set_WL_Delay");

                Ini_Write("Connection", "GPIB_Write_WL_DelayTime", value.ToString());
            }
        }

        private int _int_Lambda_Scan_Delay = 2500;
        public int Int_Lambda_Scan_Delay
        {
            get { return _int_Lambda_Scan_Delay; }
            set
            {
                _int_Lambda_Scan_Delay = value;
                OnPropertyChanged("Int_Lambda_Scan_Delay");

                Ini_Write("Connection", "Int_Lambda_Scan_Delay", value.ToString());
            }
        }

        private string _chart_title = "Power X Time";
        public string Chart_title
        {
            get { return _chart_title; }
            set
            {
                _chart_title = value;
                OnPropertyChanged("Chart_title");
            }
        }

        private string _chart_x_title = "Time (s)";
        public string Chart_x_title
        {
            get { return _chart_x_title; }
            set
            {
                _chart_x_title = value;
                OnPropertyChanged("Chart_x_title");
            }
        }

        private string _chart_y_title = "Power (dBm)";
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

        private string _str_boardID = "";
        public string str_boardID
        {
            get { return _str_boardID; }
            set
            {
                _str_boardID = value;
                OnPropertyChanged("str_boardID");
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
                isStop = value ? false : true;
                Str_Go_Content = value == false ? "Go" : "Stop";
                OnPropertyChanged("IsGoOn");
            }
        }

        private bool _isDistributedSystem = false;
        public bool IsDistributedSystem
        {
            get { return _isDistributedSystem; }
            set
            {
                _isDistributedSystem = value;
                Ini_Write("Connection", "IsDistributedSystem", _isDistributedSystem.ToString());
                OnPropertyChanged("IsDistributedSystem");
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

                    GetPWSettingModel.TypeName = "PM";
                    GetPWSettingModel.Interface = "GPIB";
                }
                else
                {
                    Main_Color = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF33D3C4"));
                    Ini_Write("Connection", "PD_or_PM", "PD");

                    GetPWSettingModel.TypeName = "PD";
                    GetPWSettingModel.Interface = "RS232";
                }
                foreach (GaugeModel gm in _list_GaugeModels)
                {
                    gm.PD_or_PM = value;
                }
                OnPropertyChanged("PD_or_PM");

            }
        }

        //private bool _IsArduinoOn = false;  //IsArduinoOn
        //public bool IsArduinoOn
        //{
        //    get { return _IsArduinoOn; }
        //    set
        //    {
        //        _IsArduinoOn = value;
        //        if (value)
        //            Chart_y_title = "Voltage(V)";
        //        else
        //            Chart_y_title = "Power(dBm)";

        //        Ini_Write("Connection", "Is_Arduino_On", value.ToString());

        //        OnPropertyChanged("IsArduinoOn");
        //    }
        //}

        private bool _IsArduinoConnected = false;  //IsArduinoConnected
        public bool IsArduinoConnected
        {
            get { return _IsArduinoConnected; }
            set
            {
                _IsArduinoConnected = value;
                OnPropertyChanged("IsArduinoConnected");
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
                Ini_Write("Connection", "tls_BoardNumber", value.ToString());
                OnPropertyChanged("tls_BoardNumber");

                if (tls != null)
                    tls.BoardNumber = value;
            }
        }

        private int _tls_Addr = 24;
        public int tls_Addr
        {
            get { return _tls_Addr; }
            set
            {
                _tls_Addr = value;
                Ini_Write("Connection", "tls_Addr", value.ToString());
                OnPropertyChanged("tls_Addr");

                if (tls != null)
                    tls.Addr = value;
            }
        }

        private int _pm_BoardNumber = 0;
        public int pm_BoardNumber
        {
            get { return _pm_BoardNumber; }
            set
            {
                _pm_BoardNumber = value;
                Ini_Write("Connection", "pm_BoardNumber", value.ToString());
                OnPropertyChanged("pm_BoardNumber");

                if (pm != null)
                    pm.BoardNumber = value;
            }
        }

        private int _pm_Addr = 24;
        public int pm_Addr
        {
            get { return _pm_Addr; }
            set
            {
                _pm_Addr = value;
                Ini_Write("Connection", "pm_Addr", value.ToString());
                OnPropertyChanged("pm_Addr");

                if (pm != null)
                    pm.Addr = value;
            }
        }

        private int _pdl_BoardNumber = 0;
        public int pdl_BoardNumber
        {
            get { return _pdl_BoardNumber; }
            set
            {
                _pdl_BoardNumber = value;
                OnPropertyChanged("pdl_BoardNumber");
            }
        }

        private int _pdl_Addr = 11;
        public int pdl_Addr
        {
            get { return _pdl_Addr; }
            set
            {
                _pdl_Addr = value;
                Ini_Write("Connection", "pdl_Addr", value.ToString());
                OnPropertyChanged("pdl_Addr");
            }
        }

        private int _multiMeter_Addr = 22;
        public int multiMeter_Addr
        {
            get { return _multiMeter_Addr; }
            set
            {
                _multiMeter_Addr = value;
                Ini_Write("Connection", "multiMeter_Addr", value.ToString());
                OnPropertyChanged_Normal("multiMeter_Addr");
            }
        }


        private int _PM_slot = 1;
        public int PM_slot
        {
            get { return _PM_slot; }
            set
            {
                _PM_slot = value;
                Ini_Write("Connection", "PM_slot", value.ToString());
                OnPropertyChanged("PM_slot");
            }
        }

        private int _PM_AveTime = 20;
        public int PM_AveTime
        {
            get { return _PM_AveTime; }
            set
            {
                _PM_AveTime = value;
                OnPropertyChanged("PM_AveTime");

                Ini_Write("Connection", "PM_AveTime", value.ToString());
                pm.aveTime(_PM_AveTime);
            }
        }

        private int _BoudRate = 115200;
        public int BoudRate
        {
            get { return _BoudRate; }
            set
            {
                _BoudRate = value;
                OnPropertyChanged("BoudRate");

                GetPWSettingModel.BaudRate = value;
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

        private List<List<DataPoint>> _chart_all_datapoints = new List<List<DataPoint>>() { new List<DataPoint>(16) };
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

        private ObservableCollection<DataPoint> _chart_datapoints_ref = new ObservableCollection<DataPoint>();
        public ObservableCollection<DataPoint> Chart_DataPoints_ref
        {
            get { return _chart_datapoints_ref; }
            set
            {
                _chart_datapoints_ref = value;
                OnPropertyChanged("Chart_DataPoints_ref");
            }
        }

        //private List<DataPoint> _chart_datapoints_ref = new List<DataPoint>();
        //public List<DataPoint> Chart_DataPoints_ref
        //{
        //    get { return _chart_datapoints_ref; }
        //    set
        //    {
        //        _chart_datapoints_ref = value;
        //        OnPropertyChanged("Chart_DataPoints_ref");
        //    }
        //}

        private ObservableCollection<ObservableCollection<DataPoint>> _chart_all_datapoints_ref = new ObservableCollection<ObservableCollection<DataPoint>>();
        public ObservableCollection<ObservableCollection<DataPoint>> Chart_All_DataPoints_ref
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

        public int bearSay { get; set; } = -1;


        public List<double> maxIL { get; set; } = new List<double>();
        public List<double> minIL { get; set; } = new List<double>();

        private List<double> _Double_Powers = new List<double>(); //0~-64dBm
        public List<double> Double_Powers
        {
            get { return _Double_Powers; }
            set
            {
                _Double_Powers = value;
                OnPropertyChanged("Double_Powers");
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

        private Visibility _GaugeChart_visible = Visibility.Collapsed;
        public Visibility GaugeChart_visible
        {
            get { return _GaugeChart_visible; }
            set
            {
                _GaugeChart_visible = value;
                OnPropertyChanged("GaugeChart_visible");
            }
        }

        private Visibility _GaugeGrid1_visible = Visibility.Visible;
        public Visibility GaugeGrid1_visible
        {
            get { return _GaugeGrid1_visible; }
            set
            {
                _GaugeGrid1_visible = value;
                OnPropertyChanged("GaugeGrid1_visible");
            }
        }

        private Visibility _GaugeGrid2_visible = Visibility.Visible;
        public Visibility GaugeGrid2_visible
        {
            get { return _GaugeGrid2_visible; }
            set
            {
                _GaugeGrid2_visible = value;
                OnPropertyChanged("GaugeGrid2_visible");
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

        private double _GaugePage_Width = 1200;
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

        public bool isPageLogorCommand { get; set; } = true;

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

        private static readonly object key_cmd = new object();
        private string _str_command = " ";
        public string Str_Command
        {
            get
            {
                lock (key_cmd) { return _str_command; }
            }
            set
            {
                lock (key_cmd) { _str_command = value; }
                OnPropertyChanged("Str_Command");
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

        private string _btn_cmd_txt = "";
        public string btn_cmd_txt
        {
            get { return _btn_cmd_txt; }
            set
            {
                _btn_cmd_txt = value;
                OnPropertyChanged("btn_cmd_txt");
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

        private List<List<string>> _list_bear_say;
        public List<List<string>> List_bear_say
        {
            get { return _list_bear_say; }
            set
            {
                _list_bear_say = value;
                OnPropertyChanged("List_bear_say");
            }
        }

        private ObservableCollection<RefModel> _Ref_memberDatas = new ObservableCollection<RefModel>();
        public ObservableCollection<RefModel> Ref_memberDatas
        {
            get { return _Ref_memberDatas; }
            set
            {
                _Ref_memberDatas = value;
                OnPropertyChanged("Ref_memberDatas");
            }
        }

        private ObservableCollection<string> _list_bear_grid_data = new ObservableCollection<string>();
        public ObservableCollection<string> List_bear_grid_data
        {
            get { return _list_bear_grid_data; }
            set
            {
                _list_bear_grid_data = value;
                OnPropertyChanged("List_bear_grid_data");
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

        //private List<List<List<string>>> _Collection_bear_say = new List<List<List<string>>>();
        //public List<List<List<string>>> Collection_bear_say
        //{
        //    get { return _Collection_bear_say; }
        //    set
        //    {
        //        _Collection_bear_say = value;
        //        OnPropertyChanged("Collection_bear_say");
        //    }
        //}

        private ObservableCollection<ObservableCollection<string>> _list_D0_value = new ObservableCollection<ObservableCollection<string>>();
        public ObservableCollection<ObservableCollection<string>> List_D0_value
        {
            get { return _list_D0_value; }
            set
            {
                _list_D0_value = value;
                OnPropertyChanged("List_D0_value");
            }
        }

        private string[,] _str_D0_value = new string[8, 3];
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

        private string _title = "";
        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                OnPropertyChanged("Title");
            }
        }

        private ItemsControl _combox_items;
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

    public class enum_station
    {
        public string Testing;
        public string Hermetic_Test;
        public string Chamber_S;
        public string Chamber_S_16ch;
        public string BR;
        public string UV_Curing;
        public string Fast_Calibration;
        public string TF2;
    }
}
