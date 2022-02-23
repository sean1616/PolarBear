using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PD.ViewModel;

namespace PD.Models
{
    public class GetPowerSettingModel
    {
        public string TypeName { get; set; }
        public string Interface { get; set; }
        public string Comport { get; set; }        
        public int BaudRate { get; set; }
        public int DelayTime { get; set; }
    }
}
