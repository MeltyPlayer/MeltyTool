using fin.io;
using fin.io.bundles;
using fin.util.progress;

using level5.api;

using uni.platforms.threeDs;
using uni.platforms.threeDs.tools;
using uni.util.io;

namespace uni.games.professor_layton_vs_phoenix_wright;

public class ProfessorLaytonVsPhoenixWrightFileBundleGatherer
    : IAnnotatedFileBundleGatherer {
  public void GatherFileBundles(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress) {
    if (!new ThreeDsFileHierarchyExtractor().TryToExtractFromGame(
            "professor_layton_vs_phoenix_wright",
            out var fileHierarchy)) {
      return;
    }

    if (new ThreeDsXfsaTool().Extract(fileHierarchy.Root.GetExistingFiles()
                                                   .SingleByName("vs1.fa"))) {
      fileHierarchy.Root.Refresh(true);
    }

    new FileHierarchyAssetBundleSeparator(
        fileHierarchy,
        (directory, organizer) => {
          var xcFiles = directory.FilesWithExtension(".xc");

          var xcBundles = Array.Empty<IXcFiles>();
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
                GameName = "professor_layton_vs_phoenix_wright",
                HumanReadableName = xcBundle.Name,
                ModelXcFile = xcBundle.ModelFile,
                AnimationXcFiles = xcBundle.AnimationFiles,
            }.Annotate(xcBundle.ModelFile));
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
              animationFiles = [xcFile];
            } else {
              animationFiles = xcFiles
                               .Where(fileWithAnimations
                                          => fileWithAnimations.Name
                                              .StartsWith(
                                                  name))
                               .ToArray();
            }

            organizer.Add(new XcModelFileBundle {
                GameName = "professor_layton_vs_phoenix_wright",
                ModelXcFile = xcFile,
                AnimationXcFiles = [xcFile],
            }.Annotate(xcFile));
          }
        }
    ).GatherFileBundles(organizer, mutablePercentageProgress);
  }

  internal IXcFiles GetModelOnly(string name,
                                 IFileHierarchyDirectory directory,
                                 string modelFileName)
    => new ModelOnly(name,
                     directory.GetExistingFiles().SingleByName(modelFileName));

  internal IXcFiles GetSameFile(string name,
                                IFileHierarchyDirectory directory,
                                string modelFileName) {
    var modelFile =
        directory.GetExistingFiles()
                 .Single(file => file.NameWithoutExtension ==
                                 modelFileName);
    var animationFiles =
        directory.GetExistingFiles()
                 .Where(
                     file => file.NameWithoutExtension != modelFileName &&
                             file.NameWithoutExtension.StartsWith(
                                 modelFileName));
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
        directory.GetExistingFiles()
                 .Single(file => file.Name == modelFileName),
        animationFileNames.Select(animationFileName => directory
                                      .GetExistingFiles()
                                      .SingleByName(animationFileName))
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
    public IFileHierarchyFile[] AnimationFiles { get; } = [ModelFile];
  }

  internal record ModelAndAnimations(
      string Name,
      IFileHierarchyFile ModelFile,
      params IFileHierarchyFile[] AnimationFiles) : IXcFiles;
}