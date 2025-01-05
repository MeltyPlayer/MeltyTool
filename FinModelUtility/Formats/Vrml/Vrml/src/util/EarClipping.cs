using System.Diagnostics;

namespace vrml.util;

// Implementation of Triangulation by Ear Clipping
// by David Eberly
public class EarClipping {
  private Polygon _mainPointList;
  private Vector3m Normal;
  public List<Vector3m> Result { get; private set; }

  public void SetPoints(List<Vector3m> points,
                        Vector3m normal = null) {
    if (points == null || points.Count < 3) {
      throw new ArgumentException("No list or an empty list passed");
    }

    if (normal == null)
      CalcNormal(points);
    else {
      Normal = normal;
    }

    _mainPointList = new Polygon();
    LinkAndAddToList(_mainPointList, points);

    Result = new List<Vector3m>();
  }

  // calculating normal using Newell's method
  private void CalcNormal(List<Vector3m> points) {
    Vector3m normal = Vector3m.Zero();
    for (var i = 0; i < points.Count; i++) {
      var j = (i + 1) % (points.Count);
      normal.X += (points[i].Y - points[j].Y) * (points[i].Z + points[j].Z);
      normal.Y += (points[i].Z - points[j].Z) * (points[i].X + points[j].X);
      normal.Z += (points[i].X - points[j].X) * (points[i].Y + points[j].Y);
    }

    Normal = normal;
  }

  private void LinkAndAddToList(Polygon polygon, List<Vector3m> points) {
    ConnectionEdge prev = null, first = null;
    Dictionary<Vector3m, Vector3m> pointsHashSet
        = new Dictionary<Vector3m, Vector3m>();
    int pointCount = 0;
    for (int i = 0; i < points.Count; i++) {
      // we don't wanna have duplicates
      Vector3m p0;
      if (pointsHashSet.ContainsKey(points[i])) {
        p0 = pointsHashSet[points[i]];
      } else {
        p0 = points[i];
        pointsHashSet.Add(p0, p0);
        List<ConnectionEdge> list = new List<ConnectionEdge>();
        p0.DynamicProperties.AddProperty(PropertyConstants.IncidentEdges, list);
        pointCount++;
      }

      ConnectionEdge current = new ConnectionEdge(p0, polygon);

      first = (i == 0) ? current : first; // remember first

      if (prev != null) {
        prev.Next = current;
      }

      current.Prev = prev;
      prev = current;
    }

    first.Prev = prev;
    prev.Next = first;
    polygon.Start = first;
    polygon.PointCount = pointCount;
  }

  public void Triangulate() {
    if (Normal.Equals(Vector3m.Zero()))
      throw new Exception("The input is not a valid polygon");

    List<ConnectionEdge> nonConvexPoints = FindNonConvexPoints(_mainPointList);

    if (nonConvexPoints.Count == _mainPointList.PointCount)
      throw new ArgumentException("The triangle input is not valid");

    while (_mainPointList.PointCount > 2) {
      bool guard = false;
      foreach (var cur in _mainPointList.GetPolygonCirculator()) {
        if (!IsConvex(cur))
          continue;

        if (!IsPointInTriangle(cur.Prev.Origin,
                               cur.Origin,
                               cur.Next.Origin,
                               nonConvexPoints)) {
          // cut off ear
          guard = true;
          Result.Add(cur.Prev.Origin);
          Result.Add(cur.Origin);
          Result.Add(cur.Next.Origin);

          // Check if prev and next are still nonconvex. If not, then remove from non convex list
          if (IsConvex(cur.Prev)) {
            int index = nonConvexPoints.FindIndex(x => x == cur.Prev);
            if (index >= 0)
              nonConvexPoints.RemoveAt(index);
          }

          if (IsConvex(cur.Next)) {
            int index = nonConvexPoints.FindIndex(x => x == cur.Next);
            if (index >= 0)
              nonConvexPoints.RemoveAt(index);
          }

          _mainPointList.Remove(cur);
          break;
        }
      }

      if (PointsOnLine(_mainPointList))
        break;
      if (!guard) {
        throw new Exception("No progression. The input must be wrong");
      }
    }
  }

