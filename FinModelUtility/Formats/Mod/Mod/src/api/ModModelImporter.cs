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
using fin.util.image;
using fin.util.lists;

using gx;
using gx.impl;
using gx.vertex;

using mod.schema.anm;
using mod.schema.mod;
using mod.util;

using schema.binary;


namespace mod.api;

public class ModModelImporter : IModelImporter<ModModelFileBundle> {
  private const bool DEDUPLICATE_MATERIALS = false;

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

    /*var colInfos = mod.colltris.collinfo;
    if (colInfos.Count != 0) {
      os.WriteLine();
      os.WriteLine("o collision mesh");
      foreach (var colInfo in colInfos) {
        os.WriteLine(
            $"f ${colInfo.indice.X + 1} ${colInfo.indice.Y + 1} ${colInfo.indice.Z + 1}");
      }
    }*/

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

    var materialAnimation = model.AnimationManager.AddAnimation();
    materialAnimation.FrameRate = 30;

    var lazyTextureDictionary = new GxLazyTextureDictionary<Material, string>(
        model,
        (gxTextureBundle, modMaterial)
            => $"{gxTextureBundle.GetHashCode()},{modMaterial.GetHashCode()}",
        (gxTextureBundle, modMaterial, finTexture) => {
          var modTextureData
              = modMaterial.texInfo.TexturesInMaterial[
                  (int) gxTextureBundle.TexMap];
          var animationLength = modTextureData.AnimationLength;
          if (animationLength == 0) {
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
    Func<int, Material, IMaterial>
        getFinMaterialFromModMaterial = (i, modMaterial) => {
          lazyTextureDictionary.State = modMaterial;

          IMaterial finMaterial;
          if (modMaterial.flags.CheckFlag(MaterialFlags.HIDDEN)) {
            finMaterial = model.MaterialManager.AddHiddenMaterial();
          } else if (modMaterial.flags.CheckFlag(MaterialFlags.ENABLED)) {
            var modPopulatedMaterial =
                new ModPopulatedMaterial(
                    i,
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

          finMaterial.Name = $"material{i}";

          return finMaterial;
        };

    var finMaterialByModMaterial = new LazyDictionary<Material, IMaterial>(
        (dict, modMaterial) => {
          var i = dict.Count;
          return getFinMaterialFromModMaterial(i, modMaterial);
        });
    var modMaterialAndFinMaterialByIndex
        = new LazyDictionary<int, (Material, IMaterial)>(
            index => {
              var modMaterial = mod.materials.materials[index];

              IMaterial finMaterial;
              if (DEDUPLICATE_MATERIALS) {
                finMaterial = finMaterialByModMaterial[modMaterial];
              } else {
                finMaterial = getFinMaterialFromModMaterial(index, modMaterial);
              }

              return (modMaterial, finMaterial);
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
      var (modMaterial, finMaterial)
          = modMaterialAndFinMaterialByIndex[matPoly.matIdx];

      this.AddMesh_(mod,
                    mesh,
                    modMaterial,
                    finMaterial,
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
      Material modMaterial,
      IMaterial finMaterial,
      ModelImpl model,
      IReadOnlyList<IBone> bones,
      IBoneWeights[] envelopeBoneWeights,
      FinModCache finModCache) {
    var vertexDescriptor = new VertexDescriptor();
    vertexDescriptor.FromPikmin1(mesh.vtxDescriptor, mod.hasNormals);

    var vertexDescriptorValues = vertexDescriptor.ToArray();

    var finSkin = model.Skin;
    var finMesh = finSkin.AddMesh();

    foreach (var meshPacket in mesh.packets) {
      foreach (var dlist in meshPacket.displaylists) {
        var br =
            new SchemaBinaryReader(dlist.dlistData, Endianness.BigEndian);

        while (!br.Eof) {
          var opcode = (GxOpcode) br.ReadByte();
          if (opcode == GxOpcode.NOP) {
            continue;
          }

          if (opcode != GxOpcode.DRAW_TRIANGLE_STRIP &&
              opcode != GxOpcode.DRAW_TRIANGLE_FAN) {
            continue;
          }

          var faceCount = br.ReadUInt16();
          var positionIndices = new List<ushort>();
          var allVertexWeights = new List<IBoneWeights>();
          var normalIndices = new List<ushort>();
          var color0Indices = new List<ushort>();

          var texCoordIndices = new List<ushort>[8];
          for (var t = 0; t < 8; ++t) {
            texCoordIndices[t] = [];
          }

          for (var f = 0; f < faceCount; f++) {
            foreach (var (attr, format) in vertexDescriptorValues) {
              if (format == null) {
                var unused = br.ReadByte();

                if (attr == GxVertexAttribute.PosMatIdx) {
                  // Internally, this represents which of the 10 active
                  // matrices to bind to.
                  var activeMatrixIndex = unused / 3;

                  Asserts.Equal(0, unused % 3);

                  // This represents which vertex matrix the active matrix is
                  // sourced from.
                  var vertexMatrixIndex =
                      meshPacket.indices[activeMatrixIndex];

                  // -1 means no active matrix set by this display list,
                  // defers to whatever the existing matrix is in this slot.
                  if (vertexMatrixIndex == -1) {
                    vertexMatrixIndex =
                        this.activeMatrices_[activeMatrixIndex];
                    Asserts.False(vertexMatrixIndex == -1);
                  }

                  this.activeMatrices_[activeMatrixIndex] = vertexMatrixIndex;

                  // TODO: Is there a real name for this?
                  // Remaps from vertex matrix to "attachment" index.
                  var attachmentIndex =
                      mod.vtxMatrix[vertexMatrixIndex].index;

                  // Positive indices refer to joints/bones.
                  if (attachmentIndex >= 0) {
                    var boneIndex = attachmentIndex;
                    allVertexWeights.Add(
                        finSkin.GetOrCreateBoneWeights(
                            VertexSpace.RELATIVE_TO_BONE,
                            bones[boneIndex]));
                  }
                  // Negative indices refer to envelopes.
                  else {
                    var envelopeIndex = -1 - attachmentIndex;
                    allVertexWeights.Add(envelopeBoneWeights[envelopeIndex]);
                  }
                } else {
                  ;
                }

                continue;
              }

              if (attr == GxVertexAttribute.Position) {
                positionIndices.Add(Read_(br, format));
              } else if (attr == GxVertexAttribute.Normal) {
                normalIndices.Add(Read_(br, format));
              } else if (attr == GxVertexAttribute.Color0) {
                color0Indices.Add(Read_(br, format));
              } else if (attr is >= GxVertexAttribute.Tex0Coord
                                 and <= GxVertexAttribute.Tex7Coord) {
                texCoordIndices[attr - GxVertexAttribute.Tex0Coord]
                    .Add(Read_(br, format));
              } else if (format == GxAttributeType.INDEX_16) {
                br.ReadUInt16();
              } else {
                Asserts.Fail(
                    $"Unexpected attribute/format ({attr}/{format})");
              }
            }
          }

          var finVertexList = new List<IReadOnlyVertex>();
          for (var v = 0; v < positionIndices.Count; ++v) {
            var position = finModCache.PositionsByIndex[positionIndices[v]];
            var finVertex =
                model.Skin.AddVertex(position);

            if (allVertexWeights.Count > 0) {
              finVertex.SetBoneWeights(allVertexWeights[v]);
            }

            // TODO: For collision models, there can be normal indices when
            // there are 0 normals. What does this mean? Is this how surface
            // types are defined?
            if (normalIndices.Count > 0 && mod.vnormals.Count > 0) {
              var normalIndex = normalIndices[v];

              if (!vertexDescriptor.useNbt) {
                var normal = finModCache.NormalsByIndex[normalIndex];
                finVertex.SetLocalNormal(normal);
              } else {
                var normal = finModCache.NbtNormalsByIndex[normalIndex];
                var tangent = finModCache.TangentsByIndex[normalIndex];
                finVertex.SetLocalNormal(normal);
                finVertex.SetLocalTangent(tangent);
              }
            }

            if (color0Indices.Count > 0) {
              var color = finModCache.ColorsByIndex[color0Indices[v]];
              finVertex.SetColor(color);
            } else {
              finVertex.SetColor(finModCache.Default);
            }

            for (var t = 0; t < 8; ++t) {
              if (texCoordIndices[t].Count > 0) {
                var texCoord =
                    finModCache.TexCoordsByIndex[t][texCoordIndices[t][v]];
                finVertex.SetUv(t, texCoord);
              }
            }

            finVertexList.Add(finVertex);
          }

          var finVertices = finVertexList.ToArray();
          IPrimitive? primitive = null;
          if (opcode == GxOpcode.DRAW_TRIANGLE_FAN) {
            primitive
                = finMesh.AddTriangleFan(
                    (IReadOnlyList<IReadOnlyVertex>) finVertices);
          } else if (opcode == GxOpcode.DRAW_TRIANGLE_STRIP) {
            primitive = finMesh.AddTriangleStrip(
                (IReadOnlyList<IReadOnlyVertex>) finVertices);
          }

          if (primitive != null) {
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

    public IColor Default { get; } =
      FinColor.FromRgbaBytes(255, 255, 255, 255);

    public Vector2[][] TexCoordsByIndex { get; }

    public FinModCache(Mod mod) {
      this.PositionsByIndex =
          mod.vertices.Select(
                 position => new Vector3(
                     position.X,
                     position.Y,
                     position.Z
                 ))
             .ToArray();
      this.NormalsByIndex =
          mod.vnormals.Select(
                 vnormals => new Vector3(
                     vnormals.X,
                     vnormals.Y,
                     vnormals.Z
                 ))
             .ToArray();
      this.NbtNormalsByIndex =
          mod.vertexnbt.Select(vertexnbt => new Vector3(
                                   vertexnbt.Normal.X,
                                   vertexnbt.Normal.Y,
                                   vertexnbt.Normal.Z
                               ))
             .ToArray();
      this.TangentsByIndex = mod.vertexnbt.Select(
                                    vertexnbt => new Vector4(
                                        vertexnbt.Tangent.X,
                                        vertexnbt.Tangent.Y,
                                        vertexnbt.Tangent.Z,
                                        0
                                    ))
                                .ToArray();
      this.ColorsByIndex =
          mod.vcolours.Select(color => (IColor) color).ToArray();
      this.TexCoordsByIndex =
          mod.texcoords.Select(
                 texcoords
                     => texcoords.Select(
                                     texcoord => new Vector2(
                                         texcoord.X,
                                         texcoord.Y))
                                 .ToArray())
             .ToArray();
    }
  }

  private static ushort Read_(IBinaryReader br,
                              GxAttributeType? format) {
    if (format == GxAttributeType.INDEX_16) {
      return br.ReadUInt16();
    }

    if (format == GxAttributeType.INDEX_8) {
      return br.ReadByte();
    }

    Asserts.Fail($"Unsupported format: {format}");
    return 0;
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