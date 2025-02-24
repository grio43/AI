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
    ///     Description of ConnectController.
    /// </summary>
    public class WinsockSendToRecvFromController : IDisposable, IHook
    {
        #region Fields

        private LocalHook _hook;
        private LocalHook _hook2;

        #endregion Fields

        #region Constructors

        public WinsockSendToRecvFromController()
        {
            Name = typeof(WinsockSendToRecvFromController).Name;
            try
            {
                _hook = LocalHook.Create(LocalHook.GetProcAddress("WS2_32.dll", "recvfrom"), new RecvFromDelegate(RecvFromDetour), this);
                _hook.ThreadACL.SetExclusiveACL(new Int32[] { });
                _hook2 = LocalHook.Create(LocalHook.GetProcAddress("WS2_32.dll", "sendto"), new SendToDelegate(SendToDetour), this);
                _hook2.ThreadACL.SetExclusiveACL(new Int32[] { });
            }
            catch (Exception)
            {
                Error = true;
            }
        }

        #endregion Constructors

        #region Delegates

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate int RecvFromDelegate(IntPtr s, IntPtr buf, int len, int flags, IntPtr from, ref int fromlen);


        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate int SendToDelegate(IntPtr s, IntPtr buf, int len, int flags, IntPtr to, int tolen);

        #endregion Delegates

        #region Properties

        public bool Error { get; set; }

        public string Name { get; set; }

        #endregion Properties

        #region Methods

        [DllImport("WS2_32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int recvfrom(IntPtr s, IntPtr buf, int len, int flags, IntPtr from, ref int fromlen);


        [DllImport("WS2_32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int sendto(IntPtr s, IntPtr buf, int len, int flags, IntPtr to, int tolen);

        public void Dispose()
        {
            if (_hook == null)
                return;

            if (_hook2 == null)
                return;

            _hook.Dispose();
            _hook2.Dispose();
            _hook = null;
            _hook2 = null;
        }

        private bool _showDebugLogs = true;
        private void WriteTrace(string msg)
        {
            if (_showDebugLogs)
            {
                Trace.WriteLine(msg);
            }
        }

        private int RecvFromDetour(IntPtr s, IntPtr buf, int len, int flags, IntPtr from, ref int fromlen)
        {
            HookManagerImpl.Log("WARNING: RecvFrom was called.");
            WriteTrace("WARNING: RecvFrom was called.");
            return -1; // SOCKET_ERROR 
        }


        private int SendToDetour(IntPtr s, IntPtr buf, int len, int flags, IntPtr to, int tolen)
        {
            HookManagerImpl.Log("WARNING: SendTo was called.");
            WriteTrace("WARNING: SendTo was called.");
            return -1; // SOCKET_ERROR 
        }

        #endregion Methods
    }
}