using fin.audio;

using OpenTK.Audio.OpenAL;

namespace fin.ui.playback.al;

public partial class AlAudioManager {
  private class AlJitAudioPlayback : BAlAudioPlayback,
                                     IJitAudioPlayback<short> {
    private readonly List<SingleBuffer> allBuffers_ = [];
    private readonly Queue<SingleBuffer> readyForDataBuffers_ = new();
    private readonly Dictionary<int, SingleBuffer> buffersById_ = new();

    public IJitAudioDataSource<short> TypedSource { get; }

    public AlJitAudioPlayback(IAudioPlayer<short> player,
                              IJitAudioDataSource<short> source,
                              uint bufferCount)
        : base(player, source) {
      this.TypedSource = source;

      for (var i = 0; i < bufferCount; ++i) {
        var buffer = new SingleBuffer(source.AudioChannelsType,
                                      source.Frequency);
        this.allBuffers_.Add(buffer);
        this.readyForDataBuffers_.Enqueue(buffer);
        this.buffersById_[buffer.AlBufferId] = buffer;
      }

      source.OnNextBufferPopulated += data => {
        if (this.State != PlaybackState.PLAYING) {
          return;
        }

        this.FreeUpProcessedBuffers_();

        if (this.readyForDataBuffers_.TryDequeue(out var nextBuffer)) {
          nextBuffer.PopulateAndQueueUpInSource(data, this.AlSourceId);
        }
      };
    }

    protected override void DisposeInternal() {
      foreach (var buffer in this.allBuffers_) {
        buffer.Dispose();
      }
    }

    private void FreeUpProcessedBuffers_() {
      AL.GetSource(this.AlSourceId,
                   ALGetSourcei.BuffersProcessed,
                   out var numBuffersProcessed);
      AssertNoError_();

      if (numBuffersProcessed > 0) {
        var unqueuedBuffers = AL.SourceUnqueueBuffers((int) this.AlSourceId,
          numBuffersProcessed);
        AssertNoError_();

        foreach (var unqueuedBuffer in unqueuedBuffers) {
          this.readyForDataBuffers_.Enqueue(
              this.buffersById_[unqueuedBuffer]);
        }
      }
    }

    private static void AssertNoError_() {
      var error = AL.GetError();
      if (error != ALError.NoError) {
        ;
      }
    }

    private class SingleBuffer : IDisposable {
      public int AlBufferId { get; }

      private bool isDisposed_;

      private readonly AudioChannelsType audioChannelsType_;
      private readonly int frequency_;

      public SingleBuffer(
          AudioChannelsType audioChannelsType,
          int frequency) {
        this.audioChannelsType_ = audioChannelsType;
        this.frequency_ = frequency;

        AL.GenBuffer(out var alBufferId);
        this.AlBufferId = (int) alBufferId;
      }

      ~SingleBuffer() => this.ReleaseUnmanagedResources_();

      public void Dispose() {
        this.AssertNotDisposed_();

        this.ReleaseUnmanagedResources_();
        GC.SuppressFinalize(this);
      }

      private void ReleaseUnmanagedResources_() {
        this.isDisposed_ = true;
        AL.DeleteBuffer(this.AlBufferId);
      }

      private void AssertNotDisposed_() {
        if (this.isDisposed_) {
          throw new Exception("Expected active sound to not be disposed");
        }
      }

      public void PopulateAndQueueUpInSource(
          short[] shortBufferData,
          int sourceId) {
        this.AssertNotDisposed_();

        ALFormat bufferFormat = default;
        switch (this.audioChannelsType_) {
          case AudioChannelsType.MONO: {
            bufferFormat = ALFormat.Mono16;
            break;
          }
          case AudioChannelsType.STEREO: {
            bufferFormat = ALFormat.Stereo16;
            break;
          }
        }

        var byteCount = 2 * shortBufferData.Length;
        var byteBufferData = new byte[byteCount];
        Buffer.BlockCopy(shortBufferData,
                         0,
                         byteBufferData,
                         0,
                         byteCount);

        AL.BufferData(this.AlBufferId,
                      bufferFormat,
                      byteBufferData,
                      this.frequency_);
        AssertNoError_();

        AL.SourceQueueBuffer((int) sourceId, (int) this.AlBufferId);
        AssertNoError_();
      }
    }
  }
}