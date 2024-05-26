using System.Numerics;

using fin.math;
using fin.model;
using fin.schema.vector;

using glo.schema;

namespace glo.api {
  public class KnownDarkVerticesHack {
    private static HashSet<string> knownDarkVertexMeshAndTextureNames_
        = [
            // hub1
            "atlport.newbit3.bmp",
            "caveback.darkrk.bmp",
            "pirpot.newbit3.bmp",
            "wellbot.tchest3.bmp",
            // wayroom_bak
            "entrexit.newbit5.bmp",
            "enti.water1g.bmp",
            "entii.water1g.bmp",
            "entiii.water1g.bmp",
            "bossrm.water1g.bmp",
            "bonusrm.water1g.bmp",
            "enti.cavewew.bmp",
            "entii.cavewew.bmp",
            "entiii.cavewew.bmp",
            "bossrm.cavewew.bmp",
            "bonusrm.cavewew.bmp",
        ];

    private readonly Dictionary<GloVertexRef, Vector3> knownDarkVertexRefs_
        = new();

    public void IdentifyDarkVertices(
        IModel model,
        IReadOnlyList<(GloMesh, IBone)> gloMeshesAndFinBones) {
      var boneTransformManager = new BoneTransformManager();
      boneTransformManager.CalculateStaticMatricesForManualProjection(
          model,
          true);

      var knownDarkVertexPositions
          = new HashSet<Vector3>(new Vector3EqualityComparer(5));

      foreach (var (gloMesh, finBone) in gloMeshesAndFinBones) {
        foreach (var gloFace in gloMesh.Faces) {
          if (!knownDarkVertexMeshAndTextureNames_.Contains(
                  $"{gloMesh.Name}.{gloFace.TextureFilename}")) {
            continue;
          }

          foreach (var gloVertexRef in gloFace.VertexRefs) {
            var position = (Vector3) gloMesh.Vertices[gloVertexRef.Index];
            boneTransformManager.ProjectPosition(finBone, ref position);
            knownDarkVertexPositions.Add(position);
          }
        }
      }

      foreach (var (gloMesh, finBone) in gloMeshesAndFinBones) {
        foreach (var gloFace in gloMesh.Faces) {
          foreach (var gloVertexRef in gloFace.VertexRefs) {
            var orig = (Vector3) gloMesh.Vertices[gloVertexRef.Index];
            var position = (Vector3) gloMesh.Vertices[gloVertexRef.Index];
            boneTransformManager.ProjectPosition(finBone, ref position);

            if (knownDarkVertexPositions.TryGetValue(
                    position,
                    out var actualPosition)) {
              var test = position;
              boneTransformManager.UnprojectPosition(finBone, ref test);

              boneTransformManager.UnprojectPosition(
                  finBone,
                  ref actualPosition);
              this.knownDarkVertexRefs_[gloVertexRef] = actualPosition;
            }
          }
        }
      }
    }

    public bool IsDarkVertex(GloVertexRef vertexRef, out Vector3 actualPosition)
      => this.knownDarkVertexRefs_.TryGetValue(vertexRef, out actualPosition);
  }
}