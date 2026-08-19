using System.Linq.Expressions;
using System.Reflection;

using Microsoft.Scripting.Hosting;

using Pfim;


namespace ModelPluginWrappers;

public static class ScopeExtensions {
  public static void PushEnumIntoScope<TEnum>(this ScriptScope scriptScope)
      where TEnum : struct, Enum {
    foreach (var value in Enum.GetValues<TEnum>()) {
      var name = value.ToString();
      scriptScope.SetVariable(name, value);
    }
  }

  public static ScriptScope AddInstanceMembers<T>(
      this ScriptScope scope,
      T instance) {
    var type = typeof(T);

    var instanceMethodInfos
        = type.GetMethods()
              .Where(m => m.DeclaringType != typeof(object));
    foreach (var instanceMethodInfo in instanceMethodInfos) {
      scope.SetVariable(instanceMethodInfo.Name,
                        instanceMethodInfo.CreateInstanceDelegate_(instance));
    }

    return scope;
  }

  public static ScriptScope AddStaticMembers<T>(this ScriptScope scope) {
    var type = typeof(T);

    var staticMethodInfos = type.GetMethods().Where(m => m.IsStatic);
    foreach (var staticMethodInfo in staticMethodInfos) {
      scope.SetVariable(staticMethodInfo.Name,
                        staticMethodInfo.CreateStaticDelegate_());
    }

    var pushEnumIntoScopeInfo =
        typeof(ScopeExtensions).GetMethod("PushEnumIntoScope");
    foreach (var enumType in type.GetNestedTypes().Where(t => t.IsEnum)) {
      pushEnumIntoScopeInfo
          .MakeGenericMethod(enumType)
          .Invoke(null, [scope]);
    }

    return scope;
  }

  private static Type GetDelegateType_(this MethodInfo methodInfo) {
    var parmTypes =
        methodInfo.GetParameters().Select(parm => parm.ParameterType);
    var parmAndReturnTypes = parmTypes.Append(methodInfo.ReturnType).ToArray();
    return Expression.GetDelegateType(parmAndReturnTypes);
  }

  private static Delegate CreateInstanceDelegate_<T>(
      this MethodInfo methodInfo,
      T target)
    => methodInfo.CreateDelegate(methodInfo.GetDelegateType_(), target);

  private static Delegate CreateStaticDelegate_(this MethodInfo methodInfo)
    => methodInfo.CreateDelegate(methodInfo.GetDelegateType_());
}