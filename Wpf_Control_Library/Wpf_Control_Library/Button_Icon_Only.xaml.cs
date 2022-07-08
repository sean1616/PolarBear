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
using System.Windows.Data;
using System.Globalization;

namespace Wpf_Control_Library
{
    /// <summary>
    /// Interaction logic for Button_Icon_Only.xaml
    /// </summary>
    public partial class Button_Icon_Only : UserControl
    {
        public Button_Icon_Only()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty CornerRadius_Property =
                  DependencyProperty.Register("CornerRadius", typeof(int), typeof(Button_Icon_Only),
                  new UIPropertyMetadata(null));

        public int CornerRadius //提供內部binding之相依屬性
        {
            get { return (int)GetValue(CornerRadius_Property); }
            set { SetValue(CornerRadius_Property, value); }
        }       

        public static readonly DependencyProperty ImgSource_Property =
                DependencyProperty.Register("ImgSource", typeof(ImageSource), typeof(Button_Icon_Only),
                new UIPropertyMetadata(null));

        public ImageSource ImgSource //提供內部binding之相依屬性
        {
            get { return (ImageSource)GetValue(ImgSource_Property); }
            set { SetValue(ImgSource_Property, value); }
        }

        public static readonly DependencyProperty BTN_Background_Property =
                  DependencyProperty.Register("BTN_Background", typeof(SolidColorBrush), typeof(Text_TextBox),
                  new UIPropertyMetadata(null));

        public SolidColorBrush BTN_Background //提供內部binding之相依屬性
        {
            get { return (SolidColorBrush)GetValue(BTN_Background_Property); }
            set { SetValue(BTN_Background_Property, value); }
        }

        public static readonly DependencyProperty BTN_Background_MouseOver_Property =
                  DependencyProperty.Register("BTN_Background_MouseOver", typeof(SolidColorBrush), typeof(Text_TextBox),
                  new UIPropertyMetadata(null));

        public SolidColorBrush BTN_Background_MouseOver //提供內部binding之相依屬性
        {
            get { return (SolidColorBrush)GetValue(BTN_Background_MouseOver_Property); }
            set { SetValue(BTN_Background_MouseOver_Property, value); }
        }

        public event RoutedEventHandler Click = delegate { };
        private void BTN_Click(object sender, RoutedEventArgs e)
        {
            Click(sender, e);
        }

    }

    //public class BitmapToString : IValueConverter
    //{
    //    object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        if (value == null) return "";

    //        string str = value.ToString();

    //        return str;
    //    }

    //    object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}
}
