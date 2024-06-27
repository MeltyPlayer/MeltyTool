using System;
using System.Numerics;

using fin.math.rotations;

namespace fin.model.impl;
/*public class QuaternionImpl : IQuaternion {
  private const float DEG_2_RAD = (float) (Math.PI / 180);
  private const float RAD_2_DEG = 1 / DEG_2_RAD;

  private Quaternion impl_;

  public QuaternionImpl() => this.impl_ = Quaternion.Identity;

  public QuaternionImpl(Quaternion impl) {
    this.impl_ = impl;
  }

  public float X => this.impl_.X;
  public float Y => this.impl_.Y;
  public float Z => this.impl_.Z;
  public float W => this.impl_.W;

  /// <summary>
  ///   Length of the (x, y, z, w) quaternion vector.
  /// </summary>
  public float Length => this.impl_.Length();

  public float XDegrees => this.XRadians * QuaternionImpl.RAD_2_DEG;
  public float YDegrees => this.YRadians * QuaternionImpl.RAD_2_DEG;
  public float ZDegrees => this.ZRadians * QuaternionImpl.RAD_2_DEG;

  public IQuaternion SetDegrees(float x, float y, float z)
    => this.SetRadiansImpl_(x * QuaternionImpl.DEG_2_RAD,
                           y * QuaternionImpl.DEG_2_RAD,
                           z * QuaternionImpl.DEG_2_RAD);

  public float XRadians { get; private set; }
  public float YRadians { get; private set; }
  public float ZRadians { get; private set; }

  public IQuaternion SetRadians(float x, float y, float z)
    => this.SetRadiansImpl_(x, y, z);

  private IQuaternion SetRadiansImpl_(float x, float y, float z) {
    this.impl_ = QuaternionUtil.Create(x, y, z);
    this.XRadians = x;
    this.YRadians = y;
    this.ZRadians = z;
    return this;
  }
}*/

public class RotationImpl : IRotation {
  private const float DEG_2_RAD = MathF.PI / 180;
  private const float RAD_2_DEG = 1 / DEG_2_RAD;

  public float XDegrees => this.XRadians * RotationImpl.RAD_2_DEG;
  public float YDegrees => this.YRadians * RotationImpl.RAD_2_DEG;
  public float ZDegrees => this.ZRadians * RotationImpl.RAD_2_DEG;

  public IRotation SetDegrees(float x, float y, float z)
    => this.SetRadiansImpl_(x * RotationImpl.DEG_2_RAD,
                            y * RotationImpl.DEG_2_RAD,
                            z * RotationImpl.DEG_2_RAD);

  public float XRadians { get; private set; }
  public float YRadians { get; private set; }
  public float ZRadians { get; private set; }

  public IRotation SetRadians(float x, float y, float z)
    => this.SetRadiansImpl_(x, y, z);

  private RotationImpl SetRadiansImpl_(float x, float y, float z) {
    this.XRadians = x;
    this.YRadians = y;
    this.ZRadians = z;
    return this;
  }

  public IRotation SetQuaternion(Quaternion q) {
    var eulerRadians = QuaternionUtil.ToEulerRadians(q);
    return this.SetRadians(eulerRadians.X, eulerRadians.Y, eulerRadians.Z);
  }

  public override string ToString() =>
      $"{{{this.XDegrees}°, {this.YDegrees}°, {this.ZDegrees}°}}";
}