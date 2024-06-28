using System.Numerics;

using fin.util.strings;

using schema.text;
using schema.text.reader;

namespace pmdc.schema.lvl;

public class Lvl : ITextDeserializable {
  public string? BackgroundName { get; set; }
  public bool HasRoomModel { get; set; }

  public List<Vector3> Trees { get; set; } = [];

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
        this.HasRoomModel = true;
      } else if (
          line.TryRemoveStart("parCamera.img:", out var backgroundName)) {
        this.BackgroundName = backgroundName.Trim();
      } else if (line.TryRemoveStart("objTree1(", out var treeParamsText)) {
        var treeParams
            = treeParamsText.Split(",", StringSplitOptions.TrimEntries);
        this.Trees.Add(new Vector3(float.Parse(treeParams[0]),
                                   float.Parse(treeParams[1]),
                                   float.Parse(treeParams[2])));
      }
    }
  }
}