using System.Numerics;

using CommunityToolkit.Diagnostics;

using f3dzex2.combiner;
using f3dzex2.displaylist;
using f3dzex2.displaylist.opcodes;
using f3dzex2.displaylist.opcodes.f3dzex2;
using f3dzex2.image;
using f3dzex2.io;
using f3dzex2.model;

using fin.color;
using fin.data.dictionaries;
using fin.data.queues;
using fin.io;
using fin.model;
using fin.model.io;
using fin.model.io.importers;
using fin.model.util;
using fin.schema.color;
using fin.schema.vector;
using fin.util.linq;
using fin.util.sets;

using marioartist.schema;

using schema.binary;
using schema.binary.attributes;


namespace marioartist.api;

using MeshDefinitionTuple =
    (Segment segment, MeshDefinition meshDefinition, UnkSection5 unkSection5,
    ChosenPart chosenPart);

public record TstltModelFileBundle(IReadOnlyTreeFile MainFile)
    : IModelFileBundle;

public enum JointIndex {
  HEAD = 15,

  TORSO = 16,
  HIP = 17,

  UPPER_ARM_0 = 18,
  UPPER_ARM_1 = 19,

  FOREARM_0 = 20,
  FOREARM_1 = 21,

  HAND_0 = 22,
  HAND_1 = 23,

  UPPER_LEG_0 = 24,
  UPPER_LEG_1 = 25,

  LOWER_LEG_0 = 26,
  LOWER_LEG_1 = 27,

  FOOT_0 = 28,
  FOOT_1 = 29,
}

