using Avalonia.Headless.NUnit;

using fin.data.dictionaries;
using fin.io;
using fin.model;
using fin.model.impl;
using fin.util.asserts;

using uni.ui.avalonia.helpers;
using uni.ui.avalonia.resources.model;

namespace uni.ui.avalonia.resources;

public class FilesPanelTests {
  [AvaloniaTest]
  public void TestSortsFilesByName() {
    var files = new HashSet<IReadOnlyStandaloneFile>();

    var root = new FinRootDirectory("game", new FinDirectory("root"));

    files.Add(new FinFile("root/foo/bar.bin", root));
    files.Add(new FinFile("root/abc/xyz.bin", root));
    files.Add(new FinFile("root/files 10.bin", root));
    files.Add(new FinFile("root/files 1.bin", root));
    files.Add(new FinFile("root/files 2.bin", root));

    var model = new ModelImpl { FileBundle = default, Files = files };

    var filesPanel = FilesPanel.Bootstrap(new FilesPanelViewModel(model));

    Asserts.SequenceEqual(
        [
            "//game/abc/xyz.bin",
            "//game/files 1.bin",
            "//game/files 2.bin",
            "//game/files 10.bin",
            "//game/foo/bar.bin",
        ],
        filesPanel.GetAllPaths());
  }
}