using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using PD.ViewModel;

namespace PD.NavigationPages
{
    /// <summary>
    /// Window_Password.xaml 的互動邏輯
    /// </summary>
    public partial class Window_Password : Window
    {
        ComViewModel vm;       
        private bool mRestoreForDragMove;

        public Window_Password(ComViewModel vm)
        {
            InitializeComponent();

            this.vm = vm;
            this.DataContext = vm;
        }

        private void btn_min_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void btn_close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Image_Loaded(object sender, RoutedEventArgs e)
        {
            vm.window_bear_width = 500;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Left = vm.mainWin_point.X + (vm.mainWin_size[0] / 2) - (this.ActualWidth / 2);
            this.Top = vm.mainWin_point.Y + (vm.mainWin_size[1] / 2) - (this.ActualHeight / 2);
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //判斷滑鼠點擊次數
            if (e.ClickCount == 2)
            {
            }
            else
            {
                mRestoreForDragMove = this.WindowState == WindowState.Normal;
            }
        }

        private void TitleBar_MouseUp(object sender, MouseButtonEventArgs e)
        {
            mRestoreForDragMove = false;
        }

        private void TitleBar_MouseMove(object sender, MouseEventArgs e)
        {
            if (mRestoreForDragMove)
            {
                mRestoreForDragMove = false;
                this.DragMove();
            }
        }

        private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
        {           
            if (e.Key == Key.Enter)
            {
                string password = PassworBox.Password;
                if(string.Compare(password, "candy666", true) == 0 || string.Compare(password, "candy555", true) == 0 || string.Compare(password, "QQ123", true) == 0 || string.Compare(password, "1234", true) == 0)
                {
                    Engineer_Mode();
                }
                else
                {
                    User_Mode();
                }
            }
        }

        private void Image_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (txt_username.Text == "Engineer")
                User_Mode();
        }

        private void User_Mode()
        {
            txt_username.Text = "User";
            #region Calibration Items Setting
            vm.list_combox_Calibration_items.Clear();
            for (int i = 1; i < 9; i++)
            {
                string _item = vm.Ini_Read("Calibration", "Item_" + i.ToString());
                if (string.IsNullOrEmpty(_item))
                    continue;

                vm.list_combox_Calibration_items.Add(_item);
            }

            if (vm.list_combox_Calibration_items.Count == 0)
                vm.list_combox_Calibration_items = new List<string>() { "Calibration", "VOA -> 0", "TF -> 0", "K VOA", "K TF", "K TF (Rough)", "K TF (Detail)", "Curve Fitting", "K WL" };
            #endregion
        }

        private void Engineer_Mode()
        {
            vm.list_combox_Calibration_items = new List<string>() { "Calibration", "VOA -> 0", "TF -> 0", "K VOA", "K TF", "K TF (Rough)", "K TF (Detail)", "Curve Fitting", "K WL" };
            txt_username.Text = "Engineer";
        }
    }
}
