using System;
using System.Runtime.CompilerServices;

using CommunityToolkit.Diagnostics;

namespace fin.util.enums;

public static class EnumExtensions {
  /// <summary>
  ///   Shamelessly stolen from: https://stackoverflow.com/a/63530177
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining |
              MethodImplOptions.AggressiveOptimization)]
  public static unsafe bool CheckFlag<TEnum>(this TEnum instance, TEnum value)
      where TEnum : unmanaged, Enum {
    var pf = (byte*) &instance;
    var ps = (byte*) &value;

    for (var i = 0; i < sizeof(TEnum); i++) {
      if ((pf[i] & ps[i]) != ps[i]) {
        return false;
      }
    }

    return true;
  }

  extension<TEnum>(TEnum)
      where TEnum : unmanaged, Enum {
    public static TEnum Parse(ReadOnlySpan<char> text) {
      Guard.IsTrue(Enum.TryParse(text, out TEnum value));
      return value;
    }
  }
}