using EasyHook;
using SharedComponents.IPC;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace HookManager.Win32Hooks
{
    public class DeviceIoControlController : IHook, IDisposable
    {
        [DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int DeviceIoControl(
        IntPtr hDevice,
        uint dwIoControlCode,
        ref long InBuffer,
        int nInBufferSize,
        ref long OutBuffer,
        int nOutBufferSize,
        ref int pBytesReturned,
        [In] ref NativeOverlapped lpOverlapped);

        #region Fields

        private LocalHook _hook;

        #endregion Fields

        #region Constructors

        public DeviceIoControlController()
        {
            Error = false;
            Name = typeof(DeviceIoControlController).Name;

            try
            {

                _hook = LocalHook.Create(
                    LocalHook.GetProcAddress("kernel32.dll", "DeviceIoControl"),
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
        private delegate int Delegate(
        IntPtr hDevice,
        uint dwIoControlCode,
        ref long InBuffer,
        int nInBufferSize,
        ref long OutBuffer,
        int nOutBufferSize,
        ref int pBytesReturned,
        [In] ref NativeOverlapped lpOverlapped);

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

        private enum IOCTL_METHOD : uint
        {
            METHOD_BUFFERED = 0,
            METHOD_IN_DIRECT = 1,
            METHOD_OUT_DIRECT = 2,
            METHOD_NEITHER = 3
        }

        private enum IOCTL_ACCESS : uint
        {
            FILE_ANY_ACCESS = 0,
            FILE_READ_ACCESS = 1,
            FILE_WRITE_ACCESS = 2
        }


        private static UInt32 CTL_CODE(uint DeviceType, uint Function, IOCTL_METHOD Method, IOCTL_ACCESS Access)
        {
            return ((DeviceType << 16) | (((uint)Access) << 14) | (Function << 2) | ((uint)Method));
        }


        public static void DecodeCTL_CODE(uint ctlCode, out uint DeviceType, out uint Function, out uint Method, out uint Access)
        {
            DeviceType = ctlCode >> 16;
            Access = (ctlCode >> 14) & 3;
            Function = (ctlCode >> 2) & 0xFFF;
            Method = ctlCode & 3;
        }


        public enum DEVICE_TYPE : uint
        {
            FILE_DEVICE_BEEP = 0x01,
            FILE_DEVICE_CD_ROM = 0x02,
            FILE_DEVICE_CD_ROM_FILE_SYSTEM = 0x03,
            FILE_DEVICE_CONTROLLER = 0x04,
            FILE_DEVICE_DATALINK = 0x05,
            FILE_DEVICE_DFS = 0x06,
            FILE_DEVICE_DISK = 0x07, // IOCTL_DISK_BASE
            FILE_DEVICE_DISK_FILE_SYSTEM = 0x08,
            FILE_DEVICE_FILE_SYSTEM = 0x09,
            FILE_DEVICE_INPORT_PORT = 0x0a,
            FILE_DEVICE_KEYBOARD = 0x0b,
            FILE_DEVICE_MAILSLOT = 0x0c,
            FILE_DEVICE_MIDI_IN = 0x0d,
            FILE_DEVICE_MIDI_OUT = 0x0e,
            FILE_DEVICE_MOUSE = 0x0f,
            FILE_DEVICE_MULTI_UNC_PROVIDER = 0x10,
            FILE_DEVICE_NAMED_PIPE = 0x11,
            FILE_DEVICE_NETWORK = 0x12,
            FILE_DEVICE_NETWORK_BROWSER = 0x13,
            FILE_DEVICE_NETWORK_FILE_SYSTEM = 0x14,
            FILE_DEVICE_NULL = 0x15,
            FILE_DEVICE_PARALLEL_PORT = 0x16,
            FILE_DEVICE_PHYSICAL_NETCARD = 0x17,
            FILE_DEVICE_PRINTER = 0x18,
            FILE_DEVICE_SCANNER = 0x19,
            FILE_DEVICE_SERIAL_MOUSE_PORT = 0x1a,
            FILE_DEVICE_SERIAL_PORT = 0x1b,
            FILE_DEVICE_SCREEN = 0x1c,
            FILE_DEVICE_SOUND = 0x1d,
            FILE_DEVICE_STREAMS = 0x1e,
            FILE_DEVICE_TAPE = 0x1f,
            FILE_DEVICE_TAPE_FILE_SYSTEM = 0x20,
            FILE_DEVICE_TRANSPORT = 0x21,
            FILE_DEVICE_UNKNOWN = 0x22,
            FILE_DEVICE_VIDEO = 0x23,
            FILE_DEVICE_VIRTUAL_DISK = 0x24,
            FILE_DEVICE_WAVE_IN = 0x25,
            FILE_DEVICE_WAVE_OUT = 0x26,
            FILE_DEVICE_8042_PORT = 0x27,
            FILE_DEVICE_NETWORK_REDIRECTOR = 0x28,
            FILE_DEVICE_BATTERY = 0x29,
            FILE_DEVICE_BUS_EXTENDER = 0x2a,
            FILE_DEVICE_MODEM = 0x2b,
            FILE_DEVICE_VDM = 0x2c,
            FILE_DEVICE_MASS_STORAGE = 0x2d, // IOCTL_STORAGE_BASE
            FILE_DEVICE_SMB = 0x2e,
            FILE_DEVICE_KS = 0x2f,
            FILE_DEVICE_CHANGER = 0x30, // IOCTL_CHANGER_BASE
            FILE_DEVICE_SMARTCARD = 0x31,
            FILE_DEVICE_ACPI = 0x32,
            FILE_DEVICE_DVD = 0x33,
            FILE_DEVICE_FULLSCREEN_VIDEO = 0x34,
            FILE_DEVICE_DFS_FILE_SYSTEM = 0x35,
            FILE_DEVICE_DFS_VOLUME = 0x36,
            FILE_DEVICE_SERENUM = 0x37,
            FILE_DEVICE_TERMSRV = 0x38,
            FILE_DEVICE_KSEC = 0x39,
            FILE_DEVICE_FIPS = 0x3A,
            FILE_DEVICE_INFINIBAND = 0x3B,
            FILE_DEVICE_VMBUS = 0x3E,
            FILE_DEVICE_CRYPT_PROVIDER = 0x3F,
            FILE_DEVICE_WPD = 0x40,
            FILE_DEVICE_BLUETOOTH = 0x41,
            FILE_DEVICE_MT_COMPOSITE = 0x42,
            FILE_DEVICE_MT_TRANSPORT = 0x43,
            FILE_DEVICE_BIOMETRIC = 0x44,
            FILE_DEVICE_PMI = 0x45,
            FILE_DEVICE_EHSTOR = 0x46,
            FILE_DEVICE_DEVAPI = 0x47,
            FILE_DEVICE_GPIO = 0x48,
            FILE_DEVICE_USBEX = 0x49,
            FILE_DEVICE_CONSOLE = 0x50,
            FILE_DEVICE_NFP = 0x51,
            FILE_DEVICE_SYSENV = 0x52,
            FILE_DEVICE_VIRTUAL_BLOCK = 0x53,
            FILE_DEVICE_POINT_OF_SERVICE = 0x54,
            FILE_DEVICE_STORAGE_REPLICATION = 0x55,
            FILE_DEVICE_TRUST_ENV = 0x56 // IOCTL_VOLUME_BASE
        }

        //private const uint FILE_DEVICE_MASS_STORAGE = 0x0000002d;
        // IOCTLs 0x0470 to 0x047f reserved for device and stack telemetry interfaces

        
        private static int Detour(
        IntPtr hDevice,
        uint dwIoControlCode,
        ref long InBuffer,
        int nInBufferSize,
        ref long OutBuffer,
        int nOutBufferSize,
        ref int pBytesReturned,
        [In] ref NativeOverlapped lpOverlapped)
        {
            DecodeCTL_CODE(dwIoControlCode, out uint DeviceType, out uint Function, out uint Method, out uint Access);
            try
            {
                if (Enum.IsDefined(typeof(DEVICE_TYPE), DeviceType))
                {
                    //MessageBox.Show($"DeviceType: {(DEVICE_TYPE)DeviceType} Function: {Function:X8} Method: {Method:X8} Access: {Access:X8}");
                    //WCFClient.Instance.GetPipeProxy.RemoteLog($"DeviceType: {(DEVICE_TYPE)DeviceType} Function: {Function:X8} Method: {Method:X8} Access: {Access:X8}");

                    if ((DEVICE_TYPE)DeviceType == DEVICE_TYPE.FILE_DEVICE_DEVAPI)
                    {
                        var r = DeviceIoControl(hDevice, dwIoControlCode, ref InBuffer, nInBufferSize, ref OutBuffer, nOutBufferSize, ref pBytesReturned, ref lpOverlapped);
                        return r;
                    }
                }
                else
                {
                    //MessageBox.Show($"DeviceIoControlController DEVICE_TYPE failure. Value {DeviceType:X8}");
                    WCFClient.Instance.GetPipeProxy.RemoteLog($"DeviceIoControlController DEVICE_TYPE failure. Value {DeviceType:X8}");
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }



            return 0; // just block any access for now, seems fine
        }

        #endregion Methods
    }
}