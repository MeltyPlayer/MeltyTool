using System.Numerics;

using fin.animation.keyframes;
using fin.data.dictionaries;
using fin.data.queues;
using fin.image;
using fin.io;
using fin.math;
using fin.math.rotations;
using fin.model;
using fin.model.impl;
using fin.model.io;
using fin.model.io.importers;
using fin.model.util;
using fin.util.enums;

using gdl.schema.anim;
using gdl.schema.objects;

using schema.binary;

namespace gdl.api;

public sealed class GauntletDarkLegacyModelFileBundle
    : IModelFileBundle<GauntletDarkLegacyModelFileBundle,
        GauntletDarkLegacyModelImporter> {
  public required IReadOnlyTreeFile ObjectsFile { get; init; }
  public required IReadOnlyTreeFile AnimFile { get; init; }
  public required IReadOnlyTreeFile TexturesFile { get; init; }

  public IReadOnlyTreeFile MainFile => this.ObjectsFile;

  public bool UseSubMeshes { get; init; }
  public bool IncludeMeshAnimations { get; init; } = true;
}

public sealed class GauntletDarkLegacyModelImporter
    : IModelImporter<GauntletDarkLegacyModelImporter,
        GauntletDarkLegacyModelFileBundle> {
  public IModel Import(GauntletDarkLegacyModelFileBundle fileBundle) {
    var objects = fileBundle.ObjectsFile.ReadNew<Objects>();
    var anim = fileBundle.AnimFile.ReadNew<Anim>();

    return ImportImpl(
        fileBundle,
        new HashSet<IReadOnlyStandaloneFile>([
            fileBundle.ObjectsFile,
            fileBundle.AnimFile,
            fileBundle.TexturesFile,
        ]),
        objects,
        anim,
        fileBundle.TexturesFile,
        new Dictionary<int, IReadOnlyImage>(),
        null,
        default);
  }

  public static IModel ImportImpl(
      GauntletDarkLegacyModelFileBundle fileBundle,
      IReadOnlySet<IReadOnlyStandaloneFile> files,
      Objects objects,
      Anim anim,
      IReadOnlyStandaloneFile texturesFile,
      Dictionary<int, IReadOnlyImage> textureImageCache,
      string? rootObjectName,
      MbFlags worldObjectMbFlags) {
    var finModel = new ModelImpl {
        FileBundle = fileBundle,
        Files = files,
    };

    using var textureBr = texturesFile.OpenReadAsBinary(Endianness.BigEndian);
    var lazyFinMaterials =
        new LazyGdlMaterials(finModel, textureImageCache, textureBr, objects);

    var gdlNodeAndFinBonesByMeshNamePart
        = new Dictionary<string, (ANodeInfo, IReadOnlyBone)>();

    IBone? weaponBone = null;
    var finBones = new List<IReadOnlyBone>();
    foreach (var gdlSkeleton in anim.Atrees) {
      var gdlBones = gdlSkeleton.Data.ANodeInfos;

      var gdlBonesByParent = new ListDictionary<ANodeInfo?, ANodeInfo>();
      foreach (var gdlBone in gdlBones) {
        gdlBonesByParent.Add(
            gdlBone.ParentId == -1 ? null : gdlBones[gdlBone.ParentId],
            gdlBone);
      }

      var boneQueue = new FinTuple2Queue<ANodeInfo, IBone>(
          gdlBonesByParent[null]
              .Select(rootGdlBone => (rootGdlBone, finModel.Skeleton.Root)));
      var finBoneByGdlBone = new Dictionary<ANodeInfo, IReadOnlyBone>();

      while (boneQueue.TryDequeue(out var gdlANodeInfo,
                                  out var finParentBone)) {
        var initialPosition = gdlANodeInfo.InitialPosition;

        // For some inexplicable reason, the meshes are mirrored.
        initialPosition.X *= -1;

        var worldMatrix = Matrix4x4.CreateTranslation(initialPosition);

        var finBone = finParentBone.AddChild(worldMatrix);
        finBone.Name = gdlANodeInfo.MbDesc;

        finBones.Add(finBone);
        finBoneByGdlBone[gdlANodeInfo] = finBone;

        // Fuck, man. There are way easier ways to do this... why the fuck did
        // Midway not use integer IDs to link bones/meshes??????
        if (gdlANodeInfo.Type is AnimType.OBJ_ANIM) {
          if (gdlSkeleton.Data.ObjectAnimationHeader
                         .ObjectAnimationSequencesByBone.TryGetList(
                             gdlANodeInfo,
                             out var objectAnimations) &&
              objectAnimations.Count > 0) {
            foreach (var objectAnimation in objectAnimations) {
              SplitObjAnimMeshName_(
                  objectAnimation.Name,
                  out var meshNamePrefix,
                  out _,
                  out _);
              gdlNodeAndFinBonesByMeshNamePart[meshNamePrefix]
                  = (gdlANodeInfo, finBone);
            }
          }
        } else {
          gdlNodeAndFinBonesByMeshNamePart[gdlANodeInfo.MbDesc]
              = (gdlANodeInfo, finBone);
        }

        SetFaceTowardsCamera(finBone, gdlANodeInfo.MbFlags);

        // HACK: Need to manually attach the bone to the right wrist.
        // This is fucking stupid. Why is the weapon's parent bone not set?
        if (finBone.Name is "R_WRIST") {
          weaponBone = finBone.AddChild(Vector3.Zero);
          weaponBone.Name = "WEAPON";
        }

        if (gdlBonesByParent.TryGetList(gdlANodeInfo, out var childGdlBones)) {
          boneQueue.Enqueue(
              childGdlBones.Select(childGdlBone => (childGdlBone, finBone)));
        }
      }

      var gdlAnimationData = gdlSkeleton.Data.SkeletalAnimationHeader;
      var gdlAnimationHeaders = gdlSkeleton.Data.ATreeSequences;

      var logicalAnimations = new List<(string name, IEnumerable<int>)>();
      var gdlAnimationHeaderIndicesByName = new Dictionary<string, int>();
      for (var i = 0; i < gdlAnimationHeaders.Length; ++i) {
        gdlAnimationHeaderIndicesByName[gdlAnimationHeaders[i].Name] = i;

        logicalAnimations.Add((gdlAnimationHeaders[i].Name, [i]));
      }

      // HACK: Combine animations that swap back and forth
      {
        if (gdlAnimationHeaderIndicesByName.TryGetValue(
                "WALK1",
                out var walk1I) &&
            gdlAnimationHeaderIndicesByName.TryGetValue(
                "WALK2",
                out var walk2I)) {
          logicalAnimations.Add(("WALK", [walk1I, walk2I]));
        }

        if (gdlAnimationHeaderIndicesByName
                .TryGetValue("RUN1", out var run1I) &&
            gdlAnimationHeaderIndicesByName
                .TryGetValue("RUN2", out var run2I)) {
          logicalAnimations.Add(("RUN", [run1I, run2I]));
        }
      }

      foreach (var (animationName, headerIs) in logicalAnimations) {
        var finAnimation = finModel.AnimationManager.AddAnimation();
        finAnimation.Name = animationName;

        var gdlHeaders = headerIs.Select(i => gdlAnimationHeaders[i]).ToArray();
        finAnimation.FrameCount = gdlHeaders.Sum(h => h.FrameCount);

        var header0 = gdlAnimationHeaders[headerIs.First()];
        finAnimation.FrameRate = header0.FrameRate;
        finAnimation.UseLoopingInterpolation
            = header0.Loop || gdlHeaders.Length > 1;

        foreach (var gdlBone in gdlAnimationData.SkeletalSequencesByBone.Keys) {
          var allGdlSequencesForBone
              = gdlAnimationData.SkeletalSequencesByBone[gdlBone];
          var gdlBoneSequencesForAnimation
              = headerIs.Select(i => allGdlSequencesForBone[i]).ToArray();

          if (gdlBoneSequencesForAnimation.All(s => s.Size == 0)) {
            continue;
          }

          var totalSequenceFrameCount
              = gdlBoneSequencesForAnimation.Sum(s => s.FrameCount);

          var finBone = finBoneByGdlBone[gdlBone];
          var finBoneTracks = finAnimation.GetOrCreateBoneTracks(finBone);

          finAnimation.FrameCount
              = Math.Max(finAnimation.FrameCount, totalSequenceFrameCount);

          var localPosition = finBone.Transform.LocalTranslation;

          var rotationKeyframes
              = finBoneTracks.UseSeparateEulerRadiansKeyframes(
                  totalSequenceFrameCount);
          rotationKeyframes.ConvertRadiansToQuaternionImpl =
              GdlRadiansUtil.CreateQuaternion;
          var positionKeyframes = finBoneTracks.UseSeparateTranslationKeyframes(
              totalSequenceFrameCount);
          var scaleKeyframes
              = finBoneTracks.UseSeparateScaleKeyframes(
                  totalSequenceFrameCount);

          var startFrame = 0;

          foreach (var gdlSequence in gdlBoneSequencesForAnimation) {
            var header = gdlSequence.Header;

            var isReversed = header.Flags.GetBit(0);

            for (var f = 0; f < gdlSequence.FrameCount; ++f) {
              var frame = startFrame +
                          (!isReversed ? f : (gdlSequence.FrameCount - 1 - f));

              var rotationX = gdlSequence.RotationXs[f];
              if (rotationX != null) {
                rotationKeyframes.SetKeyframe(0,
                                              frame,
                                              rotationX.Value);
              }

              var rotationY = gdlSequence.RotationYs[f];
              if (rotationY != null) {
                rotationKeyframes.SetKeyframe(1,
                                              frame,
                                              rotationY.Value);
              }

              var rotationZ = gdlSequence.RotationZs[f];
              if (rotationZ != null) {
                rotationKeyframes.SetKeyframe(2,
                                              frame,
                                              rotationZ.Value);
              }

              var positionX = gdlSequence.PositionXs[f];
              if (positionX != null) {
                // For some inexplicable reason, the meshes are mirrored.
                positionKeyframes.SetKeyframe(
                    0,
                    frame,
                    localPosition.X - positionX.Value);
              }

              var positionY = gdlSequence.PositionYs[f];
              if (positionY != null) {
                positionKeyframes.SetKeyframe(
                    1,
                    frame,
                    localPosition.Y +
                    positionY.Value);
              }

              var positionZ = gdlSequence.PositionZs[f];
              if (positionZ != null) {
                positionKeyframes.SetKeyframe(
                    2,
                    frame,
                    localPosition.Z + positionZ.Value);
              }

              var scaleX = gdlSequence.ScaleXs[f];
              if (scaleX != null) {
                scaleKeyframes.SetKeyframe(0, frame, scaleX.Value);
              }

              var scaleY = gdlSequence.ScaleYs[f];
              if (scaleY != null) {
                scaleKeyframes.SetKeyframe(1, frame, scaleY.Value);
              }

              var scaleZ = gdlSequence.ScaleZs[f];
              if (scaleZ != null) {
                scaleKeyframes.SetKeyframe(2, frame, scaleZ.Value);
              }
            }

            startFrame += gdlSequence.FrameCount;
          }
        }
      }
    }

    var finSkin = finModel.Skin;
    var rootFinMeshByName = new Dictionary<string, IMesh>();
    foreach (var (definition, obj) in objects.ObjectDefinitions.Zip(
                 objects.RootObjects)) {
      if (rootObjectName != null && definition.Name != rootObjectName) {
        continue;
      }

      var (gdlNode, finBone)
          = gdlNodeAndFinBonesByMeshNamePart
            .FirstOrDefault(kvp => definition.Name.Contains(kvp.Key))
            .Value;

      // HACK: Weapon isn't attached to the hand otherwise
      if (finBone == null && definition.Name.StartsWith("WEAP_")) {
        finBone = weaponBone;
      }

      var finBoneWeights
          = finBone != null
              ? finSkin.GetOrCreateBoneWeights(VertexSpace.RELATIVE_TO_BONE,
                                               finBone)
              : null;

      MeshUtil.AddObjectMesh(finModel,
                             finBoneWeights,
                             Matrix4x4.Identity,
                             definition,
                             obj,
                             lazyFinMaterials,
                             (gdlNode?.MbFlags ?? default) | worldObjectMbFlags,
                             fileBundle.UseSubMeshes,
                             out var finRootMesh);

      rootFinMeshByName[finRootMesh.Name!] = finRootMesh;
    }

    if (fileBundle.IncludeMeshAnimations) {
      foreach (var gdlSkeleton in anim.Atrees) {
        foreach (var gdlObjectAnimation in gdlSkeleton.Data.ObjectAnimationHeader
                     .ObjectAnimations) {
          SplitObjAnimMeshName_(
              gdlObjectAnimation.Name,
              out var meshNamePrefix,
              out var lengthOfIndex,
              out var currentMeshIndex);
          var finMeshAnimation = finModel.AnimationManager.AddAnimation();
          finMeshAnimation.Name = meshNamePrefix;

          finMeshAnimation.FrameCount = gdlObjectAnimation.FrameCount;
          finMeshAnimation.FrameRate = 30;

          var meshesWithPrefix
              = rootFinMeshByName
                .Where(kvp => kvp.Key.StartsWith(meshNamePrefix))
                .OrderBy(kvp => kvp.Key)
                .Select(kvp => kvp.Value)
                .GetEnumerator();

          IMeshTracks? previousMeshTracks = null;
          for (var f = gdlObjectAnimation.StartFrame;
               f < gdlObjectAnimation.FrameCount;
               ++f) {
            meshesWithPrefix.MoveNext();
            var finMesh = meshesWithPrefix.Current;

            finMesh.DefaultDisplayState = MeshDisplayState.HIDDEN;

            var finMeshTracks = finMeshAnimation.AddMeshTracks(finMesh);
            finMeshTracks.DisplayStates.Add(
                new Keyframe<MeshDisplayState>(f, MeshDisplayState.VISIBLE));

            previousMeshTracks?.DisplayStates.Add(
                new Keyframe<MeshDisplayState>(f, MeshDisplayState.HIDDEN));
            previousMeshTracks = finMeshTracks;
          }

          previousMeshTracks.DisplayStates.Add(
              new Keyframe<MeshDisplayState>(gdlObjectAnimation.FrameCount,
                                             MeshDisplayState.HIDDEN));
        }
      }
    }

    return finModel;
  }

  public static void SetFaceTowardsCamera(IBone bone, MbFlags mbFlags) {
    if (mbFlags.CheckFlag(MbFlags.YAW_ONLY_BILLBOARD)) {
      bone.AlwaysFaceTowardsCamera(FaceTowardsCameraType.YAW_ONLY,
                                   QuaternionUtil.CreateZyxRadians(
                                       -MathF.PI / 2,
                                       0,
                                       0));
    } else if (mbFlags.CheckFlag(MbFlags.YAW_AND_PITCH_BILLBOARD)) {
      bone.AlwaysFaceTowardsCamera(FaceTowardsCameraType.YAW_AND_PITCH);
    }
  }

  private static void SplitObjAnimMeshName_(
      string objAnimMeshName,
      out string prefix,
      out int lengthOfIndex,
      out int firstIndex) {
    var indexOfFMarker = objAnimMeshName.LastIndexOf('F');

    prefix = objAnimMeshName[..indexOfFMarker];
    lengthOfIndex = objAnimMeshName.Length - 1 - indexOfFMarker;
    firstIndex = int.Parse(objAnimMeshName[(indexOfFMarker + 1)..]);
  }
}