using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SpotifyLib.Models;

namespace SpotifyLib.Interfaces
{
    public abstract class WebsocketHandler : IDisposable
    {
        public abstract event EventHandler<string> MessageReceived;
        public abstract event EventHandler<WebsocketclosedEventArgs> SocketDisconnected;
        public abstract event EventHandler<string> SocketConnected;
        public abstract Task SendMessageAsync(string jsonContent);
        public abstract Task ConnectSocketAsync(Uri connectionUri);
        public abstract Task KeepAlive();
        public virtual void Dispose()
        {
        }
    }
}
