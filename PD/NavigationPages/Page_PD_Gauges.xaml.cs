using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Input;
using System.Threading.Tasks;
using System.IO.Ports;
using PD;
using PD.Functions;
using PD.AnalysisModel;
using PD.ViewModel;
using PD.Models;

namespace PD.NavigationPages
{
    /// <summary>
    /// Page_PD_Gauges.xaml 的互動邏輯
    /// </summary>
    public partial class Page_PD_Gauges : UserControl
    {
        ComViewModel vm;
        ControlCmd cmd;
        TextBox obj;        
        string[] ch_v;
        bool _isDrag = false;

        public Page_PD_Gauges(ComViewModel vm)
        {
            InitializeComponent();

            //this.DataContext = vm;
            grid_bear_say.DataContext = vm;
            GaugeListView.DataContext = this;
            this.vm = vm;

            cmd = new ControlCmd(vm);

            //GaugeListView.DataContext = vm;

            //gauge1.DataContext = vm;  //將DataContext指給使用者控制項，必要!
            //gauge2.DataContext = vm;
            //gauge3.DataContext = vm;
            //gauge4.DataContext = vm;

            var gauges = GetGauges();
            if (gauges.Count > 0)
                vm.Gauges = gauges;
            ListViewGauges.ItemsSource = gauges;




            vm.Bool_Gauge.CopyTo(vm.bo_temp_gauge, 0);
        }

        private List<Gauge> GetGauges()
        {
            return new List<Gauge>()
            {
                new Gauge("1"),
                new Gauge("2"),
                new Gauge("3"),
                new Gauge("4"),
                new Gauge("5"),
                new Gauge("6"),
                new Gauge("7"),
                new Gauge("8")
            };
        }

        private async void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key==Key.Up || e.Key == Key.Down)
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
                    else if(e.Key==Key.Down)
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

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {            
            obj = sender as TextBox; //Get the focused textbox name
            string str_textBox_name = obj.Name;
            ch_v = str_textBox_name.Split('_');  // get the channel and which voltage (TF or VOA)         
        }

