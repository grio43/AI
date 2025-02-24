using EasyHook;
using SharedComponents.EVE;
using System;
using SharedComponents.Utility;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace HookManager.Win32Hooks
{
    using SharedComponents.IPC;
    using SharedComponents.WinApiUtil;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Threading;


    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct MONITORINFO_W
    {
        public int cbSize;
        public RectStruct rcMonitor;
        public RectStruct rcWork;
        public int dwFlags;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct MONITORINFO_A
    {
        public int cbSize;
        public RectStruct rcMonitor;
        public RectStruct rcWork;
        public int dwFlags;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct MONITORINFOEX_W
    {
        public int cbSize;
        public RectStruct rcMonitor;
        public RectStruct rcWork;
        public int dwFlags;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string szDevice;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct MONITORINFOEX_A
    {
        public int cbSize;
        public RectStruct rcMonitor;
        public RectStruct rcWork;
        public int dwFlags;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string szDevice;
    }

    /// <summary>
    /// The RECT structure defines the coordinates of the upper-left and lower-right corners of a rectangle.
    /// </summary>
    /// <see cref="http://msdn.microsoft.com/en-us/library/dd162897%28VS.85%29.aspx"/>
    /// <remarks>
    /// By convention, the right and bottom edges of the rectangle are normally considered exclusive.
    /// In other words, the pixel whose coordinates are ( right, bottom ) lies immediately outside of the the rectangle.
    /// For example, when RECT is passed to the FillRect function, the rectangle is filled up to, but not including,
    /// the right column and bottom row of pixels. This structure is identical to the RECTL structure.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    public struct RectStruct
    {
        /// <summary>
        /// The x-coordinate of the upper-left corner of the rectangle.
        /// </summary>
        public int Left;

        /// <summary>
        /// The y-coordinate of the upper-left corner of the rectangle.
        /// </summary>
        public int Top;

        /// <summary>
        /// The x-coordinate of the lower-right corner of the rectangle.
        /// </summary>
        public int Right;

        /// <summary>
        /// The y-coordinate of the lower-right corner of the rectangle.
        /// </summary>
        public int Bottom;

        public override string ToString()
        {
            return string.Format("Left: {0}, Top: {1}, Right: {2}, Bottom: {3}", Left, Top, Right, Bottom);
        }
    }


    /// <summary>
    ///     Description of GetMonitorInfoController.
    /// </summary>
    public class GetMonitorInfoController : IHook, IDisposable
    {
        #region Fields

        private LocalHook _hookW;
        private LocalHook _hookA;
        private HWSettings _hWSettings;

        #endregion Fields

        #region Constructors

        public GetMonitorInfoController(HWSettings hWSettings)
        {
            Error = false;
            this._hWSettings = hWSettings;
            Name = typeof(GetMonitorInfoController).Name;

            try
            {
                _hookW = LocalHook.Create(
                    LocalHook.GetProcAddress("user32.dll", "GetMonitorInfoW"),
                    new Delegate(DetourW),
                    this);

                _hookA = LocalHook.Create(
                    LocalHook.GetProcAddress("user32.dll", "GetMonitorInfoA"),
                    new Delegate(DetourA),
                    this);

                _hookW.ThreadACL.SetExclusiveACL(new Int32[] { });
                _hookA.ThreadACL.SetExclusiveACL(new Int32[] { });
                Error = false;
            }
            catch (Exception)
            {
                Error = true;
            }
        }

        #endregion Constructors
        [DllImport("user32.dll")]
        public static extern bool GetMonitorInfoW(IntPtr hMonitor, IntPtr lpmi);

        [DllImport("user32.dll")]
        public static extern bool GetMonitorInfoA(IntPtr hMonitor, IntPtr lpmi);

        #region Delegates

        private delegate bool Delegate(IntPtr hMonitor, IntPtr lpmi);

        #endregion Delegates

        #region Properties

        public bool Error { get; set; }
        public string Name { get; set; }

        #endregion Properties

        #region Methods

        public void Dispose()
        {
            _hookW.Dispose();
            _hookA.Dispose();
        }


        private bool DetourW(IntPtr hMonitor, IntPtr lpmi)
        {

            var orig = GetMonitorInfoW(hMonitor, lpmi);
            var info = Marshal.PtrToStructure<MONITORINFO_W>(lpmi);
            //WCFClient.Instance.GetPipeProxy.RemoteLog($"GetMonitorInfoW Monitor {info.rcMonitor} WorkArea {info.rcWork}");

            
            info.rcMonitor.Right = info.rcMonitor.Left + _hWSettings.MonitorWidth;
            info.rcMonitor.Bottom = info.rcMonitor.Top + _hWSettings.MonitorHeight;
            info.rcWork.Right = info.rcWork.Right + _hWSettings.MonitorWidth;
            info.rcWork.Bottom = info.rcWork.Top + _hWSettings.MonitorHeight;


            if (info.cbSize == Marshal.SizeOf<MONITORINFOEX_W>())
            {
               var infoEx = Marshal.PtrToStructure<MONITORINFOEX_W>(lpmi);



                infoEx.rcMonitor.Right = info.rcMonitor.Left + _hWSettings.MonitorWidth;
                infoEx.rcMonitor.Bottom = info.rcMonitor.Top + _hWSettings.MonitorHeight;
                infoEx.rcWork.Right = info.rcWork.Right + _hWSettings.MonitorWidth;
                infoEx.rcWork.Bottom = info.rcWork.Top + _hWSettings.MonitorHeight;

                Marshal.StructureToPtr(infoEx, lpmi, true);
            } else
            {
                Marshal.StructureToPtr(info, lpmi, true);
            }            

            
            return orig;
        }

        private bool DetourA(IntPtr hMonitor, IntPtr lpmi)
        {
            var orig = GetMonitorInfoA(hMonitor, lpmi);

            var info = Marshal.PtrToStructure<MONITORINFO_A>(lpmi);
            //WCFClient.Instance.GetPipeProxy.RemoteLog($"GetMonitorInfoA Monitor {info.rcMonitor} WorkArea {info.rcWork}");

            info.rcMonitor.Right = info.rcMonitor.Left + _hWSettings.MonitorWidth;
            info.rcMonitor.Bottom = info.rcMonitor.Top + _hWSettings.MonitorHeight;
            info.rcWork.Right = info.rcWork.Right + _hWSettings.MonitorWidth;
            info.rcWork.Bottom = info.rcWork.Top + _hWSettings.MonitorHeight;


            if (info.cbSize == Marshal.SizeOf<MONITORINFOEX_A>())
            {
                var infoEx = Marshal.PtrToStructure<MONITORINFOEX_A>(lpmi);


                infoEx.rcMonitor.Right = info.rcMonitor.Left + _hWSettings.MonitorWidth;
                infoEx.rcMonitor.Bottom = info.rcMonitor.Top + _hWSettings.MonitorHeight;
                infoEx.rcWork.Right = info.rcWork.Right + _hWSettings.MonitorWidth;
                infoEx.rcWork.Bottom = info.rcWork.Top + _hWSettings.MonitorHeight;

                
                Marshal.StructureToPtr(infoEx, lpmi, true);
            }
            else
            {
                Marshal.StructureToPtr(info, lpmi, true);
            }

            
            return orig;
        }

        #endregion Methods

    }
}