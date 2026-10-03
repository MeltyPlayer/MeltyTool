using System.Reflection;

using fin.io.bundles;

namespace uni.games;

/// <summary>
///   Specifies the known disc IDs for a file bundle gatherer, so that ROMs
///   can still be found if they aren't named exactly right.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class GameIDsAttribute(params string[] gameIDs) : Attribute {
  public IReadOnlyList<string> GameIDs { get; } = gameIDs;
}

public static class GameIDsExtensions {
  /// <summary>
  ///   Gets the game IDs that this file bundle gatherer supports, or an empty
  ///   list if none were specified.
  /// </summary>
  public static IReadOnlyList<string> GetSupportedGameIDs(
      this INamedFileBundleGatherer gatherer)
    => gatherer.GetType().GetCustomAttribute<GameIDsAttribute>()?.GameIDs ??
       [];
}
