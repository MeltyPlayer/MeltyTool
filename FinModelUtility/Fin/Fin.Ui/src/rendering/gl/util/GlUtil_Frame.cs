using fin.data.queues;
using fin.util.time;

using OpenTK.Graphics.OpenGL4;

namespace fin.ui.rendering.gl;

public partial record GlState {
  public GlFence? RequestedFenceForEndOfFrame { get; set; }

  public int RequestedFenceForEndOfFrameCount { get; set; }
}

public static partial class GlUtil {
  public static void MarkStartOfFrame() {
    FrameTime.MarkStartOfFrame();
    GlFence.CleanUpLostSyncs();
  }

  public static void MarkEndOfFrame() {
    var requestedFenceForEndOfFrame = currentState_.RequestedFenceForEndOfFrame;
    if (requestedFenceForEndOfFrame != null) {
      for (var i = 0; i < currentState_.RequestedFenceForEndOfFrameCount; ++i) {
        requestedFenceForEndOfFrame.AddOwner();
      }

      requestedFenceForEndOfFrame.SubmitFenceSync();

      currentState_.RequestedFenceForEndOfFrameCount = 0;
      currentState_.RequestedFenceForEndOfFrame = null;
    }

    FrameTime.MarkEndOfFrameForFpsDisplay();
  }

  public static GlFence RequestFenceForEndOfFrame() {
    ++currentState_.RequestedFenceForEndOfFrameCount;
    return (currentState_.RequestedFenceForEndOfFrame
        ??= GlFence.RequestNewFence());
  }
}

public class GlFence {
  private static Queue<GlFence> freeFences_ = new(3);

  public static GlFence RequestNewFence() {
    if (freeFences_.TryDequeue(out var fence)) {
      return fence;
    }

    return new GlFence();
  }

  private GlFence() { }

  ~GlFence() {
    requestedSyncDeletions_.Enqueue(this.impl_);
  }

  private static readonly FinQueue<nint> requestedSyncDeletions_ = new();

  public static void CleanUpLostSyncs() {
    while (requestedSyncDeletions_.TryDequeue(out var sync)) {
      GL.DeleteSync(sync);
    }
  }

  private nint impl_
      = GL.FenceSync(SyncCondition.SyncGpuCommandsComplete, WaitSyncFlags.None);

  private int ownerCount_ = 0;

  public void SubmitFenceSync() {
    this.impl_ = GL.FenceSync(
        SyncCondition.SyncGpuCommandsComplete,
        WaitSyncFlags.None);
    //GL.Flush();
  }

  public void AddOwner() => ++this.ownerCount_;

  public void RemoveOwner() {
    --this.ownerCount_;

    if (this.ownerCount_ <= 0) {
      this.isSync_ = false;
      this.ownerCount_ = 0;
      freeFences_.Enqueue(this);
    }
  }

  private bool isSync_;

  public WaitSyncStatus Sync() {
    if (this.isSync_) {
      return WaitSyncStatus.AlreadySignaled;
    }

    if (this.impl_ == 0) {
      return WaitSyncStatus.WaitFailed;
    }

    var status = GL.ClientWaitSync(
        this.impl_,
        ClientWaitSyncFlags.SyncFlushCommandsBit,
        (long) 1e9);

    if (status != WaitSyncStatus.TimeoutExpired) {
      this.isSync_ = true;
    }

    return status;
  }
}