using EasyHook;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;

namespace HookManager.Win32Hooks
{
    public class RegisterClassExWController : IHook, IDisposable
    {
        #region Fields

        private LocalHook _hook;
        private LocalHook _WNDPROCHook;

        [DllImport("user32.dll")]
        static extern IntPtr DefWindowProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr RegisterClassExW(IntPtr s);


        struct WNDCLASSEX
        {
            [MarshalAs(UnmanagedType.U4)]
            public int cbSize;
            [MarshalAs(UnmanagedType.U4)]
            public int style;
            public IntPtr lpfnWndProc;
            public int cbClsExtra;
            public int cbWndExtra;
            public IntPtr hInstance;
            public IntPtr hIcon;
            public IntPtr hCursor;
            public IntPtr hbrBackground;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string lpszMenuName;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string lpszClassName;
            public IntPtr hIconSm;
        }

        #endregion Fields

        #region Constructors

        public RegisterClassExWController()
        {
            Error = false;
            Name = typeof(RegisterClassExWController).Name;

            try
            {
                _hook = LocalHook.Create(
                    LocalHook.GetProcAddress("user32.dll", "RegisterClassExW"),
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

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Auto)]
        private delegate IntPtr Delegate(IntPtr s);

        private delegate IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        #endregion Delegates

        #region Properties

        public bool Error { get; set; }
        public string Name { get; set; }

        public static bool WndProcHookadded { get; set; }

        private bool _wndHookAdded;

        #endregion Properties

        #region Methods

        private static WndProc _origFunc;

        public void AddWndProcHook(IntPtr address)
        {
            try
            {
                _origFunc = Marshal.GetDelegateForFunctionPointer<WndProc>(address);
                _WNDPROCHook = LocalHook.Create(address, new WndProc(WndProcDetour), this);
                _WNDPROCHook.ThreadACL.SetExclusiveACL(new Int32[] { });
            }
            catch (Exception)
            {
                CryptHashDataController.ForceQuit("Error: AddWndProcHook failed.");
                return;
            }
            WndProcHookadded = true;
        }

        public void Dispose()
        {
            _hook.Dispose();
        }

        private IntPtr WndProcDetour(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            switch (msg)
            {
                case 0x0006: // WM_ACTIVATE                 
                case 0x001C: // WM_ACTIVATEAPP
                    return DefWindowProc(hWnd, msg, wParam, lParam);
                default:
                    return _origFunc(hWnd, msg, wParam, lParam);
            }
        }

        private IntPtr Detour(IntPtr s)
        {
            var struc = Marshal.PtrToStructure<WNDCLASSEX>(s);
            if (!_wndHookAdded && struc.lpszClassName.Equals("trinityWindow"))
            {
                Console.WriteLine($"RegisterClassExW proc! lpszClassName {struc.lpszClassName} lpszMenuName {struc.lpszMenuName}");
                AddWndProcHook(struc.lpfnWndProc);
                _wndHookAdded = true;
            }
            return RegisterClassExW(s);
        }

        #endregion Methods
    }
}