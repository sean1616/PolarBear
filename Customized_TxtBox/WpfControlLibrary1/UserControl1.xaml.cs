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

namespace WpfControlLibrary1
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class UserControl1 : UserControl
    {
        public UserControl1()
        {
            InitializeComponent();
        }

        #region 定義相依屬性               
        public static readonly DependencyProperty txtbox_content_Property =
                    DependencyProperty.Register("txtbox_content", typeof(string), typeof(UserControl1),
                    new UIPropertyMetadata(null));

        public static readonly DependencyProperty txtbox_value_Property =
                    DependencyProperty.Register("txtbox_value", typeof(double), typeof(UserControl1),
                    new UIPropertyMetadata(null));


        public string txtbox_content //提供內部binding之相依屬性
        {
            get { return (string)GetValue(txtbox_content_Property); }
            set { SetValue(txtbox_content_Property, value); }
        }

        public double txtbox_value //提供內部binding之相依屬性
        {
            get { return (double)GetValue(txtbox_value_Property); }
            set { SetValue(txtbox_value_Property, value); }
        }
        #endregion

        private void userControl_MouseEnter(object sender, MouseEventArgs e)
        {
            border_background.Background= (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF98DEB5"));
        }

        private void userControl_MouseLeave(object sender, MouseEventArgs e)
        {
            border_background.Background = Brushes.Transparent;
        }

        private void textBox_KeyDown(object sender, KeyEventArgs e)
        {
            TextBox obj = (TextBox)sender;
            if (e.Key == Key.Enter)
            {
                txtbox_value = Convert.ToDouble(obj.Text);
            }
        }

        private void textBox_GotFocus(object sender, RoutedEventArgs e)
        {
            border_background.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF98DEB5"));
        }

        private void textBox_LostFocus(object sender, RoutedEventArgs e)
        {
            border_background.Background = Brushes.Transparent;
        }
    }
}
