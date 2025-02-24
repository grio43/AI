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
    public class SHGetFolderPathAController : IHook, IDisposable
    {
        #region Fields

        private LocalHook _hook;

        #endregion Fields

        #region Constructors

        public SHGetFolderPathAController(string newPathPersonal, string newPathLocalAppData)
        {
            NewPathPersonal = newPathPersonal;
            NewPathLocalAppData = newPathLocalAppData;
            Error = false;
            Name = typeof(SHGetFolderPathAController).Name;
            try
            {
                _hook = LocalHook.Create(
                    LocalHook.GetProcAddress("shell32.dll", "SHGetFolderPathA"),
                    new SHGetFolderPathADelegate(SHGetFolderPathADetour),
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

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        private delegate int SHGetFolderPathADelegate(IntPtr hwndOwner, int nFolder, IntPtr hToken,
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
        private static extern int SHGetFolderPathA(IntPtr hwndOwner, int nFolder, IntPtr hToken,
            UInt32 dwFlags, [In] [Out] IntPtr pszPath);

        private static int SHGetFolderPathADetour(IntPtr hwndOwner, int nFolder, IntPtr hToken,
            UInt32 dwFlags, [In] [Out] IntPtr pszPath)
        {
            var ret = SHGetFolderPathA(hwndOwner, nFolder, hToken, dwFlags, pszPath);
            if (nFolder == 0x0005 && NewPathPersonal != string.Empty)
            {
                // PERSONAL

                var str = NewPathPersonal + Char.MinValue;
                var buffer = ASCIIEncoding.ASCII.GetBytes(str);
                for (var i = 0; i < buffer.Length; i++)
                    Marshal.WriteByte(pszPath, i, buffer[i]);
            }

            if (nFolder == 0x001c && NewPathLocalAppData != string.Empty)
            {
                // LOCAL APP DATA

                var str = NewPathLocalAppData + Char.MinValue;
                var buffer = ASCIIEncoding.ASCII.GetBytes(str);
                for (var i = 0; i < buffer.Length; i++)
                    Marshal.WriteByte(pszPath, i, buffer[i]);
            }
            return ret;
        }

        #endregion Methods
    }
}