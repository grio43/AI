/*
* ---------------------------------------
* User: duketwo
* Date: 12.12.2013
* Time: 12:51
*
* ---------------------------------------
*/

using EasyHook;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace HookManager.Win32Hooks
{
    /// <summary>
    ///     Description of EnumProcessesControllerK32.
    /// </summary>
    public class EnumProcessesControllerK32 : IHook, IDisposable
    {
        #region Fields

        private LocalHook _hook;
        private int currentEvePID = -1;

        #endregion Fields

        #region Constructors

        public EnumProcessesControllerK32()
        {
            Error = false;
            Name = typeof(EnumProcessesControllerPSAPI).Name;
            try
            {
                currentEvePID = Process.GetCurrentProcess().Id;
                _hook = LocalHook.Create(
                    LocalHook.GetProcAddress("kernel32.dll", "K32EnumProcesses"),
                    new EnumProcessesDelegate(EnumProcessesControllerK32Detour),
                    this);

                _hook.ThreadACL.SetExclusiveACL(new Int32[] { });
                Error = false;
            }
            catch (Exception)
            {
                Error = true;
            }
        }

        #endregion Constructors

        #region Delegates

        private delegate bool EnumProcessesDelegate([In] [Out] IntPtr processIds, uint arrayBytesSize, [In] [Out] IntPtr bytesCopied);

        #endregion Delegates

        #region Properties

        public bool Error { get; set; }
        public string Name { get; set; }

        #endregion Properties

        #region Methods

        public void Dispose()
        {
            _hook.Dispose();
        }

        [DllImport("psapi.dll")]
        private static extern bool EnumProcesses(
            [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U4)][In][Out] UInt32[] processIds,
            UInt32 arraySizeBytes,
            [MarshalAs(UnmanagedType.U4)] out UInt32 bytesCopied);

        private bool EnumProcessesControllerK32Detour([In] [Out] IntPtr processIds, uint arrayBytesSize, [In] [Out] IntPtr bytesCopied)
        {
            if (processIds == IntPtr.Zero || bytesCopied == IntPtr.Zero)
            {
                return false;
            }

            Marshal.WriteInt32(processIds, 0, this.currentEvePID);
            Marshal.WriteInt32(bytesCopied, (1 * sizeof(uint)));
            return true;
        }

        #endregion Methods
    }
}