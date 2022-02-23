using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PD.ViewModel;
using OxyPlot;

namespace PD.Models
{
    public class KModel : NotifyBase
    {
        public KModel()
        {
            dataPoints = new List<DataPoint>();
        }

        public string[] preDac { get; set; }
        public List<DataPoint> dataPoints { get; set; }
    }
}
