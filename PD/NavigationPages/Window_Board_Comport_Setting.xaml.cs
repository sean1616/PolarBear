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

        private async void btn_saveBoard_Click(object sender, RoutedEventArgs e)
        {
            int i = 0;
            foreach(List<string> board_info in vm.list_Board_Setting)
            {
                if (string.IsNullOrEmpty(board_info[0]))
                    continue;

                vm.board_read.Add(new List<string>());

                string board_id = board_info[0];
                string path = string.Concat(vm.txt_board_table_path, board_id, "-boardtable.txt");

                if (!File.Exists(path))
                {
                    vm.Str_cmd_read = "UFV Board table is not exist";
                    continue;
                }

                StreamReader str = new StreamReader(path);

                while (true)  //Read board v3 data
                {
                    string readline = str.ReadLine();

                    if (string.IsNullOrEmpty(readline)) break;

                    vm.board_read[i].Add(readline);
                }
                str.Close(); //(關閉str)

                i++;
            }



            for (int c = 0; c < 12; c++)
            {
                if (vm.board_read[c].Count == 0)
                {
                    vm.Str_cmd_read = "UFV Board table is empty";
                    continue;
                }

                int count = 0;
                foreach (string strline in vm.board_read[c])
                {
                    string voltage;
                    string[] board_read = strline.Split(',');
                    if (board_read.Length == 1)
                    {
                        voltage = board_read[0];
                        continue;
                    }                        
                    else if(board_read.Length<1)
                        break;
                                        
                    int dac = int.Parse(board_read[1]);

                    //list_voltage.Add(Convert.ToDouble(voltage));
                    //list_dac.Add(dac);

                    //if (dac >= vm.List_V3_dac[c][maxpower_index[c]] && count > 0)
                    //{
                    //    int delta_x = (vm.List_V3_dac[c][maxpower_index[c]] - list_dac[count - 1]);
                    //    int delta_X = (list_dac[count] - list_dac[count - 1]);
                    //    double delta_Y = (list_voltage[count] - list_voltage[count - 1]);
                    //    final_voltage = (Convert.ToDouble(delta_x) / Convert.ToDouble(delta_X)) * delta_Y + list_voltage[count - 1];
                    //    final_voltage = Math.Round(final_voltage, 1);
                    //    break;
                    //}

                    count++;
                }
            }
        }
    }
}
