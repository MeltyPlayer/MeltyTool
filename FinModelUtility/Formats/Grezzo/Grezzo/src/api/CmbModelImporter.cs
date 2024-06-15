using System.Collections.Generic;
using System.Linq;

using CommunityToolkit.HighPerformance.Helpers;

using fin.io;
using fin.model;
using fin.model.io.importers;

using grezzo.schema.cmb;
using grezzo.schema.csab;
using grezzo.schema.ctxb;
using grezzo.schema.shpa;

using schema.binary;

namespace grezzo.api {
  public class CmbModelImporter : IModelImporter<CmbModelFileBundle> {
    public IModel Import(CmbModelFileBundle modelFileBundle) {
      var cmbFile = modelFileBundle.CmbFile;
      var csabFiles = modelFileBundle.CsabFiles;
      var ctxbFiles = modelFileBundle.CtxbFiles;
      var shpaFiles = modelFileBundle.ShpaFiles;

      var cmb = cmbFile.ReadNew<Cmb>();

      (string, Csab)[]? namesAndCsabs = null;
      if (csabFiles != null) {
        namesAndCsabs = new (string, Csab)[csabFiles.Count];
        ParallelHelper.For(0,
                           csabFiles.Count,
                           new CsabReader(csabFiles, namesAndCsabs));
      }

      var ctxbs = ctxbFiles?.Select(ctxbFile => ctxbFile.ReadNew<Ctxb>())
                           .ToList();

      var namesAndShpas =
          shpaFiles?
              .Select(shpaFile => {
                        var shpa
                            = shpaFile.ReadNew<Shpa>(Endianness.LittleEndian);
                        return (shpaFile.NameWithoutExtension, shpa);
                      })
              .ToList();

      return new CmbModelBuilder().BuildModel(
          modelFileBundle,
          cmb,
          ctxbs,
          namesAndCsabs,
          namesAndShpas);
    }

    public readonly struct CsabReader : IAction {
      private readonly IReadOnlyList<IReadOnlyTreeFile> src_;
      private readonly (string, Csab)[] dst_;

      public CsabReader(
          IReadOnlyList<IReadOnlyTreeFile> src,
          (string, Csab)[] dst) {
        this.src_ = src;
        this.dst_ = dst;
      }

      public void Invoke(int i) {
        var csabFile = this.src_[i];
        var csab =
            csabFile.ReadNew<Csab>(Endianness.LittleEndian);
        this.dst_[i] = (csabFile.NameWithoutExtension, csab);
      }
    }
  }
}