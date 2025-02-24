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
    public class WSAConnectByNameController : IDisposable, IHook
    {
        #region Fields

        private LocalHook _hookA;
        private LocalHook _hookB;

        #endregion Fields

        #region Constructors

        public WSAConnectByNameController()
        {
            Name = typeof(WSAConnectByNameController).Name;
            try
            {
                _hookA = LocalHook.Create(LocalHook.GetProcAddress("WS2_32.dll", "WSAConnectByNameA"), new WSAConnectByNameDelegate(WSAConnectByNameDetour), this);
                _hookA.ThreadACL.SetExclusiveACL(new Int32[] { });
                _hookB = LocalHook.Create(LocalHook.GetProcAddress("WS2_32.dll", "WSAConnectByNameW"), new WSAConnectByNameDelegate(WSAConnectByNameDetour), this);
                _hookB.ThreadACL.SetExclusiveACL(new Int32[] { });
            }
            catch (Exception)
            {
                Error = true;
            }
        }

        #endregion Constructors

        #region Delegates

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate bool WSAConnectByNameDelegate(IntPtr s, string nodename, string servicename, ref uint LocalAddressLength,
            [Out] IntPtr LocalAddress, ref uint RemoteAddressLength, [Out] IntPtr RemoteAddress, [In, Optional] IntPtr timeout, IntPtr Reserved = default);

        #endregion Delegates

        #region Properties

        public bool Error { get; set; }

        public string Name { get; set; }

        #endregion Properties

        #region Methods

        [DllImport("WS2_32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool WSAConnectByNameA(IntPtr s, string nodename, string servicename, ref uint LocalAddressLength,
            [Out] IntPtr LocalAddress, ref uint RemoteAddressLength, [Out] IntPtr RemoteAddress, [In, Optional] IntPtr timeout, IntPtr Reserved = default);

        [DllImport("WS2_32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool WSAConnectByNameW(IntPtr s, string nodename, string servicename, ref uint LocalAddressLength,
            [Out] IntPtr LocalAddress, ref uint RemoteAddressLength, [Out] IntPtr RemoteAddress, [In, Optional] IntPtr timeout, IntPtr Reserved = default);

        public void Dispose()
        {
            if (_hookA == null)
                return;

            if (_hookB == null)
                return;

            _hookA.Dispose();
            _hookA = null;

            _hookB.Dispose();
            _hookB = null;
        }

        private bool _showDebugLogs = true;
        private void WriteTrace(string msg)
        {
            if (_showDebugLogs)
            {
                Trace.WriteLine(msg);
            }
        }


        private bool WSAConnectByNameDetour(IntPtr s, string nodename, string servicename, ref uint LocalAddressLength,
            [Out] IntPtr LocalAddress, ref uint RemoteAddressLength, [Out] IntPtr RemoteAddress, [In, Optional] IntPtr timeout, IntPtr Reserved = default)
        {
            HookManagerImpl.Log("WARNING: WSAConnectByNameA/W was called.");
            WriteTrace("WARNING: WSAConnectByNameA/W was called.");
            return false;
            //var result = WSAConnect(s, name, namelen, lpCalleeData, lpCalleeData, lpSQOS, lpGQOS);
            //return result;
        }

        #endregion Methods
    }
}