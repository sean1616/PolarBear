using System;
using System.Windows;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Windows.Controls;
using PD.ViewModel;
using ExcelDataReader;
using System.Data;
using System.IO;
using System.Text;

namespace PD.NavigationPages
{
    /// <summary>
    /// Interaction logic for Page_Board_Grid.xaml
    /// </summary>
    public partial class Page_Board_Grid : UserControl
    {
        ComViewModel vm;
        ObservableCollection<Member> memberData = new ObservableCollection<Member>();

        public Page_Board_Grid(ComViewModel vm)
        {
            InitializeComponent();

            DataContext = vm;
            this.vm = vm;

            dataGrid.DataContext = memberData;

            Read_Ref(vm.txt_ref_path);
        }

        public class Member
        {
            public string Board_ID { get; set; }
            public string V_1 { get; set; }
            public string V_2 { get; set; }
            public string V_3 { get; set; }
        }

        private void Read_Ref(string path)
        {
            DataSet ds;
            if (path.Length > 0)
            {
                if (Path.GetExtension(path)==string.Empty) path = path + ".xlsx";

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
                            string data, board_id="", v1 = "", v2 = "", v3 = "";
                            var table = ds.Tables[0];
                            for (int row = 1; row < table.Rows.Count; row++)
                            {
                                vm.List_BoardTable.Add(new List<string>() { "", "", "", "" });
                                for (var col = 0; col < table.Columns.Count; col++)
                                {
                                    data = table.Rows[row][col].ToString();
                                    Console.Write(data + ",");

                                    if (col == 0) vm.List_BoardTable[row - 1][col] = data;
                                    else if (col == 1) vm.List_BoardTable[row - 1][col] = data;
                                    else if (col == 2) vm.List_BoardTable[row - 1][col] = data;
                                    else if (col == 3) vm.List_BoardTable[row - 1][col] = data;
                                    else
                                    {
                                        memberData.Add(new Member()
                                        {
                                            Board_ID = vm.List_BoardTable[row - 1][0],
                                            V_1 = vm.List_BoardTable[row - 1][1],
                                            V_2 = vm.List_BoardTable[row - 1][2],
                                            V_3 = vm.List_BoardTable[row - 1][3]
                                        });
                                    }
                                }                         
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
            Member member = (Member)dataGrid.SelectedItems[0];
            vm.Str_cmd_read = member.Board_ID;
            vm.BoardTable_SelectedBoard = member.Board_ID;
            vm.BoardTable_SelectedIndex = dataGrid.SelectedIndex;
        }
    }
}
