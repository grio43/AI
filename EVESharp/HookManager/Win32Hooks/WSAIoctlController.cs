using EasyHook;
using SharedComponents.EveMarshal;
using SharedComponents.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using static HookManager.Win32Hooks.GetAdaptersAddressesController;
using System.Threading;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using SharedComponents.IPC;

namespace HookManager.Win32Hooks
{
    public class WSAIoctlController : IHook, IDisposable
    {
        #region Fields

        private LocalHook _WSAIoctlhook;
        private LocalHook _connectExHook;
        private LocalHook _WSAGetOverlappedResultHook;
        private LocalHook _bindHook;
        private LocalHook _WSASendHook;

        public const int AF_INET = 2;
        public const long FIONBIO = 0x8004667E;
        public const string INADDR_ANY = "0.0.0.0";
        public const int INVALID_SOCKET = ~0;
        public const int PPROTO_TCP = 6;
        public const int SOCK_STREAM = 1;
        public const int SOCKET_ERROR = -1;
        public const int WSAEALREADY = 10037;

        public const int WSAEINVAL = 10022;

        public const int WSAEISCONN = 10056;

        public const int WSAENOTCONN = 10057;

        public const int WSAEWOULDBLOCK = 10035;

        public const int WSANOERROR = 0;

        private string proxyIp, proxyPort, proxyUser, proxyPass;

        [DllImport("ws2_32.dll", SetLastError = false)]
        private static extern int WSAIoctl(
            IntPtr socket,
            uint dwIoControlCode,
            IntPtr lpvInBuffer,
            uint cbInBuffer,
            out IntPtr lpvOutBuffer,
            uint cbOutBuffer,
            out uint lpcbBytesReturned,
            IntPtr lpOverlapped,
            IntPtr lpCompletionRoutine);

        private static ConcurrentDictionary<IntPtr, (string, ushort, bool)> _concDict;

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern void SetLastError(int errorCode);

        // Import the bind function from the Winsock API
        [DllImport("ws2_32.dll", SetLastError = true)]
        public static extern int bind(
            IntPtr socketHandle,     // Socket handle (IntPtr)
            IntPtr address, // Pointer to a sockaddr_in structure
            int addressSize          // Size of the sockaddr_in structure in bytes
        );


        // Import the WSASend function from the Winsock API
        [DllImport("ws2_32.dll", SetLastError = true)]
        public static extern int WSASend(
            IntPtr socketHandle,                    // Socket handle (IntPtr)
            ref WSABuffer lpBuffers,                // Pointer to an array of WSABuffer structures
            uint dwBufferCount,                     // Number of WSABuffer structures in the array
            out uint lpNumberOfBytesSent,           // Number of bytes sent (out parameter)
            uint dwFlags,                           // Flags
            IntPtr lpOverlapped,                    // Pointer to an OVERLAPPED structure (IntPtr)
            IntPtr lpCompletionRoutine              // Pointer to a completion routine (IntPtr)
        );

        [DllImport("ws2_32.dll", SetLastError = true)]
        public static extern int WSARecv(
            IntPtr socketHandle,                    // Socket handle (IntPtr)
            ref WSABuffer lpBuffers,                // Pointer to an array of WSABuffer structures
            uint dwBufferCount,                     // Number of WSABuffer structures in the array
            out uint lpNumberOfBytesRecvd,          // Number of bytes received (out parameter)
            ref uint lpFlags,                       // Flags
            IntPtr lpOverlapped,                    // Pointer to an OVERLAPPED structure (IntPtr)
            IntPtr lpCompletionRoutine              // Pointer to a completion routine (IntPtr)
        );

        // Define the WSABuffer structure
        [StructLayout(LayoutKind.Sequential)]
        public struct WSABuffer
        {
            public uint Length;                     // Length of the buffer
            public IntPtr Pointer;                  // Pointer to the buffer
        }

        #endregion Fields

        #region Constructors

