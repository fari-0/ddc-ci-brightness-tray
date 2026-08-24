using Microsoft.Win32;

namespace DdcCi.BrightnessTray.App
{
    internal static class StartupManager
    {
        private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string ValueName = "DdcCiBrightnessTray";

        public static bool IsEnabled()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RunKeyPath))
            {
                return key != null && key.GetValue(ValueName) != null;
            }
        }

        public static void Enable()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RunKeyPath, true))
            {
                if (key == null) return;
                key.SetValue(ValueName, "\"" + System.Windows.Forms.Application.ExecutablePath + "\"", RegistryValueKind.String);
            }
        }

        public static void Disable()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RunKeyPath, true))
            {
                if (key == null) return;
                if (key.GetValue(ValueName) != null) key.DeleteValue(ValueName, false);
            }
        }

        public static void Toggle()
        {
            if (IsEnabled()) Disable();
            else Enable();
        }
    }
}
