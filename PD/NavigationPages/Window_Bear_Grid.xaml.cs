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

using PD.ViewModel;
using PD.Models;
using PD.Functions;
using PD.AnalysisModel;

namespace PD.NavigationPages
{
    /// <summary>
    /// Interaction logic for Window_Bear_Grid.xaml
    /// </summary>
    public partial class Window_Bear_Grid : Window
    {
        ComViewModel vm;
        ControlCmd cmd;

        //public ObservableCollection<Member> memberData = new ObservableCollection<Member>();
               
        public Window_Bear_Grid(ComViewModel vm, string title)
        {
            InitializeComponent();

            this.vm = vm;
            this.DataContext = vm;
                        
            cmd = new ControlCmd(vm);            

            dataGrid.DataContext = vm.DeltaILMembers;
        }
        
        private void btn_close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }                      

        private void btn_min_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        //public class Member
        //{
        //    public string Channel { get; set; }
        //    public string Delta_IL { get; set; }            
        //}

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < vm.ch_count; i++)
            {
                vm.DeltaILMembers.Add(new DeltaILMember()
                {
                    Channel = "Ch " + (i + 1).ToString() + " :",
                    Delta_IL = "0",
                });
            }
        }


        private bool mRestoreForDragMove;
        private void border_title_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //判斷滑鼠點擊次數
            if (e.ClickCount == 2)
            {
                if ((this.ResizeMode != ResizeMode.CanResize) && (this.ResizeMode != ResizeMode.CanResizeWithGrip))
                    return;
                this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized; //雙擊最大化
            }
            else
            {
                mRestoreForDragMove = this.WindowState == WindowState.Normal;
            }
        }

        private void border_title_MouseMove(object sender, MouseEventArgs e)
        {
            if (mRestoreForDragMove && this.WindowState == WindowState.Normal)
            {
                //mRestoreForDragMove = false;
                mRestoreForDragMove = false;
                try
                {
                    this.DragMove();
                }
                catch { }
            }
        }

        private void border_title_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            mRestoreForDragMove = false;
        }

        private void btn_cal_deltaIL_Click(object sender, RoutedEventArgs e)
        {
            if (vm.Chart_DataPoints.Count == 0) return;            

            vm.List_bear_grid_data = new System.Collections.ObjectModel.ObservableCollection<string>();
            
            try
            {               
                for (int i = 0; i < vm.ch_count; i++)
                {
                    if (i != 0)
                    {
                        //var Series = vm.Chart_DataPoints.OrderBy(o => o.Y).ToList();
                        //OxyPlot.DataPoint Dp_IL_Max = orderedSeries.Last();
                        //double IL_Max = Dp_IL_Max.Y;

                        //memberData.Add(new Member()
                        //{
                        //    Channel = "Ch " + (i + 1).ToString() + " :",
                        //    Delta_IL = str_maxDeltaIL,
                        //}
                        //   );
                    }
                    else
                    {
                       
                        dataGrid.Items.Refresh();
                    }
                   

                    //List<double> list_all_PD_Value = new List<double>();
                    //int c = vm.Chart_All_DataPoints[i].Count();

                    //for (int j = 0; j < c; j++)
                    //{
                    //    list_all_PD_Value.Add(vm.Chart_All_DataPoints[i][j].Y);
                    //}
                    //string str_maxDeltaIL = Math.Round(list_all_PD_Value.Max() - list_all_PD_Value.Min(), 6).ToString();

                    //vm.List_bear_grid_data.Add(str_maxDeltaIL);

                   
                }                               
            }
            catch { vm.Show_Bear_Window("No Data", false, "String", false); }
        }
    }
}
