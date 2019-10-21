using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace PD.UI
{
    /// <summary>
    /// PD_Gauge.xaml 的互動邏輯
    /// </summary>

    public partial class PD_Gauge : UserControl
    {        
        public PD_Gauge()
        {
            InitializeComponent();

            this.DataContext = this;
        }

        #region 定義相依屬性       

        public static readonly DependencyProperty str_btn_content_Property =
                DependencyProperty.Register("str_btn_content", typeof(string), typeof(PD_Gauge),
                new UIPropertyMetadata(null));

        public static readonly DependencyProperty str_PD_value_Property =
                    DependencyProperty.Register("str_PD_value", typeof(string), typeof(PD_Gauge),
                    new UIPropertyMetadata(null));

        public static readonly DependencyProperty str_channel_Property =
                    DependencyProperty.Register("str_channel", typeof(string), typeof(PD_Gauge),
                    new UIPropertyMetadata(null));

        public static readonly DependencyProperty Arc_EndAngle_Property =
                    DependencyProperty.Register("Arc_EndAngle", typeof(float), typeof(PD_Gauge),
                    new UIPropertyMetadata(null));

        public static readonly DependencyProperty Arc_Color_Property =
                    DependencyProperty.Register("Arc_Color", typeof(SolidColorBrush), typeof(PD_Gauge),
                    new UIPropertyMetadata(null));

        public static readonly DependencyProperty str_Unit_Property =
                    DependencyProperty.Register("str_Unit", typeof(string), typeof(PD_Gauge),
                    new UIPropertyMetadata(null));
               
        public string str_btn_content //提供內部binding之相依屬性
        {
            get { return (string)GetValue(str_btn_content_Property); }
            set { SetValue(str_btn_content_Property, value); }
        }

        public string str_PD_value //提供內部binding之相依屬性
        {
            get { return (string)GetValue(str_PD_value_Property); }
            set { SetValue(str_PD_value_Property, value); }
        }

        public string str_channel //提供內部binding之相依屬性
        {
            get { return (string)GetValue(str_channel_Property); }
            set { SetValue(str_channel_Property, value); }
        }

        public float Arc_EndAngle //提供內部binding之相依屬性
        {
            get { return (float)GetValue(Arc_EndAngle_Property); }
            set { SetValue(Arc_EndAngle_Property, value); }
        }

        public SolidColorBrush Arc_Color //提供內部binding之相依屬性
        {
            get { return (SolidColorBrush)GetValue(Arc_Color_Property); }
            set { SetValue(Arc_Color_Property, value); }
        }

        public string str_Unit //提供內部binding之相依屬性
        {
            get { return (string)GetValue(str_Unit_Property); }
            set { SetValue(str_Unit_Property, value); }
        }
        #endregion
    }
}
