using System;
using System.Collections.Generic;
using System.Text;

namespace BreadPlayer.Helpers.Interfaces
{
    public interface IWindowHelper
    {
        void EnterFullscreen();
        void RotatePotrait();
        void RotateLandscape();
        string GetOrientation();
    }
}
