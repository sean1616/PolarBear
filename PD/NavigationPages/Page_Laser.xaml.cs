using System;
using System.Windows.Media;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Collections.Generic;
using TH300;
using PD.Models;
using PD.ViewModel;
using PD.Functions;
using System.Threading.Tasks;
using DiCon.Instrument.HP;
using OxyPlot;

namespace PD.NavigationPages
{
    /// <summary>
    /// Page_Laser.xaml 的互動邏輯
    /// </summary>
    public partial class Page_Laser : UserControl
    {
        ComViewModel vm;
        ControlCmd cmd;
        //HPTLS tls;
        HPPDL pdl;
        private bool auto_connect;

        public Page_Laser(ComViewModel vm)
        {
            InitializeComponent();

            this.vm = vm;
            this.DataContext = this.vm;

            cmd = new ControlCmd(vm);

            try
            {
                int _tls_Addr = 24;
                int.TryParse(vm.Ini_Read("Connection", "tls_Addr"), out _tls_Addr);
                if (_tls_Addr == 0) vm.tls_Addr = 24;
                else vm.tls_Addr = _tls_Addr;

                int _pm_Addr = 24;
                if (int.TryParse(vm.Ini_Read("Connection", "pm_Addr"), out _pm_Addr)) vm.pm_Addr = _pm_Addr;

                int _pm_Slot = 1;
                if (int.TryParse(vm.Ini_Read("Connection", "PM_slot"), out _pm_Slot)) vm.PM_slot = _pm_Slot;

                int.TryParse(vm.Ini_Read("Connection", "multiMeter_Addr"), out _tls_Addr);
                vm.multiMeter_Addr = _tls_Addr;

                bool.TryParse(vm.Ini_Read("Connection", "Auto_Connect_TLS"), out auto_connect);
                vm.Auto_Connect_TLS = auto_connect;

                int PM_AveTime = 20;
                if (int.TryParse(vm.Ini_Read("Connection", "PM_AveTime"), out PM_AveTime))
                    vm.PM_AveTime = PM_AveTime;

                if (auto_connect)
                {
                    #region Tunable Laser setting
                    vm.tls = new HPTLS();
                    vm.tls.BoardNumber = vm.tls_BoardNumber;
                    vm.tls.Addr = vm.tls_Addr;

                    try
                    {
                        if (vm.tls.Open() == false)
                        {
                            vm.Str_cmd_read = "GPIB Setting Error. Check Address.";
                            return;
                        }
                        else
                        {
                            double d = vm.tls.ReadWL();
                            if (string.IsNullOrWhiteSpace(d.ToString()) || d < 0)
                            {
                                vm.Str_cmd_read = "Laser Connection Error.";
                                vm.LogMembers.Add(new Models.LogMember() { Status = "Connection", Message = "Laser Connection Error.", Date = DateTime.Now.ToShortDateString(), Time = DateTime.Now.ToLongTimeString() });
                                return;
                            }
                        }
                        vm.tls.init();

                        try
                        {
                            vm.Double_Laser_Wavelength = vm.tls.ReadWL();
                            slider_WL.Value = vm.Double_Laser_Wavelength;

                            if (vm.selected_band == "Auto" || string.IsNullOrEmpty(vm.selected_band))
                            {
                                float[] wl_minmax = new float[2];
                                wl_minmax[0] = (float)vm.tls.ReadWL_Min();
                                wl_minmax[1] = (float)vm.tls.ReadWL_Max();
                                vm.float_TLS_WL_Range = new float[] { wl_minmax[0], wl_minmax[1] };
                            }
                        }
                        catch { }

                        vm.isConnected = true;

                        //btn_Laser_Status.Background = Brushes.Green;
                    }
                    catch
                    {
                        vm.Str_cmd_read = "TLS GPIB Setting Error.";

                        //btn_Laser_Status.Background = Brushes.Red;
                    }
                    #endregion

                    #region PowerMeter Setting
                    //Power Meter setting
                    vm.pm = new HPPM();
                    vm.pm.Addr = vm.tls_Addr;
                    vm.pm.Slot = vm.PM_slot;
                    vm.pm.BoardNumber = vm.tls_BoardNumber;
                    if (vm.pm.Open() == false)
                    {
                        vm.Str_cmd_read = "PM GPIB Setting Error.  Check  Address.";
                        return;
                    }
                    vm.pm.init();
                    vm.pm.setUnit(1);
                    vm.pm.AutoRange(true);
                    vm.pm.aveTime(vm.PM_AveTime);

                    try
                    {
                        vm.Double_PM_Wavelength = vm.pm.ReadWL();
                        slider_PM_WL.Value = vm.Double_PM_Wavelength;
                    }
                    catch { }
                    #endregion

                    #region PDL controller setting
                    pdl = new HPPDL();
                    pdl.BoardNumber = Convert.ToInt32("0");
                    pdl.Addr = 11;
                    pdl.Open();
                    pdl.init();
                    pdl.scanRate(8);
                    #endregion
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.StackTrace.ToString());
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void btn_TLS_on_Click(object sender, RoutedEventArgs e)
        {
            vm.isLaserActive = !vm.isLaserActive;
            cmd.Set_TLS_Active(vm.isLaserActive);
        }

        private void btn_TLS_connect_Click(object sender, RoutedEventArgs e)
        {
            cmd.Connect_TLS();
          

        }

        private async void slider_WL_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            double wl = slider_WL.Value;
            if (!vm.IsGoOn)
                try
                {
                    txt_WL.Text = (wl).ToString();
                    cmd.Set_WL(wl, false, true);
                    await Task.Delay(vm.Int_Set_WL_Delay / 2);
                }
                catch { }
            else
                vm.Save_cmd(new ComMember() { YN = true, No = vm.Cmd_Count.ToString(), Command = "SETWL", Value_1 = wl.ToString() });

            vm.Double_Laser_Wavelength = wl;
        }

        private async void slider_PM_WL_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (vm.isConnected)
            {
                if (vm.PD_or_PM == true && vm.IsGoOn == true)
                    await vm.PM_Stop();

                vm.pm.SetWL(slider_PM_WL.Value);

                await vm.AccessDelayAsync(vm.Int_Set_WL_Delay);

                try
                {
                    vm.Double_PM_Wavelength = vm.pm.ReadWL();
                }
                catch { }

                if (vm.PD_or_PM == true && vm.IsGoOn == true) vm.PM_GO();
            }
        }

