using PD.Functions;
using PD.ViewModel;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Excel = Microsoft.Office.Interop.Excel;
using PD.Models;
using OxyPlot.Wpf;
using OxyPlot;

namespace PD.NavigationPages
{
    /// <summary>
    /// Page_Chart.xaml 的互動邏輯
    /// </summary>
    public partial class Page_Chart : System.Windows.Controls.UserControl
    {
        //public PlotModel MyModel { get; private set; }
#pragma warning disable CS0109 // Member does not hide an inherited member; new keyword is not required
        public new string Title { get; private set; }
#pragma warning restore CS0109 // Member does not hide an inherited member; new keyword is not required
        ComViewModel vm;
        //public IList<DataPoint> Points { get; private set; }

        ControlCmd cmd;

        public Page_Chart(ComViewModel vm)
        {
            InitializeComponent();

            this.DataContext = vm;
            this.vm = vm;
            viewer.DataContext = vm;  //將DataContext指給使用者控制項，必要!

            cmd = new ControlCmd(vm);

            //window_Bear_Grid = new Window_Bear_Grid(vm, "Delta IL");
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //一個plotModel同一時間只能供給一個plotview使用，頁面切換時需重新指派
            vm.PlotViewModel_TF2 = new PlotModel();
            vm.PlotViewModel_UTF600 = new PlotModel();
            vm.PlotViewModel_BR = new PlotModel();
            vm.PlotViewModel_Testing = new PlotModel();
            vm.PlotViewModel_Chart = vm.PlotViewModel;

            vm.Update_ALL_PlotView();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {

            for (int i = 0; i < vm.list_Chart_UI_Models.Count; i++)
            {
                if (vm.Plot_Series.Count > i)
                {
                    vm.Plot_Series[i].IsVisible = vm.list_Chart_UI_Models[i].Button_IsChecked;
                    vm.Plot_Series[i].Color = vm.list_OxyColor[i];
                }

                OxyPlot.Annotations.LineAnnotation lat = vm.PlotViewModel.Annotations[i] as OxyPlot.Annotations.LineAnnotation;
                if (lat != null)
                {
                    if (vm.list_Chart_UI_Models[i].Button_IsChecked)
                    {
                        lat.StrokeThickness = 2;
                        lat.TextColor = OxyColors.Black;
                    }
                    else
                    {
                        lat.StrokeThickness = 0;
                        lat.TextColor = OxyColors.Transparent;
                    }
                    lat.TextLinePosition = 1 - (0.1 * i);  //Vertical postion of annotation title text
                }
            }

            vm.Update_ALL_PlotView();
        }

        private void ALL_CheckBox_Click(object sender, RoutedEventArgs e)
        {
            bool judge = false;  //if one channel is not checked
            foreach (Chart_UI_Model chUIModel in vm.list_Chart_UI_Models)
            {
                judge = chUIModel.Button_IsChecked;

                if (!judge)
                    break;
            }

            cbox_all.IsChecked = judge ? false : true;

            for (int i = 0; i < vm.list_Chart_UI_Models.Count; i++)
            {
                vm.list_Chart_UI_Models[i].Button_IsChecked = !judge;

                OxyPlot.Annotations.LineAnnotation lat = vm.PlotViewModel.Annotations[i] as OxyPlot.Annotations.LineAnnotation;
                lat.StrokeThickness = 2;
                lat.TextColor = OxyColors.Black;
                lat.TextLinePosition = 1 - (0.1 * i);  //Vertical postion of annotation title text
            }
        }

        private void btn_Save_Chart_Click(object sender, RoutedEventArgs e)
        {
            #region Save Chart

            if (mainPlotView.ActualHeight == 0 || mainPlotView.ActualWidth == 0) return;

            bool isDataCount = false;
            for (int i = 0; i < vm.Plot_Series.Count; i++)
            {
                if (vm.Plot_Series[i].Points.Count > 0)
                    isDataCount = true;
            }

            if (!isDataCount)
            {
                var rst = System.Windows.MessageBox.Show("無任何資料，仍要儲存？", "Save Data", MessageBoxButton.YesNo);
                if (rst == MessageBoxResult.No) return;
            }

            RenderTargetBitmap renderTargetBitmap =
               new RenderTargetBitmap((int)mainPlotView.ActualWidth + 30, (int)mainPlotView.ActualHeight + 10, 96, 96, PixelFormats.Pbgra32);
            renderTargetBitmap.Render(mainPlotView);
            PngBitmapEncoder pngImage = new PngBitmapEncoder();
            pngImage.Frames.Add(BitmapFrame.Create(renderTargetBitmap));

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = "Save Data";
            //saveFileDialog.InitialDirectory = @"D:\PD";
            saveFileDialog.FileName = "Image001.jpg";
            saveFileDialog.Filter = "Image (*.jpg)|*.jpg|All files (*.*)|*.*";

            string img_filePath = @"D:\PD\Image001.jpg";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                img_filePath = saveFileDialog.FileName;

                using (Stream fileStream = File.Create(img_filePath))
                {
                    pngImage.Save(fileStream);
                }
            }

            string folder = Directory.GetParent(img_filePath).ToString();
            #endregion

            #region Save Datapoint
            bool csv_or_excel = true;

            ////Save as excel file
            //try
            //{
            //    //設定檔案路徑
            //    string datapoint_filePath = Path.Combine(folder, Path.GetFileNameWithoutExtension(img_filePath));
            //    //string datapoint_filePath = @"D:\PD\" + Path.GetFileNameWithoutExtension(img_filePath);

            //    //應用程序
            //    Excel.Application Excel_App1 = new Excel.Application();
            //    //檔案
            //    Excel.Workbook Excel_WB1 = Excel_App1.Workbooks.Add();
            //    //工作表
            //    Excel.Worksheet Excel_WS1 = new Excel.Worksheet();
            //    Excel_WS1 = Excel_WB1.Worksheets[1];
            //    Excel_WS1.Name = "001";

            //    //儲存格
            //    for (int i = 1; i <= vm.Chart_All_DataPoints.Count; i++)
            //    {
            //        if (vm.Chart_All_DataPoints[i - 1].Count != 0)
            //            Excel_App1.Cells[i, 1] = "Ch " + i.ToString();

            //        if (vm.Chart_All_DataPoints[i - 1].Count > 0)
            //        {
            //            for (int j = 1; j <= vm.Chart_All_DataPoints[i - 1].Count; j++)
            //            {
            //                Excel_App1.Cells[j + 1, i] = vm.Chart_All_DataPoints[i - 1][j - 1].X.ToString();
            //            }
            //        }
            //    }

            //    //存檔
            //    Excel_WB1.SaveAs(datapoint_filePath);
            //    //關閉物件
            //    Excel_WS1 = null;
            //    Excel_WB1.Close();
            //    Excel_WB1 = null;
            //    Excel_App1.Quit();
            //    Excel_App1 = null;
            //}
            //catch { csv_or_excel = false; }

            csv_or_excel = false;

            //若電腦沒有安裝Excel，則存成Csv檔
            if (!csv_or_excel)
            {
                string csvpath = Path.Combine(folder, Path.GetFileNameWithoutExtension(img_filePath)) + ".csv";
                //string csvpath = @"D:\PD\" + Path.GetFileNameWithoutExtension(img_filePath) + ".csv";

                if (!File.Exists(csvpath))
                {
                    File.AppendAllText(csvpath, "");
                }

                //Clear file content and lock the file
                FileStream fileStream = File.Open(csvpath, FileMode.Open);
                fileStream.SetLength(0);
                fileStream.Close();

                StringBuilder _stringBuilder = new StringBuilder();

                _stringBuilder.Append(vm.Chart_x_title);
                _stringBuilder.Append(",");

                //資料Title
                for (int i = 0; i < vm.Plot_Series.Count; i++)
                {
                    if (vm.Plot_Series[i].Points.Count != 0)
                    {
                        _stringBuilder.Append("Ch " + (i + 1).ToString());
                        _stringBuilder.Append(",");
                    }
                }

                _stringBuilder.AppendLine();

                List<int> list_data_count = new List<int>();
                //foreach (List<OxyPlot.DataPoint> d in vm.Chart_All_DataPoints)
                //{
                //    list_data_count.Add(d.Count);
                //}

                foreach (var ls in vm.Plot_Series)
                {
                    list_data_count.Add(ls.Points.Count);
                }

                int max_dataCount = list_data_count.Max();

                //資料內容
                for (int i = 0; i < max_dataCount; i++)
                {
                    _stringBuilder.Append(vm.Plot_Series[0].Points[i].X);
                    _stringBuilder.Append(",");

                    for (int j = 0; j < vm.Plot_Series.Count; j++)
                    {
                        try
                        {
                            _stringBuilder.Append(vm.Plot_Series[j].Points[i].Y);
                            _stringBuilder.Append(",");
                        }
                        catch { }
                    }
                    _stringBuilder.AppendLine();
                }

                File.AppendAllText(csvpath, _stringBuilder.ToString());  //Save string builder to csv
            }

            #endregion

            vm.Show_Bear_Window("Image & Data Saved", false, "String", false);
        }

        public Window_Bear_Timer window_Bear;
        private void btn_Timer_Click(object sender, RoutedEventArgs e)
        {
            window_Bear = new Window_Bear_Timer(vm);
            window_Bear.Show();
        }

        //private void btn_PopupWindow_Click(object sender, RoutedEventArgs e)
        //{
        //    Window_Chart_Popup window = new Window_Chart_Popup(vm);
        //    window.Show();
        //}



        //public Window_Bear_Grid window_Bear_Grid;
    }
}
