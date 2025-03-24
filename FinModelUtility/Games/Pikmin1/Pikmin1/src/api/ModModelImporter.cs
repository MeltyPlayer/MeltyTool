using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;

using CommunityToolkit.HighPerformance.Helpers;

using fin.animation.keyframes;
using fin.color;
using fin.data.dictionaries;
using fin.data.lazy;
using fin.data.queues;
using fin.image;
using fin.io;
using fin.math.transform;
using fin.model;
using fin.model.impl;
using fin.model.io.importers;
using fin.model.util;
using fin.util.asserts;
using fin.util.enums;
using fin.image.util;
using fin.util.lists;

using gx;
using gx.impl;
using gx.displayList;

using pikmin1.schema.anm;
using pikmin1.schema.mod;
using pikmin1.util;

using schema.binary;


namespace pikmin1.api;

public class ModModelImporter : IModelImporter<ModModelFileBundle> {
  /// <summary>
  ///   GX's active matrices. These are deferred to when a vertex matrix is
  ///   -1, which corresponds to using an active matrix from a previous
  ///   display list.
  /// </summary>
  private short[] activeMatrices_ = new short[10];

  public IModel Import(ModModelFileBundle modelFileBundle) {
    var mod =
        modelFileBundle.ModFile.ReadNew<Mod>(Endianness.BigEndian);
    var anm =
        modelFileBundle.AnmFile?.ReadNew<Anm>(Endianness.BigEndian);

    // Resets the active matrices to -1. This lets us catch issues when
    // attempting to use an invalid active matrix.
    for (var i = 0; i < 10; ++i) {
      this.activeMatrices_[i] = -1;
    }

    var finModCache = new FinModCache(mod);

    var model = new ModelImpl {
        FileBundle = modelFileBundle,
        Files = modelFileBundle.Files.ToHashSet()
    };

    var hasVertices = mod.vertices.Any();
    var hasNormals = mod.vnormals.Any();
    var hasFaces = mod.colltris.collinfo.Any();

    if (!hasVertices && !hasNormals && !hasFaces) {
      Asserts.Fail("Loaded file has nothing to export!");
    }

    var textureImages = new IReadOnlyImage[mod.textures.Count][];
    ParallelHelper.For(0,
                       textureImages.Length,
                       new TextureImageReader(mod.textures, textureImages));

    // Writes textures
    var gxTextures = new IGxTexture[mod.texattrs.Count];
    for (var i = 0; i < mod.texattrs.Count; ++i) {
      var textureAttr = mod.texattrs[i];

      var textureIndex = textureAttr.TextureImageIndex;
      var image = textureImages[textureIndex];

      gxTextures[i] = new GxTexture2d(
          null,
          image,
          GxWrapMode.GX_CLAMP,
          GxWrapMode.GX_CLAMP,
          LodBias: textureAttr.WidthPercent);
    }

    var hasMaterialAnimation = mod.materials.materials.Any(
        m => m.texInfo.TexturesInMaterial.Any(t => t.AnimationLength > 0));
    IModelAnimation? materialAnimation = null;
    if (hasMaterialAnimation) {
      materialAnimation = model.AnimationManager.AddAnimation();
      materialAnimation.FrameRate = 30;
    }

    var lazyTextureDictionary = new GxLazyTextureDictionary<Material, string>(
        model,
        (gxTextureBundle, modMaterial)
            => $"{gxTextureBundle.GetHashCode()},{modMaterial.GetHashCode()}",
        (gxTextureBundle, modMaterial, finTexture) => {
          var modTextureData
              = modMaterial.texInfo.TexturesInMaterial[
                  (int) gxTextureBundle.TexMap];
          var animationLength = modTextureData.AnimationLength;
          if (animationLength == 0 || materialAnimation == null) {
            return;
          }

          var animationSpeed = modTextureData.AnimationSpeed;
          var adjustedAnimationLength
              = (int) (animationLength / animationSpeed);

          materialAnimation.FrameCount = Math.Max(materialAnimation.FrameCount,
                                                  adjustedAnimationLength);

          var finTextureTracks = materialAnimation.AddTextureTracks(finTexture);
          {
            var modTranslationKeyframes
                = modTextureData.TranslationAnimationData;
            var finTranslationKeyframes = finTextureTracks
                .UseSeparateTranslationKeyframesWithTangents(
                    animationLength: adjustedAnimationLength);
            foreach (var modTranslationKeyframe in modTranslationKeyframes) {
              var adjustedFrame = modTranslationKeyframe.Frame / animationSpeed;
              finTranslationKeyframes
                  .Axes[0]
                  .SetKeyframe(adjustedFrame,
                               modTranslationKeyframe.X.Value,
                               modTranslationKeyframe.X.InTangent *
                               animationSpeed,
                               modTranslationKeyframe.X.OutTangent *
                               animationSpeed);
              finTranslationKeyframes
                  .Axes[1]
                  .SetKeyframe(adjustedFrame,
                               modTranslationKeyframe.Y.Value,
                               modTranslationKeyframe.Y.InTangent *
                               animationSpeed,
                               modTranslationKeyframe.Y.OutTangent *
                               animationSpeed);
              finTranslationKeyframes
                  .Axes[2]
                  .SetKeyframe(adjustedFrame,
                               modTranslationKeyframe.Z.Value,
                               modTranslationKeyframe.Z.InTangent *
                               animationSpeed,
                               modTranslationKeyframe.Z.OutTangent *
                               animationSpeed);
            }
          }
          {
            var modRotationKeyframes = modTextureData.RotationAnimationData;
            var finRotationKeyframes
                = finTextureTracks.UseSeparateRotationKeyframesWithTangents(
                    animationLength: adjustedAnimationLength);
            foreach (var modRotationKeyframe in modRotationKeyframes) {
              var adjustedFrame = modRotationKeyframe.Frame / animationSpeed;
              finRotationKeyframes
                  .Axes[0]
                  .SetKeyframe(adjustedFrame,
                               modRotationKeyframe.X.Value,
                               modRotationKeyframe.X.InTangent *
                               animationSpeed,
                               modRotationKeyframe.X.OutTangent *
                               animationSpeed);
              finRotationKeyframes
                  .Axes[1]
                  .SetKeyframe(adjustedFrame,
                               modRotationKeyframe.Y.Value,
                               modRotationKeyframe.Y.InTangent *
                               animationSpeed,
                               modRotationKeyframe.Y.OutTangent *
                               animationSpeed);
              finRotationKeyframes
                  .Axes[2]
                  .SetKeyframe(adjustedFrame,
                               modRotationKeyframe.Z.Value,
                               modRotationKeyframe.Z.InTangent *
                               animationSpeed,
                               modRotationKeyframe.Z.OutTangent *
                               animationSpeed);
            }
          }
          {
            var modScaleKeyframes = modTextureData.ScaleAnimationData;
            var finScaleKeyframes
                = finTextureTracks.UseSeparateScaleKeyframesWithTangents(
                    animationLength: adjustedAnimationLength);
            foreach (var modScaleKeyframe in modScaleKeyframes) {
              var adjustedFrame = modScaleKeyframe.Frame / animationSpeed;
              finScaleKeyframes.Axes[0]
                               .SetKeyframe(adjustedFrame,
                                            modScaleKeyframe.X.Value,
                                            modScaleKeyframe.X.InTangent *
                                            animationSpeed,
                                            modScaleKeyframe.X.OutTangent *
                                            animationSpeed);
              finScaleKeyframes.Axes[1]
                               .SetKeyframe(adjustedFrame,
                                            modScaleKeyframe.Y.Value,
                                            modScaleKeyframe.Y.InTangent *
                                            animationSpeed,
                                            modScaleKeyframe.Y.OutTangent *
                                            animationSpeed);
              finScaleKeyframes.Axes[2]
                               .SetKeyframe(adjustedFrame,
                                            modScaleKeyframe.Z.Value,
                                            modScaleKeyframe.Z.InTangent *
                                            animationSpeed,
                                            modScaleKeyframe.Z.OutTangent *
                                            animationSpeed);
            }
          }
        });

    // Writes materials
    Func<int, Material, ModCullMode, IMaterial>
        getFinMaterialFromModMaterial
            = (materialIndex, modMaterial, modCullMode) => {
              lazyTextureDictionary.State = modMaterial;

              IMaterial finMaterial;
              if (modMaterial.flags.CheckFlag(MaterialFlags.HIDDEN)) {
                finMaterial = model.MaterialManager.AddHiddenMaterial();
              } else if (modMaterial.flags.CheckFlag(MaterialFlags.ENABLED)) {
                var modPopulatedMaterial =
                    new ModPopulatedMaterial(
                        materialIndex,
                        modMaterial,
                        (int) modMaterial.TevGroupId,
                        mod.materials.texEnvironments[
                            (int) modMaterial.TevGroupId]);

                finMaterial = new GxFixedFunctionMaterial(
                    model,
                    model.MaterialManager,
                    modPopulatedMaterial,
                    gxTextures,
                    lazyTextureDictionary).Material;

                var flags = modMaterial.flags;
                finMaterial.TransparencyType
                    = flags.CheckFlag(MaterialFlags.TRANSPARENT_BLEND)
                        ? TransparencyType.TRANSPARENT
                        : flags.CheckFlag(MaterialFlags.ALPHA_CLIP)
                            ? TransparencyType.MASK
                            : TransparencyType.OPAQUE;
              } else {
                finMaterial = model.MaterialManager.AddNullMaterial();
              }

              finMaterial.Name = $"material{materialIndex}";
              finMaterial.CullingMode = modCullMode switch {
                  ModCullMode.SHOW_FRONT_ONLY => CullingMode.SHOW_FRONT_ONLY,
                  ModCullMode.SHOW_BACK_ONLY  => CullingMode.SHOW_BACK_ONLY,
                  ModCullMode.SHOW_BOTH       => CullingMode.SHOW_BOTH,
                  ModCullMode.SHOW_NEITHER    => CullingMode.SHOW_NEITHER,
                  _ => throw new ArgumentOutOfRangeException(
                      nameof(modCullMode),
                      modCullMode,
                      null)
              };

              return finMaterial;
            };

    var lazyMaterials = new LazyDictionary<(int, ModCullMode), IMaterial>(
        materialIndexAndCullMode => {
          var (materialIndex, cullMode) = materialIndexAndCullMode;
          var modMaterial = mod.materials.materials[materialIndex];
          var finMaterial
              = getFinMaterialFromModMaterial(materialIndex,
                                              modMaterial,
                                              cullMode);
          return finMaterial;
        });

    // Writes bones
    // TODO: Simplify these loops
    var jointCount = mod.joints.Count;
    // Pass 1: Creates lists at each index in joint children
    var jointChildren = new List<int>[jointCount];
    var childrenByJoint = new ListDictionary<Joint, Joint>();
    for (var i = 0; i < jointCount; ++i) {
      jointChildren[i] = [];
    }

    // Pass 2: Gathers up children of each bone via parent index
    for (var i = 0; i < jointCount; ++i) {
      var joint = mod.joints[i];
      var parentIndex = (int) joint.parentIdx;
      if (parentIndex != -1) {
        jointChildren[parentIndex].Add(i);
        childrenByJoint.Add(mod.joints[parentIndex], joint);
      }
    }

    // Pass 3: Creates skeleton
    var finBones = ListUtil.OfLength<IBone>(jointCount);

    var jointQueue = new FinTuple2Queue<int, IBone?>((0, null));
    while (jointQueue.TryDequeue(out var jointIndex, out var parent)) {
      var joint = mod.joints[jointIndex];

      var bone = (parent ?? model.Skeleton.Root).AddChild(joint.position);
      bone.LocalTransform.SetRotationRadians(joint.rotation);
      bone.LocalTransform.SetScale(joint.scale);

      if (mod.jointNames.Count > 0) {
        var jointName = mod.jointNames[jointIndex];
        bone.Name = jointName;
      } else {
        bone.Name = $"bone {jointIndex}";
      }

      finBones[jointIndex] = bone;

      jointQueue.Enqueue(jointChildren[jointIndex]
                             .Select(childI => (childI, bone)));
    }

    // Creates extra bones if there are any indices unaccounted for in the animation
    if (anm != null) {
      foreach (var dcxWrapper in anm.Wrappers) {
        foreach (var jointData in
                 dcxWrapper.Dcx.AnimationData.JointDataList) {
          var jointIndex = jointData.JointIndex;
          while (jointIndex >= finBones.Count) {
            finBones.Add(null);
          }

          if (finBones[jointIndex] == null) {
            finBones[jointIndex]
                = finBones[jointData.ParentIndex].AddChild(0, 0, 0);
          }
        }
      }
    }

    // Pass 4: Writes each bone's meshes as skin
    var envelopeBoneWeights =
        mod.envelopes.Select(
               envelope =>
                   model.Skin.CreateBoneWeights(
                       VertexSpace.RELATIVE_TO_WORLD,
                       envelope.indicesAndWeights
                               .Select(
                                   indexAndWeight =>
                                       new BoneWeight(
                                           finBones[indexAndWeight.index],
                                           null,
                                           indexAndWeight.weight)
                               )
                               .ToArray()))
           .ToArray();

    model.Skin.AllowMaterialRendererMerging = false;

    // Ripped directly from the decomp
    var sortedMatPolys = new LinkedList<JointMatPoly>();
    AddSortedMatPolysSiblings_(sortedMatPolys,
                               [mod.joints[0]],
                               childrenByJoint,
                               mod.materials.materials,
                               MaterialFlags.TRANSPARENT_BLEND);
    AddSortedMatPolysSiblings_(sortedMatPolys,
                               [mod.joints[0]],
                               childrenByJoint,
                               mod.materials.materials,
                               MaterialFlags.ALPHA_CLIP);
    AddSortedMatPolysSiblings_(sortedMatPolys,
                               [mod.joints[0]],
                               childrenByJoint,
                               mod.materials.materials,
                               MaterialFlags.OPAQUE);

    foreach (var matPoly in sortedMatPolys) {
      var mesh = mod.meshes[matPoly.meshIdx];

      this.AddMesh_(mod,
                    mesh,
                    matPoly,
                    lazyMaterials,
                    model,
                    finBones,
                    envelopeBoneWeights,
                    finModCache);
    }

    // Converts animations
    if (anm != null) {
      foreach (var dcxWrapper in anm.Wrappers) {
        DcxHelpers.AddAnimation(finBones,
                                model.AnimationManager,
                                dcxWrapper.Dcx);
      }
    }

    return model;
  }

