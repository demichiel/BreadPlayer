using BreadPlayer.Helpers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Display;
using Windows.UI.ViewManagement;

namespace BreadPlayer.Helpers
{
    public class WindowHelper : IWindowHelper
    {
        public void EnterFullscreen()
        {
            ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
        }

        public string GetOrientation()
        {
           return DisplayInformation.GetForCurrentView().CurrentOrientation.ToString();
        }

        public void RotateLandscape()
        {
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape;
        }

        public void RotatePotrait()
        {
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait;
        }
    }
}
