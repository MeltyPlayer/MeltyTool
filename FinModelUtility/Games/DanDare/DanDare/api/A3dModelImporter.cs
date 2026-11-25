using fin.image;
using fin.io;
using fin.model;
using fin.model.impl;
using fin.model.io;
using fin.model.io.importers;
using fin.model.util;
using fin.util.sets;

using schema.binary;
using schema.binary.attributes;

namespace DanDare.api;

public sealed record A3dModelFileBundle(IReadOnlyTreeFile MainFile)
    : IModelFileBundle<A3dModelFileBundle, A3dModelImporter>;

public sealed partial class A3dModelImporter
    : IModelImporter<A3dModelImporter, A3dModelFileBundle> {
  public IModel Import(A3dModelFileBundle fileBundle) {
    using var br = fileBundle.MainFile.OpenReadAsBinary(Endianness.BigEndian);

    br.AssertString("AFP\0");

    var fileCount = br.ReadInt32();
    var fileHeaders = br.ReadNews<FileHeader>(fileCount);

    var files = new Dictionary<string, byte[]>();
    for (var f = 0; f < fileCount; ++f) {
      var fileHeader = fileHeaders[f];
      var bytes = br.ReadBytes(fileHeader.Size);
      files[fileHeader.Name] = bytes;
    }

    var finModel = new ModelImpl {
        FileBundle = fileBundle,
        Files = fileBundle.MainFile.AsFileSet(),
    };

    var imageFiles
        = files.Where(t => Path.GetExtension(t.Key).ToLower() is ".gif"
                          or ".jpg");
    foreach (var (imageFileName, imageFileBytes) in imageFiles) {
      using var ms = new MemoryStream(imageFileBytes);
      var img = FinImage.FromStream(ms);
      finModel.MaterialManager.CreateTexture(img);
    }

    return finModel;
  }

  [BinarySchema]
  private partial class FileHeader : IBinaryConvertible {
    [NullTerminatedString]
    public string Name { get; set; }

    public uint Size { get; set; }
  }
}