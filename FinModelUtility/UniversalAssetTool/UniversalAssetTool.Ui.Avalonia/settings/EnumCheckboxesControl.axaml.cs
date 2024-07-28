using System;
using System.Collections.Generic;
using System.Linq;

using Avalonia.Controls;

using fin.data.sets;

using uni.config;
using uni.ui.avalonia.ViewModels;

namespace uni.ui.avalonia.settings;

public class EnumCheckboxesViewModelForDesigner()
    : EnumCheckboxesViewModel(
        new FinSet<Enum>(),
        GetValuesAndControlsForEnum(new HashSet<ExportedFormat>()));

public class EnumCheckboxesViewModel : ViewModelBase {
  public IReadOnlyList<CheckBox> Checkboxes { get; }

  public static EnumCheckboxesViewModel From<TEnum>(ISet<TEnum> values)
      where TEnum : struct, Enum
    => new(new CastSet<TEnum, Enum>(values),
           GetValuesAndControlsForEnum(values));

  protected EnumCheckboxesViewModel(
      IFinSet<Enum> values,
      IReadOnlyList<(Enum, CheckBox)> valuesAndCheckboxes) {
    this.Checkboxes = valuesAndCheckboxes.Select(t => t.Item2).ToArray();
    foreach (var (value, checkbox) in valuesAndCheckboxes) {
      checkbox.IsCheckedChanged += (_, _) => {
        switch (checkbox.IsChecked) {
          case true: {
            values.Add(value);
            break;
          }
          case false: {
            values.Remove(value);
            break;
          }
        }
      };
    }
  }

  protected static IReadOnlyList<(Enum, CheckBox)>
      GetValuesAndControlsForEnum<TEnum>(ISet<TEnum>? values = null)
      where TEnum : struct, Enum
    => Enum.GetValues<TEnum>()
           .Select(value => ((Enum) value, new CheckBox {
               IsChecked = values?.Contains(value) ?? false,
               Content = value.ToString(),
           }))
           .ToArray();
}

public partial class EnumCheckboxesControl : UserControl {
  public EnumCheckboxesControl() => this.InitializeComponent();
}