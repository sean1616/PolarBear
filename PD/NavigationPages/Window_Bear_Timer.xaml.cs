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
using PD;

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

        //public event EventHandler CallMainGoFunction;
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TimeSpan interval = new TimeSpan(vm.int_timer_hrs, vm.int_timer_min, vm.int_timer_sec);
            vm.int_timer_timespan = (int)interval.TotalSeconds;

            if(!vm.isTimerOn && !vm.IsGoOn)  //開始計時
            {
                vm.isTimerOn = true;
                vm.IsGoOn = true;

                //bear_msg.Text = "Stop";

                var mainWnd = Application.Current.MainWindow as MainWindow;
                if (mainWnd != null)
                    mainWnd.Main_Go();
            }
           else  //結束計時
            {
                vm.isTimerOn = false;
                vm.IsGoOn = false;

                //bear_msg.Text = "Go";
                //vm.Str_Status = "Stop";

                var mainWnd = Application.Current.MainWindow as MainWindow;
                if (mainWnd != null)
                    mainWnd.Main_Go();
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
