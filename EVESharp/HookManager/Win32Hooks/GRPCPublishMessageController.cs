
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
using System.Net;
using System.Xml.Linq;

namespace HookManager.Win32Hooks
{
    public class GRPCPublishMessageController : IHook, IDisposable
    {
        #region Fields

        private LocalHook _publishHook;

        private LocalHook _pingHook;

        private LocalHook _setConnectionHook;

        private Delegate _origPublishFunc;

        private Delegate _origPingFunc;

        private Delegate _origSetConnectionFunc;

        private SharedArray<bool> _userInformationGatheredSharedArray = null;

        #endregion Fields

        #region Constructors

        public GRPCPublishMessageController(IntPtr funcAddr, IntPtr pingFuncAddress, IntPtr setConnectionPtr, PySharp pySharp)
        {
            Error = false;
            Name = typeof(GRPCPublishMessageController).Name;
            _userInformationGatheredSharedArray = new SharedArray<bool>(HookManagerImpl.Instance.CharName + nameof(UsedSharedMemoryNames.GRPCUserInformationGathered), 1);
            _userInformationGatheredSharedArray[0] = false;
            try
            {

                var builtin = pySharp.Import("__builtin__");
                var sm = builtin.Attribute("sm");
                var services = sm["services"].ToDictionary<string>();
                var publicGatewaySvc = services["publicGatewaySvc"];

                // native publisher
                var nativePublisher = publicGatewaySvc.Attribute("publicGateway")
                    .Attribute("grpc_event_publisher").Attribute("native_publisher");
                // set_connection

                //__builtin__.sm.services[publicGatewaySvc].publicGateway.connection_config.connection
                var connection = publicGatewaySvc.Attribute("publicGateway")
                    .Attribute("connection_config").Attribute("connection");
                // connect, disconnect

                if(connection == null || !connection.IsValid)
                    throw new Exception("connection obj is not valid");

                var connectionCopy = connection.DeepCopy();

                if (!connection.IsValid || !nativePublisher.IsValid || !connectionCopy.IsValid)
                    throw new Exception("Connection, nativePublisher or connectionCopy is not valid");

                connectionCopy.IncRef();

                //if (connectionCopy.IsValid)
                //{
                //    Log($"ConnectionCopy is valid");
                //}
                //else
                //{
                //    Log($"ConnectionCopy is NOT valid");
                //}

                nativePublisher.Call("set_connection", connectionCopy);


                _publishHook = LocalHook.Create(
                    funcAddr,
                    new Delegate(DetourPublishEvent),
                    this);

                _pingHook = LocalHook.Create(
                                       pingFuncAddress,
                                       new Delegate(DetourPing),
                                       this);

                _setConnectionHook = LocalHook.Create(
                    setConnectionPtr,
                    new Delegate(DetourSetConnection),
                    this);


                _origPublishFunc = Marshal.GetDelegateForFunctionPointer<Delegate>(funcAddr);
                _origPingFunc = Marshal.GetDelegateForFunctionPointer<Delegate>(pingFuncAddress);
                _origSetConnectionFunc = Marshal.GetDelegateForFunctionPointer<Delegate>(setConnectionPtr);

                //_origSetConnectionFunc(PySharp.PyNone, PySharp.PyNone, PySharp.PyNone);

                _publishHook.ThreadACL.SetExclusiveACL(new Int32[] { });
                _pingHook.ThreadACL.SetExclusiveACL(new Int32[] { });
                _setConnectionHook.ThreadACL.SetExclusiveACL(new Int32[] { });

                //connection.Call("disconnect");

                Error = false;
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
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
            Debug.WriteLine(s);
            WCFClient.Instance.GetPipeProxy.RemoteLog(s);
        }

        public void Dispose()
        {
            _publishHook.Dispose();
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

        private IntPtr DetourPing(IntPtr self, IntPtr args, IntPtr x)
        {
            //Log($"GRPCPublishMessageController Ping Hook Proc!");
            var res = IntPtr.Zero;
            try
            {
                //res = _origPingFunc(self, args, x);
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: " + e);
            }
            //return res;
            return PySharp.PyNone;
        }


        private IntPtr DetourSetConnection(IntPtr self, IntPtr args, IntPtr x)
        {
            Log($"GRPCPublishMessageController set_connection Hook Proc!");
            var res = IntPtr.Zero;
            try
            {
                //res = _origPingFunc(self, args, x);
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: " + e);
            }
            //return res;
            return PySharp.PyNone;
        }

        private IntPtr DetourPublishEvent(IntPtr self, IntPtr args, IntPtr x)
        {
            //Log($"GRPCPublishMessageController Hook proc!");
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
                            {
                                var Cs = obj["duration_opened"]["seconds"].ToLong();
                                var Cns = obj["duration_opened"]["nanos"].ToLong();
                                var Cname = obj["window"]["unique_name"].ToUnicodeString();
                                HookManager.Log.WriteLine($"{GPRCPrefix} Window Closed [{Cname}] DurationSeconds[{Cs}] DurationNanoSeconds[{Cns}]");
                                CheckTimes(Cs, Cns, classModuleName, Cname);
                            }
                            break;

                        case "eve_public.app.eveonline.login.login_pb2.UserInformationGathered":
                            HookManager.Log.RemoteWriteLine($"[{classModuleName}] \n\n\n\n\n\n\n {obj.LogObject()}");
                            _userInformationGatheredSharedArray[0] = true;
                            break;

                        case "eve_public.app.eveonline.generic_ui.tab.tab_pb2.Selected":
                            {
                                var groupName = obj["group_name"].ToUnicodeString();
                                var name = obj["name"].ToUnicodeString();
                                HookManager.Log.WriteLine($"{GPRCPrefix} GroupName [{groupName}] Name [{name}]");

                            }
                            break;

                        case "eve_public.public_pb2.JourneyLinked":
                            {
                                var journey = obj["journey"].ToUnicodeString();
                                var reason = obj["reason"].ToUnicodeString();
                                HookManager.Log.WriteLine($"{GPRCPrefix} JourneyLinked Journey [{journey}] Reason [{reason}]");
                            }
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

            //var res = IntPtr.Zero;
            try
            {
                //res = _origPublishFunc(self, args, x);
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: " + e);
            }
            //return res;
            return PySharp.PyNone;

        }
        #endregion Methods
    }
}