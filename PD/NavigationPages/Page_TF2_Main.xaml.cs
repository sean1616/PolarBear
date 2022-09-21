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
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Data;
using System.Reflection;
using DiCon.Instrument.HP;
using PD;
using PD.Functions;
using PD.AnalysisModel;
using PD.ViewModel;
using PD.Models;
using PD.UI;
using DiCon.UCB.Communication;
using OxyPlot;

namespace PD.NavigationPages
{
    /// <summary>
    /// Page_PD_Gauges.xaml 的互動邏輯
    /// </summary>
    public partial class Page_TF2_Main : UserControl
    {
        ComViewModel vm;
        ControlCmd cmd;
        TextBox obj;
        string[] ch_v = new string[2];
        bool _isDrag = false;
        Analysis anly;

        public Page_TF2_Main(ComViewModel vm)
        {
            InitializeComponent();

            this.DataContext = vm;

            this.vm = vm;

            cmd = new ControlCmd(this.vm);

            //GaugeView.DataContext = vm;  //將DataContext指給使用者控制項，必要!
            grid_bear_say.DataContext = vm;
            //viewer.DataContext = vm;  //將DataContext指給使用者控制項，必要!


            bool dac_volt;
            if (bool.TryParse(this.vm.Ini_Read("Connection", "DACorVolt"), out dac_volt))
            {
                vm.isDACorVolt = dac_volt;
                if (this.vm.isDACorVolt)
                    this.vm.DacType = "Voltage";
                else
                    this.vm.DacType = "Dac";
            }

            vm.Bool_Gauge.CopyTo(vm.bo_temp_gauge, 0);

            anly = new Analysis(vm);

            vm.TF2_station_type = string.IsNullOrEmpty(vm.Ini_Read("Connection", "TF2_station_type")) ? "Alignment" : vm.Ini_Read("Connection", "TF2_station_type");
            //vm.TF2_station_selected = anly.Generic_GetINISetting(vm.TF2_station_selected, "Connection", "TF2_station_selected");
        }

