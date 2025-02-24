/*
 * ---------------------------------------
 * User: duketwo
 * Date: 18.05.2020
 * Time: 00:59
 *
 * ---------------------------------------
 */

using EasyHook;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace HookManager.Win32Hooks
{
    /// <summary>
    ///     Description of WSAConnectController.
    /// </summary>
    public class WSAConnectController : IDisposable, IHook
    {
        #region Fields

        private LocalHook _hook;

        #endregion Fields

        #region Constructors

        public WSAConnectController()
        {
            Name = typeof(WSAConnectController).Name;
            try
            {
                _hook = LocalHook.Create(LocalHook.GetProcAddress("WS2_32.dll", "WSAConnect"), new WSAConnectDelegate(WSAConnectDetour), this);
                _hook.ThreadACL.SetExclusiveACL(new Int32[] { });
            }
            catch (Exception)
            {
                Error = true;
            }
        }

        #endregion Constructors

        #region Delegates

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate int WSAConnectDelegate(IntPtr s, [In] IntPtr name, int namelen, [In, Optional] IntPtr lpCallerData,
            [Out, Optional] IntPtr lpCalleeData, [Optional] IntPtr lpSQOS, [Optional] IntPtr lpGQOS);

        #endregion Delegates

        #region Properties

        public bool Error { get; set; }

        public string Name { get; set; }

        #endregion Properties

        #region Methods

        [DllImport("WS2_32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int WSAConnect(IntPtr s, [In] IntPtr name, int namelen, [In, Optional] IntPtr lpCallerData,
            [Out, Optional] IntPtr lpCalleeData, [Optional] IntPtr lpSQOS, [Optional] IntPtr lpGQOS);

        public void Dispose()
        {
            if (_hook == null)
                return;

            _hook.Dispose();
            _hook = null;
        }

        private bool _showDebugLogs = true;
        private void WriteTrace(string msg)
        {
            if (_showDebugLogs)
            {
                Trace.WriteLine(msg);
            }
        }

        private int WSAConnectDetour(IntPtr s, [In] IntPtr name, int namelen, [In, Optional] IntPtr lpCallerData,
            [Out, Optional] IntPtr lpCalleeData, [Optional] IntPtr lpSQOS, [Optional] IntPtr lpGQOS)
        {
            HookManagerImpl.Log("WARNING: WSAConnect was called.");
            WriteTrace("WARNING: WSAConnect was called.");
            return 10050; // WSAENETDOWN
            //var result = WSAConnect(s, name, namelen, lpCalleeData, lpCalleeData, lpSQOS, lpGQOS);
            //return result;
        }

        #endregion Methods
    }
}