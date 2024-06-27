namespace fin.audio.midi;

/// <summary>
///   Summaries of each field shamelessly stolen from:
///   https://support.apple.com/guide/logicpro/attack-decay-sustain-and-release-lgsife419620/mac
/// </summary>
public interface IEnvelope {
  /// <summary>
  ///   The time it takes for the signal to rise from an amplitude of 0 to
  ///   100% (full amplitude).
  /// </summary>
  float AttackSeconds { get; }

  /// <summary>
  ///   The time it takes for the signal to fall from 100% amplitude to the
  ///   designated sustain level.
  /// </summary>
  float DecaySeconds { get; }

  /// <summary>
  ///   The steady amplitude level produced when a key is held down.
  ///
  ///   Note: If a key is released during the attack or decay stage, the
  ///   sustain phase is usually skipped. A sustain level of 0 produces a
  ///   piano-like—or percussive—envelope, with no continuous steady level,
  ///   even when a key is held.
  /// </summary>
  float SustainFraction { get; }

  /// <summary>
  ///   The time it takes for the sound to decay from the sustain level to an
  ///   amplitude of 0 when the key is released.
  /// </summary>
  float ReleaseSeconds { get; }
}