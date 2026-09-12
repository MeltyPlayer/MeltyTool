using Avalonia.Controls;

using fin.util.asserts;

using Material.Icons;
using Material.Icons.Avalonia;

using uni.ui.avalonia.helpers;

namespace uni.ui.avalonia.resources;

internal static class FullHierarchyTreeExtensions {
  extension(FullHierarchyTree fullHierarchyTree) {
    public IEnumerable<(MaterialIcon materialIcon, TextBlock textBlock)> GetAllNodes()
      => fullHierarchyTree
         .All<StackPanel>()
         .Select(sp => (sp.Single<MaterialIcon>(), sp.Single<TextBlock>()));


    public IEnumerable<(MaterialIconKind icon, string text)> GetAllIconsAndText()
      => fullHierarchyTree
         .GetAllNodes()
         .Select(tuple => (tuple.materialIcon.Kind.AssertNonnull()!.Value,
                           tuple.textBlock.Text.AssertNonnull()));
  }
}