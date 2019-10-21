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
using System.IO;

using DiCon.UCB.Communication;
using DiCon.UCB;

using PD.AnalysisModel;
using PD.ViewModel;

namespace PD.NavigationPages
{
    /// <summary>
    /// Interaction logic for Window_Board_Comport_Setting.xaml
    /// </summary>
    public partial class Window_Board_Comport_Setting : Window
    {
        ComViewModel vm;
        private bool mRestoreForDragMove;
        bool[] chkPM = new bool[12];

        public Window_Board_Comport_Setting(ComViewModel vm)
        {
            InitializeComponent();

            this.vm = vm;
            this.DataContext = this.vm;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            vm.list_Board_Setting.Clear();

            for (int i = 0; i < 12; i++)
            {
                string Board_ID = "Board_ID_" + (i + 1).ToString();
                string Board_COM = "Board_COM_" + (i + 1).ToString();
                vm.list_Board_Setting.Add(new List<string>() { vm.Ini_Read("Board_ID", Board_ID), vm.Ini_Read("Board_Comport", Board_COM) });
            }

            vm.list_Board_Setting = new List<List<string>>(vm.list_Board_Setting);
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextBox obj = (TextBox)sender;

                string ch = obj.Name.Split('_')[1];
                string ID_COM = obj.Name.Split('_')[2];

                if (ID_COM == "1")
                    write_ID(ch, obj.Text);
                else
                    write_COM(ch, obj.Text);
            }
        }

        private void write_ID(string ch, string value)
        {
            string board_id = "Board_ID_" + ch;
            vm.Ini_Write("Board_ID", board_id, value);
        }

        private void write_COM(string ch, string value)
        {
            string board_com = "Board_COM_" + ch;
            vm.Ini_Write("Board_Comport", board_com, value);
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
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

        private void _LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox obj = (TextBox)sender;

            string ch = obj.Name.Split('_')[1];
            string ID_COM = obj.Name.Split('_')[2];

            if (ID_COM == "1")
                write_ID(ch, obj.Text);
            else
                write_COM(ch, obj.Text);
        }

        private async void btn_fillBoard_Click(object sender, RoutedEventArgs e)
        {
            ICommunication icomm;
            DiCon.UCB.Communication.RS232.RS232 rs232;
            DiCon.UCB.MTF.IMTFCommand tf;

            for (int idz = 0; idz < vm.list_Board_Setting.Count; idz++)
            {
                if (vm.list_Board_Setting[idz].Count == 2)
                {
                    if (!string.IsNullOrEmpty(vm.list_Board_Setting[idz][1]))
                    {
                        rs232 = new DiCon.UCB.Communication.RS232.RS232(vm.list_Board_Setting[idz][1]);
                        rs232.OpenPort();
                        icomm = (ICommunication)rs232;

                        tf = new DiCon.UCB.MTF.RS232.RS232(icomm);

                        vm.list_Board_Setting[idz][0] = tf.ReadSN();
                        await vm.AccessDelayAsync(500);
                        rs232.ClosePort();
                    }
                }
            }
        }
    }
}
