using fin.language.equations.fixedFunction.util;

namespace fin.language.equations.fixedFunction.impl;

// TODO: Merge this back into the main value type logic
public interface IFixedFunctionOps<TValue, out TConstant>
    where TValue : IValue<TValue>
    where TConstant : IConstant<TValue>, TValue {
  TConstant Zero { get; }
  TConstant Half { get; }
  TConstant One { get; }

  bool IsZero(TValue? value) => value == null || value.Equals(this.Zero);

  TValue? Add(TValue? lhs, TValue? rhs);
  TValue? AddWithScalar(TValue? lhs, IScalarValue? rhs);
  TValue? Subtract(TValue? lhs, TValue? rhs);
  TValue? Multiply(TValue? lhs, TValue? rhs);
  TValue? MultiplyWithScalar(TValue? lhs, IScalarValue? rhs);

  TValue? AddWithConstant(TValue? lhs, double constant);
  TValue? MultiplyWithConstant(TValue? lhs, double constant);

  TValue? MixWithConstant(TValue? lhs, TValue? rhs, double mixAmount);
  TValue? MixWithScalar(TValue? lhs, TValue? rhs, IScalarValue? mixAmount);
}

public interface IColorOps : IFixedFunctionOps<IColorValue, IColorConstant>;

public interface IScalarOps : IFixedFunctionOps<IScalarValue, IScalarConstant>;

public abstract class BFixedFunctionOps<TValue, TConstant>
    : IFixedFunctionOps<TValue, TConstant>
    where TValue : IValue<TValue>
    where TConstant : IConstant<TValue>, TValue {
  public abstract TConstant Zero { get; }
  public abstract TConstant Half { get; }
  public abstract TConstant One { get; }

  public abstract TValue? Add(TValue? lhs, TValue? rhs);
  public abstract TValue? AddWithScalar(TValue? lhs, IScalarValue? rhs);

  public abstract TValue? Subtract(TValue? lhs, TValue? rhs);

  public abstract TValue? Multiply(TValue? lhs, TValue? rhs);
  public abstract TValue? MultiplyWithScalar(TValue? lhs, IScalarValue? rhs);

  public TValue? AddWithConstant(TValue? lhs, double constant)
    => this.AddWithScalar(lhs, new ScalarConstant(constant));

  public TValue? MultiplyWithConstant(TValue? lhs, double constant)
    => this.MultiplyWithScalar(lhs, new ScalarConstant(constant));

  public TValue? MixWithConstant(
      TValue? lhs,
      TValue? rhs,
      double mixAmount) {
    lhs = this.MultiplyWithConstant(lhs, 1 - mixAmount);
    rhs = this.MultiplyWithConstant(rhs, mixAmount);

    return this.Add(lhs, rhs);
  }

  public abstract TValue? MixWithScalar(TValue? lhs,
                                        TValue? rhs,
                                        IScalarValue? mixAmount);
}

public static class ColorWrapperExtensions {
  public static ColorWrapper? Wrap(this IScalarValue? scalarValue)
    => scalarValue != null ? new ColorWrapper(scalarValue) : null;
}

