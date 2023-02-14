using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PD.ViewModel;
using PD.Utility;

namespace PD.Models
{
    public class SettingUnitModel : NotifyBase
    {
        private int _No;
        public int No
        {
            get { return _No; }
            set { _No = value; }
        }

        private string _Name = "";
        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }

        private string _Value;
        public string Value
        {
            get { return _Value; }
            set { _Value = value; }
        }

        private string _Tag;
        public string Tag
        {
            get { return _Tag; }
            set { _Tag = value; }
        }
    }
}
