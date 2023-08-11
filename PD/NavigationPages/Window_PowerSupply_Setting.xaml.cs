using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using System.Data;
using PD.ViewModel;
using PD.Models;
using PD.Functions;

namespace PD.NavigationPages
{
    /// <summary>
    /// Window_PowerSupply_Setting.xaml 的互動邏輯
    /// </summary>
    public partial class Window_PowerSupply_Setting : Window
    {
        ComViewModel vm;
        ControlCmd cmd;
        AnalysisModel.Analysis analysis;

        public Window_PowerSupply_Setting(ComViewModel vm)
        {
            InitializeComponent();

            this.vm = vm;
            viewbox.DataContext = vm;
            //viewBox_2.DataContext = vm;
            viewer.DataContext = vm;  //將DataContext指給使用者控制項，必要!
            //viewer_2.DataContext = vm;
            sPanel_other_ports.DataContext = vm;
            txt_equipment_setting_Path.DataContext = vm;

            cmd = new ControlCmd(vm);
            analysis = new AnalysisModel.Analysis(vm);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < vm.list_ChannelModels.Count; i++)
            {
                vm.list_ChannelModels[i].Board_Port = vm.list_Board_Setting[i][1];
                vm.list_ChannelModels[i].Board_ID = vm.list_Board_Setting[i][0];
            }
        }

        private void SaveBoardTable()
        {
            #region Save Comport in INI
            try
            {
                for (int ch = 0; ch < vm.ch_count; ch++)
                {
                    string board_id = "Board_ID_" + (ch + 1);
                    vm.Ini_Write("Board_ID", board_id, vm.list_ChannelModels[ch].Board_ID);

                    string board_com = "Board_COM_" + (ch + 1);
                    vm.Ini_Write("Board_Comport", board_com, vm.list_ChannelModels[ch].Board_Port);
                }

                #region Get board calibration data

                var task = Task.Run(() => vm.CheckDirectoryExist(vm.txt_board_table_path));
                var result = task.Wait(1500) ? task.Result : false;

                if (result)
                {
                    vm.list_Board_Setting.Clear();

                    for (int i = 0; i < vm.ch_count; i++)
                    {
                        string Board_ID = "Board_ID_" + (i + 1).ToString();
                        string Board_COM = "Board_COM_" + (i + 1).ToString();
                        vm.list_Board_Setting.Add(new List<string>() { vm.Ini_Read("Board_ID", Board_ID), vm.Ini_Read("Board_Comport", Board_COM) });
                        vm.list_Chart_UI_Models[i].Button_IsChecked = false;
                    }
                    vm.list_Board_Setting = new List<List<string>>(vm.list_Board_Setting);

                    if (!analysis.CheckDirectoryExist(vm.txt_board_table_path))
                    {
                        vm.Str_cmd_read = vm.txt_board_table_path + "\r\n" + " Directory is not exist";
                        return;
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
                        string path = string.Concat(vm.txt_board_table_path, board_id, "-boardtable.txt");

                        if (!File.Exists(path))
                        {
                            vm.Str_cmd_read = "UFV Board table is not exist";
                            vm.Save_Log(new LogMember()
                            {
                                Channel = (k + 1).ToString(),
                                Status = "Save Board Table",
                                Message = "Board Table is not exit",
                                Result = $"{board_id}-boardtable.txt",
                                Date = DateTime.Now.Date.ToShortDateString(),
                                Time = DateTime.Now.ToLongTimeString()
                            });
                            k++;
                            continue;
                        }

                        StreamReader str = new StreamReader(path);

                        //Read board v3 data
                        while (true)  
                        {
                            string readline = str.ReadLine();

                            if (string.IsNullOrEmpty(readline)) break;

                            vm.board_read[k].Add(readline);
                        }
                        str.Close(); //(關閉str)

                        k++;
                    }
                }
                #endregion

                vm.Ini_Write("Connection", "Comport_Switch", vm.Comport_Switch);
                vm.Ini_Write("Connection", "Comport_TLS_Filter", vm.Comport_TLS_Filter);

                vm.Ini_Write("Connection", "COM_PD_A", vm.PD_A_ChannelModel.Board_Port);
                vm.Ini_Write("Connection", "COM_PD_B", vm.PD_B_ChannelModel.Board_Port);

                vm.Ini_Write("Connection", "COM_Golight", vm.Golight_ChannelModel.Board_Port);
            }
            catch (TimeoutException ex)
            {
                MessageBox.Show(ex.StackTrace.ToString());
            }
            #endregion

            #region Save equipment setting table
            try
            {

                List<string> list_titles = new List<string>(){"Channel", "ID", "Comport", "BautRate",
                    "PM_Type", "PM_GPIB_BoardName", "PM_Address", "PM_Slot", "PM_AverageTime", "PM_ID", "PM_Comport", "PM_BautRate", "PM_GetPower_Cmd" };
                //string filePath = CSVFunctions.Creat_New_CSV("Control_Board_Setting", list_titles);
                string filePath = CSVFunctions.Creat_New_CSV(Directory.GetCurrentDirectory(), "Control_Board_Setting", list_titles, true, true);

                if (string.IsNullOrEmpty(filePath))
                {
                    vm.Save_Log(new LogMember() { Message = "Creat CSV file failed." });
                    return;
                }

                for (int i = 0; i < vm.list_ChannelModels.Count; i++)
                {
                    ChannelModel cm = vm.list_ChannelModels[i];
                    List<string> list_datas = new List<string>() {
                        cm.channel,
                        cm.Board_ID,
                        cm.Board_Port,
                        cm.BautRate.ToString(),
                        cm.PM_Type,
                        cm.PM_GPIB_BoardNum.ToString(),
                        cm.PM_Address.ToString(),
                        cm.PM_Slot.ToString(),
                        cm.PM_AveTime.ToString(),
                        cm.PM_Board_ID,
                        cm.PM_Board_Port,
                        cm.PM_BautRate.ToString(),
                        cm.PM_GetPower_CMD};
                    CSVFunctions.Write_a_row_in_CSV(filePath, list_datas);
                }

                vm.txt_Equip_Setting_Path = filePath;
                vm.Ini_Write("Connection", "Equip_Setting_Path", filePath);
                vm.Str_cmd_read = "Board Table Saved";
                vm.Save_Log(new LogMember() { Message = vm.Str_cmd_read, isShowMSG = false });
            }
            catch { }
            #endregion
        }

