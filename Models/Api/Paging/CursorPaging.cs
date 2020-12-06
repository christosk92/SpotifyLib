using System.Collections.Generic;

namespace SpotifyLib.Models.Api.Paging
{
    public class CursorPaging<T> : IPaginatable<T>
    {
        public string Href { get; set; } = default!;
        public List<T>? Items { get; set; } = default!;
        public int Limit { get; set; }
        public string? Next { get; set; } = default!;
        public Cursor Cursors { get; set; } = default!;
        public int Total { get; set; }
    }

    public class CursorPaging<T, TNext> : IPaginatable<T, TNext>
    {
        public string Href { get; set; } = default!;
        public List<T>? Items { get; set; } = default!;
        public int Limit { get; set; }
        public string? Next { get; set; } = default!;
        public Cursor Cursors { get; set; } = default!;
        public int Total { get; set; }
    }

    public class Cursor
    {
        public string Before { get; set; } = default!;
        public string After { get; set; } = default!;
    }
}
