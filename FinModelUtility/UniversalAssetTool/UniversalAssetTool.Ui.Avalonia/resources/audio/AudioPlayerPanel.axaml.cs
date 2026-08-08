using System;
using System.Collections.Generic;
using System.Linq;

using Avalonia.Controls;
using Avalonia.Interactivity;

using fin.audio;
using fin.audio.io;
using fin.audio.io.importers.ogg;
using fin.data;
using fin.io;
using fin.services;
using fin.ui;
using fin.ui.avalonia.controls;
using fin.ui.playback.al;
using fin.util.asserts;
using fin.util.enumerables;
using fin.util.time;

using ReactiveUI;

using uni.api;

namespace uni.ui.avalonia.resources.audio;

public interface IAudioPlayerPanelViewModel {
  IReadOnlyList<IAudioFileBundle>? AudioFileBundles { get; }
  IReadOnlyList<ILoadedAudioBuffer<short>>? LoadedAudioBuffers { get; }
  ILoadedAudioBuffer<short>? AudioBuffer { get; }
  IAotAudioPlayback<short>? ActivePlayback { get; }
}

public sealed class AudioPlayerPanelViewModelForDesigner
    : BViewModel, IAudioPlayerPanelViewModel {
  public AudioPlayerPanelViewModelForDesigner() {
    var bundle = new OggAudioFileBundle(new FinFile("//fake/file.ogg"));
    this.AudioFileBundles = [bundle];
  }

  public IReadOnlyList<IAudioFileBundle>? AudioFileBundles {
    get;
    set => this.RaiseAndSetIfChanged(ref field, value);
  }

  public IReadOnlyList<ILoadedAudioBuffer<short>>? LoadedAudioBuffers => null;
  public ILoadedAudioBuffer<short>? AudioBuffer => null;
  public IAotAudioPlayback<short>? ActivePlayback => null;
}

public sealed class AudioPlayerPanelViewModel
    : BViewModel, IAudioPlayerPanelViewModel {
  private readonly IAudioManager<short> audioManager_ =
      AlAudioManager.TryToCreateOrStub();
  private readonly IAudioPlayer<short> audioPlayer_;
  private readonly TimedCallback playNextCallback_;

  private ShuffledListView<ILoadedAudioBuffer<short>>? shuffledListView_;

  private IAotAudioPlayback<short>? activePlayback_;

  private readonly object playNextLock_ = new();

  public AudioPlayerPanelViewModel() {
    this.audioPlayer_ = this.audioManager_.AudioPlayer;

    this.playNextCallback_ = new TimedCallback(
        () => {
          if (this.shuffledListView_ == null) {
            return;
          }

          this.PlayRandomFromShuffledList(true);
        },
        .1f);
  }

  public IReadOnlyList<IAudioFileBundle>? AudioFileBundles {
    get;
    set {
      if ((value?.Count ?? 0) == 0) {
        value = null;
      }

      if (field.SequenceEqualOrBothEmpty(value)) {
        this.LoadedAudioBuffers = null;
        return;
      }

      this.RaiseAndSetIfChanged(ref field, value);
      field = value;

      var ar = new GlobalAudioReader();
      this.LoadedAudioBuffers
          = value?.SelectMany(a => ar.ImportAudio(this.audioManager_, a))
                 .ToArray();
    }
  }

  public IReadOnlyList<ILoadedAudioBuffer<short>>? LoadedAudioBuffers {
    get;
    set {
      if ((value?.Count ?? 0) == 0) {
        value = null;
      }

      if (field.SequenceEqualOrBothEmpty(value)) {
        this.AudioBuffer = null;
        return;
      }

      this.RaiseAndSetIfChanged(ref field, value);
      field = value;

      this.shuffledListView_
          = value != null
              ? new ShuffledListView<ILoadedAudioBuffer<short>>(value)
              : null;

      this.PlayRandomFromShuffledList();
    }
  }


  public void PlayRandomFromShuffledList(bool onlyIfNotPlaying = false) {
    lock (this.playNextLock_) {
      if (onlyIfNotPlaying &&
          this.activePlayback_?.State is PlaybackState.PLAYING
                                         or PlaybackState.PAUSED) {
        return;
      }

      if (this.shuffledListView_ != null &&
          this.shuffledListView_.TryGetNext(out var nextAudioFileBundle)) {
        this.AudioBuffer = nextAudioFileBundle;
      } else {
        this.AudioBuffer = null;
      }
    }
  }

  public ILoadedAudioBuffer<short>? AudioBuffer {
    get;
    set {
      if (value == field) {
        if (value != null) {
          this.activePlayback_.Stop();
          this.activePlayback_.Play();
        }

        return;
      }

      this.RaiseAndSetIfChanged(ref field, value);

      IAotAudioPlayback<short>? newPlayback = null;
      if (value != null) {
        newPlayback = this.audioPlayer_.CreatePlayback(value);
      }

      this.ActivePlayback = newPlayback;
    }
  }

  public IAotAudioPlayback<short>? ActivePlayback {
    get => this.activePlayback_;
    set {
      if (value == this.activePlayback_) {
        return;
      }

      this.activePlayback_?.Stop();
      this.activePlayback_?.Dispose();

      this.RaiseAndSetIfChanged(ref this.activePlayback_, value);

      if (value != null) {
        value.Volume = .1f;
        this.IsPlaying = true;
      } else {
        this.IsPlaying = false;
      }
    }
  }

  public bool IsPlaying {
    get;
    set {
      this.RaiseAndSetIfChanged(ref field, value);

      if (this.activePlayback_ != null) {
        if (field) {
          this.activePlayback_.Play();
        } else {
          this.activePlayback_.Pause();
        }
      }

      this.PlayButtonTooltip = value ? "Playing" : "Paused";
    }
  }

  public string PlayButtonTooltip {
    get;
    set => this.RaiseAndSetIfChanged(ref field, value);
  }
}

public partial class AudioPlayerPanel
    : BUserControl<AudioPlayerPanelViewModel> {
  public AudioPlayerPanel() {
    this.InitializeComponent();
  }

  private void ClosePanel_(object? sender, RoutedEventArgs e)
    => this.ViewModel.AudioFileBundles = null;

  private void PlayNextRandom_(object? sender, RoutedEventArgs e)
    => this.ViewModel.PlayRandomFromShuffledList();
}