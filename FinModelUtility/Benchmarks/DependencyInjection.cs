using System.Runtime.CompilerServices;

using BenchmarkDotNet.Attributes;

namespace benchmarks;

public class DependencyInjection {
  private const int n = 100000;
  private const float value = 1;


  public interface IMethod {
    double Run(double x);
  }


    
    
  [Benchmark]
  public void CallDirectly() {
      for (var i = 0; i < n; i++) {
        var x = Math.Acos(value);
      }
    }



  [Benchmark]
  public void CallViaMethod() {
      var viaMethod = new ViaMethodWrapper();
      for (var i = 0; i < n; i++) {
        var x = viaMethod.Run(value);
      }
    }

  public class ViaMethodWrapper : IMethod {
    public double Run(double d) => Math.Acos(d);
  }




  [Benchmark]
  public void CallViaInline() {
      var viaInlineWrapper = new ViaInlineWrapper();
      for (var i = 0; i < n; i++) {
        var x = viaInlineWrapper.Run(value);
      }
    }

  public class ViaInlineWrapper : IMethod {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double Run(double d) => Math.Acos(d);
  }



  [Benchmark]
  public void CallViaOptimizedInline() {
      var viaOptimizedInlineWrapper = new ViaOptimizedInlineWrapper();
      for (var i = 0; i < n; i++) {
        var x = viaOptimizedInlineWrapper.Run(value);
      }
    }

  public class ViaOptimizedInlineWrapper : IMethod {
    [MethodImpl(MethodImplOptions.AggressiveInlining |
                MethodImplOptions.AggressiveOptimization)]
    public double Run(double d) => Math.Acos(d);
  }





  [Benchmark]
  public void CallViaMethodGroup() {
      var viaMethodGroup = new ViaMethodGroup(Math.Acos);
      for (var i = 0; i < n; i++) {
        var x = viaMethodGroup.methodGroup(value);
      }
    }

  public class ViaMethodGroup {
    public Func<double, double> methodGroup;

    public ViaMethodGroup(Func<double, double> methodGroup) {
        this.methodGroup = methodGroup;
      }
  }




  [Benchmark]
  public void CallViaLambda() {
      var viaLambda = new ViaLambda(d => Math.Acos(d));
      for (var i = 0; i < n; i++) {
        var x = viaLambda.lambda(value);
      }
    }

  public class ViaLambda {
    public Func<double, double> lambda;

    public ViaLambda(Func<double, double> lambda) {
        this.lambda = lambda;
      }
  }




  [Benchmark]
  public void CallViaInterfaceImpl() {
      var viaInterface = new ViaInterfaceImpl(new ViaOptimizedInlineWrapper());
      for (var i = 0; i < n; i++) {
        var x = viaInterface.Run(value);
      }
    }

  public class ViaInterfaceImpl : IMethod {
    private readonly IMethod impl_;

    public ViaInterfaceImpl(IMethod impl) {
        this.impl_ = impl;
      }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double Run(double x)  => this.impl_.Run(x);
  }



  [Benchmark]
  public void CallViaGenericImpl() {
      var viaGeneric = new ViaGenericImpl<ViaOptimizedInlineWrapper>(new ViaOptimizedInlineWrapper());
      for (var i = 0; i < n; i++) {
        var x = viaGeneric.Run(value);
      }
    }

  public class ViaGenericImpl<TMethod> where TMethod : IMethod {
    private readonly TMethod impl_;

    public ViaGenericImpl(TMethod impl) {
        this.impl_ = impl;
      }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double Run(double x) => this.impl_.Run(x);
  }




  [Benchmark]
  public void CallViaStruct() {
      var viaGeneric = new ViaStruct();
      for (var i = 0; i < n; i++) {
        var x = viaGeneric.Run(value);
      }
    }

  public readonly struct ViaStruct : IMethod {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double Run(double x) => Math.Acos(x);
  }


  [Benchmark]
  public void CallViaGenericStruct() {
      var viaGeneric = new ViaGenericImpl<ViaStruct>(new ViaStruct());
      for (var i = 0; i < n; i++) {
        var x = viaGeneric.Run(value);
      }
    }
}