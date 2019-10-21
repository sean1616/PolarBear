using System;
using System.Windows;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using System.Linq;
using System.IO;
using System.IO.Ports;

using GPIB_utility;
using PD.ViewModel;
using PD.NavigationPages;

using OxyPlot;
using OxyPlot.Wpf;



namespace PD.NavigationPages
{
    /// <summary>
    /// Page_DataGrid.xaml 的互動邏輯
    /// </summary>
    public partial class Page_DataGrid : UserControl
    {
        ComViewModel vm;
        Page_Ref_Grid _Page_Ref_Grid;
        Page_Ref_Chart _Page_Ref_Chart;
        Page_Board_Grid _Page_Board_Grid;
        List<DataPoint> list_datapoint;
        List<List<DataPoint>> list_list_datapoint;

        string save_path = "";

        public Page_DataGrid(ComViewModel vm)
        {
            InitializeComponent();

            this.DataContext = vm;
            this.vm = vm;

            save_path = txt_path.Text;

            _Page_Ref_Grid = new Page_Ref_Grid(vm, txt_path.Text);

            Calculate_Ref_Chart_DataPoint();

            _Page_Ref_Chart = new Page_Ref_Chart(vm, list_datapoint);

            pageTransitionControl.ShowPage(_Page_Ref_Grid);

            if (vm.isConnected == false)
            {
                if (vm.Ini_Read("Connection", "Band") == "C Band")
                    vm.Double_Laser_Wavelength = 1523;  //Set wavelength to setup ref value
                else
                    vm.Double_Laser_Wavelength = 1571;
            }           
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        public class Member
        {
            public float Wavelength { get; set; }
            public float Ch_1 { get; set; }
            public float Ch_2 { get; set; }
            public float Ch_3 { get; set; }
            public float Ch_4 { get; set; }
            public float Ch_5 { get; set; }
            public float Ch_6 { get; set; }
            public float Ch_7 { get; set; }
            public float Ch_8 { get; set; }
        }

        private void Btn_show_Click(object sender, RoutedEventArgs e)
        {
            if (txt_path.Text == save_path)    //判斷Ref檔路徑是否改變
                pageTransitionControl.ShowPage(_Page_Ref_Grid);
            else
            {
                _Page_Ref_Grid = new Page_Ref_Grid(vm, txt_path.Text);
                pageTransitionControl.ShowPage(_Page_Ref_Grid);
                Calculate_Ref_Chart_DataPoint();
                _Page_Ref_Chart = new Page_Ref_Chart(vm, list_datapoint);
                save_path = txt_path.Text;
            }            
        }

        private void Calculate_Ref_Chart_DataPoint()
        {
            list_datapoint = new List<DataPoint>();
            list_list_datapoint = new List<List<DataPoint>>();

            double criteria = double.Parse(txt_criteria.Text);

            for (int i = 0; i < 8; i++)
                vm.ref_Color[i] = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF33D3C4"));  //設定小燈顏色

            int count_ch = vm.list_WL_Ref.Count;
            for (int ch = 0; ch < count_ch; ch++)
            {
                if (vm.list_WL_Ref[ch].Count == 0)
                    continue;

                int count = 0;
                list_datapoint.Clear();
                foreach (double f in vm.list_wl)
                {
                    list_datapoint.Add(new DataPoint(f, vm.list_WL_Ref[ch][count]));
                    if (count > 0)
                        if ((vm.list_WL_Ref[ch][count] - vm.list_WL_Ref[ch][count - 1]) >= criteria)
                        {
                            vm.ref_Color[0] = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF870D0D"));  //設定小燈顏色
                            SolidColorBrush[] temp_ref_color = vm.ref_Color;
                            vm.ref_Color = temp_ref_color;
                        }
                            
                    count++;
                }
                list_list_datapoint.Add(new List<DataPoint>(list_datapoint));

                LinearAxis linearAxis = new LinearAxis();
                linearAxis.MajorGridlineStyle = LineStyle.Solid;
                linearAxis.MinorGridlineStyle = LineStyle.Dot;
                linearAxis.Position = OxyPlot.Axes.AxisPosition.Left;
                linearAxis.Minimum = -8.5;
                linearAxis.Maximum = 0.1;
                linearAxis.Minimum = vm.list_WL_Ref[ch].Min() - 0.1;
                linearAxis.Maximum = vm.list_WL_Ref[ch].Max() + 0.1;

                LinearAxis linearAxis2 = new LinearAxis();
                linearAxis2.MajorGridlineStyle = LineStyle.Solid;
                linearAxis2.MinorGridlineStyle = LineStyle.Dot;
                linearAxis2.Position = OxyPlot.Axes.AxisPosition.Bottom;
                linearAxis2.Minimum = vm.list_wl.Min();
                linearAxis2.Maximum = vm.list_wl.Max();
            }

            vm.Chart_All_DataPoints_ref = list_list_datapoint;            
        }
        
        private void Btn_select_all_Click(object sender, RoutedEventArgs e)
        {
            _Page_Ref_Grid.dataGrid.Focus();
            _Page_Ref_Grid.dataGrid.SelectAll();
        }

        private void Btn_show_ref_chart_Click(object sender, RoutedEventArgs e)
        {
            if (txt_path.Text == save_path)
                pageTransitionControl.ShowPage(_Page_Ref_Chart);
            else
            {
                Calculate_Ref_Chart_DataPoint();
                _Page_Ref_Chart = new Page_Ref_Chart(vm, list_datapoint);
                pageTransitionControl.ShowPage(_Page_Ref_Chart);
                save_path = txt_path.Text;
            }
        }

        private void btn_show_ref_now_Click(object sender, RoutedEventArgs e)
        {
            string s = "";
            foreach(double d in vm.float_WL_Ref)
                s = s + ", " + d;

            vm.Str_cmd_read = s.Substring(1);
        }

        private void btn_reload_Click(object sender, RoutedEventArgs e)
        {
            _Page_Ref_Grid = new Page_Ref_Grid(vm, txt_path.Text);            
            Calculate_Ref_Chart_DataPoint();
            _Page_Ref_Chart = new Page_Ref_Chart(vm, list_datapoint);
            pageTransitionControl.ShowPage(_Page_Ref_Chart);
            save_path = txt_path.Text;
        }

        private void btn_board_v_Click(object sender, RoutedEventArgs e)
        {
            _Page_Board_Grid = new Page_Board_Grid(vm);

            pageTransitionControl.ShowPage(_Page_Board_Grid);
        }

        int initial_dac = 30000;
        //List<string> _write_line = new List<string>();
        private async void btn_board_v123_Click(object sender, RoutedEventArgs e)
        {
            Button obj = (Button)sender;
            vm.BoardTable_V123 = int.Parse(obj.Tag.ToString());

            #region MultiMeter Setting
            GPIB_CMD myGPIB = new GPIB_CMD(0, 22);  //MultiMeter GPIB Setting
            string mystring = "MEAS:VOLT:DC? 50,0.0001";  //MultiMeter GPIB Read Voltage Command
            Byte[] mybytes = System.Text.Encoding.Default.GetBytes(mystring);
            #endregion

            vm.port_PD = new SerialPort(vm.Selected_Comport, 115200, Parity.None, 8, StopBits.One);
            List<List<string>> _List_Bear_Say = new List<List<string>>();

            for (int dac = initial_dac; dac <= 60000; dac+=10000)
            {
                if (vm.BoardTable_V123 == 1)
                    vm.WriteDac("1", "D", dac.ToString() + ",0,0");  //V1 Write Dac
                else if (vm.BoardTable_V123 == 2)
                    vm.WriteDac("1", "D", "0," + dac.ToString() + ",0");  //V2 Write Dac
                else
                    vm.WriteDac("1", "D", "0,0," + dac.ToString());  //V3 Write Dac

                await vm.AccessDelayAsync(vm.Int_Write_Delay);
                vm.port_PD.Close();

                myGPIB.Write(mybytes);
                string volt = myGPIB.Read();

                if (vm.List_BoardTable.Count > 0)
                {
                    double volt_before = 0;
                    if (vm.BoardTable_V123 == 3)
                    {
                        List<double> list_voltage = new List<double>();
                        List<int> list_dac = new List<int>();

                        double final_voltage = 0;

                        for (int c = 0; c < 1; c++)
                        {
                            if (vm.isStop) break;

                            if (!vm.Bool_Gauge[c]) continue;

                            string board_id = vm.BoardTable_SelectedBoard;
                            string path = @"\\192.168.2.3\tff\Data\BoardCalibration\UFA\" + board_id + "-boardtable.txt";

                            if (!File.Exists(path))
                            {
                                vm.Show_Bear_Window("UFV Board table is not exist", false, "String");
                                return;
                            }

                            StreamReader str = new StreamReader(path);
                            str.ReadLine();

                            int count = 0;
                            while (!vm.isStop)  //Read board v3 data
                            {
                                string[] board_read = str.ReadLine().Split(',');  //(一行一行讀取)
                                if (board_read.Length < 1)
                                    break;

                                string v3_table_voltage = board_read[0];
                                int v3_table_dac = int.Parse(board_read[1]);

                                list_voltage.Add(Convert.ToDouble(v3_table_voltage));
                                list_dac.Add(v3_table_dac);

                                if (v3_table_dac >= dac && count > 0)
                                {
                                    int delta_x = (dac - list_dac[count - 1]);
                                    int delta_X = (list_dac[count] - list_dac[count - 1]);
                                    double delta_Y = (list_voltage[count] - list_voltage[count - 1]);
                                    final_voltage = (Convert.ToDouble(delta_x) / Convert.ToDouble(delta_X)) * delta_Y + list_voltage[count - 1];
                                    final_voltage = Math.Round(final_voltage, 2);
                                    break;
                                }

                                count++;
                            }
                            str.Close(); //(關閉str)

                            volt_before = final_voltage;
                        }
                    }
                    else
                        volt_before = Convert.ToDouble(vm.List_BoardTable[vm.BoardTable_SelectedIndex][vm.BoardTable_V123]) * dac;   //V1: BoardTable_V123 = 1

                    double volt_after = Math.Round(Convert.ToDouble(volt), 4);
                    double delta_volt = Math.Round(volt_after - volt_before, 4);
                    vm.Str_Status = "Write DAC";
                    vm.Str_cmd_read = "DAC: " + dac.ToString() + " , Delta Volt: " + delta_volt.ToString();

                    #region Bear Say
                    vm.txt_No = new string[] { "30000", "40000", "50000", "60000", "", "", "", "" };
                    _List_Bear_Say.Add(new List<string>() { volt_before.ToString(), volt_after.ToString() });                                     
                    #endregion
                }
            }

            #region Show Bear Say
            vm._write_line = new List<string>();
            for (int i = 0; i < 4; i++)   //判斷是否NG
            {
                if (_List_Bear_Say[i].Count >= 2)
                {
                    double NG_Criteria = i == 0 ? 0.015 : 0.015;   //Setting delta V criteria
                    double delta_volt = Math.Round(double.Parse(_List_Bear_Say[i][1]) - double.Parse(_List_Bear_Say[i][0]), 4);
                    if (Math.Abs(delta_volt) > NG_Criteria)
                        _List_Bear_Say.Add(new List<string>() { delta_volt.ToString(), "NG" });
                    else
                        _List_Bear_Say.Add(new List<string>() { delta_volt.ToString(), "PASS" });

                    //Save Data to WriteLine                    
                    vm._write_line.Add(vm.BoardTable_SelectedBoard + " , " + "V" + vm.BoardTable_V123 + " , " + (10000*i+30000).ToString() + " , " + _List_Bear_Say[i][0] + " , " + _List_Bear_Say[i][1] + " , " + delta_volt.ToString() + " , " + _List_Bear_Say[i+4][1]);
                }
            }            

            vm.List_bear_say = _List_Bear_Say;
            vm.Collection_bear_say.Add(_List_Bear_Say);
            vm.bear_say_all++;
            vm.bear_say_now = vm.bear_say_all;
            vm.Show_Bear_Window(vm.List_bear_say, false, "List");
            #endregion
        }

        private void btn_board_v2_Click(object sender, RoutedEventArgs e)
        {
            _Page_Board_Grid = new Page_Board_Grid(vm);

            pageTransitionControl.ShowPage(_Page_Board_Grid);
        }

        private void btn_board_v3_Click(object sender, RoutedEventArgs e)
        {
            _Page_Board_Grid = new Page_Board_Grid(vm);

            pageTransitionControl.ShowPage(_Page_Board_Grid);
        }

        private void btn_opendialog_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
            openFileDialog.InitialDirectory = @"D:\Ref";
            //openFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                vm.txt_ref_path = openFileDialog.FileName;
            }
        }

        private void btn_save_v_Click(object sender, RoutedEventArgs e)
        {
            //// Create a string array that consists of three lines.
            //string[] lines = { "First line", "Second line", "Third line" };

            //// WriteAllLines creates a file, writes a collection of strings to the file,
            //System.IO.File.WriteAllLines(@"D:\Ref\WriteLines.txt", lines);

            using (System.IO.StreamWriter file =
            new System.IO.StreamWriter(@"D:\Ref\BoardTable.txt", true))
            {
                for (int i = 0; i < 4; i++)
                {
                    file.WriteLine(vm._write_line[i]);
                }                
                vm._write_line = new List<string>();
            }

            vm.Str_cmd_read = "Saved";
        }
    }
}
