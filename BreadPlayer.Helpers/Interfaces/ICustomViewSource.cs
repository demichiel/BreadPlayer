using System;
using System.Collections.Generic;
using System.Text;

namespace BreadPlayer.Helpers.Interfaces
{
    public interface ICustomViewSource
    {
        object Source { get; set; }
        bool IsSourceGrouped { get; set; }
    }
}
