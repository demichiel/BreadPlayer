using BreadPlayer.Helpers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace BreadPlayer.Helpers
{
    public class CustomViewSource : ICustomViewSource
    {
        private CollectionViewSource ViewSource { get; set; }
        public object Source
        {
            get => ViewSource.Source;
            set => ViewSource.Source = value;
        }
        public bool IsSourceGrouped
        {
            get => ViewSource.IsSourceGrouped;
            set => ViewSource.IsSourceGrouped = value;
        }
        public CustomViewSource(CollectionViewSource source)
        {
            ViewSource = source;
        }
    }
}
