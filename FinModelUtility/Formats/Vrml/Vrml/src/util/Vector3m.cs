using System.Diagnostics;

using PeterO.Numbers;

namespace vrml.util;

public class Vector3m : ICloneable {
  internal DynamicProperties DynamicProperties = new DynamicProperties();

  public Vector3m(ERational x, ERational y, ERational z) {
    X = x;
    Y = y;
    Z = z;
  }

  public Vector3m(Vector3m v) {
    X = v.X;
    Y = v.Y;
    Z = v.Z;
  }

  public static Vector3m Zero() {
    return new Vector3m(0, 0, 0);
  }

  public ERational X { get; set; }

  public Vector3m Absolute() {
    return new Vector3m(X.Abs(), Y.Abs(), Z.Abs());
  }

  public ERational Y { get; set; }
  public ERational Z { get; set; }

  public object Clone() {
    return new Vector3m(X, Y, Z);
  }

  public void ImplizitNegated() {
    X = -X;
    Y = -Y;
    Z = -Z;
  }

  public Vector3m Negated() {
    return new Vector3m(-X, -Y, -Z);
  }

  public Vector3m Plus(Vector3m a) {
    return new Vector3m(this.X + a.X, this.Y + a.Y, this.Z + a.Z);
  }

  public Vector3m Minus(Vector3m a) {
    return new Vector3m(this.X - a.X, this.Y - a.Y, this.Z - a.Z);
  }

  public Vector3m Times(ERational a) {
    return new Vector3m(this.X * a, this.Y * a, this.Z * a);
  }

  public Vector3m DividedBy(ERational a) {
    return new Vector3m(this.X / a, this.Y / a, this.Z / a);
  }

  public ERational Dot(Vector3m a) {
    return this.X * a.X + this.Y * a.Y + this.Z * a.Z;
  }

  public Vector3m Lerp(Vector3m a, ERational t) {
    return this.Plus(a.Minus(this).Times(t));
  }

  public Vector3m ProjectOntoPlane(Vector3m normal, Vector3m pointOnPlane) {
    var a = normal.X;
    var b = normal.Y;
    var c = normal.Z;
    var d = pointOnPlane.X;
    var e = pointOnPlane.Y;
    var f = pointOnPlane.Z;

    var t = (a * d - a * X + b * e - b * Y + c * f - c * Z) /
            (a * a + b * b + c * c);
    return new Vector3m(X + t * a, Y + t * b, Z + t * c);
  }

  public double Length() {
    return System.Math.Sqrt(Dot(this).ToDouble());
  }

  public (float, float, float) ToFloat() {
    return (X.ToSingle(), Y.ToSingle(), Z.ToSingle());
  }

  public (double, double, double) ToDouble() {
    return (X.ToDouble(), Y.ToDouble(), Z.ToDouble());
  }

  public ERational LengthSquared() {
    return Dot(this);
  }

  public double RoughAngleBetween(Vector3m a) {
    var dot = Dot(a);
    var len = Length() * a.Length();
    if (len == 0) {
      return 0;
    }

    return System.Math.Acos(dot.ToDouble() / len);
  }

  public Vector3m ShortenByLargestComponent() {
    if (this.LengthSquared().IsZero)
      return new Vector3m(0, 0, 0);
    var absNormal = Absolute();
    ERational largestValue = 0;
    if (absNormal.X.CompareTo(absNormal.Y) >= 0 &&
        absNormal.X.CompareTo(absNormal.Z) >= 0)
      largestValue = absNormal.X;
    else if (absNormal.Y.CompareTo(absNormal.X) >= 0 &&
             absNormal.Y.CompareTo(absNormal.Z) >= 0)
      largestValue = absNormal.Y;
    else {
      largestValue = absNormal.Z;
    }

    Debug.Assert(!largestValue.IsZero);
    return this / largestValue;
  }

  public Vector3m Cross(Vector3m a) {
    return new Vector3m(
        this.Y * a.Z - this.Z * a.Y,
        this.Z * a.X - this.X * a.Z,
        this.X * a.Y - this.Y * a.X
    );
  }

  internal bool IsZero() {
    return X.IsZero && Y.IsZero && Z.IsZero;
  }

  public override bool Equals(object obj) {
    var other = obj as Vector3m;

    if (other == null) {
      return false;
    }

    return X.CompareTo(other.X) == 0 &&
           Y.CompareTo(other.Y) == 0 &&
           Z.CompareTo(other.Z) == 0;
  }

  public override int GetHashCode() {
    return this.X.GetHashCode() ^ this.Y.GetHashCode() ^ this.Z.GetHashCode();
  }

  public static Vector3m operator+(Vector3m a, Vector3m b) {
    return a.Plus(b);
  }

  public static Vector3m operator-(Vector3m a, Vector3m b) {
    return a.Minus(b);
  }

  public static Vector3m operator*(Vector3m a, ERational d) {
    return new Vector3m(a.X * d, a.Y * d, a.Z * d);
  }

  public static Vector3m operator/(Vector3m a, ERational d) {
    return a.DividedBy(d);
  }

  public override string ToString() {
    return "Vector:" +
           " " +
           X.ToDouble() +
           " " +
           Y.ToDouble() +
           " " +
           Z.ToDouble() +
           " ";
  }

  public static Vector3m PlaneNormal(Vector3m v0, Vector3m v1, Vector3m v2) {
    Vector3m a = v1 - v0;
    Vector3m b = v2 - v0;
    return a.Cross(b);
  }

  public bool SameDirection(Vector3m he) {
    var res = Cross(he);
    return res.X.IsZero && res.Y.IsZero && res.Z.IsZero;
  }

  public bool SameExactDirection(Vector3m he) {
    return SameDirection(he) && Dot(he).Sign == 1;
  }
}

class DirectionEqualComparer : IEqualityComparer<Vector3m> {
  public bool Equals(Vector3m x, Vector3m y) {
    if (x == null && y == null) {
      return true;
    }

    if (x == null || y == null) {
      return false;
    }

    return x.SameExactDirection(y);
  }

  public int GetHashCode(Vector3m obj) {
    //needs to return a constant value to ensure that the Equals method is called
    return 1;
  }
}