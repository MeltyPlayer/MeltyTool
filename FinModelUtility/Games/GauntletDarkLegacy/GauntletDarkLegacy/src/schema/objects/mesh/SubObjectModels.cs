using System.Numerics;

using CommunityToolkit.Diagnostics;

using fin.color;
using fin.util.asserts;

using schema.binary;
using schema.binary.attributes;

namespace gdl.schema.objects.mesh;

public class Mesh {
  public required SubObject SubObject { get; init; }
  public required IReadOnlyList<Primitive> Primitives { get; init; }
}

public class Primitive {
  public required IReadOnlyList<Vector3> Positions { get; init; }
  public required IReadOnlyList<Uv> Uvs { get; init; }
  public required IReadOnlyList<Vector3> Normals { get; init; }
  public required IReadOnlyList<bool> FacesDrawn { get; init; }
  public required IReadOnlyList<IColor> VertexColors { get; init; }
  public required float FaceDir { get; init; }
}

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/MosesofEgypt/gdl_tools/blob/main/gdl/compilation/g3d/serialization/model_vif.py#L92
///   https://github.com/haekb/gdl-tools/blob/master/Addons/GDLFormat/Models/Objects.gd#L527
/// </summary>
public sealed class SubObjectModels : IBinaryDeserializable, IChildOf<Object> {
  public Object Parent { get; set; }

  public List<Mesh> All { get; set; } = new();