        private async void _MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta >= 120)  //換頁 page1
            {
                if (vm.Gauge_Page_now == 2)
                {
                    vm.Gauge_Page_now = 1;
                    vm.Bool_Page2 = vm.Bool_Gauge_Show;
                    await vm.AccessDelayAsync(50);
                    vm.Bool_Gauge_Show = vm.Bool_Page1;
                    await vm.AccessDelayAsync(50);
                    //if (vm.Bool_Page1.Length != 8 || vm.Bool_Page2.Length != 4)
                    //{
                    //    MessageBox.Show("Error Occur");
                    //}
                    vm.Str_Channel = new List<string>() { "1", "2", "3", "4", "5", "6", "7", "8", };
                    vm.Channel_visible = new List<Visibility>() { };
                }               
            }
            else if (e.Delta <= -120)  //換頁 page2
            {
                if (vm.Gauge_Page_now == 1)
                {
                    vm.Gauge_Page_now = 2;
                    vm.Bool_Page1 = vm.Bool_Gauge_Show;
                    await vm.AccessDelayAsync(50);
                    vm.Bool_Gauge_Show = vm.Bool_Page2;
                    await vm.AccessDelayAsync(50);

                    vm.Str_Channel = new List<string>() { "9", "10", "11", "12" };
                    vm.Channel_visible = new List<Visibility>()
                    {
                        Visibility.Visible, Visibility.Visible, Visibility.Visible, Visibility.Visible,
                        Visibility.Hidden, Visibility.Hidden, Visibility.Hidden, Visibility.Hidden
                    };
                }                
            }
        }
                
        private async void tbtn_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (vm.station_type != "Hermetic Test") return;

            if (!vm.Is_switch_mode) return;

            ToggleButton obj = (ToggleButton)sender;

            int switch_index = int.Parse(obj.Name.Substring(2));

            if (vm.Gauge_Page_now == 2)
                switch_index += 8;
            vm.switch_selected_index = switch_index;
            if (switch_index > 12) return;
            if (switch_index == vm.switch_index) return;
            if (string.IsNullOrWhiteSpace("I1 " + switch_index.ToString())) //Check comment box is empty or not
                return;

            #region switch re-open
            try
            {
                await vm.Port_Switch_ReOpen();
            }
            catch
            {
                vm.Str_cmd_read = "Switch Error";
                return;
            }
            #endregion

            if (switch_index > 0)   //Switch 1~12
            {
                try
                {                   
                    vm.Str_comment = "I1 " + switch_index.ToString();
                    vm.port_Switch.Write(vm.Str_comment + "\r");
                    await vm.AccessDelayAsync(vm.Int_Write_Delay);

                    vm.switch_index = switch_index;                    
                    vm.ch = switch_index;   //Save Switch channel
                }
                catch { }

                if (switch_index < 9 && int_saved_combox_index >= 9)  //換頁 page1
                {
                    vm.Bool_Page2 = vm.Bool_Gauge_Show;
                    vm.Str_Channel = new List<string>() { "1", "2", "3", "4", "5", "6", "7", "8", };
                    vm.Channel_visible = new List<Visibility>() { };
                    vm.Bool_Gauge_Show = vm.Bool_Page1;
                    vm.Gauge_Page_now = 1;
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
                    vm.Gauge_Page_now = 2;
                }                
            }
            else if (switch_index == 0)   //Switch ?
            {
                try
                {
                    vm.Str_comment = "I1?";
                    vm.port_Switch.Write(vm.Str_comment + "\r");
                    await vm.AccessDelayAsync(vm.Int_Read_Delay);
                }
                catch { vm.Str_cmd_read = "Switch? Error"; }
            }
            else
            {
                vm.Str_Channel = new List<string>() { "1", "2", "3", "4", "5", "6", "7", "8", };
                vm.Channel_visible = new List<Visibility>() { };
            }

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

        private async void slider_WL_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (vm.isConnected)
            {
                if (vm.PD_or_PM == true && vm.IsGoOn == true)
                    await vm.PM_Stop();

                vm.tls.SetWL(slider_WL.Value);
                await vm.AccessDelayAsync(vm.Int_Set_WL_Delay);

                try
                {
                    vm.Double_Laser_Wavelength = vm.tls.ReadWL();
                }
                catch { }

                if (vm.PD_or_PM == true && vm.IsGoOn == true) vm.PM_GO();
            }
        }

        private async void slider_Power_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Slider slid = (Slider)sender;

            int count = 1;
            foreach (bool i in vm.Bool_Gauge)  //找第一個為True的channel, 若無的話，預設為第一channel
            {
                if (i == true)
                    break;
                count++;
            }

            try
            {
                if (vm.PD_or_PM == true && vm.IsGoOn == true)
                    await vm.PM_Stop();

                string TF_or_VOA;
                string ch = count.ToString();
                double _dac = Math.Round(slid.Value, 0);
                string DAC = "0,0,0";

                TF_or_VOA = "D";

                if (vm.PD_or_PM == true)  //PM mode
                {
                    if (vm.Control_board_type == 0)  //Control board type: UFV
                    {
                        if (_dac >= 0)
                            DAC = _dac.ToString() + ",0," + vm.List_D0_value[0][2].ToString();
                        else
                            DAC = "0," + Math.Abs(_dac).ToString() + "," + vm.List_D0_value[0][2].ToString();
                    }
                    else
                    {
                        if (_dac >= 0)
                            DAC = _dac.ToString() + ",0";
                        else
                            DAC = "0," + Math.Abs(_dac).ToString();
                    }

                }
                else  //PD mode
                {
                    DAC = _dac.ToString();
                }

                vm.WriteDac(ch, TF_or_VOA, DAC);

                await vm.AccessDelayAsync(vm.Int_Write_Delay);
                vm.port_PD.Close();

                //UI顯示
                double dac = 0;
                if (vm.PD_or_PM == false)  //PD mode
                {
                    txt_power.Text = DAC;
                    dac = Convert.ToDouble(DAC);
                }
                else  //PM mode
                {
                    txt_power.Text = _dac.ToString();
                    dac = _dac;
                }

                if (vm.List_D0_value.Count > 0)
                {
                    List<List<string>> dac_change_list = vm.List_D0_value;
                    if (vm.PD_or_PM == true)
                        DAC = _dac.ToString();

                    if (vm.Control_board_type == 0)  //UFV board
                    {
                        if (dac >= 0)
                            dac_change_list[count - 1] = new List<string>() { DAC, "0", vm.List_D0_value[count - 1][2] };
                        else
                            dac_change_list[count - 1] = new List<string>() { "0", Math.Abs(dac).ToString(), vm.List_D0_value[count - 1][2] };
                    }
                    else  //V board
                    {
                        if (dac >= 0)
                            dac_change_list[count - 1] = new List<string>() { DAC, "0" };
                        else
                            dac_change_list[count - 1] = new List<string>() { "0", Math.Abs(dac).ToString() };
                    }

                    vm.List_D0_value = dac_change_list;
                }

                if (vm.PD_or_PM == true && vm.IsGoOn == true) vm.PM_GO();
            }
            catch { vm.Str_cmd_read = "Write DAC error."; }
        }

        private void slider_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Slider _slider = (Slider)sender;
            var slider_ActualWidth = _slider.ActualWidth;

            //相對於視窗中的滑鼠位置
            var point = e.MouseDevice.GetPosition(_slider);

            //計算滑鼠在Slider X方向上的位置(以百分比計算)
            var percentOfpoint = (point.X / slider_ActualWidth);

            //移動slider至滑鼠位置
            _slider.Value = Math.Round(percentOfpoint * (_slider.Maximum - _slider.Minimum) + _slider.Minimum, 2);
        }

        bool _is_txtWL_already_click = false;
        private async void txt_WL_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (_is_txtWL_already_click)
                return;

            _is_txtWL_already_click = true;

            if (e.Key == Key.Enter)
            {
                txt_SetWL(true);
            }
            if (e.Key==Key.Up)
            {
                if (vm.PD_or_PM == true && vm.IsGoOn == true)
                    await vm.PM_Stop();

                try
                {
                    double wl = Convert.ToDouble(txt_WL.Text);
                    txt_WL.Text = (wl + 0.01).ToString();
                    vm.tls.SetWL(wl + 0.01);
                    await vm.AccessDelayAsync(vm.Int_Set_WL_Delay);
                    vm.pm.SetWL(Convert.ToDouble(txt_WL.Text));
                    await vm.AccessDelayAsync(vm.Int_Set_WL_Delay/2);
                }
                catch { }

                if (vm.PD_or_PM == true && vm.IsGoOn == true) vm.PM_GO();
            }
            if (e.Key == Key.Down)
            {
                if (vm.PD_or_PM == true && vm.IsGoOn == true)
                    await vm.PM_Stop();

                try
                {
                    double wl = Convert.ToDouble(txt_WL.Text);
                    txt_WL.Text = (wl - 0.01).ToString();
                    vm.tls.SetWL(wl - 0.01);
                    await vm.AccessDelayAsync(vm.Int_Set_WL_Delay);
                    vm.pm.SetWL(Convert.ToDouble(txt_WL.Text));
                    await vm.AccessDelayAsync(vm.Int_Set_WL_Delay/2);
                }
                catch { }

                if (vm.PD_or_PM == true && vm.IsGoOn == true) vm.PM_GO();
            }

            _is_txtWL_already_click = false;
        }

        private async void txt_Dac_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            TextBox obj = (TextBox)sender;
            double textbox_value = double.Parse(obj.Text);
            int final_dac = 0;
            int switch_index = 1;
            //vm.switch_index = switch_index;

            string selected_comport;

            try
            {
                if (e.Key == Key.Enter)
                {
                    if (vm.PD_or_PM)  //PM
                    {
                        if (vm.station_type == "Hermetic Test")
                            selected_comport = vm.list_Board_Setting[vm.switch_index - 1][1];
                        else
                            selected_comport = vm.Selected_Comport;
                    }
                    else  //PD
                    {
                        return;
                    }

                    //Reset COM port
                    if (string.IsNullOrEmpty(selected_comport)) return;

                    await vm.Port_ReOpen(selected_comport);

                    if (!vm.isDACorVolt && !string.IsNullOrEmpty(obj.Text))  //Dac mode
                    {
                        final_dac = int.Parse(obj.Text);
                    }
                    else  //Voltage mode
                    {
                        if (vm.station_type != "Hermetic Test")
                        {

                        }
                        else
                        {
                            #region Read Board Table
                            List<double> list_voltage = new List<double>();
                            List<int> list_dac = new List<int>();
                                                        
                            int count = 0;
                            foreach (string strline in vm.board_read[vm.switch_index - 1])
                            {
                                string[] board_read = strline.Split(',');
                                if (board_read.Length <= 1)
                                    continue;

                                double voltage = double.Parse(board_read[0]);
                                int board_dac = int.Parse(board_read[1]);

                                list_voltage.Add(voltage);
                                list_dac.Add(board_dac);

                                if (voltage >= textbox_value && count > 0)
                                {
                                    final_dac = board_dac;
                                    break;
                                }

                                count++;
                            }
                            #endregion
                        }
                    }

                    //Set Dac
                    try
                    {
                        vm.Str_comment = "D1 0,0," + (final_dac).ToString();  //cmd = D1 0,0,1000
                        vm.port_PD.Write(vm.Str_comment + "\r");
                        await vm.AccessDelayAsync(vm.Int_Write_Delay);
                        vm.port_PD.Close();
                    }
                    catch { vm.Str_cmd_read = "Write Dac Error"; }
                }
                if (e.Key == Key.Up)
                {
                    if (vm.PD_or_PM == true && vm.IsGoOn == true)
                        await vm.PM_Stop();
                    
                    if (vm.PD_or_PM)  //PM
                    {
                        if (vm.station_type == "Hermetic Test")
                            selected_comport = vm.list_Board_Setting[vm.switch_index - 1][1];
                        else
                            selected_comport = vm.Selected_Comport;
                    }
                    else  //PD
                    {
                        return;
                    }

                    try
                    {
                        if(vm.DacType=="V3 Dac")
                        {
                            int dac = Convert.ToInt32(obj.Text);
                            obj.Text = (dac + 50).ToString();

                            cmd.Set_V3_Dac(selected_comport, dac + 50);
                        }
                        else
                        {
                            double volt = Convert.ToDouble(obj.Text);
                            obj.Text = (volt + 0.1).ToString();

                            cmd.Set_V3_Volt(selected_comport, volt + 0.1);
                        }
                        
                    }
                    catch { }

                    if (vm.PD_or_PM == true && vm.IsGoOn == true) vm.PM_GO();
                }
                if (e.Key == Key.Down)
                {
                    if (vm.PD_or_PM == true && vm.IsGoOn == true)
                        await vm.PM_Stop();

                    if (vm.PD_or_PM)  //PM
                    {
                        if (vm.station_type == "Hermetic Test")
                            selected_comport = vm.list_Board_Setting[vm.switch_index - 1][1];
                        else
                            selected_comport = vm.Selected_Comport;
                    }
                    else  //PD
                    {
                        return;
                    }

                    try
                    {
                        if (vm.DacType == "V3 Dac")
                        {
                            int dac = Convert.ToInt32(obj.Text);
                            obj.Text = (dac + 50).ToString();

                            cmd.Set_V3_Dac(selected_comport, dac - 50);
                        }
                        else
                        {
                            double volt = Convert.ToDouble(obj.Text);
                            obj.Text = (volt + 0.1).ToString();

                            cmd.Set_V3_Volt(selected_comport, volt - 0.1);
                        }

                    }
                    catch { }

                    if (vm.PD_or_PM == true && vm.IsGoOn == true) vm.PM_GO();
                }
            }
            catch
            {
                vm.Str_cmd_read = "Write Dac error";
            }

            _is_txtWL_already_click = false;
        }

        private async void txt_SetWL(bool _is_KeyEnter_Enter)
        {
            if (vm.PD_or_PM == true && vm.IsGoOn == true)
                await vm.PM_Stop();
                       
            if (_is_KeyEnter_Enter)
            {
                try
                {
                    vm.tls.SetWL(Convert.ToDouble(txt_WL.Text));
                    vm.Double_Laser_Wavelength = vm.tls.ReadWL();
                    await vm.AccessDelayAsync(vm.Int_Set_WL_Delay);
                    vm.pm.SetWL(Convert.ToDouble(txt_WL.Text));
                }
                catch { }
            }                

            if (vm.PD_or_PM == true && vm.IsGoOn == true)
                vm.PM_GO();
        }

        private void image_MouseEnter(object sender, MouseEventArgs e)
        {
            image.Visibility = Visibility.Hidden;
            grid_sector_btns.Visibility = Visibility.Visible;
        }

        private void grid_sector_btns_MouseLeave(object sender, MouseEventArgs e)
        {
            image.Visibility = Visibility.Visible;
            grid_sector_btns.Visibility = Visibility.Hidden;
        }

        private void btn_aft_Click(object sender, RoutedEventArgs e)
        {
            if (vm.bear_say_now < vm.bear_say_all)
            {
                if (vm.bear_say_now < vm.Collection_bear_say.Count)
                {
                    vm.List_bear_say = vm.Collection_bear_say[vm.bear_say_now];
                    //vm.List_bear_say_DataLabel = new List<string>() { "K WL", "WL", "IL" };
                }
                    
                vm.bear_say_now++;
            }
        }

        private void btn_pre_Click(object sender, RoutedEventArgs e)
        {
            if (vm.bear_say_now > 1)
            {
                vm.bear_say_now--;
                vm.List_bear_say = vm.Collection_bear_say[vm.bear_say_now - 1];
            }
        }

        List<List<string>> temp_list_bear_say = new List<List<string>>();
        private void Btn_page2_Click(object sender, RoutedEventArgs e)
        {
            if (vm.station_type == "Hermetic Test")   //to page 2
            {
                if (vm.List_bear_say == null) return;
                if (vm.List_bear_say.Count < 9) return;
                if (page_now == 2) return;
                page_now = 2;

                temp_list_bear_say = vm.List_bear_say;
                vm.List_bear_say = Analysis.ListDefine<string>(new List<List<string>>(), vm.ch_count, new List<string>());
                for (int ch = 9; ch <= 12; ch++)
                {
                    vm.List_bear_say[ch - 9] = temp_list_bear_say[ch - 1];
                }
                vm.List_bear_say = new List<List<string>>(vm.List_bear_say);
                //txt_No5.Visibility = Visibility.Hidden;
                //txt_No6.Visibility = Visibility.Hidden;
                //txt_No7.Visibility = Visibility.Hidden;
                //txt_No8.Visibility = Visibility.Hidden;

                vm.txt_No = new string[] { "Ch 9", "Ch 10", "Ch 11", "Ch 12", "", "", "", "" };

            }
        }

        private void Btn_page1_Click(object sender, RoutedEventArgs e)
        {
            if (vm.station_type == "Hermetic Test")   //to page 1
            {
                if (vm.List_bear_say == null) return;
                if (page_now == 1) return;
                page_now = 1;

                vm.List_bear_say = temp_list_bear_say;
                //txt_No5.Visibility = Visibility.Visible;
                //txt_No6.Visibility = Visibility.Visible;
                //txt_No7.Visibility = Visibility.Visible;
                //txt_No8.Visibility = Visibility.Visible;

                vm.txt_No = new string[] { "Ch 1", "Ch 2", "Ch 3", "Ch 4", "Ch 5", "Ch 6", "Ch 7", "Ch 8" };
            }
        }

        private void Border_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Border border = (Border)sender;
            border.Focus();
        }

        int page_now = 1;
        int int_saved_combox_index;
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (vm.station_type != "Hermetic Test") return;

            Button obj = (Button)sender;

            int switch_index;
            string[] tag_index = obj.Tag.ToString().Split('_');
            if (tag_index.Length == 2)
            {
                if (page_now == 1)
                    switch_index = int.Parse(tag_index[0]);
                else
                    switch_index = int.Parse(tag_index[1]);
            }
            else return;

            if (obj.Content == null) return;
            if (string.IsNullOrEmpty(obj.Content.ToString())) return;
            if (vm.PD_or_PM == true && vm.IsGoOn == true) await vm.PM_Stop();

            double txt_value = Convert.ToDouble(obj.Content);
            if (txt_value > 1000 && txt_value < 2000)  //波長
            {
                #region set WL
                double wl = txt_value;
                vm.tls.SetWL(wl);
                vm.Double_Laser_Wavelength = wl;
                await vm.AccessDelayAsync(vm.Int_Set_WL_Delay);
                vm.pm.SetWL(wl);
                #endregion
            }
            else  //電壓
            {
                double dac = txt_value;

                string selected_comport = vm.list_Board_Setting[switch_index - 1][1];
                //Reset COM port
                if (string.IsNullOrEmpty(selected_comport)) return;
                
                vm.port_PD = new SerialPort(selected_comport, 115200, Parity.None, 8, StopBits.One);
                //savePort = vm.port_PD;

                await vm.Port_ReOpen(selected_comport);
                
                //Set voltage
                try
                {
                    vm.Str_comment = "D1 0,0," + (dac).ToString();  //cmd = D1 0,0,1000
                    vm.port_PD.Write(vm.Str_comment + "\r");
                    await vm.AccessDelayAsync(vm.Int_Write_Delay);

                    vm.port_PD.Close();
                    vm.port_PD.DiscardInBuffer();       // RX
                    vm.port_PD.DiscardOutBuffer();      // TX
                }
                catch { vm.Str_cmd_read = "Write Dac Error"; }
            }

            if (vm.PD_or_PM == true && vm.IsGoOn == true) vm.PM_GO();

            #region set switch
            try
            {
                if (vm.port_Switch != null) vm.port_Switch.Close();

                await vm.AccessDelayAsync(50);

                if (vm.Comport_Switch > 0)
                {
                    vm.port_Switch = new SerialPort("COM" + vm.Comport_Switch.ToString(), 115200, Parity.None, 8, StopBits.One);
                    vm.port_Switch.Open();

                    vm.port_Switch.DiscardInBuffer();       // RX
                    vm.port_Switch.DiscardOutBuffer();      // TX
                }
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
                }
                catch { }
            }
            else if (switch_index == 0)   //Switch ?
            {
                vm.Str_comment = "I1?";
                vm.port_Switch.Write(vm.Str_comment + "\r");
                await vm.AccessDelayAsync(vm.Int_Read_Delay);
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

        private void grid_BearSay_Loaded(object sender, RoutedEventArgs e)
        {
            if (vm.List_bear_say == null)
                vm.List_bear_say = new List<List<string>>();
            else
                vm.List_bear_say = new List<List<string>>(vm.List_bear_say);

            #region Load History Bear Say                
            if (vm.is_BearSay_History_Loaded != true && File.Exists(@"D:\PD\BearSay.txt"))
            {
                List<List<string>> collection = new List<List<string>>();

                string[] lines = System.IO.File.ReadAllLines(@"D:\PD\BearSay.txt");
                for (int i = 0; i < lines.Length; i++)
                {
                    collection.Add(new List<string>());

                    string[] line = lines[i].Split(',');

                    if (line.Length == 2)
                    {
                        collection[i].Add(line[0]);
                        collection[i].Add(line[1]);
                    }
                }

                vm.Collection_bear_say = new List<List<List<string>>>();
                vm.Collection_bear_say.Add(collection);  //只讀最後一項
                vm.List_bear_say = collection;

                vm.bear_say_all = vm.Collection_bear_say.Count;
                vm.bear_say_now = vm.bear_say_all;

                vm.is_BearSay_History_Loaded = true;
            }
            #endregion

            if (vm.List_bear_say.Count == 0) return;

            #region Delete and Save Bear say to txt file
            if (File.Exists(@"D:\PD\BearSay.txt")) File.Delete(@"D:\PD\BearSay.txt");

            try
            {
                using (System.IO.StreamWriter file =
                 new System.IO.StreamWriter(@"D:\PD\BearSay.txt", true))
                {
                    string str = "";
                    for (int i = 0; i < vm.List_bear_say.Count; i++)
                    {
                        for (int j = 0; j < vm.List_bear_say[i].Count; j++)
                        {
                            str = str + vm.List_bear_say[i][j];
                            if (j == 0) str += ",";
                        }
                        str = str + "\r\n";
                    }
                    file.WriteLine(str);

                    vm._write_line = new List<string>();
                }
            }
            catch { vm.Str_cmd_read = "Write Bear Say Error"; }
            #endregion
        }

        private void btn_bearsay_visual_Click(object sender, RoutedEventArgs e)
        {
            //grid_BearSay.Visibility = Visibility.Hidden;
            grid_second_rowdefinition.Height = new GridLength(0, GridUnitType.Star);
        }

        bool gaugetxt_focus = false;
        private void _GaugeTxT__GotFocus(object sender, RoutedEventArgs e)
        {
            gaugetxt_focus = true;
        }

        private void _GaugeTxT__LostFocus(object sender, RoutedEventArgs e)
        {
            gaugetxt_focus = false;
        }

        private async void Btn_WL_Click(object sender, RoutedEventArgs e)
        {
            if (vm.station_type != "Hermetic Test") return;

            Button obj = (Button)sender;

            int switch_index;
            string tag = obj.Tag.ToString(), tag_index;
            
            if (page_now == 1)
                tag_index = tag.Substring(0,1);
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

        private void Btn_IL_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("IL");
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

        private void Gauge_btn_IL_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void CustomBTN_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Test");
        }

        private void CustomBTN_KeyDown(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("KeyDown");
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            vm.GaugePage_Width = this.Width;
        }
    }
}
