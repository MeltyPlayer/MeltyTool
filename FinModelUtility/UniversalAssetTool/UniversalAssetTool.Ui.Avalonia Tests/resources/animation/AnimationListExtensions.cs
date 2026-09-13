using Avalonia.Controls;

using fin.util.asserts;

using uni.ui.avalonia.helpers;

namespace uni.ui.avalonia.resources.animation;

internal static class AnimationListExtensions {
  extension(AnimationList animationList) {
    public IEnumerable<string> GetAnimationNames()
      => animationList
         .Single<ListBox>()
         .All<TextBlock>(t => t.Classes.Contains("regular"))
         .Select(t => t.Text.AssertNonnull());
  }
}