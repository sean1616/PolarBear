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

using PD.ViewModel;

namespace PD.UI
{
    /// <summary>
    /// UC_Gauge.xaml 的互動邏輯
    /// </summary>
    public partial class UC_Gauge : UserControl
    {
        public UC_Gauge()
        {
            InitializeComponent();

            grid_dac_txt.DataContext = this;

            //btn_1.Click += new System.Windows.RoutedEventHandler(innerButton_Click);
        }

        #region 定義相依屬性       

        public static readonly DependencyProperty List_D0_value_1_Property =
                DependencyProperty.Register("List_D0_value_1", typeof(string), typeof(UC_Gauge),
                new UIPropertyMetadata(null));

        public static readonly DependencyProperty List_D0_value_2_Property =
                    DependencyProperty.Register("List_D0_value_2", typeof(string), typeof(UC_Gauge),
                    new UIPropertyMetadata(null));

        public static readonly DependencyProperty List_D0_value_3_Property =
                   DependencyProperty.Register("List_D0_value_3", typeof(string), typeof(UC_Gauge),
                   new UIPropertyMetadata(null));

        public static readonly DependencyProperty List_bear_say_1_Property =
                DependencyProperty.Register("List_bear_say_1", typeof(string), typeof(UC_Gauge),
                new UIPropertyMetadata(null));

        public static readonly DependencyProperty List_bear_say_2_Property =
                DependencyProperty.Register("List_bear_say_2", typeof(string), typeof(UC_Gauge),
                new UIPropertyMetadata(null));

        public static readonly DependencyProperty List_bear_say_3_Property =
                DependencyProperty.Register("List_bear_say_3", typeof(string), typeof(UC_Gauge),
                new UIPropertyMetadata(null));

        public string List_D0_value_1 //提供內部binding之相依屬性
        {
            get { return (string)GetValue(List_D0_value_1_Property); }
            set { SetValue(List_D0_value_1_Property, value); }
        }

        public string List_D0_value_2 //提供內部binding之相依屬性
        {
            get { return (string)GetValue(List_D0_value_2_Property); }
            set { SetValue(List_D0_value_2_Property, value); }
        }

        public string List_D0_value_3 //提供內部binding之相依屬性
        {
            get { return (string)GetValue(List_D0_value_3_Property); }
            set { SetValue(List_D0_value_3_Property, value); }
        }

        public string List_bear_say_1 //提供內部binding之相依屬性
        {
            get { return (string)GetValue(List_bear_say_1_Property); }
            set { SetValue(List_bear_say_1_Property, value); }
        }

        public string List_bear_say_2 //提供內部binding之相依屬性
        {
            get { return (string)GetValue(List_bear_say_2_Property); }
            set { SetValue(List_bear_say_2_Property, value); }
        }

        public string List_bear_say_3 //提供內部binding之相依屬性
        {
            get { return (string)GetValue(List_bear_say_3_Property); }
            set { SetValue(List_bear_say_3_Property, value); }
        }
        #endregion

        #region Click event

        //private void Btn_1_Click(object sender, RoutedEventArgs e)
        //{
        //    RaiseEvent(new RoutedEventArgs(Btn_WL_ClickEvent, this));
        //}

        //public static readonly RoutedEvent Btn_WL_ClickEvent = EventManager.RegisterRoutedEvent(
        //    "Click", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(UC_Gauge));

        //public event RoutedEventHandler Btn_WL_Click
        //{
        //    add { AddHandler(Btn_WL_ClickEvent, value); }
        //    remove { RemoveHandler(Btn_WL_ClickEvent, value); }
        //}


        public event RoutedEventHandler Btn_WL_Click = delegate { };
        private void Btn_1_Click(object sender, RoutedEventArgs e)
        {
            Btn_WL_Click(sender, e);
        }

        public event RoutedEventHandler Btn_IL_Click = delegate { };
        private void Btn_2_Click(object sender, RoutedEventArgs e)
        {
            Btn_IL_Click(sender, e);
        }
                
        public event RoutedEventHandler Btn_Voltage_Click = delegate { };
        private void Btn_3_Click(object sender, RoutedEventArgs e)
        {
            Btn_Voltage_Click(sender, e);
        }
        #endregion


        #region Click event


        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(TextBox_GotFocusEvent, this));
        }

        //註冊事件
        public static readonly RoutedEvent TextBox_GotFocusEvent = EventManager.RegisterRoutedEvent(
            "PreviewKeyDown", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(UC_Gauge));
        
        //註冊事件的名稱
        public event RoutedEventHandler TxtBox_GotFocus
        {
            add { AddHandler(TextBox_GotFocusEvent, value); }
            remove { RemoveHandler(TextBox_GotFocusEvent, value); }
        }
        #endregion

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void _GaugeTxT__GotFocus(object sender, RoutedEventArgs e)
        {
            //gaugetxt_focus = true;
        }

        private void _GaugeTxT__LostFocus(object sender, RoutedEventArgs e)
        {
            //gaugetxt_focus = false;
        }

       
    }
}
