using fin.schema;
using fin.schema.matrix;
using fin.util.asserts;

using gx;

using modl.schema.modl.common;

using schema.binary;

namespace modl.schema.modl.bw2.node;

public enum WeirdFlag {
  VALUE_0,
  VALUE_1, 
  VALUE_2, 
  VALUE_3, 
  VALUE_4, 
}

public class Bw2Node : IBwNode, IBinaryDeserializable {
  private int additionalDataCount_;

  public string GetIdentifier() => this.Name;
  public string Name { get; set; }

  public WeirdFlag WeirdFlag { get; set; }

  public bool IsHidden => this.Name.StartsWith("PHYSICS_COLLIDE") ||
                          this.Name.StartsWith("COLLISION_") ||
                          this.Name.StartsWith("NODRAW");

  public BwTransform Transform { get; } = new();

  public float Scale { get; set; }

  public List<IBwMaterial> Materials { get; } = [];

  public Bw2Node(int additionalDataCount) {
    this.additionalDataCount_ = additionalDataCount;
  }

  [Unknown]
  public void Read(IBinaryReader br) {
    SectionHeaderUtil.AssertNameAndReadSize(
        br,
        "NODE",
        out var nodeSize);
    var nodeStart = br.Position;
    var expectedNodeEnd = nodeStart + nodeSize;

    var nodeNameLength = br.ReadInt32();
    var nodeName = br.ReadString(nodeNameLength);
    this.Name = nodeName;

    var headerStart = br.Position;
    var expectedHeaderEnd = headerStart + 0x38;
    {
      // TODO: What are these used for?
      var someMin = br.ReadUInt16();
      var someMax = br.ReadUInt16();

      // TODO: unknown, probably enum values
      var unknowns0 = br.ReadUInt32s(2);

      var flagValue = unknowns0[0] >> (6 * 4);
      this.WeirdFlag = (WeirdFlag) flagValue;

      {
        br.PushMemberEndianness(Endianness.LittleEndian);
        this.Transform.Read(br);
        br.PopEndianness();
      }

      // TODO: unknown, also transform??
      // These look very similar to the values defined in the constructor
      var unknowns1 = br.ReadSingles(4);
    }
    Asserts.Equal(br.Position, expectedHeaderEnd);

    // TODO: additional data
    var additionalData = br.ReadUInt32s(this.additionalDataCount_);

    SectionHeaderUtil.AssertNameAndSize(br, "BBOX", 4 * 6);
    var bbox = br.ReadNew<BwBoundingBox>();

    string sectionName;
    uint sectionSize;
    SectionHeaderUtil.ReadNameAndSize(br, out sectionName, out sectionSize);

    while (sectionName != "MATL") {
      if (sectionName == "VSCL") {
        Asserts.Equal(4, (int) sectionSize);
        br.PushMemberEndianness(Endianness.LittleEndian);
        this.Scale = br.ReadSingle();
        br.PopEndianness();
      } else if (sectionName == "RNOD") {
        this.ReadRnod_(br);
      } else {
        throw new NotImplementedException();
      }

      SectionHeaderUtil.ReadNameAndSize(br, out sectionName, out sectionSize);
    }

    Asserts.SequenceEqual("MATL", sectionName);

    var materialSize = 0xA4;
    Asserts.Equal(0, sectionSize % materialSize);

    this.Materials.Clear();
    for (var i = 0; i < sectionSize / materialSize; ++i) {
      this.Materials.Add(br.ReadNew<Bw2Material>());
    }

    var vertexDescriptor = new GxVertexDescriptor();
    while (br.Position < expectedNodeEnd) {
      SectionHeaderUtil.ReadNameAndSize(br, out sectionName, out sectionSize);
      var expectedSectionEnd = br.Position + sectionSize;

      switch (sectionName) {
        case "VUV1":
        case "VUV2":
        case "VUV3":
        case "VUV4": {
          // TODO: Need to keep track of section order
          var uvMapIndex = sectionName[3] - '1';
          this.ReadUvMap_(br, uvMapIndex, sectionSize / (2 * 2));
          break;
        }
        case "VPOS": {
          // TODO: Handle this properly
          // Each new VPOS section seems to correspond to a new LOD mesh, but we only need the first one.
          if (Positions.Count > 0) {
            br.Position = expectedNodeEnd;
            goto BreakEarly;
          }

          var vertexPositionSize = 2 * 3;
          Asserts.Equal(0, sectionSize % vertexPositionSize);
          this.ReadPositions_(br, (uint) (sectionSize / vertexPositionSize));
          break;
        }
        case "VNRM": {
          var normalSize = 3;
          Asserts.Equal(0, sectionSize % normalSize);
          this.ReadNormals_(br, (uint) (sectionSize / normalSize));
          break;
        }
        case "VNBT": {
          var nbtSize = 4 * 9;
          Asserts.Equal(0, sectionSize % nbtSize);
          var nbtCount = sectionSize / nbtSize;
          for (var i = 0; i < nbtCount; ++i) {
            this.Normals.Add(new VertexNormal {
                X = br.ReadSingle(),
                Y = br.ReadSingle(),
                Z = br.ReadSingle(),
            });
            br.Position += 24;
          }
          break;
        }
        case "XBS2": {
          this.ReadOpcodes_(br, sectionSize, ref vertexDescriptor);
          break;
        }
        case "SCNT": {
          // TODO: Support this
          // This explains why multiple VPOS sections are included.
          Asserts.Equal(4, (int) sectionSize);
          var lodCount = br.ReadUInt32();
          break;
        }
        case "VCOL": {
          br.Position += sectionSize;
          break;
        }
        case "ANIM": {
          br.Position += sectionSize;
          break;
        }
        default: throw new NotImplementedException();
      }

      Asserts.Equal(br.Position, expectedSectionEnd);
    }

    BreakEarly: ;
    Asserts.Equal(br.Position, expectedNodeEnd);
  }


