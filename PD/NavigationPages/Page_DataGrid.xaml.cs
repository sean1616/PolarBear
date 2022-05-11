﻿using System;
using System.Windows;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using System.Linq;
using System.IO;
using System.Data.SqlClient;
using System.Data;
using System.IO.Ports;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;

using GPIB_utility;
using PD.ViewModel;
using PD.NavigationPages;
using PD.AnalysisModel;
using PD.Functions;

using OxyPlot;
using OxyPlot.Wpf;

using ExcelDataReader;


namespace PD.NavigationPages
{
    /// <summary>
    /// Page_DataGrid.xaml 的互動邏輯
    /// </summary>
    public partial class Page_DataGrid : UserControl
    {
        ComViewModel vm;
        Page_Ref_Grid _Page_Ref_Grid;
        Page_Ref_Chart _Page_Ref_Chart;
        Page_Board_Grid _Page_Board_Grid;
        ObservableCollection<DataPoint> list_datapoint;
        ObservableCollection<ObservableCollection<DataPoint>> list_list_datapoint;
        Analysis analysis;
        ControlCmd cmd;

        string save_path = "";

        public Page_DataGrid(ComViewModel vm)
        {
            InitializeComponent();

            this.DataContext = vm;
            this.vm = vm;

            analysis = new Analysis(vm);

            cmd = new ControlCmd(vm);

            save_path = txt_path.Text;

            _Page_Ref_Grid = new Page_Ref_Grid(vm, txt_path.Text);

            pageTransitionControl.ShowPage(_Page_Ref_Grid);
            pageTransitionControl.CurrentPage = _Page_Ref_Grid;

            if (vm.isConnected == false)
            {
                if (vm.Ini_Read("Connection", "Band") == "C Band")
                    vm.Double_Laser_Wavelength = 1523;  //Set wavelength to setup ref value
                else
                    vm.Double_Laser_Wavelength = 1571;
            }

            try
            {
                //取得此層資料夾中所有資料夾
                FldArray = System.IO.Directory.GetDirectories(path);

                combox_path.Items.Clear();

                combox_path.Items.Add(@"D:\Ref");

                foreach (string s in FldArray)
                    combox_path.Items.Add(s);
            }
            catch { }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {

        }

        public class Member
        {
            public float Wavelength { get; set; }
            public float Ch_1 { get; set; }
            public float Ch_2 { get; set; }
            public float Ch_3 { get; set; }
            public float Ch_4 { get; set; }
            public float Ch_5 { get; set; }
            public float Ch_6 { get; set; }
            public float Ch_7 { get; set; }
            public float Ch_8 { get; set; }
        }

        private void Btn_show_Click(object sender, RoutedEventArgs e)
        {
            _Page_Ref_Grid = new Page_Ref_Grid(vm, txt_path.Text);

            pageTransitionControl.ShowPage(_Page_Ref_Grid);

            save_path = txt_path.Text;

            pageTransitionControl.CurrentPage.Name = "Grid";

            currentPage = false;
        }

        private void Btn_show_ref_chart_Click(object sender, RoutedEventArgs e)
        {
            Calculate_Ref_Chart_DataPoint();
            _Page_Ref_Chart = new Page_Ref_Chart(vm, list_datapoint);

            pageTransitionControl.ShowPage(_Page_Ref_Chart);
            save_path = txt_path.Text;

            pageTransitionControl.CurrentPage.Name = "Chart";
            currentPage = true;
        }

        private void Calculate_Ref_Chart_DataPoint()
        {
            list_datapoint = new ObservableCollection<DataPoint>();
            list_list_datapoint = new ObservableCollection<ObservableCollection<DataPoint>>();

            double criteria = double.Parse(txt_criteria.Text);

            for (int i = 0; i < 16; i++)
                vm.ref_Color[i] = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF33D3C4"));  //設定小燈顏色

            int count_ch = vm.Ref_Dictionaries.Count;
            for (int ch = 0; ch < count_ch; ch++)
            {
                list_datapoint.Clear();

                if (vm.Ref_Dictionaries[ch].Count == 0)
                {
                    list_list_datapoint.Add(new ObservableCollection<DataPoint>(list_datapoint));
                    continue;
                }

                double pre_IL = 0;

                foreach (var d in vm.Ref_Dictionaries[ch])
                {
                    list_datapoint.Add(new DataPoint(d.Key, d.Value));
                    if (pre_IL != 0)
                        if (Math.Abs(d.Value - pre_IL) >= criteria)
                        {
                            vm.ref_Color[ch] = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF870D0D"));  //設定小燈顏色
                            SolidColorBrush[] temp_ref_color = vm.ref_Color;
                            vm.ref_Color = temp_ref_color;
                        }

                    pre_IL = d.Value;
                }
                list_list_datapoint.Add(new ObservableCollection<DataPoint>(list_datapoint));

                LinearAxis linearAxis = new LinearAxis();
                linearAxis.MajorGridlineStyle = LineStyle.Solid;
                linearAxis.MinorGridlineStyle = LineStyle.Dot;
                linearAxis.Position = OxyPlot.Axes.AxisPosition.Left;
                linearAxis.Minimum = -8.5;
                linearAxis.Maximum = 0.1;
                linearAxis.Minimum = vm.Ref_Dictionaries[ch].Values.Min() - 0.1;
                linearAxis.Maximum = vm.Ref_Dictionaries[ch].Values.Max() + 0.1;

                LinearAxis linearAxis2 = new LinearAxis();
                linearAxis2.MajorGridlineStyle = LineStyle.Solid;
                linearAxis2.MinorGridlineStyle = LineStyle.Dot;
                linearAxis2.Position = OxyPlot.Axes.AxisPosition.Bottom;
                linearAxis2.Minimum = vm.list_wl.Min();
                linearAxis2.Maximum = vm.list_wl.Max();
            }

            vm.Chart_All_DataPoints_ref = list_list_datapoint;
        }

        private void Btn_select_all_Click(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show(pageTransitionControl.CurrentPage.Name);
            if (pageTransitionControl.CurrentPage.Name == "page_ref_grid")
            {
                _Page_Ref_Grid.dataGrid.Focus();
                _Page_Ref_Grid.dataGrid.SelectAll();
            }
            else if (pageTransitionControl.CurrentPage.Name == "page_board_grid")
            {
                _Page_Board_Grid.dataBoardGrid.Focus();
                _Page_Board_Grid.dataBoardGrid.SelectAll();
            }
        }

        private void btn_reload_Click(object sender, RoutedEventArgs e)
        {
            _Page_Ref_Grid = new Page_Ref_Grid(vm, txt_path.Text);
            Calculate_Ref_Chart_DataPoint();
            _Page_Ref_Chart = new Page_Ref_Chart(vm, list_datapoint);
            pageTransitionControl.ShowPage(_Page_Ref_Chart);
            save_path = txt_path.Text;
        }

        private void btn_board_v_Click(object sender, RoutedEventArgs e)
        {
            _Page_Board_Grid = new Page_Board_Grid(vm);

            pageTransitionControl.ShowPage(_Page_Board_Grid);

            vm.memberBoardDatas.Add(new Models.BoardTable_Members()
            {
                Board_ID = "UFV000",
                V_123 = vm.BoardTable_V123.ToString(),
                DAC = 10000.ToString(),
                Vb = 0.1.ToString(),
                Va = 0.2.ToString(),
                delta_V = Math.Round((0.1 - 0.2), 8).ToString(),
                Ratio = Math.Round((0.2 / 10000), 8).ToString()
            });
        }

        int initial_dac = 40000;

        private void Get_Board_Ratio(string board_no, string dataBase)
        {
            string value = string.Empty;

            string connstring = "User ID=" + "opticomm_pe" + ";" +
                                        "Password=" + "opticomm_pe!@#456" + ";" +
                                        "Trusted_Connection=false;" +
                                        "Server=" + "OPTICOMM-MFG" + ";" +
                                        "Data Source=" + "192.168.8.200" + ";" +
                                        "Initial Catalog=" + dataBase + ";Pooling=false;Connection Timeout=90";  //DataBase： "UFA", "CTF"為不同的資料庫

            string tableName = "Board_V";
            string boardSN = board_no;   //板號 ex: "U4V35"
            string sql = "SELECT [Board_SN],[V1],[V2] FROM [dbo]." + tableName + " WHERE [Board_SN]= '" + boardSN + "'";

            DataSet ds = new DataSet();
            SqlConnection connection = new SqlConnection(connstring);
            SqlDataAdapter dataAdapter = new SqlDataAdapter(sql, connection);
            connection.Open();
            dataAdapter.Fill(ds, tableName);
            connection.Close();
            connection = null;

            if (ds.Tables[0].Rows.Count > 0)
            {
                //vm.memberBoardDatas.Clear();
                vm.BoardTable_Dictionary.Clear();
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    string board_SN, V1_Ratio, V2_Ratio;
                    board_SN = ds.Tables[0].Rows[i]["Board_SN"].ToString().Trim();
                    V1_Ratio = ds.Tables[0].Rows[i]["V1"].ToString().Trim();
                    V2_Ratio = ds.Tables[0].Rows[i]["V2"].ToString().Trim();

                    try
                    {
                        vm.BoardTable_Dictionary.Add(board_SN,
                            new List<string>() { V1_Ratio, V2_Ratio });
                    }
                    catch { vm.Save_Log("Get Board Data", "Add board data to dictionary error.", false); }
                }
            }
        }

