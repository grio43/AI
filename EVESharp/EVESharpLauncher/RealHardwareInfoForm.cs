using SharedComponents.EVE;
using SharpDX.Direct3D9;
using SharpDX.DXGI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Device = SharpDX.Direct3D9.Device;
using PresentParameters = SharpDX.Direct3D9.PresentParameters;

namespace EVESharpLauncher
{
    public partial class RealHardwareInfoForm : Form
    {
        #region Constructors

        public RealHardwareInfoForm()
        {
            InitializeComponent();
        }

        #endregion Constructors

        #region Delegates

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate UInt32 GetAdapterIdentifierDelegate([In] UInt32 Adapter, [In] UInt64 Flags, [Out] [In] IntPtr pIdentifier);

        #endregion Delegates

        #region Methods

        [DllImport("d3d9.dll")]
        private static extern IntPtr Direct3DCreate9(uint sdkVersion);

        private Dictionary<string, string> GetDx11Info()
        {
            var ret = new Dictionary<string, string>();

            Factory f = new Factory1();
            var a = f.GetAdapter(0);

            var deviceLUID = a.Description.Luid.ToString().Trim().Replace(Environment.NewLine, "");
            var deviceId = a.Description.DeviceId.ToString().Trim().Replace(Environment.NewLine, "");
            var deviceDesc = a.Description.Description.ToString().Trim().Replace(Environment.NewLine, "");
            var revision = a.Description.Revision.ToString().Trim().Replace(Environment.NewLine, "");
            var vendorId = a.Description.VendorId.ToString().Trim().Replace(Environment.NewLine, "");
            var subsystemId = a.Description.SubsystemId.ToString().Trim().Replace(Environment.NewLine, "");

            ret.Add("DEVICE_ID", deviceId);
            ret.Add("DEVICE_LUID", deviceLUID);
            ret.Add("RIVISION", revision);
            ret.Add("VENDOR_ID", vendorId);
            ret.Add("DESCRIPTION", deviceDesc);
            ret.Add("SUBSYSTEM_ID", subsystemId);
            return ret;
        }

        private Dictionary<string, string> GetDx9Info()
        {
            var ret = new Dictionary<string, string>();

            var device = new Device(new Direct3D(), 0, DeviceType.Hardware, Handle, CreateFlags.HardwareVertexProcessing,
                new PresentParameters(ClientSize.Width, ClientSize.Height));
            var deviceIdentifier = device.Direct3D.GetAdapterIdentifier(0).DeviceIdentifier.ToString().Trim().Replace(Environment.NewLine, "");
            var deviceId = device.Direct3D.GetAdapterIdentifier(0).DeviceId.ToString().Trim().Replace(Environment.NewLine, "");
            var deviceDesc = device.Direct3D.GetAdapterIdentifier(0).Description.ToString().Trim().Replace(Environment.NewLine, "");
            var driverVersion = device.Direct3D.GetAdapterIdentifier(0).DriverVersion.ToString().Trim().Replace(Environment.NewLine, "");
            var revision = device.Direct3D.GetAdapterIdentifier(0).Revision.ToString().Trim().Replace(Environment.NewLine, "");
            var vendorId = device.Direct3D.GetAdapterIdentifier(0).VendorId.ToString().Trim().Replace(Environment.NewLine, "");

            ret.Add("DEVICE_ID", deviceId);
            ret.Add("DEVICE_IDENTIFIER", deviceIdentifier);
            ret.Add("DRIVER_VERSION", driverVersion);
            ret.Add("RIVISION", revision);
            ret.Add("VENDOR_ID", vendorId);
            ret.Add("DESCRIPTION", deviceDesc);

            //            Debug.WriteLine($"{deviceId} {deviceIdentifier} {driverVersion} {revision} {vendorId} {deviceDesc}");

            return ret;

            //            var direct3D = Direct3DCreate9(32);
            //
            //            if (direct3D == IntPtr.Zero)
            //            {
            //                MessageBox.Show("error");
            //                throw new Exception("Failed to create D3D.");
            //            }
            //
            //            var adapterIdentPtr = Marshal.ReadIntPtr(Marshal.ReadIntPtr(direct3D), 20);
            //
            //            GetAdapterIdentifierOriginal =
            //                (GetAdapterIdentifierDelegate)Marshal.GetDelegateForFunctionPointer(adapterIdentPtr, typeof(GetAdapterIdentifierDelegate));

            // Initialize unmanged memory to hold the struct.
            //__Native tmp = new __Native();
            //IntPtr d3d_ident_struc = Marshal.AllocHGlobal(Marshal.SizeOf(tmp));

            //Marshal.StructureToPtr(tmp, d3d_ident_struc, true);

            //var result = GetAdapterIdentifierOriginal(0, 0, x);

            //            var result_strict = (D3DADAPTER_IDENTIFIER9)Marshal.PtrToStructure(x, typeof(D3DADAPTER_IDENTIFIER9));
            //            var before = string.Format("[BEFORE] [DESC] {0} [DEVICE_ID] {1} [IDENT] {2} [DRIVER_VER] {3} [REVISION] {4} [VENDOR_ID] {5}",
            //                result_strict.Description, result_strict.DeviceId.ToString(), result_strict.DeviceIdentifier.ToString(),
            //                ((long)result_strict.DriverVersion.QuadPart).ToString(), result_strict.Revision.ToString(), result_strict.VendorId.ToString());

            //            Debug.WriteLine(before);
        }

