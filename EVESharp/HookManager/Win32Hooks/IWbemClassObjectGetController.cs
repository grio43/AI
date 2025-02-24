using EasyHook;
using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using SharedComponents.EVE;
using SharedComponents.IPC;
using SharedComponents.Utility.AsyncLogQueue;
using SharedComponents.Utility;

namespace HookManager.Win32Hooks
{
    /// <summary>
    ///     Description of IWbemClassObjectGetController.
    ///     https://docs.microsoft.com/en-us/windows/desktop/api/wbemcli/nf-wbemcli-iwbemclassobject-get
    ///     https://msdn.microsoft.com/en-us/windows/aa391442(v=vs.71)
    /// </summary>
    ///
    ///
    /// HRESULT Get(
    //  [in]            LPCWSTR wszName,
    //  [in]            LONG    lFlags,
    //  [out]           VARIANT *pVal,
    //  [out, optional] CIMTYPE *pvtType,
    //  [out, optional] LONG    *plFlavor
    //);
    public class IWbemClassObjectGetController : IHook, IDisposable
    {
        #region Fields

        private LocalHook _hook;

        #endregion Fields

        #region Constructors

        public IWbemClassObjectGetController()
        {
            Error = false;
            Name = typeof(IWbemClassObjectGetController).Name;
            try
            {
                //var mod = NativeAPI.GetModuleHandle("fastprox.dll");
                //IntPtr addr = LocalHook.GetProcAddress("fastprox.dll", "?Get@CWbemObject@@UAGJPBGJPAUtagVARIANT@@PAJ2@Z");

                _hook = LocalHook.Create(
                    LocalHook.GetProcAddress("fastprox.dll", "?Get@CWbemObject@@UEAAJPEBGJPEAUtagVARIANT@@PEAJ2@Z"),
                    new GetDelegate(GetDetour),
                    this);
                //Util.GlobalRemoteLog($"fastprox base {mod} wbemObject::get addr {getAddr}");
                //_hook = LocalHook.Create(getAddr, new GetDelegate(GetDetour), this);

                _hook.ThreadACL.SetExclusiveACL(new Int32[] { });
                Error = false;
            }
            catch (Exception e)
            {
                Util.GlobalRemoteLog("Exception: " + e.ToString());
                Error = true;
            }
        }

        #endregion Constructors

        #region Delegates

        [DllImport("fastprox.dll",
            EntryPoint = "?Get@CWbemObject@@UEAAJPEBGJPEAUtagVARIANT@@PEAJ2@Z",
            ExactSpelling = true,
            CallingConvention = CallingConvention.StdCall,
            CharSet = CharSet.Unicode)]

        public static extern int Get([In] IntPtr callRef, [In][MarshalAs(UnmanagedType.LPWStr)]  string wszName, [In] Int32 lFlags, [In][Out] ref object pVal, [In][Out] ref Int32 pType, [In][Out] ref Int32 plFlavor);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        internal delegate int GetDelegate([In] IntPtr callRef, [In][MarshalAs(UnmanagedType.LPWStr)]  string wszName, [In] Int32 lFlags, [In][Out] ref object pVal, [In][Out] ref Int32 pType, [In][Out] ref Int32 plFlavor);

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

        private static int GetDetour([In] IntPtr callRef, [In][MarshalAs(UnmanagedType.LPWStr)] string wszName, [In] Int32 lFlags, [In][Out] ref object pVal, [In][Out] ref Int32 pType, [In][Out] ref Int32 plFlavor)
        {
            // quit on any WMI call
            WCFClient.Instance.GetPipeProxy.RemoteLog("A WMI call was made. Qutting/disabling this instance!");

            try
            {
                WCFClient.Instance.GetPipeProxy.RemoteLog("WMI value called: Property [" + wszName + "]");
                WCFClient.Instance.GetPipeProxy.RemoteLog("WMI value called: pVal.GetType() [" + pVal.GetType() + "]");
                WCFClient.Instance.GetPipeProxy.RemoteLog("WMI value called: pVal [" + pVal + "]");
            }
            catch (Exception ex)
            {
                WCFClient.Instance.GetPipeProxy.RemoteLog("Exception [" + ex + "]");
            }

            //WCFClient.Instance.GetPipeProxy.RemoteLog("A WMI call was made. Qutting/disabling this instance!");
            //WCFClient.Instance.GetPipeProxy.SetEveAccountAttributeValue(WCFClient.Instance.GUID,
            //    nameof(EveAccount.UseScheduler), false);

            //Environment.Exit(0);
            //Environment.FailFast("exit"); // shouldn't reach the code below, failfast terminates instantly.

            var orig = Get(callRef, wszName, lFlags, ref pVal, ref pType, ref plFlavor);
            //if (wszName.Equals("MACAddress"))
            //{
            //    pVal = "E9-67-CB-D9-17-B4"; // VARIANT marshalling works by ref in both directions :)
            //}
            //HookManagerImpl.Log($"WMI value called. Property [{wszName}] ValueType [{pVal.GetType()}] Value[{pVal}]");
            return orig;
        }

        #endregion Methods
    }
}