﻿using BreadPlayer.Core.Models;
using System.Collections.ObjectModel;
using System.Linq;

namespace BreadPlayer.Core.Engines.Interfaces
{
    public abstract class Equalizer : ObservableObject
    {
        // Center: Frequency center. 20.0 to 22000.0. Default = 8000.0
        // Bandwith: Octave range around the center frequency to filter. 0.2 to 5.0. Default = 1.0
        // Gain: Frequency Gain. -30 to +30. Default = 0
        public static readonly float[][] EqDefaultValues = {
              new[] {32f, 1f, 0f},
              new[] {64f, 1f, 0f},
              new[] {125f, 1f, 0f},
              new[] {250f, 1f, 0f},
              new[] {500f, 1f, 0f},
              new[] {1000f, 1f, 0f},
              new[] {2000f, 1f, 0f},
              new[] {4000f, 1f, 0f},
              new[] {8000f, 1f, 0f},
              new[] {16000f, 1f, 0f }
        };

        public EqualizerSettings EqualizerSettings { get; set; }
        public ObservableCollection<EqualizerSettings> Presets { get; set; }
        private int _selectedPreset = -1;

        public int SelectedPreset
        {
            get => _selectedPreset;
            set
            {
                _selectedPreset = value;
                if (_selectedPreset == -1)
                {
                    return;
                }

                var preset = Presets[_selectedPreset];
                EqualizerSettings = preset;
                DeInit();
                Init();
            }
        }

        public ObservableCollection<IEqualizerBand> Bands { get; set; }
        private bool _isEnabled;

        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                Set(ref _isEnabled, value);
                if (value)
                {
                    Init();
                }
                else
                {
                    SaveEqualizerSettings();
                    DeInit();
                }
            }
        }

        private float _preamp = 1f;

        public float Preamp
        {
            get => _preamp;
            set => _preamp = value;
        }

        public bool IsPreampAvailable { get; set; }
        public string Name { get; set; }

        public abstract void Init(bool setToDefaultValues = false);

        public abstract void DeInit();

        public abstract void Dispose();

        public void SaveEqualizerSettings()
        {
            var equalizerSettings = EqualizerSettings;
            if (equalizerSettings == null || equalizerSettings.Name == null)
            {
                equalizerSettings = new EqualizerSettings { Name = Name };
                EqualizerSettings = equalizerSettings;
            }
            equalizerSettings.GainValues = Bands.ToDictionary(b => b.BandCaption, b => b.Gain);
            equalizerSettings.IsEnabled = IsEnabled;
            InitializeCore.EqualizerSettingsHelper.SaveEqualizerSettings(equalizerSettings, 1);
        }

        public void SetToDefault()
        {
            DeInit();
            Init();
        }

        public abstract IEqualizerBand GetEqualizerBand(bool isActive, float centerValue, float bandwithValue, float gainValue);
    }
}