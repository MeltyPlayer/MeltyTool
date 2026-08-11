using System;

using Avalonia.Controls;
using Avalonia.Controls.Templates;

using fin.util.asserts;
using fin.util.strings;

namespace uni.ui.avalonia;

// TODO: Associate view model with type via generics instead, get them that way
// instead of via reflection
public class ViewLocator : IDataTemplate {
  public Control Build(object? data) {
    var type = GetViewType_(data).AssertNonnull();
    return (Control) Activator.CreateInstance(type)!;
  }

  public bool Match(object? data) => GetViewType_(data) != null;

  private static Type? GetViewType_(object? data) {
    var viewModelTypeName = data?.GetType().FullName;
    if (viewModelTypeName == null) {
      return null;
    }

    return viewModelTypeName.TryRemoveEnd("ViewModel", out var viewTypeName)
        ? Type.GetType(viewTypeName)
        : null;
  }
}