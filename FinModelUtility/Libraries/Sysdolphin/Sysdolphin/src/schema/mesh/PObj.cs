using CommunityToolkit.HighPerformance;

using fin.color;
using fin.model;
using fin.util.asserts;
using fin.util.enumerables;
using fin.util.enums;

using gx;
using gx.displayList;

using schema.binary;

using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;

namespace sysdolphin.schema.mesh;

[Flags]
public enum PObjFlags : ushort {
  BIT_1 = (1 << 0),
  BIT_2 = (1 << 1),
  UNKNOWN2 = (1 << 2),
  ANIM = (1 << 3),

  BIT_5 = (1 << 4),
  BIT_6 = (1 << 5),
  BIT_7 = (1 << 6),
  BIT_8 = (1 << 7),
  BIT_9 = (1 << 8),
  BIT_10 = (1 << 9),
  BIT_11 = (1 << 10),
  BIT_12 = (1 << 11),

  OBJTYPE_SHAPEANIM = 1 << 12,
  OBJTYPE_ENVELOPE = 1 << 13,

  CULLBACK = (1 << 14),
  CULLFRONT = (1 << 15)
}

/// <summary>
///   Polygon object.
/// </summary>
public partial class PObj : IDatLinkedListNode<PObj>, IBinaryDeserializable {
  [BinarySchema]
  public partial class PObjHeader : IBinaryDeserializable {
    public uint StringOffset { get; set; }
    public uint NextPObjOffset { get; set; }
    public uint VertexDescriptorListOffset { get; set; }
    public PObjFlags Flags { get; set; }
    public ushort DisplayListSize { get; set; }
    public uint DisplayListOffset { get; set; }
    public uint WeightListOrShapeAnimOffset { get; set; }
  }

  public PObjHeader Header { get; } = new();
  public PObj? NextSibling { get; private set; }

  public VertexDescriptors VertexDescriptors { get; } = new();
  public List<DatPrimitive> Primitives { get; } = [];

  public VertexSpace VertexSpace { get; private set; }
  public List<IList<PObjWeight>>? Weights { get; private set; }

  public void Read(IBinaryReader br) {
    this.Header.Read(br);

    // TODO: Read weights
    // https://github.com/jam1garner/Smash-Forge/blob/c0075bca364366bbea2d3803f5aeae45a4168640/Smash%20Forge/Filetypes/Melee/DAT.cs#L1515C21-L1515C38

    if (this.Header.VertexDescriptorListOffset != 0) {
      br.Position = this.Header.VertexDescriptorListOffset;
      this.VertexDescriptors.Read(br);
    }

    this.ReadDisplayList_(br);

    if (this.Header.NextPObjOffset != 0) {
      br.Position = this.Header.NextPObjOffset;

      this.NextSibling = new PObj();
      this.NextSibling.Read(br);
    }
  }

