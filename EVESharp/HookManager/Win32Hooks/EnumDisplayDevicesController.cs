using EasyHook;
using SharedComponents.EVE;
using System;
using SharedComponents.Utility;
using System.Runtime.InteropServices;
using SharedComponents.IPC;

namespace HookManager.Win32Hooks
{
    /// <summary>
    ///     Description of EnumDisplayDevicesController.
    /// </summary>
    public class EnumDisplayDevicesAController : IHook, IDisposable
    {
        #region Fields

        private LocalHook _hook;
        private HWSettings _hWSettings;

        #endregion Fields

        #region Constructors

        public EnumDisplayDevicesAController(HWSettings hWSettings)
        {
            Error = false;
            this._hWSettings = hWSettings;
            Name = typeof(EnumDisplayDevicesAController).Name;

            try
            {
                _hook = LocalHook.Create(
                    LocalHook.GetProcAddress("user32.dll", "EnumDisplayDevicesA"),
                    new EnumDisplayDevicesDelegate(EnumDisplayDevicesDetour),
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

        private delegate bool EnumDisplayDevicesDelegate(string lpDevice, uint iDevNum, ref DISPLAY_DEVICE lpDisplayDevice, uint dwFlags);

        [DllImport("user32.dll")]
        static extern bool EnumDisplayDevicesA(string lpDevice, uint iDevNum, ref DISPLAY_DEVICE lpDisplayDevice, uint dwFlags);

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

        private bool EnumDisplayDevicesDetour(string lpDevice, uint iDevNum, ref DISPLAY_DEVICE lpDisplayDevice, uint dwFlags)
        {
            var orig = EnumDisplayDevicesA(lpDevice, iDevNum, ref lpDisplayDevice, dwFlags);
            if (lpDisplayDevice.StateFlags.HasFlag(DisplayDeviceStateFlags.PrimaryDevice))
            {

                if (lpDisplayDevice.DeviceID.Contains("DEV_"))
                {
                    var pos = lpDisplayDevice.DeviceID.IndexOf("DEV_");
                    var substring = lpDisplayDevice.DeviceID.Substring(pos, ("DEV_").Length + 4);
                    var replacement = "DEV_" + _hWSettings.GpuDeviceId.ToString("X");
                    //Util.GlobalRemoteLog($"EnumDisplayDevicesADetour Orig [{substring}] Spoofed [{replacement}]");
                    //HookManagerImpl.Log($"EnumDisplayDevicesDetour Orig [{substring}] Spoofed [{replacement}]");
                    //WCFClient.Instance.GetPipeProxy.RemoteLog("DeviceString " + lpDisplayDevice.DeviceString); // device string is the gpu
                    lpDisplayDevice.DeviceID = lpDisplayDevice.DeviceID.Replace(substring, replacement);
                }
            }
            return orig;
        }

        #endregion Methods


        [Flags()]
        public enum DisplayDeviceStateFlags : int
        {
            /// <summary>The device is part of the desktop.</summary>
            AttachedToDesktop = 0x1,
            MultiDriver = 0x2,
            /// <summary>The device is part of the desktop.</summary>
            PrimaryDevice = 0x4,
            /// <summary>Represents a pseudo device used to mirror application drawing for remoting or other purposes.</summary>
            MirroringDriver = 0x8,
            /// <summary>The device is VGA compatible.</summary>
            VGACompatible = 0x10,
            /// <summary>The device is removable; it cannot be the primary display.</summary>
            Removable = 0x20,
            /// <summary>The device has more display modes than its output devices support.</summary>
            ModesPruned = 0x8000000,
            Remote = 0x4000000,
            Disconnect = 0x2000000
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct DISPLAY_DEVICE
        {
            [MarshalAs(UnmanagedType.U4)]
            public int cb;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string DeviceName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceString;
            [MarshalAs(UnmanagedType.U4)]
            public DisplayDeviceStateFlags StateFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceID;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceKey;
        }
    }
}