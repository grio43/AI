/*
 * ---------------------------------------
 * User: duketwo
 * Date: 18.05.2020
 * Time: 00:59
 *
 * ---------------------------------------
 */

using EasyHook;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace HookManager.Win32Hooks
{


    public enum MsgFlags
    {
        /// <summary>Processes OOB data.</summary>
        MSG_OOB = 0x1,

        /// <summary>
        /// Peeks at the incoming data. The data is copied into the buffer, but is not removed from the input queue. This flag is valid
        /// only for nonoverlapped sockets.
        /// </summary>
        MSG_PEEK = 0x2,

        /// <summary/>
        MSG_DONTROUTE = 0x4,

        /// <summary>
        /// The receive request will complete only when one of the following events occurs: Be aware that if the underlying transport
        /// provider does not support MSG_WAITALL, or if the socket is in a non-blocking mode, then this call will fail with
        /// WSAEOPNOTSUPP. Also, if MSG_WAITALL is specified along with MSG_OOB, MSG_PEEK, or MSG_PARTIAL, then this call will fail with
        /// WSAEOPNOTSUPP. This flag is not supported on datagram sockets or message-oriented sockets.
        /// </summary>
        MSG_WAITALL = 0x8,

        /// <summary>
        /// This flag is for stream-oriented sockets only. This flag allows an application that uses stream sockets to tell the
        /// transport provider not to delay completion of partially filled pending receive requests. This is a hint to the transport
        /// provider that the application is willing to receive any incoming data as soon as possible without necessarily waiting for
        /// the remainder of the data that might still be in transit. What constitutes a partially filled pending receive request is a
        /// transport-specific matter. In the case of TCP, this refers to the case of incoming TCP segments being placed into the
        /// receive request data buffer where none of the TCP segments indicated a PUSH bit value of 1. In this case, TCP may hold the
        /// partially filled receive request a little longer to allow the remainder of the data to arrive with a TCP segment that has
        /// the PUSH bit set to 1. This flag tells TCP not to hold the receive request but to complete it immediately. Using this flag
        /// for large block transfers is not recommended since processing partial blocks is often not optimal. This flag is useful only
        /// for cases where receiving and processing the partial data immediately helps decrease processing latency. This flag is a hint
        /// rather than an actual guarantee. This flag is supported on Windows 8.1, Windows Server 2012 R2, and later.
        /// </summary>
        MSG_PUSH_IMMEDIATE = 0x20,

        /// <summary>
        /// This flag is for message-oriented sockets only. On output, this flag indicates that the data specified is a portion of the
        /// message transmitted by the sender. Remaining portions of the message will be specified in subsequent receive operations. A
        /// subsequent receive operation with the MSG_PARTIAL flag cleared indicates end of sender's message. As an input parameter,
        /// this flag indicates that the receive operation should complete even if only part of a message has been received by the
        /// transport provider.
        /// </summary>
        MSG_PARTIAL = 0x8000,

        /// <summary/>
        MSG_INTERRUPT = 0x10,

        /// <summary>The datagram was truncated. More data was present than the process allocated room for.</summary>
        MSG_TRUNC = 0x0100,

        /// <summary>The control (ancillary) data was truncated. More control data was present than the process allocated room for.</summary>
        MSG_CTRUNC = 0x0200,

        /// <summary>The datagram was received as a link-layer broadcast or with a destination IP address that is a broadcast address.</summary>
        MSG_BCAST = 0x0400,

        /// <summary>The datagram was received with a destination IP address that is a multicast address.</summary>
        MSG_MCAST = 0x0800,

        /// <summary>
        /// This flag specifies that queued errors should be received from the socket error queue. The error is passed in an ancillary
        /// message with a type dependent on the protocol (for IPv4 IP_RECVERR). The user should supply a buffer of sufficient size.See
        /// cmsg(3) and ip(7) for more information.The payload of the original packet that caused the error is passed as normal data via
        /// msg_iovec. The original destination address of the datagram that caused the error is supplied via msg_name.
        /// </summary>
        MSG_ERRQUEUE = 0x1000,
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct WSABUF
    {
        /// <summary>The length of the buffer, in bytes.</summary>
        public uint len;

        /// <summary>A pointer to the buffer.</summary>
        public IntPtr buf;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WSAOVERLAPPED
    {
        /// <summary>
        /// <para>Type: <c>ULONG_PTR</c></para>
        /// <para>
        /// Reserved for internal use. The Internal member is used internally by the entity that implements overlapped I/O. For service
        /// providers that create sockets as installable file system (IFS) handles, this parameter is used by the underlying operating
        /// system. Other service providers (non-IFS providers) are free to use this parameter as necessary.
        /// </para>
        /// </summary>
        public uint Internal;

        /// <summary>
        /// <para>Type: <c>ULONG_PTR</c></para>
        /// <para>
        /// Reserved. Used internally by the entity that implements overlapped I/O. For service providers that create sockets as IFS
        /// handles, this parameter is used by the underlying operating system. NonIFS providers are free to use this parameter as necessary.
        /// </para>
        /// </summary>
        public uint InternalHigh;

        /// <summary>
        /// <para>Type: <c>DWORD</c></para>
        /// <para>Reserved for use by service providers.</para>
        /// </summary>
        public uint Offset;

        /// <summary>
        /// <para>Type: <c>DWORD</c></para>
        /// <para>Reserved for use by service providers.</para>
        /// </summary>
        public uint OffsetHigh;

        /// <summary>
        /// <para>Type: <c>HANDLE</c></para>
        /// <para>
        /// If an overlapped I/O operation is issued without an I/O completion routine (the operation's lpCompletionRoutine parameter is
        /// set to null), then this parameter should either contain a valid handle to a WSAEVENT object or be null. If the
        /// lpCompletionRoutine parameter of the call is non-null then applications are free to use this parameter as necessary.
        /// </para>
        /// </summary>
        public IntPtr hEvent;
    }


    /// <summary>
    ///     Description of WSAConnectController.
    /// </summary>
    public class WSASendToRecvFromController : IDisposable, IHook
    {
        #region Fields

        private LocalHook _hook;
        private LocalHook _hook2;

        #endregion Fields

        #region Constructors

        public WSASendToRecvFromController()
        {
            Name = typeof(WSASendToRecvFromController).Name;
            try
            {
                _hook = LocalHook.Create(LocalHook.GetProcAddress("WS2_32.dll", "WSARecvFrom"), new WSARecvFromDelegate(WSARecvFromDetour), this);
                _hook.ThreadACL.SetExclusiveACL(new Int32[] { });
                _hook2 = LocalHook.Create(LocalHook.GetProcAddress("WS2_32.dll", "WSASendTo"), new WSASendToDelegate(WSASendToDetour), this);
                _hook2.ThreadACL.SetExclusiveACL(new Int32[] { });
            }
            catch (Exception)
            {
                Error = true;
            }
        }

        #endregion Constructors

        #region Delegates

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate int WSARecvFromDelegate(IntPtr s, [In, Out, Optional, MarshalAs(UnmanagedType.LPArray)] WSABUF[] lpBuffers, uint dwBufferCount, out uint lpNumberOfBytesRecvd, ref MsgFlags lpFlags,
            [Out] IntPtr lpFrom, ref int lpFromlen, [In, Out, Optional] IntPtr lpOverlapped, [In, Optional, MarshalAs(UnmanagedType.FunctionPtr)] LPWSAOVERLAPPED_COMPLETION_ROUTINE lpCompletionRoutine);


        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate int WSASendToDelegate(IntPtr s, [In, MarshalAs(UnmanagedType.LPArray)] WSABUF[] lpBuffers, uint dwBufferCount, out uint lpNumberOfBytesSent,
            MsgFlags dwFlags, [In, Optional] IntPtr lpTo, int iTolen, [In, Out, Optional] IntPtr lpOverlapped,
            [In, Optional, MarshalAs(UnmanagedType.FunctionPtr)] LPWSAOVERLAPPED_COMPLETION_ROUTINE lpCompletionRoutine);

        public delegate void LPWSAOVERLAPPED_COMPLETION_ROUTINE([In] uint dwError, [In] uint cbTransferred, [In] in WSAOVERLAPPED lpOverlapped, [In] uint dwFlags);


        #endregion Delegates

        #region Properties

        public bool Error { get; set; }

        public string Name { get; set; }

        #endregion Properties

        #region Methods

        [DllImport("WS2_32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int WSARecvFrom(IntPtr s, [In, Out, Optional, MarshalAs(UnmanagedType.LPArray)] WSABUF[] lpBuffers, uint dwBufferCount, out uint lpNumberOfBytesRecvd, ref MsgFlags lpFlags,
            [Out] IntPtr lpFrom, ref int lpFromlen, [In, Out, Optional] IntPtr lpOverlapped, [In, Optional, MarshalAs(UnmanagedType.FunctionPtr)] LPWSAOVERLAPPED_COMPLETION_ROUTINE lpCompletionRoutine);


        [DllImport("WS2_32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int WSASendTo(IntPtr s, [In, MarshalAs(UnmanagedType.LPArray)] WSABUF[] lpBuffers, uint dwBufferCount, out uint lpNumberOfBytesSent,
            MsgFlags dwFlags, [In, Optional] IntPtr lpTo, int iTolen, [In, Out, Optional] IntPtr lpOverlapped,
            [In, Optional, MarshalAs(UnmanagedType.FunctionPtr)] LPWSAOVERLAPPED_COMPLETION_ROUTINE lpCompletionRoutine);

        public void Dispose()
        {
            if (_hook == null)
                return;

            if (_hook2 == null)
                return;

            _hook.Dispose();
            _hook2.Dispose();
            _hook = null;
            _hook2 = null;
        }

        private bool _showDebugLogs = true;
        private void WriteTrace(string msg)
        {
            if (_showDebugLogs)
            {
                Trace.WriteLine(msg);
            }
        }

        private int WSARecvFromDetour(IntPtr s, [In, Out, Optional, MarshalAs(UnmanagedType.LPArray)] WSABUF[] lpBuffers, uint dwBufferCount, out uint lpNumberOfBytesRecvd, ref MsgFlags lpFlags,
            [Out] IntPtr lpFrom, ref int lpFromlen, [In, Out, Optional] IntPtr lpOverlapped, [In, Optional, MarshalAs(UnmanagedType.FunctionPtr)] LPWSAOVERLAPPED_COMPLETION_ROUTINE lpCompletionRoutine)
        {
            HookManagerImpl.Log("WARNING: WSARecvFrom was called.");
            WriteTrace("WARNING: WSARecvFrom was called.");
            lpNumberOfBytesRecvd = 0;
            return -1; // SOCKET_ERROR 
        }


        private int WSASendToDetour(IntPtr s, [In, MarshalAs(UnmanagedType.LPArray)] WSABUF[] lpBuffers, uint dwBufferCount, out uint lpNumberOfBytesSent,
            MsgFlags dwFlags, [In, Optional] IntPtr lpTo, int iTolen, [In, Out, Optional] IntPtr lpOverlapped,
            [In, Optional, MarshalAs(UnmanagedType.FunctionPtr)] LPWSAOVERLAPPED_COMPLETION_ROUTINE lpCompletionRoutine)
        {
            HookManagerImpl.Log("WARNING: WSASendTo was called.");
            WriteTrace("WARNING: WSASendTo was called.");
            lpNumberOfBytesSent = 0;
            return -1; // SOCKET_ERROR 
        }

        #endregion Methods
    }
}