  public void Read(IBinaryReader br) {
    this.All.Clear();

    var signals = new LinkedList<Signal?>();

    foreach (var subObj in this.Parent.SubObjects) {
      var unpackCommand = br.ReadUInt32();

      var qwc = subObj.Qwc;

      var unpackSize = qwc * 16;

      var endPosition = br.Position + unpackSize;

      var primitives = new List<Primitive>();
      var positions = new List<Vector3>();
      var uvs = new List<Uv>();
      var normals = new List<Vector3>();
      // It's so annoying we have to track this, but the models straight-up do
      // not look right without this.
      var facesDrawn = new List<bool>();
      var vertexColors = new List<IColor>();
      var faceDir = 1f;

      var mesh = new Mesh {
          SubObject = subObj,
          Primitives = primitives
      };
      this.All.Add(mesh);

      void AddCurrentPrimitiveToMesh() {
        if (positions.Count == 0) {
          return;
        }

        primitives.Add(new Primitive {
            Positions = positions, 
            Uvs = uvs,
            Normals = normals, 
            FacesDrawn = facesDrawn,
            VertexColors = vertexColors,
            FaceDir = faceDir,
        });

        positions = [];
        uvs = [];
        normals = [];
        facesDrawn = [];
        vertexColors = [];
      }

      while (br.Position + 4 < endPosition) {
        // Ensure we're always 4-byte aligned
        br.Align(4);

        var startingOffset = br.Position;

        // Read the header data and unpack it
        var signal = br.ReadNew<Signal>();
        signals.AddFirst(signal);

        if (!signal.IsValid) {
          Asserts.Fail(
              $"Invalid signal at offset {startingOffset.ToHexString()}: {signal}");
        }

        if (signal.Mode is SignalMode.NULL
                           or SignalMode.STRIP_0_END
                           or SignalMode.STRIP_N_END) {
          AddCurrentPrimitiveToMesh();
        } else if (signal.Mode is SignalMode.FLOAT32) {
          switch (signal.Index) {
            case SignalIndex.GEOM: {
              var unk0 = br.ReadUInt32();
              var unk1 = br.ReadUInt32();
              var unk2 = br.ReadSingle();
              var unk3 = br.ReadSingle();

              // What the fuck is this format, why in the ever loving fuck is
              // the vertex winding direction stored as a float????
              faceDir = unk2;
              break;
            }
            case SignalIndex.VERTEX: {
              throw new NotImplementedException();
            }
            default: throw new ArgumentOutOfRangeException();
          }
        } else {
          var dataCount = signal.DataCount;

          switch (signal.Index) {
            case SignalIndex.VERTEX: {
              // TODO: Still not quite right, positions are messed up (e.g.
              // the warrior's face)

              for (var i = 0; i < dataCount; ++i) {
                // Shamelessly stolen from:
                // https://github.com/haekb/gdl-tools/blob/master/Addons/GDLFormat/Models/Objects.gd#L305
                switch (signal.Mode) {
                  case SignalMode.SINT8_XYZ: {
                    var x = br.ReadSByte();
                    var y = br.ReadSByte();
                    var z = br.ReadSByte();
                    positions.Add(new Vector3(x, y, z));
                    break;
                  }
                  case SignalMode.SINT16_XYZ: {
                    var x = br.ReadInt16();
                    var y = br.ReadInt16();
                    var z = br.ReadInt16();
                    positions.Add(new Vector3(x, y, z));
                    break;
                  }
                  case SignalMode.SINT32_XYZ: {
                    var x = br.ReadInt32();
                    var y = br.ReadInt32();
                    var z = br.ReadInt32();
                    positions.Add(new Vector3(x, y, z));
                    break;
                  }
                  default:
                    throw new ArgumentOutOfRangeException(
                        nameof(signal.Mode),
                        signal.Mode,
                        null);
                }
              }

              positions.RemoveAt(positions.Count - 1);

              break;
            }
            case SignalIndex.UV: {
              // TODO: Still not quite right, UVs are still broken (e.g.
              // warrior's chest is messed up)
              for (var i = 0; i < dataCount; ++i) {
                var uv = new Uv(signal.Mode);
                uv.Read(br);
                uvs.Add(uv);
              }

              break;
            }
            case SignalIndex.NORMAL: {
              for (var i = 0; i < dataCount; ++i) {
                // Shamelessly stolen from:
                // https://github.com/haekb/gdl-tools/blob/master/Addons/GDLFormat/Models/Objects.gd#L367
                switch (signal.Mode) {
                  case SignalMode.UINT16_BITPACKED: {
                    var packedNormal = br.ReadUInt16();

                    var normalScale = 0.06666667f;
                    var x = (packedNormal & 0x1f) * normalScale - 1;
                    var y = ((packedNormal >> 5) & 0x1f) * normalScale - 1;
                    var z = ((packedNormal >> 10) & 0x1f) * normalScale - 1;

                    normals.Add(Vector3.Normalize(new Vector3(x, y, z)));

                    // This is super annoying. Just don't include faces that
                    // invalid, why do it this way????
                    if (i >= 2) {
                      facesDrawn.Add(packedNormal < 0x8000);
                    }
                    break;
                  }
                  default:
                    throw new ArgumentOutOfRangeException(
                        nameof(signal.Mode),
                        signal.Mode,
                        null);
                }
              }

              break;
            }
            case SignalIndex.VERTEX_COLOR: {
              for (var i = 0; i < dataCount; ++i) {
                switch (signal.Mode) {
                  case SignalMode.UINT16_BITPACKED: {
                    // Shamelessly stolen from:
                    // https://github.com/haekb/gdl-tools/blob/master/Addons/GDLFormat/Models/Objects.gd#L410
                    var value = br.ReadUInt16();
                    ColorUtil.SplitRgb5A1(value,
                                          out var b,
                                          out var g,
                                          out var r,
                                          out var a);
                    vertexColors.Add(FinColor.FromRgbaBytes(r, g, b, a));
                    break;
                  }
                  default:
                    throw new ArgumentOutOfRangeException(
                        nameof(signal.Mode),
                        signal.Mode,
                        null);
                }
              }

              break;
            }
            default:
              throw new ArgumentOutOfRangeException(
                  nameof(signal.Index),
                  signal.Index,
                  null);
          }
        }
      }

      signals.AddFirst((Signal?) null);

      AddCurrentPrimitiveToMesh();
    }
  }
}