using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace BreadPlayer.Helpers.Interfaces
{
    public interface IFilePickerHelper
    {
        Task<string> PickFileAsync(IEnumerable<string> filters);
        Task<Stream> SaveFileAsync(IDictionary<string, IList<string>> filters);
    }
}
