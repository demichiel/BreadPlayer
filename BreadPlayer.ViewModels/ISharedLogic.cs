using BreadPlayer.Core;
using BreadPlayer.Models;
using BreadPlayer.Web.Lastfm;
using System.Threading.Tasks;

namespace BreadPlayer.Helpers.Interfaces
{
    public interface ISharedLogic
    {
        Task<Mediafile> CreateMediafile(string path, bool cache = false);
        Task<bool> AskPasswordForPlaylist(Playlist playlist);
        Task<bool> SaveAlbumArtAsync(Mediafile mediafile);
        CoreBreadPlayer Player { get; set; }
        Lastfm LastfmScrobbler { get; set; }
        ICommand ShowPropertiesCommand { get; set; }
        ICommand OpenSongLocationCommand { get; set; }
        ICommand ChangeAlbumArtCommand { get; set; }
        string DatabasePath { get; set; }
    }
}
