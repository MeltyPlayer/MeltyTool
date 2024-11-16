using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using CommunityToolkit.HighPerformance;

using fin.io;
using fin.util.asserts;
using fin.util.strings;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace fin.testing;

public static class GoldenAssert {
  private const string TMP_NAME = "tmp";

  public static ISystemDirectory GetRootGoldensDirectory(
      Assembly executingAssembly) {
    var assemblyName =
        executingAssembly.ManifestModule.Name.SubstringUpTo(".dll");

    var executingAssemblyDll = new FinFile(executingAssembly.Location);
    var executingAssemblyDir = executingAssemblyDll.AssertGetParent();

    var currentDir = executingAssemblyDir;
    while (!currentDir.Name.Equals(assemblyName,
                                   StringComparison.OrdinalIgnoreCase)) {
      currentDir = currentDir.AssertGetParent();
    }

    Assert.IsNotNull(currentDir);

    var gloTestsDir = currentDir;
    var goldensDirectory = gloTestsDir.AssertGetExistingSubdir("goldens");

    return goldensDirectory;
  }

  public static IEnumerable<IFileHierarchyDirectory> GetGoldenDirectories(
      ISystemDirectory rootGoldenDirectory) {
    var hierarchy = FileHierarchy.From(rootGoldenDirectory);
    return hierarchy.Root.GetExistingSubdirs()
                    .Where(subdir => !subdir.Name.SequenceEqual(TMP_NAME));
  }

  public static IEnumerable<IFileHierarchyDirectory>
      GetGoldenInputDirectories(ISystemDirectory rootGoldenDirectory)
    => GetGoldenDirectories(rootGoldenDirectory)
        .Select(subdir => subdir.AssertGetExistingSubdir("input"));

  public static void RunInTestDirectory(
      IFileHierarchyDirectory goldenSubdir,
      Action<ISystemDirectory> handler) {
    var tmpDirectory = goldenSubdir.Impl.GetOrCreateSubdir(TMP_NAME);
    tmpDirectory.DeleteContents();

    try {
      handler(tmpDirectory);
    } finally {
      tmpDirectory.DeleteContents();
      tmpDirectory.Delete();
    }
  }

  public static void AssertFilesInDirectoriesAreIdentical(
      IReadOnlyTreeDirectory lhs,
      IReadOnlyTreeDirectory rhs) {
    var lhsFiles = lhs.GetExistingFiles()
                      .ToDictionary(file => file.Name.ToString());
    var rhsFiles = rhs.GetExistingFiles()
                      .ToDictionary(file => file.Name.ToString());

    Assert.IsTrue(lhsFiles.Keys.ToHashSet()
                          .SetEquals(rhsFiles.Keys.ToHashSet()));

    foreach (var (name, lhsFile) in lhsFiles) {
      var rhsFile = rhsFiles[name];
      try {
        AssertFilesAreIdentical_(lhsFile, rhsFile);
      } catch (Exception ex) {
        throw new Exception($"Found a change in file {name}: ", ex);
      }
    }
  }

  private static void AssertFilesAreIdentical_(
      IReadOnlyTreeFile lhs,
      IReadOnlyTreeFile rhs) {
    using var lhsStream = lhs.OpenRead();
    using var rhsStream = rhs.OpenRead();

    Assert.AreEqual(lhsStream.Length, rhsStream.Length);

    var bytesToRead = sizeof(long);
    int iterations =
        (int) Math.Ceiling((double) lhsStream.Length / bytesToRead);

    long lhsLong = 0;
    long rhsLong = 0;

    var lhsSpan = new Span<long>(ref lhsLong).AsBytes();
    var rhsSpan = new Span<long>(ref rhsLong).AsBytes();

    for (int i = 0; i < iterations; i++) {
      lhsStream.Read(lhsSpan);
      rhsStream.Read(rhsSpan);

      if (lhsLong != rhsLong) {
        Asserts.Fail(
            $"Files with name \"{lhs.Name}\" are different around byte #: {i * bytesToRead}");
      }
    }
  }
}