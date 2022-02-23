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

namespace PD.NavigationPages
{
    /// <summary>
    /// Page_Variable.xaml 的互動邏輯
    /// </summary>
    public partial class Page_Variable : UserControl
    {
        ComViewModel vm;

        public Page_Variable(ComViewModel vm)
        {
            InitializeComponent();

            this.vm = vm;

            this.DataContext = vm;

            for (int i = 0; i < 100; i++)
            {
                vm.list_VariableModels.Add(new Models.VariableModel() { VariableName = i.ToString() });
            }

            for (int i = 0; i < 100; i++)
            {
                vm.list_VarBoolModels.Add(new Models.VariableModel() { VariableName = i.ToString() });
            }
        }
    }
}
