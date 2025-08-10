using System.Numerics;

using CommunityToolkit.Diagnostics;

using f3dzex2.combiner;
using f3dzex2.displaylist;
using f3dzex2.displaylist.opcodes;
using f3dzex2.displaylist.opcodes.f3dzex2;
using f3dzex2.image;
using f3dzex2.io;
using f3dzex2.model;

using fin.data.dictionaries;
using fin.io;
using fin.math.rotations;
using fin.model;
using fin.model.io;
using fin.model.io.importers;
using fin.model.util;
using fin.schema.vector;
using fin.util.enumerables;

using schema.binary;
using schema.binary.attributes;


namespace marioartist.schema;

public record TstltModelFileBundle(IReadOnlyTreeFile MainFile)
    : IModelFileBundle;

public partial class TstltModelLoader : IModelImporter<TstltModelFileBundle> {
  public IModel Import(TstltModelFileBundle fileBundle) {
    using var br = fileBundle.MainFile.OpenReadAsBinary(Endianness.BigEndian);
    var tstlt = br.ReadNew<Tstlt>();

    var n64Hardware = new N64Hardware<N64Memory>();
    n64Hardware.Rdp = new Rdp {
        Tmem = new NoclipTmem(n64Hardware),
    };
    n64Hardware.Rsp = new Rsp();
    var n64Memory = n64Hardware.Memory = new N64Memory(fileBundle.MainFile);
    n64Memory.SetSegment(0, 0, (uint) br.Length);

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

    br.Position = 0x16770;
    var image2 = new Argb1555Image(32, 32 * 8);
    image2.Read(br);
    var image2Texture =
        materialManager.CreateTexture(image2.ToImage());
    image2Texture.Name = "palette";

    br.Position = 0x49C;
    var headSectionLength = br.ReadUInt32();

    br.Position = 0xa938;

    var joints = br.ReadNews<Joint>(0x1F);
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
            0xc6c8,
            0xd530,
            0x20
        ),
        (
            bodySectionOffset,
            br.Length - bodySectionOffset,
            0xeb00,
            0xf968,
            0x4C
        )
    };

    foreach (var (meshGroupOffset,
                 meshGroupLength,
                 meshDefinitions0Offset,
                 meshDefinitions1Offset,
                 meshDefinition1Count) in
             meshGroupOffsets) {
      n64Memory.SetSegment(0xF, (uint) meshGroupOffset, (uint) meshGroupLength);

      br.Position = meshDefinitions0Offset;
      var meshDefinition0Count = br.ReadUInt32();

      for (var i = 0; i < meshDefinition0Count; ++i) {
        br.Position = meshDefinitions0Offset + 4 + i * 0xb8;

        try {
          TryToAddMeshDefinition0_((uint) meshGroupOffset,
                                   br,
                                   n64Hardware,
                                   dlModelBuilder);
        } catch (Exception e) {
          ;
        }
      }

      for (var i = 0; i < meshDefinition1Count; ++i) {
        br.Position = meshDefinitions1Offset + i * 0xa8;

        try {
          TryToAddMeshDefinition1_((uint) meshGroupOffset,
                                   br,
                                   n64Hardware,
                                   dlModelBuilder);
        } catch (Exception e) {
          ;
        }
      }
    }

    return model;
  }

  private static void TryToAddMeshDefinition0_(
      uint meshGroupOffset,
      IBinaryReader br,
      N64Hardware<N64Memory> n64Hardware,
      DlModelBuilder dlModelBuilder) { }

  private static void TryToAddMeshDefinition1_(
      uint meshGroupOffset,
      IBinaryReader br,
      N64Hardware<N64Memory> n64Hardware,
      DlModelBuilder dlModelBuilder) {
    /*var meshSegmentedAddress = br.ReadUInt32();
    if (meshSegmentedAddress != 0) {
      using var sbr =
          n64Hardware.Memory.OpenAtSegmentedAddress(
              meshSegmentedAddress);

      var baseOffset = sbr.Position;
      if (sbr.ReadUInt32() == 0) {
        dlModelBuilder.StartNewMesh(meshSegmentedAddress.ToHexString());

        sbr.Position = baseOffset;
        AddMesh_(meshGroupOffset, sbr, n64Hardware, dlModelBuilder);
      }
    }*/

    var baseOffset = br.Position;

    var meshSegmentedAddress = br.ReadUInt32();
    if (meshSegmentedAddress == 0) {
      return;
    }

    if (!n64Hardware.Memory.TryToOpenPossibilitiesAtSegmentedAddress(
            meshSegmentedAddress,
            out var possibilities)) {
      return;
    }

    var sbr = possibilities.First();

    var meshBaseOffset = sbr.Position;
    if (sbr.ReadUInt32() != 0) {
      return;
    }

    sbr.Position = meshBaseOffset;

    sbr.Position = meshBaseOffset + 4 * 2;
    var imageSectionSize = sbr.ReadUInt32();
    var vertexSectionSize = sbr.ReadUInt32();

    sbr.Position = meshBaseOffset + 4 * 7;
    var imageSectionOffset = sbr.ReadUInt32();
    var vertexSectionOffset = sbr.ReadUInt32();

    sbr.Position = meshBaseOffset + 4 * 14;
    var imageCount = sbr.ReadUInt16();

    n64Hardware.Memory.SetSegment(0xE,
                                  (uint) (meshGroupOffset + meshBaseOffset +
                                          vertexSectionOffset),
                                  (uint) vertexSectionSize);

    br.Position = baseOffset + 0x74;
    var vertexDlSegmentedAddress = br.ReadUInt32();

    br.Position = baseOffset + 0x40;
    var primitiveDlSegmentedAddress = br.ReadUInt32();

    // TODO: What does it mean if only primitive DL is present?
    if (vertexDlSegmentedAddress == 0 ||
        primitiveDlSegmentedAddress == 0) {
      return;
    }

    // TODO: Factor in skin color via prim or env color

    if (imageCount > 0) {
      n64Hardware.Rdp.CombinerCycleParams0 =
          CombinerCycleParams.FromTexture0AndVertexColor();
      n64Hardware.Rdp.Tmem.GsSpTexture(1,
                                       1,
                                       0,
                                       TileDescriptorIndex.TX_LOADTILE,
                                       TileDescriptorState.ENABLED);
    } else {
      n64Hardware.Rdp.CombinerCycleParams0 =
          CombinerCycleParams.FromVertexColor();
      n64Hardware.Rdp.Tmem.GsSpTexture(1,
                                       1,
                                       0,
                                       TileDescriptorIndex.TX_LOADTILE,
                                       TileDescriptorState.DISABLED);
    }

    var vertexDl =
        new DisplayListReader().ReadDisplayList(
            n64Hardware.Memory,
            new F3dzex2OpcodeParser(),
            vertexDlSegmentedAddress);
    dlModelBuilder.AddDl(vertexDl);

    var primitiveDl =
        new DisplayListReader().ReadDisplayList(
            n64Hardware.Memory,
            new F3dzex2OpcodeParser(),
            primitiveDlSegmentedAddress);
    dlModelBuilder.StartNewMesh(
        primitiveDlSegmentedAddress.ToHexString());
    dlModelBuilder.AddDl(primitiveDl);
  }

  private static void AddMesh_(
      uint meshGroupOffset,
      IBinaryReader br,
      N64Hardware<N64Memory> n64Hardware,
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

    n64Hardware.Memory.SetSegment(0xE,
                                  (uint) (meshGroupOffset + baseOffset +
                                          vertexSectionOffset),
                                  (uint) vertexSectionSize);

    // TODO: Factor in skin color via prim or env color

    if (imageCount > 0) {
      n64Hardware.Rdp.CombinerCycleParams0 =
          CombinerCycleParams.FromTexture0AndVertexColor();
      n64Hardware.Rdp.Tmem.GsSpTexture(1,
                                       1,
                                       0,
                                       TileDescriptorIndex.TX_LOADTILE,
                                       TileDescriptorState.ENABLED);
    } else {
      n64Hardware.Rdp.CombinerCycleParams0 =
          CombinerCycleParams.FromVertexColor();
      n64Hardware.Rdp.Tmem.GsSpTexture(1,
                                       1,
                                       0,
                                       TileDescriptorIndex.TX_LOADTILE,
                                       TileDescriptorState.DISABLED);
    }

    br.Position = baseOffset + opcodeSectionOffset;
    var parser = new SimpleF3dzex2OpcodeParser();
    var rawOpcodes = br.Subread(
        opcodeSectionSize,
        () => {
          var opcodes = new LinkedList<IOpcodeCommand>();
          while (!br.Eof) {
            opcodes.AddLast(parser.Parse(br));
          }
          return opcodes;
        });

    var processedOpcodes =
        rawOpcodes.UpToFirstMatchExclusive(o => o is SimpleDlOpcodeCommand)
                  .Where(o => o is not EndDlOpcodeCommand)
                  .Select(o => {
                    if (o is SimpleVtxOpcodeCommand simpleVtxOpcodeCommand) {
                      var numVerticesToLoad =
                          simpleVtxOpcodeCommand.NumVerticesToLoad;
                      var indexToBeginStoringVertices = simpleVtxOpcodeCommand
                          .IndexToBeginStoringVertices;

                      using var sbr =
                          n64Hardware.Memory.OpenAtSegmentedAddress(
                              simpleVtxOpcodeCommand.SegmentedAddress);
                      return new VtxOpcodeCommand {
                          IndexToBeginStoringVertices =
                              indexToBeginStoringVertices,
                          Vertices =
                              sbr.ReadNews<F3dVertex>(numVerticesToLoad),
                      };
                    }

                    return o;
                  });

    var displayList = new DisplayList {
        OpcodeCommands = processedOpcodes.ToArray(),
        Type = DisplayListType.F3DZEX2
    };

    dlModelBuilder.AddDl(displayList);
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