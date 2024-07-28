using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using ConfigFactory.Core;
using ConfigFactory.Generics;

namespace uni.ui.avalonia.settings;

public class EnumCheckboxesControlBuilder
    : ControlBuilder<EnumCheckboxesControlBuilder> {
  public override object? Build(IConfigModule context,
                                PropertyInfo propertyInfo) {
    dynamic value = propertyInfo.GetValue(context);
    return new EnumCheckboxesControl {
        DataContext = EnumCheckboxesViewModel.From(value),
    };
  }

  public override bool IsValid(Type type)
    => type.IsGenericType &&
       type.GetGenericTypeDefinition() == typeof(HashSet<>) &&
       type.GetGenericArguments()[0].IsEnum;
}