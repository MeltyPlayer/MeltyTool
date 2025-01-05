using System.Diagnostics;

using PeterO.Numbers;

namespace vrml.util;

// Implementation of Triangulation by Ear Clipping
// by David Eberly
public class EarClipping {
  private Polygon _mainPointList;
  private List<Polygon> _holes;
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
    if (_holes != null && _holes.Count > 0) {
      ProcessHoles();
    }

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

  private void ProcessHoles() {
    for (int h = 0; h < _holes.Count; h++) {
      List<Polygon> polygons = new List<Polygon>();
      polygons.Add(_mainPointList);
      polygons.AddRange(_holes);
      ConnectionEdge M, P;
      GetVisiblePoints(h + 1, polygons, out M, out P);
      if (M.Origin.Equals(P.Origin))
        throw new Exception();

      var insertionEdge = P;
      InsertNewEdges(insertionEdge, M);
      _holes.RemoveAt(h);
      h--;
    }
  }

  private void InsertNewEdges(ConnectionEdge insertionEdge, ConnectionEdge m) {
    insertionEdge.Polygon.PointCount += m.Polygon.PointCount;
    var cur = m;
    var forwardEdge
        = new ConnectionEdge(insertionEdge.Origin, insertionEdge.Polygon);
    forwardEdge.Prev = insertionEdge.Prev;
    forwardEdge.Prev.Next = forwardEdge;
    forwardEdge.Next = m;
    forwardEdge.Next.Prev = forwardEdge;
    var end = insertionEdge;
    ConnectionEdge prev = null;
    do {
      cur.Polygon = insertionEdge.Polygon;
      prev = cur;
      cur = cur.Next;
    } while (m != cur);

    var backEdge = new ConnectionEdge(cur.Origin, insertionEdge.Polygon);
    cur = prev;
    cur.Next = backEdge;
    backEdge.Prev = cur;
    backEdge.Next = end;
    end.Prev = backEdge;
  }

  private void GetVisiblePoints(int holeIndex,
                                List<Polygon> polygons,
                                out ConnectionEdge M,
                                out ConnectionEdge P) {
    M = FindLargest(polygons[holeIndex]);

    var direction
        = (polygons[holeIndex].Start.Next.Origin -
           polygons[holeIndex].Start.Origin).Cross(Normal);
    var I = FindPointI(M, polygons, holeIndex, direction);

    Vector3m res;
    if (polygons[I.PolyIndex].Contains(I.I, out res)) {
      var incidentEdges =
          (List<ConnectionEdge>) res.DynamicProperties.GetValue(
              PropertyConstants.IncidentEdges);
      foreach (var connectionEdge in incidentEdges) {
        if (Misc.IsBetween(connectionEdge.Origin,
                           connectionEdge.Next.Origin,
                           connectionEdge.Prev.Origin,
                           M.Origin,
                           Normal) ==
            1) {
          P = connectionEdge;
          return;
        }
      }

      throw new Exception();
    } else {
      P = FindVisiblePoint(I, polygons, M, direction);
    }
  }

  private ConnectionEdge FindVisiblePoint(Candidate I,
                                          List<Polygon> polygons,
                                          ConnectionEdge M,
                                          Vector3m direction) {
    ConnectionEdge P = null;

    if (I.Origin.Origin.Dot(direction)
         .CompareTo(I.Origin.Next.Origin.Dot(direction)) >
        0) {
      P = I.Origin;
    } else {
      P = I.Origin.Next;
    }

    List<ConnectionEdge> nonConvexPoints
        = FindNonConvexPoints(polygons[I.PolyIndex]);


    nonConvexPoints.Remove(P);

    var m = M.Origin;
    var i = I.I;
    var p = P.Origin;
    List<ConnectionEdge> candidates = new List<ConnectionEdge>();

    // invert i and p if triangle is oriented CW
    if (Misc.GetOrientation(m, i, p, Normal) == -1) {
      var tmp = i;
      i = p;
      p = tmp;
    }

    foreach (var nonConvexPoint in nonConvexPoints) {
      if (Misc.PointInOrOnTriangle(m, i, p, nonConvexPoint.Origin, Normal)) {
        candidates.Add(nonConvexPoint);
      }
    }

    if (candidates.Count == 0)
      return P;
    return FindMinimumAngle(candidates, m, direction);
  }

  private ConnectionEdge FindMinimumAngle(List<ConnectionEdge> candidates,
                                          Vector3m M,
                                          Vector3m direction) {
    ERational angle = -double.MaxValue;
    ConnectionEdge result = null;
    foreach (var R in candidates) {
      var a = direction;
      var b = R.Origin - M;
      var num = a.Dot(b) * a.Dot(b);
      var denom = b.Dot(b);
      var res = num / denom;
      if (res.CompareTo(angle) > 0) {
        result = R;
        angle = res;
      }
    }

    return result;
  }

