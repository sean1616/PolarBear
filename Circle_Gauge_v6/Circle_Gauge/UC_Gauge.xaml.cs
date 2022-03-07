using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Controls.Primitives;

namespace Circle_Gauge
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class UserControl1 : UserControl
    {
        public UserControl1()
        {
            InitializeComponent();

            //this.DataContext = this;
        }

        public event RoutedEventHandler TBtn_PreviewMouseRightButtonDown = delegate { };
        private void tbtn_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            //vm.is_Gauge_ContinueSelect = true;
            bool_Gauge_ContinueSelect = true;
            ToggleButton obj = (ToggleButton)sender;
            obj.IsChecked = !obj.IsChecked;
            TBtn_PreviewMouseRightButtonDown(sender, e);
        }

        public event RoutedEventHandler TBtn_PreviewMouseRightButtonUp = delegate { };
        private void tbtn_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            TBtn_PreviewMouseRightButtonUp(sender, e);
            //vm.is_Gauge_ContinueSelect = false;
            bool_Gauge_ContinueSelect = false;
        }

        public event RoutedEventHandler TBtn_PreviewMouseLeftButtonDown = delegate { };
        private void tbtn_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ToggleButton obj = (ToggleButton)sender;
            obj.Tag = Tag;
            TBtn_PreviewMouseLeftButtonDown(sender, e);
        }

        public event RoutedEventHandler TBtn_PreviewMouseLeftButtonUp = delegate { };
        private void tbtn_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            TBtn_PreviewMouseLeftButtonUp(sender, e);
        }                

        #region 定義Gauge相依屬性       
        public static readonly DependencyProperty Bool_Gauge_Property =
                    DependencyProperty.Register("Bool_Gauge", typeof(bool), typeof(UserControl1),
                    new UIPropertyMetadata(null));

        public bool Bool_Gauge //提供內部binding之相依屬性
        {
            get { return (bool)GetValue(Bool_Gauge_Property); }
            set { SetValue(Bool_Gauge_Property, value); }
        }

        public static readonly DependencyProperty str_PD_value_Property =
                    DependencyProperty.Register("Gauge_value", typeof(string), typeof(UserControl1),
                    new UIPropertyMetadata(null));

        public string Gauge_value //提供內部binding之相依屬性
        {
            get { return (string)GetValue(str_PD_value_Property); }
            set { SetValue(str_PD_value_Property, value); }
        }

        public static readonly DependencyProperty str_channel_Property =
                    DependencyProperty.Register("Gauge_channel", typeof(string), typeof(UserControl1),
                    new UIPropertyMetadata(null));

        public string Gauge_channel //提供內部binding之相依屬性
        {
            get { return (string)GetValue(str_channel_Property); }
            set { SetValue(str_channel_Property, value); }
        }

        public static readonly DependencyProperty Arc_EndAngle_Property =
                    DependencyProperty.Register("Gauge_EndAngle", typeof(float), typeof(UserControl1),
                    new UIPropertyMetadata(null));

        public float Gauge_EndAngle //提供內部binding之相依屬性
        {
            get { return (float)GetValue(Arc_EndAngle_Property); }
            set { SetValue(Arc_EndAngle_Property, value); }
        }

        public static readonly DependencyProperty Arc_Color_Property =
                    DependencyProperty.Register("Gauge_Color", typeof(SolidColorBrush), typeof(UserControl1),
                    new UIPropertyMetadata(null));

        public SolidColorBrush Gauge_Color //提供內部binding之相依屬性
        {
            get { return (SolidColorBrush)GetValue(Arc_Color_Property); }
            set { SetValue(Arc_Color_Property, value); }
        }

        public static readonly DependencyProperty str_Unit_Property =
                    DependencyProperty.Register("Gauge_Unit", typeof(string), typeof(UserControl1),
                    new UIPropertyMetadata(null));

        public string Gauge_Unit //提供內部binding之相依屬性
        {
            get { return (string)GetValue(str_Unit_Property); }
            set { SetValue(str_Unit_Property, value); }
        }

        public static readonly DependencyProperty bool_Gauge_ContinueSelect_Property =
                 DependencyProperty.Register("bool_Gauge_ContinueSelect", typeof(bool), typeof(UserControl1),
                 new UIPropertyMetadata(null));

        public bool bool_Gauge_ContinueSelect //提供內部binding之相依屬性
        {
            get { return (bool)GetValue(bool_Gauge_ContinueSelect_Property); }
            set { SetValue(bool_Gauge_ContinueSelect_Property, value); }
        }

        public static readonly DependencyProperty Gauge_Mode_Property =
                 DependencyProperty.Register("Gauge_Mode", typeof(Visibility), typeof(UserControl1),
                 new UIPropertyMetadata(null));

        public Visibility Gauge_Mode //提供內部binding之相依屬性
        {
            get { return (Visibility)GetValue(Gauge_Mode_Property); }
            set { SetValue(Gauge_Mode_Property, value); }
        }

        public static readonly DependencyProperty GaugeD0_Select_Property =
                    DependencyProperty.Register("GaugeD0_Select", typeof(string), typeof(UserControl1),
                    new UIPropertyMetadata(null));

        public string GaugeD0_Select //提供內部binding之相依屬性
        {
            get { return (string)GetValue(GaugeD0_Select_Property); }
            set { SetValue(GaugeD0_Select_Property, value); }
        }

        #endregion

        #region 定義Grid control相依屬性       
        public static readonly DependencyProperty SN_number_Property =
                    DependencyProperty.Register("SN_number", typeof(string), typeof(UserControl1),
                    new UIPropertyMetadata(null));

        public static readonly DependencyProperty D0_value_1_Property =
                    DependencyProperty.Register("D0_value_1", typeof(string), typeof(UserControl1),
                    new UIPropertyMetadata(null));

        public static readonly DependencyProperty D0_value_2_Property =
                    DependencyProperty.Register("D0_value_2", typeof(string), typeof(UserControl1),
                    new UIPropertyMetadata(null));

        public static readonly DependencyProperty D0_value_3_Property =
                    DependencyProperty.Register("D0_value_3", typeof(string), typeof(UserControl1),
                    new UIPropertyMetadata(null));

        public static readonly DependencyProperty List_bear_say_1_Property =
                DependencyProperty.Register("List_bear_say_1", typeof(string), typeof(UserControl1),
                new UIPropertyMetadata(null));

        public static readonly DependencyProperty List_bear_say_2_Property =
                DependencyProperty.Register("List_bear_say_2", typeof(string), typeof(UserControl1),
                new UIPropertyMetadata(null));

        public static readonly DependencyProperty List_bear_say_3_Property =
                DependencyProperty.Register("List_bear_say_3", typeof(string), typeof(UserControl1),
                new UIPropertyMetadata(null));

        public static readonly DependencyProperty SN_Row_Property =
               DependencyProperty.Register("SN_Row", typeof(int), typeof(UserControl1),
               new UIPropertyMetadata(null));

        public int SN_Row //提供內部binding之相依屬性
        {
            get { return (int)GetValue(SN_Row_Property); }
            set { SetValue(SN_Row_Property, value); }
        }

        public string SN_number //提供內部binding之相依屬性
        {
            get { return (string)GetValue(SN_number_Property); }
            set { SetValue(SN_number_Property, value); }
        }

        public string D0_value_1 //提供內部binding之相依屬性
        {
            get { return (string)GetValue(D0_value_1_Property); }
            set { SetValue(D0_value_1_Property, value); }
        }

        public string D0_value_2 //提供內部binding之相依屬性
        {
            get { return (string)GetValue(D0_value_2_Property); }
            set { SetValue(D0_value_2_Property, value); }
        }

        public string D0_value_3 //提供內部binding之相依屬性
        {
            get { return (string)GetValue(D0_value_3_Property); }
            set { SetValue(D0_value_3_Property, value); }
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

        public event RoutedEventHandler Txt_SN_Changed = delegate { };
        private void _txt_1_TextChanged(object sender, TextChangedEventArgs e)
        {
            SN_number = _txt_1.Text;
            Txt_SN_Changed(sender, e);
        }

        public event RoutedEventHandler Btn_WL_Click = delegate { };
        private void Btn_1_Click(object sender, RoutedEventArgs e)
        {
            Button obj = (Button)sender;
            obj.Tag = this.Tag;
            Btn_WL_Click(sender, e);
            //UC_Gauge_Tbn.IsChecked = !UC_Gauge_Tbn.IsChecked;
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


        #region Click event 註冊事件

        //private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        //{
        //    RaiseEvent(new RoutedEventArgs(TextBox_GotFocusEvent, this));
        //}

        //註冊事件
        //public static readonly RoutedEvent TextBox_GotFocusEvent = EventManager.RegisterRoutedEvent(
        //    "PreviewKeyDown", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(UC_Gauge));

        //註冊事件的名稱
        //public event RoutedEventHandler TxtBox_GotFocus
        //{
        //    add { AddHandler(TextBox_GotFocusEvent, value); }
        //    remove { RemoveHandler(TextBox_GotFocusEvent, value); }
        //}
        #endregion

        public event RoutedEventHandler TxtBox_GotFocus = delegate { };
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox obj = sender as TextBox;
            //obj.Tag = this.Tag;
            GaugeD0_Select = obj.Name.ToString().Substring(1);  //如果要從控制項內部驅動變數，則在使用端Xaml需選擇mode=twoway
            //MessageBox.Show(GaugeD0_Select);
            TxtBox_GotFocus(sender, e);
        }

        public event RoutedEventHandler TxtBox_KeyDown = delegate { };
        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            TxtBox_KeyDown(sender, e);
        }

        private void _GaugeTxT__GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox obj = (TextBox)sender;
            
            obj.SelectAll();
        }

        private void _GaugeTxT__LostFocus(object sender, RoutedEventArgs e)
        {
            //gaugetxt_focus = false;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //this.vm = (ComViewModel)this.DataContext;
        }

        private void ToggleButton_Loaded(object sender, RoutedEventArgs e)
        {
            ToggleButton obj = (ToggleButton)sender;
            obj.IsChecked = obj.IsChecked;
        }

        private void btn_1_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            //Button obj = (Button)sender;
            //UC_Gauge_Tbn.IsChecked = !UC_Gauge_Tbn.IsChecked;
        }

        public event RoutedEventHandler Btn_update = delegate { };
        private void btn_update_Click(object sender, RoutedEventArgs e)
        {
            MenuItem obj = (MenuItem)sender;
            obj.Tag = Tag;
            Btn_update(sender, e);
            //int index = int.Parse(Tag.ToString().Substring(1));
            //vm.List_D0_value[index][0] = vm.Double_Laser_Wavelength.ToString();
        }

        private void _txt_SN_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //UC_Gauge_Tbn.IsChecked = !UC_Gauge_Tbn.IsChecked;
        }

        public event RoutedEventHandler TxtBox_SN_PreviewKeyDown = delegate { };
        private void _txt_SN_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            TxtBox_SN_PreviewKeyDown(sender, e);
        }
    }
}
