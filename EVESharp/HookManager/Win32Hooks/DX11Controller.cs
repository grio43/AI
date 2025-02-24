/*
 * ---------------------------------------
 * User: duketwo
 * Date: 01.08.2018
 * Time: 03:23
 * ---------------------------------------
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using EasyHook;
using SharedComponents.EVE;
using SharedComponents.IPC;
using SharedComponents.Utility;
using Utility;

namespace HookManager.Win32Hooks
{
    /// <summary>
    ///     Description of DX11Controller.
    /// </summary>
    public class DX11Controller : IDisposable, IHook
    {
        [DllImport("d3d11.dll")]
        private static extern unsafe int D3D11CreateDeviceAndSwapChain(IntPtr pAdapter, int driverType, IntPtr Software,
          int flags, IntPtr pFeatureLevels,
          int FeatureLevels, int SDKVersion,
          void* pSwapChainDesc, [Out] out IntPtr ppSwapChain,
          [Out] out IntPtr ppDevice, [Out] out IntPtr pFeatureLevel,
          [Out] out IntPtr ppImmediateContext);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void D3DVirtVoid(IntPtr instance);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int Direct3D11Present(IntPtr swapChainPtr, int syncInterval, int flags);

        [StructLayout(LayoutKind.Sequential)]
        public struct SwapChainDescription
        {
            public ModeDescription ModeDescription;
            public SampleDescription SampleDescription;
            public int Usage;
            public int BufferCount;
            public IntPtr OutputHandle;
            [MarshalAs(UnmanagedType.Bool)] public bool IsWindowed;
            public int SwapEffect;
            public int Flags;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Rational
        {
            public int Numerator;
            public int Denominator;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ModeDescription
        {
            public int Width;
            public int Height;
            public Rational RefreshRate;
            public int Format;
            public int ScanlineOrdering;
            public int Scaling;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SampleDescription
        {
            public int Count;
            public int Quality;
        }

        private LocalHook _hook;
        private HWSettings _settings;
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate uint GetDesc1Delegate(IntPtr index, IntPtr adapter);
        //private int getDescOffset = 0x9200;

        private GetDesc1Delegate GetDesc1Original;

        //
        // An example _dxgiOffsets entry for clarity:
        // { "2dc2df394ae60bb84e480e3e14256903", 0x9200 },
        // 2dc2df394ae60bb84e480e3e14256903 is the md5 checksum of the dxgi.dll file
        // every version of the dxgi.dll file will have a different md5 checksum.
        // The version varies based on Windows version and patch level.
        // 0x9200 is the offset in that particular file to the function named: CDXGIAdapter::GetDesc1(DXGI_ADAPTER_DESC1*)
        //
        // calculating the offsets in dxgi.dll took me a bit to work out how to accomplish.
        // These are notes you may find useful if you need to add a new dxgi.dll offset
        // when the dxgi.dll file gets updated by Microsoft as part of them updating DirectX
        //
        //Requires IDA (free): https://www.hex-rays.com/products/ida/support/download_freeware/
        // Open IDA: Load (a copy of) dxgi.dll (original lives in c:\windows\system32\dxgi.dll)
        // Load it as a Portable executable for AMD64 (NOT Binary!): rest of the options can be default
        // In the left functions window sort by Function Name
        // look through the Function Name list for: CDXGIAdapter::GetDesc1(DXGI_ADAPTER_DESC1*)
        // Double click it and the IDA View-A tab should show the function loaded.
        // You will see:
        //?GetDesc1 @CDXGIAdapter@@UEAAJPEAUDXGI_ADAPTER_DESC1@@@Z
        // in the first (topmost) box
        // Change the tab/view to the Hex View-1 tab
        // the highlighted line will be the entry point for that function.
        // in most cases you need to note the last 4 or 5 characters in the
        // left column matching the syntax of the data below: for example: 9200 becomes 0x9200
        //
        // the other info you need is the md5 sum of the dxgi.dll file you can get that many ways
        // the easiest being one of the many websites that will calculate it for you if you upload the file.
        // http://onlinemd5.com/
        // of course there any also many ways to calculate it locally: https://www.winmd5.com/
        //
        //

        /**
        // Read the CSV file
            using (StreamReader reader = new StreamReader("offsets.csv"))
            {
                while (!reader.EndOfStream)
                {
                    // Read a line from the CSV file
                    string line = reader.ReadLine();

                    // Split the line into an array of strings
                    string[] values = line.Split(',');

                    // Store the values in the dictionary
                    dxgiOffsets[values[0]] = int.Parse(values[1]);
}
            }
        **/

        private static readonly Dictionary<string, int> _dxgiOffsets = new Dictionary<string, int>(){
            { "2dc2df394ae60bb84e480e3e14256903".ToLower(), 0x9200 },
            { "fe386753853bc8a910726e938aac3b99".ToLower(), 0x95B0 },
            { "5c419ab64d8753e9f2fca21ea801b976".ToLower(), 0x87E0 },
            { "1cb14c76f967e5279779c19bdcc2c3a2".ToLower(), 0xF420 },
            { "e89cf605264b491c7b5040d94291f915".ToLower(), 0x21DC0 },
            { "b1492bdad8d4ce701d9198e66650ed14".ToLower(), 0x1C920 },
            { "9e85ba32728294a61b63799a3cf57471".ToLower(), 0x2ED0 }, //Windows 2012 R2
            { "ae299b218772ce03dfa7449169b47149".ToLower(), 0x1CCB0 }, //Windows 10 - 10.0.18362.693
            { "02712dc0fd3c21a143678fe4b8594f09".ToLower(), 0x1CC80 }, //Windows 10 - 10.0.18362.1316
            { "453eab88962e44562020ebd490b42007".ToLower(), 0x9B40 },
            { "54c366919d6243df93bd7f8f97ad7fae".ToLower(), 0x8CE0 }, //Windows 10 - 10.0.19041.541
            { "3edd65ee32ec6f94955bf18417da1707".ToLower(), 0x0F84 },  //Windows 10 - 10.0.18362.815
            { "b98af6c470b3186d530910c3910f0603".ToLower(), 0x8CE0 }, //Windows 10 - 10.0.19041.1
            { "6c1a1404ab50140afdd1a763fd782281".ToLower(), 0x1CC80},
            { "399fbbe83d62e0512512474773b5526f".ToLower(), 0x9f3f3 },
            { "5994c21a736c23eb86f828cb07318495".ToLower(), 0x9740 }, //Windows 10 - 10.0.19041.964
            { "354cdc55a153c7c63a11bc8eebc2aad1".ToLower(), 0x1D560 }, //Windows 10 - 10.0.19041.1266
            { "5e9aa83fceb2cd95265fdb67c6996d00".ToLower(), 0x1CB00 }, //Windows 10 - 10.0.19041.1566
            { "2085b48a6ed171df02464ed37814b441".ToLower(), 0x1E040 }, //Windows 10 - 10.0.19041.2075

            { "b1d378f38dc2fe914fd34aa8884792d9".ToLower(),  0x1C710 }, // b1d378f38dc2fe914fd34aa8884792d9;0x1C710
            { "fd9ded09e33cd6a80398bcf4caf22e9e".ToLower(),  0x1DB40 },
            { "2caae1e040bb68689f7fdb84b31e3cf0".ToLower(),  0x7BD0 }, // 2caae1e040bb68689f7fdb84b31e3cf0;0x7BD0

            { "b60e4efaba67ce7f867aa5c2a9cfc66d".ToLower(), 0x95B0 }, //11-24-3019
            { "3c32d763740c83db2c44dea4b6f18c54".ToLower(), 0x9200 },
            { "3cccdef0c50f0aa4f8e01d0187b1d565".ToLower(), 0x1DB40}, //12-2022
            { "02b331c9d6e5eeff79f9d32e546f0c7d".ToLower(), 0x1E1E0}, //12-2022
            { "fd398c164aa4e89feb9bae6746931484".ToLower(), 0x1D200}, //12-2022
            { "80040903bd2759543c546bb3bf4929ba".ToLower(), 0x23B80}, //7-2023
            { "203bf6d095ca67b2ee9a2f490b9327c9".ToLower(), 0x1D930}, //9-2023
            { "ffc27ec0cfb55a6c763a4549f283232c".ToLower(), 0x1E1E0}, //10-2023
            { "c9718023e250867031c1c3eaffdd0363".ToLower(), 0x1E1E0}, //3-2024
            { "68a100379f65db180379c70ed4fed9d4".ToLower(), 0x1DAF0}, //5-2024
            { "2c2bdd1861252fa760f9cfecfa5738a8".ToLower(), 0x1DC00}, //5-2024
            { "66d75d06bbeeefe595047da2e0ad2422".ToLower(), 0x1DC20}, //7-2024
            { "8fa9f4beba69b582d3f71a5faafc1cc9".ToLower(), 0x1DC20}, //1-2025
            { "2302f8adcd715a7fb1315c5147e468a8".ToLower(), 0x480E0}, //2-2025
        };

        public unsafe DX11Controller(HWSettings settings)
        {
            _settings = settings;
            Name = typeof(DX11Controller).Name;
            try
            {
                var dxgiHandle = Util.GetModuleHandle("dxgi.dll");

                var filePath = @"C:\Windows\System32\dxgi.dll";

                //var filePathImport = @"C:\dxgi\dxgi.dll";
                //if (File.Exists(filePathImport))
                //{
                //    var bBytes = File.ReadAllBytes(filePathImport);
                //    var sigScan = new SigScan();
                //    var res = sigScan.FindPatternInByArray(bBytes, new byte[] { 0x45, 0x33, 0xC9, 0x48, 0x85, 0xD2, 0x0F, 0x84, 0xDE, 0xBD, 0x01, 0x00, 0x8B, 0x81, 0xC4, 0x02, 0x00, 0x00, 0x83, 0xF8, 0xFF, 0x0F, 0x84, 0xC3, 0xBD, 0x01, 0x00, 0x4C, 0x69, 0xC0, 0xC8, 0x01 }, "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", 0x0);
                //    Util.GlobalRemoteLog($"C:\\dxgi\\dxgi.dll pattern off: [{res}]");
                //}

                if (!File.Exists(filePath))
                    throw new Exception("dxgi.dll could not be found?");
                var md5Dxgi = Util.CalculateMD5(filePath).ToLower();

                if (!_dxgiOffsets.ContainsKey(md5Dxgi))
                {
                    var ptr = SigScan.FindDxgiDesc1Offset();
                    HookManagerImpl.Log($"Error. Unknown dxgi.dll. Possible offset: [{ptr}] MD5 [" + md5Dxgi + "]");
                    WCFClient.Instance.GetPipeProxy.RemoteLog("DX11Controller Error. ---------------------------------------------------------------");
                    WCFClient.Instance.GetPipeProxy.RemoteLog("DX11Controller Error. ---------------------------------------------------------------");
                    WCFClient.Instance.GetPipeProxy.RemoteLog("DX11Controller Error. dxgi.dll NOT hooked! Unknown dxgi.dll. Possible offset: [{ptr}] MD5 [" + md5Dxgi + "]");
                    WCFClient.Instance.GetPipeProxy.RemoteLog("DX11Controller Error. ---------------------------------------------------------------");
                    WCFClient.Instance.GetPipeProxy.RemoteLog("DX11Controller Error. ---------------------------------------------------------------");
                    throw new Exception($"Error. Unknown dxgi.dll. Possible offset: [{ptr}] MD5 [" + md5Dxgi + "]");
                }

                var getDescPtr = dxgiHandle + _dxgiOffsets[md5Dxgi];

                //var form = new Form();
                //var chainDescription = new SwapChainDescription()
                //{
                //    BufferCount = 1,
                //    ModeDescription = new ModeDescription()
                //    {
                //        Format = 28
                //    },
                //    Usage = 32,
                //    OutputHandle = form.Handle,
                //    SampleDescription = new SampleDescription()
                //    {
                //        Count = 1
                //    },
                //    IsWindowed = true
                //};

                //var ppSwapChain = IntPtr.Zero;
                //var ppDevice = IntPtr.Zero;
                //var ppImmediateContext = IntPtr.Zero;
                //var pFeatureLevel = IntPtr.Zero;
                //var ret = D3D11CreateDeviceAndSwapChain(IntPtr.Zero, 1, IntPtr.Zero, 0, IntPtr.Zero, 0, 7, &chainDescription, out ppSwapChain, out ppDevice,
                //    out pFeatureLevel, out ppImmediateContext);

                //var ptrDesc1 = Marshal.ReadIntPtr(Marshal.ReadIntPtr(ppSwapChain), (int)DXGI.AddressIndicies.GetDesc * IntPtr.Size);

                //var d3dChain = (D3DVirtVoid)Marshal.GetDelegateForFunctionPointer(Marshal.ReadIntPtr(Marshal.ReadIntPtr(ppSwapChain), 8), typeof(D3DVirtVoid));
                //var d3dDevicex = (D3DVirtVoid)Marshal.GetDelegateForFunctionPointer(Marshal.ReadIntPtr(Marshal.ReadIntPtr(ppDevice), 8), typeof(D3DVirtVoid));
                //var d3dContext =
                //    (D3DVirtVoid)Marshal.GetDelegateForFunctionPointer(Marshal.ReadIntPtr(Marshal.ReadIntPtr(ppImmediateContext), 8), typeof(D3DVirtVoid));
                //d3dChain(ppSwapChain);
                //d3dDevicex(ppDevice);
                //d3dContext(ppImmediateContext);

                GetDesc1Original = (GetDesc1Delegate)Marshal.GetDelegateForFunctionPointer(getDescPtr, typeof(GetDesc1Delegate));
                _hook = LocalHook.Create(getDescPtr, new GetDesc1Delegate(GetDesc1Detour), this);
                _hook.ThreadACL.SetExclusiveACL(new Int32[] { 0 });
            }
            catch (Exception ex)
            {
                WCFClient.Instance.GetPipeProxy.RemoteLog("Exception: " + ex.ToString());
                Util.GlobalRemoteLog("Exception: " + ex.ToString());
                HookManagerImpl.Log("Exception: " + ex.ToString());
                Error = true;
            }
        }

        public void Dispose()
        {
            if (_hook == null)
                return;

            _hook.Dispose();
            _hook = null;
        }

        public bool Error { get; set; }
        public string Name { get; set; }

        private uint GetDesc1Detour(IntPtr index, IntPtr adapter)
        {

            var result = GetDesc1Original(index, adapter);
            //Console.WriteLine($"index {index} Ptr {adapter}");
            //HookManagerImpl.Log($"index {index} Ptr {adapter}");
            if (result == 0)
            {
                var structure = (DXGI_ADAPTER_DESC1)Marshal.PtrToStructure(adapter, typeof(DXGI_ADAPTER_DESC1));
                HookManagerImpl.Log($"[BEFORE] [DESC] {structure.Description} [DEVICE_ID] {structure.DeviceId} [REV] {structure.Revision} [VENDOR_ID] {structure.VendorId} [LUID] {structure.AdapterLuid.LowPart + " " + structure.AdapterLuid.HighPart}", Color.Orange);
                structure.Description = _settings.GpuDescription;
                structure.DeviceId = _settings.GpuDeviceId;
                structure.Revision = _settings.GpuRevision;
                structure.VendorId = _settings.GpuVendorId;
                Marshal.StructureToPtr(structure, adapter, true);
                var structureAfter = (DXGI_ADAPTER_DESC1)Marshal.PtrToStructure(adapter, typeof(DXGI_ADAPTER_DESC1));
                HookManagerImpl.Log($"[AFTER] [DESC] {structureAfter.Description} [DEVICE_ID] {structureAfter.DeviceId} [REV] {structureAfter.Revision} [VENDOR_ID] {structureAfter.VendorId} [LUID] {structure.AdapterLuid.LowPart + " " + structure.AdapterLuid.HighPart}", Color.Orange);
            }
            else
            {
                WCFClient.Instance.GetPipeProxy.RemoteLog("DXGI_ADAPTER_DESC1 error.");
                WCFClient.Instance.GetPipeProxy.SetEveAccountAttributeValue(WCFClient.Instance.GUID,
                    nameof(EveAccount.UseScheduler), false);
                Environment.Exit(0);
                Environment.FailFast("exit"); // shouldn't reach the code below, failfast terminates instantly.
            }
            return result;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 1)]
        public struct DXGI_ADAPTER_DESC1
        {
            /// <summary>
            ///     A string that contains the adapter description.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public String Description;

            /// <summary>
            ///     The PCI ID of the hardware vendor.
            /// </summary>
            public UInt32 VendorId;

            /// <summary>
            ///     The PCI ID of the hardware device.
            /// </summary>
            public UInt32 DeviceId;

            /// <summary>
            ///     The PCI ID of the sub system.
            /// </summary>
            public UInt32 SubSysId;

            /// <summary>
            ///     The PCI ID of the revision number of the adapter.
            /// </summary>
            public UInt32 Revision;

            /// <summary>
            ///     The number of bytes of dedicated video memory that are not shared with the CPU.
            /// </summary>
            public IntPtr DedicatedVideoMemory;

            /// <summary>
            ///     The number of bytes of dedicated system memory that are not shared with the GPU. This memory is allocated from
            ///     available system memory at boot time.
            /// </summary>
            public IntPtr DedicatedSystemMemory;

            /// <summary>
            ///     The number of bytes of shared system memory. This is the maximum value of system memory that may be consumed by the
            ///     adapter during operation.
            ///     Any incidental memory consumed by the driver as it manages and uses video memory is additional.
            /// </summary>
            public IntPtr SharedSystemMemory;

            /// <summary>
            ///     A unique value that identifies the adapter. See <see cref="LUID" /> for a definition of the structure.
            /// </summary>
            public LUID AdapterLuid;

            /// <summary>
            ///     A member of the <see cref="DXGI_ADAPTER_FLAG" /> enumerated type that describes the adapter type.
            ///     The <see cref="DXGI_ADAPTER_FLAG.DXGI_ADAPTER_FLAG_REMOTE" /> flag specifies that the adapter is a remote adapter.
            /// </summary>
            public DXGI_ADAPTER_FLAG Flags;
        }

        public struct LUID
        {
            public UInt32 LowPart;
            public Int32 HighPart;
        }

        public enum DXGI_ADAPTER_FLAG : uint
        {
            DXGI_ADAPTER_FLAG_NONE = 0,
            DXGI_ADAPTER_FLAG_REMOTE = 1,
        }
    }
}