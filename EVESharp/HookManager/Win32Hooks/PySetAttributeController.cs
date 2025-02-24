using EasyHook;
using SharedComponents.Py;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;

namespace HookManager.Win32Hooks
{
    public class PySetAttributeController : IHook, IDisposable
    {
        #region Fields

        private LocalHook _hook;

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int PyObject_SetAttr(IntPtr a, IntPtr b, IntPtr c);

        #endregion Fields

        #region Constructors

        public PySetAttributeController()
        {
            Error = false;
            Name = typeof(PySetAttributeController).Name;
            System.Runtime.GCSettings.LatencyMode = System.Runtime.GCLatencyMode.Batch;
            try
            {

                _hook = LocalHook.Create(
                    LocalHook.GetProcAddress("python27.dll", "PyObject_SetAttr"),
                    new Delegate(Detour),
                    this);

                //_hook = LocalHook.Create(
                //    funcAddr,
                //    new Delegate(Detour),
                //    this);

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
        private delegate int Delegate(IntPtr a, IntPtr b, IntPtr c);

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

        private static int Detour(IntPtr obj, IntPtr attributeName, IntPtr value)
        {
            var ret = PyObject_SetAttr(obj, attributeName, value);


            // your critical code
            if (obj == IntPtr.Zero || attributeName == IntPtr.Zero || value == IntPtr.Zero)
                return ret;


            try
            {
                using (var pySharp = new PySharp(false))
                {

                    var pyObj = new PyObject(pySharp, obj, false);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }



            return ret;
        }

        #endregion Methods
    }
}