public class ColorFixedFunctionOps<TIdentifier>(
    IFixedFunctionEquations<TIdentifier> equations)
    : BFixedFunctionOps<IColorValue, IColorConstant>, IColorOps
    where TIdentifier : notnull {
  private IScalarOps ScalarOps_ => equations.ScalarOps;

  private readonly IScalarConstant scMinusOne_
      = equations.CreateScalarConstant(-1);

  public override IColorConstant Zero { get; }
    = equations.CreateColorConstant(0);

  public override IColorConstant Half { get; }
    = equations.CreateColorConstant(.5f);

  public override IColorConstant One { get; }
    = equations.CreateColorConstant(1);

  public bool IsSingleScalarValue(
      IColorValue? value,
      out IScalarValue scalarValue) {
    if (value is IColorConstant {
            Intensity: IScalarConstant scalarConstant
        }) {
      scalarValue = scalarConstant;
      return true;
    }

    if (value is ColorWrapper { Intensity: IScalarConstant wrappedScalar }) {
      scalarValue = wrappedScalar;
      return true;
    }

    scalarValue = default;
    return false;
  }

  public override IColorValue? Add(IColorValue? lhs, IColorValue? rhs) {
    if (!FixedFunctionConstants.REMOVE_ZERO_TERMS) {
      lhs ??= this.Zero;
      rhs ??= this.Zero;
    } else {
      if (this.IsSingleScalarValue(rhs, out var rhsScalar)) {
        return this.AddWithScalar(lhs, rhsScalar);
      }

      var lhsIsZero = lhs.IsZero();
      var rhsIsZero = rhs.IsZero();

      if (lhsIsZero && rhsIsZero) {
        return null;
      }

      if (lhsIsZero) {
        return rhs;
      }

      if (rhsIsZero) {
        return lhs;
      }
    }

    return lhs!.Add(rhs!);
  }

  public override IColorValue? AddWithScalar(IColorValue? lhs,
                                             IScalarValue? rhs) {
    if (!FixedFunctionConstants.REMOVE_ZERO_TERMS) {
      lhs ??= this.Zero;
      rhs ??= this.ScalarOps_.Zero;
    } else {
      var lhsIsZero = lhs.IsZero();
      var rhsIsZero = this.ScalarOps_.IsZero(rhs);

      if (lhsIsZero && rhsIsZero) {
        return null;
      }

      if (lhsIsZero) {
        return equations.CreateColor(rhs!);
      }

      if (rhsIsZero) {
        return lhs;
      }

      if (this.IsSingleScalarValue(lhs, out var lhsScalar)) {
        return this.ScalarOps_.Add(lhsScalar, rhs).Wrap();
      }
    }

    return lhs!.Add(rhs!);
  }

  public override IColorValue? Subtract(IColorValue? lhs, IColorValue? rhs) {
    if (!FixedFunctionConstants.REMOVE_ZERO_TERMS) {
      lhs ??= this.Zero;
      rhs ??= this.Zero;
    } else {
      var lhsIsZero = lhs.IsZero();
      var rhsIsZero = rhs.IsZero();

      if ((lhsIsZero && rhsIsZero) || (lhs?.Equals(rhs) ?? false)) {
        return null;
      }

      if (lhsIsZero) {
        return rhs?.Multiply(this.scMinusOne_);
      }

      if (rhsIsZero) {
        return lhs;
      }
    }

    return lhs!.Subtract(rhs!);
  }

  public override IColorValue? Multiply(IColorValue? lhs, IColorValue? rhs) {
    if (!FixedFunctionConstants.REMOVE_ZERO_TERMS) {
      lhs ??= this.Zero;
      rhs ??= this.Zero;
    } else {
      if (this.IsSingleScalarValue(rhs, out var rhsScalar)) {
        return this.MultiplyWithScalar(lhs, rhsScalar);
      }

      if (lhs.IsZero() || rhs.IsZero()) {
        return null;
      }

      var lhsIsOne = lhs?.Equals(this.One) ?? false;
      var rhsIsOne = rhs?.Equals(this.One) ?? false;

      if (lhsIsOne && rhsIsOne) {
        return this.One;
      }

      if (lhsIsOne) {
        return rhs;
      }

      if (rhsIsOne) {
        return lhs;
      }
    }

    return lhs!.Multiply(rhs!);
  }

  public override IColorValue? MultiplyWithScalar(
      IColorValue? lhs,
      IScalarValue? rhs) {
    if (!FixedFunctionConstants.REMOVE_ZERO_TERMS) {
      lhs ??= this.Zero;
      rhs ??= this.ScalarOps_.Zero;
    } else {
      if (lhs.IsZero() || this.ScalarOps_.IsZero(rhs)) {
        return null;
      }

      var lhsIsOne = lhs?.Equals(this.One) ?? false;
      var rhsIsOne = rhs?.Equals(this.ScalarOps_.One) ?? false;

      if (lhsIsOne && rhsIsOne) {
        return this.One;
      }

      if (lhsIsOne) {
        return equations.CreateColor(rhs!);
      }

      if (rhsIsOne) {
        return lhs;
      }

      if (this.IsSingleScalarValue(lhs, out var lhsScalar)) {
        return this.ScalarOps_.Multiply(lhsScalar, rhs).Wrap();
      }
    }

    return lhs!.Multiply(rhs!);
  }

  public override IColorValue? MixWithScalar(IColorValue? lhs,
                                             IColorValue? rhs,
                                             IScalarValue? mixAmount) {
    if (lhs.IsZero()) {
      lhs = null;
    }

    if (rhs.IsZero()) {
      rhs = null;
    }

    if (lhs == null && rhs == null) {
      return null;
    }

    // No progress, so return starting value
    if (this.ScalarOps_.IsZero(mixAmount)) {
      return lhs;
    }

    // Fully progressed, return final value
    if (mixAmount?.Equals(this.ScalarOps_.One) ?? false) {
      return rhs;
    }

    // Some combination
    lhs = this.MultiplyWithScalar(
        lhs,
        this.ScalarOps_.Subtract(this.ScalarOps_.One, mixAmount));
    rhs = this.MultiplyWithScalar(rhs, mixAmount);

    return this.Add(lhs, rhs);
  }
}

