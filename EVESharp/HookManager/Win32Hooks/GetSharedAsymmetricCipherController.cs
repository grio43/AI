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

namespace HookManager.Win32Hooks
{
    public class GetSharedAsymmetricCipherController : IHook, IDisposable
    {
        #region Fields

        private LocalHook _hook;

        private Delegate _origiFunc;

        #endregion Fields

        #region Constructors

        public GetSharedAsymmetricCipherController(IntPtr funcAddr)
        {
            Error = false;
            Name = typeof(GetSharedAsymmetricCipherController).Name;

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

        private IntPtr Detour(IntPtr self, IntPtr args)
        {
            Log($"GetSharedAsymmetricCipherController Hook proc!");
            try
            {
                //using (var pySharp = new PySharp(false))
                //{
                //    Log($"GetSharedAsymmetricCipherController Hook proc!");
                //    var argList = new PyObject(pySharp, args, false);
                //    HookManager.Log.RemoteWriteLine(argList.LogObject());
                //}
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            var res = _origiFunc(self, args);

            using (var pySharp = new PySharp(false))
            {
                
                var resultObj = new PyObject(pySharp, res, false);
                var decryptFunc = resultObj["Encrypt"];
                //var decryptFunc = resultObj["Encrypt"];
                var decryptFuncPtr = Py.PyCFunction_GetFunction(decryptFunc.PyRefPtr);
                //HookManager.Log.RemoteWriteLine($"decryptFuncPtr {decryptFuncPtr} decryptFunc.type {decryptFunc.GetPyType()}");
                //HookManager.Log.RemoteWriteLine(resultObj.LogObject());
                Log($"Adding 'blue.crypto.GetSharedAsymmetricCipher.Encrypt' hook.");
                var hook = new GetSharedAsymmetricCipherEncryptController(decryptFuncPtr);
                HookManagerImpl.Instance.AddController(hook);
                if (hook.Error)
                {
                    Log("ERROR: failed to init 'blue.crypto.GetSharedAsymmetricCipher.Decrypt' hook");
                    Environment.Exit(0);
                    Environment.FailFast("exit");
                }
            }
            return res;

        }
        #endregion Methods
    }
}