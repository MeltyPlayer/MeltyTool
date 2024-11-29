using fin.io;
using fin.util.asserts;

namespace uni.platforms;

public static class DirectoryConstants {
  public static ISystemDirectory BASE_DIRECTORY { get; } =
    GetBaseDirectory_();

  private static ISystemDirectory GetBaseDirectory_() {
    // Launched externally
    var exeDirectory = new FinDirectory(AppContext.BaseDirectory);
    if (exeDirectory.Name is "universal_asset_tool") {
      return exeDirectory.AssertGetParent()  // tools
                         .AssertGetParent()  // cli
                         .AssertGetParent(); // FinModelUtility
    }

    // Launched via Visual Studio
    return
        Asserts
            .CastNonnull(Files.GetCwd().GetAncestry())
            .Where(ancestor => {
              var subdirNames = ancestor
                                .GetExistingSubdirs()
                                .Select(directory => directory.Name.ToString());
              return subdirNames.Contains("cli") &&
                     subdirNames.Contains("FinModelUtility");
            })
            .Single();
  }

  public static ISystemDirectory CLI_DIRECTORY =
      BASE_DIRECTORY.AssertGetExistingSubdir("cli");


  public static ISystemDirectory GAME_CONFIG_DIRECTORY { get; } =
    CLI_DIRECTORY.AssertGetExistingSubdir("config");

  public static ISystemFile CONFIG_FILE { get; } =
    CLI_DIRECTORY.AssertGetExistingFile("config.json");


  public static ISystemDirectory ROMS_DIRECTORY =
      CLI_DIRECTORY.AssertGetExistingSubdir("roms");

  public static ISystemDirectory TOOLS_DIRECTORY =
      CLI_DIRECTORY.AssertGetExistingSubdir("tools");

  public static ISystemDirectory OUT_DIRECTORY =
      CLI_DIRECTORY.AssertGetExistingSubdir("out");
}