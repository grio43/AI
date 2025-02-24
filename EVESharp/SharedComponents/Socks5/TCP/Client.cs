/*
    Socks5 - A full-fledged high-performance socks5 proxy server written in C#. Plugin support included.
    Copyright (C) 2016 ThrDev

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.IO;
using System.Net.Sockets;

namespace SharedComponents.Socks5.TCP
{
    public class Client : Stream
    {
        private byte[] buffer;

        private bool disposed;
        private int packetSize = 4096;
        public bool Receiving;

        public Client(Socket sock, int PacketSize)
        {
            //start the data exchange.
            Sock = sock;
            onClientDisconnected = delegate { };
            buffer = new byte[PacketSize];
            packetSize = PacketSize;
            sock.ReceiveBufferSize = PacketSize;
        }

        public Socket Sock { get; set; }

        public override bool CanRead => Sock.Connected;

        public override bool CanSeek => false;

        public override bool CanWrite => Sock.Connected;

        public override long Length => 0;

        public override long Position
        {
            get => 0;

            set => throw new NotImplementedException();
        }

        public event EventHandler<ClientEventArgs> onClientDisconnected;

        public event EventHandler<DataEventArgs> onDataReceived = delegate { };

        public event EventHandler<DataEventArgs> onDataSent = delegate { };

        private void DataReceived(IAsyncResult res)
        {
            Receiving = false;
            try
            {
                SocketError err = SocketError.Success;
                if (disposed)
                    return;
                int received = ((Socket)res.AsyncState).EndReceive(res, out err);
                if (received <= 0 || err != SocketError.Success)
                {
                    Disconnect();
                    return;
                }
                DataEventArgs data = new DataEventArgs(this, buffer, received);
                onDataReceived(this, data);
            }
            catch (Exception ex)
            {
#if DEBUG
#if DEBUG
                Console.WriteLine(ex.ToString());
#endif
#endif
                Disconnect();
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return Receive(buffer, offset, count);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            Send(buffer, offset, count);
        }

        public int Receive(byte[] data, int offset, int count)
        {
            try
            {
                int received = Sock.Receive(data, offset, count, SocketFlags.None);
                if (received <= 0)
                {
                    Disconnect();
                    return -1;
                }
                DataEventArgs dargs = new DataEventArgs(this, data, received);
                //this.onDataReceived(this, dargs);
                return received;
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine(ex.ToString());
#endif
                Disconnect();
                return -1;
            }
        }

        public void ReceiveAsync(int buffersize = -1)
        {
            try
            {
                if (buffersize > -1)
                    buffer = new byte[buffersize];
                Receiving = true;
                Sock.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, DataReceived, Sock);
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine(ex.ToString());
#endif
                Disconnect();
            }
        }

        public void Disconnect()
        {
            try
            {
                //while (Receiving) Thread.Sleep(10);
                if (!disposed)
                {
                    if (Sock != null && Sock.Connected)
                    {
                        onClientDisconnected(this, new ClientEventArgs(this));
                        Sock.Close();
                        //this.Sock = null;
                        return;
                    }
                    onClientDisconnected(this, new ClientEventArgs(this));
                    Dispose();
                }
            }
            catch
            {
            }
        }

        private void DataSent(IAsyncResult res)
        {
            try
            {
                int sent = ((Socket)res.AsyncState).EndSend(res);
                if (sent < 0)
                {
                    Sock.Shutdown(SocketShutdown.Both);
                    Sock.Close();
                    return;
                }
                DataEventArgs data = new DataEventArgs(this, new byte[0] { }, sent);
                onDataSent(this, data);
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine(ex.ToString());
#endif
            }
        }

        public bool Send(byte[] buff)
        {
            return Send(buff, 0, buff.Length);
        }

        public void SendAsync(byte[] buff, int offset, int count)
        {
            try
            {
                if (Sock != null && Sock.Connected)
                    Sock.BeginSend(buff, offset, count, SocketFlags.None, DataSent, Sock);
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine(ex.ToString());
#endif
                Disconnect();
            }
        }

        public bool Send(byte[] buff, int offset, int count)
        {
            try
            {
                if (Sock != null)
                {
                    if (Sock.Send(buff, offset, count, SocketFlags.None) <= 0)
                    {
                        Disconnect();
                        return false;
                    }
                    DataEventArgs data = new DataEventArgs(this, buff, count);
                    onDataSent(this, data);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
#if DEBUG
#if DEBUG
                Console.WriteLine(ex.ToString());
#endif
#endif
                Disconnect();
                return false;
            }
        }

        // Protected implementation of Dispose pattern.
        protected override void Dispose(bool disposing)
        {
            if (disposed)
                return;

            disposed = true;

            if (disposing)
            {
                // Free any other managed objects here.
                //
                Sock = null;
                buffer = null;
                onClientDisconnected = null;
                onDataReceived = null;
                onDataSent = null;
            }

            // Free any unmanaged objects here.
            //
        }

        public override void Flush()
        {
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }
    }
}