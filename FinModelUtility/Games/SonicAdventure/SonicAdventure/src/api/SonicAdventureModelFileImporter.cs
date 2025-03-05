using fin.model;
using fin.model.io.importers;

using schema.binary;
using schema.util.streams;

namespace sonicadventure.api;

public class KeyedStream(Stream stream, long key) : ISeekableReadableStream {
  private readonly ReadableStream impl_ = new(stream);

  public void Dispose() => this.impl_.Dispose();
  public byte ReadByte() => this.impl_.ReadByte();
  public void ReadIntoBuffer(Span<byte> dst) => this.impl_.ReadIntoBuffer(dst);

  public int TryToReadIntoBuffer(Span<byte> dst)
    => this.impl_.TryToReadIntoBuffer(dst);

  public long Position => this.impl_.Position + key;

  long ISeekableStream.Position {
    get => this.impl_.Position + key;
    set => this.impl_.Position = value - key;
  }

  public long Length => key + this.impl_.Length;
}

public class SonicAdventureModelFileImporter
    : IModelImporter<SonicAdventureModelFileBundle> {
  public IModel Import(SonicAdventureModelFileBundle fileBundle) {
    using var fs = fileBundle.ModelFile.OpenRead();
    fs.Position = fileBundle.ModelFileOffset;

    var br = new SchemaBinaryReader(new KeyedStream(fs, fileBundle.ModelFileKey), Endianness.LittleEndian);
    var obj = br.ReadNew<schema.model.Object>();

    ;

    return null!;
  }
}