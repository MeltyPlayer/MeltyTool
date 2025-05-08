﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using CommunityToolkit.HighPerformance;

using fin.image;
using fin.io;
using fin.util.asserts;
using fin.util.strings;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SixLabors.ImageSharp.PixelFormats;


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

  public static void AssertGoldenFiles(
      IFileHierarchyDirectory goldenSubdir,
      Action<IFileHierarchyDirectory, ISystemDirectory> handler) {
    var inputDirectory = goldenSubdir.AssertGetExistingSubdir("input");
    var outputDirectory = goldenSubdir.AssertGetExistingSubdir("output");
    var hasGoldenExport = !outputDirectory.IsEmpty;

    GoldenAssert.RunInTestDirectory(
        goldenSubdir,
        tmpDirectory => {
          var targetDirectory =
              hasGoldenExport ? tmpDirectory : outputDirectory.Impl;

          handler(inputDirectory, targetDirectory);

          if (hasGoldenExport) {
            GoldenAssert.AssertFilesInDirectoriesAreIdentical(
                tmpDirectory,
                outputDirectory.Impl);
          }
        });
  }

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
        try {
          AssertFilesAreIdentical_(lhsFile, rhsFile);
        } catch {
          if (lhsFile.FileType.ToLower() is ".bmp"
                                            or ".jpg"
                                            or ".jpeg"
                                            or ".gif"
                                            or ".png") {
            AssertImageFilesAreIdentical_(lhsFile, rhsFile);
          } else {
            throw;
          }
        }
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

  private static void AssertImageFilesAreIdentical_(
      IReadOnlyTreeFile lhs,
      IReadOnlyTreeFile rhs,
      float allowableError = 2) {
    using var lhsImage = FinImage.FromFile(lhs);
    using var rhsImage = FinImage.FromFile(rhs);

    Assert.AreEqual(lhsImage.Width, rhsImage.Width);
    Assert.AreEqual(lhsImage.Height, rhsImage.Height);

    lhsImage.Access(lhsGet => {
      rhsImage.Access(rhsGet => {
        for (var y = 0; y < lhsImage.Height; ++y) {
          for (var x = 0; x < lhsImage.Width; ++x) {
            lhsGet(x,
                   y,
                   out var lhsR,
                   out var lhsG,
                   out var lhsB,
                   out var lhsA);

            rhsGet(x,
                   y,
                   out var rhsR,
                   out var rhsG,
                   out var rhsB,
                   out var rhsA);

            var lPixel = new Rgba32(lhsR, lhsG, lhsB, lhsA);
            var rPixel = new Rgba32(rhsR, rhsG, rhsB, rhsA);

            if (Math.Abs(lhsR - rhsR) > allowableError ||
                Math.Abs(lhsG - rhsG) > allowableError ||
                Math.Abs(lhsB - rhsB) > allowableError ||
                Math.Abs(lhsA - rhsA) > allowableError) {
              Asserts.Fail(
                  $"Files with name \"{lhs.Name}\" are different at pixel ({x},{y}): {lPixel} / {rPixel}");
            }
          }
        }
      });
    });
  }
}