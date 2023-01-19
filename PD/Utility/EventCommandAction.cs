using System.Windows;
using System;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Reflection;

namespace PD.Utility
{
    public class EventCommandAction : TriggerAction<DependencyObject>
    {
        // 用 Command 相依屬性來儲存要綁定的命令
        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        // 要註冊之相依性屬性的名稱 (String)
        // 屬性的類型 (Type)
        // 正在註冊相依性屬性的擁有者類型 (Type)
        // 相依性屬性的屬性中繼資料 (PropertyMetadata)
        public static readonly
            DependencyProperty CommandProperty =
            DependencyProperty.Register(nameof(Command),
                                        typeof(ICommand),
                                        typeof(EventCommandAction),
                                        null);

        // 改寫Invoke()，讓傳入的parameter傳遞給Command.CanExecute()與Command.Execute()
        protected override void Invoke(object parameter)
        {
            if (Command != null && Command.CanExecute(parameter))
                Command.Execute(parameter);
        }
    }

    public class EventCommand : TriggerAction<DependencyObject>
    {
        private string commandName;

        public ICommand Command
        {
            get
            {
                return (ICommand)this.GetValue(CommandProperty);
            }
            set
            {
                this.SetValue(CommandProperty, value);
            }
        }
        public string CommandName
        {
            get
            {
                return this.commandName;
            }
            set
            {
                if (this.CommandName != value)
                {
                    this.commandName = value;
                }
            }
        }
        public object CommandParameter
        {
            get
            {
                return this.GetValue(CommandParameterProperty);
            }

            set
            {
                this.SetValue(CommandParameterProperty, value);
            }
        }
        public object InvokeParameter
        {
            get
            {
                return this.GetValue(InvokeParameterProperty);
            }
            set
            {
                this.SetValue(InvokeParameterProperty, value);
            }
        }

        public object Sender { get; set; }

        public static readonly
            DependencyProperty CommandProperty =
            DependencyProperty.Register(nameof(Command),
                                        typeof(ICommand),
                                        typeof(EventCommand),
                                        null);
        public static readonly
            DependencyProperty CommandParameterProperty =
            DependencyProperty.Register(nameof(CommandParameter),
                                        typeof(object),
                                        typeof(EventCommand),
                                        null);
        public static readonly
            DependencyProperty InvokeParameterProperty =
            DependencyProperty.Register(nameof(InvokeParameter),
                                        typeof(object),
                                        typeof(EventCommand),
                                        null);

        protected override void Invoke(object parameter)
        {
            this.InvokeParameter = parameter;
            if (this.AssociatedObject != null)
            {
                ICommand command = this.ResolveCommand();
                if ((command != null) && command.CanExecute(this.CommandParameter))
                {
                    command.Execute(this.CommandParameter);
                }
            }
        }

        private ICommand ResolveCommand()
        {
            ICommand command = null;
            if (this.Command != null)
            {
                return this.Command;
            }
            var frameworkElement = this.AssociatedObject as FrameworkElement;
            if (frameworkElement != null)
            {
                object dataContext = frameworkElement.DataContext;
                if (dataContext != null)
                {
                    //PropertyInfo commandPropertyInfo = dataContext
                    //    .GetType()
                    //    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    //    .FirstOrDefault(
                    //        p =>
                    //        typeof(ICommand).IsAssignableFrom(p.PropertyType) &&
                    //        string.Equals(p.Name, this.CommandName, StringComparison.Ordinal)
                    //    );

                    PropertyInfo[] commandPropertyInfo = dataContext
                        .GetType()
                        .GetProperties(BindingFlags.Public | BindingFlags.Instance);

                    PropertyInfo pti = commandPropertyInfo[0];
                    foreach (PropertyInfo p in commandPropertyInfo)
                    {
                        if (typeof(ICommand).IsAssignableFrom(p.PropertyType) && string.Equals(p.Name, this.CommandName, StringComparison.Ordinal))
                        {
                            pti = p;
                        }
                    }

                    if (pti != null)
                    {
                        command = (ICommand)pti.GetValue(dataContext, null);
                    }
                }
            }
            return command;
        }
    }
}
