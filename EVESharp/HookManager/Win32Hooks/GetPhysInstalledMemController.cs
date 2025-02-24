/*
 * ---------------------------------------
 * User: duketwo
 * Date: 16.11.2016
 * Time: 07:47
 *
 * ---------------------------------------
 */

using EasyHook;
using System;
using System.Runtime.InteropServices;

namespace HookManager.Win32Hooks
{
    /// <summary>
    ///     Description of GetPhysInstalledMemController.
    /// </summary>
    public class GetPhysInstalledMemController : IDisposable, IHook
    {
        #region Fields

        private LocalHook _hook;

        private readonly string _name;
        private readonly ulong _totalPhys;

        #endregion Fields

        #region Constructors

        public GetPhysInstalledMemController(ulong totalPhys)
        {
            Name = typeof(GetPhysInstalledMemController).Name;
            try
            {
                _totalPhys = totalPhys;
                _name = string.Format("GetPhysInstalledMemController{0:X}", LocalHook.GetProcAddress("kernel32.dll", "GetPhysicallyInstalledSystemMemory"));
                _hook = LocalHook.Create(LocalHook.GetProcAddress("kernel32.dll", "GetPhysicallyInstalledSystemMemory"), new GetPhysicallyInstalledSystemMemoryDelegate(GetPhysicallyInstalledSystemMemoryDetour), this);
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
        public delegate bool GetPhysicallyInstalledSystemMemoryDelegate(out long memKb);

        #endregion Delegates

        #region Properties

        public bool Error { get; set; }

        public string Name { get; set; }

        #endregion Properties

        #region Methods

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetPhysicallyInstalledSystemMemory(out long memKb);

        public void Dispose()
        {
            if (_hook == null)
                return;

            _hook.Dispose();
            _hook = null;
        }

        private bool GetPhysicallyInstalledSystemMemoryDetour(out long memKb)
        {
            memKb = (long)_totalPhys * 1024;
            return true;
        }

        #endregion Methods
    }
}