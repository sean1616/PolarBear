using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.IO.Ports;
using System.Diagnostics;
using System.Collections.ObjectModel;
using WpfAnimatedGif;

using PD;
using PD.Models;
using PD.ViewModel;

using Wpf_Control_Library;

namespace PD.NavigationPages
{
    /// <summary>
    /// Interaction logic for Page_Setting.xaml
    /// </summary>
    public partial class Page_Setting : UserControl
    {
        readonly ComViewModel vm;
        //readonly string ini_path = @"d:\PD\Instrument.ini";
        readonly static string CurrentDirectory = Directory.GetCurrentDirectory();

        public Page_Setting(ComViewModel vm)
        {
            InitializeComponent();

            this.vm = vm;
            this.DataContext = this.vm;

            SettiingPage_Initialize();
        }

        /// <summary>
        /// 初始化設定頁面的UI項目
        /// </summary>
        private void SettiingPage_Initialize()
        {
            List<DateTime> list_dt = new List<DateTime>();
            list_dt.Add(DateTime.Now);

            //UIElementCollection children = stcP_1.Children;
            vm.list_stcP_children.Add(stcP_1.Children);
            vm.list_stcP_children.Add(stcP_2.Children);
            vm.list_stcP_children.Add(stcP_3.Children);
            vm.list_stcP_children.Add(stcP_4.Children);

            list_dt.Add(DateTime.Now);

            //新增station標籤至Setting Unit
            vm.SettingUnit_Tag_Setting();

            list_dt.Add(DateTime.Now);

            //Update Setting page ui
            if (vm.list_stcP_children != null && vm.list_stcP_children.Count > 0)
                vm.Define_Setting_Unit(vm.station_type.ToString());

            list_dt.Add(DateTime.Now);

            for (int i = 1; i < list_dt.Count; i++)
            {
                TimeSpan tp = list_dt[i] - list_dt[i - 1];
                Console.WriteLine($"{i} : {tp.TotalMilliseconds}");
            }
        }

        int i, c = 1;

        private ImageAnimationController _GIF_controller;
        private void Img_gif_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (c < i)
            {
                _GIF_controller.GotoFrame(c);
                _GIF_controller.Pause();
                c++;
            }
            else
                c = 0;
        }

        private void Img_gif_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (vm.WinPass != null)
                if (vm.WinPass.IsLoaded)
                    vm.WinPass.Close();

