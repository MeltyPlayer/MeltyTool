using System;
using System.Linq;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;

using gbGroupBox = GroupBox.Avalonia.Controls.GroupBox;

namespace fin.ui.avalonia.styles;

using SelectorDelegate = Func<Selector?, Selector>;
using NonnullSelectorDelegate = Func<Selector, Selector>;

public class HeaderStyles : Styles {
  public HeaderStyles() {
    var maxSizeIndex = 1;
    var minSizeIndex = 4;

    for (var i = maxSizeIndex; i <= minSizeIndex; i++) {
      var targetSelectorTuples
          = new (SelectorDelegate topLevelSelector, NonnullSelectorDelegate
                  childSelector)
                  [] {
                      (Selectors.OfType<TextBlock>, x => x),
                      (Selectors.OfType<SelectableTextBlock>, x => x),
                      (Selectors.OfType<TabControl>,
                       x => x.ChildOfType<TabItem>()
                             .Template()
                             .OfType<Border>()
                             .ChildOfType<Panel>()
                             .ChildOfType<ContentPresenter>()
                             .Name("PART_ContentPresenter")),
                      (Selectors.OfType<gbGroupBox>,
                       x => x.ChildOfType<Grid>()
                             .ChildOfType<ContentPresenter>()
                             .Name("PART_HeaderPresenter")),
                  }
              .Select(tuple => this.GetTargetSelectorDelegates_(
                          i,
                          tuple.topLevelSelector,
                          tuple.childSelector));

      var fontSize = 13 + (minSizeIndex - maxSizeIndex - i) * 2;
      var topPadding = fontSize * .5;

      foreach (var tuple in targetSelectorTuples) {
        var (topLevelSelector,
            targetSelector,
            targetWithoutSpaceFirstSelector) = tuple;

        this.AddStyle(targetSelector)
            .AddSetter(TextBlock.FontSizeProperty, fontSize)
            .AddSetter(TextBlock.FontWeightProperty, FontWeight.Medium)
            .AddSetter(TextBlock.PaddingProperty, new Thickness(0));

        this.AddStyle(targetWithoutSpaceFirstSelector)
            .AddSetter(TextBlock.PaddingProperty,
                       new Thickness(0, topPadding, 0, 0));

        this.AddStyle(topLevelSelector.Child().OfType<TabItem>())
            .AddSetter(Layoutable.MinHeightProperty, 0);
      }
    }
  }

  private TargetSelectorDelegates GetTargetSelectorDelegates_(
      int headingIndex,
      SelectorDelegate topLevelSelector,
      NonnullSelectorDelegate childSelector) {
    var topLevelWithHeadingSelector
        = topLevelSelector.Class($"h{headingIndex}");
    var topLevelWithHeadingWithoutSpacingFirstSelector
        = topLevelWithHeadingSelector.Not(y => y.Class("spaceFirst"));

    return new TargetSelectorDelegates(
        topLevelWithHeadingSelector,
        topLevelWithHeadingSelector.Then(childSelector),
        topLevelWithHeadingWithoutSpacingFirstSelector.Then(childSelector)
    );
  }
}

public record TargetSelectorDelegates(
    SelectorDelegate TopLevelSelector,
    SelectorDelegate TargetSelector,
    SelectorDelegate TargetWithoutSpaceFirstSelector);

public static class SelectorExtensions {
  public static Selector ChildOfType<T>(this Selector s)
      where T : StyledElement
    => s.Child().OfType<T>();

  public static Selector DescendantOfType<T>(this Selector s)
      where T : StyledElement
    => s.Descendant().OfType<T>();
}

public static class SelectorDelegateExtensions {
  public static SelectorDelegate Then(this SelectorDelegate d,
                                      SelectorDelegate next)
    => x => next(d(x));

  public static SelectorDelegate OfType<T>(this SelectorDelegate d)
      where T : StyledElement
    => x => d(x).OfType<T>();

  public static SelectorDelegate Class(this SelectorDelegate d,
                                       string className)
    => x => d(x).Class(className);

  public static SelectorDelegate Child(this SelectorDelegate d)
    => x => d(x).Child();

  public static SelectorDelegate Not(this SelectorDelegate d,
                                     SelectorDelegate notHandler)
    => x => d(x).Not(notHandler);
}

public static class StyleExtensions {
  public static Style AddStyle(this Styles parent,
                               SelectorDelegate childHandler) {
    var child = new Style(childHandler);
    parent.Add(child);
    return child;
  }

  public static Style AddChild(this Style parent,
                               SelectorDelegate childHandler) {
    var child = new Style(x => childHandler(x.Nesting()));
    parent.Children.Add(child);
    return child;
  }

  public static Style AddSetter<T>(
      this Style style,
      AvaloniaProperty<T> property,
      T value) {
    style.Setters.Add(new Setter(property, value));
    return style;
  }
}