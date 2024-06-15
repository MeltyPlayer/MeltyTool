using fin.io.bundles;

namespace uni {
  public static class FileBundleService {
    public static event Action<IFileBundle> OnFileBundleOpened;

    public static void OpenFileBundle(IFileBundle fileBundle)
      => OnFileBundleOpened?.Invoke(fileBundle);
  }
}