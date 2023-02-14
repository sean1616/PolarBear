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
using PD.ViewModel;

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

            if (!vm.CheckDirectoryExist(@"D:\PD"))
                vm.ini_path = Path.Combine(CurrentDirectory, "Instrument.ini");
            else
            {
                try
                {
                    if (File.Exists(vm.ini_path))
                    {
                        vm.selected_band = vm.Ini_Read("Connection", "Band");

                        if (!string.IsNullOrEmpty(vm.Ini_Read("Connection", "Laser_type")))
                            vm.Laser_type = vm.Ini_Read("Connection", "Laser_type");

                        if (!string.IsNullOrEmpty(vm.Ini_Read("Connection", "Switch_Comport"))) vm.Comport_Switch = vm.Ini_Read("Connection", "Switch_Comport");

                        if (!string.IsNullOrEmpty(vm.Ini_Read("Connection", "Station")))
                            vm.station_type = vm.Ini_Read("Connection", "Station");

                        if (!string.IsNullOrEmpty(vm.Ini_Read("Connection", "Control_Board_Type")))
                        {
                            ComBox_Control_Board_Type.SelectedItem = vm.Ini_Read("Connection", "Control_Board_Type");

                            try
                            {
                                string control_board_type = ComBox_Control_Board_Type.SelectedItem.ToString();
                                switch (control_board_type)
                                {
                                    case "UFV":
                                        vm.Control_board_type = 0;
                                        break;

                                    case "V":
                                        vm.Control_board_type = 1;
                                        break;
                                }
                            }
                            catch { }
                        }

                        vm.Golight_ChannelModel.Board_Port = vm.Ini_Read("Connection", "COM_Golight");
                    }
                }
                catch { }
            }
            
            if (vm.Laser_type.Equals("Golight") && vm.Auto_Connect_TLS)
            {               
                if (!string.IsNullOrEmpty(vm.Golight_ChannelModel.Board_Port))
                {
                    vm.tls_GL = new DiCon.Instrument.HP.GLTLS.GLTLS();
                    if (vm.tls_GL.Open(vm.Golight_ChannelModel.Board_Port))
                    {
                        vm.isConnected = true;
                    }
                    else
                        vm.Save_Log(new Models.LogMember()
                        {
                            isShowMSG = true,
                            Message = "Connect Golight TLS Fail"
                        });
                }
                else
                    vm.Save_Log(new Models.LogMember()
                    {
                        isShowMSG = true,
                        Message = "Golight comport is null or empty"
                    });
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
            i = _GIF_controller.FrameCount;
            //_GIF_controller.CurrentFrame;
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

                    vm.Laser_type = laserType;
                    
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
