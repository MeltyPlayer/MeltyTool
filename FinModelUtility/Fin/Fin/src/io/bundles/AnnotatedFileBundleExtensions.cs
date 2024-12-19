namespace fin.io.bundles;

public static class AnnotatedFileBundleExtensions {
  public static bool IsOfType<TSpecificFile>(
      this IAnnotatedFileBundle file,
      out IAnnotatedFileBundle<TSpecificFile> outFile)
      where TSpecificFile : IFileBundle {
    if (file is IAnnotatedFileBundle<TSpecificFile>) {
      outFile = (IAnnotatedFileBundle<TSpecificFile>) file;
      return true;
    }

    outFile = default;
    return false;
  }
}