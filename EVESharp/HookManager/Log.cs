using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using HookManager.Win32Hooks;
using SharedComponents.IPC;

namespace HookManager
{
    public static class Log
    {
        public static void RemoteWriteLine(string s)
        {
            WCFClient.Instance.GetPipeProxy.RemoteLog(s);
        }

        public static void RemoteConsoleWriteLine(string s, bool isErr)
        {
            WCFClient.Instance.GetPipeProxy.RemoteConsoleLog(HookManagerImpl.Instance.EveAccount.CharacterName, s, isErr);
        }

        public static void WriteLine(string text, Color? col = null, [CallerMemberName] string memberName = "")
        {
            HookManagerImpl.Log(text, col, memberName);
        }
    }
}
