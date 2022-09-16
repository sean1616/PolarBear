using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Data;
using System.Windows;
using ExcelDataReader;
using PD.ViewModel;
using PD.Models;
namespace PD.Functions
{
    public static class CSVFunctions
    {
        public static void Read_Ref_CSV(string path, string pageName, ComViewModel vm)
        {
            DataSet ds;
            if (path.Length > 0)
            {
                if (System.IO.Path.GetExtension(path) == string.Empty) path = path + ".csv";

                //Find Ref.xlsx file
                if (System.IO.File.Exists(path))
                {
                    var extension = System.IO.Path.GetExtension(path).ToLower();
                    using (var stream = new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite))
                    {
                        #region 判斷格式套用讀取方法
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
                        #endregion

                        //顯示已讀取資料
                        using (reader)
                        {

                            ds = reader.AsDataSet(new ExcelDataSetConfiguration()
                            {
                                UseColumnDataType = false,
                                ConfigureDataTable = (tableReader) => new ExcelDataTableConfiguration()
                                {
                                    //設定讀取資料時是否忽略標題(True為不使用Header, False為使用Header)
                                    UseHeaderRow = false
                                }
                            });

                            //Turn DataTable into Dictionary type
                            string data;
                            var table = ds.Tables[0];

                            Dictionary<string, List<string>> CSV_keyValuePair = new Dictionary<string, List<string>>();

                            for (var col = 0; col < table.Columns.Count; col++)
                            {
                                List<string> listValue = new List<string>();

                                if (table.Rows.Count <= 1) return;
                                for (int row = 1; row < table.Rows.Count; row++)
                                {
                                    data = table.Rows[row][col].ToString();
                                    listValue.Add(data);
                                }

                                if (!CSV_keyValuePair.ContainsKey(table.Rows[0][col].ToString()))
                                    CSV_keyValuePair.Add(table.Rows[0][col].ToString(), listValue);
                            }

                            string[] CSV_Header = CSV_keyValuePair.Keys.ToArray();

                            switch (pageName)
                            {
                                case "page_stringList":
                                    vm.list_StringModels.Clear();

                                    for (int i = 0; i < table.Rows.Count - 1; i++)
                                    {
                                        StringModel member = new StringModel();

                                        foreach (string s in CSV_Header)
                                        {
                                            string Header = s;
                                            switch (s.ToUpper())
                                            {
                                                case "NO.":
                                                    if (CSV_keyValuePair.ContainsKey(s))
                                                        member.No = CSV_keyValuePair[s][i];
                                                    break;
                                                case "STRING":
                                                    if (CSV_keyValuePair.ContainsKey(s))
                                                        member.String = CSV_keyValuePair[s][i];
                                                    break;
                                                case "DESCRIPTION":
                                                    if (CSV_keyValuePair.ContainsKey(s))
                                                        member.Description = CSV_keyValuePair[s][i];
                                                    break;
                                            }
                                        }

                                        vm.list_StringModels.Add(member);
                                    }
                                    break;

                                //case "Chamber_Status":
                                //    if (CSV_keyValuePair.Count == 7)
                                //        if (CSV_Header[5].Equals("Delta V"))
                                //        {
                                //            List<string> list_UFV = new List<string>();
                                //            for (int i = 0; i < table.Rows.Count; i++)
                                //            {
                                //                foreach(string boardName in CSV_keyValuePair["Board ID"])
                                //                {
                                //                    if (!list_UFV.Contains(boardName)) list_UFV.Add(boardName);  //Get all UFV names
                                //                }
                                //            }

                                //            foreach(string UFVName in list_UFV)
                                //            {
                                //                //i is channel
                                //                for (int i = 1; i < table.Rows.Count; i++)
                                //                {
                                //                    if(table.Rows[i][0] == UFVName)
                                //                    {
                                //                        vm.List_FastCalibration_Status
                                //                    }
                                //                }
                                //            }
                                           

                                //        }
                                //    break;

                                case "page_commandList":
                                    vm.ComMembers.Clear();

                                    //Change Type: Data Dictionary -> CommandMember type
                                    for (int i = 0; i < table.Rows.Count - 1; i++)
                                    {
                                        ComMember member = new ComMember();

                                        foreach (string s in CSV_Header)
                                        {
                                            string Header = s;
                                            switch (s.ToUpper())
                                            {
                                                case "NO.":
                                                    if (CSV_keyValuePair.ContainsKey(s))
                                                        member.No = CSV_keyValuePair[s][i];
                                                    break;
                                                case "STATUS":
                                                    if (CSV_keyValuePair.ContainsKey(s))
                                                        member.Status = CSV_keyValuePair[s][i];
                                                    break;
                                                case "TYPE":
                                                    if (CSV_keyValuePair.ContainsKey(s))
                                                        member.Type = CSV_keyValuePair[s][i];
                                                    break;
                                                case "COMPORT":
                                                    if (CSV_keyValuePair.ContainsKey(s))
                                                        member.Comport = CSV_keyValuePair[s][i];
                                                    break;
                                                case "CHANNEL":
                                                    if (CSV_keyValuePair.ContainsKey(s))
                                                        member.Channel = CSV_keyValuePair[s][i];
                                                    break;
                                                case "COMMAND":
                                                    if (CSV_keyValuePair.ContainsKey(s))
                                                        member.Command = CSV_keyValuePair[s][i];
                                                    break;
                                                case "VALUE_1":
                                                    if (CSV_keyValuePair.ContainsKey(s))
                                                        member.Value_1 = CSV_keyValuePair[s][i];
                                                    break;
                                                case "VALUE_2":
                                                    if (CSV_keyValuePair.ContainsKey(s))
                                                        member.Value_2 = CSV_keyValuePair[s][i];
                                                    break;
                                                case "VALUE_3":
                                                    if (CSV_keyValuePair.ContainsKey(s))
                                                        member.Value_3 = CSV_keyValuePair[s][i];
                                                    break;
                                                case "VALUE_4":
                                                    if (CSV_keyValuePair.ContainsKey(s))
                                                        member.Value_4 = CSV_keyValuePair[s][i];
                                                    break;
                                                //case "READ":
                                                //    if (CSV_keyValuePair.ContainsKey(s))
                                                //        member.Read = CSV_keyValuePair[s][i];
                                                //    break;
                                                case "DESCRIPTION":
                                                    if (CSV_keyValuePair.ContainsKey(s))
                                                        member.Description = CSV_keyValuePair[s][i];
                                                    break;
                                            }
                                        }

                                        vm.ComMembers.Add(member);
                                    }
                                    break;
                                case "page_log":
                                    break;

                                case "product_wl_setting":
                                    vm.Dictionary_Product_WL_Setting.Clear();
                                    List<string> list_wl_set = new List<string>();

                                    for (int row = 1; row < table.Rows.Count; row++)
                                    {
                                        list_wl_set.Clear();
                                        for (var col = 0; col < table.Columns.Count; col++)
                                        {
                                            data = table.Rows[row][col].ToString();
                                            Console.Write(data + ",");

                                            list_wl_set.Add(data);
                                        }
                                        if (list_wl_set.Count >= 5)
                                            vm.Dictionary_Product_WL_Setting.Add
                                                (
                                                string.Format("{0}_{1}", list_wl_set[0], list_wl_set[1]),
                                                list_wl_set.GetRange(2, 3)
                                                );
                                    }
                                    break;

                                default:
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
                                            vm.Save_Log("Get Board Data", "Data columns less than 4.", false);
                                            continue;
                                        }

                                        try
                                        {
                                            vm.BoardTable_Dictionary.Add(vm.List_BoardTable[i][0],
                                                new List<string>() { vm.List_BoardTable[i][1], vm.List_BoardTable[i][2], vm.List_BoardTable[i][3] });
                                        }
                                        catch { vm.Save_Log("Get Board Data", "Add board data to dictionary error", false); }
                                    }
                                    break;
                            }
                        }
                    }
                }
                else vm.Str_cmd_read = "檔案 " + path + " 不存在!";
            }
            else vm.Str_cmd_read = "沒有提供Path參數!";
        }

        public static DataTable Read_CSV(string path)
        {
            DataSet ds;
            DataTable table = new DataTable();
            if (path.Length > 0)
            {
                if (Path.GetExtension(path) == string.Empty) path = path + ".csv";

                //Find Ref.xlsx file
                if (File.Exists(path))
                {
                    var extension = Path.GetExtension(path).ToLower();
                    using (var stream = new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite))
                    {
                        #region 判斷格式套用讀取方法
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
                        #endregion

                        //顯示已讀取資料
                        using (reader)
                        {
                            ds = reader.AsDataSet(new ExcelDataSetConfiguration()
                            {
                                UseColumnDataType = false,
                                ConfigureDataTable = (tableReader) => new ExcelDataTableConfiguration()
                                {
                                    //設定讀取資料時是否忽略標題(True為不使用Header, False為使用Header)
                                    UseHeaderRow = false
                                }
                            });

                            table = ds.Tables[0]; //Read the first table in the file
                        }
                    }
                }
                //else vm.Str_cmd_read = "檔案 " + path + " 不存在!";
            }
            //else vm.Str_cmd_read = "沒有提供Path參數!";

            return table;
        }

        public static string Creat_New_CSV(string fileName, List<string> list_column_titles)
        {
            try
            {
                string folder = System.Reflection.Assembly.GetEntryAssembly().Location;
                string FileName = $"{fileName}_{DateTime.Today.Year}{DateTime.Today.Month.ToString("00")}{DateTime.Today.Day.ToString("00")}.csv";

                for (int i = 1; i <= 1000; i++)
                {
                    if (File.Exists(Path.Combine(folder, FileName)))
                    {
                        FileName =
                            String.Format("ControlBoard_Table_{0}_{1}.csv",
                            DateTime.Today.Year + DateTime.Today.Month.ToString("00") + DateTime.Today.Day.ToString("00"),
                            i.ToString());
                    }
                    else break;
                }

                System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
                saveFileDialog.Title = "Save Data";
                saveFileDialog.InitialDirectory = folder;
                saveFileDialog.FileName = FileName;
                saveFileDialog.Filter = "CSV (*.csv)|*.csv|TXT (*.txt)|*.txt|All files (*.*)|*.*";

                //string BoardData_filePath = @"D:\" + fileName + @"_001.csv";
                string BoardData_filePath = $@"D:\{fileName}_001.csv";
                if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    BoardData_filePath = saveFileDialog.FileName;
                    folder = Path.GetDirectoryName(BoardData_filePath);

                    string extension = Path.GetExtension(BoardData_filePath);

                    string filepath = "";

                    #region String Builder
                    StringBuilder _stringBuilder = new StringBuilder();

                    //Header Title
                    for (int i = 0; i < list_column_titles.Count; i++)
                    {
                        _stringBuilder.Append(list_column_titles[i]);

                        if (i != list_column_titles.Count - 1)
                            _stringBuilder.Append(",");
                    }

                    _stringBuilder.AppendLine();  //換行                                                         
                    #endregion

                    if (extension.Equals(".csv"))
                    {
                        #region Save as csv

                        filepath = Path.Combine(folder, Path.GetFileNameWithoutExtension(BoardData_filePath)) + ".csv";


                        if (!File.Exists(filepath))
                        {
                            File.AppendAllText(filepath, "");  //Creat a csv file
                        }

                        //Clear file content and lock the file
                        FileStream fileStream = File.Open(filepath, FileMode.Open);
                        fileStream.SetLength(0);
                        fileStream.Close();

                        File.AppendAllText(filepath, _stringBuilder.ToString());  //Save string builder to csv
                        #endregion
                    }

                    else if (extension.Equals(".txt"))
                    {
                        #region Save as txt
                        filepath = Path.Combine(folder, Path.GetFileNameWithoutExtension(BoardData_filePath)) + ".txt";
                        if (File.Exists(filepath))
                            File.Delete(filepath);

                        File.AppendAllText(filepath, _stringBuilder.ToString());  //Save string builder to csv

                        #endregion
                    }

                    return filepath;
                }
                return "";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.StackTrace.ToString());
                return "Creat CSV Failed";

            }
        }

        public static string Creat_New_CSV(string folder_path, string fileName, List<string> list_column_titles, bool isDate, bool isOverWrite)
        {
            try
            {
                string str_date = isDate ? "_" + DateTime.Today.Year + DateTime.Today.Month.ToString("00") + DateTime.Today.Day.ToString("00") : "";
                //string folder = System.Reflection.Assembly.GetEntryAssembly().Location;
                string FileName = $"{fileName}{str_date}.csv";

                if (!isOverWrite)
                    for (int i = 1; i <= 1000; i++)
                    {
                        if (File.Exists(Path.Combine(folder_path, FileName)))
                                FileName = $"{fileName}{str_date}_{i.ToString()}.csv";
                        else break;
                    }

                string filepath = Path.Combine(folder_path, FileName);

                #region String Builder
                StringBuilder _stringBuilder = new StringBuilder();

                //Header Title
                for (int i = 0; i < list_column_titles.Count; i++)
                {
                    _stringBuilder.Append(list_column_titles[i]);

                    if (i != list_column_titles.Count - 1)
                        _stringBuilder.Append(",");
                }

                _stringBuilder.AppendLine();  //換行                                                         
                #endregion

                #region Save as csv

                if (!File.Exists(filepath))
                {
                    File.AppendAllText(filepath, "");  //Creat a csv file
                }
                else
                {
                    MessageBoxResult msgR = MessageBox.Show("File is exist, overwrite ?", "File" , MessageBoxButton.YesNo);
                    if (msgR == MessageBoxResult.No) return "";
                }

                //Clear file content and lock the file
                FileStream fileStream = File.Open(filepath, FileMode.Open);
                fileStream.SetLength(0);
                fileStream.Close();

                File.AppendAllText(filepath, _stringBuilder.ToString());  //Save string builder to csv
                #endregion

                return filepath;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.StackTrace.ToString());
                return "Creat CSV Failed";

            }
        }


        public static void Write_a_row_in_CSV(int columns, string filepath, List<string> list_a_row_datas)
        {
            try
            {
                #region String Builder
                StringBuilder _stringBuilder = new StringBuilder();

                columns = list_a_row_datas.Count;

                //資料內容
                for (int i = 0; i < columns; i++)
                {
                    _stringBuilder.Append(list_a_row_datas[i]);

                    if (i != columns)
                        _stringBuilder.Append(",");
                }
                _stringBuilder.AppendLine();  //換行
                #endregion

                // Save data in csv type
                File.AppendAllText(filepath, _stringBuilder.ToString());  //Save string builder to csv
            }
            catch
            {
            }
        }
    }
}
