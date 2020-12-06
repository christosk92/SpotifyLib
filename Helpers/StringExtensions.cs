namespace SpotifyLib.Helpers
{
    public static class StringExtensions
    {
        public static bool IsEmpty(this string input) => string.IsNullOrEmpty(input);
    }
}
