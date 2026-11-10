using System.Numerics;

namespace fin.model.skeleton.boneTransformViews;

public class NoopBoneTransformView : IBoneTransformView {
  public static NoopBoneTransformView Instance { get; } = new();

  private NoopBoneTransformView() { }

  public bool TryGetWorldTranslation(out Vector3 translation) {
    translation = default;
    return false;
  }

  public bool TryGetWorldRotation(out Quaternion rotation) {
    rotation = default;
    return false;
  }

  public bool TryGetWorldScale(out Vector3 scale) {
    scale = default;
    return false;
  }

  public bool TryGetLocalTranslation(out Vector3 translation) {
    translation = default;
    return false;
  }

  public bool TryGetLocalRotation(out Quaternion rotation) {
    rotation = default;
    return false;
  }

  public bool TryGetLocalScale(out Vector3 scale) {
    scale = default;
    return false;
  }
}