using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;

namespace uni.ui.avalonia.helpers;

internal static class VisualExtensions {
  extension<TControl>(Visual scope) where TControl : Control {
    public bool Any() => scope.All<TControl>().Any();
    public TControl First() => scope.All<TControl>().First();

    public TControl Single() => scope.All<TControl>().Single();

    public TControl Single(Func<TControl, bool> predicate)
      => scope.All<TControl>().Single(predicate);

    public IEnumerable<TControl> All()
      => scope.GetVisualDescendants().OfType<TControl>();

    public IEnumerable<TControl> All(Func<TControl, bool> predicate)
      => scope.GetVisualDescendants().OfType<TControl>().Where(predicate);
  }

  extension(Visual scope) {
    public void ClickText(string text)
      => scope
         .All<Control>()
         .Single(c => c.Text?.Contains(text) ?? false)
         .Click();
  }
}