using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using EasyHook;
using SharedComponents.IPC;
using SharedComponents.Py;

namespace HookManager.Win32Hooks
{
    public class DX11CreateDeviceController : IDisposable, IHook
    {
        public bool Error { get; set; }
        public string Name { get; set; }


        [DllImport("d3d11.dll")]
        private static extern unsafe int D3D11CreateDeviceAndSwapChain(IntPtr pAdapter, int driverType, IntPtr Software,
            int flags, IntPtr pFeatureLevels,
            int FeatureLevels, int SDKVersion,
            void* pSwapChainDesc, [Out] out IntPtr ppSwapChain,
            [Out] out IntPtr ppDevice, [Out] out IntPtr pFeatureLevel,
            [Out] out IntPtr ppImmediateContext);


        private LocalHook _hook;

        private unsafe delegate int D3D11CreateDeviceAndSwapChainDelegate(IntPtr pAdapter, int driverType, IntPtr Software,
            int flags, IntPtr pFeatureLevels,
            int FeatureLevels, int SDKVersion,
            void* pSwapChainDesc, [Out] out IntPtr ppSwapChain,
            [Out] out IntPtr ppDevice, [Out] out IntPtr pFeatureLevel,
            [Out] out IntPtr ppImmediateContext);

        public unsafe DX11CreateDeviceController()
        {
            Error = false;
            Name = typeof(EnumDisplayDevicesAController).Name;

            try
            {
                _hook = LocalHook.Create(
                    LocalHook.GetProcAddress("d3d11.dll", "D3D11CreateDeviceAndSwapChain"),
                    new D3D11CreateDeviceAndSwapChainDelegate(D3D11CreateDeviceAndSwapChainDetour),
                    this);

                _hook.ThreadACL.SetExclusiveACL(new Int32[] { });
                Error = false;
            }
            catch (Exception)
            {
                Error = true;
            }
        }

        private void Log(string s)
        {
            WCFClient.Instance.GetPipeProxy.RemoteLog(s);
        }

        private unsafe int D3D11CreateDeviceAndSwapChainDetour(IntPtr pAdapter, int driverType, IntPtr Software,
            int flags, IntPtr pFeatureLevels,
            int FeatureLevels, int SDKVersion,
            void* pSwapChainDesc, [Out] out IntPtr ppSwapChain,
            [Out] out IntPtr ppDevice, [Out] out IntPtr pFeatureLevel,
            [Out] out IntPtr ppImmediateContext)
        {



            HookManagerImpl.Log($"D3D11CreateDeviceAndSwapChain called with flags [{flags}]. Adding flag 0x00000020.");
            flags = flags | 0x00000020;
            var ret = D3D11CreateDeviceAndSwapChain(pAdapter, driverType, Software, flags, pFeatureLevels, FeatureLevels,
                SDKVersion, pSwapChainDesc, out ppSwapChain, out ppDevice, out pFeatureLevel, out ppImmediateContext);
            return ret;
        }

        public void Dispose()
        {
            if (_hook == null)
                return;

            _hook.Dispose();
            _hook = null;
        }

    }
}
