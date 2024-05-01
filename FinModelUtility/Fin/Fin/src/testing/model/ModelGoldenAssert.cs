using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using CommunityToolkit.HighPerformance;

using fin.model.io.exporters;
using fin.model.io.exporters.assimp.indirect;
using fin.io;
using fin.model.io;
using fin.model.io.importers;
using fin.util.asserts;
using fin.util.strings;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace fin.testing.model {
  public static class ModelGoldenAssert {
    private const string TMP_NAME = "tmp";

    public static ISystemDirectory GetRootGoldensDirectory(
        Assembly executingAssembly) {
      var assemblyName =
          executingAssembly.ManifestModule.Name.SubstringUpTo(".dll");

      var executingAssemblyDll = new FinFile(executingAssembly.Location);
      var executingAssemblyDir = executingAssemblyDll.AssertGetParent();

      var currentDir = executingAssemblyDir;
      while (currentDir.Name != assemblyName) {
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
                      .Where(
                          subdir => subdir.Name != TMP_NAME);
    }

    public static IEnumerable<IFileHierarchyDirectory>
        GetGoldenInputDirectories(ISystemDirectory rootGoldenDirectory)
      => GetGoldenDirectories(rootGoldenDirectory)
          .Select(subdir => subdir.AssertGetExistingSubdir("input"));

    public static IEnumerable<TModelBundle> GetGoldenModelBundles<TModelBundle>(
        ISystemDirectory rootGoldenDirectory,
        Func<IFileHierarchyDirectory, TModelBundle>
            gatherModelBundleFromInputDirectory)
        where TModelBundle : IModelFileBundle {
      foreach (var goldenSubdir in GetGoldenDirectories(rootGoldenDirectory)) {
        var inputDirectory = goldenSubdir.AssertGetExistingSubdir("input");
        var modelBundle = gatherModelBundleFromInputDirectory(inputDirectory);

        yield return modelBundle;
      }
    }

    /// <summary>
    ///   Asserts model goldens. Assumes that directories will be stored as the following:
    ///
    ///   - {goldenDirectory}
    ///     - {goldenName1}
    ///       - input
    ///         - {raw golden files}
    ///       - output
    ///         - {exported files}
    ///     - {goldenName2}
    ///       ... 
    /// </summary>
    public static void AssertExportGoldens<TModelBundle>(
        ISystemDirectory rootGoldenDirectory,
        IModelImporter<TModelBundle> modelImporter,
        Func<IFileHierarchyDirectory, TModelBundle>
            gatherModelBundleFromInputDirectory)
        where TModelBundle : IModelFileBundle {
      foreach (var goldenSubdir in
               GetGoldenDirectories(rootGoldenDirectory)) {
        ModelGoldenAssert.AssertGolden(goldenSubdir,
                                       modelImporter,
                                       gatherModelBundleFromInputDirectory);
      }
    }

    private static string[] EXTENSIONS = [".glb"];

    public static void AssertGolden<TModelBundle>(
        IFileHierarchyDirectory goldenSubdir,
        IModelImporter<TModelBundle> modelImporter,
        Func<IFileHierarchyDirectory, TModelBundle>
            gatherModelBundleFromInputDirectory)
        where TModelBundle : IModelFileBundle {
      var tmpDirectory = goldenSubdir.Impl.GetOrCreateSubdir(TMP_NAME);
      tmpDirectory.DeleteContents();

      var inputDirectory = goldenSubdir.AssertGetExistingSubdir("input");
      var modelBundle = gatherModelBundleFromInputDirectory(inputDirectory);

      var outputDirectory = goldenSubdir.AssertGetExistingSubdir("output");
      var hasGoldenExport =
          outputDirectory.GetExistingFiles()
                         .Any(file => EXTENSIONS.Contains(file.FileType));

      var targetDirectory =
          hasGoldenExport ? tmpDirectory : outputDirectory.Impl;

      var model = modelImporter.Import(modelBundle);
      new AssimpIndirectModelExporter() {
          LowLevel = modelBundle.UseLowLevelExporter,
          ForceGarbageCollection = modelBundle.ForceGarbageCollection,
      }.ExportExtensions(
          new ModelExporterParams {
              Model = model,
              OutputFile =
                  new FinFile(Path.Combine(targetDirectory.FullPath,
                                           $"{modelBundle.MainFile.NameWithoutExtension}.foo")),
          },
          EXTENSIONS,
          true);

      if (hasGoldenExport) {
        AssertFilesInDirectoriesAreIdentical_(
            tmpDirectory,
            outputDirectory.Impl);
      }

      tmpDirectory.DeleteContents();
      tmpDirectory.Delete();
    }

    private static void AssertFilesInDirectoriesAreIdentical_(
        ISystemDirectory lhs,
        ISystemDirectory rhs) {
      var lhsFiles = lhs.GetExistingFiles()
                        .ToDictionary(file => (string) file.Name);
      var rhsFiles = rhs.GetExistingFiles()
                        .ToDictionary(file => (string) file.Name);

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
}