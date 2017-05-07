using BreadPlayer.Models.Interfaces;

namespace BreadPlayer.Models
{
    public class ChildSong : IDBRecord
    {
        public long Id { get; set; }
        public long SongId { get; set; }
        public long PlaylistId { get; set; }

        public string GetTextSearchKey()
        {
            return string.Format("pId={0};songid={1}", PlaylistId, SongId);
        }
    }
}
