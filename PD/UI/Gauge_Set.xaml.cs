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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Controls.Primitives;

using PD.Functions;
using PD.AnalysisModel;
using PD.ViewModel;
using PD.NavigationPages;

namespace PD.UI
{
    /// <summary>
    /// Interaction logic for Gauge_Set.xaml
    /// </summary>
    public partial class Gauge_Set : UserControl
    {
        ComViewModel vm;
        ControlCmd cmd;
        TextBox obj;

        string[] ch_v;
        bool gaugetxt_focus = false;
        bool _isDrag = false;
        int page_now = 1;
        int int_saved_combox_index;

        public Gauge_Set(ComViewModel vm)
        {
            InitializeComponent();

            this.DataContext = vm;
            this.vm = vm;

            cmd = new ControlCmd(vm);
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            obj = sender as TextBox; //Get the focused textbox name
            string str_textBox_name = obj.Name;
            ch_v = str_textBox_name.Split('_');  // get the channel and which voltage (TF or VOA)         
        }

        private async void Btn_WL_Click(object sender, RoutedEventArgs e)
        {
            if (vm.station_type != "Hermetic Test") return;

            Button obj = (Button)sender;

            int switch_index;
            string tag = obj.Tag.ToString(), tag_index;

            if (page_now == 1)
                tag_index = tag.Substring(0, 1);
            else
                tag_index = tag.Substring(2);

            switch_index = int.Parse(tag_index);

            if (obj.Content == null) return;
            if (string.IsNullOrEmpty(obj.Content.ToString())) return;
            if (vm.PD_or_PM == true && vm.IsGoOn == true) await vm.PM_Stop();

            double txt_value = Convert.ToDouble(obj.Content);

            #region set WL
            double wl = txt_value;
            vm.tls.SetWL(wl);
            vm.Double_Laser_Wavelength = wl;
            await vm.AccessDelayAsync(vm.Int_Set_WL_Delay);
            vm.pm.SetWL(wl);
            #endregion

            if (vm.PD_or_PM == true && vm.IsGoOn == true) vm.PM_GO();

            #region set switch
            try
            {
                if (vm.Comport_Switch > 0) await vm.Port_Switch_ReOpen();
            }
            catch
            {
                vm.Str_bear_say = "Switch Comport Error";
                vm.Winbear = new Window_Bear(vm, false, "String");
                vm.Winbear.Show();
                return;
            }

            if (switch_index > 12) return;

            if (switch_index > 0 && switch_index < 13)   //Switch 1~12
            {
                if (string.IsNullOrWhiteSpace("I1 " + switch_index.ToString())) //Check comment box is empty or not
                    return;

                if (switch_index < 9 && int_saved_combox_index >= 9)  //換頁 page1
                {
                    vm.Bool_Page2 = vm.Bool_Gauge_Show;
                    vm.Str_Channel = new List<string>() { "1", "2", "3", "4", "5", "6", "7", "8", };
                    vm.Channel_visible = new List<Visibility>() { };
                    vm.Bool_Gauge_Show = vm.Bool_Page1;
                }
                else if (switch_index > 8 && int_saved_combox_index <= 8)  //換頁 page2
                {
                    vm.Bool_Page1 = vm.Bool_Gauge_Show;
                    vm.Str_Channel = new List<string>() { "9", "10", "11", "12" };
                    vm.Channel_visible = new List<Visibility>()
                    {
                        Visibility.Visible, Visibility.Visible, Visibility.Visible, Visibility.Visible,
                        Visibility.Hidden, Visibility.Hidden, Visibility.Hidden, Visibility.Hidden
                    };
                    vm.Bool_Gauge_Show = vm.Bool_Page2;
                }

                try
                {
                    vm.Str_comment = "I1 " + switch_index.ToString();
                    vm.port_Switch.Write(vm.Str_comment + "\r");
                    await vm.AccessDelayAsync(vm.Int_Write_Delay);
                    vm.port_Switch.Close();
                }
                catch { }
            }
            else if (switch_index == 0)   //Switch ?
            {
                vm.Str_comment = "I1?";
                vm.port_Switch.Write(vm.Str_comment + "\r");
                await vm.AccessDelayAsync(vm.Int_Read_Delay);
                vm.port_Switch.Close();
            }
            else
            {
                vm.Str_Channel = new List<string>() { "1", "2", "3", "4", "5", "6", "7", "8", };
                vm.Channel_visible = new List<Visibility>() { };
            }

            int_saved_combox_index = switch_index;
            vm.ch = switch_index;   //Save Switch channel

            vm.switch_index = switch_index;
            #endregion
        }

