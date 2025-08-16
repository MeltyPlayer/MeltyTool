using System.Drawing;
using System.Numerics;

using Assimp.Unmanaged;

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
using fin.math.matrix.four;
using fin.math.rotations;
using fin.model;
using fin.model.io;
using fin.model.io.importers;
using fin.model.util;
using fin.schema.color;
using fin.schema.vector;
using fin.util.enumerables;
using fin.util.linq;
using fin.util.sets;

using marioartist.schema;

using schema.binary;
using schema.binary.attributes;

using SharpGLTF.Schema2;


namespace marioartist.api;

using ChosenPart0Tuple =
    (Segment segment, MeshDefinition meshDefinition, UnkSection5 unkSection5,
    ChosenPart0 chosenPart);
using ChosenPart1Tuple = (Segment segment, ChosenPart1 chosenPart, IBone bone);

public record TstltModelFileBundle(IReadOnlyTreeFile MainFile)
    : IModelFileBundle;

public enum JointIndex {
  // Head bones
  HEAD_ROOT = 0,
  HAIR = 2,
  NOSE = 5,

  EAR_0 = 6,
  EAR_1 = 7,

  EARRING_0 = 10,
  EARRING_1 = 11,

  // Body bones
  BODY_ROOT = 13,

  // Connects head bones to body
  NECK = 14,

