using System.Buffers.Binary;
using System.IO.Compression;
using System.Text;

using schema.binary;

namespace DanDare.schema;

public enum A3dFieldType {
  BOOL = 0,
  INT = 2,
  INT_ARRAY = 3,
  FLOAT = 4,
  FLOAT_ARRAY = 5,
  DOUBLE = 6,
  STRING = 8,
  NODE = 10,
  NODE_ARRAY = 11,
  FLOAT3 = 12,
  FLOAT4 = 13
}

public sealed record A3dField(string Name, A3dFieldType Type) {
  public object? Value { get; set; }
  public List<A3dField> Links { get; } = new();
}

public sealed record A3dNode(string Type, string Name) {
  public Dictionary<string, A3dField> Fields { get; } = new();

  public A3dField GetOrCreateField(string name, A3dFieldType defaultType) {
    if (!Fields.TryGetValue(name, out var field)) {
      field = new A3dField(name, defaultType);
      Fields[name] = field;
    }

    return field;
  }
}

public sealed class A3dScene {
  public A3dNode? Root { get; set; }
  public Dictionary<string, byte[]> EmbeddedFiles { get; } = new();
}

public sealed class A3dReader {
  private const int ARCHIVE_MAGIC_ = 1095127040; // "AFP\0" (0x41465000)

  private static readonly (int Components, int ByteWidth)[] QUANTIZATION_MODES_
      = {
          (0, 0),
          (1, 1),
          (1, 2),
          (2, 1),
          (2, 2),
          (3, 1),
          (3, 2)
      };

  private static readonly string[] NODE_TYPES_ = {
      "aCamera", "aExtern", "aRotationLinear", "aPositionLinear",
      "aVertexLinear",
      "aFloatLinear", "aColorLinear", "aLight", "aMaterial", "aMesh",
      "aParameter", "aTexture", "aTimer", "aTouch", "aTransform",
      "USE", "LINK", "aLightDirectional", "aLightOmni"
  };

  private static readonly string[] FIELD_NAMES_ = {
      "children", "fov", "position", "orientation", "rotation",
      "scaleRotation", "scale", "ccw", "vertex", "vertexIndex",
      "map", "mapIndex", "material", "diffuse", "texture",
      "image", "crease", "center", "cycleInterval", "keyTime",
      "keyValue", "loop", "isActive", "emissive", "startTime",
      "choice", "touch", "enabled", "pressed", "isOver",
      "touchTime", "description", "on", "type", "color",
      "intensity", "source", "attenuation", "radius", "direction",
      "near", "far", "stopTime", "cycleTime", "alpha",
      "time", "value", "transparency", "shading", "bilinear",
      "normal", "normalIndex", "colorIndex", "materialIndex", "sendBool",
      "sendTime", "command", "LINK", "url", "copyright",
      "showCopyright", "sceneBilinear", "backgroundColor", "backgroundImage",
      "sceneAntialias",
      "fogColor", "fogNear", "fogFar", "fogActive"
  };

  private readonly Dictionary<string, A3dNode> namedNodes_ = new();

  /// <summary>
  /// Reads a full .a3d archive (header + scene + embedded textures) or a raw/gzipped .a3x scene stream.
  /// </summary>
  public A3dScene Load(Stream stream) {
    using var br = new SchemaBinaryReader(stream, Endianness.BigEndian);
    int magic = br.ReadInt32();

    // Check if this is an AFP\0 container archive
    if (magic == ARCHIVE_MAGIC_) {
      int entryCount = br.ReadInt32();
      var entries = new (string Name, int Size)[entryCount];

      for (int i = 0; i < entryCount; i++) {
        string name = br.ReadStringNT();
        int size = br.ReadInt32();
        entries[i] = (name, size);
      }

      byte[] sceneBytes = br.ReadBytes(entries[0].Size);
      var scene = this.ParseScene(sceneBytes);

      for (int i = 1; i < entryCount; i++) {
        scene.EmbeddedFiles[entries[i].Name]
            = br.ReadBytes(entries[i].Size);
      }

      return scene;
    }

    // Otherwise, rewind the 4 magic bytes and parse directly as an .a3x scene
    using var combined = new MemoryStream();
    Span<byte> prefix = stackalloc byte[4];
    BinaryPrimitives.WriteInt32BigEndian(prefix, magic);
    combined.Write(prefix);
    stream.CopyTo(combined);
    return this.ParseScene(combined.ToArray());
  }

  public A3dScene ParseScene(byte[] data) {
    this.namedNodes_.Clear();
    var scene = new A3dScene();

    Stream baseStream = new MemoryStream(data);
    if (data.Length >= 2 && data[0] == 0x1F && data[1] == 0x8B) {
      baseStream = new GZipStream(baseStream, CompressionMode.Decompress);
    }

    using (baseStream) {
      using var br = new SchemaBinaryReader(baseStream);
      string header = Encoding.ASCII.GetString(br.ReadBytes(4));
      if (header != "a3dx") {
        throw new InvalidDataException(
            $"Invalid a3x file header: '{header}'");
      }

      var root = new A3dNode("aTransform", "SCENEGRAPH");
      var childrenField
          = root.GetOrCreateField("children", A3dFieldType.NODE_ARRAY);
      this.ReadNodeArrayField_(br, root, childrenField);

      this.ReadTrailingLinks_(br);

      scene.Root = root;
      return scene;
    }
  }

