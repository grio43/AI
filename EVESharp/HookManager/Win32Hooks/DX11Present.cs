using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using EasyHook;
using SharedComponents.IPC;
using SharedComponents.Py;

namespace HookManager.Win32Hooks
{
    public class DX11Present : IHook
    {
        private Direct3D11Present _presentDelegate;
        private LocalHook Hook;
        private bool _pyHooksInitialized { get; set; }


        public DX11Present()
        {
            try
            {
                Initialize();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                this.Error = true;
            }

        }

        public unsafe void Initialize()
        {
            var form = new Form();
            var chainDescription = new SwapChainDescription()
            {
                BufferCount = 1,
                ModeDescription = new ModeDescription()
                {
                    Format = 28
                },
                Usage = 32,
                OutputHandle = form.Handle,
                SampleDescription = new SampleDescription()
                {
                    Count = 1
                },
                IsWindowed = true
            };

            var ppSwapChain = IntPtr.Zero;
            var ppDevice = IntPtr.Zero;
            var ppImmediateContext = IntPtr.Zero;
            var pFeatureLevel = IntPtr.Zero;
            var ret = D3D11CreateDeviceAndSwapChain(IntPtr.Zero, 1, IntPtr.Zero, 0, IntPtr.Zero, 0, 7, &chainDescription, out ppSwapChain, out ppDevice,
                out pFeatureLevel, out ppImmediateContext);
            //WCFClient.Instance.GetPipeProxy.RemoteLog($"ppSwapChain {ppSwapChain} ppDevice {ppDevice} pFeatureLevel {pFeatureLevel} ppImmediateContext {ppImmediateContext}");
            var ptr = Marshal.ReadIntPtr(Marshal.ReadIntPtr(ppSwapChain), 64);

            //WCFClient.Instance.GetPipeProxy.RemoteLog($"PTR {ptr}");
            var d3dChain = (D3DVirtVoid)Marshal.GetDelegateForFunctionPointer(Marshal.ReadIntPtr(Marshal.ReadIntPtr(ppSwapChain), 8), typeof(D3DVirtVoid));
            var d3dDevice = (D3DVirtVoid)Marshal.GetDelegateForFunctionPointer(Marshal.ReadIntPtr(Marshal.ReadIntPtr(ppDevice), 8), typeof(D3DVirtVoid));
            var d3dContext =
                (D3DVirtVoid)Marshal.GetDelegateForFunctionPointer(Marshal.ReadIntPtr(Marshal.ReadIntPtr(ppImmediateContext), 8), typeof(D3DVirtVoid));
            d3dChain(ppSwapChain);
            d3dDevice(ppDevice);
            d3dContext(ppImmediateContext);
            _presentDelegate = (Direct3D11Present)Marshal.GetDelegateForFunctionPointer(ptr, typeof(Direct3D11Present));
            Hook = LocalHook.Create(ptr, (Delegate)new Direct3D11Present(Callback), (object)this);
            Hook.ThreadACL.SetExclusiveACL(new Int32[] { });

        }

        public void RemoteLog(string s)
        {
            WCFClient.Instance.GetPipeProxy.RemoteLog(s);
        }

        private void Log(string s)
        {
            WCFClient.Instance.GetPipeProxy.RemoteLog(s);
        }

        private int Callback(IntPtr swapChainPtr, int syncInterval, int flags)
        {
            if (!_pyHooksInitialized)
            {
                //using (var pySharp = new PySharp(false))
                //{
                //    var xx = pySharp.Import("carbon")["common"]["script"]["net"]["machobase"];
                //    //carbon.common.script.net.machobase
                //    //var crypto = pySharp.Import("evecrypto")["crypto"]["impl"]["CryptoApiCryptoContext"];
                //    //carbon.common.script.net.SocketGPS.SSLSocketPacketTransport
                //    if (xx.IsValid && xx["Dumps"].IsValid)
                //    {
                //        //HookManagerImpl.Log("Init PyHooks.");
                //        //WCFClient.Instance.GetPipeProxy.RemoteLog("Init PyHooks.");
                //        HookManagerImpl.Instance.InitPyHooks();
                //        RemoteLog("xx valid.");
                //        HookManagerImpl.Instance.PyHook.AddGetHook(xx.PyRefPtr, "Loads");
                //        _pyHooksInitialized = true;

                //        // remove myself afterwards
                //        //Hook.Dispose();
                //        //HookManagerImpl.Instance.RemoveController(this);
                //    }
                //}
            }

            return _presentDelegate(swapChainPtr, syncInterval, flags);
        }

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

        public void Dispose()
        {
            Hook.Dispose();
        }

        public bool Error { get; set; }
        public string Name { get; set; }
    }
}