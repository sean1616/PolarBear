using System;
using System.Windows.Input;
using System.Diagnostics;
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

    public class DelegateCommand<T> : ICommand where T : class
    {
        private readonly Func<T, bool> _canExecute;
        private readonly Action<T> _execute;

        bool canExecuteCache;

        //// 建構子(多型)
        //public DelegateCommand(Action<T> execute) 
        //{
        //}
        // 建構子(傳入參數)
        public DelegateCommand(Action<T> execute)
        {
            // 簡化寫法 if(execute == null) throw new ArgumentNullException("execute");
            this._execute = execute ?? throw new ArgumentNullException("execute");
            //this._canExecute = canExecute;
        }

        //public DelegateCommand(Action<double> act, Func<object, bool> canExecute)
        //{
        //    _act = act;
        //    CanExecute1 = canExecute;
        //}

        //public Action<double> _act { get; }
        //public Func<object, bool> CanExecute1 { get; }

        #region -- ICommand Members --
        // 在XAML使用Interaction繫結這個事件
        public event EventHandler CanExecuteChanged;

        // 下面兩個方法是提供給 View 使用的
        [DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            //bool temp = _canExecute((T)parameter);

            //if (canExecuteCache != temp)
            //{
            //    canExecuteCache = temp;
            //    if (CanExecuteChanged != null)
            //    {
            //        CanExecuteChanged(this, new EventArgs());
            //    }
            //}
            //return canExecuteCache;

            return true;
        }

        public void Execute(object parameter)
        {
            _execute((T)parameter);
        }
        #endregion
    }

    public class DelegateCommand_T<TParam> : ICommand where TParam : class
    {
        private readonly Func<TParam, bool> _canExecute;
        private readonly Action<TParam> _execute;

        bool canExecuteCache;

        // 建構子(傳入參數)
        public DelegateCommand_T(Action<TParam> execute)
        {
            // 簡化寫法 if(execute == null) throw new ArgumentNullException("execute");
            this._execute = execute ?? throw new ArgumentNullException("execute");
        }

        #region -- ICommand Members --
        // 在XAML使用Interaction繫結這個事件
        public event EventHandler CanExecuteChanged;

        // 下面兩個方法是提供給 View 使用的
        [DebuggerStepThrough]
        public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter as TParam);

        public void Execute(object parameter) => _execute(parameter as TParam);
        #endregion
    }


    //public class DelegateCommand<T> : ICommand
    //{
    //    #region Private Properties       
    //    private Action<T> ExecuteAction { get; set; }
    //    #endregion Private Properties

    //    #region Public Events      
    //    //public event EventHandler CanExecuteChanged;
    //    public event EventHandler CanExecuteChanged { add { } remove { } }
    //    #endregion Public Events

    //    #region Public Constructors               
    //    public DelegateCommand(Action<T> executeAction)
    //    {
    //        ExecuteAction = executeAction;
    //    }
    //    #endregion Public Constructors

    //    #region Public Methods                
    //    public bool CanExecute
    //      (
    //      object parameter
    //      )
    //    {
    //        return true;
    //    }

    //    public void Execute
    //      (
    //      object parameter
    //      )
    //    {
    //        ExecuteAction((T)Convert.ChangeType(parameter, typeof(T)));
    //    }
    //    #endregion Public Methods
    //}
}
