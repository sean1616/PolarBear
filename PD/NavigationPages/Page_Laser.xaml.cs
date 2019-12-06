using System;
using System.Windows.Media;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Collections.Generic;
using DiCon.Instrument.HP;
using TH300;
using PD.ViewModel;
using System.Threading.Tasks;
using OxyPlot;

namespace PD.NavigationPages
{    
    /// <summary>
    /// Page_Laser.xaml 的互動邏輯
    /// </summary>
    public partial class Page_Laser : UserControl
    {
        ComViewModel vm;
        //HPTLS tls;
        HPPDL pdl;
        bool _isActive = false, auto_connect;

        public Page_Laser(ComViewModel vm)
        {
            InitializeComponent();

            this.vm = vm;
            this.DataContext = this.vm;

            bool.TryParse(vm.Ini_Read("Connection", "Auto_Connect_TLS"), out auto_connect);
            vm.Auto_Connect_TLS = auto_connect;

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
                    vm.tls.init();

                    vm.Double_Laser_Wavelength = vm.tls.ReadWL();
                    slider_WL.Value = vm.Double_Laser_Wavelength;

                    vm.isConnected = true;

                    btn_Laser_Status.Background = Brushes.Green;
                }
                catch
                {
                    vm.Str_cmd_read = "TLS GPIB Setting Error.";
                    btn_Laser_Status.Background = Brushes.Red;
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
                vm.pm.aveTime(20);
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

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
           
        }
               
        private async void btn_TLS_on_Click(object sender, RoutedEventArgs e)
        {
            if (vm.PD_or_PM == true && vm.IsGoOn == true)
                await vm.PM_Stop();

            vm.isLaserActive = vm.isLaserActive == true ? false : true;
            vm.tls.SetActive(vm.isLaserActive);

            await vm.AccessDelayAsync(vm.Int_Set_WL_Delay + 100);

            if (vm.PD_or_PM == true && vm.IsGoOn == true) vm.PM_GO();
        }

        private void btn_TLS_connect_Click(object sender, RoutedEventArgs e)
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
                vm.tls.init();

                vm.Double_Laser_Wavelength = vm.tls.ReadWL();
                slider_WL.Value = vm.Double_Laser_Wavelength;

                vm.isConnected = true;

                btn_Laser_Status.Background = Brushes.Green;
            }
            catch
            {
                vm.Str_cmd_read = "TLS GPIB Setting Error.";
                btn_Laser_Status.Background = Brushes.Red;
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
            vm.pm.aveTime(20);
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

        private async void slider_WL_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (vm.isConnected)
            {
                await vm.AccessDelayAsync(vm.Int_Read_Delay);
                vm.tls.SetWL(slider_WL.Value);
                try
                {
                    vm.Double_Laser_Wavelength = vm.tls.ReadWL();
                }
                catch { }
            }                
        }

        private void btn_Laser_Status_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void btn_WL_Scan_Click(object sender, RoutedEventArgs e)
        {
            List<double> list_ISO_IL = new List<double>();
            List<double> list_ISO_WL = new List<double>();
            List<DataPoint> list_ISO_DataPoint = new List<DataPoint>();

            for (double i = 1527; i <= 1570; i=i+0.1)
            {
                vm.tls.SetWL(i);
                //list_ISO_IL.Add(vm.pm.ReadPower());  //y data
                //list_ISO_WL.Add(i);  //x data
                list_ISO_DataPoint.Add(new DataPoint(i, vm.pm.ReadPower()));
                vm.Chart_DataPoints = new List<DataPoint>(list_ISO_DataPoint);  //The first lineseries 
            }              
        }

        int count = 0;
        private void btn_Set_Unit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                vm.tls.setUnit(count);
                if (count == 0)
                    count = 1; //dBm
                else
                    count = 0; //uW
                //vm.Str_cmd_read = count.ToString();
            }
            catch { };
            
        }

        private async void slider_Power_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (vm.isConnected)
            {
                vm.timer3.Stop();
                await vm.AccessDelayAsync(vm.Int_Read_Delay);
                vm.tls.SetPower(slider_Power.Value);
                vm.timer3.Start();
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
                vm.tls.SetWL(Convert.ToDouble(txt_WL.Text));
                try
                {
                    vm.Double_Laser_Wavelength = vm.tls.ReadWL();
                }
                catch { btn_Laser_Status.Background = Brushes.Red; }
            }                
        }
        
        private void txt_power_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                vm.tls.SetPower(Convert.ToDouble(txt_power.Text));
                vm.Double_Laser_Power = Convert.ToDouble(txt_power.Text);
            }
        }

        private void btn_PDL_connect_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btn_PDL_On_Click(object sender, RoutedEventArgs e)
        {
            pdl.startPolarizationScan();
        }

        private void btn_PDL_Stop_Click(object sender, RoutedEventArgs e)
        {
            pdl.stopPolarizationScan();
        }
    }
}
