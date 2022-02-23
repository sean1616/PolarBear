using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Reflection;
using System.Data;
using PD.ViewModel;
using PD.NavigationPages;
using PD.Models;
using PD.Functions;
using PD.AnalysisModel;
using Microsoft.Win32;
using ExcelDataReader;
using OxyPlot;

namespace PD.NavigationPages
{
    /// <summary>
    /// Interaction logic for Page_Log_Command.xaml
    /// </summary>
    public partial class Page_Log_Command : UserControl
    {
        ComViewModel vm;
        ControlCmd cmd;

        Page_Log _Page_Log;
        //Page_Command _Page_Command;
        Page_StringList _Page_StringList;
        Page_Variable _page_Variable;

        //string page_name = "";

        public Page_Log_Command(ComViewModel vm)
        {
            InitializeComponent();

            this.vm = vm;

            cmd = new ControlCmd(vm);

            if (vm._Page_Command == null)
                vm._Page_Command = new Page_Command(vm);
            _page_Variable = new Page_Variable(vm);

            vm.LastScript_Path = vm.Ini_Read("CommandList", "LastScript_Path");

            if (!string.IsNullOrEmpty(vm.LastScript_Path))
            {
                vm.list_scriptModels.Add(new ScriptModel()
                { ScriptName = Path.GetFileNameWithoutExtension(vm.LastScript_Path), ScriptPath = vm.LastScript_Path });
                CSVFunctions.Read_Ref_CSV(vm.LastScript_Path, "page_commandList", vm);

                for (int i = 0; i < vm.ComMembers.Count; i++)
                {
                    vm.ComMembers[i].No = i.ToString();
                }
            }            
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {                        
            
            

            if (!vm.isPageLogorCommand)
            {
                grid_cmdList_function.Visibility = Visibility.Collapsed;
                pageTransitionControl.ShowPage(_Page_Log);
            }
            else
            {
                grid_cmdList_function.Visibility = Visibility.Visible;

                switch (vm.pageName_LogCmd)
                {
                    case "page_log":
                        if (_Page_Log == null)
                            _Page_Log = new Page_Log(vm);
                        pageTransitionControl.ShowPage(_Page_Log);
                        break;

                    case "page_commandList":
                        if (vm._Page_Command == null)
                            vm._Page_Command = new Page_Command(vm);
                        pageTransitionControl.ShowPage(vm._Page_Command);
                        break;

                    case "page_Variable":
                        if (_page_Variable == null)
                            _page_Variable = new Page_Variable(vm);
                        pageTransitionControl.ShowPage(_page_Variable);
                        break;

                    case "page_stringList":
                        if (_Page_StringList == null)
                            _Page_StringList = new Page_StringList(vm);
                        pageTransitionControl.ShowPage(_page_Variable);
                        break;

                    default:
                        if (vm._Page_Command == null)
                            vm._Page_Command = new Page_Command(vm);
                        pageTransitionControl.ShowPage(vm._Page_Command);
                        break;
                }
                
            }

            

            //page_name = pageTransitionControl.CurrentPage.Name;
        }

        private void Button_Page_Log_Click(object sender, RoutedEventArgs e)
        {
            if (_Page_Log == null)
                _Page_Log = new Page_Log(vm);

            if (vm.pageName_LogCmd == "page_log") return;

            grid_cmdList_function.Visibility = Visibility.Collapsed;

            pageTransitionControl.ShowPage(_Page_Log);

            vm.isPageLogorCommand = false;

            vm.pageName_LogCmd = "page_log";
        }

        private void Button_Page_Command_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (vm._Page_Command == null)
                    vm._Page_Command = new Page_Command(vm);

                if (vm.pageName_LogCmd == "page_commandList") return;

                grid_cmdList_function.Visibility = Visibility.Visible;

                pageTransitionControl.ShowPage(vm._Page_Command);

                vm.isPageLogorCommand = true;

                vm.pageName_LogCmd = vm._Page_Command.Name;
            }
            catch { }
        }

