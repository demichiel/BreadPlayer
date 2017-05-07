/* 
	BreadPlayer. A music player made for Windows 10 store.
    Copyright (C) 2016  theweavrs (Abdullah Atta)

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using BreadPlayer.ViewModels;
using Windows.ApplicationModel.DataTransfer;
using BreadPlayer.Models;
using System.IO;
using Windows.Storage;
using System.Collections.Generic;
using BreadPlayer.Helpers;
using BreadPlayer.Messengers;
using Windows.UI.Xaml.Data;
using BreadPlayer.Services;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace BreadPlayer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LibraryView
    {
        public LibraryView()
        {
            this.InitializeComponent();
            CrossPlatformHelper.CustomViewSource = new CustomViewSource((Grid.Resources["Source"] as CollectionViewSource));
            this.NavigationCacheMode = NavigationCacheMode.Required;
           // NavigationService.Instance.Frame.Navigated += Frame_Navigated;
        }

        private void Frame_Navigated(object sender, NavigationEventArgs e)
        {
            if(e.SourcePageType == typeof(LibraryView))
                Messenger.Instance.NotifyColleagues(MessageTypes.MSG_LIBRARY_NAVIGATE, e.Parameter ?? "MusicCollection");
        }

        private async void fileBox_Drop(object sender, DragEventArgs e)
        {
            //if (e.DataView.Contains(StandardDataFormats.StorageItems))
            //{
            //    var items = await e.DataView.GetStorageItemsAsync();
            //    if (items.Any())
            //    {
            //        foreach (var item in items)
            //        {
            //            if (item.IsOfType(StorageItemTypes.File) && Path.GetExtension(item.Path) == ".mp3")
            //            {
            //                Mediafile mp3file = null;
            //                string path = item.Path;
            //                var tempList = new List<Mediafile>();
            //                if (TracksCollection.Elements.All(t => t.Path != path))
            //                {
            //                    try
            //                    {
            //                        mp3file = await ViewModels.Init.SharedLogic.CreateMediafile((item as StorageFile).Path);
            //                        await SettingsViewModel.SaveSingleFileAlbumArtAsync(mp3file).ConfigureAwait(false);
            //                        SharedLogic.AddMediafile(mp3file);
            //                    }
            //                    catch (Exception ex)
            //                    {
            //                        CrossPlatformHelper.Log.E("Error occured while drag/drop operation.", ex);
            //                    }
            //                }
            //            }
            //            else if (item.IsOfType(StorageItemTypes.Folder))
            //            {
            //                await Init.SharedLogic.SettingsVM.AddFolderToLibraryAsync((item as StorageFolder).CreateFileQueryWithOptions(DirectoryWalker.GetQueryOptions()));
            //            }

            //        }
            //        Messenger.Instance.NotifyColleagues(MessageTypes.MSG_ADD_ALBUMS, "");
            //    }
            //}
        }
        private void fileBox_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
            e.DragUIOverride.Caption = "Add file/folder(s) to library";
            e.DragUIOverride.IsCaptionVisible = true;
            e.DragUIOverride.IsContentVisible = true;
            e.DragUIOverride.IsGlyphVisible = true;
        }

        private void semanticZoom_ViewChangeStarted(object sender, SemanticZoomViewChangedEventArgs e)
        {
            // only interested in zoomed out->zoomed in transitions
            if (e.IsSourceZoomedInView)
            {
                return;
            }
            try
            { 
                // get the selected group
                var selectedGroup = e.SourceItem.Item as string;
                Grouping<string, Mediafile> myGroup = (DataContext as LibraryViewModel).TracksCollection.FirstOrDefault(g => g.Key.StartsWith(selectedGroup));
                backBtn.Visibility = Visibility.Collapsed;
                e.DestinationItem = new SemanticZoomLocation()
                {
                    Bounds = new Windows.Foundation.Rect(0, 0, 1, 1),
                    Item = myGroup
                };
            }
            catch { }
        }

       
    }
}
