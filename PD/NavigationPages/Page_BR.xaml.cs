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
using DiCon.Instrument.HP;
using PD.Functions;
using PD.AnalysisModel;
using PD.ViewModel;
using PD.Models;
using PD.UI;
using DiCon.UCB.Communication;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;
using OxyPlot.Annotations;
using ValueGetter;

namespace PD.NavigationPages
{
    /// <summary>
    /// Page_PD_Gauges.xaml 的互動邏輯
    /// </summary>
    public partial class Page_BR : UserControl
    {
        ComViewModel vm;
        ControlCmd cmd;
        TextBox obj;
        string[] ch_v = new string[2];
        bool _isDrag = false;
        Analysis anly;

        bool isRightMouseinPlot = false;
        bool _is_txtWL_already_click = false;

        //readonly public System.Windows.Media.Animation.Storyboard sb_bear_shake;
        //readonly public System.Windows.Media.Animation.Storyboard sb_bear_reset;

        //List<string> WL_Special_List;
        public Dictionary<string, string> dict_WL_to_Dac;

        public Page_BR(ComViewModel vm)
        {
            InitializeComponent();

            this.vm = vm;
            this.DataContext = vm;

            cmd = new ControlCmd(vm);

            grid_bear_say.DataContext = vm;
            viewer.DataContext = vm;  //將DataContext指給使用者控制項，必要!

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

            //vm.PlotViewModel.MouseMove += PlotViewModel_MouseMove;
            mainPlotView.PreviewMouseLeftButtonDown += MainPlotView_PreviewMouseLeftButtonDown;
            mainPlotView.PreviewMouseRightButtonDown += MainPlotView_PreviewMouseRightButtonDown;
            mainPlotView.PreviewMouseRightButtonUp += MainPlotView_PreviewMouseRightButtonUp;
            vm.PlotViewModel.MouseDown += PlotViewModel_MouseDown;
            //vm.PlotViewModel.MouseUp += PlotViewModel_MouseUp;

        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //一個plotModel同一時間只能供給一個plotview使用，頁面切換時需重新指派
            vm.PlotViewModel_Chart = new PlotModel();
            vm.PlotViewModel_Testing = new PlotModel();
            vm.PlotViewModel_TF2 = new PlotModel();
            vm.PlotViewModel_UTF600 = new PlotModel();
            vm.PlotViewModel_BR = vm.PlotViewModel;
            vm.PlotViewModel.IsLegendVisible = false;
            //vm.PlotViewModel.LegendPlacement = LegendPlacement.Outside;

            vm.Update_ALL_PlotView();

            vm.sb_bear_shake = FindResource("Storyboard_Bear_Shake") as System.Windows.Media.Animation.Storyboard;
            vm.sb_bear_reset = FindResource("Storyboard_Bear_Reset") as System.Windows.Media.Animation.Storyboard;

            if (vm.sb_bear_shake != null)
                vm.sb_bear_shake.Begin();

            if (vm.sb_bear_reset != null)
            {
                vm.sb_bear_reset.Begin();
                vm.sb_bear_reset.Pause();
            }

            vm.WL_Special_List = new List<string>();
            dict_WL_to_Dac = new Dictionary<string, string>();

            if (double.TryParse(vm.Ini_Read("Scan", "BR_Diff"), out double dif))
                vm.BR_Diff = dif;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            vm.PlotViewModel.LegendPlacement = LegendPlacement.Inside;
        }


