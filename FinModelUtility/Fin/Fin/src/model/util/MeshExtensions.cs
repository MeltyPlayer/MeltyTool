using System.Numerics;

using fin.math.floats;

namespace fin.model.util;

public static class MeshExtensions {
  public static void AddSimpleQuad<TVertex>(
      this IMesh mesh,
      ISkin<TVertex> skin,
      Vector3 ul,
      Vector3 ur,
      Vector3 lr,
      Vector3 ll,
      IMaterial? material = null,
      IReadOnlyBone? bone = null,
      (float, float)? repeat = null)
      where TVertex : INormalVertex, ISingleUvVertex {
    var a = ul;
    var b = ur;
    var c = lr;

    var normal = Vector3.Cross(b - a, c - a);
    normal = Vector3.Normalize(normal);

    // TODO: Support non-rectangular quad UVs
    float rightU = repeat?.Item1 ?? 1;
    float lowerV = repeat?.Item2 ?? 1;

    var vUl = skin.AddVertex(ul);
    vUl.SetUv(0, 0);
    vUl.SetLocalNormal(normal);

    var vUr = skin.AddVertex(ur);
    vUr.SetUv(rightU, 0);
    vUr.SetLocalNormal(normal);

    var vLr = skin.AddVertex(lr);
    vLr.SetUv(rightU, lowerV);
    vLr.SetLocalNormal(normal);

    var vLl = skin.AddVertex(ll);
    vLl.SetUv(0, lowerV);
    vLl.SetLocalNormal(normal);

    if (bone != null) {
      var boneWeights
          = skin.GetOrCreateBoneWeights(VertexSpace.RELATIVE_TO_BONE, bone);
      vUl.SetBoneWeights(boneWeights);
      vUr.SetBoneWeights(boneWeights);
      vLr.SetBoneWeights(boneWeights);
      vLl.SetBoneWeights(boneWeights);
    }

    mesh.AddQuads(vUl, vUr, vLr, vLl).SetMaterial(material);
  }

  public static void AddSimpleWall<TVertex>(
      this IMesh mesh,
      ISkin<TVertex> skin,
      Vector3 point1,
      Vector3 point2,
      IMaterial? material = null,
      IReadOnlyBone? bone = null,
      (float, float)? repeat = null)
      where TVertex : INormalVertex, ISingleUvVertex {
    var ul = point1;
    var ur = point1 with { X = point2.X, Y = point2.Y };
    var lr = point2;
    var ll = point2 with { X = point1.X, Y = point1.Y };

    mesh.AddSimpleQuad(skin, ul, ur, lr, ll, material, bone, repeat);
    mesh.AddSimpleQuad(skin, ur, ul, ll, lr, material, bone, repeat);
  }

  public static void AddSimpleCube<TVertex>(
      this IMesh mesh,
      ISkin<TVertex> skin,
      Vector3 point1,
      Vector3 point2,
      IMaterial? material = null,
      IReadOnlyBone? bone = null,
      (float, float, float)? repeat = null)
      where TVertex : INormalVertex, ISingleUvVertex {
    var tUl = point1;
    var tUr = point1 with { X = point2.X };
    var tLr = point1 with { X = point2.X, Y = point2.Y };
    var tLl = point1 with { Y = point2.Y };

    var bUl = point2 with { X = point1.X, Y = point1.Y };
    var bUr = point2 with { Y = point1.Y };
    var bLr = point2;
    var bLl = point2 with { X = point1.X };

    var sameX = point1.X.IsRoughly(point2.X);
    var sameY = point1.Y.IsRoughly(point2.Y);
    var sameZ = point1.Z.IsRoughly(point2.Z);

    var (xRepeat, yRepeat, zRepeat) = repeat ?? (1, 1, 1);

    if (!sameX && !sameY) {
      var topBottomRepeat = (xRepeat, yRepeat);

      // Top
      mesh.AddSimpleQuad(skin, tUl, tUr, tLr, tLl, material, bone, topBottomRepeat);
      // Bottom
      mesh.AddSimpleQuad(skin, bUr, bUl, bLl, bLr, material, bone, topBottomRepeat);
    }

    if (!sameX && !sameZ) {
      var frontBackRepeat = (xRepeat, zRepeat);

      // Front
      mesh.AddSimpleQuad(skin, tLl, tLr, bLr, bLl, material, bone, frontBackRepeat);
      // Back
      mesh.AddSimpleQuad(skin, tUr, tUl, bUl, bUr, material, bone, frontBackRepeat);
    }

    if (!sameY && !sameZ) {
      var leftRightRepeat = (yRepeat, zRepeat);

      // Left
      mesh.AddSimpleQuad(skin, tUl, tLl, bLl, bUl, material, bone, leftRightRepeat);
      // Right
      mesh.AddSimpleQuad(skin, tLr, tUr, bUr, bLr, material, bone, leftRightRepeat);
    }
  }
}