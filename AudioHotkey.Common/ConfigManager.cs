using Microsoft.Win32;
using static System.Runtime.CompilerServices.RuntimeHelpers;

namespace AudioHotkey.Common
{
    public class ConfigManager
    {
        private static readonly string RegKey = @"HKEY_CURRENT_USER\Software\VaporSoft\AudioHotkey";
        private static readonly string RegRunKey = @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run";
        private static readonly string RegListenDeviceId = @"listenDeviceId";
        private static readonly string RegPlaybackDeviceIds = @"playbackDeviceIds";
        private static readonly string RegListenKeybind = @"listenKeybind";
        private static readonly string RegSwitchKeybind = @"switchKeybind";
        private static readonly string RegListenEnabled = @"listenEnabled";
        private static readonly string RegRun = @"AudioHotkey";

        public static void SetListenDeviceId(string listenDeviceId)
        {
            Registry.SetValue(RegKey, RegListenDeviceId, listenDeviceId);
        }

        public static string? GetListenDeviceId()
        {
            string? retval = (string?)Registry.GetValue(RegKey, RegListenDeviceId, null);
            if (string.IsNullOrEmpty(retval)) return null;
            return retval;
        }

        public static void SetPlaybackDeviceIds(ICollection<string> playbackDeviceIds)
        {
            Registry.SetValue(RegKey, RegPlaybackDeviceIds, string.Join('|', playbackDeviceIds));
        }

        public static IList<string>? GetPlaybackDeviceIds()
        {
            string? strVal = (string?) Registry.GetValue(RegKey, RegPlaybackDeviceIds, null);
            if (strVal == null ) return null;
            var retVal = strVal.Split('|');
            if(retVal.Length == 0 ) return null;
            return retVal.ToList();
        }

        public static void SetListenKeybind(uint keycode)
        {
            Registry.SetValue(RegKey, RegListenKeybind, keycode);
        }

        public static uint? GetListenKeybind()
        {
            string? retval = (string?)Registry.GetValue(RegKey, RegListenKeybind, 0);
            if (string.IsNullOrEmpty(retval)) return null;
            if(uint.TryParse(retval, out var val))
            {
                return val;
            }
            return null;
        }

        public static void SetSwitchKeybind(uint keycode)
        {
            Registry.SetValue(RegKey, RegSwitchKeybind, keycode);
        }

        public static uint? GetSwitchKeybind()
        {
            string? retval = (string?)Registry.GetValue(RegKey, RegSwitchKeybind, 0);
            if (string.IsNullOrEmpty(retval)) return null;
            if (uint.TryParse(retval, out var val))
            {
                return val;
            }
            return null;
        }

        public static void SetListenEnabled(bool value)
        {
            Registry.SetValue(RegKey, RegListenEnabled, value);
        }

        public static bool? GetListenEnabled()
        {
            string? retval = (string?)Registry.GetValue(RegKey, RegListenEnabled, null);
            if (string.IsNullOrEmpty(retval)) return null;
            if (bool.TryParse(retval, out var val))
            {
                return val;
            }
            return null;
        }

        public static void SetStartup(string path)
        {
            Registry.SetValue(RegRunKey, RegRun, path);
        }
    }
}
