using System;

using Avalonia.OpenGL;

using OpenTK;

namespace uni.ui.avalonia.common.gl {
  public class
      AvaloniaOpenTkContext(GlInterface glInterface) : IBindingsContext {
    public IntPtr GetProcAddress(string procName)
      => glInterface.GetProcAddress(procName);
  }
}