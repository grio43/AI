/*
 * ---------------------------------------
 * User: duketwo
 * Date: 12.12.2013
 * Time: 13:07
 *
 * ---------------------------------------
 */

using System;

namespace HookManager.Win32Hooks
{
    /// <summary>
    ///     Description of IHook.
    /// </summary>
    public interface IHook : IDisposable
    {
        #region Properties

        bool Error { get; set; }
        string Name { get; set; }

        #endregion Properties
    }
}