using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PD.Models
{
    public class CmdMsgModel
    {
        public bool isJump { get; set; } = false;
        public int JumpIndex { get; set; } = 0;
        public bool isLoop { get; set; } = true;
    }
}
