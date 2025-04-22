using fin.math;
using fin.schema.color;
using fin.schema.vector;

using schema.binary;
using schema.binary.attributes;

using sonicadventure.util;

namespace sonicadventure.schema.model;

public enum PolyType {
  TRIANGLES = 0,
  QUADS = 1,
  TRIANGLE_STRIP1 = 2,
  TRIANGLE_STRIP2 = 3,
}

[BinarySchema]
public partial class Mesh(uint key) : IKeyedInstance<Mesh> {
  public static Mesh New(uint key) => new(key);

  public ushort PolyTypeAndMaterialId { get; set; }

  [Skip]
  public ushort MaterialId
    => this.PolyTypeAndMaterialId.ExtractFromRight(0, 14);

  [Skip]
  public PolyType PolyType
    => (PolyType) this.PolyTypeAndMaterialId.ExtractFromRight(14, 2);

  public ushort PolyCount { get; set; }

  private uint polyOffset_;
  private uint pAttrOffset_;
  private uint polyNormalOffset_;
  private uint vertexColorsOffset_;
  private uint uvsOffset_;
  private uint unused_;

  [Skip]
  public IPoly[] Polys { get; private set; }

  [Skip]
  public Argb32[]? VertexColors { get; private set; }

  [Skip]
  public Vector2s[]? Uvs { get; private set; }

  [ReadLogic]
  private void ReadPolys_(IBinaryReader br) {
    this.Polys = br.ReadAtPointerOrNull(
        this.polyOffset_,
        key,
        () => this.PolyType switch {
            PolyType.TRIANGLES
                => (IPoly[]) br.ReadNews<TrianglesPoly>(this.PolyCount),
            PolyType.QUADS
                => br.ReadNews<QuadsPoly>(this.PolyCount),
            PolyType.TRIANGLE_STRIP1 or PolyType.TRIANGLE_STRIP2
                => br.ReadNews<TriangleStripPoly>(this.PolyCount),
        });

    var vertexCount = this.Polys switch {
        TrianglesPoly[] => 3 * this.PolyCount,
        QuadsPoly[]     => 4 * this.PolyCount,
        TriangleStripPoly[] triangleStripPolys => triangleStripPolys.Sum(
            t => t.NumStrips),
    };

    this.VertexColors = br.ReadAtPointerOrNull(
        this.vertexColorsOffset_,
        key,
        () => br.ReadNews<Argb32>(vertexCount));
    this.Uvs = br.ReadAtPointerOrNull(
        this.uvsOffset_,
        key,
        () => br.ReadNews<Vector2s>(vertexCount));
  }
}