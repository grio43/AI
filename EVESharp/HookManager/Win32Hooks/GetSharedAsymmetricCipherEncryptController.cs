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

namespace HookManager.Win32Hooks
{
    public class GetSharedAsymmetricCipherEncryptController : IHook, IDisposable
    {
        #region Fields

        private LocalHook _hook;

        private Delegate _origiFunc;

        #endregion Fields

        #region Constructors

        public GetSharedAsymmetricCipherEncryptController(IntPtr funcAddr)
        {
            Error = false;
            Name = typeof(GetSharedAsymmetricCipherEncryptController).Name;

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

        public static Byte[] AESKey { get; set; }
        public static Byte[] AESIV { get; set; }

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

        private IntPtr Detour(IntPtr self, IntPtr args)
        {
            Log($"GetSharedAsymmetricCipherControllerEncrypt Hook proc!");
            try
            {
                using (var pySharp = new PySharp(false))
                {

                    //var __builtin__ = pySharp.Import("__builtin__");
                    //var sm = __builtin__["sm"];
                    //var services = sm["services"].ToDictionary<string>();
                    //var machoNet = services["machoNet"];
                    //Log($"machoNet.IsValid {machoNet.IsValid}");
                    //var transports = machoNet["transportsByID"].ToDictionary<int>();
                    //Log($"transports.cnt {transports.Count}");
                    //var transpsport = transports[1];
                    ////__builtin__.sm.services[machoNet].transportsByID[1].transport.cryptoContext.symmetricKeyCipher
                    //Log($"VALID {transpsport.IsValid}");
                    var argList = new PyObject(pySharp, args, false);

                    //HookManager.Log.RemoteWriteLine(argList.LogObject());
                    var argL = argList.ToList();
                    var bytes = argL[0].GetStringBytes();

                    if (bytes.Length == 32)
                    {
                        // key bytes
                        AESKey = bytes;
                        //Log($"AESKEY(x2): {Util.ByteToHex(AESKey)}");
                    }

                    if (bytes.Length == 16)
                    {
                        // iv bytes
                        AESIV = bytes;
                        //Log($"AESIV(x2): {Util.ByteToHex(AESIV)}");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            var res = _origiFunc(self, args);

            using (var pySharp = new PySharp(false))
            {

                var resultObj = new PyObject(pySharp, res, false);
                //HookManager.Log.RemoteWriteLine(resultObj.LogObject());
                //HookManager.Log.RemoteWriteLine($"Encrypted(x2): {Util.ByteToHex(resultObj.GetStringBytes())}");
                //HookManager.Log.RemoteWriteLine($"Encrypted(str): {resultObj.ToUnicodeString()}");
            }
            return res;

        }
        #endregion Methods
    }
}