using System.Numerics;

namespace fin.ui;

public interface ICamera {
  Vector3 Position { get; }
  Vector3 Normal { get; }
  Vector3 Up { get; }

  float YawDegrees { get; }
  float PitchDegrees { get; }
}