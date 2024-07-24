using BenchmarkDotNet.Attributes;

using fin.io.sharpDirLister;

namespace benchmarks {
  [MemoryDiagnoser(false)]
  public class ListingFiles {
    private const int n = 1000;

    private const string path
        = "C:\\Users\\Ryan\\Documents\\CSharpWorkspace\\FinModelUtility\\cli\\roms\\super_mario_sunshine\\extracted";

    private readonly SharpFileLister queueLister_ = new();
    private readonly RecursiveSharpFileLister recursiveLister_ = new();

    [Benchmark]
    public void ViaQueue() {
      var directoryInfo = this.queueLister_.FindNextFilePInvoke(path);
    }

    [Benchmark]
    public void ViaRecursion() {
      var directoryInfo = this.recursiveLister_.FindNextFilePInvoke(path);
    }
  }
}