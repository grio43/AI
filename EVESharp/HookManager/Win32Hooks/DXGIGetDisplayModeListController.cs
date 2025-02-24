using EasyHook;
using SharedComponents.EVE;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System;
using System.Runtime.InteropServices;
using DXGI = SharpDX.DXGI;
using SharedComponents.Utility;
using SharedComponents.Extensions;
using System.Reflection;
using SharedComponents.IPC;
using SharpDX.DXGI;

namespace HookManager.Win32Hooks
{
    public class DXGIGetDisplayModeListController : IHook, IDisposable
    {

        [ComImport, Guid("ae02eedb-c735-4690-8d52-5a8dc20213aa"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IDXGIOutput
        {
            // Define methods for the IDXGIOutput interface here
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ModeDescription
        {
            public int Width;
            public int Height;
            public DXGI.Rational RefreshRate;
            public DXGI.Format Format;
            public DXGI.DisplayModeScanlineOrder ScanlineOrdering;
            public DXGI.DisplayModeScaling Scaling;

            public override string ToString()
            {
                return $"Width: {Width}, Height: {Height}, RefreshRate: {RefreshRate}, Format: {Format}, ScanlineOrdering: {ScanlineOrdering}, Scaling: {Scaling}";
            }
        }

        enum DXGI_FORMAT
        {
            DXGI_FORMAT_UNKNOWN = 0,
            DXGI_FORMAT_R32G32B32A32_TYPELESS = 1,
            DXGI_FORMAT_R32G32B32A32_FLOAT = 2,
            DXGI_FORMAT_R32G32B32A32_UINT = 3,
            DXGI_FORMAT_R32G32B32A32_SINT = 4,
            DXGI_FORMAT_R32G32B32_TYPELESS = 5,
            DXGI_FORMAT_R32G32B32_FLOAT = 6,
            DXGI_FORMAT_R32G32B32_UINT = 7,
            DXGI_FORMAT_R32G32B32_SINT = 8,
            DXGI_FORMAT_R16G16B16A16_TYPELESS = 9,
            DXGI_FORMAT_R16G16B16A16_FLOAT = 10,
            DXGI_FORMAT_R16G16B16A16_UNORM = 11,
            DXGI_FORMAT_R16G16B16A16_UINT = 12,
            DXGI_FORMAT_R16G16B16A16_SNORM = 13,
            DXGI_FORMAT_R16G16B16A16_SINT = 14,
            DXGI_FORMAT_R32G32_TYPELESS = 15,
            DXGI_FORMAT_R32G32_FLOAT = 16,
            DXGI_FORMAT_R32G32_UINT = 17,
            DXGI_FORMAT_R32G32_SINT = 18,
            DXGI_FORMAT_R32G8X24_TYPELESS = 19,
            DXGI_FORMAT_D32_FLOAT_S8X24_UINT = 20,
            DXGI_FORMAT_R32_FLOAT_X8X24_TYPELESS = 21,
            DXGI_FORMAT_X32_TYPELESS_G8X24_UINT = 22,
            DXGI_FORMAT_R10G10B10A2_TYPELESS = 23,
            DXGI_FORMAT_R10G10B10A2_UNORM = 24,
            DXGI_FORMAT_R10G10B10A2_UINT = 25,
            DXGI_FORMAT_R11G11B10_FLOAT = 26,
            DXGI_FORMAT_R8G8B8A8_TYPELESS = 27,
            DXGI_FORMAT_R8G8B8A8_UNORM = 28,
            DXGI_FORMAT_R8G8B8A8_UNORM_SRGB = 29,
            DXGI_FORMAT_R8G8B8A8_UINT = 30,
            DXGI_FORMAT_R8G8B8A8_SNORM = 31,
            DXGI_FORMAT_R8G8B8A8_SINT = 32,
            DXGI_FORMAT_R16G16_TYPELESS = 33,
            DXGI_FORMAT_R16G16_FLOAT = 34,
            DXGI_FORMAT_R16G16_UNORM = 35,
            DXGI_FORMAT_R16G16_UINT = 36,
            DXGI_FORMAT_R16G16_SNORM = 37,
            DXGI_FORMAT_R16G16_SINT = 38,
            DXGI_FORMAT_R32_TYPELESS = 39,
            DXGI_FORMAT_D32_FLOAT = 40,
            DXGI_FORMAT_R32_FLOAT = 41,
            DXGI_FORMAT_R32_UINT = 42,
            DXGI_FORMAT_R32_SINT = 43,
            DXGI_FORMAT_R24G8_TYPELESS = 44,
            DXGI_FORMAT_D24_UNORM_S8_UINT = 45,
            DXGI_FORMAT_R24_UNORM_X8_TYPELESS = 46,
            DXGI_FORMAT_X24_TYPELESS_G8_UINT = 47,
            DXGI_FORMAT_R8G8_TYPELESS = 48,
            DXGI_FORMAT_R8G8_UNORM = 49,
            DXGI_FORMAT_R8G8_UINT = 50,
            DXGI_FORMAT_R8G8_SNORM = 51,
            DXGI_FORMAT_R8G8_SINT = 52,
            DXGI_FORMAT_R16_TYPELESS = 53,
            DXGI_FORMAT_R16_FLOAT = 54,
            DXGI_FORMAT_D16_UNORM = 55,
            DXGI_FORMAT_R16_UNORM = 56,
            DXGI_FORMAT_R16_UINT = 57,
            DXGI_FORMAT_R16_SNORM = 58,
            DXGI_FORMAT_R16_SINT = 59,
            DXGI_FORMAT_R8_TYPELESS = 60,
            DXGI_FORMAT_R8_UNORM = 61,
            DXGI_FORMAT_R8_UINT = 62,
            DXGI_FORMAT_R8_SNORM = 63,
            DXGI_FORMAT_R8_SINT = 64,
            DXGI_FORMAT_A8_UNORM = 65,
            DXGI_FORMAT_R1_UNORM = 66,
            DXGI_FORMAT_R9G9B9E5_SHAREDEXP = 67,
            DXGI_FORMAT_R8G8_B8G8_UNORM = 68,
            DXGI_FORMAT_G8R8_G8B8_UNORM = 69,
            DXGI_FORMAT_BC1_TYPELESS = 70,
            DXGI_FORMAT_BC1_UNORM = 71,
            DXGI_FORMAT_BC1_UNORM_SRGB = 72,
            DXGI_FORMAT_BC2_TYPELESS = 73,
            DXGI_FORMAT_BC2_UNORM = 74,
            DXGI_FORMAT_BC2_UNORM_SRGB = 75,
            DXGI_FORMAT_BC3_TYPELESS = 76,
            DXGI_FORMAT_BC3_UNORM = 77,
            DXGI_FORMAT_BC3_UNORM_SRGB = 78,
            DXGI_FORMAT_BC4_TYPELESS = 79,
            DXGI_FORMAT_BC4_UNORM = 80,
            DXGI_FORMAT_BC4_SNORM = 81,
            DXGI_FORMAT_BC5_TYPELESS = 82,
            DXGI_FORMAT_BC5_UNORM = 83,
            DXGI_FORMAT_BC5_SNORM = 84,
            DXGI_FORMAT_B5G6R5_UNORM = 85,
            DXGI_FORMAT_B5G5R5A1_UNORM = 86,
            DXGI_FORMAT_B8G8R8A8_UNORM = 87,
            DXGI_FORMAT_B8G8R8X8_UNORM = 88,
            DXGI_FORMAT_R10G10B10_XR_BIAS_A2_UNORM = 89,
            DXGI_FORMAT_B8G8R8A8_TYPELESS = 90,
            DXGI_FORMAT_B8G8R8A8_UNORM_SRGB = 91,
            DXGI_FORMAT_B8G8R8X8_TYPELESS = 92,
            DXGI_FORMAT_B8G8R8X8_UNORM_SRGB = 93,
            DXGI_FORMAT_BC6H_TYPELESS = 94,
            DXGI_FORMAT_BC6H_UF16 = 95,
            DXGI_FORMAT_BC6H_SF16 = 96,
            DXGI_FORMAT_BC7_TYPELESS = 97,
            DXGI_FORMAT_BC7_UNORM = 98,
            DXGI_FORMAT_BC7_UNORM_SRGB = 99,
            DXGI_FORMAT_AYUV = 100,
            DXGI_FORMAT_Y410 = 101,
            DXGI_FORMAT_Y416 = 102,
            DXGI_FORMAT_NV12 = 103,
            DXGI_FORMAT_P010 = 104,
            DXGI_FORMAT_P016 = 105,
            DXGI_FORMAT_420_OPAQUE = 106,
            DXGI_FORMAT_YUY2 = 107,
            DXGI_FORMAT_Y210 = 108,
            DXGI_FORMAT_Y216 = 109,
            DXGI_FORMAT_NV11 = 110,
            DXGI_FORMAT_AI44 = 111,
            DXGI_FORMAT_IA44 = 112,
            DXGI_FORMAT_P8 = 113,
            DXGI_FORMAT_A8P8 = 114,
            DXGI_FORMAT_B4G4R4A4_UNORM = 115,
            DXGI_FORMAT_P208 = 130,
            DXGI_FORMAT_V208 = 131,
            DXGI_FORMAT_V408 = 132,
            DXGI_FORMAT_SAMPLER_FEEDBACK_MIN_MIP_OPAQUE,
            DXGI_FORMAT_SAMPLER_FEEDBACK_MIP_REGION_USED_OPAQUE,
        };

        #region Fields

        private LocalHook _hook;

        #endregion Fields

        #region Constructors

        public DXGIGetDisplayModeListController(HWSettings hWSettings)
        {
            Error = false;
            Name = typeof(DXGIGetDisplayModeListController).Name;

            try
            {
                var modeListPtr = GetGetDisplayModeListPointer();
                if (modeListPtr != IntPtr.Zero)
                {
                    WCFClient.Instance.GetPipeProxy.RemoteLog($"Offset {modeListPtr: X}");
                    Util.GlobalRemoteLog($"Offset {modeListPtr:X}");
                    if (modeListPtr != IntPtr.Zero)
                    {
                        _original = (Delegate)Marshal.GetDelegateForFunctionPointer(modeListPtr, typeof(Delegate));
                        WCFClient.Instance.GetPipeProxy.RemoteLog("_original = (Delegate)Marshal.GetDelegateForFunctionPointer(modeListPtr, typeof(Delegate));");
                        _hook = LocalHook.Create(modeListPtr, new Delegate(Detour), this);
                        WCFClient.Instance.GetPipeProxy.RemoteLog("_hook = LocalHook.Create(modeListPtr, new Delegate(Detour), this);");
                        _hook.ThreadACL.SetExclusiveACL(new Int32[] { });
                        WCFClient.Instance.GetPipeProxy.RemoteLog("_hook.ThreadACL.SetExclusiveACL(new Int32[] { });");
                        Error = false;
                    }
                    else
                    {
                        Error = true;
                        WCFClient.Instance.GetPipeProxy.RemoteLog("if (modeListPtr == IntPtr.Zero)");
                    }
                }
                else
                {
                    Error = true;
                    WCFClient.Instance.GetPipeProxy.RemoteLog("if (offset == IntPtr.Zero)");
                }
            }
            catch (Exception ex)
            {
                Error = true;
                WCFClient.Instance.GetPipeProxy.RemoteLog(ex.ToString());
            }
            HWSettings = hWSettings;
        }

        #endregion Constructors

        #region Delegates

        #endregion Delegates

        #region Properties

        public bool Error { get; set; }
        public string Name { get; set; }
        private static HWSettings HWSettings { get; set; }

        #endregion Properties

        #region Methods

        static unsafe IntPtr GetModeListOffset()
        {
            try
            {
                Util.LoadLibrary("dxgi.dll", "");
                var dxgiHandle = Util.GetModuleHandle("dxgi.dll");
                var dxgiFactory = new DXGI.Factory2();
                WCFClient.Instance.GetPipeProxy.RemoteLog("var dxgiFactory = new DXGI.Factory2();");
                var adapter = dxgiFactory.Adapters1[0];
                WCFClient.Instance.GetPipeProxy.RemoteLog("var adapter = dxgiFactory.Adapters1[0];");
                var output = adapter.GetOutput(0);
                WCFClient.Instance.GetPipeProxy.RemoteLog("var output = adapter.GetOutput(0);");
                var nativePtr = output.GetFieldValue<System.Reflection.Pointer>("_nativePointer");
                WCFClient.Instance.GetPipeProxy.RemoteLog("var nativePtr = output.GetFieldValue<System.Reflection.Pointer>(_nativePointer);");
                var displayModeListOffset = (IntPtr)((*(IntPtr*)(IntPtr*)((byte*)*(IntPtr*)Pointer.Unbox(nativePtr) + 8 * IntPtr.Size)).ToInt64() - dxgiHandle.ToInt64());
                WCFClient.Instance.GetPipeProxy.RemoteLog("var displayModeListOffset = (IntPtr)((*(IntPtr*)(IntPtr*)((byte*)*(IntPtr*)Pointer.Unbox(nativePtr) + 8 * IntPtr.Size)).ToInt64() - dxgiHandle.ToInt64());");
                //Console.WriteLine($"{displayModeListOffset.ToInt64():X}");
                WCFClient.Instance.GetPipeProxy.RemoteLog("displayModeListOffset [" + displayModeListOffset.ToInt64().ToString() + "]");
                return displayModeListOffset;
            }
            catch (Exception ex)
            {
                WCFClient.Instance.GetPipeProxy.RemoteLog(ex.ToString());
                return new IntPtr();
            }

        }

        public static unsafe IntPtr GetGetDisplayModeListPointer()
        {
            Util.LoadLibrary("dxgi.dll", "");
            var dxgiHandle = Util.GetModuleHandle("dxgi.dll");
            using (var factory = new Factory1())
            using (var adapter = factory.GetAdapter1(0))
            using (var output = adapter.GetOutput(0))
            {
                IntPtr outputPtr = output.NativePointer; // native com ptr
                IntPtr* vtable = *(IntPtr**)outputPtr.ToPointer(); // vtable
                return (IntPtr)((Int64)vtable[8]); // index 8 == GetGetDisplayMode
            }
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Auto)]
        private delegate int Delegate(IntPtr y, DXGI_FORMAT format, uint flags, IntPtr numModes, IntPtr modes);

        private static Delegate _original;


        public enum DXGI_MODE_SCANLINE_ORDER
        {
            DXGI_MODE_SCANLINE_ORDER_UNSPECIFIED = 0,
            DXGI_MODE_SCANLINE_ORDER_PROGRESSIVE = 1,
            DXGI_MODE_SCANLINE_ORDER_UPPER_FIELD_FIRST = 2,
            DXGI_MODE_SCANLINE_ORDER_LOWER_FIELD_FIRST = 3
        }


        public enum DXGI_MODE_SCALING
        {
            DXGI_MODE_SCALING_UNSPECIFIED = 0,
            DXGI_MODE_SCALING_CENTERED = 1,
            DXGI_MODE_SCALING_STRETCHED = 2
        }


        // This is only a partial structure, only the fields we need are defined
        public struct DXGI_MODE_DESC
        {
            UInt32 Width;
            UInt32 Height;
            DXGI_RATIONAL RefreshRate;
            DXGI_FORMAT format;
            DXGI_MODE_SCANLINE_ORDER scanlineOrder;
            DXGI_MODE_SCALING modeScaling;

            public override string ToString()
            {
                return $"Width: {Width}, Height: {Height}, RefreshRate: {RefreshRate.Numerator}/{RefreshRate.Denominator}, Format: {format}, ScanlineOrder: {scanlineOrder}, Scaling: {modeScaling}";
            }
        }



        public struct DXGI_RATIONAL
        {
            public UInt32 Numerator;
            public UInt32 Denominator;
        }


        public void Dispose()
        {
            _hook.Dispose();
        }

        private static int Detour(IntPtr y, DXGI_FORMAT format, uint flags, IntPtr numModes, IntPtr modes)
        {
            var orig = _original(y, format, flags, numModes, modes);
            var numberOfModes = Marshal.ReadInt32(numModes);

            if (modes == IntPtr.Zero)
            {
                return orig;
            }

            for (int i = 0; i < numberOfModes; i++)
            {
                IntPtr strucPtr = new IntPtr(modes.ToInt64() + i * Marshal.SizeOf(typeof(ModeDescription)));
                var mode = Marshal.PtrToStructure<ModeDescription>(strucPtr);
                //Util.GlobalRemoteLog($"Detour Mode [{mode}]");
                mode.RefreshRate.Numerator = HWSettings.MonitorRefreshrate * 1000;
                mode.RefreshRate.Denominator = 1000;
                Marshal.StructureToPtr(mode, strucPtr, true);
            }
            //Util.GlobalRemoteLog($"Detour DXGI_FORMAT [{format}] numberOfModes {numberOfModes}");

            return (int)orig;
        }

        #endregion Methods
    }
}