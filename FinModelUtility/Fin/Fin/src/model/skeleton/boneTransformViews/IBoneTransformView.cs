using System.Numerics;

namespace fin.model.skeleton.boneTransformViews;

public interface IBoneTransformView {
  bool TryGetWorldTranslation(out Vector3 translation);
  bool TryGetWorldRotation(out Quaternion rotation);
  bool TryGetWorldScale(out Vector3 scale);

  bool TryGetLocalTranslation(out Vector3 translation);
  bool TryGetLocalRotation(out Quaternion rotation);
  bool TryGetLocalScale(out Vector3 scale);
}

public interface ITargetedBoneTransformView : IBoneTransformView {
  void TargetBone(IReadOnlyBone bone);
}