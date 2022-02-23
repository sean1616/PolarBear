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

        public Window_PowerSupply_Setting(ComViewModel vm)
        {
            InitializeComponent();

            this.vm = vm;
            viewbox.DataContext = vm;
            //viewBox_2.DataContext = vm;
            viewer.DataContext = vm;  //將DataContext指給使用者控制項，必要!
            //viewer_2.DataContext = vm;
            sPanel_other_ports.DataContext = vm;

            cmd = new ControlCmd(vm);
        }                

        private void btn_save_boardtable_Click(object sender, RoutedEventArgs e)
        {
            if (!vm.IsDistributedSystem)
            {
                try
                {
                    for (int ch = 0; ch < vm.ch_count; ch++)
                    {
                        string board_id = "Board_ID_" + (ch + 1);
                        vm.Ini_Write("Board_ID", board_id, vm.list_ChannelModels[ch].Board_ID);

                        string board_com = "Board_COM_" + (ch + 1);
                        vm.Ini_Write("Board_Comport", board_com, vm.list_ChannelModels[ch].Board_Port);
                    }

                    #region Board Setting
                    vm.txt_board_table_path = @"\\192.168.2.3\tff\Data\BoardCalibration\UFA\";

                    if (vm.station_type.Equals("Hermetic_Test"))
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

                    #endregion

                    vm.Ini_Write("Connection", "Comport_Switch", vm.Comport_Switch);
                    vm.Ini_Write("Connection", "Comport_TLS_Filter", vm.Comport_TLS_Filter);

                    vm.Ini_Write("Connection", "COM_PD_A", vm.PD_A_ChannelModel.Board_Port);
                    vm.Ini_Write("Connection", "COM_PD_B", vm.PD_B_ChannelModel.Board_Port);
                
                    vm.Ini_Write("Connection", "COM_Golight", vm.Golight_ChannelModel.Board_Port);

                    vm.Str_cmd_read = "Board Table Saved";

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.StackTrace.ToString());
                }
            }
            else  //distribution system
            {
                List<string> list_titles = new List<string>(){"Channel", "ID", "Comport", "BautRate",
                    "PM_Type", "PM_Board", "PM_Address", "PM_Slot", "PM_AverageTime", "PM_ID", "PM_Comport", "PM_BautRate", "PM_GetPower_Cmd" };
                string filePath = CSVFunctions.Creat_New_CSV("Control_Board_Setting", list_titles);

                if (string.IsNullOrEmpty(filePath))
                {
                    vm.Save_Log(new LogMember() { Message = "Creat CSV file failed." });
                    return;
                }

                for (int i = 0; i < vm.list_ChannelModels.Count; i++)
                {
                    ChannelModel cm = vm.list_ChannelModels[i];
                    List<string> list_datas = new List<string>() { cm.channel,
                        cm.Board_ID,
                        cm.Board_Port,
                        cm.BautRate.ToString(),
                        cm.PM_Type, cm.PM_Board_Port,
                        cm.PM_Address.ToString(),
                        cm.PM_Slot.ToString(),
                        cm.PM_AveTime.ToString(),
                        cm.PM_Board_ID,
                        cm.PM_Board_Port,
                        cm.PM_BautRate.ToString(),
                        cm.PM_GetPower_CMD};
                    CSVFunctions.Write_a_row_in_CSV(list_titles.Count, filePath, list_datas);
                }
            }
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < vm.list_ChannelModels.Count; i++)
            {
                vm.list_ChannelModels[i].Board_Port = vm.list_Board_Setting[i][1];
                vm.list_ChannelModels[i].Board_ID = vm.list_Board_Setting[i][0];
                //vm.list_GaugeModels[i].Board_ID = vm.list_ChannelModels[i].Board_ID;
            }

            vm.Comport_Switch = vm.Ini_Read("Connection", "Comport_Switch");
            vm.Comport_TLS_Filter = vm.Ini_Read("Connection", "Comport_TLS_Filter");
        }

        private void btn_load_boardtable_Click(object sender, RoutedEventArgs e)
        {
            #region Board Setting
            vm.txt_board_table_path = @"\\192.168.2.3\tff\Data\BoardCalibration\UFA\";

            if (vm.station_type.Equals("Hermetic_Test"))
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

            for (int i = 0; i < vm.list_ChannelModels.Count; i++)
            {
                vm.list_ChannelModels[i].Board_Port = vm.list_Board_Setting[i][1];
                vm.list_ChannelModels[i].Board_ID = vm.list_Board_Setting[i][0];
            }

            #endregion

            vm.Str_cmd_read = "Board Table Loaded";
        }

        //DiCon.UCB.Communication.ICommunication icomm;
        //DiCon.UCB.Communication.RS232.RS232 rs232;
        //DiCon.UCB.MTF.IMTFCommand tf;
        private async void btn_Board_ID_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < vm.list_ChannelModels.Count; i++)
            {
                vm.list_ChannelModels[i].Board_ID = await cmd.Get_Board_ID(vm.list_ChannelModels[i].Board_Port, i+1);
            }
            
            //try
            //{
            //    await vm.Port_ReOpen(vm.Selected_Comport);

            //    vm.Str_Command = "ID?";
            //    await cmd.Cmd_Write_RecieveData(vm.Str_Command, true, 0);

            //    #region Get Board Name     
            //    if (vm.Str_cmd_read.Equals("DiCon Fiberoptics Inc, MEMS UFA"))
            //    {
            //        try
            //        {
            //            rs232 = new DiCon.UCB.Communication.RS232.RS232(vm.Selected_Comport);
            //            rs232.OpenPort();
            //            icomm = (DiCon.UCB.Communication.ICommunication)rs232;

            //            tf = new DiCon.UCB.MTF.RS232.RS232(icomm);

            //            string str_ID = string.Empty;

            //            str_ID = tf.ReadSN();
            //            vm.Str_Status = str_ID;

            //            await Task.Delay(125);
            //            rs232.ClosePort();
            //        }
            //        catch { }
            //    }
            //    #endregion

            //    vm.Save_Log(new LogMember()
            //    {
            //        Status = vm.Str_Status,
            //        Message = vm.Str_cmd_read
            //    });
            //}catch(Exception ex)
            //{
            //    MessageBox.Show(ex.StackTrace.ToString());
            //}
            //cmd.id
        }
    }

    public class VisValueConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {                        
            if((Visibility)value == Visibility.Visible)
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
