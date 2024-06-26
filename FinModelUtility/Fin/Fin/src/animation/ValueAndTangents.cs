namespace fin.animation;

public readonly record struct ValueAndTangents<T>(
    T IncomingValue,
    T OutgoingValue,
    float? IncomingTangent,
    float? OutgoingTangent) {
  public ValueAndTangents(T value) : this(value, null) { }

  public ValueAndTangents(T value, float? tangent)
      : this(value, tangent, tangent) { }

  public ValueAndTangents(T value,
                          float? incomingTangent,
                          float? outgoingTangent)
      : this(value,
             value,
             incomingTangent,
             outgoingTangent) { }
}