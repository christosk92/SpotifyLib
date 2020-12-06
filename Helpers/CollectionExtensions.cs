using System.Collections.Generic;

namespace SpotifyLib.Helpers
{
    public static class Collections
    {
        public static bool AddOrUpdate<T1, T2>(this IDictionary<T1, T2> dict, T1 key, T2 value)
        {
            if (dict.ContainsKey(key))
            {
                if (object.Equals((object)dict[key], (object)value))
                    return false;
                dict[key] = value;
                return true;
            }
            dict.Add(key, value);
            return true;
        }

        public static void AddUnique<T>(this ICollection<T> destination, T item)
        {
            if (destination.Contains(item))
                return;
            destination.Add(item);
        }

        public static void AddUnique<T>(this ICollection<T> destination, params T[] source)
        {
            foreach (T obj in source)
            {
                if (!destination.Contains(obj))
                    destination.Add(obj);
            }
        }

        public static void AddUnique<T>(this ICollection<T> destination, IEnumerable<T> source)
        {
            foreach (T obj in source)
            {
                if (!destination.Contains(obj))
                    destination.Add(obj);
            }
        }

        public static void Add<T>(this ICollection<T> destination, params T[] source)
        {
            foreach (T obj in source)
                destination.Add(obj);
        }

        public static void Add<T>(this ICollection<T> destination, IEnumerable<T> source)
        {
            foreach (T obj in source)
                destination.Add(obj);
        }
    }
}
