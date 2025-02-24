using EasyHook;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography;
using System.Text;
using SharedComponents.EVE;
using SharedComponents.EveMarshal;
using SharedComponents.IPC;
using SharedComponents.Py;
using SharedComponents.Utility;
using SharedComponents.Utility.AsyncLogQueue;
using SharedComponents.SharedMemory;

namespace HookManager.Win32Hooks
{
    public class GPRCHookController : IHook, IDisposable
    {
        #region Fields

        private LocalHook _hook;

        private Delegate _origiFunc;

        private SharedArray<bool> _userInformationGatheredSharedArray = null;

        #endregion Fields

        #region Constructors

        public GPRCHookController(IntPtr funcAddr)
        {
            Error = false;
            Name = typeof(GPRCHookController).Name;
            _userInformationGatheredSharedArray = new SharedArray<bool>(HookManagerImpl.Instance.EveAccount.GUID.ToString() + nameof(UsedSharedMemoryNames.GRPCUserInformationGathered), 1);
            _userInformationGatheredSharedArray[0] = false;
            try
            {
                _hook = LocalHook.Create(
                    funcAddr,
                    new Delegate(Detour),
                    this);

                _origiFunc = Marshal.GetDelegateForFunctionPointer<Delegate>(funcAddr);

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
        private delegate IntPtr Delegate(IntPtr self, IntPtr args, IntPtr x);

        #endregion Delegates

        #region Properties

        public bool Error { get; set; }
        public string Name { get; set; }

        #endregion Properties

        #region Methods
        private void Log(string s)
        {
            WCFClient.Instance.GetPipeProxy.RemoteLog(s);
        }

        public void Dispose()
        {
            _hook.Dispose();
        }

        private void CheckTimes(long s, long ns, string clsName, string wndName)
        {
            if (s == 0 && ns > 0)
            {
                double val = ns / (long)1000000;
                if (val < 500)
                {
                    var msg = $"WARNING: Window/View [{wndName}] Class [{clsName}] was opened/focused for less than 500ms. Ms [{val}]";
                    HookManager.Log.WriteLine(msg);
                    //HookManagerImpl.Instance.SendToInjectorLog(msg);
                }
            }
        }

        private IntPtr Detour(IntPtr self, IntPtr args, IntPtr x)
        {
            //Log($"GPRCHookController Hook proc!");
            try
            {
                using (var pySharp = new PySharp(false))
                {
                    var argList = new PyObject(pySharp, args, false);
                    //HookManager.Log.RemoteWriteLine(argList.LogObject());
                    //HookManager.Log.RemoteWriteLine(argList.GetItemAt(0).LogObject());

                    var obj = argList.GetItemAt(1);

                    var className = obj["__class__"]["__name__"].ToUnicodeString();
                    var moduleName = obj["__class__"]["__module__"].ToUnicodeString();
                    var classModuleName = moduleName + "." + className;
                    //HookManager.Log.RemoteWriteLine($"classModuleName: {classModuleName}");
                    //HookManager.Log.RemoteWriteLine(argList.LogObject());
                    //HookManager.Log.RemoteWriteLine(obj.LogObject());

                    var GPRCPrefix = "GPRC-Event: ";

                    switch (classModuleName)
                    {
                        // views
                        case "eve_public.app.eveonline.generic_ui.view.view_pb2.Activated":
                            HookManager.Log.WriteLine($"{GPRCPrefix} View Activated [{obj["view_unique_name"].ToUnicodeString()}]");
                            break;
                        case "eve_public.app.eveonline.generic_ui.view.view_pb2.Deactivated":
                            var As = obj["duration_active"]["seconds"].ToLong();
                            var Ans = obj["duration_active"]["nanos"].ToLong();
                            var Aname = obj["view_unique_name"].ToUnicodeString();
                            HookManager.Log.WriteLine($"{GPRCPrefix} View Deactivated [{Aname}] DurationSeconds [{As}] DurationNanoSeconds [{Ans}]");
                            CheckTimes(As, Ans, classModuleName, Aname);
                            break;

                        // windows
                        case "eve_public.app.eveonline.generic_ui.window.analytics_pb2.Opened":
                            HookManager.Log.WriteLine($"{GPRCPrefix} Window Opened [{obj["window"]["unique_name"].ToUnicodeString()}]");
                            break;
                        case "eve_public.app.eveonline.generic_ui.window.analytics_pb2.Focused":
                            HookManager.Log.WriteLine($"{GPRCPrefix} Window Focused [{obj["window"]["unique_name"].ToUnicodeString()}]");
                            break;

                        case "eve_public.app.eveonline.generic_ui.window.analytics_pb2.Unfocused":
                            var Bs = obj["duration_focused"]["seconds"].ToLong();
                            var Bns = obj["duration_focused"]["nanos"].ToLong();
                            var Bname = obj["window"]["unique_name"].ToUnicodeString();
                            HookManager.Log.WriteLine($"{GPRCPrefix} Window Unfocused [{Bname}] DurationSeconds [{Bs}] DurationNanoSeconds [{Bns}]");
                            CheckTimes(Bs, Bns, classModuleName, Bname);
                            break;
                        case "eve_public.app.eveonline.generic_ui.window.analytics_pb2.Closed":
                            var Cs = obj["duration_opened"]["seconds"].ToLong();
                            var Cns = obj["duration_opened"]["nanos"].ToLong();
                            var Cname = obj["window"]["unique_name"].ToUnicodeString();
                            HookManager.Log.WriteLine($"{GPRCPrefix} Window Closed [{Cname}] DurationSeconds[{Cs}] DurationNanoSeconds[{Cns}]");
                            CheckTimes(Cs, Cns, classModuleName, Cname);
                            break;

                        case "eve_public.app.eveonline.login.login_pb2.UserInformationGathered":
                            HookManager.Log.RemoteWriteLine($"[{classModuleName}] \n\n\n\n\n\n\n {obj.LogObject()}");
                            _userInformationGatheredSharedArray[0] = true;
                            break;

                        default:
                            HookManager.Log.WriteLine($"WARN: classModuleName not known yet. ClassModuleName [{classModuleName}]");
                            HookManager.Log.WriteLine($"Dump {obj.LogObject()}");
                            HookManager.Log.RemoteWriteLine($"WARN: classModuleName not known yet. ClassModuleName [{classModuleName}]");
                            HookManager.Log.RemoteWriteLine($"Dump {obj.LogObject()}");
                            break;
                    }

                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: " + e);
            }

            var res = IntPtr.Zero;
            try
            {
                res = _origiFunc(self, args, x);
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: " + e);
            }
            return res;

        }
        #endregion Methods
    }
}