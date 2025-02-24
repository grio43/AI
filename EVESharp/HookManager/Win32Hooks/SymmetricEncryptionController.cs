using EasyHook;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using SharedComponents.EVE;
using SharedComponents.EveMarshal;
using SharedComponents.IPC;
using SharedComponents.Py;
using SharedComponents.SharedMemory;
using SharedComponents.Utility;
using SharedComponents.Utility.AsyncLogQueue;

namespace HookManager.Win32Hooks
{
    public class SymmetricEncryptionController : IHook, IDisposable
    {
        #region Fields

        private LocalHook _hook;

        private Delegate _origiFunc;

        public static AsyncLogQueue AsyncLogQueue = AsyncLogQueue = new AsyncLogQueue();
        private HWSettings _hwSettings;
        private SharedArray<bool> _sharedArray;
        #endregion Fields

        #region Constructors

        public SymmetricEncryptionController(IntPtr funcAddr, HWSettings hwSettings)
        {
            Error = false;
            _hwSettings = hwSettings;
            Name = typeof(SymmetricEncryptionController).Name;

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

        [DllImport("MemMan.dll")]
        public static extern void SendPacket(IntPtr byteArrayPtr, int length);

        #endregion Delegates

        #region Properties

        public bool Error { get; set; }
        public string Name { get; set; }

        private long _packetCnt;

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

        private void VerifiyRCode(PyDict dict)
        {
            // md5 of the mac
            string s = _hwSettings.MacAddress;
            s = s.Replace("-", "");
            var bytes = Util.StringToByteArray(s);
            s = Util.PyRepr(bytes);
            var mac = $"('{s}',)";
            if (s.Contains("'"))
                mac = $"(\"{s}\",)";
            Debug.WriteLine("mac: " + mac);
            var macHash = Util.GetMd5Hash(mac);

            //setup the dict to compare
            var d = new Dictionary<string, string>()
            {
                { "card_name", _hwSettings.GpuDescription },
                { "adapter_deviceid", _hwSettings.GpuDeviceId.ToString() },
                { "adapter_vendorid", _hwSettings.GpuVendorId.ToString() },
                { "launcher_machine_hash", _hwSettings.LauncherMachineHash.Replace("-","") },
                { "network_adapters", _hwSettings.NetworkAdapterGuid.Replace("-","").ToUpper() + "\n"},
                { "video_adapter", "00000000000000000000000000000000" },
                { "adapter_driver_version", _hwSettings.GpuDriverversion.ToString() },
                { "network_computername", _hwSettings.Computername.ToUpper()},
                { "os_md5hash", Util.GetMd5Hash(_hwSettings.WindowsKey)},
                { "network_mac", macHash },
                { "host_ram", (_hwSettings.TotalPhysRam - _hwSettings.SystemReservedMemory + 1).ToString() },
            };

            foreach (var kvp in d)
            {
                if (dict.Contains(kvp.Key))
                {
                    var val = dict[kvp.Key].StringValue ?? dict[kvp.Key].IntValue.ToString();
                    var kvpVal = kvp.Value;

                    //HookManager.Log.RemoteWriteLine($"VAL {val}");
                    if (!val.Equals(kvpVal))
                        throw new Exception($"Value [{val}] of [{kvp.Key}] does not equal the expected value [{kvpVal}]");
                }
                else
                {
                    throw new Exception($"Key {kvp.Key} was not found in the RCode dictionary.");
                }
            }

            // check vertex and pixel shader version > 5
            //if (dict["pixel_shader_version"].IntValue < 5)
            //    throw new Exception($"PixelShader < 5.");

            //if (dict["vertex_shader_version"].IntValue < 5)
            //    throw new Exception($"VertexShader < 5.");
        }

        private IntPtr Detour(IntPtr self, IntPtr args)
        {

            byte[] _recPacketBytes = null;
            //Log($"SymmetricEncryptionController Hook proc!");
            try
            {
                using (var pySharp = new PySharp(false))
                {
                    var argList = new PyObject(pySharp, args, false);


                    var cbytes = argList.ToList()[0].GetStringBytes();
                    if (cbytes[0] == 0x78)
                    {
                        cbytes = SharedComponents.EveMarshal.Zlib.Decompress(cbytes);
                    }

                    _recPacketBytes = cbytes;

                    if (cbytes[0] == 0x7E)
                    {
                        try
                        {
                            unsafe
                            {
                                fixed (byte* pointerToFirst = cbytes)
                                {
                                    SendPacket((IntPtr)pointerToFirst, cbytes.Length);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                        _packetCnt++;
                        if (_packetCnt < 100 || AsyncLogQueue.IsSubscribed) // always capture the first 100 packets
                        {
                            var unMarshal = new Unmarshal();
                            var k = unMarshal.Process(cbytes, null);
                            var pp = new PrettyPrinter();
                            AsyncLogQueue.Enqueue(new LogEntry(pp.Print(k), $"Sent {cbytes.Length,7}b", null));

                            if (_packetCnt == 1) // first packet is always the first rcode return value
                            {
                                try
                                {
                                    var dict = k[2] as PyDict;
                                    HookManager.Log.WriteLine($"Values going to the server [{dict.Dictionary.Count}]:");
                                    int i = 1;
                                    foreach (var kvp in dict.Dictionary)
                                    {
                                        HookManager.Log.WriteLine($"[{i:00}] KEY {kvp.Key.ToString()} VALUE {kvp.Value.ToString()}");
                                        i++;
                                    }

                                    HookManager.Log.WriteLine("Verifying values submitted to the server.");
                                    VerifiyRCode(dict);
                                    HookManager.Log.WriteLine("Verified values.", Color.LightGreen);
                                    _sharedArray = new SharedComponents.SharedMemory.SharedArray<bool>(HookManagerImpl.Instance.CharName + nameof(UsedSharedMemoryNames.RcodeVerified), 1);
                                    _sharedArray[0] = true;
                                }
                                catch (Exception e)
                                {
                                    HookManager.Log.RemoteWriteLine($"Error: {e}");
                                    HookManagerImpl.Instance.ForceQuit($"Error: {e}");
                                }
                            }
                        }
                    }
                    else
                    {
                        AsyncLogQueue.Enqueue(new LogEntry(argList.LogObject(), "PACKET_FAILURE", null));
                        HookManager.Log.RemoteWriteLine("Error: PACKET_FAILURE");
                        HookManager.Log.RemoteWriteLine(argList.LogObject());
                        //HookManagerImpl.Instance.ForceQuit("Error: PACKET_FAILURE");
                        // exit on packet failure
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("SENT ERROR: " + e);
                Debug.WriteLine("SENT ERROR: " + e);

                //using (var pySharp = new PySharp(true))
                //{
                //    var blueMarshal = pySharp.Import("blue").Attribute("marshal");
                //    var stringTable = blueMarshal.Attribute("stringTableRev");
                //    var keywordArgs = new Dictionary<string, object>()
                //    {
                //        ["buffer"] = _recPacketBytes,
                //        ["callback"] = PySharp.PyNone,
                //        ["skipCrcCheck"] = true,
                //        ["offset"] = 0,
                //        ["stringTable"] = stringTable
                //    };
                //    var pyObjectMarshal = blueMarshal.CallWithKeywords("Load", keywordArgs);
                //    Console.WriteLine("pyObjectMarshal: " + pyObjectMarshal.IsValid);
                //    Console.WriteLine($"{pyObjectMarshal.LogObject()}");
                //}
            }

            var res = _origiFunc(self, args);

            try
            {
                using (var pySharp = new PySharp(false))
                {
                    var resultObj = new PyObject(pySharp, res, false);
                    //HookManager.Log.RemoteWriteLine(resultObj.LogObject());

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Debug.WriteLine(e);
            }
            return res;

        }
        #endregion Methods
    }
}