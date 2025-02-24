using System;
using System.Collections.Generic;
using System.Threading;
using SharedComponents.EVE;
using SharedComponents.SharpLogLite;
using SharedComponents.SharpLogLite.Model;

namespace EVESharpLauncher
{
    public class SharpLogLiteHandler : IDisposable
    {
        #region Delegates

        public delegate void SharpLogMessageDelegate(SharpLogMessage msg);

        #endregion Delegates

        #region Constructors

        private SharpLogLiteHandler()
        {
        }

        #endregion Constructors

        #region Destructors

        ~SharpLogLiteHandler()
        {
            Dispose(false);
        }

        #endregion Destructors

        #region Fields

        private static readonly Lazy<SharpLogLiteHandler> lazy =
            new Lazy<SharpLogLiteHandler>(() => new SharpLogLiteHandler());

        private readonly List<SharpLogMessageDelegate> delegates = new List<SharpLogMessageDelegate>();
        private SharpLogLite _handler;

        #endregion Fields

        #region Events

        public event SharpLogMessageDelegate OnSharpLogLiteMessage
        {
            add
            {
                _onSharpLogLiteMessage += value;
                delegates.Add(value);
            }

            remove
            {
                _onSharpLogLiteMessage -= value;
                delegates.Remove(value);
            }
        }

        private event SharpLogMessageDelegate _onSharpLogLiteMessage;

        #endregion Events

        #region Properties

        public static SharpLogLiteHandler Instance => lazy.Value;
        private LogSeverity LogModelSeverity => Cache.Instance.EveSettings.SharpLogLiteLogSeverity;

        #endregion Properties

        #region Methods

        public void StartListening()
        {
            if (_handler == null)
            {
                Cache.Instance.Log("Starting SharpLogLite handler.");
                _handler = new SharpLogLite(LogModelSeverity);
                new Thread(() => { _handler.StartListening(); }).Start();
                LogModelHandler.OnMessage += LogModelHandlerOnMessage;
            }
        }

        public void StopListening()
        {
            if (_handler != null)
            {
                LogModelHandler.OnMessage -= LogModelHandlerOnMessage;
                _handler.Dispose();
                Cache.Instance.Log("Stopping SharpLogLite handler.");
            }
            _handler = null;
        }

        private void LogModelHandlerOnMessage(SharpLogMessage msg)
        {
            _onSharpLogLiteMessage?.Invoke(msg);
        }

        private void RemoveAllEventHandlers()
        {
            foreach (SharpLogMessageDelegate eh in delegates)
                _onSharpLogLiteMessage -= eh;
            delegates.Clear();
        }

        #endregion Methods

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                StopListening();
                RemoveAllEventHandlers();
            }
        }

        #endregion IDisposable
    }
}