using fin.model;

namespace fin.ui.rendering;

public static class SelectedBoneService {
  public static event Action<IReadOnlyBone?> OnBoneSelected;

  public static void SelectBone(IReadOnlyBone? bone) {
    if (SelectedServiceUtils.SelectableType is SelectableType.BONE ||
        bone == null) {
      OnBoneSelected?.Invoke(bone);
    }
  }
}