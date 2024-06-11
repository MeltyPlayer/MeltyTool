using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using CommunityToolkit.HighPerformance.Helpers;

using fin.data.dictionaries;
using fin.data.lazy;
using fin.data.queues;
using fin.image;
using fin.io;
using fin.math;
using fin.model;
using fin.model.impl;
using fin.model.io.importers;
using fin.util.asserts;
using fin.util.enumerables;

using grezzo.material;
using grezzo.schema.cmb;
using grezzo.schema.cmb.skl;
using grezzo.schema.cmb.tex;
using grezzo.schema.csab;
using grezzo.schema.ctxb;
using grezzo.schema.shpa;

using schema.binary;

using Version = grezzo.schema.cmb.Version;

namespace grezzo.api {
  public class CmbModelImporter : IModelImporter<CmbModelFileBundle> {
    // TODO: Split these out into separate classes
    // TODO: Reading from the file here is gross
    public IModel Import(CmbModelFileBundle modelFileBundle) {
      var cmbFile = modelFileBundle.CmbFile;
      var csabFiles = modelFileBundle.CsabFiles;
      var ctxbFiles = modelFileBundle.CtxbFiles;
      var shpaFiles = modelFileBundle.ShpaFiles;

      var fps = 30;

      var cmb = cmbFile.ReadNew<Cmb>();

      (IReadOnlyTreeFile, Csab)[] filesAndCsabs;
      if (csabFiles == null) {
        filesAndCsabs = Array.Empty<(IReadOnlyTreeFile, Csab)>();
      } else {
        filesAndCsabs = new (IReadOnlyTreeFile, Csab)[csabFiles.Count];
        ParallelHelper.For(0,
                           csabFiles.Count,
                           new CsabReader(csabFiles, filesAndCsabs));
      }

      var filesAndCtxbs =
          ctxbFiles?.Select(ctxbFile => {
            var ctxb = ctxbFile.ReadNew<Ctxb>();
            return (ctxbFile, ctxb);
          })
                   .ToList() ??
          new List<(IReadOnlyTreeFile shpaFile, Ctxb ctxb)>();

      var filesAndShpas =
          shpaFiles?.Select(shpaFile => {
            var shpa =
                shpaFile.ReadNew<Shpa>(
                    Endianness.LittleEndian);
            return (shpaFile, shpa);
          })
                   .ToList() ??
          [];

      var finModel = new ModelImpl((int) cmb.vatr.maxIndex);
      var finSkin = finModel.Skin;

      // Adds bones
      var cmbBones = cmb.skl.Data.bones;
      var boneChildren = new ListDictionary<Bone, Bone>();
      foreach (var bone in cmbBones) {
        var parentId = bone.parentId;
        if (parentId != -1) {
          boneChildren.Add(cmbBones[parentId], bone);
        }
      }

      var finBones = new IBone[cmbBones.Length];
      var boneQueue =
          new FinTuple2Queue<Bone, IBone?>((cmbBones[0], null));
      while (boneQueue.TryDequeue(out var cmbBone, out var finBoneParent)) {
        var translation = cmbBone.translation;
        var radians = cmbBone.rotation;
        var scale = cmbBone.scale;

        var finBone =
            (finBoneParent ?? finModel.Skeleton.Root)
            .AddChild(translation.X, translation.Y, translation.Z)
            .SetLocalRotationRadians(radians.X, radians.Y, radians.Z)
            .SetLocalScale(scale.X, scale.Y, scale.Z);
        finBones[cmbBone.id] = finBone;

        if (boneChildren.TryGetList(cmbBone, out var children)) {
          boneQueue.Enqueue(children!.Select(child => (child, finBone)));
        }
      }

      // Adds animations
      foreach (var (csabFile, csab) in filesAndCsabs) {
        var finAnimation = finModel.AnimationManager.AddAnimation();
        finAnimation.Name = csabFile.NameWithoutExtension;

        finAnimation.FrameCount = 1 + (int) csab.Duration;
        finAnimation.FrameRate = fps;

        foreach (var (boneIndex, anod) in csab.BoneIndexToAnimationNode) {
          var boneTracks = finAnimation.AddBoneTracks(
              finBones[boneIndex]);

          var positionsTrack =
              boneTracks.UseSeparatePositionAxesTrack(
                  anod.TranslationAxes[0].Keyframes.Count,
                  anod.TranslationAxes[1].Keyframes.Count,
                  anod.TranslationAxes[2].Keyframes.Count);
          var rotationsTrack =
              boneTracks.UseEulerRadiansRotationTrack(
                  anod.RotationAxes[0].Keyframes.Count,
                  anod.RotationAxes[1].Keyframes.Count,
                  anod.RotationAxes[2].Keyframes.Count);
          var scalesTrack =
              boneTracks.UseScaleTrack(
                  anod.ScaleAxes[0].Keyframes.Count,
                  anod.ScaleAxes[1].Keyframes.Count,
                  anod.ScaleAxes[2].Keyframes.Count);

          for (var i = 0; i < 3; ++i) {
            var translationAxis = anod.TranslationAxes[i];
            foreach (var translation in translationAxis.Keyframes) {
              positionsTrack.Set((int) translation.Time,
                                 i,
                                 translation.Value,
                                 translation.IncomingTangent,
                                 translation.OutgoingTangent);
            }

            var rotationAxis = anod.RotationAxes[i];
            foreach (var rotation in rotationAxis.Keyframes) {
              rotationsTrack.Set((int) rotation.Time,
                                 i,
                                 rotation.Value,
                                 rotation.IncomingTangent,
                                 rotation.OutgoingTangent);
            }

            var scaleAxis = anod.ScaleAxes[i];
            foreach (var scale in scaleAxis.Keyframes) {
              scalesTrack.Set((int) scale.Time,
                              i,
                              scale.Value,
                              scale.IncomingTangent,
                              scale.OutgoingTangent);
            }
          }
        }
      }

      // TODO: Move these reads into the model reading logic
      var cmbTextures = cmb.tex.Data.textures;

      var textureImages = new LazyArray<IImage>(
          cmbTextures.Length,
          imageIndex => {
            var cmbTexture = cmbTextures[imageIndex];
            var textureImage = cmb.TextureImages?[imageIndex];

            IImage image;
            if (textureImage != null) {
              image = textureImage;
            } else {
              var ctxb =
                  filesAndCtxbs
                      .Select(fileAndCtxb => fileAndCtxb.Item2)
                      .FirstOrDefault(
                          ctxb => ctxb.Chunk.Entry.Name == cmbTexture.name);
              image = ctxb != null
                  ? cmbTexture.GetImageReader()
                              .ReadImage(ctxb.Chunk.Entry.Data)
                  : FinImage.Create1x1FromColor(Color.Magenta);
            }

            return image;
          });

      var cmbMaterials = cmb.mats.Data.Materials;

      var finMaterials = new LazyArray<IMaterial>(
          cmbMaterials.Length,
          index => new CmbFixedFunctionMaterial(
              finModel,
              cmb,
              index,
              textureImages).Material);

      // Creates meshes
      var verticesByIndex = new ListDictionary<int, IVertex>();

      // Adds meshes
      var sklm = cmb.sklm.Data;
      foreach (var cmbMesh in sklm.mshs.Meshes) {
        var shape = sklm.shapes.shapes[cmbMesh.shapeIndex];

        uint vertexCount = 0;
        var meshIndices = new List<uint>();
        foreach (var pset in shape.primitiveSets) {
          foreach (var index in pset.primitive.indices) {
            meshIndices.Add(index);
            vertexCount = Math.Max(vertexCount, index);
          }
        }

        ++vertexCount;

        var preproject = new bool?[vertexCount];
        var skinningModes = new SkinningMode?[vertexCount];
        foreach (var pset in shape.primitiveSets) {
          foreach (var index in pset.primitive.indices) {
            skinningModes[index] = pset.skinningMode;
            preproject[index] = pset.skinningMode != SkinningMode.Smooth;
          }
        }

        // Gets flags
        var inc = 1;
        var hasNrm = shape.vertFlags.GetBit(inc++);
        if (cmb.header.version > Version.OCARINA_OF_TIME_3D) {
          // Skip "HasTangents" for now
          inc++;
        }

        var hasClr = shape.vertFlags.GetBit(inc++);
        var hasUv0 = shape.vertFlags.GetBit(inc++);
        var hasUv1 = shape.vertFlags.GetBit(inc++);
        var hasUv2 = shape.vertFlags.GetBit(inc++);
        var hasBi = shape.vertFlags.GetBit(inc++);
        var hasBw = shape.vertFlags.GetBit(inc++);

        var boneWeightEnumerator
            = new BoneWeightMemoryEnumerator(cmb,
                                             shape,
                                             hasBi,
                                             hasBw,
                                             finBones);

        var finMesh = finSkin.AddMesh();

        // Get vertices
        var finVertices = new IVertex[vertexCount];

        var positionEnumerator =
            DataTypeUtil.Read(cmb.vatr.position, shape.position, 3)
                        .SeparateTriplets()
                        .ToMemoryEnumerator();
        var normalEnumerator =
            DataTypeUtil.Read(cmb.vatr.normal, shape.normal, 3)
                        .SeparateTriplets()
                        .ToMemoryEnumerator();
        var colorEnumerator =
            DataTypeUtil.Read(cmb.vatr.color, shape.color, 4)
                        .Select(v => (byte) (255 * v))
                        .SeparateQuadruplets()
                        .ToMemoryEnumerator();
        var uv0Enumerator =
            DataTypeUtil.Read(cmb.vatr.uv0, shape.uv0, 2)
                        .SeparatePairs()
                        .ToMemoryEnumerator();
        var uv1Enumerator =
            DataTypeUtil.Read(cmb.vatr.uv1, shape.uv1, 2)
                        .SeparatePairs()
                        .ToMemoryEnumerator();
        var uv2Enumerator =
            DataTypeUtil.Read(cmb.vatr.uv2, shape.uv2, 2)
                        .SeparatePairs()
                        .ToMemoryEnumerator();

        for (var i = 0; i < vertexCount; ++i) {
          var position = positionEnumerator.TryMoveNextAndGetCurrent();

          var finVertex = finSkin.AddVertex(position.X, position.Y, position.Z);
          finVertices[i] = finVertex;

          var index = (ushort) (shape.position.Start / 3 + i);
          verticesByIndex.Add(index, finVertex);

          if (hasNrm) {
            var normal = normalEnumerator.TryMoveNextAndGetCurrent();
            finVertex.SetLocalNormal(normal.X, normal.Y, normal.Z);
          }

          if (hasClr) {
            var color = colorEnumerator.TryMoveNextAndGetCurrent();
            finVertex.SetColorBytes(color.X, color.Y, color.Z, color.W);
          }

          if (hasUv0) {
            var uv0 = uv0Enumerator.TryMoveNextAndGetCurrent();
            finVertex.SetUv(0, uv0.X, 1 - uv0.Y);
          }

          if (hasUv1) {
            var uv1 = uv1Enumerator.TryMoveNextAndGetCurrent();
            finVertex.SetUv(1, uv1.X, 1 - uv1.Y);
          }

          if (hasUv2) {
            var uv2 = uv2Enumerator.TryMoveNextAndGetCurrent();
            finVertex.SetUv(2, uv2.X, 1 - uv2.Y);
          }

          var preprojectMode = preproject[i].Value
              ? VertexSpace.RELATIVE_TO_BONE
              : VertexSpace.RELATIVE_TO_WORLD;

          boneWeightEnumerator.TryMoveNextBones(
              out var bones,
              out var single);

          var boneCount = single ? 1 : shape.boneDimensions;

          if (hasBw) {
            var totalWeight = 0f;
            var boneWeights = new List<BoneWeight>();

            boneWeightEnumerator.TryMoveNextWeights(out var weightValues);

            for (var j = 0; j < boneCount; ++j) {
              var weight = weightValues[j];
              totalWeight += weight;

              if (weight > 0) {
                var bone = bones[j];
                var boneWeight = new BoneWeight(bone, null, weight);

                boneWeights.Add(boneWeight);
              }
            }

            Asserts.True(boneWeights.Count > 0);
            Asserts.True(Math.Abs(1 - totalWeight) < .0001);
            finVertex.SetBoneWeights(
                finSkin.GetOrCreateBoneWeights(preprojectMode,
                                               boneWeights.ToArray()));
          } else {
            finVertex.SetBoneWeights(
                finSkin.GetOrCreateBoneWeights(preprojectMode, bones[0]));
          }
        }

        // Adds faces. Thankfully, it's all just triangles!
        var triangleVertices = meshIndices
                               .Select(meshIndex => finVertices[meshIndex])
                               .ToArray();
        finMesh.AddTriangles(triangleVertices)
               .SetMaterial(finMaterials[cmbMesh.materialIndex]);
      }

      // Adds morph targets
      foreach (var (shpaFile, shpa) in filesAndShpas) {
        var shpaIndexToPosi =
            shpa?.Posi.Data.Values
                .Select((posi, i) => (shpa.Idxs.Indices[i], posi))
                .ToDictionary(indexAndPosi => indexAndPosi.Item1,
                              indexAndPosi => indexAndPosi.posi);

        var morphTarget = finModel.AnimationManager.AddMorphTarget();
        morphTarget.Name = shpaFile.NameWithoutExtension;

        foreach (var (index, position) in shpaIndexToPosi) {
          if (!verticesByIndex.TryGetList(index, out var finVertices)) {
            continue;
          }

          foreach (var finVertex in finVertices) {
            morphTarget.MoveTo(finVertex,
                               new Position(position.X,
                                            position.Y,
                                            position.Z));
          }
        }
      }

      return finModel;
    }

    public readonly struct CsabReader : IAction {
      private readonly IReadOnlyList<IReadOnlyTreeFile> src_;
      private readonly (IReadOnlyTreeFile, Csab)[] dst_;

      public CsabReader(
          IReadOnlyList<IReadOnlyTreeFile> src,
          (IReadOnlyTreeFile, Csab)[] dst) {
        this.src_ = src;
        this.dst_ = dst;
      }

      public void Invoke(int i) {
        var csabFile = this.src_[i];
        var csab =
            csabFile.ReadNew<Csab>(Endianness.LittleEndian);
        this.dst_[i] = (csabFile, csab);
      }
    }
  }
}