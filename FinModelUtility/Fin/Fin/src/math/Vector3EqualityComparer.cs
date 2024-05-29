using System;
using System.Collections.Generic;
using System.Numerics;

using fin.util.hash;

namespace fin.math {
  public class Vector3EqualityComparer(float precision)
      : IEqualityComparer<Vector3> {
    public bool Equals(Vector3 lhs, Vector3 rhs)
      => Math.Abs(lhs.X - rhs.X) < precision &&
         Math.Abs(lhs.Y - rhs.Y) < precision &&
         Math.Abs(lhs.Z - rhs.Z) < precision;

    public int GetHashCode(Vector3 obj)
      => FluentHash.Start()
                   .With((int) Math.Round(obj.X / precision))
                   .With((int) Math.Round(obj.Y / precision))
                   .With((int) Math.Round(obj.Z / precision));
  }
}