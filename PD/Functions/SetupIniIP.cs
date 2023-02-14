using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using IniParser;
using IniParser.Model;

namespace PD.Functions
{
    public class SetupIniIP
    {
        public string path;

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern long WritePrivateProfileString(string section,
        string key, string val, string filePath);
        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileString(string section,
        string key, string def, StringBuilder retVal,
        int size, string filePath);

        //ini write
        public void IniWriteValue(string Section, string Key, string Value, string inipath)
        {
            WritePrivateProfileString(Section, Key, Value, inipath);
        }

        //ini read
        public string IniReadValue(string Section, string Key, string inipath)
        {
            StringBuilder temp = new StringBuilder(255);
            int i = GetPrivateProfileString(Section, Key, "", temp, 255, inipath);
            return temp.ToString();
        }

        //ini read all sections
        public List<string> IniReadAllSections(string inipath)
        {
            List<string> results = new List<string>();

            var parser = new FileIniDataParser();
            IniData data = parser.ReadFile(inipath);
            foreach (var section in data.Sections)
            {
                Console.WriteLine(section.SectionName);
                results.Add(section.SectionName);
            }
            return results;
        }

        //ini read all key's name in a section
        public List<string> IniReadAllKeyNames(string inipath, string section)
        {
            List<string> results = new List<string>();

            var parser = new FileIniDataParser();
            IniData data = parser.ReadFile(inipath);
            foreach (var s in data.Sections)
            {
                if (s.SectionName.Equals(section))
                {
                    foreach(KeyData kd in s.Keys)
                    {
                        results.Add(kd.KeyName);
                    }
                }
            }
            return results;
        }

        //ini read all keys in a section
        public Dictionary<string, string> IniReadAllKeys(string inipath, string section)
        {
            Dictionary<string, string> results = new Dictionary<string, string>();

            var parser = new FileIniDataParser();
            IniData data = parser.ReadFile(inipath);
            foreach (var s in data.Sections)
            {
                if (s.SectionName.Equals(section))
                {
                    foreach (KeyData kd in s.Keys)
                    {
                        results.Add(kd.KeyName, kd.Value);
                    }
                }
            }
            return results;
        }
    }
}
