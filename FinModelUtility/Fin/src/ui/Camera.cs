using System;
using fin.math;


namespace fin.ui {
  public class Camera {
    // TODO: Add x/y/z locking.

    public static Camera Instance { get; private set; }

    public Camera() {
      Camera.Instance = this;
    }

    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }


    /// <summary>
    ///   The left-right angle of the camera, in degrees.
    /// </summary>
    public float Yaw { get; set; }

    /// <summary>
    ///   The up-down angle of the camera, in degrees.
    /// </summary>
    public float Pitch { get; set; }


    public float HorizontalNormal => FinTrig.Cos(this.Pitch / 180 * MathF.PI);
    public float VerticalNormal => FinTrig.Sin(this.Pitch / 180 * MathF.PI);


    public float XNormal
      => this.HorizontalNormal * FinTrig.Cos(this.Yaw / 180 * MathF.PI);

    public float YNormal
      => this.HorizontalNormal * FinTrig.Sin(this.Yaw / 180 * MathF.PI);

    public float ZNormal => this.VerticalNormal;


    public void Reset() => this.X = this.Y = this.Z = this.Yaw = this.Pitch = 0;

    // TODO: These negative signs and flipped cos/sin don't look right but they
    // work???
    public void Move(float forwardVector, float rightVector, float upwardVector, float speed) {
      var forwardYawRads = this.Yaw / 180 * MathF.PI;
      var rightYawRads = (this.Yaw - 90) / 180 * MathF.PI;

      this.Z += speed * 
                //this.VerticalNormal * 
                //forwardVector *
                upwardVector;

      this.X += speed *
                //this.HorizontalNormal *
                (forwardVector * FinTrig.Cos(forwardYawRads) +
                 rightVector * FinTrig.Cos(rightYawRads));

      this.Y += speed *
               // this.HorizontalNormal *
                (forwardVector * FinTrig.Sin(forwardYawRads) +
                 rightVector * FinTrig.Sin(rightYawRads));
    }
  }
}