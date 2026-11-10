using f3dzex2.displaylist;
using f3dzex2.displaylist.opcodes;
using f3dzex2.displaylist.opcodes.f3dzex2;
using f3dzex2.image;
using f3dzex2.io;
using f3dzex2.model;

using fin.io;
using fin.model;
using fin.model.io;
using fin.model.io.importers;
using fin.util.sets;

namespace ac.api;

public sealed class AnimalCrossingModelFileBundle
    : IModelFileBundle<AnimalCrossingModelFileBundle,
        AnimalCrossingModelImporter> {
  public required IReadOnlyTreeFile ModelFile { get; init; }
  public required IReadOnlyTreeFile VertexFile { get; init; }

  public IReadOnlyTreeFile? MainFile => this.ModelFile;
}

public sealed class AnimalCrossingModelImporter
    : IModelImporter<AnimalCrossingModelImporter, AnimalCrossingModelFileBundle> {
  public IModel Import(AnimalCrossingModelFileBundle fileBundle) {
    var n64Hardware = new N64Hardware<SlicedN64Memory>();
    var rdp = n64Hardware.Rdp = new Rdp {
        Tmem = new NoclipTmem(n64Hardware),
    };
    var rsp = n64Hardware.Rsp = new Rsp {
        GeometryMode = GeometryMode.G_LIGHTING
    };

    using var mergedMemoryStream = new MemoryStream();
    {
      using var vertexStream = fileBundle.VertexFile.OpenRead();
      vertexStream.CopyTo(mergedMemoryStream);
    }

    var displayListOffset = mergedMemoryStream.Length;

    {
      using var modelStream = fileBundle.ModelFile.OpenRead();
      modelStream.CopyTo(mergedMemoryStream);
    }

    var n64Memory = n64Hardware.Memory
        = new SlicedN64Memory(mergedMemoryStream.ToArray());
    n64Memory.SetSegment(0, 0, (uint) mergedMemoryStream.Length);

    var displayListReader = new DisplayListReader();
    var f3dzex2OpcodeParser = new DolphinF3dzex2OpcodeParser();

    var displayList = displayListReader.ReadDisplayList(
        n64Hardware.Memory,
        f3dzex2OpcodeParser,
        (uint) displayListOffset);

    var dlModelBuilder = new DlModelBuilder(
        n64Hardware,
        fileBundle,
        new[] {
            fileBundle.ModelFile,
            fileBundle.VertexFile,
        }.AsFileSet());
    dlModelBuilder.AddDl(displayList);

    return dlModelBuilder.Model;
  }
}