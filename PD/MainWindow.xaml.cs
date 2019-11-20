using System;
using System.IO;
using System.Data;
using System.Management;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Runtime.InteropServices;
using System.IO.Ports;
using System.Timers;
using System.Threading;

using GPIB_utility;
using PD.ViewModel;
using PD.AnalysisModel;
using PD.NavigationPages;
using PD.Functions;
using OxyPlot;
using DiCon.Instrument.HP;
using ExcelDataReader;

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
        
        int read_delay ; //Must >105 ms
        double power_PM;
        int v12_maxpower_index_PM;
        List<int> maxPowder_dac = new List<int>();
        Page_PD_Gauges _Page_PD_Gauges;
        Page_Chart _Page_Chart;
        Page_DataGrid _Page_DataGrid;
        Page_Laser _Page_Laser;
        Page_Setting _Page_Setting;
        int int_saved_combox_index;

        public MainWindow()
        {
            InitializeComponent();

            //設定datacontext
            vm = new ComViewModel();
            this.DataContext = vm;

            setting = new Setting(vm);
            cmd = new ControlCmd(vm);

            combox_product.Items.Clear();
            combox_product.Items.Add("CTF");
            combox_product.Items.Add("UFA");
            combox_product.Items.Add("UFA-T");
            combox_product.Items.Add("UFA(H)");
            combox_product.Items.Add("UTF");
            combox_product.Items.Add("UTF400");
            combox_product.Items.Add("UTF500");
            combox_product.Items.Add("MTF");

            //Read ini file and setting            
            string ini_path = vm.ini_exist();

            if (File.Exists(ini_path))
            {
                combox_comport.SelectedItem = vm.Ini_Read("Connection", "Comport");
                vm.station_type = vm.Ini_Read("Connection", "Station");
                vm.product_type = vm.Ini_Read("Productions", "Product");
                vm.selected_K_WL_Type = vm.Ini_Read("Productions", "K_WL_Type");

                if (vm.Ini_Read("Connection", "PD_or_PM") == "PM")
                {
                    vm.PD_or_PM = true;

                    run_PD.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF878787"));
                    run_PM.Foreground = new SolidColorBrush(Colors.White);
                    vm.Main_Color = (SolidColorBrush)(new BrushConverter().ConvertFrom("#0085CA"));
                    vm.Ini_Write("Connection", "PD_or_PM", "PM");
                }
                
                if (vm.Ini_Read("Productions", "Unit") == "dBm")
                {
                    run_dBm.Foreground = new SolidColorBrush(Colors.White);
                    run_dB.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF878787"));
                    vm.str_Unit = "dBm";
                    vm.dB_or_dBm = false;
                }
            }
            else
            {
                Directory.CreateDirectory(System.IO.Directory.GetParent(ini_path).ToString());  //建立資料夾      
                vm.Ini_Write("Connection", "Comport", "COM2");  //創建ini file並寫入基本設定
                vm.Ini_Write("Productions", "Product", "CTF");
            }           

            #region Calibration Items Setting
            vm.list_combox_Calibration_items.Clear();
            for (int i = 1; i < 10; i++)
            {
                string _item = vm.Ini_Read("Calibration", "Item_" + i.ToString());
                if (string.IsNullOrEmpty(_item))
                    continue;

                vm.list_combox_Calibration_items.Add(_item);
            }

            if (vm.list_combox_Calibration_items.Count == 0) 
                vm.list_combox_Calibration_items= new List<string>() { "Calibration", "VOA -> 0", "TF -> 0", "K VOA", "K TF", "K TF (Rough)", "K TF (Detail)", "Curve Fitting", "K WL" };
            #endregion

            #region Board Setting
            vm.txt_board_table_path = @"\\192.168.2.3\tff\Data\BoardCalibration\UFA\";
            //vm.txt_board_table_path = @"D:\PD\UFV board\"";

            vm.list_Board_Setting.Clear();
            for (int i = 0; i < 12; i++)
            {
                string Board_ID = "Board_ID_" + (i + 1).ToString();
                string Board_COM = "Board_COM_" + (i + 1).ToString();
                vm.list_Board_Setting.Add(new List<string>() { vm.Ini_Read("Board_ID", Board_ID), vm.Ini_Read("Board_Comport", Board_COM) });
            }
            vm.list_Board_Setting = new List<List<string>>(vm.list_Board_Setting);


            int k = 0;
            foreach (List<string> board_info in vm.list_Board_Setting)
            {
                vm.board_read.Add(new List<string>());

                if (string.IsNullOrEmpty(board_info[0]))
                {
                    k++; continue;
                }
                               
                string board_id = board_info[0];
                string path = string.Concat(vm.txt_board_table_path, board_id, "-boardtable.txt");

                if (!File.Exists(path))
                {
                    vm.Str_cmd_read = "UFV Board table is not exist";
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
            #endregion

            if (vm.station_type == "Hermetic Test")
                vm.Is_switch_mode = true;
            else
                vm.Is_switch_mode = false;

            anly = new Analysis(vm);

            read_delay = vm.Int_Read_Delay;  //Set read cmd delay time

            vm.timer1 = new System.Timers.Timer();
            vm.timer1.Interval = 2000;
            vm.timer1.Elapsed += Timer1_Elapsed;

            vm.timer2 = new System.Timers.Timer();
            vm.timer2.Interval = read_delay;
            vm.timer2.Elapsed += Timer2_Elapsed;

            vm.timer3 = new System.Timers.Timer();
            vm.timer3.Interval = read_delay;
            vm.timer3.Elapsed += Timer3_Elapsed;

            myPorts = SerialPort.GetPortNames(); //取得所有port的方法
            foreach (string s in myPorts) combox_comport.Items.Add(s);  //寫入所有取得的com

            //if (myPorts.Length > 0) combox_comport.SelectedIndex = 0;  //預設com

            if (combox_comport.SelectedItem != null)
                str_selected_com = combox_comport.SelectedItem.ToString();
            else
                vm.Str_cmd_read = "Selected Port is null";

            //初始化port
            vm.port_PD = new SerialPort(str_selected_com, 115200, Parity.None, 8, StopBits.One);

            vm.Save_PD_Value = new List<DataPoint>();
             vm.Save_All_PD_Value = Analysis.ListDefault<List<DataPoint>>(8);

            #region Page Navigation Setting
            _Page_PD_Gauges = new Page_PD_Gauges(vm);
            _Page_Chart = new Page_Chart(vm);
            _Page_DataGrid = new Page_DataGrid(vm);
            _Page_Laser = new Page_Laser(vm);
            _Page_Setting = new Page_Setting(vm);
            RBtn_Gauge_Page.IsChecked = true;
            //Frame_Navigation.Navigate(_Page_PD_Gauges);
            #endregion
            
            #region Production Setting
            if (combox_product.SelectedItem != null)
            {
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
            }
            #endregion
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

        private async void btn_GO_Click(object sender, RoutedEventArgs e)
        {
            if (vm.IsGoOn)
            {
                if (vm.isConnected == false && vm.PD_or_PM == true)  //檢查TLS是否連線，若無，則進行連線並取續TLS狀態
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
                }

                vm.Chart_x_title = "Time(s)"; //Set Chart x axis title
                vm.Str_Status = "Get Power";
                if (vm.PD_or_PM == false)
                    await vm.PD_GO();
                else
                    vm.PM_GO();                
            }
            else
            {
                vm.Str_Status = "Stop";
                if (vm.PD_or_PM == false)
                {
                    await vm.PD_Stop();
                    await cmd.Save_Chart();
                }                    
                else
                    await vm.PM_Stop();
            }
        }

        private void Timer1_Elapsed(object sender, ElapsedEventArgs e)
        {
            vm.dateTime.Clear();
            vm.dateTime.Add(DateTime.Now.ToShortTimeString());
            vm.dateTime.Add(DateTime.Now.ToShortDateString());
            vm.dateTime = new List<string>(vm.dateTime);
        }

        private void Timer2_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (vm.port_PD.IsOpen)
                {
                    if (vm.IsGoOn)
                    {
                        vm.port_PD.Write(vm.Str_comment + "\r");
                    }

                    int size = vm.port_PD.BytesToRead;
                    byte[] dataBuffer = new byte[size];
                    int length = vm.port_PD.Read(dataBuffer, 0, size);
                    vm = anly._read_analysis(vm.Str_comment, dataBuffer);

                    #region Set Chart data points   

                    if (vm.timer2_count > 26000)  //Default 36000
                        vm.Save_PD_Value.RemoveAt(0);  //Make sure points count less than 36000

                    if (vm.isDeltaILModeOn == true)
                    {
                        for (int i = 0; i < 8; i++)
                             vm.Save_All_PD_Value[i].Add(new DataPoint((double)Math.Round((decimal)vm.timer2_count * vm.Int_Read_Delay / 1000, 2), vm.double_Maxdelta[i]));
                    }
                    else
                    {
                        for (int i = 0; i < 8; i++)
                             vm.Save_All_PD_Value[i].Add(new DataPoint((double)Math.Round((decimal)vm.timer2_count * vm.Int_Read_Delay / 1000, 2), vm.Float_PD[i]));
                    }

                    vm.Chart_All_DataPoints = new List<List<DataPoint>>( vm.Save_All_PD_Value);

                    vm.Chart_DataPoints = new List<DataPoint>(vm.Chart_All_DataPoints[0]);  //A lineseries
                    vm.timer2_count++;
                    #endregion                    
                }
            }
            catch { }
        }
        private void Timer3_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (vm.IsGoOn)
                {
                    if (vm.dB_or_dBm)  //true is dB
                    {
                        //power_PM = -30;

                        if (vm.station_type == "Hermetic Test")
                            power_PM = Math.Round(vm.pm.ReadPower() - vm.float_WL_Ref[vm.switch_selected_index], 4);
                        else
                            power_PM = Math.Round(vm.pm.ReadPower() - vm.float_WL_Ref[0], 4);
                    }
                    else
                        power_PM = Math.Round(vm.pm.ReadPower(), 4);
                }
                    

                vm.Value_PD.Clear();
                vm.Float_PD.Clear();

                float y = Convert.ToSingle(power_PM);
                float z = (y * 300 / -64 - 150) * -1;
                z = z != 1350 ? z : 150;

                if (z < -150)
                    z = -150;

                if (vm.ch < 9) //Switch mode  1~8
                {
                    if (vm.Gauge_Page_now == 1)
                    {
                        vm.Value_PD = new List<float>() { -150, -150, -150, -150, -150, -150, -150, -150 };
                        vm.Float_PD = Analysis.ListDefault<double>(8);

                        vm.Value_PD[vm.ch - 1] = z;
                        vm.Float_PD[vm.ch - 1] = y;
                        vm.Value_PD = new List<float>(vm.Value_PD);

                        vm.Str_PD = Analysis.ListDefault<string>(8);
                        vm.Str_PD[vm.ch - 1] = power_PM.ToString();
                        vm.Str_PD = new List<string>(vm.Str_PD);
                    }
                    else
                        vm.Str_PD = new List<string>();
                }
                else if (vm.ch < 13 && vm.ch >= 9)  //Switch mode  9~12
                {
                    if (vm.Gauge_Page_now == 2)
                    {
                        vm.Value_PD = new List<float>() { -150, -150, -150, -150, -150, -150, -150, -150 };
                        vm.Float_PD = Analysis.ListDefault<double>(8);

                        vm.Value_PD[vm.ch - 9] = z;
                        vm.Float_PD[vm.ch - 9] = y;
                        vm.Value_PD = new List<float>(vm.Value_PD);

                        vm.Str_PD = Analysis.ListDefault<string>(8);
                        vm.Str_PD[vm.ch - 9] = power_PM.ToString();
                        vm.Str_PD = new List<string>(vm.Str_PD);
                    }
                    else
                        vm.Str_PD = new List<string>();
                }
                else  //Normal mode
                {
                    vm.Value_PD.Add(z);  //-150~150 degree, for gauge binding
                    vm.Float_PD.Add(y);  //list 0~-64dBm in float type

                    vm.Str_PD = new List<string>() { power_PM.ToString() };
                }

                vm.Value_PD = new List<float>(vm.Value_PD);
                vm.Float_PD = new List<double>(vm.Float_PD);
            }
            catch { }
        }

        private async void btn_comment_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtBox_comment.Text)) //Check comment box is empty or not
                return;

            bool _isGoOn_On = vm.IsGoOn;

            await anly.Port_ReOpen();
            vm.Str_comment = txtBox_comment.Text;
            await Cmd_Write_RecieveData(vm.Str_comment, true, 0);

            if (_isGoOn_On)
                await vm.PD_GO();
        }

        private async void btn_ID_Click(object sender, RoutedEventArgs e)
        {
            bool _isGoOn_On = vm.IsGoOn;
            await vm.Port_ReOpen(vm.Selected_Comport);
            vm.Str_comment = "ID?";
            await Cmd_Write_RecieveData(vm.Str_comment, true, 0);

            if (_isGoOn_On)
            {
                if (vm.PD_or_PM == false)
                    await vm.PD_GO();
                else
                    vm.PM_GO();
            }
        }

        private async void btn_D0_Click(object sender, RoutedEventArgs e)
        {
            if (vm.PD_or_PM == false)
                await D0_show();
            else
                await D0_show_PM();
        }
               
        private async Task D0_show()
        {
            vm.list_D_All = new List<List<string>>();

            if (!vm.PD_or_PM)  //PD mode
            {
                for (int i = 1; i <= 8; i++)
                {
                    vm.Str_comment = "D" + i.ToString() + "?";
                    try
                    {
                        if (!vm.IsGoOn)  //Go is off
                        {
                            try
                            {
                                vm.port_PD.Open();
                            }
                            catch { }
                            await _cmd_write_recieveData_ForD0(vm.Str_comment);
                        }
                        else  //Go is on
                        {
                            vm.timer2.Stop();
                            await vm.AccessDelayAsync(105);
                            vm.port_PD.DiscardInBuffer();       // RX
                            vm.port_PD.DiscardOutBuffer();      // TX
                            vm.port_PD.Close();

                            vm.port_PD.Open();
                            await _cmd_write_recieveData_ForD0(vm.Str_comment);
                        }
                    }
                    catch
                    {
                        i = 9;
                        vm.Str_cmd_read = "Port is closed";
                    }
                }
            }

            else  //PM mode
            {
                if (vm.station_type == "Hermetic Test")
                {
                    for (int ch = 0; ch < 8; ch++)
                    {
                        if (!vm.Bool_Gauge[ch])
                        {
                            vm.list_D_All.Add(new List<string>());  //Add one channel list to All channel list
                            continue;
                        }

                        vm.Str_comment = "D1?";

                        if (!string.IsNullOrEmpty(vm.list_Board_Setting[ch][1]))
                        {
                            str_selected_com = vm.list_Board_Setting[ch][1];
                            vm.port_PD = new SerialPort(str_selected_com, 115200, Parity.None, 8, StopBits.One);
                        }
                        else
                        {
                            vm.list_D_All.Add(new List<string>());  //Add one channel list to All channel list
                            continue;
                        }

                        try
                        {
                            if (!vm.IsGoOn)  //Go is off
                            {
                                await anly.Port_ReOpen();
                                await Cmd_Write_RecieveData(vm.Str_comment, true, ch);
                            }
                            else  //Go is on
                            {
                                await vm.PM_Stop();
                                await anly.Port_ReOpen();
                                await Cmd_Write_RecieveData(vm.Str_comment, true, ch);
                                vm.PM_GO();
                            }
                        }
                        catch { vm.Str_cmd_read = "Port is closed"; }
                    }
                }
                else
                {
                    for (int ch = 0; ch < 1; ch++)
                    {
                        vm.Str_comment = "D1?";

                        try
                        {
                            if (!vm.IsGoOn)  //Go is off
                            {
                                await vm.Port_ReOpen(vm.Selected_Comport);
                                await Cmd_Write_RecieveData(vm.Str_comment, true, ch);
                            }
                            else  //Go is on
                            {
                                await vm.PM_Stop();
                                await vm.Port_ReOpen(vm.Selected_Comport);
                                await Cmd_Write_RecieveData(vm.Str_comment, true, ch);
                                vm.PM_GO();
                            }
                        }
                        catch { vm.Str_cmd_read = "Port is closed"; }
                    }
                }
            }
        }

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
                    vm.Str_comment = "P0?";
                    await Cmd_Write_RecieveData(vm.Str_comment, true, 0);                    
                }
            }
            else   //PM mode
            {
                if (_isGoOn_On) vm.PM_GO();
                else vm.Str_cmd_read = vm.pm.ReadPower().ToString();
            }
        }

        private async void ComBox_Calibration_DropDownClosed(object sender, EventArgs e)
        {            
            ComboBox obj = (ComboBox)sender;

            if (obj.SelectedIndex == int_saved_combox_index) return;

            bool _isGoOn_On = vm.IsGoOn;
            vm.isStop = false;

            obj.IsEnabled = false;  //防呆
            btn_GO.IsEnabled = false;

            switch (obj.SelectedItem.ToString())
            {
                case "VOA -> 0":
                    await Reset_VOA(_isGoOn_On);
                    break;

                case "TF -> 0":
                    await Reset_TF(_isGoOn_On);
                    break;

                case "K WEP":
                    break;

                case "K VOA":
                    await K_V3(true);
                    break;

                case "K TF (Rough)":
                    await K_TF_Rough();
                    await cmd.Save_Chart();
                    PD_or_PM_Go(_isGoOn_On);                    
                    break;

                case "K TF (Detail)":
                    await K_TF_Detail();
                    await cmd.Save_Chart();
                    PD_or_PM_Go(_isGoOn_On);
                    break;

                case "K TF":                    
                    await K_TF(_isGoOn_On);                    
                    break;

                case "Curve Fitting":
                    await K_Curfit( vm.Save_All_PD_Value, Points, BestCoeffs, "K TF");
                    await cmd.Save_Chart();
                    PD_or_PM_Go(_isGoOn_On);
                    break;

                case "K WL":                    
                    #region initial setting
                    vm.Chart_All_DataPoints.Clear();
                    vm.Chart_DataPoints.Clear();
                    vm.Save_All_PD_Value.Clear();
                    vm.Save_All_PD_Value = new List<List<DataPoint>>()
            {
                new List<DataPoint>(),new List<DataPoint>(),new List<DataPoint>(),new List<DataPoint>(),
                new List<DataPoint>(),new List<DataPoint>(),new List<DataPoint>(),new List<DataPoint>()
            };
                    #endregion

                    process_level = 2;
                    process_step = 0;
                    process_steps = anly.Calculate_All_Process_Steps(process_level);

                    if (vm.PD_or_PM == false)
                        K_WL_PD();
                    else
                    {
                        if (vm.station_type == "Hermetic Test") K_WL_PM_12CH(new List<string>());
                        else K_WL_PM();
                    }
                    break;
            }

            ComBox_Calibration.SelectedIndex = 0;
            obj.IsEnabled = true;
            btn_GO.IsEnabled = true;
        }

        private async Task K_TF(bool _isGoOn_On)
        {
            if (combox_product.SelectedItem.ToString() == "UFA")
            {
                await Reset_TF(_isGoOn_On);
                await K_V3(false);
            }

            if (vm.isStop == true)
                return;

            await K_TF_Rough();  //粗掃
            await cmd.Save_Chart();

            if (vm.isStop == true)
                return;

            await K_TF_Detail();  //第一次細掃
            await cmd.Save_Chart();

            if (vm.isStop == true)
                return;
            
            await K_Curfit( vm.Save_All_PD_Value, Points, BestCoeffs, "K TF");  //Curfitting
            await cmd.Save_Chart();
            vm.int_detail_scan_gap = 200;
        }

        private async Task K_WEP(bool _isGoOn_On)
        {
            if (vm.product_type == "UFA")
            {                
                if (vm.selected_band == "C Band")
                    vm.center_WL = 1548.5;
                else
                    vm.center_WL = 1594;

                vm.tls.SetWL(vm.center_WL);                

                await Reset_TF(_isGoOn_On);
                await K_V3(false);
            }

            if (vm.isStop == true) return;

            //await K_TF_Rough();  //粗掃
            //await cmd.Save_Chart();

            //if (vm.isStop == true)
            //    return;

            //await K_TF_Detail();  //第一次細掃
            //await cmd.Save_Chart();

            //if (vm.isStop == true)
            //    return;

            //await K_Curfit(vm.Save_All_PD_Value, Points, BestCoeffs, "K TF");  //Curfitting
            //await cmd.Save_Chart();
            //vm.int_detail_scan_gap = 200;
        }

        private async void btn_K_v12_detail_Click(object sender, RoutedEventArgs e)
        {
            ToggleButton obj = (ToggleButton)sender;

            if (obj.IsChecked == true)
            {
                cmd.Clean_Chart();
                vm.List_PDvalue_byV12 = new List<List<float>>(new List<float>[8]);
                vm.List_V12 = new List<int>();
            }

            vm.Chart_x_title = "DAC"; //Rename Chart x axis title
            vm.Str_Status = "Calibration TF (Detail)";
            bool _isGoOn_On = vm.IsGoOn;
            await anly.Port_ReOpen();

            vm.Float_PD = new List<double>();

            List<int> list_prescan_dac = new List<int>();
            for (int i = -2; i <= 2; i++)
            {
                list_prescan_dac.Add(v12_maxpower_index_PM + vm.int_detail_scan_gap * i);
            }

            if (vm.PD_or_PM == false)  //PD mode
            {
                foreach (int dac in list_prescan_dac)
                {
                    for (int i = 1; i <= 8; i++)  //Set dec from ch1 to ch8
                    {
                        if (obj.IsChecked == true)
                        {
                            try
                            {
                                vm.Str_comment = "D" + i.ToString() + " " + (dac).ToString();  //cmd = D1 1000
                                vm.port_PD.Write(vm.Str_comment + "\r");
                                await vm.AccessDelayAsync(vm.Int_Write_Delay);
                            }
                            catch { vm.Str_cmd_read = "Port Write Cmd Error"; return; }
                        }
                        else break;
                    }
                    if (obj.IsChecked == true)  //Get all PD value in the same dec
                    {
                        try
                        {
                            vm.Str_comment = "P0?";
                            vm.port_PD.Write(vm.Str_comment + "\r");
                            await vm.AccessDelayAsync(vm.Int_Read_Delay);

                            int size = vm.port_PD.BytesToRead;
                            byte[] dataBuffer = new byte[size];
                            int length = vm.port_PD.Read(dataBuffer, 0, size);

                            if (dataBuffer.Length > 0)
                                vm = anly._K12_analysis(dac, dataBuffer);
                            else
                                vm.Title = vm.Str_cmd_read;

                            vm.Str_cmd_read = (dac).ToString();
                        }
                        catch { vm.Str_cmd_read = "P0? Error"; return; }
                    }
                    else break;

                    #region Set Chart data points   
                    try
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            int a = vm.List_PDvalue_byV12[i].Count;
                             vm.Save_All_PD_Value[i].Add(new DataPoint(vm.List_V12[a - 1], vm.List_PDvalue_byV12[i][a - 2]));
                        }
                        vm.Chart_All_DataPoints = new List<List<DataPoint>>( vm.Save_All_PD_Value);
                        vm.Chart_DataPoints = new List<DataPoint>(vm.Chart_All_DataPoints[0]);  //The first lineseries   
                    }
                    catch { }
                    #endregion
                }

                try
                {
                    //Set voltage at the best power
                    for (int i = 1; i <= 8; i++)
                    {
                        try
                        {
                            int maxvalue_index = vm.List_PDvalue_byV12[i - 1].FindIndex(x => x.Equals(vm.List_PDvalue_byV12[i - 1].Max()));
                            if (maxvalue_index > 0)
                            {
                                vm.Str_comment = "D" + i.ToString() + " " + (vm.List_V12[maxvalue_index]).ToString();
                                vm.port_PD.Write(vm.Str_comment + "\r");
                            }
                            await vm.AccessDelayAsync(vm.Int_Read_Delay);
                        }
                        catch { };
                    }
                    await anly.Port_ReOpen();
                    await vm.AccessDelayAsync(vm.Int_Read_Delay);
                    await D0_show();

                    obj.IsChecked = false;
                    if (_isGoOn_On)
                        await vm.PD_GO();
                }
                catch { }

                vm.Str_Status = "Stop";
            }
            else //PM mode
            {
                foreach (int dac in list_prescan_dac)
                {
                    if (obj.IsChecked == true)
                    {
                        if (dac <= 0)
                            vm.Str_comment = "D1 " + Math.Abs(dac).ToString() + ",0,0";  //cmd = D1 1000,0,0

                        else
                            vm.Str_comment = "D1 0," + (dac).ToString() + ",0";  //cmd = D1 0,1000,0

                        try
                        {
                            vm.port_PD.Write(vm.Str_comment + "\r");
                            await vm.AccessDelayAsync(vm.Int_Write_Delay);
                        }
                        catch { vm.Str_cmd_read = "Port Write Cmd Error"; return; }
                    }
                    else break;

                    if (obj.IsChecked == true)  //Get all PD value in the same dec
                    {
                        List<double> power = new List<double>() { vm.pm.ReadPower() };

                        await vm.AccessDelayAsync(vm.Int_Read_Delay);

                        if (power[0] != 99)
                            vm = anly._K12_analysis_PM(dac, power);
                        else
                            vm.Title = vm.Str_cmd_read;

                        vm.Str_cmd_read = (dac).ToString();
                    }
                    else break;

                    #region Set Chart data points   
                    try
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            int a = vm.List_PDvalue_byV12[i].Count;
                             vm.Save_All_PD_Value[i].Add(new DataPoint(vm.List_V12[a - 1], vm.List_PDvalue_byV12[i][a - 2]));
                        }
                        vm.Chart_All_DataPoints = new List<List<DataPoint>>( vm.Save_All_PD_Value);
                        vm.Chart_DataPoints = new List<DataPoint>(vm.Chart_All_DataPoints[0]);  //The first lineseries   
                    }
                    catch { }
                    #endregion
                }
                               
                try
                {
                    //Set voltage at the best power
                    try
                    {
                        int maxpower_index = vm.List_PDvalue_byV12[0].FindIndex(x => x.Equals(vm.List_PDvalue_byV12[0].Max()));
                        
                        if (maxpower_index >= 0)
                        {
                            if (vm.List_V12[maxpower_index] <= 0)
                                vm.Str_comment = "D1 " + (Math.Abs(vm.List_V12[maxpower_index])).ToString() + ",0,0";  //cmd = D1 1000,0,0
                            else
                                vm.Str_comment = "D1 0," + (vm.List_V12[maxpower_index]).ToString() + ",0";  //cmd = D1 0,1000,0

                            vm.port_PD.Write(vm.Str_comment + "\r");
                        }
                        await vm.AccessDelayAsync(vm.Int_Read_Delay);
                    }
                    catch { };

                    await D0_show_PM();

                    obj.IsChecked = false;
                    if (_isGoOn_On)
                    {
                        if (vm.PD_or_PM == false)
                            await vm.PD_GO();
                        else
                            vm.PM_GO();
                    }

                }
                catch { }

                vm.Str_Status = "Stop";
            }
        }

        private async Task Reset_VOA(bool _isGoOn_On)
        {
            _isGoOn_On = vm.IsGoOn;
            await anly.Port_ReOpen();

            for (int i = 1; i <= 8; i++)  //Set dec from ch1 to ch8
            {
                if (vm.isStop == false)
                {
                    try
                    {
                        vm.Str_comment = "VOA" + i.ToString() + " " + (0).ToString();
                        vm.port_PD.Write(vm.Str_comment + "\r");
                        await vm.AccessDelayAsync(vm.Int_Write_Delay);
                    }
                    catch { vm.Str_cmd_read = "Reset VOA Error"; return; }
                }
                else break;
            }
            await vm.AccessDelayAsync(vm.Int_Read_Delay);
            await D0_show();

            vm.isStop = false;

            if (_isGoOn_On)
                await vm.PD_GO();
        }

        private async Task Reset_TF(bool _isGoOn_On)
        {
            _isGoOn_On = vm.IsGoOn;
            await vm.Port_ReOpen(vm.Selected_Comport);

            int ch_all = 8;

            if (vm.station_type == "Testing")
                ch_all = 1;

            for (int i = 1; i <= ch_all; i++)  //Set dec from ch1 to ch8
            {
                if (vm.PD_or_PM)  
                {
                    if (vm.Control_board_type == 1)  //0: UFV, 1: V, ...
                        vm.Str_comment = string.Concat("D", i, " ", "0,0");
                    else
                    {
                        if (vm.List_D0_value.Count == 0)
                        {
                            await D0_show();
                            await vm.AccessDelayAsync(500);
                            await vm.Port_ReOpen(vm.Selected_Comport);
                            vm.Str_comment = string.Concat("D", i, " ", "0,0,", vm.List_D0_value[0][2]);
                        }
                            
                        else
                            vm.Str_comment = string.Concat("D", i, " ", "0,0,", vm.List_D0_value[0][2]);
                    }
                        
                }
                else //PD: False
                {
                    vm.Str_comment = string.Concat("D", i, " ", "0,0");
                }

                try
                {                    
                    vm.port_PD.Write(vm.Str_comment + "\r");
                    await vm.AccessDelayAsync(vm.Int_Write_Delay);
                }
                catch { vm.Str_cmd_read = "Reset TF Error"; return; }
            }

            await vm.AccessDelayAsync(vm.Int_Read_Delay);
            await D0_show();
            vm.isStop = false;
            if (_isGoOn_On)
                vm.PM_GO();
        }

        private async Task<List<string>> K_V3(bool _isSingleAction)
        {
            vm.isStop = false;
            cmd.Clean_Chart();
            vm.List_PDvalue_byV3 = new List<List<float>>(new List<float>[8]);
            vm.List_V3 = new List<int>();
            List<string> list_final_voltage = new List<string>();            

            vm.Chart_x_title = "DAC"; //Rename Chart x axis title
            vm.Str_Status = "Calibration VOA";
            bool _isGoOn_On = vm.IsGoOn;

            if (_isGoOn_On)
            {
                if (vm.PD_or_PM == false) await vm.PD_Stop();
                else await vm.PM_Stop();
            }

            vm.Float_PD = new List<double>();

            var watch = System.Diagnostics.Stopwatch.StartNew();

            if (vm.PD_or_PM == false)  //PD mode
            {
                await anly.Port_ReOpen();
                List<List<string>> list_temp_D0 = vm.List_D0_value;
                for (int dac = vm.int_V3_scan_start; dac <= vm.int_V3_scan_end; dac = dac + vm.int_V3_scan_gap)
                {
                    for (int i = 1; i <= 8; i++)  //Set dec from ch1 to ch8
                    {
                        if (vm.isStop == false)
                        {
                            try
                            {
                                vm.Str_comment = "VOA" + i.ToString() + " " + (dac).ToString();
                                vm.port_PD.Write(vm.Str_comment + "\r");
                                await vm.AccessDelayAsync(15);
                            }
                            catch { vm.Str_cmd_read = "Port Write Cmd Error"; return new List<string>() ; }

                            if (list_temp_D0.Count != 0)
                            {
                                list_temp_D0[i - 1][2] = dac.ToString();
                            }
                        }
                        else break;
                    }
                    vm.List_D0_value = new List<List<string>>(list_temp_D0);
                    if (vm.isStop == false)  //Get all PD value in the same dec
                    {
                        try
                        {
                            vm.Str_comment = "P0?";
                            vm.port_PD.Write(vm.Str_comment + "\r");
                            await vm.AccessDelayAsync(vm.Int_Read_Delay);
                        }
                        catch { vm.Str_cmd_read = "P0? Error"; return new List<string>(); }

                        int size = vm.port_PD.BytesToRead;
                        byte[] buffer = new byte[size];
                        byte[] dataBuffer = new byte[0];
                        do
                        {
                            size = vm.port_PD.BytesToRead;  //check read data length
                            if (size > 0)
                            {
                                buffer = new byte[size];
                                int length = vm.port_PD.Read(buffer, 0, size);
                                dataBuffer = dataBuffer.Concat(buffer).ToArray();
                            }
                            else
                                break;
                        }
                        while (true);

                        if (dataBuffer.Length > 0)
                            vm = anly._K_analysis(dac, dataBuffer);
                        else
                            vm.Title = vm.Str_cmd_read;

                        vm.Str_cmd_read = (dac).ToString();
                    }
                    else break;

                    #region Set Chart data points   
                    try
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            int a = vm.List_PDvalue_byV3[i].Count;
                             vm.Save_All_PD_Value[i].Add(new DataPoint(vm.List_V3[a - 1], vm.List_PDvalue_byV3[i][a - 1]));
                        }
                        vm.Chart_All_DataPoints = new List<List<DataPoint>>( vm.Save_All_PD_Value);
                        vm.Chart_DataPoints = new List<DataPoint>(vm.Chart_All_DataPoints[0]);  //A lineseries   
                    }
                    catch { }
                    #endregion
                }

                //Set voltage at the best power
                try
                {
                    for (int i = 1; i <= 8; i++)
                    {
                        int maxvalue_index = vm.List_PDvalue_byV3[i - 1].FindIndex(x => x.Equals(vm.List_PDvalue_byV3[i - 1].Max()));
                        if (maxvalue_index > 0)
                        {
                            try
                            {
                                vm.Str_comment = "VOA" + i.ToString() + " " + (vm.List_V3[maxvalue_index]).ToString();
                                vm.port_PD.Write(vm.Str_comment + "\r");
                            }
                            catch { vm.Str_cmd_read = "Port Write Cmd Error"; return new List<string>(); }
                        }
                        await vm.AccessDelayAsync(vm.Int_Read_Delay);
                    }
                    vm.port_PD.Close();

                    await vm.AccessDelayAsync(vm.Int_Read_Delay);
                    await D0_show();
                    
                    if (_isGoOn_On)
                        await vm.PD_GO();
                }
                catch { vm.Str_cmd_read = "Set power error"; }
            }
            else //PM mode
            {
                //根據選擇的ch作動
                vm.Save_All_PD_Value.Clear();
                vm.List_PDvalue_byV3.Clear();
                vm.List_V3_dac.Clear();
                List<int> maxpower_index = Analysis.ListDefault<int>(8);
                                
                for (int ch = 0; ch < 8; ch++)
                {
                    if (vm.isStop)
                    {
                        vm.Show_Bear_Window("Stop", false, "String");
                        return new List<string>();
                    }                                       
                        
                    vm.Save_All_PD_Value.Add(new List<DataPoint>());
                    vm.List_PDvalue_byV3.Add(new List<float>());
                    vm.List_V3_dac.Add(new List<int>());

                    if (!vm.Bool_Gauge[ch])
                        continue;

                    if (vm.Is_switch_mode)  //change channel
                    {
                        try
                        {
                            await vm.Port_Switch_ReOpen();

                            vm.Str_comment = "I1 " + (ch + 1).ToString();
                            vm.port_Switch.Write(vm.Str_comment + "\r");
                            await vm.AccessDelayAsync(vm.Int_Write_Delay);
                            vm.port_Switch.Close();
                        }
                        catch
                        {
                            vm.Str_cmd_read = "Switch Error";
                            vm.port_Switch.Close();
                            return new List<string>();
                        }
                    }

                    await vm.AccessDelayAsync(150);

                    //Reset COM port
                    if (string.IsNullOrEmpty(vm.list_Board_Setting[ch][1])) break;                    

                    if (vm.station_type == "Hermetic Test")
                    {
                        if ("COM" + vm.Comport_Switch == vm.list_Board_Setting[ch][1])
                        {
                            //if switch port and Control board port are the same , show a message 
                            continue;
                        }
                        else
                            await vm.Port_ReOpen(vm.list_Board_Setting[ch][1]);
                    }
                        
                    else
                        await vm.Port_ReOpen(vm.Selected_Comport);                   

                    int cnt = 0;

                    int int_V3_gap_now = vm.int_V3_scan_gap;

                    //Scan V3 (Rough)
                    for (int dac = vm.int_V3_scan_start; dac <= vm.int_V3_scan_end; dac = dac + int_V3_gap_now)
                    {
                        if (vm.isStop == false)
                        {
                            //Write Dac
                            try
                            {
                                vm.Str_comment = "D1 0,0," + (dac).ToString();  //cmd = D1 0,0,1000
                                vm.port_PD.Write(vm.Str_comment + "\r");
                                await vm.AccessDelayAsync(vm.Int_Write_Delay + 50);//wait for chip stable

                                if (dac == vm.int_V3_scan_start)
                                    await vm.AccessDelayAsync(50);  //first dac need more time for chip stable
                            }
                            catch { vm.Str_cmd_read = "Port Write Cmd Error : " + vm.list_Board_Setting[ch][1]; vm.isStop = true; return new List<string>(); }
                        }
                        else break;

                        //Read Power Meter
                        if (vm.isStop == false) 
                        {
                            try
                            {
                                double power = vm.pm.ReadPower();

                                await vm.AccessDelayAsync(vm.Int_Read_Delay);

                                vm.Convert_ReadPower_to_UIGauge(power, ch);

                                vm.List_V3_dac[ch].Add(dac);

                                vm.List_PDvalue_byV3[ch].Add(Convert.ToSingle(power));

                                DataPoint dp = new DataPoint(dac, power);
                                vm.Save_All_PD_Value[ch].Add(dp);

                                vm.Str_cmd_read = "Ch" + (ch + 1).ToString() + " : " + dac.ToString();

                                if (power > -20)
                                    int_V3_gap_now = vm.int_V3_scan_gap / 2;
                                else if (power > -10)
                                    int_V3_gap_now = vm.int_V3_scan_gap / 6;
                            }
                            catch { vm.Str_cmd_read = "Read Power Meter Step Error."; return new List<string>(); }
                        }
                        else break;

                        #region Set Chart data points   
                        try
                        {
                            vm.Chart_All_DataPoints = new List<List<DataPoint>>(vm.Save_All_PD_Value);
                            vm.Chart_DataPoints = new List<DataPoint>(vm.Chart_All_DataPoints[0]);  //The first lineseries                                   
                        }
                        catch { }
                        #endregion

                        try
                        {
                            if (cnt > 0)
                            {
                                double d = vm.List_PDvalue_byV3[ch][cnt] - vm.List_PDvalue_byV3[ch][cnt - 1];
                                if (d < -0.3)
                                {
                                    break;
                                }
                            }
                            cnt++;
                        }
                        catch { vm.Str_cmd_read = "Power Level Judge Error."; return new List<string>(); }
                        
                    }

                    int scan_v3_start, scan_v3_end, cnt_round1_last;
                    try
                    {
                        cnt_round1_last = vm.List_PDvalue_byV3[ch].Count - 1;
                        cnt = cnt_round1_last;

                        if (vm.List_V3_dac[ch].Count <= cnt)
                        {
                            vm.Str_cmd_read = "Voltage scan range too small or no best voltage.";
                            return new List<string>();
                        }

                        scan_v3_start = (int)(vm.List_V3_dac[ch].Last() - int_V3_gap_now / 2);
                        scan_v3_end = (int)(vm.List_V3_dac[ch][cnt - 1] - (int_V3_gap_now));

                        int_V3_gap_now = int_V3_gap_now / 3;
                    }
                    catch { vm.Str_cmd_read = "Between Rough and Normal Error."; return new List<string>(); }

                    //Scan V3 (Normal)
                    for (int dac = scan_v3_start; dac > scan_v3_end; dac = dac - int_V3_gap_now)
                    {
                        if (vm.isStop == false)
                        {
                            if (int_V3_gap_now <= 100)
                                break;

                            //Write Dac
                            try
                            {
                                vm.Str_comment = "D1 0,0," + (dac).ToString();  //cmd = D1 0,0,1000
                                vm.port_PD.Write(vm.Str_comment + "\r");
                                await vm.AccessDelayAsync(vm.Int_Write_Delay);
                                await vm.AccessDelayAsync(50);  //wait for chip stable
                            }
                            catch { vm.Str_cmd_read = "Port Write Cmd Error"; return new List<string>(); }

                            if (dac == vm.int_V3_scan_start)
                                await vm.AccessDelayAsync(200);
                        }
                        else break;

                        //Read Power Meter
                        if (vm.isStop == false)
                        {
                            double power = vm.pm.ReadPower();

                            await vm.AccessDelayAsync(vm.Int_Read_Delay);

                            vm.Convert_ReadPower_to_UIGauge(power, ch);

                            vm.List_V3_dac[ch].Add(dac);

                            vm.List_PDvalue_byV3[ch].Add(Convert.ToSingle(power));

                            DataPoint dp = new DataPoint(dac, power);
                            vm.Save_All_PD_Value[ch].Add(dp);

                            vm.Str_cmd_read = "Ch" + (ch + 1).ToString() + " : " + dac.ToString();
                        }
                        else break;

                        #region Set Chart data points   
                        try
                        {
                            vm.Chart_All_DataPoints = new List<List<DataPoint>>(vm.Save_All_PD_Value);
                            vm.Chart_DataPoints = new List<DataPoint>(vm.Chart_All_DataPoints[0]);  //The first lineseries                                   
                        }
                        catch { }
                        #endregion

                        if (cnt > cnt_round1_last)
                        {
                            if (vm.List_PDvalue_byV3[ch][cnt + 1] < vm.List_PDvalue_byV3[ch][cnt])
                            {
                                break;
                            }
                        }

                        cnt++;
                    }

                    int cnt_round2_last = vm.List_PDvalue_byV3[ch].Count - 1;
                    cnt = cnt_round2_last;

                    if (vm.List_V3_dac[ch].Count <= cnt)
                    {
                        vm.Str_cmd_read = "Voltage scan range too small or no best voltage.";
                        return new List<string>();
                    }

                    scan_v3_start = (int)(vm.List_V3_dac[ch].Last() + (int_V3_gap_now / 2));
                    scan_v3_end = (int)(scan_v3_start + (int_V3_gap_now));

                    int_V3_gap_now = int_V3_gap_now / 3;

                    //Scan V3 (Detail)
                    for (int dac = scan_v3_start; dac < scan_v3_end; dac = dac + int_V3_gap_now)
                    {
                        if (vm.isStop == false)
                        {
                            if (int_V3_gap_now <= 50)
                                break;

                            //Write Dac
                            try
                            {
                                vm.Str_comment = "D1 0,0," + (dac).ToString();  //cmd = D1 0,0,1000
                                vm.port_PD.Write(vm.Str_comment + "\r");
                                await vm.AccessDelayAsync(vm.Int_Write_Delay);
                                await vm.AccessDelayAsync(50);  //wait for chip stable
                            }
                            catch { vm.Str_cmd_read = "Port Write Cmd Error"; return new List<string>(); }

                            if (dac == vm.int_V3_scan_start)
                                await vm.AccessDelayAsync(200);
                        }
                        else break;

                        //Read Power Meter
                        if (vm.isStop == false)
                        {
                            double power = vm.pm.ReadPower();

                            await vm.AccessDelayAsync(vm.Int_Read_Delay);

                            vm.Convert_ReadPower_to_UIGauge(power, ch);

                            vm.List_V3_dac[ch].Add(dac);

                            vm.List_PDvalue_byV3[ch].Add(Convert.ToSingle(power));

                            DataPoint dp = new DataPoint(dac, power);
                            vm.Save_All_PD_Value[ch].Add(dp);

                            vm.Str_cmd_read = "Ch" + (ch + 1).ToString() + " : " + dac.ToString();
                        }
                        else break;

                        #region Set Chart data points   
                        try
                        {
                            vm.Chart_All_DataPoints = new List<List<DataPoint>>(vm.Save_All_PD_Value);
                            vm.Chart_DataPoints = new List<DataPoint>(vm.Chart_All_DataPoints[0]);  //The first lineseries                                   
                        }
                        catch { }
                        #endregion

                        if (cnt > cnt_round2_last)
                        {
                            if (vm.List_PDvalue_byV3[ch][cnt + 1] < vm.List_PDvalue_byV3[ch][cnt])
                            {
                                break;
                            }
                        }

                        cnt++;
                    }

                    //Set voltage at the best power
                    try
                    {
                        maxpower_index[ch] = vm.List_PDvalue_byV3[ch].FindIndex(x => x.Equals(vm.List_PDvalue_byV3[ch].Max()));
                        if (maxpower_index[ch] >= 0)
                        {
                            vm.Str_comment = "D1 0,0," + (vm.List_V3_dac[ch][maxpower_index[ch]]).ToString();  //cmd = D1 0,0,1000
                            await Cmd_Write_RecieveData(vm.Str_comment, true, ch);
                        }
                        await vm.AccessDelayAsync(vm.Int_Read_Delay);
                    }
                    catch { };
                }

                var elapsedMs = watch.ElapsedMilliseconds;
                
                //Bear Say
                #region Bear Say
                List<double> list_voltage = new List<double>();
                List<int> list_dac = new List<int>();
                List<List<string>> lls = Analysis.ListDefault<List<string>>(8);
                double final_voltage = 0;

                int count_ch = vm.List_V3_dac.Count;

                for (int c = 0; c < count_ch; c++)
                {
                    if (vm.isStop) break;

                    if (!vm.Bool_Gauge[c])
                    {
                        lls[c] = new List<string>();
                        list_final_voltage.Add("");
                        continue;
                    }

                    #region Read Board Table
                   
                    if (vm.board_read[c].Count==0)
                    {
                        vm.Str_cmd_read = "UFV Board table is empty";
                        continue;
                    }

                    int count = 0;
                    foreach (string strline in vm.board_read[c])
                    {
                        string[] board_read = strline.Split(',');
                        if (board_read.Length <= 1)
                            continue;

                        string voltage = board_read[0];
                        int dac = int.Parse(board_read[1]);

                        list_voltage.Add(Convert.ToDouble(voltage));
                        list_dac.Add(dac);

                        if (dac >= vm.List_V3_dac[c][maxpower_index[c]] && count > 0)
                        {
                            int delta_x = (vm.List_V3_dac[c][maxpower_index[c]] - list_dac[count - 1]);
                            int delta_X = (list_dac[count] - list_dac[count - 1]);
                            double delta_Y = (list_voltage[count] - list_voltage[count - 1]);
                            final_voltage = (Convert.ToDouble(delta_x) / Convert.ToDouble(delta_X)) * delta_Y + list_voltage[count - 1];
                            final_voltage = Math.Round(final_voltage, 1);
                            list_final_voltage.Add(final_voltage.ToString());
                            break;
                        }

                        count++;
                    }
                    //vm.List_V3_dac[c][maxpower_index[c]].ToString()   //電壓對應的DAC
                    lls[c] = new List<string>() { "", "" , final_voltage.ToString() };
                    await vm.AccessDelayAsync(30);
                    #endregion
                }

                if (!vm.isStop && _isSingleAction)
                {
                    vm.List_bear_say = new List<List<string>>(lls);
                    await vm.AccessDelayAsync(50);
                    vm.Show_Bear_Window("K V3 完成 (" + Math.Round((decimal)elapsedMs / 1000, 1).ToString() + " s)", false, "String");
                    vm.Collection_bear_say.Add(lls);
                    vm.bear_say_all++;
                    vm.bear_say_now = vm.bear_say_all;
                    cmd.Save_Calibration_Data("K V3");
                }
                #endregion

                #region Save Chart
                vm.int_chart_count++;
                vm.int_chart_now = vm.int_chart_count;
                vm.Chart_All_Datapoints_History.Add(vm.Chart_All_DataPoints);
                #endregion

                await D0_show_PM();
            }

            if (_isGoOn_On)
            {
                if (vm.PD_or_PM == false) await vm.PD_GO();
                else vm.PM_GO();
            }

            vm.Str_Status = "Stop";

            return list_final_voltage;
        }
        
        private async Task K_V3_Backup()
        {
            vm.isStop = false;
            cmd.Clean_Chart();
            vm.List_PDvalue_byV3 = new List<List<float>>(new List<float>[8]);
            vm.List_V3 = new List<int>();

            vm.Chart_x_title = "DAC"; //Rename Chart x axis title
            vm.Str_Status = "Calibration VOA";
            bool _isGoOn_On = vm.IsGoOn;
            vm.Float_PD = new List<double>();

            var watch = System.Diagnostics.Stopwatch.StartNew();

            if (vm.PD_or_PM == false)  //PD mode
            {
                await anly.Port_ReOpen();
                List<List<string>> list_temp_D0 = vm.List_D0_value;
                for (int dac = vm.int_V3_scan_start; dac <= vm.int_V3_scan_end; dac = dac + vm.int_V3_scan_gap)
                {
                    for (int i = 1; i <= 8; i++)  //Set dec from ch1 to ch8
                    {
                        if (vm.isStop == false)
                        {
                            try
                            {
                                vm.Str_comment = "VOA" + i.ToString() + " " + (dac).ToString();
                                vm.port_PD.Write(vm.Str_comment + "\r");
                                await vm.AccessDelayAsync(15);
                            }
                            catch { vm.Str_cmd_read = "Port Write Cmd Error"; return; }

                            if (list_temp_D0.Count != 0)
                            {
                                list_temp_D0[i - 1][2] = dac.ToString();
                            }
                        }
                        else break;
                    }
                    vm.List_D0_value = new List<List<string>>(list_temp_D0);
                    if (vm.isStop == false)  //Get all PD value in the same dec
                    {
                        try
                        {
                            vm.Str_comment = "P0?";
                            vm.port_PD.Write(vm.Str_comment + "\r");
                            await vm.AccessDelayAsync(vm.Int_Read_Delay);
                        }
                        catch { vm.Str_cmd_read = "P0? Error"; return; }

                        int size = vm.port_PD.BytesToRead;
                        byte[] buffer = new byte[size];
                        byte[] dataBuffer = new byte[0];
                        do
                        {
                            size = vm.port_PD.BytesToRead;  //check read data length
                            if (size > 0)
                            {
                                buffer = new byte[size];
                                int length = vm.port_PD.Read(buffer, 0, size);
                                dataBuffer = dataBuffer.Concat(buffer).ToArray();
                            }
                            else
                                break;
                        }
                        while (true);

                        if (dataBuffer.Length > 0)
                            vm = anly._K_analysis(dac, dataBuffer);
                        else
                            vm.Title = vm.Str_cmd_read;

                        vm.Str_cmd_read = (dac).ToString();
                    }
                    else break;

                    #region Set Chart data points   
                    try
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            int a = vm.List_PDvalue_byV3[i].Count;
                            vm.Save_All_PD_Value[i].Add(new DataPoint(vm.List_V3[a - 1], vm.List_PDvalue_byV3[i][a - 1]));
                        }
                        vm.Chart_All_DataPoints = new List<List<DataPoint>>(vm.Save_All_PD_Value);
                        vm.Chart_DataPoints = new List<DataPoint>(vm.Chart_All_DataPoints[0]);  //A lineseries   
                    }
                    catch { }
                    #endregion
                }

                //Set voltage at the best power
                try
                {
                    for (int i = 1; i <= 8; i++)
                    {
                        int maxvalue_index = vm.List_PDvalue_byV3[i - 1].FindIndex(x => x.Equals(vm.List_PDvalue_byV3[i - 1].Max()));
                        if (maxvalue_index > 0)
                        {
                            try
                            {
                                vm.Str_comment = "VOA" + i.ToString() + " " + (vm.List_V3[maxvalue_index]).ToString();
                                vm.port_PD.Write(vm.Str_comment + "\r");
                            }
                            catch { vm.Str_cmd_read = "Port Write Cmd Error"; return; }
                        }
                        await vm.AccessDelayAsync(vm.Int_Read_Delay);
                    }
                    vm.port_PD.Close();

                    await vm.AccessDelayAsync(vm.Int_Read_Delay);
                    await D0_show();

                    if (_isGoOn_On)
                        await vm.PD_GO();
                }
                catch { vm.Str_cmd_read = "Set power error"; }
            }
            else //PM mode
            {
                //根據選擇的ch作動
                vm.Save_All_PD_Value.Clear();
                vm.List_PDvalue_byV3.Clear();
                vm.List_V3_dac.Clear();
                List<int> maxpower_index = Analysis.ListDefault<int>(8);
                await vm.Port_Switch_ReOpen();

                for (int ch = 0; ch < 8; ch++)
                {
                    if (vm.isStop)
                    {
                        vm.Show_Bear_Window("Stop", false, "String");
                        return;
                    }

                    vm.Save_All_PD_Value.Add(new List<DataPoint>());
                    vm.List_PDvalue_byV3.Add(new List<float>());
                    vm.List_V3_dac.Add(new List<int>());

                    if (!vm.Bool_Gauge[ch])
                        continue;

                    if (vm.Is_switch_mode)  //change channel
                    {
                        try
                        {
                            vm.Str_comment = "I1 " + (ch + 1).ToString();
                            vm.port_Switch.Write(vm.Str_comment + "\r");
                            await vm.AccessDelayAsync(vm.Int_Write_Delay);
                        }
                        catch { vm.Str_cmd_read = "Switch Error"; return; }
                    }

                    await vm.AccessDelayAsync(200);

                    //Reset COM port
                    if (string.IsNullOrEmpty(vm.list_Board_Setting[ch][1])) break;

                    vm.port_PD = new SerialPort(vm.list_Board_Setting[ch][1], 115200, Parity.None, 8, StopBits.One);
                    await anly.Port_ReOpen();

                    //Scan V3 (Rough)
                    for (int dac = vm.int_V3_scan_start; dac <= vm.int_V3_scan_end; dac = dac + (vm.int_V3_scan_gap))
                    {
                        if (vm.isStop == false)
                        {
                            try
                            {
                                vm.Str_comment = "D1 0,0," + (dac).ToString();  //cmd = D1 0,0,1000
                                vm.port_PD.Write(vm.Str_comment + "\r");
                                await vm.AccessDelayAsync(vm.Int_Write_Delay);
                            }
                            catch { vm.Str_cmd_read = "Port Write Cmd Error"; return; }

                            if (dac == vm.int_V3_scan_start)
                                await vm.AccessDelayAsync(200);
                        }
                        else break;

                        if (vm.isStop == false)  //Read Power Meter
                        {
                            double power = vm.pm.ReadPower();

                            await vm.AccessDelayAsync(vm.Int_Read_Delay);

                            vm.List_V3_dac[ch].Add(dac);

                            vm.List_PDvalue_byV3[ch].Add(Convert.ToSingle(power));

                            vm.Save_All_PD_Value[ch].Add(new DataPoint(dac, power));

                            vm.Str_cmd_read = "Ch" + (ch + 1).ToString() + " : " + dac.ToString();
                        }
                        else break;

                        //anly.Process_Schedule(process_step++, process_steps);

                        #region Set Chart data points   
                        try
                        {
                            vm.Chart_All_DataPoints = new List<List<DataPoint>>(vm.Save_All_PD_Value);
                            vm.Chart_DataPoints = new List<DataPoint>(vm.Chart_All_DataPoints[0]);  //The first lineseries                                   
                        }
                        catch { }
                        #endregion
                    }

                    //Set voltage at the best power
                    try
                    {
                        maxpower_index[ch] = vm.List_PDvalue_byV3[ch].FindIndex(x => x.Equals(vm.List_PDvalue_byV3[ch].Max()));
                        if (maxpower_index[ch] >= 0)
                        {
                            vm.Str_comment = "D1 0,0," + (vm.List_V3_dac[ch][maxpower_index[ch]]).ToString();  //cmd = D1 0,0,1000
                            await Cmd_Write_RecieveData(vm.Str_comment, true, ch);
                        }
                        await vm.AccessDelayAsync(vm.Int_Read_Delay);
                    }
                    catch { };
                }

                var elapsedMs = watch.ElapsedMilliseconds;

                //Show bear window
                #region Show Bear Window
                List<double> list_voltage = new List<double>();
                List<int> list_dac = new List<int>();
                List<List<string>> lls = Analysis.ListDefault<List<string>>(8);

                double final_voltage = 0;
                int count_ch = vm.List_V3_dac.Count;

                for (int c = 0; c < count_ch; c++)
                {
                    if (vm.isStop) break;

                    if (!vm.Bool_Gauge[c])
                    {
                        lls[c] = new List<string>();
                        continue;
                    }

                    string board_id = vm.Ini_Read("Board_ID", "Board_ID_" + (c + 1).ToString());
                    string path = @"\\192.168.2.3\tff\Data\BoardCalibration\UFA\" + board_id + "-boardtable.txt";

                    if (!File.Exists(path))
                    {
                        //vm.Show_Bear_Window("UFV Board table is not exist", false, "String");
                        vm.Str_cmd_read = "UFV Board table is not exist";
                        continue;
                    }

                    StreamReader str = new StreamReader(path);
                    str.ReadLine();

                    int count = 0;
                    while (!vm.isStop)  //Read board v3 data
                    {
                        string[] board_read = str.ReadLine().Split(',');  //(一行一行讀取)
                        if (board_read.Length < 1)
                            break;

                        string voltage = board_read[0];
                        int dac = int.Parse(board_read[1]);

                        list_voltage.Add(Convert.ToDouble(voltage));
                        list_dac.Add(dac);

                        if (dac >= vm.List_V3_dac[c][maxpower_index[c]] && count > 0)
                        {
                            int delta_x = (vm.List_V3_dac[c][maxpower_index[c]] - list_dac[count - 1]);
                            int delta_X = (list_dac[count] - list_dac[count - 1]);
                            double delta_Y = (list_voltage[count] - list_voltage[count - 1]);
                            final_voltage = (Convert.ToDouble(delta_x) / Convert.ToDouble(delta_X)) * delta_Y + list_voltage[count - 1];
                            final_voltage = Math.Round(final_voltage, 1);
                            break;
                        }

                        count++;
                    }
                    str.Close(); //(關閉str)

                    lls[c] = new List<string>() { vm.List_V3_dac[c][maxpower_index[c]].ToString(), final_voltage.ToString() };
                    //lls[c] = new List<string>() { Math.Round(vm.List_PDvalue_byV3[c].Max(), 3).ToString(), final_voltage.ToString() };
                }

                if (!vm.isStop)
                {
                    vm.List_bear_say = new List<List<string>>(lls);
                    await vm.AccessDelayAsync(50);
                    vm.Show_Bear_Window("K V3 完成" + "  (" + elapsedMs.ToString() + " ms)", false, "String");
                    vm.Collection_bear_say.Add(lls);
                    vm.bear_say_all++;
                    vm.bear_say_now = vm.bear_say_all;

                    cmd.Save_Calibration_Data("K V3");
                }
                #endregion

                #region Save Chart
                vm.int_chart_count++;
                vm.int_chart_now = vm.int_chart_count;
                vm.Chart_All_Datapoints_History.Add(vm.Chart_All_DataPoints);
                #endregion

                await D0_show_PM();
            }

            if (_isGoOn_On)
            {
                if (vm.PD_or_PM == false) await vm.PD_GO();
                else vm.PM_GO();
            }

            vm.Str_Status = "Stop";
        }

        private async Task K_TF_Rough()
        {
            vm.isStop = false;
            cmd.Clean_Chart();
            vm.List_V12 = new List<int>();
            bool _isGoOn_On;

            if (vm.PD_or_PM == false)  //PD mode
            {
                vm.List_PDvalue_byV12 = new List<List<float>>(new List<float>[8]);
                vm.Chart_x_title = "DAC"; //Rename Chart x axis title
                vm.Str_Status = "Calibration TF (Rough)";

                _isGoOn_On = vm.IsGoOn;
                if (_isGoOn_On)
                    await vm.PD_Stop();

                await vm.Port_ReOpen(vm.Selected_Comport);
                vm.Float_PD = new List<double>();                

                List<List<string>> list_temp_D0 = vm.List_D0_value;

                int rough_stop; bool _is_rough_scan_direction_oppsite;
                if (vm.int_rough_scan_start < vm.int_rough_scan_stop)
                {
                    _is_rough_scan_direction_oppsite = false;
                    rough_stop = vm.int_rough_scan_stop;
                }
                else
                {
                    _is_rough_scan_direction_oppsite = true;            
                    rough_stop = vm.int_rough_scan_stop * -1;
                }

                for (int dac = vm.int_rough_scan_start; dac <= rough_stop; dac = dac + vm.int_rough_scan_gap)
                {
                    if (_is_rough_scan_direction_oppsite)
                        dac = dac * -1;

                    for (int i = 1; i <= 8; i++)  //Set dec from ch1 to ch8
                    {
                        if (vm.isStop == false)
                        {
                            try
                            {
                                vm.Str_comment = "D" + i.ToString() + " " + (dac).ToString();  //cmd = D1 1000
                                vm.port_PD.Write(vm.Str_comment + "\r");
                                await vm.AccessDelayAsync(vm.Int_Write_Delay);
                            }
                            catch { vm.Str_cmd_read = "Port Write Cmd Error"; }

                            if (list_temp_D0.Count != 0)
                            {
                                if (dac >= 0)
                                    list_temp_D0[i - 1][0] = dac.ToString();
                                else
                                    list_temp_D0[i - 1][1] = Math.Abs(dac).ToString();
                            }
                        }
                        else break;
                    }
                    vm.List_D0_value = new List<List<string>>(list_temp_D0);
                    if (vm.isStop == false)  //Get all PD value in the same dec
                    {
                        try
                        {
                            vm.Str_comment = "P0?";
                            vm.port_PD.Write(vm.Str_comment + "\r");
                            await vm.AccessDelayAsync(vm.Int_Read_Delay);
                        }
                        catch { vm.Str_cmd_read = "P0? Error"; }

                        int size = vm.port_PD.BytesToRead;
                        byte[] dataBuffer = new byte[size];
                        int length = vm.port_PD.Read(dataBuffer, 0, size);

                        if (dataBuffer.Length > 0)
                            vm = anly._K12_analysis(dac, dataBuffer);
                        else
                            vm.Title = vm.Str_cmd_read;

                        vm.Str_cmd_read = (dac).ToString();
                    }
                    else break;

                    #region Set Chart data points   
                    try
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            int a = vm.List_PDvalue_byV12[i].Count;
                             vm.Save_All_PD_Value[i].Add(new DataPoint(vm.List_V12[a - 1], vm.List_PDvalue_byV12[i][a - 1]));
                        }
                        vm.Chart_All_DataPoints = new List<List<DataPoint>>( vm.Save_All_PD_Value);
                        vm.Chart_DataPoints = new List<DataPoint>(vm.Chart_All_DataPoints[0]);  //The first lineseries   
                    }
                    catch { }
                    #endregion

                    if (_is_rough_scan_direction_oppsite)
                        dac = dac * -1;
                }

                try
                {
                    //Set voltage at the best power
                    maxPowder_dac = new List<int>();
                    for (int i = 1; i <= 8; i++)
                    {
                        try
                        {
                            int maxvalue_index = vm.List_PDvalue_byV12[i - 1].FindIndex(x => x.Equals(vm.List_PDvalue_byV12[i - 1].Max()));
                            if (maxvalue_index > 0)
                            {
                                maxPowder_dac.Add(vm.List_V12[maxvalue_index]);
                                vm.Str_comment = "D" + i.ToString() + " " + (vm.List_V12[maxvalue_index]).ToString();
                                vm.port_PD.Write(vm.Str_comment + "\r");
                            }
                            await vm.AccessDelayAsync(vm.Int_Read_Delay);
                        }
                        catch { };
                    }
                    await anly.Port_ReOpen();
                    await vm.AccessDelayAsync(vm.Int_Read_Delay);

                    await D0_show();
                }
                catch { }

                vm.Str_Status = "Stop";
            }

            else //PM mode
            {
                vm.List_PMvalue_byV12 = new List<float>();
                vm.Chart_x_title = "DAC"; //Rename Chart x axis title
                vm.Str_Status = "Calibration TF";
                _isGoOn_On = vm.IsGoOn;

                if (vm.IsGoOn)  //Go is off
                {
                    await vm.PM_Stop();
                }

                await vm.Port_ReOpen(vm.Selected_Comport);

                vm.Float_PD = new List<double>();

                for (int dac = vm.int_rough_scan_start; dac <= vm.int_rough_scan_stop; dac = dac + vm.int_rough_scan_gap)
                {            
                    if (!vm.isStop)
                    {
                        if (vm.Control_board_type == 0)  //control board : UFV
                        {
                            if (dac >= 0)
                                vm.Str_comment = "D1 " + Math.Abs(dac).ToString() + ",0,0";  //cmd = D1 1000,0,0
                            else
                                vm.Str_comment = "D1 0," + (dac).ToString() + ",0";  //cmd = D1 0,1000,0
                        }
                        else     //control board : V
                        {
                            if (dac >= 0)
                                vm.Str_comment = "D1 " + Math.Abs(dac).ToString() + ",0";  //cmd = D1 1000,0
                            else
                                vm.Str_comment = "D1 0," + (Math.Abs(dac)).ToString();  //cmd = D1 0,1000
                        }

                        vm.port_PD.Write(vm.Str_comment + "\r");
                        await vm.AccessDelayAsync(vm.Int_Write_Delay);
                        
                    }
                    else break;

                    if (vm.isStop == false)  //Get all PD value in the same dec
                    {
                        List<double> power = new List<double>() { vm.pm.ReadPower() };
                        
                        await vm.AccessDelayAsync(vm.Int_Read_Delay);

                        if (power[0] != 99)
                            vm = anly._K12_analysis_PM(dac, power);

                        vm.Str_cmd_read = (dac).ToString();

                        #region Set Chart data points   
                        int a = vm.List_PMvalue_byV12.Count;
                        DataPoint dp = new DataPoint(vm.List_V12[a - 1], vm.List_PMvalue_byV12[a - 1]);
                        if (a > 0)
                             vm.Save_All_PD_Value[0].Add(dp);

                        vm.Chart_DataPoints = new List<DataPoint>( vm.Save_All_PD_Value[0]);  //The first lineseries   

                        #endregion
                    }
                    else break;
                }
                                
                try
                {
                    if (!vm.isStop)
                    {
                        await vm.Port_ReOpen(vm.Selected_Comport);

                        //Set voltage at the best power
                        int maxpower_index = vm.List_PMvalue_byV12.FindIndex(x => x.Equals(vm.List_PMvalue_byV12.Max()));
                        v12_maxpower_index_PM = vm.List_V12[maxpower_index];  //Save maxpower index
                        if (maxpower_index >= 0)
                        {
                            if (vm.Control_board_type == 0)  //control board : UFV
                            {
                                if (vm.List_V12[maxpower_index] >= 0)
                                    vm.Str_comment = "D1 " + (Math.Abs(vm.List_V12[maxpower_index])).ToString() + ",0,0";  //cmd = D1 1000,0,0
                                else
                                    vm.Str_comment = "D1 0," + (vm.List_V12[maxpower_index]).ToString() + ",0";  //cmd = D1 0,1000,0
                            }
                            else     //control board : UFV
                            {
                                if (vm.List_V12[maxpower_index] >= 0)
                                    vm.Str_comment = "D1 " + (Math.Abs(vm.List_V12[maxpower_index])).ToString() + ",0";  //cmd = D1 1000,0
                                else
                                    vm.Str_comment = "D1 0," + (vm.List_V12[maxpower_index]).ToString();  //cmd = D1 0,1000
                            }


                            vm.port_PD.Write(vm.Str_comment + "\r");
                        }

                        await vm.AccessDelayAsync(vm.Int_Read_Delay);

                        await D0_show_PM();
                    }
                    
                }
                catch { }

                vm.Str_Status = "Stop";
            }            
        }

        List<List<int>> list_7points_dac;
        private async Task<bool> K_TF_Detail()
        {
            cmd.Clean_Chart();
            vm.Chart_x_title = "DAC"; //Rename Chart x axis title
            vm.Str_Status = "Calibration TF (Detail)";
            bool _isGoOn_On = vm.IsGoOn;

            if (_isGoOn_On)
            {
                if (vm.PD_or_PM == true)
                    await vm.PM_Stop();
                else
                    await vm.PD_Stop();
            }

            await vm.Port_ReOpen(vm.Selected_Comport);            

            list_7points_dac = new List<List<int>>();
            maxPowder_dac = new List<int>();

            int all_ch_count = 8;
            if (vm.PD_or_PM == true)  //PM mode
                all_ch_count = 1;

            maxPowder_dac = Analysis.ListDefault<int>(8);
            for (int ch = 0; ch < all_ch_count; ch++)
            {
                if (!vm.Bool_Gauge[ch])
                {
                    list_7points_dac.Add(new List<int>());
                    continue;
                }                    

                if (vm.Bool_Gauge[ch])
                {
                    vm.Str_Status = "Calibration TF (Detail) : Ch " + (ch + 1).ToString();
                    vm.List_PDvalue_byV12 = new List<List<float>>(new List<float>[8]);
                    vm.List_PMvalue_byV12 = new List<float>();
                    vm.Float_PD = new List<double>();

                    //Define 7 Dac points for curve fitting                
                    try
                    {
                        if (vm.List_D0_value[ch][0] == "0")
                        {
                            maxPowder_dac[ch] = Convert.ToInt32(vm.List_D0_value[ch][1]) * -1;
                        }
                        else { maxPowder_dac[ch] = Convert.ToInt32(vm.List_D0_value[ch][0]); }

                        List<int> points_dac = new List<int>();
                        for (int j = -3; j <= 3; j++) { points_dac.Add(maxPowder_dac[ch] + vm.int_detail_scan_gap * j); }
                        list_7points_dac.Add(points_dac);
                    }
                    catch { }

                    if (vm.PD_or_PM == false)  //PD mode
                    {
                        try
                        {
                            foreach (int dac in list_7points_dac[ch])
                            {
                                if (vm.isStop == false)
                                {
                                    vm.Str_comment = "D" + (ch + 1).ToString() + " " + (dac).ToString();  //cmd = D1 1000
                                    vm.port_PD.Write(vm.Str_comment + "\r");
                                    await vm.AccessDelayAsync(vm.Int_Write_Delay);
                                }
                                else break;

                                if (vm.isStop == false)  //Get all PD value in the same dec
                                {
                                    vm.Str_comment = "P0?";
                                    vm.port_PD.Write(vm.Str_comment + "\r");
                                    await vm.AccessDelayAsync(vm.Int_Read_Delay);

                                    int size = vm.port_PD.BytesToRead;
                                    byte[] dataBuffer = new byte[size];
                                    int length = vm.port_PD.Read(dataBuffer, 0, size);

                                    if (dataBuffer.Length > 0)
                                        vm = anly._K12_analysis(dac, dataBuffer);
                                    else
                                        vm.Title = vm.Str_cmd_read;

                                    vm.Str_cmd_read = (dac).ToString();
                                }
                                else break;

                                #region Set Chart data points   
                                try
                                {
                                    int a = vm.List_PDvalue_byV12[ch].Count;
                                     vm.Save_All_PD_Value[ch].Add(new DataPoint(list_7points_dac[ch][a - 1], vm.List_PDvalue_byV12[ch][a - 1]));

                                    vm.Chart_All_DataPoints = new List<List<DataPoint>>( vm.Save_All_PD_Value);  //Update Chart's show data
                                    vm.Chart_DataPoints = new List<DataPoint>(vm.Chart_All_DataPoints[0]);  //The first lineseries   
                                }
                                catch { }
                                #endregion
                            }

                            //Set voltage at the best power
                            try
                            {
                                int maxvalue_index = vm.List_PDvalue_byV12[ch].FindIndex(x => x.Equals(vm.List_PDvalue_byV12[ch].Max()));
                                if (maxvalue_index > -1)
                                {
                                    vm.Str_comment = "D" + (ch + 1).ToString() + " " + (list_7points_dac[ch][maxvalue_index]).ToString();
                                    vm.port_PD.Write(vm.Str_comment + "\r");
                                }
                                await vm.AccessDelayAsync(vm.Int_Read_Delay);
                                vm.Str_read = maxvalue_index.ToString();
                            }
                            catch { };
                        }
                        catch { }
                    }

                    #region PM Mode
                    else //PM mode
                    {                
                        try
                        {
                            await vm.Port_ReOpen(vm.Selected_Comport);

                            foreach (int dac in list_7points_dac[ch])
                            {
                                if (vm.isStop == false)
                                {
                                    if (vm.Control_board_type == 0)  //control board : UFV
                                    {
                                        if (dac >= 0)
                                            vm.Str_comment = "D1 " + Math.Abs(dac).ToString() + ",0,0";  //cmd = D1 1000,0,0
                                        else
                                            vm.Str_comment = "D1 0," + (dac).ToString() + ",0";  //cmd = D1 0,1000,0
                                    }
                                    else     //control board : V
                                    {
                                        if (dac >= 0)
                                            vm.Str_comment = "D1 " + Math.Abs(dac).ToString() + ",0";  //cmd = D1 1000,0
                                        else
                                            vm.Str_comment = "D1 0," + (Math.Abs(dac)).ToString();  //cmd = D1 0,1000
                                    }
                                    
                                    vm.port_PD.Write(vm.Str_comment + "\r");
                                    await vm.AccessDelayAsync(vm.Int_Write_Delay);
                                }
                                else break;

                                if (vm.isStop == false)  //Get PM value in the same dac
                                {
                                    vm.Str_comment = "D1?";
                                    vm.port_PD.Write(vm.Str_comment + "\r");
                                    await vm.AccessDelayAsync(vm.Int_Read_Delay);

                                    vm.List_PMvalue_byV12.Add(Convert.ToSingle(vm.pm.ReadPower()));
                                   
                                    vm.Str_cmd_read = (dac).ToString();
                                }
                                else break;

                                #region Set Chart data points   
                                try
                                {
                                    int a = vm.List_PMvalue_byV12.Count;
                                     vm.Save_All_PD_Value[ch].Add(new DataPoint(list_7points_dac[ch][a - 1], vm.List_PMvalue_byV12[a - 1]));

                                    vm.Chart_All_DataPoints = new List<List<DataPoint>>( vm.Save_All_PD_Value);  //Update Chart's show data
                                    vm.Chart_DataPoints = new List<DataPoint>(vm.Chart_All_DataPoints[0]);  //The first lineseries   
                                }
                                catch { }
                                #endregion
                            }

                            //Set voltage at the best power
                            try
                            {                                
                                int maxvalue_index = vm.List_PMvalue_byV12.FindIndex(x => x.Equals(vm.List_PMvalue_byV12.Max()));
                                if (maxvalue_index > -1)
                                {
                                    int dac = list_7points_dac[ch][maxvalue_index];

                                    if (vm.Control_board_type == 0)  //control board : UFV
                                    {
                                        if (dac >= 0)
                                            vm.Str_comment = "D1 " + Math.Abs(dac).ToString() + ",0,0";  //cmd = D1 1000,0,0
                                        else
                                            vm.Str_comment = "D1 0," + (Math.Abs(dac)).ToString() + ",0";  //cmd = D1 0,1000,0
                                    }
                                    else     //control board : V
                                    {
                                        if (dac >= 0)
                                            vm.Str_comment = "D1 " + Math.Abs(dac).ToString() + ",0";  //cmd = D1 1000,0
                                        else
                                            vm.Str_comment = "D1 0," + (Math.Abs(dac)).ToString();  //cmd = D1 0,1000
                                    }

                                    await Cmd_Write_RecieveData(vm.Str_comment, true, 0);
                                    await vm.AccessDelayAsync(vm.Int_Read_Delay);
                                }                               
                            }
                            catch { };

                            break;
                        }
                        catch { }
                    }
                    #endregion
                }
            }

            await vm.Port_ReOpen(vm.Selected_Comport);
            await vm.AccessDelayAsync(vm.Int_Read_Delay);
                       
            vm.Str_Status = "Stop";            

            return _isGoOn_On;
        }

        private async Task K_Curfit(List<List<DataPoint>> Save_All_PD_Value, List<PointF> Points, List<double> BestCoeffs, string action)
        {
            await anly.CurFit( vm.Save_All_PD_Value, Points, BestCoeffs, action);  //Calculation best dac
            
            if(action=="K TF")
            {
                #region Write Dac
                await vm.Port_ReOpen(vm.Selected_Comport);

                int ch = 0;
                List<int> List_dac = new List<int>();
                foreach (int dac in vm.List_curfit_resultDac)
                {
                    if (vm.Bool_Gauge[ch])
                    {
                        if (vm.PD_or_PM == true)  //PM mode
                        {
                            if (vm.Control_board_type == 0)  //control board : UFV
                            {
                                if (dac >= 0)
                                    vm.Str_comment = "D1 " + Math.Abs(dac).ToString() + ",0,0";  //cmd = D1 1000,0,0
                                else
                                    vm.Str_comment = "D1 0," + (dac).ToString() + ",0";  //cmd = D1 0,1000,0
                            }
                            else     //control board : V
                            {
                                if (dac >= 0)
                                    vm.Str_comment = "D1 " + Math.Abs(dac).ToString() + ",0";  //cmd = D1 1000,0
                                else
                                    vm.Str_comment = "D1 0," + Math.Abs(dac).ToString();  //cmd = D1 0,1000
                            }

                            List_dac.Add(dac);
                        }
                        else  //PD mode
                        {
                            vm.Str_comment = "D" + (ch+1).ToString() + " " + Math.Abs(dac).ToString();
                        }

                        await vm.Port_ReOpen(vm.Selected_Comport);
                        await Cmd_Write_RecieveData(vm.Str_comment, true, 0);
                        await vm.AccessDelayAsync(vm.Int_Read_Delay);
                    }
                    ch++;
                }
                #endregion
                
                if (vm.PD_or_PM == false)  //PD mode
                {
                    if (vm.IsGoOn)
                        await vm.PD_GO();
                    else
                    {
                        vm.Str_comment = "P0?";
                        await Cmd_Write_RecieveData(vm.Str_comment, true, 0);
                    }
                    await D0_show();
                }
                else   //PM mode
                {
                    if (vm.IsGoOn) vm.PM_GO();
                    else
                    {
                        double power = vm.pm.ReadPower(); ;
                        vm.Str_cmd_read = power.ToString();
                        vm.Str_PD = new List<string>() { Math.Round(power, 3).ToString(), "", "", "", "", "", "", "" };

                        //Show Bear Window
                        vm.List_bear_say = new List<List<string>>();
                        vm.List_bear_say.Add(new List<string>() { List_dac[0].ToString(), Math.Round(power, 3).ToString() });
                        vm.Show_Bear_Window(List_dac, false, "");
                    }

                    await D0_show_PM();
                }
            }            
        }

        private async Task D0_show_PM()
        {
            vm.list_D_All = new List<List<string>>();

            if(vm.station_type=="Hermetic Test")
            {
                for (int ch = 0; ch < 8; ch++)
                {
                    if (!vm.Bool_Gauge[ch])
                    {
                        vm.list_D_All.Add(new List<string>());  //Add one channel list to All channel list
                        continue;
                    }

                    vm.Str_comment = "D1?";

                    if (!string.IsNullOrEmpty(vm.list_Board_Setting[ch][1]))
                    {
                        str_selected_com = vm.list_Board_Setting[ch][1];
                        vm.port_PD = new SerialPort(str_selected_com, 115200, Parity.None, 8, StopBits.One);
                    }
                    else
                    {
                        vm.list_D_All.Add(new List<string>());  //Add one channel list to All channel list
                        continue;
                    }

                    try
                    {
                        if (!vm.IsGoOn)  //Go is off
                        {
                            await anly.Port_ReOpen();
                            await Cmd_Write_RecieveData(vm.Str_comment, true, ch);
                        }
                        else  //Go is on
                        {
                            await vm.PM_Stop();
                            await anly.Port_ReOpen();
                            await Cmd_Write_RecieveData(vm.Str_comment, true, ch);
                            vm.PM_GO();
                        }
                    }
                    catch { vm.Str_cmd_read = "Port is closed"; }
                }
            }
            else
            {
                for (int ch = 0; ch < 1; ch++)
                {                  
                    vm.Str_comment = "D1?";
                                      
                    try
                    {
                        if (!vm.IsGoOn)  //Go is off
                        {
                            await vm.Port_ReOpen(vm.Selected_Comport);
                            await Cmd_Write_RecieveData(vm.Str_comment, true, ch);
                        }
                        else  //Go is on
                        {
                            await vm.PM_Stop();
                            await vm.Port_ReOpen(vm.Selected_Comport);
                            await Cmd_Write_RecieveData(vm.Str_comment, true, ch);
                            vm.PM_GO();
                        }
                    }
                    catch { vm.Str_cmd_read = "Port is closed"; }
                }
            }
        }        
       
        async Task _cmd_write_recieveData_ForD0(string cmd)  //for D0?
        {
            try
            {
                if (vm.port_PD.IsOpen)
                {
                    vm.port_PD.Write(vm.Str_comment + "\r");

                    await Task.Delay(vm.Int_Read_Delay);

                    int size = vm.port_PD.BytesToRead;
                    byte[] dataBuffer = new byte[size];
                    do
                    {
                        size = vm.port_PD.BytesToRead;  //check read data length
                        if (size > 0)
                        {
                            dataBuffer = new byte[size];
                            int length = vm.port_PD.Read(dataBuffer, 0, size);
                        }
                        else
                            break;
                    }
                    while (true);

                    //Show read back message
                    vm = anly._read_analysis(vm.Str_comment, dataBuffer);

                    vm.port_PD.DiscardInBuffer();       // RX
                    vm.port_PD.DiscardOutBuffer();      // TX

                    vm.port_PD.Close();

                    #region Analyze Dx?
                    if (vm.Str_comment.ToCharArray(0, 1)[0] == 'D' && vm.Str_comment.ToCharArray(2, 1)[0] == '?') //D1?, D2?...
                    {
                        List<string> list_words = new List<string>();  //one channel list[v1.v2.v3]
                        string[] words = vm.Str_cmd_read.Split(',');  //V1,V2,V3 
                        foreach (string s in words) list_words.Add(s);  //Convert array to list
                        vm.list_D_All.Add(list_words);  //Add one channel list to All channel list
                        vm.List_D0_value = new List<List<string>>(vm.list_D_All); //Make propertychanged event happen                        
                    }
                    #endregion

                    if (vm.IsGoOn)
                    {
                        vm.Str_comment = "P0?";
                        vm.port_PD.Open();
                        vm.timer2.Start();
                    }
                }
            }
            catch { }
        }
             
        private async Task<bool> Cmd_Write_RecieveData(string cmd, bool _is_port_close_after_CmdWrite, int ch)
        {
            try
            {
                if (vm.port_PD.IsOpen)
                {
                    vm.port_PD.Write(vm.Str_comment + "\r");

                    await vm.AccessDelayAsync(vm.Int_Read_Delay);

                    int size = vm.port_PD.BytesToRead;
                    byte[] dataBuffer = new byte[size];
                    int length = vm.port_PD.Read(dataBuffer, 0, size);

                    //Show read back message
                    vm = anly._PM_read_analysis(vm.Str_comment, dataBuffer, ch);

                    if (vm.Str_comment != "ID?")
                    {
                        #region Analyze Dx?
                        if (vm.Str_comment.ToCharArray(0, 1)[0] == 'D' && vm.Str_comment.ToCharArray(2, 1)[0] == '?') //D1?, D2?...
                        {
                            List<string> list_words = new List<string>();  //one channel list[v1.v2.v3]
                            string[] words = vm.Str_cmd_read.Split(',');  //V1,V2,V3 
                            foreach (string s in words) list_words.Add(s);  //Convert array to list
                            vm.list_D_All.Add(list_words);  //Add one channel list to All channel list
                            vm.List_D0_value = new List<List<string>>(vm.list_D_All); //Make propertychanged event happen                        
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

        private void combox_comport_DropDownOpened(object sender, EventArgs e)
        {
            combox_comport.Items.Clear();
            myPorts = SerialPort.GetPortNames(); 

            foreach (string s in myPorts) combox_comport.Items.Add(s);  //寫入所有取得的com
        }

        private void combox_comport_DropDownClosed(object sender, EventArgs e)
        {
            if (combox_comport.SelectedItem != null)
            {
                 str_selected_com = combox_comport.SelectedItem.ToString();
                vm.port_PD = new SerialPort(str_selected_com, 115200, Parity.None, 8, StopBits.One);
            }
            
            vm.Ini_Write("Connection", "Comport", str_selected_com);  //創建ini file並寫入基本設定
        }

        private void RBtn_Gauge_Page_Checked(object sender, RoutedEventArgs e)
        {
            pageTransitionControl.Visibility = Visibility.Visible;
            Frame_Navigation.Visibility = Visibility.Hidden;
            pageTransitionControl.ShowPage(_Page_PD_Gauges);
            //Frame_Navigation.Navigate(_Page_PD_Gauges);
        }

        private void RBtn_Chart_Page_Checked(object sender, RoutedEventArgs e)
        {
            pageTransitionControl.Visibility = Visibility.Visible;
            Frame_Navigation.Visibility = Visibility.Hidden;
            pageTransitionControl.ShowPage(_Page_Chart);
            //Frame_Navigation.Navigate(_Page_Chart);
        }

        private void RBtn_DataGrid_Page_Checked(object sender, RoutedEventArgs e)
        {
            //pageTransitionControl.ShowPage(_Page_DataGrid);

            pageTransitionControl.Visibility = Visibility.Hidden;
            Frame_Navigation.Visibility = Visibility.Visible;
            Frame_Navigation.Navigate(_Page_DataGrid);
        }

        private void RBtn_Laser_Page_Checked(object sender, RoutedEventArgs e)
        {
            pageTransitionControl.Visibility = Visibility.Visible;
            Frame_Navigation.Visibility = Visibility.Hidden;
            pageTransitionControl.ShowPage(_Page_Laser);
            //Frame_Navigation.Navigate(_Page_Laser);
        }

        private void RBtn_Setting_Checked(object sender, RoutedEventArgs e)
        {
            pageTransitionControl.Visibility = Visibility.Visible;
            Frame_Navigation.Visibility = Visibility.Hidden;
            pageTransitionControl.ShowPage(_Page_Setting);
            //Frame_Navigation.Navigate(_Page_Setting);
        }
                                
        private void Window_Closed(object sender, EventArgs e)
        {
            vm.port_PD.Close();
        }

        private void btn_show_function_Click(object sender, RoutedEventArgs e)
        {
            if (vm.Mainfunction_visibility == Visibility.Hidden)
                vm.Mainfunction_visibility = Visibility.Visible;
            else
                vm.Mainfunction_visibility = Visibility.Hidden;
        }

        private void ToggleBtn_ControlMode_Click(object sender, RoutedEventArgs e)
        {
            if (ToggleBtn_ControlMode.IsChecked == false)  //PD mode
            {
                run_PD.Foreground = new SolidColorBrush(Colors.White);
                run_PM.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF878787"));
                vm.Main_Color = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF33D3C4"));
                vm.Ini_Write("Connection", "PD_or_PM", "PD");
            }
            else  //PM mode
            {
                #region PowerMeter Setting
                //Power Meter setting
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
                vm.pm.aveTime(20);
                #endregion

                run_PD.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF878787"));
                run_PM.Foreground = new SolidColorBrush(Colors.White);
                vm.Main_Color = (SolidColorBrush)(new BrushConverter().ConvertFrom("#0085CA"));
                vm.Ini_Write("Connection", "PD_or_PM", "PM");
            }
        }

        private async void txtBox_comment_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                if (string.IsNullOrWhiteSpace(txtBox_comment.Text)) //Check comment box is empty or not
                    return;

                bool _isGoOn_On = vm.IsGoOn;

                await anly.Port_ReOpen();
                vm.Str_comment = txtBox_comment.Text;
                await Cmd_Write_RecieveData(vm.Str_comment, true, 0);

                if (_isGoOn_On)
                    await vm.PD_GO();
            }
        }
                   
        private void txtBox_comment_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox obj = (TextBox)sender;
            
            if (vm.PD_or_PM == false)
                obj.BorderBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF10E2C4"));
            else
                obj.BorderBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#0085CA"));

            txt_label.Opacity = 0;
        }

        private void txtBox_comment_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox obj = (TextBox)sender;
            obj.BorderBrush = new SolidColorBrush(Colors.Gray);

            if (string.IsNullOrEmpty(obj.Text))
                txt_label.Opacity = 1;
        }               
        
        private async void btn_Stop_Click(object sender, RoutedEventArgs e)
        {
            vm.isStop = true;

            if (!vm.PD_or_PM)
            {
                if (vm.IsGoOn == true)
                    await vm.PD_Stop();
            }
        }

        private void combox_product_DropDownClosed(object sender, EventArgs e)
        {
            vm.isStop = false;

            setting.Product_Setting();

            vm.Ini_Write("Productions", "Product", vm.product_type);  //創建ini file並寫入基本設定
        }
                
        List<double> BestCoeffs;
        List<PointF> Points = new List<PointF>();
        CurveFitting CurveFunctions = new CurveFitting();      
        List<List<double>> IL_7points, Dac_7points;

        private void TBtn_Unit_Click(object sender, RoutedEventArgs e)
        {
            if (vm.dB_or_dBm == false)
            {
                run_dBm.Foreground = new SolidColorBrush(Colors.White);
                run_dB.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF878787"));
                vm.str_Unit = "dBm";
            }
            else
            {
                run_dBm.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF878787"));
                run_dB.Foreground = new SolidColorBrush(Colors.White);
                vm.str_Unit = "dB";
            }
            vm.Ini_Write("Productions", "Unit", vm.str_Unit);
        }

        int process_steps, process_step, process_level;
        private async void K_WL_Click(object sender, RoutedEventArgs e)
        {
            if (vm.IsGoOn)
            {
                if (vm.PD_or_PM)
                    vm.timer3.Stop();
                else
                    vm.timer2.Stop();
            }
            
            #region initial setting
            vm.isStop = false;

            Button obj = (Button)sender;
            obj.IsEnabled = false;  //防呆
            btn_GO.IsEnabled = false;            
            #endregion

            cmd.Set_WL();  //Auto Set TLS Center Wavelength by station type and production type

            List<string> list_finalVoltage = new List<string>();
            if (vm.product_type == "UFA" || vm.product_type == "UFA(H)")
                list_finalVoltage = await K_V3(false);   //K V3 

            if (!vm.isStop)
            {
                if (vm.PD_or_PM == false)
                    K_WL_PD();
                else
                {
                    if (vm.station_type == "Hermetic Test")
                    {
                        if(vm.selected_K_WL_Type=="Human Like")
                            K_WL_PM_12CH(list_finalVoltage);
                        else
                            K_WL_PM_12CH_Backup();
                    }
                    else K_WL_PM();
                }
            }
            else
                vm.Show_Bear_Window("Stop", false, "String");

            obj.IsEnabled = true;  //取消防呆
            btn_GO.IsEnabled = true;
        }

        private async void K_WL_PD()
        {
            List<List<double>> _saved_power = new List<List<double>>();
            await anly.Port_ReOpen();

            #region Rough scan
            for (double wl = vm.float_WL_Scan_Start; wl <= vm.float_WL_Scan_End; wl = wl + vm.float_WL_Scan_Gap)
            {
                if (vm.isStop == true)
                    break;

                setting.Set_Laser_WL(Math.Round(wl, 2));  //逐步切換TLS WL
                vm.Str_cmd_read = wl.ToString();
                await vm.AccessDelayAsync(vm.Int_Set_WL_Delay + 100);

                if(await cmd.Get_PD_Value())
                {
                    vm.Str_cmd_read = "Get PD Value Error";
                    return;
                }
                
                _saved_power.Add(vm.Float_PD);

                for (int ch = 0; ch < 8; ch++)
                {
                    if (!vm.Bool_Gauge[ch]) continue;
                    if (vm.Float_PD.Count <= ch) continue;
                    DataPoint dp = new DataPoint(wl, vm.Float_PD[ch]);
                     vm.Save_All_PD_Value[ch].Add(dp);
                }               

                //更新圖表
                #region Set Chart data points   
                try
                {
                    vm.Chart_All_DataPoints = new List<List<DataPoint>>( vm.Save_All_PD_Value);
                    vm.Chart_DataPoints = new List<DataPoint>(vm.Chart_All_DataPoints[0]);  //A lineseries   
                }
                catch { }
                #endregion
            }
            await cmd.Save_Chart();
            #endregion

            List<double> list_wl = new List<double>();
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
                    list_wl.Add(0);
                    continue;
                }

                //取出某一channel的所有讀到的power值，並存在_saved_ch_power變數
                List<double> _saved_ch_power = new List<double>();
                for (int i = 0; i < _saved_power.Count; i++)
                {
                    if (_saved_power[i].Count <= ch) continue;
                    _saved_ch_power.Add(_saved_power[i][ch]);
                }

                //找到最大power時的WL值
                int wl_index = _saved_ch_power.FindIndex(x => x.Equals(_saved_ch_power.Max()));
                double Best_WL = vm.float_WL_Scan_Start + vm.float_WL_Scan_Gap * wl_index;
                list_wl.Add(Best_WL);  //所有channel的最佳loss相對應的波長

                #region Create new scan range (Detail scan)       
                for (double wl = Best_WL - vm.float_WL_Scan_Gap * 1; wl <= Best_WL + vm.float_WL_Scan_Gap * 1; wl = wl + vm.float_WL_Scan_Gap / 4)
                {
                    if (vm.isStop == true)
                        return;
                    
                    setting.Set_Laser_WL(Math.Round(wl, 2));  //逐步切換TLS WL
                    vm.Str_cmd_read = "Ch " + (ch + 1).ToString() + ":" + wl.ToString();
                    await vm.AccessDelayAsync(vm.Int_Set_WL_Delay);

                    vm.Float_PD = Analysis.ListDefault<double>(8);

                    if (!vm.Bool_Gauge[ch]) continue;

                    if (vm.Bool_Gauge[ch])
                    {
                        if(await cmd.Get_PD_Value())
                        {
                            vm.Str_cmd_read = "Get PD Value Error";
                            return;
                        }
                    }

                    if (vm.Float_PD.Count <= ch) continue;

                    DataPoint dp = new DataPoint(wl, vm.Float_PD[ch]);
                     vm.Save_All_PD_Value[ch].Add(dp);

                    _saved_power.Add(vm.Float_PD);

                    //更新圖表
                    #region Set Chart data points   
                    try
                    {
                        vm.Chart_All_DataPoints = new List<List<DataPoint>>( vm.Save_All_PD_Value);
                        vm.Chart_DataPoints = new List<DataPoint>(vm.Chart_All_DataPoints[0]);  //A lineseries   
                    }
                    catch { }
                    #endregion
                }
                #endregion
            }
            
            await cmd.Save_Chart();

            if (!vm.isStop)
            {
                await K_Curfit( vm.Save_All_PD_Value, Points, BestCoeffs, "K WL");
                await cmd.Save_Chart();
            }

            //Collect data for Bear say
            _save_all_WL_and_IL.Clear();
            for (int ch = 0; ch < 8; ch++)
            {
                if (!vm.Bool_Gauge[ch])
                {
                    _save_all_WL_and_IL.Add(new List<string>());
                    continue;
                }

                vm.tls.SetWL(vm.List_curfit_resultWL[ch]);
                await vm.AccessDelayAsync(vm.Int_Set_WL_Delay);
                await cmd.Get_PD_Value();
                _save_all_WL_and_IL.Add(new List<string>() { vm.List_curfit_resultWL[ch].ToString(), Math.Round(vm.Float_PD[ch], 3).ToString() });
            }

            vm.List_bear_say = _save_all_WL_and_IL;

            vm.Show_Bear_Window(_save_all_WL_and_IL, false, "");

            vm.Str_Status = "K Wavelength Stop";
        }

        private async void K_WL_PM()
        {
            List<List<double>> _saved_power = new List<List<double>>();

            if (vm.IsGoOn == true)
                await vm.PM_Stop();

            vm.Str_Status = "K Wavelength (Rough)";
            for (double wl = vm.float_WL_Scan_Start; wl <= vm.float_WL_Scan_End; wl = wl + vm.float_WL_Scan_Gap)
            {
                if (vm.isStop == true)
                    return;

                setting.Set_Laser_WL(Math.Round(wl, 2));  //逐步切換TLS WL                    
                await vm.AccessDelayAsync(vm.Int_Set_WL_Delay);

                vm.Float_PD = new List<double>() { -99, -99, -99, -99, -99, -99, -99, -99 };

                await vm.Port_Switch_ReOpen();
                for (int ch = 0; ch < 8; ch++)
                {
                    if (vm.isStop == true)
                        return;

                    if (!vm.Bool_Gauge[ch])
                        continue;

                    #region Set Switch
                    try
                    {
                        vm.Str_comment = "I1 " + (ch + 1).ToString();
                        vm.port_Switch.Write(vm.Str_comment + "\r");
                        await vm.AccessDelayAsync(vm.Int_Read_Delay);
                    }
                    catch { vm.Str_cmd_read = "Set Switch Error"; return; }
                    #endregion

                    vm.Str_cmd_read = "Ch " + (ch + 1).ToString() + ":" + wl.ToString();

                    await cmd.Get_PM_Value(ch);

                    DataPoint dp = new DataPoint(wl, vm.Float_PD[ch]);
                     vm.Save_All_PD_Value[ch].Add(dp);
                }

                _saved_power.Add(vm.Float_PD);

                //更新圖表
                #region Set Chart data points   
                try
                {
                    vm.Chart_All_DataPoints = new List<List<DataPoint>>( vm.Save_All_PD_Value);
                    vm.Chart_DataPoints = new List<DataPoint>(vm.Chart_All_DataPoints[0]);  //A lineseries   
                }
                catch { }
                #endregion
            }
            await cmd.Save_Chart();
            await K_Curfit( vm.Save_All_PD_Value, Points, BestCoeffs, "K WL"); //第一次Curfitting

            List<double> list_wl = new List<double>();
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
                    list_wl.Add(0);
                    _save_all_WL_and_IL.Add(new List<string>());
                    continue;
                }

                //取出某一channel的所有讀到的power值，並存在_saved_ch_power變數
                List<double> _saved_ch_power = new List<double>();
                for (int i = 0; i < _saved_power.Count; i++)
                {
                    _saved_ch_power.Add(_saved_power[i][ch]);
                }

                //找到最大power時的WL值
                int wl_index = _saved_ch_power.FindIndex(x => x.Equals(_saved_ch_power.Max()));
                double Best_WL = vm.float_WL_Scan_Start + vm.float_WL_Scan_Gap * wl_index;
                list_wl.Add(Best_WL);  //所有channel的最佳loss相對應的波長
                
                #region Set Switch
                await vm.Port_Switch_ReOpen();
                try
                {
                    vm.Str_comment = "I1 " + (ch + 1).ToString();
                    vm.port_Switch.Write(vm.Str_comment + "\r");
                    await vm.AccessDelayAsync(vm.Int_Read_Delay);
                }
                catch { vm.Str_cmd_read = "Set Switch Error"; return; }
                #endregion

                #region Create new scan range (Detail scan)       
                for (double wl = Best_WL - vm.float_WL_Scan_Gap * 2; wl <= Best_WL + vm.float_WL_Scan_Gap * 2; wl = wl + vm.float_WL_Scan_Gap / 4)
                {
                    if (vm.isStop == true)
                        return;

                    setting.Set_Laser_WL(Math.Round(wl, 2));  //逐步切換TLS WL
                    vm.Str_cmd_read = "Ch " + (ch + 1).ToString() + ":" + wl.ToString();
                    await vm.AccessDelayAsync(vm.Int_Set_WL_Delay);

                    vm.Float_PD = new List<double>(8) { -99, -99, -99, -99, -99, -99, -99, -99 };

                    if (!vm.Bool_Gauge[ch])
                        continue;

                    if (vm.Bool_Gauge[ch]) await cmd.Get_PM_Value(ch);

                    DataPoint dp = new DataPoint(wl, vm.Float_PD[ch]);
                     vm.Save_All_PD_Value[ch].Add(dp);

                    _saved_power.Add(vm.Float_PD);

                    //更新圖表
                    #region Set Chart data points   
                    try
                    {
                        vm.Chart_All_DataPoints = new List<List<DataPoint>>( vm.Save_All_PD_Value);
                        vm.Chart_DataPoints = new List<DataPoint>(vm.Chart_All_DataPoints[0]);  //A lineseries   
                    }
                    catch { }
                    #endregion
                }
                #endregion        
            }

            await cmd.Save_Chart();

            if (!vm.isStop) await K_Curfit(vm.Save_All_PD_Value, Points, BestCoeffs, "K WL");  //第二次Curfitting

            _save_all_WL_and_IL = new List<List<string>>();
            vm.Float_PD = new List<double>() { -99, -99, -99, -99, -99, -99, -99, -99 };
            for (int ch = 0; ch < 8; ch++)
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
                        vm.Str_comment = "I1 " + (ch + 1).ToString();
                        vm.port_Switch.Write(vm.Str_comment + "\r");
                        await vm.AccessDelayAsync(vm.Int_Read_Delay);
                    }
                    catch { vm.Str_cmd_read = "Set Switch Error"; return; }
                }
                #endregion

                #region Do curfit
                //vm.tls.SetWL(vm.List_curfit_resultWL[ch]);
                //await vm.AccessDelayAsync(vm.Int_Set_WL_Delay + 150);
                //await cmd.Get_PM_Value(ch);
                //_save_all_WL_and_IL.Add(new List<string>() { vm.List_curfit_resultWL[ch].ToString(), Math.Round(vm.Float_PD[ch], 3).ToString() });
                #endregion

                #region Final Scan (9 points)  
                List<double> _saved_ch_wl = new List<double>();
                List<double> _saved_ch_power = new List<double>();
                for (double wl = vm.List_curfit_resultWL[ch] - 0.01 * 4; wl <= vm.List_curfit_resultWL[ch] + 0.01 * 4; wl = wl + 0.01)
                {
                    if (vm.isStop == true) return;

                    setting.Set_Laser_WL(Math.Round(wl, 2));  //逐步切換TLS WL
                    vm.Str_cmd_read = "Ch " + (ch + 1).ToString() + ":" + wl.ToString();
                    await vm.AccessDelayAsync(vm.Int_Set_WL_Delay);

                    if (!vm.Bool_Gauge[ch]) continue;       
                    
                    await cmd.Get_PM_Value(ch);

                    _saved_ch_wl.Add(wl);
                    _saved_ch_power.Add(vm.Float_PD[ch]);                   
                }

                double best_power = _saved_ch_power.Max();
                double best_wl = _saved_ch_wl[_saved_ch_power.FindIndex(x => x.Equals(best_power))];
                _save_all_WL_and_IL.Add(new List<string>() { best_wl.ToString(), Math.Round(best_power, 3).ToString() });

                #endregion        
            }

            vm.List_bear_say = _save_all_WL_and_IL;

            vm.Show_Bear_Window(_save_all_WL_and_IL, false, "");
            
            vm.Str_Status = "K Wavelength Stop";
        }

        private async void K_WL_PM_12CH(List<string> list_finalVoltage)
        {
            vm.Save_All_PD_Value = Analysis.ListDefine<DataPoint>(vm.Save_All_PD_Value, vm.ch_count, new List<DataPoint>());

            List<List<string>> _save_all_WL_and_IL = new List<List<string>>();

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
                                
                if (!vm.Bool_Gauge[ch])
                {
                    _save_all_WL_and_IL.Add(new List<string>());
                    continue;
                }

                #region Set Switch
                if (vm.Is_switch_mode)
                    await vm.Port_Switch_ReOpen();

                if (vm.Is_switch_mode)
                {
                    vm.Str_comment = $"I1 {(ch + 1)}";
                    try { vm.port_Switch.Write(vm.Str_comment + "\r"); }
                    catch { vm.Str_cmd_read = "Set Switch Error"; return; }
                    //vm.port_Switch.Close();
                    await vm.AccessDelayAsync(vm.Int_Read_Delay);
                }
                #endregion

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

                        double power = vm.pm.ReadPower();

                        vm.Convert_ReadPower_to_UIGauge(power, ch);

                        list_ch_power.Add(power);
                        list_ch_wl.Add(wl);
                                                
                        #region Set Chart data points   
                        try
                        {
                            DataPoint dp = new DataPoint(wl, power);
                            vm.Save_All_PD_Value[ch].Add(dp);

                            await vm.AccessDelayAsync(40);

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
                                    wl_next_start = Math.Round(wl - WL_Scan_Gap / 2,2);
                                    wl_next_end = Math.Round(wl_next_start - (WL_Scan_Gap * 3 / 2),2);
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

                            await vm.AccessDelayAsync(40);

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
                                if (power > -15)
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
                    continue;
                }

                //int round2_last_index = list_ch_power.Count - 1;

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

                        double power = vm.pm.ReadPower();
                        vm.Convert_ReadPower_to_UIGauge(power, ch);
                        list_ch_power.Add(power);
                        list_ch_wl.Add(wl);

                        #region Set Chart data points   
                        try
                        {
                            DataPoint dp = new DataPoint(wl, power);
                            vm.Save_All_PD_Value[ch].Add(dp);

                            await vm.AccessDelayAsync(40);

                            vm.Chart_All_DataPoints = new List<List<DataPoint>>(vm.Save_All_PD_Value);
                            vm.Chart_DataPoints = new List<DataPoint>(vm.Chart_All_DataPoints[0]);  //A lineseries   
                        }
                        catch { }
                        #endregion         

                        int index = list_ch_power.Count - 1;
                        if (index > 0 )
                        {
                            if (list_ch_power[index] < list_ch_power[index - 1])
                            {
                                if (power > -8)
                                {
                                    double best_power = list_ch_power.Max();
                                    double best_wl = list_ch_wl[list_ch_power.FindIndex(x => x.Equals(best_power))];

                                    if (list_finalVoltage.Count != vm.ch_count-1 && list_finalVoltage.Count > 0)
                                        _save_all_WL_and_IL.Add(new List<string>() { best_wl.ToString(), Math.Round(best_power, 3).ToString(), list_finalVoltage[ch] });
                                    else
                                        _save_all_WL_and_IL.Add(new List<string>() { best_wl.ToString(), Math.Round(best_power, 3).ToString() });

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
            await vm.AccessDelayAsync(50);
            
            vm.Show_Bear_Window("K WL 完成 (" + Math.Round((decimal)elapsedMs/1000,1).ToString() + " s)", false, "String");
            vm.Collection_bear_say.Add(_save_all_WL_and_IL);   //Save data in history record
            vm.bear_say_all++;
            vm.bear_say_now = vm.bear_say_all;

            cmd.Save_Calibration_Data("K WL");

            vm.Str_Status = "K Wavelength Stop";

            vm.analysis = anly;

            if (vm.IsGoOn == true)
                vm.PM_GO();
        }

        private async void K_WL_PM_12CH_Backup()
        {
            List<List<double>> _saved_power = new List<List<double>>();
            vm.Save_All_PD_Value = Analysis.ListDefine<DataPoint>(vm.Save_All_PD_Value, vm.ch_count, new List<DataPoint>());
            ;
            if (vm.IsGoOn == true)
                await vm.PM_Stop();

            var watch = System.Diagnostics.Stopwatch.StartNew();

            vm.Str_Status = "K Wavelength (Rough)";
            for (double wl = vm.float_WL_Scan_Start; wl <= vm.float_WL_Scan_End; wl = wl + vm.float_WL_Scan_Gap)
            {
                if (vm.isStop == true) return;

                setting.Set_Laser_WL(Math.Round(wl, 2));  //切換TLS WL         

                vm.Float_PD = Analysis.ListDefault<double>(vm.ch_count);

                await vm.Port_Switch_ReOpen();
                for (int ch = 0; ch < vm.ch_count; ch++)
                {
                    if (vm.isStop == true) return;

                    if (!vm.Bool_Gauge[ch]) continue;

                    #region Set Switch
                    vm.Str_comment = "I1 " + (ch + 1).ToString();
                    try { vm.port_Switch.Write(vm.Str_comment + "\r"); }
                    catch { vm.Str_cmd_read = "Set Switch Error"; return; }

                    await vm.AccessDelayAsync(vm.Int_Read_Delay);
                    #endregion

                    vm.Str_cmd_read = "Ch " + (ch + 1).ToString() + ":" + wl.ToString();

                    await cmd.Get_PM_Value(ch);

                    DataPoint dp = new DataPoint(wl, vm.Float_PD[ch]);
                    vm.Save_All_PD_Value[ch].Add(dp);
                }
                _saved_power.Add(vm.Float_PD);

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
            await cmd.Save_Chart();
            await K_Curfit(vm.Save_All_PD_Value, Points, BestCoeffs, "K WL"); //第一次Curfitting

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
                    vm.Str_comment = "I1 " + (ch + 1).ToString();
                    vm.port_Switch.Write(vm.Str_comment + "\r");
                    await vm.AccessDelayAsync(vm.Int_Read_Delay);
                }
                catch { vm.Str_cmd_read = "Set Switch Error"; return; }
                #endregion

                #region Create new scan range (Detail scan)       
                for (double wl = Best_WL - vm.float_WL_Scan_Gap * 2; wl <= Best_WL + vm.float_WL_Scan_Gap * 2; wl = wl + vm.float_WL_Scan_Gap / 2)
                {
                    if (vm.isStop == true) return;

                    setting.Set_Laser_WL(Math.Round(wl, 2));  //逐步切換TLS WL
                    vm.Str_cmd_read = "Ch " + (ch + 1).ToString() + ":" + wl.ToString();
                    await vm.AccessDelayAsync(vm.Int_Set_WL_Delay);

                    vm.Float_PD = Analysis.ListDefault<double>(vm.ch_count);

                    if (!vm.Bool_Gauge[ch]) continue;

                    if (vm.Bool_Gauge[ch]) await cmd.Get_PM_Value(ch);

                    DataPoint dp = new DataPoint(wl, vm.Float_PD[ch]);
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

            if (!vm.isStop) await K_Curfit(vm.Save_All_PD_Value, Points, BestCoeffs, "K WL");  //第二次Curfitting

            _save_all_WL_and_IL = new List<List<string>>();
            vm.Float_PD = Analysis.ListDefault<double>(vm.ch_count);
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
                        vm.Str_comment = "I1 " + (ch + 1).ToString();
                        vm.port_Switch.Write(vm.Str_comment + "\r");
                        await vm.AccessDelayAsync(vm.Int_Read_Delay);
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
                    _saved_ch_power.Add(vm.Float_PD[ch]);

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
                //await vm.AccessDelayAsync(vm.Int_Set_WL_Delay);                
                //best_power = vm.pm.ReadPower();
                //await vm.AccessDelayAsync(vm.Int_Set_WL_Delay);

                _save_all_WL_and_IL.Add(new List<string>() { best_wl.ToString(), Math.Round(best_power, 3).ToString() });
                #endregion        
            }

            var elapsedMs = watch.ElapsedMilliseconds;

            vm.List_bear_say = new List<List<string>>(_save_all_WL_and_IL);
            await vm.AccessDelayAsync(50);

            vm.Show_Bear_Window("K WL 完成" + "  (" + elapsedMs.ToString() + " ms)", false, "String");
            vm.Collection_bear_say.Add(_save_all_WL_and_IL);
            vm.bear_say_all++;
            vm.bear_say_now = vm.bear_say_all;

            cmd.Save_Calibration_Data("K WL");

            vm.Str_Status = "K Wavelength Stop";

            vm.analysis = anly;

            if (vm.IsGoOn == true)
                vm.PM_GO();
        }

        private void btn_close_Click(object sender, RoutedEventArgs e)
        {
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
                grid_clock.Visibility = Visibility.Visible;
                btn_desktop.Visibility = Visibility.Visible;
            }                
            else   //視窗大小不含工作列
            {
                grid_clock.Visibility = Visibility.Collapsed;
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
                System.Windows.Point p = this.PointToScreen(new System.Windows.Point(0,0));
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
                //mRestoreForDragMove = false;

                ////取得最大化時之視窗大小
                //var windowsize_X = Math.Round(this.ActualWidth);
                //var windowsize_Y = Math.Round(this.ActualHeight);

                //this.WindowState = WindowState.Normal;

                ////相對於視窗中的滑鼠位置
                //var point = e.MouseDevice.GetPosition(this);

                ////計算滑鼠在視窗X方向上的位置(以百分比計算)
                //var percentOfpoint = (point.X / windowsize_X) * this.RestoreBounds.Width;

                ////滑鼠在螢幕中的位置                
                ////var point = ((Window)Main_e.Sender).PointToScreen(e.MouseDevice.GetPosition((Window)Main_e.Sender));
                //this.Left = point.X - percentOfpoint;
                //this.Top = 0;

                //this.Height = this.RestoreBounds.Height;
                //this.Width = this.RestoreBounds.Width;

                //this.DragMove();
            }
            else if (mRestoreForDragMove && this.WindowState == WindowState.Normal)
            {
                //mRestoreForDragMove = false;
                mRestoreForDragMove = false;
                this.DragMove();
            }
        }

        private void border_title_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            mRestoreForDragMove = false;
        }

        private void Image_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            List<List<string>> lls = new List<List<string>>();
            lls.Add(new List<string>() { "1530.33", "-1.561", "28.9" });
            lls.Add(new List<string>() { "1531.58", "-1.528", "29.3" });
            vm.List_bear_say = new List<List<string>>(lls);

            vm.Collection_bear_say.Add(lls);
            vm.bear_say_all++;
            vm.bear_say_now = vm.bear_say_all;

            lls = new List<List<string>>();
            lls.Add(new List<string>() { "1550.33", "-1.336", "27.9" });
            lls.Add(new List<string>() { "1551.58", "-1.358", "28.3" });
            vm.List_bear_say = new List<List<string>>(lls);

            vm.Collection_bear_say.Add(lls);
            vm.bear_say_all++;
            vm.bear_say_now = vm.bear_say_all;

            if (lls.First().Count >= 3)
            {
                vm.GaugeTxtSize_Column[1] = new GridLength(lls.First()[0].Count(), GridUnitType.Star);
                vm.GaugeTxtSize_Column[2] = new GridLength(lls.First()[1].Count(), GridUnitType.Star);
                vm.GaugeTxtSize_Column[3] = new GridLength(lls.First()[2].Count(), GridUnitType.Star);
                vm.GaugeTxtSize_Column = new List<GridLength>(vm.GaugeTxtSize_Column);
            }
        }
        
        private void Close_Bear_Window()
        {
            vm.Winbear.Close();
        }

        private void ToggleBtn_ControlMode_Loaded(object sender, RoutedEventArgs e)
        {
            if (vm.PD_or_PM == true)
            {
                #region PowerMeter Setting
                //Power Meter setting
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
                vm.pm.aveTime(20);
                #endregion
            }
        }

        private void Grid_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (lab_read.Visibility == Visibility)
            {
                lab_read.Visibility = Visibility.Hidden;
                grid_All_Ch_Status.Visibility = Visibility.Visible;
            }
            else
            {
                lab_read.Visibility = Visibility.Visible;
                grid_All_Ch_Status.Visibility = Visibility.Hidden;
            }
        }

        private void grid_Unit_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            cmd.Clean_Chart();

            ToggleButton obj = (ToggleButton)sender;
            if (obj.IsChecked == true)
            {
                vm.str_Unit = "dB";
                vm.isDeltaILModeOn = true;
                vm.double_Maxdelta = new List<double>() { 0, 0, 0, 0, 0, 0, 0, 0 };
                vm.savedPower_for_deltaMode = new List<double>();
                foreach (string s in vm.Str_PD)
                {
                    vm.savedPower_for_deltaMode.Add(Convert.ToDouble(s));
                }
                vm.double_MinIL_for_DeltaMode = new List<double>() { 0, 0, 0, 0, 0, 0, 0, 0 };
                vm.double_MaxIL_for_DeltaMode = new List<double>() { 0, 0, 0, 0, 0, 0, 0, 0 };
            }
            else
            {
                vm.str_Unit = "dBm";
                vm.isDeltaILModeOn = false;
            }
        }

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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            vm.mainWin_size = new double[] { ActualWidth, ActualHeight };            
            vm.mainWin_point = new System.Windows.Point(Left, Top);
        }

        //bool[] Bool_Page1 = new bool[8], Bool_Page2 = new bool[4];
        //private async void Combox_Switch_DropDownClosed(object sender, EventArgs e)
        //{
        //    ComboBox obj = (ComboBox)sender;
            
        //    if (obj.SelectedIndex == int_saved_combox_index)
        //        return;

        //    vm.switch_selected_index = obj.SelectedIndex;

        //    await vm.Port_Switch_ReOpen();

        //    if (obj.SelectedIndex > 0 && obj.SelectedIndex < 13)   //Switch 1~12
        //    {
        //        if (string.IsNullOrWhiteSpace("I1 " + obj.SelectedIndex.ToString())) //Check comment box is empty or not
        //            return;

        //        if (obj.SelectedIndex < 9 && int_saved_combox_index >= 9)  //換頁 page1
        //        {
        //            vm.Bool_Page2 = vm.Bool_Gauge_Show;
        //            vm.Str_Channel = new List<string>() { "1", "2", "3", "4", "5", "6", "7", "8", };
        //            vm.Channel_visible = new List<Visibility>() { };
        //            vm.Bool_Gauge_Show = vm.Bool_Page1;
        //            vm.Gauge_Page_now = 1;
        //        }
        //        else if (obj.SelectedIndex > 8 && int_saved_combox_index <= 8)  //換頁 page2
        //        {
        //            vm.Bool_Page1 = vm.Bool_Gauge_Show;
        //            vm.Str_Channel = new List<string>() { "9", "10", "11", "12" };
        //            vm.Channel_visible = new List<Visibility>()
        //            {
        //                Visibility.Visible, Visibility.Visible, Visibility.Visible, Visibility.Visible,
        //                Visibility.Hidden, Visibility.Hidden, Visibility.Hidden, Visibility.Hidden
        //            };
        //            vm.Bool_Gauge_Show = vm.Bool_Page2;
        //            vm.Gauge_Page_now = 2;
        //        }

        //        try
        //        {
        //            vm.Str_comment = "I1 " + obj.SelectedIndex.ToString();
        //            vm.port_Switch.Write(vm.Str_comment + "\r");
        //            await vm.AccessDelayAsync(vm.Int_Write_Delay);
        //        }
        //        catch { vm.Str_cmd_read = "Switch Error"; return; }
        //    }
        //    else if (obj.SelectedIndex == 0)   //Switch ?
        //    {                
        //        try
        //        {
        //            vm.Str_comment = "I1?";
        //            vm.port_Switch.Write(vm.Str_comment + "\r");
        //            await vm.AccessDelayAsync(vm.Int_Read_Delay);
        //        }
        //        catch { vm.Str_cmd_read = "Switch Error"; return; }
        //    }
        //    else
        //    {
        //        vm.Str_Channel = new List<string>() { "1", "2", "3", "4", "5", "6", "7", "8", };
        //        vm.Channel_visible = new List<Visibility>() { };
        //    }                

        //    vm.Switch_Number = obj.SelectedIndex;   //Save Switch channel
        //}

        private void btn_desktop_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void btn_help_Click(object sender, RoutedEventArgs e)
        {
            vm.Show_Bear_Window("有 問 題 請 撥 5 1 7", false, "String_Step");
        }

        private void Grid_clock_Loaded(object sender, RoutedEventArgs e)
        {
            vm.dateTime = new List<string>();
            vm.dateTime.Add(DateTime.Now.ToShortTimeString());
            vm.dateTime.Add(DateTime.Now.ToShortDateString());
            vm.dateTime = new List<string>(vm.dateTime);
            vm.timer1.Start();            
        }

        private void Combox_Switch_Laser_Band_DropDownClosed(object sender, EventArgs e)
        {
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

            vm.Ini_Write("Connection", "Band", vm.selected_band);  //創建ini file並寫入基本設定
        }

        private void btn_max_Loaded(object sender, RoutedEventArgs e)
        {
            grid_clock.Visibility = Visibility.Collapsed;
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

        //private void Combox_Switch_DropDownOpened(object sender, EventArgs e)
        //{
        //    ComboBox obj = (ComboBox)sender;
        //    int_saved_combox_index = obj.SelectedIndex;
        //}
    }
}
