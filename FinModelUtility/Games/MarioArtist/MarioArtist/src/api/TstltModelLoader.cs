using System.Drawing;
using System.Drawing.Drawing2D;
using System.Numerics;

using CommunityToolkit.Diagnostics;

using f3dzex2.displaylist;
using f3dzex2.displaylist.opcodes;
using f3dzex2.displaylist.opcodes.f3dzex2;
using f3dzex2.image;
using f3dzex2.io;
using f3dzex2.model;

using fin.color;
using fin.data.dictionaries;
using fin.data.lazy;
using fin.data.queues;
using fin.io;
using fin.math.fixedPoint;
using fin.math.matrix.four;
using fin.math.rotations;
using fin.model;
using fin.model.impl;
using fin.model.io;
using fin.model.io.importers;
using fin.model.util;
using fin.schema.vector;
using fin.util.hex;
using fin.util.sets;

using schema.binary;
using schema.binary.attributes;

using WrapMode = fin.model.WrapMode;


namespace marioartist.schema;

public record TstltModelFileBundle(IReadOnlyTreeFile MainFile)
    : IModelFileBundle;

public partial class TstltModelLoader : IModelImporter<TstltModelFileBundle> {
  public IModel Import(TstltModelFileBundle fileBundle) {
    using var br = fileBundle.MainFile.OpenReadAsBinary(Endianness.BigEndian);
    var tstlt = br.ReadNew<Tstlt>();

    var defaultFile = new FinFile(
        "C:\\Users\\Ryan\\Desktop\\Mario Artist Experiments\\files\\DEFAULT.TSTLT");
    using var fs = defaultFile.OpenRead();
    var defaultBr = new SchemaBinaryReader(fs, Endianness.BigEndian);

    var n64Hardware = new N64Hardware<N64Memory>();
    n64Hardware.Rdp = new Rdp {Tmem = new NoclipTmem(n64Hardware)};
    n64Hardware.Rsp = new Rsp();
    var n64Memory = n64Hardware.Memory = new N64Memory(defaultFile);
    n64Memory.SetSegment(0, 0, (uint) defaultBr.Length);

    var dlModelBuilder = new DlModelBuilder(n64Hardware);

    var model = dlModelBuilder.Model;
    n64Hardware.Rsp.ActiveBone = model.Skeleton.Root;

    var materialManager = model.MaterialManager;
    var thumbnailTexture =
        materialManager.CreateTexture(tstlt.Thumbnail.ToImage());
    thumbnailTexture.Name = "thumbnail";

    var faceTextures =
        materialManager.CreateTexture(tstlt.FaceTextures.ToImage());
    faceTextures.Name = "face";

    br.Position = 0x12e80;
    var image1 = new L8Image(32, 32 * 16);
    image1.Read(br);
    var image1Texture =
        materialManager.CreateTexture(image1.ToImage());
    image1Texture.Name = "image1";

    br.Position = 0x1bef0;
    var image3 = new Argb1555Image(32, 32 * 16);
    image3.Read(br);
    var image3Texture =
        materialManager.CreateTexture(image3.ToImage());
    image3Texture.Name = "image3";

    br.Position = 0x16770;
    var image2 = new Argb1555Image(32, 32 * 8);
    image2.Read(br);
    var image2Texture =
        materialManager.CreateTexture(image2.ToImage());
    image2Texture.Name = "palette";

    defaultBr.Position = 0x49C;
    var headSectionLength = defaultBr.ReadUInt32();

    defaultBr.Position = 0xa938;

    var joints = defaultBr.ReadNews<Joint>(0x1F);
    var jointsByParent =
        new BidirectionalDictionary<Joint?, List<(Joint joint, int index)>>();
    for (var i = 0; i < joints.Length; ++i) {
      var joint = joints[i];
      var parentIndex = joint.parentIndex;
      var parentJoint = parentIndex < 0 || parentIndex >= joints.Length
          ? null
          : joints[parentIndex];

      List<(Joint joint, int index)> parentChildren;
      if (jointsByParent.ContainsKey(parentJoint)) {
        parentChildren = jointsByParent[parentJoint];
      } else {
        parentChildren = jointsByParent[parentJoint] =
            new List<(Joint joint, int index)>();
      }

      parentChildren.Add((joint, i));
    }

    var translations = new Vector3[joints.Length];
    var rotations = new Vector3[joints.Length];
    var quaternions = new Quaternion[joints.Length];
    var scales = new Vector3[joints.Length];

    for (var i = 0; i < joints.Length; ++i) {
      var joint = joints[i];
      var worldMatrix = Matrix4x4.Transpose(joint.matrix);

      var bone = model.Skeleton.Root.AddChild(worldMatrix);
      bone.Name = $"bone {i}";

      Matrix4x4.Decompose(worldMatrix,
                          out var scale,
                          out var quaternion,
                          out var translation);

      translations[i] = translation;
      quaternions[i] = quaternion;
      scales[i] = scale;
      rotations[i] = quaternion.ToEulerRadians();
    }

    ;

    /*var jointsAndTransformStuff =
        joints.Select(j => {
          var matrix = Matrix4x4.Transpose(j.matrix);
          Matrix4x4.Decompose(matrix,
                              out var scale,
                              out var rotation,
                              out var translation);

          return (j, matrix, translation, rotation, scale);
        }).ToArray();

    var finBonesAndWorldMatrices = new IBone[joints.Length];
    var jointQueue = new FinTuple3Queue<(Joint joint, int index), Matrix4x4, IBone>(
        jointsByParent[(Joint?) null]
            .Select(rootJoint => (rootJoint, Matrix4x4.Identity,
                                  model.Skeleton.Root)));
    while (jointQueue.TryDequeue(out var jointAndIndex, out var parentMatrix, out var parentBone)) {
      Matrix4x4.Invert(parentMatrix, out var invertedParentMatrix);

      var (joint, index) = jointAndIndex;
      var worldMatrix = Matrix4x4.Transpose(joint.matrix);
      var localMatrix = worldMatrix * invertedParentMatrix;

      var bone = parentBone.AddChild(localMatrix);
      finBonesAndWorldMatrices[index] = bone;

      if (jointsByParent.ContainsKey(joint)) {
        jointQueue.Enqueue(jointsByParent[joint]
                               .Select(childJoint => (childJoint, worldMatrix, bone)));
      }
    }*/

    var headSectionOffset = 0x16770;
    var bodySectionOffset = headSectionOffset + headSectionLength;

    var meshGroupOffsets = new[] {
        (
            headSectionOffset,
            headSectionLength,
            new[] {
                0x1a770,
                0x1cca0,
                0x1e6b0,
                0x1e6b0,
                0x1f0c0,
            }
        ),
        (
            bodySectionOffset,
            defaultBr.Length - bodySectionOffset,
            new[] {
                0x290f0,
                0x2ab90,
                0x2b780,
                0x2c320,
                0x2e100,
                0x2f1a0,
                0x2fcf0,
                0x30b00,
                0x31790,
                0x32190,
                0x328b0,
                0x33000,
                0x337c0,
                0x33e70,
                0x34900,
                0x35360,
                0x36040,
            }
        )
    };

    var skin = model.Skin;

    foreach (var (meshGroupOffset, meshGroupLength, meshOffsets) in
             meshGroupOffsets) {
      n64Memory.SetSegment(0xF, (uint) meshGroupOffset, (uint) meshGroupLength);

      var lazyVertices =
          new LazyDictionary<(uint segmentedAddress, (int width, int height)?),
              IReadOnlyVertex>(tuple => {
            var (segmentedAddress, textureResolution) = tuple;

            var fileAddress = segmentedAddress - 0x0f000000 + meshGroupOffset;
            var v = defaultBr.SubreadAt(fileAddress,
                                        () => defaultBr.ReadNew<Vertex>());

            var finVertex =
                skin.AddVertex(v.Position.X, v.Position.Y, v.Position.Z);

            var floatU = v.U / ((textureResolution?.width ?? 32) * 32f);
            var floatV = v.V / ((textureResolution?.height ?? 32) * 32f);

            finVertex.SetUv(floatU, floatV);

            finVertex.SetLocalNormal(
                Vector3.Normalize(
                    new Vector3(v.NormalX, v.NormalY, v.NormalZ)));

            return finVertex;
          });

      var lazyMaterials =
          new LazyDictionary<(uint segmentedAddress, (int width, int height,
              WrapMode, WrapMode, N64ColorFormat)?),
              (IReadOnlyMaterial, ITexture)>(tuple => {
            var (segmentedAddress, textureResolutionAndColorFormat) = tuple;

            IoUtils.SplitSegmentedAddress(segmentedAddress,
                                          out var segment,
                                          out var address);
            var fileAddress = segment switch {
                0x0f => meshGroupOffset + address,
            };

            var (textureWidth, textureHeight, wrapModeU, wrapModeV, colorFormat
                    ) =
                textureResolutionAndColorFormat ?? (
                    32, 32, WrapMode.CLAMP, WrapMode.CLAMP, N64ColorFormat.L);

            var image = defaultBr.SubreadAt(
                fileAddress,
                () => {
                  var image = colorFormat switch {
                      N64ColorFormat.L => (IMarioArtistImage) new L8Image(
                          textureWidth,
                          textureHeight),
                      N64ColorFormat.RGBA => new Argb1555Image(
                          textureWidth,
                          textureHeight),
                  };

                  image.Read(defaultBr);

                  return image.ToImage();
                });


            var (finMaterial, finTexture) = model.MaterialManager
                                                 .AddSimpleTextureMaterialFromImage(
                                                     image,
                                                     tuple.segmentedAddress
                                                         .ToHexString());

            finTexture.WrapModeU = wrapModeU;
            finTexture.WrapModeV = wrapModeV;

            // TODO: How do we know when *not* to use the skin color?
            finMaterial.DiffuseColor =
                tstlt.AnotherHeader.SkinColor.ToSystemColor();

            return (finMaterial, finTexture);
          });

      foreach (var meshOffset in meshOffsets) {
        try {
          defaultBr.Position = meshOffset;
          AddMesh_(tstlt,
                   meshGroupOffset,
                   model,
                   defaultBr,
                   lazyVertices,
                   lazyMaterials,
                   n64Memory,
                   dlModelBuilder);
        } catch (Exception e) {
          ;
        }
      }
    }

    return model;
  }

