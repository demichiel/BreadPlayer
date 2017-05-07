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
using BreadPlayer.Database;
using BreadPlayer.Extensions;
using BreadPlayer.Helpers;
using BreadPlayer.Helpers.Interfaces;
using BreadPlayer.Messengers;
using BreadPlayer.Models;
using BreadPlayer.Models.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace BreadPlayer.ViewModels
{
    /// <summary>
    /// ViewModel for Library View (Severe cleanup and documentation needed.)
    /// </summary>
    public class LibraryViewModel : ViewModelBase
    {
        #region Fields        
        ThreadSafeObservableCollection<Playlist> PlaylistCollection = new ThreadSafeObservableCollection<Playlist>();
        IEnumerable<Mediafile> files = null;
        IEnumerable<Mediafile> OldItems;
        bool grouped = false;
        bool libgrouped = false;
        object source;
        bool isPlayingFromPlaylist = false;
        bool libraryLoaded = false;
        #endregion

        #region MessageHandling
        async void HandleLibraryNavigatedMessage(Message message)
        {
            if (message.Payload != null && message.Payload is string payload)
            {
                // e.Parameter can be null and throw exception
                string param = (payload ?? string.Empty);
                switch (param)
                {
                    case "Recent":
                        await ChangeView("Recently Played", false, await GetRecentlyPlayedSongsAsync().ConfigureAwait(false));
                        break;
                    case "MostEaten":
                        await ChangeView("Most Eaten", false, await GetMostPlayedSongsAsync().ConfigureAwait(false));
                        break;
                    case "RecentlyAdded":
                        await ChangeView("Recently Added", false, await GetRecentlyAddedSongsAsync().ConfigureAwait(false));
                        break;
                    case "Favorites":
                        await ChangeView("Favorites", false, await GetFavoriteSongs().ConfigureAwait(false));
                        break;
                    case "MusicCollection":
                    default:
                        await ChangeView("Music Collection", libgrouped, TracksCollection.Elements);
                        break;
                }
                await RefreshSourceAsync().ConfigureAwait(false);
            }
            else if (ViewSource.Source != null)
            {
                source = ViewSource.Source;
                grouped = ViewSource.IsSourceGrouped;
                ViewSource.Source = null;
            }
        }
        void HandleSearchStartedMessage(Message message)
        {
            if (message.Payload != null)
            {
                message.HandledStatus = MessageHandledStatus.HandledCompleted;
                //if (!Windows.Foundation.Metadata.ApiInformation.IsApiContractPresent("Windows.Phone.PhoneContract", 1))
                //    Header = message.Payload.ToString();
            }
        }
        void HandleDisposeMessage()
        {
            Reset();
        }
        async void HandleUpdateSongCountMessage(Message message)
        {
            if (message.Payload is short || message.Payload is Int32)
            {
                message.HandledStatus = MessageHandledStatus.HandledCompleted;
                SongCount = Convert.ToInt32(message.Payload);
                Status = SongCount.ToString() + " Song(s) Loaded";
                IsLibraryLoading = true;
            }
            else
            {
                await GetGenres().ConfigureAwait(false);
                IsLibraryLoading = false;
            }
        }
        async void HandleAddPlaylistMessage(Message message)
        {
            var plist = message.Payload as Playlist;
            if (plist != null)
            {
                message.HandledStatus = MessageHandledStatus.HandledCompleted;
                await AddPlaylistAsync(plist, false);
            }
        }
        void HandlePlaySongMessage(Message message)
        {
            if (message.Payload is Mediafile song)
            {
                if (message != null)
                    message.HandledStatus = MessageHandledStatus.HandledCompleted;
                if (song != null)
                {
                    PlayCommand.Execute(song);
                }
            }
        }
        #endregion

        #region Contructor
        /// <summary>
        /// Creates a new instance of LibraryViewModel
        /// </summary>
        public LibraryViewModel()
        {
           // Header = "Music Collection";
            MusicLibraryLoaded += LibraryViewModel_MusicLibraryLoaded;
            RecentlyPlayedCollection.CollectionChanged += Elements_CollectionChanged;
            LoadLibrary();

            Messenger.Instance.Register(MessageTypes.MSG_PLAY_SONG, new Action<Message>(HandlePlaySongMessage));
            Messenger.Instance.Register(MessageTypes.MSG_DISPOSE, new Action(HandleDisposeMessage));
            Messenger.Instance.Register(MessageTypes.MSG_ADD_PLAYLIST, new Action<Message>(HandleAddPlaylistMessage));
            Messenger.Instance.Register(MessageTypes.MSG_UPDATE_SONG_COUNT, new Action<Message>(HandleUpdateSongCountMessage));
            Messenger.Instance.Register(MessageTypes.MSG_SEARCH_STARTED, new Action<Message>(HandleSearchStartedMessage));
            Messenger.Instance.Register(MessageTypes.MSG_LIBRARY_NAVIGATE, new Action<Message>(HandleLibraryNavigatedMessage));
        }
        #endregion

        #region Properties  
        string status = "Loading Library...";
        public string Status
        {
            get { return status; }
            set { Set(ref status, value); }
        }
        private List<string> _alphabetList;
        public List<string> AlphabetList
        {
            get { return _alphabetList; }
            set
            {
                _alphabetList = value;
                OnPropertyChanged();
            }
        }        
        
        LibraryService libraryservice;
        public LibraryService LibraryService
        {
            get { if (libraryservice == null)
                    libraryservice = new LibraryService(new KeyValueStoreDatabaseService(ViewModels.Init.SharedLogic.DatabasePath, "Tracks", "TracksText"));
                return libraryservice; }
            set { Set(ref libraryservice, value); }
        }

        PlaylistService playlistService;
        public PlaylistService PlaylistService
        {
            get
            {
                if (playlistService == null)
                    playlistService = new PlaylistService(new KeyValueStoreDatabaseService(ViewModels.Init.SharedLogic.DatabasePath, "Playlists", "PlaylistsText"));
                return playlistService;
            }
            set { Set(ref playlistService, value); }
        }
      
        public ICustomViewSource ViewSource
        {
            get { return CrossPlatformHelper.CustomViewSource; }
        }
        bool isMultiSelectModeEnabled = false;
        public bool IsMultiSelectModeEnabled
        {
            get { return isMultiSelectModeEnabled; }
            set { Set(ref isMultiSelectModeEnabled, value); }
        }
       
        string _genre;
        public string Genre
        {
            get { return _genre; }
            set { Set(ref _genre, value); }
        }
        string _sort = "Unsorted";
        public string Sort
        {
            get { return _sort; }
            set
            {
                Set(ref _sort, value);
                CrossPlatformHelper.SettingsHelper.SaveSetting("Sort", _sort);
            }
        }
        Mediafile selectedItem;
        public Mediafile SelectedItem
        {
            get { return selectedItem; }
            set { Set(ref selectedItem, value); }
        }

        int songCount;
        public int SongCount
        {
            get { return songCount; }
            set { Set(ref songCount, value); }
        }
        ThreadSafeObservableCollection<Mediafile> _MostEatenCollection;
        /// <summary>
        /// Gets or sets a grouped observable collection of Tracks/Mediafiles. <seealso cref="GroupedObservableCollection{TKey, TElement}"/>
        /// </summary>
        public ThreadSafeObservableCollection<Mediafile> MostEatenSongsCollection
        {
            get { if (_MostEatenCollection == null) _MostEatenCollection = new ThreadSafeObservableCollection<Mediafile>(); return _MostEatenCollection; }
            set { Set(ref _MostEatenCollection, value); }
        }
        ThreadSafeObservableCollection<Mediafile> _FavoriteSongsCollection;
        /// <summary>
        /// Gets or sets a grouped observable collection of Tracks/Mediafiles. <seealso cref="GroupedObservableCollection{TKey, TElement}"/>
        /// </summary>
        public ThreadSafeObservableCollection<Mediafile> FavoriteSongsCollection
        {
            get { if (_FavoriteSongsCollection == null) _FavoriteSongsCollection = new ThreadSafeObservableCollection<Mediafile>(); return _FavoriteSongsCollection; }
            set { Set(ref _FavoriteSongsCollection, value); }
        }
        ThreadSafeObservableCollection<Mediafile> _RecentlyAddedSongsCollection;
        /// <summary>
        /// Gets or sets a grouped observable collection of Tracks/Mediafiles. <seealso cref="GroupedObservableCollection{TKey, TElement}"/>
        /// </summary>
        public ThreadSafeObservableCollection<Mediafile> RecentlyAddedSongsCollection
        {
            get { if (_RecentlyAddedSongsCollection == null) _RecentlyAddedSongsCollection = new ThreadSafeObservableCollection<Mediafile>(); return _RecentlyAddedSongsCollection; }
            set { Set(ref _RecentlyAddedSongsCollection, value); }
        }
        ThreadSafeObservableCollection<Mediafile> _RecentlyPlayedCollection;
        /// <summary>
        /// Gets or sets a grouped observable collection of Tracks/Mediafiles. <seealso cref="GroupedObservableCollection{TKey, TElement}"/>
        /// </summary>
        public ThreadSafeObservableCollection<Mediafile> RecentlyPlayedCollection
        {
            get { if (_RecentlyPlayedCollection == null) _RecentlyPlayedCollection = new ThreadSafeObservableCollection<Mediafile>(); return _RecentlyPlayedCollection; }
            set { Set(ref _RecentlyPlayedCollection, value); }
        }
        GroupedObservableCollection<string, Mediafile> _TracksCollection;
        /// <summary>
        /// Gets or sets a grouped observable collection of Tracks/Mediafiles. <seealso cref="GroupedObservableCollection{TKey, TElement}"/>
        /// </summary>
        public GroupedObservableCollection<string, Mediafile> TracksCollection
        {
            get
            {
                if (_TracksCollection == null)
                    _TracksCollection = new GroupedObservableCollection<string, Mediafile>(GetSortFunction("FolderPath"));
                return _TracksCollection;
            }
            set
            {
                Set(ref _TracksCollection, value);
                Messenger.Instance.NotifyColleagues(MessageTypes.MSG_LIBRARY_LOADED, new List<object>() { TracksCollection, grouped });
            }
        }

        ThreadSafeObservableCollection<string> _GenreCollection;
        /// <summary>
        /// Gets or Sets a flyout for genres. This is a dynamic control bound to <see cref="LibraryView"/>.
        /// </summary>
        public ThreadSafeObservableCollection<string> GenreCollection
        {
            get { if (_GenreCollection == null) _GenreCollection = new ThreadSafeObservableCollection<string>(); return _GenreCollection; }
            set { Set(ref _GenreCollection, value); }
        }
        bool isLibraryLoading;
        /// <summary>
        /// Gets or Sets <see cref="BreadPlayer.Models.Mediafile"/> for this ViewModel
        /// </summary>
        public bool IsLibraryLoading
        {
            get { return isLibraryLoading; }
            set { Set(ref isLibraryLoading, value); }
        }
        #endregion

        #region Commands 

        #region Definitions
        RelayCommand _deleteCommand;
        RelayCommand _playCommand;
        RelayCommand _stopAfterCommand;
        RelayCommand _addtoplaylistCommand;   
        RelayCommand _sortByCommand;
        RelayCommand _filterByGenreCommand;
        RelayCommand _initCommand;
        RelayCommand _relocateSongCommand;
        DelegateCommand changeSelectionModeCommand;
        RelayCommand addToFavoritesCommand;
        public ICommand RelocateSongCommand
        {
            get
            { if (_relocateSongCommand == null) { _relocateSongCommand = new RelayCommand(RelocateSong); } return _relocateSongCommand; }
        }
        public ICommand ChangeSelectionModeCommand
        {
            get
            { if (changeSelectionModeCommand == null) { changeSelectionModeCommand = new DelegateCommand(ChangeSelectionMode); } return changeSelectionModeCommand; }
        }
        public ICommand AddToFavoritesCommand
        {
            get
            { if (addToFavoritesCommand == null) { addToFavoritesCommand = new RelayCommand(AddToFavorites); } return addToFavoritesCommand; }
        }

        /// <summary>
        /// Gets command for initialization. This calls the <see cref="Init(object)"/> method. <seealso cref="ICommand"/>
        /// </summary>
        public ICommand InitCommand
        {
            get
            { if (_initCommand == null) { _initCommand = new RelayCommand(param => this.Init(param)); } return _initCommand; }
        }
      
        /// <summary>
        /// Gets AddToPlaylist command. This calls the <see cref="AddToPlaylist(object)"/> method. <seealso cref="ICommand"/>
        /// </summary>
        public ICommand AddToPlaylistCommand
        {
            get
            { if (_addtoplaylistCommand == null) { _addtoplaylistCommand = new RelayCommand(param => this.AddToPlaylist(param)); } return _addtoplaylistCommand; }
        }
        /// <summary>
        /// Gets refresh command. This calls the <see cref="RefreshView(object)"/> method. <seealso cref="ICommand"/>
        /// </summary>
        public ICommand SortByCommand
        {
            get
            { if (_sortByCommand == null) { _sortByCommand = new RelayCommand(param => this.SortBy(param)); } return _sortByCommand; }
        }
        public ICommand FilterByGenreCommand
        {
            get
            { if (_filterByGenreCommand == null) { _filterByGenreCommand = new RelayCommand(param => this.FilterByGenre(param)); } return _filterByGenreCommand; }
        }
        /// <summary>
        /// Gets Play command. This calls the <see cref="Play(object)"/> method. <seealso cref="ICommand"/>
        /// </summary>
        public ICommand PlayCommand
        {
            get
            { if (_playCommand == null) { _playCommand = new RelayCommand(param => this.Play(param)); } return _playCommand; }
        }
        
        /// <summary>
        /// Gets Stop command. This calls the <see cref="StopAfter(object)"/> method. <seealso cref="ICommand"/>
        /// </summary>
        public ICommand StopAfterCommand
        {
           get
           { if (_stopAfterCommand == null) { _stopAfterCommand = new RelayCommand(param => this.StopAfter(param)); } return _stopAfterCommand; }
        }

        /// <summary>
        /// Gets Play command. This calls the <see cref="Delete(object)"/> method. <seealso cref="ICommand"/>
        /// </summary>
        public ICommand DeleteCommand
        {
            get
            { if (_deleteCommand == null) { _deleteCommand = new RelayCommand(param => this.Delete(param)); } return _deleteCommand; }
        }
        #endregion

        #region Implementations 
        private async void AddToFavorites(object para)
        {
            var mediaFile = para as Mediafile;
            mediaFile.IsFavorite = true;
            await LibraryService.UpdateMediafile(mediaFile);
        }
        /// <summary>
        /// Relocates song to a new location. We only update _id, Path and Length of the song.
        /// </summary>
        /// <param name="para">The Mediafile to relocate</param>
        private async void RelocateSong(object para)
        {
            if (para is Mediafile mediafile)
            {
                var filePath = await CrossPlatformHelper.FilePickerHelper.PickFileAsync(new List<string>() { ".mp3", ".wav", ".ogg", ".flac", ".m4a", ".aif", ".wma" });             
                if (filePath != null)
                {
                    var newMediafile = await ViewModels.Init.SharedLogic.CreateMediafile(filePath);
                    TracksCollection.Elements.Single(t => t.Path == mediafile.Path).Length = newMediafile.Length;
                    TracksCollection.Elements.Single(t => t.Path == mediafile.Path).Id = newMediafile.Id;
                    TracksCollection.Elements.Single(t => t.Path == mediafile.Path).Path = newMediafile.Path;
                    await LibraryService.UpdateMediafile(TracksCollection.Elements.Single(t => t.Id == mediafile.Id));
                }
            }
        }
        private void ChangeSelectionMode()
        {
            IsMultiSelectModeEnabled = IsMultiSelectModeEnabled ? false : true;
        }
        async void FilterByGenre(object para)
        {
            Genre = para.ToString();
            await RefreshView(Genre, null, false).ConfigureAwait(false);
        }
        /// <summary>
        /// Refreshes the view with new sorting order and/or filtering. <seealso cref="RefreshViewCommand"/>
        /// </summary>
        /// <param name="para"><see cref="MenuFlyoutItem"/> to get sorting/filtering base from.</param>
        async void SortBy(object para)
        {
            await RefreshView(null, para.ToString()).ConfigureAwait(false);
        }
        /// <summary>
        /// Deletes a song from the FileCollection. <seealso cref="DeleteCommand"/>
        /// </summary>
        /// <param name="path"><see cref="BreadPlayer.Models.Mediafile"/> to delete.</param>
        public async void Delete(object path)
        {
            try
            {
                int index = 0;
                if(SelectedItems.Count > 0)
                {
                    foreach (var item in SelectedItems)
                    {
                        index = TracksCollection.Elements.IndexOf(item);
                        TracksCollection.RemoveItem(item);
                        await LibraryService.RemoveMediafile(item);
                    }
                }
               
                if (TracksCollection.Elements.Count > 0)
                {
                    await Task.Delay(100);
                    SelectedItem = index < TracksCollection.Elements.Count ? TracksCollection.Elements.ElementAt(index) : TracksCollection.Elements.ElementAt(index - 1);
                }
            }
            catch (Exception ex)
            {
                CrossPlatformHelper.Log.E("Error occured while deleting a song from collection and list.", ex);
            }
        }
        
        public async void StopAfter(object path)
        {
            Mediafile mediaFile = await GetMediafileFromParameterAsync(path);
            Messenger.Instance.NotifyColleagues(MessageTypes.MSG_STOP_AFTER_SONG, mediaFile);
        }
        
        /// <summary>
        /// Plays the selected file. <seealso cref="PlayCommand"/>
        /// </summary>
        /// <param name="path"><see cref="BreadPlayer.Models.Mediafile"/> to play.</param>
        public async void Play(object path)
        {
            var currentlyPlaying = Player.CurrentlyPlayingFile;
            Mediafile mediaFile =  await GetMediafileFromParameterAsync(path, true);
            Messenger.Instance.NotifyColleagues(MessageTypes.MSG_PLAY_SONG, new List<object>() { mediaFile, true, isPlayingFromPlaylist });
            mediaFile.LastPlayed = DateTime.Now.ToString();           
        }
        
        async void Init(object para)
        {
            //NavigationService.Instance.Frame.Navigated += Frame_Navigated;
            //if (ViewSource == null)
            //    ViewSource = (para as Grid).Resources["Source"] as CollectionViewSource;

            await RefreshSourceAsync().ConfigureAwait(false);

            if (source == null && Sort != null && Sort != "Unsorted")
            {
                await LoadCollectionAsync(GetSortFunction(Sort), true).ConfigureAwait(false);
            }
            else if (source == null && Sort == "Unsorted" || Sort == null)
            {
                await LoadCollectionAsync(GetSortFunction("FolderPath"), false).ConfigureAwait(false);
            }
        }
        #endregion

        #endregion
        
        #region Methods 
        private void SendLibraryLoadedMessage(object payload, bool sendMessage)
        {
            if (sendMessage)
            {
                Messenger.Instance.NotifyColleagues(MessageTypes.MSG_LIBRARY_LOADED, payload);
                isPlayingFromPlaylist = true;
            }
        }
        private async Task<Mediafile> GetMediafileFromParameterAsync(object path, bool sendUpdateMessage = false)
        {
            if (path is Mediafile mediaFile)
            {
                isPlayingFromPlaylist = false;
               // SendLibraryLoadedMessage(TracksCollection.Elements, true);
                return mediaFile;
            }
            else if (path is IEnumerable<Mediafile> tmediaFile)
            {
                var col = new ThreadSafeObservableCollection<Mediafile>(tmediaFile);
                SendLibraryLoadedMessage(col, sendUpdateMessage);
                return col[0];
            }
            else if (path is Playlist playlist)
            { 
                var songList = new ThreadSafeObservableCollection<Mediafile>(await PlaylistService.GetTracksAsync(playlist.Id));
                SendLibraryLoadedMessage(songList, sendUpdateMessage);
                return songList[0];
            }
            else if(path is Album album)
            {
                var songList = new ThreadSafeObservableCollection<Mediafile>(await LibraryService.Query(album.AlbumName + " " + album.Artist));
                SendLibraryLoadedMessage(songList, sendUpdateMessage);
                return songList[0];
            }
            return null;
        }
        private async Task<ThreadSafeObservableCollection<Mediafile>> GetMostPlayedSongsAsync()
        {
            return await Task.Run(() =>
            {
                MostEatenSongsCollection.AddRange(TracksCollection.Elements.Where(t => t.PlayCount > 1 && !MostEatenSongsCollection.All(a => a.Path == t.Path)));
                return MostEatenSongsCollection;
            });
        }
        private async Task<ThreadSafeObservableCollection<Mediafile>> GetRecentlyPlayedSongsAsync()
        {
            return await Task.Run(() =>
            {
                RecentlyPlayedCollection.AddRange(TracksCollection.Elements.Where(t => t.LastPlayed != null && (DateTime.Now.Subtract(DateTime.Parse(t.LastPlayed))).Days <= 2 && !RecentlyPlayedCollection.All(a => a.Path == t.Path)));
                return RecentlyPlayedCollection;
            });
        }
        private async Task<ThreadSafeObservableCollection<Mediafile>> GetFavoriteSongs()
        {
            return await Task.Run(() =>
            {
                FavoriteSongsCollection.AddRange(TracksCollection.Elements.Where(t => t.IsFavorite));
                return FavoriteSongsCollection;
            });
        }
        private async Task<ThreadSafeObservableCollection<Mediafile>> GetRecentlyAddedSongsAsync()
        {
            return await Task.Run(() =>
            {
                RecentlyAddedSongsCollection.AddRange(TracksCollection.Elements.Where(item => item.AddedDate != null && (DateTime.Now.Subtract(DateTime.Parse(item.AddedDate))).Days < 3 && !RecentlyAddedSongsCollection.All(t => t.Path == item.Path)));
                return RecentlyAddedSongsCollection;
            });
        }

        async Task RefreshSourceAsync()
        {
            await CrossPlatformHelper.Dispatcher.RunAsync(() =>
            {
                if (source != null)
                {
                    if (grouped && source == TracksCollection.Elements)
                        ViewSource.Source = TracksCollection;
                    else
                        ViewSource.Source = source;

                    ViewSource.IsSourceGrouped = grouped;
                }
            });
        }
        async Task ChangeView(string header, bool group, object src)
        {
            await CrossPlatformHelper.Dispatcher.RunAsync(() =>
            {
                ViewSource.Source = null;
                //Header = header;
                grouped = group;
                source = src;
                libgrouped = ViewSource.IsSourceGrouped;
                var tMediaFile = src as ThreadSafeObservableCollection<Mediafile>;
                if (tMediaFile?.Any() == true && Player.CurrentlyPlayingFile != null && tMediaFile.FirstOrDefault(t => t.Path == Player.CurrentlyPlayingFile?.Path) != null)
                    tMediaFile.FirstOrDefault(t => t.Path == Player.CurrentlyPlayingFile?.Path).State = PlayerState.Playing;

            });
        }
        async Task LoadCollectionAsync(Func<Mediafile, string> sortFunc, bool group)
        {
            await CrossPlatformHelper.Dispatcher.RunAsync(async () =>
            {
                grouped = group;
                TracksCollection = new GroupedObservableCollection<string, Mediafile>(sortFunc);
                TracksCollection.CollectionChanged += TracksCollection_CollectionChanged;

                SongCount = LibraryService.SongCount;
                if (group)
                {
                    ViewSource.Source = TracksCollection;
                }
                else
                    ViewSource.Source = TracksCollection.Elements;

                ViewSource.IsSourceGrouped = group;
                //await SplitList(TracksCollection, 300).ConfigureAwait(false);
                await TracksCollection.AddRange(await LibraryService.GetAllMediafiles());
            });
        }

        /// <summary>
        /// Refresh the view, based on filters and sorting mechanisms.
        /// </summary>
        public async Task RefreshView(string genre = "All genres", string propName = "Title", bool doOrderFiles = true)
        {
            if (doOrderFiles)
            {
                Sort = propName;
                if (propName != "Unsorted")
                {
                    await CrossPlatformHelper.Dispatcher.RunAsync(async () =>
                    {
                        if (files == null)
                            files = TracksCollection.Elements;
                        grouped = true;
                        TracksCollection = new GroupedObservableCollection<string, Mediafile>(GetSortFunction(propName));
                        ViewSource.Source = TracksCollection;
                        ViewSource.IsSourceGrouped = true;
                        await TracksCollection.AddRange(files, true, false);
                        UpdateJumplist(propName);
                        await RemoveDuplicateGroups();
                    });
                }
                else
                {
                    ViewSource.Source = TracksCollection.Elements;
                    ViewSource.IsSourceGrouped = false;
                    grouped = false;
                    Messenger.Instance.NotifyColleagues(MessageTypes.MSG_LIBRARY_LOADED, new List<object>() { TracksCollection, grouped });
                }
            }
            else
            {
                Genre = genre;
                ThreadSafeObservableCollection<Mediafile> FilteredSongsCollection = new ThreadSafeObservableCollection<Mediafile>();
                if (genre != "All genres")
                {
                    var results = await LibraryService.Query(genre);
                    FilteredSongsCollection.AddRange(results.ToList());
                }
                else
                    FilteredSongsCollection.AddRange(OldItems);
                await ChangeView("Music Collection", false, FilteredSongsCollection);
                await RefreshSourceAsync();
            }
        }
        
        async Task RemoveDuplicateGroups()
        {
            //the only workaround to remove the first group which is a 'false' duplicate really.
            await CrossPlatformHelper.Dispatcher.RunAsync(() =>
            {
                if (ViewSource.IsSourceGrouped)
                {
                    UpdateJumplist(Sort);
                    //ViewSource.IsSourceGrouped = false;
                   // ViewSource.IsSourceGrouped = true;
                }
            });
        }
        Func<Mediafile, string> GetSortFunction(string propName)
        {
            if (propName == null)
                propName = "FolderPath";

            Func<Mediafile, string> f = null;
            switch (propName)
            {
                case "Title":
                    //determine whether the Title, by which groups are made, start with number, letter or symbol. On the basis of that we define the Key for each Group.
                    f = t =>
                    {
                        if (t.Title.StartsWithLetter()) return t.Title[0].ToString().ToUpper();
                        if (t.Title.StartsWithNumber()) return "#";
                        if (t.Title.StartsWithSymbol()) return "&";
                        return t.Title;
                    };
                    break;
                case "Year":
                    f = t => string.IsNullOrEmpty(t.Year) ? "Unknown Year" : t.Year;
                    break;
                case "Length":
                    string[] timeformats = { @"m\:ss", @"mm\:ss", @"h\:mm\:ss", @"hh\:mm\:ss" };
                    f = t => string.IsNullOrEmpty(t.Length) || t.Length == "0:00" 
                    ? "Unknown Length"
                    : Math.Round(TimeSpan.ParseExact(t.Length, timeformats, CultureInfo.InvariantCulture).TotalMinutes) + " minutes";
                    break;
                case "TrackNumber":
                    f = t => string.IsNullOrEmpty(t.TrackNumber) ? "Unknown Track No." : t.TrackNumber;
                    break;
                case "FolderPath":
                    f = t => string.IsNullOrEmpty(t.FolderPath) ? "Unknown Folder" : new DirectoryInfo(t.FolderPath).FullName;
                    break;
                default:
                    f = t => GetPropValue(t, propName) as string; 
                    break;
            }          
            return f;
        }
        private async void UpdateJumplist(string propName)
        {
            try
            {
                switch (propName)
                {
                    case "Year":
                    case "TrackNumber":
                        AlphabetList = TracksCollection.Keys.DistinctBy(t => t).ToList();
                        break;
                    case "Length":
                        AlphabetList = TracksCollection.Keys.Select(t => t.Replace(" minutes", "")).DistinctBy(a => a).ToList();
                        break;
                    case "FolderPath":
                        AlphabetList = TracksCollection.Keys.Select(t => new DirectoryInfo(t).Name.Remove(1)).DistinctBy(t => t).ToList();
                        break;
                    default:
                        AlphabetList = "&#ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray().Select(x => x.ToString()).ToList();
                        break;
                }
                AlphabetList.Sort();
            }
            catch(ArgumentOutOfRangeException ex)
            {
                await CrossPlatformHelper.NotificationManager.ShowMessageAsync("Unable to update jumplist due to some problem with TracksCollection. ERROR INFO: " + ex.Message);
            }
        }
        
        /// <summary>
        /// Creates genre menu.
        /// </summary>
        async Task GetGenres()
        {
            await CrossPlatformHelper.Dispatcher.RunAsync(() =>
            {
                GenreCollection.Add("All genres");
                var genres = TracksCollection.Elements.GroupBy(t => t.Genre).Where(t=> t.Key != "NaN").DistinctBy(t => t.Key);
                GenreCollection.AddRange(genres.Select(t => t.Key));               
            });
        }
      
        private void GetSettings()
        {
            Sort = CrossPlatformHelper.SettingsHelper.GetSetting<string>("Sort", "Unsorted") ?? "Unsorted";
        }
        
        #endregion

        #region Helper Methods

        /// <summary>
        /// Gets name of a property in a class. 
        /// </summary>
        /// <param name="src">Class to search for property.</param>
        /// <param name="propName">Property to search for.</param>
        /// <returns></returns>
        public static object GetPropValue(object src, string propName)
        {
            return src.GetType().GetTypeInfo().GetDeclaredProperty(propName).GetValue(src, null);
        }       

        #endregion

        #region Disposable
        public void Reset()
        {
            LibraryService.Dispose();
            LibraryService = null;
            TracksCollection.Clear();
            RecentlyPlayedCollection.Clear();
            CrossPlatformHelper.PlaylistHelper.ClearPlaylists();
            OldItems = null;
            PlaylistCollection.Clear();
            ViewModels.Init.SharedLogic.OptionItems.Clear();
            GenreCollection.Clear();
            SongCount = -1;
        }
        #endregion

        #region Library Methods
        /// <summary>
        /// Loads library from the database file.
        /// </summary>
        async Task LoadLibrary()
        {
            GetSettings();
            ViewModels.Init.SharedLogic.OptionItems.Add(new ContextMenuCommand(AddToPlaylistCommand, "New Playlist"));
            await LoadPlaylists();
            UpdateJumplist("Title");
        }     
      
        #endregion

        #region Playlist Methods

       async Task LoadPlaylists()
        {
            foreach (var list in await PlaylistService.GetPlaylistsAsync())
            {
                CrossPlatformHelper.PlaylistHelper.AddPlaylist(list);
            }
        }
        async void AddToPlaylist(object file)
        {
            if (file != null)
            {
            //    MenuFlyoutItem menu = file as MenuFlyoutItem;
            //    //songList is a variable to initiate both (if available) sources of songs. First is AlbumSongs and the second is the direct library songs.
            //    List<Mediafile> songList = new List<Mediafile>();
            //    if (menu.Tag == null)
            //    {
            //        songList = SelectedItems;
            //    }
            //    else
            //    {
            //        songList.Add(Player.CurrentlyPlayingFile);
            //    }
            //    Playlist dictPlaylist = menu.Text == "New Playlist" ? await CrossPlatformHelper.PlaylistHelper.ShowAddPlaylistDialogAsync() : await PlaylistService.GetPlaylistAsync(menu?.Text);
            //    bool proceed = false;
            //    if (menu.Text != "New Playlist")
            //        proceed = await ViewModels.Init.SharedLogic.AskPasswordForPlaylist(dictPlaylist);
            //    else
            //        proceed = true;
            //    if (dictPlaylist != null && proceed)
            //        await AddPlaylistAsync(dictPlaylist, true, songList);
            //}
            //else
            //{
            //    var pList = await CrossPlatformHelper.PlaylistHelper.ShowAddPlaylistDialogAsync();
            //    if(pList != null)
            //        await AddPlaylistAsync(pList, false);
            }
        }

        //async Task<Playlist> ShowAddPlaylistDialogAsync(string title = "Name this playlist", string playlistName = "", string desc = "", string password = "")
        //{
        //    var dialog = new InputDialog()
        //    {
        //        Title = title,
        //        Text = playlistName,
        //        Description = desc,
        //        IsPrivate = password.Length > 0,
        //        Password = password
        //    };
        //    if (CoreWindow.GetForCurrentThread().Bounds.Width <= 501)
        //        dialog.DialogWidth = CoreWindow.GetForCurrentThread().Bounds.Width - 50;
        //    else
        //        dialog.DialogWidth = CoreWindow.GetForCurrentThread().Bounds.Width - 300;
        //    if (await dialog.ShowAsync() == ContentDialogResult.Primary && dialog.Text != "")
        //    {
        //        var salthash = Core.Common.PasswordStorage.CreateHash(dialog.Password);
        //        var Playlist = new Playlist();
        //        Playlist.Name = dialog.Text;
        //        Playlist.Description = dialog.Description;
        //        Playlist.IsPrivate = dialog.Password.Length > 0;
        //        Playlist.Hash = salthash.Hash;
        //        Playlist.Salt = salthash.Salt;
        //        if (PlaylistService.PlaylistExists(Playlist.Name))
        //        {
        //            Playlist = await ShowAddPlaylistDialogAsync("Playlist already exists! Please choose another name.", Playlist.Name, Playlist.Description);
        //        }
        //        return Playlist;
        //    }
        //    return null;
        //}

        public async Task AddSongsToPlaylist(Playlist list, List<Mediafile> songsToadd)
        {
            if (songsToadd.Any())
            {
                await Task.Run(async () =>
                {
                    await PlaylistService.InsertTracksAsync(songsToadd.Where(t => !PlaylistService.Exists(t.Id)), list);
                });
            }
        }
      
        //public void AddPlaylist(Playlist Playlist)
        //{
        //    var cmd = new ContextMenuCommand(AddToPlaylistCommand, Playlist.Name);
        //    ViewModels.Init.SharedLogic.OptionItems.Add(cmd);
        //    SharedLogic.PlaylistsItems.Add(new SplitViewMenu.SimpleNavMenuItem
        //    {
        //        Arguments = Playlist,
        //        Label = Playlist.Name,
        //        DestinationPage = typeof(PlaylistView),
        //        Symbol = Symbol.List,
        //        FontGlyph = "\u0045",
        //        ShortcutTheme = ElementTheme.Dark,
        //        HeaderVisibility = Visibility.Collapsed
        //    });
        //}
        public async Task AddPlaylistAsync(Playlist plist, bool addsongs, List<Mediafile> songs = null)
        {
            await CrossPlatformHelper.Dispatcher.RunAsync(async () =>
            {
                if (!PlaylistService.PlaylistExists(plist.Name))
                {
                    CrossPlatformHelper.PlaylistHelper.AddPlaylist(plist);
                    await PlaylistService.AddPlaylistAsync(plist);
                }
                if (addsongs)
                    await AddSongsToPlaylist(plist, songs);
            });
        }
        #endregion

        #region Events
        private async void TracksCollection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (TracksCollection.Elements.Count == SongCount)
            {
                await RemoveDuplicateGroups().ConfigureAwait(false);
                await CrossPlatformHelper.Dispatcher.RunAsync(() =>
                {
                    MusicLibraryLoaded?.Invoke(this, new EventArgs());
                });
                OldItems = TracksCollection.Elements;
                TracksCollection.CollectionChanged -= TracksCollection_CollectionChanged;
            }
        }
        private async void LibraryViewModel_MusicLibraryLoaded(object sender, EventArgs e)
        {
            if (!libraryLoaded)
            {
                libraryLoaded = true;
                await GetGenres().ConfigureAwait(false);
                CrossPlatformHelper.Log.I("Library successfully loaded!");
                await CrossPlatformHelper.NotificationManager.ShowMessageAsync("Library successfully loaded!", 4);
                Messenger.Instance.NotifyColleagues(MessageTypes.MSG_LIBRARY_LOADED, new List<object>() { TracksCollection, grouped });
                //await Task.Delay(10000);
                //Common.DirectoryWalker.SetupDirectoryWatcher(SharedLogic.SettingsVM.LibraryFoldersCollection);
            }
        }
        private async void Elements_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            await Task.Delay(1000);
            if (RecentlyPlayedCollection.Count <= 100)
            {
                RecentlyPlayedCollection.RemoveAt(RecentlyPlayedCollection.Count + 1);
            }
        }     

        //public void PlayOnTap(object sender, TappedRoutedEventArgs e)
        //{
        //    if (e.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch && !IsMultiSelectModeEnabled)
        //    {
        //        Play((e.OriginalSource as Border).Tag);
        //    }
        //}
        public List<Mediafile> SelectedItems { get; set; } = new List<Mediafile>();

        //public void SelectionChanged(object para, SelectionChangedEventArgs selectionEvent)
        //{
        //    if (selectionEvent.RemovedItems.Count > 0)
        //    {
        //        foreach (var toRemove in selectionEvent.RemovedItems.Cast<Mediafile>())
        //        {
        //            SelectedItems.Remove(toRemove);
        //        }
        //    }
        //    if (selectionEvent.AddedItems.Count > 0)
        //        SelectedItems.AddRange(selectionEvent.AddedItems.Cast<Mediafile>().ToList());
        //}
        #endregion

        public event OnMusicLibraryLoaded MusicLibraryLoaded;
    }

    public delegate void OnMusicLibraryLoaded(object sender, EventArgs e);    
}