  private void ReadFieldValue_(IBinaryReader br,
                               A3dNode ownerNode,
                               A3dField field) {
    switch (field.Type) {
      case A3dFieldType.BOOL:
        field.Value = br.ReadByte() != 0;
        break;
      case A3dFieldType.INT:
        field.Value = br.ReadInt32();
        break;
      case A3dFieldType.INT_ARRAY:
        field.Value = ReadPackedIntArray_(br);
        break;
      case A3dFieldType.FLOAT:
        field.Value = br.ReadSingle();
        break;
      case A3dFieldType.FLOAT_ARRAY:
        field.Value = ReadQuantizedFloatArray_(br);
        break;
      case A3dFieldType.DOUBLE:
        field.Value = br.ReadDouble();
        break;
      case A3dFieldType.STRING:
        field.Value = br.ReadStringNT();
        break;
      case A3dFieldType.NODE:
        field.Value = this.ReadNode_(br);
        break;
      case A3dFieldType.NODE_ARRAY:
        this.ReadNodeArrayField_(br, ownerNode, field);
        break;
      case A3dFieldType.FLOAT3:
        field.Value = new[]
            { br.ReadSingle(), br.ReadSingle(), br.ReadSingle() };
        break;
      case A3dFieldType.FLOAT4:
        field.Value = new[] {
            br.ReadSingle(), br.ReadSingle(), br.ReadSingle(),
            br.ReadSingle()
        };
        break;
      default:
        throw new NotSupportedException(
            $"Unsupported field type {(int) field.Type} on '{ownerNode.Type}.{field.Name}'");
    }
  }

  private void ReadNodeArrayField_(IBinaryReader br,
                                   A3dNode ownerNode,
                                   A3dField field) {
    int count = br.ReadInt32();
    var list = field.Value as List<A3dNode> ?? new List<A3dNode>(count);
    for (int i = 0; i < count; i++) {
      var node = this.ReadNode_(br);
      if (node != null) {
        list.Add(node);
      }
    }

    field.Value = list;
  }

  private static int[] ReadPackedIntArray_(IBinaryReader br) {
    uint header = br.ReadUInt32();
    int mode = (int) (header >> 24);
    int count = (int) (header & 0x00FFFFFF);
    int[] result = new int[count];

    switch (mode) {
      case 0:
        for (int i = 0; i < count; i++)
          result[i] = br.ReadInt32();
        break;

      case 1:
        for (int i = 0; i < count; i++) {
          int val = br.ReadByte();
          result[i] = val == 255 ? -1 : val;
        }

        break;

      case 2:
        for (int i = 0; i < count; i++) {
          int val = br.ReadUInt16();
          result[i] = val == 65535 ? -1 : val;
        }

        break;

      default:
        throw new InvalidDataException(
            $"Unknown packed int array mode: {mode}");
    }

    return result;
  }

  private static float[] ReadQuantizedFloatArray_(IBinaryReader br) {
    uint header = br.ReadUInt32();
    int mode = (int) (header >> 24);
    int totalFloats = (int) (header & 0x00FFFFFF);
    float[] result = new float[totalFloats];

    var (components, byteWidth) = QUANTIZATION_MODES_[mode];

    if (components == 0) {
      for (int i = 0; i < totalFloats; i++)
        result[i] = br.ReadSingle();
      return result;
    }

    Span<float> mins = stackalloc float[components];
    Span<float> ranges = stackalloc float[components];
    for (int c = 0; c < components; c++) {
      mins[c] = br.ReadSingle();
      ranges[c] = br.ReadSingle();
    }

    int tupleCount = totalFloats / components;
    int outIndex = 0;
    if (byteWidth == 1) {
      for (int t = 0; t < tupleCount; t++) {
        for (int c = 0; c < components; c++) {
          float q = br.ReadByte();
          result[outIndex++] = q * ranges[c] / 255.0f + mins[c];
        }
      }
    } else if (byteWidth == 2) {
      for (int t = 0; t < tupleCount; t++) {
        for (int c = 0; c < components; c++) {
          float q = br.ReadUInt16();
          result[outIndex++] = q * ranges[c] / 65535.0f + mins[c];
        }
      }
    }

    return result;
  }

  private A3dNode? ReadNode_(IBinaryReader br) {
    byte typeId = br.ReadByte();
    string nodeType = NODE_TYPES_[typeId];
    if (nodeType == "USE") {
      string targetName = br.ReadStringNT();
      this.namedNodes_.TryGetValue(targetName, out var existing);
      return existing;
    }

    if (nodeType == "LINK") {
      this.ReadLink_(br);
      return null;
    }

    string nodeName = br.ReadStringNT();
    return this.ReadNodeBody_(br, nodeType, nodeName);
  }

