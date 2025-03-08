using System.Numerics;

using fin.animation.keyframes;
using fin.animation.types.quaternion;
using fin.animation.types.vector3;
using fin.data.lazy;
using fin.data.nodes;
using fin.data.queues;
using fin.io;
using fin.math.rotations;
using fin.model;
using fin.model.impl;
using fin.model.io;
using fin.model.io.importers;
using fin.model.util;
using fin.util.asserts;
using fin.util.enums;

using schema.binary;

using ttyd.schema.model;
using ttyd.schema.model.blocks;
using ttyd.schema.tpl;

namespace ttyd.api;

public class TtydModelFileBundle : IModelFileBundle {
  public required IReadOnlyTreeFile ModelFile { get; init; }
  public IReadOnlyTreeFile MainFile => this.ModelFile;
}

public class TtydModelImporter : IModelImporter<TtydModelFileBundle> {
  public IModel Import(TtydModelFileBundle fileBundle) {
    var modelFile = fileBundle.ModelFile;
    var ttydModel = modelFile.ReadNew<Model>(Endianness.BigEndian);


    Tpl? tpl = null;
    if (modelFile.AssertGetParent()
                 .TryToGetExistingFile($"{ttydModel.Header.TextureFileName}-",
                                       out var textureFile)) {
      tpl = textureFile.ReadNew<Tpl>(Endianness.BigEndian);
    }

    var ttydGroups = ttydModel.Groups;
    var ttydGroupTransforms = ttydModel.GroupTransforms;
    var ttydGroupToParent = new Dictionary<Group, Group>();

    var finModel = new ModelImpl {
        FileBundle = fileBundle,
        Files = new HashSet<IReadOnlyGenericFile>([modelFile, textureFile])
    };

    // Sets up materials
    var finTextureMap = new LazyDictionary<Sampler, ITexture>(
        sampler => {
          var ttydTextureIndex = sampler.TextureIndex;
          var ttydTexture = ttydModel.Textures[ttydTextureIndex];

          var tplTextureIndex = ttydTexture.TplTextureIndex;
          var tplTexture = tpl.AssertNonnull().Textures[tplTextureIndex];

          var finTexture
              = finModel.MaterialManager.CreateTexture(tplTexture.Image);
          finTexture.Name = ttydTexture.Name;

          var wrapFlags = sampler.WrapFlags;
          finTexture.WrapModeU = WrapModeUtil.FromMirrorAndRepeat(
              wrapFlags.CheckFlag(WrapFlags.MIRROR_S),
              wrapFlags.CheckFlag(WrapFlags.REPEAT_S));
          finTexture.WrapModeV = WrapModeUtil.FromMirrorAndRepeat(
              wrapFlags.CheckFlag(WrapFlags.MIRROR_T),
              wrapFlags.CheckFlag(WrapFlags.REPEAT_T));

          return finTexture;
        });
    var finMaterialMap
        = new LazyDictionary<(Sampler?, BlendMode, CullMode), IMaterial?>(
            tuple => {
              var (sampler, blendMode, cullMode) = tuple;
              if (sampler == null) {
                return null;
              }

              var finTexture = finTextureMap[sampler];
              var finMaterial
                  = finModel.MaterialManager.AddTextureMaterial(finTexture);
              finMaterial.Name = $"texMap{sampler.TextureIndex}";
              finMaterial.CullingMode = cullMode switch {
                  CullMode.BACK  => CullingMode.SHOW_FRONT_ONLY,
                  CullMode.FRONT => CullingMode.SHOW_BACK_ONLY,
                  CullMode.ALL   => CullingMode.SHOW_NEITHER,
                  CullMode.NONE  => CullingMode.SHOW_BOTH,
              };

              return finMaterial;
            });

    // Sets up meshes for each group visibility
    var finGroupVisibilityMeshes
        = ttydModel.GroupVisibilities
                   .Select(visible => {
                     var finMesh = finModel.Skin.AddMesh();
                     finMesh.DefaultDisplayState = visible
                         ? MeshDisplayState.VISIBLE
                         : MeshDisplayState.HIDDEN;
                     return finMesh;
                   })
                   .ToArray();

    // Adds bones/meshes
    var groupsAndBoneSets
        = new (Group, TtydTransformData<IReadOnlyBone, IReadOnlyBone>)
            [ttydGroups.Length];
    var groupsAndLastBones = new (Group, IReadOnlyBone)[ttydGroups.Length];
    var groupTreeRoot = new TreeNode<Group>();

    var groupAndBoneQueue
        = new FinTuple3Queue<int, Group?, (IBone, TreeNode<Group>)>(
            (ttydGroups.Length - 1, null,
             (finModel.Skeleton.Root, groupTreeRoot)));
    while (groupAndBoneQueue.TryDequeue(
               out var ttydGroupIndex,
               out var ttydParentGroup,
               out var parentFinBoneAndTreeNode)) {
      var ttydGroup = ttydGroups[ttydGroupIndex];
      var (parentFinBone, parentTreeNode) = parentFinBoneAndTreeNode;

      var treeNode = new TreeNode<Group> { Value = ttydGroup };
      parentTreeNode.AddChild(treeNode);

      if (ttydParentGroup != null) {
        ttydGroupToParent[ttydGroup] = ttydParentGroup;
      }

      var transformData = TtydGroupTransformUtils.GetTransformData(
          ttydGroup,
          ttydGroupToParent,
          ttydGroupTransforms);

      IBone finBone;
      {
        var translationBone
            = parentFinBone.AddChild(
                Matrix4x4.CreateTranslation(transformData.Translation));
        translationBone.IgnoreParentScale = true;
        translationBone.Name = $"{ttydGroup.Name}_translation";

        var applyRotationCenterAndTranslationBone
            = translationBone.AddChild(
                Matrix4x4.CreateTranslation(
                    transformData.ApplyRotationCenterAndTranslation));
        applyRotationCenterAndTranslationBone.Name
            = $"{ttydGroup.Name}_applyRotationCenterAndTranslation";

        var rotation2Bone
            = applyRotationCenterAndTranslationBone.AddChild(
                Matrix4x4.CreateFromQuaternion(
                    transformData.Rotation2.CreateZyxRadians()));
        rotation2Bone.Name = $"{ttydGroup.Name}_rotation2";

        var rotation1Bone
            = rotation2Bone.AddChild(
                Matrix4x4.CreateFromQuaternion(
                    transformData.Rotation1.CreateZyxRadians()));
        rotation1Bone.Name = $"{ttydGroup.Name}_rotation1";

        var undoRotationCenterBone = rotation1Bone.AddChild(
            Matrix4x4.CreateTranslation(transformData.UndoRotationCenter));
        undoRotationCenterBone.Name = $"{ttydGroup.Name}_undoRotationCenter";

        var applyScaleCenterAndTranslationBone
            = undoRotationCenterBone.AddChild(
                Matrix4x4.CreateTranslation(
                    transformData.ApplyScaleCenterAndTranslation));
        applyScaleCenterAndTranslationBone.Name
            = $"{ttydGroup.Name}_applyRotationCenterAndTranslation";

        var scaleBone
            = applyScaleCenterAndTranslationBone.AddChild(
                Matrix4x4.CreateScale(transformData.Scale));
        scaleBone.Name = $"{ttydGroup.Name}_scale";

        var undoScaleCenterBone = scaleBone.AddChild(
            Matrix4x4.CreateTranslation(transformData.UndoScaleCenter));
        undoScaleCenterBone.Name = $"{ttydGroup.Name}_undoScaleCenter";

        groupsAndBoneSets[ttydGroupIndex] = (
            ttydGroup, new TtydTransformData<IReadOnlyBone, IReadOnlyBone> {
                Translation = translationBone,
                ApplyRotationCenterAndTranslation
                    = applyRotationCenterAndTranslationBone,
                Rotation2 = rotation2Bone,
                Rotation1 = rotation1Bone,
                UndoRotationCenter = undoRotationCenterBone,
                ApplyScaleCenterAndTranslation
                    = applyScaleCenterAndTranslationBone,
                Scale = scaleBone,
                UndoScaleCenter = undoScaleCenterBone
            });
        finBone = undoScaleCenterBone;
      }

      groupsAndLastBones[ttydGroupIndex] = (ttydGroup, finBone);
      if (ttydGroup.NextGroupIndex != -1) {
        groupAndBoneQueue.Enqueue(
            (ttydGroup.NextGroupIndex, ttydParentGroup,
             (parentFinBone, parentTreeNode)));
      }

      if (ttydGroup.ChildGroupIndex != -1) {
        groupAndBoneQueue.Enqueue((ttydGroup.ChildGroupIndex, ttydGroup,
                                   (finBone, treeNode)));
      }
    }

    // Sets up meshes
    foreach (var (ttydGroup, finBone) in groupsAndLastBones) {
      if (ttydGroup.SceneGraphObjectIndex == -1) {
        continue;
      }

      var boneWeights = finModel.Skin.GetOrCreateBoneWeights(
          VertexSpace.RELATIVE_TO_BONE,
          finBone);

      var ttydSceneGraphObject
          = ttydModel.SceneGraphObjects[
              ttydGroup.SceneGraphObjectIndex];
      var finMesh = finGroupVisibilityMeshes[ttydGroup.VisibilityGroupIndex];

      var objectPositions = ttydModel.Vertices.AsSpan(
          ttydSceneGraphObject.VertexPosition.BaseIndex);
      var objectNormals = ttydModel.Normals.AsSpan(
          ttydSceneGraphObject.VertexNormal.BaseIndex);
      var objectColors = ttydModel.Colors.AsSpan(
          ttydSceneGraphObject.VertexColor.BaseIndex);
      var objectTexCoords = ttydModel.TexCoords.AsSpan(
          ttydSceneGraphObject.TexCoords[0].BaseIndex);

      var ttydMeshes = ttydModel.Meshes.AsSpan(
          ttydSceneGraphObject.MeshBaseIndex,
          ttydSceneGraphObject.MeshCount);
      foreach (var ttydMesh in ttydMeshes) {
        if (ttydMesh.PolygonBaseIndex == -1) {
          continue;
        }

        var sampler = ttydMesh.SamplerIndex != -1
            ? ttydModel.TextureMaps[ttydMesh.SamplerIndex]
            : null;

        var finMaterial = finMaterialMap[(sampler,
                                          ttydSceneGraphObject.BlendMode,
                                          ttydSceneGraphObject.CullMode)];

        var ttydPolygons
            = ttydModel.Polygons.AsSpan(ttydMesh.PolygonBaseIndex,
                                        ttydMesh.PolygonCount);
        foreach (var ttydPolygon in ttydPolygons) {
          var finVertices = new IVertex[ttydPolygon.VertexCount];
          for (var i = 0; i < ttydPolygon.VertexCount; i++) {
            var vertexPosition = objectPositions[
                ttydModel.VertexIndices[
                    ttydMesh.VertexPositionBaseIndex +
                    ttydPolygon.VertexBaseIndex +
                    i]];
            var vertexNormal = objectNormals[
                ttydModel.NormalIndices[
                    ttydMesh.VertexNormalBaseIndex +
                    ttydPolygon.VertexBaseIndex +
                    i]];
            var vertexColor = objectColors[
                ttydModel.ColorIndices[
                    ttydMesh.VertexColorBaseIndex +
                    ttydPolygon.VertexBaseIndex +
                    i]];

            var finVertex = finModel.Skin.AddVertex(vertexPosition);
            finVertex.SetLocalNormal(vertexNormal);
            finVertex.SetColor(vertexColor);
            finVertex.SetBoneWeights(boneWeights);

            if (ttydMesh.SamplerIndex != -1) {
              var vertexTexCoord = objectTexCoords[
                  ttydModel.TexCoordIndices.Length > 0
                      ? ttydModel.TexCoordIndices[
                          ttydMesh.VertexTexCoordBaseIndices[0] +
                          ttydPolygon.VertexBaseIndex +
                          i]
                      : i];
              finVertex.SetUv(vertexTexCoord);
            }

            finVertices[i] = finVertex;
          }

          var finPrimitive = finMesh.AddTriangleFan(finVertices);
          if (finMaterial != null) {
            finPrimitive.SetMaterial(finMaterial);
          }
        }
      }
    }

    // Sets up animations
    foreach (var ttydAnimation in ttydModel.Animations) {
      var ttydAnimationData = ttydAnimation.Data;
      if (ttydAnimationData == null) {
        continue;
      }

      var finAnimation = finModel.AnimationManager.AddAnimation();
      finAnimation.Name = ttydAnimation.Name;

      var baseInfo = Asserts.CastNonnull(ttydAnimationData.BaseInfos.First());

      // TODO: is this right?
      var length = baseInfo.End;
      finAnimation.FrameCount = (int) length;
      finAnimation.FrameRate = 60;
      finAnimation.UseLoopingInterpolation = baseInfo.Loop;

      var boneTrackDataByGroup
          = new Dictionary<Group, TtydTransformData<
              ICombinedVector3Keyframes<Keyframe<Vector3>>,
              ICombinedQuaternionKeyframes<Keyframe<Quaternion>>>>();
      foreach (var (group, transformData) in groupsAndBoneSets) {
        boneTrackDataByGroup[group] = new TtydTransformData<
            ICombinedVector3Keyframes<Keyframe<Vector3>>,
            ICombinedQuaternionKeyframes<Keyframe<Quaternion>>> {
            Translation
                = finAnimation
                  .GetOrCreateBoneTracks(transformData.Translation)
                  .UseCombinedTranslationKeyframes(),
            ApplyRotationCenterAndTranslation
                = finAnimation
                  .GetOrCreateBoneTracks(
                      transformData.ApplyRotationCenterAndTranslation)
                  .UseCombinedTranslationKeyframes(),
            Rotation1 = finAnimation
                        .GetOrCreateBoneTracks(transformData.Rotation1)
                        .UseCombinedQuaternionKeyframes(),
            Rotation2 = finAnimation
                        .GetOrCreateBoneTracks(transformData.Rotation2)
                        .UseCombinedQuaternionKeyframes(),
            UndoRotationCenter
                = finAnimation
                  .GetOrCreateBoneTracks(
                      transformData.UndoRotationCenter)
                  .UseCombinedTranslationKeyframes(),
            ApplyScaleCenterAndTranslation
                = finAnimation
                  .GetOrCreateBoneTracks(
                      transformData.ApplyScaleCenterAndTranslation)
                  .UseCombinedTranslationKeyframes(),
            Scale = finAnimation
                    .GetOrCreateBoneTracks(transformData.Scale)
                    .UseCombinedScaleKeyframes(),
            UndoScaleCenter
                = finAnimation
                  .GetOrCreateBoneTracks(transformData.UndoScaleCenter)
                  .UseCombinedTranslationKeyframes(),
        };
      }

      var allFinMeshTracks
          = finGroupVisibilityMeshes
            .Select(finAnimation.AddMeshTracks)
            .ToArray();

      var keyframes
          = new TtydGroupTransformKeyframes(ttydModel.GroupTransforms,
                                            finAnimation.FrameCount);
      foreach (var ttydKeyframe in ttydAnimationData.Keyframes) {
        var keyframe = (int) ttydKeyframe.Time;

        var groupTransformDataDeltaCount
            = ttydKeyframe.GroupTransformDataDeltaCount;
        if (groupTransformDataDeltaCount > 0) {
          var groupTransformDataDeltas =
              ttydAnimationData.GroupTransformDataDeltas.AsSpan(
                  (int) ttydKeyframe.GroupTransformDataDeltaBaseIndex,
                  (int) groupTransformDataDeltaCount);

          keyframes.AddDeltasForKeyframe(ttydKeyframe.Time,
                                         groupTransformDataDeltas);
        }

        // Sets up visibility animations
        var visibilityIndexAccumulator = 0;

        var visibilityGroupDeltaCount
            = ttydKeyframe.VisibilityGroupDeltaCount;
        if (visibilityGroupDeltaCount > 0) {
          var visibilityGroupDeltas
              = ttydAnimationData.VisibilityGroupDeltas.AsSpan(
                  (int) ttydKeyframe.VisibilityGroupDeltaBaseIndex,
                  (int) visibilityGroupDeltaCount);

          foreach (var visibilityGroupDelta in visibilityGroupDeltas) {
            Asserts.True(visibilityGroupDelta.Visible is 1 or -1);

            visibilityIndexAccumulator
                += visibilityGroupDelta.VisibilityGroupId;

            var finMeshTracks
                = allFinMeshTracks[visibilityIndexAccumulator];
            finMeshTracks.DisplayStates.SetKeyframe(
                keyframe,
                visibilityGroupDelta.Visible == 1
                    ? MeshDisplayState.VISIBLE
                    : MeshDisplayState.HIDDEN);
          }
        }
      }

      var bakedKeyframes = keyframes.BakeTransformsAtFrames();

      foreach (var (ttydGroup, boneTrackData) in boneTrackDataByGroup) {
        for (var i = 0; i < finAnimation.FrameCount; ++i) {
          var transformData = TtydGroupTransformUtils.GetTransformData(
                  ttydGroup,
                  ttydGroupToParent,
                  bakedKeyframes,
                  i);

          boneTrackData.Translation.SetKeyframe(i, transformData.Translation);
          boneTrackData.ApplyRotationCenterAndTranslation.SetKeyframe(
              i,
              transformData.ApplyRotationCenterAndTranslation);
          boneTrackData.Rotation1.SetKeyframe(i,
                                             transformData.Rotation1
                                                 .CreateZyxRadians());
          boneTrackData.Rotation2.SetKeyframe(i,
                                             transformData.Rotation2
                                                 .CreateZyxRadians());
          boneTrackData.UndoRotationCenter.SetKeyframe(
              i,
              transformData.UndoRotationCenter);
          boneTrackData.ApplyScaleCenterAndTranslation.SetKeyframe(
              i,
              transformData.ApplyScaleCenterAndTranslation);
          boneTrackData.Scale.SetKeyframe(i, transformData.Scale);
          boneTrackData.UndoScaleCenter.SetKeyframe(
              i,
              transformData.UndoScaleCenter);
        }
      }
    }

    return finModel;
  }
}