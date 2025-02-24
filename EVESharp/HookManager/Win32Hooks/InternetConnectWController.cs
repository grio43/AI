/*
 * ---------------------------------------
 * User: duketwo
 * Date: 03.09.2015
 * Time: 11:34
 *
 * ---------------------------------------
 */

using EasyHook;
using System;
using System.Runtime.InteropServices;

namespace HookManager.Win32Hooks
{
    /// <summary>
    ///     Description of InternetConnectWHook.
    /// </summary>
    public class InternetConnectWController : IDisposable, IHook
    {
        #region Fields

        private LocalHook _hook;

        #endregion Fields

        #region Constructors

        public InternetConnectWController()
        {
            Name = "InternetConnectWHook_" + "wininet.dll";

            try
            {
                _hook = LocalHook.Create(LocalHook.GetProcAddress("wininet.dll", "InternetConnectW"), new HInternetConnectWDelegate(HInternetConnectWDetour),
                    this);
                _hook.ThreadACL.SetExclusiveACL(new Int32[] { });
                Error = false;
            }
            catch (Exception e)
            {
                Error = true;
                HookManagerImpl.Log("Exception: " + e);
            }
        }

        #endregion Constructors

        #region Delegates

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        private delegate IntPtr HInternetConnectWDelegate(IntPtr hInternet, string lpszServerName, short nServerPort, string lpszUsername, string lpszPassword,
            int dwService, int dwFlags, IntPtr dwContext);

        #endregion Delegates


        #region Properties

        public bool Error { get; set; }

        public string Name { get; set; }

        #endregion Properties

        #region Methods

        public void Dispose()
        {
            if (_hook == null)
                return;

            _hook.Dispose();
            _hook = null;
        }

        [DllImport("wininet.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr InternetConnectW(IntPtr hInternet, string lpszServerName, short nServerPort, string lpszUsername, string lpszPassword,
            int dwService, int dwFlags, IntPtr dwContext);

        private IntPtr HInternetConnectWDetour(IntPtr hInternet, string lpszServerName, short nServerPort, string lpszUsername, string lpszPassword,
            int dwService, int dwFlags, IntPtr dwContext)
        {
            //var result = InternetConnectW(hInternet, lpszServerName, nServerPort, lpszUsername, lpszPassword, dwService, dwFlags, dwContext);
            try
            {
                var structure = (HINTERNET)Marshal.PtrToStructure(hInternet, typeof(HINTERNET));
                HookManagerImpl.Log("[" + lpszServerName + "][" + nServerPort + "][" + dwService + "]");
            }
            catch (Exception e)
            {
                Error = true;
                HookManagerImpl.Log("Exception: " + e);
            }
            return IntPtr.Zero;
        }

        #endregion Methods

        #region Structs

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct HINTERNET
        {
            public string lpszAgent;
            public int dwAccessType;
            public string lpszProxyName;
            public string lpszProxyBypass;
            public int dwFlags;
        }

        #endregion Structs
    }
}