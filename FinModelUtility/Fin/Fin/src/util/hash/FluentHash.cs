using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace fin.util.hash {
  public struct FluentHash {
    public int Hash { get; private set; }
    private readonly int primeCoefficient_;

    private FluentHash(int startingPrimeHash, int primeCoefficient) {
      this.Hash = startingPrimeHash;
      this.primeCoefficient_ = primeCoefficient;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FluentHash Start() => FluentHash.Start(17, 23);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FluentHash Start(int startingPrimeHash,
                                   int primeCoefficient)
      => new(startingPrimeHash, primeCoefficient);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FluentHash With<T>(T other) where T : notnull {
      this.Hash = this.Hash * this.primeCoefficient_ + other.GetHashCode();
      return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FluentHash With<T>(ReadOnlySpan<T> other) where T : notnull {
      var hash = this.Hash;
      for (var i = 0; i < other.Length; ++i) {
        hash = hash * this.primeCoefficient_ + other[i].GetHashCode();
      }

      this.Hash = hash;
      return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FluentHash With(ReadOnlySpan<byte> other) {
      var hash = this.Hash;
      foreach (var b in other) {
        hash = hash * this.primeCoefficient_ + b;
      }

      this.Hash = hash;
      return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FluentHash With<T>(IEnumerable<T> others) where T : notnull {
      var hash = this.Hash;
      foreach (var other in others) {
        hash = hash * this.primeCoefficient_ + other.GetHashCode();
      }

      this.Hash = hash;
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
}