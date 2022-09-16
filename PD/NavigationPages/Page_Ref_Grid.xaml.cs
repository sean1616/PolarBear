using System;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Windows.Controls;
using PD.ViewModel;
using PD.Models;

namespace PD.NavigationPages
{
    /// <summary>
    /// Page_Ref_Grid.xaml 的互動邏輯
    /// </summary>
    public partial class Page_Ref_Grid : UserControl
    {
        ComViewModel vm;
        //ObservableCollection<RefModel> Ref_memberDatas = new ObservableCollection<RefModel>();

        List<Dictionary<double, double>> dictionaries = new List<System.Collections.Generic.Dictionary<double, double>>();

        public Page_Ref_Grid(ComViewModel vm, string path)
        {
            InitializeComponent();

            DataContext = vm;
            this.vm = vm;

            dataGrid.DataContext = vm.Ref_memberDatas;

            for (int i = 0; i < 16; i++)
            {
                dictionaries.Add(new Dictionary<double, double>());  //Initialize dictionaries for all ref file
            }

            if (System.IO.Directory.Exists(path))
                Read_Ref(path);
            else
                vm.Str_cmd_read = "Ref folder is not exist or different name";

            //Auto add DataGrid Column by vm.Ref_Dictionaries.Count
            if (vm.Ref_Dictionaries.Count > 8)
            {
                for (int i = 9; i <= vm.Ref_Dictionaries.Count; i++)
                {
                    var col = new DataGridTextColumn();
                    string channel = string.Concat("Ch_", i.ToString());
                    col.Header = channel;
                    col.Binding = new Binding(channel);
                    col.Width = new DataGridLength(80, DataGridLengthUnitType.Star);
                    col.MinWidth = 80;
                    dataGrid.Columns.Add(col);
                }
            }

            vm.float_WL_Ref = new List<double>();  //目前波長的Ref值

            //從字典找Ref值
            try
            {
                if (vm.Double_Laser_Wavelength == 0)
                {
                    vm.Save_Log("Get ref", "Laser Wavelength : 0", false);
                    return;
                }

                if (vm.Ref_Dictionaries.Count >= vm.ch_count)
                {
                    System.Threading.Tasks.Parallel.For(0, vm.ch_count, ch =>
                    {
                        vm.float_WL_Ref.Add(vm.Ref_Dictionaries[ch].ContainsKey(vm.Double_Laser_Wavelength) ? vm.Ref_Dictionaries[ch][vm.Double_Laser_Wavelength] : 0);
                    });
                }
            }
            catch { }
        }

        //public class Member
        //{
        //    public double Wavelength { get; set; }
        //    public double Ch_1 { get; set; }
        //    public double Ch_2 { get; set; }
        //    public double Ch_3 { get; set; }
        //    public double Ch_4 { get; set; }
        //    public double Ch_5 { get; set; }
        //    public double Ch_6 { get; set; }
        //    public double Ch_7 { get; set; }
        //    public double Ch_8 { get; set; }
        //    public double Ch_9 { get; set; }
        //    public double Ch_10 { get; set; }
        //    public double Ch_11 { get; set; }
        //    public double Ch_12 { get; set; }
        //    public double Ch_13 { get; set; }
        //    public double Ch_14 { get; set; }
        //    public double Ch_15 { get; set; }
        //    public double Ch_16 { get; set; }
        //}

        private void Read_Ref_Analyze(string path, int ch)
        {
            string filepath = $@"{path}\Ref{ch}.txt";
            bool _isfileExis = System.IO.File.Exists(filepath);

            if (_isfileExis)  //判斷Ref.txt是否存在
            {
                int counter = 0;
                string line;

                // Read the file and display it line by line.  
                System.IO.StreamReader file = new System.IO.StreamReader(filepath);

                //讀取Ref 並分析
                try
                {
                    while ((line = file.ReadLine()) != null)
                    {
                        string[] line_array = line.Split(',');

                        if (line_array.Length == 2)
                        {
                            double data_WL = Convert.ToDouble(line_array[0]);
                            double data_IL = Convert.ToDouble(line_array[1]);

                            if (!vm.list_wl.Contains(data_WL))
                                vm.list_wl.Add(data_WL);  //新增波長

                            dictionaries[ch - 1].Add(Math.Round(data_WL,2), data_IL);          //Add ref to dictionary                   
                        }
                        else
                            vm.Save_Log("Gef ref", "Ref format is wrong", false);

                        counter++;
                    }

                    vm.list_wl.Remove(0);
                }
                catch { }

                file.Close();

                vm.Ref_Dictionaries = dictionaries;
            }
        }

        private void Read_Ref(string path)
        {
            vm.list_wl = new List<double>();

            try
            {
                System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                sw.Reset();
                sw.Start();

                System.Threading.Tasks.Parallel.For(1, 17, ch => { Read_Ref_Analyze(path, ch); });  //平行運算
                sw.Stop();

                vm.Save_Log("Read Reference", vm.ch_count.ToString(), "TimeSpan", sw.ElapsedMilliseconds.ToString() + " ms");

                double d = new double();

                //int count_ch_Number = vm.Ref_Dictionaries.Count;
                foreach (double wl in vm.list_wl) //顯示讀取的Ref data
                {
                    vm.Ref_memberDatas.Add(new RefModel()
                    {
                        Wavelength = wl,
                        Ch_1 = dictionaries[0].ContainsKey(wl) ? dictionaries[0][wl] : d,
                        Ch_2 = dictionaries[1].ContainsKey(wl) ? dictionaries[1][wl] : d,
                        Ch_3 = dictionaries[2].ContainsKey(wl) ? dictionaries[2][wl] : d,
                        Ch_4 = dictionaries[3].ContainsKey(wl) ? dictionaries[3][wl] : d,
                        Ch_5 = dictionaries[4].ContainsKey(wl) ? dictionaries[4][wl] : d,
                        Ch_6 = dictionaries[5].ContainsKey(wl) ? dictionaries[5][wl] : d,
                        Ch_7 = dictionaries[6].ContainsKey(wl) ? dictionaries[6][wl] : d,
                        Ch_8 = dictionaries[7].ContainsKey(wl) ? dictionaries[7][wl] : d,
                        Ch_9 = dictionaries[8].ContainsKey(wl) ? dictionaries[8][wl] : d,
                        Ch_10 = dictionaries[9].ContainsKey(wl) ? dictionaries[9][wl] : d,
                        Ch_11 = dictionaries[10].ContainsKey(wl) ? dictionaries[10][wl] : d,
                        Ch_12 = dictionaries[11].ContainsKey(wl) ? dictionaries[11][wl] : d,
                        Ch_13 = dictionaries[12].ContainsKey(wl) ? dictionaries[12][wl] : d,
                        Ch_14 = dictionaries[13].ContainsKey(wl) ? dictionaries[13][wl] : d,
                        Ch_15 = dictionaries[14].ContainsKey(wl) ? dictionaries[14][wl] : d,
                        Ch_16 = dictionaries[15].ContainsKey(wl) ? dictionaries[15][wl] : d,
                    });
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Get Ref.txt error");
                MessageBox.Show(ex.StackTrace.ToString());
            }
        }
    }
}