        private async void TextBox_Dac_KeyDown(object sender, RoutedEventArgs e)
        {
            KeyEventArgs ee = (KeyEventArgs)e;
            GaugeModel gm = (GaugeModel)obj.DataContext;
            if (ee.Key == Key.Enter)
            {
                try
                {
                    bool _isGoOn = vm.IsGoOn;

                    if (!vm.IsDistributedSystem)
                    {
                        if (vm.PD_or_PM && vm.IsGoOn)
                            await vm.PM_Stop();
                    }

                    if (!vm.PD_or_PM)  //PD mode
                    {
                        if (vm.IsGoOn)
                        {
                            switch (gm.GaugeD0_Select)
                            {
                                case "1":
                                    vm.Save_cmd(new ComMember()
                                    {
                                        No = vm.Cmd_Count.ToString(),
                                        Type = "PD",
                                        Comport = vm.Selected_Comport,
                                        Channel = gm.GaugeChannel,
                                        Command = "WRITEDAC",
                                        Value_1 = gm.GaugeD0_1,
                                        Value_2 = "0",
                                        Value_3 = gm.GaugeD0_3
                                    });
                                    break;
                                case "2":
                                    vm.Save_cmd(new ComMember()
                                    {
                                        No = vm.Cmd_Count.ToString(),
                                        Type = "PD",
                                        Comport = vm.Selected_Comport,
                                        Channel = gm.GaugeChannel,
                                        Command = "WRITEDAC",
                                        Value_1 = "0",
                                        Value_2 = gm.GaugeD0_2,
                                        Value_3 = gm.GaugeD0_3
                                    });
                                    //vm.Save_Command(vm.Cmd_Count, "", "PD", vm.Selected_Comport, gm.GaugeChannel, "WRITEDAC", "0", gm.GaugeD0_2, gm.GaugeD0_3, "", "", "");
                                    break;
                                case "3":
                                    //string[] preDac = await anly.Analyze_PreDAC(vm.Selected_Comport, gm.GaugeChannel);
                                    vm.Save_cmd(new ComMember()
                                    {
                                        No = vm.Cmd_Count.ToString(),
                                        Type = "PD",
                                        Comport = vm.Selected_Comport,
                                        Channel = gm.GaugeChannel,
                                        Command = "WRITEDAC",
                                        Value_3 = gm.GaugeD0_3
                                    });
                                    break;
                                default:
                                    vm.Str_cmd_read = "Selected Dac Textbox error";
                                    return;
                            }
                        }
                        else  //PD mode , Go off
                        {
                            switch (gm.GaugeD0_Select)
                            {
                                case "1":
                                    vm.Cmd_WriteDac(gm.GaugeChannel, gm.GaugeD0_1, "0", gm.GaugeD0_3);  //Write Dac
                                    break;

                                case "2":
                                    vm.Cmd_WriteDac(gm.GaugeChannel, "0", gm.GaugeD0_2, gm.GaugeD0_3);  //Write Dac
                                    break;

                                case "3":
                                    string[] preDac = await anly.Analyze_PreDAC(vm.Selected_Comport, gm.GaugeChannel);
                                    vm.Cmd_WriteDac(gm.GaugeChannel, preDac[0], preDac[1], gm.GaugeD0_3);  //Write Dac
                                    break;
                            }
                        }
                    }
                    else if (vm.PD_or_PM)  //PM mode
                    {
                        if (!vm.IsGoOn)
                        {

                            //vm.Cmd_WriteDac(gm.GaugeChannel, preDac[0], preDac[1], gm.GaugeD0_3);  //Write Dac

                            cmd.Set_Dac(vm.Selected_Comport, gm);
                        }
                        else
                        {
                            string D0_1, D0_2;
                            D0_1 = gm.GaugeD0_1;
                            D0_2 = gm.GaugeD0_2;

                            if (gm.GaugeD0_Select == "1")
                                D0_2 = "0";
                            else if (gm.GaugeD0_Select == "2")
                                D0_1 = "0";
                            else if (gm.GaugeD0_Select == "3")
                            {
                                //Ask DAC? first
                                try
                                {
                                    await vm.Port_ReOpen(vm.Selected_Comport);
                                    if (vm.port_PD.IsOpen)
                                    {
                                        string cmd = "D1?";
                                        vm.port_PD.Write(cmd + "\r");

                                        await Task.Delay(vm.Int_Read_Delay);

                                        int size = vm.port_PD.BytesToRead;
                                        byte[] dataBuffer = new byte[size];
                                        if (size > 0)
                                        {
                                            dataBuffer = new byte[size];
                                            int length = vm.port_PD.Read(dataBuffer, 0, size);
                                        }

                                        //資料分析並顯示於狀態列
                                        anly.Read_analysis(cmd, dataBuffer);

                                        #region Analyze Dx? and show data                                        
                                        string[] words = vm.Str_cmd_read.Split(',');  //V1,V2,V3 
                                        if (words.Length >= 2)
                                        {
                                            int v1, v2;
                                            if (int.TryParse(words[0], out v1) && int.TryParse(words[1], out v2))
                                            {
                                                //If V1(or V2) is not equal to zero, keep it as usual
                                                if (v1 == 0) D0_1 = "0";
                                                else D0_2 = "0";
                                            }
                                        }
                                        #endregion
                                    }
                                }
                                catch { }
                            }

                            vm.Save_cmd(new ComMember()
                            {
                                YN = true,
                                No = vm.Cmd_Count.ToString(),
                                Type = "PM",
                                Comport = vm.Selected_Comport,
                                Command = CommandList.WriteDac,
                                Value_1 = D0_1,
                                Value_2 = D0_2,
                                Value_3 = gm.GaugeD0_3
                            });
                        }
                    }

                }
                catch
                {
                    vm.Str_cmd_read = "Write DAC error.";
                }
            }
            else if (ee.Key == Key.Up)
            {

                int input_value = 0;

                switch (gm.GaugeD0_Select)
                {
                    case "1":
                        input_value = int.Parse(gm.GaugeD0_1) + 100;
                        gm.GaugeD0_1 = input_value.ToString();
                        break;
                    case "2":
                        input_value = int.Parse(gm.GaugeD0_2) + 100;
                        gm.GaugeD0_2 = input_value.ToString();
                        break;
                    case "3":
                        input_value = int.Parse(gm.GaugeD0_3) + 100;
                        gm.GaugeD0_3 = input_value.ToString();
                        break;
                    default:
                        vm.Str_cmd_read = "Selected Dac Textbox error";
                        return;
                }
                cmd.Set_Dac(vm.Selected_Comport, gm);
            }
            else if (ee.Key == Key.Down)
            {
                int input_value = 0;

                switch (gm.GaugeD0_Select)
                {
                    case "1":
                        input_value = int.Parse(gm.GaugeD0_1) - 100;
                        gm.GaugeD0_1 = input_value.ToString();
                        break;
                    case "2":
                        input_value = int.Parse(gm.GaugeD0_2) - 100;
                        gm.GaugeD0_2 = input_value.ToString();
                        break;
                    case "3":
                        input_value = int.Parse(gm.GaugeD0_3) - 100;
                        gm.GaugeD0_3 = input_value.ToString();
                        break;
                    default:
                        vm.Str_cmd_read = "Selected Dac Textbox error";
                        return;
                }
                cmd.Set_Dac(vm.Selected_Comport, gm);
            }
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            obj = sender as TextBox; //Get the focused textbox name            
            //ch_v[0] = obj.Tag.ToString().Substring(1);  // get the channel and which voltage (TF or VOA)         
            //ch_v[1] = obj.Name.ToString().Substring(1);  // get the channel and which voltage (TF or VOA)                 
        }

        private void tbtn_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (vm.station_type != "Hermetic_Test") return;
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

        private void tbtn_MouseEnter(object sender, MouseEventArgs e)
        {
            ToggleButton obj = (ToggleButton)sender;
            if (_isDrag)
                obj.IsChecked = !obj.IsChecked;
        }

        private void slider_WL_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (vm.isConnected)
            {
                if (!vm.IsGoOn) cmd.Set_WL(Convert.ToDouble(txt_WL.Text), true, true);
                else
                    vm.Save_cmd(new ComMember() { YN = true, No = vm.Cmd_Count.ToString(), Command = "SETWL", Value_1 = txt_WL.Text });
            }
        }

        private async void slider_Power_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (vm.isConnected)
            {
                if (vm.PD_or_PM == true && vm.IsGoOn == true)
                    await vm.PM_Stop();
                if (vm.PD_or_PM == true && vm.IsGoOn == true) vm.PM_GO();
            }
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
        private void txt_WL_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (_is_txtWL_already_click) return;

