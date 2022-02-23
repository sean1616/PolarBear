using System;
using System.Data.SqlClient;
using System.Windows;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Windows.Controls;
using PD.ViewModel;
using ExcelDataReader;
using System.Data;
using System.IO;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Media;

namespace PD.NavigationPages
{   
    /// <summary>
    /// Interaction logic for Page_Board_Grid.xaml
    /// </summary>
    public partial class Page_Board_Grid : UserControl
    {
        ComViewModel vm;
        ObservableCollection<Member> memberData = new ObservableCollection<Member>();

        //static double V_threshold = 0.02;

        public Page_Board_Grid(ComViewModel vm)
        {
            InitializeComponent();

            DataContext = vm;
            this.vm = vm;

            dataBoardGrid.DataContext = vm.memberBoardDatas;
                       
            //Get_Board_Ratio();  //Get Board ratio data from SQL
            
            //Read_Ref_CSV(vm.txt_ref_path);
        }

        public class Member
        {
            public string Board_ID { get; set; }
            public int V_123 { get; set; }
            public string DAC { get; set; }
            public string Vb { get; set; }
            public string Va { get; set; }
            public string delta_V { get; set; }
        }

        public string connstring = "User ID=" + "opticomm_pe" + ";" +
                                         "Password=" + "opticomm_pe!@#456" + ";" +
                                         "Trusted_Connection=false;" +
                                         "Server=" + "OPTICOMM-MFG" + ";" +
                                         "Data Source=" + "192.168.8.200" + ";" +
                                         "Initial Catalog=" + "UFA" + ";Pooling=false;Connection Timeout=90";  //"UFA", "CTF"為不同的資料庫

        //private string Get_Board_Ratio()
        //{
        //    string value = string.Empty;

        //    string tableName = "Board_V";
        //    string boardSN = "U4V35";   //板號
        //    string sql = "SELECT [Board_SN],[V1],[V2] FROM [dbo]." + tableName + " WHERE [Board_SN]= '" + boardSN + "'";

        //    DataSet ds = new DataSet();
        //    SqlConnection connection = new SqlConnection(connstring);
        //    SqlDataAdapter dataAdapter = new SqlDataAdapter(sql, connection);
        //    connection.Open();
        //    dataAdapter.Fill(ds, tableName);
        //    connection.Close();
        //    connection = null;

        //    if (ds.Tables[0].Rows.Count > 0)
        //    {
        //        vm.memberBoardDatas.Clear();
        //        vm.BoardTable_Dictionary.Clear();
        //        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
        //        {
        //            string board_SN, V1_Ratio, V2_Ratio;
        //            board_SN = ds.Tables[0].Rows[i]["Board_SN"].ToString().Trim();
        //            V1_Ratio = ds.Tables[0].Rows[i]["V1"].ToString().Trim();
        //            V2_Ratio = ds.Tables[0].Rows[i]["V2"].ToString().Trim();

        //            Models.BoardTable_Members boardTable_Members = new Models.BoardTable_Members();
        //            boardTable_Members.Board_ID = board_SN;
        //            boardTable_Members.V_123 = "1";
        //            boardTable_Members.Vb = V1_Ratio;
        //            vm.memberBoardDatas.Add(boardTable_Members);

        //            try
        //            {
        //                vm.BoardTable_Dictionary.Add(board_SN,
        //                    new List<string>() { V1_Ratio, V2_Ratio });
        //            }
        //            catch { vm.Save_Log("Get Board Data", "Add board data to dictionary error.", false); }
        //        }
        //    }

        //    return value;
        //}

        private void Read_Ref_CSV(string path)
        {
            DataSet ds;
            if (path.Length > 0)
            {                
                if (Path.GetExtension(path)==string.Empty) path = path + ".xlsx";

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

                                    if (col == 0) vm.List_BoardTable[row ][col] = data;  //board ID
                                    else if (col == 1) vm.List_BoardTable[row ][col] = data;  //V1
                                    else if (col == 2) vm.List_BoardTable[row ][col] = data;  //V2
                                    else if (col == 3) vm.List_BoardTable[row ][col] = data;  //V3                            
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
       
        private void dataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            //Member member = (Member)dataGrid.SelectedItems[0];
            //vm.Str_cmd_read = member.Board_ID;
            //vm.BoardTable_SelectedBoard = member.Board_ID;
            //vm.BoardTable_SelectedIndex = dataGrid.SelectedIndex;
        }       
    }

    public class ValueConverter : IValueConverter
    {        
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return Brushes.Red;

            double d = 0;
            if(!double.TryParse(value.ToString(), out d)) return Brushes.Red;

            double num = Math.Abs(double.Parse(value.ToString()));
            if (num < 0.015)   
            {
                return Brushes.DarkGreen;
            }
            else if (num > 0.02)   //Delta Voltage Threshold
            {
                return Brushes.Red;
            }

            return Brushes.Brown;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }       
    }
}
