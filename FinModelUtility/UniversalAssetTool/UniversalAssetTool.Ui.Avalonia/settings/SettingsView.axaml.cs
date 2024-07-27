using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.SettingsFactory.ViewModels;

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

using Avalonia.SettingsFactory;

namespace uni.ui.avalonia.settings;

public partial class SettingsView : SettingsFactory, ISettingsValidator {
  public SettingsView() {
    AvaloniaXamlLoader.Load(this);

    // Very much unnecessary, but not having this bothers me.
    // Allows you to focus seemingly nothing.
    Grid focusDelegate = this.FindControl<Grid>("FocusDelegate")!;
    focusDelegate.PointerPressed += (_, _) => focusDelegate.Focus();
    Grid focusDelegate2 = this.FindControl<Grid>("FocusDelegate2")!;
    focusDelegate2.PointerPressed += (_, _) => focusDelegate.Focus();

    SettingsFactoryOptions options = new() {
        // Application implementation of a message prompt
        AlertAction = (msg) => Debug.WriteLine(msg),

        // Custom resource loader
        FetchResource = (res)
            => JsonSerializer.Deserialize<Dictionary<string, string>>(
                File.ReadAllText(res))
    };

    // Initialize the settings layout
    InitializeSettingsFactory(new SettingsFactoryViewModel(true),
                              this,
                              new SettingsViewModel(),
                              options);
  }

  public bool? ValidateString(string key, string value) => null;
  public bool? ValidateBool(string key, bool value) => null;
  public string? ValidateSave(Dictionary<string, bool?> validated) => null;
}