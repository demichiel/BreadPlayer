using System;
using System.Collections.Generic;
using System.Text;

namespace BreadPlayer.Helpers.Interfaces
{
    public interface ISettingsHelper : IEqualizerSettingsHelper
    {
        void SaveSetting(string key, object value);
        T GetSetting<T>(string key, object def);
    }
}
