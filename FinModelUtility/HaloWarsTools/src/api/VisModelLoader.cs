using fin.io;
using fin.model;

using HaloWarsTools;


namespace hw.api {
  public class VisModelFileBundle : IHaloWarsModelFileBundle {
    public VisModelFileBundle(IFileHierarchyFile visFile, HWContext context) {
      this.VisFile = visFile;
      this.Context = context;
    }

    public string GameName => "halo_wars";
    public IFileHierarchyFile MainFile => this.VisFile;
    public IFileHierarchyFile VisFile { get; }

    public HWContext Context { get; }
  }

  public class VisModelLoader : IModelLoader<VisModelFileBundle> {
    public IModel LoadModel(VisModelFileBundle modelFileBundle) {
      var visResource =
          HWVisResource.FromFile(modelFileBundle.Context,
                                 modelFileBundle.VisFile.FullName);
      return visResource.Model;
    }
  }
}