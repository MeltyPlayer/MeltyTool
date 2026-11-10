using fin.io;
using fin.io.bundles;
using fin.util.progress;

using level5.api;

using uni.platforms.threeDs.tools;
using uni.util.io;


namespace uni.games.professor_layton_vs_phoenix_wright;

public sealed class ProfessorLaytonVsPhoenixWrightFileBundleGatherer
    : B3dsFileBundleGatherer {
  public override string Name => "professor_layton_vs_phoenix_wright";
  public override string Title => "Professor Layton vs. Phoenix Wright: Ace Attorney";

  protected override void GatherFileBundlesFromHierarchy(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress,
      IFileHierarchy fileHierarchy) {
    if (new ThreeDsXfsaTool().Extract(fileHierarchy.Root.GetExistingFiles()
                                                   .SingleByName("vs1.fa"))) {
      fileHierarchy.Root.Refresh(true);
    }

    var didUpdateAny = false;
    var extractor = new XcArchiveExtractor();
    foreach (var xcFile in
             fileHierarchy.Root.GetFilesWithFileType(".xc", true)) {
      try {
        if (extractor.TryToExtractIntoDirectory(
                xcFile,
                new FinDirectory(xcFile.AssertGetParent().FullPath))) {
          didUpdateAny = true;
        }
      } catch (Exception e) { }
    }

    if (didUpdateAny) {
      fileHierarchy.RefreshRootAndUpdateCache();
    }

    new FileHierarchyAssetBundleSeparator(
        fileHierarchy,
        (directory, organizer) => {
          var xcDirectories
              = directory
                .FilesWithExtension(".xc")
                .Select(f => directory.TryToGetExistingSubdir(
                            f.NameWithoutExtension,
                            out var d)
                            ? d
                            : null);

          var xcBundles = Array.Empty<IXcDirectories>();
          if (directory.LocalPath == "\\vs1\\chr") {
            xcBundles = [
                this.GetSameFile("Emeer Punchenbaug", directory, "c206"),
                this.GetSameFile("Espella Cantabella", directory, "c105"),
                this.GetSameFile("Flynch", directory, "c203"),
                this.GetSameFile("Johnny Smiles", directory, "c201"),
                this.GetSameFile("Judge", directory, "c107"),
                this.GetSameFile("Kira", directory, "c215"), this.GetSameFile(
                    "Kira (with flower petals)",
                    directory,
                    "c216"),
                this.GetSameFile("Knightle", directory, "c213"),
                this.GetSameFile("Maya Fey", directory, "c104"),
                this.GetSameFile("Miles Edgeworth", directory, "c401"),
                this.GetSameFile("Olivia Aldente", directory, "c202"),
                this.GetSameFile("Phoenix Wright", directory, "c102"),
                this.GetSameFile("Phoenix Wright (Baker)", directory, "c113"),
                this.GetSameFile("Professor Layton", directory, "c101"),
                this.GetSameFile("Professor Layton (Gold)", directory, "c301"),
                this.GetSameFile("Storyteller", directory, "c134"),
                this.GetSameFile("Wordsmith", directory, "c211"),
                this.GetSameFile("Zacharias Barnham", directory, "c106_a")
            ];
          }

          foreach (var xcBundle in xcBundles) {
            organizer.Add(new XcModelFileBundle {
                HumanReadableName = xcBundle.Name,
                ModelDirectory = xcBundle.ModelDirectory,
                AnimationDirectories = xcBundle.AnimationDirectories,
            });
          }

          foreach (var xcDirectory in xcDirectories) {
            if (xcBundles.Any(xcBundle =>
                                  xcBundle.ModelDirectory == xcDirectory)) {
              continue;
            }

            IFileHierarchyDirectory[] animationDirectories;
            var name = xcDirectory.Name.ToString();
            var underscoreIndex = name.IndexOf('_');
            if (underscoreIndex != -1) {
              animationDirectories = [xcDirectory];
            } else {
              animationDirectories = xcDirectories
                                     .Where(fileWithAnimations
                                                => fileWithAnimations.Name
                                                    .StartsWith(
                                                        name))
                                     .ToArray();
            }

            organizer.Add(new XcModelFileBundle {
                ModelDirectory = xcDirectory,
                AnimationDirectories = animationDirectories,
            });
          }
        }
    ).GatherFileBundles(organizer, mutablePercentageProgress);
  }

  internal IXcDirectories GetModelOnly(string name,
                                       IFileHierarchyDirectory directory,
                                       string modelFileName)
    => new ModelOnly(
        name,
        directory.GetExistingSubdirs().SingleByName(modelFileName).Impl);

  internal IXcDirectories GetSameFile(string name,
                                      IFileHierarchyDirectory directory,
                                      string modelFileName) {
    var modelFile = directory.AssertGetExistingSubdir(modelFileName).Impl;
    var animationFiles =
        directory.GetExistingSubdirs()
                 .Where(file => file.Name != modelFileName &&
                                file.Name.StartsWith(modelFileName))
                 .Select(d => d.Impl);
    return new ModelAndAnimations(
        name,
        modelFile,
        new[] { modelFile }.Concat(animationFiles).ToArray());
  }

  internal IXcDirectories GetModelAndAnimations(string name,
                                                IFileHierarchyDirectory
                                                    directory,
                                                string modelFileName,
                                                params string[]
                                                    animationFileNames)
    => new ModelAndAnimations(
        name,
        directory.AssertGetExistingSubdir(modelFileName).Impl,
        animationFileNames.Select(f => directory.AssertGetExistingSubdir(f))
                          .Select(d => d.Impl)
                          .ToArray());

  internal interface IXcDirectories {
    string Name { get; }
    IReadOnlyTreeDirectory ModelDirectory { get; }
    IReadOnlyTreeDirectory[]? AnimationDirectories { get; }
  }


  internal record ModelOnly(
      string Name,
      IReadOnlyTreeDirectory ModelDirectory) : IXcDirectories {
    public IReadOnlyTreeDirectory[]? AnimationDirectories => null;
  }

  internal record SameDirectories(
      string Name,
      IReadOnlyTreeDirectory ModelDirectory) : IXcDirectories {
    public IReadOnlyTreeDirectory[] AnimationDirectories { get; } =
      [ModelDirectory];
  }

  internal record ModelAndAnimations(
      string Name,
      IReadOnlyTreeDirectory ModelDirectory,
      params IReadOnlyTreeDirectory[] AnimationDirectories) : IXcDirectories;
}