        private async void Btn_IL_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void Btn_Voltage_Click(object sender, RoutedEventArgs e)
        {
            if (vm.station_type != "Hermetic Test") return;

            Button obj = (Button)sender;

            int switch_index;
            string tag = obj.Tag.ToString(), tag_index;

            if (page_now == 1)
                tag_index = tag.Substring(0, 1);
            else
                tag_index = tag.Substring(2);

            switch_index = int.Parse(tag_index);  //定義switch channel

            if (obj.Content == null) return;
            if (string.IsNullOrEmpty(obj.Content.ToString())) return;
            //if (vm.PD_or_PM == true && vm.IsGoOn == true) await vm.PM_Stop();

            double txt_value = Convert.ToDouble(obj.Content);

            double vtg = txt_value;

            string selected_comport = vm.list_Board_Setting[switch_index - 1][1];
            //Reset COM port
            if (string.IsNullOrEmpty(selected_comport)) return;

            await vm.Port_ReOpen(selected_comport);

            //Read Board Table
            #region Read Board Table
            List<double> list_voltage = new List<double>();
            List<int> list_dac = new List<int>();
            int final_dac = 0;

            int count = 0;
            foreach (string strline in vm.board_read[switch_index - 1])
            {
                string[] board_read = strline.Split(',');
                if (board_read.Length <= 1)
                    continue;

                double voltage = double.Parse(board_read[0]);
                int board_dac = int.Parse(board_read[1]);

                list_voltage.Add(voltage);
                list_dac.Add(board_dac);

                if (voltage >= vtg && count > 0)
                {
                    final_dac = board_dac;
                    break;
                }

                count++;
            }
            #endregion

            //Set voltage
            try
            {
                vm.Str_comment = "D1 0,0," + (final_dac).ToString();  //cmd = D1 0,0,1000
                vm.port_PD.Write(vm.Str_comment + "\r");
                await vm.AccessDelayAsync(vm.Int_Write_Delay);
                vm.port_PD.Close();
            }
            catch { vm.Str_cmd_read = "Write Dac Error"; }

            if (vm.PD_or_PM == true && vm.IsGoOn == true) vm.PM_GO();

            #region set switch
            try
            {
                if (vm.Comport_Switch > 0) await vm.Port_Switch_ReOpen();
            }
            catch
            {
                vm.Str_bear_say = "Switch Comport Error";
                vm.Winbear = new Window_Bear(vm, false, "String");
                vm.Winbear.Show();
                return;
            }

            if (switch_index > 12) return;

            if (switch_index > 0 && switch_index < 13)   //Switch 1~12
            {
                if (string.IsNullOrWhiteSpace("I1 " + switch_index.ToString())) //Check comment box is empty or not
                    return;

                if (switch_index < 9 && int_saved_combox_index >= 9)  //換頁 page1
                {
                    vm.Bool_Page2 = vm.Bool_Gauge_Show;
                    vm.Str_Channel = new List<string>() { "1", "2", "3", "4", "5", "6", "7", "8", };
                    vm.Channel_visible = new List<Visibility>() { };
                    vm.Bool_Gauge_Show = vm.Bool_Page1;
                }
                else if (switch_index > 8 && int_saved_combox_index <= 8)  //換頁 page2
                {
                    vm.Bool_Page1 = vm.Bool_Gauge_Show;
                    vm.Str_Channel = new List<string>() { "9", "10", "11", "12" };
                    vm.Channel_visible = new List<Visibility>()
                    {
                        Visibility.Visible, Visibility.Visible, Visibility.Visible, Visibility.Visible,
                        Visibility.Hidden, Visibility.Hidden, Visibility.Hidden, Visibility.Hidden
                    };
                    vm.Bool_Gauge_Show = vm.Bool_Page2;
                }

                try
                {
                    vm.Str_comment = "I1 " + switch_index.ToString();
                    vm.port_Switch.Write(vm.Str_comment + "\r");
                    await vm.AccessDelayAsync(vm.Int_Write_Delay);
                    vm.port_Switch.Close();
                }
                catch { }
            }
            else
            {
                vm.Str_Channel = new List<string>() { "1", "2", "3", "4", "5", "6", "7", "8", };
                vm.Channel_visible = new List<Visibility>() { };
            }

            int_saved_combox_index = switch_index;
            vm.ch = switch_index;   //Save Switch channel

            vm.switch_index = switch_index;
            #endregion
        }

