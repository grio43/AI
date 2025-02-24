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
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SharedComponents.Socks5.TCP
{
    public class TcpServer
    {
        //public event EventHandler<DataEventArgs> onDataReceived = delegate { };
        //public event EventHandler<DataEventArgs> onDataSent = delegate { };

        #region Constructors

        public TcpServer(IPAddress ip, int port)
        {
            p = new TcpListener(ip, port);
        }

        #endregion Constructors

        #region Properties

        public int PacketSize { get; set; }

        #endregion Properties

        #region Fields

        private readonly TcpListener p;
        private readonly ManualResetEvent Task = new ManualResetEvent(false);
        private bool accept;

        #endregion Fields

        #region Events

        public event EventHandler<ClientEventArgs> onClientConnected = delegate { };

        public event EventHandler<ClientEventArgs> onClientDisconnected = delegate { };

        #endregion Events

        #region Methods

        public void Start()
        {
            if (!accept)
            {
                accept = true;
                p.Start(10000);
                new Thread(AcceptConnections).Start();
            }
        }

        public void Stop()
        {
            if (accept)
            {
                accept = false;
                p.Stop();
                Task.Set();
            }
        }

        private void AcceptClient(IAsyncResult res)
        {
            try
            {
                TcpListener px = (TcpListener)res.AsyncState;
                Socket x = px.EndAcceptSocket(res);
                Task.Set();
                Client f = new Client(x, PacketSize);
                //f.onClientDisconnected += onClientDisconnected;
                //f.onDataReceived += onDataReceived;
                //f.onDataSent += onDataSent;
                onClientConnected(this, new ClientEventArgs(f));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                //server stopped or client errored?
            }
        }

        private void AcceptConnections()
        {
            while (accept)
                try
                {
                    Task.Reset();
                    p.BeginAcceptSocket(AcceptClient, p);
                    Task.WaitOne();
                }
                catch
                {
                    //error, most likely server shutdown.
                }
        }

        #endregion Methods
    }
}