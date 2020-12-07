using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using SpotifyLib.Enums;
using SpotifyLib.SpotifyConnect.Models;

namespace SpotifyLib.Interfaces
{
    public interface IDeviceStateHandlerListener
    {
        Task Ready();

        Task Command(Endpoint endpoint, [NotNull] CommandBody data);

        void VolumeChanged();

        void NotActive();
    }
}