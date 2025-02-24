/*
 * ---------------------------------------
 * User: duketwo
 * Date: 19.05.2018
 * Time: 14:20
 *
 * ---------------------------------------
 */

using EasyHook;
using SharedComponents.Py;
using SharedComponents.Utility;
using System;
using System.Runtime.InteropServices;
using SharedComponents.EveMarshal;
using SharedComponents.Utility.AsyncLogQueue;

namespace HookManager.Win32Hooks
{
    //BOOL WINAPI CryptEncrypt(
    //    _In_ HCRYPTKEY  hKey,
    //_In_ HCRYPTHASH hHash,
    //_In_ BOOL       Final,
    //_In_ DWORD      dwFlags,
    //_Inout_ BYTE       *pbData,
    //_Inout_ DWORD      *pdwDataLen,
    //_In_ DWORD      dwBufLen
    //);

    /// <summary>
    ///     Description of CryptEncryptController.
    /// </summary>
    public class CryptEncryptController : IDisposable, IHook
    {
        #region Fields

        public const byte ZlibHeader = 0x78;

        private LocalHook _hook;

        private string _name;

        public static AsyncLogQueue AsyncLogQueue = AsyncLogQueue = new AsyncLogQueue();

        #endregion Fields

        #region Constructors

        public CryptEncryptController()
        {
            Name = typeof(CryptEncryptController).Name;
            try
            {
                _name = string.Format("CryptEncrypt{0:X}", LocalHook.GetProcAddress("advapi32.dll", "CryptEncrypt"));
                _hook = LocalHook.Create(LocalHook.GetProcAddress("advapi32.dll", "CryptEncrypt"), new CryptEncryptDelegate(CryptEncryptDetour), this);
                _hook.ThreadACL.SetExclusiveACL(new Int32[] { });
            }
            catch (Exception)
            {
                Error = true;
            }
        }

        #endregion Constructors

        #region Delegates

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate bool CryptEncryptDelegate(IntPtr hKey, IntPtr hHash, int Final, uint dwFlags, [In][Out] IntPtr pbData, [In][Out] IntPtr pdwDataLen, uint dwBufLen);

        #endregion Delegates

        #region Properties

        public bool Error { get; set; }

        public string Name { get; set; }

        private bool _rcodeDumpLogged { get; set; }

        private bool _rcodeValueChecked { get; set; }

        #endregion Properties

        #region Methods

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: System.Runtime.InteropServices.MarshalAs(UnmanagedType.Bool)]
        public static extern bool CryptEncrypt(IntPtr hKey, IntPtr hHash, int Final, uint dwFlags, [In][Out] IntPtr pbData, [In][Out] IntPtr pdwDataLen, uint dwBufLen);

        public void Dispose()
        {
            if (_hook == null)
                return;

            _hook.Dispose();
            _hook = null;
        }

        private void RcodeDumpLog(IntPtr hKey, IntPtr hHash, uint dwFlags, [In][Out] IntPtr pbData, [In][Out] IntPtr pdwDataLen, uint dwBufLen, int size)
        {
            using (var pySharp = new PySharp(false))
            {
                var services = pySharp.Import("__builtin__")["sm"]["services"];
                if (services.IsValid)
                {
                    var serviceDict = ((PyObject)services).ToDictionary<string>();
                    if (serviceDict.ContainsKey("machoNet") && serviceDict["machoNet"].Attribute("state").ToInt() == 4)
                    {
                        bool zLib = false;
                        var cbytes = new byte[size];
                        Marshal.Copy(pbData, cbytes, 0, size);
                        var dbytes = cbytes;
                        if (cbytes[0] == ZlibHeader)
                        {
                            dbytes = SharedComponents.EveMarshal.Zlib.Decompress(cbytes);
                            zLib = true;
                        }
                        var blueMarshal = pySharp.Import("blue").Attribute("marshal");
                        if (blueMarshal.IsValid)
                        {
                            var packet = blueMarshal.Call("Load", dbytes);
                            if (packet.IsValid)
                            {
                                if (!zLib)
                                {
                                    if (packet.GetPyType() == PyType.TupleType)
                                    {
                                        var pyDict = packet.GetItemAt(2);
                                        if (pyDict.IsValid && pyDict.GetPyType() == PyType.DictType)
                                        {
                                            var dict = pyDict.ToDictionary<string>();
                                            HookManagerImpl.Log($"Values ({dict.Count}) going to the server:");
                                            _rcodeDumpLogged = true;
                                            if (dict.ContainsKey("cpu_sse2"))
                                            {
                                                int i = 0;
                                                foreach (var kv in dict)
                                                {
                                                    i++;
                                                    //HookManagerImpl.Log($"{i}: {kv.Value.GetPyType()}");
                                                    if (kv.Value.GetValue(out var obj, out _))
                                                    {
                                                        if (obj is long)
                                                            obj = (long)obj;
                                                        if (obj is int)
                                                            obj = (int)obj;
                                                        if (obj is bool)
                                                            obj = (bool)obj;
                                                        if (obj is float)
                                                            obj = (float)obj;
                                                        if (obj is string)
                                                            obj = ((string)obj).Replace("\n", "");
                                                        if (obj == null)
                                                        {
                                                            HookManagerImpl.Log($"[{i:D2}] {kv.Key} : ''");
                                                            continue;
                                                        }
                                                        HookManagerImpl.Log($"[{i:D2}] {kv.Key} : '{obj.ToString()}'");
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                //HookManagerImpl.Log($"{packet.LogObject()}");
                                HookManagerImpl.Log($"Packet sent. Size {size / 1024d} kb.");
                            }
                            else
                            {
                                HookManagerImpl.Log($"Warning: Packet could not be marshalled.");
                            }
                        }
                    }
                }
            }
        }

        private bool CryptEncryptDetour(IntPtr hKey, IntPtr hHash, int Final, uint dwFlags, [In][Out] IntPtr pbData, [In][Out] IntPtr pdwDataLen, uint dwBufLen)
        {
            var size = Marshal.ReadInt32(pdwDataLen);
            try
            {
                if (size != 0 && pbData != IntPtr.Zero && Final == 1)
                {
                    if (!_rcodeDumpLogged)
                    {
                        RcodeDumpLog(hKey, hHash, dwFlags, pbData, pdwDataLen, dwBufLen, size);
                    }

                    if (AsyncLogQueue.IsSubscribed)
                    {
                        var cbytes = new byte[size];
                        Marshal.Copy(pbData, cbytes, 0, size);
                        if (cbytes[0] == ZlibHeader)
                        {
                            cbytes = SharedComponents.EveMarshal.Zlib.Decompress(cbytes);
                        }
                        if (cbytes[0] == 0x7E)
                        {
                            var unMarshal = new Unmarshal();
                            var k = unMarshal.Process(cbytes, null);

                            var pp = new PrettyPrinter();
                            AsyncLogQueue.Enqueue(new LogEntry(pp.Print(k), "Sent", null));
                        }

                    }

                }
            }
            catch (Exception e)
            {
                //Console.WriteLine(e);
            }

            var result = CryptEncrypt(hKey, hHash, Final, dwFlags, pbData, pdwDataLen, dwBufLen);
            return result;
        }

        #endregion Methods
    }
}