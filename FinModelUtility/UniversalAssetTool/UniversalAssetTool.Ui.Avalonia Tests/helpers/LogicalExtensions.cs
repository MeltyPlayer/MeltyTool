using Avalonia.Controls;
using Avalonia.LogicalTree;

namespace uni.ui.avalonia.helpers;

internal static class LogicalExtensions {
  extension<TControl>(ILogical scope) where TControl : Control {
    public TControl First() => scope.All<TControl>().First();
    public TControl Single() => scope.All<TControl>().Single();

    public IEnumerable<TControl> All()
      => scope.GetLogicalDescendants().OfType<TControl>();
  }

  extension(ILogical scope) {
    public void ClickText(string text)
      => scope
         .All<Control>()
         .Single(c => c.Text?.Contains(text) ?? false)
         .Click();
  }
}