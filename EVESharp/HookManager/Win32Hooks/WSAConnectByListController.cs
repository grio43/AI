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
    public class WSAConnectByListController : IDisposable, IHook
    {
        #region Fields

        private LocalHook _hook;

        #endregion Fields

        #region Constructors

        public WSAConnectByListController()
        {
            Name = typeof(WSAConnectByListController).Name;
            try
            {
                _hook = LocalHook.Create(LocalHook.GetProcAddress("WS2_32.dll", "WSAConnectByList"), new WSAConnectByListDelegate(WSAConnectByListDetour), this);
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
        public delegate bool WSAConnectByListDelegate(IntPtr s, in IntPtr SocketAddress, ref uint LocalAddressLength, [Out] IntPtr LocalAddress,
            ref uint RemoteAddressLength, [Out] IntPtr RemoteAddress, [In, Optional] IntPtr timeout, IntPtr Reserved = default);

        #endregion Delegates

        #region Properties

        public bool Error { get; set; }

        public string Name { get; set; }

        #endregion Properties

        #region Methods

        [DllImport("WS2_32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool WSAConnectByList(IntPtr s, in IntPtr SocketAddress, ref uint LocalAddressLength, [Out] IntPtr LocalAddress,
            ref uint RemoteAddressLength, [Out] IntPtr RemoteAddress, [In, Optional] IntPtr timeout, IntPtr Reserved = default);

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

        private bool WSAConnectByListDetour(IntPtr s, in IntPtr SocketAddress, ref uint LocalAddressLength, [Out] IntPtr LocalAddress,
            ref uint RemoteAddressLength, [Out] IntPtr RemoteAddress, [In, Optional] IntPtr timeout, IntPtr Reserved = default)
        {
            HookManagerImpl.Log("WARNING: WSAConnectByList was called.");
            WriteTrace("WARNING: WSAConnectByList was called.");
            return false; 
            //var result = WSAConnect(s, name, namelen, lpCalleeData, lpCalleeData, lpSQOS, lpGQOS);
            //return result;
        }

        #endregion Methods
    }
}