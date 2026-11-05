using System.Numerics;

using fin.animation.keyframes;
using fin.color;
using fin.data.dictionaries;
using fin.data.lazy;
using fin.data.queues;
using fin.io;
using fin.math.rotations;
using fin.math.transform;
using fin.model;
using fin.model.impl;
using fin.model.io;
using fin.model.io.importers;
using fin.model.util;
using fin.util.enumerables;

using mp64.schema;
using mp64.schema.form;

using schema.binary;

namespace mp64.api;

public sealed record FormModelFileBundle(
    IReadOnlyTreeFile MainFile,
    IReadOnlyTreeFile[] MtnxFiles)
    : IModelFileBundle<FormModelFileBundle, FormModelImporter> {
  public IEnumerable<IReadOnlyStandaloneFile> Files
    => this.MainFile.Yield().Concat(this.MtnxFiles);
}

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/PartyPlanner64/PartyPlanner64/blob/980d043217823d0e5901ee93d3a50e36f728db10/packages/lib/models/FormToThreeJs.ts
/// </summary>
public sealed class FormModelImporter
    : IModelImporter<FormModelImporter, FormModelFileBundle> {
  public IModel Import(FormModelFileBundle fileBundle) {
    var form = fileBundle.MainFile.ReadNew<Form>(Endianness.BigEndian);
    var mtnxes = fileBundle.MtnxFiles
                           .Select(f => (f.NameWithoutExtension.ToString(),
                                         f.ReadNew<Mtnx>(Endianness.BigEndian)))
                           .ToArray();

    var model = new ModelImpl {
        FileBundle = fileBundle,
        Files = fileBundle.Files.ToHashSet(),
    };

    var formChunks = form.Chunks.Select(c => c.Impl.Data).ToArray();

    var attributes
        = formChunks.OfType<Atr1Data>().SingleOrDefault()?.Attributes ?? [];
    var bmp1s = formChunks.OfType<Bmp1Data>().ToArray();
    var pal1ByGlobalIndex
        = formChunks.OfType<Pal1Data>().ToDictionary(p => p.GlobalIndex);
    var bmpImageByGlobalIndex
        = bmp1s.Select(b => (b, b.ToImage(pal1ByGlobalIndex)))
               .ToDictionary(t => t.b.GlobalIndex, t => t.Item2);

    var col1 = formChunks.OfType<Col1Data>().SingleOrDefault();
    var colors = col1?.Colors ?? [];
    var mat1 = formChunks.OfType<Mat1Data>().SingleOrDefault();
    var materials = mat1?.Materials ?? [];

    var colorByMaterialIndex
        = materials.Select(m => colors[m.ColorIndex]).ToArray();

    var lazyFinMaterials
        = new LazyDictionary<(int attributeIndex, int materialIndex),
            IReadOnlyMaterial>(tuple => {
          var (attributeIndex, materialIndex) = tuple;

          var color = colorByMaterialIndex[materialIndex];

          if (attributeIndex != -1) {
            var attribute = attributes[attributeIndex];
            var image = bmpImageByGlobalIndex[attribute.BmpGlobalIndex];
            var (material, texture)
                = model.MaterialManager
                       .AddSimpleTextureMaterialFromImage(
                           image);
            texture.WrapModeU = attribute.XBehavior switch {
                WrapBehavior.MIRRORED_REPEAT => WrapMode
                    .MIRROR_REPEAT,
                WrapBehavior.REPEAT => WrapMode
                    .REPEAT,
                WrapBehavior.CLAMP => WrapMode
                    .CLAMP,
            };
            texture.WrapModeV = attribute.YBehavior switch {
                WrapBehavior.MIRRORED_REPEAT => WrapMode
                    .MIRROR_REPEAT,
                WrapBehavior.REPEAT => WrapMode
                    .REPEAT,
                WrapBehavior.CLAMP => WrapMode
                    .CLAMP,
            };

            material.DiffuseColor = color.ToSystemColor();
            material.Shininess = 0;
            return material;
          } else {
            var material = model.MaterialManager.AddColorMaterial(color.ToSystemColor());
            material.Shininess = 0;
            return material;
          }
        });

    var obj1 = formChunks.OfType<Obj1Data>().SingleOrDefault();
    var objects = obj1?.Objects.ToArray() ?? [];
    var objByGlobalIndex
        = obj1?.Objects.ToDictionary(o => o.GlobalIndex) ??
          [];

    var fac1 = formChunks.OfType<Fac1Data>().SingleOrDefault();
    var allFaces = (fac1?.Faces ?? []).AsSpan();

    var vtx1 = formChunks.OfType<Vtx1Data>().SingleOrDefault();
    var allVertices = vtx1?.Vertices ?? [];

    var strg = formChunks.OfType<StrgData>().SingleOrDefault();
    var allStrings = strg?.Strings ?? [];

    var vertexScale = vtx1?.Scale ?? 1;
    var lazyVertices
        = new LazyDictionary<(ushort vtxI, short materialIndex, Vector2 uv,
            IReadOnlyBoneWeights
            boneWeights),
            IReadOnlyVertex>(tuple => {
          var (vtxI, materialIndex, uv, boneWeights) = tuple;
          var v = allVertices[vtxI];
          var vertex
              = model.Skin.AddVertex(v.Position.AsVector3() * vertexScale);
          vertex.SetLocalNormal(Vector3.Normalize(v.Normal.AsVector3()));
          vertex.SetUv(uv);

          if (materialIndex != -1) {
            vertex.SetColor(colorByMaterialIndex[materialIndex]);
          }

          vertex.SetBoneWeights(boneWeights);

          return vertex;
        });

    var boneByGlobalIndex = new Dictionary<ushort, IReadOnlyBone>();

    var globalIndexByWeirdIndex = objects.OfType<Object0x3d>()
                                         .Select(o => o.GlobalIndex)
                                         .ToArray();

    var objQueue = new FinTuple2Queue<IBone, ushort>((model.Skeleton.Root, 0));
    while (objQueue.TryDequeue(out var parentBone,
                               out var currentGlobalIndex)) {
      var obj = objByGlobalIndex[currentGlobalIndex];
      var transform = obj.Transform;

      IBone bone = parentBone;
      if (transform != null) {
        bone = parentBone.AddChild(transform.Position);
        bone.Transform.SetRotationDegrees(transform.Rotation);
        bone.Transform.SetScale(transform.Scale);

        boneByGlobalIndex[currentGlobalIndex] = bone;
      }

      var name = allStrings[currentGlobalIndex];
      bone.Name = name;

      switch (obj) {
        case Object0x10: {
          // TODO: Support
          break;
        }
        case Object0x3a obj0x3a: {
          var skin = model.Skin;
          var mesh = skin.AddMesh();
          mesh.Name = name;
          var boneWeights
              = skin.GetOrCreateBoneWeights(VertexSpace.RELATIVE_TO_BONE, bone);
          var faces = allFaces.Slice(obj0x3a.FaceIndex, obj0x3a.FaceCount);
          foreach (var face in faces) {
            var vertices
                = face.VtxEntries
                      .Select(v => lazyVertices[
                                  (v.VertexIndex, v.MaterialIndex, v.Uv,
                                   boneWeights)])
                      .ToArray();

            IPrimitive primitive;
            if (vertices.Length == 3) {
              primitive = mesh.AddTriangles(vertices);
            } else {
              primitive = mesh.AddQuads(vertices);
            }

            primitive.SetMaterial(
                lazyFinMaterials[(face.AtrIndex, face.MaterialIndex)]);
            primitive.SetVertexOrder(VertexOrder.COUNTER_CLOCKWISE);
          }

          break;
        }
        case Object0x3d obj0x3d: {
          objQueue.Enqueue(obj0x3d.Children.Select(c => (bone, c)));
          break;
        }
      }
    }

    foreach (var (mtnxName, mtnx) in mtnxes) {
      var animation = model.AnimationManager.AddAnimation();
      animation.Name = mtnxName;
      animation.FrameRate = 60;
      animation.FrameCount = mtnx.FrameCount;

      var mtnxTrackByObj = mtnx.Tracks.Select(t => t.Data)
                               .ToListDictionary(t => globalIndexByWeirdIndex[
                                                     t.WeirdObjIndex]);

      foreach (var (globalIndex, mtnxTracks) in mtnxTrackByObj.GetPairs()) {
        var boneTracks
            = animation.GetOrCreateBoneTracks(boneByGlobalIndex[globalIndex]);

        var positionXTrack
            = mtnxTracks.Find(t => t is {
                Type: MtnxTrackType.POSITION, Dimension: MtnxDimension.X
            });
        var positionYTrack
            = mtnxTracks.Find(t => t is {
                Type: MtnxTrackType.POSITION, Dimension: MtnxDimension.Y
            });
        var positionZTrack
            = mtnxTracks.Find(t => t is {
                Type: MtnxTrackType.POSITION, Dimension: MtnxDimension.Z
            });

        if (positionXTrack != null ||
            positionYTrack != null ||
            positionZTrack != null) {
          var positionKeyframes = boneTracks.UseSeparateTranslationKeyframes();

          foreach (var (frame, value) in positionXTrack?.Keyframes ?? []) {
            positionKeyframes.Axes[0].SetKeyframe(frame, value.X);
          }

          foreach (var (frame, value) in positionYTrack?.Keyframes ?? []) {
            positionKeyframes.Axes[1].SetKeyframe(frame, value.X);
          }

          foreach (var (frame, value) in positionZTrack?.Keyframes ?? []) {
            positionKeyframes.Axes[2].SetKeyframe(frame, value.X);
          }
        }

        var rotationXTrack
            = mtnxTracks.Find(t => t is {
                Type: MtnxTrackType.ROTATION, Dimension: MtnxDimension.X
            });
        var rotationYTrack
            = mtnxTracks.Find(t => t is {
                Type: MtnxTrackType.ROTATION, Dimension: MtnxDimension.Y
            });
        var rotationZTrack
            = mtnxTracks.Find(t => t is {
                Type: MtnxTrackType.ROTATION, Dimension: MtnxDimension.Z
            });

        if (rotationXTrack != null ||
            rotationYTrack != null ||
            rotationZTrack != null) {
          var rotationKeyframes = boneTracks.UseSeparateEulerRadiansKeyframes();

          foreach (var (frame, value) in rotationXTrack?.Keyframes ?? []) {
            rotationKeyframes.Axes[0]
                             .SetKeyframe(frame, value.X * FinTrig.DEG_2_RAD);
          }

          foreach (var (frame, value) in rotationYTrack?.Keyframes ?? []) {
            rotationKeyframes.Axes[1]
                             .SetKeyframe(frame, value.X * FinTrig.DEG_2_RAD);
          }

          foreach (var (frame, value) in rotationZTrack?.Keyframes ?? []) {
            rotationKeyframes.Axes[2]
                             .SetKeyframe(frame, value.X * FinTrig.DEG_2_RAD);
          }
        }

        var scaleXTrack
            = mtnxTracks.Find(t => t is {
                Type: MtnxTrackType.SCALE, Dimension: MtnxDimension.X
            });
        var scaleYTrack
            = mtnxTracks.Find(t => t is {
                Type: MtnxTrackType.SCALE, Dimension: MtnxDimension.Y
            });
        var scaleZTrack
            = mtnxTracks.Find(t => t is {
                Type: MtnxTrackType.SCALE, Dimension: MtnxDimension.Z
            });

        if (scaleXTrack != null ||
            scaleYTrack != null ||
            scaleZTrack != null) {
          var scaleKeyframes = boneTracks.UseSeparateScaleKeyframes();

          foreach (var (frame, value) in scaleXTrack?.Keyframes ?? []) {
            scaleKeyframes.Axes[0].SetKeyframe(frame, value.X);
          }

          foreach (var (frame, value) in scaleYTrack?.Keyframes ?? []) {
            scaleKeyframes.Axes[1].SetKeyframe(frame, value.X);
          }

          foreach (var (frame, value) in scaleZTrack?.Keyframes ?? []) {
            scaleKeyframes.Axes[2].SetKeyframe(frame, value.X);
          }
        }
      }
    }

    return model;
  }
}