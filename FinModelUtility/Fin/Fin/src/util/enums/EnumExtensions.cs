using System;

namespace fin.util.enums;

public static class EnumExtensions {
  public static bool CheckFlag<TEnum>(this TEnum instance, TEnum value)
      where TEnum : Enum {
    return instance.HasFlag(value);
  }
}