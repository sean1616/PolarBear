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

namespace PD.UI
{
    /// <summary>
    /// Interaction logic for UC_TimerBlock.xaml
    /// </summary>
    public partial class UC_TimerBlock : UserControl
    {
        public UC_TimerBlock()
        {
            InitializeComponent();
        }

        #region 定義Timer相依屬性       

        public static readonly DependencyProperty Arc_EndAngle_Property =
                DependencyProperty.Register("Gauge_EndAngle", typeof(float), typeof(UC_TimerBlock),
                new UIPropertyMetadata(null));

        public static readonly DependencyProperty Arc_Color_Property =
                    DependencyProperty.Register("Gauge_Color", typeof(SolidColorBrush), typeof(UC_TimerBlock),
                    new UIPropertyMetadata(null));

        public static readonly DependencyProperty Txt_Time_Property =
                    DependencyProperty.Register("Txt_Time", typeof(string), typeof(UC_TimerBlock),
                    new UIPropertyMetadata(null));

        public static readonly DependencyProperty Txt_Time_Msg_Property =
                    DependencyProperty.Register("Txt_Time_Msg", typeof(string), typeof(UC_TimerBlock),
                    new UIPropertyMetadata(null));
            
        public float Gauge_EndAngle //提供內部binding之相依屬性
        {
            get { return (float)GetValue(Arc_EndAngle_Property); }
            set { SetValue(Arc_EndAngle_Property, value); }
        }

        public SolidColorBrush Gauge_Color //提供內部binding之相依屬性
        {
            get { return (SolidColorBrush)GetValue(Arc_Color_Property); }
            set { SetValue(Arc_Color_Property, value); }
        }
        
        public string Txt_Time //提供內部binding之相依屬性
        {
            get { return (string)GetValue(Txt_Time_Property); }
            set { SetValue(Txt_Time_Property, value); }
        }

        public string Txt_Time_Msg //提供內部binding之相依屬性
        {
            get { return (string)GetValue(Txt_Time_Msg_Property); }
            set { SetValue(Txt_Time_Msg_Property, value); }
        }

        //public string Txt_Time_Msg //提供內部binding之相依屬性
        //{
        //    get { return (string)GetValue(Txt_Time_Msg_Property); }
        //    set { SetValue(Txt_Time_Msg_Property, value); }
        //}

        #endregion

        private void txtBlock_timer_Value_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox obj = (TextBox)sender;
            SetValue(Txt_Time_Property, obj.Text);
        }
    }
}
