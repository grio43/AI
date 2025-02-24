using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using SharedComponents.EVE;

namespace EVESharpLauncher
{
    public static class Log
    {
        public static void WriteLine(string text, Color? col = null, [CallerMemberName] string memberName = "")
        {
            Cache.Instance.Log(text, col, memberName);
        }
    }
}
