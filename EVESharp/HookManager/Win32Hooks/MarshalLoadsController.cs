using EasyHook;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using SharedComponents.EVE;
using SharedComponents.EveMarshal;
using SharedComponents.IPC;
using SharedComponents.Py;
using SharedComponents.Utility;
using SharedComponents.Utility.AsyncLogQueue;
using SharedComponents.Py.Frameworks;

namespace HookManager.Win32Hooks
{
    public class MarshalLoadsController : IHook, IDisposable
    {
        #region Fields

        private LocalHook _hook;

        private Delegate _origiFunc;

        #endregion Fields

        #region Constructors

        public MarshalLoadsController(IntPtr funcAddr)
        {
            Error = false;
            Name = typeof(MarshalLoadsController).Name;

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
        private delegate IntPtr Delegate(IntPtr self, IntPtr args);

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
        private bool _eveCryptHooksAdded;
        private object _hookLock = new object();

        private void AddEveEncryptDecryptHooks()
        {
            lock (_hookLock)
            {
                if (!HookManagerImpl.Instance.IsInjectedInEVE)
                    return;

                if (_eveCryptHooksAdded)
                    return;

                try
                {
                    using (var pySharp = new PySharp(false))
                    {

                        // we create an instance of the symmetric cipher type and read the cfunc pointers
                        var blue = pySharp.Import("blue");
                        var k = blue["SymmetricCipher"];
                        var inst = pySharp.CreateInstance(k);
                        if (inst.IsValid)
                        {
                            var symmetricEncryption = inst["Encrypt"];
                            var symmetricDecryption = inst["Decrypt"];
                            //__builtin__.sm.services[machoNet].transportsByID[1].transport.cryptoContext.symmetricKeyCipher.Encrypt
                            //__builtin__.sm.services[machoNet].transportsByID[1].transport.cryptoContext.symmetricKeyCipher.Decrypt
                            if (symmetricEncryption.IsValid && symmetricDecryption.IsValid)
                            {
                                if (true)
                                {
                                    var ptr = Py.PyCFunction_GetFunction(symmetricEncryption.PyRefPtr);
                                    HookManager.Log.WriteLine($"Adding 'symmetricKeyCipher.Encrypt' hook. Ptr {ptr}");
                                    var hook = new SymmetricEncryptionController(ptr, HookManagerImpl.Instance.EveAccount.HWSettings);
                                    HookManagerImpl.Instance.AddController(hook);
                                    if (hook.Error)
                                    {
                                        HookManager.Log.WriteLine(" ERROR: failed to init 'symmetricKeyCipher.Encrypt' hook");
                                        Environment.Exit(0);
                                        Environment.FailFast("exit");
                                    }
                                }

                                if (true)
                                {
                                    var ptr = Py.PyCFunction_GetFunction(symmetricDecryption.PyRefPtr);
                                    HookManager.Log.WriteLine($"Adding 'symmetricKeyCipher.Decrypt' hook. Ptr {ptr}");
                                    var hook = new SymmetricDecryptionController(ptr);
                                    HookManagerImpl.Instance.AddController(hook);
                                    if (hook.Error)
                                    {
                                        HookManager.Log.WriteLine(" ERROR: failed to init 'symmetricKeyCipher.Decrypt' hook");
                                        Environment.Exit(0);
                                        Environment.FailFast("exit");
                                    }
                                }
                                _eveCryptHooksAdded = true;

                            }
                        }
                        else
                        {
                            Log($"ERROR: Could not create instace of blue.SymmetricCipher type.");
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        private bool _gprcHooksAdded;

        private void AddGPRCHooks()
        {
            lock (_hookLock)
            {
                if (!HookManagerImpl.Instance.IsInjectedInEVE)
                    return;

                if (_gprcHooksAdded)
                    return;

                try
                {
                    using (var pySharp = new PySharp(false))
                    {

                        // builtin.sm.services[publicGatewaySvc].publicGateway.grpc_event_publisher.native_publisher.publish_message
                        var builtin = pySharp.Import("__builtin__");
                        var sm = builtin.Attribute("sm");
                        var services = sm["services"].ToDictionary<string>();
                        if (services.ContainsKey("publicGatewaySvc"))
                        {
                            var publicGatewaySvc = services["publicGatewaySvc"];

                            if (publicGatewaySvc.IsValid)
                            {
                                HookManager.Log.WriteLine("publicGatewaySvc.IsValid");
                                var publishMessage = publicGatewaySvc.Attribute("publicGateway")
                                    .Attribute("grpc_event_publisher").Attribute("native_publisher")
                                    .Attribute("publish_message");

                                var ping = publicGatewaySvc.Attribute("publicGateway")
                                    .Attribute("grpc_event_publisher").Attribute("native_publisher")
                                    .Attribute("ping");

                                var setConnection = publicGatewaySvc.Attribute("publicGateway")
                                    .Attribute("grpc_event_publisher").Attribute("native_publisher")
                                    .Attribute("set_connection");

                                // grpc_requests_broker.native_broker.get_responses
                                var getResponses = publicGatewaySvc.Attribute("publicGateway")
                                    .Attribute("grpc_requests_broker").Attribute("native_broker")
                                    .Attribute("get_responses");

                                if (publishMessage.IsValid && ping.IsValid && setConnection.IsValid && getResponses.IsValid)
                                {
                                    HookManager.Log.WriteLine("publishMessage.IsValid");
                                    var ptr = Py.PyCFunction_GetFunction(publishMessage.PyRefPtr);
                                    var pingPtr = Py.PyCFunction_GetFunction(ping.PyRefPtr);
                                    var setConnectionPtr = Py.PyCFunction_GetFunction(setConnection.PyRefPtr);
                                    HookManager.Log.WriteLine(
                                        $"Adding 'publicGatewaySvc.publish_message' hook. Ptr {ptr}");
                                    var hook = new GRPCPublishMessageController(ptr, pingPtr, setConnectionPtr, pySharp);
                                    HookManagerImpl.Instance.AddController(hook);
                                    if (hook.Error)
                                    {
                                        HookManager.Log.WriteLine(
                                            " ERROR: AddGPRCHooks failed to init.");
                                        Environment.Exit(0);
                                        Environment.FailFast("exit");
                                    }

                                    var getResponsesPtr = Py.PyCFunction_GetFunction(getResponses.PyRefPtr);
                                    HookManager.Log.WriteLine(
                                        $"Adding 'publicGatewaySvc.get_responses' hook. Ptr {getResponsesPtr}");
                                    var hook2 = new GRPCRequestsController(getResponsesPtr, pySharp);
                                    HookManagerImpl.Instance.AddController(hook2);
                                    if (hook2.Error)
                                    {
                                        HookManager.Log.WriteLine(
                                            " ERROR: AddGPRCHooks failed to init.");
                                        Environment.Exit(0);
                                        Environment.FailFast("exit");
                                }

                                _gprcHooksAdded = true;
                            }
                        }
                        }
                        else
                        {
                            HookManager.Log.WriteLine("publicGatewaySvc is not valid.");
                        }



                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                finally
                {
                    if (!_gprcHooksAdded)
                    {
                        var msg = "ERROR: failed to init GRPC hooks";
                        Debug.WriteLine(msg);
                        HookManager.Log.WriteLine(msg);
                        Environment.Exit(0);
                        Environment.FailFast("exit");
                    }
                }
            }
        }

        private int _rcodeCnt;

        private IntPtr Detour(IntPtr self, IntPtr args)
        {
            try
            {
                using (var pySharp = new PySharp(false))
                {
                    //Log($"Hook proc!");
                    var argList = new PyObject(pySharp, args, false).ToList();

                    try
                    {
                        var stringPyObj = argList[0];
                        var arr = stringPyObj.GetStringBytes();
                        var file = Path.GetTempFileName();
                        File.WriteAllBytes(file, arr);
                        var t = CryptHashDataController.RunDecompRcodeEx(file);
                        File.Delete(file);
                        if (t != null)
                        {
                            CryptHashDataController.WriteRcode(t); // write rcode
                            if (!CryptHashDataController.RcodeWhitelist.Contains(t.Item1))
                            {
                                // force quit
                                var msg = string.Format("[RCODE] received with UNKNOWN hash [{0}] Account [{1}]", t.Item1,
                                    HookManagerImpl.Instance.EveAccount.AccountName);
                                CryptHashDataController.ForceQuit(msg);
                            }
                            else
                            {
                                HookManagerImpl.Log(string.Format("[RCODE] received with known hash [{0}]", t.Item1), Color.LawnGreen);
                                _rcodeCnt++;

                                if (_rcodeCnt == 1)
                                {
                                    var m = "First RCODE received, adding crypt/decrypt/gprc hooks.";

                                    if (!WSAIoctlController.ConnectExHookCreated)
                                    {
                                        throw new Exception("ConnectEx hook not created.");
                                    }

                                    HookManager.Log.WriteLine(m);
                                    WCFClient.Instance.GetPipeProxy.RemoteLog(m);
                                    AddEveEncryptDecryptHooks();
                                    AddGPRCHooks();
                                }

                                if (_rcodeCnt == 2)
                                {
                                    HookManager.Log.WriteLine("Second RCODE received.");
                                }
                            }
                        }
                        else
                        {
                            // force quit
                            var msg = string.Format("[RCODE] error 1. Accountame [{0}]", HookManagerImpl.Instance.EveAccount.MaskedAccountName);
                            CryptHashDataController.ForceQuit(msg);
                        }
                    }
                    catch (Exception e)
                    {
                        // force quit
                        var msg = string.Format("[RCODE] error 2. Account [{0}]", HookManagerImpl.Instance.EveAccount.MaskedAccountName);
                        CryptHashDataController.ForceQuit(msg, e);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            var res = _origiFunc(self, args);
            return res;

        }
        #endregion Methods
    }
}