using fin.io;
using fin.model;
using fin.model.impl;
using fin.model.io;
using fin.model.io.importers;
using fin.util.enumerables;

using mp64.schema;
using mp64.schema.form;

using schema.binary;

namespace mp64.api;

public sealed record FormModelFileBundle(
    IReadOnlyTreeFile MainFile,
    IReadOnlyTreeFile[] MtnxFiles)
    : IModelFileBundle<FormModelFileBundle, FormModelImporter> {
  public IEnumerable<IReadOnlyStandaloneFile> Files
    => this.MainFile.Yield().Concat(this.MtnxFiles);
}

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/PartyPlanner64/PartyPlanner64/blob/980d043217823d0e5901ee93d3a50e36f728db10/packages/lib/models/FormToThreeJs.ts
/// </summary>
public sealed class FormModelImporter
    : IModelImporter<FormModelImporter, FormModelFileBundle> {
  public IModel Import(FormModelFileBundle fileBundle) {
    var form = fileBundle.MainFile.ReadNew<Form>(Endianness.BigEndian);
    var mtnxes = fileBundle.MtnxFiles
                           .Select(f => f.ReadNew<Mtnx>(Endianness.BigEndian))
                           .ToArray();

    var model = new ModelImpl {
        FileBundle = fileBundle,
        Files = fileBundle.Files.ToHashSet(),
    };

    return model;
  }
}