using System;

using CommunityToolkit.Diagnostics;

namespace fin.util.spans;

public static class SpanExtensions {
  extension<T>(ReadOnlySpan<T> self) where T : unmanaged {
    public unsafe void CopyTo(WriteOnlySpan<T> other) {
      Guard.IsLessThanOrEqualTo(self.Length, other.Length);

      var sizeofT = sizeof(T);
      fixed (T* srcPtr = &self[0]) {
        Buffer.MemoryCopy(srcPtr,
                          other.Pointer,
                          other.Length * sizeofT,
                          self.Length * sizeofT);
      }
    }
  }
}