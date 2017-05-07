using BreadPlayer.Helpers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Input;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace BreadPlayer.Helpers
{
    public class DeviceHelper : IDeviceHelper
    {
        public bool IsDeviceTouchEnabled()
        {
            TouchCapabilities touchCapabilities = new Windows.Devices.Input.TouchCapabilities();
            return touchCapabilities.TouchPresent != 0;
        }
    }
}