        public WSAIoctlController(string proxyIp, string proxyPort, string proxyUser, string proxyPass)
        {
            _concDict = new ConcurrentDictionary<IntPtr, (string, ushort, bool)>();
            Error = false;
            Name = typeof(WSAIoctlController).Name;
            this.proxyIp = proxyIp;
            this.proxyPort = proxyPort;
            this.proxyUser = proxyUser;
            this.proxyPass = proxyPass;

            try
            {
                _WSAIoctlhook = LocalHook.Create(
                    LocalHook.GetProcAddress("ws2_32.dll", "WSAIoctl"),
                    new WSAIoctlDelegate(WSAIoctlDetour),
                    this);

                _WSAIoctlhook.ThreadACL.SetExclusiveACL(new Int32[] { });

                _WSAGetOverlappedResultHook = LocalHook.Create(LocalHook.GetProcAddress("ws2_32.dll", "WSAGetOverlappedResult"), new WSAGetOverlappedResultDelegate(WSAGetOverlappedResultDetour), this);
                _WSAGetOverlappedResultHook.ThreadACL.SetExclusiveACL(new Int32[] { });


                _bindHook = LocalHook.Create(LocalHook.GetProcAddress("ws2_32.dll", "bind"), new BindDelegate(BindDetour), this);
                _bindHook.ThreadACL.SetExclusiveACL(new Int32[] { });

                _WSASendHook = LocalHook.Create(LocalHook.GetProcAddress("ws2_32.dll", "WSASend"), new WSASendDelegate(WSASendDetour), this);
                _WSASendHook.ThreadACL.SetExclusiveACL(new Int32[] { });

                Error = false;
            }
            catch (Exception)
            {
                Error = true;
            }
        }

