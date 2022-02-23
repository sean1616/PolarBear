using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PD.Models
{
    public class LogMember
    {
        public string Status { get; set; }
        public string Channel { get; set; }
        public string Message { get; set; }
        public string Result { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }

        public bool isShowMSG { get; set; } = false;
        public string TimeSpan { get; set; }
    }
}
