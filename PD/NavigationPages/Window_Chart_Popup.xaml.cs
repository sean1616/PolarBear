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
using PD.ViewModel;

namespace PD.NavigationPages
{
    /// <summary>
    /// Interaction logic for Window_Chart_Popup.xaml
    /// </summary>
    public partial class Window_Chart_Popup : Window
    {
        ComViewModel vm;

        public Window_Chart_Popup(ComViewModel vm)
        {
            InitializeComponent();

            this.DataContext = vm;
            this.vm = vm;
        }
    }
}
