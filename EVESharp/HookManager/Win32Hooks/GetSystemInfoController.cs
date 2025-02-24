using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using EasyHook;
using SharedComponents.EVE;
using SharedComponents.IPC;
using static HookManager.Win32Hooks.GetAdaptersAddressesController;

namespace HookManager.Win32Hooks
{
    public class GetSystemInfoController : IHook, IDisposable
    {
        #region Fields

        private LocalHook _hook;
        private LocalHook _hookNative;
        private static HWSettings _hWSettings;

        #endregion Fields

        #region Constructors

        // Import the GetSystemInfo function from the kernel32.dll library
        [DllImport("kernel32.dll")]
        private static extern void GetSystemInfo(IntPtr lpSystemInfo);

        // Import the GetSystemInfo function from the kernel32.dll library
        [DllImport("kernel32.dll")]
        private static extern void GetNativeSystemInfo(IntPtr lpSystemInfo);

        // The SYSTEM_INFO structure contains information about the current system
        [StructLayout(LayoutKind.Sequential)]
        internal struct SYSTEM_INFO
        {
            internal int dwOemId;    // This is a union of a DWORD and a struct containing 2 WORDs.
            internal int dwPageSize;
            internal IntPtr lpMinimumApplicationAddress;
            internal IntPtr lpMaximumApplicationAddress;
            internal IntPtr dwActiveProcessorMask;
            internal int dwNumberOfProcessors;
            internal int dwProcessorType;
            internal int dwAllocationGranularity;
            internal short wProcessorLevel;
            internal short wProcessorRevision;

        }

        public GetSystemInfoController(SharedComponents.EVE.HWSettings hWSettings)
        {
            Error = false;
            Name = typeof(GetSystemInfoController).Name;
            _hWSettings = hWSettings;

            try
            {
                // Create the function hook
                _hook = LocalHook.Create(
                    LocalHook.GetProcAddress("kernel32.dll", "GetSystemInfo"),
                            new DelegateGetSystemInfo(HookedGetSystemInfo),
                            this);

                _hook.ThreadACL.SetExclusiveACL(new int[] { });

                // Create the function hook
                _hookNative = LocalHook.Create(
                    LocalHook.GetProcAddress("kernel32.dll", "GetNativeSystemInfo"),
                            new DelegateGetSystemInfo(HookedGetNativeSystemInfo),
                            this);

                _hookNative.ThreadACL.SetExclusiveACL(new int[] { });

                Error = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                Error = true;
            }

        }

        #endregion Constructors

        #region Delegates

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        private delegate void DelegateGetSystemInfo(IntPtr lpSystemInfo);

        #endregion Delegates

        #region Properties

        public bool Error { get; set; }
        public string Name { get; set; }

        #endregion Properties

        #region Methods


        // The function that will be called instead of the original GetSystemInfo function
        // This crashes if we modify anything, but it does look like eve is using the native version below
        private static void HookedGetSystemInfo(IntPtr lpSystemInfo)
        {
            // Call the original GetSystemInfo function
            GetSystemInfo(lpSystemInfo);

            try
            {
                // Cast the IntPtr back to the SYSTEM_INFO struct
                //var sysInfo = (SYSTEM_INFO)Marshal.PtrToStructure(lpSystemInfo, typeof(SYSTEM_INFO));


            //WCFClient.Instance.GetPipeProxy.RemoteLog($"before {lpSystemInfo.ToString()}");
            // Modify the system information as desired

                // Marshal the modified struct back to the original memory location
                //Marshal.StructureToPtr(sysInfo, lpSystemInfo, fDeleteOld: true);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }


        }

        // The function that will be called instead of the original GetSystemInfo function
        private static void HookedGetNativeSystemInfo(IntPtr lpSystemInfo)
        {
            // Call the original GetSystemInfo function
            GetNativeSystemInfo(lpSystemInfo);
            try
            {
                // Cast the IntPtr back to the SYSTEM_INFO struct
                var sysInfo = (SYSTEM_INFO)Marshal.PtrToStructure(lpSystemInfo, typeof(SYSTEM_INFO));

                //Debug.WriteLine("--- HookedGetSystemInfo");

            //WCFClient.Instance.GetPipeProxy.RemoteLog($"before native {lpSystemInfo.ToString()}");
            // Modify the system information as desired
                //sysInfo.wProcessorLevel = short.Parse(_hWSettings.ProcessorLevel);
                //sysInfo.wProcessorRevision = short.Parse(_hWSettings.ProcessorRev, System.Globalization.NumberStyles.HexNumber);
                //sysInfo.dwNumberOfProcessors = Convert.ToInt32(_hWSettings.ProcessorCoreAmount);
                //sysInfo.dwNumberOfProcessors = 12;

                // Marshal the modified struct back to the original memory location
                Marshal.StructureToPtr(sysInfo, lpSystemInfo, fDeleteOld: true);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
        }

        public void Dispose()
        {
            _hook.Dispose();
        }

        #endregion Methods
    }
}
