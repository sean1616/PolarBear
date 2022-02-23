using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PD.Models
{
    public class StringModel
    {
        public StringModel()
        {

        }

        public StringModel(StringModel member)
        {
            
            No = member.No;
            String = member.String;
            Description = member.Description;
        }
                
        public string No { get; set; }
        public string String { get; set; }
        public string Description { get; set; }
    }
}
