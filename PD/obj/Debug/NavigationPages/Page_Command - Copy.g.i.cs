﻿#pragma checksum "..\..\..\NavigationPages\Page_Command - Copy.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "58F8837AC564AFA7AF851EC9063F9DB87C2E64FF"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using MahApps.Metro;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using PD.NavigationPages;
using PD.ViewModel;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace PD.NavigationPages {
    
    
    /// <summary>
    /// Page_Command
    /// </summary>
    public partial class Page_Command : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 18 "..\..\..\NavigationPages\Page_Command - Copy.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.DataGrid dataGrid;
        
        #line default
        #line hidden
        
        
        #line 232 "..\..\..\NavigationPages\Page_Command - Copy.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.DataGridComboBoxColumn ComboBox_Command;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/PD-v5.1.1;component/navigationpages/page_command%20-%20copy.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\NavigationPages\Page_Command - Copy.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.dataGrid = ((System.Windows.Controls.DataGrid)(target));
            
            #line 23 "..\..\..\NavigationPages\Page_Command - Copy.xaml"
            this.dataGrid.PreviewKeyDown += new System.Windows.Input.KeyEventHandler(this.dataGrid_PreviewKeyDown);
            
            #line default
            #line hidden
            
            #line 24 "..\..\..\NavigationPages\Page_Command - Copy.xaml"
            this.dataGrid.CopyingRowClipboardContent += new System.EventHandler<System.Windows.Controls.DataGridRowClipboardEventArgs>(this.dataGrid_CopyingRowClipboardContent);
            
            #line default
            #line hidden
            
            #line 25 "..\..\..\NavigationPages\Page_Command - Copy.xaml"
            this.dataGrid.SelectedCellsChanged += new System.Windows.Controls.SelectedCellsChangedEventHandler(this.dataGrid_SelectedCellsChanged);
            
            #line default
            #line hidden
            return;
            case 2:
            this.ComboBox_Command = ((System.Windows.Controls.DataGridComboBoxColumn)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}
