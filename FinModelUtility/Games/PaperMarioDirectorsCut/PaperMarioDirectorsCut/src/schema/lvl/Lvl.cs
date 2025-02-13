using System.Numerics;

using fin.util.strings;

using schema.text;
using schema.text.reader;

namespace pmdc.schema.lvl;

public enum FloorBlockType {
  WALL,
  FLOOR,
  STEP,
  ELEVATOR,
}

[Flags]
public enum FloorBlockFlags {
  INVISIBLE = 1 << 0,
  NO_COLLIDE = 1 << 1,
  NO_REPEAT = 1 << 2,
}

public class Lvl : ITextDeserializable {
  public string? BackgroundName { get; set; }
  public bool HasRoomModel { get; set; }

  public List<Vector3> Trees { get; set; } = [];

  public List<(Vector3 start, Vector3 end, string? textureName, FloorBlockType
      type, FloorBlockFlags flags)> FloorBlocks { get; set; } = [];

  public void Read(ITextReader tr) {
    this.HasRoomModel = false;
    this.BackgroundName = null;
    this.Trees.Clear();

    while (!tr.Eof) {
      var line = tr.ReadLine().Trim();

      if (line.Length == 0 || line.StartsWith("//")) {
        continue;
      }

      if (line.TryRemoveStart("global.roomIsModel:",
                              out var roomIsModelValue)) {
        this.HasRoomModel = CoerceStringToBool_(roomIsModelValue);
      } else if (
          line.TryRemoveStart("parCamera.img:", out var backgroundName)) {
        this.BackgroundName = backgroundName.Trim();
      } else if (line.TryRemoveStart("objTree1(", out var treeParamsText)) {
        var treeParams
            = treeParamsText.Split(",", StringSplitOptions.TrimEntries);
        this.Trees.Add(new Vector3(float.Parse(treeParams[0]),
                                   float.Parse(treeParams[1]),
                                   float.Parse(treeParams[2])));
      } else if (line.TryRemoveStart("objFloorBlock(",
                                     out var floorBlockParamsText)) {
        var floorBlockParams
            = floorBlockParamsText.SubstringUpTo(')')
                                  .Split(',', StringSplitOptions.TrimEntries);
        var start = new Vector3(float.Parse(floorBlockParams[0]),
                                float.Parse(floorBlockParams[1]),
                                float.Parse(floorBlockParams[2]));
        var end = new Vector3(float.Parse(floorBlockParams[3]),
                              float.Parse(floorBlockParams[4]),
                              float.Parse(floorBlockParams[5]));
        var textureName
            = floorBlockParams[6] == "-1" ? null : floorBlockParams[6];

        var behavior = floorBlockParams[7].Replace(@"""", "");
        var type = GetFloorBlockType(behavior);
        var flags = GetFloorBlockFlags(behavior);

        this.FloorBlocks.Add((start, end, textureName, type, flags));
      }
    }
  }

  public static FloorBlockType GetFloorBlockType(string behavior) {
    if (behavior.StartsWith("wall")) {
      return FloorBlockType.WALL;
    }

    if (behavior.StartsWith("floor")) {
      return FloorBlockType.FLOOR;
    }

    if (behavior.StartsWith("step")) {
      return FloorBlockType.STEP;
    }

    if (behavior.StartsWith("elevator")) {
      return FloorBlockType.ELEVATOR;
    }

    throw new NotSupportedException();
  }

  public static FloorBlockFlags GetFloorBlockFlags(string behavior) {
    FloorBlockFlags flags = default;

    if (behavior.Contains("-invisible")) {
      flags |= FloorBlockFlags.INVISIBLE;
    }

    if (behavior.Contains("-noCollide")) {
      flags |= FloorBlockFlags.NO_COLLIDE;
    }

    if (behavior.Contains("-noRepeat")) {
      flags |= FloorBlockFlags.NO_REPEAT;
    }

    return flags;
  }

  private static bool CoerceStringToBool_(string text) => text switch {
      "0"     => false,
      "1"     => true,
      "false" => false,
      "true"  => true,
      _       => throw new ArgumentOutOfRangeException(nameof(text), text, null)
  };
}