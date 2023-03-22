using System;
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
using PD.Models;

using OxyPlot;
using OxyPlot.Wpf;

using ExcelDataReader;
using ValueGetter;


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
        Page_Chamber_Status _Page_Chamber_Status;
        ObservableCollection<DataPoint> list_datapoint;
        ObservableCollection<ObservableCollection<DataPoint>> list_list_datapoint;
        Analysis analysis;
        ControlCmd cmd;

        //string save_path = "";

        public Page_DataGrid(ComViewModel vm)
        {
            InitializeComponent();

            this.DataContext = vm;
            this.vm = vm;

            analysis = new Analysis(vm);

            cmd = new ControlCmd(vm);

            if(vm.station_type == ComViewModel.StationTypes.BR)
            {
                if (vm.CheckDirectoryExist(vm.txt_BR_Ref_Path))
                    vm.txt_ref_path = vm.txt_BR_Ref_Path;
            }

            //save_path = txt_path.Text;

            _Page_Ref_Grid = new Page_Ref_Grid(vm, txt_path.Text);

            pageTransitionControl.ShowPage(_Page_Ref_Grid);
            pageTransitionControl.CurrentPage = _Page_Ref_Grid;

            if (!vm.isConnected)
            {
                if (vm.Ini_Read("Connection", "Band") == "C Band")
                {
                    vm.Double_Laser_Wavelength = 1523;  //Set wavelength to setup ref value
                    vm.Double_PM_Wavelength = 1523;
                }
                else
                {
                    vm.Double_Laser_Wavelength = 1571;
                    vm.Double_PM_Wavelength = 1571;
                }
            }

            try
            {
                //取得此層資料夾中所有資料夾
                FldArray = Directory.GetDirectories(path);

                combox_path.Items.Clear();

                combox_path.Items.Add(@"D:\Ref");

                foreach (string s in FldArray)
                    combox_path.Items.Add(s);
            }
            catch { }
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

            //save_path = txt_path.Text;

            pageTransitionControl.CurrentPage.Name = "Grid";

            currentPage = false;
        }

        private void Btn_show_ref_chart_Click(object sender, RoutedEventArgs e)
        {
            Calculate_Ref_Chart_DataPoint();
            _Page_Ref_Chart = new Page_Ref_Chart(vm, list_datapoint);

            pageTransitionControl.ShowPage(_Page_Ref_Chart);
            //save_path = txt_path.Text;

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
            //save_path = txt_path.Text;
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
                            string path = Path.Combine(vm.txt_board_table_path, board_id + "-boardtable.txt");

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

                //save_path = txt_path.Text;
            }
        }

        private void btn_save_v_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (pageTransitionControl.CurrentPage.Name != "Chamber_Status")
                {
                    string folder = @"D:\";

                    System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
                    saveFileDialog.Title = "Save Data";

                    var task = Task.Run(() => analysis.CheckDirectoryExist(vm.txt_Chamber_Status_Path));
                    var result = (task.Wait(1500)) ? task.Result : false;

                    saveFileDialog.InitialDirectory = result ? saveFileDialog.InitialDirectory : saveFileDialog.InitialDirectory = @"D:\";

                    saveFileDialog.FileName = "BoardData_" + DateTime.Today.Year + DateTime.Today.Month.ToString("00") + DateTime.Today.Day.ToString("00") + ".csv";
                    saveFileDialog.Filter = "CSV (*.csv)|*.csv|TXT (*.txt)|*.txt|All files (*.*)|*.*";

                    string BoardData_server_filePath = vm.txt_Chamber_Status_Path;  //\\192.168.2.3\shared\SeanWu\PD_Fast_Calibration_Voltage_Monitor
                    string BoardData_filePath = @"D:\BoardData_001.csv";
                    if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        BoardData_filePath = saveFileDialog.FileName;
                        folder = Path.GetDirectoryName(BoardData_filePath);

                        task = Task.Run(() => analysis.CheckDirectoryExist(BoardData_server_filePath));
                        result = (task.Wait(1500)) ? task.Result : false;

                        if (!result)
                        {
                            task = Task.Run(() => analysis.CreateDirectory(BoardData_server_filePath)); //Creat folder on 192 server
                            result = (task.Wait(1500)) ? task.Result : false;

                            if (!result)
                            {
                                vm.Save_Log(new LogMember() { isShowMSG = true, Status = "Save Voltage Data", Message= BoardData_server_filePath, Result = "CreateDirectory Failded" });
                            }
                        }

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

                            task = Task.Run(() => analysis.CheckDirectoryExist(BoardData_server_filePath));
                            result = (task.Wait(1500)) ? task.Result : false;

                            if (result)
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
                else if (pageTransitionControl.CurrentPage.Name == "Chamber_Status")
                {

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

                if(vm.CheckDirectoryExist(vm.txt_BR_Ref_Path))
                    combox_path.Items.Add(vm.txt_BR_Ref_Path);

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
            try
            {
                MessageBoxResult msgBoxResult = MessageBox.Show("Cover old Ref.txt ?", "Get Ref", MessageBoxButton.YesNoCancel);

                if (msgBoxResult == MessageBoxResult.Cancel)
                    return;

                #region Initial setting
                vm.isStop = false;

                analysis.JudgeAllBoolGauge();

                string RefName = String.Empty;
                string RefPath = String.Empty;

                //File name setting
                for (int ch = 0; ch < vm.ch_count; ch++)
                {
                    RefName = $"Ref{ch + 1}.txt";
                    RefPath = Path.Combine(@"D:\Ref\", RefName);

                    if (!File.Exists(RefPath))
                    {
                        string s = Directory.GetParent(RefPath).ToString();
                        if (!analysis.CheckDirectoryExist(s))
                            Directory.CreateDirectory(s);  //Creat ref folder

                        if (analysis.CheckDirectoryExist(s))
                            File.AppendAllText(RefPath, "");  //Creat txt file
                        else
                            return;  //If Ref folder still not exist, return.
                    }
                }

                string savePath = @"D:\";
                if (!vm.CheckDirectoryExist(@"D:"))
                {
                    MessageBox.Show($"D槽不存在，更改路徑為{vm.CurrentPath}");
                    savePath = vm.CurrentPath;
                }

                if (!vm.CheckDirectoryExist(Path.Combine(savePath, @"\Ref\")))
                {
                    savePath = vm.CurrentPath;
                    Directory.CreateDirectory(Path.Combine(savePath, @"\Ref\"));
                }

                vm.Str_Status = "Get Ref";
                vm.dB_or_dBm = false;

                cmd.Clean_Chart();
                #endregion

                //TLS mode
                if (!vm.is_BR_OSA)
                {
                    List<double> list_wl = new List<double>();

                    if (msgBoxResult == MessageBoxResult.Yes)
                    {
                        for (int ch = 0; ch < vm.ch_count; ch++)
                        {
                            RefName = $"Ref{ch + 1}.txt";
                            RefPath = Path.Combine(savePath, @"\Ref\", RefName);

                            if (File.Exists(RefPath))
                            {
                                File.Delete(RefPath);
                                File.AppendAllText(RefPath, "");
                            }
                        }

                        for (double wl = vm.float_WL_Scan_Start; wl <= vm.float_WL_Scan_End; wl = wl + vm.float_WL_Scan_Gap)
                        {
                            list_wl.Add(wl);
                        }

                        vm.Ref_memberDatas.Clear();

                        for (int i = 0; i < vm.Ref_Dictionaries.Count; i++)
                        {
                            vm.Ref_Dictionaries[i].Clear();
                        }
                    }

                    else if (msgBoxResult == MessageBoxResult.No)
                    {
                        //add specific channel ref to ref.txt
                        for (double wl = vm.float_WL_Scan_Start; wl <= vm.float_WL_Scan_End; wl = wl + vm.float_WL_Scan_Gap)
                        {
                            for (int ch = 0; ch < vm.ch_count; ch++)
                            {
                                if (vm.BoolAllGauge || vm.list_GaugeModels[ch].boolGauge)
                                {
                                    if (vm.Ref_Dictionaries[ch].ContainsKey(wl))
                                        continue;

                                    if (!list_wl.Contains(wl))
                                        list_wl.Add(wl);
                                }
                            }
                        }
                    }

                    bool tempBool = vm.Is_FastScan_Mode;
                    vm.Is_FastScan_Mode = false;

                    //Scan Points
                    foreach (double wl in list_wl)
                    {
                        if (vm.isStop) break;

                        await cmd.Set_WL(wl, false);

                        vm.Double_Laser_Wavelength = Math.Round(wl, 2);

                        vm.Str_cmd_read = Math.Round(wl, 2).ToString();

                        await Task.Delay(vm.Int_Set_WL_Delay);

                        double IL = 0;

                        if (!vm.IsDistributedSystem)
                        {
                            //PM mode
                            if (vm.PD_or_PM)
                            {
                                for (int ch = 0; ch < vm.ch_count; ch++)
                                {
                                    if (vm.BoolAllGauge || vm.list_GaugeModels[ch].boolGauge)
                                    {
                                        vm.switch_index = ch + 1;
                                        if (vm.station_type == ComViewModel.StationTypes.Hermetic_Test)
                                        {
                                            RefName = string.Format("Ref{0}.txt", vm.switch_index);
                                            RefPath = Path.Combine(savePath, @"\Ref\", RefName);

                                            await vm.Port_Switch_ReOpen();
                                            vm.port_Switch.Write(string.Format("SW0 {0}", vm.switch_index));
                                        }
                                        else
                                        {
                                            RefName = $"Ref{ch + 1}.txt";
                                            RefPath = Path.Combine(savePath, @"\Ref\", RefName);
                                        }

                                        IL = await cmd.Get_PM_Value((vm.switch_index - 1));

                                        string msg = $"{Math.Round(wl, 2)},{IL}";

                                        File.AppendAllText(RefPath, msg + "\r");

                                        vm.Ref_Dictionaries[ch].Add(Math.Round(wl, 2), IL);

                                        vm.Ref_memberDatas.Add(new RefModel()
                                        {
                                            Wavelength = Math.Round(wl, 2),
                                            Ch_1 = vm.Ref_Dictionaries[0].ContainsKey(Math.Round(wl, 2)) ? vm.Ref_Dictionaries[0][Math.Round(wl, 2)] : 0,
                                            Ch_2 = vm.Ref_Dictionaries[1].ContainsKey(Math.Round(wl, 2)) ? vm.Ref_Dictionaries[1][Math.Round(wl, 2)] : 0,
                                            Ch_3 = vm.Ref_Dictionaries[2].ContainsKey(Math.Round(wl, 2)) ? vm.Ref_Dictionaries[2][Math.Round(wl, 2)] : 0,
                                            Ch_4 = vm.Ref_Dictionaries[3].ContainsKey(Math.Round(wl, 2)) ? vm.Ref_Dictionaries[3][Math.Round(wl, 2)] : 0,
                                            Ch_5 = vm.Ref_Dictionaries[4].ContainsKey(Math.Round(wl, 2)) ? vm.Ref_Dictionaries[4][Math.Round(wl, 2)] : 0,
                                            Ch_6 = vm.Ref_Dictionaries[5].ContainsKey(Math.Round(wl, 2)) ? vm.Ref_Dictionaries[5][Math.Round(wl, 2)] : 0,
                                            Ch_7 = vm.Ref_Dictionaries[6].ContainsKey(Math.Round(wl, 2)) ? vm.Ref_Dictionaries[6][Math.Round(wl, 2)] : 0,
                                            Ch_8 = vm.Ref_Dictionaries[7].ContainsKey(Math.Round(wl, 2)) ? vm.Ref_Dictionaries[7][Math.Round(wl, 2)] : 0,
                                            Ch_9 = vm.Ref_Dictionaries[8].ContainsKey(Math.Round(wl, 2)) ? vm.Ref_Dictionaries[8][Math.Round(wl, 2)] : 0,
                                            Ch_10 = vm.Ref_Dictionaries[9].ContainsKey(Math.Round(wl, 2)) ? vm.Ref_Dictionaries[9][Math.Round(wl, 2)] : 0,
                                            Ch_11 = vm.Ref_Dictionaries[10].ContainsKey(Math.Round(wl, 2)) ? vm.Ref_Dictionaries[10][Math.Round(wl, 2)] : 0,
                                            Ch_12 = vm.Ref_Dictionaries[11].ContainsKey(Math.Round(wl, 2)) ? vm.Ref_Dictionaries[11][Math.Round(wl, 2)] : 0,
                                            Ch_13 = vm.Ref_Dictionaries[12].ContainsKey(Math.Round(wl, 2)) ? vm.Ref_Dictionaries[12][Math.Round(wl, 2)] : 0,
                                            Ch_14 = vm.Ref_Dictionaries[13].ContainsKey(Math.Round(wl, 2)) ? vm.Ref_Dictionaries[13][Math.Round(wl, 2)] : 0,
                                            Ch_15 = vm.Ref_Dictionaries[14].ContainsKey(Math.Round(wl, 2)) ? vm.Ref_Dictionaries[14][Math.Round(wl, 2)] : 0,
                                            Ch_16 = vm.Ref_Dictionaries[15].ContainsKey(Math.Round(wl, 2)) ? vm.Ref_Dictionaries[15][Math.Round(wl, 2)] : 0,
                                        });
                                    }
                                }
                            }
                            //PD mode
                            else
                            {
                                await cmd.Get_PD_Value(vm.Selected_Comport);

                                vm.Ref_memberDatas.Add(new RefModel());
                                vm.Ref_memberDatas.Last().Wavelength = Math.Round(wl, 2);

                                for (int ch = 0; ch < vm.ch_count; ch++)
                                {
                                    if (vm.BoolAllGauge || vm.list_GaugeModels[ch].boolGauge)
                                    {
                                        IL = vm.Double_Powers[ch];
                                        vm.Save_All_PD_Value[ch].Add(new DataPoint(Math.Round(wl, 2), IL));
                                        vm.list_GaugeModels[ch].GaugeValue = IL.ToString();

                                        string msg = $"{wl},{IL}\r";

                                        RefName = $"Ref{ch + 1}.txt";
                                        RefPath = Path.Combine(savePath, @"\Ref\", RefName);

                                        File.AppendAllText(RefPath, msg);  //Add new line to ref file

                                        vm.Ref_Dictionaries[ch].Add(Math.Round(wl, 2), IL);

                                        vm.Ref_memberDatas.Last().Wavelength = Math.Round(wl, 2);   //Add a new data to grid ref

                                        //get all properties and it's values in the last of vm.Ref_memberDatas
                                        var props = vm.Ref_memberDatas.Last().GetPropertiesFromCache();

                                        foreach (var prop in props)
                                        {
                                            //Set value to which property name is match channel now
                                            if (prop.Name == $"Ch_{ch + 1}")
                                            {
                                                prop.SetValue(vm.Ref_memberDatas.Last(), vm.Ref_Dictionaries[0][Math.Round(wl, 2)]);
                                            }
                                        }
                                    }
                                }
                                vm.Chart_All_DataPoints = new List<List<DataPoint>>(vm.Save_All_PD_Value);

                                vm.Chart_DataPoints = new List<DataPoint>(vm.Chart_All_DataPoints[0]);  //A lineseries
                            }
                        }
                        //Distribution system
                        else
                        {
                            for (int ch = 0; ch < vm.ch_count; ch++)
                            {
                                await cmd.Get_Power(ch, true);
                                IL = vm.Double_Powers[ch];
                                File.AppendAllText(RefPath, $"{Math.Round(wl, 2)},{IL}\r");

                                vm.Ref_Dictionaries[ch].Add(Math.Round(wl, 2), IL);

                                vm.Ref_memberDatas.Add(new RefModel()
                                {
                                    Wavelength = Math.Round(wl, 2),
                                    Ch_1 = vm.Ref_Dictionaries[0].ContainsKey(Math.Round(wl, 2)) ? vm.Ref_Dictionaries[0][Math.Round(wl, 2)] : 0,
                                    Ch_2 = vm.Ref_Dictionaries[1].ContainsKey(Math.Round(wl, 2)) ? vm.Ref_Dictionaries[1][Math.Round(wl, 2)] : 0,
                                    Ch_3 = vm.Ref_Dictionaries[2].ContainsKey(Math.Round(wl, 2)) ? vm.Ref_Dictionaries[2][Math.Round(wl, 2)] : 0,
                                    Ch_4 = vm.Ref_Dictionaries[3].ContainsKey(Math.Round(wl, 2)) ? vm.Ref_Dictionaries[3][Math.Round(wl, 2)] : 0,
                                    Ch_5 = vm.Ref_Dictionaries[4].ContainsKey(Math.Round(wl, 2)) ? vm.Ref_Dictionaries[4][Math.Round(wl, 2)] : 0,
                                    Ch_6 = vm.Ref_Dictionaries[5].ContainsKey(Math.Round(wl, 2)) ? vm.Ref_Dictionaries[5][Math.Round(wl, 2)] : 0,
                                    Ch_7 = vm.Ref_Dictionaries[6].ContainsKey(Math.Round(wl, 2)) ? vm.Ref_Dictionaries[6][Math.Round(wl, 2)] : 0,
                                    Ch_8 = vm.Ref_Dictionaries[7].ContainsKey(Math.Round(wl, 2)) ? vm.Ref_Dictionaries[7][Math.Round(wl, 2)] : 0,
                                    Ch_9 = vm.Ref_Dictionaries[8].ContainsKey(Math.Round(wl, 2)) ? vm.Ref_Dictionaries[8][Math.Round(wl, 2)] : 0,
                                    Ch_10 = vm.Ref_Dictionaries[9].ContainsKey(Math.Round(wl, 2)) ? vm.Ref_Dictionaries[9][Math.Round(wl, 2)] : 0,
                                    Ch_11 = vm.Ref_Dictionaries[10].ContainsKey(Math.Round(wl, 2)) ? vm.Ref_Dictionaries[10][Math.Round(wl, 2)] : 0,
                                    Ch_12 = vm.Ref_Dictionaries[11].ContainsKey(Math.Round(wl, 2)) ? vm.Ref_Dictionaries[11][Math.Round(wl, 2)] : 0,
                                    Ch_13 = vm.Ref_Dictionaries[12].ContainsKey(Math.Round(wl, 2)) ? vm.Ref_Dictionaries[12][Math.Round(wl, 2)] : 0,
                                    Ch_14 = vm.Ref_Dictionaries[13].ContainsKey(Math.Round(wl, 2)) ? vm.Ref_Dictionaries[13][Math.Round(wl, 2)] : 0,
                                    Ch_15 = vm.Ref_Dictionaries[14].ContainsKey(Math.Round(wl, 2)) ? vm.Ref_Dictionaries[14][Math.Round(wl, 2)] : 0,
                                    Ch_16 = vm.Ref_Dictionaries[15].ContainsKey(Math.Round(wl, 2)) ? vm.Ref_Dictionaries[15][Math.Round(wl, 2)] : 0,
                                });
                            }
                        }
                    }

                    vm.Is_FastScan_Mode = tempBool;

                    if (!vm.PD_or_PM && !vm.IsDistributedSystem)
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

                    //Close TLS filter port for other control action
                    cmd.Close_TLS_Filter();
                }

                //OSA mode
                else
                {
                    List<DataPoint> list_WL_IL = await Task.Run(() => cmd.OSA_Scan());

                    RefName = $"Ref1.txt";
                    RefPath = Path.Combine(savePath, @"\Ref\", RefName);

                    if (msgBoxResult == MessageBoxResult.Yes)
                    {
                        File.Create(RefPath).Close();

                        StringBuilder sb = new StringBuilder();
                        foreach (DataPoint dp in list_WL_IL)
                            sb.Append($"{dp.X},{dp.Y}\r");

                        File.AppendAllText(RefPath, sb.ToString());
                    }
                    else if (msgBoxResult == MessageBoxResult.No)
                    {
                        StringBuilder sb = new StringBuilder();

                        if (File.Exists(RefPath))
                        {
                            SortedDictionary<string, string> ref_dictionary = new SortedDictionary<string, string>();

                            string[] arrayString = File.ReadAllLines(RefPath);

                            for (int i = 0; i < arrayString.Length; i++)
                            {
                                string[] s = arrayString[i].Split(',');
                                if (s.Length != 2)
                                {
                                    vm.Show_Bear_Window($"{RefName}內容有誤");
                                    return;
                                }

                                if (!ref_dictionary.ContainsKey(s[0]))
                                    ref_dictionary.Add(s[0], s[1]);
                            }

                            foreach (DataPoint dp in list_WL_IL)
                            {
                                if (ref_dictionary.Keys.Contains(dp.X.ToString()))
                                    ref_dictionary.Remove(dp.X.ToString());

                                ref_dictionary.Add(dp.X.ToString(), dp.Y.ToString());
                            }

                            File.Create(RefPath).Close();
                            foreach (KeyValuePair<string, string> kp in ref_dictionary)
                            {
                                sb.Append($"{kp.Key},{kp.Value}\r");
                            }
                        }
                        else  //Ref.txt file is not exist
                        {
                            foreach (DataPoint dp in list_WL_IL)
                                sb.Append($"{dp.X},{dp.Y}\r");
                        }
                        File.AppendAllText(RefPath, sb.ToString());
                    }
                }

                vm.Str_Status = "Get Ref End";

                #region Get data from txt file and show
                _Page_Ref_Grid = new Page_Ref_Grid(vm, txt_path.Text);

                pageTransitionControl.ShowPage(_Page_Ref_Grid);

                //save_path = txt_path.Text;

                pageTransitionControl.CurrentPage.Name = "Grid";

                currentPage = false;
                #endregion  
            }
            catch { }
        }

        private void combox_chSelect_DropDownClosed(object sender, EventArgs e)
        {
            TextBlock txtBK = (TextBlock)combox_chSelect.SelectedItem;
            txt_chSelected.Text = txtBK.Text;
        }

        private void Button_FastC_Status_Click(object sender, RoutedEventArgs e)
        {
            _Page_Chamber_Status = new Page_Chamber_Status(vm);

            pageTransitionControl.ShowPage(_Page_Chamber_Status);

            //save_path = txt_path.Text;

            pageTransitionControl.CurrentPage.Name = "Chamber_Status";


            #region Get Chamber Status Table Data from server

            var task = Task.Run(() => analysis.CheckDirectoryExist(vm.txt_Chamber_Status_Path));
            var result = (task.Wait(1500)) ? task.Result : false;

            if (result)
            {
                string[] list_chamber_directory = Directory.GetDirectories(vm.txt_Chamber_Status_Path);

                if (list_chamber_directory.Length > 0)
                {
                    vm.List_FastCalibration_Status.Clear();
                    vm.List_FastCalibration_Status = new ObservableCollection<FastCalibrationStatusModel>();

                    for (int i = 0; i < list_chamber_directory.Length; i++)
                    {
                        string str = list_chamber_directory[i];

                        vm.List_FastCalibration_Status.Add(new FastCalibrationStatusModel()
                        {
                            station_name = Path.GetFileName(str),
                            station_volt_measurment_directory_path = str,
                            ch_count = 16,
                            station_volt_measurment_log_path_list = Directory.GetFiles(str).ToList()
                        });

                        //Set ch amount
                        vm.List_FastCalibration_Status[i].FailureMode_Models = new ObservableCollection<FastCalibrationStatus_FailureMode_Model>();

                        for (int j = 0; j < vm.List_FastCalibration_Status[i].ch_count; j++)
                        {
                            vm.List_FastCalibration_Status[i].FailureMode_Models.Add(new FastCalibrationStatus_FailureMode_Model() { is_PDFail = false, is_VoltFail = false });
                            vm.List_FastCalibration_Status[i].FailureMode_Models[j].ch_name = (j + 1).ToString();
                        }

                        //lastest file name
                        List<string> list_log_a = new List<string>();
                        List<string> list_log_b = new List<string>();
                        foreach (string path in vm.List_FastCalibration_Status[i].station_volt_measurment_log_path_list)
                        {
                            if (Path.GetFileNameWithoutExtension(path).Last() == 'a' || Path.GetFileNameWithoutExtension(path).Last() == 'A')
                            {
                                list_log_a.Add(path);
                            }
                            else if (Path.GetFileNameWithoutExtension(path).Last() == 'b' || Path.GetFileNameWithoutExtension(path).Last() == 'B')
                            {
                                list_log_b.Add(path);
                            }
                        }
                        vm.List_FastCalibration_Status[i].station_volt_measurment_A_latest = list_log_a.Count > 0 ? list_log_a.Last() : "";
                        vm.List_FastCalibration_Status[i].station_volt_measurment_B_latest = list_log_b.Count > 0 ? list_log_b.Last() : "";

                        //Read CSV file
                        #region Read CSV file
                        List<string> list_UFV_A = new List<string>();
                        List<string> list_UFV_B = new List<string>();

                        //Read A FastPD
                        if (File.Exists(vm.List_FastCalibration_Status[i].station_volt_measurment_A_latest))
                            using (DataTable dtt = CSVFunctions.Read_CSV(vm.List_FastCalibration_Status[i].station_volt_measurment_A_latest))
                            {
                                if (dtt.Rows.Count != 0)
                                {
                                    if (dtt.Columns.Count == 7)
                                        if (dtt.Rows[0][5].ToString().Equals("Delta V"))
                                        {
                                            list_UFV_A = new List<string>();
                                            for (int j = 1; j < dtt.Rows.Count; j++)
                                            {
                                                string boardName = dtt.Rows[j][0].ToString();
                                                if (!list_UFV_A.Contains(boardName)) list_UFV_A.Add(boardName);  //Get all UFV names
                                            }

                                            foreach (string UFVName in list_UFV_A)
                                            {
                                                int index_of_channel = 0;
                                                index_of_channel = list_UFV_A.IndexOf(UFVName);

                                                vm.List_FastCalibration_Status[i].FailureMode_Models[index_of_channel].ch_UFV = UFVName;

                                                for (int k = 1; k < dtt.Rows.Count; k++)
                                                {
                                                    if (dtt.Rows[k][0].ToString() == UFVName)
                                                    {
                                                        if (double.TryParse(dtt.Rows[k][5].ToString(), out double dV))
                                                        {
                                                            if (Math.Abs(dV) >= 0.025)
                                                            {
                                                                vm.List_FastCalibration_Status[i].FailureMode_Models[index_of_channel].is_VoltFail = true;
                                                            }
                                                        }
                                                    }
                                                }
                                            }


                                        }
                                }
                            }

                        //Read B FastPD
                        if (File.Exists(vm.List_FastCalibration_Status[i].station_volt_measurment_B_latest))
                            using (DataTable dtt = CSVFunctions.Read_CSV(vm.List_FastCalibration_Status[i].station_volt_measurment_B_latest))
                            {
                                if (dtt.Rows.Count != 0)
                                {
                                    if (dtt.Columns.Count == 7)
                                        if (dtt.Rows[0][5].ToString().Equals("Delta V"))
                                        {
                                            list_UFV_B = new List<string>();
                                            for (int j = 1; j < dtt.Rows.Count; j++)
                                            {
                                                string boardName = dtt.Rows[j][0].ToString();
                                                if (!list_UFV_B.Contains(boardName)) list_UFV_B.Add(boardName);  //Get all UFV names

                                                list_UFV_B = list_UFV_B.Except(list_UFV_A).ToList();
                                            }

                                            foreach (string UFVName in list_UFV_B)
                                            {
                                                int index_of_channel = 0;
                                                index_of_channel = list_UFV_B.IndexOf(UFVName) + 8; //becase B Fast PD is 8~15

                                                if (vm.List_FastCalibration_Status[i].FailureMode_Models.Count > index_of_channel)
                                                    vm.List_FastCalibration_Status[i].FailureMode_Models[index_of_channel].ch_UFV = UFVName;

                                                for (int k = 1; k < dtt.Rows.Count; k++)
                                                {
                                                    if (dtt.Rows[k][0].ToString() == UFVName)
                                                    {
                                                        if (double.TryParse(dtt.Rows[k][5].ToString(), out double dV))
                                                        {
                                                            if (Math.Abs(dV) >= 0.025)
                                                            {
                                                                if (vm.List_FastCalibration_Status[i].FailureMode_Models.Count > index_of_channel)
                                                                {
                                                                    vm.List_FastCalibration_Status[i].FailureMode_Models[index_of_channel].is_VoltFail = true;
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }


                                        }
                                }
                            }
                        #endregion
                    }
                }
            }
            else
            {
                vm.List_FastCalibration_Status = new ObservableCollection<FastCalibrationStatusModel>()
                {
                    new FastCalibrationStatusModel(),
                    new FastCalibrationStatusModel()
                };
            }




            #endregion
        }
    }
}