        private Dictionary<string, string> GetEnvVars()
        {
            var ret = new Dictionary<string, string>();
            foreach (DictionaryEntry kv in Environment.GetEnvironmentVariables())
                if (kv.Key.ToString().StartsWith("USERNAME") ||
                    kv.Key.ToString().StartsWith("COMPUTERNAME") ||
                    kv.Key.ToString().StartsWith("USERDOMAIN") ||
                    kv.Key.ToString().StartsWith("USERDOMAIN_ROAMINGPROFILE") ||
                    kv.Key.ToString().StartsWith("TMP") ||
                    kv.Key.ToString().StartsWith("VISUALSTUDIODIR") ||
                    kv.Key.ToString().StartsWith("PROCESSOR_IDENTIFIER") ||
                    kv.Key.ToString().StartsWith("PROCESSOR_REVISION") ||
                    kv.Key.ToString().StartsWith("NUMBER_OF_PROCESSORS") ||
                    kv.Key.ToString().StartsWith("PROCESSOR_LEVEL") ||
                    kv.Key.ToString().StartsWith("USERPROFILE") ||
                    kv.Key.ToString().StartsWith("HOMEPATH") ||
                    kv.Key.ToString().StartsWith("LOCALAPPDATA") ||
                    kv.Key.ToString().StartsWith("TEMP") ||
                    kv.Key.ToString().StartsWith("APPDATA") ||
                    kv.Key.ToString().StartsWith("PATH")
                )
                    ret.Add(kv.Key.ToString(), kv.Value.ToString());
            return ret;
        }

        private void RealHardwareInfo_Shown(object sender, EventArgs e)
        {
            var s = String.Empty;
            s += "------------Environment------------" + Environment.NewLine;
            s += string.Join(Environment.NewLine, GetEnvVars().Select(x => x.Key + "=" + x.Value)) + Environment.NewLine + Environment.NewLine;
            s += "------------DirectX9----------------" + Environment.NewLine;
            s += string.Join(Environment.NewLine, GetDx9Info().Select(x => x.Key + "=" + x.Value)) + Environment.NewLine + Environment.NewLine;
            s += "------------DirectX11----------------" + Environment.NewLine;
            s += string.Join(Environment.NewLine, GetDx11Info().Select(x => x.Key + "=" + x.Value)) + Environment.NewLine + Environment.NewLine;
            s += "------------EVELauncher--------------" + Environment.NewLine;
            s += "MachineHashMD5=" + LauncherHash.GetLauncherHash().Item2 + Environment.NewLine;
            s += "CCPMagicEncodedHash=" + LauncherHash.GetLauncherHash().Item3 + Environment.NewLine;
            textBox1.Text = s;

            GetDx9Info();
        }

        #endregion Methods

        #region Structs

        [StructLayout(LayoutKind.Sequential)]
        public struct _GUID
        {
            public Int32 Data1;
            public Int16 Data2;
            public Int16 Data3;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] Data4;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct D3DADAPTER_IDENTIFIER9
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 512)]
            public string Driver;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 512)]
            public string Description;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string DeviceName;

            [MarshalAs(UnmanagedType.Struct)] public LARGE_INTEGER DriverVersion;
            [MarshalAs(UnmanagedType.U4)] public UInt32 VendorId;
            [MarshalAs(UnmanagedType.U4)] public UInt32 DeviceId;
            [MarshalAs(UnmanagedType.U4)] public UInt32 SubSysId;
            [MarshalAs(UnmanagedType.U4)] public UInt32 Revision;
            [MarshalAs(UnmanagedType.Struct)] public Guid DeviceIdentifier;
            [MarshalAs(UnmanagedType.U4)] public UInt32 WHQLLevel;
        }

        [StructLayout(LayoutKind.Explicit, Size = 8)]
        public struct LARGE_INTEGER
        {
            [FieldOffset(0)] public Int64 QuadPart;
            [FieldOffset(0)] public UInt32 LowPart;
            [FieldOffset(4)] public UInt32 HighPart;
        }

        #endregion Structs
    }
}