  private void AddMesh_(
      Mod mod,
      Mesh mesh,
      JointMatPoly matPoly,
      ILazyDictionary<(int, ModCullMode), IMaterial> lazyMaterialDictionary,
      ModelImpl model,
      IReadOnlyList<IBone> bones,
      IBoneWeights[] envelopeBoneWeights,
      FinModCache finModCache) {
    var vertexDescriptor
        = new Pikmin1VertexDescriptor(mesh.vtxDescriptor, mod.hasNormals);

    var finSkin = model.Skin;
    var finMesh = finSkin.AddMesh();

    var gxDisplayListReader = new GxDisplayListReader();

    foreach (var meshPacket in mesh.packets) {
      foreach (var dlist in meshPacket.displaylists) {
        var finMaterial
            = lazyMaterialDictionary[(matPoly.matIdx, dlist.CullMode)];

        var br =
            new SchemaBinaryReader(dlist.dlistData, Endianness.BigEndian);

        while (!br.Eof) {
          var gxPrimitive = gxDisplayListReader.Read(br, vertexDescriptor);
          if (gxPrimitive != null) {
            var finVertexList = new List<IReadOnlyVertex>();

            foreach (var gxVertex in gxPrimitive.Vertices) {
              var position
                  = finModCache.PositionsByIndex[gxVertex.PositionIndex];
              
              var finVertex = model.Skin.AddVertex(position);
              finVertexList.Add(finVertex);

              var jointIndex = gxVertex.JointIndex;
              if (jointIndex != null) {
                // This represents which vertex matrix the active matrix is
                // sourced from.
                var vertexMatrixIndex = meshPacket.indices[jointIndex.Value];

                // -1 means no active matrix set by this display list,
                // defers to whatever the existing matrix is in this slot.
                if (vertexMatrixIndex == -1) {
                  vertexMatrixIndex = this.activeMatrices_[jointIndex.Value];
                  Asserts.False(vertexMatrixIndex == -1);
                }

                this.activeMatrices_[jointIndex.Value] = vertexMatrixIndex;

                // TODO: Is there a real name for this?
                // Remaps from vertex matrix to "attachment" index.
                var attachmentIndex =
                    mod.vtxMatrix[vertexMatrixIndex].index;

                // Positive indices refer to joints/bones.
                IBoneWeights boneWeights;
                if (attachmentIndex >= 0) {
                  var boneIndex = attachmentIndex;
                  boneWeights = finSkin.GetOrCreateBoneWeights(
                          VertexSpace.RELATIVE_TO_BONE,
                          bones[boneIndex]);
                }
                // Negative indices refer to envelopes.
                else {
                  var envelopeIndex = -1 - attachmentIndex;
                  boneWeights = envelopeBoneWeights[envelopeIndex];
                }
                finVertex.SetBoneWeights(boneWeights);
              }

              var normalIndex = gxVertex.NormalIndex;
              // TODO: For collision models, there can be normal indices when
              // there are 0 normals. What does this mean? Is this how surface
              // types are defined?
              if (normalIndex != null && mod.vnormals.Count > 0) {
                if (!vertexDescriptor.UseNbt) {
                  var normal = finModCache.NormalsByIndex[normalIndex.Value];
                  finVertex.SetLocalNormal(normal);
                } else {
                  var normal = finModCache.NbtNormalsByIndex[normalIndex.Value];
                  var tangent = finModCache.TangentsByIndex[normalIndex.Value];
                  finVertex.SetLocalNormal(normal);
                  finVertex.SetLocalTangent(tangent);
                }
              }

              var colorIndex = gxVertex.Color0IndexOrValue?.AsT0;
              if (colorIndex != null) {
                var color = finModCache.ColorsByIndex[colorIndex.Value];
                finVertex.SetColor(color);
              }

              for (var t = 0; t < 8; ++t) {
                var texCoordIndex = gxVertex.GetTexCoord(t);
                if (texCoordIndex != null) {
                  var texCoord
                      = finModCache.TexCoordsByIndex[t][texCoordIndex.Value];
                  finVertex.SetUv(t, texCoord);
                }
              }
            }

            var finVertices = finVertexList.ToArray();
            IPrimitive? primitive = gxPrimitive.PrimitiveType switch {
                GxPrimitiveType.GX_TRIANGLE_FAN => finMesh.AddTriangleFan(finVertices),
                GxPrimitiveType.GX_TRIANGLE_STRIP => finMesh.AddTriangleStrip(
                    finVertices),
                _ => throw new ArgumentOutOfRangeException()
            };
            primitive.SetMaterial(finMaterial);
          }
        }
      }
    }
  }

