using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    internal class usersettings : List<string>
    {
        public bool AutoUpdatePref { get; set; } = false;
        public int CurrentTheme { get; set; } = 0;

        public int easyMode { get; set; } = 1;
    }
}