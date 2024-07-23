using System.Numerics;

using fin.math;
using fin.schema;

using level5.decompression;

using schema.binary;

namespace level5.schema;

public class Prm {
  public string Name { get; private set; }

  public uint[] AnimationReferenceHashes { get; private set; }

  public string MaterialName { get; private set; }

  private uint[] nodeTable_;

  public List<uint> Triangles { get; set; }
  public List<GenericVertex> Vertices { get; set; }

  public Prm(byte[] data) {
    using (var r = new SchemaBinaryReader(new MemoryStream(data),
                                          Endianness.LittleEndian)) {
      Open(r);
    }
  }

  [Unknown]
  public void Open(IBinaryReader r) {
    r.AssertString("XMPR");
    var prmOffset = r.ReadUInt32();
    var unknownOffset = r.ReadUInt32();
    var prmHashesOffset = r.ReadUInt32();

    r.Position = prmOffset;
    r.AssertString("XPRM");

    // buffers-------------------------------------------

    uint pvbOffset = r.ReadUInt32();
    int pvbSize = r.ReadInt32();
    var polygonVertexBuffer =
        r.SubreadAt(pvbOffset + prmOffset, ser => ser.ReadBytes(pvbSize));

    uint pviOffset = r.ReadUInt32();
    int pviSize = r.ReadInt32();
    var polygonVertexIndexBuffer =
        r.SubreadAt(pviOffset + prmOffset, ser => ser.ReadBytes(pviSize));

    // node table-------------------------------------------

    r.Position = 0x28;
    uint noOffset = r.ReadUInt32();
    int noSize = r.ReadInt32() / 4 + 1;

    this.nodeTable_ = new uint[noSize];
    r.Position = noOffset;
    for (int i = 0; i < noSize; i++) {
      this.nodeTable_[i] = r.ReadUInt32();
    }

    // name and material-------------------------------------------
    r.Position = 0x30;

    var nameOffset = r.ReadUInt32();
    var nameLength = r.ReadUInt32();
    var name = r.SubreadAt(nameOffset, ser => ser.ReadString(nameLength));
    Name = name;

    var mtlNameOffset = r.ReadUInt32();
    var mtlNameLength = r.ReadUInt32();
    var mtlName
        = r.SubreadAt(mtlNameOffset, ser => ser.ReadString(mtlNameLength));
    MaterialName = mtlName;

    Triangles = this.ParseIndexBuffer_(polygonVertexIndexBuffer);
    Vertices = this.ParseBuffer_(polygonVertexBuffer);

    // hashes
    r.Position = prmHashesOffset;
    this.AnimationReferenceHashes = r.ReadUInt32s(4);
  }


  private List<uint> ParseIndexBuffer_(byte[] buffer) {
    List<uint> indices = [];
    int primitiveType = 0;
    int faceCount = 0;

    var endianness = Endianness.LittleEndian;

    using (var br = new SchemaBinaryReader(new MemoryStream(buffer),
                                           endianness)) {
      br.Position = 0x04;
      primitiveType = br.ReadInt16();
      uint faceOffset = br.ReadUInt16();
      faceCount = br.ReadInt32();

      buffer = new Level5Decompressor().Decompress(
          br.SubreadAt(faceOffset,
                       ser => ser.ReadBytes((int) (br.Length - faceOffset))));
    }

    if (primitiveType != 2 && primitiveType != 0)
      throw new NotSupportedException("Primitve Type no implemented");

    if (primitiveType == 0)
      using (var br = new SchemaBinaryReader(new MemoryStream(buffer),
                                             endianness)) {
        br.Position = 0;
        for (int i = 0; i < faceCount / 2; i++)
          indices.Add(br.ReadUInt16());
      }

    if (primitiveType == 2)
      using (var br = new SchemaBinaryReader(new MemoryStream(buffer),
                                             endianness)) {
        //Console.WriteLine(PrimitiveType + " " + FaceCount + " " + r.BaseStream.Length / 2);
        br.Position = 0;
        int f1 = br.ReadInt16();
        int f2 = br.ReadInt16();
        int f3;
        int dir = -1;
        int startdir = -1;
        for (int i = 0; i < faceCount - 2; i++) {
          if (br.Position + 2 > br.Length)
            break;
          //if (r.Position + 2 > r.Length)
          //    break;
          f3 = br.ReadInt16();
          if (f3 == 0xFFFF || f3 == -1) {
            f1 = br.ReadInt16();
            f2 = br.ReadInt16();
            dir = startdir;
          } else {
            dir *= -1;
            if (f1 != f2 && f2 != f3 && f3 != f1) {
              /*if (f1 > vCount || f2 > vCount || f3 > vCount)
               {
                   f1 = 0;
               }*/
              if (dir > 0) {
                indices.Add((uint) f1);
                indices.Add((uint) f2);
                indices.Add((uint) f3);
              } else {
                indices.Add((uint) f1);
                indices.Add((uint) f3);
                indices.Add((uint) f2);
              }
            }

            f1 = f2;
            f2 = f3;
          }
        }
      }

    return indices;
  }

