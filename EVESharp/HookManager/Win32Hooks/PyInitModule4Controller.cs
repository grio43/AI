using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using EasyHook;
using SharedComponents.IPC;
using SharedComponents.Py;

namespace HookManager.Win32Hooks
{
    public class PyInitModule4Controller : IHook
    {
        public bool Error { get; set; }
        public string Name { get; set; }

        private LocalHook _hook;

        public PyInitModule4Controller()
        {
            Error = false;
            Name = typeof(PyInitModule4Controller).Name;

            try
            {
                _hook = LocalHook.Create(
                    LocalHook.GetProcAddress("python27.dll", "Py_InitModule4_64"),
                    new Py_InitModule4Dele(Py_InitModule4Detour),
                    this);

                _hook.ThreadACL.SetExclusiveACL(new Int32[] { });
                Error = false;
            }
            catch (Exception)
            {
                Error = true;
            }
        }

        private delegate IntPtr Py_InitModule4Dele(string name, IntPtr methodsPtr, string doc, IntPtr selfPtr,
            int apiver);

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Py_InitModule4_64(string name, IntPtr methodsPtr, string doc, IntPtr selfPtr,
            int apiver);

        public void Dispose()
        {
            _hook.Dispose();
        }

        private void Log(string s)
        {
            Debug.WriteLine(s);
            //WCFClient.Instance.GetPipeProxy.RemoteLog(s);
            HookManagerImpl.Log(s);
        }

        private void DumpBlueSysInfoGetPDMData()
        {
            using (var pySharp = new PySharp(false))
            {
                var blue = pySharp.Import("blue");
                var sysInfo = blue["sysinfo"];
                var call = sysInfo.Call("GetPDMData");
                WCFClient.Instance.GetPipeProxy.RemoteLog($"[blue.sysinfo.GetPDMData] \n\n\n {call.ToUnicodeString()}");
            }
        }


        private unsafe IntPtr Py_InitModule4Detour(string name, IntPtr methodsPtr, string doc, IntPtr selfPtr, int apiver)
        {
            if (methodsPtr != IntPtr.Zero)
            {
                var curr = methodsPtr;

                for (; ; )
                {
                    var methDef = Marshal.PtrToStructure<Py.PyMethodDef>(curr);
                    if (methDef.ml_name == IntPtr.Zero) // break on sentinel
                        break;
                    var methodName = Marshal.PtrToStringAnsi(methDef.ml_name);
                    curr += sizeof(Py.PyMethodDef);
                    //Log($"name {name} methodName {methodName}");

                    if (name.Equals("marshal") && methodName.Equals("loads"))
                    {
                        Log($"Adding 'marshal.loads' hook.");
                        var hook = new MarshalLoadsController(methDef.ml_meth);
                        HookManagerImpl.Instance.AddController(hook);
                        if (hook.Error)
                        {
                            Log(" Error: failed to init 'marshal.loads' hook");
                            Environment.Exit(0);
                            Environment.FailFast("exit");
                        }
                    }

                    if (name.Equals("blue.crypto") && methodName.Equals("GetSharedAsymmetricCipher"))
                    {
                        DumpBlueSysInfoGetPDMData();
                        Log($"Adding 'blue.crypto.GetSharedAsymmetricCipher' hook.");
                        var hook = new GetSharedAsymmetricCipherController(methDef.ml_meth);
                        HookManagerImpl.Instance.AddController(hook);
                        if (hook.Error)
                        {
                            Log(" ERROR: failed to init 'blue.crypto.GetSharedAsymmetricCipher' hook");
                            Environment.Exit(0);
                            Environment.FailFast("exit");
                        }
                    }

                }
            }
            return Py_InitModule4_64(name, methodsPtr, doc, selfPtr, apiver);
        }
    }
}