        private void MainPlotView_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Released)
            {
                isRightMouseinPlot = false;
            }
        }

        private void MainPlotView_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!vm.PlotViewModel.Axes[0].Title.Contains("Wavelength") && !vm.PlotViewModel.Axes[0].Title.Contains("nm")) return;

            if (e.RightButton == MouseButtonState.Pressed)
                isRightMouseinPlot = true;
        }

        bool isLeftMouseinPlot = false;
        private void MainPlotView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!vm.PlotViewModel.Axes[0].Title.Contains("Wavelength") && !vm.PlotViewModel.Axes[0].Title.Contains("nm")) return;

            if (e.LeftButton == MouseButtonState.Pressed)
                isLeftMouseinPlot = true;
            else
                isLeftMouseinPlot = false;
        }

        //private void PlotViewModel_MouseMove(object sender, OxyMouseEventArgs e)
        //{
        //    if (!vm.isMouseSelecte_WLScanRange || isRightMouseinPlot) return;

        //    var position = Axis.InverseTransform(e.Position, vm.PlotViewModel.Axes[0], vm.PlotViewModel.Axes[1]);

        //    if (!isPlotMouseDown)
        //    {
        //        vm.Str_cmd_read = $"WL Start :{Math.Round(position.X, 2)}";
        //    }
        //    else
        //    {
        //        vm.Str_cmd_read = $"WL End :{Math.Round(position.X, 2)}";
        //    }
        //}

        bool isPlotMouseDown = false;
        private void PlotViewModel_MouseDown(object sender, OxyMouseDownEventArgs e)
        {
            if (!vm.isMouseSelecte_WLScanRange || !isLeftMouseinPlot) return;

            if (!vm.PlotViewModel.Axes[0].Title.Contains("Wavelength") && !vm.PlotViewModel.Axes[0].Title.Contains("nm")) return;

            isPlotMouseDown = !isPlotMouseDown;

            var position = Axis.InverseTransform(e.Position, vm.PlotViewModel.Axes[0], vm.PlotViewModel.Axes[1]);

            vm.Str_cmd_read = $"x:{Math.Round(position.X, 2)}, y:{Math.Round(position.Y, 2)}";

            vm.PlotViewModel.InvalidatePlot(true);

            if (isPlotMouseDown)
            {
                vm.LineAnnotation_X_1.X = Math.Round(position.X, 2);
                vm.LineAnnotation_X_1.StrokeThickness = 2.3;
                vm.float_WL_Scan_Start = Math.Round(position.X, 2);
            }
            else
            {
                vm.LineAnnotation_X_2.X = Math.Round(position.X, 2);
                vm.LineAnnotation_X_2.StrokeThickness = 2.3;
                vm.float_WL_Scan_End = Math.Round(position.X, 2);
                //vm.isMouseSelecte_WLScanRange = false;
            }

            isLeftMouseinPlot = false;
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
                        if (vm.PD_or_PM && vm.IsGoOn)
                            await vm.PM_Stop();

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
                            cmd.Set_Dac(vm.Selected_Comport, gm);
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
        }

        private void tbtn_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (vm.station_type != ComViewModel.StationTypes.Testing) return;
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

        private async void slider_WL_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (vm.isConnected)
            {
                if (!vm.IsGoOn) await cmd.Set_WL(Convert.ToDouble(txt_WL.Text), true);
                else
                    vm.Save_cmd(new ComMember() { YN = true, No = vm.Cmd_Count.ToString(), Command = "SETWL", Value_1 = txt_WL.Text });
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


        private async void txt_Dac_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            TextBox obj = (TextBox)sender;
            double textbox_value;

            if (!double.TryParse(obj.Text, out textbox_value)) return;

            if (!vm.isDACorVolt)
            {
                if (vm.ch_count == 1)
                {
                    int v3_index = int.Parse(vm.list_GaugeModels[0].GaugeD0_3);
                    if (e.Key == Key.Up)
                        v3_index += 100;
                    else if (e.Key == Key.Down)
                        v3_index -= 100;

                    vm.list_GaugeModels[0].GaugeD0_3 = (v3_index).ToString();


                    if (!vm.IsGoOn)
                        cmd.Set_Dac(vm.Selected_Comport, vm.list_GaugeModels[0]);
                    else
                    {
                        string D0_1, D0_2;
                        D0_1 = vm.list_GaugeModels[0].GaugeD0_1;
                        D0_2 = vm.list_GaugeModels[0].GaugeD0_2;

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

                        vm.Save_cmd(new ComMember()
                        {
                            YN = true,
                            No = vm.Cmd_Count.ToString(),
                            Type = "PM",
                            Comport = vm.Selected_Comport,
                            Command = CommandList.WriteDac,
                            Value_1 = D0_1,
                            Value_2 = D0_2,
                            Value_3 = vm.list_GaugeModels[0].GaugeD0_3
                        });
                    }

                    vm.int_Dac_cmd = v3_index;
                }
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

        //private void btn_aft_Click(object sender, RoutedEventArgs e)
        //{         
        //    try
        //    {
        //        if (vm.int_chart_now >= vm.int_chart_count)
        //            return;

        //        if (vm.Chart_All_Datapoints_History.Count <= vm.int_chart_now)
        //        {
        //            vm.Chart_All_DataPoints = new List<List<OxyPlot.DataPoint>>(vm.Chart_All_Datapoints_History.Last());
        //        }
        //        else
        //            vm.Chart_All_DataPoints = new List<List<OxyPlot.DataPoint>>(vm.Chart_All_Datapoints_History[vm.int_chart_now]);

        //        if (vm.Chart_All_DataPoints.Count != 0)
        //            vm.Chart_DataPoints = new List<OxyPlot.DataPoint>(vm.Chart_All_DataPoints[0]);  //Update Chart UI

        //        vm.int_chart_now++;

        //        vm.ChartNowModel = vm.list_ChartModels[vm.int_chart_now - 1];

        //        vm.Chart_x_title = vm.ChartNowModel.title_x;
        //        vm.Chart_y_title = vm.ChartNowModel.title_y;

        //        vm.msgModel.msg_3 = Math.Round(vm.ChartNowModel.TimeSpan, 1).ToString();

        //        for (int i = 0; i < vm.ch_count; i++)
        //        {
        //            vm.list_GaugeModels[i].GaugeBearSay_1 = vm.list_collection_GaugeModels[vm.int_chart_now - 1][i].GaugeBearSay_1;
        //            vm.list_GaugeModels[i].GaugeBearSay_2 = vm.list_collection_GaugeModels[vm.int_chart_now - 1][i].GaugeBearSay_2;
        //            vm.list_GaugeModels[i].GaugeBearSay_3 = vm.list_collection_GaugeModels[vm.int_chart_now - 1][i].GaugeBearSay_3;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        System.Windows.MessageBox.Show(ex.StackTrace.ToString());
        //    }
        //}

        //private void btn_pre_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        if (vm.int_chart_now > 1)
        //        {
        //            vm.int_chart_now--;
        //            vm.Chart_All_DataPoints = new List<List<OxyPlot.DataPoint>>(vm.Chart_All_Datapoints_History[vm.int_chart_now - 1]);
        //            if (vm.Chart_All_DataPoints.Count == 0) return;
        //            vm.Chart_DataPoints = new List<OxyPlot.DataPoint>(vm.Chart_All_DataPoints[0]);

        //            vm.ChartNowModel = vm.list_ChartModels[vm.int_chart_now - 1];

        //            vm.Chart_x_title = vm.ChartNowModel.title_x;
        //            vm.Chart_y_title = vm.ChartNowModel.title_y;

        //            vm.msgModel.msg_3 = Math.Round(vm.ChartNowModel.TimeSpan, 1).ToString();

        //            for (int i = 0; i < vm.ch_count; i++)
        //            {
        //                vm.list_GaugeModels[i].GaugeBearSay_1 = vm.list_collection_GaugeModels[vm.int_chart_now - 1][i].GaugeBearSay_1;
        //                vm.list_GaugeModels[i].GaugeBearSay_2 = vm.list_collection_GaugeModels[vm.int_chart_now - 1][i].GaugeBearSay_2;
        //                vm.list_GaugeModels[i].GaugeBearSay_3 = vm.list_collection_GaugeModels[vm.int_chart_now - 1][i].GaugeBearSay_3;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        System.Windows.MessageBox.Show(ex.StackTrace.ToString());
        //    }
        //}

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

        private async void txt_V1_DAC_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextBox obj = (TextBox)sender;
                if (double.TryParse(obj.Text, out double volt))
                {
                    for (int i = 0; i < vm.list_GaugeModels.Count; i++)
                    {
                        vm.list_GaugeModels[i].GaugeD0_1 = obj.Text;
                        vm.list_GaugeModels[i].GaugeD0_Select = "1";

                        cmd.Set_Dac(vm.Selected_Comport, vm.list_GaugeModels[i]);

                        await Task.Delay(vm.Int_Read_Delay);
                    }
                }
            }
        }

        private async void txt_V2_DAC_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextBox obj = (TextBox)sender;
                if (double.TryParse(obj.Text, out double volt))
                {
                    for (int i = 0; i < vm.list_GaugeModels.Count; i++)
                    {
                        vm.list_GaugeModels[i].GaugeD0_2 = obj.Text;
                        vm.list_GaugeModels[i].GaugeD0_Select = "2";
                        cmd.Set_Dac(vm.Selected_Comport, vm.list_GaugeModels[i]);
                        await Task.Delay(vm.Int_Read_Delay);
                    }
                }
            }
        }

        private async void txt_V3_DAC_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextBox obj = (TextBox)sender;
                if (double.TryParse(obj.Text, out double volt))
                {
                    for (int i = 0; i < vm.list_GaugeModels.Count; i++)
                    {
                        vm.list_GaugeModels[i].GaugeD0_3 = obj.Text;
                        vm.list_GaugeModels[i].GaugeD0_Select = "3";
                        cmd.Set_Dac(vm.Selected_Comport, vm.list_GaugeModels[i]);
                        await Task.Delay(vm.Int_Read_Delay);
                    }
                }
            }
        }

        private async void Write_Dac_Volt(object sender, KeyEventArgs e, int v123)
        {
            try
            {
                //Dac
                if (!vm.isDACorVolt)
                {
                    if (e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Enter)
                    {
                        TextBox obj = (TextBox)sender;
                        double textbox_value;

                        if (!double.TryParse(obj.Text, out textbox_value)) return;

                        for (int i = 0; i < vm.list_GaugeModels.Count; i++)
                        {
                            if (!vm.list_GaugeModels[i].boolGauge && !vm.BoolAllGauge) continue;

                            if (vm.IsDistributedSystem) vm.Selected_Comport = vm.list_GaugeModels[i].chModel.Board_Port;

                            int dac_input = int.Parse(obj.Text);
                            if (e.Key == Key.Up)
                                dac_input += 100;
                            else if (e.Key == Key.Down)
                                dac_input -= 100;

                            if (!vm.IsGoOn)
                            {
                                //Ask DAC? first
                                await cmd.D0_show();

                                switch (v123)
                                {
                                    case 1:
                                        vm.list_GaugeModels[i].GaugeD0_1 = (dac_input).ToString();
                                        vm.list_GaugeModels[i].GaugeD0_2 = "0";
                                        break;
                                    case 2:
                                        vm.list_GaugeModels[i].GaugeD0_1 = "0";
                                        vm.list_GaugeModels[i].GaugeD0_2 = (dac_input).ToString();
                                        break;
                                    case 3:
                                        vm.list_GaugeModels[i].GaugeD0_3 = (dac_input).ToString();
                                        break;
                                }
                                vm.list_GaugeModels[i].GaugeD0_Select = v123.ToString();

                                cmd.Set_Dac(vm.Selected_Comport, vm.list_GaugeModels[i]);
                            }
                            else
                            {
                                vm.Save_cmd(new ComMember()
                                {
                                    YN = true,
                                    Channel = vm.list_GaugeModels[i].GaugeChannel,
                                    No = vm.Cmd_Count.ToString(),
                                    Type = "PM",
                                    Command = CommandList.DAC,
                                    Comport = vm.Selected_Comport
                                });

                                string[] dac_123 = new string[3] { vm.list_GaugeModels[i].GaugeD0_1, vm.list_GaugeModels[i].GaugeD0_2, vm.list_GaugeModels[i].GaugeD0_3 };

                                switch (v123)
                                {
                                    case 1:
                                        dac_123[1] = "0";
                                        break;
                                    case 2:
                                        dac_123[0] = "0";
                                        break;
                                }
                                dac_123[v123 - 1] = (dac_input).ToString();

                                vm.Save_cmd(new ComMember()
                                {
                                    YN = true,
                                    No = vm.Cmd_Count.ToString(),
                                    Type = "PM",
                                    Channel = vm.list_GaugeModels[i].GaugeChannel,
                                    Comport = vm.Selected_Comport,
                                    Command = CommandList.WriteDac,
                                    Value_1 = dac_123[0],
                                    Value_2 = dac_123[1],
                                    Value_3 = dac_123[2]
                                });
                            }
                        }
                    }
                }

                //Volt
                else
                {
                    if (e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Enter)
                    {
                        TextBox obj = (TextBox)sender;
                        double textbox_value;

                        if (!double.TryParse(obj.Text, out textbox_value)) return;

                        for (int i = 0; i < vm.list_GaugeModels.Count; i++)
                        {
                            if (!vm.list_GaugeModels[i].boolGauge && !vm.BoolAllGauge) continue;

                            if (vm.IsDistributedSystem) vm.Selected_Comport = vm.list_GaugeModels[i].chModel.Board_Port;

                            double volt_input = double.Parse(obj.Text);
                            if (e.Key == Key.Up)
                                volt_input += 0.1;
                            else if (e.Key == Key.Down)
                                volt_input -= 0.1;

                            if (!vm.IsGoOn)
                            {
                                //Ask DAC? first
                                await cmd.D0_show();

                                if (vm.board_read.Count == 0)
                                {
                                    vm.Str_cmd_read = "Get/Set Board ID First";
                                    vm.Save_Log(new LogMember
                                    {
                                        Message = vm.Str_cmd_read
                                    });

                                    await vm.Port_ReOpen(vm.Selected_Comport);

                                    await cmd.Cmd_Write_RecieveData("ID?", true, 0);

                                    #region Get Board Name     
                                    if (vm.Str_cmd_read == "DiCon Fiberoptics Inc, MEMS UFA")
                                    {
                                        try
                                        {
                                            rs232 = new DiCon.UCB.Communication.RS232.RS232(vm.Selected_Comport);
                                            rs232.OpenPort();
                                            icomm = (ICommunication)rs232;

                                            tf = new DiCon.UCB.MTF.RS232.RS232(icomm);

                                            string str_ID = string.Empty;

                                            str_ID = tf.ReadSN();
                                            vm.Str_Status = str_ID;

                                            if (vm.ch_count == 1 && !string.IsNullOrEmpty(str_ID) && vm.board_read.Count != 1)
                                            {
                                                vm.board_read.Clear();
                                                vm.board_read.Add(new List<string>());

                                                string board_id = str_ID;
                                                string path = Path.Combine(vm.txt_board_table_path, board_id + "-boardtable.txt");

                                                if (!File.Exists(path))
                                                {
                                                    vm.Str_cmd_read = "UFV Board table is not exist";
                                                    vm.Save_Log(new LogMember()
                                                    {
                                                        Channel = "1",
                                                        Status = "Write_Dac_Volt",
                                                        Message = "Board Table is not exit",
                                                        Result = $"{board_id}-boardtable.txt",
                                                        Date = DateTime.Now.Date.ToShortDateString(),
                                                        Time = DateTime.Now.ToLongTimeString()
                                                    });
                                                }
                                                else
                                                {
                                                    StreamReader str = new StreamReader(path);

                                                    while (true)  //Read board v3 data
                                                    {
                                                        string readline = str.ReadLine();

                                                        if (string.IsNullOrEmpty(readline)) break;

                                                        vm.board_read[0].Add(readline);
                                                    }
                                                    str.Close(); //(關閉str)
                                                }
                                            }

                                            rs232.ClosePort();
                                        }
                                        catch { }
                                    }
                                    #endregion

                                    if (vm.board_read.Count == 0) return;
                                }

                                #region Interpolation Dac

                                List<double> list_voltage = new List<double>();
                                List<int> list_dac = new List<int>();

                                int count = 0; double final_dac = 0;
                                foreach (string strline in vm.board_read[i])
                                {
                                    string[] board_read = strline.Split(',');
                                    if (board_read.Length <= 1)
                                        continue;

                                    double voltage = double.Parse(board_read[0]);
                                    int dac = int.Parse(board_read[1]);

                                    list_voltage.Add(Convert.ToDouble(voltage));
                                    list_dac.Add(dac);

                                    if (voltage >= volt_input && count > 0)
                                    {
                                        double delta_x = (volt_input - list_voltage[count - 1]);
                                        double delta_X = (list_voltage[count] - list_voltage[count - 1]);
                                        int delta_Y = (list_dac[count] - list_dac[count - 1]);
                                        final_dac = (Convert.ToDouble(delta_x) / Convert.ToDouble(delta_X)) * delta_Y + list_dac[count - 1];
                                        final_dac = Math.Round(final_dac, 0);

                                        if (final_dac < 0)
                                        {
                                            vm.Str_cmd_read = "Wrong Dac from board table";
                                            return;
                                        }

                                        switch (v123)
                                        {
                                            case 1:
                                                vm.list_GaugeModels[i].GaugeD0_1 = (final_dac).ToString();
                                                vm.list_GaugeModels[i].GaugeD0_2 = "0";
                                                break;
                                            case 2:
                                                vm.list_GaugeModels[i].GaugeD0_1 = "0";
                                                vm.list_GaugeModels[i].GaugeD0_2 = (final_dac).ToString();
                                                break;
                                            case 3:
                                                vm.list_GaugeModels[i].GaugeD0_3 = final_dac.ToString();
                                                break;
                                        }

                                        break;
                                    }

                                    count++;
                                }

                                if (volt_input != 0 && final_dac == 0)
                                {
                                    vm.Str_cmd_read = "No Dac in board table";
                                    return;
                                }

                                #endregion

                                vm.Save_Log(new LogMember()
                                {
                                    Status = vm.Str_Status,
                                    Message = vm.Str_cmd_read
                                });

                                vm.list_GaugeModels[i].GaugeD0_Select = v123.ToString();

                                cmd.Set_Dac(vm.Selected_Comport, vm.list_GaugeModels[i]);
                            }
                            else
                            {
                                string boardType = vm.list_GaugeModels[i].chModel.Board_Type;
                                string cmd_Type = vm.PD_or_PM ? "PD" : "PM";

                                vm.Save_cmd(new ComMember()
                                {
                                    YN = true,
                                    Channel = vm.list_GaugeModels[i].GaugeChannel,
                                    No = vm.Cmd_Count.ToString(),
                                    Type = cmd_Type,
                                    Command = CommandList.DAC,
                                    Comport = vm.Selected_Comport
                                });

                                if (vm.board_read[i].Count == 0)
                                {
                                    vm.Save_Log(new LogMember
                                    {
                                        Message = "Get/Set Board ID First"
                                    });

                                    vm.Save_cmd(new ComMember()
                                    {
                                        YN = true,
                                        Channel = vm.list_GaugeModels[i].GaugeChannel,
                                        No = vm.Cmd_Count.ToString(),
                                        Type = cmd_Type,
                                        Command = CommandList.SN,
                                        Comport = vm.Selected_Comport
                                    });

                                    vm.Save_cmd(new ComMember()
                                    {
                                        YN = true,
                                        No = vm.Cmd_Count.ToString(),
                                        Command = CommandList.Delay,
                                        Value_1 = "300"
                                    });

                                    vm.Save_cmd(new ComMember()
                                    {
                                        YN = true,
                                        Channel = vm.list_GaugeModels[i].GaugeChannel,
                                        No = vm.Cmd_Count.ToString(),
                                        Type = cmd_Type,
                                        Command = CommandList.GetBoardTable,
                                        Comport = vm.Selected_Comport
                                    });
                                }

                                ComMember cm = new ComMember()
                                {
                                    YN = true,
                                    No = vm.Cmd_Count.ToString(),
                                    Channel = vm.list_GaugeModels[i].GaugeChannel,
                                    Comport = vm.Selected_Comport,
                                    Command = CommandList.WriteVolt
                                };

                                switch (v123)
                                {
                                    case 1:
                                        cm.Value_1 = volt_input.ToString();
                                        break;
                                    case 2:
                                        cm.Value_2 = volt_input.ToString();
                                        break;
                                    case 3:
                                        cm.Value_3 = volt_input.ToString();
                                        break;
                                }

                                vm.Save_cmd(cm);
                            }
                        }
                    }
                }

                _is_txtWL_already_click = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.StackTrace.ToString());
            }
        }

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
            if (vm.station_type != ComViewModel.StationTypes.Hermetic_Test) return;

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
                        if (!vm.IsGoOn) await cmd.Set_WL(Convert.ToDouble(txt_WL.Text), true);
                        else
                            vm.Save_cmd(new ComMember() { YN = true, No = vm.Cmd_Count.ToString(), Command = "SETWL", Value_1 = wl.ToString() });

                        #region Switch write Cmd
                        try
                        {
                            vm.Str_Command = "SW0 " + gm.GaugeChannel;
                            vm.port_Switch.Write(vm.Str_Command + "\r");
                            //await Task.Delay(vm.Int_Write_Delay);

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
                MessageBox.Show(ex.StackTrace.ToString());
            }
        }

        private async void Btn_Voltage_Click(object sender, RoutedEventArgs e)
        {
            if (vm.station_type != ComViewModel.StationTypes.Testing) return;

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
                                break;
                            case "3":
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
            if (vm.station_type != ComViewModel.StationTypes.Hermetic_Test) return;

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
            Circle_Gauge.UC_Gauge_BR uc = (Circle_Gauge.UC_Gauge_BR)sender;
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

        private void TextBox_WL_Scan_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;

            var txtbox = sender as TextBox;

            if (txtbox.Tag.ToString() == "Start")
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

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!cmd.isLambdaInit)
                cmd.isLambdaInit = cmd.Init_Lambda_Scan();

            cmd.Lambda_Scan_Setting(vm.float_WL_Scan_Start, vm.float_WL_Scan_End);

            cmd.Init_TLS_Filter();

            await Task.Delay(125);

            await cmd.Set_WL(vm.float_WL_Scan_Start, false);

            await Task.Delay(vm.Int_Lambda_Scan_Delay);

            cmd.MTF_Scan((decimal)vm.float_WL_Scan_Start, (decimal)vm.float_WL_Scan_End);
            cmd.TLS_Agilent_Sweep();

            await Task.Delay(1250);

            cmd.Close_TLS_Filter();
        }

        private void TextBox_SendWL_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var obj = sender as TextBox;
                int index = int.Parse(obj.Tag.ToString());

                if (double.TryParse(obj.Text, out double wl))
                {
                    if (!vm.IsGoOn)
                        cmd.Set_WL(wl, true, true);
                    else
                        vm.Save_cmd(new ComMember() { YN = true, No = vm.Cmd_Count.ToString(), Command = "SETWL", Value_1 = wl.ToString() });

                    vm.Ini_Write("Scan", $"Fix_WL_{index}", wl.ToString());
                }
            }
        }

        private void Button_Fix_WL_Update_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var obj = sender as Button;
            int index = int.Parse(obj.Tag.ToString());

            if (float.TryParse(txt_WL.Text, out float value))
            {
                vm.List_Fix_WL[index - 1] = value.ToString();
            }
        }

        private void ToggleBtn_BR_INOUT_Checked(object sender, RoutedEventArgs e)
        {
            Tbtn_BR_INOUT.txtbox_content = "BR Out";
        }

        private void Tbtn_BR_INOUT_Unchecked(object sender, RoutedEventArgs e)
        {
            Tbtn_BR_INOUT.txtbox_content = "BR In";
        }

        private async void btn_Get_BR_Ref_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBoxResult msgBoxResult = MessageBox.Show("Cover old BR Ref.txt ?", "Get Ref", MessageBoxButton.YesNo);

                if (msgBoxResult == MessageBoxResult.No) return;

                #region Initial setting
                vm.isStop = false;

                var watch = System.Diagnostics.Stopwatch.StartNew();
                decimal scan_timespan = watch.ElapsedMilliseconds / (decimal)1000;

                anly.JudgeAllBoolGauge();

                string RefName = String.Empty;
                string RefPath = String.Empty;
                string RefPath_BaseFolder = String.Empty;

                if (!vm.CheckDirectoryExist(@"D:"))
                {
                    MessageBox.Show($"D槽不存在");
                    vm.Save_Log(new LogMember()
                    {
                        Status = "Get Ref",
                        Message = "D槽不存在"
                    });
                    return;
                }

                vm.txt_ref_path = vm.txt_BR_Ref_Path;

                //File name setting
                for (int ch = 0; ch < vm.ch_count; ch++)
                {
                    RefName = $"Ref{ch + 1}.txt";
                    RefPath = Path.Combine(vm.txt_ref_path, RefName);

                    if (!File.Exists(RefPath))
                    {
                        string s = Directory.GetParent(RefPath).ToString();
                        if (!anly.CheckDirectoryExist(s))
                            Directory.CreateDirectory(s);  //Creat ref folder

                        if (anly.CheckDirectoryExist(s))
                            File.AppendAllText(RefPath, "");  //Creat txt file
                        else
                            return;  //If Ref folder still not exist, return.
                    }
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
                            RefPath = Path.Combine(vm.txt_ref_path, RefName);

                            if (File.Exists(RefPath))
                            {
                                File.Delete(RefPath);
                                File.AppendAllText(RefPath, "");
                            }
                        }

                        for (double wl = vm.float_WL_Scan_Start; wl <= vm.float_WL_Scan_End; wl = Math.Round(wl + vm.float_WL_Scan_Gap, 2))
                        {
                            list_wl.Add(Math.Round(wl, 2));
                        }

                        vm.Ref_memberDatas.Clear();

                        for (int i = 0; i < vm.Ref_Dictionaries.Count; i++)
                        {
                            vm.Ref_Dictionaries[i].Clear();
                        }
                    }
                    else return;

                    bool tempBool = vm.Is_FastScan_Mode;
                    vm.Is_FastScan_Mode = false;

                    vm.isLaserActive = true;
                    vm.dB_or_dBm = false;

                    await Task.Delay(500);

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
                                            RefPath = Path.Combine(RefPath, RefName);

                                            await vm.Port_Switch_ReOpen();
                                            vm.port_Switch.Write(string.Format("SW0 {0}", vm.switch_index));
                                        }
                                        else
                                        {
                                            RefName = $"Ref{ch + 1}.txt";
                                            RefPath = Path.Combine(vm.txt_ref_path, RefName);
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
                                        RefPath = Path.Combine(vm.txt_ref_path, RefName);

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

                    LineSeries ls = vm.Plot_Series[0];

                    foreach (DataPoint dp in list_WL_IL)
                    {
                        ls.Points.Add(new DataPoint(dp.X, dp.Y));
                    }

                    ls.Title = "Ref";

                    vm.Update_ALL_PlotView();
                    await cmd.Save_Chart();

                    RefName = $"Ref1.txt";
                    RefPath = Path.Combine(RefPath, RefName);

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

                vm.txt_ref_path = vm.txt_BR_Ref_Path;

                vm.Read_Ref(vm.txt_ref_path);

                scan_timespan = watch.ElapsedMilliseconds / (decimal)1000;
                vm.msgModel.msg_3 = $"{Math.Round(scan_timespan, 1)} s";

                #endregion  
            }
            catch { }
        }

        //string preSelectItm = "";
        private void ComBox_BR_Para_List_DropDownOpened(object sender, EventArgs e)
        {
            //preSelectItm = vm.OSA_Scan_Para;
            if (File.Exists(vm.BR_Scan_Para_Path))
            {
                vm.list_BR_Scan_Para = new ObservableCollection<string>(vm.Ini_Read_All_Sections(vm.BR_Scan_Para_Path));
            }
        }

        private void ComBox_BR_Para_List_DropDownClosed(object sender, EventArgs e)
        {
            var obj = sender as ComboBox;

            if (obj.SelectedIndex != -1)
            {
                string section = vm.list_BR_Scan_Para[obj.SelectedIndex];
                Dictionary<string, string> keys = vm.Ini_Read_All_Keys(vm.BR_Scan_Para_Path, section);

                KeyValuePair<string, string> key = new KeyValuePair<string, string>();
                List<string> keyNames = vm.Ini_Read_All_KeyNames(vm.BR_Scan_Para_Path, section);

                //Set WL(DAC) to UI list
                key = keys.Where(k => k.Key == "WL").FirstOrDefault();
                string[] list_s = key.Value.Split(',');
                vm.list_BR_DAC_WL = new ObservableCollection<string>(list_s);

                cmd.Reset_Chart(vm.list_BR_DAC_WL.ToList());

                vm.PlotViewModel.Annotations.Clear();
                vm.ChartNowModel.list_BR_Model.Clear();

                vm.WL_Special_List.Clear();

                for (int i = 0; i < vm.Plot_Series.Count; i++)
                {
                    vm.Plot_Series[i].Points.Clear();

                    if (vm.PlotViewModel.Annotations.Count > i)
                    {
                        LineAnnotation pat = vm.PlotViewModel.Annotations[i] as LineAnnotation;
                        pat.StrokeThickness = 0;
                        pat.Text = "";
                    }
                    else
                    {
                        vm.PlotViewModel.Annotations.Add(new LineAnnotation()
                        {
                            Type = LineAnnotationType.Vertical,
                            Color = OxyColors.Black,
                            ClipByYAxis = false,
                            X = 0,
                            StrokeThickness = 0
                        });
                    }
                }

                foreach (string s in list_s)
                {
                    vm.ChartNowModel.list_BR_Model.Add(new BR_Model() { Set_WL = s });
                }

                vm.Update_ALL_PlotView();

                vm.Control_board_type = 2;

                //For UFA, UFV board
                dict_WL_to_Dac.Clear();

                foreach (string s in keyNames)
                {
                    if (s.Contains("Dac_"))
                    {
                        foreach (KeyValuePair<string, string> k in keys)
                        {
                            if (k.Key.Contains("Dac_"))
                            {
                                string dacWL = k.Key.Replace("Dac_", "");
                                if (!dict_WL_to_Dac.ContainsKey(dacWL))
                                {
                                    dict_WL_to_Dac.Add(dacWL, k.Value);
                                }
                            }
                        }

                        vm.Control_board_type = 0;
                    }
                }

                //Set WL(TLS) to UI, Start WL, End WL, Gap
                if (keyNames.Contains("Ref_Range"))
                {
                    key = keys.Where(k => k.Key == "Ref_Range").FirstOrDefault();
                    string[] list_wl_setting = key.Value.Split(',');  //Start WL, End WL, Gap
                    if (list_wl_setting.Length == 3)
                    {
                        if (double.TryParse(list_wl_setting[0], out double wl_start))
                            vm.float_WL_Scan_Start = wl_start;

                        if (double.TryParse(list_wl_setting[1], out double wl_end))
                            vm.float_WL_Scan_End = wl_end;

                        if (double.TryParse(list_wl_setting[2], out double wl_gap))
                            vm.float_WL_Scan_Gap = wl_gap;
                    }
                }

                //Special wl list
                if (keyNames.Contains("WL_Range"))
                {
                    key = keys.Where(k => k.Key == "WL_Range").FirstOrDefault();
                    string[] list_wl_setting = key.Value.Split(',');  //Start WL, End WL, Gap

                    vm.WL_Special_List = list_wl_setting.ToList();
                }


                //Set save path
                if (keyNames.Contains("File_Path"))
                {
                    key = keys.Where(k => k.Key == "File_Path").FirstOrDefault();

                    vm.txt_BR_Save_Path = key.Value;
                }
                else
                    vm.Show_Bear_Window($"{Path.GetFileName(vm.BR_Scan_Para_Path)}\r無File_Path設定值");
            }
        }

        bool _is_OSA_WL_Working = false;
        private async void UC_BR_WL_Button_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            UC_OSA_WL_Button obj = sender as UC_OSA_WL_Button;

            if (_is_OSA_WL_Working) return;

            _is_OSA_WL_Working = true;

            string WL = obj.txtbox_content;

            if (string.IsNullOrEmpty(WL)) return;

            try
            {
                if (string.IsNullOrWhiteSpace(vm.Selected_Comport)) return;

                #region Initial setting

                vm.IsGoOn = false;
                vm.isStop = false;

                var watch = System.Diagnostics.Stopwatch.StartNew();
                var elapsedMs = watch.ElapsedMilliseconds;
                decimal scan_timespan = elapsedMs / (decimal)1000;

                cmd.Reset_Chart(vm.list_BR_DAC_WL.ToList());

                vm.sb_bear_shake.Begin();

                for (int i = 0; i < vm.Plot_Series.Count; i++)
                {
                    vm.Plot_Series[i].Points.Clear();

                    if (vm.PlotViewModel.Annotations.Count > i)
                    {
                        LineAnnotation pat = vm.PlotViewModel.Annotations[i] as LineAnnotation;
                        pat.StrokeThickness = 0;
                        pat.Text = "";
                    }
                    else
                    {
                        vm.PlotViewModel.Annotations.Add(new LineAnnotation()
                        {
                            Type = LineAnnotationType.Vertical,
                            Color = OxyColors.Black,
                            ClipByYAxis = false,
                            X = 0,
                            StrokeThickness = 0
                        });
                    }
                }

                for (int i = 0; i < vm.ChartNowModel.list_dataPoints.Count; i++)
                {
                    vm.ChartNowModel.list_dataPoints[i].Clear();
                }

                for (int i = 0; i < vm.ChartNowModel.list_BR_Model.Count; i++)
                {
                    if (vm.ChartNowModel.list_BR_Model[i].Set_WL == WL)
                    {
                        vm.ChartNowModel.list_BR_Model[i].BR_WL = "";
                        vm.ChartNowModel.list_BR_Model[i].BR = "";
                    }
                }

                vm.Update_ALL_PlotView();
                #endregion

                #region Set Dac
                await vm.Port_ReOpen(vm.Selected_Comport);

                //dict_WL_to_Dac
                if (dict_WL_to_Dac.Count != 0)
                {
                    vm.Str_Command = $"D1 {dict_WL_to_Dac[WL]}";

                    if (dict_WL_to_Dac[WL].Split(',').Length == 3)
                    {
                        vm.list_GaugeModels.First().GaugeD0_1 = dict_WL_to_Dac[WL].Split(',')[0];
                        vm.list_GaugeModels.First().GaugeD0_2 = dict_WL_to_Dac[WL].Split(',')[1];
                        vm.list_GaugeModels.First().GaugeD0_3 = dict_WL_to_Dac[WL].Split(',')[2];
                    }
                }
                else
                {
                    vm.Str_Command = $"WL {WL}";
                    vm.Save_Log(new LogMember()
                    {
                        Status = "BR Set Dac",
                        Message = $"Set Board WL : {WL}",
                        Result = "No UFA Dac Para"
                    });
                }

                await cmd.Write_Cmd(vm.Str_Command, true);

                #endregion

                vm.Str_cmd_read = $"WL : {WL}";

                List<DataPoint> list_WL_IL = new List<DataPoint>();
                LineSeries ls = vm.Plot_Series.Where(L => L.Title == WL.ToString()).FirstOrDefault();

                if (vm.is_BR_OSA)
                {
                    //Call OSA scan and show Data function (one lineseries)
                    list_WL_IL = await Task.Run(() => cmd.OSA_Scan());

                    #region Show Datapoints on UI

                    foreach (DataPoint dp in list_WL_IL)
                    {
                        double ref_value = 0;

                        if (vm.dB_or_dBm && vm.is_BR_OSA)
                        {
                            RefModel rm = vm.Ref_memberDatas.Where(r => r.Wavelength == dp.X).FirstOrDefault();

                            if (rm != null)
                                ref_value = rm.Ch_1;
                        }

                        if (ls != null)
                            ls.Points.Add(new DataPoint(dp.X, dp.Y - vm.BR_Diff - ref_value));
                    }
                    #endregion
                }
                //TLS mode
                else
                {
                    vm.Str_Status = "WL Scan";

                    #region Build scan wl list
                    vm.wl_list.Clear();

                    if (vm.WL_Special_List.Count > 0)
                        vm.wl_list = vm.WL_Special_List.Select(w => Convert.ToDouble(w)).ToList();
                    else
                    {
                        if (vm.float_WL_Scan_End >= vm.float_WL_Scan_Start)
                        {
                            for (double wl = vm.float_WL_Scan_Start; wl <= vm.float_WL_Scan_End; wl = wl + vm.float_WL_Scan_Gap)
                            {
                                vm.wl_list.Add(Math.Round(wl, 2));
                            }
                        }
                        else
                        {
                            for (double wl = vm.float_WL_Scan_Start; wl >= vm.float_WL_Scan_End; wl = wl - vm.float_WL_Scan_Gap)
                            {
                                vm.wl_list.Add(Math.Round(wl, 2));
                            }
                        }
                    }

                    vm.Save_Log(new LogMember()
                    {
                        Status = "BR WL Scan",
                        Message = $"WL : {WL}",
                        Result = $"Scan points : {vm.wl_list.Count}"
                    });

                    #endregion

                    foreach (double wl in vm.wl_list)
                    {
                        if (vm.isStop) break;

                        await cmd.Set_WL(wl, false);

                        if (wl == vm.wl_list.First())
                            await Task.Delay(800);

                        if (!(!vm.IsDistributedSystem && !vm.PD_or_PM && vm.Is_FastScan_Mode))
                            await Task.Delay(vm.Int_Set_WL_Delay);

                        vm.Double_Laser_Wavelength = wl;
                        vm.Str_cmd_read = $"WL : {wl}";

                        for (int ch = 0; ch < vm.ch_count; ch++)
                        {
                            if (vm.isStop) break;

                            if (!vm.BoolAllGauge)
                                if (!vm.list_GaugeModels[ch].boolGauge) continue;

                            await cmd.Get_Power(ch, true);

                            list_WL_IL.Add(new DataPoint(vm.Double_Laser_Wavelength, vm.Double_Powers[ch]));
                        }


                        //更新圖表
                        #region Set Chart data points  

                        double ref_value = 0;

                        DataPoint dp = list_WL_IL.Last();

                        if (ls != null)
                            ls.Points.Add(new DataPoint(dp.X, dp.Y - vm.BR_Diff - ref_value));

                        vm.Update_ALL_PlotView();

                        #endregion
                    }
                }

                //Calculate BR and it's wl peak position
                for (int i = 0; i < vm.ChartNowModel.list_BR_Model.Count; i++)
                {
                    if (vm.ChartNowModel.list_BR_Model[i].Set_WL == WL)
                    {
                        double IL_Max = ls.Points.Where(p => p.Y == ls.Points.Max(s => s.Y)).FirstOrDefault().Y;
                        double IL_Min = ls.Points.Where(p => p.Y == ls.Points.Min(s => s.Y)).FirstOrDefault().Y;

                        vm.ChartNowModel.list_BR_Model[i].BR_WL = ls.Points.Where(p => p.Y == ls.Points.Max(s => s.Y)).FirstOrDefault().X.ToString();
                        vm.ChartNowModel.list_BR_Model[i].BR = IL_Max.ToString("f1");

                        vm.PlotViewModel.Axes[1].Maximum = IL_Max < 0 ? 0 : IL_Max;
                        vm.PlotViewModel.Axes[1].Minimum = IL_Min - 10;

                        LineAnnotation pat = vm.PlotViewModel.Annotations[i] as LineAnnotation;
                        pat.StrokeThickness = 2;

                        pat.X = ls.Points.Where(p => p.Y == ls.Points.Max(s => s.Y)).FirstOrDefault().X;

                        pat.Text = $"{IL_Max.ToString("f3")}\r{pat.X.ToString("f2")}";
                        pat.TextOrientation = AnnotationTextOrientation.Horizontal;

                        if (i < 10)
                            pat.TextLinePosition = 1 - (0.1 * i);  //Vertical postion of annotation title text
                        else
                            pat.TextLinePosition = 1 - (0.1 * (i - 10));  //Vertical postion of annotation title text
                    }
                }

                vm.ChartNowModel.list_Annotation = new List<Annotation>(vm.PlotViewModel.Annotations);

                vm.Update_ALL_PlotView();

                vm.Str_cmd_read = vm.Str_cmd_read + $", {list_WL_IL.Count} points";

                scan_timespan = watch.ElapsedMilliseconds / (decimal)1000;
                vm.msgModel.msg_3 = $"{Math.Round(scan_timespan, 1)} s";

                vm.ChartNowModel.TimeSpan = (double)Math.Round(scan_timespan, 1);

                vm.sb_bear_shake.Pause();
                vm.sb_bear_reset.Begin();

                await cmd.Save_Chart();

                _is_OSA_WL_Working = false;

                if (!vm.isStop)
                    vm.Show_Bear_Window($"WL Scan 完成  ({Math.Round(scan_timespan, 1)} s)", false, "String", false);

                vm.IsGoOn = false;

                #region 儲存資料 - 初始設定判斷
                if (string.IsNullOrEmpty(vm.UserID))
                {
                    vm.Show_Bear_Window("工號空白", false, "String", false);
                    return;
                }
                else
                {
                    if (vm.UserID.Length != 5 || (vm.UserID.First() != 'P' && vm.UserID.First() != 'E'))
                    {
                        vm.Show_Bear_Window("工號格式應為PXXX", false, "String", false);
                        return;
                    }
                }

                if (string.IsNullOrEmpty(vm.list_GaugeModels.First().GaugeSN))
                {
                    vm.Show_Bear_Window("序號空白", false, "String", false);
                    return;
                }

                if (!vm.CheckDirectoryExist(vm.txt_BR_Save_Path))
                {
                    vm.Show_Bear_Window("儲存路徑不存在", false, "String", false);
                    return;
                }
                #endregion

                cmd.Save_BR_Data();
            }
            catch
            {
                vm.sb_bear_shake.Pause();
                vm.sb_bear_reset.Begin();
                _is_OSA_WL_Working = false;
            }
        }

        private async void UC_Gauge_BR_Btn_SN_Click(object sender, RoutedEventArgs e)
        {
            var obj = sender as Button;

            await cmd.Cmd_Write_RecieveData("SN?", true, vm.ch);
            vm.list_GaugeModels.First().GaugeSN = vm.Str_cmd_read;
        }

        private void ALL_CheckBox_Click(object sender, RoutedEventArgs e)
        {
            bool judge = false;  //if one channel is not checked
            foreach (Chart_UI_Model chUIModel in vm.list_Chart_UI_Models)
            {
                judge = chUIModel.Button_IsChecked;

                if (!judge)
                    break;
            }

            cbox_all.IsChecked = judge ? false : true;

            for (int i = 0; i < vm.list_Chart_UI_Models.Count; i++)
            {
                vm.list_Chart_UI_Models[i].Button_IsChecked = !judge;

                LineAnnotation lat = vm.PlotViewModel.Annotations[i] as LineAnnotation;
                lat.StrokeThickness = 2;
                lat.TextColor = OxyColors.Black;
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < vm.list_Chart_UI_Models.Count; i++)
            {
                if (vm.Plot_Series.Count > i)
                {
                    vm.Plot_Series[i].IsVisible = vm.list_Chart_UI_Models[i].Button_IsChecked;
                    vm.Plot_Series[i].Color = vm.list_OxyColor[i];

                    if (i > vm.PlotViewModel.Annotations.Count) return;

                    LineAnnotation lat = vm.PlotViewModel.Annotations[i] as LineAnnotation;
                    if (vm.list_Chart_UI_Models[i].Button_IsChecked)
                    {
                        lat.StrokeThickness = 2;
                        lat.TextColor = OxyColors.Black;
                    }
                    else
                    {
                        lat.StrokeThickness = 0;
                        lat.TextColor = OxyColors.Transparent;
                    }

                    lat.TextLinePosition = 1 - (0.1 * i);  //Vertical postion of annotation title text
                }
            }

            vm.Update_ALL_PlotView();
        }


    }
}
