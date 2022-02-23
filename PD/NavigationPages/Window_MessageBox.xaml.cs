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

namespace PD.NavigationPages
{
    /// <summary>
    /// Interaction logic for Window_MessageBox.xaml
    /// </summary>
    public partial class Window_MessageBox : Window
    {
        ComViewModel vm;
        //string bearSay = "";

        public Window_MessageBox(ComViewModel vm, string bearSay)
        {
            InitializeComponent();

            this.vm = vm;
            
            txt_bear_say_something.Text = bearSay;
        }

        private bool mRestoreForDragMove;
        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //判斷滑鼠點擊次數
            if (e.ClickCount == 2)
            {
                //if ((this.ResizeMode != ResizeMode.CanResize) && (this.ResizeMode != ResizeMode.CanResizeWithGrip))
                //    return;
                //this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized; //雙擊最大化
            }
            else
            {
                mRestoreForDragMove = this.WindowState == WindowState.Normal;
            }
        }

        private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            mRestoreForDragMove = false;
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            if (mRestoreForDragMove)
            {
                mRestoreForDragMove = false;
                this.DragMove();
            }
        }

        private void btn_close_Click(object sender, RoutedEventArgs e)
        {
            vm.bearSay = -1;
            this.Close();
        }

        private void btn_min_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        //private void btn_bear_setting_Click(object sender, RoutedEventArgs e)
        //{
           
        //}

        private void btn_Yes_Click(object sender, RoutedEventArgs e)
        {
            vm.bearSay = 1;

            this.Close();
        }

        private void btn_No_Click(object sender, RoutedEventArgs e)
        {
            vm.bearSay = 0;

            this.Close();
        }
    }
}
