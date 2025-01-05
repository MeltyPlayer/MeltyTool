using PeterO.Numbers;

namespace vrml.util;

public class Vector3m : ICloneable {
  internal DynamicProperties DynamicProperties = new DynamicProperties();

  public Vector3m(ERational x, ERational y, ERational z) {
    this.X = x;
    this.Y = y;
    this.Z = z;
  }

  public Vector3m(Vector3m v) {
    this.X = v.X;
    this.Y = v.Y;
    this.Z = v.Z;
  }

  public static Vector3m Zero() {
    return new Vector3m(0, 0, 0);
  }

  public ERational X { get; set; }
  public ERational Y { get; set; }
  public ERational Z { get; set; }

  public object Clone() {
    return new Vector3m(this.X, this.Y, this.Z);
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

  public double Length() {
    return System.Math.Sqrt(this.Dot(this).ToDouble());
  }

  public (double, double, double) ToDouble() {
    return (this.X.ToDouble(), this.Y.ToDouble(), this.Z.ToDouble());
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
}