  private static void AddSortedMatPolysSiblings_(
      LinkedList<JointMatPoly> sortedMatPolys,
      IEnumerable<Joint> joints,
      ListDictionary<Joint, Joint> childrenByJoint,
      IReadOnlyList<Material> modMaterials,
      MaterialFlags targetFlags) {
    foreach (var joint in joints) {
      if (childrenByJoint.TryGetList(joint, out var children)) {
        AddSortedMatPolysSiblings_(sortedMatPolys,
                                   children,
                                   childrenByJoint,
                                   modMaterials,
                                   targetFlags);
      }

      foreach (var matPoly in joint.matpolys) {
        var modMaterial = modMaterials[matPoly.matIdx];
        if ((modMaterial.flags & targetFlags) != 0) {
          sortedMatPolys.AddFirst(matPoly);
        }
      }
    }
  }

  private record FinModCache {
    public Vector3[] PositionsByIndex { get; }

    public Vector3[] NormalsByIndex { get; }

    public Vector3[] NbtNormalsByIndex { get; }
    public Vector4[] TangentsByIndex { get; }

    public IColor[] ColorsByIndex { get; }

    public Vector2[][] TexCoordsByIndex { get; }

    public FinModCache(Mod mod) {
      this.PositionsByIndex = mod.vertices.ToArray();
      this.NormalsByIndex = mod.vnormals.ToArray();
      this.NbtNormalsByIndex =
          mod.vertexnbt.Select(vertexnbt => vertexnbt.Normal).ToArray();
      this.TangentsByIndex
          = mod.vertexnbt.Select(vertexnbt => new Vector4(vertexnbt.Tangent, 0))
               .ToArray();
      this.ColorsByIndex =
          mod.vcolours.Select(color => (IColor) color).ToArray();
      this.TexCoordsByIndex =
          mod.texcoords.Select(texcoords => texcoords.ToArray())
             .ToArray();
    }
  }

  private readonly struct TextureImageReader(
      IList<Texture> srcTextures,
      IList<IReadOnlyImage[]> dstImages)
      : IAction {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Invoke(int index)
      => dstImages[index] = srcTextures[index].ToMipmapImages();
  }
}