using Avalonia.Controls;

using fin.util.asserts;

using uni.ui.avalonia.helpers;

namespace uni.ui.avalonia.resources.registers;

internal static class RegistersPanelExtensions {
  extension(RegistersPanel registersPanel) {
    public IEnumerable<string> GetColorRegisterNames()
      => registersPanel
         .Single<GroupBox>(gb => gb.Header is "Color registers")
         .GetRegisterNames_();

    public IEnumerable<string> GetScalarRegisterNames()
      => registersPanel
         .Single<GroupBox>(gb => gb.Header is "Scalar registers")
         .GetRegisterNames_();
  }

  private static IEnumerable<string> GetRegisterNames_(
      this GroupBox scope)
    => scope
       .First<ItemsControl>()
       .All<TextBlock>()
       .Select(t => t.Text.AssertNonnull());
}