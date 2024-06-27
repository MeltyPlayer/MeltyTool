using System;
using System.Collections.Generic;
using System.Numerics;

using fin.data.disposables;
using fin.importers;
using fin.io;
using fin.io.bundles;

using schema.readOnly;

namespace fin.audio;
// Playback types

/// <summary>
///   Top-level type used to manage storing and playing back audio.
/// </summary>
public interface IAudioManager<TPcm> : IFinDisposable
    where TPcm : INumber<TPcm> {
  // TODO: Add support for looping a certain section of audio

  IAudioBuffer<TPcm> CreateAudioBuffer();

  ILoadedAudioBuffer<TPcm> CreateLoadedAudioBuffer(
      IFileBundle fileBundle,
      IReadOnlySet<IReadOnlyGenericFile> files);

  IJitAudioDataSource<TPcm> CreateJitAudioDataSource(
      AudioChannelsType audioChannelsType,
      int frequency);

  IAudioPlayer<TPcm> AudioPlayer { get; }
}

/// <summary>
///   A virtual speaker that can be used to play audio.
/// </summary>
public interface IAudioPlayer<TPcm> : IFinDisposable
    where TPcm : INumber<TPcm> {
  IAudioPlayer<TPcm> CreateSubPlayer();

  IAudioPlayback<TPcm> CreatePlayback(IAudioDataSource<TPcm> buffer);
  IAotAudioPlayback<TPcm> CreatePlayback(IAotAudioDataSource<TPcm> buffer);

  IJitAudioPlayback<TPcm> CreatePlayback(IJitAudioDataSource<TPcm> buffer,
                                         uint bufferCount);

  float Volume { get; set; }
}

public enum PlaybackState {
  UNDEFINED,
  STOPPED,
  PLAYING,
  PAUSED,
  DISPOSED,
}

/// <summary>
///   An actively played back sound.
/// </summary>
public interface IAudioPlayback<out TPcm> : IFinDisposable
    where TPcm : INumber<TPcm> {
  IAudioDataSource<TPcm> Source { get; }

  PlaybackState State { get; }
  void Play();
  void Stop();

  float Volume { get; set; }
}

/// <summary>
///   Type representing "ahead of time" audio that is currently being played
///   back.
///
///   Since the total length is known ahead of time, supports tracking the
///   offset, looping, and getting the current PCM amplitude.
/// </summary>
public interface IAotAudioPlayback<out TPcm> : IAudioPlayback<TPcm>
    where TPcm : INumber<TPcm> {
  IAotAudioDataSource<TPcm> TypedSource { get; }

  void Pause();

  int SampleOffset { get; set; }
  bool Looping { get; set; }

  // TODO: Support this for JIT too
  TPcm GetPcm(AudioChannelType channelType);
}

/// <summary>
///   Type representing just in time audio that is currently being played
///   back.
/// </summary>
public interface IJitAudioPlayback<TPcm> : IAudioPlayback<TPcm>
    where TPcm : INumber<TPcm> {
  IJitAudioDataSource<TPcm> TypedSource { get; }
}


// Data source types
public enum AudioChannelsType {
  UNDEFINED,
  MONO,
  STEREO,
}

public enum AudioChannelType {
  UNDEFINED,
  MONO,
  STEREO_LEFT,
  STEREO_RIGHT,
}


/// <summary>
///   Type representing some kind of audio data that can be played back.
/// </summary>
public interface IAudioDataSource<out TPcm> where TPcm : INumber<TPcm> {
  AudioChannelsType AudioChannelsType { get; }
  int Frequency { get; }
}


/// <summary>
///   Type representing audio data whose values are known ahead of time. This
///   makes it quite easy to handle playback without any kind of stuttering
///   or jitter.
/// </summary>
public interface IAotAudioDataSource<out TPcm> : IAudioDataSource<TPcm>
    where TPcm : INumber<TPcm> {
  int LengthInSamples { get; }
  TPcm GetPcm(AudioChannelType channelType, int sampleOffset);
}

/// <summary>
///   Type for storing static audio data that can be mutated dynamically.
/// </summary>
[GenerateReadOnly]
public partial interface IAudioBuffer<TPcm>
    : IAotAudioDataSource<TPcm> where TPcm : INumber<TPcm> {
  new int Frequency { get; set; }

  void SetPcm(TPcm[][] samples);

  void SetMonoPcm(TPcm[] samples);

  void SetStereoPcm(TPcm[] leftChannelSamples,
                    TPcm[] rightChannelSamples);
}

[GenerateReadOnly]
public partial interface ILoadedAudioBuffer<TPcm>
    : IAudioBuffer<TPcm>, IResource
    where TPcm : INumber<TPcm>;

/// <summary>
///   An audio data source that represents live data that is received just in
///   time, i.e. streamed data.
///
///   Since the data isn't known ahead of time, playback has to handle
///   rapidly processing samples as they're passed in.
/// </summary>
public interface IJitAudioDataSource<TPcm> : IAudioDataSource<TPcm>
    where TPcm : INumber<TPcm> {
  int LengthInQueuedSamples { get; }

  void PopulateNextBufferPcm(TPcm[] samples);

  event Action<TPcm[]> OnNextBufferPopulated;
}