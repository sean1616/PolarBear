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
    /// Interaction logic for Sector_BTN.xaml
    /// </summary>
    public partial class Sector_BTN : UserControl
    {
        public Sector_BTN()
        {
            InitializeComponent();
        }

        #region 定義相依屬性         
        public static readonly DependencyProperty img_width_Property =
                    DependencyProperty.Register("img_width", typeof(double), typeof(Sector_BTN),
                    new UIPropertyMetadata(null));

        public static readonly DependencyProperty img_height_Property =
                    DependencyProperty.Register("img_height", typeof(double), typeof(Sector_BTN),
                    new UIPropertyMetadata(null));

        public static readonly DependencyProperty rotate_angle_Property =
                    DependencyProperty.Register("rotate_angle", typeof(string), typeof(Sector_BTN),
                    new UIPropertyMetadata(null));

        public static readonly DependencyProperty grid_row_Property =
                    DependencyProperty.Register("grid_row", typeof(string), typeof(Sector_BTN),
                    new UIPropertyMetadata(null));

        public static readonly DependencyProperty grid_column_Property =
                    DependencyProperty.Register("grid_column", typeof(string), typeof(Sector_BTN),
                    new UIPropertyMetadata(null));

        public static readonly DependencyProperty str_img_source_Property =
                    DependencyProperty.Register("str_img_source", typeof(string), typeof(Sector_BTN),
                    new UIPropertyMetadata(null));

        public static readonly DependencyProperty Arc_EndAngle_Property =
                    DependencyProperty.Register("Arc_EndAngle", typeof(float), typeof(Sector_BTN),
                    new UIPropertyMetadata(null));

        public static readonly DependencyProperty Arc_StartAngle_Property =
                    DependencyProperty.Register("Arc_StartAngle", typeof(float), typeof(Sector_BTN),
                    new UIPropertyMetadata(null));

        public double img_width //提供內部binding之相依屬性
        {
            get { return (double)GetValue(img_width_Property); }
            set { SetValue(img_width_Property, value); }
        }

        public double img_height //提供內部binding之相依屬性
        {
            get { return (double)GetValue(img_height_Property); }
            set { SetValue(img_height_Property, value); }
        }

        public string rotate_angle //提供內部binding之相依屬性
        {
            get { return (string)GetValue(rotate_angle_Property); }
            set { SetValue(rotate_angle_Property, value); }
        }

        public string grid_row //提供內部binding之相依屬性
        {
            get { return (string)GetValue(grid_row_Property); }
            set { SetValue(grid_row_Property, value); }
        }

        public string grid_column //提供內部binding之相依屬性
        {
            get { return (string)GetValue(grid_column_Property); }
            set { SetValue(grid_column_Property, value); }
        }

        public string str_img_source //提供內部binding之相依屬性
        {
            get { return (string)GetValue(str_img_source_Property); }
            set { SetValue(str_img_source_Property, value); }
        }

        public float Arc_StartAngle //提供內部binding之相依屬性
        {
            get { return (float)GetValue(Arc_StartAngle_Property); }
            set { SetValue(Arc_StartAngle_Property, value); }
        }

        public float Arc_EndAngle //提供內部binding之相依屬性
        {
            get { return (float)GetValue(Arc_EndAngle_Property); }
            set { SetValue(Arc_EndAngle_Property, value); }
        }
        #endregion
    }
}
