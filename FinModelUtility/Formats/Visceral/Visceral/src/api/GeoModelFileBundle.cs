using fin.io;
using fin.model.io;
using fin.util.enumerables;

namespace visceral.api {
  public class GeoModelFileBundle : IModelFileBundle {
    // TODO: Is there a better thing to rely on?
    public required string GameName { get; init; }

    public IReadOnlyTreeFile? MainFile
      => this.RcbFile ?? this.GeoFiles.First();

    public IEnumerable<IReadOnlyGenericFile> Files
      => this.GeoFiles
             .ConcatIfNonnull(this.RcbFile)
             .ConcatIfNonnull(
                 this.Tg4ImageFileBundles
                     ?.SelectMany(tg4Bundle => new IReadOnlyGenericFile[] {
                         tg4Bundle.Tg4hFile, tg4Bundle.Tg4dFile
                     }));

    public required IReadOnlyList<IReadOnlyTreeFile> GeoFiles { get; init; }
    public required IReadOnlyList<IReadOnlyTreeFile> BnkFiles { get; init; }
    public required IReadOnlyTreeFile? RcbFile { get; init; }

    public required MtlbFileIdsDictionary MtlbFileIdsDictionary { get; init; }
    
    public IReadOnlyList<Tg4ImageFileBundle>? Tg4ImageFileBundles { get; init; }
  }
}