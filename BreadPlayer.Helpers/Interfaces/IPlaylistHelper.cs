using BreadPlayer.Database;
using BreadPlayer.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BreadPlayer.Helpers.Interfaces
{
    public interface IPlaylistHelper
    {
        Task<Playlist> ShowAddPlaylistDialogAsync(PlaylistService PlaylistService, string title = "Name this playlist", string playlistName = "", string desc = "", string password = "");
        void ClearPlaylists();
        void AddPlaylist(Playlist playlist);
    }
}