  private Candidate FindPointI(ConnectionEdge M,
                               List<Polygon> polygons,
                               int holeIndex,
                               Vector3m direction) {
    Candidate candidate = new Candidate();
    for (int i = 0; i < polygons.Count; i++) {
      if (i == holeIndex) // Don't test the hole with itself
        continue;
      foreach (var connectionEdge in polygons[i].GetPolygonCirculator()) {
        ERational rayDistanceSquared;
        Vector3m intersectionPoint;

        if (RaySegmentIntersection(out intersectionPoint,
                                   out rayDistanceSquared,
                                   M.Origin,
                                   direction,
                                   connectionEdge.Origin,
                                   connectionEdge.Next.Origin,
                                   direction)) {
          if (rayDistanceSquared ==
              candidate
                  .currentDistance) // if this is an M/I edge, then both edge and his twin have the same distance; we take the edge where the point is on the left side
          {
            if (Misc.GetOrientation(connectionEdge.Origin,
                                    connectionEdge.Next.Origin,
                                    M.Origin,
                                    Normal) ==
                1) {
              candidate.currentDistance = rayDistanceSquared;
              candidate.Origin = connectionEdge;
              candidate.PolyIndex = i;
              candidate.I = intersectionPoint;
            }
          } else if (rayDistanceSquared.CompareTo(candidate.currentDistance) <
                     0) {
            candidate.currentDistance = rayDistanceSquared;
            candidate.Origin = connectionEdge;
            candidate.PolyIndex = i;
            candidate.I = intersectionPoint;
          }
        }
      }
    }

    return candidate;
  }

  private ConnectionEdge FindLargest(Polygon testHole) {
    ERational maximum = 0;
    ConnectionEdge maxEdge = null;
    Vector3m v0 = testHole.Start.Origin;
    Vector3m v1 = testHole.Start.Next.Origin;
    foreach (var connectionEdge in testHole.GetPolygonCirculator()) {
      // we take the first two points as a reference line

      if (Misc.GetOrientation(v0, v1, connectionEdge.Origin, Normal) < 0) {
        var r = Misc.PointLineDistance(v0, v1, connectionEdge.Origin);
        if (r.CompareTo(maximum) > 0) {
          maximum = r;
          maxEdge = connectionEdge;
        }
      }
    }

    if (maxEdge == null)
      return testHole.Start;
    return maxEdge;
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

  public bool RaySegmentIntersection(out Vector3m intersection,
                                     out ERational distanceSquared,
                                     Vector3m linePoint1,
                                     Vector3m lineVec1,
                                     Vector3m linePoint3,
                                     Vector3m linePoint4,
                                     Vector3m direction) {
    var lineVec2 = linePoint4 - linePoint3;
    Vector3m lineVec3 = linePoint3 - linePoint1;
    Vector3m crossVec1and2 = lineVec1.Cross(lineVec2);
    Vector3m crossVec3and2 = lineVec3.Cross(lineVec2);

    var res = Misc.PointLineDistance(linePoint3, linePoint4, linePoint1);
    if (res.IsZero) // line and ray are collinear
    {
      var p = linePoint1 + lineVec1;
      var res2 = Misc.PointLineDistance(linePoint3, linePoint4, p);
      if (res2.IsZero) {
        var s = linePoint3 - linePoint1;
        if (s.X == direction.X && s.Y == direction.Y && s.Z == direction.Z) {
          intersection = linePoint3;
          distanceSquared = s.LengthSquared();
          return true;
        }
      }
    }

    //is coplanar, and not parallel
    if ( /*planarFactor == 0.0f && */crossVec1and2.LengthSquared().Sign > 0) {
      var s = crossVec3and2.Dot(crossVec1and2) / crossVec1and2.LengthSquared();
      if (s.Sign >= 0) {
        intersection = linePoint1 + (lineVec1 * s);
        distanceSquared = (lineVec1 * s).LengthSquared();
        if (((intersection - linePoint3).LengthSquared() +
             (intersection - linePoint4).LengthSquared())
            .CompareTo(lineVec2.LengthSquared()) <=
            0)
          return true;
      }
    }

    intersection = Vector3m.Zero();
    distanceSquared = 0;
    return false;
  }
}

internal class Candidate {
  internal ERational currentDistance = double.MaxValue;
  internal Vector3m I;
  internal ConnectionEdge Origin;
  internal int PolyIndex;
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