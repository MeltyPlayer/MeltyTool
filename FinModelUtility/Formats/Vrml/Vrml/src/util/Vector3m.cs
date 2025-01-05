using PeterO.Numbers;

namespace vrml.util;

public class Vector3m {
  public Vector3m(ERational x, ERational y, ERational z) {
    this.X = x;
    this.Y = y;
    this.Z = z;
  }

  public static Vector3m Zero() {
    return new Vector3m(0, 0, 0);
  }

  public ERational X { get; set; }
  public ERational Y { get; set; }
  public ERational Z { get; set; }

  public Vector3m Minus(Vector3m a) {
    return new Vector3m(this.X - a.X, this.Y - a.Y, this.Z - a.Z);
  }

  public ERational Dot(Vector3m a) {
    return this.X * a.X + this.Y * a.Y + this.Z * a.Z;
  }

  public ERational LengthSquared() {
    return this.Dot(this);
  }

  public Vector3m Cross(Vector3m a) {
    return new Vector3m(
        this.Y * a.Z - this.Z * a.Y,
        this.Z * a.X - this.X * a.Z,
        this.X * a.Y - this.Y * a.X
    );
  }

  public override bool Equals(object obj) {
    var other = obj as Vector3m;

    if (other == null) {
      return false;
    }

    return this.X.CompareTo(other.X) == 0 &&
           this.Y.CompareTo(other.Y) == 0 &&
           this.Z.CompareTo(other.Z) == 0;
  }

  public override int GetHashCode() {
    return this.X.GetHashCode() ^ this.Y.GetHashCode() ^ this.Z.GetHashCode();
  }

  public static Vector3m operator-(Vector3m a, Vector3m b) {
    return a.Minus(b);
  }
}