        private async void btn_board_v123_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _Page_Board_Grid = new Page_Board_Grid(vm);

                pageTransitionControl.ShowPage(_Page_Board_Grid);

                Button obj = (Button)sender;
                vm.BoardTable_V123 = int.Parse(obj.Tag.ToString());
                string ch = txt_chSelected.Text;
                if (int.Parse(txt_chSelected.Text) == 0) ch = "1";

                string BoardNo = string.Empty;

                #region MultiMeter Setting
                byte adrs = (byte)vm.multiMeter_Addr;
                GPIB_CMD myGPIB = new GPIB_CMD(vm.tls_BoardNumber, adrs);  //MultiMeter GPIB Setting
                string mystring = "MEAS:VOLT:DC? 50,0.0001";  //MultiMeter GPIB Read Voltage Command
                Byte[] mybytes = System.Text.Encoding.Default.GetBytes(mystring);
                #endregion

                #region Get Board No.

                if (string.IsNullOrEmpty(vm.Selected_Comport))
                {
                    vm.Str_cmd_read = "Comport空白";
                    vm.Save_Log(new Models.LogMember()
                    {
                        Status = "K Board",
                        Message = "Comport空白",
                        isShowMSG = false
                    });
                    return;
                }

                if (vm.PD_or_PM)  //PM mode
                {
                    DiCon.UCB.Communication.ICommunication icomm;
                    DiCon.UCB.Communication.RS232.RS232 rs232;
                    DiCon.UCB.MTF.IMTFCommand tf;

                    try
                    {
                        rs232 = new DiCon.UCB.Communication.RS232.RS232(vm.Selected_Comport);
                        rs232.OpenPort();
                        icomm = rs232;

                        tf = new DiCon.UCB.MTF.RS232.RS232(icomm);

                        string str_ID = string.Empty;

                        str_ID = tf.ReadSN();
                        BoardNo = str_ID;

                        await Task.Delay(125);
                        rs232.ClosePort();
                    }
                    catch { }
                }
                else  //PD mode
                {
                    vm.Str_cmd_read = string.Empty;

                    await vm.Port_ReOpen(vm.Selected_Comport);

                    vm.Str_Command = string.Concat("SN", ch, "?");
                    await analysis.Cmd_Write_RecieveData(vm.Str_Command, true, 0);

                    BoardNo = vm.Str_cmd_read;
                }
                #endregion

