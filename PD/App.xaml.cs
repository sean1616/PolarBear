using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Threading.Tasks;

namespace PD
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public PD.NavigationPages.Window_Waiting ww = new NavigationPages.Window_Waiting();
        public MainWindow mainWindow;

        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            mainWindow = new MainWindow();

            if (ww != null)
                ww.Show();

            //await Task.Run(()=> mainWindow.Initializing());  //非同步初始化

            if (mainWindow != null)
                mainWindow.Show();

            mainWindow.Initializing_MainThread();  //STA緒執行

            if (ww != null)
                ww.Close();
        }
    }
}
