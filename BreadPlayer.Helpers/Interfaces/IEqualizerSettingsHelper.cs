
namespace BreadPlayer.Helpers.Interfaces
{
    public interface IEqualizerSettingsHelper
    {
        (float[] EqConfig, bool IsEnabled, float PreAMP) LoadEqualizerSettings();
        void SaveEqualizerSettings(float[] eqConfig, bool isEnabled, float PreAmp);
    }
}
