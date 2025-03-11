using System.Collections.Generic;
using System.IO;
using System.Linq;

using fin.io;
using fin.model;
using fin.model.impl;
using fin.model.io.importers;

using grezzo.schema.zsi;

using schema.binary;


namespace grezzo.api;

// TODO: Have a way to convert the scene to a model instead?
public class ZsiModelImporter : IModelImporter<ZsiModelFileBundle> {
  private readonly CmbModelBuilder cmbModelBuilder_ = new();

  public IModel Import(ZsiModelFileBundle fileBundle) {
    var zsiFile = fileBundle.ZsiFile;

    var zsi = zsiFile.ReadNew<Zsi>(Endianness.LittleEndian);

    var fileSet = new HashSet<IReadOnlyGenericFile>();
    var finModel = new ModelImpl {
        FileBundle = fileBundle,
        Files = fileBundle.Files.ToHashSet(),
    };

    this.AddZsiMesh_(fileBundle, finModel, fileSet, zsi);

    foreach (var roomFileName in zsi.RoomFileNames) {
      var roomZsiFile = zsiFile.AssertGetParent()
                               .AssertGetExistingFile(
                                   Path.GetFileName(roomFileName));
      var roomZsi = roomZsiFile.ReadNew<Zsi>(Endianness.LittleEndian);
      this.AddZsiMesh_(fileBundle, finModel, fileSet, roomZsi);
    }

    return finModel;
  }

  private void AddZsiMesh_(ZsiModelFileBundle fileBundle,
                           ModelImpl finModel,
                           ISet<IReadOnlyGenericFile> fileSet,
                           Zsi zsi) {
    foreach (var meshHeader in zsi.MeshHeaders) {
      foreach (var meshEntry in meshHeader.MeshEntries) {
        var opaqueMesh = meshEntry.OpaqueMesh;
        if (opaqueMesh != null) {
          this.cmbModelBuilder_.AddToModel(finModel,
                                           fileBundle,
                                           fileSet,
                                           opaqueMesh);
        }

        var translucentMesh = meshEntry.TranslucentMesh;
        if (translucentMesh != null) {
          this.cmbModelBuilder_.AddToModel(finModel,
                                           fileBundle,
                                           fileSet,
                                           translucentMesh);
        }
      }
    }
  }
}