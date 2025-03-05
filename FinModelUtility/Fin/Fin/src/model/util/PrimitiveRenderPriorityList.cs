using System.Collections;
using System.Collections.Generic;
using System.Linq;

using fin.data.lists;
using fin.image.util;

namespace fin.model.util;

using PrimitiveBundle
    = ((IReadOnlyMesh, IReadOnlyPrimitive) meshAndPrimitive,
    uint inversePriority,
    TransparencyType
    transparencyType);

public class PrimitiveRenderPriorityList
    : IEnumerable<(IReadOnlyMesh, IReadOnlyPrimitive)> {
  private readonly BinarySortedList<PrimitiveBundle> elements_
      = new(new RenderPriorityComparer());

  public void Add(
      IReadOnlyMesh mesh,
      IReadOnlyPrimitive primitive,
      uint inversePriority,
      TransparencyType transparencyType)
    => this.elements_.Add(((mesh, primitive),
                           inversePriority,
                           transparencyType));

  IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

  public IEnumerator<(IReadOnlyMesh, IReadOnlyPrimitive)> GetEnumerator()
    => this.elements_.Select(e => e.meshAndPrimitive).GetEnumerator();

  private class RenderPriorityComparer : IComparer<PrimitiveBundle> {
    public int Compare(PrimitiveBundle lhs, PrimitiveBundle rhs) {
      // Order first by primitive's inverse priority
      if (lhs.inversePriority != rhs.inversePriority) {
        return lhs.inversePriority.CompareTo(rhs.inversePriority);
      }

      // Then, order by transparency
      if (lhs.transparencyType != rhs.transparencyType) {
        return lhs.transparencyType.CompareTo(rhs.transparencyType);
      }

      var (lhsMesh, lhsPrimitive) = lhs.meshAndPrimitive;
      var (rhsMesh, rhsPrimitive) = rhs.meshAndPrimitive;

      // Then, order by material to reduce state getting needlessly updated
      if (lhsPrimitive.Material == rhsPrimitive.Material) {
        return 0;
      }

      // Finally, order by mesh
      return lhsMesh.GetHashCode().CompareTo(rhsMesh.GetHashCode());
    }
  }
}