  private static void AddMesh_(
      Tstlt tstlt,
      long meshGroupOffset,
      ModelImpl model,
      IBinaryReader br,
      ILazyDictionary<(uint, (int, int)?), IReadOnlyVertex> lazyVertices,
      ILazyDictionary<(uint, (int, int, WrapMode, WrapMode, N64ColorFormat)?), (
              IReadOnlyMaterial,
              ITexture)>
          lazyMaterials,
      N64Memory n64Memory,
      DlModelBuilder dlModelBuilder) {
    var baseOffset = br.Position;

    br.Position = baseOffset + 4 * 2;
    var imageSectionSize = br.ReadUInt32();
    var vertexSectionSize = br.ReadUInt32();
    var opcodeSectionSize = br.ReadUInt32();

    br.Position = baseOffset + 4 * 7;
    var imageSectionOffset = br.ReadUInt32();
    var vertexSectionOffset = br.ReadUInt32();
    var opcodeSectionOffset = br.ReadUInt32();

    br.Position = baseOffset + 4 * 14;
    var imageCount = br.ReadUInt16();
    var vertexCount = br.ReadUInt16();

    n64Memory.SetSegment(0xE,
                         (uint) (baseOffset + vertexSectionOffset),
                         (uint) vertexSectionSize);

    br.Position = baseOffset + imageSectionOffset;

    var singleImageSize = imageCount > 0 ? imageSectionSize / imageCount : 0;

    br.Position = baseOffset + opcodeSectionOffset;

    var displayList =
        new DisplayListReader().ReadDisplayList(
            n64Memory,
            new F3dzex2OpcodeParser(),
            (uint) (baseOffset + opcodeSectionOffset));

    //dlModelBuilder.AddDl(displayList);

    var parser = new SimpleF3dzex2OpcodeParser();
    var opcodes = br.Subread(
        opcodeSectionSize,
        () => {
          var opcodes = new LinkedList<IOpcodeCommand>();
          while (!br.Eof) {
            opcodes.AddLast(parser.Parse(br));
          }
          return opcodes;
        });

    var mesh = model.Skin.AddMesh();
    mesh.Name = baseOffset.ToHexString();

    var activeVertices = new IReadOnlyVertex[0x20];
    (IReadOnlyMaterial, ITexture)? currentTextureAndMaterial = null;
    SetTimgOpcodeCommand? timgOpcodeCommand = null;
    SetTileOpcodeCommand? tileOpcodeCommand = null;
    LoadBlockOpcodeCommand blockOpcodeCommand = null;
    foreach (var opcode in opcodes) {
      switch (opcode) {
        case SetTileOpcodeCommand setTileOpcodeCommand: {
          tileOpcodeCommand = setTileOpcodeCommand;
          break;
        }
        case SetTimgOpcodeCommand setTimgOpcodeCommand: {
          timgOpcodeCommand = setTimgOpcodeCommand;
          break;
        }
        case SetTileSizeOpcodeCommand setTileSizeOpcodeCommand: {
          var width = (int) (setTileSizeOpcodeCommand.Lrs + 1);
          var height = (int) (setTileSizeOpcodeCommand.Lrt + 1);

          var format = N64ColorFormat.L;

          var wrapModeU = tileOpcodeCommand.WrapModeS.AsFinWrapMode();
          var wrapModeV = tileOpcodeCommand.WrapModeT.AsFinWrapMode();

          currentTextureAndMaterial =
              lazyMaterials[
                  (timgOpcodeCommand.TextureSegmentedAddress,
                   (width, height, wrapModeU, wrapModeV, format))];
          break;
        }
        case LoadBlockOpcodeCommand loadBlockOpcodeCommand: {
          blockOpcodeCommand = loadBlockOpcodeCommand;
          break;
        }
        case SimpleVtxOpcodeCommand vtxOpcodeCommand: {
          var currentTexture = currentTextureAndMaterial?.Item2;

          var baseRamAddress = vtxOpcodeCommand.SegmentedAddress;
          var segment = baseRamAddress >> 24;
          switch (segment) {
            case 0xE: {
              var address = baseRamAddress & 0xFFFFFF;

              var vertexAddress = baseOffset + vertexSectionOffset;
              var ramVertexAddress =
                  vertexAddress - meshGroupOffset + 0x0f000000;

              baseRamAddress = (uint) (ramVertexAddress + address);
              break;
            }
            case 0xF: {
              break;
            }
            default:
              throw new NotSupportedException(
                  "Unsupported segment for vertex address: " + segment.ToHex());
          }

          for (var i = 0; i < vtxOpcodeCommand.NumVerticesToLoad; ++i) {
            activeVertices[vtxOpcodeCommand.IndexToBeginStoringVertices + i] =
                lazyVertices[(baseRamAddress + (uint) (i * 0x10),
                              currentTexture != null
                                  ? (currentTexture.Image.Width,
                                     currentTexture.Image.Height)
                                  : null)];
          }

          break;
        }
        case Tri1OpcodeCommand tri1OpcodeCommand: {
          var material = currentTextureAndMaterial?.Item1;

          var triangleVertices =
              tri1OpcodeCommand.VertexIndicesInOrder
                               .Select(i => activeVertices[i])
                               .ToArray();
          var triangle = mesh.AddTriangles(triangleVertices);
          triangle.SetMaterial(material);
          triangle.SetVertexOrder(VertexOrder.COUNTER_CLOCKWISE);

          break;
        }
        case Tri2OpcodeCommand tri2OpcodeCommand: {
          var material = currentTextureAndMaterial?.Item1;

          var triangle0Vertices =
              tri2OpcodeCommand.VertexIndicesInOrder0
                               .Select(i => activeVertices[i])
                               .ToArray();
          var triangle0 = mesh.AddTriangles(triangle0Vertices);
          triangle0.SetMaterial(material);
          triangle0.SetVertexOrder(VertexOrder.COUNTER_CLOCKWISE);

          var triangle1Vertices =
              tri2OpcodeCommand.VertexIndicesInOrder1
                               .Select(i => activeVertices[i])
                               .ToArray();
          var triangle1 = mesh.AddTriangles(triangle1Vertices);
          triangle1.SetMaterial(material);
          triangle1.SetVertexOrder(VertexOrder.COUNTER_CLOCKWISE);

          break;
        }
      }
    }
  }
}

[BinarySchema]
public partial class Joint : IBinaryDeserializable {
  public short parentIndex;
  public short unk5;
  public uint unk6;

  public byte index;
  public byte unk0;
  public ushort unk1;

  public ushort unk3;
  public byte previousIndex;
  public byte nextIndex;

  public Matrix4x4 matrix;

  [SequenceLengthSource(16)]
  public byte[] unk4;
}

// https://wiki.cloudmodding.com/oot/F3DZEX2#Vertex_Structure
[BinarySchema]
public partial class Vertex : IBinaryDeserializable {
  public Vector3s Position { get; } = new();

  private ushort padding_ = 0;

  public short U { get; set; }
  public short V { get; set; }

  [NumberFormat(SchemaNumberType.SN8)]
  public float NormalX { get; set; }

  [NumberFormat(SchemaNumberType.SN8)]
  public float NormalY { get; set; }

  [NumberFormat(SchemaNumberType.SN8)]
  public float NormalZ { get; set; }

  public byte Alpha { get; set; }
}