        private int WSASendDetour(
                       IntPtr socketHandle, // Socket handle (IntPtr)
                                                        ref WSABuffer lpBuffers, // Pointer to an array of WSABuffer structures
                                                        uint dwBufferCount, // Number of WSABuffer structures in the array
                                                        out uint lpNumberOfBytesSent, // Number of bytes sent (out parameter)
                                                        uint dwFlags, // Flags
                                                        IntPtr lpOverlapped, // Pointer to an OVERLAPPED structure (IntPtr)
                                                        IntPtr lpCompletionRoutine // Pointer to a completion routine (IntPtr)
                                                    )
        {

            try
            {

                if (!SocketInfo.SocketsInfoDictionary.ContainsKey(socketHandle) && (_concDict.ContainsKey(socketHandle)))
                {
                    var ep = GetLocalEndPoint(socketHandle);
                    if (ep != null)
                    {
                        var socketInfo = new SocketInfo(
                            ep.Address,
                            ep.Port,
                            IPAddress.Parse(proxyIp),
                            int.Parse(proxyPort),
                            IPAddress.Parse(_concDict[socketHandle].Item1),
                            _concDict[socketHandle].Item2,
                            string.Empty,
                            socketHandle
                            );
                        SocketInfo.SocketsInfoDictionary.AddOrUpdate(socketHandle, socketInfo);
                    }
                }

                if (_concDict.ContainsKey(socketHandle) && _concDict[socketHandle].Item3 == false)
                {
                    _concDict[socketHandle] = (_concDict[socketHandle].Item1, _concDict[socketHandle].Item2, true);
                    Trace.WriteLine($"WSASendDetour: InitSocks5TunnelOnNativeSocket - socketHandle [{socketHandle.ToString("X")}]");
                    Socks5Helper.InitSocks5TunnelOnNativeSocket(socketHandle, _concDict[socketHandle].Item1, _concDict[socketHandle].Item2, proxyUser, proxyPass, Socks5Helper.WSockMethod.LEGACY);
                    SetLastError(WSAEWOULDBLOCK);
                    lpNumberOfBytesSent = 0;
                    return -1;

                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }

            var res = WSASend(socketHandle, ref lpBuffers, dwBufferCount, out lpNumberOfBytesSent, dwFlags, lpOverlapped, lpCompletionRoutine);
            //Debug.WriteLine($"WSASendDetour: socketHandle [{socketHandle.ToString("X")}] dwBufferCount [{dwBufferCount}] lpNumberOfBytesSent [{lpNumberOfBytesSent}] dwFlags [{dwFlags}] lpOverlapped [{lpOverlapped.ToString("X")}] lpCompletionRoutine [{lpCompletionRoutine.ToString("X")}]");
            return res;
        }

        public int BindDetour(
            IntPtr socketHandle, // Socket handle (IntPtr)
            IntPtr address, // Pointer to a sockaddr_in structure
            int addressSize // Size of the sockaddr_in structure in bytes
        )
        {
            var res = bind(socketHandle, address, addressSize);
            Debug.WriteLine($"BindDetour: socketHandle [{socketHandle.ToString("X")}] address [{address.ToString("X")}] addressSize [{addressSize}]");
            return res;
        }


        private int WSAGetOverlappedResultDetour(IntPtr socketHandle, IntPtr overlappedPtr, out uint bytesTransferred, bool wait, out SocketError socketError)
        {
            return WSAGetOverlappedResult(socketHandle, overlappedPtr, out bytesTransferred, true, out socketError);
        }

        #endregion Constructors

        #region Delegates

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate int WSAIoctlDelegate(IntPtr socket,
            uint dwIoControlCode,
            IntPtr lpvInBuffer,
            uint cbInBuffer,
            out IntPtr lpvOutBuffer,
            uint cbOutBuffer,
            out uint lpcbBytesReturned,
            IntPtr lpOverlapped,
            IntPtr lpCompletionRoutine);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate int BindDelegate(IntPtr socketHandle, // Socket handle (IntPtr)
            IntPtr address, // Pointer to a sockaddr_in structure
            int addressSize);          // Size of the sockaddr_in structure in bytes);


        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate int WSASendDelegate(
            IntPtr socketHandle, // Socket handle (IntPtr)
            ref WSABuffer lpBuffers, // Pointer to an array of WSABuffer structures
            uint dwBufferCount, // Number of WSABuffer structures in the array
            out uint lpNumberOfBytesSent, // Number of bytes sent (out parameter)
            uint dwFlags, // Flags
            IntPtr lpOverlapped, // Pointer to an OVERLAPPED structure (IntPtr)
            IntPtr lpCompletionRoutine // Pointer to a completion routine (IntPtr)
        );

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate int ConnectExDelegate(
            IntPtr s,
            IntPtr name,
            int namelen,
            IntPtr lpSendBuffer,
            uint dwSendDataLength,
            ref uint lpdwBytesSent,
            IntPtr lpOverlapped
        );

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate int WSAGetOverlappedResultDelegate(
              IntPtr socketHandle, // Socket handle
              IntPtr overlappedPtr, // Pointer to the OVERLAPPED structure
              out uint bytesTransferred, // Number of bytes transferred
              bool wait, // Wait flag
              out SocketError socketError // Socket error code
        );

        #endregion Delegates

        #region Properties

        public bool Error { get; set; }
        public string Name { get; set; }

        #endregion Properties

        [DllImport("ws2_32.dll", SetLastError = false)]
        public static extern int WSAGetOverlappedResult(
              IntPtr socketHandle, // Socket handle
              IntPtr overlappedPtr, // Pointer to the OVERLAPPED structure
              out uint bytesTransferred, // Number of bytes transferred
              bool wait, // Wait flag
              out SocketError socketError // Socket error code
          );

        [DllImport("Ws2_32.dll")]
        public static extern ushort htons(ushort hostshort);

        [DllImport("Ws2_32.dll", CharSet = CharSet.Ansi)]
        public static extern uint inet_addr(string cp);

        [DllImport("Ws2_32.dll")]
        public static extern ushort ntohs(ushort netshort);

        public ushort IPV4 = 2;
        public ushort IPV6 = 10;

        [StructLayout(LayoutKind.Sequential, Size = 16)]
        public struct sockaddr_in
        {
            public const int Size = 16;

            public short sin_family;
            public ushort sin_port;

            public struct in_addr
            {
                public uint S_addr;

                public struct _S_un_b
                {
                    public byte s_b1, s_b2, s_b3, s_b4;
                }

                public _S_un_b S_un_b;

                public struct _S_un_w
                {
                    public ushort s_w1, s_w2;
                }

                public _S_un_w S_un_w;
            }

            public in_addr sin_addr;
        }

        private bool _showDebugLogs = true;
        private void WriteTrace(string msg)
        {
            if (_showDebugLogs)
            {
                Trace.WriteLine(msg);
            }
        }


        [StructLayout(LayoutKind.Sequential)]
        public struct in6_addr
        {
            [System.Runtime.InteropServices.MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] u6_addr8; // Array to store IPv6 address bytes
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct sockaddr_in6
        {
            public ushort sin6_family;         // Address family, AF_INET6
            public ushort sin6_port;           // Port number
            public uint sin6_flowinfo;         // IPv6 flow information
            public in6_addr sin6_addr;         // IPv6 address
            public uint sin6_scope_id;         // Scope ID (interface index)
        }

        public static string IPv6ToString(in6_addr ipv6Address)
        {
            byte[] ipv6Bytes = ipv6Address.u6_addr8;
            IPAddress ipAddress = new IPAddress(ipv6Bytes);
            return ipAddress.ToString();
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SOCKADDR
        {
            public ushort sin_family;
        }

        public static IPAddress ConvertIPv6MappedToIPv4(IPAddress ipv6Address)
        {
            byte[] addressBytes = ipv6Address.GetAddressBytes();

            // Check if it's an IPv6-mapped IPv4 address
            if (addressBytes.Length == 16 &&
                addressBytes[0] == 0x00 && addressBytes[1] == 0x00 &&
                addressBytes[2] == 0x00 && addressBytes[3] == 0x00 &&
                addressBytes[4] == 0x00 && addressBytes[5] == 0x00 &&
                addressBytes[6] == 0x00 && addressBytes[7] == 0x00 &&
                addressBytes[8] == 0x00 && addressBytes[9] == 0x00 &&
                addressBytes[10] == 0xFF && addressBytes[11] == 0xFF)
            {
                byte[] ipv4Bytes = new byte[4];
                Array.Copy(addressBytes, 12, ipv4Bytes, 0, 4);
                return new IPAddress(ipv4Bytes);
            }
            else
            {
                return ipv6Address;
            }
        }

        // Import the getsockname function from the ws2_32.dll
        [DllImport("ws2_32.dll", SetLastError = true)]
        public static extern int getsockname(IntPtr s, IntPtr name, ref int namelen);


        // Function to get the local endpoint information using native Winsock functions
        public static IPEndPoint? GetLocalEndPoint(IntPtr socketHandle)
        {
            byte[] addressBytes = new byte[28]; // Maximum size for IPv6
            int addressLength = addressBytes.Length;

            // Allocate memory for the sockaddr structure
            IntPtr sockaddrPtr = Marshal.AllocHGlobal(addressLength);

            // Call getsockname function
            int result = getsockname(socketHandle, sockaddrPtr, ref addressLength);

            if (result != 0)
            {
                Marshal.FreeHGlobal(sockaddrPtr); // Free allocated memory
                return null; // Unable to retrieve local endpoint
            }

            // Marshal the sockaddr structure from the pointer
            sockaddr_in addr4 = Marshal.PtrToStructure<sockaddr_in>(sockaddrPtr);
            sockaddr_in6 addr6 = Marshal.PtrToStructure<sockaddr_in6>(sockaddrPtr);

            IPAddress ipAddress;
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
            else
            {
                Marshal.FreeHGlobal(sockaddrPtr); // Free allocated memory
                return null; // Unsupported address family
            }

            Marshal.FreeHGlobal(sockaddrPtr); // Free allocated memory

            return new IPEndPoint(ipAddress, port);
        }

        private static IntPtr CreateAddrIPv6(string ip, string port)
        {
            var s = Marshal.AllocHGlobal(Marshal.SizeOf<sockaddr_in6>());
            var sockAddr = new sockaddr_in6
            {
                sin6_family = 23, // AF_INET6 value
                sin6_port = htons(Convert.ToUInt16(port)),
                sin6_flowinfo = 0, // You might adjust this according to your requirements
                sin6_scope_id = 0  // Scope ID (interface index)
            };

            // Convert IPv6 address string to byte array
            byte[] ipv6Bytes = IPAddress.Parse(ip).GetAddressBytes();
            sockAddr.sin6_addr.u6_addr8 = ipv6Bytes;

            Marshal.StructureToPtr(sockAddr, s, true);
            return s;
        }

        #region Methods
        // GUID for ConnectEx
        private static readonly Guid WSAID_CONNECTEX = new Guid("25A207B9-DDF3-4660-8EE9-76E58C74063E");

        private const uint SIO_GET_EXTENSION_FUNCTION_POINTER = ((0x80000000 | 0x40000000) | (0x08000000) | (6));

        public static bool ConnectExHookCreated;

        public void Dispose()
        {
            _WSAIoctlhook.Dispose();
        }


        private IntPtr CreateAddr(string ip, string port)
        {
            var s = Marshal.AllocHGlobal(16);
            var sockAddr = new sockaddr_in();
            sockAddr.sin_addr.S_addr = inet_addr(ip);
            sockAddr.sin_port = htons(Convert.ToUInt16(port));
            sockAddr.sin_family = 2;
            Marshal.StructureToPtr(sockAddr, s, true);
            return s;
        }

        private static ConnectExDelegate _connectExDelegate = null;

        private int DetourConnectEx(
            IntPtr s, // the ptr/handle to the socket
            IntPtr sockAddr, // a sockaddr structure that specifies the address to which to connect. For IPv4, the sockaddr contains AF_INET for the address family, the destination IPv4 address
            int addrsize, // The length, in bytes, of the sockaddr structure pointed to by the name parameter.
            IntPtr lpSendBuffer, // optional, A pointer to the buffer to be transferred after a connection is established.
            uint dwSendDataLength, // optional, length of above buffer
            ref uint lpdwBytesSent, // On successful return, this parameter points to a DWORD value that indicates the number of bytes that were sent after the connection was established. The bytes sent are from the buffer pointed to by the lpSendBuffer parameter.
            IntPtr lpOverlapped) // An OVERLAPPED structure used to process the request. The lpOverlapped parameter must be specified, and cannot be NULL. // https://learn.microsoft.com/en-us/windows/win32/api/minwinbase/ns-minwinbase-overlapped
        {
            Debug.WriteLine($"DetourConnectEx call! dwSendDataLength [{dwSendDataLength}]");
            SOCKADDR sAddr = new SOCKADDR();

            try
            {
                if (_connectExDelegate == null)
                {
                    _connectExDelegate = Marshal.GetDelegateForFunctionPointer<ConnectExDelegate>(_connectExHook.HookBypassAddress);
                }
                sAddr = (SOCKADDR)Marshal.PtrToStructure(sockAddr, typeof(SOCKADDR));

                // if it's not ipv4 and ipv6, just return
                if (sAddr.sin_family != (short)AddressFamily.InterNetwork && sAddr.sin_family != (short)AddressFamily.InterNetworkV6)
                {
                    return _connectExDelegate(s, sockAddr, addrsize, lpSendBuffer, dwSendDataLength, ref lpdwBytesSent, lpOverlapped);
                }

            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }

            if (_connectExDelegate == null)
            {
                return -1;
            }

            var allocatedMemory = new List<IntPtr>();
            try
            {
                IntPtr addrStruct = IntPtr.Zero;
                int addrStructSize = 0;
                string remoteIp = string.Empty;
                ushort remotePort = 0;

                if (sAddr.sin_family == (short)AddressFamily.InterNetwork)
                {
                    var ipv4Struct = (sockaddr_in)Marshal.PtrToStructure(sockAddr, typeof(sockaddr_in));
                    remoteIp = new IPAddress(ipv4Struct.sin_addr.S_addr).ToString();
                    remotePort = ntohs(ipv4Struct.sin_port);
                    Debug.WriteLine($"ConnectEx IP4 RemoteIp [{remoteIp}] RemotePort [{remotePort}] Family [{sAddr.sin_family}] Socket Handle [{s.ToString("X")}]");
                    addrStruct = CreateAddr(proxyIp, proxyPort.ToString()); // We need to clear that, else we're leaking
                    allocatedMemory.Add(addrStruct);
                    Marshal.StructureToPtr(Marshal.PtrToStructure<sockaddr_in>(addrStruct), sockAddr, true);
                }

                if (sAddr.sin_family == (short)AddressFamily.InterNetworkV6)
                {
                    var ipv6Struct = (sockaddr_in6)Marshal.PtrToStructure(sockAddr, typeof(sockaddr_in6));
                    remoteIp = IPv6ToString(ipv6Struct.sin6_addr).ToString().Substring(7);
                    remotePort = ntohs(ipv6Struct.sin6_port);
                    Debug.WriteLine($"ConnectEx IP6 RemoteIp [{remoteIp}] RemotePort [{remotePort}] Family [{sAddr.sin_family}] Socket Handle [{s.ToString("X")}]");
                    addrStruct = CreateAddrIPv6("::ffff:" + proxyIp, proxyPort.ToString()); // We need to clear that, else we're leaking
                    allocatedMemory.Add(addrStruct);
                    Marshal.StructureToPtr(Marshal.PtrToStructure<sockaddr_in6>(addrStruct), sockAddr, true);
                }

                if (!string.IsNullOrWhiteSpace(remoteIp) && !remoteIp.Contains("127.0.0.1") && remotePort != 0)
                {

                    try
                    {
                        _concDict.AddOrUpdate(s, (remoteIp, remotePort, false), (key, oldValue) => (remoteIp, remotePort, false));
                        // Initiate the ConnectEx operation using the dedicated OVERLAPPED structure
                        var result = _connectExDelegate(s, sockAddr, addrsize, lpSendBuffer, dwSendDataLength,
                            ref lpdwBytesSent, lpOverlapped);

                        Debug.WriteLine($"ConnectEx [REMOTE_IP] " + remoteIp + " [REMOTE_PORT] " + remotePort.ToString());

                        return result;
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.ToString());
                    }
                    finally
                    {
                    }

                }
                else
                {
                    // Directly connect to loopback endpoints
                    var ret = _connectExDelegate(s, sockAddr, addrsize, lpSendBuffer, dwSendDataLength, ref lpdwBytesSent, lpOverlapped);
                    if (ret == 0)
                    {
                        //HookManagerImpl.Log("ConnectExLocal [REMOTE_IP] " + remoteIp + " [REMOTE_PORT] " + remotePort.ToString());
                        //Debug.WriteLine("ConnectExLocal [REMOTE_IP] " + remoteIp + " [REMOTE_PORT] " + remotePort.ToString());
                    }
                    return ret;
                }

            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
            finally
            {
                // clean memory
                try
                {
                    foreach (var ptr in allocatedMemory)
                    {
                        if (ptr != IntPtr.Zero)
                            Marshal.FreeHGlobal(ptr);
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.ToString());
                }

            }
            return -1;
        }

        private int WSAIoctlDetour(IntPtr socket,
            uint dwIoControlCode,
            IntPtr lpvInBuffer,
            uint cbInBuffer,
            out IntPtr lpvOutBuffer,
            uint cbOutBuffer,
            out uint lpcbBytesReturned,
            IntPtr lpOverlapped,
            IntPtr lpCompletionRoutine)
        {
            var res = WSAIoctl(socket, dwIoControlCode, lpvInBuffer, cbInBuffer, out lpvOutBuffer, cbOutBuffer, out lpcbBytesReturned, lpOverlapped, lpCompletionRoutine);

            if (ConnectExHookCreated)
                return res;

            try
            {
                if (lpvInBuffer == IntPtr.Zero || cbInBuffer == 0)
                {
                    return res;
                }

                if (dwIoControlCode == SIO_GET_EXTENSION_FUNCTION_POINTER)
                {
                    // Extract the GUID from lpvInBuffer
                    Guid guidFromBuffer = (Guid)Marshal.PtrToStructure(lpvInBuffer, typeof(Guid));
                    var msg =
                        $"WSAIoctlController.Detour dwIoControlCode [{dwIoControlCode}] cbInBuffer [{cbInBuffer}] Guid [{guidFromBuffer}]";
                    Debug.WriteLine(msg);
                    HookManagerImpl.Log(msg);
                    if (guidFromBuffer == WSAID_CONNECTEX)
                    {
                        Debug.WriteLine("WSAIoctl with WSAID_CONNECTEX detected.");
                        Debug.WriteLine($"WSA_CONNECTEX PTR RETRIEVED! [{lpvOutBuffer.ToString("X")}]");
                        WCFClient.Instance.GetPipeProxy.RemoteLog($"ConnectEx hook added.");
                        _connectExHook = LocalHook.Create(lpvOutBuffer, new ConnectExDelegate(DetourConnectEx), this);
                        _connectExHook.ThreadACL.SetExclusiveACL(new Int32[] { });
                        ConnectExHookCreated = true;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }

            return res;
        }

        #endregion Methods


    }
}