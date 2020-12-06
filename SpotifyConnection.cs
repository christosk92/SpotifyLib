using System;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using SpotifyLib.Helpers;

namespace SpotifyLib
{
    internal class SpotifyConnection
    {
        internal TcpClient Conn;
        internal NetworkStream NetworkStream;


        private SpotifyConnection([NotNull] TcpClient conn)
        {
            this.Conn = conn;
            this.NetworkStream = Conn.GetStream();
        }

        internal static async Task<SpotifyConnection> Create(
            [NotNull] Configuration config,
            string address = null)
        {
            var host = address;
            if (!address.IsEmpty())
                return new SpotifyConnection(new TcpClient(host.Split(':')[0], int.Parse(host.Split(':')[1])));
            var random = new Random();
            using var cl = new HttpClient();
            var content = await cl.GetAsync("http://apresolve.spotify.com/?type=accesspoint");
            var stringContent = await content.Content.ReadAsStringAsync();
            var apEndPointsSpotify = JArray.Parse(
                JObject.Parse(stringContent)[
                        "accesspoint"]
                    ?.ToString() ?? string.Empty);

            host = apEndPointsSpotify[1]?.ToString();

            return new SpotifyConnection(new TcpClient(host.Split(':')[0], int.Parse(host.Split(':')[1])));
        }
    }
}