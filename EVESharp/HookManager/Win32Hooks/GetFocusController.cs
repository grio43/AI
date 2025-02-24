using EasyHook;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Windows.Forms;

namespace HookManager.Win32Hooks
{
    public class GetFocusWindowController : IHook, IDisposable
    {
        #region Fields

        private LocalHook _hook;

        [DllImport("user32.dll")]
        private static extern IntPtr GetFocus();

        #endregion Fields

        #region Constructors

        public GetFocusWindowController()
        {
            Error = false;
            Name = typeof(GetFocusWindowController).Name;

            try
            {

                _hook = LocalHook.Create(
                    LocalHook.GetProcAddress("user32.dll", "GetFocus"),
                    new Delegate(Detour),
                    this);

                _hook.ThreadACL.SetExclusiveACL(new Int32[] { 0 });
                Error = false;
            }
            catch (Exception)
            {
                Error = true;
            }
        }

        #endregion Constructors

        #region Delegates

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Auto)]
        private delegate IntPtr Delegate();

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

        private static IntPtr Detour()
        {
            //Console.WriteLine("GetFocus proc!");
            return CreateWindowExWController.EVEHandle;
        }

        #endregion Methods
    }
}