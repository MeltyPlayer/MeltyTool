using System;
using System.IO;
using System.Linq;

using fin.io;
using fin.util.asserts;

namespace fin.common;

public static class DirectoryConstants {
  public static ISystemDirectory BASE_DIRECTORY { get; } =
    GetBaseDirectory_();

  private static ISystemDirectory GetBaseDirectory_() {
    // Launched externally
    var exeDirectory = new FinDirectory(AppContext.BaseDirectory);
    if (exeDirectory.Name is "universal_asset_tool") {
      return exeDirectory.AssertGetParent()  // tools
                         .AssertGetParent(); // cli
    }

    foreach (var ancestor in Files.GetCwd().GetAncestry()) {
      var subdirByName = ancestor
                         .GetExistingSubdirs()
                         .ToDictionary(d => d.Name.ToString(), d => d);

      // Launched via Visual Studio
      if (subdirByName.ContainsKey("cli") &&
          subdirByName.ContainsKey("FinModelUtility")) {
        return subdirByName["cli"];
      }

      if (subdirByName.ContainsKey("roms") &&
          subdirByName.ContainsKey("tools")) {
        return ancestor;
      }
    }

    throw new DirectoryNotFoundException("Failed to find the base directory.");
  }

  public static ISystemDirectory GAME_CONFIG_DIRECTORY { get; } =
    BASE_DIRECTORY.AssertGetExistingSubdir("config");

  public static ISystemFile CONFIG_FILE { get; } =
    BASE_DIRECTORY.AssertGetExistingFile("config.json");


  public static ISystemDirectory ROMS_DIRECTORY =
      BASE_DIRECTORY.AssertGetExistingSubdir("roms");

  public static ISystemDirectory TOOLS_DIRECTORY =
      BASE_DIRECTORY.AssertGetExistingSubdir("tools");

  public static ISystemDirectory DLL_DIRECTORY =
      TOOLS_DIRECTORY.AssertGetExistingSubdir("dll");

  public static ISystemDirectory OUT_DIRECTORY =
      BASE_DIRECTORY.AssertGetExistingSubdir("out");
}