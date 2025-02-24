using EasyHook;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;

namespace HookManager.Win32Hooks
{
    public class TemplateController : IHook, IDisposable
    {
        #region Fields

        private LocalHook _hook;

        #endregion Fields

        #region Constructors

        public TemplateController(IntPtr funcAddr)
        {
            Error = false;
            Name = typeof(TemplateController).Name;

            try
            {

                //_hook = LocalHook.Create(
                //    LocalHook.GetProcAddress("kernel32.dll", "CreateFileA"),
                //    new CreateFileAController.CreateFileADelegate(CreateFileADetour),
                //    this);

                _hook = LocalHook.Create(
                    funcAddr,
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

        private static IntPtr Detour()
        {
            return IntPtr.Zero;
        }

        #endregion Methods
    }
}