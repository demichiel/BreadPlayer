using BreadPlayer.Helpers;
using BreadPlayer.Models;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace BreadPlayer.PlaylistBus
{
    public class M3U : IPlaylist
    {
        public async Task<IEnumerable<string>> LoadPlaylist(string playlistPath)
        {
            using (var streamReader = new StreamReader(new FileStream(playlistPath, FileMode.Open, FileAccess.Read)))
            {
                List<string> Songs = new List<string>();
                string line;
                int index = 0;
                int failedFiles = 0;
                bool ext = false;
                while ((line = streamReader.ReadLine()) != null)
                {
                    if (line.ToLower() == "#extm3u") //m3u header
                        ext = true;
                    else if (ext && line.ToLower().StartsWith("#extinf:")) //extinfo of each song
                        continue;
                    else if (line.StartsWith("#") || line == "") //pass blank lines
                        continue;
                    else
                    {
                        try
                        {
                            index++;
                            FileInfo info = new FileInfo(playlistPath);//get playlist file info to get directory path
                            string path = line;
                            if (!File.Exists(line) && line[1] != ':') // if file doesn't exist then perhaps the path is relative
                            {
                                path = info.DirectoryName + line; //add directory path to song path.
                            }
                            Songs.Add(path);
                        }
                        catch
                        {
                            failedFiles++;
                        }
                    }                  
                    string message = string.Format("Playlist \"{3}\" successfully imported! Total Songs: {0} Failed: {1} Succeeded: {2}", index, failedFiles, index - failedFiles, Path.GetFileNameWithoutExtension(playlistPath));
                    await CrossPlatformHelper.NotificationManager.ShowMessageAsync(message);
                    CrossPlatformHelper.Log.I(message);
                }
                return Songs;
            }
        }

        public async Task<bool> SavePlaylist(IEnumerable<Mediafile> Songs)
        {
            var filters = new Dictionary<string, IList<string>>();
            filters.Add("M3U Playlist", new List<string>() { ".m3u" });
            using (StreamWriter writer = new StreamWriter(await CrossPlatformHelper.FilePickerHelper.SaveFileAsync(filters)))
            {
                writer.WriteLine("#EXTM3U");
                writer.WriteLine("");
                foreach (var track in Songs)
                {
                    writer.WriteLine(string.Format("#EXTINF:{0},{1} - {2}", track.Length, track.LeadArtist, track.Title));
                    writer.WriteLine(track.Path);
                    writer.WriteLine("");
                }
            }
            return false;
        }
    }
}