  private void ReadDisplayList_(IBinaryReader br) {
    if (this.Header.DisplayListOffset == 0 ||
        this.Header.DisplayListSize == 0) {
      return;
    }

    this.VertexSpace = VertexSpace.RELATIVE_TO_BONE;

    var flags = this.Header.Flags;
    var hasEnvelope = flags.CheckFlag(PObjFlags.OBJTYPE_ENVELOPE);
    var hasShapeAnim = flags.CheckFlag(PObjFlags.OBJTYPE_SHAPEANIM);
    var hasUnknown2 = flags.CheckFlag(PObjFlags.UNKNOWN2);

    var weightListOrShapeAnimOffset = this.Header.WeightListOrShapeAnimOffset;
    if (weightListOrShapeAnimOffset != 0) {
      br.Position = this.Header.WeightListOrShapeAnimOffset;

      var pObjWeights = this.Weights = [];
      if (hasEnvelope) {
        int offset = 0;
        while ((offset = br.ReadInt32()) != 0) {
          br.SubreadAt(
              offset,
              () => {
                var weights = new List<PObjWeight>();

                uint jObjOffset;
                while ((jObjOffset = br.ReadUInt32()) != 0) {
                  var weight = br.ReadSingle();
                  weights.Add(new PObjWeight {
                      JObjOffset = jObjOffset,
                      Weight = weight,
                  });
                }

                pObjWeights.Add(weights);
              });
        }
      } else if (hasShapeAnim) {
        // TODO: Support this
      } else if (!hasUnknown2) {
        var currentJObjOffset = weightListOrShapeAnimOffset;
        pObjWeights.Add(new PObjWeight {
            JObjOffset = currentJObjOffset,
            Weight = 1,
        }.AsList());
      }
    }

    // Reads display list
    var gxDisplayListReader = new GxDisplayListReader();
    var positionAttr = this.VertexDescriptors[GxVertexAttribute.Position];
    var normalAttr = this.VertexDescriptors[GxVertexAttribute.Normal];
    var nbtAttr = this.VertexDescriptors[GxVertexAttribute.NBT];
    var uv0Attr = this.VertexDescriptors[GxVertexAttribute.Tex0Coord];
    var uv1Attr = this.VertexDescriptors[GxVertexAttribute.Tex1Coord];
    var color0Attr = this.VertexDescriptors[GxVertexAttribute.Color0];

    br.Position = this.Header.DisplayListOffset;
    for (var d = 0; d < this.Header.DisplayListSize; ++d) {
      var opcode
          = gxDisplayListReader.ReadOpcode(br,
                                           this.VertexDescriptors,
                                           out var gxPrimitive);

      // TODO: Is this correct, or should it just skip this instead?
      if (opcode == GxOpcode.NOP) {
        break;
      }

      if (gxPrimitive == null) {
        continue;
      }

      var datVertices
          = gxPrimitive
            .Vertices
            .Select(v => {
              var position = br.ReadVector3(v.PositionIndex, positionAttr!);
              var datVertex = new DatVertex {
                  Position = position,
                  WeightId = v.JointIndex
              };

              var normalIndex = v.NormalIndex;
              if (normalIndex != null) {
                datVertex.Normal = Vector3.Normalize(
                    br.ReadVector3(normalIndex.Value, normalAttr!));
              }

              var nbtIndex = v.NbtIndex;
              if (nbtIndex != null) {
                br.SubreadAt(
                    nbtAttr!.GetOffset(nbtIndex.Value),
                    () => {
                      datVertex.Normal
                          = Vector3.Normalize(br.ReadVector3(nbtAttr!));
                      datVertex.Binormal
                          = Vector3.Normalize(br.ReadVector3(nbtAttr!));
                      datVertex.Tangent
                          = Vector3.Normalize(br.ReadVector3(nbtAttr!));
                    });
              }

              var uv0Index = v.TexCoord0Index;
              if (uv0Index != null) {
                datVertex.Uv0 = br.ReadVector2(uv0Index.Value, uv0Attr!);
              }

              var uv1Index = v.TexCoord1Index;
              if (uv1Index != null) {
                datVertex.Uv1 = br.ReadVector2(uv1Index.Value, uv1Attr!);
              }

              var color0IndexOrValue = v.Color0IndexOrValue;
              color0IndexOrValue?
                  .Switch(index => {
                            var offset = color0Attr!.GetOffset(index);
                            var originalPos = br.Position;
                            br.Position = offset;
                            datVertex.Color = GxAttributeUtil.ReadColor(
                                br,
                                color0Attr!.ColorComponentType);
                            br.Position = originalPos;
                          },
                          color => datVertex.Color = color);

              return datVertex;
            })
            .ToArray();

      this.Primitives.Add(new DatPrimitive {
          Type = gxPrimitive.PrimitiveType,
          Vertices = datVertices
      });
    }
  }
}

public static class BinaryReaderExtensions {
  public static Vector2 ReadVector2(this IBinaryReader br,
                                    uint index,
                                    VertexDescriptor descriptor) {
    var vec2 = new Vector2();
    br.ReadIntoVector(descriptor,
                      index,
                      new Span<Vector2>(ref vec2).Cast<Vector2, float>());
    return vec2;
  }

  public static Vector3 ReadVector3(this IBinaryReader br,
                                    uint index,
                                    VertexDescriptor descriptor) {
    var vec3 = new Vector3();
    br.ReadIntoVector(descriptor,
                      index,
                      new Span<Vector3>(ref vec3).Cast<Vector3, float>());
    return vec3;
  }

  public static Vector3 ReadVector3(this IBinaryReader br,
                                    VertexDescriptor descriptor) {
    var vec3 = new Vector3();
    br.ReadIntoVector(descriptor,
                      new Span<Vector3>(ref vec3).Cast<Vector3, float>());
    return vec3;
  }

  public static void ReadIntoVector(this IBinaryReader br,
                                    VertexDescriptor descriptor,
                                    uint index,
                                    Span<float> floats) {
    Asserts.True(floats.Length >= descriptor.ComponentCount);
    var offset = descriptor.GetOffset(index);
    var originalPos = br.Position;
    br.Position = offset;
    br.ReadIntoVector(descriptor, floats);
    br.Position = originalPos;
  }

  public static void ReadIntoVector(this IBinaryReader br,
                                    VertexDescriptor descriptor,
                                    Span<float> floats) {
    Asserts.True(floats.Length >= descriptor.ComponentCount);
    var scaleMultiplier = 1f / MathF.Pow(2, descriptor.Scale);
    for (var i = 0; i < descriptor.ComponentCount; ++i) {
      floats[i] = scaleMultiplier *
                  GxAttributeUtil.ReadValue(br, descriptor.AxesComponentType);
    }
  }

  public static uint GetOffset(this VertexDescriptor descriptor, uint index)
    => descriptor.ArrayOffset + descriptor.Stride * index;
}

public class DatPrimitive {
  public required GxPrimitiveType Type { get; init; }
  public required IReadOnlyList<DatVertex> Vertices { get; init; }
}

public class DatVertex {
  public required Vector3 Position { get; set; }
  public int? WeightId { get; set; }
  public Vector3? Normal { get; set; }
  public Vector3? Binormal { get; set; }
  public Vector3? Tangent { get; set; }
  public Vector2? Uv0 { get; set; }
  public Vector2? Uv1 { get; set; }
  public IColor? Color { get; set; }
}

public class PObjWeight {
  public required uint JObjOffset { get; init; }
  public required float Weight { get; init; }
}