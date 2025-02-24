/*
 * ---------------------------------------
 * User: duketwo
 * Date: 31.12.2013
 * Time: 00:17
 *
 * ---------------------------------------
 */

using EasyHook;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace HookManager.Win32Hooks
{
    /// <summary>
    ///     Description of SHGetFolderPathWController.
    /// </summary>
    public class SHGetFolderPathWController : IHook, IDisposable
    {
        #region Fields

        private LocalHook _hook;

        #endregion Fields

        #region Constructors

        public SHGetFolderPathWController(string newPathPersonal, string newPathLocalAppData)
        {
            NewPathPersonal = newPathPersonal;
            NewPathLocalAppData = newPathLocalAppData;
            Error = false;
            Name = typeof(SHGetFolderPathWController).Name;
            try
            {
                _hook = LocalHook.Create(
                    LocalHook.GetProcAddress("shell32.dll", "SHGetFolderPathW"),
                    new SHGetFolderPathWDelegate(SHGetFolderPathWDetour),
                    this);

                _hook.ThreadACL.SetExclusiveACL(new Int32[] { });
                Error = false;
            }
            catch (Exception e)
            {
                HookManagerImpl.Log("Exception: " + e.ToString());
                Error = true;
            }
        }

        #endregion Constructors

        #region Delegates

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        private delegate int SHGetFolderPathWDelegate(IntPtr hwndOwner, int nFolder, IntPtr hToken,
            UInt32 dwFlags, [In] [Out] IntPtr pszPath);

        #endregion Delegates

        #region Properties

        public bool Error { get; set; }
        public string Name { get; set; }
        private static string NewPathLocalAppData { get; set; }
        private static string NewPathPersonal { get; set; }

        #endregion Properties

        #region Methods

        public void Dispose()
        {
            _hook.Dispose();
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern void SetLastError(int errorCode);

        [DllImport("shell32.dll")]
        private static extern int SHGetFolderPathW(IntPtr hwndOwner, int nFolder, IntPtr hToken,
            UInt32 dwFlags, [In] [Out] IntPtr pszPath);

        private static int SHGetFolderPathWDetour(IntPtr hwndOwner, int nFolder, IntPtr hToken,
            UInt32 dwFlags, [In] [Out] IntPtr pszPath)
        {
            var ret = SHGetFolderPathW(hwndOwner, nFolder, hToken, dwFlags, pszPath);
            if (nFolder == 0x0005 && NewPathPersonal != string.Empty)
            {
                // PERSONAL

                var str = NewPathPersonal + Char.MinValue;
                var buffer = UnicodeEncoding.Unicode.GetBytes(str);
                for (var i = 0; i < buffer.Length; i++) Marshal.WriteByte(pszPath, i, buffer[i]);
            }

            if (nFolder == 0x001c && NewPathLocalAppData != string.Empty)
            {
                // LOCAL APP DATA

                var str = NewPathLocalAppData + Char.MinValue;
                var buffer = UnicodeEncoding.Unicode.GetBytes(str);
                for (var i = 0; i < buffer.Length; i++) Marshal.WriteByte(pszPath, i, buffer[i]);
            }
            return ret;
        }

        #endregion Methods
    }
}