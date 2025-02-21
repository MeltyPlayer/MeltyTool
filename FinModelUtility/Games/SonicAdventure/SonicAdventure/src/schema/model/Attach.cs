using System.Numerics;

using schema.binary;
using schema.binary.attributes;

namespace sonicadventure.schema.model;

[BinarySchema]
public partial class Attach : IBinaryDeserializable {
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

  [RAtPosition(nameof(verticesPointer_))]
  [RSequenceLengthSource(nameof(vertexNormalTotal_))]
  public Vector3[] Vertices { get; set; }

  [RAtPosition(nameof(verticesPointer_))]
  [RSequenceLengthSource(nameof(vertexNormalTotal_))]
  public Vector3[] Normals { get; set; }

  [RAtPosition(nameof(meshesPointer_))]
  [RSequenceLengthSource(nameof(meshTotal_))]
  public Mesh[] Meshes { get; set; }

  [RAtPosition(nameof(materialsPointer_))]
  [RSequenceLengthSource(nameof(materialTotal_))]
  public Material[] Materials { get; set; }
}