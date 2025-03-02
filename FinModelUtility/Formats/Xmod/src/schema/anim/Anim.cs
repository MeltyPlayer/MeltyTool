using System.Numerics;

using fin.math.rotations;

using schema.binary;


namespace xmod.schema.anim;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/Dummiesman/AngelStudiosBlenderAddon/blob/cac6dbc59cbc0bae7afd99f49043538548d94396/io_scene_angelstudios/import_anim.py
/// </summary>
public class Anim : IBinaryDeserializable {
  public int FrameCount { get; set; }

  public IReadOnlyList<Vector3> RootPositions { get; set; }
  public IReadOnlyList<IReadOnlyList<Quaternion>> BoneEulerRotations { get; set; }

  public void Read(IBinaryReader br) {
    var isOldAnimation = true;
    this.FrameCount = br.ReadInt32();
    if (this.FrameCount == 0) {
      this.FrameCount = br.ReadInt32();
      isOldAnimation = false;
    }

    var boneCount = isOldAnimation ? br.ReadInt32() : br.ReadInt32() / 3 - 1;

    var unk = br.ReadBytes(5);

    var rootPositions = new LinkedList<Vector3>();
    var boneEulerRotations = Enumerable.Range(0, boneCount)
                                       .Select(_ => new LinkedList<Quaternion>())
                                       .ToArray();

    for (var i = 0; i < this.FrameCount; ++i) {
      rootPositions.AddLast(br.ReadVector3());

      for (var b = 0; b < boneCount; ++b) {
        var r = br.ReadSingle();
        var p = br.ReadSingle();
        var h = br.ReadSingle();

        var x = h;
        var y = r;
        var z = p;

        boneEulerRotations[b].AddLast(QuaternionUtil.CreateXyz(x, y, z));
      }
    }

    this.RootPositions = rootPositions.ToArray();
    this.BoneEulerRotations
        = boneEulerRotations.Select(b => b.ToArray()).ToArray();
  }
}