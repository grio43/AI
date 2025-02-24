using EasyHook;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using DWORD = System.UInt32;
using System.Windows.Forms;
using SharedComponents.EVE;
using SharedComponents.Utility;

namespace HookManager.Win32Hooks
{
    public class CreateWindowExWController : IHook, IDisposable
    {
        #region Fields

        private LocalHook _hook;





        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "CreateWindowExW")]
        public static extern IntPtr CreateWindowExW(
            DWORD dwExStyle,
            IntPtr lpClassName,
            IntPtr lpWindowName,
            DWORD dwStyle,
            int x,
            int y,
            int nWidth,
            int nHeight,
            IntPtr hWndParent,
            IntPtr hMenu,
            IntPtr hInstance,
            IntPtr lpParam);

        #endregion Fields

        #region Constructors

        public CreateWindowExWController()
        {
            Error = false;
            Name = typeof(CreateWindowExWController).Name;

            try
            {

                _hook = LocalHook.Create(
                    LocalHook.GetProcAddress("user32.dll", "CreateWindowExW"),
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

        private delegate IntPtr Delegate(
            DWORD dwExStyle,
            IntPtr lpClassName,
            IntPtr lpWindowName,
            DWORD dwStyle,
            int x,
            int y,
            int nWidth,
            int nHeight,
            IntPtr hWndParent,
            IntPtr hMenu,
            IntPtr hInstance,
            IntPtr lpParam);

        #endregion Delegates

        #region Properties

        public bool Error { get; set; }
        public string Name { get; set; }

        public static IntPtr EVEHandle = IntPtr.Zero;

        private static bool _handleSet;

        #endregion Properties

        #region Methods

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hwnd, int nCmdShow);


        public void Dispose()
        {
            _hook.Dispose();
        }

        private static IntPtr Detour(
            DWORD dwExStyle,
            IntPtr lpClassName,
            IntPtr lpWindowName,
            DWORD dwStyle,
            int x,
            int y,
            int nWidth,
            int nHeight,
            IntPtr hWndParent,
            IntPtr hMenu,
            IntPtr hInstance,
            IntPtr lpParam)
        {
            try
            {
                var className = Marshal.PtrToStringAuto(lpClassName);
                var windowName = Marshal.PtrToStringAuto(lpWindowName);

                int dwNoActStyle = (int)dwExStyle;

                // Ensure WS_EX_NOACTIVATE and WS_EX_APPWINDOW are set
                dwNoActStyle |= 0x08000000; // WS_EX_NOACTIVATE
                dwNoActStyle |= 0x00040000; // WS_EX_APPWINDOW

                // Ensure WS_EX_TOOLWINDOW is NOT set
                dwNoActStyle &= ~0x00000080; // Remove WS_EX_TOOLWINDOW

                // Ensure WS_CHILD is NOT set
                //dwNoActStyle &= ~0x40000000; // Remove WS_CHILD

                bool isRelatedWnd = "EVE".Equals(windowName) || "EVESharp".Equals(windowName) || "HookManager".Equals(windowName);
                //dwStyle = "EVE".Equals(windowName) ? dwStyle | 0x20000000 : dwStyle;
                dwExStyle = isRelatedWnd ? (DWORD)dwNoActStyle : dwExStyle;

                //Debug.WriteLine($"windowName {windowName}");

                var ret = CreateWindowExW(dwExStyle, lpClassName, lpWindowName, dwStyle, x, y, nWidth, nHeight, hWndParent, hMenu, hInstance, lpParam);

                try
                {

                    if (isRelatedWnd)
                    {
                        //var eveDesktopWindowId = VirtualDesktopHelper.VirtualDesktopManager.GetWindowDesktopId(ret);
                        //var instVirtDesktopId = HookManagerImpl.Instance.EveAccount.StartOnVirtualDesktopId;
                        //if (instVirtDesktopId.HasValue && eveDesktopWindowId != instVirtDesktopId)
                        //{
                        //    VirtualDesktopHelper.VirtualDesktopManager.MoveWindowToDesktop(ret, instVirtDesktopId.Value);
                        //}
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }

                if (_handleSet || windowName == null || !windowName.Equals("EVE")) return ret;
                Console.WriteLine($"CreateWindowEx proc! className {className} windowName {windowName}");

                _handleSet = true;
                EVEHandle = ret;

                return ret;
            }
            catch (Exception e)
            {
                Console.WriteLine($"EX: {e}");
                return IntPtr.Zero;
            }

        }

        #endregion Methods
    }
}