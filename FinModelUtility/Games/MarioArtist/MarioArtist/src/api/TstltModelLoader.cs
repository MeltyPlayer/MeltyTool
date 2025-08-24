using System.Drawing;
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

using OneOf;

using schema.binary;
using schema.binary.attributes;


namespace marioartist.api;

using ChosenPart0Tuple =
    (Segment segment, MeshDefinition meshDefinition, SubUnkSection5 unkSection5,
    ChosenPart0 chosenPart, int unkSection5I, int subUnkSection5I);
using ChosenPart1Tuple
    = (Segment segment, ChosenPart1 chosenPart, IBone bone, bool isHead);

public record TstltModelFileBundle(IReadOnlyTreeFile MainFile)
    : IModelFileBundle;

public enum JointIndex {
  // Head bones
  HEAD_ROOT = 0,
  HAIR = 2,
  HAT = 3,
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
  public const int HARDCODED_MESH_SET_ID = -1;
  public const bool INCLUDE_FACE = true;

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

    var skinChosenPart = new ChosenPart0();
    skinChosenPart.ChosenColor0.Color = skinColor;

    br.Position = 0xb840;
    var headChosenPart0s = br.ReadNews<ChosenPart0>(5);
    var headChosenPart0sById = headChosenPart0s.ToDictionary(p => p.Id);
    var headUnkSection5s = br.ReadNews<UnkSection5>(8);

    br.Position = 0xbd08;
    var bodyChosenPart0s = br.ReadNews<ChosenPart0>(8);
    var bodyChosenPart0sById = bodyChosenPart0s.ToDictionary(p => p.Id);
    bodyChosenPart0sById[0] = skinChosenPart;
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

    var materialManager = model.MaterialManager;
    var thumbnailTexture =
        materialManager.CreateTexture(tstlt.Thumbnail.ToImage());
    thumbnailTexture.Name = "thumbnail";

    br.Position = headSectionOffset;
    var headPaletteImage = new Argb1555Image(32, 32 * 8);
    headPaletteImage.Read(br);
    var headPaletteTexture =
        materialManager.CreateTexture(headPaletteImage.ToImage());
    headPaletteTexture.Name = "headPalette";

    br.Position = bodySectionOffset;
    var bodyPaletteImage = new Argb1555Image(32, 32 * 16);
    bodyPaletteImage.Read(br);
    var bodyPaletteTexture =
        materialManager.CreateTexture(bodyPaletteImage.ToImage());
    bodyPaletteTexture.Name = "bodyPalette";

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
        // HACK: Fixes position of head meshes
        switch ((JointIndex) i) {
          case JointIndex.NOSE: {
            jointTranslation = joint.matrix.Translation with {
                Z = 0
            };
            break;
          }
          case JointIndex.HAIR:
          case JointIndex.HAT: {
            jointTranslation = Vector3.Zero;
            break;
          }
        }

        var scaledJointMatrix =
            SystemMatrix4x4Util.FromTrs(jointTranslation * neckScale,
                                        forwardRotation,
                                        neckScale);

