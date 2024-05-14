using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace AudioHotkey.Common
{
    public static class Utils
    {
        public static void DisplayErrorBox(string title, string message)
        {
            PInvoke.MessageBoxEx((HWND) 0, message, title, MESSAGEBOX_STYLE.MB_OK | MESSAGEBOX_STYLE.MB_ICONERROR, 0);
        }
    }
}
