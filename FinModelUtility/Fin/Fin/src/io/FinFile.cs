using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Runtime.CompilerServices;

using fin.util.asserts;

namespace fin.io;

public readonly struct FinFile(string fullName) : ISystemFile {
  public override string ToString() => this.DisplayFullName;


  // Equality
  public bool Equals(object? other) {
    if (ReferenceEquals(this, other)) {
      return true;
    }

    if (other is not IReadOnlySystemIoObject otherSelf) {
      return false;
    }

    return this.Equals(otherSelf);
  }

  public bool Equals(ISystemIoObject? other)
    => this.Equals(other as IReadOnlyTreeIoObject);

  public bool Equals(IReadOnlySystemIoObject? other)
    => this.Equals(other as IReadOnlyTreeIoObject);

  public bool Equals(IReadOnlyTreeIoObject? other)
    => this.FullPath == other?.FullPath;


  // File fields
  public string Name => FinIoStatic.GetName(this.FullPath);
  public string FullPath { get; } = fullName;
  public string DisplayFullName => this.FullPath;


  // Ancestry methods
  public string? GetParentFullPath()
    => FinIoStatic.GetParentFullName(this.FullPath);

  public ISystemDirectory AssertGetParent() {
    if (this.TryGetParent(out ISystemDirectory parent)) {
      return parent;
    }

    throw new Exception("Expected parent directory to exist!");
  }

  public bool TryGetParent(out ISystemDirectory parent) {
    var parentName = this.GetParentFullPath();
    if (parentName != null) {
      parent = new FinDirectory(parentName);
      return true;
    }

    parent = default;
    return false;
  }

  public IEnumerable<ISystemDirectory> GetAncestry()
    => this.GetUpwardAncestry_().Reverse();

  private IEnumerable<ISystemDirectory> GetUpwardAncestry_() {
    if (!this.TryGetParent(out ISystemDirectory firstParent)) {
      yield break;
    }

    var current = firstParent;
    while (current.TryGetParent(out var parent)) {
      yield return parent;
      current = parent;
    }
  }


  // File methods
  public bool Exists => FinFileStatic.Exists(this.FullPath);

  public string DisplayFullPath => this.FullPath;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Delete() => FinFileStatic.Delete(this.FullPath);

  public string FileType => FinFileStatic.GetExtension(this.FullPath);

  public string FullNameWithoutExtension
    => FinFileStatic.GetNameWithoutExtension(this.FullPath);

  public string NameWithoutExtension
    => FinFileStatic.GetNameWithoutExtension(this.Name);

  public ISystemFile CloneWithFileType(string newExtension) {
    Asserts.True(newExtension.StartsWith("."),
                 $"'{newExtension}' is not a valid extension!");
    return new FinFile(this.FullNameWithoutExtension + newExtension);
  }


  // Read methods
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public FileSystemStream OpenRead()
    => FinFileStatic.OpenRead(this.FullPath);


  // Write methods
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public FileSystemStream OpenWrite()
    => FinFileStatic.OpenWrite(this.FullPath);
}