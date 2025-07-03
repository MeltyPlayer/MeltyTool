using System;
using System.Collections.Generic;

using Autofac;

namespace fin.ioc;

public static class Ioc {
  private static readonly IContainer ROOT_;
  private static readonly Stack<ILifetimeScope> SCOPE_STACK_ = new();

  static Ioc() {
    var containerBuilder = new ContainerBuilder();

    ROOT_ = containerBuilder.Build();
    SCOPE_STACK_.Push(ROOT_);
  }

  public static ILifetimeScope WithDisposableScope(
      Action<ContainerBuilder> builder)
    => ROOT_.BeginLifetimeScope(builder);

  public static void WithBlockScope(Action<ContainerBuilder> builder,
                                    Action scopeHandler) {
    var currentScope = SCOPE_STACK_.Peek();
    var newScope = currentScope.BeginLifetimeScope(builder);
    SCOPE_STACK_.Push(newScope);

    scopeHandler();

    SCOPE_STACK_.Pop();
  }

  public static T Get<T>() where T : notnull
    => SCOPE_STACK_.Peek().Resolve<T>();
}