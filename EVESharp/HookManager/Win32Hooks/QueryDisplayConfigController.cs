using EasyHook;
using SharedComponents.EVE;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using static HookManager.Win32Hooks.DX11Controller;

namespace HookManager.Win32Hooks
{
    public class QueryDisplayConfigController : IHook, IDisposable
    {
        #region Fields

        private LocalHook _hook;

        #endregion Fields


        public enum DisplayConfigTopologyId : uint
        {
            None = 0,
            Internal = 0x00000001,
            Clone = 0x00000002,
            Extend = 0x00000004,
            External = 0x00000008,
            ForceUint32 = 0xFFFFFFFF
        }

        #region Constructors

        public QueryDisplayConfigController(HWSettings hWSettings)
        {
            Error = false;
            Name = typeof(QueryDisplayConfigController).Name;

            try
            {

                _hook = LocalHook.Create(
                    LocalHook.GetProcAddress("user32.dll", "QueryDisplayConfig"),
                    new Delegate(Detour),
                    this);


                _hook.ThreadACL.SetExclusiveACL(new Int32[] { });
                Error = false;
            }
            catch (Exception)
            {
                Error = true;
            }
            HWSettings = hWSettings;
        }

        #endregion Constructors

        #region Delegates

        [Flags]
        public enum QueryDisplayConfigFlags : uint
        {
            None = 0,
            AllPaths = 0x01,
            OnlyActivePaths = 0x02,
            DatabaseCurrent = 0x04,
            VirtualModeAware = 0x10,
            IncludeHmd = 0x20
        }

        [DllImport("User32.dll")]
        private static extern ErrorCode QueryDisplayConfig(
      QueryDisplayConfigFlags flags,
        IntPtr numPathArrayElements,
        IntPtr pathArray,
        IntPtr numModeInfoArrayElements,
        IntPtr modeInfoArray,
        IntPtr currentTopologyId);


        public enum ErrorCode
        {
            Success = 0,
            AccessDenied = 5,
            GenFailure = 31,
            NotSupported = 50,
            InvalidParameter = 87,
            InsufficientBuffer = 122
        }


        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Auto)]
        private delegate int Delegate(QueryDisplayConfigFlags flags,
        IntPtr numPathArrayElements,
        IntPtr pathArray,
        IntPtr numModeInfoArrayElements,
        IntPtr modeInfoArray,
        IntPtr currentTopologyId);

        #endregion Delegates

        #region Properties

        public bool Error { get; set; }
        public string Name { get; set; }
        public HWSettings HWSettings { get; }

        #endregion Properties

        #region Methods



        public void Dispose()
        {
            _hook.Dispose();
        }

        private static int Detour(QueryDisplayConfigFlags flags,
        IntPtr numPathArrayElements,
        IntPtr pathArray,
        IntPtr numModeInfoArrayElements,
        IntPtr modeInfoArray,
        IntPtr currentTopologyId)
        {

            var orig = QueryDisplayConfig(flags, numPathArrayElements, pathArray, numModeInfoArrayElements, modeInfoArray, currentTopologyId);
            var pathAmount = Marshal.ReadInt32(numPathArrayElements);
            //Log.RemoteWriteLine($"QueryDisplayConfigController.Detour. Path amount [{pathAmount}]");

            for (int i = 0; i < pathAmount; i++)
            {
                IntPtr strucPtr = new IntPtr(pathArray.ToInt64() + i * Marshal.SizeOf(typeof(DISPLAYCONFIG_PATH_INFO)));
                var path = Marshal.PtrToStructure<DISPLAYCONFIG_PATH_INFO>(strucPtr);
                //Log.RemoteWriteLine($"Refreshrate: {path.targetInfo.refreshRate.Numerator/path.targetInfo.refreshRate.Denominator}");
                //path.targetInfo.refreshRate.Numerator = 100_000;
                //Marshal.StructureToPtr(path, strucPtr, true);
            }
            
            return (int)orig;
        }

        #endregion Methods
    }
}