  public Matrix4x4f[] RnodMatrices { get; set; }

  private void ReadRnod_(IBinaryReader br) {
    br.PushMemberEndianness(Endianness.LittleEndian);

    var size = br.ReadUInt32();
    this.RnodMatrices = new Matrix4x4f[size];

    for (var i = 0; i < this.RnodMatrices.Length; ++i) {
      this.RnodMatrices[i] = br.ReadNew<Matrix4x4f>();
    }

    br.PopEndianness();
  }


  public VertexUv[][] UvMaps { get; } = new VertexUv[4][];

  private void ReadUvMap_(IBinaryReader br,
                          int uvMapIndex,
                          uint uvCount) {
    var scale = MathF.Pow(2, 11);
    var uvMap = this.UvMaps[uvMapIndex] = new VertexUv[uvCount];
    for (var i = 0; i < uvCount; ++i) {
      uvMap[i] = new VertexUv {
          U = br.ReadInt16() / scale,
          V = br.ReadInt16() / scale,
      };
    }
  }


  public List<VertexPosition> Positions { get; } = [];

  private void ReadPositions_(IBinaryReader br, uint vertexCount) {
    for (var i = 0; i < vertexCount; ++i) {
      this.Positions.Add(br.ReadNew<VertexPosition>());
    }
  }


  public List<VertexNormal> Normals { get; } = [];

  private void ReadNormals_(IBinaryReader br, uint vertexCount) {
    for (var i = 0; i < vertexCount; ++i) {
      this.Normals.Add(br.ReadNew<VertexNormal>());
    }
  }

  public List<BwMesh> Meshes { get; } = [];

