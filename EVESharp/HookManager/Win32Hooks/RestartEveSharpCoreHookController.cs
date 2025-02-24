using EasyHook;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Remoting.Messaging;

namespace HookManager.Win32Hooks
{
    public class RestartEveSharpCoreHookController : IHook, IDisposable
    {
        #region Fields

        private LocalHook _hook;

        #endregion Fields

        #region Constructors

        private delegate void RestartEveSharpCoreHookDelegate();

        public RestartEveSharpCoreHookController()
        {
            Error = false;
            Name = typeof(RestartEveSharpCoreHookController).Name;

            try
            {
                var memManModhandle = NativeAPI.GetModuleHandle("MemMan.dll");
                var procAdd = NativeAPI.GetProcAddress(memManModhandle, "RestartEveSharpCore");
                _hook = LocalHook.Create(
                    procAdd,
                    new RestartEveSharpCoreHookDelegate(Detour),
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

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
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

        public static void Detour()
        {
            Debug.WriteLine("RestartEveSharpCoreHookController detour");
            //Program.MainForm.RestartQuestor(null, false);
        }

        #endregion Methods
    }
}