public class ScalarFixedFunctionOps<TIdentifier>(
    IFixedFunctionEquations<TIdentifier> equations)
    : BFixedFunctionOps<IScalarValue, IScalarConstant>,
      IScalarOps
    where TIdentifier : notnull {
  private readonly IScalarValue
      scMinusOne_ = equations.CreateScalarConstant(-1);

  public override IScalarConstant Zero { get; }
    = equations.CreateScalarConstant(0);

  public override IScalarConstant Half { get; }
    = equations.CreateScalarConstant(.5f);

  public override IScalarConstant One { get; }
    = equations.CreateScalarConstant(1);

  public override IScalarValue? AddWithScalar(
      IScalarValue? lhs,
      IScalarValue? rhs)
    => this.Add(lhs, rhs);

  public override IScalarValue? Add(IScalarValue? lhs, IScalarValue? rhs) {
    if (!FixedFunctionConstants.REMOVE_ZERO_TERMS) {
      lhs ??= this.Zero;
      rhs ??= this.Zero;
    } else {
      var lhsIsZero = lhs.IsZero();
      var rhsIsZero = rhs.IsZero();

      if (lhsIsZero && rhsIsZero) {
        return null;
      }

      if (lhsIsZero) {
        return rhs;
      }

      if (rhsIsZero) {
        return lhs;
      }

      if (lhs.IsConstant(out var lhsConstant) &&
          rhs.IsConstant(out var rhsConstant)) {
        return new ScalarConstant(lhsConstant + rhsConstant);
      }
    }

    return lhs!.Add(rhs!);
  }

  public override IScalarValue? Subtract(IScalarValue? lhs,
                                         IScalarValue? rhs) {
    if (!FixedFunctionConstants.REMOVE_ZERO_TERMS) {
      lhs ??= this.Zero;
      rhs ??= this.Zero;
    } else {
      var lhsIsZero = lhs.IsZero();
      var rhsIsZero = rhs.IsZero();

      if ((lhsIsZero && rhsIsZero) || (lhs?.Equals(rhs) ?? false)) {
        return null;
      }

      if (lhsIsZero) {
        return rhs?.Multiply(this.scMinusOne_);
      }

      if (rhsIsZero) {
        return lhs;
      }

      if (lhs.IsConstant(out var lhsConstant) &&
          rhs.IsConstant(out var rhsConstant)) {
        return new ScalarConstant(lhsConstant - rhsConstant);
      }
    }

    return lhs!.Subtract(rhs!);
  }

  public override IScalarValue? MultiplyWithScalar(
      IScalarValue? lhs,
      IScalarValue? rhs)
    => this.Multiply(lhs, rhs);

  public override IScalarValue? Multiply(IScalarValue? lhs,
                                         IScalarValue? rhs) {
    if (!FixedFunctionConstants.REMOVE_ZERO_TERMS) {
      lhs ??= this.Zero;
      rhs ??= this.Zero;
    } else {
      if (lhs.IsZero() || rhs.IsZero()) {
        return null;
      }

      var lhsIsOne = lhs?.Equals(this.One) ?? false;
      var rhsIsOne = rhs?.Equals(this.One) ?? false;

      if (lhsIsOne && rhsIsOne) {
        return this.One;
      }

      if (lhsIsOne) {
        return rhs;
      }

      if (rhsIsOne) {
        return lhs;
      }

      if (lhs.IsConstant(out var lhsConstant) &&
          rhs.IsConstant(out var rhsConstant)) {
        return new ScalarConstant(lhsConstant * rhsConstant);
      }
    }

    return lhs!.Multiply(rhs!);
  }

  public override IScalarValue? MixWithScalar(IScalarValue? lhs,
                                              IScalarValue? rhs,
                                              IScalarValue? mixAmount) {
    lhs = this.Multiply(lhs, this.Subtract(this.One, mixAmount));
    rhs = this.Multiply(rhs, mixAmount);

    return this.Add(lhs, rhs);
  }
}