  private bool PointsOnLine(Polygon pointList) {
    foreach (var connectionEdge in pointList.GetPolygonCirculator()) {
      if (Misc.GetOrientation(connectionEdge.Prev.Origin,
                              connectionEdge.Origin,
                              connectionEdge.Next.Origin,
                              Normal) !=
          0)
        return false;
    }

    return true;
  }

  private bool IsConvex(ConnectionEdge curPoint) {
    int orientation = Misc.GetOrientation(curPoint.Prev.Origin,
                                          curPoint.Origin,
                                          curPoint.Next.Origin,
                                          Normal);
    return orientation == 1;
  }

  private bool IsPointInTriangle(Vector3m prevPoint,
                                 Vector3m curPoint,
                                 Vector3m nextPoint,
                                 List<ConnectionEdge> nonConvexPoints) {
    foreach (var nonConvexPoint in nonConvexPoints) {
      if (nonConvexPoint.Origin == prevPoint ||
          nonConvexPoint.Origin == curPoint ||
          nonConvexPoint.Origin == nextPoint)
        continue;
      if (Misc.PointInOrOnTriangle(prevPoint,
                                   curPoint,
                                   nextPoint,
                                   nonConvexPoint.Origin,
                                   Normal))
        return true;
    }

    return false;
  }

  private List<ConnectionEdge> FindNonConvexPoints(Polygon p) {
    List<ConnectionEdge> resultList = new List<ConnectionEdge>();
    foreach (var connectionEdge in p.GetPolygonCirculator()) {
      if (Misc.GetOrientation(connectionEdge.Prev.Origin,
                              connectionEdge.Origin,
                              connectionEdge.Next.Origin,
                              Normal) !=
          1)
        resultList.Add(connectionEdge);
    }

    return resultList;
  }
}

internal class ConnectionEdge {
  protected bool Equals(ConnectionEdge other) {
    return Next.Origin.Equals(other.Next.Origin) && Origin.Equals(other.Origin);
  }

  public override bool Equals(object obj) {
    if (ReferenceEquals(null, obj)) return false;
    if (ReferenceEquals(this, obj)) return true;
    if (obj.GetType() != this.GetType()) return false;
    return Equals((ConnectionEdge) obj);
  }

  public override int GetHashCode() {
    unchecked {
      return ((Next.Origin != null ? Next.Origin.GetHashCode() : 0) * 397) ^
             (Origin != null ? Origin.GetHashCode() : 0);
    }
  }

  internal Vector3m Origin { get; set; }
  internal ConnectionEdge Prev;
  internal ConnectionEdge Next;
  internal Polygon Polygon { get; set; }

  public ConnectionEdge(Vector3m p0, Polygon parentPolygon) {
    Origin = p0;
    Polygon = parentPolygon;
    AddIncidentEdge(this);
  }

  public override string ToString() {
    return "Origin: " + Origin + " Next: " + Next.Origin;
  }

  internal void AddIncidentEdge(ConnectionEdge next) {
    var list
        = (List<ConnectionEdge>) Origin.DynamicProperties.GetValue(
            PropertyConstants.IncidentEdges);
    list.Add(next);
  }
}

internal class Polygon {
  internal ConnectionEdge Start;
  internal int PointCount = 0;

  internal IEnumerable<ConnectionEdge> GetPolygonCirculator() {
    if (Start == null) {
      yield break;
    }

    var h = Start;
    do {
      yield return h;
      h = h.Next;
    } while (h != Start);
  }

  internal void Remove(ConnectionEdge cur) {
    cur.Prev.Next = cur.Next;
    cur.Next.Prev = cur.Prev;
    var incidentEdges =
        (List<ConnectionEdge>) cur.Origin.DynamicProperties.GetValue(
            PropertyConstants.IncidentEdges);
    int index = incidentEdges.FindIndex(x => x.Equals(cur));
    Debug.Assert(index >= 0);
    incidentEdges.RemoveAt(index);
    if (incidentEdges.Count == 0)
      PointCount--;
    if (cur == Start)
      Start = cur.Prev;
  }

  public bool Contains(Vector3m vector2M, out Vector3m res) {
    foreach (var connectionEdge in GetPolygonCirculator()) {
      if (connectionEdge.Origin.Equals(vector2M)) {
        res = connectionEdge.Origin;
        return true;
      }
    }

    res = null;
    return false;
  }
}