using EasyHook;
using HookManager.Win32Hooks;
using SharedComponents.Py;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace HookManager.PythonHooks
{
    public class PyFileWriteObjectController : IHook, IDisposable
    {
        #region Fields

        private LocalHook _hook;

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        #endregion Fields

        #region Constructors

        public PyFileWriteObjectController()
        {
            Error = false;
            Name = nameof(PyFileWriteObjectController);

            try
            {
                // Get the address of PyFile_WriteObject from python27.dll
                IntPtr pythonDll = GetModuleHandle("python27.dll");
                if (pythonDll == IntPtr.Zero)
                {
                    throw new Exception("Failed to find python27.dll.");
                }

                IntPtr writeStringAddress = GetProcAddress(pythonDll, "PyFile_WriteObject");
                if (writeStringAddress == IntPtr.Zero)
                {
                    throw new Exception("Failed to find PyFile_WriteObject.");
                }

                // Hook the PyFile_WriteObject function
                _hook = LocalHook.Create(
                    writeStringAddress,
                    new PyFileWriteObjectDelegate(PyFileWriteObjectDetour),
                    this);

                _hook.ThreadACL.SetExclusiveACL(new Int32[] { });
                Error = false;
            }
            catch (Exception ex)
            {
                Error = true;
                Debug.WriteLine($"Error initializing PyFileWriteObjectController: {ex}");
            }
        }

        #endregion Constructors

        #region Delegates

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int PyFileWriteObjectDelegate(IntPtr s, IntPtr f, int flag);

        #endregion Delegates

        #region Properties

        public bool Error { get; set; }
        public string Name { get; set; }

        #endregion Properties

        #region Methods

        public void Dispose()
        {
            _hook?.Dispose();
        }

        private int PyFileWriteObjectDetour(IntPtr s, IntPtr f, int flag)
        {
            try
            {
                var pyStdOut = PySys_GetObject("stdout");
                var pyStdErr = PySys_GetObject("stderr");


                if (f != pyStdErr && f != pyStdOut)
                    return PyFile_WriteObject(s, f, flag);

                using (var pySharp = new PySharp(false))
                {
                    var sx = new PyObject(pySharp, s, false);
                    //var sf = new PyObject(pySharp, f, false);

                    var isError = pyStdErr == f;
                    if (sx.GetPyType() == PyType.StringType || (flag & 1) > 0)
                    {
                        //Debug.WriteLine($"[Hooked PyFile_WriteObject] SX {sx.ToUnicodeString()} -- Flag [{flag}]");
                        Log.RemoteConsoleWriteLine(sx.ToUnicodeString(), isError);
                    }
                    else
                    {
                        Log.RemoteConsoleWriteLine(sx.Repr, isError);
                        //Debug.WriteLine($"--------------- [Hooked PyFile_WriteObject] SX {sx.Repr} -- Flag [{flag}]");
                    }
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception in PyFile_WriteObjectDetour: {ex}");
            }

            // Call the original PyFile_WriteObject function
            return PyFile_WriteObject(s, f, flag);
        }

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int PyFile_WriteObject(IntPtr s, IntPtr f, int flag);

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr PySys_GetObject(string s);

        #endregion Methods
    }
}
