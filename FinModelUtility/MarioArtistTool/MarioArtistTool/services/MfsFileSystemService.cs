using System;

using marioartist.api;

namespace marioartisttool.services;

public static class MfsFileSystemService {
  public static event Action<MfsTreeDirectory?>? OnFileSystemLoaded;

  public static void LoadFileSystem(MfsTreeDirectory? root)
    => OnFileSystemLoaded?.Invoke(root);


  public static event Action<MfsTreeFile?>? OnFileSelected;

  public static void SelectFile(MfsTreeFile? file)
    => OnFileSelected?.Invoke(file);
}