using AudioHotkey.Common;
using Microsoft.Win32;
using SharpHook;
using SharpHook.Native;

namespace AudioHotkey.Worker
{
    internal class Program
    {
        private static string? listenDeviceId;
        private static IList<string>? playbackDeviceIds;
        private static uint? listenKeybind;
        private static uint? switchKeybind;
        private static TaskPoolGlobalHook? hook;

        static void Main(string[] args)
        {
            listenDeviceId = ConfigManager.GetListenDeviceId();
            playbackDeviceIds = ConfigManager.GetPlaybackDeviceIds();
            listenKeybind = ConfigManager.GetListenKeybind();
            switchKeybind = ConfigManager.GetSwitchKeybind();

            if (listenDeviceId == null || playbackDeviceIds == null || listenKeybind == null || switchKeybind == null)
            {
                Utils.DisplayErrorBox("AudioHotkey Error", "Missing configuration. Run the Config app and set it up.");
                return;
            }

            hook = new();
            hook.KeyPressed += KeyPressed;
            SystemEvents.SessionEnding += SessionEnding;
            hook.Run();
        }

        private static void SessionEnding(object sender, SessionEndingEventArgs e)
        {
            hook?.Dispose();
        }

        private static void KeyPressed(object? sender, KeyboardHookEventArgs e)
        {
            if (e.Data.KeyCode == (KeyCode) listenKeybind!)
            {
                var enabledOld = ConfigManager.GetListenEnabled();
                if (enabledOld == null) enabledOld = false;
                AudioManager.SetListening(listenDeviceId!, !((bool) enabledOld));
                ConfigManager.SetListenEnabled(!((bool) enabledOld));
            }
            else if (e.Data.KeyCode == (KeyCode) switchKeybind!)
            {
                var currentDeviceId = AudioManager.GetCurrentPlaybackDeviceId();
                var index = playbackDeviceIds!.IndexOf(currentDeviceId);
                if (index == -1) index = playbackDeviceIds.Count - 1;
                index = (index + 1) % playbackDeviceIds.Count;
                AudioManager.SetPlaybackDevice(playbackDeviceIds[index]);
            }
        }
    }
}
