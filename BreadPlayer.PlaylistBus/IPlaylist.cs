using BreadPlayer.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BreadPlayer.PlaylistBus
{
	interface IPlaylist
    {
        Task<IEnumerable<string>> LoadPlaylist(string playlistPath);
        Task<bool> SavePlaylist(IEnumerable<Mediafile> songs);
    }
}