                #region Get Board Ratio

                if (vm.BoardTable_V123 != 3)    //V3比值存在192上，而不是在資料庫中
                {
                    vm.BoardTable_Dictionary.Clear();

                    if (BoardNo.Contains("UFV"))
                        Get_BoardRatio_Database(BoardNo, "UFA", "172.16.10.108");
                    else
                        Get_BoardRatio_Database(BoardNo, "CTF", "172.16.10.108");

                    if (!vm.BoardTable_Dictionary.ContainsKey(BoardNo))   //若資料庫中無此板號
                    {
                        vm.txt_ref_path = vm.CurrentPath + @"\Ref";
                        CSVFunctions.Read_Ref_CSV(vm.txt_ref_path, "", vm);

                        vm.Save_Log(new Models.LogMember()
                        {
                            Status = "K Board",
                            Message = string.Format("資料庫中無板號{0}", BoardNo),
                            Result = "自CSV中取值",
                            isShowMSG = false
                        });
                    }
                    else
                        vm.Save_Log(new Models.LogMember()
                        {
                            Status = "K Board",
                            Message = string.Format("自資料庫中取板號{0}", BoardNo),
                            isShowMSG = false
                        });
                }
                else
                {
                    vm.txt_ref_path = vm.CurrentPath + @"\Ref";
                    CSVFunctions.Read_Ref_CSV(vm.txt_ref_path, "", vm);
                }

                #endregion

                //await vm.Port_ReOpen(vm.Selected_Comport);

