using System;

using Avalonia.OpenGL;

using OpenTK;

namespace uni.ui.avalonia.common.gl;

public class AvaloniaOpenTkContext : IBindingsContext {
  private readonly GlInterface glInterface_;

  public AvaloniaOpenTkContext(GlInterface glInterface) {
      this.glInterface_ = glInterface;
    }

  public IntPtr GetProcAddress(string procName)
    => this.glInterface_.GetProcAddress(procName);
}