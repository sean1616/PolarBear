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
using PD.ViewModel;
using PD.Functions;

using DiCon.Instrument.HP;

namespace PD.NavigationPages
{
    /// <summary>
    /// Interaction logic for Window_Bear_Timer.xaml
    /// </summary>
    public partial class Window_Bear_Timer : Window
    {
        ComViewModel vm;
        ControlCmd cmd;

        public Window_Bear_Timer(ComViewModel vm)
        {
            InitializeComponent();

            this.vm = vm;
            this.DataContext = vm;

            cmd = new ControlCmd(vm);
        }

        private void btn_min_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void btn_close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TimeSpan interval = new TimeSpan(vm.int_timer_hrs, vm.int_timer_min, vm.int_timer_sec);
            vm.int_timer_timespan = (int)interval.TotalSeconds;

            if(!vm.isTimerOn && !vm.IsGoOn)  //開始計時
            {
                vm.isTimerOn = true;
                vm.IsGoOn = true;

                bear_msg.Text = "Stop";

                if (vm.IsGoOn)
                {
                    if (vm.isConnected == false && vm.PD_or_PM == true)  //檢查TLS是否連線，若無，則進行連線並取續TLS狀態
                    {
                        #region Tunable Laser setting
                        vm.tls = new HPTLS();
                        vm.tls.BoardNumber = vm.tls_BoardNumber;
                        vm.tls.Addr = vm.tls_Addr;

                        try
                        {
                            if (vm.tls.Open() == false)
                            {
                                vm.Str_cmd_read = "TLS GPIB Setting Error. Check Address.";
                                return;
                            }
                            vm.tls.init();

                            vm.Double_Laser_Wavelength = vm.tls.ReadWL();

                            vm.isConnected = true;
                        }
                        catch
                        {
                            vm.Str_cmd_read = "GPIB Setting Error.";
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
                        #endregion
                    }

                    vm.Chart_x_title = "Time(s)"; //Set Chart x axis title
                    vm.Str_Status = "Get Power";
                    if (vm.PD_or_PM == false)
                    {
                        vm.Cmd_Count = 0;
                        //vm.Cmd_excute_order = 0;

                        //await vm.PD_GO();

                        cmd.CommandListCycle();
                    }
                        
                    else
                        vm.PM_GO();
                }
            }
           else  //結束計時
            {
                vm.isTimerOn = false;
                vm.IsGoOn = false;

                bear_msg.Text = "Go";
                vm.Str_Status = "Stop";

                if (vm.PD_or_PM == false)
                {
                    await vm.PD_Stop();
                    await cmd.Save_Chart();
                }
                else
                    await vm.PM_Stop();
            }
        }

        private bool mRestoreForDragMove;
        private void border_title_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //判斷滑鼠點擊次數
            if (e.ClickCount == 2)
            {
                if ((this.ResizeMode != ResizeMode.CanResize) && (this.ResizeMode != ResizeMode.CanResizeWithGrip))
                    return;
                this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized; //雙擊最大化
            }
            else
            {
                mRestoreForDragMove = this.WindowState == WindowState.Normal;
            }
        }

        private void border_title_MouseMove(object sender, MouseEventArgs e)
        {
            if (mRestoreForDragMove && this.WindowState == WindowState.Normal)
            {
                //mRestoreForDragMove = false;
                mRestoreForDragMove = false;
                this.DragMove();
            }
        }

        private void border_title_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            mRestoreForDragMove = false;
        }
    }
}
