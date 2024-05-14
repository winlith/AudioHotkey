using AudioHotkey.Common;
using SharpHook;
using SharpHook.Native;

namespace AudioHotkey.Config
{
    internal class Program
    {
        private static KeyCode switchKey;
        private static KeyCode listenKey;
        private static TaskPoolGlobalHook? hook;
        private static bool waitingForKey;
        private static bool waitingForListen;

        static void Main(string[] args)
        {
            Console.WriteLine("AudioHotkey Configuration Tool\n");

            var playbackDevices = AudioManager.GetAllPlaybackDevices();
            var listenDevices = AudioManager.GetAllListenDevices();

            Console.WriteLine("Select playback devices you want to use:\n(space separated, e.g. 0 5 8)");
            foreach (var device in playbackDevices)
            {
                Console.WriteLine($"[{playbackDevices.IndexOf(device)}] {device.Name}");
            }

            bool inputCorrect = false;
            uint[]? playbackChoices = null;
            do
            {
                var choiceLine = Console.ReadLine();
                playbackChoices = choiceLine?.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(c =>
                {
                    if (!uint.TryParse(c, out var value))
                    {
                        return uint.MaxValue;
                    }
                    return value;
                }).ToArray();

                if (playbackChoices == null || playbackChoices.Contains(uint.MaxValue) || playbackChoices.Where(c => c < 0 || c >= playbackDevices.Count).Count() > 0)
                {
                    Console.WriteLine(@"Invalid input, try again.");
                }
                else
                {
                    inputCorrect = true;
                }
            }
            while (!inputCorrect);

            Console.WriteLine("Select the input device you want to listen to:");
            foreach (var device in listenDevices)
            {
                Console.WriteLine($"[{listenDevices.IndexOf(device)}] {device.Name}");
            }

            inputCorrect = false;
            uint listenChoice;
            do
            {
                var choiceLine = Console.ReadLine();
                if (!uint.TryParse(choiceLine, out listenChoice))
                {
                    Console.WriteLine(@"Invalid input, try again.");
                }
                else
                {
                    if (listenChoice < 0 || listenChoice >= listenDevices.Count)
                    {
                        Console.WriteLine(@"Invalid input, try again.");
                    }
                    else
                    {
                        inputCorrect = true;
                    }
                }
            }
            while (!inputCorrect);



            Console.WriteLine(@"Press a key you want to use for switching playback devices...");
            hook = new();
            hook.KeyPressed += KeyPressed;
            waitingForKey = true;
            waitingForListen = false;
            hook.RunAsync();
            while (waitingForKey) { Thread.Sleep(100); }

            Console.WriteLine(@"Press a key you want to use for switching listening of an input device...");
            waitingForKey = true;
            waitingForListen = true;
            while (waitingForKey) { Thread.Sleep(100); }

            hook.Dispose();

            List<string> selectedPlaybackDeviceIds = new();
            foreach (var index in playbackChoices!)
            {
                selectedPlaybackDeviceIds.Add(playbackDevices[(int)index].Id);
            }
            ConfigManager.SetPlaybackDeviceIds(selectedPlaybackDeviceIds);
            ConfigManager.SetListenDeviceId(listenDevices[(int)listenChoice].Id);
            ConfigManager.SetSwitchKeybind((uint)switchKey);
            ConfigManager.SetListenKeybind((uint)listenKey);

            Console.WriteLine(@"Do you want AudioHotkey to run at startup? (y/n)");
            inputCorrect = false;
            do
            {
                var choiceLine = Console.ReadLine();
                switch (choiceLine?[0])
                {
                    case 'y':
                        var currentPath = AppDomain.CurrentDomain.BaseDirectory;
                        var fullPath = Path.Combine(currentPath, "AudioHotkey.Worker.exe");
                        ConfigManager.SetStartup(fullPath);
                        inputCorrect = true;
                        break;
                    case 'n':
                        inputCorrect = true;
                        break;
                    default: break;
                }
            }
            while (!inputCorrect);

            Console.WriteLine("Configuration complete.");
            Console.ReadLine();
        }

        private static void KeyPressed(object? sender, KeyboardHookEventArgs e)
        {
            if(waitingForKey)
            {
                if(waitingForListen)
                {
                    listenKey = e.Data.KeyCode;
                }
                else
                {
                    switchKey = e.Data.KeyCode;
                }
                Console.WriteLine(e.Data.ToString());
                waitingForKey = false;
            }
        }
    }
}
