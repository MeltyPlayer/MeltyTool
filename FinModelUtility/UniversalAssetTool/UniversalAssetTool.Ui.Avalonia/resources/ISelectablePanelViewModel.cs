using fin.ui.rendering;

namespace uni.ui.avalonia.resources;

public interface ISelectablePanelViewModel {
  SelectableType Type { get; }
  void ReselectSelectedElementViaService();
}