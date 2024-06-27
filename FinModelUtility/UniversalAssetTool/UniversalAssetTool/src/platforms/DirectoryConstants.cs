using fin.io;
using fin.util.asserts;

namespace uni.platforms {
  public static class DirectoryConstants {
    public static ISystemDirectory BASE_DIRECTORY { get; } =
      DirectoryConstants.GetBaseDirectory_();

    private static ISystemDirectory GetBaseDirectory_() {
      var cwd = Files.GetCwd();
      if (cwd.Name == "FinModelUtility") {
        return cwd;
      }

      if (cwd.Name == "cli") {
        return cwd.AssertGetParent();
      }

      if (cwd.Name == "universal_model_extractor") {
        return cwd.AssertGetParent() // tools
                  .AssertGetParent() // cli
                  .AssertGetParent(); // FinModelUtility
      }

      return
          Asserts
              .CastNonnull(Files.GetCwd().GetAncestry())
              .Where(ancestor => {
                var subdirNames = ancestor
                                  .GetExistingSubdirs()
                                  .Select(
                                      directory
                                          => directory.Name);
                return subdirNames.Contains("cli") &&
                       subdirNames.Contains("FinModelUtility");
              })
              .Single();
    }

    public static ISystemDirectory CLI_DIRECTORY =
        DirectoryConstants.BASE_DIRECTORY.AssertGetExistingSubdir("cli");


    public static ISystemDirectory GAME_CONFIG_DIRECTORY { get; } =
      CLI_DIRECTORY.AssertGetExistingSubdir("config");

    public static ISystemFile CONFIG_FILE { get; } =
      DirectoryConstants.CLI_DIRECTORY.AssertGetExistingFile("config.json");


    public static ISystemDirectory ROMS_DIRECTORY =
        DirectoryConstants.CLI_DIRECTORY.AssertGetExistingSubdir("roms");

    public static ISystemDirectory TOOLS_DIRECTORY =
        DirectoryConstants.CLI_DIRECTORY.AssertGetExistingSubdir("tools");

    public static ISystemDirectory OUT_DIRECTORY =
        DirectoryConstants.CLI_DIRECTORY.AssertGetExistingSubdir("out");
  }
}