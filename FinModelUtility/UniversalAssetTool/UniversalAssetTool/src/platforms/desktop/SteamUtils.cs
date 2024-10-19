using fin.io;
using fin.util.enumerables;
using fin.util.linq;

using Gameloop.Vdf;
using Gameloop.Vdf.Linq;

namespace uni.platforms.desktop;

internal static class SteamUtils {
  private static string? InstallPath_ { get; } =
    RegistryExtensions.GetSoftwareValueEither32Or64Bit(
        @"Valve\Steam",
        "InstallPath") as string;

  private static IReadOnlySystemDirectory? InstallDirectory_ { get; } =
    InstallPath_ != null
        ? new FinDirectory(InstallPath_)
        : null;

  private static IReadOnlySystemFile? LibraryFoldersVdf_ { get; } =
    (InstallDirectory_?.TryToGetExistingFile(
         "config/libraryfolders.vdf",
         out var libraryFoldersVdf) ??
     false)
        ? libraryFoldersVdf
        : null;

  private static ISystemDirectory[] Libraries_ { get; } =
    InitializeLibraries_();

  private static ISystemDirectory[] InitializeLibraries_() {
    if (!(LibraryFoldersVdf_?.Exists ?? false)) {
      return [];
    }

    using var ls = LibraryFoldersVdf_.OpenReadAsText();
    var root = VdfConvert.Deserialize(ls);

    if (root.Key != "libraryfolders") {
      return [];
    }

    return root.Value.Children()
               .Select(section => {
                 try {
                   return section.Value<VProperty>()
                                 .Value["path"]
                                 ?.ToString();
                 } catch {
                   return null;
                 }
               })
               .Nonnull()
               .Select(path => new FinDirectory(path))
               .CastTo<FinDirectory, ISystemDirectory>()
               // A steam library directory may not exist if it lives on an
               // external hard drive
               .Where(steamDirectory => steamDirectory.Exists)
               .ToArray();
  }

  private static ISystemDirectory[] CommonDirectories_ { get; } =
    Libraries_
        .SelectMany(libraryFolder
                        => libraryFolder.GetExistingSubdirs()
                                        .ByName("steamapps"))
        .SelectMany(
            steamApps => steamApps.GetExistingSubdirs().ByName("common"))
        .ToArray();

  public static ISystemDirectory[] GameDirectories { get; }
    = CommonDirectories_
      .SelectMany(common => common.GetExistingSubdirs())
      .ToArray();

  public static bool TryGetGameDirectory(string name,
                                         out ISystemDirectory directory)
    => GameDirectories.ByName(name).TryGetFirst(out directory);
}