public partial class TstltModelLoader : IModelImporter<TstltModelFileBundle> {
  public IModel Import(TstltModelFileBundle fileBundle) {
    using var br = fileBundle.MainFile.OpenReadAsBinary(Endianness.BigEndian);
    var tstlt = br.ReadNew<Tstlt>();

    var n64Hardware = new N64Hardware<N64Memory>();
    n64Hardware.Rdp = new Rdp {
        Tmem = new NoclipTmem(n64Hardware),
    };
    n64Hardware.Rsp = new Rsp {
        GeometryMode = GeometryMode.G_LIGHTING
    };
    var n64Memory = n64Hardware.Memory = new N64Memory(fileBundle.MainFile);
    n64Memory.SetSegment(0, 0, (uint) br.Length);

    var dlModelBuilder =
        new DlModelBuilder(n64Hardware,
                           fileBundle,
                           fileBundle.MainFile.AsFileSet());

    var model = dlModelBuilder.Model;
    var headSectionOffset = 0x16770;
    br.Position = 0x49C;
    var headSectionLength = br.ReadUInt32();
    var bodySectionOffset = headSectionOffset + headSectionLength;
    var bodySectionLength = br.Length - bodySectionOffset;

    var headSegment = new Segment {
        Offset = (uint) headSectionOffset, Length = headSectionLength
    };
    var bodySegment = new Segment {
        Offset = (uint) bodySectionOffset, Length = (uint) bodySectionLength
    };

    br.Position = 0x91bc;
    var skinColor = br.ReadNew<Rgba32>();

    br.Position = 0xb840;
    var headChosenParts = br.ReadNews<ChosenPart>(5);
    var headChosenPartsById = headChosenParts.ToDictionary(p => p.Id);
    var headUnkSection5s = br.ReadNews<UnkSection5>(8);

    br.Position = 0xbd08;
    var bodyChosenParts = br.ReadNews<ChosenPart>(8);
    var bodyChosenPartsById = bodyChosenParts.ToDictionary(p => p.Id);
    var bodyUnkSection5s = br.ReadNews<UnkSection5>(19);

    br.Position = 0xd530;
    var headMeshDefinitions = br.ReadNews<MeshDefinition>(0x20);

    br.Position = 0xf968;
    var bodyMeshDefinitions = br.ReadNews<MeshDefinition>(0x4C);

    var skinChosenPart = new ChosenPart();
    skinChosenPart.ChosenColor0.Color = skinColor;

    var headBundles = headMeshDefinitions.Select(meshDefinition => {
      var segment = headSegment;
      var unkSection5 = headUnkSection5s[meshDefinition.UnkSection5Index];
      var chosenPart =
          headChosenPartsById.GetValueOrDefault(unkSection5.ChosenPartId,
                                                skinChosenPart);
      return (segment, meshDefinition, unkSection5, chosenPart);
    });
    var bodyBundles = bodyMeshDefinitions.Select(meshDefinition => {
      var segment = bodySegment;
      var unkSection5 = bodyUnkSection5s[meshDefinition.UnkSection5Index];
      var chosenPart =
          bodyChosenPartsById.GetValueOrDefault(unkSection5.ChosenPartId,
                                                skinChosenPart);
      return (segment, meshDefinition, unkSection5, chosenPart);
    });

    var meshDefinitionTuplesByMeshSetId =
        new SetDictionary<uint, (Segment, MeshDefinition, UnkSection5,
            ChosenPart)>();
    foreach (var tuple in headBundles.Concat(bodyBundles)) {
      meshDefinitionTuplesByMeshSetId.Add(tuple.meshDefinition.MeshSetId,
                                          tuple);
    }

    var materialManager = model.MaterialManager;
    var thumbnailTexture =
        materialManager.CreateTexture(tstlt.Thumbnail.ToImage());
    thumbnailTexture.Name = "thumbnail";

    var faceTextures =
        materialManager.CreateTexture(tstlt.FaceTextures.ToImage());
    faceTextures.Name = "face";

    br.Position = headSectionOffset;
    var image2 = new Argb1555Image(32, 32 * 8);
    image2.Read(br);
    var image2Texture =
        materialManager.CreateTexture(image2.ToImage());
    image2Texture.Name = "palette";

    br.Position = 0xa934;
    var joints = br.ReadNews<Joint>(0x1F);
    var jointsByParent =
        new BidirectionalDictionary<Joint?, List<(Joint joint, int index)>>();
    for (var i = 0; i < joints.Length; ++i) {
      var joint = joints[i];

      // Is this in the file somewhere???
      var jointIndex = (JointIndex) i;
      var parentIndex = (int) (jointIndex switch {
          JointIndex.UPPER_LEG_0 => JointIndex.HIP,
          JointIndex.UPPER_LEG_1 => JointIndex.HIP,

          JointIndex.LOWER_LEG_0 => JointIndex.UPPER_LEG_0,
          JointIndex.LOWER_LEG_1 => JointIndex.UPPER_LEG_1,

          JointIndex.FOOT_0 => JointIndex.LOWER_LEG_0,
          JointIndex.FOOT_1 => JointIndex.LOWER_LEG_1,

          JointIndex.UPPER_ARM_0 => JointIndex.TORSO,
          JointIndex.UPPER_ARM_1 => JointIndex.TORSO,

          JointIndex.FOREARM_0 => JointIndex.UPPER_ARM_0,
          JointIndex.FOREARM_1 => JointIndex.UPPER_ARM_1,

          JointIndex.HAND_0 => JointIndex.FOREARM_0,
          JointIndex.HAND_1 => JointIndex.FOREARM_1,

          _ => (JointIndex) (-1),
      });

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

    var finBonesAndWorldMatrices = new IBone[joints.Length];
    var jointQueue =
        new FinTuple3Queue<(Joint joint, int index), Matrix4x4, IBone>(
            jointsByParent[(Joint?) null]
                .Select(rootJoint => (rootJoint, Matrix4x4.Identity,
                                      model.Skeleton.Root)));
    while (jointQueue.TryDequeue(out var jointAndIndex,
                                 out var parentMatrix,
                                 out var parentFinBone)) {
      Matrix4x4.Invert(parentMatrix, out var invertedParentMatrix);

      var (joint, index) = jointAndIndex;
      var worldMatrix = Matrix4x4.Transpose(joint.matrix);
      var localMatrix = worldMatrix * invertedParentMatrix;

      var finBone = parentFinBone.AddChild(localMatrix);
      finBone.Name = $"bone {index}";
      finBonesAndWorldMatrices[index] = finBone;

      var meshSetId = joint.MeshSetId;
      if (meshDefinitionTuplesByMeshSetId.TryGetSet(
              meshSetId,
              out var meshDefinitionTuples)) {
        foreach (var meshDefinitionTuple in meshDefinitionTuples) {
          var mesh = TryToAddMeshDefinition_(model,
                                             meshDefinitionTuple,
                                             n64Hardware,
                                             dlModelBuilder,
                                             parentFinBone,
                                             finBone);
          if (mesh == null) {
            continue;
          }

          foreach (var p in mesh.Primitives) {
            p.SetVertexOrder(joint.isLeft
                                 ? VertexOrder.CLOCKWISE
                                 : VertexOrder.COUNTER_CLOCKWISE);
          }
        }
      }

      if (jointsByParent.ContainsKey(joint)) {
        jointQueue.Enqueue(jointsByParent[joint]
                               .Select(childJoint => (
                                           childJoint, worldMatrix,
                                           bone: finBone)));
      }
    }

    // Adds face
    {
      n64Memory.SetSegment(0xF, headSegment);

      n64Hardware.Rdp.CombinerCycleParams0 =
          CombinerCycleParams.FromTexture0AndVertexColor();
      n64Hardware.Rdp.Tmem.GsSpTexture(1,
                                       1,
                                       0,
                                       TileDescriptorIndex.TX_LOADTILE,
                                       TileDescriptorState.ENABLED);

      dlModelBuilder.StartNewMesh("face");

      br.Position = 0xeae0;
      br.Position += 4 * 2; // Nose position
      foreach (var dlSegmentedAddress in br.ReadUInt32s(6)) {
        if (dlSegmentedAddress == 0) {
          continue;
        }

        try {
          var dl = new DisplayListReader().ReadDisplayList(
              n64Hardware.Memory,
              new F3dzex2OpcodeParser(),
              dlSegmentedAddress);
          dlModelBuilder.AddDl(dl);
        } catch (Exception ex) {
          ;
        }
      }
    }

    return model;
  }

  private static IMesh? TryToAddMeshDefinition_(
      IModel model,
      MeshDefinitionTuple meshDefinitionTuple,
      N64Hardware<N64Memory> n64Hardware,
      DlModelBuilder dlModelBuilder,
      IBone parentBone,
      IBone childBone) {
    var (segment, meshDefinition, unkSection5, chosenPart) =
        meshDefinitionTuple;

    n64Hardware.Memory.SetSegment(0xF, segment);

    var meshSegmentedAddress = meshDefinition.MeshSegmentedAddresses[0];
    if (meshSegmentedAddress == 0) {
      return null;
    }

    if (!n64Hardware.Memory.TryToOpenPossibilitiesAtSegmentedAddress(
            meshSegmentedAddress,
            out var possibilities)) {
      return null;
    }

    if (!possibilities.TryGetFirst(out var sbr)) {
      return null;
    }

    var meshBaseOffset = sbr.Position;
    if (sbr.ReadUInt32() != 0) {
      return null;
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

    n64Hardware.Memory.SetSegment(
        0xE,
        (uint) (segment.Offset + meshBaseOffset + vertexSectionOffset),
        (uint) vertexSectionSize);

    var vertexDlSegmentedAddress =
        meshDefinition.VertexDisplayListSegmentedAddress;
    var primitiveDlSegmentedAddress =
        meshDefinition.PrimitiveDisplayListSegmentedAddress;

    // TODO: What does it mean if only primitive DL is present?
    if (vertexDlSegmentedAddress == 0 ||
        primitiveDlSegmentedAddress == 0) {
      return null;
    }

    // TODO: Factor in skin color via prim or env color

    var color0 = chosenPart.ChosenColor0.Color;

    var rsp = n64Hardware.Rsp;
    var rdp = n64Hardware.Rdp;
    if (imageCount > 0) {
      rdp.CycleType = CycleType.TWO_CYCLE;
      (rdp.CombinerCycleParams0, rdp.CombinerCycleParams1) =
          CombinerCycleParams.FromTexture0AndLightingAndPrimitive();
      n64Hardware.Rsp.PrimColor = color0.ToSystemColor();
      rdp.Tmem.GsSpTexture(1,
                           1,
                           0,
                           TileDescriptorIndex.TX_LOADTILE,
                           TileDescriptorState.ENABLED);
    } else {
      rdp.CycleType = CycleType.ONE_CYCLE;
      rdp.CombinerCycleParams0 = CombinerCycleParams.FromVertexColor();
      rdp.Tmem.GsSpTexture(1,
                           1,
                           0,
                           TileDescriptorIndex.TX_LOADTILE,
                           TileDescriptorState.DISABLED);
    }

    IDisplayList vertexDl;
    rsp.ActiveBoneWeights =
        model.Skin.GetOrCreateBoneWeights(VertexSpace.RELATIVE_TO_BONE,
                                          childBone);
    try {
      vertexDl =
          new DisplayListReader().ReadDisplayList(
              n64Hardware.Memory,
              new F3dzex2OpcodeParser(),
              vertexDlSegmentedAddress);
      dlModelBuilder.AddDl(vertexDl);
    } catch (Exception e) {
      return null;
    }

    rsp.ActiveBoneWeights =
        model.Skin.GetOrCreateBoneWeights(VertexSpace.RELATIVE_TO_BONE,
                                          childBone);
    var primitiveDl =
        new DisplayListReader().ReadDisplayList(
            n64Hardware.Memory,
            new F3dzex2OpcodeParser(),
            primitiveDlSegmentedAddress);
    var mesh = dlModelBuilder.StartNewMesh(
        primitiveDlSegmentedAddress.ToHexString());
    dlModelBuilder.AddDl(primitiveDl);

    if (meshDefinition.MeshSetId == 0x18) {
      ;
    }

    return mesh;
  }
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