            _is_txtWL_already_click = true;

            if (e.Key == Key.Enter)
            {
                double wl = Convert.ToDouble(txt_WL.Text);

                if (!vm.IsGoOn) cmd.Set_WL(Convert.ToDouble(txt_WL.Text), true, true);
                else
                    vm.Save_cmd(new ComMember() { YN = true, No = vm.Cmd_Count.ToString(), Command = "SETWL", Value_1 = txt_WL.Text });
            }
            if (e.Key == Key.Up)
            {
                double wl = Convert.ToDouble(txt_WL.Text) + 0.01;

                if (!vm.IsGoOn)
                    try
                    {
                        txt_WL.Text = (wl).ToString();
                        cmd.Set_WL(wl, true, true);
                    }
                    catch { }
                else
                    vm.Save_cmd(new ComMember() { YN = true, No = vm.Cmd_Count.ToString(), Command = "SETWL", Value_1 = wl.ToString() });

                vm.Double_Laser_Wavelength = wl;
            }
            if (e.Key == Key.Down)
            {
                double wl = Convert.ToDouble(txt_WL.Text) - 0.01;

                if (!vm.IsGoOn)
                    try
                    {
                        txt_WL.Text = (wl).ToString();
                        cmd.Set_WL(wl, true, true);
                    }
                    catch { }
                else
                    vm.Save_cmd(new ComMember() { YN = true, No = vm.Cmd_Count.ToString(), Command = "SETWL", Value_1 = wl.ToString() });

                vm.Double_Laser_Wavelength = wl;
            }

            _is_txtWL_already_click = false;
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
            //if (vm.bear_say_now < vm.bear_say_all)
            //{
            //    vm.bear_say_now++;

            //    for (int ch = 0; ch < vm.ch_count; ch++)
            //    {
            //        vm.list_GaugeModels[ch].GaugeBearSay_1 = vm.list_collection_GaugeModels[vm.bear_say_now - 1][ch].GaugeBearSay_1;
            //        vm.list_GaugeModels[ch].GaugeBearSay_2 = vm.list_collection_GaugeModels[vm.bear_say_now - 1][ch].GaugeBearSay_2;
            //        vm.list_GaugeModels[ch].GaugeBearSay_3 = vm.list_collection_GaugeModels[vm.bear_say_now - 1][ch].GaugeBearSay_3;

            //        vm.Save_All_PD_Value[ch] = vm.list_collection_GaugeModels[vm.bear_say_now - 1][ch].DataPoints;
            //        vm.Chart_All_DataPoints = new List<List<DataPoint>>(vm.Save_All_PD_Value);
            //        vm.Chart_DataPoints = new List<DataPoint>(vm.Chart_All_DataPoints[0]);  //A lineseries                
            //    }
            //}

