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
            Logger.Log($"Parsing protocol arguments. Total args: {args.Length}");
            foreach (var arg in args)
            {
                Logger.Log($"Processing argument: {arg}");
                if (arg.StartsWith($"{ProtocolName}://"))
                {
                    Logger.Log($"Found protocol match: {arg}");
                    var parts = arg.Replace($"{ProtocolName}://", "").Split('/', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 2)
                    {
                        var launchArgs = new Models.LaunchArguments
                        {
                            Type = parts[0],
                            Token = parts[1],
                            Map = parts.Length > 2 ? string.Join("/", parts.Skip(2)) : string.Empty
                        };
                        Logger.Log($"Successfully parsed launch arguments: Type={launchArgs.Type}, Token={launchArgs.Token}, Map={launchArgs.Map}");
                        return launchArgs;
                    }
                    else
                    {
                         Logger.Log($"Protocol argument has insufficient parts: {arg}", "WARNING");
                    }
                }
            }
            Logger.Log("No valid protocol argument found.");
            return null;
        }
    }
}