        private void btn_save_boardtable_Click(object sender, RoutedEventArgs e)
        {
            SaveBoardTable();
        }

        private void ComboBox_DropDownClosed(object sender, EventArgs e)
        {
            ComboBox obj = (ComboBox)sender;
            string type = "";
            if (obj.SelectedIndex == 0)   //GPIB
            {
                type = "GPIB";
                vm.typeGPIBVis = Visibility.Visible;
            }
            else
            {
                type = "RS232";
                vm.typeGPIBVis = Visibility.Collapsed;
            }

            foreach (ChannelModel cm in vm.list_ChannelModels)
            {
                cm.PM_Type = type;
            }
        }

        private void btn_load_boardtable_Click(object sender, RoutedEventArgs e)
        {
            string folder = System.Reflection.Assembly.GetEntryAssembly().Location;

            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Title = "Load equipment setting table";
            dialog.InitialDirectory = folder;
            dialog.FileName = vm.txt_Equip_Setting_Path;
            dialog.Filter = "CSV (*.csv)|*.csv|TXT (*.txt)|*.txt|All files (*.*)|*.*";

            bool? result = dialog.ShowDialog();

            //load channels setting csv file
            if (result == true)
            {
                string filePath = dialog.FileName;
                if (!string.IsNullOrEmpty(filePath))
                {
                    if (File.Exists(filePath))
                    {
                        using (DataTable dtt = CSVFunctions.Read_CSV(filePath))
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
                                            vm.Save_Log(new LogMember() { Message = vm.Str_cmd_read, isShowMSG = true });
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
                                //vm.Str_cmd_read = vm.list_ChannelModels[0].PM_Board_Port;
                            }
                        }
                    }
                    else
                        vm.Save_Log(new LogMember() { Message = "Equip setting file is not exist", isShowMSG = true });
                }
            }

            #region Get Board From Server

            if (vm.station_type == ComViewModel.StationTypes.Hermetic_Test)
            {
                vm.list_Board_Setting.Clear();
                //vm.IsCheck.Clear();
                for (int i = 0; i < vm.ch_count; i++)
                {
                    string Board_ID = "Board_ID_" + (i + 1).ToString();
                    string Board_COM = "Board_COM_" + (i + 1).ToString();
                    vm.list_Board_Setting.Add(new List<string>() { vm.Ini_Read("Board_ID", Board_ID), vm.Ini_Read("Board_Comport", Board_COM) });
                    //vm.IsCheck.Add(false);
                    vm.list_Chart_UI_Models[i].Button_IsChecked = false;
                }
                vm.list_Board_Setting = new List<List<string>>(vm.list_Board_Setting);

                if (!analysis.CheckDirectoryExist(vm.txt_board_table_path))
                {
                    vm.Str_cmd_read = vm.txt_board_table_path + "\r\n" + " Directory is not exist";
                    return;
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
                    string path = string.Concat(vm.txt_board_table_path, board_id, "-boardtable.txt");

                    if (!File.Exists(path))
                    {
                        vm.Str_cmd_read = "UFV Board table is not exist";
                        vm.Save_Log(new LogMember()
                        {
                            Channel = (k + 1).ToString(),
                            Status = "Load Board Table",
                            Message = "Board Table is not exit",
                            Result = $"{board_id}-boardtable.txt",
                            Date = DateTime.Now.Date.ToShortDateString(),
                            Time = DateTime.Now.ToLongTimeString()
                        });
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

            for (int i = 0; i < vm.list_ChannelModels.Count; i++)
            {
                vm.list_ChannelModels[i].Board_Port = vm.list_Board_Setting[i][1];
                vm.list_ChannelModels[i].Board_ID = vm.list_Board_Setting[i][0];
            }

            #endregion

            vm.Str_cmd_read = "BoardTable loaded from server";
            vm.Save_Log(new LogMember() { Message = vm.Str_cmd_read, isShowMSG = false });
        }

        private async void btn_Board_ID_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < vm.list_ChannelModels.Count; i++)
            {
                if (!string.IsNullOrEmpty(vm.list_ChannelModels[i].Board_Port))
                    vm.list_ChannelModels[i].Board_ID = await cmd.Get_Board_ID(vm.list_ChannelModels[i].Board_Port, i + 1);
            }
        }

        private void itms_gauges_Loaded(object sender, RoutedEventArgs e)
        {
            string s = vm.list_ChannelModels[0].PM_GetPower_CMD;
        }
    }

    public class VisValueConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((Visibility)value == Visibility.Visible)
            {
                return Visibility.Collapsed;
            }
            else
            {
                return Visibility.Visible;
            }
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
