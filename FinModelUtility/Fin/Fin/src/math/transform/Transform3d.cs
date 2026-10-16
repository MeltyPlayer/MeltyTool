using System.Collections.Generic;
using System.Numerics;

using fin.math.matrix.four;
using fin.math.rotations;

using readOnly;

namespace fin.math.transform;

[GenerateReadOnly]
public partial interface ITransform3d
    : ITransform<Vector3, Quaternion, Vector3> {
  bool IgnoreParentScale { get; set; }
  new Matrix4x4 WorldMatrix { get; }

  new Matrix4x4 LocalMatrix { get; }
  new Vector3 LocalEulerRadians { get; set; }
}

public sealed class Transform3d : ITransform3d {
  private readonly Transform3d? parent_;
  private readonly List<Transform3d> children_ = new();

  private bool isWorldMatrixDirty_;
  private bool isLocalMatrixDirty_;

  private bool isEulerRadiansDirty_;
  private bool isQuaternionDirty_;

  private Quaternion rotation_ = Quaternion.Identity;
  private Vector3 eulerRadians_;

  public Transform3d(Transform3d? parent = null) {
    this.parent_ = parent;
    this.parent_?.children_.Add(this);
  }

  public bool IgnoreParentScale {
    get;
    set {
      field = value;
      this.MarkLocalDirty_();
    }
  }

  public Matrix4x4 WorldMatrix {
    get {
      if (!this.isWorldMatrixDirty_) {
        return field;
      }

      this.isWorldMatrixDirty_ = false;

      if (this.parent_ != null) {
        var parentWorldMatrix = this.parent_.WorldMatrix;
        if (this.IgnoreParentScale) {
          parentWorldMatrix = parentWorldMatrix.FilterTrs(true, true, false);
        }

        return field = this.LocalMatrix * parentWorldMatrix;
      }

      return field = this.LocalMatrix;
    }
  } = Matrix4x4.Identity;

  public Matrix4x4 LocalMatrix {
    get {
      if (!this.isLocalMatrixDirty_) {
        return field;
      }

      this.isLocalMatrixDirty_ = false;

      return field
          = SystemMatrix4x4Util.FromTrs(this.LocalTranslation,
                                        this.LocalRotation,
                                        this.LocalScale);
    }
  } = Matrix4x4.Identity;

  public Vector3 LocalTranslation {
    get;
    set {
      field = value;
      this.MarkLocalDirty_();
    }
  }

  public Quaternion LocalRotation {
    get {
      if (!this.isQuaternionDirty_) {
        return this.rotation_;
      }

      this.isQuaternionDirty_ = false;
      return this.rotation_ = this.eulerRadians_.CreateZyxRadians();
    }
    set {
      this.isQuaternionDirty_ = false;
      this.rotation_ = value;

      this.isEulerRadiansDirty_ = true;
      this.MarkLocalDirty_();
    }
  }

  public Vector3 LocalEulerRadians {
    get {
      if (!this.isEulerRadiansDirty_) {
        return this.eulerRadians_;
      }

      this.isEulerRadiansDirty_ = false;
      return this.eulerRadians_ = this.rotation_.ToEulerRadians();
    }
    set {
      this.isEulerRadiansDirty_ = false;
      this.eulerRadians_ = value;

      this.isQuaternionDirty_ = true;
      this.MarkLocalDirty_();
    }
  }

  public Vector3 LocalScale {
    get;
    set {
      field = value;
      this.MarkLocalDirty_();
    }
  } = Vector3.One;

  private void MarkLocalDirty_() {
    this.isLocalMatrixDirty_ = true;
    this.MarkWorldDirty_();
  }

  private void MarkWorldDirty_() {
    if (this.isWorldMatrixDirty_) {
      return;
    }

    this.isWorldMatrixDirty_ = true;
    foreach (var child in this.children_) {
      child.MarkWorldDirty_();
    }
  }
}