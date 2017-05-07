using System;
using System.Collections.Generic;
using System.Text;

namespace BreadPlayer.Models.Interfaces
{
    public interface IDBRecord
    {
        long Id { get; set; }
        string GetTextSearchKey();
    }
}