                for (int dac = initial_dac; dac <= 60000; dac += 10000)
                {
                    if (vm.BoardTable_V123 == 1)
                        await vm.WriteDac(ch, "D", dac.ToString());  //V1 Write Dac
                    else if (vm.BoardTable_V123 == 2)
                        await vm.WriteDac(ch, "D", (dac * -1).ToString());  //V2 Write Dac
                    else
                        await vm.WriteDac(ch, "VOA", dac.ToString());  //V3 Write Dac

                    await Task.Delay(vm.Int_Write_Delay * 2);

                    vm.port_PD.DiscardInBuffer();
                    vm.port_PD.DiscardOutBuffer();

                    vm.port_PD.Close();

                    myGPIB.Write(mybytes);
                    string volt = "0";
                    await Task.Run(() => volt = myGPIB.Read());
                    //volt = myGPIB.Read();

                    //Check multimeter is connected.
                    if (string.IsNullOrEmpty(volt))
                    {
                        vm.Save_Log("K Board", "GPIB Error", false);
                        vm.Str_Status = "K Board";
                        vm.Str_cmd_read = "GPIB Error.";
                        return;
                    }

                    if (true)
                    {
                        double volt_before = 0, ratio_before = 0;
                        if (vm.BoardTable_V123 == 3) //if select V3 btn
                        {
                            List<double> list_voltage = new List<double>();
                            List<int> list_dac = new List<int>();

                            double final_voltage = 0;

                            #region Get V3 previous data from 192
                            //PM mode
                            if (vm.PD_or_PM) vm.BoardTable_SelectedBoard = BoardNo;
                            else   //PD mode
                            {
                                if (BoardNo.Contains("ERR"))
                                {
                                    BoardNo = vm.List_BoardTable[int.Parse(ch) - 1][0];
                                }

                                if (string.IsNullOrEmpty(BoardNo))  //If SN? return empty value
                                {
                                    if (vm.List_BoardTable.Count > 0)
                                        vm.BoardTable_SelectedBoard = vm.List_BoardTable[Convert.ToInt32(ch) - 1][0];
                                    else
                                    {
                                        vm.Str_cmd_read = "Local and database both have no board ID.";
                                        //return;
                                    }
                                }

                                else vm.BoardTable_SelectedBoard = BoardNo;
                            }

                            string board_id = vm.BoardTable_SelectedBoard;
                            string path = Path.Combine(vm.txt_board_table_path, board_id + "-boardtable.txt" );

                            if (!File.Exists(path))
                            {
                                vm.Show_Bear_Window("UFV Board table is not exist", false, "String");
                                return;
                            }

                            StreamReader str = new StreamReader(path);
                            str.ReadLine();

                            int count = 0;
                            while (true)  //Read board v3 data
                            {
                                string[] board_read = str.ReadLine().Split(',');  //(一行一行讀取)
                                if (board_read.Length < 1)
                                    break;

                                string v3_table_voltage = board_read[0];
                                int v3_table_dac = int.Parse(board_read[1]);

                                list_voltage.Add(Convert.ToDouble(v3_table_voltage));
                                list_dac.Add(v3_table_dac);

                                if (v3_table_dac >= dac && count > 0)
                                {
                                    int delta_x = (dac - list_dac[count - 1]);
                                    int delta_X = (list_dac[count] - list_dac[count - 1]);
                                    double delta_Y = (list_voltage[count] - list_voltage[count - 1]);
                                    final_voltage = (Convert.ToDouble(delta_x) / Convert.ToDouble(delta_X)) * delta_Y + list_voltage[count - 1];
                                    final_voltage = Math.Round(final_voltage, 2);
                                    break;
                                }

                                count++;
                            }
                            str.Close(); //(關閉str)

                            volt_before = final_voltage;
                            #endregion
                        }
                        else
                        {
                            //Find the ratio_before in the table
                            //double ratio_before = 0;

                            if (BoardNo.Contains("ERR") || string.IsNullOrEmpty(BoardNo))
                            {
                                BoardNo = vm.List_BoardTable[int.Parse(ch) - 1][0];
                                Get_BoardRatio_Database(BoardNo, "UFA", "172.16.10.108");
                            }

                            if (vm.BoardTable_Dictionary.ContainsKey(BoardNo))
                            {
                                List<string> list = vm.BoardTable_Dictionary[BoardNo];

                                if (!double.TryParse(list[vm.BoardTable_V123 - 1], out ratio_before))
                                {
                                    vm.Save_Log("Get board data", "Get " + BoardNo + " board data error", false);
                                    continue;
                                }
                            }
                            else
                            {
                                vm.Str_cmd_read = string.Format("板號表中無{0}資料", BoardNo);
                                vm.Save_Log("Get board data", vm.Str_cmd_read, false);
                                return;
                            }

                            volt_before = ratio_before * dac;   //V1: BoardTable_V123 = 1
                        }


                        double volt_after = Math.Abs(Math.Round(Convert.ToDouble(volt), 4));
                        double delta_volt = Math.Round(volt_after - volt_before, 4);
                        vm.Str_Status = "Write DAC";
                        vm.Str_cmd_read = "ch :" + ch + ", Ratio: " + ratio_before.ToString() + ", DAC: " + dac.ToString() + " , Delta Volt: " + delta_volt.ToString();
                        vm.Save_Log(new Models.LogMember()
                        {
                            Status = vm.Str_Status,
                            Message = string.Format("Ratio:{0}", ratio_before.ToString()),
                            isShowMSG = false
                        });

                        #region Save Data to Grid

                        if (BoardNo == string.Empty) BoardNo = vm.List_BoardTable[Convert.ToInt32(ch) - 1][0];

                        vm.memberBoardDatas.Add(new Models.BoardTable_Members()
                        {
                            Board_ID = BoardNo,
                            V_123 = vm.BoardTable_V123.ToString(),
                            DAC = dac.ToString(),
                            Vb = volt_before.ToString(),
                            Va = volt_after.ToString(),
                            delta_V = Math.Round((volt_after - volt_before), 8).ToString(),
                            Ratio = Math.Round((volt_after / dac), 8).ToString()
                        });
                        #endregion

                        //Auto-scrolling the view to end
                        if (_Page_Board_Grid.dataBoardGrid.Items.Count > 0)
                        {
                            var border = VisualTreeHelper.GetChild(_Page_Board_Grid.dataBoardGrid, 0) as Decorator;
                            if (border != null)
                            {
                                var scroll = border.Child as ScrollViewer;
                                if (scroll != null) scroll.ScrollToEnd();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.StackTrace.ToString());
            }
        }

        private void Read_Ref_CSV(string path)
        {
            DataSet ds;
            if (path.Length > 0)
            {
                if (Path.GetExtension(path) == string.Empty) path = path + ".xlsx";

                //Find Ref.xlsx file
                if (File.Exists(path))
                {
                    var extension = Path.GetExtension(path).ToLower();
                    using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        //判斷格式套用讀取方法
                        IExcelDataReader reader = null;
                        if (extension == ".xls")
                        {
                            Console.WriteLine(" => XLS格式");
                            reader = ExcelReaderFactory.CreateBinaryReader(stream, new ExcelReaderConfiguration()
                            {
                                FallbackEncoding = Encoding.GetEncoding("big5")
                            });
                        }
                        else if (extension == ".xlsx")
                        {
                            Console.WriteLine(" => XLSX格式");
                            reader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                        }
                        else if (extension == ".csv")
                        {
                            Console.WriteLine(" => CSV格式");
                            reader = ExcelReaderFactory.CreateCsvReader(stream, new ExcelReaderConfiguration()
                            {
                                FallbackEncoding = Encoding.GetEncoding("big5")
                            });
                        }
                        else if (extension == ".txt")
                        {
                            Console.WriteLine(" => Text(Tab Separated)格式");
                            reader = ExcelReaderFactory.CreateCsvReader(stream, new ExcelReaderConfiguration()
                            {
                                FallbackEncoding = Encoding.GetEncoding("big5"),
                                AutodetectSeparators = new char[] { '\t' }
                            });
                        }

                        //沒有對應產生任何格式
                        if (reader == null)
                        {
                            Console.WriteLine("未知的處理檔案：" + extension);
                        }
                        Console.WriteLine(" => 轉換中");

                        //顯示已讀取資料
                        using (reader)
                        {

                            ds = reader.AsDataSet(new ExcelDataSetConfiguration()
                            {
                                UseColumnDataType = false,
                                ConfigureDataTable = (tableReader) => new ExcelDataTableConfiguration()
                                {
                                    //設定讀取資料時是否忽略標題
                                    UseHeaderRow = false
                                }
                            });

                            //把 DataSet 顯示出來
                            string data;
                            var table = ds.Tables[0];
                            vm.List_BoardTable.Clear();
                            vm.BoardTable_Dictionary.Clear();

                            for (int row = 0; row < table.Rows.Count; row++)
                            {
                                vm.List_BoardTable.Add(new List<string>() { "", "", "", "" });
                                for (var col = 0; col < table.Columns.Count; col++)
                                {
                                    data = table.Rows[row][col].ToString();
                                    Console.Write(data + ",");

                                    if (col == 0) vm.List_BoardTable[row][col] = data;  //board ID
                                    else if (col == 1) vm.List_BoardTable[row][col] = data;  //V1
                                    else if (col == 2) vm.List_BoardTable[row][col] = data;  //V2
                                    else if (col == 3) vm.List_BoardTable[row][col] = data;  //V3                            
                                }
                            }

                            for (int i = 0; i < vm.List_BoardTable.Count; i++)
                            {
                                if (vm.List_BoardTable[i].Count < 4)
                                {
                                    vm.Save_Log("Get Board Data", "Data columns less than 4", false);
                                    continue;
                                }

                                try
                                {
                                    vm.BoardTable_Dictionary.Add(vm.List_BoardTable[i][0],
                                        new List<string>() { vm.List_BoardTable[i][1], vm.List_BoardTable[i][2], vm.List_BoardTable[i][3] });
                                }
                                catch { vm.Save_Log("Get Board Data", "Add board data to dictionary error", false); }
                            }
                        }
                    }
                }
                else vm.Str_cmd_read = "檔案 " + path + " 不存在!";
            }
            else vm.Str_cmd_read = "沒有提供參數!";
        }

