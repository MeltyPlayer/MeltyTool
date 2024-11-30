using System;
using System.Collections.Generic;
using System.Linq;

using fin.util.asserts;
using fin.util.linq;

namespace fin.util.types;

public static class TypesUtil {
  public static IEnumerable<Type> GetAllImplementationTypes<TInterface>()
    => AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(typeof(TInterface).IsAssignableFrom)
                .Where(t => t is {
                    IsAbstract: false, ContainsGenericParameters: false
                });

  public static IEnumerable<TInterface>
      InstantiateAllImplementationsWithDefaultConstructor<TInterface>()
    => GetAllImplementationTypes<TInterface>()
        .SelectWhere<Type, TInterface>(
            TryToInstantiateWIthDefaultConstructor_);

  private static bool TryToInstantiateWIthDefaultConstructor_<TInterface>(
      Type type,
      out TInterface value) {
    var constructor = type.GetConstructor([]);
    if (constructor == null) {
      value = default;
      return false;
    }

    value = constructor.Invoke([]).AssertAsA<TInterface>();
    return true;
  }
}