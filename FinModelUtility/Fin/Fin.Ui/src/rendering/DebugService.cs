using fin.util.types;

using ReactiveUI;

namespace fin.ui.rendering;

public class DebugServiceViewModel : BViewModel {
  public int RenderGraphElementCount {
    get;
    set => this.RaiseAndSetIfChanged(ref field, value);
  }

  public int ModelCount {
    get;
    set => this.RaiseAndSetIfChanged(ref field, value);
  }

  public int MaterialCount {
    get;
    set => this.RaiseAndSetIfChanged(ref field, value);
  }

  public int OpaqueMaterialCount {
    get;
    set => this.RaiseAndSetIfChanged(ref field, value);
  }

  public int TransparentMaterialCount {
    get;
    set => this.RaiseAndSetIfChanged(ref field, value);
  }


  public int ProgramCount {
    get;
    set => this.RaiseAndSetIfChanged(ref field, value);
  }

  public int VertexShaderCount {
    get;
    set => this.RaiseAndSetIfChanged(ref field, value);
  }

  public int FragmentShaderCount {
    get;
    set => this.RaiseAndSetIfChanged(ref field, value);
  }

  public int FinTextureCount {
    get;
    set => this.RaiseAndSetIfChanged(ref field, value);
  }

  public int OpenGlTextureCount {
    get;
    set => this.RaiseAndSetIfChanged(ref field, value);
  }

  public int SamplerCount {
    get;
    set => this.RaiseAndSetIfChanged(ref field, value);
  }
}

[IocCandiate]
public static class DebugService {
  public static DebugServiceViewModel ViewModel { get; } = new();

  public static int RenderGraphElementCount {
    get => ViewModel.RenderGraphElementCount;
    set => ViewModel.RenderGraphElementCount = value;
  }

  public static int ModelCount {
    get => ViewModel.ModelCount;
    set => ViewModel.ModelCount = value;
  }

  public static int MaterialCount {
    get => ViewModel.MaterialCount;
    set => ViewModel.MaterialCount = value;
  }

  public static int OpaqueMaterialCount {
    get => ViewModel.OpaqueMaterialCount;
    set => ViewModel.OpaqueMaterialCount = value;
  }

  public static int TransparentMaterialCount {
    get => ViewModel.TransparentMaterialCount;
    set => ViewModel.TransparentMaterialCount = value;
  }

  public static int ProgramCount {
    get => ViewModel.ProgramCount;
    set => ViewModel.ProgramCount = value;
  }

  public static int VertexShaderCount {
    get => ViewModel.VertexShaderCount;
    set => ViewModel.VertexShaderCount = value;
  }

  public static int FragmentShaderCount {
    get => ViewModel.FragmentShaderCount;
    set => ViewModel.FragmentShaderCount = value;
  }

  public static int FinTextureCount {
    get => ViewModel.FinTextureCount;
    set => ViewModel.FinTextureCount = value;
  }

  public static int OpenGlTextureCount {
    get => ViewModel.OpenGlTextureCount;
    set => ViewModel.OpenGlTextureCount = value;
  }

  public static int SamplerCount {
    get => ViewModel.SamplerCount;
    set => ViewModel.SamplerCount = value;
  }
}