using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using PD.ViewModel;
using PD.AnalysisModel;

namespace PD.NavigationPages
{
    /// <summary>
    /// Interaction logic for Window_Switch_Box.xaml
    /// </summary>
    public partial class Window_Switch_Box : Window
    {
        Analysis anly;
        Point point_to_screen;
        ComViewModel vm;
        double btn_width;

        public Window_Switch_Box(ComViewModel vm, Point point_to_screen, double btn_width) 
        {
            InitializeComponent();

            this.vm = vm;
            this.DataContext = vm;
            anly = new Analysis(vm);

            this.point_to_screen = point_to_screen;
            this.btn_width = btn_width;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Top = point_to_screen.Y - 80;
            this.Left = point_to_screen.X + btn_width;

            int i = 0;
            foreach(bool b in vm.List_switchBox_ischeck)
            {
                if (b)
                {
                    if (i == 0)
                        rbtn_1.IsChecked = true;
                    else if (i == 1)
                        rbtn_2.IsChecked = true;
                    else if (i == 2)
                        rbtn_3.IsChecked = true;
                    else if (i == 3)
                        rbtn_4.IsChecked = true;
                    else if (i == 4)
                        rbtn_5.IsChecked = true;
                    else if (i == 5)
                        rbtn_6.IsChecked = true;
                    else if (i == 6)
                        rbtn_7.IsChecked = true;
                    else if (i == 7)
                        rbtn_8.IsChecked = true;
                    else if (i == 8)
                        rbtn_9.IsChecked = true;
                    else if (i == 9)
                        rbtn_10.IsChecked = true;
                    else if (i == 10)
                        rbtn_11.IsChecked = true;
                    else if (i == 11)
                        rbtn_12.IsChecked = true;
                }
                i++;
            }
        }

        private async void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton obj = (RadioButton)sender;

            if (obj.IsChecked == false)
                return;

            int ch = 0;
            if (obj.Name.Split('_').Count() == 2)
                ch = Convert.ToInt16(obj.Name.Split('_')[1]);  //取得點擊Rbtn代表的channel

            vm.List_switchBox_ischeck = Analysis.ListDefault<bool>(12);
            for (int i = 0; i < 12; i++)
            {
                if (i == ch-1)
                    vm.List_switchBox_ischeck[i] = true;
                else
                    vm.List_switchBox_ischeck[i] = false;
            }

            try { await vm.Port_Switch_ReOpen(); } catch {  }
            

            if (ch > 0 && ch < 13)   //Switch 1~12
            {
                if (string.IsNullOrWhiteSpace("I1 " + ch.ToString())) //Check comment box is empty or not
                    return;

                //Gauge顯示項控制
                if (ch < 9)
                {
                    vm.Str_Channel = new List<string>() { "1", "2", "3", "4", "5", "6", "7", "8", };
                    vm.Channel_visible = new List<Visibility>() { };
                    vm.Bool_Gauge_Show = new bool[] { false, false, false, false, false, false, false, false };
                    vm.Bool_Gauge = new bool[8] { true, true, true, true, true, true, true, true };
                    vm.Bool_Gauge.CopyTo(vm.bo_temp_gauge, 0);
                }
                else
                {
                    vm.Str_Channel = new List<string>() { "9", "10", "11", "12" };
                    vm.Channel_visible = new List<Visibility>()
                    {
                        Visibility.Visible, Visibility.Visible, Visibility.Visible, Visibility.Visible,
                        Visibility.Hidden, Visibility.Hidden, Visibility.Hidden, Visibility.Hidden
                    };
                    vm.Bool_Gauge_Show = new bool[] { false, false, false, false, false, false, false, false };
                    vm.Bool_Gauge = new bool[8] { true, true, true, true, true, true, true, true };
                    vm.Bool_Gauge.CopyTo(vm.bo_temp_gauge, 0);
                }

                try
                {
                    vm.Str_Command = "I1 " + ch.ToString();
                    vm.port_Switch.Write(vm.Str_Command + "\r");
                    await vm.AccessDelayAsync(vm.Int_Write_Delay);
                }
                catch { }
            }
            else if (ch == 0)   //Switch 0
            {
                vm.Str_Command = "I1?";
                vm.port_Switch.Write(vm.Str_Command + "\r");
                await vm.AccessDelayAsync(vm.Int_Read_Delay);
            }
            else  //Switch > 12 , Gauge顯示項控制
            {
                vm.Str_Channel = new List<string>() { "1", "2", "3", "4", "5", "6", "7", "8", };
                vm.Channel_visible = new List<Visibility>() { };
            }

            vm.ch = ch;   //Save Switch channel
        }        
    }
}