  private List<GenericVertex> ParseBuffer_(byte[] buffer) {
    List<GenericVertex> vertices = [];
    byte[] attributeBuffer = [];
    int stride = 0;
    int vertexCount = 0;

    var endianness = Endianness.LittleEndian;

    using (var br = new SchemaBinaryReader(new MemoryStream(buffer),
                                           endianness)) {
      br.Position = 0x4;
      uint attOffset = br.ReadUInt16();
      int attSomething = br.ReadInt16();
      uint verOffset = br.ReadUInt16();
      stride = br.ReadInt16();
      vertexCount = br.ReadInt32();

      var level5Decompressor = new Level5Decompressor();
      attributeBuffer =
          level5Decompressor.Decompress(
              br.SubreadAt(attOffset, ser => ser.ReadBytes(attSomething)));
      buffer = level5Decompressor.Decompress(
          br.SubreadAt(verOffset,
                       ser => ser.ReadBytes((int) (br.Length - verOffset))));
    }

    int[] aCount = new int[10];
    int[] aOffset = new int[10];
    int[] aSize = new int[10];
    int[] aType = new int[10];
    using (var br = new SchemaBinaryReader(new MemoryStream(attributeBuffer),
                                           endianness)) {
      for (int i = 0; i < 10; i++) {
        aCount[i] = br.ReadByte();
        aOffset[i] = br.ReadByte();
        aSize[i] = br.ReadByte();
        aType[i] = br.ReadByte();

        if (aCount[i] > 0 &&
            i != 0 &&
            i != 1 &&
            i != 2 &&
            i != 4 &&
            i != 7 &&
            i != 8 &&
            i != 9) {
          Console.WriteLine(i +
                            " " +
                            aCount[i] +
                            " " +
                            aOffset[i] +
                            " " +
                            aSize[i] +
                            " " +
                            aType[i]);
        }
      }
    }

    using (var br = new SchemaBinaryReader(new MemoryStream(buffer),
                                           endianness)) {
      for (int i = 0; i < vertexCount; i++) {
        GenericVertex vert = new GenericVertex();
        vert.Clr = new Vector4(1, 1, 1, 1);
        for (int j = 0; j < 10; j++) {
          br.Position = (uint) (i * stride + aOffset[j]);
          switch (j) {
            case 0: //Position
              vert.Pos = ReadAttribute(br, aType[j], aCount[j]).Xyz();
              break;
            case 1: //Tangent
              break;
            case 2: //Normal
              vert.Nrm = ReadAttribute(br, aType[j], aCount[j]).Xyz();
              break;
            case 4: //Uv0
              vert.Uv0 = ReadAttribute(br, aType[j], aCount[j]).Xy();
              break;
            case 7: //Bone Weight
              vert.Weights = ReadAttribute(br, aType[j], aCount[j]);
              break;
            case 8: //Bone Index
              Vector4 vn = ReadAttribute(br, aType[j], aCount[j]);
              if (this.nodeTable_.Length > 0 && this.nodeTable_.Length != 1)
                vert.Bones = [
                    this.nodeTable_[(int) vn.X], this.nodeTable_[(int) vn.Y],
                    this.nodeTable_[(int) vn.Z], this.nodeTable_[(int) vn.W]
                ];
              break;
            case 9: // Color
              vert.Clr = ReadAttribute(br, aType[j], aCount[j]).Yzwx();
              break;
          }
        }

        vertices.Add(vert);
      }
    }

    //
    return vertices;
  }

  public Vector4 ReadAttribute(IBinaryReader f, int type, int count) {
    Vector4 o = new Vector4();
    switch (type) {
      case 0: //nothing
        break;
      case 1: //Vec3
        break;
      case 2: //Vec4
        if (count > 0 && f.Position + 4 < f.Length)
          o.X = f.ReadSingle();
        if (count > 1 && f.Position + 4 < f.Length)
          o.Y = f.ReadSingle();
        if (count > 2 && f.Position + 4 < f.Length)
          o.Z = f.ReadSingle();
        if (count > 3 && f.Position + 4 < f.Length)
          o.W = f.ReadSingle();
        break;
      default:
        throw new Exception("Unknown Type 0x" +
                            type.ToString("x") +
                            " " +
                            f.ReadInt32().ToString("X") +
                            f.ReadInt32().ToString("X"));
    }

    return o;
  }
}

public class GenericVertex {
  public Vector3 Pos { get; set; }
  public Vector3 Nrm { get; set; }
  public Vector2 Uv0 { get; set; }
  public Vector4 Weights { get; set; }
  public Vector4 Clr { get; set; }
  public uint[]? Bones { get; set; }
}