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


    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct DEVMODE_A
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string dmDeviceName;
        public short dmSpecVersion;
        public short dmDriverVersion;
        public short dmSize;
        public short dmDriverExtra;
        public int dmFields;
        public short dmOrientation;
        public short dmPaperSize;
        public short dmPaperLength;
        public short dmPaperWidth;
        public short dmScale;
        public short dmCopies;
        public short dmDefaultSource;
        public short dmPrintQuality;
        public short dmColor;
        public short dmDuplex;
        public short dmYResolution;
        public short dmTTOption;
        public short dmCollate;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string dmFormName;
        public short dmUnusedPadding;
        public short dmBitsPerPel;
        public int dmPelsWidth;
        public int dmPelsHeight;
        public int dmDisplayFlags;
        public int dmDisplayFrequency;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct DEVMODE
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string dmDeviceName;
        public short dmSpecVersion;
        public short dmDriverVersion;
        public short dmSize;
        public short dmDriverExtra;
        public int dmFields;
        public short dmOrientation;
        public short dmPaperSize;
        public short dmPaperLength;
        public short dmPaperWidth;
        public short dmScale;
        public short dmCopies;
        public short dmDefaultSource;
        public short dmPrintQuality;
        public short dmColor;
        public short dmDuplex;
        public short dmYResolution;
        public short dmTTOption;
        public short dmCollate;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string dmFormName;
        public short dmUnusedPadding;
        public short dmBitsPerPel;
        public int dmPelsWidth;
        public int dmPelsHeight;
        public int dmDisplayFlags;
        public int dmDisplayFrequency;

        public override String ToString()
        {
            return "dmDeviceName: " + dmDeviceName + " dmSpecVersion: " + dmSpecVersion + " dmDriverVersion: " + dmDriverVersion + " dmSize: " + dmSize + " dmDriverExtra: " + dmDriverExtra + " dmFields: " + dmFields + " dmOrientation: " + dmOrientation + " dmPaperSize: " + dmPaperSize + " dmPaperLength: " + dmPaperLength + " dmPaperWidth: " + dmPaperWidth + " dmScale: " + dmScale + " dmCopies: " + dmCopies + " dmDefaultSource: " + dmDefaultSource + " dmPrintQuality: " + dmPrintQuality + " dmColor: " + dmColor + " dmDuplex: " + dmDuplex + " dmYResolution: " + dmYResolution + " dmTTOption: " + dmTTOption + " dmCollate: " + dmCollate + " dmFormName: " + dmFormName + " dmUnusedPadding: " + dmUnusedPadding + " dmBitsPerPel: " + dmBitsPerPel + " dmPelsWidth: " + dmPelsWidth + " dmPelsHeight: " + dmPelsHeight + " dmDisplayFlags: " + dmDisplayFlags + " dmDisplayFrequency: " + dmDisplayFrequency;
        }
    }



    /// <summary>
    ///     Description of EnumDisplaySettingController.
    /// </summary>
    public class EnumDisplaySettingController : IHook, IDisposable
    {
        #region Fields

        private LocalHook _hook;
        private LocalHook _hookA;
        private HWSettings _hWSettings;

        #endregion Fields

        #region Constructors

        public EnumDisplaySettingController(HWSettings hWSettings)
        {
            Error = false;
            this._hWSettings = hWSettings;
            Name = typeof(EnumDisplaySettingController).Name;

            try
            {
                _hook = LocalHook.Create(
                    LocalHook.GetProcAddress("user32.dll", "EnumDisplaySettingsW"),
                    new Delegate(Detour),
                    this);

                _hookA = LocalHook.Create(
                    LocalHook.GetProcAddress("user32.dll", "EnumDisplaySettingsA"),
                    new Delegate(DetourA),
                    this);

                _hook.ThreadACL.SetExclusiveACL(new Int32[] { });
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
        public static extern bool EnumDisplaySettingsW(IntPtr lpszDeviceName, int iModeNum, IntPtr lpDevMode);

        [DllImport("user32.dll")]
        public static extern bool EnumDisplaySettingsA(IntPtr lpszDeviceName, int iModeNum, IntPtr lpDevMode);

        #region Delegates

        private delegate bool Delegate(IntPtr lpszDeviceName, int iModeNum, IntPtr lpDevMode);



        #endregion Delegates

        #region Properties

        public bool Error { get; set; }
        public string Name { get; set; }

        #endregion Properties

        #region Methods

        public void Dispose()
        {
            _hook.Dispose();
            _hookA.Dispose();
        }

        private bool Detour(IntPtr lpszDeviceName, int iModeNum, IntPtr lpDevMode)
        {
            
            var orig = EnumDisplaySettingsW(lpszDeviceName, iModeNum, lpDevMode);
            var struc = Marshal.PtrToStructure<DEVMODE>(lpDevMode);

            //WCFClient.Instance.GetPipeProxy.RemoteLog($"EnumDisplaySettingsW Struct [{struc}]");
            
            //struc.dmPelsWidth = _hWSettings.MonitorWidth;
            //struc.dmPelsHeight = _hWSettings.MonitorHeight;
            //struc.dmDisplayFrequency = 240;

            Marshal.StructureToPtr(struc, lpDevMode, true);
            return orig;
        }

        private bool DetourA(IntPtr lpszDeviceName, int iModeNum, IntPtr lpDevMode)
        {
            var orig = EnumDisplaySettingsA(lpszDeviceName, iModeNum, lpDevMode);
            var struc = Marshal.PtrToStructure<DEVMODE_A>(lpDevMode);
            //WCFClient.Instance.GetPipeProxy.RemoteLog($"EnumDisplaySettingsA  Struct [{struc}]");
            //struc.dmPelsWidth = _hWSettings.MonitorWidth;
            //struc.dmPelsHeight = _hWSettings.MonitorHeight;
            //struc.dmDisplayFrequency = 240;

            Marshal.StructureToPtr(struc, lpDevMode, true);
            return orig;
        }

        #endregion Methods

    }
}