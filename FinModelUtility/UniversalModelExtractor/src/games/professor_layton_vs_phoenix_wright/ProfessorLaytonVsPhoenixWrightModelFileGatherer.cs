using fin.io;
using fin.io.bundles;

using level5.api;
using level5.schema;

using uni.platforms;
using uni.platforms.threeDs;
using uni.platforms.threeDs.tools;
using uni.util.io;


namespace uni.games.professor_layton_vs_phoenix_wright {
  public class ProfessorLaytonVsPhoenixWrightModelFileGatherer
      : IFileBundleGatherer<XcModelFileBundle> {
    public IEnumerable<XcModelFileBundle> GatherFileBundles(bool assert) {
      if (!DirectoryConstants.ROMS_DIRECTORY.PossiblyAssertExistingFile(
              "professor_layton_vs_phoenix_wright.cia",
              assert,
              out var professorLaytonVsPhoenixWrightRom)) {
        return Enumerable.Empty<XcModelFileBundle>();
      }

      var fileHierarchy =
          new ThreeDsFileHierarchyExtractor().ExtractFromRom(
              professorLaytonVsPhoenixWrightRom);

      if (new ThreeDsXfsaTool().Extract(
              fileHierarchy.Root.Files.Single(file => file.Name == "vs1.fa"))) {
        fileHierarchy.Root.Refresh(true);
      }

      return new FileHierarchyAssetBundleSeparator<XcModelFileBundle>(
          fileHierarchy,
          directory => {
            var xcFiles = directory.FilesWithExtension(".xc");

            var xcBundles = Array.Empty<IXcFiles>();
            if (directory.LocalPath == "\\vs1\\chr") {
              xcBundles = new[] {
                  GetSameFile("Emeer Punchenbaug", directory, "c206"),
                  GetSameFile("Espella Cantabella", directory, "c105"),
                  GetSameFile("Flynch", directory, "c203"),
                  GetSameFile("Johnny Smiles", directory, "c201"),
                  GetSameFile("Judge", directory, "c107"),
                  GetSameFile("Kira", directory, "c215"),
                  GetSameFile("Kira (with flower petals)",
                              directory,
                              "c216"),
                  GetSameFile("Knightle", directory, "c213"),
                  GetSameFile("Maya Fey", directory, "c104"),
                  GetSameFile("Miles Edgeworth", directory, "c401"),
                  GetSameFile("Olivia Aldente", directory, "c202"),
                  GetSameFile("Phoenix Wright", directory, "c102"),
                  GetSameFile("Phoenix Wright (Baker)", directory, "c113"),
                  GetSameFile("Professor Layton", directory, "c101"),
                  GetSameFile("Professor Layton (Gold)", directory, "c301"),
                  GetSameFile("Storyteller", directory, "c134"),
                  GetSameFile("Wordsmith", directory, "c211"),
                  GetSameFile("Zacharias Barnham", directory, "c106_a"),
              };
            }

            var bundles = new List<XcModelFileBundle>();
            foreach (var xcBundle in xcBundles) {
              bundles.Add(new XcModelFileBundle {
                  GameName = "professor_layton_vs_phoenix_wright",
                  BetterName = xcBundle.Name,
                  ModelXcFile = xcBundle.ModelFile,
                  AnimationXcFiles = xcBundle.AnimationFiles,
              });
            }

            foreach (var xcFile in xcFiles) {
              if (xcBundles.Any(xcBundle =>
                                    xcBundle.ModelFile == xcFile)) {
                continue;
              }

              IFileHierarchyFile[] animationFiles;
              var name = xcFile.NameWithoutExtension;
              var underscoreIndex = name.IndexOf('_');
              if (underscoreIndex != -1) {
                animationFiles = new[] { xcFile };
              } else {
                animationFiles = xcFiles
                                 .Where(fileWithAnimations
                                            => fileWithAnimations.Name
                                                .StartsWith(
                                                    name))
                                 .ToArray();
              }

              bundles.Add(new XcModelFileBundle {
                  GameName = "professor_layton_vs_phoenix_wright",
                  ModelXcFile = xcFile,
                  AnimationXcFiles = new[] { xcFile },
              });
            }

            return bundles
                   .OrderBy(bundle => bundle.BetterName ??
                                      bundle.MainFile.Name)
                   .ToArray();
          }
      ).GatherFileBundles(assert);
    }

    internal IXcFiles GetModelOnly(string name,
                                   IFileHierarchyDirectory directory,
                                   string modelFileName)
      => new ModelOnly(name,
                       directory.Files.Single(
                           file => file.Name == modelFileName));

    internal IXcFiles GetSameFile(string name,
                                  IFileHierarchyDirectory directory,
                                  string modelFileName) {
      var modelFile =
          directory.Files.Single(file => file.NameWithoutExtension ==
                                         modelFileName);
      var animationFiles =
          directory.Files.Where(
              file => file.NameWithoutExtension != modelFileName &&
                      file.NameWithoutExtension.StartsWith(modelFileName));
      return new ModelAndAnimations(
          name,
          modelFile,
          new[] { modelFile, }.Concat(animationFiles).ToArray());
    }

    internal IXcFiles GetModelAndAnimations(string name,
                                            IFileHierarchyDirectory directory,
                                            string modelFileName,
                                            params string[] animationFileNames)
      => new ModelAndAnimations(
          name,
          directory.Files.Single(file => file.Name == modelFileName),
          animationFileNames.Select(animationFileName
                                        => directory.Files.Single(
                                            file => file.Name ==
                                                    animationFileName))
                            .ToArray());

    internal interface IXcFiles {
      string Name { get; }
      IFileHierarchyFile ModelFile { get; }
      IFileHierarchyFile[]? AnimationFiles { get; }
    }


    internal record ModelOnly(
        string Name,
        IFileHierarchyFile ModelFile) : IXcFiles {
      public IFileHierarchyFile[]? AnimationFiles => null;
    }

    internal record SameFile(
        string Name,
        IFileHierarchyFile ModelFile) : IXcFiles {
      public IFileHierarchyFile[] AnimationFiles { get; } = { ModelFile };
    }

    internal record ModelAndAnimations(
        string Name,
        IFileHierarchyFile ModelFile,
        params IFileHierarchyFile[] AnimationFiles) : IXcFiles;
  }
}