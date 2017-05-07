using BreadPlayer.Helpers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using System.IO;

namespace BreadPlayer.Helpers
{
    public class FilePickerHelper : IFilePickerHelper
    {
        public async Task<string> PickFileAsync(IEnumerable<string> filters)
        {
            FileOpenPicker picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".mp3");
            picker.FileTypeFilter.Concat(filters);
            picker.CommitButtonText = "Play";
            picker.SuggestedStartLocation = PickerLocationId.MusicLibrary;
            picker.ViewMode = PickerViewMode.Thumbnail;
            return (await picker.PickSingleFileAsync()).Path;
        }

        public async Task<Stream> SaveFileAsync(IDictionary<string, IList<string>> filters)
        {
            FileSavePicker picker = new FileSavePicker();
            picker.FileTypeChoices.Concat(filters);
            picker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
            var file = await picker.PickSaveFileAsync();
            return await file.OpenStreamForWriteAsync();
        }
    }
}