  private void ReadOpcodes_(IBinaryReader br,
                            uint sectionSize,
                            ref GxVertexDescriptor vertexDescriptor) {
    var start = br.Position;
    var expectedEnd = start + sectionSize;

    var materialIndex = br.ReadUInt32();

    var posMatIdxMap = br.ReadNew<Bw2PosMatIdxMap>();

    var gxDataSize = br.ReadUInt32();
    Asserts.Equal(expectedEnd, br.Position + gxDataSize);

    var triangleStrips = new List<BwTriangleStrip>();
    var mesh = new BwMesh {
        MaterialIndex = materialIndex,
        TriangleStrips = triangleStrips
    };
    this.Meshes.Add(mesh);

    while (br.Position < expectedEnd) {
      var opcode = br.ReadByte() & 0xFA;
      var opcodeEnum = (GxOpcode) opcode;

      if (opcodeEnum == GxOpcode.LOAD_CP_REG) {
        var command = br.ReadByte();
        var value = br.ReadUInt32();

        if (command == 0x50) {
          vertexDescriptor =
              new GxVertexDescriptor(
                  (vertexDescriptor.Value & ~((uint) 0x1FFFF)) |
                  value);
        } else if (command == 0x60) {
          vertexDescriptor =
              new GxVertexDescriptor(
                  (vertexDescriptor.Value & 0x1FFFF) |
                  (value << 17));
        } else {
          throw new NotImplementedException();
        }
      } else if (opcodeEnum == GxOpcode.LOAD_XF_REG) {
        var lengthMinusOne = br.ReadUInt16();
        var length = lengthMinusOne + 1;

        // http://hitmen.c02.at/files/yagcd/yagcd/chap5.html#sec5.11.4
        var firstXfRegisterAddress = br.ReadUInt16();

        var values = br.ReadUInt32s(length);
        // TODO: Implement
      } else if (opcodeEnum == GxOpcode.DRAW_TRIANGLE_STRIP) {
        var vertexCount = br.ReadUInt16();
        var vertexAttributeIndicesList =
            new List<BwVertexAttributeIndices>(vertexCount);

        var triangleStrip = new BwTriangleStrip {
            VertexAttributeIndicesList = vertexAttributeIndicesList,
        };
        triangleStrips.Add(triangleStrip);

        for (var i = 0; i < vertexCount; ++i) {
          var vertexAttributeIndices = new BwVertexAttributeIndices {
              TexCoordIndices = new ushort?[8],
          };

          foreach (var (vertexAttribute, vertexFormat) in
                   vertexDescriptor) {
            var value = vertexFormat switch {
                null                     => br.ReadByte(),
                GxAttributeType.INDEX_8  => br.ReadByte(),
                GxAttributeType.INDEX_16 => br.ReadUInt16(),
                _                        => throw new NotImplementedException(),
            };

            switch (vertexAttribute) {
              case GxVertexAttribute.PosMatIdx: {
                Asserts.Equal(0, value % 3);
                value /= 3;
                vertexAttributeIndices.NodeIndex = posMatIdxMap[value];
                break;
              }
              case GxVertexAttribute.Position: {
                vertexAttributeIndices.PositionIndex = value;
                break;
              }
              case GxVertexAttribute.Normal: {
                vertexAttributeIndices.NormalIndex = value;
                break;
              }
              case GxVertexAttribute.Tex0Coord:
              case GxVertexAttribute.Tex1Coord:
              case GxVertexAttribute.Tex2Coord:
              case GxVertexAttribute.Tex3Coord:
              case GxVertexAttribute.Tex4Coord:
              case GxVertexAttribute.Tex5Coord:
              case GxVertexAttribute.Tex6Coord:
              case GxVertexAttribute.Tex7Coord: {
                var index = vertexAttribute - GxVertexAttribute.Tex0Coord;
                vertexAttributeIndices.TexCoordIndices[index] = value;
                break;
              }
              case GxVertexAttribute.Color0:
              case GxVertexAttribute.Color1: {
                break;
              }
              default: {
                throw new NotImplementedException();
              }
            }
          }

          vertexAttributeIndicesList.Add(vertexAttributeIndices);
        }
      } else if (opcodeEnum == GxOpcode.NOP) { } else {
        throw new NotImplementedException();
      }
    }

    Asserts.Equal(expectedEnd, br.Position);
  }
}