        private void Button_Page_StringList_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_Page_StringList == null)
                    _Page_StringList = new Page_StringList(vm);

                if (grid_cmdList_function.Visibility == Visibility.Collapsed)
                    grid_cmdList_function.Visibility = Visibility.Visible;

                pageTransitionControl.ShowPage(_Page_StringList);

                vm.isPageLogorCommand = true;

                vm.pageName_LogCmd = _Page_StringList.Name;
            }
            catch { }
        }

        private void btn_clear_Click(object sender, RoutedEventArgs e)
        {
            if (!vm.isPageLogorCommand)
                vm.LogMembers.Clear();
            else if (vm.isPageLogorCommand && !vm.IsGoOn)
                vm.ComMembers.Clear();
        }

        private void btn_Go_Click(object sender, RoutedEventArgs e)
        {
            if (vm.lastCMD.Command != null)
            {
                if (vm.lastCMD.Command.Equals("PAUSE") || vm.lastCMD.Command.Equals("MSG"))
                {
                    vm.lastCMD.YN = false;
                    return;
                }
            }
            
            //Set all command's No.
            for (int i = 0; i < vm.ComMembers.Count; i++)
            {
                vm.ComMembers[i].No = i.ToString();
            }
            vm._Page_Command.dataGrid.Items.Refresh();

            btn_Go.IsEnabled = false;

            cmd.CommandList_Start(0, true);

            btn_Go.IsEnabled = true;
        }
                
        private void btn_load_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                if (string.IsNullOrEmpty(openFileDialog.FileName)) return;

                vm.list_scriptModels.Clear();
                vm.list_scriptModels.Add(new ScriptModel() { ScriptName = Path.GetFileNameWithoutExtension(openFileDialog.FileName), ScriptPath = openFileDialog.FileName });
                CSVFunctions.Read_Ref_CSV(openFileDialog.FileName, "page_commandList", vm);

                switch (vm.pageName_LogCmd)
                {                  
                    case "page_log":
                        return;

                    default:  //Page Command List
                        for (int i = 0; i < vm.ComMembers.Count; i++)
                        {
                            vm.ComMembers[i].No = i.ToString();
                        }
                        vm._Page_Command.dataGrid.Items.Refresh();
                        vm.Ini_Write("CommandList", "LastScript_Path", openFileDialog.FileName);
                        break;
                }
            }                
        }

        private void Read_Ref_CSV(string path)
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
                                {
                                    CSV_keyValuePair.Add(table.Rows[0][col].ToString(), listValue);
                                }
                                
                            }

                            string[] CSV_Header = CSV_keyValuePair.Keys.ToArray();

                            switch (vm.pageName_LogCmd)
                            {
                                case "page_stringList":
                                    vm.list_StringModels.Clear();

                                    //Turn Data Dictionary into CommandMember type
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
                                case "page_commandList":
                                    vm.ComMembers.Clear();

                                    //if (table.Rows.Count > 0)
                                    //{
                                    //    vm.list_scriptModels.Clear();
                                    //    vm.list_scriptModels.Add(new ScriptModel() { ScriptName = Path.GetFileNameWithoutExtension(path), ScriptPath = path });
                                    //}

                                    //Turn Data Dictionary into CommandMember type
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
                                                        member.Command = CSV_keyValuePair[s][i];
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
                                    return;
                            }
                        }
                    }
                }
                else vm.Str_cmd_read = "檔案 " + path + " 不存在!";
            }
            else vm.Str_cmd_read = "沒有提供參數!";
        }

        private void btn_save_Click(object sender, RoutedEventArgs e)
        {
            string page_name = pageTransitionControl.CurrentPage.Name;

            #region Data Setting  

            StringBuilder _stringBuilder = new StringBuilder();

            object members = new object();

            switch (page_name)
            {
                case "page_stringList":
                    members = new StringModel();
                    
                    //StringMember strMember = new StringMember();
                    //props = strMember.GetType().GetProperties();
                    break;
                case "page_commandList":
                    members = new ComMember();
                    break;
                case "page_log":
                    return;
            }

            PropertyInfo[] props = members.GetType().GetProperties();

            //Header Title
            for (int i = 0; i < props.Length; i++)
            {
                if (props[i].Name != "YN")
                {
                    _stringBuilder.Append(props[i].Name);

                    if (i != props.Length - 1)
                    {
                        _stringBuilder.Append(",");
                    }
                }
            }
            _stringBuilder.AppendLine();  //換行

            int max_dataCount = 0;
            switch (page_name)
            {
                case "page_stringList":
                    max_dataCount = vm.list_StringModels.Count;  //計算資料行數

                    //資料內容
                    for (int i = 0; i < max_dataCount; i++)
                    {
                        foreach (var prop in props)
                        {
                            if (prop.Name != "YN")
                            {
                                if (prop.GetValue(vm.list_StringModels[i]) != null)
                                {
                                    string value = prop.GetValue(vm.list_StringModels[i]).ToString();
                                    _stringBuilder.Append(value);
                                    _stringBuilder.Append(",");
                                }
                            }
                        }

                        _stringBuilder.AppendLine();  //換行
                    }
                    break;
                case "page_commandList":
                    max_dataCount = vm.ComMembers.Count;  //計算資料行數

                    //資料內容
                    for (int i = 0; i < max_dataCount; i++)
                    {
                        foreach (var prop in props)
                        {
                            if (prop.Name != "YN")
                            {
                                if (prop.GetValue(vm.ComMembers[i]) != null)
                                {
                                    string value = prop.GetValue(vm.ComMembers[i]).ToString();
                                    _stringBuilder.Append(value);
                                    _stringBuilder.Append(",");
                                }
                                else
                                {
                                    _stringBuilder.Append(",");
                                }
                            }
                        }

                        _stringBuilder.AppendLine();  //換行
                    }
                    break;                
            }           
            #endregion

            #region CSV Setting
            //string folder = @"C:\";

            string folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();            
            saveFileDialog.InitialDirectory = folder;            
            saveFileDialog.Filter = "CSV (*.csv)|*.csv|All files (*.*)|*.*";
            string BoardData_filePath = "";

            switch (page_name)
            {
                case "page_stringList":
                    saveFileDialog.Title = "Save String List";
                    saveFileDialog.FileName = "StringList_001.csv";
                    BoardData_filePath = Path.Combine(folder, saveFileDialog.FileName);
                    break;
                case "page_commandList":
                    saveFileDialog.Title = "Save Command List";
                    saveFileDialog.FileName = "CommandList_001.csv";
                    BoardData_filePath = Path.Combine(folder, saveFileDialog.FileName);
                    //BoardData_filePath = @"D:\CommandList_001.csv";
                    break;
                case "page_log":
                    return;
            }

            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                BoardData_filePath = saveFileDialog.FileName;

                if (string.IsNullOrEmpty(BoardData_filePath)) return;
            }
            else return;

            string csvpath = BoardData_filePath;

            if (!System.IO.File.Exists(csvpath))
            {
                System.IO.File.AppendAllText(csvpath, "");  //Creat a csv file
            }

            //Clear file content and lock the file
            try
            {
                System.IO.FileStream fileStream = System.IO.File.Open(csvpath, System.IO.FileMode.Open);
                fileStream.SetLength(0);
                fileStream.Close();
            }
            catch
            {
                vm.Save_Log("Open CSV file.", "File has been used, unable to open it", true);
                return;
            }
            #endregion

            System.IO.File.AppendAllText(csvpath, _stringBuilder.ToString());  //Save string builder to csv

            Process.Start(csvpath);
        }              

        private void btn_add_Click(object sender, RoutedEventArgs e)
        {
            if (vm.CmdSelected_Index >= 0)
            {
                switch (vm.pageName_LogCmd)
                {
                    case "page_stringList":
                        StringModel stringMember = new StringModel();
                        stringMember.No = vm.CmdSelected_Index.ToString();
                        vm.list_StringModels.Insert(vm.CmdSelected_Index, stringMember);

                        for (int i = 0; i < vm.list_StringModels.Count; i++)
                        {
                            vm.list_StringModels[i].No = i.ToString();
                        }
                        _Page_StringList.dataGrid.Items.Refresh();
                        break;

                    case "page_commandList":
                        ComMember comMember = new ComMember();
                        comMember.YN = true;
                        comMember.No = vm.CmdSelected_Index.ToString();
                        vm.ComMembers.Insert(vm.CmdSelected_Index, comMember);

                        for (int i = 0; i < vm.ComMembers.Count; i++)
                        {
                            vm.ComMembers[i].No = i.ToString();
                        }
                        vm._Page_Command.dataGrid.Items.Refresh();
                        break;
                    case "page_log":
                        return;
                }
            }               
        }

        private void btn_remove_Click(object sender, RoutedEventArgs e)
        {
            if (vm.CmdSelected_Index >= 0)
            {
                switch (vm.pageName_LogCmd)
                {
                    case "page_stringList":
                        vm.list_StringModels.RemoveAt(vm.CmdSelected_Index);
                        for (int i = 0; i < vm.list_StringModels.Count; i++)
                        {
                            vm.list_StringModels[i].No = i.ToString();
                        }
                        _Page_StringList.dataGrid.Items.Refresh();
                        break;

                    case "page_commandList":
                        vm.ComMembers.RemoveAt(vm.CmdSelected_Index);
                        for (int i = 0; i < vm.ComMembers.Count; i++)
                        {
                            vm.ComMembers[i].No = i.ToString();
                        }
                        vm._Page_Command.dataGrid.Items.Refresh();
                        break;
                    case "page_log":
                        return;
                }
            }
        }

        private void btn_refresh_Click(object sender, RoutedEventArgs e)
        {
            switch (vm.pageName_LogCmd)
            {
                case "page_stringList":
                    for (int i = 0; i < vm.list_StringModels.Count; i++)
                    {
                        vm.list_StringModels[i].No = i.ToString();
                    }
                    _Page_StringList.dataGrid.Items.Refresh();
                    break;

                case "page_commandList":                    
                    for (int i = 0; i < vm.ComMembers.Count; i++)
                    {
                        vm.ComMembers[i].No = i.ToString();
                    }
                    vm._Page_Command.dataGrid.Items.Refresh();
                    break;
                case "page_log":
                    return;
            }           
        }

        //ComMember member_copy = new ComMember();
        private void btn_copy_Click(object sender, RoutedEventArgs e)
        {
            if (vm.CmdSelected_Index >= 0)
            {
                switch (vm.pageName_LogCmd)
                {
                    case "page_stringList":
                        _Page_StringList.member_copy = new StringModel((StringModel)_Page_StringList.dataGrid.SelectedItem);
                        break;

                    case "page_commandList":
                        vm._Page_Command.list_memberCopy.Clear();
                        foreach(ComMember cm in vm._Page_Command.dataGrid.SelectedItems)
                        {
                            vm._Page_Command.list_memberCopy.Add(cm);
                        }
                        //vm._Page_Command.list_memberCopy = new ComMember((ComMember)vm._Page_Command.dataGrid.SelectedItem);
                        break;
                    case "page_log":
                        return;
                }                                        
            }
        }

        private void btn_paste_Click(object sender, RoutedEventArgs e)
        {
            if (vm.CmdSelected_Index >= 0)
            {
                switch (vm.pageName_LogCmd)
                {
                    case "page_stringList":
                        vm.list_StringModels.Insert(vm.CmdSelected_Index, new StringModel(_Page_StringList.member_copy));
                        break;

                    case "page_commandList":
                        //vm._Page_Command.list_memberCopy.Clear();
                        vm._Page_Command.list_memberCopy.Reverse();
                        foreach (ComMember cm in vm._Page_Command.list_memberCopy)
                        {
                            vm.ComMembers.Insert(vm.CmdSelected_Index, cm);
                        }
                        vm._Page_Command.list_memberCopy.Reverse();
                        //vm.ComMembers.Insert(vm.CmdSelected_Index, new ComMember(vm._Page_Command.member_copy));
                        break;
                    case "page_log":
                        return;
                }
            }
                     
        }

        private void btn_Test_Click(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show(vm.ComMembers[0].Command);

            vm.CmdSelected_Index++;
            vm._Page_Command.dataGrid.SelectedIndex=vm.CmdSelected_Index;
        }

        private void btn_page_variable_Click(object sender, RoutedEventArgs e)
        {            
            try
            {
                if (_page_Variable == null)
                    _page_Variable = new Page_Variable(vm);

                if (vm.pageName_LogCmd == "page_Variable") return;

                if (grid_cmdList_function.Visibility == Visibility.Collapsed)
                    grid_cmdList_function.Visibility = Visibility.Visible;

                pageTransitionControl.ShowPage(_page_Variable);

                vm.isPageLogorCommand = true;

                vm.pageName_LogCmd = _page_Variable.Name;


               
            }
            catch { }
        }

        private void btn_lastScript_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btn_LastScript_Click_1(object sender, RoutedEventArgs e)
        {
            vm.list_scriptModels.Clear();
            vm.list_scriptModels.Add(new ScriptModel() { ScriptName = Path.GetFileNameWithoutExtension(vm.LastScript_Path), ScriptPath = vm.LastScript_Path });
            CSVFunctions.Read_Ref_CSV(vm.LastScript_Path, "page_commandList", vm);

            for (int i = 0; i < vm.ComMembers.Count; i++)
            {
                vm.ComMembers[i].No = i.ToString();
            }
            vm._Page_Command.dataGrid.Items.Refresh();

            vm.Ini_Write("CommandList", "LastScript_Path", vm.LastScript_Path);
        }
    }
}
