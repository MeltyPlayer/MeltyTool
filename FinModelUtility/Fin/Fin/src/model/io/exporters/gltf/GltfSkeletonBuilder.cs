using System.Linq;

using fin.data.queues;
using fin.math;

using SharpGLTF.Schema2;

namespace fin.model.io.exporters.gltf;

using GltfNode = Node;
using GltfSkin = Skin;

public sealed class GltfSkeletonBuilder {
  public static (GltfNode, IReadOnlyBone)[] BuildAndBindSkeleton(
      GltfNode rootNode,
      GltfSkin skin,
      float scale,
      IReadOnlySkeleton skeleton) {
    var rootBone = skeleton.Root;

    var boneQueue
        = new FinQueue<(GltfNode, IReadOnlyBone)>((rootNode, rootBone));

    var skinNodesAndBones
        = new (GltfNode, IReadOnlyBone)[skeleton.Bones.Count];
    while (boneQueue.Count > 0) {
      var (node, bone) = boneQueue.Dequeue();

      ApplyBoneOrientationToNode_(node, bone, scale);

      skinNodesAndBones[bone.Index] = (node, bone);

      boneQueue.Enqueue(
          bone.Children.Select(child => (
                                   node.CreateNode(child.Name), child)));
    }

    var skinNodes = skinNodesAndBones
                    .Select(skinNodesAndBone => skinNodesAndBone.Item1)
                    .ToArray();
    skin.BindJoints(skinNodes);

    return skinNodesAndBones.ToArray();
  }

  private static void ApplyBoneOrientationToNode_(GltfNode node,
                                                  IReadOnlyBone bone,
                                                  float scale) {
    var finTransform = bone.Transform;
    node.WithLocalTranslation(finTransform.LocalTranslation * scale);

    if (!finTransform.LocalRotation.IsIdentity) {
      node.WithLocalRotation(finTransform.LocalRotation);
    }

    if (!finTransform.LocalScale.IsRoughly1()) {
      node.WithLocalScale(finTransform.LocalScale);
    }
  }
}