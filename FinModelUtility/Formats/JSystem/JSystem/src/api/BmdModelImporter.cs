using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

using fin;
using fin.animation.keyframes;
using fin.io;
using fin.log;
using fin.math.matrix.four;
using fin.math.transform;
using fin.model;
using fin.model.impl;
using fin.model.io.importers;
using fin.model.util;
using fin.schema.matrix;
using fin.util.asserts;

using gx;

using jsystem._3D_Formats;
using jsystem.exporter;
using jsystem.GCN;
using jsystem.schema.j3dgraph.bcx;
using jsystem.schema.j3dgraph.bmd.inf1;
using jsystem.schema.j3dgraph.bmd.jnt1;
using jsystem.schema.j3dgraph.bmd.mat3;
using jsystem.schema.jutility.bti;

using schema.binary;

namespace jsystem.api;

using MkdsNode = MA.Node;
using GxPrimitiveType = BMD.SHP1Section.Batch.Packet.Primitive.GXPrimitive;

public class BmdModelImporter : IModelImporter<BmdModelFileBundle> {
  public IModel Import(BmdModelFileBundle modelFileBundle) {
      var logger = Logging.Create<BmdModelImporter>();

      var bmd = new BMD(modelFileBundle.BmdFile.ReadAllBytes());

      List<(string, IBcx)>? pathsAndBcxs;
      try {
        pathsAndBcxs =
            modelFileBundle
                .BcxFiles?
                .Select(bcxFile => {
                  var extension = bcxFile.FileType.ToLower();
                  IBcx bcx = extension switch {
                    ".bca" => new Bca(bcxFile.ReadAllBytes()),
                    ".bck" => new Bck(bcxFile.ReadAllBytes()),
                    _      => throw new NotSupportedException(),
                  };
                  return (FullName: bcxFile.FullPath, bcx);
                })
                .ToList();
      } catch {
        logger.LogError("Failed to load BCX!");
        throw;
      }

      List<(string, Bti)>? pathsAndBtis;
      try {
        pathsAndBtis =
            modelFileBundle
                .BtiFiles?
                .Select(btiFile
                            => (FullName: btiFile.FullPath,
                                btiFile.ReadNew<Bti>(
                                    Endianness.BigEndian)))
                .ToList();
      } catch {
        logger.LogError("Failed to load BTI!");
        throw;
      }

      var model = new ModelImpl {
          FileBundle = modelFileBundle,
          Files = modelFileBundle.Files.ToHashSet()
      };

      var materialManager =
          new BmdMaterialManager(model, bmd, pathsAndBtis);

      var jointsAndBones = this.ConvertBones_(model, bmd);
      this.ConvertAnimations_(model,
                              bmd,
                              pathsAndBcxs,
                              modelFileBundle.FrameRate,
                              jointsAndBones);
      this.ConvertMesh_(model, bmd, jointsAndBones, materialManager);

      return model;
    }

  private (MkdsNode, IBone)[] ConvertBones_(IModel model, BMD bmd) {
      var joints = bmd.GetJoints();

      var jointsAndBones = new (MkdsNode, IBone)[joints.Length];
      var jointIdToBone = new Dictionary<int, IBone>();

      for (var j = 0; j < joints.Length; ++j) {
        var node = joints[j];

        var parentBone = node.ParentJointIndex == -1
                             ? model.Skeleton.Root
                             : jointIdToBone[node.ParentJointIndex];

        var joint = node.Entry;
        var jointName = node.Name;

        var rotationFactor = 1f / 32768f * 3.14159f;
        var bone = parentBone.AddChild(joint.Translation);
        bone.LocalTransform.SetRotationRadians(
                joint.Rotation.X * rotationFactor,
                joint.Rotation.Y * rotationFactor,
                joint.Rotation.Z * rotationFactor);
        bone.LocalTransform.SetScale(joint.Scale);
        bone.Name = jointName;

        bone.IgnoreParentScale = node.Entry.IgnoreParentScale;

        // TODO: How to do this without hardcoding???
        if (node.Entry.JointType == JointType.MANUAL) {
          if (node.Name.StartsWith("balloon")) {
            var rotateYaw =
                Quaternion.CreateFromYawPitchRoll(MathF.PI / 2, 0, 0);
            var rotatePitch =
                Quaternion.CreateFromYawPitchRoll(0, -MathF.PI / 2, 0);
            bone.AlwaysFaceTowardsCamera(rotateYaw * rotatePitch);
          }

          // Japanese word for light
          if (node.Name.StartsWith("hikair") ||
              node.Name.StartsWith("hikari")) {
            var rotateYaw =
                Quaternion.CreateFromYawPitchRoll(-MathF.PI / 2, 0, 0);
            bone.AlwaysFaceTowardsCamera(rotateYaw);
          }
        }

        jointsAndBones[j] = (node, bone);
        jointIdToBone[j] = bone;
      }

      return jointsAndBones;
    }

