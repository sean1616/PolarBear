﻿#pragma checksum "..\..\..\UI\UC_Setting_ToggleButton.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "B11C09A4642B2B4384E387E4CC43735EAB647265F24A2E000DEE3FC8A576A71C"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using PD.UI;
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


namespace PD.UI {
    
    
    /// <summary>
    /// UC_Setting_ToggleButton
    /// </summary>
    public partial class UC_Setting_ToggleButton : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 19 "..\..\..\UI\UC_Setting_ToggleButton.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Border border_background;
        
        #line default
        #line hidden
        
        
        #line 24 "..\..\..\UI\UC_Setting_ToggleButton.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock txtblock;
        
        #line default
        #line hidden
        
        
        #line 31 "..\..\..\UI\UC_Setting_ToggleButton.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Documents.Run run_Auto_Connect_TLS;
        
        #line default
        #line hidden
        
        
        #line 33 "..\..\..\UI\UC_Setting_ToggleButton.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Primitives.ToggleButton TBtn_Auto_Connect_TLS;
        
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
            System.Uri resourceLocater = new System.Uri("/PD-v5.5.14;component/ui/uc_setting_togglebutton.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\UI\UC_Setting_ToggleButton.xaml"
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
            
            #line 7 "..\..\..\UI\UC_Setting_ToggleButton.xaml"
            ((PD.UI.UC_Setting_ToggleButton)(target)).MouseEnter += new System.Windows.Input.MouseEventHandler(this.userControl_MouseEnter);
            
            #line default
            #line hidden
            
            #line 8 "..\..\..\UI\UC_Setting_ToggleButton.xaml"
            ((PD.UI.UC_Setting_ToggleButton)(target)).MouseLeave += new System.Windows.Input.MouseEventHandler(this.userControl_MouseLeave);
            
            #line default
            #line hidden
            return;
            case 2:
            this.border_background = ((System.Windows.Controls.Border)(target));
            return;
            case 3:
            this.txtblock = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 4:
            this.run_Auto_Connect_TLS = ((System.Windows.Documents.Run)(target));
            return;
            case 5:
            this.TBtn_Auto_Connect_TLS = ((System.Windows.Controls.Primitives.ToggleButton)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}

