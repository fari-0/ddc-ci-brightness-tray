using System;
using System.IO;
using System.Security;
using Microsoft.Win32;

namespace DdcCi.BrightnessTray.App
{
    internal static class StartupManager
    {
        private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string ValueName = "DdcCiBrightnessTray";

        public static bool IsEnabled()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RunKeyPath))
                {
                    if (key == null) return false;
                    object raw = key.GetValue(ValueName);
                    if (raw == null) return false;
                    string stored = raw.ToString().Trim().Trim('"').Trim();
                    string exe = System.Windows.Forms.Application.ExecutablePath;
                    return string.Equals(stored, exe, StringComparison.OrdinalIgnoreCase);
                }
            }
            catch (SecurityException) { return false; }
            catch (UnauthorizedAccessException) { return false; }
            catch (IOException) { return false; }
        }

        public static void Enable()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(RunKeyPath))
                {
                    if (key == null) return;
                    key.SetValue(ValueName, "\"" + System.Windows.Forms.Application.ExecutablePath + "\"", RegistryValueKind.String);
                }
            }
            catch (SecurityException) { }
            catch (UnauthorizedAccessException) { }
            catch (IOException) { }
        }

        public static void Disable()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RunKeyPath, true))
                {
                    if (key == null) return;
                    if (key.GetValue(ValueName) != null) key.DeleteValue(ValueName, false);
                }
            }
            catch (SecurityException) { }
            catch (UnauthorizedAccessException) { }
            catch (IOException) { }
        }

        public static void Toggle()
        {
            if (IsEnabled()) Disable();
            else Enable();
        }
    }
}
