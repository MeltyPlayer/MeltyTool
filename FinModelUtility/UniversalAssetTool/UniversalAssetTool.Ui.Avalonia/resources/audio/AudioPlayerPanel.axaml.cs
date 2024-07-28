using System.Collections.Generic;
using System.Linq;

using Avalonia.Controls;
using Avalonia.Interactivity;

using fin.audio;
using fin.audio.io;
using fin.audio.io.importers.ogg;
using fin.data;
using fin.io;
using fin.ui.playback.al;
using fin.util.asserts;
using fin.util.enumerables;
using fin.util.time;

using ReactiveUI;

using uni.api;
using uni.ui.avalonia.ViewModels;

namespace uni.ui.avalonia.resources.audio;

public interface IAudioPlayerPanelViewModel {
  IReadOnlyList<IAudioFileBundle>? AudioFileBundles { get; }
  IReadOnlyList<ILoadedAudioBuffer<short>>? LoadedAudioBuffers { get; }
  ILoadedAudioBuffer<short>? AudioBuffer { get; }
  IAotAudioPlayback<short>? ActivePlayback { get; }
}

public class AudioPlayerPanelViewModelForDesigner
    : ViewModelBase, IAudioPlayerPanelViewModel {
  private IReadOnlyList<IAudioFileBundle>? audioFileBundles_;

  public AudioPlayerPanelViewModelForDesigner() {
    var bundle = new OggAudioFileBundle(new FinFile("//fake/file.ogg"));
    this.AudioFileBundles = [bundle];
  }

  public IReadOnlyList<IAudioFileBundle>? AudioFileBundles {
    get => this.audioFileBundles_;
    set => this.RaiseAndSetIfChanged(ref this.audioFileBundles_, value);
  }

  public IReadOnlyList<ILoadedAudioBuffer<short>>? LoadedAudioBuffers => null;
  public ILoadedAudioBuffer<short>? AudioBuffer => null;
  public IAotAudioPlayback<short>? ActivePlayback => null;
}

public class AudioPlayerPanelViewModel
    : ViewModelBase, IAudioPlayerPanelViewModel {
  private readonly IAudioManager<short> audioManager_ = new AlAudioManager();
  private readonly IAudioPlayer<short> audioPlayer_;
  private readonly TimedCallback playNextCallback_;

  private IReadOnlyList<IAudioFileBundle>? audioFileBundles_;
  private IReadOnlyList<ILoadedAudioBuffer<short>>? loadedAudioBuffers_;
  private ShuffledListView<ILoadedAudioBuffer<short>>? shuffledListView_;

  private ILoadedAudioBuffer<short>? audioBuffer_;
  private IAotAudioPlayback<short>? activePlayback_;

  private bool isPlaying_;
  private string playButtonTooltip_;

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
    get => this.audioFileBundles_;
    set {
      if ((value?.Count ?? 0) == 0) {
        value = null;
      }

      if (this.audioFileBundles_.SequenceEqualOrBothEmpty(value)) {
        return;
      }

      this.RaiseAndSetIfChanged(ref this.audioFileBundles_, value);
      this.audioFileBundles_ = value;

      var ar = new GlobalAudioReader();
      this.LoadedAudioBuffers
          = value?.SelectMany(a => ar.ImportAudio(this.audioManager_, a))
                 .ToArray();
    }
  }

  public IReadOnlyList<ILoadedAudioBuffer<short>>? LoadedAudioBuffers {
    get => this.loadedAudioBuffers_;
    set {
      if ((value?.Count ?? 0) == 0) {
        value = null;
      }

      if (this.loadedAudioBuffers_.SequenceEqualOrBothEmpty(value)) {
        return;
      }

      this.RaiseAndSetIfChanged(ref this.loadedAudioBuffers_, value);
      this.loadedAudioBuffers_ = value;

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
          this.activePlayback_?.State == PlaybackState.PLAYING) {
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
    get => this.audioBuffer_;
    set {
      if (value == this.audioBuffer_) {
        if (value != null) {
          this.activePlayback_.Stop();
          this.activePlayback_.Play();
        }

        return;
      }

      this.RaiseAndSetIfChanged(ref this.audioBuffer_, value);

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
    get => this.isPlaying_;
    set {
      this.RaiseAndSetIfChanged(ref this.isPlaying_, value);

      if (this.activePlayback_ != null) {
        if (this.isPlaying_) {
          this.activePlayback_.Play();
        } else {
          this.activePlayback_.Pause();
        }
      }

      this.PlayButtonTooltip = value ? "Playing" : "Paused";
    }
  }

  public string PlayButtonTooltip {
    get => this.playButtonTooltip_;
    set => this.RaiseAndSetIfChanged(ref this.playButtonTooltip_, value);
  }
}

public partial class AudioPlayerPanel : UserControl {
  public AudioPlayerPanel() {
    this.InitializeComponent();
  }

  private AudioPlayerPanelViewModel ViewModel_
    => this.DataContext.AssertAsA<AudioPlayerPanelViewModel>();

  private void ClosePanel_(object? sender, RoutedEventArgs e)
    => this.ViewModel_.AudioFileBundles = null;

  private void PlayNextRandom_(object? sender, RoutedEventArgs e)
    => this.ViewModel_.PlayRandomFromShuffledList();
}