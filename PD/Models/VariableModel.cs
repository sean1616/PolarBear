using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PD.ViewModel;

namespace PD.Models
{
    public class VariableModel : NotifyBase
    {
        public string VariableName { get; set; }
        public int VariableIndex { get; set; }

        private double _VariableContent;
        public double VariableContent
        {
            get { return _VariableContent; }
            set
            {
                _VariableContent = value;
                OnPropertyChanged_Normal("VariableContent");
            }
        }

        private bool _VariableBool = false;
        public bool VariableBool
        {
            get { return _VariableBool; }
            set
            {
                _VariableBool = value;
                OnPropertyChanged_Normal("VariableBool");
            }
        }
    }
}
