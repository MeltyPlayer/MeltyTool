using fin.io;

namespace fin.model.io.exporters;

public interface IModelExporterParams {
  ISystemFile OutputFile { get; }
  IReadOnlyModel Model { get; }
  float Scale { get; }
}

public class ModelExporterParams : IModelExporterParams {
  public required ISystemFile OutputFile { get; set; }
  public required IReadOnlyModel Model { get; set; }
  public float Scale { get; set; } = 1;
}

public interface IModelExporter {
  void ExportModel(IModelExporterParams modelExporterParams);
}