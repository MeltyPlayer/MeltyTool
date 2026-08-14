using System.Numerics;

using fin.schema;
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

public sealed class Lvl : ITextDeserializable {
  public string? BackgroundName { get; set; }
  public bool HasRoomModel { get; set; }
  public float RoomScale { get; set; } = 1;

  public List<(Vector3, string characterType)> Enemies { get; set; } = [];

  public List<(Vector3 start, Vector3 end, string? textureName, FloorBlockType
      type, FloorBlockFlags flags)> FloorBlocks { get; set; } = [];

  public List<(Vector3, string name, string characterType, string text)> Npcs {
    get;
    set;
  } = [];

  public List<Vector3> SaveBlocks { get; set; } = [];
  public List<Vector3> Trees { get; set; } = [];

  public void Read(ITextReader tr) {
    this.HasRoomModel = false;
    this.RoomScale = 5;
    this.BackgroundName = null;
    this.Trees.Clear();

    while (!tr.Eof) {
      tr.SkipCommentsAndWhitespace();

      if (tr.Matches("global.roomIsModel:")) {
        var roomIsModelValue = tr.ReadLine();
        this.HasRoomModel = CoerceStringToBool_(roomIsModelValue);
      } else if (tr.Matches("global.roomScale:")) {
        var roomScaleValue = tr.ReadLine();
        this.RoomScale = float.Parse(roomScaleValue);
      } else if (tr.Matches("parCamera.img:")) {
        var backgroundName = tr.ReadLine();
        this.BackgroundName = backgroundName.Trim();
      } else if (TryToParseObj(tr, out var objType, out var objParams)) {
        switch (objType) {
          case "objEnemy": {
            var position = ParseVector3(objParams);

            using var lastParamTr
                = new SchemaTextReader(objParams[7].Replace("\"", ""));
            var lastParamArgs = lastParamTr.ReadArguments([','], []);

            var characterType = lastParamArgs[1];

            this.Enemies.Add((position, characterType));
            break;
          }
          case "objFloorBlock": {
            var start = ParseVector3(objParams);
            var end = ParseVector3(objParams.AsSpan(3));
            var textureName = objParams[6] == "-1"
                ? null
                : objParams[6];

            var behavior = objParams[7].Replace(@"""", "");
            var type = GetFloorBlockType(behavior);
            var flags = GetFloorBlockFlags(behavior);

            this.FloorBlocks.Add((start, end, textureName, type, flags));
            break;
          }
          case "objNPC": {
            var position = ParseVector3(objParams);

            using var lastParamTr
                = new SchemaTextReader(objParams[7].Replace("\"", ""));

            var name = lastParamTr.ReadUpToStartOfTerminator(',');
            lastParamTr.ReadChar();
            var characterType = lastParamTr.ReadUpToStartOfTerminator(',');
            lastParamTr.ReadChar();
            characterType = characterType.SubstringUpTo('-');
            var text = lastParamTr.ReadRemainder();

            this.Npcs.Add((position, name, characterType, text));
            break;
          }
          case "objSaveBlock": {
            this.SaveBlocks.Add(ParseVector3(objParams));
            break;
          }
          case "objTree1": {
            this.Trees.Add(ParseVector3(objParams));
            break;
          }
        }
      } else {
        tr.SkipToEndOfLine();
      }
    }
  }

  public static bool TryToParseObj(ITextReader tr,
                                   out string type,
                                   out string[] prms) {
    type = null!;
    prms = null!;

    if (!tr.Matches("obj")) {
      return false;
    }

    type = $"obj{tr.ReadUpToStartOfTerminator('(')}";
    tr.ReadChar();

    prms = tr.ReadArguments([','], [')']);
    return true;
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

  public static Vector3 ParseVector3(ReadOnlySpan<string> span)
    => new(float.Parse(span[0]), float.Parse(span[1]), float.Parse(span[2]));
}