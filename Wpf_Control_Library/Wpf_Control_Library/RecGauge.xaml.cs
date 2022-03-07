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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Wpf_Control_Library
{
    /// <summary>
    /// Interaction logic for RecGauge.xaml
    /// </summary>
    public partial class RecGauge : UserControl
    {
        public RecGauge()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty str_PD_value_Property =
                    DependencyProperty.Register("Gauge_value", typeof(string), typeof(RecGauge),
                    new UIPropertyMetadata(null));

        public static readonly DependencyProperty Gauge_Scale_Property =
                   DependencyProperty.Register("Gauge_Scale", typeof(float), typeof(RecGauge),
                   new UIPropertyMetadata(null));

        public string Gauge_value //提供內部binding之相依屬性
        {
            get { return (string)GetValue(str_PD_value_Property); }
            set { SetValue(str_PD_value_Property, value); }
        }

        public float Gauge_Scale //提供內部binding之相依屬性
        {
            get { return (float)GetValue(Gauge_Scale_Property); }
            set { SetValue(Gauge_Scale_Property, value); }
        }
    }
}
