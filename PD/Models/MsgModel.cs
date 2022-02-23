using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PD.ViewModel;

namespace PD.Models
{
    public class MsgModel : NotifyBase
    {
        private string _msg_1 = "";
        public string msg_1
        {
            get { return _msg_1; }
            set
            {
                _msg_1 = value;
                OnPropertyChanged_Normal("msg_1");
            }
        }

        private string _msg_2 = "";
        public string msg_2
        {
            get { return _msg_2; }
            set
            {
                _msg_2 = value;
                OnPropertyChanged_Normal("msg_2");
            }
        }

        private string _msg_3 = "";
        public string msg_3
        {
            get { return _msg_3; }
            set
            {
                _msg_3 = value;
                OnPropertyChanged_Normal("msg_3");
            }
        }
    }
}
