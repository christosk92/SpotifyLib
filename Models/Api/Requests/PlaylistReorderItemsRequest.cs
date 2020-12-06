using Newtonsoft.Json;

namespace SpotifyLib.Models.Api.Requests
{
    public class PlaylistReorderItemsRequest
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="rangeStart">
        /// The position of the first item to be reordered.
        /// </param>
        /// <param name="insertBefore">
        /// The position where the items should be inserted.
        /// To reorder the items to the end of the playlist,
        /// simply set insert_before to the position after the last item.
        /// Examples: To reorder the first item to the last position in a playlist with 10 items,
        /// set range_start to 0, and insert_before to 10. To reorder the last item in a playlist
        /// with 10 items to the start of the playlist, set range_start to 9, and insert_before to 0.
        /// </param>
        public PlaylistReorderItemsRequest(int rangeStart, int insertBefore)
        {
            RangeStart = rangeStart;
            InsertBefore = insertBefore;
        }

        /// <summary>
        /// The position of the first item to be reordered.
        /// </summary>
        /// <value></value>
        [JsonProperty("range_start")]
        public int RangeStart { get; set; }

        /// <summary>
        /// The position where the items should be inserted.
        /// To reorder the items to the end of the playlist,
        /// simply set insert_before to the position after the last item.
        /// Examples: To reorder the first item to the last position in a playlist with 10 items,
        /// set range_start to 0, and insert_before to 10. To reorder the last item in a playlist
        /// with 10 items to the start of the playlist, set range_start to 9, and insert_before to 0.
        /// </summary>
        /// <value></value>
        [JsonProperty("insert_before")]
        public int InsertBefore { get; set; }

        /// <summary>
        /// The amount of items to be reordered. Defaults to 1 if not set.
        /// The range of items to be reordered begins from the range_start position, and
        /// includes the range_length subsequent items.
        /// Example: To move the items at index 9-10 to the start of the playlist,
        /// range_start is set to 9, and range_length is set to 2.
        /// </summary>
        /// <value></value>
        [JsonProperty("range_length")]
        public int? RangeLength { get; set; }

        /// <summary>
        /// The playlist’s snapshot ID against which you want to make the changes.
        /// </summary>
        /// <value></value>
        [JsonProperty("snapshot_id", NullValueHandling = NullValueHandling.Ignore)]
        public string? SnapshotId { get; set; }
    }
}