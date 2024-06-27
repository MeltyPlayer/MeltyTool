using fin.math.rotations;

namespace fin.ui;

public class Camera : ICamera {
  // TODO: Add x/y/z locking.

  public static Camera NewLookingAt(float x,
                                    float y,
                                    float z,
                                    float yaw,
                                    float pitch,
                                    float distance) {
    var camera = new Camera { YawDegrees = yaw, PitchDegrees = pitch };
    camera.X = x - camera.XNormal * distance;
    camera.Y = y - camera.YNormal * distance;
    camera.Z = z - camera.ZNormal * distance;
    return camera;
  }

  public static ICamera Instance { get; private set; }

  public Camera() {
    Camera.Instance = this;
  }

  public float X { get; set; }
  public float Y { get; set; }
  public float Z { get; set; }


  /// <summary>
  ///   The left-right angle of the camera, in degrees.
  /// </summary>
  public float YawDegrees { get; set; }

  /// <summary>
  ///   The up-down angle of the camera, in degrees.
  /// </summary>
  public float PitchDegrees { get; set; }


  public float HorizontalNormal => FinTrig.Cos(this.PitchDegrees * FinTrig.DEG_2_RAD);
  public float VerticalNormal => FinTrig.Sin(this.PitchDegrees * FinTrig.DEG_2_RAD);


  public float XNormal
    => this.HorizontalNormal * FinTrig.Cos(this.YawDegrees * FinTrig.DEG_2_RAD);

  public float YNormal
    => this.HorizontalNormal * FinTrig.Sin(this.YawDegrees * FinTrig.DEG_2_RAD);

  public float ZNormal => this.VerticalNormal;


  public float XUp
    => -this.VerticalNormal * FinTrig.Cos(this.YawDegrees * FinTrig.DEG_2_RAD);

  public float YUp
    => -this.VerticalNormal * FinTrig.Sin(this.YawDegrees * FinTrig.DEG_2_RAD);

  public float ZUp => this.HorizontalNormal;


  // TODO: These negative signs and flipped cos/sin don't look right but they
  // work???
  public void Move(float forwardVector,
                   float rightVector,
                   float upVector,
                   float speed) {
    this.Z += speed * (this.VerticalNormal * forwardVector +
                       this.HorizontalNormal * upVector);

    var forwardYawRads = this.YawDegrees * FinTrig.DEG_2_RAD;
    var rightYawRads = (this.YawDegrees - 90) * FinTrig.DEG_2_RAD;

    this.X +=
        speed *
        (this.HorizontalNormal *
         (forwardVector * FinTrig.Cos(forwardYawRads) +
          rightVector * FinTrig.Cos(rightYawRads)) +
         -this.VerticalNormal * upVector * FinTrig.Cos(forwardYawRads));

    this.Y +=
        speed *
        (this.HorizontalNormal *
         (forwardVector * FinTrig.Sin(forwardYawRads) +
          rightVector * FinTrig.Sin(rightYawRads)) +
         -this.VerticalNormal * upVector * FinTrig.Sin(forwardYawRads));
  }
}