  private void ConvertAnimations_(
      IModel model,
      BMD bmd,
      IList<(string, IBcx)>? pathsAndBcxs,
      float frameRate,
      (MkdsNode, IBone)[] jointsAndBones) {
      var bcxCount = pathsAndBcxs?.Count ?? 0;
      for (var a = 0; a < bcxCount; ++a) {
        var (bcxPath, bcx) = pathsAndBcxs![a];
        var animationName = new FileInfo(bcxPath).Name.Split('.')[0];

        var animation = model.AnimationManager.AddAnimation();
        animation.Name = animationName;

        animation.FrameCount = bcx.Anx1.FrameCount;
        animation.FrameRate = frameRate;

        // Writes translation/rotation/scale for each joint.
        foreach (var (joint, bone) in jointsAndBones) {
          var jointIndex = bmd.JNT1.Data.StringTable[joint.Name];

          if (FinConstants.ALLOW_INVALID_JOINT_INDICES &&
              (jointIndex < 0 || jointIndex >= bcx.Anx1.Joints.Length)) {
            // TODO: What does this mean???
            continue;
          }
          var bcxJoint = bcx.Anx1.Joints[jointIndex];

          var boneTracks = animation.AddBoneTracks(bone);

          // TODO: Handle mirrored animations
          var positions = boneTracks.UseSeparateTranslationKeyframesWithTangents(
              bcxJoint.Values.Translations[0].Length,
              bcxJoint.Values.Translations[1].Length,
              bcxJoint.Values.Translations[2].Length);
          for (var i = 0; i < bcxJoint.Values.Translations.Length; ++i) {
            foreach (var key in bcxJoint.Values.Translations[i]) {
              if (key is Bck.ANK1Section.AnimatedJoint.JointAnim.Key bckKey) {
                positions.Axes[i]
                         .Add(new KeyframeWithTangents<float>(
                                  bckKey.Frame,
                                  bckKey.Value,
                                  bckKey.IncomingTangent,
                                  bckKey.OutgoingTangent));
              } else {
                positions.Axes[i]
                         .Add(new KeyframeWithTangents<float>(
                                  key.Frame,
                                  key.Value));
              }
            }
          }

          var rotations = boneTracks.UseEulerRadiansRotationTrack(
              bcxJoint.Values.Rotations[0].Length,
              bcxJoint.Values.Rotations[1].Length,
              bcxJoint.Values.Rotations[2].Length);
          for (var i = 0; i < bcxJoint.Values.Rotations.Length; ++i) {
            foreach (var key in bcxJoint.Values.Rotations[i]) {
              if (key is Bck.ANK1Section.AnimatedJoint.JointAnim.Key bckKey) {
                rotations.Set(bckKey.Frame, i, bckKey.Value, bckKey.IncomingTangent, bckKey.OutgoingTangent);
              } else {
                rotations.Set(key.Frame, i, key.Value);
              }
            }
          }

          var scales = boneTracks.UseSeparateScaleKeyframesWithTangents(
              bcxJoint.Values.Scales[0].Length,
              bcxJoint.Values.Scales[1].Length,
              bcxJoint.Values.Scales[2].Length);
          for (var i = 0; i < bcxJoint.Values.Scales.Length; ++i) {
            foreach (var key in bcxJoint.Values.Scales[i]) {
              if (key is Bck.ANK1Section.AnimatedJoint.JointAnim.Key bckKey) {
                scales.Axes[i]
                      .Add(new KeyframeWithTangents<float>(
                               bckKey.Frame,
                               bckKey.Value,
                               bckKey.IncomingTangent,
                               bckKey.OutgoingTangent));
              } else {
                scales.Axes[i]
                      .Add(new KeyframeWithTangents<float>(
                               key.Frame,
                               key.Value));
              }
            }
          }
        }
      }
    }

