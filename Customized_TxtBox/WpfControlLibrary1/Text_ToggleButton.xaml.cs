﻿using System;
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

namespace Customized_TxtBox
{
    /// <summary>
    /// Interaction logic for Text_ToggleButton.xaml
    /// </summary>
    public partial class Text_ToggleButton : UserControl
    {
        public Text_ToggleButton()
        {
            InitializeComponent();
        }

        #region 定義相依屬性               
        public static readonly DependencyProperty txtbox_content_Property =
                    DependencyProperty.Register("txtbox_content", typeof(string), typeof(Text_ToggleButton),
                    new UIPropertyMetadata(null));

        public static readonly DependencyProperty Ischecked_Property =
                   DependencyProperty.Register("Ischecked", typeof(bool), typeof(Text_ToggleButton),
                   new UIPropertyMetadata(null));

        public string txtbox_content //提供內部binding之相依屬性
        {
            get { return (string)GetValue(txtbox_content_Property); }
            set { SetValue(txtbox_content_Property, value); }
        }

        public bool Ischecked //提供內部binding之相依屬性
        {
            get { return (bool)GetValue(Ischecked_Property); }
            set { SetValue(Ischecked_Property, value); }
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