        joint.matrix = scaledJointMatrix * neckTMatrix;
      }

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


    // Adds face
    var faceMeshes = new List<IMesh>();
    if (INCLUDE_FACE) {
      n64Memory.SetSegment(0xF, headSegment);

      SetCombiner_(n64Hardware, true, false);

      br.Position = 0xeae0;
      var nosePosition = br.ReadUInt32s(2);
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

      // Top of head (for bald characters)
      {
        var headDlSegmentedAddresses = br.ReadUInt32s(2);
        rdp.Tmem.SetImage(0x4b0,
                          N64ColorFormat.RGBA,
                          BitsPerTexel._16BPT,
                          64,
                          1,
                          F3dWrapMode.CLAMP,
                          F3dWrapMode.CLAMP);
        for (var i = 1; i >= 0; --i) {
          var headDlSegmentedAddress = headDlSegmentedAddresses[i];
          if (headDlSegmentedAddress == 0) {
            continue;
          }

          var headMesh =
              dlModelBuilder.StartNewMesh(
                  $"head {i}/2: {headDlSegmentedAddress.ToHexString()}");
          faceMeshes.Add(headMesh);

          dlModelBuilder.AddDl(new DisplayListReader().ReadDisplayList(
                                   n64Hardware.Memory,
                                   new F3dzex2OpcodeParser(),
                                   headDlSegmentedAddress));
        }
      }

      var noseDlSegmentedAddress = br.ReadUInt32();
      rsp.ActiveBoneWeights =
          model.Skin.GetOrCreateBoneWeights(VertexSpace.RELATIVE_TO_BONE,
                                            noseBone);

      var noseX = nosePosition[0];
      var noseY = nosePosition[1];
      // These values look super random, but they come from digging into what
      // the game does--it generates a display list like this at runtime.
      var noseUls = (ushort) (noseX - 15);
      var noseUlt = (ushort) (noseY - 22);
      var noseLrs = (ushort) (noseX + 16);
      var noseLrt = (ushort) (noseY + 9);

      rdp.Tmem.SetImage(0x4b0,
                        N64ColorFormat.RGBA,
                        BitsPerTexel._16BPT,
                        63,
                        noseUls,
                        noseUlt,
                        noseLrs,
                        noseLrt,
                        F3dWrapMode.CLAMP,
                        F3dWrapMode.CLAMP);

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

    var faceTextures = faceMeshes.SelectMany(m => m.Primitives)
                                 .SelectMany(p => p.Material?.Textures ?? [])
                                 .ToHashSet();

    var headChosenPart0Tuples = headUnkSection5s
        .Select((unkSection5, unkSection5I) => {
          var segment = headSegment;
          return (segment, unkSection5, headMeshDefinitions,
                  headChosenPart0sById, unkSection5I);
        });
    var bodyChosenPart0Tuples = bodyUnkSection5s
        .Select((unkSection5, unkSection5I) => {
          var segment = bodySegment;
          return (segment, unkSection5, bodyMeshDefinitions,
                  bodyChosenPart0sById, unkSection5I);
        });

    var headAndBodyChosenPart0Tuples =
        headChosenPart0Tuples
            .Concat(bodyChosenPart0Tuples)
            .SelectMany(t => {
              var (segment, unkSection5, meshDefinitions, chosenPart0sById,
                  unkSection5I) = t;

              return unkSection5
                     .Subs
                     .Select((subUnkSection5, subUnkSection5I) => {
                       var meshDefinitionI = 4 * unkSection5I + subUnkSection5I;
                       var meshDefinition = meshDefinitions[meshDefinitionI];

                       var chosenPart = chosenPart0sById.GetValueOrDefault(
                           subUnkSection5.ChosenPartId,
                           skinChosenPart);

                       return (segment, meshDefinition, subUnkSection5,
                               chosenPart, unkSection5I, subUnkSection5I);
                     })
                     .Where(t => t.subUnkSection5.IsEnabled)
                     .Reverse();
            });

    var chosenPart0TuplesByMeshSetId =
        new ListDictionary<uint, ChosenPart0Tuple>();
    foreach (var chosenPart0Tuple in headAndBodyChosenPart0Tuples) {
      chosenPart0TuplesByMeshSetId.Add(
          chosenPart0Tuple.meshDefinition.MeshSetId,
          chosenPart0Tuple);
    }

    var headChosenPart1Tuples = headChosenPart1s.Select(chosenPart => {
      var segment = headSegment;
      return (segment, chosenPart,
              finBonesAndJoints[(uint) JointIndex.NECK].Item1, true);
    });
    var bodyChosenPart1Tuples = bodyChosenPart1s.Select(chosenPart => {
      var segment = bodySegment;
      return (segment, chosenPart,
              finBonesAndJoints[(uint) JointIndex.BODY_ROOT].Item1, false);
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
        // TODO: This only sometimes works
        var chosenPart0TuplesInOrder =
            jointIndex is not (int) JointIndex.TORSO
                ? chosenPart0Tuples.AsEnumerable()
                : chosenPart0Tuples.Reverse();

        var firstChosenPart0Tuple = chosenPart0TuplesInOrder.First();
        foreach (var chosenPart0Tuple in chosenPart0TuplesInOrder) {
          var isFirst = chosenPart0Tuple.unkSection5I ==
                        firstChosenPart0Tuple.unkSection5I;

          TryToAddChosenPart0Tuple_(
              model,
              skinChosenPart,
              chosenPart0Tuple,
              n64Hardware,
              dlModelBuilder,
              joint,
              jointIndex,
              isFirst,
              bone.Parent!,
              bone);
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
              chosenPart1Tuple.bone,
              skinColor
          );
        }
      }
    }

    // HACK: Fixes textures so that they actually wrap. For some reason,
    // a lot of these are incorrectly set to clamp (e.g. hair).
    foreach (var texture in
             model.MaterialManager.Textures
                  .Where(t => !faceTextures.Contains(t))) {
      if (texture.WrapModeU == WrapMode.CLAMP) {
        texture.WrapModeU = WrapMode.REPEAT;
      }
      if (texture.WrapModeV == WrapMode.CLAMP) {
        texture.WrapModeV = WrapMode.REPEAT;
      }
    }

    return model;
  }

  private static void TryToAddChosenPart0Tuple_(
      IModel model,
      ChosenPart0 skinChosenPart,
      ChosenPart0Tuple chosenPart0Tuple,
      N64Hardware<N64Memory> n64Hardware,
      DlModelBuilder dlModelBuilder,
      Joint joint,
      int jointIndex,
      bool useParentBone,
      IBone parentBone,
      IBone childBone) {
    var (segment, meshDefinition, unkSection5, chosenPart0, unkSection5I,
            subUnkSection5I) =
        chosenPart0Tuple;

    if (HARDCODED_MESH_SET_ID != -1 &&
        meshDefinition.MeshSetId != HARDCODED_MESH_SET_ID) {
      return;
    }

    n64Hardware.Memory.SetSegment(0xF, segment);

    var tuples = meshDefinition.MeshSegmentedAddresses.Zip(
                                   meshDefinition
                                       .VertexDisplayListSegmentedAddresses,
                                   meshDefinition
                                       .PrimitiveDisplayListSegmentedAddresses)
                               .ToArray();

    var addedDisplayList = false;
    for (var i = 0; i < 4; ++i) {
      var (meshSegmentedAddress,
          vertexDlSegmentedAddress,
          primitiveDlSegmentedAddress) = tuples[i];
      if (primitiveDlSegmentedAddress == 0) {
        continue;
      }

      if (meshSegmentedAddress != 0 &&
          n64Hardware.Memory
                     .TryToOpenPossibilitiesAtSegmentedAddress(
                         meshSegmentedAddress,
                         out var possibilities) &&
          possibilities.TryGetFirst(out var sbr)) {
        var meshBaseOffset = sbr.Position;
        if (sbr.ReadUInt32() != 0) {
          continue;
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

        ResetRspAndRdp_(n64Hardware);

        var withTexture = imageCount > 0;
        if (chosenPart0 == skinChosenPart) {
          SetCombiner_(n64Hardware,
                       withTexture,
                       false,
                       OneOf<uint, Color>.FromT1(
                           chosenPart0.ChosenColor0.Color.ToSystemColor()));
        } else {
          SetCombiner_(n64Hardware,
                       withTexture,
                       false,
                       OneOf<uint, Color>.FromT0(
                           i == 1
                               ? chosenPart0.Pattern1SegmentedAddress
                               : chosenPart0.Pattern0SegmentedAddress));
        }
      } else {
        SetCombiner_(n64Hardware, true, false);
      }

      var primitiveDlBoneWeights = model.Skin.GetOrCreateBoneWeights(
          VertexSpace.RELATIVE_TO_BONE,
          childBone);

      // HACK: What a fucking nightmare. Why does being first impact this????
      Matrix4x4? vertexMatrix = null;
      IBoneWeights vertexDlBoneWeights;
      if (!useParentBone) {
        vertexDlBoneWeights = primitiveDlBoneWeights;
      } else if (!joint.isLeft ||
                 jointIndex is not ((int) JointIndex.UPPER_ARM_1
                                    or (int) JointIndex.UPPER_LEG_1)) {
        vertexDlBoneWeights = model.Skin.GetOrCreateBoneWeights(
            VertexSpace.RELATIVE_TO_BONE,
            parentBone);
      } else {
        // HACK: Flips shoulder/hip across the axis.
        vertexDlBoneWeights = model.Skin.GetOrCreateBoneWeights(
            VertexSpace.RELATIVE_TO_BONE,
            parentBone);

        vertexMatrix = Matrix4x4.CreateScale(1, -1, 1);
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

      TryToAddDisplayLists_(
          model,
          segment,
          n64Hardware,
          dlModelBuilder,
          $"joint({(JointIndex) jointIndex}): chosenPart0(meshSetId: {meshDefinition.MeshSetId}, unkSection5: {unkSection5I}, subUnkSection5: {subUnkSection5I}): {meshSegmentedAddress.ToHexString()}",
          joint.isLeft,
          displayLists);

      ResetRspAndRdp_(n64Hardware);
      dlModelBuilder.TransparentCutoff = .5f;
    }
  }

  private static void TryToAddChosenPart1Tuple_(
      IModel model,
      ChosenPart1Tuple chosenPart1Tuple,
      N64Hardware<N64Memory> n64Hardware,
      DlModelBuilder dlModelBuilder,
      Joint joint,
      IBone bone,
      Rgba32 skinColor) {
    var (segment, chosenPart1, _, isHead) = chosenPart1Tuple;

    if (HARDCODED_MESH_SET_ID != -1 &&
        chosenPart1.MeshSetId != HARDCODED_MESH_SET_ID) {
      return;
    }

    n64Hardware.Memory.SetSegment(0xF, segment);

    // TODO: Is this correct??
    var dlIndex = 3;
    var dlCount = 2;

    ResetRspAndRdp_(n64Hardware);

    switch (isHead, chosenPart1Tuple.chosenPart.MeshSetId) {
      // Ear
      case (true, 6): {
          // HACK: Hardcodes ear color to skin color.
          SetCombiner_(
              n64Hardware,
              true,
              false,
              OneOf<uint, Color>.FromT1(skinColor.ToSystemColor()));
          n64Hardware.Rsp.EnvironmentColor = Color.FromArgb(
            0xFF,
            (byte) (0xFF - skinColor.Rb),
            (byte) (0xFF - skinColor.Gb),
            (byte) (0xFF - skinColor.Bb));
        n64Hardware.Rsp.PrimColor = Color.FromArgb(0xD2, 0, 0, 0);
        break;
      }
      // Beard
      case (true, 7): {
        // HACK: Hardcodes pattern 7.
        SetCombiner_(
            n64Hardware,
            true,
            false,
            OneOf<uint, Color>.FromT0(0x0F003800));
        break;
      }
      // Glasses
      case (true, 8): {
        // HACK: Hardcodes cycle count to 2.
        n64Hardware.Rdp.CycleType = CycleType.TWO_CYCLE;
        break;
      }
    }

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
                   .Skip(dlIndex)
                   .Take(dlCount)
                   .Where(dlSegmentedAddress => dlSegmentedAddress != 0)
                   .Select(dlSegmentedAddress
                               => (dlSegmentedAddress, (Matrix4x4?) null,
                                   boneWeights)));

    ResetRspAndRdp_(n64Hardware);

    dlModelBuilder.TransparentCutoff = .5f;
  }

  private static void ResetRspAndRdp_(IN64Hardware n64Hardware) {
    // TODO: The way these are reset in the original game is actually really, really, really complicated
    var rsp = n64Hardware.Rsp;
    rsp.UvType = N64UvType.LINEAR;
    rsp.EnvironmentColor = Color.White;
    rsp.PrimColor = Color.White;

    var rdp = n64Hardware.Rdp;
    rdp.ForceBlending = false;
    rdp.P0 = BlenderPm.G_BL_CLR_MEM;
    rdp.A0 = BlenderA.G_BL_0;
    rdp.M0 = BlenderPm.G_BL_CLR_IN;
    rdp.B0 = BlenderB.G_BL_1;
    rdp.P1 = BlenderPm.G_BL_CLR_MEM;
    rdp.A1 = BlenderA.G_BL_0;
    rdp.M1 = BlenderPm.G_BL_CLR_IN;
    rdp.B1 = BlenderB.G_BL_1;
    rdp.CycleType = CycleType.TWO_CYCLE;
  }

  private static bool TryToAddDisplayLists_(
      IModel model,
      Segment segment,
      N64Hardware<N64Memory> n64Hardware,
      DlModelBuilder dlModelBuilder,
      string meshName,
      bool isLeft,
      IEnumerable<(uint, Matrix4x4?, IBoneWeights)>
          displayListSegmentedOffsetAndBones) {
    n64Hardware.Memory.SetSegment(0xF, segment);

    var displayListReader = new DisplayListReader();
    var f3dzex2OpcodeParser = new F3dzex2OpcodeParser();
    var displayListTuples = displayListSegmentedOffsetAndBones
                            .Select((t, i) => {
                              try {
                                var displayList
                                    = displayListReader.ReadDisplayList(
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
      } catch (Exception e) {
        ;
      }
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
      bool withTexture0,
      bool withAlpha,
      OneOf<uint, Color>? patternSegmentedOffsetOrColor = null) {
    var rdp = n64Hardware.Rdp;
    var rsp = n64Hardware.Rsp;

    rsp.UvType = N64UvType.STANDARD;
    rdp.Tmem.GsSpTexture(1,
                         1,
                         0,
                         TileDescriptorIndex.TX_LOADTILE,
                         withTexture0
                             ? TileDescriptorState.ENABLED
                             : TileDescriptorState.DISABLED);

    switch (withTexture0, patternSegmentedOffsetOrColor) {
      case (true, not null): {
        if (patternSegmentedOffsetOrColor.Value.TryPickT0(
                out var patternSegmentedOffset,
                out var color)) {
          rdp.SetCombinerCycleParams(
              CombinerCycleParams
                  .FromBlendingTexture0AndTexture1WithEnvColorAndShade(
                      withAlpha));
          rdp.Tmem.SetImage(patternSegmentedOffset,
                            N64ColorFormat.RGBA,
                            BitsPerTexel._16BPT,
                            32,
                            32,
                            F3dWrapMode.REPEAT,
                            F3dWrapMode.REPEAT,
                            1);
          rsp.EnvironmentColor = Color.FromArgb(0xff, 0xc8, 0xc8, 0xc8);
        } else {
          rdp.SetCombinerCycleParams(
              CombinerCycleParams
                  .FromTexture0AndLightingAndPrimitive(withAlpha));
          rsp.PrimColor = color;
        }

        break;
      }
      case (true, null): {
        rdp.SetCombinerCycleParams(
            CombinerCycleParams.FromTexture0AndShade(withAlpha));
        break;
      }
      default: {
        rdp.SetCombinerCycleParams(CombinerCycleParams.FromShade(withAlpha));
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