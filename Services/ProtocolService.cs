using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace PolyLauncher.Services
{
    public class ProtocolService
    {
        private const string ProtocolName = "polytoria";
        
        public static bool IsRegisteredAsProtocolHandler()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey($@"Software\Classes\{ProtocolName}");
                return key != null;
            }
            catch
            {
                return false;
            }
        }

        public static void RegisterAsProtocolHandler(string? exePath = null)
        {
            try
            {
                exePath ??= Process.GetCurrentProcess().MainModule?.FileName;
                if (string.IsNullOrEmpty(exePath))
                    return;

                using (var key = Registry.CurrentUser.CreateSubKey($@"Software\Classes\{ProtocolName}"))
                {
                    key.SetValue("", $"URL:{ProtocolName} Protocol");
                    key.SetValue("URL Protocol", "");
                }

                using (var key = Registry.CurrentUser.CreateSubKey($@"Software\Classes\{ProtocolName}\DefaultIcon"))
                {
                    key.SetValue("", $"\"{exePath}\",0");
                }

                using (var key = Registry.CurrentUser.CreateSubKey($@"Software\Classes\{ProtocolName}\shell\open\command"))
                {
                    key.SetValue("", $"\"{exePath}\" \"%1\"");
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Failed to register protocol handler: {ex.Message}", 
                    "Registration Error", 
                    System.Windows.MessageBoxButton.OK, 
                    System.Windows.MessageBoxImage.Error);
            }
        }

        public static void UnregisterProtocolHandler()
        {
            try
            {
                Registry.CurrentUser.DeleteSubKeyTree($@"Software\Classes\{ProtocolName}", false);
            }
            catch
            {
                // Ignore errors during unregistration
            }
        }

        public static Models.LaunchArguments? ParseProtocolArguments(string[] args)
        {
            foreach (var arg in args)
            {
                if (arg.StartsWith($"{ProtocolName}://"))
                {
                    var parts = arg.Replace($"{ProtocolName}://", "").Split('/');
                    if (parts.Length >= 2)
                    {
                        return new Models.LaunchArguments
                        {
                            Type = parts[0],
                            Token = parts[1],
                            Map = parts.Length > 2 ? string.Join("/", parts.Skip(2)) : string.Empty
                        };
                    }
                }
            }
            return null;
        }
    }
}
