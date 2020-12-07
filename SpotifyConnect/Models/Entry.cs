using JetBrains.Annotations;

namespace SpotifyLib.SpotifyConnect.Models
{
    public abstract class Entry
    {
        public PlayerQueueEntry Next = null;
        public PlayerQueueEntry Prev = null;
        public void SetNext([NotNull] PlayerQueueEntry entry)
        {
            if (Next == null)
            {
                Next = entry;
                entry.Prev = (PlayerQueueEntry)this;
            }
            else
            {
                Next.SetNext(entry);
            }
        }

        public bool Remove([NotNull] PlayerQueueEntry entry)
        {
            if (Next == null) return false;
            if (Next == entry)
            {
                using var tmp = Next;
                Next = tmp.Next;
                return true;
            }
            else
            {
                return Next.Remove(entry);
            }
        }

        public bool Swap(
            [NotNull] PlayerQueueEntry oldEntry,
            [NotNull] PlayerQueueEntry newEntry)
        {
            if (Next == null) return false;
            if (Next == oldEntry)
            {
                Next = newEntry;
                Next.Prev = oldEntry.Prev;
                Next.Next = oldEntry.Next;
                return true;
            }
            else
            {
                return Next.Swap(oldEntry, newEntry);
            }
        }

        public void Clear()
        {
            if (Prev != null)
            {
                Entry tmp = Prev;
                Prev = null;
                if (tmp != this) tmp.Clear();
            }

            if (Next != null)
            {
                Entry tmp = Next;
                Next = null;
                if (tmp != this) tmp.Clear();
            }

            ((PlayerQueueEntry)this).Dispose();
        }
    }
}