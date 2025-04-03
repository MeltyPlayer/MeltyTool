using System;
using System.Collections.Generic;
using System.Numerics;

using CommunityToolkit.Diagnostics;


namespace fin.math.splines;

public class LinearSpline(IReadOnlyList<Vector3> positions) {
  public Vector3 GetPositionAtOffset(float offset) {
    Guard.IsGreaterThan(offset, 0);

    for (var i = 1; i < positions.Count; i++) {
      var previous = positions[i - 1];
      var current = positions[i];

      var distance = (current - previous).Length();
      if (offset < distance) {
        var fraction = offset / distance;
        return Vector3.Lerp(previous, current, fraction);
      }

      offset -= distance;
    }

    throw new NotSupportedException();
  }
}