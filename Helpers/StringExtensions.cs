using System;
using System.Linq;

namespace SpotifyLib.Helpers
{
    public static class StringExtensions
    {
        public static bool IsEmpty(this string input) => string.IsNullOrEmpty(input);

        public static string FirstCharToUpper(this string input) =>
            input switch
            {
                null => throw new ArgumentNullException(nameof(input)),
                "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
                _ => input.First().ToString().ToUpper() + input.Substring(1)
            };
    }
}
