/*
 * ---------------------------------------
 * User: duketwo
 * Date: 12.12.2013
 * Time: 12:51
 *
 * ---------------------------------------
 */

using EasyHook;
using System;
using System.Runtime.InteropServices;

namespace HookManager.Win32Hooks
{
    /// <summary>
    ///     Description of IsDebuggerPresent.
    /// </summary>
    public class PyEvalGetRestrictedController : IHook, IDisposable
    {
        #region Fields

        private LocalHook _hook;

        #endregion Fields

        #region Constructors

        public PyEvalGetRestrictedController()
        {
            Error = false;
            Name = typeof(PyEvalGetRestrictedController).Name;
            var ptr = IntPtr.Zero;

            try
            {
                _hook = LocalHook.Create(
                    LocalHook.GetProcAddress("python27.dll", "PyEval_GetRestricted"),
                    new PyEval_GetRestrictedDelegate(PyEval_GetRestrictedDetour),
                    this);

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

        private delegate int PyEval_GetRestrictedDelegate();

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

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int PyEval_GetRestricted();


        private int PyEval_GetRestrictedDetour()
        {
            return 0;
        }

        #endregion Methods
    }
}