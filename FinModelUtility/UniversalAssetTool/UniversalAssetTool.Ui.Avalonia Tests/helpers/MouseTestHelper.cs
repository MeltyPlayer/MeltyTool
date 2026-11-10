using Avalonia;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

using fin.util.asserts;

namespace uni.ui.avalonia.helpers;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/AvaloniaUI/Avalonia/blob/eb5cce2492afaf2e296c290acbcffbf117e46dcc/tests/Avalonia.UnitTests/MouseTestHelper.cs#L62
/// </summary>
public static class MouseTestHelper {
  private static readonly Pointer pointer_
      = new(Pointer.GetNextFreeId(), PointerType.Mouse, true);

  private static ulong nextStamp_ = 1;
  private static ulong Timestamp_() => nextStamp_++;

  private static RawInputModifiers Convert_(MouseButton mouseButton)
    => mouseButton switch {
        MouseButton.Left   => RawInputModifiers.LeftMouseButton,
        MouseButton.Right  => RawInputModifiers.RightMouseButton,
        MouseButton.Middle => RawInputModifiers.MiddleMouseButton,
        _                  => RawInputModifiers.None,
    };

  private static int ButtonCount_(PointerPointProperties props) {
    var rv = 0;
    if (props.IsLeftButtonPressed)
      rv++;
    if (props.IsMiddleButtonPressed)
      rv++;
    if (props.IsRightButtonPressed)
      rv++;
    return rv;
  }

  public static void Down(Interactive target,
                          MouseButton mouseButton = MouseButton.Left,
                          Point? position = null,
                          KeyModifiers modifiers = default,
                          int clickCount = 1)
    => Down(target, target, mouseButton, position, modifiers, clickCount);

  public static void Down(Interactive target,
                          Interactive source,
                          MouseButton mouseButton = MouseButton.Left,
                          Point? position = null,
                          KeyModifiers modifiers = default,
                          int clickCount = 1) {
    var props = new PointerPointProperties(
        Convert_(mouseButton),
        mouseButton == MouseButton.Left
            ? PointerUpdateKind.LeftButtonPressed
            : mouseButton == MouseButton.Middle
                ? PointerUpdateKind.MiddleButtonPressed
                : mouseButton == MouseButton.Right
                    ? PointerUpdateKind.RightButtonPressed
                    : PointerUpdateKind.Other
    );
    if (ButtonCount_(props) > 1)
      Move(target, source, position ?? default);
    else {
      source.RaiseEvent(
          new PointerPressedEventArgs(
              source,
              pointer_,
              GetRoot_(target),
              position ??
              MidpointRelativeToRoot_(
                  target),
              Timestamp_(),
              props,
              modifiers,
              clickCount));
    }
  }

  public static void Move(Interactive target,
                          in Point position,
                          KeyModifiers modifiers = default)
    => Move(target, target, position, modifiers);

  public static void Move(Interactive target,
                          Interactive source,
                          in Point position,
                          KeyModifiers modifiers = default) {
    var e = new PointerEventArgs(InputElement.PointerMovedEvent,
                                 source,
                                 pointer_,
                                 GetRoot_(target),
                                 position,
                                 Timestamp_(),
                                 new PointerPointProperties(
                                     default,
                                     PointerUpdateKind.Other),
                                 modifiers);

    target.RaiseEvent(e);
  }

  public static void Up(Interactive target,
                        MouseButton mouseButton = MouseButton.Left,
                        Point? position = null,
                        KeyModifiers modifiers = default)
    => Up(target, target, mouseButton, position, modifiers);

  public static void Up(Interactive target,
                        Interactive source,
                        MouseButton mouseButton = MouseButton.Left,
                        Point? position = null,
                        KeyModifiers modifiers = default) {
    var props = new PointerPointProperties(
        Convert_(mouseButton),
        mouseButton == MouseButton.Left
            ? PointerUpdateKind.LeftButtonReleased
            : mouseButton == MouseButton.Middle
                ? PointerUpdateKind.MiddleButtonReleased
                : mouseButton == MouseButton.Right
                    ? PointerUpdateKind.RightButtonReleased
                    : PointerUpdateKind.Other
    );
    if (ButtonCount_(props) == 0) {
      var e = new PointerReleasedEventArgs(
          source,
          pointer_,
          GetRoot_(target),
          position ??
          MidpointRelativeToRoot_(
              target),
          Timestamp_(),
          props,
          modifiers,
          mouseButton);

      target.RaiseEvent(e);
    } else {
      Move(target, source, position ?? default);
    }
  }

  public static void Click(Interactive target,
                           MouseButton button = MouseButton.Left,
                           Point? position = null,
                           KeyModifiers modifiers = default)
    => Click(target, target, button, position, modifiers);

  public static void Click(Interactive target,
                           Interactive source,
                           MouseButton button = MouseButton.Left,
                           Point? position = null,
                           KeyModifiers modifiers = default) {
    Down(target, source, button, position, modifiers);
    var captured = (pointer_.Captured as Interactive) ?? source;
    Up(captured, captured, button, position, modifiers);
  }

  public static void DoubleClick(Interactive target,
                                 MouseButton button = MouseButton.Left,
                                 Point? position = null,
                                 KeyModifiers modifiers = default)
    => DoubleClick(target, target, button, position, modifiers);

  public static void DoubleClick(Interactive target,
                                 Interactive source,
                                 MouseButton button = MouseButton.Left,
                                 Point? position = null,
                                 KeyModifiers modifiers = default) {
    Down(target, source, button, position, modifiers, clickCount: 1);
    var captured = (pointer_.Captured as Interactive) ?? source;
    Up(captured, captured, button, position, modifiers);
    Down(target, source, button, position, modifiers, clickCount: 2);
  }

  public static void Enter(Interactive target) {
    target.RaiseEvent(new PointerEventArgs(InputElement.PointerEnteredEvent,
                                           target,
                                           pointer_,
                                           (Visual) target,
                                           default,
                                           Timestamp_(),
                                           new PointerPointProperties(
                                               default,
                                               PointerUpdateKind.Other),
                                           KeyModifiers.None));
  }

  public static void Leave(Interactive target) {
    target.RaiseEvent(new PointerEventArgs(InputElement.PointerExitedEvent,
                                           target,
                                           pointer_,
                                           (Visual) target,
                                           default,
                                           Timestamp_(),
                                           new PointerPointProperties(
                                               default,
                                               PointerUpdateKind.Other),
                                           KeyModifiers.None));
  }

  private static Visual GetRoot_(Interactive source)
    => source.GetPresentationSource()
             .AssertNonnull()
             .RootVisual
             .AssertNonnull();

  private static Point MidpointRelativeToRoot_(Interactive element) {
    var root = GetRoot_(element);
    return element
           .TranslatePoint(new(element.Bounds.Width / 2,
                               element.Bounds.Height / 2),
                           root)
           .GetValueOrDefault();
  }
}