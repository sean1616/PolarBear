using System;
using System.ComponentModel;

namespace PD.ViewModel
{
    public class NotifyBase : INotifyPropertyChanged
    {
        //泛型寫法, 簡化型別判斷，並判斷新舊值是否相等
        protected virtual bool SetProperty<T>(ref T storage, T value, string propertyName = null)
        {
            if (Object.Equals(storage, value))
                return false;

            storage = value;
            //OnPropertyChanged(propertyName);
            OnPropertyChanged_Normal(propertyName);
            return true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        //OnPropertyChanged 寫成共用副程式,並寫成強型別
        //不指名發生改變的參數，並激發"所有"參數的改變
        protected void OnPropertyChanged(string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        //一般寫法
        //指名已改變的參數並激發
        //public event PropertyChangedEventHandler PropertyChanged_Normal;
        public void OnPropertyChanged_Normal(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
