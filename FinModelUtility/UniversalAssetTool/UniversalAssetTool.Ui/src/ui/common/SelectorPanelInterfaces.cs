using System.Collections.Generic;
using System.Linq;

using fin.data.dictionaries;
using fin.io.bundles;
using fin.util.enumerables;

namespace uni.ui.common {
  public interface ISelectorPanel<T> where T : notnull {
    T? Selected { get; set; }


    event OnSelectedDelegate OnSelected;

    delegate void OnSelectedDelegate(T selected);
  }

  public record SelectorPanelKey<T> { }

  public static class SelectorPanels {
    private static readonly SortedSetDictionary<object, object> panelsByKey_ =
        new();

    private static readonly SortedDictionary<object, object> delegatesByKey_;

    private static readonly SortedSetDictionary<object, object> handlersByKey =
        new();

    public static SelectorPanelKey<IAnnotatedFileBundle> FILE_BUNDLE { get; } =
      new();

    public static void RegisterPanel<T>(SelectorPanelKey<T> key,
                                        ISelectorPanel<T> panel)
        where T : notnull {
      panelsByKey_.Add(key, panel);

      if (handlersByKey.TryGetSet(key, out var handlers)) {
        SubscribeHandlersToPanel_(
            handlers.Cast<ISelectorPanel<T>.OnSelectedDelegate>(),
            panel);
      }
    }

    public static void AddHandler<T>(
        SelectorPanelKey<T> key,
        ISelectorPanel<T>.OnSelectedDelegate handler)
        where T : notnull {
      handlersByKey.Add(key, handler);

      if (panelsByKey_.TryGetSet(key, out var panels)) {
        foreach (var panel in panels.Cast<ISelectorPanel<T>>()) {
          SubscribeHandlersToPanel_(handler.Yield(), panel);
        }
      }
    }

    private static void SubscribeHandlersToPanel_<T>(
        IEnumerable<ISelectorPanel<T>.OnSelectedDelegate> handlers,
        ISelectorPanel<T> panel) where T : notnull {
      foreach (var handler in handlers) {
        panel.OnSelected += handler;
      }
    }
  }
}