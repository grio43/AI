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

using SharedComponents.Socks5.Encryption;
using SharedComponents.Socks5.Plugin;
using SharedComponents.Socks5.TCP;
using System;
using System.Collections.Generic;
using System.Net;

namespace SharedComponents.Socks5.Socks
{
    public class SocksClient
    {
        #region Fields

        public Client Client;

        #endregion Fields

        #region Constructors

        public SocksClient(Client cli)
        {
            Client = cli;
        }

        #endregion Constructors

        #region Events

        public event EventHandler<SocksClientEventArgs> onClientDisconnected = delegate { };

        #endregion Events

        #region Properties

        public int Authenticated { get; private set; }
        public SocksRequest Destination { get; private set; }

        #endregion Properties

        #region Methods

        public void Begin(IPAddress outboundInterface, int PacketSize, int Timeout)
        {
            Client.onClientDisconnected += Client_onClientDisconnected;
            List<AuthTypes> authtypes = Socks5.RequestAuth(this);
            if (authtypes.Count <= 0)
            {
                Client.Send(new byte[] { 0x00, 0xFF });
                Client.Disconnect();
                return;
            }
            Authenticated = 0;
            SocksEncryption w = null;
            List<object> lhandlers = PluginLoader.LoadPlugin(typeof(LoginHandler));
            //check out different auth types, none will have no authentication, the rest do.
            if (lhandlers.Count > 0 && (authtypes.Contains(AuthTypes.SocksBoth) || authtypes.Contains(AuthTypes.SocksEncrypt) ||
                                        authtypes.Contains(AuthTypes.SocksCompress) || authtypes.Contains(AuthTypes.Login)))
            {
                //this is the preferred method.
                w = Socks5.RequestSpecialMode(authtypes, Client);
                foreach (LoginHandler lh in lhandlers)
                {
                    //request login.
                    User user = Socks5.RequestLogin(this);
                    if (user == null)
                    {
                        Client.Disconnect();
                        return;
                    }
                    LoginStatus status = lh.HandleLogin(user);
                    Client.Send(new[] { (byte)HeaderTypes.Socks5, (byte)status });
                    if (status == LoginStatus.Denied)
                    {
                        Client.Disconnect();
                        return;
                    }
                    if (status == LoginStatus.Correct)
                    {
                        Authenticated = w.GetAuthType() == AuthTypes.Login ? 1 : 2;
                        break;
                    }
                }
            }
            else if (authtypes.Contains(AuthTypes.None))
            {
                //no authentication.
                if (lhandlers.Count <= 0)
                {
                    //unsupported methods y0
                    Authenticated = 1;
                    Client.Send(new[] { (byte)HeaderTypes.Socks5, (byte)HeaderTypes.Zero });
                }
                else
                {
                    //unsupported.
                    Client.Send(new[] { (byte)HeaderTypes.Socks5, (byte)AuthTypes.Unsupported });
                    Client.Disconnect();
                    return;
                }
            }
            else
            {
                //unsupported.
                Client.Send(new[] { (byte)HeaderTypes.Socks5, (byte)AuthTypes.Unsupported });
                Client.Disconnect();
                return;
            }
            //Request Site Data.
            if (Authenticated == 1)
            {
                w = new SocksEncryption();
                w.SetType(AuthTypes.Login);
                SocksRequest req = Socks5.RequestTunnel(this, w);
                if (req == null)
                {
                    Client.Disconnect();
                    return;
                }
                Destination = new SocksRequest(req.StreamType, req.Type, req.Address, req.Port);
                //call on plugins for connect callbacks.
                foreach (ConnectHandler conn in PluginLoader.LoadPlugin(typeof(ConnectHandler)))
                    if (conn.OnConnect(Destination) == false)
                    {
                        req.Error = SocksError.Failure;
                        Client.Send(req.GetData(true));
                        Client.Disconnect();
                        return;
                    }
                //Send Tunnel Data back.
                SocksTunnel x = new SocksTunnel(this, req, Destination, PacketSize, Timeout);
                x.Open(outboundInterface);
            }
            else if (Authenticated == 2)
            {
                SocksRequest req = Socks5.RequestTunnel(this, w);
                if (req == null)
                {
                    Client.Disconnect();
                    return;
                }
                Destination = new SocksRequest(req.StreamType, req.Type, req.Address, req.Port);
                foreach (ConnectHandler conn in PluginLoader.LoadPlugin(typeof(ConnectHandler)))
                    if (conn.OnConnect(Destination) == false)
                    {
                        req.Error = SocksError.Failure;
                        Client.Send(req.GetData(true));
                        Client.Disconnect();
                        return;
                    }
                //Send Tunnel Data back.
                SocksSpecialTunnel x = new SocksSpecialTunnel(this, w, req, Destination, PacketSize, Timeout);
                x.Open(outboundInterface);
            }
        }

        private void Client_onClientDisconnected(object sender, ClientEventArgs e)
        {
            onClientDisconnected(this, new SocksClientEventArgs(this));
            Client.onClientDisconnected -= Client_onClientDisconnected;
            //added to clear up memory
        }

        #endregion Methods
    }

    public class User
    {
        #region Constructors

        public User(string un, string pw, IPEndPoint ip)
        {
            Username = un;
            Password = pw;
            IP = ip;
        }

        #endregion Constructors

        #region Properties

        public IPEndPoint IP { get; }
        public string Password { get; }
        public string Username { get; }

        #endregion Properties
    }
}