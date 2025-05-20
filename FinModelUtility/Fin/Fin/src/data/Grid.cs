using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using fin.util.asserts;

using schema.readOnly;

namespace fin.data;

[GenerateReadOnly]
public partial interface IGrid<T> {
  new int Width { get; }
  new int Height { get; }
  new T this[int x, int y] { get; set; }
}

public class Grid<T> : IGrid<T> {
  private IList<T> impl_;

  public Grid(int width, int height, T defaultValue = default!) {
    this.Width = width;
    this.Height = height;

    this.impl_ = new T[width * height];
    for (var i = 0; i < this.impl_.Count; ++i) {
      this.impl_[i] = defaultValue;
    }
  }

  public Grid(int width, int height, Func<T> defaultValueHandler) {
    this.Width = width;
    this.Height = height;

    this.impl_ = new T[width * height];
    for (var i = 0; i < this.impl_.Count; ++i) {
      this.impl_[i] = defaultValueHandler();
    }
  }

  public int Width { get; }
  public int Height { get; }

  public T this[int x, int y] {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.impl_[this.GetIndex_(x, y)];
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    set => this.impl_[this.GetIndex_(x, y)] = value;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private int GetIndex_(int x, int y) {
    if (!(x >= 0 && x < this.Width && y >= 0 && y < this.Height)) {
      Asserts.Fail(
          $"Expected ({x}, {y}) to be a valid index in grid of size ({this.Width}, {this.Height}).");
    }

    return y * this.Width + x;
  }
}

public static class GridExtensions {
  public static void Fill<T>(this IGrid<T> grid, T value) {
    for (var y = 0; y < grid.Height; ++y) {
      for (var x = 0; x < grid.Width; ++x) {
        grid[x, y] = value;
      }
    }
  }

  public static IEnumerable<T> GetColumn<T>(this IReadOnlyGrid<T> grid, int x) {
    for (var y = 0; y < grid.Height; ++y) {
      yield return grid[x, y];
    }
  }

  public static IEnumerable<T> GetRow<T>(this IReadOnlyGrid<T> grid, int y) {
    for (var x = 0; x < grid.Width; ++x) {
      yield return grid[x, y];
    }
  }
}