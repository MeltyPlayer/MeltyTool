using System;
using System.Numerics;

using fin.data;
using fin.image.formats;

using SixLabors.ImageSharp.PixelFormats;

namespace fin.image.effects;

public interface IMeshWarp {
  Grid<IMeshWarpPin> Pins { get; }
  IReadOnlyImage WarpImage(IReadOnlyImage input);
}

public interface IMeshWarpPin {
  Vector2 OriginalPosition { get; }
  Vector2 Position { get; set; }
}

public sealed class MeshWarp : IMeshWarp {
  public Grid<IMeshWarpPin> Pins { get; }

  public MeshWarp(int width, int height, int xPinCount, int yPinCount) {
    this.Pins = new Grid<IMeshWarpPin>(
        xPinCount,
        yPinCount,
        (x, y) => {
          var position = new Vector2(x, y) /
                         new Vector2(xPinCount - 1, yPinCount - 1) *
                         new Vector2(width, height);

          return new MeshWarpPin(position);
        });
  }

  public sealed class MeshWarpPin(Vector2 originalPosition) : IMeshWarpPin {
    public Vector2 OriginalPosition => originalPosition;
    public Vector2 Position { get; set; } = originalPosition;
  }

  public IReadOnlyImage WarpImage(IReadOnlyImage input) {
    var width = input.Width;
    var height = input.Height;

    var output = new Rgba32Image(input.PixelFormat, width, height);

    input.AccessInterpolated(getHandler => {
      using var fastLock = output.Lock();
      var scan0 = fastLock.Pixels;

      for (var y = 0; y < height; y++) {
        for (var x = 0; x < width; x++) {
          var point = new Vector2(x + .5f, y + .5f);

          for (var mY = 0; mY < this.Pins.Height - 1; ++mY) {
            for (var mX = 0; mX < this.Pins.Width - 1; ++mX) {
              var vTopLeft = this.Pins[mX, mY];
              var vTopRight = this.Pins[mX + 1, mY];
              var vBottomRight = this.Pins[mX + 1, mY + 1];
              var vBottomLeft = this.Pins[mX, mY + 1];

              if (!IsPointInsideQuad_(
                      point,
                      vTopLeft.Position,
                      vTopRight.Position,
                      vBottomRight.Position,
                      vBottomLeft.Position)) {
                continue;
              }

              if (!VectorToUV_(point,
                               vBottomLeft.Position,
                               vBottomRight.Position,
                               vTopRight.Position,
                               vTopLeft.Position,
                               out var u,
                               out var v)) {
                scan0[y * width + x] = new Rgba32(255, 0, 0, 255);

                goto FoundPin;
              }

              var weightedOriginalPosition =
                  Vector2.Lerp(
                      Vector2.Lerp(
                          vTopLeft.OriginalPosition,
                          vTopRight.OriginalPosition,
                          (float) u),
                      Vector2.Lerp(
                          vBottomLeft.OriginalPosition,
                          vBottomRight.OriginalPosition,
                          (float) u),
                      (float) (1 - v));

              getHandler(
                  weightedOriginalPosition.X,
                  weightedOriginalPosition.Y,
                  out var r,
                  out var g,
                  out var b,
                  out var a);

              scan0[y * width + x]
                  = new Rgba32((byte) r, (byte) g, (byte) b, (byte) a);

              goto FoundPin;
            }
          }

          scan0[y * width + x] = new Rgba32(255, 0, 255, 255);

          FoundPin: ;
        }
      }
    });

    return output;
  }

  public static bool VectorToUV_(Vector2 p, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, out double u, out double v) {
    u = 0;
    v = 0;
    
    // Coefficients for the quadratic equation: A*v^2 + B*v + C = 0
    double ax = p0.X - p1.X + p2.X - p3.X;
    double ay = p0.Y - p1.Y + p2.Y - p3.Y;
    double bx = p1.X - p0.X;
    double by = p1.Y - p0.Y;
    double cx = p3.X - p0.X;
    double cy = p3.Y - p0.Y;
    double dx = p0.X - p.X;
    double dy = p0.Y - p.Y;

    double A = ax * cy - ay * cx;
    double B = ax * dy - ay * dx + bx * cy - by * cx;
    double C = bx * dy - by * dx;

    // Check if the equations degrade to a linear form (e.g., if the quad is a parallelogram)
    if (Math.Abs(A) < 1e-9)
    {
        if (Math.Abs(B) < 1e-9) {
          return false; // Degenerate quad
        }
        v = -C / B;
    }
    else
    {
        double discriminant = B * B - 4 * A * C;
        if (discriminant < 0) {
          return false; // Point might be outside or quad is self-intersecting
        }

        double sqrtDisc = Math.Sqrt(discriminant);
        
        // Evaluate both roots to find the one inside [0, 1]
        double v1 = (-B + sqrtDisc) / (2 * A);
        double v2 = (-B - sqrtDisc) / (2 * A);

        if (v1 >= -1e-7 && v1 <= 1.0000001) v = Math.Clamp(v1, 0, 1);
        else if (v2 >= -1e-7 && v2 <= 1.0000001) v = Math.Clamp(v2, 0, 1);
        else return false; // Both roots fall outside the valid normalized quad space
    }

    // Solve for u using the calculated v
    double denomU = bx + ax * v;
    if (Math.Abs(denomU) > 1e-9)
    {
        u = (-dx - cx * v) / denomU;
    }
    else
    {
        // Alternative calculation if denominator is zero
        double denomUAlt = by + ay * v;
        if (Math.Abs(denomUAlt) < 1e-9) {
          return false;
        }
        u = (-dy - cy * v) / denomUAlt;
    }

    u = Math.Clamp(u, 0, 1);
    return true;
  }

  /// <summary>
  ///   Shamelessly stolen from: https://stackoverflow.com/a/12634247
  /// </summary>
  private static bool IsPointInsideQuad_(
      Vector2 point,
      Vector2 v0,
      Vector2 v1,
      Vector2 v2,
      Vector2 v3) {
    var sign0 = CalculateCrossProductSign_(point, v0, v1);
    var sign1 = CalculateCrossProductSign_(point, v1, v2);
    var sign2 = CalculateCrossProductSign_(point, v2, v3);
    var sign3 = CalculateCrossProductSign_(point, v3, v0);

    // Check if the point is consistently on the same side (or on the line, where d == 0)
    bool hasNegative = sign0 < 0 || sign1 < 0 || sign2 < 0 || sign3 < 0;
    bool hasPositive = sign0 > 0 || sign1 > 0 || sign2 > 0 || sign3 > 0;

    // If it doesn't have both negative and positive signs, it's inside or on the border
    return !(hasNegative && hasPositive);
  }

  private static int CalculateCrossProductSign_(
      Vector2 point,
      Vector2 v0,
      Vector2 v1) {
    var ev = v1 - v0;
    var pv = point - v0;
    return Math.Sign(Vector2.Cross(ev, pv));
  }
}