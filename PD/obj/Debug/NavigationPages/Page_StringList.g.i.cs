﻿#pragma checksum "..\..\..\NavigationPages\Page_StringList.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "9A4789D3BDB0843E1858ECAD084A006D6B14082948BDDC4CA4F15884333DCCF2"
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
using MahApps.Metro.Accessibility;
using MahApps.Metro.Actions;
using MahApps.Metro.Automation.Peers;
using MahApps.Metro.Behaviors;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using MahApps.Metro.Converters;
using MahApps.Metro.Markup;
using MahApps.Metro.Theming;
using MahApps.Metro.ValueBoxes;
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
    /// Page_StringList
    /// </summary>
    public partial class Page_StringList : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 9 "..\..\..\NavigationPages\Page_StringList.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal PD.NavigationPages.Page_StringList page_stringList;
        
        #line default
        #line hidden
        
        
        #line 19 "..\..\..\NavigationPages\Page_StringList.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.DataGrid dataGrid;
        
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
            System.Uri resourceLocater = new System.Uri("/PD-v5.5.15;component/navigationpages/page_stringlist.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\NavigationPages\Page_StringList.xaml"
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
            this.page_stringList = ((PD.NavigationPages.Page_StringList)(target));
            return;
            case 2:
            this.dataGrid = ((System.Windows.Controls.DataGrid)(target));
            
            #line 25 "..\..\..\NavigationPages\Page_StringList.xaml"
            this.dataGrid.PreviewKeyDown += new System.Windows.Input.KeyEventHandler(this.dataGrid_PreviewKeyDown);
            
            #line default
            #line hidden
            
            #line 26 "..\..\..\NavigationPages\Page_StringList.xaml"
            this.dataGrid.PreviewKeyUp += new System.Windows.Input.KeyEventHandler(this.dataGrid_PreviewKeyUp);
            
            #line default
            #line hidden
            
            #line 27 "..\..\..\NavigationPages\Page_StringList.xaml"
            this.dataGrid.CopyingRowClipboardContent += new System.EventHandler<System.Windows.Controls.DataGridRowClipboardEventArgs>(this.dataGrid_CopyingRowClipboardContent);
            
            #line default
            #line hidden
            
            #line 28 "..\..\..\NavigationPages\Page_StringList.xaml"
            this.dataGrid.SelectedCellsChanged += new System.Windows.Controls.SelectedCellsChangedEventHandler(this.dataGrid_SelectedCellsChanged);
            
            #line default
            #line hidden
            
            #line 29 "..\..\..\NavigationPages\Page_StringList.xaml"
            this.dataGrid.Loaded += new System.Windows.RoutedEventHandler(this.dataGrid_Loaded);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

