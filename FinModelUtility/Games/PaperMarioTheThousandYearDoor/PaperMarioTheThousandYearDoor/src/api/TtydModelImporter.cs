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
        if (transformData.IsJoint) {
          var jointData = transformData.JointData;

          var translationBone
              = parentFinBone.AddChild(
                  Matrix4x4.CreateTranslation(jointData.Translation));
          translationBone.Name = $"{ttydGroup.Name}_translation";

          var undoParentScaleBone
              = translationBone.AddChild(
                  Matrix4x4.CreateScale(jointData.UndoParentScale));
          undoParentScaleBone.Name = $"{ttydGroup.Name}_undoParentScale";

          var rotation2Bone
              = undoParentScaleBone.AddChild(
                  Matrix4x4.CreateFromQuaternion(
                      jointData.Rotation2.CreateZyxRadians()));
          rotation2Bone.Name = $"{ttydGroup.Name}_rotation2";

          var rotation1Bone
              = rotation2Bone.AddChild(
                  Matrix4x4.CreateFromQuaternion(
                      jointData.Rotation1.CreateZyxRadians()));
          rotation1Bone.Name = $"{ttydGroup.Name}_rotation1";

          var scaleBone
              = rotation1Bone.AddChild(Matrix4x4.CreateScale(jointData.Scale));
          scaleBone.Name = $"{ttydGroup.Name}_scale";

          groupsAndBoneSets[ttydGroupIndex] = (
              ttydGroup, new TtydTransformData<IReadOnlyBone, IReadOnlyBone> {
                  IsJoint = true,
                  JointData
                      = new TtydTransformJointData<IReadOnlyBone, IReadOnlyBone> {
                          Translation = translationBone,
                          UndoParentScale = undoParentScaleBone,
                          Rotation1 = rotation1Bone,
                          Rotation2 = rotation2Bone,
                          Scale = scaleBone,
                      },
                  NonJointData = default,
              });
          finBone = scaleBone;
        } else {
          var nonJointData = transformData.NonJointData;

          var translationBone
              = parentFinBone.AddChild(
                  Matrix4x4.CreateTranslation(nonJointData.Translation));
          translationBone.Name = $"{ttydGroup.Name}_translation";

          var applyRotationCenterAndTranslationBone
              = translationBone.AddChild(
                  Matrix4x4.CreateTranslation(
                      nonJointData.ApplyRotationCenterAndTranslation));
          applyRotationCenterAndTranslationBone.Name
              = $"{ttydGroup.Name}_applyRotationCenterAndTranslation";

          var rotationBone
              = applyRotationCenterAndTranslationBone.AddChild(
                  Matrix4x4.CreateFromQuaternion(
                      nonJointData.Rotation.CreateZyxRadians()));
          rotationBone.Name = $"{ttydGroup.Name}_rotation";

          var undoRotationCenterBone = rotationBone.AddChild(
              Matrix4x4.CreateTranslation(nonJointData.UndoRotationCenter));
          undoRotationCenterBone.Name = $"{ttydGroup.Name}_undoRotationCenter";

          var applyScaleCenterAndTranslationBone
              = undoRotationCenterBone.AddChild(
                  Matrix4x4.CreateTranslation(
                      nonJointData.ApplyScaleCenterAndTranslation));
          applyScaleCenterAndTranslationBone.Name
              = $"{ttydGroup.Name}_applyRotationCenterAndTranslation";

          var scaleBone
              = applyScaleCenterAndTranslationBone.AddChild(
                  Matrix4x4.CreateScale(nonJointData.Scale));
          scaleBone.Name = $"{ttydGroup.Name}_scale";

          var undoScaleCenterBone = scaleBone.AddChild(
              Matrix4x4.CreateTranslation(nonJointData.UndoScaleCenter));
          undoScaleCenterBone.Name = $"{ttydGroup.Name}_undoScaleCenter";

          groupsAndBoneSets[ttydGroupIndex] = (
              ttydGroup, new TtydTransformData<IReadOnlyBone, IReadOnlyBone> {
                  IsJoint = false,
                  JointData = default,
                  NonJointData
                      = new TtydTransformNonJointData<IReadOnlyBone,
                          IReadOnlyBone> {
                          Translation = translationBone,
                          ApplyRotationCenterAndTranslation
                              = applyRotationCenterAndTranslationBone,
                          Rotation = rotationBone,
                          UndoRotationCenter = undoRotationCenterBone,
                          ApplyScaleCenterAndTranslation
                              = applyScaleCenterAndTranslationBone,
                          Scale = scaleBone,
                          UndoScaleCenter = undoScaleCenterBone
                      },
              });
          finBone = undoScaleCenterBone;
        }
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
        if (transformData.IsJoint) {
          var jointData = transformData.JointData;
          boneTrackDataByGroup[group] = new TtydTransformData<
              ICombinedVector3Keyframes<Keyframe<Vector3>>,
              ICombinedQuaternionKeyframes<Keyframe<Quaternion>>> {
              IsJoint = true,
              NonJointData = default,
              JointData
                  = new TtydTransformJointData<
                      ICombinedVector3Keyframes<Keyframe<Vector3>>,
                      ICombinedQuaternionKeyframes<Keyframe<Quaternion>>> {
                      Translation
                          = finAnimation
                            .AddBoneTracks(jointData.Translation)
                            .UseCombinedTranslationKeyframes(),
                      UndoParentScale
                          = finAnimation
                            .AddBoneTracks(jointData.UndoParentScale)
                            .UseCombinedScaleKeyframes(),
                      Rotation1 = finAnimation
                                  .AddBoneTracks(jointData.Rotation1)
                                  .UseCombinedQuaternionKeyframes(),
                      Rotation2 = finAnimation
                                  .AddBoneTracks(jointData.Rotation2)
                                  .UseCombinedQuaternionKeyframes(),
                      Scale = finAnimation
                              .AddBoneTracks(jointData.Scale)
                              .UseCombinedScaleKeyframes(),
                  }
          };
        } else {
          var nonJointData = transformData.NonJointData;
          boneTrackDataByGroup[group] = new TtydTransformData<
              ICombinedVector3Keyframes<Keyframe<Vector3>>,
              ICombinedQuaternionKeyframes<Keyframe<Quaternion>>> {
              IsJoint = false,
              JointData = default,
              NonJointData
                  = new TtydTransformNonJointData<
                      ICombinedVector3Keyframes<Keyframe<Vector3>>,
                      ICombinedQuaternionKeyframes<Keyframe<Quaternion>>> {
                      Translation
                          = finAnimation
                            .AddBoneTracks(nonJointData.Translation)
                            .UseCombinedTranslationKeyframes(),
                      ApplyRotationCenterAndTranslation
                          = finAnimation
                            .AddBoneTracks(
                                nonJointData.ApplyRotationCenterAndTranslation)
                            .UseCombinedTranslationKeyframes(),
                      Rotation = finAnimation
                                 .AddBoneTracks(nonJointData.Rotation)
                                 .UseCombinedQuaternionKeyframes(),
                      UndoRotationCenter
                          = finAnimation
                            .AddBoneTracks(nonJointData.UndoRotationCenter)
                            .UseCombinedTranslationKeyframes(),
                      ApplyScaleCenterAndTranslation
                          = finAnimation
                            .AddBoneTracks(
                                nonJointData.ApplyScaleCenterAndTranslation)
                            .UseCombinedTranslationKeyframes(),
                      Scale = finAnimation
                              .AddBoneTracks(nonJointData.Scale)
                              .UseCombinedScaleKeyframes(),
                      UndoScaleCenter
                          = finAnimation
                            .AddBoneTracks(nonJointData.UndoScaleCenter)
                            .UseCombinedTranslationKeyframes(),
                  }
          };
        }
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
        if (boneTrackData.IsJoint) {
          var jointData = boneTrackData.JointData;
          for (var i = 0; i < finAnimation.FrameCount; ++i) {
            var transformData = TtydGroupTransformUtils.GetTransformData(
                    ttydGroup,
                    ttydGroupToParent,
                    bakedKeyframes,
                    i)
                .JointData;

            jointData.Translation.SetKeyframe(i, transformData.Translation);
            jointData.UndoParentScale.SetKeyframe(
                i,
                transformData.UndoParentScale);
            jointData.Rotation1.SetKeyframe(i,
                                            transformData.Rotation1
                                                .CreateZyxRadians());
            jointData.Rotation2.SetKeyframe(i,
                                            transformData.Rotation2
                                                .CreateZyxRadians());
            jointData.Scale.SetKeyframe(i, transformData.Scale);
          }
        } else {
          var nonJointData = boneTrackData.NonJointData;
          for (var i = 0; i < finAnimation.FrameCount; ++i) {
            var transformData = TtydGroupTransformUtils.GetTransformData(
                    ttydGroup,
                    ttydGroupToParent,
                    bakedKeyframes,
                    i)
                .NonJointData;

            nonJointData.Translation.SetKeyframe(i, transformData.Translation);
            nonJointData.ApplyRotationCenterAndTranslation.SetKeyframe(
                i,
                transformData.ApplyRotationCenterAndTranslation);
            nonJointData.Rotation.SetKeyframe(
                i,
                transformData.Rotation.CreateZyxRadians());
            nonJointData.UndoRotationCenter.SetKeyframe(
                i,
                transformData.UndoRotationCenter);
            nonJointData.ApplyScaleCenterAndTranslation.SetKeyframe(
                i,
                transformData.ApplyScaleCenterAndTranslation);
            nonJointData.Scale.SetKeyframe(i, transformData.Scale);
            nonJointData.UndoScaleCenter.SetKeyframe(
                i,
                transformData.UndoScaleCenter);
          }
        }
      }
    }

    return finModel;
  }
}