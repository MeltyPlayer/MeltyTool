using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;

using Assimp;

using fin.animation.keyframes;
using fin.animation.types.vector3;
using fin.color;
using fin.data.lazy;
using fin.data.queues;
using fin.image;
using fin.image.formats;
using fin.io;
using fin.math.matrix.four;
using fin.math.rotations;
using fin.math.transform;
using fin.model.impl;
using fin.model.util;
using fin.util.sets;

using SixLabors.ImageSharp.PixelFormats;

namespace fin.model.io.importers.assimp;

public class AssimpModelImporter : IModelImporter<AssimpModelFileBundle> {
  public IModel Import(AssimpModelFileBundle modelFileBundle) {
    var mainFile = modelFileBundle.MainFile;

    var files = mainFile.AsFileSet();
    var finModel = new ModelImpl {
        FileBundle = modelFileBundle,
        Files = files
    };

    using var ctx = new AssimpContext();
    var assScene = ctx.ImportFile(mainFile.FullPath);

    // Adds materials
    var lazyFinSatelliteImages
        = new LazyCaseInvariantStringDictionary<IImage>(
            path => {
              if (mainFile.AssertGetParent()
                          .TryToGetExistingFile(path, out var imageFile)) {
                files.Add(imageFile);
                return FinImage.FromFile(imageFile);
              }

              return FinImage.Create1x1FromColor(Color.Magenta);
            });
    var lazyFinEmbeddedImages = new LazyDictionary<EmbeddedTexture, IImage>(
        assEmbeddedImage => {
          if (assEmbeddedImage.IsCompressed) {
            using var stream =
                new MemoryStream(assEmbeddedImage.CompressedData);
            return FinImage.FromStream(stream);
          }

          var finImage = new Rgba32Image(PixelFormat.RGBA8888,
                                         assEmbeddedImage.Width,
                                         assEmbeddedImage.Height);

          using var imgLock = finImage.Lock();
          var pixels = imgLock.Pixels;
          for (var y = 0; y < finImage.Height; ++y) {
            for (var x = 0; x < finImage.Width; ++x) {
              var texel =
                  assEmbeddedImage.NonCompressedData[y * finImage.Width + x];
              pixels[y * finImage.Width + x] =
                  new Rgba32(texel.R, texel.G, texel.B, texel.A);
            }
          }

          return finImage;
        });
    var lazyFinTextures = new LazyDictionary<TextureSlot, ITexture?>(
        assTextureSlot => {
          var fileName =
              assTextureSlot.FilePath ??
              assScene.Textures[assTextureSlot.TextureIndex].Filename;
          var file = new FinFile(fileName);
          var name = file.NameWithoutExtension;

          var finImage = assTextureSlot.FilePath != null
              ? lazyFinSatelliteImages[assTextureSlot.FilePath]
              : lazyFinEmbeddedImages[
                  assScene.Textures[assTextureSlot.TextureIndex]];

          var finTexture = finModel.MaterialManager.CreateTexture(finImage);
          finTexture.Name = name;

          finTexture.WrapModeU = ConvertWrapMode_(assTextureSlot.WrapModeU);
          finTexture.WrapModeV = ConvertWrapMode_(assTextureSlot.WrapModeV);

          return finTexture;
        });
    var lazyFinMaterials = new LazyDictionary<int, IMaterial>(
        assMaterialIndex => {
          var assMaterial = assScene.Materials[assMaterialIndex];

          // TODO: Handle all billion properties within assMaterial
          var finMaterial = finModel
                            .MaterialManager.AddStandardMaterial();
          finMaterial.Name = assMaterial.Name;

          if (assMaterial.HasTextureDiffuse) {
            finMaterial.DiffuseTexture =
                lazyFinTextures[assMaterial.TextureDiffuse];
          }

          if (assMaterial.HasTextureNormal) {
            finMaterial.NormalTexture =
                lazyFinTextures[assMaterial.TextureNormal];
          }

          if (assMaterial.HasTextureAmbientOcclusion) {
            finMaterial.AmbientOcclusionTexture =
                lazyFinTextures[assMaterial.TextureAmbientOcclusion];
          }

          if (assMaterial.HasTextureSpecular) {
            finMaterial.SpecularTexture =
                lazyFinTextures[assMaterial.TextureSpecular];
          }

          if (assMaterial.HasTextureEmissive) {
            finMaterial.EmissiveTexture =
                lazyFinTextures[assMaterial.TextureEmissive];
          }

          return finMaterial;
        });

    // Adds rig
    var finSkeleton = finModel.Skeleton;
    var finBoneByName = new Dictionary<string, IBone>();
    var nodeAndBoneQueue =
        new FinQueue<(Node, IBone)>((assScene.RootNode, finSkeleton.Root));
    while (nodeAndBoneQueue.TryDequeue(out var nodeAndBone)) {
      var (assNode, finBone) = nodeAndBone;

      var name = assNode.Name;
      finBone.Name = name;
      finBoneByName[name] = finBone;

      finBone.LocalTransform.SetMatrix(
          Matrix4x4ConversionUtil.ConvertAssimpToFin(assNode.Transform));

      nodeAndBoneQueue.Enqueue(
          assNode.Children.Select(childNode
                                      => (childNode,
                                          finBone.AddChild(0, 0, 0))));
    }

    // Adds animations
    foreach (var assAnimation in assScene.Animations) {
      var finAnimation = finModel.AnimationManager.AddAnimation();
      finAnimation.Name = assAnimation.Name;

      var frameRate = finAnimation.FrameRate =
          (float) Math.Round(assAnimation.TicksPerSecond);
      finAnimation.FrameCount =
          (int) Math.Round(assAnimation.DurationInTicks / frameRate);

      if (assAnimation.HasNodeAnimations) {
        foreach (var assNodeAnimationChannel in assAnimation
                     .NodeAnimationChannels) {
          var finBone = finBoneByName[assNodeAnimationChannel.NodeName];
          var finBoneTracks = finAnimation.AddBoneTracks(finBone);

          if (assNodeAnimationChannel.HasPositionKeys) {
            var positionTrack = finBoneTracks.UseCombinedPositionAxesTrack();
            foreach (var assPositionKey in assNodeAnimationChannel
                         .PositionKeys) {
              var frame = (int) Math.Round(assPositionKey.Time / frameRate);
              var assPosition = assPositionKey.Value;
              positionTrack.SetKeyframe(
                  frame,
                  new Vector3(assPosition.X, assPosition.Y, assPosition.Z));
            }
          }

          if (assNodeAnimationChannel.HasRotationKeys) {
            var rotationTrack = finBoneTracks.UseQuaternionRotationTrack();
            foreach (var assRotationKey in assNodeAnimationChannel
                         .RotationKeys) {
              var frame = (int) Math.Round(assRotationKey.Time / frameRate);
              var assQuaternion = assRotationKey.Value;
              rotationTrack.SetKeyframe(frame,
                                        new System.Numerics.Quaternion(
                                            assQuaternion.X,
                                            assQuaternion.Y,
                                            assQuaternion.Z,
                                            assQuaternion.W));
            }
          }

          if (assNodeAnimationChannel.HasScalingKeys) {
            var scaleTrack = finBoneTracks.UseSeparateScaleAxesTrack();
            foreach (var assScaleKey in assNodeAnimationChannel.ScalingKeys) {
              var frame = (int) Math.Round(assScaleKey.Time / frameRate);
              var assScale = assScaleKey.Value;
              scaleTrack.Add(new Keyframe<Vector3>(
                                 frame,
                                 new Vector3(
                                     assScale.X,
                                     assScale.Y,
                                     assScale.Z)
                             ));
            }
          }
        }
      }
    }

    // Adds skin
    var finSkin = finModel.Skin;
    foreach (var assMesh in assScene.Meshes) {
      var finMesh = finSkin.AddMesh();
      finMesh.Name = assMesh.Name;

      var finVertices =
          assMesh.Vertices
                 .Select(assPosition => finSkin.AddVertex(
                             assPosition.X,
                             assPosition.Y,
                             assPosition.Z))
                 .ToArray();

      if (assMesh.HasNormals) {
        for (var i = 0; i < finVertices.Length; ++i) {
          var assNormal = assMesh.Normals[i];
          finVertices[i]
              .SetLocalNormal(assNormal.X, assNormal.Y, assNormal.Z);
        }
      }

      var usesColorBytes = assMesh.VertexColorChannels
                                  .Where((_, i) => assMesh.HasVertexColors(i))
                                  .SelectMany(a => a)
                                  .Any(c => float.IsNaN(c.R));

      // TODO: Add support for colors
      for (var colorIndex = 0;
           colorIndex < assMesh.VertexColorChannelCount;
           colorIndex++) {
        if (!assMesh.HasVertexColors(colorIndex)) {
          continue;
        }

        var assColors = assMesh.VertexColorChannels[colorIndex];
        for (var i = 0; i < finVertices.Length; ++i) {
          var assColor = assColors[i];

          // This is so janky, what the heck is wrong with Assimp???
          if (usesColorBytes) {
            var floatValue = assColor.R;

            var intValue = Unsafe.As<float, int>(ref floatValue);
            FinColor.SplitRgba(intValue,
                               out var r,
                               out var g,
                               out var b,
                               out var a);

            finVertices[i]
                .SetColor(colorIndex, FinColor.FromRgbaBytes(r, g, b, a));
          } else {
            finVertices[i]
                .SetColor(colorIndex,
                          FinColor.FromRgbaFloats(
                              assColor.R,
                              assColor.G,
                              assColor.B,
                              assColor.A));
          }
        }
      }

      for (var uvIndex = 0;
           uvIndex < assMesh.TextureCoordinateChannelCount;
           uvIndex++) {
        if (!assMesh.HasTextureCoords(uvIndex)) {
          continue;
        }

        var assUvs = assMesh.TextureCoordinateChannels[uvIndex];
        for (var i = 0; i < finVertices.Length; ++i) {
          var assUv = assUvs[i];
          finVertices[i].SetUv(uvIndex, assUv.X, 1 - assUv.Y);
        }
      }

      // TODO: How to optimize this??
      if (assMesh.HasBones) {
        for (var i = 0; i < finVertices.Length; ++i) {
          var boneWeights = new List<IBoneWeight>();
          foreach (var assBone in assMesh.Bones) {
            foreach (var vertexWeight in assBone.VertexWeights) {
              if (vertexWeight.VertexID == i && vertexWeight.Weight > 0) {
                var finBone = finBoneByName[assBone.Name];

                var offsetMatrix = Matrix4x4ConversionUtil.ConvertAssimpToFin(
                        assBone.OffsetMatrix)
                    .TransposeInPlace();
                IFinMatrix4x4 finInverseBindMatrix = offsetMatrix;

                boneWeights.Add(new BoneWeight(finBone,
                                               finInverseBindMatrix,
                                               vertexWeight.Weight));
              }
            }
          }

          finVertices[i]
              .SetBoneWeights(
                  finSkin.GetOrCreateBoneWeights(
                      VertexSpace.RELATIVE_TO_WORLD,
                      boneWeights.ToArray()));
        }
      }

      var faceVertices =
          assMesh.GetIndices().Select(i => finVertices[i]).ToArray();
      var finPrimitive = assMesh.PrimitiveType switch {
          Assimp.PrimitiveType.Point    => finMesh.AddPoints(faceVertices),
          Assimp.PrimitiveType.Line     => finMesh.AddLines(faceVertices),
          Assimp.PrimitiveType.Triangle => finMesh.AddTriangles(faceVertices),
      };
      finPrimitive.SetVertexOrder(VertexOrder.NORMAL)
                  .SetMaterial(lazyFinMaterials[assMesh.MaterialIndex]);
    }

    return finModel;
  }

  private static WrapMode ConvertWrapMode_(TextureWrapMode assWrapMode)
    => assWrapMode switch {
        TextureWrapMode.Wrap   => WrapMode.REPEAT,
        TextureWrapMode.Clamp  => WrapMode.CLAMP,
        TextureWrapMode.Mirror => WrapMode.MIRROR_REPEAT,
    };
}