//using DiCon.Instrument.HP;
using PD.GPIB;

using DiCon.UCB.Communication;
using OxyPlot;
using PD.AnalysisModel;
using PD.Functions;
using PD.Models;
using PD.NavigationPages;
using PD.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Diagnostics;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.Annotations;

using NationalInstruments.NI4882;

namespace PD
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ComViewModel vm;
        Analysis anly;
        Setting setting;
        ControlCmd cmd;

        string[] myPorts;
        string str_selected_com = "COM1";

        //int read_delay; //Must >105 ms
        //double power_PM;
        //int v12_maxpower_index_PM;
        List<int> maxPowder_dac = new List<int>();
        Page_PD_Gauges _Page_PD_Gauges;
        Page_TF2_Main _Page_TF2_Main;
        Page_UTF600_Main _Page_UTF600_Main;
        Page_BR _Page_BR;
        Page_Chart _Page_Chart;
        Page_DataGrid _Page_DataGrid;
        Page_Laser _Page_Laser;
        Page_Log_Command _Page_Log_Command;
        Page_Command _Page_Command;
        //Page_Log _Page_Log;
        Page_Setting _Page_Setting;

        //Window_Waiting win_waiting;

        bool _isAskPortStop = false;

        int int_saved_combox_index;

        private SerialPort port_arduino = null;

        static string CurrentDirectory = Directory.GetCurrentDirectory();

        public MainWindow()
        {
            InitializeComponent();

            this.Left = System.Windows.Forms.Screen.AllScreens.FirstOrDefault().WorkingArea.Left;
            this.Top = System.Windows.Forms.Screen.AllScreens.FirstOrDefault().WorkingArea.Top;


            //設定datacontext
            vm = new ComViewModel();
            this.DataContext = vm;
            grid_process_schedule.DataContext = vm.msgModel;

            Initializing();


        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            vm.mainWin_size = new double[] { ActualWidth, ActualHeight };
            vm.mainWin_point = new System.Windows.Point(Left, Top);
            vm.BoolAllGauge = true;

            btn_desktop.Visibility = Visibility.Collapsed;

            this.ResizeMode = ResizeMode.CanResizeWithGrip;
            this.WindowState = WindowState.Normal;

            //取得可獲得之工作視窗大小(不含工作列)         
            var a = SystemParameters.WorkArea;
            this.MaxHeight = a.Height;
            this.MaxWidth = a.Width;
            Height = MaxHeight;
            Width = MaxWidth;
            System.Windows.Point p = this.PointToScreen(new System.Windows.Point(0, 0));
            this.Left = Left - p.X;
            this.Top = Top - p.Y;
        }

        public bool Initializing()
        {
            try
            {
                setting = new Setting(vm);
                cmd = new ControlCmd(vm);
                anly = new Analysis(vm);

                vm.InitializeComViewModel();

                _isAskPortStop = false;

                #region Initialization

                List<DateTime> listdateTime = new List<DateTime>();
                listdateTime.Add(DateTime.Now); //Event 0

                #region Version Setting
                string version = Path.GetFileNameWithoutExtension(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);

                if (version.Split('-').Length == 2) version = version.Split('-')[1];
                else version = "";

                vm.Title = "Polar Bear " + version;
                vm.txt_now_version = version.Replace("v", "");
                //txt_version.Text = version;
                #endregion

                listdateTime.Add(DateTime.Now);  //Event 1

                #region Read ini file and setting   

                var task = Task.Run(() => vm.CheckDirectoryExist(@"D:\"));
                var result = (task.Wait(1000)) ? task.Result : false;

                //Desk D exist !
                if (result)
                {
                    if (!vm.CheckDirectoryExist(@"D:\PD"))
                        Directory.CreateDirectory(@"D:\PD");
                }
                // Desk D is not exist
                else
                    vm.ini_path = Path.Combine(CurrentDirectory, "Instrument.ini");

                if (File.Exists(vm.ini_path))
                {
                    vm.txt_Equip_Setting_Path = string.IsNullOrEmpty(vm.Ini_Read("Connection", "Equip_Setting_Path")) ? "" : vm.Ini_Read("Connection", "Equip_Setting_Path");
                    vm.txt_Chamber_Status_Path = string.IsNullOrEmpty(vm.Ini_Read("Connection", "Chamber_Status_Path")) ? vm.txt_Chamber_Status_Path : vm.Ini_Read("Connection", "Chamber_Status_Path");
                    vm.txt_Auto_Update_Path = string.IsNullOrEmpty(vm.Ini_Read("Connection", "Auto_Update_Path")) ? vm.txt_Auto_Update_Path : vm.Ini_Read("Connection", "Auto_Update_Path");
                    vm.Server_IP = string.IsNullOrEmpty(vm.Ini_Read("Connection", "Server_IP")) ? vm.Server_IP : vm.Ini_Read("Connection", "Server_IP");
                    vm.txt_save_wl_data_path = string.IsNullOrEmpty(vm.Ini_Read("Connection", "Save_Hermetic_Data_Path")) ? vm.txt_save_wl_data_path : vm.Ini_Read("Connection", "Save_Hermetic_Data_Path");
                    vm.txt_save_TF2_wl_data_path = string.IsNullOrEmpty(vm.Ini_Read("Connection", "Save_TF2_Data_Path")) ? vm.txt_save_TF2_wl_data_path : vm.Ini_Read("Connection", "Save_TF2_Data_Path");
                    vm.txt_board_table_path = string.IsNullOrEmpty(vm.Ini_Read("Connection", "Control_Board_Table_Path")) ? vm.txt_board_table_path : vm.Ini_Read("Connection", "Control_Board_Table_Path");
                    vm.txt_BR_Save_Path = string.IsNullOrEmpty(vm.Ini_Read("Connection", "BR_Save_Path")) ? vm.txt_BR_Save_Path : vm.Ini_Read("Connection", "BR_Save_Path");

                    //combox_comport.SelectedItem = vm.Ini_Read("Connection", "Comport");
                    vm.Selected_Comport = vm.Ini_Read("Connection", "Comport");
                    vm.Comport_TLS_Filter = vm.Ini_Read("Connection", "Comport_TLS_Filter");
                    vm.Comport_Switch = vm.Ini_Read("Connection", "Comport_Switch");
                    //vm.station_type = (ComViewModel.StationTypes)Enum.Parse(typeof(ComViewModel.StationTypes) ,vm.Ini_Read("Connection", "Station"));
                    if (string.IsNullOrEmpty(vm.Ini_Read("Connection", "Station")))
                        vm.station_type_No = 0;
                    else
                        vm.station_type_No = (int)(ComViewModel.StationTypes)Enum.Parse(typeof(ComViewModel.StationTypes), vm.Ini_Read("Connection", "Station"));

                    vm.PD_A_ChannelModel.Board_Port = vm.Ini_Read("Connection", "COM_PD_A");
                    vm.PD_B_ChannelModel.Board_Port = vm.Ini_Read("Connection", "COM_PD_B");

                    //if (int.TryParse(vm.Ini_Read("Connection", "OSA_BoardNumber"), out int osa_BoardNo))
                    //    vm.OSA_BoardNumber = osa_BoardNo;

                    vm.product_type = vm.Ini_Read("Productions", "Product");
                    vm.Is_k_WL_manual_setting = Generic_GetINISetting(vm.Is_k_WL_manual_setting, "Scan", "is_k_WL_manual_setting");
                    vm.selected_K_WL_Type = String.IsNullOrEmpty(vm.Ini_Read("Productions", "K_WL_Type")) ? "ALL Range" : vm.Ini_Read("Productions", "K_WL_Type");

                    vm.OSA_BoardNumber = Generic_GetINISetting(vm.OSA_BoardNumber, "Connection", "OSA_BoardNumber");
                    vm.OSA_Addr = Generic_GetINISetting(vm.OSA_Addr, "Connection", "OSA_Addr");

                    vm.Is_FastScan_Mode = Generic_GetINISetting(vm.Is_FastScan_Mode, "Scan", "Is_FastScan_Mode");

                    vm.Auto_Update = Generic_GetINISetting(vm.Auto_Update, "Connection", "Auto_Update");

                    vm.SN_Judge = Generic_GetINISetting(vm.SN_Judge, "Productions", "SN_Judge");

                    if (!string.IsNullOrEmpty(vm.Ini_Read("Connection", "TLS_TCPIP")))
                        vm.TLS_TCPIP = vm.Ini_Read("Connection", "TLS_TCPIP");

                    vm.is_TLS_Filter = Generic_GetINISetting(vm.is_TLS_Filter, "Connection", "is_TLS_Filter");

                    vm.SN_AutoTab = Generic_GetINISetting(vm.SN_AutoTab, "Productions", "SN_AutoTab");

                    vm.Int_Read_Delay = Generic_GetINISetting(vm.Int_Read_Delay, "Connection", "RS232_Delay_Time");

                    vm.Int_Write_Delay = Generic_GetINISetting(vm.Int_Write_Delay, "Connection", "RS232_Write_DelayTime");

                    vm.Int_Set_WL_Delay = Generic_GetINISetting(vm.Int_Set_WL_Delay, "Connection", "GPIB_Write_WL_DelayTime");

                    vm.Int_Lambda_Scan_Delay = Generic_GetINISetting(vm.Int_Lambda_Scan_Delay, "Connection", "Int_Lambda_Scan_Delay");

                    vm.int_rough_scan_gap = Generic_GetINISetting(vm.int_rough_scan_gap, "Scan", "V12_Scan_Gap");
                    vm.int_rough_scan_start = Generic_GetINISetting(vm.int_rough_scan_start, "Scan", "V12_Scan_Start");
                    vm.int_rough_scan_stop = Generic_GetINISetting(vm.int_rough_scan_stop, "Scan", "V12_Scan_End");


                    vm.int_V3_scan_start = Generic_GetINISetting(vm.int_V3_scan_start, "Scan", "V3_Scan_Start");
                    vm.int_V3_scan_end = Generic_GetINISetting(vm.int_V3_scan_end, "Scan", "V3_Scan_End");
                    vm.int_V3_scan_gap = Generic_GetINISetting(vm.int_V3_scan_gap, "Scan", "V3_Scan_Gap");

                    vm.float_WL_Scan_Start = Generic_GetINISetting(vm.float_WL_Scan_Start, "Scan", "WL_Scan_Start");

                    vm.float_WL_Scan_End = Generic_GetINISetting(vm.float_WL_Scan_End, "Scan", "WL_Scan_End");

                    vm.float_WL_Scan_Gap = Generic_GetINISetting(vm.float_WL_Scan_Gap, "Scan", "WL_Scan_Gap");

                    vm.List_Fix_WL[0] = String.IsNullOrEmpty(vm.Ini_Read("Scan", "Fix_WL_1")) ? "1530" : vm.Ini_Read("Scan", "Fix_WL_1");
                    vm.List_Fix_WL[1] = String.IsNullOrEmpty(vm.Ini_Read("Scan", "Fix_WL_2")) ? "1548" : vm.Ini_Read("Scan", "Fix_WL_2");
                    vm.List_Fix_WL[2] = String.IsNullOrEmpty(vm.Ini_Read("Scan", "Fix_WL_3")) ? "1565" : vm.Ini_Read("Scan", "Fix_WL_3");
                    vm.List_Fix_WL[3] = String.IsNullOrEmpty(vm.Ini_Read("Scan", "Fix_WL_4")) ? "1550" : vm.Ini_Read("Scan", "Fix_WL_4");

                    vm.IsDistributedSystem = Generic_GetINISetting(vm.IsDistributedSystem, "Connection", "IsDistributedSystem");

                    if (vm.Ini_Read("Connection", "PD_or_PM") == "PM")
                    {
                        vm.PD_or_PM = true;
                        vm.Main_Color = (SolidColorBrush)(new BrushConverter().ConvertFrom("#0085CA"));
                        vm.Ini_Write("Connection", "PD_or_PM", "PM");
                    }

                    if (vm.Ini_Read("Productions", "Unit") == "dBm")
                    {
                        vm.run_dBm_color = new SolidColorBrush(Colors.White);
                        vm.run_dB_color = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF878787"));
                        vm.str_Unit = "dBm";
                        vm.dB_or_dBm = false;
                    }
                }
                else
                {
                    Directory.CreateDirectory(Directory.GetParent(vm.ini_path).ToString());  //建立資料夾      
                    vm.Ini_Write("Connection", "Comport", "COM2");  //創建ini file並寫入基本設定
                    vm.Ini_Write("Productions", "Product", "CTF");
                }
                #endregion

                listdateTime.Add(DateTime.Now);  //Event 2

                #region GetPowerType_Setting
                if (vm.PD_or_PM)
                {
                    vm.GetPWSettingModel.TypeName = "PM";
                    vm.GetPWSettingModel.Interface = "GPIB";
                }
                else
                {
                    vm.GetPWSettingModel.TypeName = "PD";
                    vm.GetPWSettingModel.Interface = "RS232";
                }
                vm.GetPWSettingModel.Comport = vm.Selected_Comport;
                vm.GetPWSettingModel.BaudRate = vm.BoudRate;
                vm.GetPWSettingModel.DelayTime = vm.Int_Read_Delay;
                #endregion

                listdateTime.Add(DateTime.Now);  //Event 3

                #region Calibration ComboBox Items Setting
                vm.list_combox_Calibration_items.Clear();
                for (int i = 1; i < 10; i++)
                {
                    string _item = vm.Ini_Read("Calibration", "Item_" + i.ToString());
                    if (string.IsNullOrEmpty(_item))
                        continue;

                    vm.list_combox_Calibration_items.Add(_item);
                }

                if (vm.list_combox_Calibration_items.Count == 0)
                    vm.list_combox_Calibration_items = new List<string>()
                { "Calibration", "DAC -> 0", "VOA -> 0", "TF -> 0", "K VOA", "K TF", "ReadData", "K WL" };
                #endregion

                listdateTime.Add(DateTime.Now);  //Event 4

                #region Timer Setting
                vm.timer_PD_GO = new System.Timers.Timer();
                vm.timer_PD_GO.Interval = vm.Int_Read_Delay;
                vm.timer_PD_GO.Elapsed += Timer2_PD_GO_Elapsed;

                vm.timer_PM_GO = new System.Timers.Timer();
                vm.timer_PM_GO.Interval = vm.Int_Read_Delay;
                vm.timer_PM_GO.Elapsed += Timer3_PM_GO_Elapsed;

                #endregion

                listdateTime.Add(DateTime.Now);  //Event 5

                #region Port Setting

                //myPorts = SerialPort.GetPortNames(); //取得所有port的方法
                //foreach (string s in myPorts) vm.list_combox_comports.Add(s);  //寫入所有取得的com

                //if (combox_comport.SelectedItem != null)
                //    vm.Selected_Comport = vm.Ini_Read("Connection", "Comport");
                //else
                //    vm.Str_cmd_read = "Selected Port is null";

                ////初始化port
                //if (!string.IsNullOrEmpty(vm.Selected_Comport))
                //    vm.port_PD = new SerialPort(vm.Selected_Comport, vm.BoudRate, Parity.None, 8, StopBits.One);

                //if (!string.IsNullOrEmpty(vm.Comport_TLS_Filter))
                //    vm.port_TLS_Filter = new SerialPort(vm.Comport_TLS_Filter, vm.TLS_Filter_BoudRate, Parity.None, 8, StopBits.One);

                //if (!string.IsNullOrEmpty(vm.Comport_Switch))
                //    vm.port_Switch = new SerialPort(vm.Comport_Switch, vm.Switch_BoudRate, Parity.None, 8, StopBits.One);

                #endregion

                listdateTime.Add(DateTime.Now);  //Event 6

                #region Page Navigation Setting

                //_Page_PD_Gauges = new Page_PD_Gauges(vm);
                //_Page_TF2_Main = new Page_TF2_Main(vm);
                //_Page_Chart = new Page_Chart(vm);
                //_Page_DataGrid = new Page_DataGrid(vm);
                //_Page_Laser = new Page_Laser(vm);
                //_Page_Command = new Page_Command(vm);
                ////_Page_Log = new Page_Log(vm);
                //_Page_Log_Command = new Page_Log_Command(vm);
                //_Page_Setting = new Page_Setting(vm);
                //_Page_BR = new Page_BR(vm);
                //RBtn_Gauge_Page.IsChecked = true;

                #endregion

                listdateTime.Add(DateTime.Now);  //Event 7

                #region Production Setting
                if (vm.product_type != null)
                {
                    vm.SNMembers = new ObservableCollection<SN_Member>();
                    for (int i = 0; i < vm.ch_count; i++)
                    {
                        vm.SNMembers.Add(new SN_Member() { ProductType = vm.product_type });
                    }

                    if (vm.selected_band.Equals("C Band"))
                    {
                        vm.float_TLS_WL_Range = new float[2] { 1520, 1580 };
                        if (vm.isConnected == false)
                            if (vm.list_wl != null)
                                vm.Double_Laser_Wavelength = 1523;
                    }
                    else if (vm.selected_band.Equals("L Band")) //L band
                    {
                        vm.float_TLS_WL_Range = new float[2] { 1560, 1620 };
                        if (vm.isConnected == false)
                            if (vm.list_wl != null)
                                vm.Double_Laser_Wavelength = 1560;
                    }

                    vm.Save_Product_Info = new string[] { vm.product_type, vm.selected_band };
                }
                #endregion

                listdateTime.Add(DateTime.Now);  //Event 8

                #region Board Setting (Load channels setting csv file)
                if (!string.IsNullOrEmpty(vm.txt_Equip_Setting_Path))
                    if (File.Exists(vm.txt_Equip_Setting_Path))
                        using (DataTable dtt = CSVFunctions.Read_CSV(vm.txt_Equip_Setting_Path))
                        {
                            if (dtt != null)
                            {
                                if (dtt.Rows.Count != 0)
                                {
                                    if (dtt.Columns.Count >= 13)
                                    {
                                        int loopIndex = (vm.list_ChannelModels.Count >= dtt.Rows.Count - 1) ? dtt.Rows.Count - 1 : vm.list_ChannelModels.Count;
                                        if (vm.list_ChannelModels.Count < dtt.Rows.Count - 1)
                                        {
                                            vm.Str_cmd_read = "Channel Counts < DataTable Rows";
                                            vm.Save_Log(new LogMember() { Message = vm.Str_cmd_read, isShowMSG = false });
                                        }

                                        for (int i = 0; i < loopIndex; i++)
                                        {
                                            vm.list_ChannelModels[i].channel = dtt.Rows[i + 1][0].ToString();
                                            vm.list_ChannelModels[i].Board_ID = dtt.Rows[i + 1][1].ToString();
                                            vm.list_ChannelModels[i].Board_Port = dtt.Rows[i + 1][2].ToString();
                                            if (int.TryParse(dtt.Rows[i + 1][3].ToString(), out int brt))
                                                vm.list_ChannelModels[i].BautRate = brt;
                                            vm.list_ChannelModels[i].PM_Type = dtt.Rows[i + 1][4].ToString();
                                            if (int.TryParse(dtt.Rows[i + 1][5].ToString(), out int PM_GPIB_BoardNum))
                                                vm.list_ChannelModels[i].PM_GPIB_BoardNum = PM_GPIB_BoardNum;
                                            if (int.TryParse(dtt.Rows[i + 1][6].ToString(), out int PM_Addr))
                                                vm.list_ChannelModels[i].PM_Address = PM_Addr;
                                            if (int.TryParse(dtt.Rows[i + 1][7].ToString(), out int PM_Slot))
                                                vm.list_ChannelModels[i].PM_Slot = PM_Slot;
                                            if (int.TryParse(dtt.Rows[i + 1][8].ToString(), out int PM_AveTime))
                                                vm.list_ChannelModels[i].PM_AveTime = PM_AveTime;
                                            vm.list_ChannelModels[i].PM_Board_ID = dtt.Rows[i + 1][9].ToString();
                                            vm.list_ChannelModels[i].PM_Board_Port = dtt.Rows[i + 1][10].ToString();
                                            if (int.TryParse(dtt.Rows[i + 1][11].ToString(), out int PM_BautRate))
                                                vm.list_ChannelModels[i].PM_BautRate = PM_BautRate;
                                            vm.list_ChannelModels[i].PM_GetPower_CMD = dtt.Rows[i + 1][12].ToString();
                                        }
                                    }
                                }
                            }
                        }

                if (vm.station_type == ComViewModel.StationTypes.Hermetic_Test)
                {
                    vm.list_Board_Setting.Clear();
                    for (int i = 0; i < vm.ch_count; i++)
                    {
                        string Board_ID = "Board_ID_" + (i + 1).ToString();
                        string Board_COM = "Board_COM_" + (i + 1).ToString();
                        vm.list_Board_Setting.Add(new List<string>() { vm.Ini_Read("Board_ID", Board_ID), vm.Ini_Read("Board_Comport", Board_COM) });
                        //vm.IsCheck.Add(false);
                    }
                    vm.list_Board_Setting = new List<List<string>>(vm.list_Board_Setting);

                    if (!anly.CheckDirectoryExist(vm.txt_board_table_path))
                    {
                        vm.Str_cmd_read = vm.txt_board_table_path + "\r\n" + " Directory is not exist";
                        vm.Save_Log(new LogMember() { Message = vm.Str_cmd_read });
                        //return true;
                    }

                    int k = 0;
                    foreach (List<string> board_info in vm.list_Board_Setting)
                    {
                        vm.board_read.Add(new List<string>());

                        if (string.IsNullOrEmpty(board_info[0]))
                        {
                            k++; continue;
                        }

                        string board_id = board_info[0];
                        string path = Path.Combine(vm.txt_board_table_path, board_id + "-boardtable.txt");

                        if (!File.Exists(path))
                        {
                            vm.Str_cmd_read = "UFV Board table is not exist";
                            vm.Save_Log("Get Board Table", (k + 1).ToString(), vm.Str_cmd_read);
                            k++;
                            continue;
                        }

                        StreamReader str = new StreamReader(path);

                        while (true)  //Read board v3 data
                        {
                            string readline = str.ReadLine();

                            if (string.IsNullOrEmpty(readline)) break;

                            vm.board_read[k].Add(readline);
                        }
                        str.Close(); //(關閉str)

                        k++;
                    }
                }

                vm.Comport_Switch = vm.Ini_Read("Connection", "Comport_Switch");
                vm.Comport_TLS_Filter = vm.Ini_Read("Connection", "Comport_TLS_Filter");

                #endregion

                listdateTime.Add(DateTime.Now);  //Event 9

                #region Others Setting
                vm.List_D0_value = new ObservableCollection<ObservableCollection<string>>();
                for (int j = 0; j < vm.ch_count; j++)
                {
                    vm.List_D0_value.Add(new ObservableCollection<string>());
                }

                vm.Double_Powers = Analysis.ListDefault<double>(vm.ch_count);

                string statId = vm.Ini_Read("Connection", "Station_ID");
                if (!string.IsNullOrEmpty(statId))
                    vm.Station_ID = statId;

                vm.list_collection_GaugeModels.Clear();
                #endregion

                listdateTime.Add(DateTime.Now);  //Event 10

                #region Event Setting
                //_Page_TF2_Main.K_WL_Event += new RoutedEventHandler(K_WL_Click);  //Set mainwindow event to page event
                #endregion

                Dispatcher.Invoke(() =>
                {
                    _Page_Setting = new Page_Setting(vm);
                });

                listdateTime.Add(DateTime.Now);  //Event 11

                #region Initial Chart Setting

                //vm.Plot_Series.Clear();
                //vm.Plot_Series.Add(new LineSeries
                //{
                //    Title = "Ch1",
                //    FontSize = 20,
                //    StrokeThickness = 1.8,
                //    MarkerType = MarkerType.Circle,
                //    MarkerSize = 0,  //4
                //    //Smooth = true,
                //    MarkerFill = vm.list_OxyColor[0],
                //    Color = vm.list_OxyColor[0],
                //    CanTrackerInterpolatePoints = true,
                //    TrackerFormatString = vm.Chart_x_title + " : {2}\n" + vm.Chart_y_title + " : {4}",
                //});

                //for (int i = -35; i < 36; i++)
                //{
                //    vm.Plot_Series[0].Points.Add(new DataPoint(i + 1548, vm.gauss_2D(i * 0.4, 0, 0, 0, 14.2, 0, 4.0)));
                //}

                //vm.PlotViewModel.Series.Clear();
                //vm.PlotViewModel.Series.Add(vm.Plot_Series[0]);

                #endregion

                listdateTime.Add(DateTime.Now);  //Event 12

                for (int i = 1; i < listdateTime.Count; i++)
                {
                    TimeSpan tp = listdateTime[i] - listdateTime[i - 1];
                    Console.WriteLine($"{i}: {tp.TotalMilliseconds}");
                }
                #endregion
            }
            catch (Exception ex)
            {
                //using (Stream st )
            }
            return true;
        }

        public void Initializing_MainThread()
        {
            #region Port Setting

            myPorts = SerialPort.GetPortNames(); //取得所有port的方法
            foreach (string s in myPorts) vm.list_combox_comports.Add(s);  //寫入所有取得的com

            if (combox_comport.SelectedItem != null)
                vm.Selected_Comport = vm.Ini_Read("Connection", "Comport");
            else
                vm.Str_cmd_read = "Selected Port is null";

            //初始化port
            if (!string.IsNullOrEmpty(vm.Selected_Comport))
                vm.port_PD = new SerialPort(vm.Selected_Comport, vm.BoudRate, Parity.None, 8, StopBits.One);

            if (!string.IsNullOrEmpty(vm.Comport_TLS_Filter))
                vm.port_TLS_Filter = new SerialPort(vm.Comport_TLS_Filter, vm.TLS_Filter_BoudRate, Parity.None, 8, StopBits.One);

            if (!string.IsNullOrEmpty(vm.Comport_Switch))
                vm.port_Switch = new SerialPort(vm.Comport_Switch, vm.Switch_BoudRate, Parity.None, 8, StopBits.One);

            #endregion

            #region Page Navigation Setting

            List<DateTime> list_dt = new List<DateTime>();
            list_dt.Add(DateTime.Now);

            _Page_PD_Gauges = new Page_PD_Gauges(vm);
            list_dt.Add(DateTime.Now);
            _Page_TF2_Main = new Page_TF2_Main(vm);
            list_dt.Add(DateTime.Now);
            _Page_Chart = new Page_Chart(vm);
            list_dt.Add(DateTime.Now);
            _Page_DataGrid = new Page_DataGrid(vm);
            list_dt.Add(DateTime.Now);
            _Page_Laser = new Page_Laser(vm);
            list_dt.Add(DateTime.Now);
            _Page_Command = new Page_Command(vm);
            list_dt.Add(DateTime.Now);
            _Page_Log_Command = new Page_Log_Command(vm);
            list_dt.Add(DateTime.Now);
            //_Page_Setting = new Page_Setting(vm);
            //list_dt.Add(DateTime.Now);
            _Page_BR = new Page_BR(vm);
            list_dt.Add(DateTime.Now);
            RBtn_Gauge_Page.IsChecked = true;

            for (int i = 1; i < list_dt.Count; i++)
            {
                TimeSpan tp = list_dt[i] - list_dt[i - 1];
                Console.WriteLine($"{i} : {tp.TotalMilliseconds}");
            }

            #region Setting Page init

            vm.selected_band = vm.Ini_Read("Connection", "Band");

            if (!string.IsNullOrEmpty(vm.Ini_Read("Connection", "Laser_type")))
            {
                string TLSType = vm.Ini_Read("Connection", "Laser_type");
                ComViewModel.LaserType lt;
                if (Enum.TryParse(TLSType, out lt))
                {
                    vm.Laser_type = lt;
                    //vm.Laser_type = (ComViewModel.LaserType)Enum.Parse(typeof(ComViewModel.LaserType), TLSType);
                }
            }


            if (!string.IsNullOrEmpty(vm.Ini_Read("Connection", "Switch_Comport"))) vm.Comport_Switch = vm.Ini_Read("Connection", "Switch_Comport");

            if (!string.IsNullOrEmpty(vm.Ini_Read("Connection", "Station")))
                vm.station_type = (ComViewModel.StationTypes)Enum.Parse(typeof(ComViewModel.StationTypes), vm.Ini_Read("Connection", "Station"));

            if (!string.IsNullOrEmpty(vm.Ini_Read("Connection", "Control_Board_Type")))
            {
                vm.Control_board_type_itm = vm.Ini_Read("Connection", "Control_Board_Type");
            }

            vm.Golight_ChannelModel.Board_Port = vm.Ini_Read("Connection", "COM_Golight");

            if (vm.Laser_type.Equals("Golight") && vm.Auto_Connect_TLS)
            {
                if (!string.IsNullOrEmpty(vm.Golight_ChannelModel.Board_Port))
                {
                    vm.tls_GL = new DiCon.Instrument.HP.GLTLS.GLTLS();
                    if (vm.tls_GL.Open(vm.Golight_ChannelModel.Board_Port))
                    {
                        vm.isConnected = true;
                    }
                    else
                        vm.Save_Log(new Models.LogMember()
                        {
                            isShowMSG = true,
                            Message = "Connect Golight TLS Fail"
                        });
                }
                else
                    vm.Save_Log(new Models.LogMember()
                    {
                        isShowMSG = true,
                        Message = "Golight comport is null or empty"
                    });
            }
            #endregion


            //_Page_Log = new Page_Log(vm);
            #endregion

            #region Event Setting
            if (_Page_TF2_Main != null)
                _Page_TF2_Main.K_WL_Event += new RoutedEventHandler(K_WL_Click);  //Set mainwindow event to page event
            #endregion
        }

        private async void OSA_StarTrack()
        {
            List<DataPoint> result = new List<DataPoint>();

            int points = Convert.ToInt32((vm.float_WL_Scan_End - vm.float_WL_Scan_Start) / vm.float_WL_Scan_Gap + 1);

            vm.OSA.SendCommand(string.Format("sens:bwid:res {0} nm;", vm.float_WL_Scan_Gap.ToString("f2")));
            await Task.Delay(30);
            //Set coninue
            vm.OSA.SendCommand(string.Format("init:cont {0}", "OFF"));
            await Task.Delay(30);
            //set start , end
            vm.OSA.SendCommand(string.Format("SENS:WAV:START {0}nm;STOP {1}nm;", vm.float_WL_Scan_Start.ToString("#0.00"), vm.float_WL_Scan_End.ToString("#0.00")));
            await Task.Delay(30);
            //set sensitive
            vm.OSA.SendCommand(string.Format("sens:pow:rang:low {0}dbm;", -70));
            await Task.Delay(30);
            //set points
            vm.OSA.SendCommand(string.Format("sens:swe:poin {0}", points));
            await Task.Delay(30);
            vm.OSA.SendCommand("TRAC:FEED:CONT TRA,ALW");

            await Task.Delay(300);
            vm.OSA.SendCommand("INIT:IMM");
            await Task.Delay(300);

            vm.OSA.SendCommand("DISP:WIND:TRAC:STAT TRA,ON");
            await Task.Delay(300);
            vm.OSA.SendCommand("TRAC:DATA:Y? TRA");

            string rawdata = vm.OSA.Read(17 * points); // set length of geting attenuation, 1 raw = 17 byte

            int counter = 0;
            while (rawdata == "-100" && counter < 10)
            {

                vm.OSA.SendCommand("TRAC:DATA:Y? TRA");
                rawdata = vm.OSA.Read(17 * Convert.ToInt32((vm.float_WL_Scan_End - vm.float_WL_Scan_Start + 1) / vm.float_WL_Scan_Gap));
                counter++;
            }
            if (rawdata == "-100")
                Console.WriteLine("-999,Time Limit Exceeded to complete operation");
            double osastep = vm.float_WL_Scan_Gap;// ((ewl - swl) / 1000); // each wavelength gets count
            string s = vm.OSA.GetAllPower(rawdata, osastep, vm.float_WL_Scan_Start, vm.float_WL_Scan_End);

            string[] list_s = s.Split(',');

            if (list_s.Length > 1)
            {
                for (int i = 1; i < list_s.Length; i++)
                {
                    if (i % 2 == 1)
                    {
                        if (double.TryParse(list_s[i - 1], out double wl) && double.TryParse(list_s[i], out double IL))
                            result.Add(new DataPoint(wl, IL));
                    }
                }
            }
        }

        public T Generic_GetINISetting<T>(T input, string region, string variable) where T : new()
        {
            if (input is int)
            {
                int value_int;
                if (int.TryParse(vm.Ini_Read(region, variable), out value_int))
                    return (T)(object)value_int;
                else
                    return input;
            }
            else if (input is double)
            {
                double value_int;
                if (double.TryParse(vm.Ini_Read(region, variable), out value_int))
                    return (T)(object)value_int;
                else
                    return input;
            }
            else if (input is float)
            {
                float value_int;
                if (float.TryParse(vm.Ini_Read(region, variable), out value_int))
                    return (T)(object)value_int;
                else
                    return input;
            }
            else if (input is long)
            {
                long value_int;
                if (long.TryParse(vm.Ini_Read(region, variable), out value_int))
                    return (T)(object)value_int;
                else
                    return input;
            }
            else if (input is byte)
            {
                byte value_int;
                if (byte.TryParse(vm.Ini_Read(region, variable), out value_int))
                    return (T)(object)value_int;
                else
                    return input;
            }
            else if (input is bool)
            {
                bool value_bool;
                if (bool.TryParse(vm.Ini_Read(region, variable), out value_bool))
                    return (T)(object)value_bool;
                else
                    return input;
            }

            return new T();
        }

        string arduino_ReadData;
        private void port_arduino_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                arduino_ReadData = port_arduino.ReadLine().Replace(@"\t|\n|\r", "");
            }
            catch { }
        }

        List<string> list_CalibrationItems;
        public void combox_setting(ComboBox obj)
        {
            obj.Items.Clear();
            switch (obj.Name)
            {
                case "ComBox_Calibration":
                    list_CalibrationItems = new List<string>() { "Calibration", "VOA -> 0", "TF -> 0", "K VOA", "K TF", "K TF (Detail)" };
                    break;
            }
            foreach (string s in list_CalibrationItems) { obj.Items.Add(s); }
        }

        bool commandCycleResult = false;
        public void btn_GO_Click(object sender, RoutedEventArgs e)
        {
            Main_Go();
        }

        public async void Main_Go()
        {
            try
            {
                if (vm.IsGoOn)
                {
                    vm.isStop = false;

                    if (vm.is_update_chart)
                    {
                        vm.int_chart_count++;
                        vm.int_chart_now = vm.int_chart_count;
                    }

                    vm.Chart_title = "Power x Time";
                    vm.PlotViewModel.Title = vm.Chart_title;
                    vm.Chart_x_title = "Time (s)"; //Set Chart x axis title
                    vm.Chart_y_title = vm.dB_or_dBm ? "IL (dB)" : "IL (dBm)";

                    vm.PlotViewModel.Axes[0].Title = vm.Chart_x_title;
                    vm.PlotViewModel.Axes[1].Title = vm.Chart_y_title;

                    vm.Str_Status = "Get Power";
                    if (!vm.isTimerOn)
                        vm.int_timer_timespan = int.MaxValue;
                    vm.timer2_count = 0;
                    vm.timer3_count = 0;
                    vm.watch = new System.Diagnostics.Stopwatch();
                    vm.watch.Start();
                    cmd.Clean_Chart();
                    vm.isConnected = false;

                    foreach (LineSeries ls in vm.Plot_Series)
                    {
                        ls.Points.Clear();
                    }

                    if (!vm.IsDistributedSystem)
                    {
                        if (!vm.isConnected && vm.PD_or_PM)  //檢查TLS是否連線，若無，則進行連線並取續TLS狀態
                            await cmd.Connect_TLS();

                        if (!vm.PD_or_PM)   //PD mode
                        {
                            vm.Cmd_Count = 0;

                            vm.ComMembers.Clear();
                            vm.Save_cmd(new ComMember() { No = vm.Cmd_Count++.ToString(), Command = "P0?" });

                            commandCycleResult = await cmd.CommandListCycle();
                        }
                        //PM mode
                        else
                        {
                            vm.timer3_count = 0;
                            vm.Cmd_Count = 0;

                            vm.ComMembers.Clear();

                            vm.Save_cmd(new ComMember() { No = vm.Cmd_Count++.ToString(), Command = "GETPOWER", Type = "PM", Channel = vm.switch_index.ToString() });

                            commandCycleResult = await cmd.CommandListCycle();
                        }
                    }

                    //Distributed System on
                    else
                    {
                        foreach (ChannelModel chM in vm.list_ChannelModels)
                        {
                            switch (chM.PM_Type)
                            {
                                case "GPIB":
                                    vm.Save_cmd(new ComMember() { No = vm.Cmd_Count++.ToString(), Command = "GETPOWER", Type = "PM", Channel = vm.switch_index.ToString() });
                                    break;

                                case "RS232":
                                    vm.Save_cmd(new ComMember() { No = vm.Cmd_Count++.ToString(), Channel = chM.channel.Replace("ch ", ""), Comport = chM.PM_Board_Port, Command = "GETPOWER" });
                                    break;
                            }

                        }
                        commandCycleResult = await cmd.CommandListCycle();
                    }
                }

                //Stop
                else
                {
                    if (!vm.IsDistributedSystem)
                    {
                        vm.Str_Status = "Stop";
                        if (!vm.PD_or_PM)
                            await cmd.Save_Chart();
                        else if (vm.PD_or_PM && vm.station_type == ComViewModel.StationTypes.Hermetic_Test)
                            await cmd.Save_Chart();
                    }

                    //Distributed System on
                    else
                    {
                        vm.Str_Status = "Stop";
                        vm.Chart_All_DataPoints = new List<List<DataPoint>>(vm.Save_All_PD_Value);
                        await cmd.Save_Chart();

                        for (int i = 0; i < 5; i++)
                        {
                            await Task.Delay(200);

                            if (commandCycleResult)
                            {
                                if (vm.port_PD != null)
                                {
                                    if (vm.port_PD.IsOpen)
                                    {
                                        vm.port_PD.DiscardInBuffer();
                                        vm.port_PD.DiscardOutBuffer();
                                        vm.port_PD.Close();
                                    }
                                }
                            }
                        }

                        commandCycleResult = false;
                    }
                }

                //Close TLS filter port for other control action
                cmd.Close_TLS_Filter();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.StackTrace.ToString());
            }
        }

        private async void action_go()
        {
            vm.IsGoOn = true;
            vm.isStop = false;

            if (!vm.IsDistributedSystem)  //如果Arduino未勾選
            {
                if (!vm.isConnected && vm.PD_or_PM)  //檢查TLS是否連線，若無，則進行連線並取續TLS狀態
                {
                    #region Tunable Laser setting
                    vm.tls = new HPTLS();
                    vm.tls.BoardNumber = vm.tls_BoardNumber;
                    vm.tls.Addr = vm.tls_Addr;

                    try
                    {
                        if (vm.tls.Open() == false)
                        {
                            vm.Str_cmd_read = "TLS GPIB Setting Error. Check Address.";
                            return;
                        }
                        vm.tls.init();

                        vm.Double_Laser_Wavelength = vm.tls.ReadWL();

                        vm.isConnected = true;
                    }
                    catch
                    {
                        vm.Str_cmd_read = "GPIB Setting Error.";
                    }
                    #endregion

                    #region PowerMeter Setting
                    //Power Meter setting
                    vm.pm = new HPPM();
                    vm.pm.Addr = vm.tls_Addr;
                    vm.pm.Slot = vm.PM_slot;
                    vm.pm.BoardNumber = vm.tls_BoardNumber;
                    if (vm.pm.Open() == false)
                    {
                        vm.Str_cmd_read = "PM GPIB Setting Error.  Check  Address.";
                        return;
                    }
                    vm.pm.init();
                    vm.pm.setUnit(1);
                    vm.pm.AutoRange(true);
                    vm.pm.aveTime(vm.PM_AveTime);
                    #endregion
                }

                if (vm.is_update_chart)
                {
                    vm.int_chart_count++;
                    vm.int_chart_now = vm.int_chart_count;
                }

                vm.ChartNowModel = new ChartModel(vm.ch_count);
                vm.ChartNowModel.list_delta_IL.AddRange(Enumerable.Repeat(0.0, vm.ch_count));
                vm.Chart_title = "Power X Time";
                vm.Chart_x_title = "Time(s)"; //Set Chart x axis title
                vm.Str_Status = "Get Power";
                vm.int_timer_timespan = int.MaxValue;
                vm.watch = new System.Diagnostics.Stopwatch();
                vm.watch.Start();
                cmd.Clean_Chart();

                if (!vm.PD_or_PM)   //PD mode
                {
                    vm.Cmd_Count = 0;

                    vm.ComMembers.Clear();
                    vm.Save_cmd(new ComMember() { No = vm.Cmd_Count++.ToString(), Command = "P0?" });

                    if (vm.station_type == ComViewModel.StationTypes.Chamber_S_16ch)
                        vm.CommandListCycle_16ch();
                    else if (vm.station_type == ComViewModel.StationTypes.Fast_Calibration)
                    {
                        vm.Chart_y_title = "Power (PD dac)";
                        await cmd.CommandListCycle();
                    }
                    else
                        await cmd.CommandListCycle();
                }

                else  //PM mode
                {
                    vm.timer3_count = 0;
                    vm.Cmd_Count = 0;

                    vm.ComMembers.Clear();

                    vm.Save_cmd(new ComMember() { No = vm.Cmd_Count++.ToString(), Command = "GETPOWER", Type = "PM", Channel = vm.switch_index.ToString() });

                    await cmd.CommandListCycle();
                }
            }
            else  //arduino on
            {
                foreach (ChannelModel chM in vm.list_ChannelModels)
                {
                    switch (chM.PM_Type)
                    {
                        case "GPIB":
                            vm.Save_cmd(new ComMember() { No = vm.Cmd_Count++.ToString(), Command = "GETPOWER", Type = "PM", Channel = vm.switch_index.ToString() });

                            break;

                        case "RS232":
                            vm.Save_cmd(new ComMember() { No = vm.Cmd_Count++.ToString(), Channel = chM.channel.Replace("ch ", ""), Comport = chM.PM_Board_Port, Command = "GETPOWER" });

                            break;
                    }
                }
                await cmd.CommandListCycle();
            }
        }

        private async void Timer2_PD_GO_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (vm.port_PD.IsOpen)
                {
                    if (vm.IsGoOn)
                    {
                        //根據Command List最後一個資料下指令
                        if (vm.ComMembers.Count > 0)
                        {
                            ComMember cm = vm.ComMembers.Last();
                            string c = cm.Command;

                            //根據指令決定動作
                            if (cm != null)
                                if (!string.IsNullOrEmpty(cm.Command))
                                    await cmd.CommandSwitch(cm);

                            //清除指令   
                            try { vm.isDeleteCmd = true; }
                            catch { vm.Save_Log("remove member error"); }
                        }
                    }
                }
            }
            catch
            {
                vm.Str_cmd_read = "Timer elaspe error";
            }
        }

        private async void Timer3_PM_GO_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (vm.IsGoOn)
                {
                    //根據Command List最後一個資料下指令
                    if (vm.ComMembers.Count > 0)
                    {
                        ComMember cm = vm.ComMembers.Last();
                        string c = cm.Command;

                        //根據指令決定動作
                        if (cm != null)
                            if (!string.IsNullOrEmpty(cm.Command))
                                await cmd.CommandSwitch(cm);

                        //清除指令   
                        try { vm.isDeleteCmd = true; }
                        catch { vm.Save_Log("remove member error"); }
                    }
                }
            }
            catch
            {
                vm.Str_cmd_read = "Timer elaspe error";
            }
        }

        private async void btn_comment_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtBox_comment.Text)) //Check comment box is empty or not
                return;

            bool _isGoOn_On = vm.IsGoOn;

            await vm.Port_ReOpen(vm.Selected_Comport);
            vm.Str_Command = txtBox_comment.Text;
            await Cmd_Write_RecieveData(vm.Str_Command, true, 0);

            if (_isGoOn_On)
                await vm.PD_GO();
        }

        private async void btn_ID_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(vm.Selected_Comport)) return;

                if (vm.IsGoOn)
                {
                    if (!vm.PD_or_PM)
                    {
                        vm.Save_cmd(new ComMember()
                        {
                            Type = "PD",
                            Command = "ID?",
                            Comport = vm.Selected_Comport,
                            No = vm.Cmd_Count.ToString()
                        });
                    }
                    else
                    {
                        vm.Save_cmd(new ComMember()
                        {
                            YN = true,
                            Type = "PM",
                            Command = "ID?",
                            Comport = vm.Selected_Comport,
                            No = vm.Cmd_Count.ToString()
                        });
                    }
                }
                else
                {
                    await vm.Port_ReOpen(vm.Selected_Comport);

                    vm.Str_Command = "ID?";
                    await Cmd_Write_RecieveData(vm.Str_Command, true, 0);

                    #region Get Board Name     
                    if (vm.Str_cmd_read == "DiCon Fiberoptics Inc, MEMS UFA")
                    {
                        try
                        {
                            rs232 = new DiCon.UCB.Communication.RS232.RS232(vm.Selected_Comport);
                            rs232.OpenPort();
                            icomm = (ICommunication)rs232;

                            tf = new DiCon.UCB.MTF.RS232.RS232(icomm);

                            string str_ID = string.Empty;

                            str_ID = tf.ReadSN();
                            vm.Str_Status = str_ID;

                            await Task.Delay(125);
                            rs232.ClosePort();
                        }
                        catch { }
                    }
                    #endregion

                    vm.Save_Log(new LogMember()
                    {
                        Status = vm.Str_Status,
                        Message = vm.Str_cmd_read
                    });
                }
            }
            catch { }
        }

        private async void btn_D0_Click(object sender, RoutedEventArgs e)
        {
            anly.JudgeAllBoolGauge();
            await D0_show();
        }


        private async Task D0_show()
        {
            if (!vm.PD_or_PM)  //PD mode
            {
                for (int i = 1; i <= vm.ch_count; i++)
                {
                    if (vm.list_GaugeModels[i - 1].boolGauge || vm.BoolAllGauge)
                    {
                        int channel = i;
                        if (vm.IsGoOn)
                        {
                            if (vm.station_type == ComViewModel.StationTypes.Chamber_S_16ch)
                            {
                                vm.Save_cmd(new ComMember()
                                {
                                    No = vm.Cmd_Count.ToString(),
                                    Type = "PD",
                                    Comport = vm.PD_A_ChannelModel.Board_Port,
                                    Command = "DAC?",
                                    Channel = i.ToString()
                                });

                                vm.Save_cmd(new ComMember()
                                {
                                    No = vm.Cmd_Count.ToString(),
                                    Type = "PD",
                                    Comport = vm.PD_B_ChannelModel.Board_Port,
                                    Command = "DAC?",
                                    Channel = i.ToString()
                                });
                            }
                            else
                                vm.Save_cmd(new ComMember()
                                {
                                    No = vm.Cmd_Count.ToString(),
                                    Type = "PD",
                                    Comport = vm.Selected_Comport,
                                    Command = "DAC?",
                                    Channel = i.ToString()
                                });
                            //vm.Cmd_Count++;
                        }
                        else
                        {
                            try
                            {
                                if (i > 8 && i < 17) channel -= 8;
                                string cmd = "D" + channel.ToString() + "?";
                                if (vm.station_type == ComViewModel.StationTypes.Chamber_S_16ch)
                                {
                                    try { await vm.Port_ReOpen(vm.PD_A_ChannelModel.Board_Port); } catch { }

                                    try { await vm.Port_PD_B_ReOpen(vm.PD_B_ChannelModel.Board_Port); } catch { }

                                    await _cmd_write_recieveData_ForD0(cmd);
                                }
                                else
                                {
                                    await vm.Port_ReOpen(vm.Selected_Comport);

                                    await _cmd_write_recieveData_ForD0(cmd);

                                }
                                if (!vm.IsGoOn) vm.port_PD.Close();
                            }
                            catch { vm.Save_Log("Port is closed"); return; }
                        }
                    }
                }
            }

            else  //PM mode
            {
                if (vm.station_type == ComViewModel.StationTypes.Hermetic_Test)
                {
                    for (int ch = 0; ch < vm.ch_count; ch++)
                    {
                        if (!vm.list_GaugeModels[ch].boolGauge && !vm.BoolAllGauge) continue;

                        if (!string.IsNullOrEmpty(vm.SNMembers[ch].ProductType))
                        {
                            if (!vm.SNMembers[ch].ProductType.Contains("UFA")) continue;
                        }
                        else continue;

                        string cmd = "D1?";

                        if (!string.IsNullOrEmpty(vm.list_Board_Setting[ch][1]))
                        {
                            str_selected_com = vm.list_Board_Setting[ch][1];
                            vm.Save_Log("K V3", (ch + 1).ToString(), "Board:" + str_selected_com);
                            //MessageBox.Show("Ch:" + ch.ToString() + ", Board Com:" + str_selected_com);
                            await vm.Port_ReOpen(str_selected_com);
                            //if (vm.port_PD.PortName != str_selected_com)
                            //{
                            //    if (vm.port_PD.IsOpen)
                            //        vm.port_PD.Close();
                            //    vm.port_PD.PortName = str_selected_com;

                            //}
                        }
                        else
                        {
                            vm.list_D_All.Add(new ObservableCollection<string>());  //Add one channel list to All channel list
                            continue;
                        }

                        try
                        {
                            if (!vm.IsGoOn)  //Go is off
                            {
                                await anly.Port_ReOpen();
                                await _cmd_write_recieveData_ForD0(cmd);
                            }
                            else  //Go is on
                            {
                                vm.Save_cmd(new ComMember()
                                {
                                    YN = true,
                                    No = vm.Cmd_Count.ToString(),
                                    Type = "PM",
                                    Command = CommandList.DAC,
                                    Comport = vm.Selected_Comport
                                });
                            }
                        }
                        catch { vm.Save_Log(str_selected_com + " is closed"); return; }
                    }
                }
                else
                {
                    for (int ch = 0; ch < vm.ch_count; ch++)
                    {
                        string cmd = "D1?";

                        try
                        {
                            if (!vm.IsGoOn)  //Go is off
                            {
                                await vm.Port_ReOpen(vm.Selected_Comport);
                                await _cmd_write_recieveData_ForD0(cmd);
                            }
                            else  //Go is on
                            {
                                vm.Save_cmd(new ComMember()
                                {
                                    YN = true,
                                    No = vm.Cmd_Count.ToString(),
                                    Type = "PM",
                                    Command = CommandList.DAC,
                                    Comport = vm.Selected_Comport
                                });
                            }
                        }
                        catch { vm.Save_Log(vm.Selected_Comport + " is closed"); return; }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComBox_Calibration_DropDownOpened(object sender, EventArgs e)
        {
            ComboBox obj = (ComboBox)sender;
            int_saved_combox_index = obj.SelectedIndex;
        }

        public async void PD_or_PM_Go(bool _isGoOn_On)
        {
            if (vm.PD_or_PM == false)  //PD mode
            {
                if (_isGoOn_On)   //Keep Going
                    await vm.PD_GO();
                else   //Keep Stop
                {
                    vm.Str_Command = "P0?";
                    await Cmd_Write_RecieveData(vm.Str_Command, true, 0);
                }
            }
            else   //PM mode
            {
                if (_isGoOn_On) vm.PM_GO();
                else vm.Str_cmd_read = vm.pm.ReadPower().ToString();
            }
        }

        /// <summary>
        /// ComBox_Calibration
        /// </summary>
        /// <param name="sender">object</param>
        /// <param name="e">EventArgs</param>
        private async void ComBox_Calibration_DropDownClosed(object sender, EventArgs e)
        {
            ComboBox obj = (ComboBox)sender;

            //if (obj.SelectedIndex == int_saved_combox_index) return;

            bool _isGoOn_On = vm.IsGoOn;
            //vm.IsGoOn = false;

            await Task.Delay(vm.Int_Read_Delay * 2);

            vm.isStop = false;

            obj.IsEnabled = false;  //防呆
            btn_GO.IsEnabled = false;

            switch (obj.SelectedItem.ToString())
            {
                case "DAC -> 0":
                    if (vm.list_GaugeModels.Count > 0)
                    {
                        anly.JudgeAllBoolGauge();
                        await Reset_DAC();
                        await Task.Delay(vm.Int_Read_Delay * 3);
                        await D0_show();
                    }
                    break;

                case "VOA -> 0":
                    if (vm.list_GaugeModels.Count > 0)
                    {
                        anly.JudgeAllBoolGauge();
                        await Reset_VOA();
                        await D0_show();
                    }
                    break;

                case "TF -> 0":
                    if (vm.list_GaugeModels.Count > 0)
                    {
                        anly.JudgeAllBoolGauge();
                        await Reset_TF();
                        await D0_show();
                    }
                    break;

                case "K WEP":
                    break;

                case "K VOA":
                    await K_V3();
                    break;

                case "K TF":
                    if (vm.selected_K_WL_Type.Equals("ALL Range"))
                        await K_TF_Step();  //Step Scan  
                    else if (vm.selected_K_WL_Type.Equals("Human Like"))
                        await K_TF_Iteration();

                    await cmd.Save_Chart();
                    break;
                case "K WL":
                    #region initial setting
                    vm.Chart_All_DataPoints.Clear();
                    vm.Chart_DataPoints.Clear();
                    vm.Save_All_PD_Value.Clear();

                    bool pre_GO_status = vm.IsGoOn;
                    vm.IsGoOn = false;
                    vm.isStop = false;

                    //ComBox_Calibration.IsEditable = false;
                    K_WL.IsEnabled = false;  //防呆
                    btn_GO.IsEnabled = false;

                    if (vm.List_bear_say != null) vm.List_bear_say = new List<List<string>>();

                    for (int i = 0; i < vm.list_GaugeModels.Count; i++)
                    {
                        vm.list_GaugeModels[i].GaugeValue = string.Empty;
                        vm.list_GaugeModels[i].GaugeBearSay_1 = string.Empty;
                        vm.list_GaugeModels[i].GaugeBearSay_2 = string.Empty;
                        vm.list_GaugeModels[i].GaugeBearSay_3 = string.Empty;
                        vm.list_GaugeModels[i].DataPoints.Clear();
                    }

                    anly.JudgeAllBoolGauge();
                    #endregion

                    if (vm.selected_K_WL_Type == "ALL Range") await WL_Scan();
                    else if (vm.selected_K_WL_Type == "Human Like")
                    {
                        await WL_Scan_Iteration();
                    }

                    vm.list_collection_GaugeModels.Add(new ObservableCollection<GaugeModel>());
                    for (int i = 0; i < vm.list_GaugeModels.Count; i++)
                    {
                        vm.list_collection_GaugeModels.Last().Add(new GaugeModel(vm.list_GaugeModels[i]));
                    }

                    vm.bear_say_all++;
                    vm.bear_say_now = vm.bear_say_all;

                    //ComBox_Calibration.IsEditable = true;
                    K_WL.IsEnabled = true;  //取消防呆
                    btn_GO.IsEnabled = true;

                    if (pre_GO_status)
                        action_go();
                    break;

                case "WL Scan":

                    break;

                case "ReadData":
                    string messageBoxText = "是否讀取文字檔?";
                    string caption = "氣密資料";
                    MessageBoxButton button = MessageBoxButton.YesNoCancel;
                    MessageBoxImage icon = MessageBoxImage.Warning;
                    MessageBoxResult result = MessageBox.Show(messageBoxText, caption, button, icon);

                    for (int i = 0; i < vm.list_GaugeModels.Count; i++)
                    {
                        vm.list_SN[i] = vm.list_GaugeModels[i].GaugeSN;
                    }

                    if (result == MessageBoxResult.Yes)
                        for (int i = 0; i < vm.list_SN.Count; i++)
                        {
                            if (string.IsNullOrEmpty(vm.list_SN[i])) continue;

                            string filepath = string.Concat(vm.txt_save_wl_data_path, vm.list_SN[i], ".txt");

                            if (File.Exists(filepath))
                            {
                                try
                                {
                                    System.Diagnostics.Process.Start(filepath);
                                    vm.Save_Log("Open file", vm.list_SN[i], (i + 1).ToString());
                                }
                                catch
                                {
                                    vm.Str_cmd_read = vm.list_SN[i] + ".txt is opening.";
                                    vm.Save_Log("Open file", vm.list_SN[i] + ".txt is opening.", (i + 1).ToString());
                                }
                            }
                            else
                            {
                                vm.Str_cmd_read = vm.list_SN[i] + ".txt is not exist.";
                                vm.Save_Log("Open file", vm.list_SN[i] + ".txt is not exist.", (i + 1).ToString());
                            }
                        }
                    break;

                case "Calibration":
                    //Get calibration csv file 
                    if (!File.Exists(vm.txt_Calibration_Csv_Path))
                    {
                        vm.Save_Log(new LogMember() { Message = "csv file is not exist" });
                        return;
                    }

                    List<string> list_calibrationCsv_wl = new List<string>();
                    using (DataTable dtt = CSVFunctions.Read_CSV(vm.txt_Calibration_Csv_Path))
                    {
                        if (dtt != null)
                        {
                            if (dtt.Rows.Count != 0 && dtt.Columns.Count >= 2)
                            {
                                for (int i = 6; i < dtt.Rows.Count; i++)
                                {
                                    list_calibrationCsv_wl.Add(dtt.Rows[i][1].ToString());
                                }
                            }
                        }
                    }


                    break;
            }

            ComBox_Calibration.SelectedIndex = 0;
            obj.IsEnabled = true;
            btn_GO.IsEnabled = true;
        }

        private async Task Reset_DAC()
        {
            if (!vm.PD_or_PM)  //PD mode
            {
                if (vm.IsGoOn)
                {
                    vm.Save_cmd(new ComMember()
                    {
                        No = vm.Cmd_Count.ToString(),
                        Type = "PD",
                        Command = "Delay",
                        Value_1 = 120.ToString()
                    });

                    for (int ch = 1; ch <= vm.ch_count; ch++)  //Set dec from ch1 to ch8
                    {
                        if (!vm.isStop)
                        {
                            if (vm.list_GaugeModels[ch - 1].boolGauge || vm.BoolAllGauge)
                            {
                                GaugeModel gm = vm.list_GaugeModels[ch - 1];
                                vm.Save_cmd(new ComMember()
                                {
                                    No = vm.Cmd_Count.ToString(),
                                    Type = "PD",
                                    Comport = vm.Selected_Comport,
                                    Channel = ch.ToString(),
                                    Command = "WRITEDAC",
                                    Value_1 = "0",
                                    Value_2 = "0",
                                    Value_3 = gm.GaugeD0_3,
                                });
                                //vm.Cmd_Count++;

                                vm.Save_cmd(new ComMember()
                                {
                                    No = vm.Cmd_Count.ToString(),
                                    Type = "PD",
                                    Command = "Delay",
                                    Value_1 = vm.Int_Write_Delay.ToString().ToString()
                                });
                                //vm.Cmd_Count++;
                            }
                        }
                        else break;
                    }
                }
                else
                {
                    if (vm.port_PD != null)
                    {
                        if (!vm.port_PD.IsOpen)
                            await vm.Port_ReOpen(vm.Selected_Comport);
                    }
                    else
                    {
                        vm.Save_Log(new LogMember()
                        {
                            Status = "Reset DAC",
                            Message = "Port is null"
                        });
                        return;
                    }

                    for (int ch = 1; ch <= vm.ch_count; ch++)  //Set dec from ch1 to ch8
                    {
                        if (!vm.isStop)
                        {
                            if (vm.list_GaugeModels[ch - 1].boolGauge || vm.BoolAllGauge)
                            {
                                try
                                {
                                    vm.Str_Command = string.Format("D{0} {1}", ch.ToString(), 0);
                                    vm.port_PD.Write(vm.Str_Command + "\r");
                                    vm.list_GaugeModels[ch - 1].GaugeD0_3 = "0";
                                    await Task.Delay(vm.Int_Write_Delay);
                                }
                                catch { vm.Str_cmd_read = "Reset DAC Error"; return; }
                            }
                        }
                        else break;
                    }
                }
            }
            //PM mode
            else
            {
                if (vm.station_type == ComViewModel.StationTypes.Hermetic_Test)
                {
                    for (int i = 0; i < vm.ch_count; i++)
                    {
                        string com = vm.list_Board_Setting[i][1];

                        if (!vm.Bool_Gauge[i]) continue;

                        if (string.IsNullOrWhiteSpace(com)) continue;

                        await vm.Port_ReOpen(com);

                        try
                        {
                            vm.Str_Command = "D1 0,0,0";
                            vm.port_PD.Write(vm.Str_Command + "\r");
                            await Task.Delay(vm.Int_Write_Delay);
                        }
                        catch { vm.Str_cmd_read = "Reset DAC Error"; return; }

                        await Task.Delay(vm.Int_Read_Delay);
                    }
                }
                else
                {
                    GaugeModel gm = vm.list_GaugeModels[0];
                    if (vm.IsGoOn)
                    {
                        string D0_1, D0_2;
                        D0_1 = gm.GaugeD0_1;
                        D0_2 = gm.GaugeD0_2;

                        string[] dac = await anly.Analyze_PreDAC(vm.Selected_Comport, "1");

                        vm.Save_cmd(new ComMember()
                        {
                            YN = true,
                            No = vm.Cmd_Count.ToString(),
                            Type = "PM",
                            Comport = vm.Selected_Comport,
                            Command = CommandList.WriteDac,
                            Value_1 = "0",
                            Value_2 = "0",
                            Value_3 = "0"
                        });
                    }
                    else
                    {
                        gm.GaugeD0_1 = "0";
                        gm.GaugeD0_2 = "0";
                        gm.GaugeD0_3 = "0";
                        cmd.Set_Dac(vm.Selected_Comport, gm);
                    }
                }
            }
        }

        private async Task Reset_VOA()
        {
            if (!vm.PD_or_PM)  //PD mode
            {
                if (vm.IsGoOn)
                {
                    vm.Save_cmd(new ComMember()
                    {
                        No = vm.Cmd_Count.ToString(),
                        Type = "PD",
                        Command = "Delay",
                        Value_1 = 120.ToString()
                    });

                    for (int ch = 1; ch <= vm.ch_count; ch++)  //Set dec from ch1 to ch8
                    {
                        if (!vm.isStop)
                        {
                            if (vm.list_GaugeModels[ch - 1].boolGauge || vm.BoolAllGauge)
                            {
                                GaugeModel gm = vm.list_GaugeModels[ch - 1];
                                vm.Save_cmd(new ComMember()
                                {
                                    No = vm.Cmd_Count.ToString(),
                                    Type = "PD",
                                    Comport = vm.Selected_Comport,
                                    Channel = ch.ToString(),
                                    Command = "WRITEDAC",
                                    Value_1 = gm.GaugeD0_1,
                                    Value_2 = gm.GaugeD0_2,
                                    Value_3 = "0"
                                });
                                //vm.Cmd_Count++;

                                vm.Save_cmd(new ComMember()
                                {
                                    No = vm.Cmd_Count.ToString(),
                                    Type = "PD",
                                    Command = "Delay",
                                    Value_1 = vm.Int_Write_Delay.ToString().ToString()
                                });
                                //vm.Cmd_Count++;
                            }
                        }
                        else break;
                    }
                }
                else
                {
                    if (vm.port_PD != null)
                    {
                        if (!vm.port_PD.IsOpen)
                            await vm.Port_ReOpen(vm.Selected_Comport);
                    }
                    else
                    {
                        vm.Save_Log(new LogMember()
                        {
                            Status = "Reset VOA",
                            Message = "Port is null"
                        });
                        return;
                    }

                    for (int ch = 1; ch <= vm.ch_count; ch++)  //Set dec from ch1 to ch8
                    {
                        if (!vm.isStop)
                        {
                            if (vm.list_GaugeModels[ch - 1].boolGauge || vm.BoolAllGauge)
                            {
                                try
                                {
                                    vm.Str_Command = string.Format("VOA{0} {1}", ch.ToString(), 0);
                                    vm.port_PD.Write(vm.Str_Command + "\r");
                                    vm.list_GaugeModels[ch - 1].GaugeD0_3 = "0";
                                    await Task.Delay(vm.Int_Write_Delay);
                                }
                                catch { vm.Str_cmd_read = "Reset VOA Error"; return; }
                            }
                        }
                        else break;
                    }
                }
            }
            //PM mode
            else
            {
                if (vm.station_type == ComViewModel.StationTypes.Testing)
                {
                    GaugeModel gm = vm.list_GaugeModels[0];
                    if (vm.IsGoOn)
                    {
                        string D0_1, D0_2;
                        D0_1 = gm.GaugeD0_1;
                        D0_2 = gm.GaugeD0_2;

                        string[] dac = await anly.Analyze_PreDAC(vm.Selected_Comport, "1");

                        vm.Save_cmd(new ComMember()
                        {
                            YN = true,
                            No = vm.Cmd_Count.ToString(),
                            Type = "PM",
                            Comport = vm.Selected_Comport,
                            Command = CommandList.WriteDac,
                            Value_1 = dac[0],
                            Value_2 = dac[1],
                            Value_3 = "0"
                        });
                    }
                    else
                    {
                        cmd.Set_Dac(vm.Selected_Comport, gm);
                    }
                }
                else if (vm.station_type == ComViewModel.StationTypes.Hermetic_Test)
                {
                    for (int i = 0; i < vm.ch_count; i++)
                    {
                        string com = vm.list_Board_Setting[i][1];

                        if (!vm.Bool_Gauge[i]) continue;

                        if (string.IsNullOrWhiteSpace(com)) continue;

                        await vm.Port_ReOpen(com);

                        try
                        {
                            vm.Str_Command = "D1 0,0,0";
                            vm.port_PD.Write(vm.Str_Command + "\r");
                            await Task.Delay(vm.Int_Write_Delay);
                        }
                        catch { vm.Str_cmd_read = "Reset VOA Error"; return; }

                        await Task.Delay(vm.Int_Read_Delay);
                    }
                }
            }
        }

        private async Task Reset_TF()
        {
            //PD mode
            if (!vm.PD_or_PM)
            {
                if (vm.IsGoOn)
                {
                    vm.Save_cmd(new ComMember()
                    {
                        No = vm.Cmd_Count.ToString(),
                        Type = "PD",
                        Command = "Delay",
                        Value_1 = 120.ToString()
                    });

                    for (int ch = 1; ch <= vm.ch_count; ch++)  //Set dec from ch1 to ch8
                    {
                        if (!vm.isStop)
                        {
                            if (vm.list_GaugeModels[ch - 1].boolGauge || vm.BoolAllGauge)
                            {
                                GaugeModel gm = vm.list_GaugeModels[ch - 1];
                                vm.Save_cmd(new ComMember()
                                {
                                    No = vm.Cmd_Count.ToString(),
                                    Type = "PD",
                                    Comport = vm.Selected_Comport,
                                    Channel = ch.ToString(),
                                    Command = "WRITEDAC",
                                    Value_1 = "0",
                                    Value_2 = "0",
                                    Value_3 = gm.GaugeD0_3
                                });
                                //vm.Cmd_Count++;

                                vm.Save_cmd(new ComMember()
                                {
                                    No = vm.Cmd_Count.ToString(),
                                    Type = "PD",
                                    Command = "Delay",
                                    Value_1 = vm.Int_Write_Delay.ToString()
                                });
                                //vm.Cmd_Count++;
                            }
                        }
                        else break;
                    }
                }
                else
                {
                    if (vm.port_PD != null)
                    {
                        if (!vm.port_PD.IsOpen)
                            await vm.Port_ReOpen(vm.Selected_Comport);
                    }
                    else
                    {
                        vm.Save_Log(new LogMember()
                        {
                            Status = "Reset TF",
                            Message = "Port is null"
                        });
                        return;
                    }

                    for (int ch = 1; ch <= vm.ch_count; ch++)  //Set dec from ch1 to ch8
                    {
                        if (!vm.isStop)
                        {
                            if (vm.list_GaugeModels[ch - 1].boolGauge || vm.BoolAllGauge)
                            {
                                try
                                {
                                    vm.Str_Command = string.Format("D{0} {1}", ch.ToString(), 0);
                                    vm.port_PD.Write(vm.Str_Command + "\r");
                                    vm.list_GaugeModels[ch - 1].GaugeD0_1 = "0";
                                    vm.list_GaugeModels[ch - 1].GaugeD0_2 = "0";
                                    await Task.Delay(vm.Int_Write_Delay);
                                }
                                catch { vm.Str_cmd_read = "Reset VOA Error"; return; }
                            }
                        }
                        else break;
                    }
                    await Task.Delay(vm.Int_Read_Delay);
                    await D0_show();
                }
            }

            //PM mode
            else
            {
                for (int ch = 0; ch < vm.ch_count; ch++)  //Set dec from ch1 to ch8
                {
                    GaugeModel gm = vm.list_GaugeModels[ch];
                    if (!vm.IsGoOn)
                    {
                        gm.GaugeD0_1 = "0";
                        gm.GaugeD0_2 = "0";
                        cmd.Set_Dac(vm.Selected_Comport, gm);

                        await Task.Delay(vm.Int_Read_Delay);
                        await D0_show();
                    }
                    else
                    {
                        vm.Save_cmd(new ComMember()
                        {
                            YN = true,
                            No = vm.Cmd_Count.ToString(),
                            Type = "PM",
                            Comport = vm.Selected_Comport,
                            Command = CommandList.WriteDac,
                            Value_1 = "0",
                            Value_2 = "0",
                            Value_3 = gm.GaugeD0_3
                        });
                    }
                }
            }

        }

        private async Task<List<string>> K_V3()
        {
            #region Initial Setting      
            cmd.Clean_Chart();
            List<string> list_final_voltage = new List<string>();
            vm.Chart_title = "Power X DAC";
            vm.Chart_x_title = "DAC"; //Rename Chart x axis title
            vm.Chart_y_title = vm.dB_or_dBm ? "IL (dB)" : $"IL (dBm)";

            vm.Update_ALL_PlotView_Title(vm.Chart_title, vm.Chart_x_title, vm.Chart_y_title);

            vm.Str_Status = "Calibration VOA";
            vm.Double_Powers = new List<double>();
            vm.Double_Powers.AddRange(Enumerable.Repeat(0.0, vm.ch_count));
            var watch = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < vm.ch_count; i++)
            {
                vm.Plot_Series[i].Points.Clear();
                vm.ChartNowModel.list_dataPoints[i].Clear();
            }

            #endregion

            List<int> list_scanDac = new List<int>();

            if (vm.int_V3_scan_start < vm.int_V3_scan_end)
            {
                for (int dac = vm.int_V3_scan_start; dac < vm.int_V3_scan_end; dac = dac + vm.int_V3_scan_gap)
                {
                    list_scanDac.Add(dac);
                }
            }
            else
            {
                for (int dac = vm.int_V3_scan_start; dac < vm.int_V3_scan_end; dac = dac - vm.int_V3_scan_gap)
                {
                    list_scanDac.Add(dac);
                }
            }

            anly.JudgeAllBoolGauge();

            //PD mode, sacn all at the time
            if (!vm.PD_or_PM && vm.BoolAllGauge)
            {
                if (!vm.port_PD.IsOpen)
                    await vm.Port_ReOpen(vm.Selected_Comport);

                foreach (int dac in list_scanDac)
                {
                    if (vm.isStop) break;

                    //Write Dac
                    try
                    {
                        vm.Str_cmd_read = dac.ToString();
                        for (int ch = 0; ch < vm.ch_count; ch++)
                        {
                            vm.port_PD.Write(string.Format("VOA{0} {1}\r", (ch + 1), dac));

                            vm.list_GaugeModels[ch].GaugeD0_3 = dac.ToString();

                            await Task.Delay(vm.Int_Write_Delay);//wait for chip stable

                            if (dac.Equals(vm.int_V3_scan_start))
                                await Task.Delay(120);  //first dac need more time for chip stable
                        }
                    }
                    catch
                    {
                        vm.Save_Log("K V3", "Write Dac Error", true);
                        vm.isStop = true;
                        return new List<string>();
                    }

                    //Get Power
                    double power = 0;
                    try
                    {
                        await cmd.Get_Power();
                        //await anly.Cmd_Write_RecieveData("P0?", false);

                        for (int ch = 0; ch < vm.ch_count; ch++)
                        {
                            power = vm.Double_Powers[ch];
                            vm.list_GaugeModels[ch].GaugeValue = Math.Round(power, 4).ToString();

                            DataPoint dp = new DataPoint(dac, power);
                            vm.ChartNowModel.list_dataPoints[ch].Add(dp);

                            #region Update Chart
                            cmd.Update_Chart(dac, power, ch);
                            #endregion

                            #region Cal. Delta IL    
                            cmd.Update_DeltaIL(vm.ChartNowModel.list_dataPoints[0].Count);
                            #endregion
                        }
                    }
                    catch
                    {
                        vm.Str_cmd_read = "Read Power Meter Step Error."; return new List<string>();
                    }
                }

                //Findout Max power
                try
                {
                    for (int i = 0; i < vm.ch_count; i++)
                    {
                        List<DataPoint> data = vm.ChartNowModel.list_dataPoints[i].Where(x => x.Y == vm.ChartNowModel.list_dataPoints[i].Max(y => y.Y)).ToList();
                        string command = string.Format("VOA{0} {1}", i + 1, data.First().X);
                        vm.port_PD.Write(command + "\r");
                        await Task.Delay(vm.Int_Read_Delay);
                        vm.list_GaugeModels[i].GaugeD0_3 = data.First().X.ToString();
                        vm.list_GaugeModels[i].GaugeValue = data.First().Y.ToString();
                    }
                }
                catch { };
            }

            //PD selected mode
            else if (!vm.PD_or_PM && !vm.BoolAllGauge)
            {
                if (!vm.port_PD.IsOpen)
                    await vm.Port_ReOpen(vm.Selected_Comport);

                foreach (int dac in list_scanDac)
                {
                    if (vm.isStop) break;

                    //Write Dac
                    try
                    {
                        vm.Str_cmd_read = dac.ToString();
                        for (int ch = 0; ch < vm.ch_count; ch++)
                        {
                            if (vm.list_GaugeModels[ch].boolGauge)
                            {
                                vm.port_PD.Write(string.Format("VOA{0} {1}\r", (ch + 1), dac));

                                vm.list_GaugeModels[ch].GaugeD0_3 = dac.ToString();

                                await Task.Delay(vm.Int_Write_Delay);//wait for chip stable

                                if (dac.Equals(vm.int_V3_scan_start))
                                    await Task.Delay(vm.Int_Write_Delay + 50);  //first dac need more time for chip stable
                            }
                        }
                    }
                    catch
                    {
                        vm.Save_Log("K V3", "Write Dac Error", true);
                        vm.isStop = true;
                        return new List<string>();
                    }

                    //Get Power
                    double power = 0;
                    try
                    {
                        await cmd.Get_Power();

                        for (int ch = 0; ch < vm.ch_count; ch++)
                        {
                            if (vm.list_GaugeModels[ch].boolGauge)
                            {
                                power = vm.Double_Powers[ch];
                                vm.list_GaugeModels[ch].GaugeValue = Math.Round(power, 4).ToString();

                                DataPoint dp = new DataPoint(dac, power);
                                vm.ChartNowModel.list_dataPoints[ch].Add(dp);

                                #region Set Chart data points
                                cmd.Update_Chart(dac, power, ch);
                                #endregion

                                #region Cal. Delta IL    
                                cmd.Update_DeltaIL(vm.ChartNowModel.list_dataPoints[ch].Count);
                                #endregion
                            }
                        }
                    }
                    catch
                    {
                        vm.Str_cmd_read = "Read Power Meter Step Error."; return new List<string>();
                    }
                }

                //Findout Max power
                try
                {
                    for (int i = 0; i < vm.ch_count; i++)
                    {
                        if (vm.list_GaugeModels[i].boolGauge)
                        {
                            List<DataPoint> data = vm.ChartNowModel.list_dataPoints[i].Where(x => x.Y == vm.ChartNowModel.list_dataPoints[i].Max(y => y.Y)).ToList();
                            string command = string.Format("VOA{0} {1}", i + 1, data.First().X);
                            vm.port_PD.Write(command + "\r");
                            await Task.Delay(vm.Int_Read_Delay);
                            vm.list_GaugeModels[i].GaugeD0_3 = data.First().X.ToString();
                            vm.list_GaugeModels[i].GaugeValue = data.First().Y.ToString();
                        }
                    }
                }
                catch { };
            }

            //PM mode , scan one by one
            else
            {
                for (int ch = 0; ch < vm.ch_count; ch++)
                {
                    if (vm.list_GaugeModels[ch].boolGauge || vm.BoolAllGauge)
                    {
                        //根據選擇的ch作動
                        List<int> maxpower_index = Analysis.ListDefault<int>(vm.ch_count);

                        #region Set Switch

                        if (vm.station_type == ComViewModel.StationTypes.Hermetic_Test)
                        {
                            vm.switch_index = ch + 1;

                            //switch re-open && Switch write Cmd
                            try
                            {
                                await vm.Port_Switch_ReOpen();

                                vm.Str_Command = "SW0 " + vm.switch_index.ToString();
                                vm.port_Switch.Write(vm.Str_Command + "\r");
                                await Task.Delay(vm.Int_Write_Delay);

                                vm.port_Switch.DiscardInBuffer();       // RX
                                vm.port_Switch.DiscardOutBuffer();      // TX

                                vm.port_Switch.Close();
                            }
                            catch (Exception ex)
                            {
                                vm.Str_cmd_read = "Set Switch Error";
                                vm.Save_Log(vm.Str_Status, vm.switch_index.ToString(), vm.Str_cmd_read);
                                MessageBox.Show(ex.StackTrace.ToString());
                            }
                        }

                        #endregion

                        if (vm.SN_Judge)
                        {
                            if (vm.station_type == ComViewModel.StationTypes.Hermetic_Test)
                            {
                                if (!string.IsNullOrEmpty(vm.SNMembers[ch].ProductType))
                                {
                                    vm.selected_band = vm.SNMembers[ch].LaserBand;
                                    if (!vm.SNMembers[ch].ProductType.Contains("UFA")) continue;
                                    else
                                    {
                                        //continue;
                                        vm.product_type = vm.SNMembers[ch].ProductType;
                                        setting.Product_Setting();
                                        cmd.Set_WL();
                                    }
                                }
                                else if (!vm.product_type.Equals("UFA")) continue;
                            }
                        }

                        if (vm.station_type == ComViewModel.StationTypes.Hermetic_Test)
                        {
                            vm.Selected_Comport = vm.list_Board_Setting[ch][1];

                            try
                            {
                                await vm.Port_ReOpen(vm.Selected_Comport);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.StackTrace.ToString());
                            }

                            //vm.port_PD.PortName = vm.list_Board_Setting[ch][1];
                        }

                        #region Calibration Logic

                        int int_V3_gap_now = vm.int_V3_scan_gap;

                        vm.ChartNowModel.preDac = await anly.Analyze_PreDAC(vm.Selected_Comport, "1");
                        string[] preDac = vm.ChartNowModel.preDac;
                        if (preDac.Contains(string.Empty)) { vm.Save_Log(new LogMember() { Status = vm.Str_Status, isShowMSG = true, Message = "Get Dac error", Channel = (ch + 1).ToString() }); }
                        else if (vm.station_type == ComViewModel.StationTypes.Hermetic_Test) { preDac[0] = "0"; preDac[1] = "0"; }

                        //Scan V3
                        for (int dac = vm.int_V3_scan_start; dac <= vm.int_V3_scan_end; dac = dac + int_V3_gap_now)
                        {
                            if (vm.isStop) break;

                            //Write Dac
                            try
                            {
                                string command = string.Format("D1 {0},{1},{2}", preDac[0], preDac[1], dac);
                                vm.port_PD.Write(command + "\r");

                                vm.list_GaugeModels[ch].GaugeD0_3 = dac.ToString();

                                vm.Str_cmd_read = string.Format("Ch{0} : {1},{2},{3}", (ch + 1).ToString(), preDac[0], preDac[1], dac);


                                await Task.Delay(vm.Int_Write_Delay);//wait for chip stable

                                if (dac.Equals(vm.int_V3_scan_start))
                                    await Task.Delay(50);  //first dac need more time for chip stable
                            }
                            catch
                            {
                                vm.Str_cmd_read = "Port Write Cmd Error : " + vm.list_Board_Setting[ch][1];
                                vm.Save_Log("K V3", "Write Dac Error", false);
                                vm.isStop = true;
                                return new List<string>();
                            }

                            //Read Power Meter
                            double power = 0;
                            await cmd.Get_Power();
                            power = vm.Double_Powers[ch];

                            DataPoint dp = new DataPoint(dac, power);

                            if (dac == vm.int_V3_scan_start)
                                vm.ChartNowModel.list_dataPoints[ch].Clear();

                            vm.ChartNowModel.list_dataPoints[ch].Add(dp);

                            #region Set Chart data points   
                            cmd.Update_Chart(dac, power, ch);
                            #endregion

                            #region Cal. Delta IL
                            cmd.Update_DeltaIL(vm.ChartNowModel.list_dataPoints[ch].Count);
                            #endregion
                        }

                        //Findout Max power
                        try
                        {
                            List<DataPoint> data = vm.ChartNowModel.list_dataPoints[ch].Where(x => x.Y == vm.ChartNowModel.list_dataPoints[ch].Max(y => y.Y)).ToList();

                            double best_dac = data[0].X;
                            double best_IL = data.First().Y;

                            if (vm.Bool_isCurfitting)
                            {
                                CurveFittingResultModel curveFittingResultModel = anly.CurFitting(vm.ChartNowModel.list_dataPoints[ch]);
                                List<DataPoint> dataPoints = curveFittingResultModel.GetDrawLinePoints();

                                best_dac = curveFittingResultModel.Best_X;
                            }

                            vm.list_GaugeModels[ch].GaugeValue = best_IL.ToString();

                            string command = string.Format("D1 {0},{1},{2}", vm.ChartNowModel.preDac[0], vm.ChartNowModel.preDac[1], best_dac);
                            vm.port_PD.Write(command + "\r");

                            await Task.Delay(vm.Int_Read_Delay);

                            #region Read Board Table

                            if (vm.station_type == ComViewModel.StationTypes.Hermetic_Test)
                            {
                                if (vm.board_read[ch].Count == 0)
                                {
                                    vm.Str_cmd_read = "UFV Board table is empty";
                                    vm.Save_Log("K V3", (ch + 1).ToString(), "UFV Board table is not exist in the path");
                                    continue;
                                }

                                List<double> list_voltage = new List<double>();
                                List<int> list_dac = new List<int>();

                                int count = 0; double final_voltage = 0;
                                foreach (string strline in vm.board_read[ch])
                                {
                                    string[] board_read = strline.Split(',');
                                    if (board_read.Length <= 1)
                                        continue;

                                    string voltage = board_read[0];
                                    int dac = int.Parse(board_read[1]);

                                    list_voltage.Add(Convert.ToDouble(voltage));
                                    list_dac.Add(dac);

                                    //if (vm.List_V3_dac[ch].Count == 0) continue;

                                    if (dac >= data[0].X && count > 0)
                                    {
                                        int delta_x = ((int)data[0].X - list_dac[count - 1]);
                                        int delta_X = (list_dac[count] - list_dac[count - 1]);
                                        double delta_Y = (list_voltage[count] - list_voltage[count - 1]);
                                        final_voltage = (Convert.ToDouble(delta_x) / Convert.ToDouble(delta_X)) * delta_Y + list_voltage[count - 1];
                                        final_voltage = Math.Round(final_voltage, 1);
                                        list_final_voltage.Add(final_voltage.ToString());

                                        vm.list_GaugeModels[ch].GaugeBearSay_3 = final_voltage.ToString();
                                        break;
                                    }

                                    count++;
                                }
                                //vm.List_V3_dac[c][maxpower_index[c]].ToString()   //電壓對應的DAC
                                //lls[ch] = new List<string>() { "", "", final_voltage.ToString() };
                                //await Task.Delay(30);
                            }

                            #endregion

                            vm.Save_cmd(new ComMember() { Command = "MAXPOWDAC", Value_1 = "0" });
                            vm.Save_cmd(new ComMember() { Command = "MAXPOWER", Value_1 = "1" });
                        }
                        catch { };
                        #endregion

                        if (vm.isStop) break;
                    }
                }

            }

            vm.Str_Status = "K V3 End";

            var elapsedMs = watch.ElapsedMilliseconds;
            vm.msgModel.msg_3 = string.Format("{0}s", (elapsedMs / 1000));

            if (vm.station_type == ComViewModel.StationTypes.Hermetic_Test)
            {
                if (vm.ChartNowModel.list_dataPoints.Where(x => x.Count > 0).ToList().Count > 0)
                {
                    await D0_show();
                    await cmd.Save_Chart();
                }
            }
            else
            {
                await D0_show();
                await cmd.Save_Chart();
            }

            return list_final_voltage;
        }

        private void Timespan_timer_Tick(object sender, EventArgs e)
        {
            vm.ChartNowModel.TimeSpan += 1;
            vm.msgModel.msg_3 = $"{Math.Round(vm.ChartNowModel.TimeSpan, 0)} s";
        }

        private async Task K_DAC()
        {
            //timespan
            vm.msgModel.msg_3 = "0 s";
            vm.ChartNowModel.TimeSpan = 0;

            System.Windows.Threading.DispatcherTimer timespan_timer = new System.Windows.Threading.DispatcherTimer();
            timespan_timer.Interval = TimeSpan.FromSeconds(1);
            timespan_timer.Tick += Timespan_timer_Tick;
            timespan_timer.Start();

            if (vm.sb_bear_shake != null)
                vm.sb_bear_shake.Begin();

            vm.Str_Status = "Calibration DAC";
            if (combox_product.SelectedItem.ToString().Contains("UFA"))
            {
                await Reset_TF();
                await K_V3();

                timespan_timer.Stop();
                vm.msgModel.msg_3 = "0 s";
                vm.ChartNowModel.TimeSpan = 0;
                timespan_timer.Start();
            }

            if (vm.isStop)
            {
                timespan_timer.Stop();
                return;
            }

            if (vm.selected_K_WL_Type.Equals("ALL Range"))
                await K_TF_Step();  //Step Scan  
            else if (vm.selected_K_WL_Type.Equals("Human Like"))
                await K_TF_Iteration();  //Step Scan  

            if (vm.sb_bear_shake != null && vm.sb_bear_reset != null)
            {
                if (vm.station_type != ComViewModel.StationTypes.TF2)
                {
                    vm.sb_bear_shake.Pause();
                    vm.sb_bear_reset.Begin();
                }
            }

            timespan_timer.Stop();
        }

        private async Task K_TF_Step()
        {
            #region Initial Setting      
            cmd.Clean_Chart();
            List<string> list_final_voltage = new List<string>();
            vm.Chart_title = "Power X DAC";
            vm.Chart_x_title = "DAC"; //Rename Chart x axis title
            vm.Chart_y_title = vm.dB_or_dBm ? "IL (dB)" : $"IL (dBm)";

            vm.Update_ALL_PlotView_Title(vm.Chart_title, vm.Chart_x_title, vm.Chart_y_title);

            vm.Str_Status = "Calibration TF";
            vm.Double_Powers = new List<double>();
            vm.Double_Powers.AddRange(Enumerable.Repeat(0.0, vm.ch_count));

            var watch = Stopwatch.StartNew();

            anly.JudgeAllBoolGauge();

            for (int i = 0; i < vm.Plot_Series.Count; i++)
            {
                vm.Plot_Series[i].Points.Clear();
            }

            for (int i = 0; i < vm.ChartNowModel.list_dataPoints.Count; i++)
            {
                vm.ChartNowModel.list_dataPoints[i] = new List<DataPoint>();
            }
            #endregion

            #region Scan Range Calculation
            List<int> list_scanDac = new List<int>();

            if (vm.int_rough_scan_start < vm.int_rough_scan_stop)
            {
                for (int dac = vm.int_rough_scan_start; dac < vm.int_rough_scan_stop; dac = dac + vm.int_rough_scan_gap)
                {
                    list_scanDac.Add(dac);
                }
            }
            else
            {
                for (int dac = vm.int_rough_scan_start; dac < vm.int_rough_scan_stop; dac = dac - vm.int_rough_scan_gap)
                {
                    list_scanDac.Add(dac);
                }
            }
            #endregion

            //PD mode, Scan all at the same time
            if (!vm.PD_or_PM && vm.BoolAllGauge)
            {
                if (!vm.port_PD.IsOpen)
                    await vm.Port_ReOpen(vm.Selected_Comport);

                foreach (int dac in list_scanDac)
                {
                    if (vm.isStop) break;

                    //Write Dac
                    try
                    {
                        string command = $"D0 {dac},{dac},{dac},{dac},{dac},{dac},{dac},{dac}\r";
                        vm.port_PD.Write(command);

                        await Task.Delay(vm.Int_Write_Delay);//wait for chip stable

                        if (dac.Equals(vm.int_rough_scan_start))
                            await Task.Delay(120);  //first dac need more time for chip stable

                        vm.Str_cmd_read = $"Dac : {dac}";
                        for (int ch = 0; ch < vm.ch_count; ch++)
                        {
                            if (dac >= 0)
                            {
                                vm.list_GaugeModels[ch].GaugeD0_1 = dac.ToString();
                                vm.list_GaugeModels[ch].GaugeD0_2 = "0";
                            }
                            else
                            {
                                vm.list_GaugeModels[ch].GaugeD0_1 = "0";
                                vm.list_GaugeModels[ch].GaugeD0_2 = Math.Abs(dac).ToString();
                            }
                        }
                    }
                    catch
                    {
                        vm.Save_Log("K TF", "Write Dac Error", true);
                        vm.isStop = true;
                    }

                    //Get Power
                    double power = 0;
                    try
                    {
                        await cmd.Get_Power();

                        for (int ch = 0; ch < vm.ch_count; ch++)
                        {
                            power = vm.Double_Powers[ch];
                            vm.list_GaugeModels[ch].GaugeValue = Math.Round(power, 4).ToString();

                            DataPoint dp = new DataPoint(dac, power);
                            vm.ChartNowModel.list_dataPoints[ch].Add(dp);

                            cmd.Update_Chart_Data(dac, power, ch);

                            #region Cal. Delta IL    
                            cmd.Update_DeltaIL(vm.ChartNowModel.list_dataPoints[ch].Count);
                            #endregion
                        }

                        vm.Update_ALL_PlotView();
                    }
                    catch
                    {
                        vm.Str_cmd_read = "Read Power Meter Step Error.";
                    }
                }

                //Findout Max power
                try
                {
                    string CMD_Dac_all = string.Empty;

                    for (int i = 0; i < vm.ch_count; i++)
                    {
                        if (vm.isStop) break;

                        DataPoint data = vm.ChartNowModel.list_dataPoints[i].Where(x => x.Y == vm.ChartNowModel.list_dataPoints[i].Max(y => y.Y)).ToList().FirstOrDefault();

                        if (i != vm.ch_count - 1)
                            CMD_Dac_all += $"{data.X},";
                        else
                            CMD_Dac_all += $"{data.X}";

                        if (data.X >= 0)
                            vm.list_GaugeModels[i].GaugeD0_1 = data.X.ToString();
                        else vm.list_GaugeModels[i].GaugeD0_2 = Math.Abs(data.X).ToString();

                        vm.list_GaugeModels[i].GaugeValue = data.Y.ToString();
                    }

                    vm.port_PD.Write($"D0 {CMD_Dac_all}\r");
                    await Task.Delay(vm.Int_Write_Delay);
                }
                catch { };
            }

            //PD selected mode
            else if (!vm.PD_or_PM && !vm.BoolAllGauge)
            {
                if (!vm.port_PD.IsOpen)
                    await vm.Port_ReOpen(vm.Selected_Comport);
                //PD mode
                vm.List_PDvalue_byV12 = new List<List<float>>(new List<float>[8]);
                vm.Chart_title = "Power X DAC";
                vm.Chart_x_title = "DAC"; //Rename Chart x axis title
                vm.Str_Status = "Calibration TF";


                await vm.Port_ReOpen(vm.Selected_Comport);
                vm.Double_Powers = new List<double>();

                foreach (int dac in list_scanDac)
                {
                    if (vm.isStop) break;

                    vm.Str_cmd_read = dac.ToString();

                    //Write Dac
                    try
                    {
                        for (int ch = 0; ch < vm.ch_count; ch++)
                        {
                            if (vm.list_GaugeModels[ch].boolGauge)
                            {
                                vm.port_PD.Write(string.Format("D{0} {1}\r", (ch + 1), dac));

                                if (dac >= 0)
                                {
                                    vm.list_GaugeModels[ch].GaugeD0_1 = dac.ToString();
                                    vm.list_GaugeModels[ch].GaugeD0_2 = "0";
                                }
                                else
                                {
                                    vm.list_GaugeModels[ch].GaugeD0_1 = "0";
                                    vm.list_GaugeModels[ch].GaugeD0_2 = Math.Abs(dac).ToString();
                                }

                                await Task.Delay(vm.Int_Write_Delay);//wait for chip stable

                                if (dac.Equals(vm.int_rough_scan_start))
                                    await Task.Delay(50);  //first dac need more time for chip stable           
                            }
                        }
                    }
                    catch
                    {
                        vm.Save_Log("K TF", "Write Dac Error", true);
                        vm.isStop = true;
                    }

                    //Get Power
                    double power = 0;
                    try
                    {
                        await cmd.Get_Power();
                        //await anly.Cmd_Write_RecieveData("P0?", false);

                        for (int ch = 0; ch < vm.ch_count; ch++)
                        {
                            if (vm.list_GaugeModels[ch].boolGauge)
                            {
                                power = vm.Double_Powers[ch];
                                vm.list_GaugeModels[ch].GaugeValue = Math.Round(power, 4).ToString();

                                DataPoint dp = new DataPoint(dac, power);
                                vm.ChartNowModel.list_dataPoints[ch].Add(dp);

                                #region Set Chart data points
                                cmd.Update_Chart(dac, power, ch);
                                #endregion

                                #region Cal. Delta IL    
                                cmd.Update_DeltaIL(vm.ChartNowModel.list_dataPoints[ch].Count);
                                #endregion
                            }
                        }
                    }
                    catch
                    {
                        vm.Str_cmd_read = "Read Power Meter Step Error.";
                    }
                }

                //Findout Max power
                try
                {
                    for (int i = 0; i < vm.ch_count; i++)
                    {
                        if (vm.list_GaugeModels[i].boolGauge)
                        {
                            List<DataPoint> data = vm.ChartNowModel.list_dataPoints[i].Where(x => x.Y == vm.ChartNowModel.list_dataPoints[i].Max(y => y.Y)).ToList();
                            string command = string.Format("D{0} {1}", i + 1, data.First().X);
                            vm.port_PD.Write(command + "\r");
                            await Task.Delay(vm.Int_Read_Delay);
                            if (data.First().X >= 0)
                                vm.list_GaugeModels[i].GaugeD0_1 = data.First().X.ToString();
                            else vm.list_GaugeModels[i].GaugeD0_2 = Math.Abs(data.First().X).ToString();

                            vm.list_GaugeModels[i].GaugeValue = data.First().Y.ToString();
                        }
                    }
                }
                catch { };
            }

            //PM mode
            else
            {
                for (int ch = 0; ch < vm.ch_count; ch++)
                {
                    if (vm.isStop) break;

                    //PM mode
                    vm.List_PMvalue_byV12 = new List<float>();
                    vm.Chart_title = "Power X DAC";
                    vm.Chart_x_title = "DAC"; //Rename Chart x axis title
                    vm.Str_Status = "Calibration TF";

                    await D0_show();

                    if (vm.int_rough_scan_start > vm.int_rough_scan_stop)
                        vm.int_rough_scan_gap = vm.int_rough_scan_gap * -1;

                    for (int dac = vm.int_rough_scan_start; dac <= vm.int_rough_scan_stop; dac = dac + vm.int_rough_scan_gap)
                    {
                        if (vm.isStop) break;

                        //Write Dac
                        if (vm.Control_board_type == 0)  //control board : UFV
                        {
                            string dacV3 = vm.list_GaugeModels[ch].GaugeD0_3;

                            if (dac >= 0)
                            {
                                vm.list_GaugeModels[ch].GaugeD0_1 = Math.Abs(dac).ToString();
                                vm.list_GaugeModels[ch].GaugeD0_2 = "0";
                            }
                            else
                            {
                                vm.list_GaugeModels[ch].GaugeD0_1 = "0";
                                vm.list_GaugeModels[ch].GaugeD0_2 = (Math.Abs(dac)).ToString();
                            }

                            vm.Str_Command = string.Format("D1 {0},{1},{2}", vm.list_GaugeModels[ch].GaugeD0_1, vm.list_GaugeModels[ch].GaugeD0_2, dacV3);
                        }
                        else     //control board : V
                        {
                            if (dac >= 0)
                            {
                                vm.list_GaugeModels[ch].GaugeD0_1 = Math.Abs(dac).ToString();
                                vm.list_GaugeModels[ch].GaugeD0_2 = "0";
                            }
                            else
                            {
                                vm.list_GaugeModels[ch].GaugeD0_1 = "0";
                                vm.list_GaugeModels[ch].GaugeD0_2 = (Math.Abs(dac)).ToString();
                            }
                            vm.Str_Command = string.Format("D1 {0},{1}", vm.list_GaugeModels[ch].GaugeD0_1, vm.list_GaugeModels[ch].GaugeD0_2);
                        }

                        vm.port_PD.Write(vm.Str_Command + "\r");

                        if (dac == vm.int_rough_scan_start)
                            await Task.Delay(vm.Int_Write_Delay + 50);
                        else
                            await Task.Delay(vm.Int_Write_Delay);

                        //Get Power
                        double power = Math.Round(vm.pm.ReadPower(), 4);
                        vm.list_GaugeModels[ch].GaugeValue = power.ToString();

                        //vm.kModel.dataPoints.Add(new DataPoint(dac, power));
                        vm.ChartNowModel.list_dataPoints[ch].Add(new DataPoint(dac, power));

                        await Task.Delay(vm.Int_Read_Delay / 2);
                        vm.Str_cmd_read = (dac).ToString();

                        #region Update Chart
                        cmd.Update_Chart(dac, power, 0);
                        #endregion

                        #region Cal. Delta IL    
                        cmd.Update_DeltaIL(vm.ChartNowModel.list_dataPoints[0].Count);
                        #endregion
                    }

                    try
                    {
                        //Set voltage at the best power
                        List<DataPoint> dataPoints = vm.ChartNowModel.list_dataPoints[ch].Where(x => x.Y == vm.ChartNowModel.list_dataPoints[ch].Max(y => y.Y)).ToList();

                        if (dataPoints.Count > 0)
                        {
                            int final_Dac = (int)dataPoints[0].X;
                            if (final_Dac >= 0)
                            {
                                vm.list_GaugeModels[ch].GaugeD0_1 = Math.Abs(final_Dac).ToString();
                                vm.list_GaugeModels[ch].GaugeD0_2 = "0";
                            }
                            else
                            {
                                vm.list_GaugeModels[ch].GaugeD0_1 = "0";
                                vm.list_GaugeModels[ch].GaugeD0_2 = (Math.Abs(final_Dac)).ToString();
                            }

                        }

                        if (vm.Control_board_type == 0)  //control board : UFV
                        {
                            vm.Str_Command = string.Format("D1 {0},{1},{2}", vm.list_GaugeModels[ch].GaugeD0_1, vm.list_GaugeModels[ch].GaugeD0_2, vm.list_GaugeModels[ch].GaugeD0_3);
                        }
                        else     //control board : V
                        {
                            vm.Str_Command = string.Format("D1 {0},{1}", vm.list_GaugeModels[ch].GaugeD0_1, vm.list_GaugeModels[ch].GaugeD0_2);
                        }


                        vm.port_PD.Write(vm.Str_Command + "\r");

                        await Task.Delay(vm.Int_Read_Delay);

                        await D0_show();

                    }
                    catch { }
                }
            }

            await D0_show();
            await cmd.Save_Chart();
            vm.Str_Status = "K TF Stop";

            var elapsedMs = watch.ElapsedMilliseconds;
            vm.msgModel.msg_3 = string.Format("{0} s", (elapsedMs / 1000));
            vm.ChartNowModel.TimeSpan = (elapsedMs / 1000);
        }

        private async Task K_TF_Iteration()
        {
            #region Initial Setting      
            cmd.Clean_Chart();
            List<string> list_final_voltage = new List<string>();
            vm.Chart_title = "Power X DAC";
            vm.Chart_x_title = "DAC"; //Rename Chart x axis title
            vm.Chart_y_title = vm.dB_or_dBm ? "IL (dB)" : $"IL (dBm)";

            vm.Update_ALL_PlotView_Title(vm.Chart_title, vm.Chart_x_title, vm.Chart_y_title);

            vm.Str_Status = "Calibration TF";
            vm.Double_Powers = new List<double>();
            vm.Double_Powers.AddRange(Enumerable.Repeat(0.0, vm.ch_count));
            var watch = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < vm.ch_count; i++)
            {
                vm.ChartNowModel.list_dataPoints[i].Clear();
                vm.Plot_Series[i].Points.Clear();
            }

            for (int i = 0; i < vm.ChartNowModel.list_dataPoints.Count; i++)
            {
                vm.ChartNowModel.list_dataPoints[i] = new List<DataPoint>();
            }

            anly.JudgeAllBoolGauge();
            #endregion

            #region Scan Range Calculation
            List<int> list_scanDac = new List<int>();

            if (vm.int_rough_scan_start < vm.int_rough_scan_stop)
            {
                for (int dac = vm.int_rough_scan_start; dac < vm.int_rough_scan_stop; dac = dac + vm.int_rough_scan_gap)
                {
                    list_scanDac.Add(dac);
                }
            }
            else
            {
                for (int dac = vm.int_rough_scan_start; dac < vm.int_rough_scan_stop; dac = dac - vm.int_rough_scan_gap)
                {
                    list_scanDac.Add(dac);
                }
            }
            #endregion

            //PD mode, Scan all at the same time
            if (!vm.PD_or_PM && vm.BoolAllGauge)
            {
                if (!vm.port_PD.IsOpen)
                    await vm.Port_ReOpen(vm.Selected_Comport);

                foreach (int dac in list_scanDac)
                {
                    if (vm.isStop) break;

                    //Write Dac
                    try
                    {
                        string command = $"D0 {dac},{dac},{dac},{dac},{dac},{dac},{dac},{dac}\r";
                        vm.port_PD.Write(command);

                        await Task.Delay(vm.Int_Write_Delay);//wait for chip stable

                        if (dac.Equals(vm.int_rough_scan_start))
                            await Task.Delay(120);  //first dac need more time for chip stable

                        vm.Str_cmd_read = $"Dac : {dac}";
                        for (int ch = 0; ch < vm.ch_count; ch++)
                        {
                            if (dac >= 0)
                            {
                                vm.list_GaugeModels[ch].GaugeD0_1 = dac.ToString();
                                vm.list_GaugeModels[ch].GaugeD0_2 = "0";
                            }
                            else
                            {
                                vm.list_GaugeModels[ch].GaugeD0_1 = "0";
                                vm.list_GaugeModels[ch].GaugeD0_2 = Math.Abs(dac).ToString();
                            }
                        }
                    }
                    catch
                    {
                        vm.Save_Log("K TF", "Write Dac Error", true);
                        vm.isStop = true;
                    }

                    //Get Power
                    double power = 0;
                    try
                    {
                        await cmd.Get_Power();

                        for (int ch = 0; ch < vm.ch_count; ch++)
                        {
                            power = vm.Double_Powers[ch];
                            vm.list_GaugeModels[ch].GaugeValue = Math.Round(power, 4).ToString();

                            DataPoint dp = new DataPoint(dac, power);
                            vm.ChartNowModel.list_dataPoints[ch].Add(dp);

                            cmd.Update_Chart_Data(dac, power, ch);

                            #region Cal. Delta IL    
                            cmd.Update_DeltaIL(vm.ChartNowModel.list_dataPoints[ch].Count);
                            #endregion
                        }

                        vm.Update_ALL_PlotView();
                    }
                    catch
                    {
                        vm.Str_cmd_read = "Read Power Meter Step Error.";
                    }
                }

                //Findout Max power

                try
                {
                    string CMD_Dac_all = string.Empty;

                    for (int i = 0; i < vm.ch_count; i++)
                    {
                        if (vm.isStop) break;

                        DataPoint data = vm.ChartNowModel.list_dataPoints[i].Where(x => x.Y == vm.ChartNowModel.list_dataPoints[i].Max(y => y.Y)).ToList().FirstOrDefault();

                        if (i != vm.ch_count - 1)
                            CMD_Dac_all += $"{data.X},";
                        else
                            CMD_Dac_all += $"{data.X}";

                        if (data.X >= 0)
                            vm.list_GaugeModels[i].GaugeD0_1 = data.X.ToString();
                        else vm.list_GaugeModels[i].GaugeD0_2 = Math.Abs(data.X).ToString();

                        vm.list_GaugeModels[i].GaugeValue = data.Y.ToString();
                    }

                    vm.port_PD.Write($"D0 {CMD_Dac_all}\r");
                    await Task.Delay(vm.Int_Write_Delay);
                }
                catch { };
            }

            //PD selected mode
            else if (!vm.PD_or_PM && !vm.BoolAllGauge)
            {
                if (!vm.port_PD.IsOpen)
                    await vm.Port_ReOpen(vm.Selected_Comport);
                //PD mode
                vm.List_PDvalue_byV12 = new List<List<float>>(new List<float>[8]);
                vm.Chart_title = "Power X DAC";
                vm.Chart_x_title = "DAC"; //Rename Chart x axis title

                await vm.Port_ReOpen(vm.Selected_Comport);
                //vm.Double_Powers = new List<double>();

                foreach (int dac in list_scanDac)
                {
                    if (vm.isStop) break;

                    vm.Str_cmd_read = dac.ToString();

                    bool selectedGaugeLessThanOne = true;
                    foreach (GaugeModel gm in vm.list_GaugeModels)
                    {
                        if (gm.boolGauge) selectedGaugeLessThanOne = false;
                    }
                    if (selectedGaugeLessThanOne) break;

                    //Write Dac
                    try
                    {
                        for (int ch = 0; ch < vm.ch_count; ch++)
                        {
                            if (vm.list_GaugeModels[ch].boolGauge)
                            {
                                vm.port_PD.Write(string.Format("D{0} {1}\r", (ch + 1), dac));

                                if (dac >= 0)
                                {
                                    vm.list_GaugeModels[ch].GaugeD0_1 = dac.ToString();
                                    vm.list_GaugeModels[ch].GaugeD0_2 = "0";
                                }
                                else
                                {
                                    vm.list_GaugeModels[ch].GaugeD0_1 = "0";
                                    vm.list_GaugeModels[ch].GaugeD0_2 = Math.Abs(dac).ToString();
                                }

                                if (dac.Equals(vm.int_rough_scan_start))
                                    await Task.Delay(120);  //first dac need more time for chip stable         
                                else
                                    await Task.Delay(vm.Int_Write_Delay);//wait for chip stable
                            }
                        }
                    }
                    catch
                    {
                        vm.Save_Log("K TF", "Write Dac Error", true);
                        vm.isStop = true;
                    }

                    //Get Power
                    double power = 0;
                    try
                    {
                        //await anly.Cmd_Write_RecieveData("P0?", false);
                        await cmd.Get_Power();

                        for (int ch = 0; ch < vm.ch_count; ch++)
                        {
                            if (vm.list_GaugeModels[ch].boolGauge)
                            {
                                power = vm.Double_Powers[ch];
                                DataPoint Last_Point = new DataPoint();
                                if (vm.ChartNowModel.list_dataPoints[ch].Count > 0)
                                {
                                    Last_Point = vm.ChartNowModel.list_dataPoints[ch].Last();
                                }
                                DataPoint dp = new DataPoint(dac, power);
                                vm.ChartNowModel.list_dataPoints[ch].Add(dp);

                                #region Udpate Chart
                                cmd.Update_Chart(dac, power, ch);
                                #endregion

                                #region Cal. Delta IL    
                                cmd.Update_DeltaIL(vm.ChartNowModel.list_dataPoints[ch].Count);
                                #endregion

                                #region Threshold Judgement
                                if (Last_Point.X != 0 && Last_Point.Y != 0)
                                {
                                    if (power < Last_Point.Y && Last_Point.Y > -35)
                                    {
                                        vm.port_PD.Write(string.Format("D{0} {1}\r", (ch + 1), Last_Point.X));

                                        if (Last_Point.X >= 0)
                                        {
                                            vm.list_GaugeModels[ch].GaugeD0_1 = Last_Point.X.ToString();
                                            vm.list_GaugeModels[ch].GaugeD0_2 = "0";
                                        }
                                        else
                                        {
                                            vm.list_GaugeModels[ch].GaugeD0_1 = "0";
                                            vm.list_GaugeModels[ch].GaugeD0_2 = Math.Abs(Last_Point.X).ToString();
                                        }

                                        vm.list_GaugeModels[ch].GaugeValue = Last_Point.Y.ToString();

                                        vm.list_GaugeModels[ch].boolGauge = false;
                                    }
                                }
                                #endregion
                            }
                        }
                    }
                    catch
                    {
                        vm.Str_cmd_read = "Get Power Error.";
                    }
                }

                //Findout Max power
                try
                {
                    for (int i = 0; i < vm.ch_count; i++)
                    {
                        if (vm.list_GaugeModels[i].boolGauge)
                        {
                            List<DataPoint> data = vm.ChartNowModel.list_dataPoints[i].Where(x => x.Y == vm.ChartNowModel.list_dataPoints[i].Max(y => y.Y)).ToList();
                            string command = string.Format("D{0} {1}", i + 1, data.First().X);
                            vm.port_PD.Write(command + "\r");
                            await Task.Delay(vm.Int_Read_Delay);
                            if (data.First().X >= 0)
                                vm.list_GaugeModels[i].GaugeD0_1 = data.First().X.ToString();
                            else vm.list_GaugeModels[i].GaugeD0_2 = Math.Abs(data.First().X).ToString();

                            vm.list_GaugeModels[i].GaugeValue = data.First().Y.ToString();
                        }
                    }
                }
                catch { };
            }

            //PM mode
            else
            {
                for (int ch = 0; ch < vm.ch_count; ch++)
                {
                    if (vm.isStop) break;

                    //PM mode
                    vm.List_PMvalue_byV12 = new List<float>();
                    vm.Chart_title = "Power X DAC";
                    vm.Chart_x_title = "DAC"; //Rename Chart x axis title
                    vm.Str_Status = "Calibration TF";

                    await D0_show();

                    if (vm.int_rough_scan_start > vm.int_rough_scan_stop)
                        vm.int_rough_scan_gap = vm.int_rough_scan_gap * -1;

                    foreach (int dac in list_scanDac)
                    {
                        if (vm.isStop) break;

                        //Write Dac
                        if (vm.Control_board_type == 0)  //control board : UFV
                        {
                            string dacV3 = vm.list_GaugeModels[ch].GaugeD0_3;

                            if (dac >= 0)
                            {
                                vm.list_GaugeModels[ch].GaugeD0_1 = Math.Abs(dac).ToString();
                                vm.list_GaugeModels[ch].GaugeD0_2 = "0";
                            }
                            else
                            {
                                vm.list_GaugeModels[ch].GaugeD0_1 = "0";
                                vm.list_GaugeModels[ch].GaugeD0_2 = (Math.Abs(dac)).ToString();
                            }

                            vm.Str_Command = string.Format("D1 {0},{1},{2}", vm.list_GaugeModels[ch].GaugeD0_1, vm.list_GaugeModels[ch].GaugeD0_2, dacV3);
                        }
                        else     //control board : V
                        {
                            if (dac >= 0)
                            {
                                vm.list_GaugeModels[ch].GaugeD0_1 = Math.Abs(dac).ToString();
                                vm.list_GaugeModels[ch].GaugeD0_2 = "0";
                            }
                            else
                            {
                                vm.list_GaugeModels[ch].GaugeD0_1 = "0";
                                vm.list_GaugeModels[ch].GaugeD0_2 = (Math.Abs(dac)).ToString();
                            }
                            vm.Str_Command = string.Format("D1 {0},{1}", vm.list_GaugeModels[ch].GaugeD0_1, vm.list_GaugeModels[ch].GaugeD0_2);
                        }

                        vm.port_PD.Write(vm.Str_Command + "\r");

                        await Task.Delay(vm.Int_Write_Delay);//wait for chip stable

                        if (dac.Equals(vm.int_rough_scan_start))
                            await Task.Delay(50);  //first dac need more time for chip stable

                        //Get Power
                        await cmd.Get_Power();
                        double power = vm.Double_Powers[ch];

                        DataPoint Last_Point = new DataPoint();
                        if (vm.ChartNowModel.list_dataPoints[ch].Count > 0)
                        {
                            Last_Point = vm.ChartNowModel.list_dataPoints[ch].Last();
                        }

                        vm.ChartNowModel.list_dataPoints[ch].Add(new DataPoint(dac, power));

                        await Task.Delay(vm.Int_Read_Delay / 2);
                        vm.Str_cmd_read = (dac).ToString();

                        #region Set Chart data points   
                        cmd.Update_Chart(dac, power, 0);
                        #endregion

                        #region Cal. Delta IL    
                        cmd.Update_DeltaIL(vm.ChartNowModel.list_dataPoints[ch].Count);
                        #endregion

                        #region Threshold Judgement
                        if (Last_Point.X != 0 && Last_Point.Y != 0)
                        {
                            if (power < Last_Point.Y && Last_Point.Y > -35)
                            {
                                vm.port_PD.Write(string.Format("D{0} {1}\r", (ch + 1), Last_Point.X));

                                if (Last_Point.X >= 0)
                                {
                                    vm.list_GaugeModels[ch].GaugeD0_1 = Last_Point.X.ToString();
                                    vm.list_GaugeModels[ch].GaugeD0_2 = "0";
                                }
                                else
                                {
                                    vm.list_GaugeModels[ch].GaugeD0_1 = "0";
                                    vm.list_GaugeModels[ch].GaugeD0_2 = Math.Abs(Last_Point.X).ToString();
                                }

                                vm.list_GaugeModels[ch].GaugeValue = Last_Point.Y.ToString();

                                vm.list_GaugeModels[ch].boolGauge = false;
                            }
                        }
                        #endregion
                    }

                    //Find out max point
                    try
                    {
                        //Set voltage at the best power
                        List<DataPoint> dataPoints = vm.ChartNowModel.list_dataPoints[ch].Where(x => x.Y == vm.ChartNowModel.list_dataPoints[ch].Max(y => y.Y)).ToList();

                        if (dataPoints.Count > 0)
                        {
                            int final_Dac = (int)dataPoints[0].X;
                            if (final_Dac >= 0)
                            {
                                vm.list_GaugeModels[ch].GaugeD0_1 = Math.Abs(final_Dac).ToString();
                                vm.list_GaugeModels[ch].GaugeD0_2 = "0";
                            }
                            else
                            {
                                vm.list_GaugeModels[ch].GaugeD0_1 = "0";
                                vm.list_GaugeModels[ch].GaugeD0_2 = (Math.Abs(final_Dac)).ToString();
                            }

                        }

                        if (vm.Control_board_type == 0)  //control board : UFV
                        {
                            vm.Str_Command = string.Format("D1 {0},{1},{2}", vm.list_GaugeModels[ch].GaugeD0_1, vm.list_GaugeModels[ch].GaugeD0_2, vm.list_GaugeModels[ch].GaugeD0_3);
                        }
                        else     //control board : V
                        {
                            vm.Str_Command = string.Format("D1 {0},{1}", vm.list_GaugeModels[ch].GaugeD0_1, vm.list_GaugeModels[ch].GaugeD0_2);
                        }


                        vm.port_PD.Write(vm.Str_Command + "\r");

                        await Task.Delay(vm.Int_Read_Delay);

                        await D0_show();

                    }
                    catch { }
                }
            }

            var elapsedMs = watch.ElapsedMilliseconds;
            vm.msgModel.msg_3 = string.Format("{0} s", (elapsedMs / 1000));

            await D0_show();
            await cmd.Save_Chart();
            vm.Str_Status = "K TF Stop";
        }

        public async Task<string> WriteCmd_GetMessage(string cmd)
        {
            string msg = "";
            ; try
            {
                await vm.Port_ReOpen(vm.Selected_Comport);

                if (vm.port_PD.IsOpen)
                {
                    vm.port_PD.Write(cmd + "\r");

                    await Task.Delay(vm.Int_Read_Delay);

                    int size = vm.port_PD.BytesToRead;
                    byte[] dataBuffer = new byte[size];
                    int length = vm.port_PD.Read(dataBuffer, 0, size);

                    //Read Feedback message
                    msg = anly.GetMessage(dataBuffer);

                    vm.port_PD.Close();
                }
            }
            catch { return ""; }

            return msg;
        }

        async Task _cmd_write_recieveData_ForD0(string cmd)  //for D0?
        {
            try
            {
                if (vm.station_type == ComViewModel.StationTypes.Chamber_S_16ch)
                {
                    if (vm.port_PD.IsOpen)
                    {
                        vm.port_PD.Write(cmd + "\r");
                    }

                    if (vm.port_PD_B.IsOpen)
                    {
                        vm.port_PD_B.Write(cmd + "\r");
                    }

                    await Task.Delay(vm.Int_Read_Delay);

                    //int sizeA = 0, sizeB = 0;
                    //byte[] dataBufferA = new byte[sizeA], dataBufferB = new byte[sizeB];

                    if (vm.port_PD.IsOpen)
                    {
                        int sizeA = vm.port_PD.BytesToRead;
                        byte[] dataBufferA = new byte[sizeA];

                        //A 資料分析並顯示於狀態列
                        if (sizeA > 0)
                        {
                            int length = vm.port_PD.Read(dataBufferA, 0, sizeA);

                            anly.Read_analysis(cmd, dataBufferA);

                            #region Analyze Dx? and show data
                            if (cmd.ToCharArray(0, 1)[0] == 'D' && cmd.ToCharArray(2, 1)[0] == '?') //D1?, D2?...
                            {
                                int ch = int.Parse(cmd.ToCharArray(0, 3)[1].ToString());

                                string[] words = vm.Str_cmd_read.Split(',');  //V1,V2,V3 

                                if (words.Length == 2)
                                {
                                    vm.list_GaugeModels[ch - 1].GaugeD0_1 = words[0];
                                    vm.list_GaugeModels[ch - 1].GaugeD0_2 = words[1];
                                }
                                if (words.Length == 3)
                                {
                                    vm.list_GaugeModels[ch - 1].GaugeD0_1 = words[0];
                                    vm.list_GaugeModels[ch - 1].GaugeD0_2 = words[1];
                                    vm.list_GaugeModels[ch - 1].GaugeD0_3 = words[2];
                                }
                            }
                            #endregion
                        }
                    }

                    if (vm.port_PD_B.IsOpen)
                    {
                        int sizeB = vm.port_PD_B.BytesToRead;
                        byte[] dataBufferB = new byte[sizeB];

                        //B 資料分析並顯示於狀態列
                        if (sizeB > 0)
                        {
                            int length = vm.port_PD_B.Read(dataBufferB, 0, sizeB);

                            anly.Read_analysis(cmd, dataBufferB);

                            #region Analyze Dx? and show data
                            if (cmd.ToCharArray(0, 1)[0] == 'D' && cmd.ToCharArray(2, 1)[0] == '?') //D1?, D2?...
                            {
                                int ch = int.Parse(cmd.ToCharArray(0, 3)[1].ToString());

                                string[] words = vm.Str_cmd_read.Split(',');  //V1,V2,V3 

                                if (words.Length == 2)
                                {
                                    vm.list_GaugeModels[ch + 8 - 1].GaugeD0_1 = words[0];
                                    vm.list_GaugeModels[ch + 8 - 1].GaugeD0_2 = words[1];
                                }
                                if (words.Length == 3)
                                {
                                    vm.list_GaugeModels[ch + 8 - 1].GaugeD0_1 = words[0];
                                    vm.list_GaugeModels[ch + 8 - 1].GaugeD0_2 = words[1];
                                    vm.list_GaugeModels[ch + 8 - 1].GaugeD0_3 = words[2];
                                }
                            }
                            #endregion
                        }
                    }





                }
                else
                {
                    if (vm.port_PD.IsOpen)
                    {
                        vm.port_PD.Write(cmd + "\r");

                        await Task.Delay(vm.Int_Read_Delay);

                        int size = vm.port_PD.BytesToRead;
                        byte[] dataBuffer = new byte[size];
                        if (size > 0)
                        {
                            dataBuffer = new byte[size];
                            int length = vm.port_PD.Read(dataBuffer, 0, size);
                        }

                        //資料分析並顯示於狀態列
                        anly.Read_analysis(cmd, dataBuffer);

                        #region Analyze Dx? and show data
                        if (cmd.ToCharArray(0, 1)[0] == 'D' && cmd.ToCharArray(2, 1)[0] == '?') //D1?, D2?...
                        {
                            int ch = int.Parse(cmd.ToCharArray(0, 3)[1].ToString());

                            string[] words = vm.Str_cmd_read.Split(',');  //V1,V2,V3 

                            if (words.Length == 2)
                            {
                                vm.list_GaugeModels[ch - 1].GaugeD0_1 = words[0];
                                vm.list_GaugeModels[ch - 1].GaugeD0_2 = words[1];
                            }
                            if (words.Length == 3)
                            {
                                vm.list_GaugeModels[ch - 1].GaugeD0_1 = words[0];
                                vm.list_GaugeModels[ch - 1].GaugeD0_2 = words[1];
                                vm.list_GaugeModels[ch - 1].GaugeD0_3 = words[2];
                            }
                        }
                        #endregion
                    }
                }

            }
            catch { }
        }

        private void Cmd_RecieveData(string cmd, bool _is_port_close_after_CmdWrite)
        {
            try
            {
                int size = vm.port_PD.BytesToRead;
                byte[] dataBuffer = new byte[size];
                int length = vm.port_PD.Read(dataBuffer, 0, size);

                //Show read back message
                string msg = anly.Read_analysis(cmd, dataBuffer);
                if (!string.IsNullOrEmpty(msg))
                    vm.Str_cmd_read = msg;

                #region Analyze Dx? and show data
                if (cmd.First() == 'D' && cmd.Count() == 7) //D1?, D2?...
                {
                    int ch = int.Parse(cmd.ToCharArray(0, 3)[1].ToString());

                    ObservableCollection<string> list_words = new ObservableCollection<string>();  //one channel list[v1.v2.v3]
                    string[] words = msg.Split(',');  //V1,V2,V3 

                    foreach (string s in words) list_words.Add(s);  //Convert array to list
                                                                    //vm.list_D_All[ch - 1] = (list_words);  //Add one channel list to All channel list
                    vm.List_D0_value[ch - 1] = list_words;
                    //vm.List_D0_value = new ObservableCollection<ObservableCollection<string>>(vm.list_D_All); //Rise propertychanged event                           
                }
                #endregion

                if (_is_port_close_after_CmdWrite)
                {
                    vm.port_PD.DiscardInBuffer();       // RX
                    vm.port_PD.DiscardOutBuffer();      // TX
                    vm.port_PD.Close();
                }
            }
            catch { }
        }

        private async Task<bool> Cmd_RecieveData(string cmd, bool _is_port_close_after_CmdWrite, int ch)
        {
            try
            {
                if (vm.port_PD.IsOpen)
                {
                    await Task.Delay(1);

                    int size = vm.port_PD.BytesToRead;
                    byte[] dataBuffer = new byte[size];
                    int length = vm.port_PD.Read(dataBuffer, 0, size);

                    //Show read back message
                    vm = anly._PM_read_analysis(vm.Str_Command, dataBuffer, ch);

                    if (vm.Str_Command != "ID?")
                    {
                        #region Analyze Dx?
                        if (vm.Str_Command.ToCharArray(0, 1)[0] == 'D' && vm.Str_Command.ToCharArray(2, 1)[0] == '?') //D1?, D2?...
                        {
                            ObservableCollection<string> list_words = new ObservableCollection<string>();  //one channel list[v1.v2.v3]
                            string[] words = vm.Str_cmd_read.Split(',');  //V1,V2,V3 
                            foreach (string s in words) list_words.Add(s);  //Convert array to list
                            vm.list_D_All.Add(list_words);  //Add one channel list to All channel list
                            vm.List_D0_value = new ObservableCollection<ObservableCollection<string>>(vm.list_D_All); //Make propertychanged event happen                        
                        }
                        #endregion
                    }

                    if (_is_port_close_after_CmdWrite)
                    {
                        vm.port_PD.DiscardInBuffer();       // RX
                        vm.port_PD.DiscardOutBuffer();      // TX
                        vm.port_PD.Close();
                    }
                }
            }
            catch { }

            return vm.IsGoOn;
        }

        private async Task<string> Port_Write_Read(SerialPort port, string comport, int BoudRate, string cmd, bool _is_port_close_after_CmdWrite)
        {
            string msg = "";
            try
            {
                port = new SerialPort(comport, BoudRate, Parity.None, 8, StopBits.One);
                port.Open();

                if (port.IsOpen)
                {
                    port.Write(cmd + "\r");

                    await Task.Delay(100);

                    int size = port.BytesToRead;
                    byte[] dataBuffer = new byte[size];
                    int length = port.Read(dataBuffer, 0, size);

                    //Read message
                    msg = anly.GetMessage(dataBuffer);

                    if (_is_port_close_after_CmdWrite)
                    {
                        port.DiscardInBuffer();       // RX
                        port.DiscardOutBuffer();      // TX
                        port.Close();
                    }
                }
            }
            catch { }

            return msg;
        }


        private async Task<bool> Cmd_Write_RecieveData(string cmd, bool _is_port_close_after_CmdWrite, int ch)
        {
            try
            {
                if (vm.port_PD.IsOpen)
                {
                    vm.port_PD.Write(cmd + "\r");

                    await Task.Delay(vm.Int_Read_Delay);

                    int size = vm.port_PD.BytesToRead;
                    byte[] dataBuffer = new byte[size];
                    int length = vm.port_PD.Read(dataBuffer, 0, size);

                    //Show read back message
                    vm = anly._PM_read_analysis(cmd, dataBuffer, ch);

                    if (cmd != "ID?")
                    {
                        #region Analyze Dx?
                        if (cmd.ToCharArray(0, 1)[0] == 'D' && cmd.ToCharArray(2, 1)[0] == '?') //D1?, D2?...
                        {
                            ObservableCollection<string> list_words = new ObservableCollection<string>();  //one channel list[v1.v2.v3]
                            string[] words = vm.Str_cmd_read.Split(',');  //V1,V2,V3 
                            foreach (string s in words) list_words.Add(s);  //Convert array to list
                            vm.list_D_All.Add(list_words);  //Add one channel list to All channel list
                            vm.List_D0_value = new ObservableCollection<ObservableCollection<string>>(vm.list_D_All); //Make propertychanged event happen                        
                        }
                        #endregion
                    }

                    if (_is_port_close_after_CmdWrite)
                    {
                        vm.port_PD.DiscardInBuffer();       // RX
                        vm.port_PD.DiscardOutBuffer();      // TX
                        vm.port_PD.Close();
                    }
                }
            }
            catch { }

            return vm.IsGoOn;
        }

        /// <summary>
        /// Write and Get Message from a serial port
        /// </summary>
        /// <param name="sp">Serial Port</param>
        /// <param name="cmd">Command</param>
        /// <param name="_is_port_close_after_CmdWrite">Port close or not in end</param>
        /// <param name="delay">Delay time for read</param>
        /// <returns></returns>
        private async Task<string> Cmd_Write_Get(SerialPort sp, string cmd, bool _is_port_close_after_CmdWrite, int delay)
        {
            string msg = string.Empty;
            try
            {
                if (sp.IsOpen)
                {
                    sp.Write(cmd + "\r");

                    await Task.Delay(delay);

                    int size = sp.BytesToRead;
                    byte[] dataBuffer = new byte[size];
                    int length = sp.Read(dataBuffer, 0, size);

                    //Show read back message
                    msg = anly.GetMessage(dataBuffer);

                    if (_is_port_close_after_CmdWrite)
                    {
                        sp.DiscardInBuffer();       // RX
                        sp.DiscardOutBuffer();      // TX
                        sp.Close();
                    }
                }
            }
            catch { }

            return msg;
        }

        private void Cmd_Write(SerialPort sp, string cmd, bool _is_port_close_after_CmdWrite, int delay)
        {
            try
            {
                if (sp.IsOpen)
                {
                    sp.Write(cmd + "\r");
                }
            }
            catch { }
        }

        private string Cmd_Get(SerialPort sp, string cmd, bool _is_port_close_after_CmdWrite, int delay)
        {
            string msg = string.Empty;
            try
            {
                if (sp.IsOpen)
                {
                    int size = sp.BytesToRead;
                    byte[] dataBuffer = new byte[size];
                    int length = sp.Read(dataBuffer, 0, size);

                    //Show read back message
                    msg = anly.GetMessage(dataBuffer);

                    if (_is_port_close_after_CmdWrite)
                    {
                        sp.DiscardInBuffer();       // RX
                        sp.DiscardOutBuffer();      // TX
                        sp.Close();
                    }
                }
            }
            catch { }

            return msg;
        }



        ICommunication icomm;
        DiCon.UCB.Communication.RS232.RS232 rs232;
        DiCon.UCB.MTF.IMTFCommand tf;

        private async void combox_comport_DropDownOpened(object sender, EventArgs e)
        {
            vm.isStop = false;

            vm.list_combox_comports.Clear();
            myPorts = SerialPort.GetPortNames();

            await Task.Delay(10);

            for (int i = 0; i < myPorts.Length; i++)
            {
                try
                {
                    vm.list_combox_comports.Add(myPorts[i]);  //寫入所有取得的com
                }
                catch { }
            }

            #region Get port name

            List<SerialPort> list_serialPorts = new List<SerialPort>();

            if (vm.port_PD != null)
            {
                if (vm.port_PD.IsOpen)
                {
                    vm.port_PD.DiscardInBuffer();       // RX
                    vm.port_PD.DiscardOutBuffer();      // TX
                    vm.port_PD.Close();
                }
            }

            if (vm.port_PD_B != null)
            {
                if (vm.port_PD_B.IsOpen)
                {
                    vm.port_PD_B.DiscardInBuffer();       // RX
                    vm.port_PD_B.DiscardOutBuffer();      // TX
                    vm.port_PD_B.Close();
                }
            }

            //Creat serial port
            for (int i = 0; i < vm.list_combox_comports.Count; i++)
            {
                try
                {
                    SerialPort sp = new SerialPort(vm.list_combox_comports[i], vm.BoudRate, Parity.None, 8, StopBits.One);

                    await Dispatcher.BeginInvoke(new Action(() =>
                    {
                        vm.list_combox_comports[i] = $"{myPorts[i]}  ~";

                        list_serialPorts.Add(sp);
                    }));

                    string comport = vm.list_combox_comports[i];

                    sp.Open();

                    if (sp.IsOpen)
                        sp.Write("ID?\r");
                }
                catch { }
            }

            await Task.Delay(vm.Int_Read_Delay);

            for (int i = 0; i < list_serialPorts.Count; i++)
            {
                try
                {
                    int size = list_serialPorts[i].BytesToRead;
                    byte[] dataBuffer = new byte[size];
                    int length = list_serialPorts[i].Read(dataBuffer, 0, size);

                    //Show read back message
                    string msg = anly.GetMessage_Indepen(dataBuffer);

                    Console.WriteLine($"{i}: {msg}");

                    await Dispatcher.BeginInvoke(new Action(() =>
                    {
                        if (string.IsNullOrEmpty(msg))
                            msg = "";

                        myPorts[i] = $"{myPorts[i]} {msg}";
                        vm.list_combox_comports[i] = myPorts[i];  //寫入所有取得的com
                    }));

                    if (list_serialPorts[i].IsOpen)
                    {
                        list_serialPorts[i].DiscardInBuffer();       // RX
                        list_serialPorts[i].DiscardOutBuffer();      // TX
                        list_serialPorts[i].Close();
                    };
                }
                catch { }
            };

            #endregion
        }

        private async void combox_comport_DropDownClosed(object sender, EventArgs e)
        {
            vm.isStop = true;

            if (combox_comport.SelectedItem != null)
            {
                vm.port_PD = new SerialPort(vm.Selected_Comport, vm.BoudRate, Parity.None, 8, StopBits.One);
                _isAskPortStop = true;
            }

            vm.Ini_Write("Connection", "Comport", vm.Selected_Comport);  //創建ini file並寫入基本設定

            setting.GetPowerType_Setting();

            await Task.Delay(330);
            _isAskPortStop = false;
        }

        private void RBtn_Gauge_Page_Checked(object sender, RoutedEventArgs e)
        {
            switch (vm.station_type)
            {
                case ComViewModel.StationTypes.TF2:
                    if (_Page_TF2_Main == null)
                        _Page_TF2_Main = new Page_TF2_Main(vm);

                    pageTransitionControl.Visibility = Visibility.Hidden;
                    Frame_Navigation.Visibility = Visibility.Visible;
                    Frame_Navigation.Navigate(_Page_TF2_Main);

                    //一個plotModel同一時間只能供給一個plotview使用，頁面切換時需重新指派
                    vm.PlotViewModel_Chart = new PlotModel();
                    vm.PlotViewModel_Testing = new PlotModel();
                    vm.PlotViewModel_UTF600 = new PlotModel();
                    vm.PlotViewModel_BR = new PlotModel();
                    vm.PlotViewModel_TF2 = vm.PlotViewModel;

                    break;

                case ComViewModel.StationTypes.UTF600:
                    if (_Page_UTF600_Main == null)
                        _Page_UTF600_Main = new Page_UTF600_Main(vm);

                    pageTransitionControl.Visibility = Visibility.Hidden;
                    Frame_Navigation.Visibility = Visibility.Visible;
                    Frame_Navigation.Navigate(_Page_UTF600_Main);

                    //一個plotModel同一時間只能供給一個plotview使用，頁面切換時需重新指派
                    vm.PlotViewModel_Chart = new PlotModel();
                    vm.PlotViewModel_TF2 = new PlotModel();
                    vm.PlotViewModel_Testing = new PlotModel();
                    vm.PlotViewModel_BR = new PlotModel();
                    vm.PlotViewModel_UTF600 = vm.PlotViewModel;

                    break;

                case ComViewModel.StationTypes.BR:
                    if (_Page_BR == null)
                        _Page_BR = new Page_BR(vm);

                    pageTransitionControl.Visibility = Visibility.Hidden;
                    Frame_Navigation.Visibility = Visibility.Visible;
                    Frame_Navigation.Navigate(_Page_BR);

                    //一個plotModel同一時間只能供給一個plotview使用，頁面切換時需重新指派
                    vm.PlotViewModel_Chart = new PlotModel();
                    vm.PlotViewModel_TF2 = new PlotModel();
                    vm.PlotViewModel_Testing = new PlotModel();
                    vm.PlotViewModel_UTF600 = new PlotModel();
                    vm.PlotViewModel_BR = vm.PlotViewModel;

                    break;

                default:
                    if (_Page_PD_Gauges == null)
                        _Page_PD_Gauges = new Page_PD_Gauges(vm);

                    pageTransitionControl.Visibility = Visibility.Hidden;
                    Frame_Navigation.Visibility = Visibility.Visible;
                    //pageTransitionControl.ShowPage(_Page_PD_Gauges);
                    Frame_Navigation.Navigate(_Page_PD_Gauges);

                    //一個plotModel同一時間只能供給一個plotview使用，頁面切換時需重新指派
                    vm.PlotViewModel_Chart = new PlotModel();
                    vm.PlotViewModel_TF2 = new PlotModel();
                    vm.PlotViewModel_UTF600 = new PlotModel();
                    vm.PlotViewModel_BR = new PlotModel();
                    vm.PlotViewModel_Testing = vm.PlotViewModel;
                    break;
            }

            //if (vm.station_type != "TF2")
            //{
            //    if (_Page_PD_Gauges == null)
            //        _Page_PD_Gauges = new Page_PD_Gauges(vm);

            //    pageTransitionControl.Visibility = Visibility.Hidden;
            //    Frame_Navigation.Visibility = Visibility.Visible;
            //    //pageTransitionControl.ShowPage(_Page_PD_Gauges);
            //    Frame_Navigation.Navigate(_Page_PD_Gauges);
            //}
            //else if (vm.station_type == "TF2")
            //{
            //    if (_Page_TF2_Main == null)
            //        _Page_TF2_Main = new Page_TF2_Main(vm);

            //    pageTransitionControl.Visibility = Visibility.Hidden;
            //    Frame_Navigation.Visibility = Visibility.Visible;
            //    Frame_Navigation.Navigate(_Page_TF2_Main);
            //}

            GC.Collect();
        }

        private void RBtn_Chart_Page_Checked(object sender, RoutedEventArgs e)
        {
            if (_Page_Chart == null)
                _Page_Chart = new Page_Chart(vm);
            pageTransitionControl.Visibility = Visibility.Visible;
            Frame_Navigation.Visibility = Visibility.Hidden;
            pageTransitionControl.ShowPage(_Page_Chart);

            GC.Collect();
        }

        private void RBtn_DataGrid_Page_Checked(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Reset();
            sw.Start();

            if (_Page_DataGrid == null)
                _Page_DataGrid = new Page_DataGrid(vm);

            sw.Stop();
            Console.WriteLine("TimeSpan = " + sw.ElapsedMilliseconds.ToString() + " ms");

            pageTransitionControl.Visibility = Visibility.Hidden;
            Frame_Navigation.Visibility = Visibility.Visible;
            Frame_Navigation.Navigate(_Page_DataGrid);

            GC.Collect();
        }

        private void RBtn_Laser_Page_Checked(object sender, RoutedEventArgs e)
        {
            if (_Page_Laser == null)
                _Page_Laser = new Page_Laser(vm);

            pageTransitionControl.Visibility = Visibility.Visible;
            Frame_Navigation.Visibility = Visibility.Hidden;
            pageTransitionControl.ShowPage(_Page_Laser);
            //Frame_Navigation.Navigate(_Page_Laser);
        }

        private void RBtn_Log_Checked(object sender, RoutedEventArgs e)
        {
            if (_Page_Log_Command == null)
                _Page_Log_Command = new Page_Log_Command(vm);

            pageTransitionControl.Visibility = Visibility.Visible;
            Frame_Navigation.Visibility = Visibility.Hidden;
            pageTransitionControl.ShowPage(_Page_Log_Command);
        }

        private void RBtn_Setting_Checked(object sender, RoutedEventArgs e)
        {
            if (_Page_Setting == null)
                _Page_Setting = new Page_Setting(vm);

            pageTransitionControl.Visibility = Visibility.Visible;
            Frame_Navigation.Visibility = Visibility.Hidden;
            pageTransitionControl.ShowPage(_Page_Setting);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            try
            {
                vm.port_PD.Close();
            }
            catch { }
        }

        //private Device device;

        //private void SendCommand(string cmd)
        //{
        //    try
        //    {
        //        if (vm.device != null)
        //        {
        //            vm.device.Write(cmd);
        //        }
        //    }
        //    catch { }
        //}

        //private string ReadGPIB()
        //{
        //    try
        //    {
        //        if (vm.device != null)
        //        {
        //            return vm.device.ReadString();
        //        }
        //        return "-100";
        //    }
        //    catch (Exception)
        //    {
        //        return "-100";
        //    }
        //}

        private async void Btn_Send_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtBox_comment.Text)) //Check comment box is empty or not
                return;

            bool _isGoOn_On = vm.IsGoOn;
            if (vm.IsGoOn && !vm.PD_or_PM) await vm.PD_Stop();

            vm.Str_Command = txtBox_comment.Text;

            if (vm.Selected_Comport.Equals(vm.Comport_Switch))
            {
                #region switch re-open
                try
                {
                    await vm.Port_Switch_ReOpen();
                }
                catch
                {
                    vm.Str_cmd_read = "Switch Error";
                    return;
                }
                #endregion

                try
                {
                    if (vm.port_Switch.IsOpen)
                    {
                        vm.port_Switch.Write(vm.Str_Command + "\r");

                        await Task.Delay(vm.Int_Read_Delay);

                        int size = vm.port_Switch.BytesToRead;
                        byte[] dataBuffer = new byte[size];
                        int length = vm.port_Switch.Read(dataBuffer, 0, size);

                        //Show read back message
                        vm.Str_cmd_read = anly.GetMessage(dataBuffer);

                        vm.port_Switch.Close();

                        if (!string.IsNullOrWhiteSpace(vm.Str_Command))
                        {
                            MenuItem item = new MenuItem();
                            item.Header = vm.Str_Command;

                            bool _isItemExist = false;
                            foreach (MenuItem i in Btn_cmd_list.ContextMenu.Items)
                            {
                                if (i.Header == item.Header) _isItemExist = true;
                            }

                            if (!_isItemExist)
                            {
                                item.Click += MenuItem_Click;
                                Btn_cmd_list.ContextMenu.Items.Add(item);
                            }
                        }
                    }
                }
                catch { }
            }
            else
            {
                try
                {
                    await vm.Port_ReOpen(vm.Selected_Comport);

                    if (vm.port_PD.IsOpen)
                    {
                        vm.port_PD.Write(vm.Str_Command + "\r");

                        await Task.Delay(vm.Int_Read_Delay);

                        int size = vm.port_PD.BytesToRead;
                        byte[] dataBuffer = new byte[size];
                        int length = vm.port_PD.Read(dataBuffer, 0, size);

                        //Show read back message
                        vm.Str_cmd_read = anly.GetMessage(dataBuffer);

                        vm.port_PD.Close();

                        if (!string.IsNullOrWhiteSpace(vm.Str_Command))
                        {
                            MenuItem item = new MenuItem();
                            item.Header = vm.Str_Command;

                            bool _isItemExist = false;
                            foreach (MenuItem i in Btn_cmd_list.ContextMenu.Items)
                            {
                                if (i.Header == item.Header) _isItemExist = true;
                            }

                            if (!_isItemExist)
                            {
                                item.Click += MenuItem_Click;
                                Btn_cmd_list.ContextMenu.Items.Add(item);
                            }
                        }
                    }
                }
                catch { }
            }

            await Task.Delay(vm.Int_Read_Delay);

            if (_isGoOn_On && !vm.PD_or_PM)
                await vm.PD_GO();
        }


        private async void txtBox_comment_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
            {
                if (e.Key.Equals(System.Windows.Input.Key.Enter))
                {
                    if (string.IsNullOrWhiteSpace(txtBox_comment.Text)) //Check comment box is empty or not
                        return;



                    vm.Str_Command = txtBox_comment.Text;
                    try
                    {
                        bool _isGoOn_On = vm.IsGoOn;

                        await vm.Port_ReOpen(vm.Selected_Comport);

                        if (vm.port_PD.IsOpen)
                        {
                            vm.port_PD.Write(vm.Str_Command + "\r");

                            await Task.Delay(vm.Int_Read_Delay);

                            int size = vm.port_PD.BytesToRead;
                            byte[] dataBuffer = new byte[size];
                            int length = vm.port_PD.Read(dataBuffer, 0, size);

                            //Show read back message
                            vm.Str_cmd_read = anly.GetMessage(dataBuffer);

                            if (!string.IsNullOrWhiteSpace(vm.Str_Command))
                            {
                                MenuItem item = new MenuItem();
                                item.Header = vm.Str_Command;

                                bool _isItemExist = false;
                                foreach (MenuItem i in Btn_cmd_list.ContextMenu.Items)
                                {
                                    if (i.Header == item.Header) _isItemExist = true;
                                }

                                if (!_isItemExist)
                                {
                                    item.Click += MenuItem_Click;
                                    Btn_cmd_list.ContextMenu.Items.Add(item);
                                }
                            }

                            vm.port_PD.DiscardInBuffer();
                            vm.port_PD.DiscardOutBuffer();

                            vm.port_PD.Close();
                        }
                    }
                    catch (Exception ex) { MessageBox.Show(ex.StackTrace.ToString()); }
                }
            }
            catch { }
        }

        private void txtBox_comment_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox obj = (TextBox)sender;

            if (vm.PD_or_PM == false)
                obj.BorderBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF10E2C4"));
            else
                obj.BorderBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#0085CA"));
        }

        private void txtBox_comment_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox obj = (TextBox)sender;
            obj.BorderBrush = new SolidColorBrush(Colors.Gray);

            if (string.IsNullOrEmpty(obj.Text))
                vm.waterPrint1 = "Command";
            else
                vm.waterPrint1 = "";
        }

        private void btn_Stop_Click(object sender, RoutedEventArgs e)
        {
            vm.IsGoOn = false;
            vm.isStop = true;

            //if (win_waiting != null)
            //    if (win_waiting.ShowActivated)
            //        win_waiting.Close();
        }

        private void combox_product_DropDownClosed(object sender, EventArgs e)
        {
            vm.isStop = false;

            vm.Save_Product_Info[0] = vm.product_type;

            setting.Product_Setting();

            vm.Ini_Write("Productions", "Product", vm.product_type);  //創建ini file並寫入基本設定
        }

        private List<double> BestCoeffs = new List<double>();
        List<PointF> Points = new List<PointF>();
        CurveFitting CurveFunctions = new CurveFitting();



        private void K_WL_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            vm.opModel_1.WL_No = 4;
        }

        public async void K_WL_Click(object sender, RoutedEventArgs e)
        {
            #region initial setting
            bool pre_GO_status = vm.IsGoOn;
            vm.IsGoOn = false;
            vm.isStop = false;

            //ComBox_Calibration.IsEditable = false;
            K_WL.IsEnabled = false;  //防呆
            btn_GO.IsEnabled = false;

            if (vm.List_bear_say != null) vm.List_bear_say = new List<List<string>>();

            for (int i = 0; i < vm.list_GaugeModels.Count; i++)
            {
                vm.list_GaugeModels[i].GaugeValue = string.Empty;
                vm.list_GaugeModels[i].GaugeBearSay_1 = string.Empty;
                vm.list_GaugeModels[i].GaugeBearSay_2 = string.Empty;
                vm.list_GaugeModels[i].GaugeBearSay_3 = string.Empty;
                vm.list_GaugeModels[i].DataPoints.Clear();
            }

            anly.JudgeAllBoolGauge();

            vm.isConnected = false;

            vm.isGetPowerWaitForReadBack = vm.Is_FastScan_Mode ? false : true;

            if (!vm.is_BR_OSA)
                await cmd.Connect_TLS();

            if (!vm.isConnected && !vm.is_BR_OSA)
            {
                vm.Str_cmd_read = "Connect Laser Failed";
                vm.Save_Log(new LogMember()
                {
                    isShowMSG = true,
                    Status = "K WL",
                    Message = vm.Str_cmd_read
                });
            }

            #endregion

            try
            {
                List<string> list_finalVoltage = new List<string>();

                //氣密站：掃圖前先判斷產品序號且設定波長 , Animate start
                if (vm.station_type == ComViewModel.StationTypes.Hermetic_Test)
                {
                    if (vm.SN_Judge)
                    {
                        #region Analyze Product Information
                        vm.SNMembers = new ObservableCollection<SN_Member>();

                        for (int i = 0; i < vm.ch_count; i++)
                        {
                            string SN = vm.list_GaugeModels[i].GaugeSN;
                            SN_Member snMem = anly.Product_Info_Anly(SN);
                            vm.SNMembers.Add(snMem);  //Analyze product information and return a member of SN_Information_Group     

                            if (!string.IsNullOrEmpty(snMem.ProductType) && !string.IsNullOrEmpty(snMem.LaserBand))
                                vm.Save_Log("WL Scan", (i + 1).ToString(), snMem.ProductType, snMem.LaserBand);
                        }
                        #endregion

                        list_finalVoltage = await K_V3();  //K V3 
                    }
                    else
                    {
                        cmd.Set_WL();  //Auto Set TLS Center Wavelength by station type and production type

                        if (vm.product_type == "UFA" || vm.product_type == "UFA(H)")
                            list_finalVoltage = await K_V3();   //K V3                
                    }

                    vm.sb_bear_shake.Begin();
                }
                else if (vm.station_type == ComViewModel.StationTypes.UTF600 || vm.station_type == ComViewModel.StationTypes.BR)
                {
                    vm.sb_bear_shake.Begin();
                    vm.Is_switch_mode = false;
                }
                else if (vm.station_type == ComViewModel.StationTypes.TF2)
                {
                    vm.Is_switch_mode = false;
                }
                else
                {
                    //_Page_PD_Gauges.sb_bear_shake.Begin();
                    vm.sb_bear_shake.Begin();
                    vm.Is_switch_mode = false;
                }

                if (!vm.isStop)
                {
                    if (vm.selected_K_WL_Type.Equals("ALL Range") && vm.station_type != ComViewModel.StationTypes.BR)
                    {
                        if (!vm.PD_or_PM && !vm.IsDistributedSystem)
                            await WL_Scan_PDFC();  //Scan action for getting power from PD Fast Clibration
                        else
                            await WL_Scan();
                    }
                    else if (vm.station_type == ComViewModel.StationTypes.BR)
                    {
                        await WL_Scan_BR();
                    }
                    else
                        await WL_Scan_Iteration();
                }
                else
                    vm.Show_Bear_Window("Stop", false, "String", false);

                if (vm.sb_bear_shake != null && vm.sb_bear_reset != null)
                {
                    if (vm.station_type != ComViewModel.StationTypes.TF2)
                    {
                        vm.sb_bear_shake.Pause();
                        vm.sb_bear_reset.Begin();
                    }
                }

                vm.list_collection_GaugeModels.Add(new ObservableCollection<GaugeModel>());
                for (int i = 0; i < vm.list_GaugeModels.Count; i++)
                {
                    vm.list_collection_GaugeModels.Last().Add(new GaugeModel(vm.list_GaugeModels[i]));
                }

                vm.bear_say_all++;
                vm.bear_say_now = vm.bear_say_all;

                K_WL.IsEnabled = true;  //取消防呆
                btn_GO.IsEnabled = true;

                if (pre_GO_status && !vm.IsDistributedSystem)
                    action_go();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.StackTrace.ToString());
                K_WL.IsEnabled = true;  //取消防呆
                btn_GO.IsEnabled = true;
            }
        }

        private async void K_WL_PD_HumanLike(List<string> list_finalVoltage)
        {
            await vm.Port_ReOpen(vm.Selected_Comport);

            vm.Save_All_PD_Value = Analysis.ListDefine<DataPoint>(vm.Save_All_PD_Value, vm.ch_count, new List<DataPoint>());

            List<List<string>> _save_all_WL_and_IL = new List<List<string>>();
            List<string> KV3_finalVoltage = new List<string>();

            var watch = System.Diagnostics.Stopwatch.StartNew();

            vm.Str_Status = "K Wavelength (Rough)";

            //更新圖表
            #region Set Chart data points   
            try
            {
                vm.Chart_All_DataPoints = new List<List<DataPoint>>(vm.Save_All_PD_Value);
                vm.Chart_DataPoints = new List<DataPoint>(vm.Chart_All_DataPoints[0]);  //A lineseries   
            }
            catch { }
            #endregion

            for (int ch = 0; ch < vm.ch_count; ch++)
            {
                if (vm.isStop == true) return;

                if (!vm.Bool_Gauge[ch])
                {
                    _save_all_WL_and_IL.Add(new List<string>());
                    continue;
                }

                List<double> list_ch_power = new List<double>();
                List<double> list_ch_wl = new List<double>();

                double WL_Scan_Gap = vm.float_WL_Scan_Gap;  //預設0.6nm
                double wl_next_start = vm.float_WL_Scan_Start, wl_next_end = vm.float_WL_Scan_End;

                bool _is_best_IL_exist = true;

                vm.Str_Status = "K WL (Round 1)";

                //Scan Round 1
                for (double wl = vm.float_WL_Scan_Start; wl <= vm.float_WL_Scan_End; wl = wl + WL_Scan_Gap)
                {
                    try
                    {
                        if (vm.isStop == true) return;

                        setting.Set_Laser_WL(Math.Round(wl, 2));  //切換TLS WL        

                        await Task.Delay(vm.Int_Set_WL_Delay);

                        //double power = vm.pm.ReadPower();                        
                        double power = double.Parse(await cmd.Get_PD_Value_1ch(ch));

                        vm.Convert_ReadPower_to_UIGauge(power, ch);

                        list_ch_power.Add(power);
                        list_ch_wl.Add(wl);

                        #region Set Chart data points   
                        try
                        {
                            DataPoint dp = new DataPoint(wl, power);
                            vm.Save_All_PD_Value[ch].Add(dp);

                            await Task.Delay(40);

                            vm.Chart_All_DataPoints = new List<List<DataPoint>>(vm.Save_All_PD_Value);
                            vm.Chart_DataPoints = new List<DataPoint>(vm.Chart_All_DataPoints[0]);  //A lineseries   
                        }
                        catch { }
                        #endregion

                        int index = list_ch_power.Count - 1;
                        if (index > 0)
                        {
                            if (list_ch_power[index] < list_ch_power[index - 1])
                            {
                                if (power > -28)
                                {
                                    wl_next_start = Math.Round(wl - WL_Scan_Gap / 2, 2);
                                    wl_next_end = Math.Round(wl_next_start - (WL_Scan_Gap * 3 / 2), 2);
                                    break;
                                }
                                else
                                {
                                    _is_best_IL_exist = false; //已無最佳Loss位置
                                    break;
                                }
                            }
                            else
                            {
                                if (WL_Scan_Gap >= 0.3)
                                {
                                    if (power > -12)   //依IL決定找光間距
                                        WL_Scan_Gap = Math.Round(WL_Scan_Gap / 1.2, 2);
                                    else if (power > -7)
                                        WL_Scan_Gap = Math.Round(WL_Scan_Gap / 1.6, 2);
                                    else if (power < -20)
                                        WL_Scan_Gap = Math.Round(WL_Scan_Gap * 1.2, 2);
                                }
                            }
                        }
                        vm.Str_cmd_read = string.Concat("Ch ", (ch + 1).ToString(), ":", wl.ToString());
                    }
                    catch { vm.Str_cmd_read = "K WL Round 1 Error"; }
                }

                if (!_is_best_IL_exist)
                {
                    vm.Str_cmd_read = "No best IL";
                    _save_all_WL_and_IL.Add(new List<string>() { "NG", " ", " " });
                    vm.List_bear_say = new List<List<string>>(_save_all_WL_and_IL);   //Show Data in row/column (UI)
                    continue;
                }

                WL_Scan_Gap = Math.Round(WL_Scan_Gap / 3, 2);

                double wl_start, wl_end;
                wl_start = wl_next_start;
                wl_end = wl_next_end;

                vm.Str_Status = "K WL (Round 2)";

                //Scan Round 2
                for (double wl = wl_start; wl >= wl_end; wl = wl - WL_Scan_Gap)
                {
                    try
                    {
                        if (vm.isStop == true) return;

                        setting.Set_Laser_WL(Math.Round(wl, 2));  //切換TLS WL         

                        double power = double.Parse(await cmd.Get_PD_Value_1ch(ch));

                        vm.Convert_ReadPower_to_UIGauge(power, ch);
                        list_ch_power.Add(power);
                        list_ch_wl.Add(wl);

                        #region Set Chart data points   
                        try
                        {
                            DataPoint dp = new DataPoint(wl, power);
                            vm.Save_All_PD_Value[ch].Add(dp);

                            await Task.Delay(40);

                            vm.Chart_All_DataPoints = new List<List<DataPoint>>(vm.Save_All_PD_Value);
                            vm.Chart_DataPoints = new List<DataPoint>(vm.Chart_All_DataPoints[0]);  //A lineseries   
                        }
                        catch { }
                        #endregion

                        int index = list_ch_power.Count - 1;
                        if (index > 0)
                        {
                            if (list_ch_power[index] < list_ch_power[index - 1])
                            {
                                if (power > -20)
                                {
                                    wl_next_start = Math.Round(wl + WL_Scan_Gap / 4, 2);
                                    wl_next_end = Math.Round(wl_next_start + (WL_Scan_Gap * 3 / 2), 2);
                                    break;
                                }
                                else
                                {
                                    _is_best_IL_exist = false; //已無最佳Loss位置
                                    break;
                                }
                            }
                            else
                            {
                                if (WL_Scan_Gap >= 0.06)
                                {
                                    if (power > -7)   //依IL決定找光間距
                                        WL_Scan_Gap = Math.Round(WL_Scan_Gap / 2, 2);
                                }
                            }
                        }
                        vm.Str_cmd_read = string.Concat("Ch ", (ch + 1).ToString(), ":", wl.ToString());
                    }
                    catch
                    {
                        vm.Str_cmd_read = "K WL Round 2 Error";
                    }
                }

                if (!_is_best_IL_exist)
                {
                    vm.Str_cmd_read = "No best IL";
                    _save_all_WL_and_IL.Add(new List<string>() { "NG", " ", " " });
                    vm.List_bear_say = new List<List<string>>(_save_all_WL_and_IL);   //Show Data in row/column (UI)
                    continue;
                }

                WL_Scan_Gap = 0.01;
                wl_start = wl_next_start;
                wl_end = wl_next_end + 0.03;

                vm.Str_Status = "K WL (Round 3)";

                //Scan Round 3
                for (double wl = wl_start; wl <= wl_end; wl = wl + WL_Scan_Gap)
                {
                    try
                    {
                        if (vm.isStop == true) return;

                        setting.Set_Laser_WL(Math.Round(wl, 2));  //切換TLS WL         

                        double power = double.Parse(await cmd.Get_PD_Value_1ch(ch));
                        //cmd.Get_PD_Value_1ch(ch);
                        //double power = double.Parse(vm.msg);

                        vm.Convert_ReadPower_to_UIGauge(power, ch);
                        list_ch_power.Add(power);
                        list_ch_wl.Add(wl);

                        #region Set Chart data points   
                        try
                        {
                            DataPoint dp = new DataPoint(wl, power);
                            vm.Save_All_PD_Value[ch].Add(dp);

                            await Task.Delay(40);

                            vm.Chart_All_DataPoints = new List<List<DataPoint>>(vm.Save_All_PD_Value);
                            vm.Chart_DataPoints = new List<DataPoint>(vm.Chart_All_DataPoints[0]);  //A lineseries   
                        }
                        catch { }
                        #endregion

                        int index = list_ch_power.Count - 1;
                        if (index > 0)
                        {
                            if (list_ch_power[index] < list_ch_power[index - 1])
                            {
                                if (power > -48)
                                {
                                    double best_power = list_ch_power.Max();
                                    double best_wl = list_ch_wl[list_ch_power.FindIndex(x => x.Equals(best_power))];

                                    if (list_finalVoltage.Count != vm.ch_count - 1 && list_finalVoltage.Count > 0)   //if k V3 before
                                    {
                                        _save_all_WL_and_IL.Add(new List<string>() { best_wl.ToString(), Math.Round(best_power, 3).ToString(), list_finalVoltage[ch] });
                                        vm.List_bear_say = new List<List<string>>(_save_all_WL_and_IL);   //Show Data in row/column (UI)
                                    }

                                    else  //if no k v3 before
                                    {
                                        _save_all_WL_and_IL.Add(new List<string>() { best_wl.ToString(), Math.Round(best_power, 3).ToString() });
                                        vm.List_bear_say = new List<List<string>>(_save_all_WL_and_IL);   //Show Data in row/column (UI)
                                    }

                                    if (vm.station_type != ComViewModel.StationTypes.Testing)
                                        setting.Set_Laser_WL(best_wl);

                                    break;
                                }
                                else
                                {
                                    //已無最佳Loss位置
                                }
                            }
                        }
                        vm.Str_cmd_read = string.Concat("Ch ", (ch + 1).ToString(), ":", wl.ToString());
                    }
                    catch { vm.Str_cmd_read = "K WL Round 3 Error"; }
                }
            }

            await cmd.Save_Chart();

            var elapsedMs = watch.ElapsedMilliseconds;

            vm.List_bear_say = new List<List<string>>(_save_all_WL_and_IL);   //Show Data in row/column (UI)
            await Task.Delay(50);

            vm.Show_Bear_Window("K WL 完成 (" + Math.Round((decimal)elapsedMs / 1000, 1).ToString() + " s)", false, "String", false);
            //vm.Collection_bear_say.Add(_save_all_WL_and_IL);   //Save data in history record
            vm.bear_say_all++;
            vm.bear_say_now = vm.bear_say_all;

            cmd.Save_Calibration_Data("K WL");  //Save calibration data to txt file

            vm.Str_Status = "K Wavelength Stop";
        }

        private async void K_WL_PD_AllRange()
        {
            List<List<double>> _saved_power = new List<List<double>>();

            await vm.Port_ReOpen(vm.Selected_Comport);

            var watch = System.Diagnostics.Stopwatch.StartNew();

            #region Rough scan
            for (double wl = vm.float_WL_Scan_Start; wl <= vm.float_WL_Scan_End; wl = wl + vm.float_WL_Scan_Gap)
            {
                if (vm.isStop == true)
                    break;

                setting.Set_Laser_WL(Math.Round(wl, 2));  //逐步切換TLS WL
                vm.Str_cmd_read = wl.ToString();
                await Task.Delay(vm.Int_Set_WL_Delay + 100);

                if (!await cmd.Get_PD_Value())
                {
                    vm.Str_cmd_read = "Get PD Value Error";
                    return;
                }

                _saved_power.Add(vm.Double_Powers);

                for (int ch = 0; ch < 8; ch++)
                {
                    if (!vm.Bool_Gauge[ch]) continue;
                    if (vm.Double_Powers.Count <= ch) continue;
                    DataPoint dp = new DataPoint(wl, vm.Double_Powers[ch]);
                    vm.Save_All_PD_Value[ch] = new List<DataPoint>();
                    vm.Save_All_PD_Value[ch].Add(dp);
                }

                //更新圖表
                #region Set Chart data points   
                try
                {
                    vm.Chart_All_DataPoints = new List<List<DataPoint>>(vm.Save_All_PD_Value);
                    vm.Chart_DataPoints = new List<DataPoint>(vm.Chart_All_DataPoints[0]);  //A lineseries   
                }
                catch { }
                #endregion
            }
            await cmd.Save_Chart();
            #endregion

            List<double> list_wl = new List<double>();
            List<double> list_il = new List<double>();
            List<double> list_best_wl = new List<double>();
            List<List<string>> _save_all_WL_and_IL = new List<List<string>>();

            vm.Save_All_PD_Value.Clear();
            vm.Save_All_PD_Value = new List<List<DataPoint>>()
                {
                    new List<DataPoint>(),new List<DataPoint>(),new List<DataPoint>(),new List<DataPoint>(),
                    new List<DataPoint>(),new List<DataPoint>(),new List<DataPoint>(),new List<DataPoint>()
                };

            vm.Str_Status = "K Wavelength (Detail)";

            for (int ch = 0; ch < 8; ch++)
            {
                if (!vm.Bool_Gauge[ch])
                {
                    list_best_wl.Add(0);
                    continue;
                }
                list_il.Clear();
                list_wl.Clear();

                //取出某一channel的所有讀到的power值，並存在_saved_ch_power變數
                List<double> _saved_ch_power = new List<double>();
                for (int i = 0; i < _saved_power.Count; i++)
                {
                    if (_saved_power[i].Count <= ch) continue;
                    _saved_ch_power.Add(_saved_power[i][ch]);
                }

                //找到Rough Scan時最大power時的WL值
                int wl_index = _saved_ch_power.FindIndex(x => x.Equals(_saved_ch_power.Max()));
                double Best_WL = vm.float_WL_Scan_Start + vm.float_WL_Scan_Gap * wl_index;

                #region Create new scan range (Detail scan)       
                for (double wl = Best_WL - vm.float_WL_Scan_Gap * 1; wl <= Best_WL + vm.float_WL_Scan_Gap * 1; wl = wl + 0.01)
                {
                    if (vm.isStop == true)
                        return;

                    setting.Set_Laser_WL(Math.Round(wl, 2));  //逐步切換TLS WL
                    vm.Str_cmd_read = "Ch " + (ch + 1).ToString() + ":" + wl.ToString();
                    await Task.Delay(vm.Int_Set_WL_Delay);

                    vm.Double_Powers = Analysis.ListDefault<double>(8);

                    if (!vm.Bool_Gauge[ch]) continue;

                    if (vm.Bool_Gauge[ch])
                    {
                        if (!await cmd.Get_PD_Value())
                        {
                            vm.Str_cmd_read = "Get PD Value Error";
                            return;
                        }
                    }

                    list_wl.Add(wl);  //Save every WL into list
                    list_il.Add(vm.Double_Powers[ch]);

                    if (vm.Double_Powers.Count <= ch) continue;

                    DataPoint dp = new DataPoint(wl, vm.Double_Powers[ch]);
                    vm.Save_All_PD_Value[ch].Add(dp);

                    _saved_power.Add(vm.Double_Powers);

                    //更新圖表
                    #region Set Chart data points   
                    try
                    {
                        vm.Chart_All_DataPoints = new List<List<DataPoint>>(vm.Save_All_PD_Value);
                        vm.Chart_DataPoints = new List<DataPoint>(vm.Chart_All_DataPoints[0]);  //A lineseries   
                    }
                    catch { }
                    #endregion
                }
                #endregion

                int index = list_il.FindIndex(x => x.Equals(list_il.Max()));
                double best_wl = Math.Round(list_wl[index], 2);
                list_best_wl.Add(best_wl);
            }

            await cmd.Save_Chart();

            //Collect data for Bear say
            _save_all_WL_and_IL.Clear();
            for (int ch = 0; ch < 8; ch++)
            {
                if (!vm.Bool_Gauge[ch])
                {
                    _save_all_WL_and_IL.Add(new List<string>());
                    continue;
                }

                vm.tls.SetWL(list_best_wl[ch]);
                await Task.Delay(vm.Int_Set_WL_Delay);
                await cmd.Get_PD_Value();
                _save_all_WL_and_IL.Add(new List<string>() { list_best_wl[ch].ToString(), Math.Round(vm.Double_Powers[ch], 3).ToString() });
            }

            var elapsedMs = watch.ElapsedMilliseconds;

            vm.List_bear_say = new List<List<string>>(_save_all_WL_and_IL);   //Show Data in row/column (UI)
            await Task.Delay(50);

            vm.Show_Bear_Window("K WL 完成 (" + Math.Round((decimal)elapsedMs / 1000, 1).ToString() + " s)", false, "String", false);
            //vm.Collection_bear_say.Add(_save_all_WL_and_IL);   //Save data in history record
            vm.bear_say_all++;
            vm.bear_say_now = vm.bear_say_all;

            cmd.Save_Calibration_Data("K WL");  //Save calibration data to txt file

            vm.Str_Status = "K Wavelength Stop";
        }

        private async void K_WL_PM_12CH(List<string> list_finalVoltage)
        {
            vm.Save_All_PD_Value = Analysis.ListDefine<DataPoint>(vm.Save_All_PD_Value, vm.ch_count, new List<DataPoint>());

            List<List<string>> _save_all_WL_and_IL = new List<List<string>>();
            List<string> KV3_finalVoltage = new List<string>();

            vm.List_bear_say = new List<List<string>>();

            if (vm.IsGoOn == true)
                await vm.PM_Stop();

            var watch = System.Diagnostics.Stopwatch.StartNew();

            vm.Str_Status = "K Wavelength (Rough)";

            //更新圖表
            #region Set Chart data points   
            try
            {
                vm.Chart_All_DataPoints = new List<List<DataPoint>>(vm.Save_All_PD_Value);
                vm.Chart_DataPoints = new List<DataPoint>(vm.Chart_All_DataPoints[0]);  //A lineseries   
            }
            catch { }
            #endregion

            for (int ch = 0; ch < vm.ch_count; ch++)
            {
                if (vm.isStop == true) return;

                if (!vm.Bool_Gauge[ch])  //Check choosed channel and analyze SN
                {
                    _save_all_WL_and_IL.Add(new List<string>());
                    continue;
                }
                else
                {
                    if (vm.SN_Judge)
                    {
                        if (vm.SNMembers == null)
                        {
                            vm.SNMembers = new ObservableCollection<SN_Member>();
                            for (int i = 0; i < vm.ch_count; i++)
                            {
                                vm.SNMembers.Add(new SN_Member());
                            }
                            vm.Save_Log("K WL", "SN_Members empty", false);
                        }
                        if (string.IsNullOrWhiteSpace(vm.SNMembers[ch].ProductType)) //If the Channel SN is empty
                        {
                            vm.product_type = vm.Save_Product_Info[0];
                            vm.selected_band = vm.Save_Product_Info[1];
                        }
                        else
                        {
                            vm.product_type = vm.SNMembers[ch].ProductType;
                            vm.selected_band = vm.SNMembers[ch].LaserBand;

                            #region Laser WL Initial Setting
                            if (vm.selected_band == "C Band")
                            {
                                vm.float_TLS_WL_Range = new float[2] { 1523, 1573 };
                                if (vm.isConnected == false)
                                    if (vm.list_wl != null)
                                        vm.Double_Laser_Wavelength = 1523;
                            }
                            else //L band
                            {
                                vm.float_TLS_WL_Range = new float[2] { 1560, 1620 };
                                if (vm.isConnected == false)
                                    if (vm.list_wl != null)
                                        vm.Double_Laser_Wavelength = 1560;
                            }
                            setting.Product_Setting();
                            //cmd.Set_WL();  //Auto Set TLS Center Wavelength by station type and production type
                            #endregion
                        }
                    }
                }

                #region Set Switch
                if (vm.Is_switch_mode)
                    await vm.Port_Switch_ReOpen();

                if (vm.Is_switch_mode)
                {
                    vm.Str_Command = $"SW0 {(ch + 1)}";
                    try { vm.port_Switch.Write(vm.Str_Command + "\r"); }
                    catch { vm.Str_cmd_read = "Set Switch Error"; return; }
                    //vm.port_Switch.Close();
                    //vm.switch_selected_index = ch + 1;
                    //vm.switch_index = ch;
                    vm.ch = ch + 1;   //Save Switch channel
                    await Task.Delay(vm.Int_Read_Delay);
                }
                #endregion

                List<double> list_ch_power = new List<double>();
                List<double> list_ch_wl = new List<double>();

                double WL_Scan_Gap = vm.float_WL_Scan_Gap;  //預設0.6nm
                double wl_next_start = vm.float_WL_Scan_Start, wl_next_end = vm.float_WL_Scan_End, wl_scan_end_saved = vm.float_WL_Scan_End;

                bool _is_best_IL_exist = true;

                vm.Str_Status = "K WL (Round 1)";

                //Scan Round 1
                for (double wl = vm.float_WL_Scan_Start; wl <= vm.float_WL_Scan_End; wl = wl + WL_Scan_Gap)
                {
                    try
                    {
                        if (vm.isStop == true) return;

                        setting.Set_Laser_WL(Math.Round(wl, 2));  //切換TLS WL         

                        double power = vm.pm.ReadPower();

                        vm.Convert_ReadPower_to_UIGauge(power, ch);

                        list_ch_power.Add(power);
                        list_ch_wl.Add(wl);

                        #region Set Chart data points   
                        try
                        {
                            DataPoint dp = new DataPoint(wl, power);
                            vm.Save_All_PD_Value[ch].Add(dp);

                            await Task.Delay(40);

                            vm.Chart_All_DataPoints = new List<List<DataPoint>>(vm.Save_All_PD_Value);
                            vm.Chart_DataPoints = new List<DataPoint>(vm.Chart_All_DataPoints[0]);  //A lineseries   
                        }
                        catch { }
                        #endregion

                        int index = list_ch_power.Count - 1;
                        if (index > 0)
                        {
                            if (list_ch_power[index] < list_ch_power[index - 1])  //最後一點的光小於倒數第二點
                            {
                                if (power > -28)  //判定為有光且已通過最佳Loss處
                                {
                                    if (list_ch_power[index] < list_ch_power[index - 2])  //若最後一點的IL小於倒數第三點，則Best IL is at the second last segment.
                                    {
                                        wl_next_start = Math.Round((wl - WL_Scan_Gap) - WL_Scan_Gap / 6, 2);
                                        wl_next_end = Math.Round(wl_next_start - (WL_Scan_Gap * 3 / 2), 2);
                                    }
                                    else  //Best IL is at the last segment
                                    {
                                        wl_next_start = Math.Round(wl - WL_Scan_Gap / 2, 2);
                                        wl_next_end = Math.Round(wl_next_start - (WL_Scan_Gap * 3 / 2), 2);
                                    }
                                    vm.Save_Log(vm.Str_Status, (ch + 1).ToString(), power.ToString(), "IL > -28 dbm");
                                    break;
                                }
                                else
                                {
                                    if (index > 4)  //找四個點後還是無光
                                    {
                                        _is_best_IL_exist = false; //已無最佳Loss位置      
                                        vm.Save_Log(vm.Str_Status, (ch + 1).ToString(), power.ToString(), "No power");
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                if (WL_Scan_Gap >= 0.3)
                                {
                                    if (power >= -7)
                                        WL_Scan_Gap = Math.Round(WL_Scan_Gap / 1.6, 2);
                                    else if (power >= -12 && power < -7)   //依IL決定找光間距
                                        WL_Scan_Gap = Math.Round(WL_Scan_Gap / 1.2, 2);
                                    else if (power >= -25 && power < -12)
                                        WL_Scan_Gap = Math.Round(WL_Scan_Gap / 1, 2);
                                    else if (power < -25)
                                        WL_Scan_Gap = Math.Round(WL_Scan_Gap * 1.2, 2);
                                }

                                //延伸尋找
                                //if (power > -25 && (wl + WL_Scan_Gap) > vm.float_WL_Scan_End && vm.float_WL_Scan_End < wl_scan_end_saved)  
                                //{
                                //    vm.float_WL_Scan_End += WL_Scan_Gap;
                                //}
                            }
                        }
                        vm.Str_cmd_read = string.Concat("Ch ", (ch + 1).ToString(), ":", wl.ToString());
                    }
                    catch { vm.Str_cmd_read = "K WL Round 1 Error"; }
                }

                if (_is_best_IL_exist)
                {
                    foreach (double p in list_ch_power)
                    {
                        _is_best_IL_exist = p >= -45 ? true : false;
                    }
                }

                if (!_is_best_IL_exist)
                {
                    vm.Str_cmd_read = "No best IL";
                    vm.Save_Log(vm.Str_Status, (ch + 1).ToString(), vm.Str_cmd_read);
                    _save_all_WL_and_IL.Add(new List<string>() { "NG", " ", " " });
                    vm.List_bear_say = new List<List<string>>(_save_all_WL_and_IL);   //Show Data in row/column (UI)
                    continue;
                }

                WL_Scan_Gap = Math.Round(WL_Scan_Gap / 3, 2);

                double wl_start, wl_end;
                wl_start = wl_next_start;
                wl_end = wl_next_end;

                vm.Str_Status = "K WL (Round 2)";

                //Scan Round 2
                for (double wl = wl_start; wl >= wl_end; wl = wl - WL_Scan_Gap)
                {
                    try
                    {
                        if (vm.isStop == true) return;

                        setting.Set_Laser_WL(Math.Round(wl, 2));  //切換TLS WL         

                        double power = vm.pm.ReadPower();
                        vm.Convert_ReadPower_to_UIGauge(power, ch);
                        list_ch_power.Add(power);
                        list_ch_wl.Add(wl);

                        #region Set Chart data points   
                        try
                        {
                            DataPoint dp = new DataPoint(wl, power);
                            vm.Save_All_PD_Value[ch].Add(dp);

                            await Task.Delay(40);

                            vm.Chart_All_DataPoints = new List<List<DataPoint>>(vm.Save_All_PD_Value);
                            vm.Chart_DataPoints = new List<DataPoint>(vm.Chart_All_DataPoints[0]);  //A lineseries   
                        }
                        catch { }
                        #endregion

                        int index = list_ch_power.Count - 1;
                        if (index > 0)
                        {
                            if (list_ch_power[index] < list_ch_power[index - 1])
                            {
                                if (power > -20)
                                {
                                    wl_next_start = Math.Round(wl + WL_Scan_Gap / 3, 2);
                                    wl_next_end = Math.Round(wl_next_start + (WL_Scan_Gap * 3 / 2), 2);
                                    vm.Save_Log(vm.Str_Status, (ch + 1).ToString(), power.ToString(), "IL > -20 dbm");
                                    break;
                                }
                                else
                                {
                                    _is_best_IL_exist = false; //已無最佳Loss位置
                                    vm.Save_Log(vm.Str_Status, (ch + 1).ToString(), power.ToString(), "No power");
                                    break;
                                }
                            }
                            else
                            {
                                if (WL_Scan_Gap >= 0.06)
                                {
                                    if (power > -7)   //依IL決定找光間距
                                        WL_Scan_Gap = Math.Round(WL_Scan_Gap / 2, 2);
                                }
                            }
                        }
                        vm.Str_cmd_read = string.Concat("Ch ", (ch + 1).ToString(), ":", wl.ToString());
                    }
                    catch
                    {
                        vm.Str_cmd_read = "K WL Round 2 Error";
                        vm.Save_Log("K WL", vm.Str_cmd_read, false);
                    }
                }

                if (!_is_best_IL_exist)
                {
                    vm.Str_cmd_read = "No best IL";
                    _save_all_WL_and_IL.Add(new List<string>() { "NG", " ", " " });
                    vm.List_bear_say = new List<List<string>>(_save_all_WL_and_IL);   //Show Data in row/column (UI)
                    vm.Save_Log("Scan round 2", (ch + 1).ToString(), "", "No power");
                    continue;
                }


                //Scan Round 3
                WL_Scan_Gap = (WL_Scan_Gap / 5) < 0.04 ? 0.04 : Math.Round(WL_Scan_Gap / 6, 2);
                wl_start = wl_next_start;
                wl_end = wl_next_end;

                vm.Str_Status = "K WL (Round 3)";

                for (double wl = wl_start; wl <= wl_end; wl = wl + WL_Scan_Gap)
                {
                    try
                    {
                        if (vm.isStop == true) return;

                        wl = Math.Round(wl, 2);

                        setting.Set_Laser_WL(wl);  //切換TLS WL         

                        double power = vm.pm.ReadPower();
                        vm.Convert_ReadPower_to_UIGauge(power, ch);
                        list_ch_power.Add(power);
                        list_ch_wl.Add(wl);

                        #region Set Chart data points   
                        try
                        {
                            DataPoint dp = new DataPoint(wl, power);
                            vm.Save_All_PD_Value[ch].Add(dp);

                            await Task.Delay(40);

                            vm.Chart_All_DataPoints = new List<List<DataPoint>>(vm.Save_All_PD_Value);
                            vm.Chart_DataPoints = new List<DataPoint>(vm.Chart_All_DataPoints[0]);  //A lineseries   
                        }
                        catch { }
                        #endregion

                        int index = list_ch_power.Count - 1;
                        if (index > 0)
                        {
                            if (list_ch_power[index] < list_ch_power[index - 1])
                            {
                                if (power > -20)
                                {
                                    wl_next_start = wl - 0.01;
                                    wl_next_end = wl - WL_Scan_Gap;
                                    vm.Save_Log(vm.Str_Status, (ch + 1).ToString(), power.ToString(), "IL > -20 dbm");
                                    break;
                                }
                                else
                                {
                                    _is_best_IL_exist = false; //已無最佳Loss位置
                                    vm.Str_cmd_read = "No best IL";
                                    vm.Save_Log(vm.Str_Status, (ch + 1).ToString(), power.ToString(), "No best IL");
                                    break;
                                }
                            }
                            else  //最佳光不在此範圍，需擴大尋找
                            {
                                wl_end += WL_Scan_Gap;
                            }
                        }
                        vm.Str_cmd_read = string.Concat("Ch ", (ch + 1).ToString(), ":", wl.ToString());
                    }
                    catch
                    {
                        vm.Str_cmd_read = "K WL Round 3 Error";
                        vm.Save_Log("K WL", vm.Str_cmd_read, false);
                    }
                }

                WL_Scan_Gap = 0.01;
                wl_start = wl_next_start;
                wl_end = wl_next_end;

                vm.Str_Status = "K WL (Round 4)";

                //Scan Round 4
                for (double wl = wl_start; wl >= wl_end; wl = wl - WL_Scan_Gap)
                {
                    try
                    {
                        if (vm.isStop == true) return;

                        setting.Set_Laser_WL(Math.Round(wl, 2));  //切換TLS WL         

                        double power = vm.pm.ReadPower();

                        if (power < -80)
                        {
                            vm.Str_cmd_read = "IL too low";
                            vm.Save_Log("K WL", "IL too low", false);
                            vm.Save_Log(vm.Str_Status, (ch + 1).ToString(), power.ToString(), "IL too low");
                            break;
                        }

                        vm.Convert_ReadPower_to_UIGauge(power, ch);
                        list_ch_power.Add(power);
                        list_ch_wl.Add(wl);

                        #region Set Chart data points   
                        try
                        {
                            DataPoint dp = new DataPoint(wl, power);
                            vm.Save_All_PD_Value[ch].Add(dp);

                            await Task.Delay(40);

                            vm.Chart_All_DataPoints = new List<List<DataPoint>>(vm.Save_All_PD_Value);
                            vm.Chart_DataPoints = new List<DataPoint>(vm.Chart_All_DataPoints[0]);  //A lineseries   
                        }
                        catch { }
                        #endregion

                        int index = list_ch_power.Count - 1;
                        if (index > 0)
                        {
                            if (list_ch_power[index] < list_ch_power[index - 1])  //若最後一點IL小於倒數第二點
                            {
                                if (power > -48)
                                {
                                    double best_power = list_ch_power.Max();
                                    double best_wl = list_ch_wl[list_ch_power.FindIndex(x => x.Equals(best_power))];

                                    if (list_finalVoltage.Count > 0)   //if k V3 before
                                    {
                                        if (ch < list_finalVoltage.Count)
                                            _save_all_WL_and_IL.Add(new List<string>() { best_wl.ToString(), Math.Round(best_power, 3).ToString(), list_finalVoltage[ch] });
                                        vm.List_bear_say = new List<List<string>>(_save_all_WL_and_IL);   //Show Data in row/column (UI)
                                    }

                                    else  //if no k v3 before
                                    {

                                        _save_all_WL_and_IL.Add(new List<string>() { best_wl.ToString(), Math.Round(best_power, 3).ToString() });
                                        vm.List_bear_say = new List<List<string>>(_save_all_WL_and_IL);   //Show Data in row/column (UI)
                                    }

                                    if (vm.station_type != ComViewModel.StationTypes.Testing)
                                        setting.Set_Laser_WL(best_wl);

                                    vm.Save_Log(vm.Str_Status, (ch + 1).ToString(), power.ToString(), "Gotcha!");
                                    break;
                                }
                                else
                                {
                                    _is_best_IL_exist = false; //已無最佳Loss位置
                                    vm.Save_Log(vm.Str_Status, (ch + 1).ToString(), power.ToString(), "No best IL");
                                    break;
                                }
                            }
                            else
                            {
                                if (wl == wl_end)  //若到本範圍內的最後一點仍通過最佳IL處，則再延伸找光範圍
                                {
                                    wl_end -= 0.01;
                                }
                            }
                        }
                        vm.Str_cmd_read = string.Concat("Ch ", (ch + 1).ToString(), ":", wl.ToString());
                    }
                    catch
                    {
                        vm.Str_cmd_read = "K WL Round 4 Error";
                        vm.Save_Log(vm.Str_Status, vm.Str_cmd_read, false);
                    }
                }
            }

            vm.List_bear_say = new List<List<string>>(_save_all_WL_and_IL);   //Show Data in row/column (UI)
            await Task.Delay(150);

            await cmd.Save_Chart();

            var elapsedMs = watch.ElapsedMilliseconds;

            vm.Show_Bear_Window("K WL 完成 (" + Math.Round((decimal)elapsedMs / 1000, 1).ToString() + " s)", false, "String", false);
            vm.bear_say_all++;
            vm.bear_say_now = vm.bear_say_all;

            if (vm.Is_switch_mode)
                if (vm.port_Switch != null)
                    if (vm.port_Switch.IsOpen) vm.port_Switch.Close();

            cmd.Save_Calibration_Data("K WL");  //Save calibration data to txt file

            vm.Str_Status = "K Wavelength Stop";

            vm.product_type = vm.Save_Product_Info[0];
            vm.selected_band = vm.Save_Product_Info[1];

            if (vm.IsGoOn == true)
                vm.PM_GO();
        }

        private async void K_WL_PM_12CH_AllRange()
        {
            List<List<double>> _saved_power = new List<List<double>>();
            vm.Save_All_PD_Value = Analysis.ListDefine<DataPoint>(vm.Save_All_PD_Value, vm.ch_count, new List<DataPoint>());

            if (vm.IsGoOn == true)
                await vm.PM_Stop();

            var watch = System.Diagnostics.Stopwatch.StartNew();

            vm.Str_Status = "K Wavelength (Rough)";
            for (double wl = vm.float_WL_Scan_Start; wl <= vm.float_WL_Scan_End; wl = wl + vm.float_WL_Scan_Gap)
            {
                if (vm.isStop == true) return;

                setting.Set_Laser_WL(Math.Round(wl, 2));  //切換TLS WL         

                vm.Double_Powers = Analysis.ListDefault<double>(vm.ch_count);

                await vm.Port_Switch_ReOpen();
                for (int ch = 0; ch < vm.ch_count; ch++)
                {
                    if (vm.isStop == true) return;

                    if (!vm.Bool_Gauge[ch]) continue;

                    #region Set Switch
                    vm.Str_Command = "SW0 " + (ch + 1).ToString();
                    try { vm.port_Switch.Write(vm.Str_Command + "\r"); }
                    catch { vm.Str_cmd_read = "Set Switch Error"; return; }

                    await Task.Delay(vm.Int_Read_Delay);
                    #endregion

                    vm.Str_cmd_read = "Ch " + (ch + 1).ToString() + ":" + wl.ToString();

                    await cmd.Get_PM_Value(ch);

                    DataPoint dp = new DataPoint(wl, vm.Double_Powers[ch]);
                    vm.Save_All_PD_Value[ch].Add(dp);
                }
                _saved_power.Add(vm.Double_Powers);

                //更新圖表
                #region Set Chart data points   
                try
                {
                    vm.Chart_All_DataPoints = new List<List<DataPoint>>(vm.Save_All_PD_Value);
                    vm.Chart_DataPoints = new List<DataPoint>(vm.Chart_All_DataPoints[0]);  //A lineseries   
                }
                catch { }
                #endregion
            }
            await cmd.Save_Chart();

            List<double> list_wl = Analysis.ListDefault<double>(vm.ch_count);
            List<List<string>> _save_all_WL_and_IL = Analysis.ListDefine<string>(new List<List<string>>(), vm.ch_count, new List<string>());

            vm.Save_All_PD_Value.Clear();
            vm.Save_All_PD_Value = Analysis.ListDefine<DataPoint>(new List<List<DataPoint>>(), vm.ch_count, new List<DataPoint>());

            vm.Str_Status = "K Wavelength (Detail)";
            for (int ch = 0; ch < vm.ch_count; ch++)
            {
                if (vm.isStop == true) return;
                if (!vm.Bool_Gauge[ch]) continue;

                //取出某一channel的所有讀到的power值，並存在_saved_ch_power變數
                List<double> _saved_ch_power = new List<double>();
                for (int i = 0; i < _saved_power.Count; i++)
                    _saved_ch_power.Add(_saved_power[i][ch]);

                //找到最大power時的WL值
                int wl_index = _saved_ch_power.FindIndex(x => x.Equals(_saved_ch_power.Max()));
                double Best_WL = vm.float_WL_Scan_Start + vm.float_WL_Scan_Gap * wl_index;
                list_wl[ch] = (Best_WL);  //所有channel的最佳loss相對應的波長

                #region Set Switch
                await vm.Port_Switch_ReOpen();
                try
                {
                    vm.Str_Command = "SW0 " + (ch + 1).ToString();
                    vm.port_Switch.Write(vm.Str_Command + "\r");
                    await Task.Delay(vm.Int_Read_Delay);
                }
                catch { vm.Str_cmd_read = "Set Switch Error"; return; }
                #endregion

                #region Create new scan range (Detail scan)       
                for (double wl = Best_WL - vm.float_WL_Scan_Gap * 2; wl <= Best_WL + vm.float_WL_Scan_Gap * 2; wl = wl + vm.float_WL_Scan_Gap / 2)
                {
                    if (vm.isStop == true) return;

                    setting.Set_Laser_WL(Math.Round(wl, 2));  //逐步切換TLS WL
                    vm.Str_cmd_read = "Ch " + (ch + 1).ToString() + ":" + wl.ToString();
                    await Task.Delay(vm.Int_Set_WL_Delay);

                    vm.Double_Powers = Analysis.ListDefault<double>(vm.ch_count);

                    if (!vm.Bool_Gauge[ch]) continue;

                    if (vm.Bool_Gauge[ch]) await cmd.Get_PM_Value(ch);

                    DataPoint dp = new DataPoint(wl, vm.Double_Powers[ch]);
                    vm.Save_All_PD_Value[ch].Add(dp);

                    //anly.Process_Schedule(process_step++, process_steps);

                    //更新圖表
                    #region Set Chart data points   
                    try
                    {
                        vm.Chart_All_DataPoints = new List<List<DataPoint>>(vm.Save_All_PD_Value);
                        vm.Chart_DataPoints = new List<DataPoint>(vm.Chart_All_DataPoints[0]);  //A lineseries   
                    }
                    catch { }
                    #endregion
                }
                #endregion
            }

            await cmd.Save_Chart();

            _save_all_WL_and_IL = new List<List<string>>();
            vm.Double_Powers = Analysis.ListDefault<double>(vm.ch_count);
            for (int ch = 0; ch < vm.ch_count; ch++)
            {
                if (!vm.Bool_Gauge[ch])
                {
                    _save_all_WL_and_IL.Add(new List<string>());
                    continue;
                }

                #region Set Switch
                if (vm.ch != 0)
                {
                    await vm.Port_Switch_ReOpen();
                    try
                    {
                        vm.Str_Command = "SW0 " + (ch + 1).ToString();
                        vm.port_Switch.Write(vm.Str_Command + "\r");
                        await Task.Delay(vm.Int_Read_Delay);
                    }
                    catch { vm.Str_cmd_read = "Set Switch Error"; return; }
                }
                #endregion

                #region Final Scan (9 points)  
                int cnt = 0;
                List<double> _saved_ch_wl = new List<double>();
                List<double> _saved_ch_power = new List<double>();
                for (double wl = vm.List_curfit_resultWL[ch] - 0.01 * 4; wl <= vm.List_curfit_resultWL[ch] + 0.01 * 4; wl = wl + 0.01)
                {
                    if (vm.isStop == true) return;

                    setting.Set_Laser_WL(Math.Round(wl, 2));  //逐步切換TLS WL
                    vm.Str_cmd_read = "Ch " + (ch + 1).ToString() + ":" + wl.ToString();

                    if (!vm.Bool_Gauge[ch]) continue;

                    await cmd.Get_PM_Value(ch);

                    _saved_ch_wl.Add(wl);
                    _saved_ch_power.Add(vm.Double_Powers[ch]);

                    //anly.Process_Schedule(process_step++, process_steps);

                    if (cnt > 0)
                    {
                        if (_saved_ch_power[cnt] - _saved_ch_power[cnt - 1] < -0.05)
                        {
                            break;
                        }
                    }
                    cnt++;
                }

                double best_power = _saved_ch_power.Max();
                double best_wl = _saved_ch_wl[_saved_ch_power.FindIndex(x => x.Equals(best_power))];

                //Final Check Best IL
                //cmd.Set_WL(best_wl);
                //await Task.Delay(vm.Int_Set_WL_Delay);                
                //best_power = vm.pm.ReadPower();
                //await Task.Delay(vm.Int_Set_WL_Delay);

                _save_all_WL_and_IL.Add(new List<string>() { best_wl.ToString(), Math.Round(best_power, 3).ToString() });
                #endregion
            }

            var elapsedMs = watch.ElapsedMilliseconds;

            vm.List_bear_say = new List<List<string>>(_save_all_WL_and_IL);
            await Task.Delay(50);

            vm.Show_Bear_Window("K WL 完成" + "  (" + elapsedMs.ToString() + " ms)", false, "String", false);
            vm.bear_say_all++;
            vm.bear_say_now = vm.bear_say_all;

            cmd.Save_Calibration_Data("K WL");

            vm.Str_Status = "K Wavelength Stop";

            vm.analysis = anly;

            if (vm.IsGoOn == true)
                vm.PM_GO();
        }

        public async Task<bool> WL_Scan()
        {
            try
            {
                vm.Save_All_PD_Value = Analysis.ListDefine<DataPoint>(vm.Save_All_PD_Value, vm.ch_count, new List<DataPoint>());
                vm.IL_ALL = "";
                vm.Double_Powers = new List<double>();
                vm.Double_Powers.AddRange(Enumerable.Repeat(0.0, vm.ch_count));

                vm.Chart_title = "Power X Wavelength";
                vm.Chart_x_title = "Wavelength (nm)";
                vm.Chart_y_title = vm.dB_or_dBm ? "IL (dB)" : "IL (dBm)";

                vm.Update_ALL_PlotView_Title(vm.Chart_title, vm.Chart_x_title, vm.Chart_y_title);

                #region Bandwith UI initialization
                //vm.SpecLine30dB = new ObservableCollection<DataPoint>();
                anly.list_SMR_slope.Clear();
                anly.list_SMR_Mountain_IL.Clear();
                anly.list_SMR_Mountain.Clear();

                for (int i = 0; i < vm.ch_count; i++)
                {
                    vm.ChartNowModel.list_dataPoints[i].Clear();
                    vm.ChartNowModel.SMRR = 0;
                    vm.ChartNowModel.FOM = 0;
                }

                cmd.Clean_Chart();

                #endregion

                var watch = Stopwatch.StartNew();
                var elapsedMs = watch.ElapsedMilliseconds;
                decimal scan_timespan = (elapsedMs / (decimal)1000);

                vm.Save_Log("WL Scan", vm.ch_count.ToString(), vm.float_WL_Scan_Start.ToString(), vm.float_WL_Scan_End.ToString());

                vm.Str_Status = "WL Scan";

                #region Build scan wl list
                vm.wl_list.Clear();
                vm.wl_list_index = 0;

                if (vm.float_WL_Scan_End >= vm.float_WL_Scan_Start)
                {
                    for (double wl = vm.float_WL_Scan_Start; wl <= vm.float_WL_Scan_End; wl = wl + vm.float_WL_Scan_Gap)
                    {
                        vm.wl_list.Add(Math.Round(wl, 2));
                    }
                }
                else
                {
                    for (double wl = vm.float_WL_Scan_Start; wl >= vm.float_WL_Scan_End; wl = wl - vm.float_WL_Scan_Gap)
                    {
                        vm.wl_list.Add(Math.Round(wl, 2));
                    }
                }
                #endregion

                for (int i = 0; i < vm.ch_count; i++)
                {
                    vm.ChartNowModel.list_dataPoints[i].Clear();
                    vm.Plot_Series[i].Points.Clear();
                }

                foreach (double wl in vm.wl_list)
                {
                    if (vm.isStop) break;

                    double WL = Math.Round(wl, 2);

                    await cmd.Set_WL(wl, false);

                    if (wl == vm.wl_list.First())
                        await Task.Delay(600);

                    if (!(!vm.IsDistributedSystem && !vm.PD_or_PM && vm.Is_FastScan_Mode))
                        await Task.Delay(vm.Int_Set_WL_Delay);

                    vm.Double_Laser_Wavelength = wl;

                    for (int ch = 0; ch < vm.ch_count; ch++)
                    {
                        if (vm.isStop) break;

                        if (!vm.BoolAllGauge)
                            if (!vm.list_GaugeModels[ch].boolGauge) continue;

                        if (vm.ch_count > 1)
                            vm.Str_Status = $"WL Scan - Ch {ch + 1}";
                        else
                            vm.Str_Status = $"WL Scan";

                        #region switch re-open && Switch write Cmd

                        if (vm.station_type == ComViewModel.StationTypes.Hermetic_Test && vm.Is_switch_mode)
                        {
                            vm.switch_index = ch + 1;
                            try
                            {
                                await vm.Port_Switch_ReOpen();

                                vm.Str_Command = "SW0 " + vm.switch_index.ToString();
                                vm.port_Switch.Write(vm.Str_Command + "\r");
                                await Task.Delay(vm.Int_Write_Delay);

                                vm.port_Switch.DiscardInBuffer();       // RX
                                vm.port_Switch.DiscardOutBuffer();      // TX

                                vm.port_Switch.Close();
                            }
                            catch (Exception ex)
                            {
                                vm.Str_cmd_read = "Set Switch Error";
                                vm.Save_Log(vm.Str_Status, vm.switch_index.ToString(), vm.Str_cmd_read);
                                MessageBox.Show(ex.StackTrace.ToString());
                                return false;
                            }
                        }
                        #endregion

                        if (vm.Is_FastScan_Mode)
                        {
                            if (wl == vm.wl_list.Last())
                            {
                                vm.isGetPowerWaitForReadBack = true;
                                List<List<double>> listDD = await cmd.Get_Power(ch, true);
                            }
                            else
                            {
                                vm.isGetPowerWaitForReadBack = false;
                                if (wl == vm.wl_list.First())
                                {
                                    vm.ChartNowModel.list_dataPoints[ch].Clear();
                                    vm.Save_All_PD_Value[ch].Clear();

                                    if (vm.port_PD != null && vm.port_PD.IsOpen)
                                    {
                                        vm.port_PD.DiscardInBuffer();
                                        vm.port_PD.DiscardOutBuffer();
                                    }
                                }

                                await cmd.Get_Power(ch, false);
                            }
                        }
                        else
                        {
                            if (wl == vm.wl_list.First())
                            {
                                vm.ChartNowModel.list_dataPoints[ch].Clear();
                                vm.Save_All_PD_Value[ch].Clear();

                                vm.Plot_Series[ch].Points.Clear();
                            }

                            await cmd.Get_Power(ch, true);

                            //Only distributedSystem will auto update chart
                            if (!vm.IsDistributedSystem)
                            {
                                DataPoint dp = new DataPoint(vm.Double_Laser_Wavelength, vm.Double_Powers[ch]);
                                vm.ChartNowModel.list_dataPoints[ch].Add(dp);
                                vm.Save_All_PD_Value[ch].Add(dp);

                                vm.Plot_Series[ch].Points.Add(dp);
                            }
                        }
                    }

                    //更新圖表
                    #region Set Chart data points   
                    vm.Chart_All_DataPoints = new List<List<DataPoint>>(vm.Save_All_PD_Value);
                    vm.Chart_DataPoints = new List<DataPoint>(vm.Chart_All_DataPoints[0]);  //A lineseries  

                    vm.Update_ALL_PlotView();

                    scan_timespan = watch.ElapsedMilliseconds / (decimal)1000;
                    vm.ChartNowModel.TimeSpan = (double)Math.Round(scan_timespan, 1);
                    vm.msgModel.msg_3 = Math.Round(scan_timespan, 1).ToString() + " s";  //Update Main UI timespan
                    #endregion

                    anly.BandWidth_Calculation();

                    if (wl == vm.wl_list.First())
                    {
                        vm.maxIL = new List<double>(vm.Double_Powers);
                        vm.minIL = new List<double>(vm.Double_Powers);
                    }
                    else
                        cmd.Update_DeltaIL();  //Update delta IL data in Chart UI
                }

                //Calculate the position with maximum IL and show data in UI
                for (int ch = 0; ch < vm.ch_count; ch++)
                {
                    if (vm.Save_All_PD_Value[ch].Count > 0)
                    {
                        double maxIL_WL = vm.Save_All_PD_Value[ch].Where(point => point.Y == vm.Save_All_PD_Value[ch].Max(x => x.Y)).First().X;

                        vm.Double_Laser_Wavelength = maxIL_WL;
                        await cmd.Set_WL(maxIL_WL, false);

                        vm.list_GaugeModels[ch].GaugeValue = vm.Save_All_PD_Value[ch].Max(x => x.Y).ToString();

                        vm.list_GaugeModels[ch].GaugeBearSay_1 = Math.Round(maxIL_WL, 2).ToString();
                        vm.list_GaugeModels[ch].GaugeBearSay_2 = vm.list_GaugeModels[ch].GaugeValue;
                    }
                }

                scan_timespan = watch.ElapsedMilliseconds / (decimal)1000;
                vm.ChartNowModel.TimeSpan = (double)Math.Round(scan_timespan, 1);
                await cmd.Save_Chart();

                vm.msgModel.msg_3 = Math.Round(scan_timespan, 1).ToString() + " s";  //Update Main UI timespan

                if (!vm.isStop)
                    vm.Show_Bear_Window($"WL Scan 完成  ({Math.Round(scan_timespan, 1)} s)", false, "String", false);

                vm.Str_Status = "WL Scan Stop";
                vm.Save_Log("WL Scan", "Stop", false);

                vm.IsGoOn = false;

                //Close TLS filter port for other control action
                cmd.Close_TLS_Filter();

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.StackTrace.ToString());
                return false;
            }
        }

        public async Task<bool> WL_Scan_PDFC()
        {
            try
            {
                vm.Save_All_PD_Value = Analysis.ListDefine<DataPoint>(vm.Save_All_PD_Value, vm.ch_count, new List<DataPoint>());
                vm.IL_ALL = "";
                vm.Double_Powers = new List<double>();
                vm.Double_Powers.AddRange(Enumerable.Repeat(0.0, vm.ch_count));

                vm.Chart_title = "Power X Wavelength";
                vm.Chart_x_title = "Wavelength (nm)";
                vm.Chart_y_title = vm.dB_or_dBm ? "IL (dB)" : "IL (dBm)";

                vm.Update_ALL_PlotView_Title(vm.Chart_title, vm.Chart_x_title, vm.Chart_y_title);

                double cwl = 0;

                var watch = System.Diagnostics.Stopwatch.StartNew();
                var elapsedMs = watch.ElapsedMilliseconds;
                decimal scan_timespan = (elapsedMs / (decimal)1000);

                vm.Save_Log("WL Scan", vm.ch_count.ToString(), vm.float_WL_Scan_Start.ToString(), vm.float_WL_Scan_End.ToString());

                vm.Str_Status = "WL Scan";

                #region Build scan wl list
                vm.wl_list.Clear();
                vm.wl_list_index = 0;

                if (vm.float_WL_Scan_End >= vm.float_WL_Scan_Start)
                {
                    for (double wl = vm.float_WL_Scan_Start; wl <= vm.float_WL_Scan_End; wl = wl + vm.float_WL_Scan_Gap)
                        vm.wl_list.Add(Math.Round(wl, 2));
                }
                else
                {
                    for (double wl = vm.float_WL_Scan_Start; wl >= vm.float_WL_Scan_End; wl = wl - vm.float_WL_Scan_Gap)
                        vm.wl_list.Add(Math.Round(wl, 2));
                }
                #endregion

                for (int i = 0; i < vm.ch_count; i++)
                {
                    vm.Plot_Series[i].Points.Clear();
                    vm.ChartNowModel.list_dataPoints[i].Clear();
                }

                foreach (double wl in vm.wl_list)
                {
                    Stopwatch sw = Stopwatch.StartNew();

                    LogMember lm = new LogMember();

                    if (vm.isStop) return false;

                    double WL = Math.Round(wl, 2);

                    await cmd.Set_WL(wl, false);

                    if (wl == vm.wl_list.First())
                    {
                        await Task.Delay(600);

                        for (int ch = 0; ch < vm.ch_count; ch++)
                            vm.ChartNowModel.list_dataPoints[ch] = new List<DataPoint>();

                        if (vm.port_PD != null && vm.port_PD.IsOpen)
                        {
                            vm.port_PD.DiscardInBuffer();
                            vm.port_PD.DiscardOutBuffer();
                        }
                    }

                    if (!(!vm.IsDistributedSystem && vm.Is_FastScan_Mode))
                        await Task.Delay(vm.Int_Set_WL_Delay);
                    else
                        ControlCmd.Delay(5);

                    vm.Double_Laser_Wavelength = wl;

                    lm.Message = $"WL: {sw.ElapsedMilliseconds.ToString()}";

                    #region Get IL and set chart data points

                    if (vm.Is_FastScan_Mode && wl != vm.wl_list.First() && wl != vm.wl_list.Last())
                        vm.isGetPowerWaitForReadBack = false;
                    else
                        vm.isGetPowerWaitForReadBack = true;

                    vm.Str_cmd_read = $"WL : {WL}";
                    string str = await cmd.Get_Power();

                    lm.Result = $"GetP: {sw.ElapsedMilliseconds.ToString()}";

                    //Only distributedSystem will auto update chart
                    if (!vm.IsDistributedSystem && !string.IsNullOrEmpty(str))
                    {
                        for (int i = 0; i < vm.Double_Powers.Count / vm.ch_count; i++)
                        {
                            for (int ch = 0; ch < vm.ch_count; ch++)
                            {
                                DataPoint dp = new DataPoint(wl, vm.Double_Powers[ch + (i * vm.ch_count)]);
                                vm.ChartNowModel.list_dataPoints[ch].Add(dp);
                                vm.Plot_Series[ch].Points.Add(new DataPoint(wl, vm.Double_Powers[ch + (i * vm.ch_count)]));
                            }
                        }
                    }
                    #endregion

                    //更新圖表
                    #region Set Chart data points   
                    vm.Update_ALL_PlotView();

                    vm.ChartNowModel.TimeSpan = (double)Math.Round(watch.ElapsedMilliseconds / (decimal)1000, 0);
                    vm.msgModel.msg_3 = vm.ChartNowModel.TimeSpan.ToString() + " s";  //Update Main UI timespan

                    if (vm.station_type == ComViewModel.StationTypes.TF2)
                        anly.BandWidth_Calculation(vm.opModel_1.WL_No);
                    else if (vm.station_type == ComViewModel.StationTypes.Testing)
                        anly.BandWidth_Calculation();

                    if (wl == vm.wl_list.First())
                    {
                        vm.maxIL = new List<double>(vm.Double_Powers);
                        vm.minIL = new List<double>(vm.Double_Powers);
                    }
                    else
                        cmd.Update_DeltaIL();  //Update delta IL data in Chart UI
                    #endregion

                    lm.Time = $"{sw.ElapsedMilliseconds.ToString()}";

                    vm.Save_Log(lm);
                }

                if (vm.station_type == ComViewModel.StationTypes.TF2)
                {
                    if (!vm.isStop)
                    {
                        #region Fine Scan if Gap is greater than 0.01

                        if (vm.float_WL_Scan_Gap != 0.01)
                        {
                            vm.Str_Status = "WL Fine Scan";

                            double MaxIL = vm.Chart_DataPoints.OrderBy(o => o.Y).ToList().Last().Y;
                            List<DataPoint> FinalDataPoints = new List<DataPoint>(vm.Chart_DataPoints);
                            double[,] list_BW_WL_Pos = anly.BandWidth_Calculation(vm.opModel_1.WL_No);

                            for (int i = 0; i < 3; i++)
                            {
                                for (int j = 0; j < 2; j++)
                                {
                                    #region Build scan wl list
                                    vm.wl_list.Clear();
                                    vm.wl_list_index = 0;

                                    if (j == 0)
                                    {
                                        //Left
                                        vm.float_WL_Scan_Start = list_BW_WL_Pos[i, j];
                                        vm.float_WL_Scan_End = list_BW_WL_Pos[i, j] + vm.float_WL_Scan_Gap;
                                    }
                                    else
                                    {
                                        //Right
                                        vm.float_WL_Scan_Start = list_BW_WL_Pos[i, j] - vm.float_WL_Scan_Gap;
                                        vm.float_WL_Scan_End = list_BW_WL_Pos[i, j];
                                    }

                                    if (vm.float_WL_Scan_End >= vm.float_WL_Scan_Start)
                                    {
                                        for (double wl = vm.float_WL_Scan_Start; wl <= vm.float_WL_Scan_End; wl = wl + 0.01)
                                        {
                                            vm.wl_list.Add(Math.Round(wl, 2));
                                        }
                                    }
                                    else
                                    {
                                        for (double wl = vm.float_WL_Scan_Start; wl >= vm.float_WL_Scan_End; wl = wl - 0.01)
                                        {
                                            vm.wl_list.Add(Math.Round(wl, 2));
                                        }
                                    }
                                    #endregion

                                    #region Scan WL

                                    double target_IL = 0;
                                    double target_dB = 0;
                                    if (i == 0)
                                        target_dB = vm.opModel_1.BW_Setting_1;
                                    else if (i == 1)
                                        target_dB = vm.opModel_1.BW_Setting_2;
                                    else if (i == 3)
                                        target_dB = vm.opModel_1.BW_Setting_3;
                                    else
                                        continue;

                                    target_IL = MaxIL - target_dB;

                                    if (j == 0)
                                        vm.Str_Status = $"Fine Scan, L, dB:{target_dB}, Target IL:{target_IL}";
                                    else if (j == 1)
                                        vm.Str_Status = $"Fine Scan, R, dB:{target_dB}, Target IL:{target_IL}";

                                    bool isbreak = false;
                                    foreach (double wl in vm.wl_list)
                                    {
                                        if (vm.isStop) return false;

                                        if (isbreak) break;

                                        double WL = Math.Round(wl, 2);

                                        //if (vm.Chart_All_DataPoints[0].Any(o => o.X == wl))
                                        //    continue;

                                        await cmd.Set_WL(wl, false);

                                        if (wl == vm.wl_list.First())
                                            await Task.Delay(vm.Int_Set_WL_Delay + 800);
                                        else
                                            await Task.Delay(vm.Int_Set_WL_Delay);

                                        vm.Double_Laser_Wavelength = wl;

                                        for (int ch = 0; ch < vm.ch_count; ch++)
                                        {
                                            if (vm.isStop) return false;

                                            if (!vm.BoolAllGauge)
                                                if (!vm.list_GaugeModels[ch].boolGauge) continue;

                                            #region switch re-open && Switch write Cmd
                                            if (vm.station_type == ComViewModel.StationTypes.Hermetic_Test && vm.Is_switch_mode)
                                            {
                                                vm.Str_cmd_read = "Ch " + (ch + 1).ToString() + " : " + WL.ToString();
                                                vm.switch_index = ch + 1;
                                                try
                                                {
                                                    await vm.Port_Switch_ReOpen();

                                                    vm.Str_Command = "SW0 " + vm.switch_index.ToString();
                                                    vm.port_Switch.Write(vm.Str_Command + "\r");
                                                    await Task.Delay(vm.Int_Write_Delay);

                                                    vm.port_Switch.DiscardInBuffer();       // RX
                                                    vm.port_Switch.DiscardOutBuffer();      // TX

                                                    vm.port_Switch.Close();
                                                }
                                                catch (Exception ex)
                                                {
                                                    vm.Str_cmd_read = "Set Switch Error";
                                                    vm.Save_Log(vm.Str_Status, vm.switch_index.ToString(), vm.Str_cmd_read);
                                                    MessageBox.Show(ex.StackTrace.ToString());
                                                    return false;
                                                }
                                            }
                                            #endregion

                                            if (vm.Chart_All_DataPoints[0].Any(o => o.X == wl))
                                            {
                                                vm.Chart_All_DataPoints[0].RemoveAll(p => p.X == wl);
                                            }

                                            if (vm.Is_FastScan_Mode && vm.list_ChannelModels.Count == 1)
                                            {
                                                if (wl == vm.wl_list.Last())
                                                {
                                                    List<List<double>> listDD = await cmd.Get_Power(ch, true);
                                                }
                                                else
                                                {
                                                    if (wl == vm.wl_list.First())
                                                    {
                                                        if (vm.port_PD.IsOpen)
                                                        {
                                                            vm.port_PD.DiscardInBuffer();
                                                            vm.port_PD.DiscardOutBuffer();
                                                        }
                                                    }

                                                    await cmd.Get_Power(ch, false);
                                                }
                                            }
                                            else
                                            {
                                                await cmd.Get_Power(ch, true);

                                                //Only distributedSystem will auto update chart
                                                if (!vm.IsDistributedSystem)
                                                {
                                                    DataPoint dp = new DataPoint(vm.wl_list[vm.wl_list_index], vm.Double_Powers[ch]);
                                                    vm.ChartNowModel.list_dataPoints[ch].Add(dp);
                                                    vm.Save_All_PD_Value[ch].Add(dp);
                                                    vm.wl_list_index++;
                                                }

                                                if (vm.ch_count == 1)
                                                {
                                                    //Left side
                                                    if (j == 0 && vm.Double_Powers[ch] > target_IL)
                                                        isbreak = true;
                                                    //Right side
                                                    else if (j == 1 && vm.Double_Powers[ch] < target_IL)
                                                        isbreak = true;
                                                }
                                            }
                                        }

                                        //更新圖表
                                        #region Set Chart data points   
                                        vm.Chart_All_DataPoints = new List<List<DataPoint>>(vm.Save_All_PD_Value);
                                        FinalDataPoints = new List<DataPoint>(vm.Chart_All_DataPoints[0]);  //A lineseries   
                                        vm.Chart_DataPoints = new List<DataPoint>(FinalDataPoints.OrderBy(o => o.X).ToList());  //A lineseries  

                                        scan_timespan = watch.ElapsedMilliseconds / (decimal)1000;
                                        vm.ChartNowModel.TimeSpan = (double)Math.Round(scan_timespan, 1);
                                        vm.msgModel.msg_3 = Math.Round(scan_timespan, 1).ToString() + " s";  //Update Main UI timespan
                                        #endregion
                                    }
                                    #endregion
                                }
                            }
                            string str_pre_bw = $"{vm.opModel_1.WL_1_CWL},{vm.opModel_1.WL_1_BW_1},{vm.opModel_1.WL_1_BW_2},{vm.opModel_1.WL_1_BW_3}";
                            vm.Chart_DataPoints = FinalDataPoints.OrderBy(o => o.X).ToList();
                            anly.BandWidth_Calculation(vm.opModel_1.WL_No);


                            string str_aft_bw = $"{vm.opModel_1.WL_1_CWL},{vm.opModel_1.WL_1_BW_1},{vm.opModel_1.WL_1_BW_2},{vm.opModel_1.WL_1_BW_3}";
                            Console.WriteLine(str_pre_bw + "\r\r" + str_aft_bw);
                        }

                        #endregion

                        foreach (var item in vm.props_opModel)
                        {
                            if (item.Name == $"WL_{vm.opModel_1.WL_No}_CWL")
                                cwl = (double)item.GetValue(vm.opModel_1, null);
                        }

                        //Get IL at CWL postion
                        if (cwl != 0)
                        {
                            vm.Str_Status = "Getting IL";
                            bool FS_Setting = vm.Is_FastScan_Mode;

                            await cmd.Set_WL(cwl, true);
                            await Task.Delay(1000);

                            vm.Is_FastScan_Mode = false;
                            await cmd.Get_Power(0, true);
                            vm.Is_FastScan_Mode = FS_Setting;
                            await Task.Delay(200);

                            //Set IL value to TF2 gridview
                            foreach (var item in vm.props_opModel)
                            {
                                if (item.Name == $"WL_{vm.opModel_1.WL_No}_IL")
                                    item.SetValue(vm.opModel_1, vm.list_GaugeModels[0].GaugeValue);
                            }
                        }

                        //PDL Scan
                        await _Page_TF2_Main.Cal_PDL_Show();
                    }
                }
                else
                    for (int ch = 0; ch < vm.ch_count; ch++)  //Calculate the position with maximum IL and show data in UI
                    {
                        if (vm.ChartNowModel.list_dataPoints[ch].Count > 0)
                        {
                            DataPoint dpMax = vm.ChartNowModel.list_dataPoints[ch].Where(point => point.Y == vm.ChartNowModel.list_dataPoints[ch].Max(x => x.Y)).First();
                            double maxIL_WL = dpMax.X;

                            vm.Double_Laser_Wavelength = maxIL_WL;

                            if (vm.PD_or_PM)
                                await cmd.Set_WL(maxIL_WL, false);

                            vm.list_GaugeModels[ch].GaugeValue = vm.ChartNowModel.list_dataPoints[ch].Max(x => x.Y).ToString();

                            vm.list_GaugeModels[ch].GaugeBearSay_1 = Math.Round(maxIL_WL, 2).ToString();
                            vm.list_GaugeModels[ch].GaugeBearSay_2 = vm.list_GaugeModels[ch].GaugeValue;
                        }
                    }

                scan_timespan = watch.ElapsedMilliseconds / (decimal)1000;
                vm.ChartNowModel.TimeSpan = (double)Math.Round(scan_timespan, 1);
                await cmd.Save_Chart();

                vm.msgModel.msg_3 = Math.Round(scan_timespan, 1).ToString() + " s";  //Update Main UI timespan

                vm.Show_Bear_Window($"WL Scan 完成  ({Math.Round(scan_timespan, 1)} s)", false, "String", false);

                int a = vm.list_ChartModels.Count;

                vm.Str_Status = "WL Scan Stop";
                vm.Save_Log("WL Scan", "Stop", false);

                vm.IsGoOn = false;

                //Close TLS filter port for other control action
                cmd.Close_TLS_Filter();

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.StackTrace.ToString());
                return false;
            }
        }

        private async Task<bool> WL_Scan_Iteration()
        {
            #region Setting
            vm.Save_All_PD_Value = Analysis.ListDefine<DataPoint>(vm.Save_All_PD_Value, vm.ch_count, new List<DataPoint>());
            for (int i = 0; i < vm.ch_count; i++)
            {
                vm.Plot_Series[i].Points.Clear();
                vm.ChartNowModel.list_dataPoints[i].Clear();
            }

            vm.Chart_title = "Power X Wavelength";
            vm.Chart_x_title = "Wavelength (nm)";
            vm.Chart_y_title = vm.dB_or_dBm ? "IL (dB)" : "IL (dBm)";

            vm.Update_ALL_PlotView_Title(vm.Chart_title, vm.Chart_x_title, vm.Chart_y_title);

            K_WL.IsEnabled = false;

            var watch = System.Diagnostics.Stopwatch.StartNew();

            vm.Save_Log("WL Scan", vm.ch_count.ToString(), vm.float_WL_Scan_Start.ToString(), vm.float_WL_Scan_End.ToString());

            vm.Str_Status = "WL Scan";

            double start_temp = vm.float_WL_Scan_Start; double end_temp = vm.float_WL_Scan_End;
            double gap_temp = vm.float_WL_Scan_Gap;
            double IL_threshold = -35;

            #endregion

            int lastCh = 0;

            for (int ch = 0; ch < vm.ch_count; ch++)
            {
                if (vm.BoolAllGauge || vm.list_GaugeModels[ch].boolGauge)
                {
                    vm.list_GaugeModels[lastCh].GaugeValue = "";

                    #region Product parameter setting
                    if (vm.SN_Judge)
                    {
                        vm.product_type = string.IsNullOrEmpty(vm.SNMembers[ch].ProductType) ? vm.product_type : vm.SNMembers[ch].ProductType;
                        vm.selected_band = string.IsNullOrEmpty(vm.SNMembers[ch].LaserBand) ? vm.selected_band : vm.SNMembers[ch].LaserBand;
                        setting.Product_Setting();
                    }
                    #endregion

                    gap_temp = vm.float_WL_Scan_Gap;
                    start_temp = vm.float_WL_Scan_Start;
                    end_temp = vm.float_WL_Scan_End;

                    vm.switch_index = ch + 1;

                    #region switch re-open && Switch write Cmd
                    if (vm.Is_switch_mode)
                    {
                        try
                        {
                            await vm.Port_Switch_ReOpen();

                            vm.Str_Command = "SW0 " + vm.switch_index.ToString();
                            vm.port_Switch.Write(vm.Str_Command + "\r");
                            await Task.Delay(vm.Int_Write_Delay);

                            vm.port_Switch.DiscardInBuffer();       // RX
                            vm.port_Switch.DiscardOutBuffer();      // TX

                            vm.port_Switch.Close();
                        }
                        catch (Exception ex)
                        {
                            vm.Str_cmd_read = "Set Switch Error";
                            vm.Save_Log(vm.Str_Status, vm.switch_index.ToString(), vm.Str_cmd_read);
                            MessageBox.Show(ex.StackTrace.ToString());
                            return false;
                        }
                    }
                    #endregion

                    IL_threshold = -35;

                    //Iteration Loop ------------------------------------------------------------------------
                    while (gap_temp >= 0.01)
                    {
                        #region Build scan wl list
                        List<double> wl_list = new List<double>();

                        if (end_temp >= start_temp)
                            for (double wl = start_temp; wl <= end_temp; wl = wl + gap_temp) wl_list.Add(wl);
                        else
                            for (double wl = start_temp; wl >= end_temp; wl = wl - gap_temp) wl_list.Add(wl);
                        #endregion

                        bool isToEnd = false;
                        for (int i = 0; i < wl_list.Count; i++)
                        {
                            if (vm.isStop) return false;

                            double wl = wl_list[i];

                            double WL = Math.Round(wl, 2);

                            try
                            {
                                await cmd.Set_WL(WL, false);

                                vm.Double_Laser_Wavelength = wl;
                            }
                            catch { }

                            if (vm.isStop) return false;

                            vm.Str_cmd_read = "Ch " + (ch + 1).ToString() + " : " + WL.ToString();

                            double IL_Last;
                            if (vm.Save_All_PD_Value[ch].Count > 0)
                                IL_Last = vm.Save_All_PD_Value[ch].Last().Y;
                            else IL_Last = 99;

                            await cmd.Get_Power(vm.switch_index - 1, true);

                            vm.ChartNowModel.list_dataPoints[vm.switch_index - 1].Add(new DataPoint(wl, double.Parse(vm.list_GaugeModels[vm.switch_index - 1].GaugeValue)));

                            //更新圖表
                            #region Set Chart data points   
                            cmd.Update_Chart(WL, vm.Double_Powers[ch], ch);
                            #endregion

                            //anly.BandWidth_Calculation();

                            //Expand wl range
                            if (i == wl_list.Count - 1)
                                if (vm.Save_All_PD_Value[ch].Count > 0)
                                    if (vm.Save_All_PD_Value[ch].Last().Y >= vm.Save_All_PD_Value[ch][vm.Save_All_PD_Value[ch].Count - 2].Y)
                                    {
                                        wl_list.Add(WL + gap_temp);
                                    }

                            #region Iteration Contiion Judge
                            double power = vm.Double_Powers[ch];
                            if (vm.Save_All_PD_Value[ch].Count > 0)
                            {
                                if (power > IL_threshold)
                                {
                                    if (power < IL_Last)
                                    {
                                        //isToEnd = false;
                                        if (gap_temp <= 0.01)
                                        {
                                            vm.Save_Log("WL Scan", (ch + 1).ToString(), "Scan Final Gap", gap_temp.ToString() + " nm");
                                            isToEnd = false;
                                            double maxIL_WL = vm.Save_All_PD_Value[ch].Where(point => point.Y == vm.Save_All_PD_Value[ch].Max(x => x.Y)).ToList().First().X;
                                            vm.tls.SetWL(maxIL_WL);  //切換TLS WL                                                                      
                                            vm.pm.SetWL(maxIL_WL);   //切換PowerMeter WL 

                                            vm.list_GaugeModels[ch].GaugeValue = vm.Save_All_PD_Value[ch].Max(x => x.Y).ToString();
                                        }
                                        break;  //Break this run, and reverse
                                    }
                                }
                            }
                            #endregion
                        }

                        #region Iteration Scan condition change         

                        if (!isToEnd)
                        {
                            //if (gap_temp <= 0.01) break;

                            vm.Save_Log("WL Scan", (ch + 1).ToString(), "Scan Gap", gap_temp.ToString() + " nm");

                            if (gap_temp <= 0.01) break;

                            List<DataPoint> listPoints = vm.Save_All_PD_Value[ch].OrderBy(x => x.Y).ToList();

                            if (listPoints.Last().Y < -40) break;
                            if (listPoints.Count > 1)
                            {
                                //原先是以全數列中最大的兩個IL做為起點、終點
                                start_temp = listPoints.Last().X;
                                listPoints.RemoveAt(listPoints.Count - 1);
                                end_temp = listPoints.Last().X;

                                //改為以數列內最後三點，其中最高的兩點，作為起點、終點
                                if (listPoints.Count > 2)
                                {
                                    if (vm.Save_All_PD_Value[ch][vm.Save_All_PD_Value[ch].Count - 2].Y > vm.Save_All_PD_Value[ch].Last().Y)
                                    {
                                        start_temp = vm.Save_All_PD_Value[ch][vm.Save_All_PD_Value[ch].Count - 2].X;
                                        end_temp = vm.Save_All_PD_Value[ch][vm.Save_All_PD_Value[ch].Count - 3].X;
                                    }
                                    else
                                    {
                                        start_temp = vm.Save_All_PD_Value[ch].Last().X;
                                        end_temp = vm.Save_All_PD_Value[ch][vm.Save_All_PD_Value[ch].Count - 2].X;
                                    }
                                }
                                else
                                {
                                    start_temp = vm.Save_All_PD_Value[ch].Last().X;
                                    end_temp = vm.Save_All_PD_Value[ch][vm.Save_All_PD_Value[ch].Count - 1].X;
                                }


                                gap_temp = gap_temp / 5 >= 0.01 ? Math.Round(gap_temp / 5, 2) : 0.01;
                                if (end_temp >= start_temp)
                                {
                                    start_temp += 0.01;
                                    end_temp -= 0.01;
                                }
                                else
                                {
                                    start_temp -= 0.01;
                                    end_temp += 0.01;
                                }

                                vm.Save_Log("WL Scan", (ch + 1).ToString(), "start_temp", start_temp.ToString() + " nm");
                                vm.Save_Log("WL Scan", (ch + 1).ToString(), "end_temp", end_temp.ToString() + " nm");
                            }
                        }
                        else
                        {
                            if (gap_temp <= 0.01)
                            {
                                double maxIL_WL = vm.Save_All_PD_Value[ch].Where(point => point.Y == vm.Save_All_PD_Value[ch].Max(x => x.Y)).ToList().First().X;
                                await cmd.Set_WL(maxIL_WL, false);
                                vm.list_GaugeModels[ch].GaugeValue = vm.Save_All_PD_Value[ch].Max(x => x.Y).ToString();
                            }
                            break;
                        }

                        if (IL_threshold < -12)
                            IL_threshold /= 1.7;
                        #endregion
                    }

                    List<DataPoint> listPoints_final = vm.Save_All_PD_Value[ch].OrderBy(x => x.Y).ToList();

                    double finalIL = Math.Round(vm.Save_All_PD_Value[ch].Max(x => x.Y), 3);

                    if (finalIL > -30)
                    {
                        vm.list_GaugeModels[ch].GaugeBearSay_1 = Math.Round(listPoints_final.Last().X, 2).ToString();
                        vm.list_GaugeModels[ch].GaugeBearSay_2 = finalIL.ToString();
                    }
                    else vm.list_GaugeModels[ch].GaugeBearSay_1 = "NG";

                    vm.list_GaugeModels[ch].DataPoints = new List<DataPoint>(vm.Save_All_PD_Value[ch]);

                    lastCh = ch;
                }
            }

            await cmd.Save_Chart();

            var elapsedMs = watch.ElapsedMilliseconds;

            vm.Show_Bear_Window("WL Scan 完成" + "  (" + (elapsedMs / 1000).ToString() + " s)", false, "String", false);

            vm.Str_Status = "WL Scan Stop";
            vm.Save_Log("WL Scan", "Stop", false);

            //Close TLS filter port for other control action
            cmd.Close_TLS_Filter();

            K_WL.IsEnabled = true;
            vm.IsGoOn = false;

            return true;
        }

        private async Task<bool> WL_Scan_BR()
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var elapsedMs = watch.ElapsedMilliseconds;
            decimal scan_timespan = elapsedMs / (decimal)1000;

            cmd.Reset_Chart(vm.list_BR_DAC_WL.ToList());

            vm.Chart_title = "Power X Wavelength";
            vm.Chart_x_title = "Wavelength (nm)";
            vm.Chart_y_title = vm.dB_or_dBm ? "IL (dB)" : "IL (dBm)";

            vm.Update_ALL_PlotView_Title(vm.Chart_title, vm.Chart_x_title, vm.Chart_y_title);

            //vm.PlotViewModel.Annotations.Clear();

            for (int i = 0; i < vm.Plot_Series.Count; i++)
            {
                vm.Plot_Series[i].Points.Clear();

                if (vm.PlotViewModel.Annotations.Count > i)
                {
                    LineAnnotation pat = vm.PlotViewModel.Annotations[i] as LineAnnotation;
                    pat.StrokeThickness = 0;
                    pat.Text = "";
                    pat.ClipByXAxis = true;
                }
                else
                {
                    vm.PlotViewModel.Annotations.Add(new LineAnnotation()
                    {
                        Type = LineAnnotationType.Vertical,
                        Color = OxyColors.Black,
                        ClipByYAxis = false,
                        X = 0,
                        StrokeThickness = 0
                    });

                }
            }

            for (int i = 0; i < vm.ChartNowModel.list_dataPoints.Count; i++)
            {
                vm.ChartNowModel.list_dataPoints[i].Clear();
            }

            for (int wl_idx = 0; wl_idx < vm.ChartNowModel.list_BR_Model.Count; wl_idx++)
            {
                vm.ChartNowModel.list_BR_Model[wl_idx].BR_WL = "";
                vm.ChartNowModel.list_BR_Model[wl_idx].BR = "";
            }

            if (string.IsNullOrWhiteSpace(vm.Selected_Comport)) return false;
            await vm.Port_ReOpen(vm.Selected_Comport);

            for (int wl_idx = 0; wl_idx < vm.list_BR_DAC_WL.Count; wl_idx++)
            {
                if (vm.isStop) break;

                //Set Dac 
                try
                {
                    string wl = vm.list_BR_DAC_WL[wl_idx];

                    if (_Page_BR.dict_WL_to_Dac.Count != 0)
                        vm.Str_Command = $"D1 {_Page_BR.dict_WL_to_Dac[wl]}";
                    else
                        vm.Str_Command = $"WL {wl}";

                    await cmd.Write_Cmd(vm.Str_Command, true);

                    await Task.Delay(vm.Int_Read_Delay);

                    await D0_show();
                }
                catch { }

                vm.Str_cmd_read = $"WL : {vm.list_BR_DAC_WL[wl_idx]}";

                List<DataPoint> list_WL_IL = new List<DataPoint>();

                //Call OSA scan and show Data function (one lineseries)
                //List<DataPoint> list_WL_IL = await Task.Run(() => cmd.OSA_Scan());

                if (vm.is_BR_OSA)
                {
                    //Call OSA scan and show Data function (one lineseries)
                    list_WL_IL = await Task.Run(() => cmd.OSA_Scan());

                    #region Show Datapoints on UI
                    foreach (DataPoint dp in list_WL_IL)
                    {
                        double ref_value = 0;

                        if (vm.dB_or_dBm && vm.is_BR_OSA)
                        {
                            RefModel rm = vm.Ref_memberDatas.Where(r => r.Wavelength == dp.X).FirstOrDefault();

                            if (rm != null)
                                ref_value = rm.Ch_1;
                        }

                        vm.Plot_Series[wl_idx].Points.Add(new DataPoint(dp.X, dp.Y - vm.BR_Diff - ref_value));
                    }
                    #endregion

                }
                //TLS mode
                else
                {
                    vm.Str_Status = "WL Scan";

                    #region Build scan wl list
                    vm.wl_list.Clear();

                    if (vm.WL_Special_List.Count > 0)
                        vm.wl_list = vm.WL_Special_List.Select(w => Convert.ToDouble(w)).ToList();
                    else
                    {
                        if (vm.float_WL_Scan_End >= vm.float_WL_Scan_Start)
                        {
                            for (double wl = vm.float_WL_Scan_Start; wl <= vm.float_WL_Scan_End; wl = wl + vm.float_WL_Scan_Gap)
                            {
                                vm.wl_list.Add(Math.Round(wl, 2));
                            }
                        }
                        else
                        {
                            for (double wl = vm.float_WL_Scan_Start; wl >= vm.float_WL_Scan_End; wl = wl - vm.float_WL_Scan_Gap)
                            {
                                vm.wl_list.Add(Math.Round(wl, 2));
                            }
                        }
                    }

                    vm.Save_Log(new LogMember()
                    {
                        Status = "BR WL Scan",
                        Message = $"WL : {vm.list_BR_DAC_WL[wl_idx]}",
                        Result = $"Scan points : {vm.wl_list.Count}"
                    });
                    #endregion

                    foreach (double wl in vm.wl_list)
                    {
                        if (vm.isStop) break;

                        await cmd.Set_WL(wl, false);

                        if (wl == vm.wl_list.First())
                            await Task.Delay(800);

                        if (!(!vm.IsDistributedSystem && !vm.PD_or_PM && vm.Is_FastScan_Mode))
                            await Task.Delay(vm.Int_Set_WL_Delay);

                        vm.Double_Laser_Wavelength = wl;

                        for (int ch = 0; ch < vm.ch_count; ch++)
                        {
                            if (vm.isStop) break;

                            if (!vm.BoolAllGauge)
                                if (!vm.list_GaugeModels[ch].boolGauge) continue;

                            await cmd.Get_Power(ch, true);

                            list_WL_IL.Add(new DataPoint(vm.Double_Laser_Wavelength, vm.Double_Powers[ch]));
                        }

                        //更新圖表
                        #region Set Chart data points   

                        double ref_value = 0;
                        DataPoint dp = list_WL_IL.Last();
                        vm.Plot_Series[wl_idx].Points.Add(new DataPoint(dp.X, dp.Y - vm.BR_Diff - ref_value));

                        vm.Update_ALL_PlotView();

                        #endregion
                    }
                }


                //Calculate BR and it's wl peak position
                double IL_Max = vm.Plot_Series[wl_idx].Points.Where(p => p.Y == vm.Plot_Series[wl_idx].Points.Max(s => s.Y)).FirstOrDefault().Y;
                double IL_Min = vm.Plot_Series[wl_idx].Points.Where(p => p.Y == vm.Plot_Series[wl_idx].Points.Min(s => s.Y)).FirstOrDefault().Y;

                vm.ChartNowModel.list_BR_Model[wl_idx].BR_WL = vm.Plot_Series[wl_idx].Points.Where(p => p.Y == vm.Plot_Series[wl_idx].Points.Max(s => s.Y)).FirstOrDefault().X.ToString();
                vm.ChartNowModel.list_BR_Model[wl_idx].BR = IL_Max.ToString("f1");

                vm.PlotViewModel.Axes[1].Maximum = IL_Max < 0 ? 0 : IL_Max;
                vm.PlotViewModel.Axes[1].Minimum = IL_Min - 10;

                LineAnnotation pat = vm.PlotViewModel.Annotations[wl_idx] as LineAnnotation;
                pat.StrokeThickness = 2;
                pat.X = vm.Plot_Series[wl_idx].Points.Where(p => p.Y == vm.Plot_Series[wl_idx].Points.Max(s => s.Y)).FirstOrDefault().X;
                pat.MinimumY = IL_Min - 5;
                pat.Text = $"{IL_Max}\r{pat.X.ToString("f2")}";
                pat.TextOrientation = AnnotationTextOrientation.Horizontal;

                if (wl_idx < 10)
                    pat.TextLinePosition = 1 - (0.1 * wl_idx);  //Vertical postion of annotation title text
                else
                    pat.TextLinePosition = 1 - (0.1 * (wl_idx - 10));  //Vertical postion of annotation title text

                vm.PlotViewModel.Axes[1].Maximum = vm.PlotViewModel.Axes[1].Maximum < 0 ? 0 : vm.PlotViewModel.Axes[1].Maximum;
                vm.PlotViewModel.Axes[1].Minimum = vm.PlotViewModel.Axes[1].Minimum - 5;
                vm.Update_ALL_PlotView();

                vm.Str_cmd_read = vm.Str_cmd_read + $", {list_WL_IL.Count} points";

                scan_timespan = watch.ElapsedMilliseconds / (decimal)1000;
                vm.msgModel.msg_3 = $"{Math.Round(scan_timespan, 1)} s";
            }

            vm.ChartNowModel.TimeSpan = (double)Math.Round(scan_timespan, 1);

            vm.ChartNowModel.list_Annotation = new List<Annotation>(vm.PlotViewModel.Annotations);

            await cmd.Save_Chart();

            if (!vm.isStop)
                vm.Show_Bear_Window($"WL Scan 完成  ({Math.Round(scan_timespan, 1)} s)", false, "String", false);

            vm.IsGoOn = false;

            #region 儲存資料 - 初始設定判斷
            if (string.IsNullOrEmpty(vm.UserID))
            {
                vm.Show_Bear_Window("工號空白", false, "String", false);
                return true;
            }
            else
            {
                if (vm.UserID.Length != 5 || (vm.UserID.First() != 'P' && vm.UserID.First() != 'E'))
                {
                    vm.Show_Bear_Window("工號格式應為PXXX", false, "String", false);
                    return true;
                }
            }

            if (string.IsNullOrEmpty(vm.list_GaugeModels.First().GaugeSN))
            {
                vm.Show_Bear_Window("序號空白", false, "String", false);
                return true;
            }

            if (!vm.CheckDirectoryExist(vm.txt_BR_Save_Path))
            {
                vm.Show_Bear_Window("儲存路徑不存在", false, "String", false);
                return true;
            }
            #endregion

            cmd.Save_BR_Data();

            return true;
        }

        private void btn_close_Click(object sender, RoutedEventArgs e)
        {
            if (vm.port_PD != null)
                vm.port_PD.Close();
            System.Environment.Exit(System.Environment.ExitCode);
        }

        private void btn_max_Click(object sender, RoutedEventArgs e)
        {
            vm.mainWin_size = new double[] { ActualWidth, ActualHeight };
            vm.mainWin_point = new System.Windows.Point(Left, Top);

            if (this.WindowState == WindowState.Normal)   //全螢幕
            {
                this.MaxHeight = double.PositiveInfinity;
                this.MaxWidth = double.PositiveInfinity;

                this.ResizeMode = ResizeMode.NoResize;
                this.WindowState = WindowState.Maximized;

                grid_process_schedule.Visibility = Visibility.Collapsed;
                btn_desktop.Visibility = Visibility.Visible;
            }
            else   //視窗大小不含工作列
            {
                btn_desktop.Visibility = Visibility.Collapsed;
                grid_process_schedule.Visibility = Visibility.Visible;

                this.ResizeMode = ResizeMode.CanResizeWithGrip;
                this.WindowState = WindowState.Normal;

                //取得可獲得之工作視窗大小(不含工作列)         
                var a = SystemParameters.WorkArea;
                this.MaxHeight = a.Height;
                this.MaxWidth = a.Width;
                Height = MaxHeight;
                Width = MaxWidth;
                System.Windows.Point p = this.PointToScreen(new System.Windows.Point(0, 0));
                this.Left = Left - p.X;
                this.Top = Top - p.Y;
            }
        }

        private void btn_min_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private bool mRestoreForDragMove;
        private void border_title_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //判斷滑鼠點擊次數
            if (e.ClickCount == 2)
            {
                if ((this.ResizeMode != ResizeMode.CanResize) && (this.ResizeMode != ResizeMode.CanResizeWithGrip))
                    return;
                this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized; //雙擊最大化
            }
            else
            {
                mRestoreForDragMove = this.WindowState == WindowState.Normal;
            }
        }

        private void border_title_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (mRestoreForDragMove && this.WindowState == WindowState.Maximized)
            {

            }
            else if (mRestoreForDragMove && this.WindowState == WindowState.Normal)
            {
                try
                {
                    mRestoreForDragMove = false;
                    this.DragMove();
                }
                catch { }
            }
        }

        private void border_title_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            mRestoreForDragMove = false;
        }

        private void Image_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            List<List<string>> list_bearSay_string = new List<List<string>>();
            list_bearSay_string.Add(new List<string>() { "1530.33", "-1.561", "28.9" });
            list_bearSay_string.Add(new List<string>() { "1531.58", "-1.528", "29.3" });
            vm.List_bear_say = new List<List<string>>(list_bearSay_string);

            vm.bear_say_all++;
            vm.bear_say_now = vm.bear_say_all;

            list_bearSay_string = new List<List<string>>();
            list_bearSay_string.Add(new List<string>() { "1550.33", "-1.336", "27.9" });
            list_bearSay_string.Add(new List<string>() { "1551.58", "-1.358", "28.3" });
            vm.List_bear_say = new List<List<string>>(list_bearSay_string);

            vm.bear_say_all++;
            vm.bear_say_now = vm.bear_say_all;

            if (list_bearSay_string.First().Count >= 3)
            {
                vm.GaugeTxtSize_Column[1] = new GridLength(list_bearSay_string.First()[0].Count(), GridUnitType.Star);
                vm.GaugeTxtSize_Column[2] = new GridLength(list_bearSay_string.First()[1].Count(), GridUnitType.Star);
                vm.GaugeTxtSize_Column[3] = new GridLength(list_bearSay_string.First()[2].Count(), GridUnitType.Star);
                vm.GaugeTxtSize_Column = new List<GridLength>(vm.GaugeTxtSize_Column);
            }
        }

        private void Close_Bear_Window()
        {
            vm.Winbear.Close();
        }

        private void ToggleBtn_ControlMode_Loaded(object sender, RoutedEventArgs e)
        {
            if (!vm.PD_or_PM) return;

            #region PowerMeter Setting
            vm.pm = new HPPM();
            vm.pm.Addr = vm.tls_Addr;
            vm.pm.Slot = vm.PM_slot;
            vm.pm.BoardNumber = vm.tls_BoardNumber;
            if (vm.pm.Open() == false)
            {
                vm.Str_cmd_read = "The GPIB Setting Error.  Check  Address.";
                return;
            }
            vm.pm.init();
            vm.pm.setUnit(1);
            vm.pm.AutoRange(true);
            vm.pm.aveTime(vm.PM_AveTime);
            #endregion
        }

        //private void grid_Unit_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        //{
        //    cmd.Clean_Chart();

        //    ToggleButton obj = (ToggleButton)sender;
        //    if (obj.IsChecked == true)
        //    {
        //        vm.str_Unit = "dB";
        //        vm.isDeltaILModeOn = true;
        //        vm.double_Maxdelta = new List<double>() { 0, 0, 0, 0, 0, 0, 0, 0 };
        //        vm.savedPower_for_deltaMode = new List<double>();
        //        foreach (string s in vm.Str_PD)
        //        {
        //            vm.savedPower_for_deltaMode.Add(Convert.ToDouble(s));
        //        }
        //        vm.double_MinIL_for_DeltaMode = new List<double>() { 0, 0, 0, 0, 0, 0, 0, 0 };
        //        vm.double_MaxIL_for_DeltaMode = new List<double>() { 0, 0, 0, 0, 0, 0, 0, 0 };
        //    }
        //    else
        //    {
        //        vm.str_Unit = "dBm";
        //        vm.isDeltaILModeOn = false;
        //    }
        //}

        private void tbtn_switch_Click(object sender, RoutedEventArgs e)
        {
            ToggleButton obj = (ToggleButton)sender;

            if (obj.IsChecked == true)
            {
                System.Windows.Point p = obj.PointToScreen(new System.Windows.Point(0, 0));
                vm.WinSwitch = new Window_Switch_Box(vm, p, obj.ActualWidth);
                vm.WinSwitch.Show();
            }
            else
                vm.WinSwitch.Close();
        }



        private void btn_desktop_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        Window_PowerSupply_Setting window_PowerSupply_Setting;
        private void btn_port_setting_Click(object sender, RoutedEventArgs e)
        {
            vm.Save_cmd(new ComMember() { No = vm.Cmd_Count++.ToString(), Command = "CLSPORT" });
            if (window_PowerSupply_Setting != null)
            {
                if (window_PowerSupply_Setting.IsActive)
                {
                    window_PowerSupply_Setting.Topmost = true;
                }
                else
                {
                    window_PowerSupply_Setting = new Window_PowerSupply_Setting(vm);
                    window_PowerSupply_Setting.Show();
                }
            }
            else
            {
                window_PowerSupply_Setting = new Window_PowerSupply_Setting(vm);
                window_PowerSupply_Setting.Show();
            }
        }

        private void btn_help_Click(object sender, RoutedEventArgs e)
        {

            //vm.station_type = ComViewModel.StationTypes.Testing;
            vm.Show_Bear_Window("有 問 題 請 撥 5 1 7", false, "String_Step", false);
        }

        private void Txt_ID_GotFocus(object sender, RoutedEventArgs e)
        {
            label_ID.Opacity = 0;
        }

        private void Txt_ID_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox obj = (TextBox)sender;
            if (string.IsNullOrWhiteSpace(obj.Text))
                label_ID.Opacity = 1;
        }

        private async void K_VOA_Click(object sender, RoutedEventArgs e)
        {
            bool _isGoOn = vm.IsGoOn;
            vm.IsGoOn = false;
            vm.isStop = false;
            if (_isGoOn) await Task.Delay(vm.Int_Read_Delay * 2);

            if (vm.SN_Judge)
            {
                vm.SNMembers = new ObservableCollection<SN_Member>();

                for (int i = 0; i < vm.ch_count; i++)
                {
                    string SN = vm.list_GaugeModels[i].GaugeSN;
                    vm.SNMembers.Add(anly.Product_Info_Anly(SN));  //Analyze product information and return a member of SN_Information_Group              
                }
            }

            await K_V3();
        }

        private async void K_DAC_Click(object sender, RoutedEventArgs e)
        {
            if (vm.station_type == ComViewModel.StationTypes.Hermetic_Test) return;

            bool _isGoOn = vm.IsGoOn;
            vm.IsGoOn = false;
            vm.isStop = false;
            if (_isGoOn) await Task.Delay(vm.Int_Read_Delay * 2);

            await K_DAC();
        }

        private void Btn_Save_Click(object sender, RoutedEventArgs e)
        {
            if (vm.station_type == ComViewModel.StationTypes.Hermetic_Test)
                vm.Show_Bear_Window("Before or After ?", false, "String", true);
            else if (vm.station_type == ComViewModel.StationTypes.TF2)
            {
                if (string.IsNullOrEmpty(vm.UserID))
                {
                    vm.Show_Bear_Window("工號空白", false, "String", false);
                    return;
                }
                else
                {
                    if (vm.UserID.Length != 5 || (vm.UserID.First() != 'P' && vm.UserID.First() != 'E'))
                    {
                        vm.Show_Bear_Window("工號格式應為PXXX", false, "String", false);
                        return;
                    }
                }

                List<string> list_titles = new List<string>(){ "WL","CWL", "IL", "PDL", "Mic",
                    $"{vm.opModel_1.BW_Setting_1}dB",
                    $"{vm.opModel_1.BW_Setting_2}dB",
                    $"{vm.opModel_1.BW_Setting_3}dB"};
                string filePath = CSVFunctions.Creat_New_CSV(Path.Combine(vm.txt_save_TF2_wl_data_path), $"{vm.opModel_1.SN}_{vm.TF2_station_type}", list_titles, false, true);

                if (!string.IsNullOrEmpty(filePath))
                {
                    string[] WLSetting = vm.opModel_1.WL_Setting_1.Split(' ');
                    CSVFunctions.Write_a_row_in_CSV(filePath, new List<string>()
                    {
                        $"{WLSetting[0]}_nm",
                        vm.opModel_1.WL_1_CWL.ToString(),
                        vm.opModel_1.WL_1_IL.ToString(),
                        vm.opModel_1.WL_1_PDL.ToString(),
                        vm.opModel_1.WL_1_Mic.ToString(),
                        vm.opModel_1.WL_1_BW_1.ToString(),
                        vm.opModel_1.WL_1_BW_2.ToString(),
                        vm.opModel_1.WL_1_BW_3.ToString()
                    });

                    WLSetting = vm.opModel_1.WL_Setting_2.Split(' ');
                    CSVFunctions.Write_a_row_in_CSV(filePath, new List<string>()
                    {
                        $"{WLSetting[0]}_nm",
                        vm.opModel_1.WL_2_CWL.ToString(),
                        vm.opModel_1.WL_2_IL.ToString(),
                        vm.opModel_1.WL_2_PDL.ToString(),
                        vm.opModel_1.WL_2_Mic.ToString(),
                        vm.opModel_1.WL_2_BW_1.ToString(),
                        vm.opModel_1.WL_2_BW_2.ToString(),
                        vm.opModel_1.WL_2_BW_3.ToString()
                    });

                    WLSetting = vm.opModel_1.WL_Setting_3.Split(' ');
                    CSVFunctions.Write_a_row_in_CSV(filePath, new List<string>()
                    {
                        $"{WLSetting[0]}_nm",
                        vm.opModel_1.WL_3_CWL.ToString(),
                        vm.opModel_1.WL_3_IL.ToString(),
                        vm.opModel_1.WL_3_PDL.ToString(),
                        vm.opModel_1.WL_3_Mic.ToString(),
                        vm.opModel_1.WL_3_BW_1.ToString(),
                        vm.opModel_1.WL_3_BW_2.ToString(),
                        vm.opModel_1.WL_3_BW_3.ToString()
                    });

                    CSVFunctions.Write_a_row_in_CSV(filePath, new List<string>()
                    {
                        "Mic. Lower", vm.opModel_1.Mic_Lower_Limit.ToString()
                    });

                    CSVFunctions.Write_a_row_in_CSV(filePath, new List<string>()
                    {
                        "Mic. Upper", vm.opModel_1.Mic_Upper_Limit.ToString()
                    });

                    CSVFunctions.Write_a_row_in_CSV(filePath, new List<string>()
                    {
                        "Tech ID", vm.UserID
                    });

                    CSVFunctions.Write_a_row_in_CSV(filePath, new List<string>()
                    {
                        "Station", vm.TF2_station_type
                    });

                    CSVFunctions.Write_a_row_in_CSV(filePath, new List<string>()
                    {
                        "Temp.", vm.opModel_1.HighPower_Temp.ToString()
                    });

                    string dateTime = DateTime.Now.ToString("yyyy'/'MM'/'dd' 'HH':'mm':'ss");
                    CSVFunctions.Write_a_row_in_CSV(filePath, new List<string>()
                    {
                        "DateTime", dateTime
                    });

                    vm.Str_Status = "Save Data to CSV";
                    vm.Str_cmd_read = vm.opModel_1.SN;
                }
            }
            else if (vm.station_type == ComViewModel.StationTypes.BR)
            {
                #region 初始設定判斷
                if (string.IsNullOrEmpty(vm.UserID))
                {
                    vm.Show_Bear_Window("工號空白", false, "String", false);
                    return;
                }
                else
                {
                    if (vm.UserID.Length != 5 || (vm.UserID.First() != 'P' && vm.UserID.First() != 'E'))
                    {
                        vm.Show_Bear_Window("工號格式應為PXXX", false, "String", false);
                        return;
                    }
                }

                if (string.IsNullOrEmpty(vm.list_GaugeModels.First().GaugeSN))
                {
                    vm.Show_Bear_Window("序號空白", false, "String", false);
                    return;
                }

                if (!vm.CheckDirectoryExist(vm.txt_BR_Save_Path))
                {
                    vm.Show_Bear_Window("儲存路徑不存在", false, "String", false);
                    return;
                }
                #endregion

                cmd.Save_BR_Data();
            }
            else
            {
                var dialog = new Microsoft.Win32.SaveFileDialog();
                dialog.Title = "Save position";
                dialog.InitialDirectory = CurrentDirectory;
                dialog.FileName = $"Data_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}";
                dialog.Filter = "TXT (*.txt)|*.txt|All files (*.*)|*.*";

                bool? result = dialog.ShowDialog();

                if (result == true)
                {
                    string filePath = dialog.FileName;
                    if (!string.IsNullOrWhiteSpace(filePath))
                    {
                        //Dictionary<double, string> dic_keyPairs = new Dictionary<double, string>();
                        Dictionary<string, string> dic_keyPairs = new Dictionary<string, string>();
                        for (int i = 0; i < vm.list_GaugeModels.Count; i++)
                        {
                            if (vm.list_GaugeModels[i].boolGauge || vm.BoolAllGauge)
                            {
                                dic_keyPairs.Clear();

                                for (int j = 0; j < vm.Save_All_PD_Value[i].Count; j++)
                                {
                                    dic_keyPairs.Add(vm.Save_All_PD_Value[i][j].X.ToString(), vm.Save_All_PD_Value[i][j].Y.ToString());
                                    //dic_keyPairs.Add(vm.Double_Laser_Wavelength, $"{vm.list_GaugeModels[i].GaugeD0_1},{vm.list_GaugeModels[i].GaugeD0_2},{vm.list_GaugeModels[i].GaugeD0_3}");

                                }

                                cmd.Save_ChartNow_Data(filePath, dic_keyPairs);
                            }
                        }

                        vm.Show_Bear_Window("Data Saved", false, "String", false);
                    }
                }
            }
        }


        private void Btn_cmd_list_Click(object sender, RoutedEventArgs e)
        {
            Button obj = (Button)sender;

            if (obj.ContextMenu.IsOpen == false) obj.ContextMenu.IsOpen = true;
            else obj.ContextMenu.IsOpen = false;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem obj = (MenuItem)sender;
            vm.btn_cmd_txt = obj.Header.ToString();
        }

        private void txtBox_comment_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox obj = (TextBox)sender;

            obj.Focus();

            if (vm.PD_or_PM == false)
                obj.BorderBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF10E2C4"));
            else
                obj.BorderBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#0085CA"));
        }

        private void Combox_Switch_Laser_Band_DropDownClosed(object sender, EventArgs e)
        {
            vm.Save_Product_Info[1] = vm.selected_band;

            switch (vm.selected_band)
            {
                case "C+L Band":
                    vm.float_TLS_WL_Range = new float[2] { 1520, 1620 };
                    if (!vm.isConnected)
                        if (vm.list_wl != null)
                            vm.Double_Laser_Wavelength = 1560;
                    break;

                case "U Band":
                    vm.float_TLS_WL_Range = new float[2] { 1625, 1675 };
                    if (!vm.isConnected)
                        if (vm.list_wl != null)
                            vm.Double_Laser_Wavelength = 1650;
                    break;

                case "L Band":
                    vm.float_TLS_WL_Range = new float[2] { 1560, 1625 };
                    if (!vm.isConnected)
                        if (vm.list_wl != null)
                            vm.Double_Laser_Wavelength = 1560;
                    break;

                case "C Band":
                    vm.float_TLS_WL_Range = new float[2] { 1520, 1573 };
                    if (!vm.isConnected)
                        if (vm.list_wl != null)
                            vm.Double_Laser_Wavelength = 1523;
                    break;

                case "S Band":
                    vm.float_TLS_WL_Range = new float[2] { 1460, 1520 };
                    if (!vm.isConnected)
                        if (vm.list_wl != null)
                            vm.Double_Laser_Wavelength = 1500;
                    break;

                case "E Band":
                    vm.float_TLS_WL_Range = new float[2] { 1360, 1460 };
                    if (!vm.isConnected)
                        if (vm.list_wl != null)
                            vm.Double_Laser_Wavelength = 1400;
                    break;

                case "O Band":
                    vm.float_TLS_WL_Range = new float[2] { 1260, 1360 };
                    if (!vm.isConnected)
                        if (vm.list_wl != null)
                            vm.Double_Laser_Wavelength = 1300;
                    break;

                case "Auto":
                    vm.float_TLS_WL_Range = new float[2] { 1260, 1675 };
                    break;
            }

            setting.Product_Setting();

            vm.Ini_Write("Connection", "Band", vm.selected_band);  //創建ini file並寫入基本設定
        }



        private void TextBlock_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            vm.timer_command.Start();
        }

        private void txt_ID_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            TextBox txtBox = (TextBox)sender;
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                if (string.Compare(txtBox.Text.ToUpper(), "Candy666", true) == 0 || string.Compare(txtBox.Text, "Candy555", true) == 0 || string.Compare(txtBox.Text, "QQ123", true) == 0 || string.Compare(txtBox.Text, "1234", true) == 0)
                {
                    Engineer_Mode();
                }
                else { vm.UserID = txtBox.Text; User_Mode(); }
            }
        }

        private void User_Mode()
        {
            vm.Str_cmd_read = "User";
            #region Calibration Items Setting
            vm.list_combox_Calibration_items.Clear();
            for (int i = 1; i < 9; i++)
            {
                string _item = vm.Ini_Read("Calibration", "Item_" + i.ToString());
                if (string.IsNullOrEmpty(_item))
                    continue;

                vm.list_combox_Calibration_items.Add(_item);
            }

            if (vm.list_combox_Calibration_items.Count == 0)
                vm.list_combox_Calibration_items = new List<string>() { "Calibration", "DAC -> 0", "VOA -> 0", "TF -> 0", "K VOA", "K TF", "K TF (Rough)", "K TF (Detail)", "Curve Fitting", "K WL" };
            #endregion
        }

        private void btn_start_cmd_list_Click(object sender, RoutedEventArgs e)
        {
            cmd.CommandList_Start(0, true);
        }

        private void Engineer_Mode()
        {
            vm.list_combox_Calibration_items = new List<string>() { "Calibration", "DAC -> 0", "VOA -> 0", "TF -> 0", "K VOA", "K TF", "K TF (Rough)", "K TF (Detail)", "Curve Fitting", "K WL" };
            vm.Str_cmd_read = "Engineer";
        }

        private void txt_version_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            OxyPlot.Wpf.LineSeries ls = new OxyPlot.Wpf.LineSeries();
            ls.MarkerType = MarkerType.Circle;
            ls.MarkerSize = 5;
            ls.MarkerFill = System.Windows.Media.Color.FromRgb(132, 220, 123);

            cmd.Clean_Chart();

            if (_Page_Chart.Plot_Chart.Series.Count > 16)
            {
                for (int i = 16; i < _Page_Chart.Plot_Chart.Series.Count; i++)
                {
                    _Page_Chart.Plot_Chart.Series.RemoveAt(i);
                }
            }

            _Page_Chart.No1.MarkerSize = 6;
            DataPoint dp = new DataPoint(0.5, 1);
            List<DataPoint> list_curfit_testPoints = new List<DataPoint>()
            {
                new DataPoint (1000 , -12),
                new DataPoint (2000 , -10),
                new DataPoint ( 3000, -3),
                new DataPoint ( 4000, -8),
                new DataPoint ( 5000, -11 ),
            };

            ls.ItemsSource = list_curfit_testPoints;

            _Page_Chart.Plot_Chart.Series.Add(ls);

            CurveFittingResultModel curveFittingResultModel = anly.CurFitting(list_curfit_testPoints);
            List<DataPoint> dataPoints = curveFittingResultModel.GetDrawLinePoints();

            #region Set Chart data points   
            foreach (DataPoint d in dataPoints)
            {
                vm.ChartNowModel.list_dataPoints[0].Add(d);
                cmd.Update_Chart(d.X, d.Y, 0);
            }
            #endregion

            MessageBox.Show("Best Dac: " + curveFittingResultModel.Best_X.ToString());
        }

        private void grid_Unit_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            cmd.Switch_PM_Unit();
        }
    }
}