  private void ConvertMesh_(
      ModelImpl model,
      BMD bmd,
      (MkdsNode, IBone)[] jointsAndBones,
      BmdMaterialManager materialManager) {
      var finSkin = model.Skin;
      // TODO: Actually split this up
      var finMesh = finSkin.AddMesh();

      var joints = bmd.GetJoints();

      var vertexPositions = bmd.VTX1.Positions;
      var vertexNormals = bmd.VTX1.Normals;
      var vertexColors = bmd.VTX1.Colors;
      var vertexUvs = bmd.VTX1.Texcoords;
      var entries = bmd.INF1.Data.Entries;
      var batches = bmd.SHP1.Batches;

      var scheduledDrawOnWayDownPrimitives = new List<IPrimitive>();
      var scheduledDrawOnWayUpPrimitives = new List<IPrimitive>();

      GxFixedFunctionMaterial? currentMaterial = null;
      MaterialEntry? currentMaterialEntry = null;

      uint currentRenderIndex = 1;

      var weightsTable = new IBoneWeights?[10];
      foreach (var entry in entries) {
        switch (entry.Type) {
          case Inf1EntryType.TERMINATOR:
            goto DoneRendering;

          case Inf1EntryType.HIERARCHY_DOWN: {
              foreach (var primitive in scheduledDrawOnWayDownPrimitives) {
                primitive.SetInversePriority(currentRenderIndex++);
              }
              scheduledDrawOnWayDownPrimitives.Clear();
              break;
            }

          case Inf1EntryType.HIERARCHY_UP: {
              foreach (var primitive in scheduledDrawOnWayUpPrimitives) {
                primitive.SetInversePriority(currentRenderIndex++);
              }
              scheduledDrawOnWayUpPrimitives.Clear();
              break;
            }

          case Inf1EntryType.MATERIAL:
            currentMaterial = materialManager.Get(entry.Index);
            currentMaterialEntry =
                bmd.MAT3.MaterialEntries[
                    bmd.MAT3.MaterialEntryIndieces[entry.Index]];
            break;

          case Inf1EntryType.SHAPE:
            var batch = batches[(int)entry.Index];

            // TODO: Pass matrix type into joint (how?)
            var matrixType = batch.MatrixType;

            foreach (var packet in batch.Packets) {
              // Updates contents of matrix table
              for (var i = 0; i < packet.MatrixTable.Length; ++i) {
                var matrixTableIndex = packet.MatrixTable[i];

                // Max value means keep old value.
                if (matrixTableIndex == ushort.MaxValue) {
                  continue;
                }

                var drw1 = bmd.DRW1.Data;
                var isWeighted = drw1.IsWeighted[matrixTableIndex];
                var drw1Index = drw1.Data[matrixTableIndex];

                BoneWeight[] weights;
                if (isWeighted) {
                  var weightedIndices = bmd.EVP1.WeightedIndices[drw1Index];
                  weights = new BoneWeight[weightedIndices.Indices.Length];
                  for (var w = 0; w < weightedIndices.Indices.Length; ++w) {
                    var jointIndex = weightedIndices.Indices[w];
                    var weight = weightedIndices.Weights[w];

                    if (jointIndex >= joints.Length) {
                      throw new InvalidDataException();
                    }

                    var skinToBoneMatrix =
                        BmdModelImporter.ConvertSchemaToFin_(
                            bmd.EVP1.InverseBindMatrices[jointIndex]);

                    var bone = jointsAndBones[jointIndex].Item2;
                    weights[w] = new BoneWeight(bone, skinToBoneMatrix, weight);
                  }
                }
                // Unweighted bones are simple, just gets our precomputed limb
                // matrix
                else {
                  var jointIndex = drw1Index;
                  if (jointIndex >= joints.Length) {
                    throw new InvalidDataException();
                  }

                  var bone = jointsAndBones[jointIndex].Item2;
                  weights = [
                      new BoneWeight(bone, FinMatrix4x4Util.IDENTITY, 1)
                  ];
                }
                weightsTable[i] =
                    finSkin.GetOrCreateBoneWeights(
                        VertexSpace.RELATIVE_TO_BONE, weights);
              }

              foreach (var primitive in packet.Primitives) {
                var points = primitive.Points;
                var pointsCount = points.Length;
                var vertices = new IVertex[pointsCount];

                var weightsUsedByPrimitive = new HashSet<IBoneWeights>();
                for (var p = 0; p < pointsCount; ++p) {
                  var point = points[p];

                  if (!batch.HasPositions) {
                    throw new Exception(
                        "How can a point not have a position??");
                  }
                  var position = vertexPositions[point.PosIndex];
                  var vertex =
                      finSkin.AddVertex(position.X, position.Y, position.Z);
                  vertices[p] = vertex;

                  if (batch.HasNormals) {
                    var normal = vertexNormals[point.NormalIndex];
                    vertex.SetLocalNormal(normal.X, normal.Y, normal.Z);
                  }

                  var matrixIndex = point.MatrixIndex / 3;
                  var weights = weightsTable[matrixIndex];
                  if (weights != null) {
                    weightsUsedByPrimitive.Add(weights);
                    vertex.SetBoneWeights(weights);
                  }

                  for (var c = 0; c < 2; ++c) {
                    if (batch.HasColors[c]) {
                      var colorIndex = point.ColorIndex[c];
                      var color = vertexColors[c][colorIndex];
                      vertex.SetColorBytes(c,
                                           color.R,
                                           color.G,
                                           color.B,
                                           color.A);
                    }
                  }

                  for (var i = 0; i < 8; ++i) {
                    if (batch.HasTexCoords[i]) {
                      var texCoord = vertexUvs[i][point.TexCoordIndex[i]];
                      vertex.SetUv(i, texCoord.S, texCoord.T);
                    }
                  }
                }

                var gxPrimitiveType = primitive.Type;

                Asserts.Nonnull(currentMaterial);

                IPrimitive finPrimitive;
                switch (gxPrimitiveType) {
                  case GxPrimitiveType.GX_TRIANGLES: {
                      finPrimitive = finMesh.AddTriangles(vertices)
                                            .SetMaterial(
                                                currentMaterial.Material);
                      break;
                    }

                  case GxPrimitiveType.GX_TRIANGLESTRIP: {
                      finPrimitive =
                          finMesh.AddTriangleStrip(vertices)
                                 .SetMaterial(currentMaterial.Material);
                      break;
                    }

                  case GxPrimitiveType.GX_TRIANGLEFAN: {
                      finPrimitive = finMesh.AddTriangleFan(vertices)
                                            .SetMaterial(
                                                currentMaterial.Material);
                      break;
                    }

                  case GxPrimitiveType.GX_QUADS: {
                      finPrimitive =
                          finMesh.AddQuads(vertices)
                                 .SetMaterial(currentMaterial.Material);
                      break;
                    }

                  default:
                    throw new NotSupportedException(
                        $"Unsupported primitive type: {gxPrimitiveType}");
                }

                var renderOrder = currentMaterialEntry?.RenderOrder ??
                                  RenderOrder.DRAW_ON_WAY_DOWN;
                switch (renderOrder) {
                  case RenderOrder.DRAW_ON_WAY_DOWN: {
                      scheduledDrawOnWayDownPrimitives.Add(finPrimitive);
                      break;
                    }
                  case RenderOrder.DRAW_ON_WAY_UP: {
                      scheduledDrawOnWayUpPrimitives.Add(finPrimitive);
                      break;
                    }
                  default: throw new ArgumentOutOfRangeException();
                }
              }
            }
            break;
        }
      }

      DoneRendering:;
    }

  private static IFinMatrix4x4 ConvertSchemaToFin_(Matrix3x4f schemaMatrix) {
      var finMatrix = new FinMatrix4x4().SetIdentity();

      for (var r = 0; r < 3; ++r) {
        for (var c = 0; c < 4; ++c) {
          finMatrix[c, r] = schemaMatrix[r, c];
        }
      }

      return finMatrix;
    }
}