        private void btn_Laser_Status_Click(object sender, RoutedEventArgs e)
        {

        }

        int unit = 0;
        private void btn_Set_Unit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                switch (vm.Laser_type)
                {
                    case "Agilent":
                        vm.tls.setUnit(unit);
                        if (unit == 0)
                            unit = 1; //dBm
                        else
                            unit = 0; //uW
                        break;

                    case "Golight":
                        vm.tls_GL.setUnit(unit);
                        if (unit == 0)
                            unit = 1; //dBm
                        else
                            unit = 0; //uW
                        break;
                }







            }
            catch { };

        }

        private void slider_Power_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (vm.isConnected)
            {
                try
                {
                    double pwr = vm.Double_Laser_Power;
                    if (pwr > 10 || pwr < -15)
                    {
                        vm.Str_cmd_read = "Power out of range";
                        return;
                    }

                    if (!vm.IsGoOn)
                    {
                        cmd.Set_TLS_Power(pwr, true);
                        //vm.tls.SetPower(pwr);
                    }
                    else
                        vm.Save_cmd(new ComMember() { YN = true, No = vm.Cmd_Count.ToString(), Command = "SETPOWER", Type = "Agilent", Value_1 = pwr.ToString() });
                }
                catch
                {

                }
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

        private void txt_WL_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                double wl = Convert.ToDouble(txt_WL.Text);

                if (!vm.IsGoOn) cmd.Set_WL(wl, false, true);
                else
                    vm.Save_cmd(new ComMember() { YN = true, No = vm.Cmd_Count.ToString(), Command = "SETWL", Value_1 = txt_WL.Text });
            }
            else if (e.Key == Key.Up)
            {
                double wl = Convert.ToDouble(txt_WL.Text) + 0.01;

                if (!vm.IsGoOn)
                    try
                    {
                        txt_WL.Text = (wl).ToString();
                        cmd.Set_WL(wl, false, true);
                    }
                    catch { }
                else
                    vm.Save_cmd(new ComMember() { YN = true, No = vm.Cmd_Count.ToString(), Command = "SETWL", Value_1 = wl.ToString() });

                vm.Double_Laser_Wavelength = wl;
            }
            else if (e.Key == Key.Down)
            {
                double wl = Convert.ToDouble(txt_WL.Text) - 0.01;

                if (!vm.IsGoOn)
                    try
                    {
                        txt_WL.Text = (wl).ToString();
                        cmd.Set_WL(wl, false, true);
                    }
                    catch { }
                else
                    vm.Save_cmd(new ComMember() { YN = true, No = vm.Cmd_Count.ToString(), Command = "SETWL", Value_1 = wl.ToString() });

                vm.Double_Laser_Wavelength = wl;
            }
        }

        private void txt_PM_WL_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                vm.pm.SetWL(Convert.ToDouble(txt_PM_WL.Text));
                try
                {
                    vm.Double_PM_Wavelength = vm.pm.ReadWL();
                }
                catch { }
            }
        }

        private void txt_power_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.Enter)
            {
                if (vm.isConnected)
                {
                    try
                    {
                        double pwr = double.Parse(txt_power.Text);
                        if (pwr > 10 || pwr < -15)
                        {
                            vm.Str_cmd_read = "Power out of range";
                            return;
                        }

                        if (!vm.IsGoOn)
                        {
                            cmd.Set_TLS_Power(pwr, true);
                            //vm.tls.SetPower(pwr);
                        }
                        else
                        {
                            vm.Save_cmd(new ComMember() { YN = true, No = vm.Cmd_Count.ToString(), Command = "SETPOWER", Type = "Agilent", Value_1 = pwr.ToString() });

                            //vm.Double_Laser_Power = pwr;
                        }
                    }
                    catch
                    {

                    }
                }
            }
        }

        private void btn_PDL_connect_Click(object sender, RoutedEventArgs e)
        {
            #region PDL controller setting
            pdl = new HPPDL();
            pdl.BoardNumber = Convert.ToInt32("0");
            pdl.Addr = 11;
            pdl.Open();
            pdl.init();
            pdl.scanRate(8);
            #endregion
        }

        private void btn_PDL_On_Click(object sender, RoutedEventArgs e)
        {
            #region PDL controller setting
            pdl = new HPPDL();
            pdl.BoardNumber = Convert.ToInt32("0");
            pdl.Addr = 11;
            pdl.Open();
            pdl.init();
            pdl.scanRate(8);
            #endregion

            if (pdl != null)
                pdl.startPolarizationScan();
        }

        private void btn_PDL_Stop_Click(object sender, RoutedEventArgs e)
        {
            if (pdl != null)
            {
                pdl.stopPolarizationScan();
                pdl.Close();
            }
        }

        private void btn_PDL_Reset_Click(object sender, RoutedEventArgs e)
        {
            //Method 1
            #region PDL controller setting
            pdl = new HPPDL();
            pdl.BoardNumber = Convert.ToInt32("0");
            pdl.Addr = 11;
            pdl.Open();
            pdl.init();
            pdl.scanRate(8);
            #endregion

            //Method 2
            //pdl.SendCommand("*RST");
        }

        bool pm_MaxMinFunc = false;
        private void btn_Set_PM_MaxMinFunc_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!pm_MaxMinFunc)
                    vm.pm.startMaxMinFunc();
                else
                {
                    vm.pm.stopMaxMinFunc(300);
                }

                pm_MaxMinFunc = !pm_MaxMinFunc;
            }
            catch { };
        }

        int pm_refStatus = 0;
        private void btn_PM_Unit_Click(object sender, RoutedEventArgs e)
        {
            if (pm_MaxMinFunc)
            {
                vm.pm.stopMaxMinFunc(300);
                pm_MaxMinFunc = false;
            }

            pm_refStatus = vm.pm.ReadReferenceStatus();

            if (pm_refStatus == 0)
            {
                vm.pm.SetReferenceStatus(1);
            }
            else
            {
                vm.pm.SetReferenceStatus(0);
            }

            //MessageBox.Show(vm.pm.ReadReferenceStatus().ToString());
            //vm.pm.setUnit(pm_unit++);
            //if (pm_unit > 3)
            //    pm_unit = 0;
        }

        private void btn_TLS_Lambda_Scan_Click(object sender, RoutedEventArgs e)
        {
            cmd.Set_TLS_Lambda_Scan();
        }

        private void txt_WL_range_setting_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                float lower = 0;
                float upper = 0;
                float.TryParse(txt_WL_lower_range.Text, out lower);
                float.TryParse(txt_WL_upper_range.Text, out upper);
                vm.float_TLS_WL_Range = new float[2] { lower, upper };
            }
        }
    }
}
