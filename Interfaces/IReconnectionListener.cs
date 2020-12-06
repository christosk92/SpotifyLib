namespace SpotifyLib.Interfaces
{
    public interface IReconnectionListener
    {
        void OnConnectionDropped();

        void OnConnectionEstablished();
    }
}
