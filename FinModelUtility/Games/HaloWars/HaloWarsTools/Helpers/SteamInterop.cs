using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;

using Microsoft.Win32;

namespace HaloWarsTools.Helpers {
  public class SteamInterop {
    [SupportedOSPlatform("windows")]
    // From https://github.com/pekempy/Breeze/blob/master/Models/ExeSearch.cs
    public static string GetGameInstallDirectory(string directory) {
      var steamGameDirs = new List<string>();
      string steam32 = "SOFTWARE\\VALVE\\";
      string steam64 = "SOFTWARE\\Wow6432Node\\Valve\\";
      string steam32path;
      string steam64path;
      string config32path;
      string config64path;
      RegistryKey key32 = Registry.LocalMachine.OpenSubKey(steam32);
      RegistryKey key64 = Registry.LocalMachine.OpenSubKey(steam64);

      if (key64.ToString() == null || key64.ToString() == "") {
        foreach (string k32subKey in key32.GetSubKeyNames()) {
          using (RegistryKey subKey = key32.OpenSubKey(k32subKey)) {
            steam32path = subKey.GetValue("InstallPath").ToString();
            config32path = steam32path + "/steamapps/libraryfolders.vdf";
            string driveRegex = @"[A-Z]:\\";
            if (File.Exists(config32path)) {
              string[] configLines = File.ReadAllLines(config32path);
              foreach (var item in configLines) {
                Match match = Regex.Match(item, driveRegex);
                if (item != string.Empty && match.Success) {
                  string matched = match.ToString();
                  string item2 = item.Substring(item.IndexOf(matched));
                  item2 = item2.Replace("\\\\", "\\");
                  item2 = item2.Replace("\"", "\\");
                  if (Directory.Exists(item2 + "\\steamapps\\common")) {
                    item2 = item2 + "steamapps\\common\\";
                    steamGameDirs.Add(item2);
                  }
                }
              }
              steamGameDirs.Add(steam32path + "\\steamapps\\common\\");
            }
          }
        }
      }

      foreach (string k64subKey in key64.GetSubKeyNames()) {
        using (RegistryKey subKey = key64.OpenSubKey(k64subKey)) {
          steam64path = subKey.GetValue("InstallPath")?.ToString();
          if (steam64path == null) {
            continue;
          }
          config64path = steam64path + "/steamapps/libraryfolders.vdf";
          string driveRegex = @"[A-Z]:\\";
          if (File.Exists(config64path)) {
            string[] configLines = File.ReadAllLines(config64path);
            foreach (var item in configLines) {
              Match match = Regex.Match(item, driveRegex);
              if (item != string.Empty && match.Success) {
                string matched = match.ToString();
                string item2 = item.Substring(item.IndexOf(matched));
                item2 = item2.Replace("\\\\", "\\");
                item2 = item2.Replace("\"", "\\");
                if (Directory.Exists(item2 + "steamapps\\common")) {
                  item2 = item2 + "steamapps\\common\\";
                  steamGameDirs.Add(item2);
                }
              }
            }
            steamGameDirs.Add(steam64path + "\\steamapps\\common\\");
          }
        }
      }

      foreach (var dir in steamGameDirs.GroupBy(dir => dir).Select(group => group.First())) {
        var directories = Directory.GetDirectories(dir).Select(dir => Path.GetFileName(dir));

        if (directories.Contains(directory)) {
          return Path.Combine(dir, directory);
        }
      }

      return null;
    }
  }
}