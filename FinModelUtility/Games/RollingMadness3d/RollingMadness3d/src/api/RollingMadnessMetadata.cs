using System.Globalization;
using System.Text.RegularExpressions;

namespace rollingMadness.api;

public sealed record AseAnimationMetadata(
    string Name,
    string ModelFileName,
    bool Loop,
    bool NoInterpolation,
    bool Reverse,
    string? SoundFileName);

public sealed record AseActorMetadata(
    string Name,
    IReadOnlyList<AseAnimationMetadata> Animations,
    float? Specular,
    string? CollisionType,
    string? MoveType,
    string? EnvironmentTexture,
    string? SoundFileName,
    string? TrailTexture);

public sealed record AseSidecarDirective(
    string Name,
    IReadOnlyList<string> Arguments);

public static partial class RollingMadnessMetadataParser {
  [GeneratedRegex("\"([^\"]*)\"|([^\\s]+)")]
  private static partial Regex TokenRegex_();

  public static IReadOnlyList<AseSidecarDirective> ParseDirectives(
      string text) {
    var directives = new List<AseSidecarDirective>();
    foreach (var rawLine in text.Split(['\r', '\n'],
                                       StringSplitOptions.RemoveEmptyEntries)) {
      var line = rawLine.Trim();
      if (line.Length == 0 || line.StartsWith('#')) {
        continue;
      }

      var tokens = TokenRegex_().Matches(line)
                                .Select(match => match.Groups[1].Success
                                                     ? match.Groups[1].Value
                                                     : match.Groups[2].Value)
                                .ToArray();
      if (tokens.Length > 0) {
        directives.Add(new AseSidecarDirective(tokens[0], tokens[1..]));
      }
    }
    return directives;
  }

  public static AseActorMetadata ParseActor(string actorName, string text) {
    var animations = new List<AseAnimationMetadata>();
    float? specular = null;
    string? collisionType = null;
    string? moveType = null;
    string? environmentTexture = null;
    string? soundFileName = null;
    string? trailTexture = null;

    foreach (var directive in ParseDirectives(text)) {
      var args = directive.Arguments;
      switch (directive.Name) {
        case "model" when args.Count >= 1:
          animations.Add(new AseAnimationMetadata(
              "vertex_animation", args[0], args.Contains("loop"),
              args.Contains("no_interpolation"), false, null));
          break;
        case "anim" when args.Count >= 3:
          animations.Add(new AseAnimationMetadata(
              args[0], args[1], args.Contains("loop"),
              args.Contains("no_interpolation"), args.Contains("reverse"),
              string.IsNullOrEmpty(args[2]) ? null : args[2]));
          break;
        case "specular" when args.Count >= 1:
          specular = float.Parse(args[0], CultureInfo.InvariantCulture);
          break;
        case "type_collide" when args.Count >= 1: collisionType = args[0]; break;
        case "type_move" when args.Count >= 1: moveType = args[0]; break;
        case "env" when args.Count >= 1: environmentTexture = args[0]; break;
        case "sound" when args.Count >= 1: soundFileName = args[0]; break;
        case "trail" when args.Count >= 1: trailTexture = args[0]; break;
      }
    }

    return new AseActorMetadata(actorName, animations, specular,
                                collisionType, moveType, environmentTexture,
                                soundFileName, trailTexture);
  }
}
