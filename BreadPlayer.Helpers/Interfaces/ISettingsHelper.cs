using System;
using System.Collections.Generic;
using System.Text;

namespace BreadPlayer.Helpers.Interfaces
{
    public abstract class SettingsHelper : IEqualizerSettingsHelper
    {
        public virtual void SaveSetting(string key, object value) { }
        public virtual T GetSetting<T>(string key, object def) { return default(T); }

        public abstract (float[] EqConfig, bool IsEnabled, float PreAMP) LoadEqualizerSettings();
        public abstract void SaveEqualizerSettings(float[] eqConfig, bool isEnabled, float PreAmp);
    }
}
