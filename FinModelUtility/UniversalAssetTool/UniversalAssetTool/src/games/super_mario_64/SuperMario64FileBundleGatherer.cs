﻿using fin.common;
using fin.io;
using fin.io.bundles;
using fin.util.progress;

using sm64.api;

namespace uni.games.super_mario_64;

public class SuperMario64FileBundleGatherer : INamedAnnotatedFileBundleGatherer {
  public string Name => "super_mario_64";

  public void GatherFileBundles(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress) {
    if (!DirectoryConstants.ROMS_DIRECTORY.TryToGetExistingFile(
            "super_mario_64.z64",
            out var superMario64Rom)) {
      return;
    }

    ExtractorUtil.GetOrCreateRomDirectoriesWithPrereqs("super_mario_64",
      out var prereqsDir,
      out var extractedDir);
    var fileHierarchy
        = ExtractorUtil.GetFileHierarchy("super_mario_64", extractedDir);
    var root = fileHierarchy.Root;

    var rootSysDir = root.Impl;
    var levelsDir = rootSysDir.GetOrCreateSubdir("levels");

    var levelIds = Enum.GetValues<LevelId>().ToList();
    levelIds.Sort((lhs, rhs) => lhs.ToString().CompareTo(rhs.ToString()));
    var levelIdsAndPaths = levelIds.Select(levelId => {
      var path = Path.Join(levelsDir.Name, levelId.ToString());
      return (levelId, path);
    });

    var didWriteAny = false;
    foreach (var (_, path) in levelIdsAndPaths) {
      var zObjectFile = new FinFile(Path.Join(rootSysDir.FullPath, path));

      // TODO: Write the actual data here
      if (!zObjectFile.Exists) {
        FinFileSystem.File.Create(zObjectFile.FullPath);
        didWriteAny = true;
      }
    }

    if (didWriteAny) {
      fileHierarchy.RefreshRootAndUpdateCache();
    }

    foreach (var (levelId, path) in levelIdsAndPaths) {
      var levelFile = root.AssertGetExistingFile(path);
      organizer.Add(new Sm64LevelSceneFileBundle(
                        root,
                        superMario64Rom,
                        levelId).Annotate(levelFile));
    }

    var marioAnimationsFile
        = prereqsDir.AssertGetExistingFile("mario_animations.csv");
    // TODO: 
  }
}