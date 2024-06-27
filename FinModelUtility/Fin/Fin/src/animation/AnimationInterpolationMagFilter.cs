namespace fin.animation;

/// <summary>
///   How animation interpolation should be upscaled to higher frame rates.
///
///   If an animation was only ever designed to be viewed at a low frame
///   rate, then it may be jittery at higher frame rates.
/// </summary>
public enum AnimationInterpolationMagFilter {
  /// <summary>
  ///   Interpolates against the original frame rate and then returns the
  ///   nearest output value.
  /// </summary>
  ORIGINAL_FRAME_RATE_NEAREST,

  /// <summary>
  ///   Interpolates against the original frame rate, and then linearly
  ///   interpolates to upscale to higher frame rates.
  /// </summary>
  ORIGINAL_FRAME_RATE_LINEAR,

  /// <summary>
  ///   Interpolates as-is against any frame rate. May result in jitter at
  ///   higher frame rates.
  /// </summary>
  ANY_FRAME_RATE,
}