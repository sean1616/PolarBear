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
using PD.Models;

namespace PD.NavigationPages
{
    /// <summary>
    /// Interaction logic for Page_Command.xaml
    /// </summary>
    public partial class Page_StringList : UserControl
    {
        ComViewModel vm;
        public StringModel member_copy = new StringModel();

        public Page_StringList(ComViewModel vm)
        {
            InitializeComponent();

            this.vm = vm;

            dataGrid.DataContext = vm.list_StringModels;      
        }

        private void dataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            vm.CmdSelected_Index = 0;
        }

        private void dataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            vm.CmdSelected_Index = dataGrid.SelectedIndex;
        }

        private void dataGrid_CopyingRowClipboardContent(object sender, DataGridRowClipboardEventArgs e)
        {
            if (vm.CmdSelected_Index >= 0)
            {
                try
                {
                    member_copy = new StringModel((StringModel)dataGrid.SelectedItem);
                }
                catch { }
            }
        }

        private void dataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            
        }

        private void dataGrid_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (vm.CmdSelected_Index >= 0)
            {
                //Ctrl + V
                if (e.Key == Key.V)
                    if (System.Windows.Forms.Control.ModifierKeys.HasFlag(System.Windows.Forms.Keys.Control))  //Check ctrl btn is pressed
                    {
                        bool _EditMode = false;
                        //Check datagrid is in editmode or not
                        try
                        {
                            dataGrid.Items.Refresh();
                        }
                        catch { _EditMode = true; }

                        if (!_EditMode)
                        {
                            vm.list_StringModels.Insert(vm.CmdSelected_Index, new StringModel(member_copy));

                            //Update member's No
                            int count = vm.list_StringModels.Count;
                            for (int i = 0; i < count; i++)
                            {
                                vm.list_StringModels[i].No = i.ToString();
                            }
                            try
                            {
                                dataGrid.Items.Refresh();
                            }
                            catch { }
                        }
                    }

                //Auto-scrolling the view to end
                if (dataGrid.Items.Count > 0)
                {
                    var border = VisualTreeHelper.GetChild(dataGrid, 0) as Decorator;
                    if (border != null)
                    {
                        var scroll = border.Child as ScrollViewer;
                        if (scroll != null) scroll.ScrollToEnd();
                    }
                }

                dataGrid.Focus();
            }
        }



        #region Set Datagrid to selected less mode
        ////事件處理函式
        //private void DataGridCell_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    PreviewMouseLeftButtonDownHandle((DataGridCell)sender, this.dataGrid);
        //}

        ////事件處理函式
        //private void PreviewMouseLeftButtonDownHandle(DataGridCell cell, DataGrid datagrid)
        //{

        //    if (cell != null && !cell.IsEditing && !cell.IsReadOnly)
        //    {
        //        if (!cell.IsFocused)
        //        {
        //            cell.Focus();
        //        }

        //        if (datagrid.SelectionUnit != DataGridSelectionUnit.FullRow)
        //        {
        //            if (!cell.IsSelected)
        //            {
        //                cell.IsSelected = true;
        //            }
        //        }
        //        else
        //        {
        //            DataGridRow row = FindVisualParent<DataGridRow>(cell);
        //            if (row != null && !row.IsSelected)
        //            {
        //                row.IsSelected = true;
        //            }
        //        }
        //    }
        //}

        ////事件處理函式
        //public static T FindVisualParent<T>(UIElement element) where T : UIElement
        //{
        //    UIElement parent = element;
        //    while (parent != null)
        //    {
        //        T correctlyTyped = parent as T;
        //        if (correctlyTyped != null)
        //        {
        //            return correctlyTyped;
        //        }
        //        parent = VisualTreeHelper.GetParent(parent) as UIElement;
        //    }
        //    return null;
        //}
        #endregion

        
    }
}
