using System;
using System.Collections.Generic;
using System.IO.Hashing;
using System.Runtime.CompilerServices;

namespace fin.util.hash;

public struct FluentHash {
  private static int NULL_HASH = 0xDEADBEEF.GetHashCode();

  public int Hash { get; private set; }
  private readonly int primeCoefficient_;

  private FluentHash(int startingPrimeHash, int primeCoefficient) {
    this.Hash = startingPrimeHash;
    this.primeCoefficient_ = primeCoefficient;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static FluentHash Start() => Start(17, 23);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static FluentHash Start(int startingPrimeHash,
                                 int primeCoefficient)
    => new(startingPrimeHash, primeCoefficient);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public FluentHash With(int otherHashCode) {
    this.Hash = this.Hash * this.primeCoefficient_ + otherHashCode;
    return this;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public FluentHash With<T>(T? other) where T : notnull
    => this.With(other?.GetHashCode() ?? NULL_HASH);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public FluentHash With<T>(ReadOnlySpan<T> others) where T : notnull {
    foreach (var other in others) {
      this.With(other);
    }

    return this;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public FluentHash With(ReadOnlySpan<byte> other)
    => this.With(Crc32.HashToUInt32(other));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public FluentHash With<T>(IEnumerable<T> others) where T : notnull {
    foreach (var other in others) {
      this.With(other);
    }

    return this;
  }


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static implicit operator int(FluentHash d) => d.Hash;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override int GetHashCode() => this.Hash;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override bool Equals(object? other)
    => other?.GetHashCode() == this.GetHashCode();
}