using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PD.Models
{
    public class Gauge
    {
        public string Channel { get; set; }

        public Gauge(string channel)
        {
            Channel = channel;
        }
    }
}
