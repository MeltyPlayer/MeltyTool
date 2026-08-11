using System;
using System.Linq;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;

namespace fin.ui.avalonia.styles;

using SelectorDelegate = Func<Selector?, Selector>;
using NonnullSelectorDelegate = Func<Selector, Selector>;

public sealed class HeaderStyles : Styles {
  public HeaderStyles() {
    for (var i = HeaderResources.MAX_SIZE_INDEX;
         i <= HeaderResources.MIN_SIZE_INDEX;
         i++) {
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
                      (Selectors.OfType<GroupBox>,
                       x => x.DescendantOfType<AccessText>()),
                  }
              .Select(tuple => this.GetTargetSelectorDelegates_(
                          i,
                          tuple.topLevelSelector,
                          tuple.childSelector));

      foreach (var tuple in targetSelectorTuples) {
        var (topLevelSelector,
            targetSelector,
            targetWithoutSpaceFirstSelector) = tuple;

        this.AddStyle(targetSelector)
            .AddSetter(TextBlock.FontSizeProperty, HeaderResources.Instance.FontSizes[i - 1])
            .AddSetter(TextBlock.FontWeightProperty, FontWeight.Medium)
            .AddSetter(TextBlock.PaddingProperty, new Thickness(0));

        this.AddStyle(targetWithoutSpaceFirstSelector)
            .AddSetter(TextBlock.PaddingProperty,
                       HeaderResources.Instance.Paddings[i - 1]);

        this.AddStyle(topLevelSelector.ChildOfType<TabItem>())
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

  public static SelectorDelegate ChildOfType<T>(this SelectorDelegate s)
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