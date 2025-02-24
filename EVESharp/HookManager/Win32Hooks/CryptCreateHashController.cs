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
    //BOOL WINAPI CryptCreateHash(
    //_In_ HCRYPTPROV hProv,
    //_In_ ALG_ID     Algid,
    //_In_ HCRYPTKEY  hKey,
    //_In_ DWORD      dwFlags,
    //_Out_ HCRYPTHASH *phHash
    //)

    /// <summary>
    ///     Description of CryptHashDataController.
    /// </summary>
    public class CryptCreateHashController : IDisposable, IHook
    {
        #region Fields

        private LocalHook _hook;

        private string _name;

        #endregion Fields

        #region Constructors

        public CryptCreateHashController()
        {
            Name = typeof(CryptCreateHashController).Name;
            try
            {
                _name = string.Format("CryptCreateHash{0:X}", LocalHook.GetProcAddress("advapi32.dll", "CryptCreateHash"));
                _hook = LocalHook.Create(LocalHook.GetProcAddress("advapi32.dll", "CryptCreateHash"), new CryptCreateHashDelegate(CryptCreateHashDetour), this);
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
        public delegate bool CryptCreateHashDelegate(IntPtr hProv, uint algId, IntPtr hKey, uint dwFlags, [In] [Out] IntPtr phHash);

        #endregion Delegates

        #region Properties

        public bool Error { get; set; }

        public string Name { get; set; }

        #endregion Properties

        #region Methods

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool CryptCreateHash(IntPtr hProv, uint algId, IntPtr hKey, uint dwFlags, [In] [Out] IntPtr phHash);

        public void Dispose()
        {
            if (_hook == null)
                return;

            _hook.Dispose();
            _hook = null;
        }

        private bool CryptCreateHashDetour(IntPtr hProv, uint algId, IntPtr hKey, uint dwFlags, [In] [Out] IntPtr phHash)
        {
            var result = CryptCreateHash(hProv, algId, hKey, dwFlags, phHash);
            return result;
        }

        #endregion Methods
    }
}