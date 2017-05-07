using BreadPlayer.Helpers.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace BreadPlayer.Helpers
{
    public class CrossPlatformHelper
    {
        public static bool IsMobile { get; set; }
        public static INotificationManager NotificationManager { get; set; }
        public static IEqualizerSettingsHelper EqualizerSettingsHelper { get; set; }
        public static IDispatcher Dispatcher { get; set; }
        public static Log Log { get; set; }
    }
}
