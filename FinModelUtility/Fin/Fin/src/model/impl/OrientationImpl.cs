using System;
using System.Numerics;

using fin.math.rotations;

namespace fin.model.impl;
public class RotationImpl : IRotation {
  private const float DEG_2_RAD = MathF.PI / 180;
  private const float RAD_2_DEG = 1 / DEG_2_RAD;

  public float XDegrees => this.XRadians * RAD_2_DEG;
  public float YDegrees => this.YRadians * RAD_2_DEG;
  public float ZDegrees => this.ZRadians * RAD_2_DEG;

  public IRotation SetDegrees(float x, float y, float z)
    => this.SetRadiansImpl_(x * DEG_2_RAD,
                            y * DEG_2_RAD,
                            z * DEG_2_RAD);

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

  public IRotation SetQuaternion(in Quaternion q) {
    var eulerRadians = QuaternionUtil.ToEulerRadians(q);
    return this.SetRadians(eulerRadians.X, eulerRadians.Y, eulerRadians.Z);
  }

  public override string ToString() =>
      $"{{{this.XDegrees}°, {this.YDegrees}°, {this.ZDegrees}°}}";
}