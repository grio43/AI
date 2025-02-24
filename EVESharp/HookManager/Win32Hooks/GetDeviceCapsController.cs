using EasyHook;
using SharedComponents.EVE;
using System;
using SharedComponents.Utility;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace HookManager.Win32Hooks
{
    using SharedComponents.IPC;
    using System.Runtime.InteropServices;





    /// <summary>
    ///     Description of GetDeviceCapsController.
    /// </summary>
    public class GetDeviceCapsController : IHook, IDisposable
    {
        #region Fields

        private LocalHook _hook;
        private HWSettings _hWSettings;

        [DllImport("gdi32.dll")]
        static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

        #endregion Fields

        #region Constructors

        public GetDeviceCapsController(HWSettings hWSettings)
        {
            Error = false;
            this._hWSettings = hWSettings;
            Name = typeof(GetDeviceCapsController).Name;

            try
            {
                _hook = LocalHook.Create(
                    LocalHook.GetProcAddress("gdi32.dll", "GetDeviceCaps"),
                    new Delegate(Detour),
                    this);


                _hook.ThreadACL.SetExclusiveACL(new Int32[] { });
                Error = false;
            }
            catch (Exception)
            {
                Error = true;
            }
        }


        #region Delegates

        private delegate int Delegate(IntPtr hdc, int nIndex);



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

        private int Detour(IntPtr hdc, int nIndex)
        {
            var orig = GetDeviceCaps(hdc, nIndex);
            return orig;
        }


        #endregion Methods

    }
}

#endregion