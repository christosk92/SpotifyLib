using System;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using SpotifyLib.Enums;

namespace SpotifyLib.Interfaces
{
    public interface IRequestListener
    {
        RequestResult OnRequest([NotNull] string mid, int pid, [NotNull] String sender, [NotNull] JObject command);
    }
}