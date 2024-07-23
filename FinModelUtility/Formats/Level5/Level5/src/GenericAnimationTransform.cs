namespace level5;

public enum AnimNodeHashType {
  Name = 0,
  CRC32C = 1,
}

/// <summary>
/// Stores animation data for transformations
/// Information is stored in tracks <see cref="GenericTransformTrack"/>
/// </summary>
public class GenericAnimationTransform {
  public string Name { get; set; }

  public uint Hash { get; set; }

  public AnimNodeHashType HashType { get; set; } = AnimNodeHashType.Name;

  public Dictionary<AnimationTrackFormat, GenericTransformTrack> Tracks {
    get;
  } = new();

  /// <summary>
  /// adds a new key frame to the animation
  /// </summary>
  /// <param name="frame"></param>
  /// <param name="value"></param>
  /// <param name="type"></param>
  /// <param name="interpolationType"></param>
  public void AddKey(float frame,
                     float value,
                     AnimationTrackFormat type,
                     InterpolationType interpolationType =
                         InterpolationType.Linear) {
      if (!Tracks.TryGetValue(type, out var track)) {
        track = new GenericTransformTrack(type);
        Tracks[type] = track;
      }

      track.AddKey(frame, value, interpolationType);
    }
}


public enum AnimationTrackFormat {
  TranslateX,
  TranslateY,
  TranslateZ,
  RotateX,
  RotateY,
  RotateZ,
  ScaleX,
  ScaleY,
  ScaleZ,
  CompensateScale
}

public static class AnimationTrackFormatExtensions {
  public static bool IsTranslation(this AnimationTrackFormat value,
                                   out int axis)
    => value.IsInRange(AnimationTrackFormat.TranslateX, 3, out axis);

  public static bool IsRotation(this AnimationTrackFormat value,
                                out int axis)
    => value.IsInRange(AnimationTrackFormat.RotateX, 3, out axis);

  public static bool IsScale(this AnimationTrackFormat value,
                             out int axis)
    => value.IsInRange(AnimationTrackFormat.ScaleX, 3, out axis);

  public static bool IsInRange(this AnimationTrackFormat value,
                               AnimationTrackFormat min,
                               int max,
                               out int id) {
      id = value - min;
      return id >= 0 && id < max;
    }
}

/// <summary>
/// A track for <see cref="SBTransformAnimation"/>
/// See <see cref="AnimationTrackFormat"/> for supported types
/// </summary>
public class GenericTransformTrack(AnimationTrackFormat type) {
  public AnimationTrackFormat Type { get; internal set; } = type;

  public GenericKeyGroup<float> Keys { get; } = new GenericKeyGroup<float>();

  public void AddKey(float frame,
                     float value,
                     InterpolationType interpolationType =
                         InterpolationType.Linear,
                     float InTan = 0,
                     float OutTan = float.MaxValue) {
      Keys.AddKey(frame, value, interpolationType, InTan, OutTan);
    }
}