  private A3dNode ReadNodeBody_(IBinaryReader br,
                                string nodeType,
                                string nodeName) {
    var node = new A3dNode(nodeType, nodeName);
    this.namedNodes_[nodeName] = node;
    while (true) {
      int rawId = br.ReadByte();
      if (rawId == '}') // 125
        break;
      string fieldName = rawId != 0xFF
          ? FIELD_NAMES_[rawId]
          : br.ReadStringNT();
      if (fieldName == "LINK") {
        this.ReadLink_(br);
        continue;
      }

      A3dFieldType fieldType = ResolveFieldType_(nodeType, fieldName);
      var field = node.GetOrCreateField(fieldName, fieldType);
      this.ReadFieldValue_(br, node, field);
    }

    return node;
  }

  private void ReadTrailingLinks_(IBinaryReader br) {
    while (!br.Eof) {
      var typeId = br.ReadByte();
      string nodeType = NODE_TYPES_[typeId];
      if (nodeType != "LINK") {
        throw new InvalidDataException(
            $"Expected LINK at trailing section, got '{nodeType}'.");
      }

      this.ReadLink_(br);
    }
  }

  private void ReadLink_(IBinaryReader br) {
    string source = br.ReadStringNT();
    string target = br.ReadStringNT();
    int srcDot = source.IndexOf('.');
    int dstDot = target.IndexOf('.');
    string srcNodeName = source[..srcDot];
    string srcFieldName = source[(srcDot + 1)..];
    string dstNodeName = target[..dstDot];
    string dstFieldName = target[(dstDot + 1)..];
    if (this.namedNodes_.TryGetValue(srcNodeName, out var srcNode) &&
        this.namedNodes_.TryGetValue(dstNodeName, out var dstNode)) {
      var srcField
          = srcNode.GetOrCreateField(srcFieldName,
                                     ResolveFieldType_(
                                         srcNode.Type,
                                         srcFieldName));
      var dstField
          = dstNode.GetOrCreateField(dstFieldName,
                                     ResolveFieldType_(
                                         dstNode.Type,
                                         dstFieldName));
      srcField.Links.Add(dstField);
    }
  }

  /// 
  /// Maps (nodeType, fieldName) to the binary A3dFieldType expected by the stream.
  /// 
  private static A3dFieldType ResolveFieldType_(string nodeType,
                                                string fieldName)
    => (nodeType, fieldName) switch {
        (_, "children") => A3dFieldType.NODE_ARRAY,
        (_, "ccw"
            or "loop"
            or "isActive"
            or "enabled"
            or "pressed"
            or "isOver"
            or "on"
            or "bilinear"
            or "sendBool"
            or "showCopyright"
            or "sceneBilinear"
            or "sceneAntialias"
            or "fogActive") => A3dFieldType.BOOL,
        (_, "choice" or "type" or "shading") => A3dFieldType.INT,
        (_, "vertexIndex"
            or "mapIndex"
            or "normalIndex"
            or "colorIndex"
            or "materialIndex") => A3dFieldType.INT_ARRAY,
        (_, "fov"
            or "crease"
            or "intensity"
            or "radius"
            or "near"
            or "far"
            or "alpha"
            or "transparency"
            or "fogNear"
            or "fogFar") => A3dFieldType.FLOAT,
        (_, "vertex" or "map" or "normal" or "keyTime") => A3dFieldType
            .FLOAT_ARRAY,
        ("aColorLinear", "keyValue")    => A3dFieldType.FLOAT_ARRAY,
        ("aPositionLinear", "keyValue") => A3dFieldType.FLOAT_ARRAY,
        ("aRotationLinear", "keyValue") => A3dFieldType.FLOAT_ARRAY,
        ("aVertexLinear", "keyValue")   => A3dFieldType.FLOAT_ARRAY,
        ("aFloatLinear", "keyValue")    => A3dFieldType.FLOAT_ARRAY,
        (_, "cycleInterval"
            or "startTime"
            or "touchTime"
            or "stopTime"
            or "cycleTime"
            or "time"
            or "sendTime") => A3dFieldType.DOUBLE,
        (_, "image"
            or "description"
            or "source"
            or "command"
            or "url"
            or "copyright"
            or "backgroundImage") => A3dFieldType.STRING,
        (_, "material" or "texture" or "touch") => A3dFieldType.NODE,
        (_, "position"
            or "scale"
            or "center"
            or "diffuse"
            or "emissive"
            or "attenuation"
            or "direction"
            or "backgroundColor"
            or "fogColor") => A3dFieldType.FLOAT3,
        ("aMesh", "color") => A3dFieldType.FLOAT_ARRAY,
        (_, "color")       => A3dFieldType.FLOAT3,
        (_, "orientation" or "rotation" or "scaleRotation") => A3dFieldType
            .FLOAT4,
        ("aFloatLinear", "value") => A3dFieldType.FLOAT,
        ("aPositionLinear" or "aColorLinear", "value") => A3dFieldType.FLOAT3,
        ("aRotationLinear", "value") => A3dFieldType.FLOAT4,
        ("aVertexLinear", "value") => A3dFieldType.FLOAT_ARRAY,
        _ => throw new KeyNotFoundException(
            $"Unknown field type mapping for '{nodeType}.{fieldName}'.")
    };
}