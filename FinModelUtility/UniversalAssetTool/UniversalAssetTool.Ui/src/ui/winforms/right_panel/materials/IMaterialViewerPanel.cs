using fin.model;

namespace uni.ui.winforms.right_panel.materials {
  public interface IMaterialViewerPanel {
    public IReadOnlyMaterial? Material { get; set; }
  }
}