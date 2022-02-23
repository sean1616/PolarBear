using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PD.Models
{
    public class CommandList
    {
        public static string ID { get; set; } = "ID?";
        public static string P0 { get; set; } = "P0?";
        public static string PD { get; set; } = "PD?";

        public static string SN { get; set; } = "SN?";
        public static string CALL { get; set; } = "CALL";
        public static string DAC { get; set; } = "DAC?";
        public static string Delay { get; set; } = "Delay";

        public static string Loop { get; set; } = "LOOP";
        public static string LoopE { get; set; } = "LOOPE";
        public static string Pause { get; set; } = "PAUSE";
        public static string FLAG { get; set; } = "FLAG";
        public static string JUMP { get; set; } = "JUMP";
        public static string CLRDP { get; set; } = "CLRDP";
        public static string IFON { get; set; } = "IFON";
        public static string IFOFF { get; set; } = "IFOFF";
        public static string BSET { get; set; } = "BSET";
        public static string BRST { get; set; } = "BRST";

        public static string Write { get; set; } = "WRITE";
        public static string WHILE { get; set; } = "WHILE";
        public static string WHILEND { get; set; } = "WHILEND";
        public static string WriteDac { get; set; } = "WRITEDAC";
        public static string WriteVolt { get; set; } = "WRITEVOLT";

        public static string GetPower { get; set; } = "GETPOWER";
        public static string SavePower { get; set; } = "SAVEPOWER";
        public static string SETPOWER { get; set; } = "SETPOWER";
        public static string SETWL { get; set; } = "SETWL";
        public static string SETSWITCH { get; set; } = "SETSWITCH";

        public static string STRPATH { get; set; } = "STRPATH";
        public static string SAVECHART { get; set; } = "SAVECHART";
        public static string SETVAR { get; set; } = "SETVAR";
        public static string SETBOOL { get; set; } = "SETBOOL";

        public static string GetBoardTable { get; set; } = "GET_BOARDTABLE";
        public static string MessageBox { get; set; } = "MESSAGEBOX";
        public static string MSG { get; set; } = "MSG";
        public static string MSGOFF { get; set; } = "MSGOFF";

        public static string MaxPower { get; set; } = "MAXPOWER";
        public static string MaxPowDAC { get; set; } = "MAXPOWDAC";

        public static string Add { get; set; } = "Add";
        public static string SUB { get; set; } = "SUB";
        public static string MULT { get; set; } = "MULT";
        public static string DIV { get; set; } = "DIV";
        public static string CMPGT { get; set; } = "CMPGT";
        public static string CMPGE { get; set; } = "CMPGE";
        public static string CMPLE { get; set; } = "CMPLE";
        public static string CMPLT { get; set; } = "CMPLT";

        public static string UVSETPOW { get; set; } = "UVSETPOW";
        public static string UVSETTIMER { get; set; } = "UVSETTIMER";
        public static string UVSTART { get; set; } = "UVSTART";
        public static string UVSTOP { get; set; } = "UVSTOP";

        public static string End { get; set; } = "End";

        public static Dictionary<string, int> Dictionary_Flag = new Dictionary<string, int>();


        //public static List<string> commandList { get; set; } = new List<string>()
        //{ "CALL", "Delay", "Write", "WriteDac", "LOOP", "LOOPE", "GETPOWER", "MESSAGEBOX", "MAXPOWER", "STRPATH", "SaveChart", "ID?", "P0?",
        //            "DAC?"};
    }
}
