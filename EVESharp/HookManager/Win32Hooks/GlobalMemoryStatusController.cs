/*
 * ---------------------------------------
 * User: duketwo
 * Date: 21.06.2014
 * Time: 16:48
 *
 * ---------------------------------------
 */

using EasyHook;
using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace HookManager.Win32Hooks
{
    /// <summary>
    ///     Description of GlobalMemoryStatusController.
    /// </summary>
    public class GlobalMemoryStatusController : IDisposable, IHook
    {
        #region Fields

        private LocalHook _hook;

        private string _name;
        private MEMORYSTATUSEX _struct;
        private readonly ulong _totalPhys;

        #endregion Fields

        #region Constructors

        public GlobalMemoryStatusController(IntPtr address, ulong totalPhys)
        {
            _totalPhys = totalPhys;
            Name = typeof(GlobalMemoryStatusController).Name;

            try
            {
                _name = string.Format("MemoryHook{0:X}", address.ToInt64());
                _hook = LocalHook.Create(address, new GlobalMemoryStatusDelegate(GlobalMemoryStatusDetour), this);
                _hook.ThreadACL.SetExclusiveACL(new Int32[] { });

                if (_struct == null)
                {
                    _struct = new MEMORYSTATUSEX();
                    var result = GlobalMemoryStatusEx(_struct);
                    var before = _struct.ullTotalPhys / 1024 / 1024;
                    _struct.ullTotalPhys = _totalPhys * 1024 * 1024;
                    var after = _struct.ullTotalPhys / 1024 / 1024;
                    //HookManagerImpl.Log("[BEFORE] " + before.ToString() + " [AFTER] " + after.ToString(), Color.Orange);
                }
            }
            catch (Exception)
            {
                Error = true;
            }
        }

        #endregion Constructors

        #region Delegates

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate bool GlobalMemoryStatusDelegate(IntPtr memStruct);

        #endregion Delegates

        #region Properties

        public bool Error { get; set; }

        public string Name { get; set; }

        #endregion Properties

        #region Methods

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GlobalMemoryStatusEx(IntPtr memStruct);

        [DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool GlobalMemoryStatusEx(MEMORYSTATUSEX lpBuffer);

        public void Dispose()
        {
            if (_hook == null)
                return;

            _hook.Dispose();
            _hook = null;
        }

        private bool GlobalMemoryStatusDetour(IntPtr memStruct)
        {
            Marshal.StructureToPtr(_struct, memStruct, true);
            return true;
        }

        #endregion Methods

        #region Classes

        [StructLayout(LayoutKind.Sequential)]
        public class MEMORYSTATUSEX
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;

            public MEMORYSTATUSEX()
            {
                dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
            }
        }

        #endregion Classes
    }
}