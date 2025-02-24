using EasyHook;
using SharedComponents.SharedMemory;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;

namespace HookManager.Win32Hooks
{
    public class GetForegroundWindowController : IHook, IDisposable
    {
        #region Fields

        private LocalHook _hook;

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        private static SharedComponents.SharedMemory.SharedArray<IntPtr> _sharedArray;

        #endregion Fields

        #region Constructors

        public GetForegroundWindowController()
        {
            Error = false;
            Name = typeof(GetForegroundWindowController).Name;
            _sharedArray = new SharedComponents.SharedMemory.SharedArray<IntPtr>(Process.GetCurrentProcess().Id.ToString() + nameof(UsedSharedMemoryNames.ForegroundWindowHWND), 1);
            HookManagerImpl.Log("GetForegroundWindowController: _sharedArray [" + _sharedArray[0] + "]");
            try
            {


                //_hook = LocalHook.Create(
                //    LocalHook.GetProcAddress("kernel32.dll", "CreateFileA"),
                //    new CreateFileAController.CreateFileADelegate(CreateFileADetour),
                //    this);

                _hook = LocalHook.Create(
                    LocalHook.GetProcAddress("user32.dll", "GetForegroundWindow"),
                    new Delegate(Detour),
                    this);

                _hook.ThreadACL.SetExclusiveACL(new Int32[] { 0 });
                Error = false;

                GetForegroundWindow(); // call once to initialize

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

        public static IntPtr CurrentForegroundWnd { get; set; } = IntPtr.Zero;

        private static bool _checked { get; set; } = false;

        public static IntPtr GetCurrentForegroundWnd
        {
            get
            {
                GetForegroundWindow(); // this triggers the hook
                return CurrentForegroundWnd; // return the original value
            }
        }

        #endregion Properties

        #region Methods



        public void Dispose()
        {
            _hook.Dispose();
        }

        private static IntPtr Detour()
        {
            if (!_checked)
            {
                if ((DateTime.UtcNow - Program.TimeStarted).TotalSeconds > 60)
                {
                    if (CreateWindowExWController.EVEHandle == IntPtr.Zero)
                    {
                        Debug.WriteLine("CreateWindowExWController error.");
                        Environment.Exit(0);
                        Environment.FailFast("exit");
                    }
                    _checked = true;
                }
            }

            CurrentForegroundWnd = GetForegroundWindow();
            _sharedArray[0] = CurrentForegroundWnd;
            //Console.WriteLine("GetForegroundWnd proc!");
            return CreateWindowExWController.EVEHandle;
        }

        #endregion Methods
    }
}