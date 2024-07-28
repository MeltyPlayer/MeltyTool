using BenchmarkDotNet.Attributes;

namespace benchmarks {
  public class FetchDirectories {
    [Benchmark]
    public void FetchDirectoriesSeparately() => this.FetchDirectoriesSeparatelyImpl().ToArray();

    [Benchmark]
    public void FetchDirectoriesAllAtOnce() => this.FetchDirectoriesAllAtOnceImpl();

    [Benchmark]
    public void FetchEntriesAllAtOnce() => this.FetchEntriesAllAtOnceImpl();

    public const string DIRECTORY =
        @"R:\Documents\CSharpWorkspace\Pikmin2Utility\cli\roms";

    public const string SEARCH_PATTERN = "*";

    public IEnumerable<string> FetchDirectoriesSeparatelyImpl() {
      var rootFolderPath = DIRECTORY;
      var pending = new Queue<string>();
      pending.Enqueue(rootFolderPath);
      string[] tmp;
      while (pending.Count > 0) {
        rootFolderPath = pending.Dequeue();
        try {
          tmp = Directory.GetFiles(rootFolderPath, SEARCH_PATTERN);
        } catch (UnauthorizedAccessException) {
          continue;
        }
        for (int i = 0; i < tmp.Length; i++) {
          yield return tmp[i];
        }
        tmp = Directory.GetDirectories(rootFolderPath);
        for (int i = 0; i < tmp.Length; i++) {
          pending.Enqueue(tmp[i]);
        }
      }
    }

    public string[] FetchDirectoriesAllAtOnceImpl() {
      return Directory.GetFiles(DIRECTORY, SEARCH_PATTERN,
                                new EnumerationOptions {
                                    RecurseSubdirectories =
                                        true
                                });
    }

    public string[] FetchEntriesAllAtOnceImpl() {
      return Directory.GetFileSystemEntries(DIRECTORY, SEARCH_PATTERN,
                                            new EnumerationOptions {
                                                RecurseSubdirectories =
                                                    true
                                            });
    }
  }
}