using fin.io;
using fin.util.asserts;

namespace uni.platforms {
  public static class DirectoryConstants {
    public static IDirectory BASE_DIRECTORY { get; } =
      DirectoryConstants.GetBaseDirectory_();

    private static IDirectory GetBaseDirectory_() {
      var cwd = Files.GetCwd();
      if (cwd.Name == "FinModelUtility") {
        return cwd;
      }

      if (cwd.Name == "cli") {
        return cwd.GetParent();
      }

      if (cwd.Name == "universal_model_extractor") {
        return cwd.GetParent() // tools
                  .GetParent() // cli
                  .GetParent(); // FinModelUtility
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

    public static IDirectory CLI_DIRECTORY =
        DirectoryConstants.BASE_DIRECTORY.GetSubdir("cli");


    public static IDirectory GAME_CONFIG_DIRECTORY { get; } =
      CLI_DIRECTORY.GetSubdir("config");

    public static IFile CONFIG_FILE { get; } =
      DirectoryConstants.CLI_DIRECTORY.GetExistingFile("config.json");


    public static IDirectory ROMS_DIRECTORY =
        DirectoryConstants.CLI_DIRECTORY.GetSubdir("roms");

    public static IDirectory TOOLS_DIRECTORY =
        DirectoryConstants.CLI_DIRECTORY.GetSubdir("tools");

    public static IDirectory OUT_DIRECTORY =
        DirectoryConstants.CLI_DIRECTORY.GetSubdir("out");
  }
}