#pragma checksum "..\..\..\NavigationPages\Page_Setting.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "FA36A39683A58A12A40F330D8B70DB29FE69D76E"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using MahApps.Metro.Controls;
using PD.NavigationPages;
using PD.UI;
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
using WpfAnimatedGif;
using WpfControlLibrary1;


namespace PD.NavigationPages {
    
    
    /// <summary>
    /// Page_Setting
    /// </summary>
    public partial class Page_Setting : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 63 "..\..\..\NavigationPages\Page_Setting.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox ComBox_TLS_WL_Range;
        
        #line default
        #line hidden
        
        
        #line 70 "..\..\..\NavigationPages\Page_Setting.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox ComBox_Station_Type;
        
        #line default
        #line hidden
        
        
        #line 77 "..\..\..\NavigationPages\Page_Setting.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox ComBox_Control_Board_Type;
        
        #line default
        #line hidden
        
        
        #line 101 "..\..\..\NavigationPages\Page_Setting.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox ComBox_K_WL_Type;
        
        #line default
        #line hidden
        
        
        #line 110 "..\..\..\NavigationPages\Page_Setting.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox ComBox_Laser_Selection;
        
        #line default
        #line hidden
        
        
        #line 223 "..\..\..\NavigationPages\Page_Setting.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal PD.UI.UC_Setting_Button btn_board_comport;
        
        #line default
        #line hidden
        
        
        #line 227 "..\..\..\NavigationPages\Page_Setting.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal PD.UI.UC_Setting_Button btn_ini;
        
        #line default
        #line hidden
        
        
        #line 236 "..\..\..\NavigationPages\Page_Setting.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox txt_path;
        
        #line default
        #line hidden
        
        
        #line 247 "..\..\..\NavigationPages\Page_Setting.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox txt_save_wl_data_path;
        
        #line default
        #line hidden
        
        
        #line 264 "..\..\..\NavigationPages\Page_Setting.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Image Img_gif;
        
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
            System.Uri resourceLocater = new System.Uri("/PD-v4.1.9;component/navigationpages/page_setting.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\NavigationPages\Page_Setting.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal System.Delegate _CreateDelegate(System.Type delegateType, string handler) {
            return System.Delegate.CreateDelegate(delegateType, this, handler);
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
            this.ComBox_TLS_WL_Range = ((System.Windows.Controls.ComboBox)(target));
            return;
            case 2:
            this.ComBox_Station_Type = ((System.Windows.Controls.ComboBox)(target));
            return;
            case 3:
            this.ComBox_Control_Board_Type = ((System.Windows.Controls.ComboBox)(target));
            
            #line 80 "..\..\..\NavigationPages\Page_Setting.xaml"
            this.ComBox_Control_Board_Type.DropDownClosed += new System.EventHandler(this.ComBox_Control_Board_Type_DropDownClosed);
            
            #line default
            #line hidden
            
            #line 81 "..\..\..\NavigationPages\Page_Setting.xaml"
            this.ComBox_Control_Board_Type.DropDownOpened += new System.EventHandler(this.ComBox_Control_Board_Type_DropDownOpened);
            
            #line default
            #line hidden
            return;
            case 4:
            this.ComBox_K_WL_Type = ((System.Windows.Controls.ComboBox)(target));
            
            #line 104 "..\..\..\NavigationPages\Page_Setting.xaml"
            this.ComBox_K_WL_Type.DropDownClosed += new System.EventHandler(this.ComBox_K_WL_Type_DropDownClosed);
            
            #line default
            #line hidden
            return;
            case 5:
            this.ComBox_Laser_Selection = ((System.Windows.Controls.ComboBox)(target));
            
            #line 113 "..\..\..\NavigationPages\Page_Setting.xaml"
            this.ComBox_Laser_Selection.DropDownClosed += new System.EventHandler(this.ComBox_Laser_Selection_DropDownClosed);
            
            #line default
            #line hidden
            
            #line 114 "..\..\..\NavigationPages\Page_Setting.xaml"
            this.ComBox_Laser_Selection.DropDownOpened += new System.EventHandler(this.ComBox_Laser_Selection_DropDownOpened);
            
            #line default
            #line hidden
            return;
            case 6:
            this.btn_board_comport = ((PD.UI.UC_Setting_Button)(target));
            
            #line 224 "..\..\..\NavigationPages\Page_Setting.xaml"
            this.btn_board_comport.AddHandler(System.Windows.Controls.Primitives.ButtonBase.ClickEvent, new System.Windows.RoutedEventHandler(this.btn_board_comport_Click));
            
            #line default
            #line hidden
            return;
            case 7:
            this.btn_ini = ((PD.UI.UC_Setting_Button)(target));
            
            #line 228 "..\..\..\NavigationPages\Page_Setting.xaml"
            this.btn_ini.AddHandler(System.Windows.Controls.Primitives.ButtonBase.ClickEvent, new System.Windows.RoutedEventHandler(this.btn_ini_Click));
            
            #line default
            #line hidden
            return;
            case 8:
            this.txt_path = ((System.Windows.Controls.TextBox)(target));
            return;
            case 9:
            this.txt_save_wl_data_path = ((System.Windows.Controls.TextBox)(target));
            return;
            case 10:
            
            #line 261 "..\..\..\NavigationPages\Page_Setting.xaml"
            ((PD.UI.UC_Setting_ToggleButton)(target)).AddHandler(System.Windows.Controls.Primitives.ToggleButton.CheckedEvent, new System.Windows.RoutedEventHandler(this.UC_Setting_ToggleButton_Checked));
            
            #line default
            #line hidden
            return;
            case 11:
            this.Img_gif = ((System.Windows.Controls.Image)(target));
            
            #line 270 "..\..\..\NavigationPages\Page_Setting.xaml"
            this.Img_gif.MouseEnter += new System.Windows.Input.MouseEventHandler(this.Img_gif_MouseEnter);
            
            #line default
            #line hidden
            
            #line 271 "..\..\..\NavigationPages\Page_Setting.xaml"
            this.Img_gif.MouseLeave += new System.Windows.Input.MouseEventHandler(this.Img_gif_MouseLeave);
            
            #line default
            #line hidden
            
            #line 272 "..\..\..\NavigationPages\Page_Setting.xaml"
            this.Img_gif.Loaded += new System.Windows.RoutedEventHandler(this.Img_gif_Loaded);
            
            #line default
            #line hidden
            
            #line 274 "..\..\..\NavigationPages\Page_Setting.xaml"
            this.Img_gif.PreviewMouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(this.Img_gif_MouseLeftButtonDown);
            
            #line default
            #line hidden
            
            #line 275 "..\..\..\NavigationPages\Page_Setting.xaml"
            this.Img_gif.PreviewMouseRightButtonDown += new System.Windows.Input.MouseButtonEventHandler(this.Img_gif_MouseRightButtonDown);
            
            #line default
            #line hidden
            return;
            case 12:
            
            #line 281 "..\..\..\NavigationPages\Page_Setting.xaml"
            ((System.Windows.Controls.Image)(target)).Loaded += new System.Windows.RoutedEventHandler(this.Image_Loaded);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

