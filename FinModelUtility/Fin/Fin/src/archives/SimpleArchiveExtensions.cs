using Newtonsoft.Json;

namespace fin.archives;

public static class SimpleArchiveExtensions {
  public static void AddJsonFile<T>(
      this ISimpleArchiveDirectory directory,
      string path,
      T value)
    => directory.AddFile(path, JsonConvert.SerializeObject(value));
}