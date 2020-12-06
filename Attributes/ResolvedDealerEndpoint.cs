using System;

namespace SpotifyLib.Attributes
{
    /// <summary>
    /// Specify a Base URL for an entire Refit interface
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface)]
    public class ResolvedDealerEndpoint
        : Attribute
    {
    }
}