        private void btn_board_v2_Click(object sender, RoutedEventArgs e)
        {
            _Page_Board_Grid = new Page_Board_Grid(vm);

            pageTransitionControl.ShowPage(_Page_Board_Grid);
            pageTransitionControl.CurrentPage.Name = "Grid";

        }

        private void btn_board_v3_Click(object sender, RoutedEventArgs e)
        {
            _Page_Board_Grid = new Page_Board_Grid(vm);

            pageTransitionControl.ShowPage(_Page_Board_Grid);
            pageTransitionControl.CurrentPage.Name = "Grid";
        }

        private void btn_opendialog_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                vm.txt_ref_path = folderBrowserDialog.SelectedPath;

                if (pageTransitionControl.CurrentPage == _Page_Ref_Grid)
                {
                    _Page_Ref_Grid = new Page_Ref_Grid(vm, txt_path.Text);
                    pageTransitionControl.ShowPage(_Page_Ref_Grid);
                }
                else if (pageTransitionControl.CurrentPage == _Page_Ref_Chart)
                {
                    _Page_Ref_Grid = new Page_Ref_Grid(vm, txt_path.Text);
                    Calculate_Ref_Chart_DataPoint();
                    _Page_Ref_Chart = new Page_Ref_Chart(vm, list_datapoint);
                    pageTransitionControl.ShowPage(_Page_Ref_Chart);
                }

