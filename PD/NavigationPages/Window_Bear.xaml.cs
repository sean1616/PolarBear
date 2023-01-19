using System;
using System.IO;
using System.IO.Ports;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

using PD.ViewModel;
using PD.Models;
using PD.Functions;
using PD.AnalysisModel;

namespace PD.NavigationPages
{
    /// <summary>
    /// Interaction logic for Window_Bear.xaml
    /// </summary>
    public partial class Window_Bear : Window
    {
        ComViewModel vm;
        ControlCmd cmd;
        Analysis anly;
        //Point p;        
        private bool mRestoreForDragMove;
        bool is_txt_reshow;
        string type;

        public Window_Bear(ComViewModel vm, bool _is_txt_reshow, string type)
        {
            InitializeComponent();

            this.vm = vm;
            this.DataContext = vm;
            this.type = type;
            
            is_txt_reshow = _is_txt_reshow;

            cmd = new ControlCmd(vm);
            anly = new Analysis(vm);

            //InitializeBackgroundWorker();  //Define progress bar
        }

        #region Declaration

        // Create a Object of BackgroundWorker Class
        private BackgroundWorker WorkerThread = new BackgroundWorker();

        private void InitializeBackgroundWorker()
        {
            try
            {
                WorkerThread.WorkerReportsProgress = true;
                WorkerThread.DoWork += new DoWorkEventHandler(WorkerThread_DoWork);
                WorkerThread.ProgressChanged += new ProgressChangedEventHandler(WorkerThread_ProgressChanged);
                WorkerThread.RunWorkerCompleted += new RunWorkerCompletedEventHandler(WorkerThread_RunWorkerCompleted);

                WorkerThread.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        void WorkerThread_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                PBar.Value = PBar.Maximum;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        void WorkerThread_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            try
            {
                PBar.Value = e.ProgressPercentage;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        void WorkerThread_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                Thread.Sleep(200);

                WorkerThread.ReportProgress(vm.Progressbar_Value);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        #endregion

        private void btn_close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        private async void Image_Loaded(object sender, RoutedEventArgs e)
        {
            if (type == "String")
            {
                txt_bear_say_something.Text = vm.Str_bear_say;

                vm.window_bear_width = 500;
                vm.window_bear_heigh = 250;
                window_grid_row_1.Height = new GridLength(1.5, GridUnitType.Star);
                               
                txt_bear_say_something.Visibility = Visibility.Visible;

                await Task.Delay(100);
                
            }
            else if (type == "String_Step")
            {
                txt_bear_say_something.Text = "";
                window_grid_row_1.Height = new GridLength(1.5, GridUnitType.Star);
                vm.window_bear_width = 500;
                vm.window_bear_heigh = 250;
                                
                await Task.Delay(400);
                string[] str_show_array = vm.Str_bear_say.Split(' ');

                txt_bear_say_something.Visibility = Visibility.Visible;

                do
                {                   
                    foreach (string s in str_show_array)
                    {
                        if (vm.isStop == true)
                            break;

                        txt_bear_say_something.Text += s;
                        await Task.Delay(300);
                    }
                }
                while (is_txt_reshow);
            }
            else
            {
                window_grid_row_1.Height = new GridLength(1, GridUnitType.Star);
                vm.window_bear_width = 800;
                vm.window_bear_heigh = 380;

                txt_bear_say_something.Visibility = Visibility.Collapsed;

                vm.List_bear_say = new List<List<string>>(vm.List_bear_say);

            }            
        }

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

        private void btn_min_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        List<List<string>> temp_list_bear_say = new List<List<string>>();
        int page_now = 1;
        private void Btn_page2_Click(object sender, RoutedEventArgs e)
        {
            if(vm.station_type=="Hermetic_Test")   //to page 2
            {
                if (vm.List_bear_say.Count < 9) return;
                if (page_now == 2) return;
                page_now = 2;

                temp_list_bear_say = vm.List_bear_say;
                vm.List_bear_say = Analysis.ListDefine<string>(new List<List<string>>(), vm.ch_count, new List<string>());
                for (int ch = 9; ch <= 12; ch++)
                {
                    vm.List_bear_say[ch - 9] = temp_list_bear_say[ch - 1];
                }
                vm.List_bear_say = new List<List<string>>(vm.List_bear_say);

                vm.txt_No=new string[] { "Ch 9", "Ch 10", "Ch 11", "Ch 12", "", "", "", "" };
            }            
        }

        private void Btn_page1_Click(object sender, RoutedEventArgs e)
        {
            if(vm.station_type=="Hermetic_Test")   //to page 1
            {
                if (page_now == 1) return;
                page_now = 1;

                vm.List_bear_say = temp_list_bear_say;

                vm.txt_No = new string[] { "Ch 1", "Ch 2", "Ch 3", "Ch 4", "Ch 5", "Ch 6", "Ch 7", "Ch 8" };
            }           
        }                

        private void image_MouseEnter(object sender, MouseEventArgs e)
        {
            image.Visibility = Visibility.Hidden;
            //grid_sector_btns.Visibility = Visibility.Visible;
        }

        private void grid_sector_btns_MouseLeave(object sender, MouseEventArgs e)
        {
            image.Visibility = Visibility.Visible;
            //grid_sector_btns.Visibility = Visibility.Hidden;
        }

        private void btn_pre_Click(object sender, RoutedEventArgs e)
        {
            if(vm.bear_say_now > 1)
            {
                vm.bear_say_now--;
                //vm.List_bear_say = vm.Collection_bear_say[vm.bear_say_now-1];
            }                
        }

        private void btn_aft_Click(object sender, RoutedEventArgs e)
        {
            if (vm.bear_say_now < vm.bear_say_all) vm.bear_say_now++;
        }

        private void grid_test_result_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (vm.station_type != "Hermetic_Test") return;
            
            if (e.Delta < 0)
            {
                if (vm.station_type == "Hermetic_Test")  //to page 2
                {
                    if (vm.List_bear_say.Count < 9) return;
                    if (page_now == 2) return;
                    page_now = 2;

                    temp_list_bear_say = vm.List_bear_say;
                    vm.List_bear_say = Analysis.ListDefine<string>(new List<List<string>>(), vm.ch_count, new List<string>());
                    for (int ch = 9; ch <= 12; ch++)
                    {
                        vm.List_bear_say[ch - 9] = temp_list_bear_say[ch - 1];
                    }
                    vm.List_bear_say = new List<List<string>>(vm.List_bear_say);
                }
            }
            else
            {
                if (vm.station_type.Equals("Hermetic_Test"))  //to page 1
                {
                    if (page_now == 1) return;
                    page_now = 1;

                    vm.List_bear_say = temp_list_bear_say;
                }
            }
        }

        int int_saved_combox_index;
        SerialPort savePort;
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (vm.station_type != "Hermetic_Test") return;

            Button obj = (Button)sender;

            int switch_index;
            string[] tag_index = obj.Tag.ToString().Split('_');
            if (tag_index.Length == 2)
            {
                if (page_now == 1)
                    switch_index = int.Parse(tag_index[0]);
                else
                    switch_index = int.Parse(tag_index[1]);
            }
            else return;

            if (obj.Content == null) return;
            if (string.IsNullOrEmpty(obj.Content.ToString())) return;
            if (vm.PD_or_PM == true && vm.IsGoOn == true) await vm.PM_Stop();

            double txt_value = Convert.ToDouble(obj.Content);
            if (txt_value > 1000 && txt_value < 2000)
            {
                #region set WL
                double wl = txt_value;
                vm.tls.SetWL(wl);
                vm.Double_Laser_Wavelength = wl;
                await Task.Delay(vm.Int_Set_WL_Delay);
                vm.pm.SetWL(Convert.ToDouble(obj.Content));
                #endregion
            }
            else
            {
                double dac = txt_value;

                //Reset COM port
                if (string.IsNullOrEmpty(vm.list_Board_Setting[switch_index-1][1])) return;

                savePort = vm.port_PD;

                #region Port Open
                try
                {
                    if (!vm.IsGoOn)
                    {
                        vm.port_PD.Open();
                    }
                    else
                    {
                        if (vm.PD_or_PM == false)
                            vm.timer_PD_GO.Stop();
                        else
                            vm.timer_PM_GO.Stop();
                        await Task.Delay(vm.Int_Read_Delay);
                        vm.port_PD.Close();
                        await Task.Delay(50);
                        vm.port_PD.Open();
                        vm.port_PD.DiscardInBuffer();       // RX
                        vm.port_PD.DiscardOutBuffer();      // TX
                    }
                }
                catch { }
                #endregion

                //Set voltage
                vm.Str_Command = "D1 0,0," + (dac).ToString();  //cmd = D1 0,0,1000
                vm.port_PD.Write(vm.Str_Command + "\r");
                await Task.Delay(vm.Int_Write_Delay);

                vm.port_PD.Close();
                await Task.Delay(50);
                vm.port_PD.DiscardInBuffer();       // RX
                vm.port_PD.DiscardOutBuffer();      // TX
            }

            if (vm.PD_or_PM == true && vm.IsGoOn == true) vm.PM_GO();

            #region set switch
            try
            {
                if (vm.port_Switch != null) vm.port_Switch.Close();

                await Task.Delay(50);

                vm.port_Switch = new SerialPort(vm.Comport_Switch.ToString(), vm.BoudRate, Parity.None, 8, StopBits.One);
                vm.port_Switch.Open();

                vm.port_Switch.DiscardInBuffer();       // RX
                vm.port_Switch.DiscardOutBuffer();      // TX
            }
            catch
            {
                vm.Str_bear_say = "Switch Comport Error";
                vm.Winbear = new Window_Bear(vm, false, "String");
                vm.Winbear.Show();
            }

            if (switch_index > 12) return;

            if (switch_index > 0 && switch_index < 13)   //Switch 1~12
            {
                if (string.IsNullOrWhiteSpace("SW0 " + switch_index.ToString())) //Check comment box is empty or not
                    return;

                if (switch_index < 9 && int_saved_combox_index >= 9)  //換頁 page1
                {
                    vm.Bool_Page2 = vm.Bool_Gauge_Show;
                    vm.Str_Channel = new List<string>() { "1", "2", "3", "4", "5", "6", "7", "8", };
                    vm.Channel_visible = new List<Visibility>() { };
                    vm.Bool_Gauge_Show = vm.Bool_Page1;
                }
                else if (switch_index > 8 && int_saved_combox_index <= 8)  //換頁 page2
                {
                    vm.Bool_Page1 = vm.Bool_Gauge_Show;
                    vm.Str_Channel = new List<string>() { "9", "10", "11", "12" };
                    vm.Channel_visible = new List<Visibility>()
                    {
                        Visibility.Visible, Visibility.Visible, Visibility.Visible, Visibility.Visible,
                        Visibility.Hidden, Visibility.Hidden, Visibility.Hidden, Visibility.Hidden
                    };
                    vm.Bool_Gauge_Show = vm.Bool_Page2;
                }

                try
                {
                    vm.Str_Command = "SW0 " + switch_index.ToString();
                    vm.port_Switch.Write(vm.Str_Command + "\r");
                    await Task.Delay(vm.Int_Write_Delay);
                }
                catch { }
            }
            else if (switch_index == 0)   //Switch ?
            {
                vm.Str_Command = "I1?";
                vm.port_Switch.Write(vm.Str_Command + "\r");
                await Task.Delay(vm.Int_Read_Delay);
            }
            else
            {
                vm.Str_Channel = new List<string>() { "1", "2", "3", "4", "5", "6", "7", "8", };
                vm.Channel_visible = new List<Visibility>() { };
            }

            int_saved_combox_index = switch_index;
            vm.ch = switch_index;   //Save Switch channel

            vm.switch_index = switch_index;
            #endregion
        }

        private void btn_bear_setting_Click(object sender, RoutedEventArgs e)
        {
            if (btn_bear_setting.IsChecked == true)
            {
                //grid_test_result.Visibility = Visibility.Hidden;
                grid_setting_btn.Visibility = Visibility.Visible;
            }
            else
            {
                //grid_test_result.Visibility = Visibility.Visible;
                grid_setting_btn.Visibility = Visibility.Collapsed;
            }
        }

        private void btn_Save_Click(object sender, RoutedEventArgs e)
        {
            Button obj = (Button)sender;
            int beforeORafter = int.Parse(obj.Tag.ToString());

            anly.JudgeAllBoolGauge();
                       
            #region Analyze Product Information
            vm.SNMembers = new ObservableCollection<SN_Member>();
            
            #endregion
            
            bool _isChSelected = false;
            List<string> list_SN = new List<string>();
            for (int i = 0; i < vm.ch_count; i++)
            {
                string SN = vm.list_GaugeModels[i].GaugeSN;
                
                vm.SNMembers.Add(anly.Product_Info_Anly(SN));  //Analyze product information and return a member of SN_Information_Group       

                if (string.IsNullOrEmpty(SN)) continue;

                #region Analyze Product Type
                string product_type = "";
                switch (vm.SNMembers[i].ProductType)
                {
                    case "UFA":
                        product_type = "UFA";
                        break;
                    case "UFA-T":
                        product_type = "UFA";
                        break;
                    case "UFA(H)":
                        product_type = "UFA";
                        break;
                    case "CTF":
                        product_type = "CTF";
                        break;
                    case "UTF":
                        product_type = "UTF";
                        break;
                    case "UTF400":
                        product_type = "UTF";
                        break;
                    case "UTF500":
                        product_type = "UTF";
                        break;
                    case "MTF":
                        product_type = "MTF";
                        break;
                    default:
                        product_type = vm.product_type;
                        break;
                }
                #endregion

                if (vm.list_GaugeModels[i].boolGauge || vm.BoolAllGauge)
                {
                    int errorCode = cmd.Save_K_WL_Data("K WL", vm.UserID, vm.list_GaugeModels[i], product_type, beforeORafter);
                    if (errorCode != 0)
                    {
                        switch (errorCode)
                        {
                            case 1:
                                vm.Show_Bear_Window("資料空白", false, "String", false);
                                return;
                            case 2:
                                vm.Show_Bear_Window("工號空白", false, "String", false);
                                return;
                            case 3:
                                vm.Show_Bear_Window("產品序號空白", false, "String", false);
                                return;
                            case 4:
                                vm.Show_Bear_Window("Save Data Error", false, "String", false);
                                return;
                        }
                    }
                    _isChSelected = true;
                }
            }

            if (_isChSelected)
                vm.Show_Bear_Window("Saved", false, "String", false);
            else
                vm.Show_Bear_Window("Choose a channel to save data", false, "String", false);
        }

       
    }
}
