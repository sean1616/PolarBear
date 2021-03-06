using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Diagnostics;
using WpfAnimatedGif;

using PD;
using PD.ViewModel;
using PD.NavigationPages;

namespace PD.NavigationPages
{
    /// <summary>
    /// Interaction logic for Page_Setting.xaml
    /// </summary>
    public partial class Page_Setting : UserControl
    {
        ComViewModel vm;
        Window_Board_Comport_Setting winBoard;
        string ini_path = @"d:\PD\Instrument.ini";

        public Page_Setting(ComViewModel vm)
        {
            InitializeComponent();

            this.vm = vm;
            this.DataContext = this.vm;

            try { if (File.Exists(ini_path)) vm.selected_band = vm.Ini_Read("Connection", "Band"); }
            catch { }

            if (!string.IsNullOrEmpty(vm.Ini_Read("Connection", "Switch_Comport")))
                vm.Comport_Switch = Convert.ToInt16(vm.Ini_Read("Connection", "Switch_Comport"));
                        
            if (!string.IsNullOrEmpty(vm.Ini_Read("Productions", "V3_Scan_Gap")))
                vm.int_V3_scan_gap = int.Parse(vm.Ini_Read("Productions", "V3_Scan_Gap"));
            if (!string.IsNullOrEmpty(vm.Ini_Read("Productions", "V3_scan_start")))
                vm.int_V3_scan_start = int.Parse(vm.Ini_Read("Productions", "V3_scan_start"));

            if (!string.IsNullOrEmpty(vm.Ini_Read("Connection", "Station")))
                vm.station_type = vm.Ini_Read("Connection", "Station");

            if (!string.IsNullOrEmpty(vm.Ini_Read("Connection", "Control_Board_Type")))
            {
                string a = vm.Ini_Read("Connection", "Control_Board_Type");
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
                } catch { }
            }
        }
      
        int i, c=1;

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
                //_GIF_controller = ImageBehavior.GetAnimationController(Img_gif);
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
                obj.Opacity = 0.05;
        }

        string str_saved_board_type;
        private void ComBox_Control_Board_Type_DropDownOpened(object sender, EventArgs e)
        {
            ComboBox obj = (ComboBox)sender;

            if(obj.SelectedItem!=null)
                str_saved_board_type = obj.SelectedItem.ToString();

        }

        private void ComBox_Control_Board_Type_DropDownClosed(object sender, EventArgs e)
        {
            ComboBox obj = (ComboBox)sender;

            if (obj.SelectedItem != null)
            {
                string control_board_type = obj.SelectedItem.ToString();

                switch (control_board_type)
                {
                    case "UFV":
                        vm.Control_board_type = 0;
                        break;

                    case "V":
                        vm.Control_board_type = 1;
                        break;
                }

                vm.Ini_Write("Connection", "Control_Board_Type", control_board_type);  //創建ini file並寫入基本設定
            }

            if (obj.SelectedItem == null && str_saved_board_type != null)
                obj.SelectedItem = str_saved_board_type;
        }
        
        private void Img_gif_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void btn_ini_Click(object sender, RoutedEventArgs e)
        {
            Process process = new Process();
            process.StartInfo.FileName = ini_path;
            process.Start();
        }

        private void btn_board_comport_Click(object sender, RoutedEventArgs e)
        {
            winBoard = new Window_Board_Comport_Setting(vm);
            winBoard.Show();
        }

        private void UC_Setting_ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("還沒寫好啦");
        }

        private void ComBox_K_WL_Type_DropDownClosed(object sender, EventArgs e)
        {
            vm.Ini_Write("Productions", "K_WL_Type", vm.selected_K_WL_Type);
        }

        private void ComBox_Laser_Selection_DropDownClosed(object sender, EventArgs e)
        {

        }

        private void ComBox_Laser_Selection_DropDownOpened(object sender, EventArgs e)
        {

        }

        private void Image_Loaded(object sender, RoutedEventArgs e)
        {
            _GIF_controller = ImageBehavior.GetAnimationController(Img_gif);
            i = _GIF_controller.FrameCount;
            int b = _GIF_controller.CurrentFrame;
            _GIF_controller.GotoFrame(1);
            _GIF_controller.Pause();
        }
    }
}
