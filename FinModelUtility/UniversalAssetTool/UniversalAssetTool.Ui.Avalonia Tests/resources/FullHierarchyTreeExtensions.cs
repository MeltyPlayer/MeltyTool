using Avalonia.Controls;
using Avalonia.Controls.Primitives;

using fin.data.queues;
using fin.util.asserts;

using Material.Icons;
using Material.Icons.Avalonia;

using uni.ui.avalonia.helpers;

namespace uni.ui.avalonia.resources;

internal static class FullHierarchyTreeExtensions {
  extension(FullHierarchyTree fullHierarchyTree) {
    public IReadOnlyList<IconAndText> GetVisibleIconsAndText() {
      var stackPanelByNode
          = fullHierarchyTree
            .Single<TreeDataGrid>()
            .All<TreeDataGridRow>()
            .Select(r => r.Single<StackPanel>())
            .Where(stackPanel => stackPanel.DataContext != null)
            .ToDictionary(stackPanel => stackPanel.DataContext);

      var rootIconsAndText = new List<IconAndText>();

      var treeQueue
          = new FinTuple2Queue<IFullHierarchyNode, List<IconAndText>>(
              fullHierarchyTree.ViewModel.Source.Items
                               .Select(n => (n, outRootNodes: rootIconsAndText)));
      while (treeQueue.TryDequeue(out var currentFhNode,
                                  out var currentIconsAndText)) {
        if (!stackPanelByNode.TryGetValue(currentFhNode, out var stackPanel)) {
          continue;
        }

        var materialIcon = stackPanel.Single<MaterialIcon>();
        var textBlock = stackPanel.Single<TextBlock>();

        var icon = materialIcon.Kind.AssertNonnull()!.Value;
        var text = textBlock.Text.AssertNonnull();
        var newIconsAndText = new List<IconAndText>();
        currentIconsAndText.Add(new IconAndText(icon, text, newIconsAndText));

        treeQueue.Enqueue(
            currentFhNode.Children.Select(n => (n, nextChildNodes: newIconsAndText)));
      }

      return rootIconsAndText;
    }
  }

  public sealed record IconAndText(
      MaterialIconKind Icon,
      string Text,
      IReadOnlyList<IconAndText> Children) {
    public IconAndText(MaterialIconKind icon, string text)
        : this(icon, text, []) { }

    public static implicit operator IconAndText(
        in (MaterialIconKind icon, string text) tuple)
      => new(tuple.icon, tuple.text);

    public static implicit operator IconAndText(
        in (MaterialIconKind icon, string text, IconAndText[] children
            ) tuple)
      => new(tuple.icon, tuple.text, tuple.children);

    public bool Equals(IconAndText? other) {
      if (other == null) {
        return false;
      }

      return this.Icon == other.Icon &&
             this.Text == other.Text &&
             this.Children.SequenceEqual(other.Children);
    }
  }
}