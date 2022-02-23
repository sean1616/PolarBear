using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PD.Models
{
    public class ScriptModel
    {
        public string ScriptName { get; set; }
        public string ScriptPath { get; set; }
        public int ScriptExcuteIndex { get; set; } = 0;

        //public string ScriptCmdPath { get; set; }
        //public string ScriptVarPath { get; set; }
        //public string ScriptStringPath { get; set; }
        //public string ScriptBoolPath { get; set; }

    }
}
