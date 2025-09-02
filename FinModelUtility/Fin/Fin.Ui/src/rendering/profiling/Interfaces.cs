namespace fin.ui.rendering.profiling;

public enum CpuStat {
  LOOK_UP_ANIMATION_FRAME,
  MATRIX_MATH,
  BIND_TEXTURE,
  BIND_SHADER,
  UPDATE_UNIFORM,
}

public enum GpuStat {
  BACKGROUND,
  MODEL,
  BIND_TEXTURE,
  BIND_SHADER,
  UPDATE_UNIFORM,
}

public interface IProfilerStats {
  float FrameTotalSeconds { get; }
  void StartFrame();
  void StopFrame();
  
  float CpuTotalSeconds
    => this.CpuLookUpAnimationFrameSeconds +
       this.CpuMatrixMathSeconds +
       this.CpuBindTextureSeconds +
       this.CpuBindShaderSeconds +
       this.CpuUpdateUniformSeconds;

  float CpuLookUpAnimationFrameSeconds { get; }
  float CpuMatrixMathSeconds { get; }
  float CpuBindTextureSeconds { get; }
  float CpuBindShaderSeconds { get; }
  float CpuUpdateUniformSeconds { get; }


  void Start(CpuStat cpuStat);
  void Stop(CpuStat cpuStat);

  float GpuTotalSeconds
    => this.GpuBackgroundSeconds +
       this.GpuModelSecondsSeconds +
       this.GpuBindTextureSeconds +
       this.GpuBindShaderSeconds +
       this.GpuUpdateUniformSeconds;

  float GpuBackgroundSeconds { get; }
  float GpuModelSecondsSeconds { get; }
  float GpuBindTextureSeconds { get; }
  float GpuBindShaderSeconds { get; }
  float GpuUpdateUniformSeconds { get; }

  void Start(GpuStat gpuStat);
  void Stop(GpuStat gpuStat);
}