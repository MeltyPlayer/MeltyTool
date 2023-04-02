using fin.data.queue;
using System.Collections.Generic;
using System.Linq;

using fin.math.matrix;
using fin.model;

using SharpGLTF.Schema2;

namespace fin.exporter.gltf {
  using GltfNode = Node;
  using GltfSkin = Skin;

  public class GltfSkeletonBuilder {
    public (GltfNode, IBone)[] BuildAndBindSkeleton(
        GltfNode rootNode,
        GltfSkin skin,
        float scale,
        ISkeleton skeleton) {
      var rootBone = skeleton.Root;

      var boneQueue = new FinQueue<(GltfNode, IBone)>((rootNode, rootBone));

      var skinNodesAndBones = new List<(GltfNode, IBone)>();
      while (boneQueue.Count > 0) {
        var (node, bone) = boneQueue.Dequeue();

        this.ApplyBoneOrientationToNode_(node, bone, scale);

        if (bone != rootBone) {
          skinNodesAndBones.Add((node, bone));
        }

        boneQueue.Enqueue(bone.Children.Select(child => (node.CreateNode(child.Name), child)));
      }

      var skinNodes = skinNodesAndBones
                      .Select(skinNodesAndBone => skinNodesAndBone.Item1)
                      .ToArray();
      if (skinNodes.Length > 0) {
        skin.BindJoints(skinNodes);
      } else {
        var nullNode = rootNode.CreateNode("null");
        skin.BindJoints(nullNode);
        skinNodesAndBones.Add((nullNode, null));
      }

      return skinNodesAndBones.ToArray();
    }

    private void ApplyBoneOrientationToNode_(GltfNode node,
                                             IBone bone,
                                             float scale) {
      var bonePosition = bone.LocalPosition;
      var scaledPosition = new Position(bonePosition.X * scale,
                                        bonePosition.Y * scale,
                                        bonePosition.Z * scale);
      node.LocalMatrix = MatrixTransformUtil.FromTrs(scaledPosition,
                                      bone.LocalRotation,
                                      bone.LocalScale).Impl;
    }
  }
}