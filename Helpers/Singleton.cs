using System;
using System.Collections.Concurrent;

namespace SpotifyLib.Helpers
{

    public static class Singleton<T>
        where T : new()
    {
        private static ConcurrentDictionary<Type, T> _instances = new ConcurrentDictionary<Type, T>();

        public static void ImplementedInstance(T implementation) =>
            _instances.GetOrAdd(typeof(T),
                (t) => implementation);

        public static bool InstanceExists() =>
             _instances.TryGetValue(typeof(T), out var x);

        public static T Instance
        {
            get
            {
                return _instances.GetOrAdd(typeof(T), (t) => new T());
            }
        }
    }
}
