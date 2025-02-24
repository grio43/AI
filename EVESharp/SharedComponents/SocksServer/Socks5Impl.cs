using System;
using System.Linq;
using System.Net;
using SharedComponents.EVE;
using SharedComponents.Socks5.SocksServer;

namespace EVESharpLauncher.SocksServer
{
    public class Socks5Impl
    {
        #region Fields

        //private static Socks5Server _server;

        #endregion Fields

        #region Constructors

        static Socks5Impl()
        {
            Instance = new Socks5Impl();
            //_server = null;
        }

        #endregion Constructors

        #region Properties

        public static Socks5Impl Instance { get; }

        #endregion Properties

        #region Methods


        private void Log(string msg)
        {
            Cache.Instance.Log(msg);
        }

        #endregion Methods
    }
}