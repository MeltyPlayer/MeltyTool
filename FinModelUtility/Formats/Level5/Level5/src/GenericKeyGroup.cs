namespace level5;

public enum InterpolationType {
  Constant,
  Linear,
  Hermite,
  Step
}

public readonly struct GenericAnimKey<T> {
  public required float Frame { get; init; } // todo: this needs to be read only or something
  public required float InTan { get; init; }
  public required float OutTan { get; init; }
  public required T Value { get; init; }
  public required InterpolationType InterpolationType { get; init; }
}

public class GenericKeyGroup<T> {
  /// <summary>
  /// A read-only view of the keys
  /// </summary>
  public LinkedList<GenericAnimKey<T>> Keys {
    get {
        return this.keys_;
      }
  }

  private LinkedList<GenericAnimKey<T>> keys_ = [];

  public void AddKey(float frame, T value, InterpolationType type = InterpolationType.Linear, float TanIn = 0, float TanOut = float.MaxValue) {
      GenericAnimKey<T> key = new() {
          Frame = frame,
          Value = value,
          InTan = TanIn,
          OutTan = TanOut == float.MaxValue ? TanIn : TanOut,
          InterpolationType = type,
      };
      this.keys_.AddLast(key);
    }
}