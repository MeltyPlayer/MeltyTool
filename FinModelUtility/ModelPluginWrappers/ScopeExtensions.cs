using System.Linq.Expressions;
using System.Reflection;

using Microsoft.Scripting.Hosting;


namespace ModelPluginWrappers;

public static class ScopeExtensions {
  public static void PushEnumIntoScope<TEnum>(this ScriptScope scriptScope)
      where TEnum : struct, Enum {
    foreach (var value in Enum.GetValues<TEnum>()) {
      var name = value.ToString();
      scriptScope.SetVariable(name, value);
    }
  }

  public static ScriptScope AddClassMembers<T>(this ScriptScope scope,
                                               T instance) {
    var type = typeof(T);

    var instanceMethodInfos
        = type.GetMethods()
              .Where(m => m.DeclaringType != typeof(object));
    foreach (var instanceMethodInfo in instanceMethodInfos) {
      scope.SetVariable(instanceMethodInfo.Name,
                        instanceMethodInfo.CreateDelegate_(instance));
    }

    var staticMethodInfos = type.GetMethods(BindingFlags.Static);
    foreach (var staticMethodInfo in staticMethodInfos) {
      scope.SetVariable(staticMethodInfo.Name, staticMethodInfo);
    }

    var pushEnumIntoScopeInfo =
        typeof(ScriptScope).GetMethod("PushEnumIntoScope");
    foreach (var enumType in type.GetNestedTypes().Where(t => t.IsEnum)) {
      pushEnumIntoScopeInfo
          .MakeGenericMethod(enumType)
          .Invoke(scope, null);
    }

    return scope;
  }

  private static Delegate CreateDelegate_<T>(this MethodInfo methodInfo,
                                             T target) {
    var parmTypes =
        methodInfo.GetParameters().Select(parm => parm.ParameterType);
    var parmAndReturnTypes = parmTypes.Append(methodInfo.ReturnType).ToArray();
    var delegateType = Expression.GetDelegateType(parmAndReturnTypes);

    if (methodInfo.IsStatic)
      return methodInfo.CreateDelegate(delegateType);
    return methodInfo.CreateDelegate(delegateType, target);
  }
}