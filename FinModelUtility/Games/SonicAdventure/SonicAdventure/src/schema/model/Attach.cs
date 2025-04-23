using System.Numerics;

using schema.binary;
using schema.binary.attributes;

using sonicadventure.util;

namespace sonicadventure.schema.model;

/// <summary>
///   Shamelessly stolen from:
///   https://info.sonicretro.org/SCHG:Sonic_Adventure/Model_Format
/// </summary>
[BinarySchema]
public partial class Attach(uint keyedPointer, uint key)
    : IKeyedInstance<Attach> {
  public static Attach New(uint keyedPointer, uint key)
    => new(keyedPointer, key);

  private uint verticesPointer_;
  private uint normalsPointer_;
  private uint vertexNormalTotal_;
  private uint meshesPointer_;
  private uint materialsPointer_;
  private ushort meshTotal_;
  private ushort materialTotal_;
  public Vector3 Center { get; set; }
  public float Radius { get; set; }
  private uint null_;

  [Skip]
  public Vector3[] Vertices { get; set; }

  [Skip]
  public Vector3[]? Normals { get; set; }

  [Skip]
  public Mesh[] Meshes { get; set; }

  [Skip]
  public Material[] Materials { get; set; }

  [ReadLogic]
  private void ReadObjects_(IBinaryReader br) {
    this.Vertices = br.ReadAtPointerOrNull(
        this.verticesPointer_,
        key,
        () => br.ReadVector3s(this.vertexNormalTotal_));
    this.Normals = br.ReadAtPointerOrNull(
        this.normalsPointer_,
        key,
        () => br.ReadVector3s(this.vertexNormalTotal_));

    this.Meshes = br.ReadAtPointerOrNull(
        this.meshesPointer_,
        key,
        () => {
          var meshes = new Mesh[this.meshTotal_];
          for (var i = 0; i < meshes.Length; ++i) {
            var mesh = new Mesh(key);
            mesh.Read(br);
            meshes[i] = mesh;
          }

          return meshes;
        });
    this.Materials = br.ReadAtPointerOrNull(
        this.materialsPointer_,
        key,
        () => br.ReadNews<Material>(this.materialTotal_));
  }
}