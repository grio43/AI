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

namespace HookManager.Win32Hooks
{
    /// <summary>
    ///     Description of IsDebuggerPresent.
    /// </summary>
    public class IsDebuggerPresentController : IHook, IDisposable
    {
        #region Fields

        private LocalHook _hook;

        #endregion Fields

        #region Constructors

        public IsDebuggerPresentController()
        {
            Error = false;
            Name = typeof(IsDebuggerPresentController).Name;

            try
            {
                _hook = LocalHook.Create(
                    LocalHook.GetProcAddress("kernel32.dll", "IsDebuggerPresent"),
                    new IsDebuggerPresentDelegate(IsDebuggerPresentDetour),
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

        private delegate bool IsDebuggerPresentDelegate();

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

        private bool IsDebuggerPresentDetour()
        {
            return false;
        }

        #endregion Methods
    }
}