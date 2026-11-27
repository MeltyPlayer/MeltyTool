using System.IO.Compression;

using DanDare.schema;

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
    var finModel = new ModelImpl {
        FileBundle = fileBundle,
        Files = fileBundle.MainFile.AsFileSet(),
    };

    using var fs = fileBundle.MainFile.OpenRead();
    var a3dScene = new A3dReader().Load(fs);

    var files = a3dScene.EmbeddedFiles;

    var imageFiles
        = files.Where(t => Path.GetExtension(t.Key).ToLower() is ".gif"
                          or ".jpg");
    foreach (var (imageFileName, imageFileBytes) in imageFiles) {
      using var ms = new MemoryStream(imageFileBytes);
      var img = FinImage.FromStream(ms);
     
      var finTexture = finModel.MaterialManager.CreateTexture(img);
      finTexture.Name = imageFileName;
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