                save_path = txt_path.Text;
            }

            //System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
            //openFileDialog.InitialDirectory = @"D:\Ref";
            //openFileDialog.RestoreDirectory = true;

            //if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            //{
            //    vm.txt_ref_path = openFileDialog.FileName;
            //}
        }

        private void btn_save_v_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string folder = @"D:\";

                System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
                saveFileDialog.Title = "Save Data";
                saveFileDialog.InitialDirectory = @"D:\";
                saveFileDialog.FileName = "BoardData_" + DateTime.Today.Year + DateTime.Today.Month.ToString("00") + DateTime.Today.Day.ToString("00") + ".csv";
                saveFileDialog.Filter = "CSV (*.csv)|*.csv|TXT (*.txt)|*.txt|All files (*.*)|*.*";

                string BoardData_server_filePath = @"\\192.168.2.3\shared\SeanWu\PD_Fast_Calibration_Voltage_Monitor";
                string BoardData_filePath = @"D:\BoardData_001.csv";
                if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    BoardData_filePath = saveFileDialog.FileName;
                    folder = Path.GetDirectoryName(BoardData_filePath);

                    if (!Directory.Exists(BoardData_server_filePath))
                        Directory.CreateDirectory(BoardData_server_filePath);  //Creat folder on 192 server

                    string extension = Path.GetExtension(BoardData_filePath);

                    string filepath = "";
                    string filepath_server = "";

                    #region String Builder
                    StringBuilder _stringBuilder = new StringBuilder();

                    //Header Title
                    _stringBuilder.Append("Board ID");
                    _stringBuilder.Append(",");
                    _stringBuilder.Append("V123");
                    _stringBuilder.Append(",");
                    _stringBuilder.Append("DAC");
                    _stringBuilder.Append(",");
                    _stringBuilder.Append("V before");
                    _stringBuilder.Append(",");
                    _stringBuilder.Append("V now");
                    _stringBuilder.Append(",");
                    _stringBuilder.Append("Delta V");
                    _stringBuilder.Append(",");
                    _stringBuilder.Append("Ratio");

                    _stringBuilder.AppendLine();  //換行

                    int max_dataCount = vm.memberBoardDatas.Count;  //計算資料行數

                    //資料內容
                    for (int i = 0; i < max_dataCount; i++)
                    {
                        _stringBuilder.Append(vm.memberBoardDatas[i].Board_ID);
                        _stringBuilder.Append(",");
                        _stringBuilder.Append(vm.memberBoardDatas[i].V_123);
                        _stringBuilder.Append(",");
                        _stringBuilder.Append(vm.memberBoardDatas[i].DAC);
                        _stringBuilder.Append(",");
                        _stringBuilder.Append(vm.memberBoardDatas[i].Vb);
                        _stringBuilder.Append(",");
                        _stringBuilder.Append(vm.memberBoardDatas[i].Va);
                        _stringBuilder.Append(",");
                        _stringBuilder.Append(vm.memberBoardDatas[i].delta_V);
                        _stringBuilder.Append(",");
                        _stringBuilder.Append(vm.memberBoardDatas[i].Ratio);

                        _stringBuilder.AppendLine();  //換行
                    }
                    #endregion

                    if (extension.Equals(".csv"))
                    {
                        #region Save as csv

                        filepath = Path.Combine(folder, Path.GetFileNameWithoutExtension(BoardData_filePath)) + ".csv";

                        if (!File.Exists(filepath))
                        {
                            File.AppendAllText(filepath, "");  //Creat a csv file
                        }

                        filepath_server = Path.Combine(BoardData_server_filePath, vm.Station_ID + "_" + Path.GetFileNameWithoutExtension(BoardData_filePath)) + ".csv";

                        if (Directory.Exists(BoardData_server_filePath))
                        {
                            if (!File.Exists(filepath_server))
                            {
                                File.AppendAllText(filepath_server, "");  //Creat a csv file
                            }
                            else
                            {
                                int name_count = 0;
                                for (int i = 0; i < 100; i++)
                                {
                                    name_count++;
                                    filepath_server = 
                                        Path.Combine(BoardData_server_filePath, vm.Station_ID + "_" + 
                                        Path.GetFileNameWithoutExtension(BoardData_filePath)) + "_" + 
                                        name_count.ToString("000") + ".csv";
                                    if (!File.Exists(filepath_server)) break;                                        
                                }

                            }
                        }

                        //Clear file content and lock the file
                        FileStream fileStream = File.Open(filepath, FileMode.Open);
                        fileStream.SetLength(0);
                        fileStream.Close();

                        File.AppendAllText(filepath, _stringBuilder.ToString());  //Save string builder to csv

                        File.AppendAllText(filepath_server, _stringBuilder.ToString());  //Save string builder to csv (save on server)
                        #endregion
                    }

                    else if (extension.Equals(".txt"))
                    {
                        #region Save as txt
                        filepath = Path.Combine(folder, Path.GetFileNameWithoutExtension(BoardData_filePath)) + ".txt";
                        if (File.Exists(filepath))
                            File.Delete(filepath);

                        filepath_server = Path.Combine(BoardData_server_filePath, Path.GetFileNameWithoutExtension(BoardData_filePath)) + ".csv";

                        if (Directory.Exists(BoardData_server_filePath))
                            if (!File.Exists(filepath_server))
                            {
                                File.AppendAllText(filepath_server, "");  //Creat a csv file
                            }

                        File.AppendAllText(filepath, _stringBuilder.ToString());  //Save string builder to csv

                        File.AppendAllText(filepath_server, _stringBuilder.ToString());  //Save string builder to csv (server)

                        #endregion
                    }

                    vm.Str_cmd_read = "File Saved";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.StackTrace.ToString());
            }
        }

        public string directory = null;  // 打開圖片的目錄
        string[] FldArray = null;
        //bool pre_or_next = false;
        string path = @"D:\Ref";

        private void btn_path_dropdown_Click(object sender, RoutedEventArgs e)
        {
            combox_path.Visibility = Visibility.Visible;

            try
            {
                //取得此層資料夾中所有資料夾
                FldArray = System.IO.Directory.GetDirectories(path);

                combox_path.Items.Clear();

                combox_path.Items.Add(@"D:\Ref");

                foreach (string s in FldArray)
                    combox_path.Items.Add(s);
                combox_path.IsDropDownOpen = true;
            }
            catch { }
        }

        bool currentPage = false;  //grid=false, chart=true

        private void combox_path_DropDownClosed(object sender, EventArgs e)
        {
            combox_path.Visibility = Visibility.Collapsed;

            if (combox_path.SelectedItem == null) return;
            vm.txt_ref_path = combox_path.SelectedItem.ToString();

            //save_path = txt_path.Text;
            if (currentPage)
            {
                _Page_Ref_Grid = new Page_Ref_Grid(vm, vm.txt_ref_path);
                Calculate_Ref_Chart_DataPoint();
                _Page_Ref_Chart = new Page_Ref_Chart(vm, list_datapoint);

                pageTransitionControl.ShowPage(_Page_Ref_Chart);

                pageTransitionControl.CurrentPage.Name = "Chart";
            }
            else
            {
                pageTransitionControl.CurrentPage.Name = "Grid";

                _Page_Ref_Grid = new Page_Ref_Grid(vm, vm.txt_ref_path);

                pageTransitionControl.ShowPage(_Page_Ref_Grid);
            }
        }

        private void Btn_clear_Click(object sender, RoutedEventArgs e)
        {
            if (pageTransitionControl.CurrentPage.Name == "page_ref_grid")
            {

            }
            else if (pageTransitionControl.CurrentPage.Name == "page_board_grid")
                vm.memberBoardDatas.Clear();
        }

        private void Btn_delete_Click(object sender, RoutedEventArgs e)
        {
            if (pageTransitionControl.CurrentPage.Name == "page_board_grid")
            {
                int index = _Page_Board_Grid.dataBoardGrid.SelectedIndex;
                if (index >= 0)
                    vm.memberBoardDatas.RemoveAt(index);
            }
        }

        private void btn_openBoardRef_Click(object sender, RoutedEventArgs e)
        {
            //Process.Start(vm.CurrentPath);
            string BoardRefPath = string.Concat(vm.CurrentPath, @"\Ref.xlsx");
            if (File.Exists(BoardRefPath))
                Process.Start(BoardRefPath);
            else if (File.Exists(string.Concat(vm.CurrentPath, @"\Ref.csv")))
                Process.Start(string.Concat(vm.CurrentPath, @"\Ref.csv"));
            else vm.Str_cmd_read = "Ref不存在";
        }

        private void Get_BoardRatio_Database(string board_no, string dataBase, string server_IP)
        {
            string value = string.Empty;

            string connstring = "User ID=" + "opticomm_pe" + ";" +
                                        "Password=" + "opticomm_pe!@#456" + ";" +
                                        "Trusted_Connection=false;" +
                                        "Server=" + "OPTICOMM-MFG" + ";" +
                                        "Data Source=" + server_IP + ";" +
                                        "Initial Catalog=" + dataBase + ";Pooling=false;Connection Timeout=90";  //DataBase： "UFA", "CTF"為不同的資料庫

            string tableName = "Board_V";
            string boardSN = board_no;   //板號 ex: "U4V35"
            string sql = "SELECT [Board_SN],[V1],[V2] FROM [dbo]." + tableName + " WHERE [Board_SN]= '" + boardSN + "'";

            DataSet ds = new DataSet();
            SqlConnection connection = new SqlConnection(connstring);
            SqlDataAdapter dataAdapter = new SqlDataAdapter(sql, connection);
            connection.Open();
            dataAdapter.Fill(ds, tableName);
            connection.Close();
            connection = null;

            if (ds.Tables[0].Rows.Count > 0)
            {
                //vm.memberBoardDatas.Clear();
                vm.BoardTable_Dictionary.Clear();
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    string board_SN, V1_Ratio, V2_Ratio;
                    board_SN = ds.Tables[0].Rows[i]["Board_SN"].ToString().Trim();
                    V1_Ratio = ds.Tables[0].Rows[i]["V1"].ToString().Trim();
                    V2_Ratio = ds.Tables[0].Rows[i]["V2"].ToString().Trim();

                    try
                    {
                        vm.BoardTable_Dictionary.Add(board_SN,
                            new List<string>() { V1_Ratio, V2_Ratio });
                    }
                    catch { vm.Save_Log("Get Board Data", "Add board data to dictionary error", false); }
                }
            }
        }

        private async void Btn_Get_Ref_Click(object sender, RoutedEventArgs e)
        {
            //string folder = @"D:\Ref";
            vm.isStop = false;
            string RefName = string.Format("Ref{0}.txt", 1);
            string RefPath = Path.Combine(@"D:\Ref\", RefName);

            analysis.JudgeAllBoolGauge();

            if (!System.IO.File.Exists(RefPath))
            {
                System.IO.File.AppendAllText(RefPath, "");
            }
            else
            {
                MessageBoxResult msgBoxResult = MessageBox.Show("Cover old ref.txt ?", "Get Ref", MessageBoxButton.YesNoCancel);

                if (msgBoxResult != MessageBoxResult.Cancel)
                {
                    vm.Str_Status = "Get Ref";

                    List<double> list_wl = new List<double>();

                    if (msgBoxResult == MessageBoxResult.Yes)
                    {
                        File.Delete(RefPath);

                        File.AppendAllText(RefPath, "");

                        for (double wl = vm.float_WL_Scan_Start; wl <= vm.float_WL_Scan_End; wl = wl + vm.float_WL_Scan_Gap)
                        {
                            list_wl.Add(wl);
                        }
                    }
                    else if (msgBoxResult == MessageBoxResult.No)
                    {
                        for (double wl = vm.float_WL_Scan_Start; wl <= vm.float_WL_Scan_End; wl = wl + vm.float_WL_Scan_Gap)
                        {
                            for (int ch = 0; ch < vm.ch_count; ch++)
                            {
                                if (vm.BoolAllGauge || vm.list_GaugeModels[ch].boolGauge)
                                    if (!vm.Ref_Dictionaries[ch].ContainsKey(wl))
                                        if (!list_wl.Contains(wl))
                                            list_wl.Add(wl);
                            }
                        }
                    }

                    foreach (double wl in list_wl)
                    {
                        if (vm.isStop) break;

                        vm.tls.SetWL(wl);  //切換TLS WL 
                        vm.pm.SetWL(wl);   //切換PowerMeter WL 

                        vm.Double_Laser_Wavelength = wl;

                        vm.Str_cmd_read = wl.ToString();

                        await Task.Delay(vm.Int_Set_WL_Delay);

                        double IL = 0;

                        if (vm.PD_or_PM)
                        {
                            for (int ch = 0; ch < vm.ch_count; ch++)
                            {
                                if (vm.BoolAllGauge || vm.list_GaugeModels[ch].boolGauge)
                                {
                                    vm.switch_index = ch + 1;
                                    if (vm.station_type.Equals("Hermetic_Test"))
                                    {
                                        RefName = string.Format("Ref{0}.txt", vm.switch_index);
                                        RefPath = Path.Combine(@"D:\Ref\", RefName);

                                        await vm.Port_Switch_ReOpen();
                                        vm.port_Switch.Write(string.Format("I1 {0}", vm.switch_index));
                                    }
                                    IL = await cmd.Get_PM_Value((vm.switch_index - 1));

                                    string msg = string.Format("{0},{1}", wl.ToString(), IL.ToString());

                                    File.AppendAllText(RefPath, msg + "\r");
                                }
                            }
                        }
                        else
                        {
                            await cmd.Get_PD_Value(vm.Selected_Comport);

                            string msg = string.Format("{0},{1}", wl.ToString(), IL.ToString());

                            File.AppendAllText(RefPath, msg + "\r");
                        }


                    }

                    if (!vm.PD_or_PM)
                    {
                        vm.port_PD.DiscardInBuffer();
                        vm.port_PD.DiscardOutBuffer();
                        vm.port_PD.Close();
                    }

                    vm.Str_Status = "Get Ref End";

                    #region Get data from txt file and show
                    _Page_Ref_Grid = new Page_Ref_Grid(vm, txt_path.Text);

                    pageTransitionControl.ShowPage(_Page_Ref_Grid);

                    save_path = txt_path.Text;

                    pageTransitionControl.CurrentPage.Name = "Grid";

                    currentPage = false;
                    #endregion
                }
            }
        }

        private void combox_chSelect_DropDownClosed(object sender, EventArgs e)
        {
            TextBlock txtBK = (TextBlock)combox_chSelect.SelectedItem;
            txt_chSelected.Text = txtBK.Text;
        }
    }
}
