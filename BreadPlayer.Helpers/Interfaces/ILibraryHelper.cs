using System;
using System.Collections.Generic;
using System.Text;

namespace BreadPlayer.Helpers.Interfaces
{
    public interface ILibraryHelper
    {
        void SelectionChanged(object sender, object parameter);
        void PlayOnTap(object sender, object parameter);
    }
}