        private async void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Up || e.Key == Key.Down)
            {
                try
                {
                    if (vm.PD_or_PM == true && vm.IsGoOn == true)
                        await vm.PM_Stop();

                    await vm.AccessDelayAsync(vm.Int_Read_Delay);

                    string TF_or_VOA;
                    string ch = ch_v[1];
                    string DAC = obj.Text;
                    int input_value = int.Parse(obj.Text);
                    int channel = Convert.ToInt32(ch);
                    int V = Convert.ToInt16(ch_v[2]) - 1;

                    if (e.Key == Key.Up)
                        input_value = int.Parse(obj.Text) + 100;
                    else if (e.Key == Key.Down)
                        input_value = int.Parse(obj.Text) - 100;

                    //判斷index型式
                    if (vm.PD_or_PM == false)  //PD mode
                    {
                        //判斷要下的電壓為TF軸還是VOA軸
                        if (ch_v[2] == "3")
                            TF_or_VOA = "VOA";
                        else
                            TF_or_VOA = "D";

                        if (ch_v[2] == "2")    //V2
                            DAC = "-" + input_value.ToString();
                    }
                    else                       //PM mode
                    {
                        if (vm.Control_board_type == 0)  //Control board type: UFV
                        {
                            TF_or_VOA = "D";

                            if (ch_v[2] == "1")     //V1                                    
                                DAC = input_value.ToString() + ",0," + vm.List_D0_value[channel - 1][2].ToString();
                            else if (ch_v[2] == "2")            //V2
                                DAC = "0," + input_value.ToString() + "," + vm.List_D0_value[channel - 1][2].ToString();
                            else                    //V3
                                DAC = vm.List_D0_value[channel - 1][0].ToString() + ","
                                    + vm.List_D0_value[channel - 1][1].ToString() + "," + input_value.ToString();
                        }
                        else  //Control board type: V
                        {
                            TF_or_VOA = "D";

                            if (ch_v[2] == "1")     //V1
                                DAC = input_value.ToString() + ",0";
                            else if (ch_v[2] == "2")            //V2
                                DAC = "0," + input_value.ToString();
                            else                    //V3
                                DAC = vm.List_D0_value[channel - 1][0].ToString() + ","
                                    + vm.List_D0_value[channel - 1][1].ToString();
                        }
                    }

                    vm.WriteDac(ch, TF_or_VOA, DAC);

                    await vm.AccessDelayAsync(vm.Int_Write_Delay);
                    vm.port_PD.Close();

                    vm.List_D0_value[channel - 1][V] = input_value.ToString();
                    vm.List_D0_value = new List<List<string>>(vm.List_D0_value);

                    if (vm.PD_or_PM == true && vm.IsGoOn == true)
                        vm.PM_GO();
                }
                catch { vm.Str_cmd_read = "Write DAC error."; }
            }
        }

        private void _GaugeTxT__GotFocus(object sender, RoutedEventArgs e)
        {
            gaugetxt_focus = true;
        }

        private void _GaugeTxT__LostFocus(object sender, RoutedEventArgs e)
        {
            gaugetxt_focus = false;
        }

        private void tbtn_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (vm.station_type != "Hermetic Test") return;
            if (gaugetxt_focus) return;
            ToggleButton obj = (ToggleButton)sender;
            obj.IsChecked = !obj.IsChecked;
        }

        private void tbtn_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isDrag = true;
            ToggleButton obj = (ToggleButton)sender;
            obj.IsChecked = !obj.IsChecked;
        }

        private void tbtn_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDrag = false;

            if (vm.station_type == "Hermetic Test")
            {
                if (vm.Gauge_Page_now == 1)
                {
                    try
                    {
                        Array.Copy(vm.Bool_Gauge_Show, 0, vm.bo_temp_gauge, 0, 8);
                    }
                    catch { vm.Str_cmd_read = vm.Bool_Gauge_Show.Length.ToString(); }
                    Array.Copy(vm.Bool_Page2, 0, vm.bo_temp_gauge, 8, 4);
                }
                else
                {
                    try
                    {
                        Array.Copy(vm.Bool_Page1, 0, vm.bo_temp_gauge, 0, 8);
                    }
                    catch { vm.Str_cmd_read = vm.Bool_Page1.Length.ToString(); }

                    Array.Copy(vm.Bool_Gauge_Show, 0, vm.bo_temp_gauge, 8, 4);
                }
            }
            else
                vm.bo_temp_gauge = vm.Bool_Gauge_Show;

            int count = 0;
            foreach (bool b in vm.bo_temp_gauge) if (b) count++;

            if (count < vm.ch_count && count > 0)
            {
                vm.Bool_Gauge = vm.bo_temp_gauge;
            }
            else //全選 or 全不選 => 全跑
            {
                vm.Bool_Gauge = new bool[vm.ch_count];
                int c = 0;
                foreach (bool b in vm.Bool_Gauge)
                {
                    vm.Bool_Gauge[c] = !b;
                    c++;
                }
            }
        }

        private void tbtn_MouseEnter(object sender, MouseEventArgs e)
        {
            ToggleButton obj = (ToggleButton)sender;
            if (_isDrag)
                obj.IsChecked = !obj.IsChecked;
        }

        private void tbtn_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
