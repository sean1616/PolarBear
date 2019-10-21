using System;
using System.Windows;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Windows.Controls;
using PD.ViewModel;

namespace PD.NavigationPages
{
    /// <summary>
    /// Page_Ref_Grid.xaml 的互動邏輯
    /// </summary>
    public partial class Page_Ref_Grid : UserControl
    {
        ComViewModel vm;
        ObservableCollection<Member> memberData = new ObservableCollection<Member>();

        public Page_Ref_Grid(ComViewModel vm, string path)
        {
            InitializeComponent();

            DataContext = vm;
            this.vm = vm;
            
            dataGrid.DataContext = memberData;

            Read_Ref(path);
        }

        public class Member
        {
            public double Wavelength { get; set; }
            public double Ch_1 { get; set; }
            public double Ch_2 { get; set; }
            public double Ch_3 { get; set; }
            public double Ch_4 { get; set; }
            public double Ch_5 { get; set; }
            public double Ch_6 { get; set; }
            public double Ch_7 { get; set; }
            public double Ch_8 { get; set; }
        }

        private void Read_Ref(string path)
        {
            vm.list_wl = new List<double>();
            vm.list_WL_Ref = new List<List<double>>();

            try
            {
                for (int ch = 1; ch <= 8; ch++)
                {
                    bool _isfileExis = System.IO.File.Exists(path + @"\Ref" + ch.ToString() + ".txt");

                    if (_isfileExis == true)  //判斷Ref.txt是否存在
                    {
                        int counter = 0;
                        string line;
                        vm.list_WL_Ref.Add(new List<double>() { });

                        // Read the file and display it line by line.  
                        System.IO.StreamReader file = new System.IO.StreamReader(path + @"\Ref" + ch.ToString() + ".txt");

                        //讀取Ref 並分析
                        while ((line = file.ReadLine()) != null)
                        {
                            string[] line_array = line.Split(',');
                            if (line_array.Length == 2)
                            {
                                if (ch == 1)
                                    vm.list_wl.Add(Convert.ToDouble(line_array[0]));  //新增波長

                                vm.list_WL_Ref[ch - 1].Add(Convert.ToDouble(line_array[1]));  //新增Ref
                            }
                            else
                                vm.Str_cmd_read = "Ref format is wrong";
                            counter++;
                        }

                        file.Close();
                    }
                    else
                    {
                        vm.list_WL_Ref.Add(new List<double>() { });
                        int c = vm.list_WL_Ref[ch - 2].Count;
                        for (int i = 0; i < c; i++)
                        {
                            vm.list_WL_Ref[ch - 1].Add(0);
                        }
                    }
                }

                int count = 0;
                int count_ch_Number = vm.list_WL_Ref.Count;
                foreach (double wl in vm.list_wl) //顯示讀取的Ref data
                {
                    if (count_ch_Number == 8)
                        memberData.Add(new Member()
                        {
                            Wavelength = wl,
                            Ch_1 = vm.list_WL_Ref[0][count],
                            Ch_2 = vm.list_WL_Ref[1][count],
                            Ch_3 = vm.list_WL_Ref[2][count],
                            Ch_4 = vm.list_WL_Ref[3][count],
                            Ch_5 = vm.list_WL_Ref[4][count],
                            Ch_6 = vm.list_WL_Ref[5][count],
                            Ch_7 = vm.list_WL_Ref[6][count],
                            Ch_8 = vm.list_WL_Ref[7][count]
                        });
                    else
                        MessageBox.Show("Ref ch count != 8");
                    count++;
                }
            }
            catch { MessageBox.Show("Get Ref.txt error"); }
        }
    }
}