            vm.WinPass = new Window_Password(vm);
            vm.WinPass.Show();
        }

        private void Img_gif_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Image obj = (Image)sender;
            obj.Opacity = 1;

            if (_GIF_controller != null)
            {
                _GIF_controller = ImageBehavior.GetAnimationController(Img_gif);
                _GIF_controller.GotoFrame(1);
                _GIF_controller.Play();
            }
            else
            {
                _GIF_controller = ImageBehavior.GetAnimationController(Img_gif);
                _GIF_controller.GotoFrame(1);
                _GIF_controller.Play();
            }
        }

        private void Img_gif_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Image obj = (Image)sender;
            if (_GIF_controller != null)
            {
                obj.Opacity = 0.05;
                _GIF_controller.GotoFrame(1);
                _GIF_controller.Pause();
            }
        }

        string str_saved_board_type;
        private void ComBox_Control_Board_Type_DropDownOpened(object sender, EventArgs e)
        {
            ComboBox obj = (ComboBox)sender;

            if (obj.SelectedItem != null)
                str_saved_board_type = obj.SelectedItem.ToString();

        }

        private void ComBox_Control_Board_Type_DropDownClosed(object sender, EventArgs e)
        {
            ComboBox obj = (ComboBox)sender;

            if (obj.SelectedItem != null)
            {
                string control_board_type = obj.SelectedItem.ToString();

                //switch (control_board_type)
                //{
                //    case "UFV":
                //        vm.Control_board_type = 0;
                //        break;

                //    case "V":
                //        vm.Control_board_type = 1;
                //        break;

                //    case "MTF Board":
                //        vm.Control_board_type = 2;
                //        break;
                //}

                vm.Ini_Write("Connection", "Control_Board_Type", control_board_type);  //創建ini file並寫入基本設定
            }

            if (obj.SelectedItem == null && str_saved_board_type != null)
                obj.SelectedItem = str_saved_board_type;
        }

        private void Img_gif_Loaded(object sender, RoutedEventArgs e)
        {
            _GIF_controller = ImageBehavior.GetAnimationController(Img_gif);
            if (_GIF_controller == null) return;
            i = _GIF_controller.FrameCount;
            _GIF_controller.GotoFrame(1);
            _GIF_controller.Pause();
        }

        private void BTN_INI_CLICK(object sender, RoutedEventArgs e)
        {
            try
            {
                if (File.Exists(vm.ini_path))
                {
                    Process process = new Process();
                    process.StartInfo.FileName = vm.ini_path;
                    process.Start();
                }
            }
            catch { }
        }

        private void ComBox_K_WL_Type_DropDownClosed(object sender, EventArgs e)
        {
            vm.Ini_Write("Productions", "K_WL_Type", vm.selected_K_WL_Type);
        }

        private void ComBox_TLS_WL_Range_DropDownClosed(object sender, EventArgs e)
        {
            vm.Save_Product_Info[1] = vm.selected_band;
            if (vm.selected_band == "C Band")
            {
                vm.float_TLS_WL_Range = new float[2] { 1523, 1573 };
                if (vm.isConnected == false)
                    if (vm.list_wl != null)
                        vm.Double_Laser_Wavelength = 1523;
            }
            else //L band
            {
                vm.float_TLS_WL_Range = new float[2] { 1560, 1620 };
                if (vm.isConnected == false)
                    if (vm.list_wl != null)
                        vm.Double_Laser_Wavelength = 1560;
            }

            //setting.Product_Setting();

            vm.Ini_Write("Connection", "Band", vm.selected_band);  //創建ini file並寫入基本設定
        }

        string[] myPorts;
        private async void ComBox_Arduino_Comport_DropDownOpened(object sender, EventArgs e)
        {
            ComBox_Arduino_Comport.Items.Clear();
            myPorts = SerialPort.GetPortNames();

            await Task.Delay(1);

            foreach (string s in myPorts)
            {
                string comport_and_ID = s;

                ComBox_Arduino_Comport.Items.Add(comport_and_ID);  //寫入所有取得的com
            }
        }

        Window_PowerSupply_Setting window_PowerSupply_Setting;
        private void BTN_SETTING_WINDOW_CLICK(object sender, RoutedEventArgs e)
        {
            //btn_setting_window_Click
            if (window_PowerSupply_Setting != null)
            {
                if (window_PowerSupply_Setting.IsActive)
                {
                    window_PowerSupply_Setting.Topmost = true;
                }
                else
                {
                    window_PowerSupply_Setting = new Window_PowerSupply_Setting(vm);
                    window_PowerSupply_Setting.Show();
                }
            }
            else
            {
                window_PowerSupply_Setting = new Window_PowerSupply_Setting(vm);
                window_PowerSupply_Setting.Show();
            }
        }

        int pre_combobox_index = 0;
        private void ComBox_DropDownOpened(object sender, EventArgs e)
        {
            ComboBox cbb = (ComboBox)sender;
            pre_combobox_index = cbb.SelectedIndex;
            cbb.SelectedIndex = -1;
        }

        private void ComBox_DropDownClosed(object sender, EventArgs e)
        {
            ComboBox cbb = (ComboBox)sender;
            if (cbb.SelectedIndex < 0)
                cbb.SelectedIndex = pre_combobox_index;
        }


        private void ComBox_Laser_Selection_DropDownClosed(object sender, EventArgs e)
        {
            ComboBox obj = (ComboBox)sender;

            if (obj.SelectedItem != null)
            {
                try
                {
                    string laserType = obj.SelectedItem.ToString();

                    ComViewModel.LaserType LT;
                    if (Enum.TryParse(laserType, out LT))
                        vm.Laser_type = LT;

                    vm.Ini_Write("Connection", "Laser_type", laserType);
                }
                catch (TimeoutException ex)
                {
                    MessageBox.Show(ex.StackTrace.ToString());
                }
            }
        }

    }
}
