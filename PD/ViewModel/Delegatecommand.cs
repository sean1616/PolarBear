using System;
using System.Windows.Input;
using System.Collections.Generic;

namespace PD.ViewModel
{
    public class Delegatecommand : ICommand
    {
        private readonly Action _action;
        //private readonly Action<KeyEventArgs> _action_generic;

        //Constructor
        public Delegatecommand(Action action)
        {
            _action = action;
        }

        public Delegatecommand(Action executeMethod, Func<bool> canExecuteMethod)
            : this(executeMethod, canExecuteMethod, false)
        {
        }

        public Delegatecommand(Action executeMethod, Func<bool> canExecuteMethod, bool isAutomaticRequeryDisabled)
        {
            if (executeMethod == null)
            {
                throw new ArgumentNullException("executeMethod");
            }

            _executeMethod = executeMethod;
            _canExecuteMethod = canExecuteMethod;
            _isAutomaticRequeryDisabled = isAutomaticRequeryDisabled;
        }

        #region Data

        private readonly Action _executeMethod = null;
        private readonly Func<bool> _canExecuteMethod = null;
        private bool _isAutomaticRequeryDisabled = false;
        //private List<WeakReference> _canExecuteChangedHandlers;

        #endregion

        //執行Action
        public void Execute(object parameter)
        {
            _action();
        }

        //判斷ICommand是否執行
        public bool CanExecute(object parameter)
        {
            return true;
        }

#pragma warning disable 67
        public event EventHandler CanExecuteChanged { add { } remove { } } //what's this ??
#pragma warning restore 67


    }

    public class DelegateCommand<T> : ICommand
    {
        #region Private Properties       
        private Action<T> ExecuteAction { get; set; }
        #endregion Private Properties

        #region Public Events      
        //public event EventHandler CanExecuteChanged;
        public event EventHandler CanExecuteChanged { add { } remove { } }
        #endregion Public Events

        #region Public Constructors               
        public DelegateCommand(Action<T> executeAction)
        {
            ExecuteAction = executeAction;
        }
        #endregion Public Constructors

        #region Public Methods                
        public bool CanExecute
          (
          object parameter
          )
        {
            return true;
        }

        public void Execute
          (
          object parameter
          )
        {
            ExecuteAction((T)Convert.ChangeType(parameter, typeof(T)));
        }
        #endregion Public Methods
    }
}
