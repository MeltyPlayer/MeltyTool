using System.IO;

using fin.io;
using fin.util.asserts;

namespace uni.platforms.desktop {
  public static class EaUtils {
    public static bool TryGetGameDirectory(
        string name,
        out ISystemDirectory directory,
        bool assert = false) {
      using (var eaGamesKey =
             RegistryExtensions.OpenSoftwareSubkeyEither32Or64Bit("EA Games")) {
        if (eaGamesKey != null) {
          foreach (var gameKeyName in eaGamesKey.GetSubKeyNames()) {
            if (gameKeyName != name) {
              continue;
            }

            using var gameKey = eaGamesKey.OpenSubKey(gameKeyName);
            if (gameKey.GetValue("Install Dir") is string gameInstallPath &&
                Directory.Exists(gameInstallPath)) {
              directory = new FinDirectory(gameInstallPath);
              return true;
            }
          }
        }
      }

      Asserts.False(assert, $"Could not find \"{name}\" installed for EA.");
      directory = null;
      return false;
    }
  }
}
