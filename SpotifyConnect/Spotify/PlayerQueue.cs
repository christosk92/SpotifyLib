using System.Diagnostics;
using JetBrains.Annotations;
using SpotifyLib.SpotifyConnect.Models;

namespace SpotifyLib.SpotifyConnect.Spotify
{
    public class PlayerQueue
    {
        public PlayerQueueEntry Head = null;
        public void Remove([NotNull] PlayerQueueEntry entry)
        {
            if (Head == null) return;

            bool removed;
            if (Head == entry)
            {
                var tmp = Head;
                Head = tmp.Next;
                tmp?.Dispose();
                removed = true;
            }
            else
            {
                removed = Head.Remove(entry);
            }

            if (removed) Debug.WriteLine("{0} removed from queue.", entry);
        }
        public PlayerQueueEntry Next() => Head?.Next;
        public PlayerQueueEntry Prev() => Head?.Prev;
        public void Add([NotNull] PlayerQueueEntry entry)
        {
            if (Head == null) Head = entry;
            else Head.SetNext(entry);
            //executorService.execute(entry);

            Debug.WriteLine("{0} added to queue.", entry);
        }
        public bool Advance()
        {
            if (Head?.Next == null)
                return false;

            var tmp = Head.Next;
            Head.Next = null;
            Head.Prev = null;
            if (!Head.CloseIfUseless) tmp.Prev = Head;
            Head = tmp;
            return true;
        }
    }
}