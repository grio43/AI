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
using System.Runtime.InteropServices;

namespace HookManager.Win32Hooks
{
    /// <summary>
    ///     Description of IsDebuggerPresent.
    /// </summary>
    public class MiniWriteDumpController : IHook, IDisposable
    {
        #region Fields

        private LocalHook _hook;

        #endregion Fields

        #region Constructors

        public MiniWriteDumpController()
        {
            Error = false;
            Name = typeof(MiniWriteDumpController).Name;
            var ptr = IntPtr.Zero;
            while (ptr == IntPtr.Zero) ptr = LoadLibrary("DbgHelp.dll");

            try
            {
                _hook = LocalHook.Create(
                    LocalHook.GetProcAddress("DbgHelp.dll", "MiniDumpWriteDump"),
                    new MiniDumpWriteDumpDelegate(MiniDumpWriteDumpDetour),
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

        private delegate bool MiniDumpWriteDumpDelegate(IntPtr lpModuleName, IntPtr processId, IntPtr hFile, IntPtr dumpType, IntPtr exceptionParam,
            IntPtr userStreamParam, IntPtr callbackParam);

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

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern void SetLastError(int errorCode);

        private bool MiniDumpWriteDumpDetour(IntPtr lpModuleName, IntPtr processId, IntPtr hFile, IntPtr dumpType, IntPtr exceptionParam,
            IntPtr userStreamParam, IntPtr callbackParam)
        {
            SetLastError(8);
            return false;
        }

        #endregion Methods
    }
}