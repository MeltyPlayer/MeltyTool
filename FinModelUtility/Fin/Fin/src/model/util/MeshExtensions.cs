using System.Collections.Generic;
using System.Linq;
using System.Numerics;

using fin.math.floats;

namespace fin.model.util;

public static class MeshExtensions {
  public static IPrimitive AddSimpleQuad<TVertex>(
      this IMesh mesh,
      ISkin<TVertex> skin,
      Vector3 ul,
      Vector3 ur,
      Vector3 lr,
      Vector3 ll,
      IMaterial? material = null,
      IReadOnlyBone? bone = null)
      where TVertex : INormalVertex, ISingleUvVertex {
    var a = ul;
    var b = ur;
    var c = lr;

    var normal = Vector3.Cross(b - a, c - a);
    normal = Vector3.Normalize(normal);

    var vUl = skin.AddVertex(ul);
    vUl.SetUv(0, 0);
    vUl.SetLocalNormal(normal);

    var vUr = skin.AddVertex(ur);
    vUr.SetUv(1, 0);
    vUr.SetLocalNormal(normal);

    var vLr = skin.AddVertex(lr);
    vLr.SetUv(1, 1);
    vLr.SetLocalNormal(normal);

    var vLl = skin.AddVertex(ll);
    vLl.SetUv(0, 1);
    vLl.SetLocalNormal(normal);

    if (bone != null) {
      var boneWeights
          = skin.GetOrCreateBoneWeights(VertexSpace.RELATIVE_TO_BONE, bone);
      vUl.SetBoneWeights(boneWeights);
      vUr.SetBoneWeights(boneWeights);
      vLr.SetBoneWeights(boneWeights);
      vLl.SetBoneWeights(boneWeights);
    }

    return mesh.AddQuads(vUl, vUr, vLr, vLl).SetMaterial(material);
  }

  public static IPrimitive[] AddSimpleCube<TVertex>(
      this IMesh mesh,
      ISkin<TVertex> skin,
      Vector3 point1,
      Vector3 point2,
      IMaterial? material = null,
      IReadOnlyBone? bone = null)
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

    var primitives = new LinkedList<IPrimitive>();
    if (!sameX && !sameY) {
      // Top
      primitives.AddLast(
          mesh.AddSimpleQuad(skin, tUl, tUr, tLr, tLl, material, bone));
      // Bottom
      primitives.AddLast(
          mesh.AddSimpleQuad(skin, bUr, bUl, bLl, bLr, material, bone));
    }

    if (!sameX && !sameZ) {
      // Front
      primitives.AddLast(
          mesh.AddSimpleQuad(skin, tLl, tLr, bLr, bLl, material, bone));
      // Back
      primitives.AddLast(
          mesh.AddSimpleQuad(skin, tUr, tUl, bUl, bUr, material, bone));
    }

    if (!sameY && !sameZ) {
      // Left
      primitives.AddLast(
          mesh.AddSimpleQuad(skin, tUl, tLl, bLl, bUl, material, bone));
      // Right
      primitives.AddLast(
          mesh.AddSimpleQuad(skin, tLr, tUr, bUr, bLr, material, bone));
    }

    return primitives.ToArray();
  }
}