using System.Runtime.CompilerServices;

namespace fin.image.io.tile;

/// <summary>
///   Stolen from:
///   https://github.com/magcius/noclip.website/blob/master/src/oot3d/pica_texture.ts
/// </summary>
public sealed class MortonPixelIndexer : IPixelIndexer {
  public void GetPixelCoordinates(int index, out int x, out int y) {
    x = this.Morton7_(index);
    y = this.Morton7_(index >>> 1);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private int Morton7_(int n)
      // 0a0b0c => 000abc
    => ((n >>> 2) & 0x04) | ((n >>> 1) & 0x02) | (n & 0x01);
}