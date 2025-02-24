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
    public class LoadLibraryWController : IHook, IDisposable
    {
        #region Fields

        private LocalHook _hook;

        #endregion Fields

        #region Constructors

        public LoadLibraryWController()
        {
            Error = false;
            Name = typeof(LoadLibraryWController).Name;

            try
            {
                _hook = LocalHook.Create(
                    LocalHook.GetProcAddress("Kernel32.dll", "LoadLibraryW"),
                    new LoadLibraryWDelegate(LoadLibraryWDetour),
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

        private delegate IntPtr LoadLibraryWDelegate(IntPtr lpModuleName);

        #endregion Delegates

        #region Properties

        public bool Error { get; set; }
        public string Name { get; set; }

        #endregion Properties

        #region Methods

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr LoadLibraryW(IntPtr lpModuleName);

        public void Dispose()
        {
            _hook.Dispose();
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern void SetLastError(int errorCode);

        private IntPtr LoadLibraryWDetour(IntPtr lpModuleName)
        {
            try
            {
                var lpModName = Marshal.PtrToStringUni(lpModuleName);
                if (lpModName != null && lpModName != "")
                    if (HookManagerImpl.NeedsToBeCloaked(lpModName))
                    {
                        HookManagerImpl.Log("[CLOAKED] " + lpModName);
                        return IntPtr.Zero;
                    }
                    else
                    {
                        if (!HookManagerImpl.IsWhiteListedFileName(lpModName)) HookManagerImpl.Log("[NOT_CLOAKING] " + lpModName);
                        return LoadLibraryW(lpModuleName);
                    }
                return LoadLibraryW(lpModuleName);
            }
            catch (Exception)
            {
                return IntPtr.Zero;
            }
        }

        #endregion Methods
    }
}