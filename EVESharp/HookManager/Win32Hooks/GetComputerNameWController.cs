

using EasyHook;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace HookManager.Win32Hooks
{
    public class GetComputerNameWController : IHook, IDisposable
    {
        #region Fields
        ///Return Type: BOOL->int
        ///lpBuffer: LPWSTR->WCHAR*
        ///nSize: LPDWORD->DWORD*
        [System.Runtime.InteropServices.DllImportAttribute("kernel32.dll", EntryPoint = "GetComputerNameW")]
        [return: System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.Bool)]
        public static extern bool GetComputerNameW([Out] IntPtr buffer, [In][Out] ref uint nSize);




        private LocalHook _hook;
        private string _computerName;
        #endregion Fields

        #region Constructors

        public GetComputerNameWController(string computerName)
        {
            _computerName = computerName;
            Error = false;
            Name = typeof(GetComputerNameWController).Name;

            try
            {
                _hook = LocalHook.Create(
                    LocalHook.GetProcAddress("kernel32.dll", "GetComputerNameW"),
                    new GetComputerNameWDelegate(GetComputerNameWDetour),
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

        private delegate bool GetComputerNameWDelegate([Out] IntPtr buffer, [In][Out] ref uint nSize);

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

        private bool GetComputerNameWDetour([Out] IntPtr lpBuffer, [In][Out] ref uint nSize)
        {
            var ret = GetComputerNameW(lpBuffer, ref nSize);
            var str = this._computerName.ToUpper() + Char.MinValue;
            var buffer = UnicodeEncoding.Unicode.GetBytes(str);
            for (var i = 0; i < buffer.Length; i++)
                Marshal.WriteByte(lpBuffer, i, buffer[i]);
            nSize = (uint)this._computerName.Length;

            return ret;
        }

        #endregion Methods
    }
}