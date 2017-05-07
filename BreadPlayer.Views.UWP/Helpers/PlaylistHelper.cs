using BreadPlayer.Helpers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BreadPlayer.Models;
using BreadPlayer.Dialogs;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using BreadPlayer.Database;
using BreadPlayer.Core;
using Windows.UI.Xaml;

namespace BreadPlayer.Helpers
{
    public class PlaylistHelper : IPlaylistHelper
    {
        public void AddPlaylist(Playlist Playlist, System.Windows.Input.ICommand command)
        {
            var cmd = new ContextMenuCommand(command, Playlist.Name);
            ViewModels.Init.SharedLogic.OptionItems.Add(cmd);
            SharedLogic.PlaylistsItems.Add(new SplitViewMenu.SimpleNavMenuItem
            {
                Arguments = Playlist,
                Label = Playlist.Name,
                DestinationPage = typeof(PlaylistView),
                Symbol = Symbol.List,
                FontGlyph = "\u0045",
                ShortcutTheme = ElementTheme.Dark,
                HeaderVisibility = Visibility.Collapsed
            });
        }

        public void ClearPlaylists()
        {
            SharedLogic.PlaylistsItems.Clear();
        }

        public async Task<Playlist> ShowAddPlaylistDialogAsync(PlaylistService PlaylistService, 
            string title = "Name this playlist", string playlistName = "", string desc = "", 
            string password = "")
        {
            var dialog = new InputDialog()
            {
                Title = title,
                Text = playlistName,
                Description = desc,
                IsPrivate = password.Length > 0,
                Password = password
            };
            if (CoreWindow.GetForCurrentThread().Bounds.Width <= 501)
                dialog.DialogWidth = CoreWindow.GetForCurrentThread().Bounds.Width - 50;
            else
                dialog.DialogWidth = CoreWindow.GetForCurrentThread().Bounds.Width - 300;
            if (await dialog.ShowAsync() == ContentDialogResult.Primary && dialog.Text != "")
            {
                var salthash = Core.Common.PasswordStorage.CreateHash(dialog.Password);
                var Playlist = new Playlist();
                Playlist.Name = dialog.Text;
                Playlist.Description = dialog.Description;
                Playlist.IsPrivate = dialog.Password.Length > 0;
                Playlist.Hash = salthash.Hash;
                Playlist.Salt = salthash.Salt;
                if (PlaylistService.PlaylistExists(Playlist.Name))
                {
                    Playlist = await ShowAddPlaylistDialogAsync(PlaylistService, "Playlist already exists! Please choose another name.", Playlist.Name, Playlist.Description);
                }
                return Playlist;
            }
            return null;
        }
    }
}
