using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PD.Models
{
    public class ComMember
    {
        public ComMember()
        {

        }

        public ComMember(ComMember member)
        {
            YN = member.YN;
            No = member.No;
            Status = member.Status;
            Type = member.Type;
            Comport = member.Comport;
            Channel = member.Channel;
            Command = member.Command;
            Value_1 = member.Value_1;
            Value_2 = member.Value_2;
            Value_3 = member.Value_3;
            Value_4 = member.Value_4;
            Read = member.Read;
            Description = member.Description;
        }

        public bool YN { get; set; } = true;
        public string No { get; set; }
        public string Status { get; set; }
        public string Type { get; set; }
        public string Comport { get; set; }
        public string Channel { get; set; }
        public string Command { get; set; }
        public string Value_1 { get; set; }
        public string Value_2 { get; set; }
        public string Value_3 { get; set; }
        public string Value_4 { get; set; }        
        public string Read { get; set; }
        public string Description { get; set; }
    }
}
