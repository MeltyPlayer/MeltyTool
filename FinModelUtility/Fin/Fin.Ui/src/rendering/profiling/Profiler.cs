using System.Diagnostics;

using OpenTK.Graphics.ES30;

namespace fin.ui.rendering.profiling;

public class ProfilerStats : IProfilerStats {
  private readonly Stopwatch frameStopwatch_ = new();

  private readonly Stopwatch cpuLookUpAnimationFrameStopwatch_ = new();
  private readonly Stopwatch cpuMatrixMathStopwatch_ = new();
  private readonly Stopwatch cpuBindTextureStopwatch_ = new();
  private readonly Stopwatch cpuBindShaderStopwatch_ = new();
  private readonly Stopwatch cpuUpdateUniformStopwatch_ = new();

  private static int gpuBackgroundQueryId_;
  private static int gpuModelQueryId_;
  private static int gpuBindTextureQueryId_;
  private static int gpuBindShaderQueryId_;
  private static int gpuUpdateUniformQueryId_;

  public static ProfilerStats INSTANCE { get; } = new();

  public static void InitGl() {
    gpuBackgroundQueryId_ = GL.GenQuery();
    gpuModelQueryId_ = GL.GenQuery();
    gpuBindTextureQueryId_ = GL.GenQuery();
    gpuBindShaderQueryId_ = GL.GenQuery();
    gpuUpdateUniformQueryId_ = GL.GenQuery();
  }

  public float FrameTotalSeconds { get; private set; }

  public void StartFrame() => this.frameStopwatch_.Restart();

  public void StopFrame() => this.FrameTotalSeconds
      = (float) this.frameStopwatch_.Elapsed.TotalSeconds;

  public float CpuLookUpAnimationFrameSeconds { get; private set; }
  public float CpuMatrixMathSeconds { get; private set; }
  public float CpuBindTextureSeconds { get; private set; }
  public float CpuBindShaderSeconds { get; private set; }
  public float CpuUpdateUniformSeconds { get; private set; }

  public void Start(CpuStat cpuStat)
    => (cpuStat switch {
        CpuStat.LOOK_UP_ANIMATION_FRAME
            => this.cpuLookUpAnimationFrameStopwatch_,
        CpuStat.MATRIX_MATH
            => this.cpuMatrixMathStopwatch_,
        CpuStat.BIND_SHADER
            => this.cpuBindShaderStopwatch_,
        CpuStat.BIND_TEXTURE
            => this.cpuBindTextureStopwatch_,
        CpuStat.UPDATE_UNIFORM
            => this.cpuUpdateUniformStopwatch_,
        _ => throw new ArgumentOutOfRangeException(
            nameof(cpuStat),
            cpuStat,
            null)
    }).Restart();

  public void Stop(CpuStat cpuStat) {
    throw new NotImplementedException();
  }

  public float GpuBackgroundSeconds { get; private set; }
  public float GpuModelSecondsSeconds { get; private set; }
  public float GpuBindTextureSeconds { get; private set; }
  public float GpuBindShaderSeconds { get; private set; }
  public float GpuUpdateUniformSeconds { get; private set; }

  public void Start(GpuStat gpuStat) {
  }

  public void Stop(GpuStat gpuStat) {
  }

  private static int GetQueryId_(GpuStat gpuStat)
    => gpuStat switch {
        GpuStat.BACKGROUND     => gpuBackgroundQueryId_,
        GpuStat.MODEL          => gpuModelQueryId_,
        GpuStat.BIND_SHADER    => gpuBindShaderQueryId_,
        GpuStat.BIND_TEXTURE   => gpuBindTextureQueryId_,
        GpuStat.UPDATE_UNIFORM => gpuUpdateUniformQueryId_,
        _ => throw new ArgumentOutOfRangeException(
            nameof(gpuStat),
            gpuStat,
            null)
    };
}