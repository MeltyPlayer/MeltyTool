namespace fin.scene.components;

public readonly struct IndefiniteRepeatArgs() {
  public static IndefiniteRepeatArgs RepeatIndefinitely()
    => new() {
        IsIndefinite = true,
        PlayCount = 0,
        IsInterruptible = true,
    };

  public static IndefiniteRepeatArgs RepeatTimes(uint count, bool isInterruptible)
    => new() {
        IsIndefinite = false,
        PlayCount = count,
        IsInterruptible = isInterruptible,
    };

  public static IndefiniteRepeatArgs Once(bool isInterruptible)
    => new() {
        IsIndefinite = false,
        PlayCount = 1,
        IsInterruptible = isInterruptible,
    };

  public bool IsIndefinite { get; private init; } = false;
  public uint PlayCount { get; private init; } = 1;
  public bool IsInterruptible { get; private init; } = false;
}

public readonly struct DefiniteRepeatArgs {
  public static DefiniteRepeatArgs RepeatIndefinitely()
    => new() {
        PlayCount = 0,
        IsInterruptible = true,
    };

  public static DefiniteRepeatArgs RepeatTimes(uint count, bool isInterruptible)
    => new() {
        PlayCount = count,
        IsInterruptible = isInterruptible,
    };

  public static DefiniteRepeatArgs Once(bool isInterruptible)
    => new() {
        PlayCount = 1,
        IsInterruptible = isInterruptible,
    };

  public DefiniteRepeatArgs() { }

  public uint PlayCount { get; private init; } = 1;
  public bool IsInterruptible { get; private init; } = false;
}