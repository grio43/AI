using EasyHook;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using static HookManager.Win32Hooks.WSAIoctlController;

namespace HookManager.Win32Hooks
{
    public class GetAddrInfoController : IHook, IDisposable
    {
        #region Fields

        public static ConcurrentDictionary<string, string> Hosts = new ConcurrentDictionary<string, string>();
        public static ConcurrentDictionary<string, string> Domains = new ConcurrentDictionary<string, string>();

        // Constants for address families
        public const int AF_INET = 2;       // IPv4
        public const int AF_INET6 = 23;     // IPv6

        // Constants for socket types
        public const int SOCK_STREAM = 1;   // Stream socket (TCP)
        public const int SOCK_DGRAM = 2;    // Datagram socket (UDP)

        // Constants for protocol families
        public const int PF_UNSPEC = 0;     // Unspecified

        // Enum for error codes
        public enum GetAddrInfoError
        {
            Success = 0,
            HostNotFound = 11001,
            TryAgain = 11002,
            NoRecovery = 11003,
            NoData = 11004,
            NoAddress = NoData
        }

        // Structure representing the addrinfo structure in C
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        struct ADDRINFO
        {
            public int ai_flags;
            public int ai_family;
            public int ai_socktype;
            public int ai_protocol;
            public int ai_addrlen;
            public IntPtr ai_canonname;
            public IntPtr ai_addr;
            public IntPtr ai_next;
        }


        // Declare the DllImport attribute to import the getnameinfo function from the Winsock library
        [DllImport("ws2_32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        public static extern int getnameinfo(
            IntPtr pSockaddr,
            int sockaddrLength,
            IntPtr pNodeBuffer,
            int nodeBufferSize,
            IntPtr pServiceBuffer,
            int serviceBufferSize,
            int flags
        );

        private LocalHook _hook;

        // Declare the DllImport attribute to import the getaddrinfo function from the Winsock library
        [DllImport("ws2_32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        public static extern int getaddrinfo(
            IntPtr pNodeName,
            IntPtr pServiceName,
            IntPtr pHints,
            out IntPtr ppResult
        );

        #endregion Fields

        #region Constructors

        public GetAddrInfoController()
        {
            Error = false;
            Name = typeof(GetAddrInfoController).Name;

            try
            {

                _hook = LocalHook.Create(
                    LocalHook.GetProcAddress("ws2_32.dll", "getaddrinfo"),
                    new GetaddrinfoDelegate(GetaddrinfoDetour),
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

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi, SetLastError = true)]
        private delegate int GetaddrinfoDelegate(IntPtr node, IntPtr service, IntPtr hints, out IntPtr result);

        // Freeaddrinfo function to release memory allocated by getaddrinfo
        [DllImport("ws2_32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        public static extern void freeaddrinfo(
            IntPtr pAddrInfo
        );

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

        private static int GetaddrinfoDetour(IntPtr node, IntPtr service, IntPtr hints, out IntPtr result)
        {
            string requestedDomain = Marshal.PtrToStringAnsi(node);
            string requestedService = Marshal.PtrToStringAnsi(service);

            // Call the original getaddrinfo function
            int res = getaddrinfo(node, service, hints, out result);

            var addrInfo = Marshal.PtrToStructure<ADDRINFO>(result);

            Debug.WriteLine($"GetaddrinfoDetour called. Node [{requestedDomain}] Service [{requestedService}] Protocol [{addrInfo.ai_protocol}] Family [{addrInfo.ai_family}] ai_addrlen [{addrInfo.ai_addrlen}] AIAddr [{addrInfo.ai_addr}]  ai_next [{addrInfo.ai_next}]");

            try
            {
                if (res == 0 && addrInfo.ai_addr != IntPtr.Zero)
                {
                    //while (addrInfo.ai_next != IntPtr.Zero)
                    //{
                    //    if (addrInfo.ai_addr != IntPtr.Zero)
                    //        break;

                    //    addrInfo = Marshal.PtrToStructure<ADDRINFO>(addrInfo.ai_next);
                    //}

                    //if (addrInfo.ai_addr == IntPtr.Zero)
                    //{
                    //    Debug.WriteLine($"No address found for [{requestedDomain}]");
                    //    return res;
                    //}

                    // Marshal the sockaddr structure from the pointer
                    sockaddr_in addr4 = Marshal.PtrToStructure<sockaddr_in>(addrInfo.ai_addr);
                    sockaddr_in6 addr6 = Marshal.PtrToStructure<sockaddr_in6>(addrInfo.ai_addr);

                    IPAddress ipAddress = IPAddress.None;
                    int port = 0;
                    if (addr4.sin_family == (ushort)AddressFamily.InterNetwork)
                    {
                        byte[] ipBytes = BitConverter.GetBytes(addr4.sin_addr.S_addr);
                        ipAddress = new IPAddress(ipBytes);
                        port = BitConverter.ToUInt16(BitConverter.GetBytes(addr4.sin_port).Reverse().ToArray(), 0);
                    }
                    else if (addr6.sin6_family == (ushort)AddressFamily.InterNetworkV6)
                    {
                        ipAddress = new IPAddress(addr6.sin6_addr.u6_addr8);
                        ipAddress = ConvertIPv6MappedToIPv4(ipAddress);
                        port = BitConverter.ToUInt16(BitConverter.GetBytes(addr6.sin6_port).Reverse().ToArray(), 0);
                    }

                    Debug.WriteLine($"Resolved [{requestedDomain}] to [{ipAddress.ToString()}]");
                    Hosts.AddOrUpdate(ipAddress.ToString(), requestedDomain, (key, oldValue) => requestedDomain);
                    Domains.AddOrUpdate(requestedDomain, ipAddress.ToString(), (key, oldValue) => ipAddress.ToString());
                }
                //else
                //{
                //    //Debug.WriteLine($"getaddrinfo failed with error code: {res}");
                //}
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }

            return res;
        }

        #endregion Methods
    }
}