            try
            {
                if (vm.int_chart_now >= vm.int_chart_count)
                    return;

                if (vm.Chart_All_Datapoints_History.Count <= vm.int_chart_now)
                {
                    vm.Chart_All_DataPoints = new List<List<OxyPlot.DataPoint>>(vm.Chart_All_Datapoints_History.Last());
                }
                else
                    vm.Chart_All_DataPoints = new List<List<OxyPlot.DataPoint>>(vm.Chart_All_Datapoints_History[vm.int_chart_now]);

                if (vm.Chart_All_DataPoints.Count != 0)
                    vm.Chart_DataPoints = new List<OxyPlot.DataPoint>(vm.Chart_All_DataPoints[0]);  //Update Chart UI

                vm.int_chart_now++;

                vm.ChartNowModel = vm.list_ChartModels[vm.int_chart_now - 1];

                vm.Chart_x_title = vm.ChartNowModel.title_x;
                vm.Chart_y_title = vm.ChartNowModel.title_y;

                vm.msgModel.msg_3 = Math.Round(vm.ChartNowModel.TimeSpan, 1).ToString();

                for (int i = 0; i < vm.ch_count; i++)
                {
                    vm.list_GaugeModels[i].GaugeBearSay_1 = vm.list_collection_GaugeModels[vm.bear_say_now - 1][i].GaugeBearSay_1;
                    vm.list_GaugeModels[i].GaugeBearSay_2 = vm.list_collection_GaugeModels[vm.bear_say_now - 1][i].GaugeBearSay_2;
                    vm.list_GaugeModels[i].GaugeBearSay_3 = vm.list_collection_GaugeModels[vm.bear_say_now - 1][i].GaugeBearSay_3;

                    if (i < vm.list_ch_title.Count)
                        if (vm.list_ChartModels.Count > (vm.int_chart_now - 1))
                        {
                            if (i <= vm.list_ch_title.Count - 1 && i <= vm.ChartNowModel.list_delta_IL.Count)
                                vm.list_ch_title[i] = string.Format("ch{0} ,Delta IL : {1}", i + 1, vm.ChartNowModel.list_delta_IL[i]);
                        }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.StackTrace.ToString());
            }

        }

        private void btn_pre_Click(object sender, RoutedEventArgs e)
        {
            //if (vm.bear_say_now > 1)
            //{
            //    vm.bear_say_now--;

            //    for (int ch = 0; ch < vm.ch_count; ch++)
            //    {
            //        vm.list_GaugeModels[ch].GaugeBearSay_1 = vm.list_collection_GaugeModels[vm.bear_say_now - 1][ch].GaugeBearSay_1;
            //        vm.list_GaugeModels[ch].GaugeBearSay_2 = vm.list_collection_GaugeModels[vm.bear_say_now - 1][ch].GaugeBearSay_2;
            //        vm.list_GaugeModels[ch].GaugeBearSay_3 = vm.list_collection_GaugeModels[vm.bear_say_now - 1][ch].GaugeBearSay_3;

            //        vm.Save_All_PD_Value[ch] = vm.list_collection_GaugeModels[vm.bear_say_now - 1][ch].DataPoints;
            //        vm.Chart_All_DataPoints = new List<List<DataPoint>>(vm.Save_All_PD_Value);
            //        vm.Chart_DataPoints = new List<DataPoint>(vm.Chart_All_DataPoints[0]);  //A lineseries
            //    }
            //}

            try
            {
                if (vm.int_chart_now > 1)
                {
                    vm.int_chart_now--;
                    vm.Chart_All_DataPoints = new List<List<OxyPlot.DataPoint>>(vm.Chart_All_Datapoints_History[vm.int_chart_now - 1]);
                    if (vm.Chart_All_DataPoints.Count == 0) return;
                    vm.Chart_DataPoints = new List<OxyPlot.DataPoint>(vm.Chart_All_DataPoints[0]);

                    vm.ChartNowModel = vm.list_ChartModels[vm.int_chart_now - 1];

                    vm.Chart_x_title = vm.ChartNowModel.title_x;
                    vm.Chart_y_title = vm.ChartNowModel.title_y;

                    vm.msgModel.msg_3 = Math.Round(vm.ChartNowModel.TimeSpan, 1).ToString();

                    for (int i = 0; i < vm.ch_count; i++)
                    {
                        vm.list_GaugeModels[i].GaugeBearSay_1 = vm.list_collection_GaugeModels[vm.bear_say_now - 1][i].GaugeBearSay_1;
                        vm.list_GaugeModels[i].GaugeBearSay_2 = vm.list_collection_GaugeModels[vm.bear_say_now - 1][i].GaugeBearSay_2;
                        vm.list_GaugeModels[i].GaugeBearSay_3 = vm.list_collection_GaugeModels[vm.bear_say_now - 1][i].GaugeBearSay_3;

                        if (i < vm.list_ch_title.Count)
                            if (vm.list_ChartModels.Count > (vm.int_chart_now - 1))
                            {
                                if (i <= vm.list_ch_title.Count - 1 && i <= vm.ChartNowModel.list_delta_IL.Count)
                                    vm.list_ch_title[i] = string.Format("ch{0} ,Delta IL : {1}", i + 1, vm.ChartNowModel.list_delta_IL[i]);
                            }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.StackTrace.ToString());
            }
        }

        List<List<string>> temp_list_bear_say = new List<List<string>>();
        private void Btn_page2_Click(object sender, RoutedEventArgs e)
        {
            //if (vm.station_type == "Hermetic_Test")   //to page 2
            //{
            //    if (vm.List_bear_say == null) return;
            //    if (vm.List_bear_say.Count < 9) return;
            //    if (page_now == 2) return;
            //    page_now = 2;

            //    temp_list_bear_say = vm.List_bear_say;
            //    vm.List_bear_say = Analysis.ListDefine<string>(new List<List<string>>(), vm.ch_count, new List<string>());
            //    for (int ch = 9; ch <= 12; ch++)
            //    {
            //        vm.List_bear_say[ch - 9] = temp_list_bear_say[ch - 1];
            //    }
            //    vm.List_bear_say = new List<List<string>>(vm.List_bear_say);

            //    vm.txt_No = new string[] { "Ch 9", "Ch 10", "Ch 11", "Ch 12", "", "", "", "" };
            //}
        }

        private void Btn_page1_Click(object sender, RoutedEventArgs e)
        {
            //if (vm.station_type == "Hermetic_Test")   //to page 1
            //{
            //    if (vm.List_bear_say == null) return;
            //    if (page_now == 1) return;
            //    page_now = 1;

            //    vm.List_bear_say = temp_list_bear_say;

            //    vm.txt_No = new string[] { "Ch 1", "Ch 2", "Ch 3", "Ch 4", "Ch 5", "Ch 6", "Ch 7", "Ch 8" };
            //}
        }

        private void Border_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Border border = (Border)sender;
            border.Focus();
        }

        //int page_now = 1;

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

                //vm.Collection_bear_say = new List<List<List<string>>>();
                //vm.Collection_bear_say.Add(collection);  //只讀最後一項
                vm.List_bear_say = collection;

                //vm.bear_say_all = vm.Collection_bear_say.Count;
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

        ICommunication icomm;
        DiCon.UCB.Communication.RS232.RS232 rs232;
        DiCon.UCB.MTF.IMTFCommand tf;
          
       
        private void btn_bearsay_visual_Click(object sender, RoutedEventArgs e)
        {
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
            if (!vm.station_type.Equals("Hermetic_Test")) return;

            Button obj = (Button)sender;

            GaugeModel gm = (GaugeModel)obj.DataContext;

            try
            {
                if (!string.IsNullOrEmpty(gm.GaugeBearSay_1))
                {
                    double bearWL = 0;
                    if (double.TryParse(gm.GaugeBearSay_1, out bearWL))
                    {
                        #region set WL
                        double wl = bearWL;

                        if (wl >= 1570)
                            vm.float_TLS_WL_Range = new float[2] { 1523, 1620 };
                        else
                            vm.float_TLS_WL_Range = new float[2] { 1523, 1573 };

                        #region switch re-open
                        try
                        {
                            await vm.Port_Switch_ReOpen();
                        }
                        catch (Exception ex)
                        {
                            vm.Str_cmd_read = "Switch Port re-open Error";
                            vm.Save_Log(vm.Str_Status, gm.GaugeChannel, vm.Str_cmd_read);
                            MessageBox.Show(ex.StackTrace.ToString());
                            return;
                        }
                        #endregion

                        //if (vm.PD_or_PM && vm.IsGoOn) await vm.PM_Stop();
                        vm.Double_Laser_Wavelength = wl;
                        if (!vm.IsGoOn) cmd.Set_WL(Convert.ToDouble(txt_WL.Text), true);
                        else
                            vm.Save_cmd(new ComMember() { YN = true, No = vm.Cmd_Count.ToString(), Command = "SETWL", Value_1 = wl.ToString() });

                        #region Switch write Cmd
                        try
                        {
                            vm.Str_Command = "SW0 " + gm.GaugeChannel;
                            vm.port_Switch.Write(vm.Str_Command + "\r");
                            //await vm.AccessDelayAsync(vm.Int_Write_Delay);

                        }
                        catch { vm.Str_cmd_read = "Set Switch Error"; vm.Save_Log(vm.Str_Status, gm.GaugeChannel, vm.Str_cmd_read); }
                        #endregion

                        #endregion

                        int preSwitchIndex = vm.switch_index;
                        vm.switch_index = int.Parse(gm.GaugeChannel);
                        vm.ch = int.Parse(gm.GaugeChannel);   //Save Switch channel

                        if (vm.port_Switch.IsOpen)
                        {
                            vm.port_Switch.DiscardInBuffer();
                            vm.port_Switch.DiscardOutBuffer();

                            vm.port_Switch.Close();
                        }

                        vm.list_GaugeModels[preSwitchIndex - 1].GaugeValue = string.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.StackTrace.ToString())
;
            }
        }

        private async void Btn_Voltage_Click(object sender, RoutedEventArgs e)
        {
            if (vm.station_type != "Hermetic_Test") return;

            Button obj = (Button)sender;

            GaugeModel gm = (GaugeModel)obj.DataContext;

            try
            {
                bool _isGoOn = vm.IsGoOn;

                if (!vm.IsDistributedSystem)
                {
                    if (vm.PD_or_PM && vm.IsGoOn)
                        await vm.PM_Stop();
                }

                if (!vm.PD_or_PM)  //PD mode
                {
                    if (vm.IsGoOn)
                    {
                        switch (gm.GaugeD0_Select)
                        {
                            case "1":
                                vm.Save_cmd(new ComMember()
                                {
                                    No = vm.Cmd_Count.ToString(),
                                    Type = "PD",
                                    Comport = vm.Selected_Comport,
                                    Channel = gm.GaugeChannel,
                                    Command = "WRITEDAC",
                                    Value_1 = gm.GaugeD0_1,
                                    Value_2 = "0",
                                    Value_3 = gm.GaugeD0_3
                                });
                                break;
                            case "2":
                                vm.Save_cmd(new ComMember()
                                {
                                    No = vm.Cmd_Count.ToString(),
                                    Type = "PD",
                                    Comport = vm.Selected_Comport,
                                    Channel = gm.GaugeChannel,
                                    Command = "WRITEDAC",
                                    Value_1 = "0",
                                    Value_2 = gm.GaugeD0_2,
                                    Value_3 = gm.GaugeD0_3
                                });
                                //vm.Save_Command(vm.Cmd_Count, "", "PD", vm.Selected_Comport, gm.GaugeChannel, "WRITEDAC", "0", gm.GaugeD0_2, gm.GaugeD0_3, "", "", "");
                                break;
                            case "3":
                                //string[] preDac = await anly.Analyze_PreDAC(vm.Selected_Comport, gm.GaugeChannel);
                                vm.Save_cmd(new ComMember()
                                {
                                    No = vm.Cmd_Count.ToString(),
                                    Type = "PD",
                                    Comport = vm.Selected_Comport,
                                    Channel = gm.GaugeChannel,
                                    Command = "WRITEDAC",
                                    Value_3 = gm.GaugeD0_3
                                });
                                break;
                            default:
                                vm.Str_cmd_read = "Selected Dac Textbox error";
                                return;
                        }
                    }
                    else  //PD mode , Go off
                    {
                        switch (gm.GaugeD0_Select)
                        {
                            case "1":
                                vm.Cmd_WriteDac(gm.GaugeChannel, gm.GaugeD0_1, "0", gm.GaugeD0_3);  //Write Dac
                                break;

                            case "2":
                                vm.Cmd_WriteDac(gm.GaugeChannel, "0", gm.GaugeD0_2, gm.GaugeD0_3);  //Write Dac
                                break;

                            case "3":
                                string[] preDac = await anly.Analyze_PreDAC(vm.Selected_Comport, gm.GaugeChannel);
                                vm.Cmd_WriteDac(gm.GaugeChannel, preDac[0], preDac[1], gm.GaugeD0_3);  //Write Dac
                                break;
                        }
                    }
                }
                else if (vm.PD_or_PM)
                {
                    if (!vm.IsGoOn)
                    {
                        //gm.GaugeD0_3 = gm.GaugeBearSay_3;
                        gm.GaugeD0_Select = "3";
                        cmd.Set_Dac(vm.Selected_Comport, gm);
                    }
                    else
                    {
                        string D0_1, D0_2;
                        D0_1 = gm.GaugeD0_1;
                        D0_2 = gm.GaugeD0_2;

                        if (gm.GaugeD0_Select == "1")
                            D0_2 = "0";
                        else if (gm.GaugeD0_Select == "2")
                            D0_1 = "0";
                        else if (gm.GaugeD0_Select == "3")
                        {
                            //Ask DAC? first
                            try
                            {
                                await vm.Port_ReOpen(vm.Selected_Comport);
                                if (vm.port_PD.IsOpen)
                                {
                                    string cmd = "D1?";
                                    vm.port_PD.Write(cmd + "\r");

                                    await vm.AccessDelayAsync(vm.Int_Read_Delay);

                                    int size = vm.port_PD.BytesToRead;
                                    byte[] dataBuffer = new byte[size];
                                    if (size > 0)
                                    {
                                        dataBuffer = new byte[size];
                                        int length = vm.port_PD.Read(dataBuffer, 0, size);
                                    }

                                    //資料分析並顯示於狀態列
                                    anly.Read_analysis(cmd, dataBuffer);

                                    #region Analyze Dx? and show data                                        
                                    string[] words = vm.Str_cmd_read.Split(',');  //V1,V2,V3 
                                    if (words.Length >= 2)
                                    {
                                        int v1, v2;
                                        if (int.TryParse(words[0], out v1) && int.TryParse(words[1], out v2))
                                        {
                                            //If V1(or V2) is not equal to zero, keep it as usual
                                            if (v1 == 0) D0_1 = "0";
                                            else D0_2 = "0";
                                        }
                                    }
                                    #endregion
                                }
                            }
                            catch { }
                        }

                        vm.Save_cmd(new ComMember()
                        {
                            YN = true,
                            No = vm.Cmd_Count.ToString(),
                            Type = "PM",
                            Comport = vm.Selected_Comport,
                            Command = CommandList.WriteDac,
                            Value_1 = D0_1,
                            Value_2 = D0_2,
                            Value_3 = gm.GaugeD0_3
                        });
                    }
                }
            }
            catch
            {
                vm.Str_cmd_read = "Write DAC error.";
            }
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

        private async void gauge_PreviewMouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            if (vm.station_type != "Hermetic_Test") return;

            if (!vm.Is_switch_mode) return;

            ToggleButton obj = (ToggleButton)sender;
            obj.IsChecked = !obj.IsChecked;

            GaugeModel gm = (GaugeModel)obj.DataContext;

            //Switch change
            int switch_index;
            if (gm.GaugeChannel != null)
            {
                if (!int.TryParse(gm.GaugeChannel, out switch_index)) return;

                int pre_ch = vm.switch_index;

                if (string.IsNullOrWhiteSpace("SW0 " + gm.GaugeChannel)) //Check comment box is empty or not
                    return;

                if (vm.IsGoOn)
                {
                    ComMember cm = new ComMember();
                    cm.Command = "SETSWITCH";
                    cm.Channel = gm.GaugeChannel;
                    cm.Comport = vm.Comport_Switch;
                    vm.Save_cmd(cm);
                }
                else
                {
                    #region switch re-open
                    try
                    {
                        await vm.Port_Switch_ReOpen();
                    }
                    catch (Exception ex)
                    {
                        vm.Str_cmd_read = "Switch Port re-open Error";
                        vm.Save_Log(vm.Str_Status, switch_index.ToString(), vm.Str_cmd_read);
                        MessageBox.Show(ex.StackTrace.ToString());
                        return;
                    }
                    #endregion

                    #region Switch write Cmd
                    if (switch_index > 0)   //Switch 1~12
                    {
                        try
                        {
                            if (vm.port_Switch.IsOpen)
                            {
                                vm.Str_Command = "SW0 " + switch_index.ToString();
                                vm.port_Switch.Write(vm.Str_Command + "\r");
                                await Task.Delay(vm.Int_Write_Delay);

                                vm.switch_index = switch_index;
                                vm.ch = switch_index - 1;   //Save Switch channel

                                vm.port_Switch.DiscardInBuffer();       // RX
                                vm.port_Switch.DiscardOutBuffer();      // TX

                                vm.port_Switch.Close();

                                vm.switch_index = switch_index;
                            }
                        }
                        catch (Exception ex) { vm.Str_cmd_read = "Set Switch Error"; vm.Save_Log(vm.Str_Status, switch_index.ToString(), vm.Str_cmd_read); MessageBox.Show(ex.StackTrace.ToString()); }
                    }
                    #endregion

                    System.Threading.Thread.Sleep(80);
                }


                #region ReSet other channel gauge
                //foreach (GaugeModel GM in vm.list_GaugeModels)
                //{
                //    if (GM.GaugeChannel != gm.GaugeChannel)
                //    {
                //        GM.GaugeValue = "";
                //    }
                //}
                //if (pre_ch != switch_index)
                //    vm.list_GaugeModels[pre_ch - 1].GaugeValue = "-64";
                #endregion
            }
        }

        private void Btn_update_Click(object sender, RoutedEventArgs e)
        {
            MenuItem obj = (MenuItem)sender;
            GaugeModel gm = (GaugeModel)obj.DataContext;

            gm.GaugeBearSay_1 = txt_WL.Text;

            return;


        }

        private void txt_power_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            TextBox obj = (TextBox)sender;

            if (e.Key == Key.Enter)
            {
                try
                {
                    if (!vm.IsGoOn)
                    {
                        vm.Double_Laser_Power = Convert.ToDouble(obj.Text);

                        cmd.Set_TLS_Power(vm.Double_Laser_Power, true);

                        //if (vm.Laser_type.Equals("Agilent"))
                        //    vm.tls.SetPower(vm.Double_Laser_Power);
                        //else if (vm.Laser_type.Equals("Golight"))
                        //    vm.tls_GL.SetPower(vm.Double_Laser_Power);
                    }
                    else
                        vm.Save_cmd(new ComMember() { YN = true, No = vm.Cmd_Count.ToString(), Command = "SETPOWER", Type = "Agilent", Value_1 = obj.Text });
                }
                catch { }
            }
            if (e.Key == Key.Up)
            {
                vm.Double_Laser_Power += 0.1;
                if (!vm.IsGoOn)
                    try
                    {
                        vm.tls.SetPower(vm.Double_Laser_Power);
                        obj.Text = vm.Double_Laser_Power.ToString();
                    }
                    catch { }
                else
                    vm.Save_cmd(new ComMember() { YN = true, No = vm.Cmd_Count.ToString(), Command = "SETPOWER", Type = "Agilent", Value_1 = vm.Double_Laser_Power.ToString() });

                obj.Text = vm.Double_Laser_Power.ToString();
            }
            if (e.Key == Key.Down)
            {
                vm.Double_Laser_Power -= 0.1;

                if (!vm.IsGoOn)
                    try
                    {
                        vm.tls.SetPower(vm.Double_Laser_Power);

                    }
                    catch { }
                else
                    vm.Save_cmd(new ComMember() { YN = true, No = vm.Cmd_Count.ToString(), Command = "SETPOWER", Type = "Agilent", Value_1 = vm.Double_Laser_Power.ToString() });

                obj.Text = vm.Double_Laser_Power.ToString();
            }
        }

        private void gauge_PreviewMouseRightButtonDown(object sender, RoutedEventArgs e)
        {
            //vm.is_Gauge_ContinueSelect = true;
            foreach (GaugeModel gm in vm.list_GaugeModels)
            {
                gm.GaugeContinueSelect = true;
            }
            vm.BoolAllGauge = vm.list_GaugeModels.Where(x => x.boolGauge).ToList().Count == 0 ? true : false;
        }

        private void gauge_PreviewMouseRightButtonUp(object sender, RoutedEventArgs e)
        {
            foreach (GaugeModel gm in vm.list_GaugeModels)
            {
                gm.GaugeContinueSelect = false;
            }
            vm.BoolAllGauge = vm.list_GaugeModels.Where(x => x.boolGauge).ToList().Count == 0 ? true : false;
        }

        private void UserControl1_MouseEnter(object sender, MouseEventArgs e)
        {
            Circle_Gauge.UserControl1 uc = (Circle_Gauge.UserControl1)sender;
            GaugeModel gm = (GaugeModel)uc.DataContext;
            if (gm.GaugeContinueSelect)
            {
                gm.boolGauge = !gm.boolGauge;
                vm.Str_cmd_read = gm.GaugeChannel;
                vm.BoolAllGauge = vm.list_GaugeModels.Where(x => x.boolGauge).ToList().Count == 0 ? true : false;
            }
        }

        private void UserControl1_TxtBox_SN_PreviewKeyDown(object sender, RoutedEventArgs e)
        {
            if (!vm.SN_AutoTab) return;

            TextBox txtBox_SN = (TextBox)sender;

            txtBox_SN.PreviewKeyDown += TxtBox_SN_PreviewKeyDown;
        }

        private void TxtBox_SN_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TraversalRequest request = new TraversalRequest(FocusNavigationDirection.Next);
                request.Wrapped = true;
                ((TextBox)sender).MoveFocus(request);
            }
        }

        private void ToggleBtn_LaserActive_Checked(object sender, RoutedEventArgs e)
        {
            cmd.Set_TLS_Active(vm.isLaserActive);
        }

        public event RoutedEventHandler K_WL_Event;
        private async void Button_WL_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (int.TryParse(btn.Tag.ToString(), out int no))
                vm.opModel_1.WL_No = no;
            else return;

            #region Clear value of opModel

            foreach (var item in vm.props_opModel)
            {
                var keyName = item.Name;

                if (keyName.Contains($"WL_{no}_"))
                {
                    Console.WriteLine(item.Name);
                    Console.WriteLine(item.PropertyType.Name);
                    var obj = item.GetValue(vm.opModel_1, null);
                    Console.WriteLine(obj.ToString());

                    if (keyName.Contains("IL"))
                        item.SetValue(vm.opModel_1, "0", null);
                    else
                        item.SetValue(vm.opModel_1, 0, null);

                    Console.WriteLine(obj.ToString());
                }
            }

            #endregion

            if(!string.IsNullOrEmpty(vm.opModel_1.SN))
                if(vm.opModel_1.SN.Length > 5)
                {
                    if (vm.opModel_1.SN[6] == 'A')
                        Read_TF2_WL_Setting(vm.csv_product_TF2_wl_setting_path, no+3);
                    else if(vm.opModel_1.SN[6] == 'B')
                        Read_TF2_WL_Setting(vm.csv_product_TF2_wl_setting_path, no);
                }

            await Task.Delay(200);

            if (no > 4 || no < 0) return;

            if (K_WL_Event != null)
            {
                K_WL_Event(this, e);  // K_WL
            }
        }

        public async Task<bool> Cal_PDL_Show()
        {
            if (!vm.opModel_1.Is_PDL_Auto_Scan) return false;

            vm.Str_Status = "Scan PDL";

            double Delta_IL = await cmd.PDL_Cal(vm.opModel_1.PDL_Scan_Time);

            int wl_no = vm.opModel_1.WL_No;

            if (wl_no == 1)
                vm.opModel_1.WL_1_PDL = Delta_IL;
            else if (wl_no == 2)
                vm.opModel_1.WL_2_PDL = Delta_IL;
            else if (wl_no == 3)
                vm.opModel_1.WL_3_PDL = Delta_IL;
            else if (wl_no == 4)
                vm.opModel_1.WL_4_PDL = Delta_IL;

            vm.Str_cmd_read = "PDL Scan Stop";

            return true;
        }

        private void Read_TF2_WL_Setting(string FilePath, int WL_No)
        {
            try
            {
                if (File.Exists(FilePath))
                    using (DataTable dtt = CSVFunctions.Read_CSV(FilePath))
                    {
                        if (dtt.Rows.Count > 0 && dtt.Rows.Count > WL_No)
                        {
                            if (dtt.Columns.Count >= 5)
                                if (dtt.Rows.Count > WL_No)
                                {
                                    string str_wl = dtt.Rows[WL_No][1].ToString();
                                    string str_start = dtt.Rows[WL_No][2].ToString();
                                    string str_end = dtt.Rows[WL_No][3].ToString();
                                    //string str_gap = dtt.Rows[WL_No][4].ToString();

                                    if (WL_No == 1)
                                        vm.opModel_1.WL_Setting_1 = str_wl + "_nm";
                                    else if (WL_No ==2)
                                        vm.opModel_1.WL_Setting_2 = str_wl + "_nm";
                                    else if (WL_No == 3)
                                        vm.opModel_1.WL_Setting_3 = str_wl + "_nm";

                                    if (double.TryParse(str_start, out double start))
                                        vm.float_WL_Scan_Start = start;

                                    if (double.TryParse(str_end, out double end))
                                        vm.float_WL_Scan_End = end;

                                    //if (double.TryParse(str_gap, out double gap))
                                    //    vm.float_WL_Scan_Gap = gap;
                                }
                        }
                    }
            }
            catch
            {
            }
        }

        private async void Button_PDL_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var btn = sender as Button;
                int wl_no;
                if (int.TryParse(btn.Tag.ToString(), out int no)) wl_no = no;
                else
                {
                    vm.Str_cmd_read = "InValid Tag of the selected button";
                    return;
                }

                vm.isStop = false;

                double Delta_IL = await cmd.PDL_Cal(vm.opModel_1.PDL_Scan_Time);

                foreach (var item in vm.props_opModel)
                {
                    if (item.Name == $"WL_{wl_no}_PDL")
                        item.SetValue(vm.opModel_1, Delta_IL);
                }

                vm.Str_cmd_read = "PDL Scan Stop";
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.StackTrace.ToString());
            }
        }

        private void TextBox_WL_Scan_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;

            var txtbox = sender as TextBox;

            if(txtbox.Tag.ToString() == "Start")
            {
                if (double.TryParse(txtbox.Text, out double wl))
                    vm.float_WL_Scan_Start = wl;
            }
            else if (txtbox.Tag.ToString() == "End")
            {
                if (double.TryParse(txtbox.Text, out double wl))
                    vm.float_WL_Scan_End = wl;
            }
            else if (txtbox.Tag.ToString() == "Gap")
            {
                if (double.TryParse(txtbox.Text, out double wl))
                    vm.float_WL_Scan_Gap = wl;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Type typ1 = vm.opModel_1.GetType();
            vm.props_opModel = typ1.GetProperties(BindingFlags.Instance | BindingFlags.Public); // Get all properties in opModel
            vm.csv_product_TF2_wl_setting_path = System.IO.Path.Combine(vm.CurrentPath, "Product_TF2_WL.csv");
        }
        private void Sector_BTN_PDL_IsAutoCal_Click(object sender, RoutedEventArgs e)
        {
            vm.opModel_1.Is_PDL_Auto_Scan = !vm.opModel_1.Is_PDL_Auto_Scan;
        }
        private void Sector_BTN_PDL_Connect_Click(object sender, RoutedEventArgs e)
        {
            cmd.PDL_Connect();
        }

        private void Sector_BTN_PDL_Stop_Click(object sender, RoutedEventArgs e)
        {
            cmd.PDL_Stop();
        }

        private void Sector_BTN_PDL_Start_Click(object sender, RoutedEventArgs e)
        {
            cmd.PDL_Start();
        }

        private void btn_Open_Location_Click(object sender, RoutedEventArgs e)
        {
            string path = Path.Combine(vm.txt_save_TF2_wl_data_path);
            if (Directory.Exists(path))
                System.Diagnostics.Process.Start(path);
        }

    }
}
