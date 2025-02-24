/*
 * ---------------------------------------
 * User: duketwo
 * Date: 21.06.2014
 * Time: 16:57
 *
 * ---------------------------------------
 */

using EasyHook;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace HookManager.Win32Hooks
{
    /// <summary>
    ///     Description of GetAdaptersInfoController.
    /// </summary>
    public class GetAdaptersInfoController : IDisposable, IHook
    {
        #region Fields

        private string _address;

        private string _guid;
        private LocalHook _hook;
        private string _mac;

        private string _name;
        private string _adapterName;

        #endregion Fields

        #region Constructors

        public GetAdaptersInfoController(IntPtr address, string guid, string mac, string ipaddress, string adapterName)
        {
            Name = typeof(GetAdaptersInfoController).Name;
            _guid = guid;
            _mac = mac.Replace("-", "");
            _address = ipaddress;

            try
            {
                _adapterName = adapterName;
                _name = string.Format("GetAdaptersInfoHook_{0:X}", address.ToInt64());
                _hook = LocalHook.Create(address, new GetAdaptersInfoDelegate(GetAdaptersInfoDetour), this);
                _hook.ThreadACL.SetExclusiveACL(new Int32[] { });
            }
            catch (Exception)
            {
                Error = true;
            }
        }

        #endregion Constructors

        #region Delegates

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi, SetLastError = true)]
        private delegate int GetAdaptersInfoDelegate(IntPtr AdaptersInfo, IntPtr OutputBuffLen);

        #endregion Delegates

        #region Properties

        public bool Error { get; set; }

        public string Name { get; set; }

        #endregion Properties

        #region Methods

        [DllImport("Iphlpapi.dll", SetLastError = true)]
        public static extern int GetAdaptersInfo(IntPtr AdaptersInfo, IntPtr OutputBuffLen);

        public void Dispose()
        {
            if (_hook == null)
                return;

            _hook.Dispose();
            _hook = null;
        }

        private string ByteArrayToString(byte[] arr)
        {
            var enc = new ASCIIEncoding();
            return enc.GetString(arr);
        }

        private int GetAdaptersInfoDetour(IntPtr AdaptersInfo, IntPtr OutputBuffLen)
        {
            var result = GetAdaptersInfo(AdaptersInfo, OutputBuffLen);
            //Debug.WriteLine("GetAdaptersInfoDetour");
            try
            {
            if (AdaptersInfo != IntPtr.Zero)
            {
                var structureBefore = (IP_ADAPTER_INFO)Marshal.PtrToStructure(AdaptersInfo, typeof(IP_ADAPTER_INFO));
                var macBefore = BitConverter.ToString(structureBefore.Address);
                    structureBefore.AddressLength = 6;
                HookManagerImpl.Log(
                        "[BEFORE] " + structureBefore.IpAddressList.IpAddress.Address.ToString() + " [GUID] " + structureBefore.AdapterName + " [MAC] " + macBefore + " [ADAPTER_NAME] " + structureBefore.AdapterName,
                    Color.Orange);
                structureBefore.AdapterName = "{" + _guid.ToUpper() + "}";
                int k = 0;
                for (var i = 0; k < structureBefore.AddressLength; i = i + 2)
                {
                    structureBefore.Address[k] = Convert.ToByte(_mac[i].ToString() + _mac[i + 1].ToString(), 16);
                    k++;
                }

                structureBefore.Next = IntPtr.Zero;
                structureBefore.IpAddressList.IpAddress.Address = _address;
                    structureBefore.AdapterName = "{" + _guid.ToUpper() + "}"; ;
                    structureBefore.AdapterDescription = _adapterName;
                Marshal.StructureToPtr(structureBefore, AdaptersInfo, true);
                var structureAfter = (IP_ADAPTER_INFO)Marshal.PtrToStructure(AdaptersInfo, typeof(IP_ADAPTER_INFO));
                var macAfter = BitConverter.ToString(structureAfter.Address);

                HookManagerImpl.Log(
                        "[AFTER] " + structureAfter.IpAddressList.IpAddress.Address.ToString() + " [GUID] " + structureAfter.AdapterName + " [MAC] " + macAfter + " [ADAPTER_NAME] " + structureAfter.AdapterName,
                    Color.Orange);
            }
            }
            catch (Exception ex)
            {

                Debug.WriteLine(ex.ToString());
            }


            return result;
        }

        #endregion Methods

        #region Structs

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct IP_ADAPTER_INFO
        {
            public IntPtr Next;
            public Int32 ComboIndex;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256 + 4)] public string AdapterName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128 + 4)] public string AdapterDescription;
            public UInt32 AddressLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)] public byte[] Address;
            public Int32 Index;
            public UInt32 Type;
            public UInt32 DhcpEnabled;
            public IntPtr CurrentIpAddress;
            public IP_ADDR_STRING IpAddressList;
            public IP_ADDR_STRING GatewayList;
            public IP_ADDR_STRING DhcpServer;
            public bool HaveWins;
            public IP_ADDR_STRING PrimaryWinsServer;
            public IP_ADDR_STRING SecondaryWinsServer;
            public Int32 LeaseObtained;
            public Int32 LeaseExpires;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct IP_ADDR_STRING
        {
            public IntPtr Next;
            public IP_ADDRESS_STRING IpAddress;
            public IP_ADDRESS_STRING IpMask;
            public Int32 Context;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct IP_ADDRESS_STRING
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)] public string Address;
        }

        #endregion Structs
    }
}