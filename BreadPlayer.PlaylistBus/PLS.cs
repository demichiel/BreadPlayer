using BreadPlayer.Database;
using BreadPlayer.Helpers;
using BreadPlayer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace BreadPlayer.PlaylistBus
{
    class PLS : IPlaylist
    {
        public async Task<IEnumerable<string>> LoadPlaylist(string playlistPath)
        {
            using (var reader = new StreamReader(new FileStream(playlistPath, FileMode.Open, FileAccess.Read)))
            {
                bool hdr = false; //[playlist] header
                string version = ""; //pls version at the end
                int noe = 0; //numberofentries at the end.
                int nr = 0;
                int failedFiles = 0;
                int count = 0;
                string line; //a single line in stream
                List<string> lines = new List<string>();
                List<string> Songs = new List<string>();
                while ((line = reader.ReadLine()) != null)
                {
                    lines.Add(line);
                    nr++;
                    if (line == "[playlist]")
                        hdr = true;
                    else if (!hdr)
                        return null;
                    else if (line.ToLower().StartsWith("numberofentries="))
                        noe = Convert.ToInt32(line.Split('=')[1]);
                    else if (line.ToLower().StartsWith("version="))
                        version = line.Split('=')[1];
                }
                string[,] tracks = new string[noe, 3];
                nr = 0;
                foreach (string l in lines)
                {
                    var _l = l.ToLower();
                    if (_l.StartsWith("file") || _l.StartsWith("title") || _l.StartsWith("length"))
                    {
                        int tmp = 4;
                        int index = 0;
                        if (_l.StartsWith("title")) { tmp = 5; index = 1; }
                        else if (_l.StartsWith("length")) { tmp = 6; index = 2; }

                        string[] split = l.Split('=');
                        int number = Convert.ToInt32(split[0].Substring(tmp));

                        if (number > noe)
                            continue;
                        else
                            tracks[number - 1, index] = split[1];
                    }
                    else if (!_l.StartsWith("numberofentries") && _l != "[playlist]" && !_l.StartsWith("version="))
                    {
                        continue;
                    }
                }

                for (int i = 0; i < noe; i++)
                {
                    try
                    {
                        string trackPath = tracks[i, 0];
                        FileInfo info = new FileInfo(playlistPath);//get playlist file info to get directory path
                        string path = trackPath;
                        if (!File.Exists(trackPath) && line[1] != ':') // if file doesn't exist then perhaps the path is relative
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
                string message = string.Format("Playlist \"{3}\" successfully imported! Total Songs: {0} Failed: {1} Succeeded: {2}", count, failedFiles, count - failedFiles, Path.GetFileNameWithoutExtension(playlistPath));
                await CrossPlatformHelper.NotificationManager.ShowMessageAsync(message);
                CrossPlatformHelper.Log.I(message);
                return Songs;
            }
        }

        public async Task<bool> SavePlaylist(IEnumerable<Mediafile> Songs)
        {
            var filters = new Dictionary<string, IList<string>>();
            filters.Add("PLS Playlist", new List<string>() { ".pls" });
            using (StreamWriter writer = new StreamWriter(await CrossPlatformHelper.FilePickerHelper.SaveFileAsync(filters)))
            {
                writer.WriteLine("[playlist]");
                writer.WriteLine("");
                int i = 0;
                foreach (var track in Songs)
                {
                    i++;
                    writer.WriteLine(string.Format("File{0}={1}", i, track.Path));
                    writer.WriteLine(string.Format("Title{0}={1}", i, track.Title));
                    writer.WriteLine(string.Format("Length{0}={1}", i, track.Length));
                    writer.WriteLine("");
                }
                writer.WriteLine("NumberOfEntries=" + i);
                writer.WriteLine("Version=2");
            }
            return false;
        }
    }
}
