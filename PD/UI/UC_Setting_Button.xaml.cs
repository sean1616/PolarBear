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
    /// Interaction logic for UC_Setting_Button.xaml
    /// </summary>
    public partial class UC_Setting_Button : UserControl
    {
        public UC_Setting_Button()
        {
            InitializeComponent();
        }

        #region 定義相依屬性               
        public static readonly DependencyProperty txtbox_content_Property =
                    DependencyProperty.Register("txtbox_content", typeof(string), typeof(UC_Setting_Button),
                    new UIPropertyMetadata(null));

        public string txtbox_content //提供內部binding之相依屬性
        {
            get { return (string)GetValue(txtbox_content_Property); }
            set { SetValue(txtbox_content_Property, value); }
        }
        #endregion

        private void userControl_MouseEnter(object sender, MouseEventArgs e)
        {
            border_background.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF98DEB5"));
        }

        private void userControl_MouseLeave(object sender, MouseEventArgs e)
        {
            border_background.Background = Brushes.Transparent;
        }
    }
}