  CLAVICLE = 15,

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
    var rdp = n64Hardware.Rdp = new Rdp {
        Tmem = new NoclipTmem(n64Hardware),
    };
    var rsp = n64Hardware.Rsp = new Rsp {
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
    var headChosenPart0s = br.ReadNews<ChosenPart0>(5);
    var headChosenPart0sById = headChosenPart0s.ToDictionary(p => p.Id);
    var headUnkSection5s = br.ReadNews<UnkSection5>(8);

    br.Position = 0xbd08;
    var bodyChosenPart0s = br.ReadNews<ChosenPart0>(8);
    var bodyChosenPart0sById = bodyChosenPart0s.ToDictionary(p => p.Id);
    var bodyUnkSection5s = br.ReadNews<UnkSection5>(19);

    br.Position = 0xc6c8;
    var headChosenPart1Count = br.ReadInt32();
    var headChosenPart1s = br.ReadNews<ChosenPart1>(headChosenPart1Count);

    br.Position = 0xeb00;
    var bodyChosenPart1Count = br.ReadInt32();
    var bodyChosenPart1s = br.ReadNews<ChosenPart1>(bodyChosenPart1Count);

    br.Position = 0xd530;
    var headMeshDefinitions = br.ReadNews<MeshDefinition>(0x20);

    br.Position = 0xf968;
    var bodyMeshDefinitions = br.ReadNews<MeshDefinition>(0x4C);

    var skinChosenPart = new ChosenPart0();
    skinChosenPart.ChosenColor0.Color = skinColor;

    var materialManager = model.MaterialManager;
    var thumbnailTexture =
        materialManager.CreateTexture(tstlt.Thumbnail.ToImage());
    thumbnailTexture.Name = "thumbnail";

    br.Position = headSectionOffset;
    var image2 = new Argb1555Image(32, 32 * 8);
    image2.Read(br);
    var image2Texture =
        materialManager.CreateTexture(image2.ToImage());
    image2Texture.Name = "palette";

    br.Position = 0xa934;
    var joints = br.ReadNews<Joint>(0x1F);
    foreach (var joint in joints) {
      joint.matrix = Matrix4x4.Transpose(joint.matrix);
    }

    var neckJoint = joints[(int) JointIndex.NECK];
    Matrix4x4.Decompose(neckJoint.matrix,
                        out var neckScale,
                        out _,
                        out var neckTranslation);
    var forwardRotation =
        QuaternionUtil.CreateZyxRadians(MathF.PI / 2, 0, MathF.PI / 2);
    var neckTMatrix = Matrix4x4.CreateTranslation(neckTranslation);

    var jointsByParent =
        new BidirectionalDictionary<Joint?, List<(Joint joint, int index)>>();
    for (var i = 0; i < joints.Length; ++i) {
      var joint = joints[i];

      if (i < 13) {
        Matrix4x4.Decompose(joint.matrix,
                            out var jointScale,
                            out _,
                            out var jointTranslation);

        // What is going on???
        switch ((JointIndex) i) {
          case JointIndex.NOSE: {
            jointTranslation = joint.matrix.Translation with {
                Z = 0
            };
            break;
          }
          // Doesn't always seem to be right...
          case JointIndex.HAIR: {
            jointTranslation = Vector3.Zero;
            break;
          }
        }

        var scaledJointMatrix =
            SystemMatrix4x4Util.FromTrs(jointTranslation * neckScale,
                                        forwardRotation,
                                        jointScale * neckScale);

        joint.matrix = scaledJointMatrix * neckTMatrix;
      }

      // Is this in the file somewhere???
      var jointIndex = (JointIndex) i;
      var parentIndex = (int) (jointIndex switch {
          JointIndex.HEAD_ROOT   => JointIndex.NECK,
          < JointIndex.BODY_ROOT => JointIndex.HEAD_ROOT,

          JointIndex.HIP      => JointIndex.BODY_ROOT,
          JointIndex.TORSO    => JointIndex.HIP,
          JointIndex.CLAVICLE => JointIndex.TORSO,

          JointIndex.NECK => JointIndex.TORSO,

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

    var finBonesAndJoints = new (IBone, Joint, int)[joints.Length];
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
      var worldMatrix = joint.matrix;
      var localMatrix = worldMatrix * invertedParentMatrix;

      var finBone = parentFinBone.AddChild(localMatrix);
      finBone.Name = $"{(JointIndex) index}: {index}";
      finBonesAndJoints[index] = (finBone, joint, index);

      if (jointsByParent.ContainsKey(joint)) {
        jointQueue.Enqueue(jointsByParent[joint]
                               .Select(childJoint => (
                                           childJoint, worldMatrix,
                                           bone: finBone)));
      }
    }

    var headChosenPart0Tuples = headMeshDefinitions.Select(meshDefinition => {
      var segment = headSegment;
      var unkSection5 = headUnkSection5s[meshDefinition.UnkSection5Index];
      var chosenPart =
          headChosenPart0sById.GetValueOrDefault(unkSection5.ChosenPartId,
                                                 skinChosenPart);
      return (segment, meshDefinition, unkSection5, chosenPart);
    });
    var bodyChosenPart0Tuples = bodyMeshDefinitions.Select(meshDefinition => {
      var segment = bodySegment;
      var unkSection5 = bodyUnkSection5s[meshDefinition.UnkSection5Index];
      var chosenPart =
          bodyChosenPart0sById.GetValueOrDefault(unkSection5.ChosenPartId,
                                                 skinChosenPart);
      return (segment, meshDefinition, unkSection5, chosenPart);
    });

    var chosenPart0TuplesByMeshSetId =
        new ListDictionary<uint, ChosenPart0Tuple>();
    foreach (var chosenPart0Tuple in headChosenPart0Tuples.Concat(
                 bodyChosenPart0Tuples)) {
      chosenPart0TuplesByMeshSetId.Add(
          chosenPart0Tuple.meshDefinition.MeshSetId,
          chosenPart0Tuple);
    }

    var headChosenPart1Tuples = headChosenPart1s.Select(chosenPart => {
      var segment = headSegment;
      return (segment, chosenPart,
              finBonesAndJoints[(uint) JointIndex.NECK].Item1);
    });
    var bodyChosenPart1Tuples = bodyChosenPart1s.Select(chosenPart => {
      var segment = bodySegment;
      return (segment, chosenPart,
              finBonesAndJoints[(uint) JointIndex.BODY_ROOT].Item1);
    });

    var chosenPart1TuplesByMeshSetId =
        new ListDictionary<uint, ChosenPart1Tuple>();
    foreach (var chosenPart1Tuple in headChosenPart1Tuples.Concat(
                 bodyChosenPart1Tuples)) {
      chosenPart1TuplesByMeshSetId.Add(
          chosenPart1Tuple.chosenPart.MeshSetId,
          chosenPart1Tuple);
    }

    foreach (var (bone, joint, jointIndex) in finBonesAndJoints) {
      var meshSetId = joint.MeshSetId;
      if (chosenPart0TuplesByMeshSetId.TryGetList(
              meshSetId,
              out var chosenPart0Tuples)) {
        var chosenPart0TuplesInOrder = jointIndex != (uint) JointIndex.TORSO
            ? chosenPart0Tuples.AsEnumerable()
            : chosenPart0Tuples.Reverse();

        var isFirst = true;

        foreach (var chosenPart0Tuple in chosenPart0TuplesInOrder) {
          if (TryToAddChosenPart0Tuple_(
                  model,
                  chosenPart0Tuple,
                  n64Hardware,
                  dlModelBuilder,
                  joint,
                  jointIndex,
                  isFirst,
                  bone.Parent!,
                  bone)) {
            isFirst = false;
          }
        }
      }

      if (chosenPart1TuplesByMeshSetId.TryGetList(
              meshSetId,
              out var chosenPart1Tuples)) {
        foreach (var chosenPart1Tuple in chosenPart1Tuples) {
          TryToAddChosenPart1Tuple_(
              model,
              chosenPart1Tuple,
              n64Hardware,
              dlModelBuilder,
              joint,
              chosenPart1Tuple.bone // bone
          );
        }
      }
    }

    // Adds face
    {
      n64Memory.SetSegment(0xF, headSegment);

      SetCombiner_(n64Hardware, true);

      var faceMeshes = new List<IMesh>();

      br.Position = 0xeae0;
      br.Position += 2 * 4;
      var faceDlSegmentedAddresses = br.ReadUInt32s(3);

      var headRootBone = finBonesAndJoints[(int) JointIndex.HEAD_ROOT].Item1;
      var noseBone = finBonesAndJoints[(int) JointIndex.NOSE].Item1;

      rsp.ActiveBoneWeights =
          model.Skin.GetOrCreateBoneWeights(VertexSpace.RELATIVE_TO_BONE,
                                            headRootBone);

      for (var i = 0; i < 3; ++i) {
        var offset = 0x4b0 + (uint) (i * 2 * 64 * 32);
        rdp.Tmem.SetImage(offset,
                          N64ColorFormat.RGBA,
                          BitsPerTexel._16BPT,
                          64,
                          32,
                          F3dWrapMode.CLAMP,
                          F3dWrapMode.CLAMP);

        var faceDlSegmentedAddress = faceDlSegmentedAddresses[i];
        var faceMesh =
            dlModelBuilder.StartNewMesh(
                $"face {i}/3: {faceDlSegmentedAddress.ToHexString()}");
        faceMeshes.Add(faceMesh);

        dlModelBuilder.AddDl(new DisplayListReader().ReadDisplayList(
                                 n64Hardware.Memory,
                                 new F3dzex2OpcodeParser(),
                                 faceDlSegmentedAddress));
      }

      br.Position += 8;
      var noseDlSegmentedAddress = br.ReadUInt32();
      rsp.ActiveBoneWeights =
          model.Skin.GetOrCreateBoneWeights(VertexSpace.RELATIVE_TO_BONE,
                                            noseBone);

      var noseMesh =
          dlModelBuilder.StartNewMesh(
              $"nose: {noseDlSegmentedAddress.ToHexString()}");
      faceMeshes.Add(noseMesh);
      dlModelBuilder.AddDl(new DisplayListReader().ReadDisplayList(
                               n64Hardware.Memory,  
                               new F3dzex2OpcodeParser(),
                               noseDlSegmentedAddress));

      foreach (var faceMesh in faceMeshes) {
        foreach (var p in faceMesh.Primitives) {
          p.SetVertexOrder(VertexOrder.COUNTER_CLOCKWISE);
        }
      }
    }

    return model;
  }

  private static bool TryToAddChosenPart0Tuple_(
      IModel model,
      ChosenPart0Tuple chosenPart0Tuple,
      N64Hardware<N64Memory> n64Hardware,
      DlModelBuilder dlModelBuilder,
      Joint joint,
      int jointIndex,
      bool isFirst,
      IBone parentBone,
      IBone childBone) {
    var (segment, meshDefinition, unkSection5, chosenPart0) =
        chosenPart0Tuple;

    n64Hardware.Memory.SetSegment(0xF, segment);

    var meshSegmentedAddress = meshDefinition.MeshSegmentedAddresses[0];
    if (meshSegmentedAddress == 0) {
      return false;
    }

    if (!n64Hardware.Memory.TryToOpenPossibilitiesAtSegmentedAddress(
            meshSegmentedAddress,
            out var possibilities)) {
      return false;
    }

    if (!possibilities.TryGetFirst(out var sbr)) {
      return false;
    }

    var meshBaseOffset = sbr.Position;
    if (sbr.ReadUInt32() != 0) {
      return false;
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
    if (primitiveDlSegmentedAddress == 0) {
      return false;
    }

    // TODO: Factor in skin color via prim or env color

    var color0 = chosenPart0.ChosenColor0.Color;
    SetCombiner_(n64Hardware, imageCount > 0, color0.ToSystemColor());

    var primitiveDlBoneWeights = model.Skin.GetOrCreateBoneWeights(
        VertexSpace.RELATIVE_TO_BONE,
        childBone);

    // What a fucking nightmare. Why does being first impact this????
    Matrix4x4? vertexMatrix = null;
    IBoneWeights vertexDlBoneWeights;
    if (!isFirst) {
      vertexDlBoneWeights = primitiveDlBoneWeights;
    } else if (!joint.isLeft) {
      vertexDlBoneWeights = model.Skin.GetOrCreateBoneWeights(
          VertexSpace.RELATIVE_TO_BONE,
          parentBone);
    } else {
      // Still need to handle this case... the left side gets mapped to the right
      vertexDlBoneWeights = model.Skin.GetOrCreateBoneWeights(
          VertexSpace.RELATIVE_TO_BONE,
          parentBone);

      /*var parentWorldMatrix = parentBone.GetWorldMatrix();

      vertexDlBoneWeights = model.Skin.GetOrCreateBoneWeights(
          VertexSpace.RELATIVE_TO_WORLD,
          parentBone);

      vertexMatrix = Matrix4x4.CreateScale(-1, 1, 1) * parentWorldMatrix;*/
    }

    (uint, Matrix4x4?, IBoneWeights)[] displayLists
        = vertexDlSegmentedAddress == 0
            ? [
                (primitiveDlSegmentedAddress, null, primitiveDlBoneWeights)
            ]
            : [
                (vertexDlSegmentedAddress, vertexMatrix, vertexDlBoneWeights),
                (primitiveDlSegmentedAddress, null, primitiveDlBoneWeights)
            ];

    return TryToAddDisplayLists_(
        model,
        segment,
        n64Hardware,
        dlModelBuilder,
        $"joint({(JointIndex) jointIndex}): chosenPart0({meshDefinition.MeshSetId}): {meshSegmentedAddress.ToHexString()}",
        joint.isLeft,
        displayLists);
  }

  private static void TryToAddChosenPart1Tuple_(
      IModel model,
      ChosenPart1Tuple chosenPart1Tuple,
      N64Hardware<N64Memory> n64Hardware,
      DlModelBuilder dlModelBuilder,
      Joint joint,
      IBone bone) {
    var (segment, chosenPart1, _) = chosenPart1Tuple;

    n64Hardware.Memory.SetSegment(0xF, segment);

    SetCombiner_(n64Hardware, true);

    var boneWeights
        = model.Skin.GetOrCreateBoneWeights(VertexSpace.RELATIVE_TO_BONE, bone);
    TryToAddDisplayLists_(
        model,
        segment,
        n64Hardware,
        dlModelBuilder,
        $"chosenPart1({chosenPart1.MeshSetId})",
        joint.isLeft,
        chosenPart1.DisplayListSegmentedAddresses
                   .Where(dlSegmentedAddress => dlSegmentedAddress != 0)
                   .Select(dlSegmentedAddress
                               => (dlSegmentedAddress, 
                                   (Matrix4x4?) null,
                                   boneWeights)));
  }

  private static bool TryToAddDisplayLists_(
      IModel model,
      Segment segment,
      N64Hardware<N64Memory> n64Hardware,
      DlModelBuilder dlModelBuilder,
      string meshName,
      bool isLeft,
      IEnumerable<(uint, Matrix4x4?, IBoneWeights)> displayListSegmentedOffsetAndBones) {
    n64Hardware.Memory.SetSegment(0xF, segment);

    var displayListReader = new DisplayListReader();
    var f3dzex2OpcodeParser = new F3dzex2OpcodeParser();
    var displayListTuples = displayListSegmentedOffsetAndBones.Select(t => {
          try {
            var displayList = displayListReader.ReadDisplayList(
                n64Hardware.Memory,
                f3dzex2OpcodeParser,
                t.Item1);
            return (displayList, t.Item2, t.Item3) as 
                (IDisplayList, Matrix4x4?, IBoneWeights)?;
          } catch (Exception e) {
            return null;
          }
        })
        .WhereNonnull()
        .Select(t => t.Value)
        .ToArray();

    if (!displayListTuples.Any()) {
      return false;
    }

    var mesh = dlModelBuilder.StartNewMesh(meshName);

    var rsp = n64Hardware.Rsp;
    foreach (var (displayList, matrix, boneWeights) in displayListTuples) {
      dlModelBuilder.Matrix = matrix ?? Matrix4x4.Identity;

      rsp.ActiveBoneWeights = boneWeights;

      try {
        dlModelBuilder.AddDl(displayList);
      } catch (Exception e) { }
    }

    dlModelBuilder.Matrix = Matrix4x4.Identity;

    foreach (var p in mesh.Primitives) {
      p.SetVertexOrder(isLeft
                           ? VertexOrder.CLOCKWISE
                           : VertexOrder.COUNTER_CLOCKWISE);
    }

    return true;
  }

  private static void SetCombiner_(
      IN64Hardware<N64Memory> n64Hardware,
      bool withTexture,
      Color? color = null) {
    var rdp = n64Hardware.Rdp;
    var rsp = n64Hardware.Rsp;

    rsp.UvType = N64UvType.STANDARD;
    rdp.Tmem.GsSpTexture(1,
                         1,
                         0,
                         TileDescriptorIndex.TX_LOADTILE,
                         withTexture
                             ? TileDescriptorState.ENABLED
                             : TileDescriptorState.DISABLED);

    switch ((withTexture, color)) {
      case (true, not null): {
        rdp.SetCombinerCycleParams(
            CombinerCycleParams.FromTexture0AndLightingAndPrimitive());
        rsp.PrimColor = color.Value;
        break;
      }
      case (true, null): {
        rdp.SetCombinerCycleParams(
            CombinerCycleParams.FromTexture0AndLighting());
        break;
      }
      default: {
        rdp.SetCombinerCycleParams(CombinerCycleParams.FromVertexColor());
        break;
      }
    }
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