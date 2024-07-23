using System;
using System.Collections.Generic;
using System.Linq;

using fin.util.asserts;
using fin.util.lists;

namespace fin.language.equations.fixedFunction;

// TODO: Optimize this.
public partial class FixedFunctionEquations<TIdentifier> {
  private readonly Dictionary<(IScalarValue, IScalarValue, IScalarValue),
          ColorWrapper>
      scalarValueColorConstants_ = new();

  private readonly Dictionary<(double, double, double), IColorConstant>
      doubleColorConstants_ = new();

  private readonly Dictionary<TIdentifier, IColorInput<TIdentifier>>
      colorInputs_ = new();

  private readonly Dictionary<TIdentifier, IColorOutput<TIdentifier>>
      colorOutputs_ = new();


  public IReadOnlyDictionary<TIdentifier, IColorInput<TIdentifier>>
      ColorInputs => this.colorInputs_;

  public IReadOnlyDictionary<TIdentifier, IColorOutput<TIdentifier>>
      ColorOutputs => this.colorOutputs_;

  public IColorConstant CreateColorConstant(
      double r,
      double g,
      double b) {
    var key = (r, g, b);
    if (this.doubleColorConstants_.TryGetValue(
            key,
            out var colorConstant)) {
      return colorConstant;
    }

    return this.doubleColorConstants_[key] = new ColorConstant(r, g, b);
  }

  public IColorConstant CreateColorConstant(
      double intensity) {
    var key = (intensity, intensity, intensity);
    if (this.doubleColorConstants_.TryGetValue(
            key,
            out var colorConstant)) {
      return colorConstant;
    }

    return this.doubleColorConstants_[key] = new ColorConstant(intensity);
  }

  public IColorFactor CreateColor(
      IScalarValue r,
      IScalarValue g,
      IScalarValue b) {
    var key = (r, g, b);
    if (this.scalarValueColorConstants_.TryGetValue(
            key,
            out var colorConstant)) {
      return colorConstant;
    }

    return this.scalarValueColorConstants_[key] = new ColorWrapper(r, g, b);
  }

  public IColorFactor CreateColor(
      IScalarValue intensity) {
    var key = (intensity, intensity, intensity);
    if (this.scalarValueColorConstants_.TryGetValue(
            key,
            out var colorConstant)) {
      return colorConstant;
    }

    return this.scalarValueColorConstants_[key] = new ColorWrapper(intensity);
  }

  public IColorInput<TIdentifier> CreateOrGetColorInput(
      TIdentifier identifier) {
    Asserts.False(this.colorOutputs_.ContainsKey(identifier));

    if (!this.colorInputs_.TryGetValue(identifier, out var input)) {
      input = new ColorInput(identifier);
      this.colorInputs_[identifier] = input;
    }

    return input;
  }

  public IColorOutput<TIdentifier> CreateColorOutput(
      TIdentifier identifier,
      IColorValue value) {
    Asserts.False(this.colorInputs_.ContainsKey(identifier));
    Asserts.False(this.colorOutputs_.ContainsKey(identifier));

    var output = new ColorOutput(identifier, value);
    this.colorOutputs_[identifier] = output;
    return output;
  }


  private class ColorInput(TIdentifier identifier)
      : BColorValue, IColorInput<TIdentifier> {
    public TIdentifier Identifier { get; } = identifier;

    public override IScalarValue? Intensity
      => throw new NotSupportedException();

    public override IScalarValue R
      => new ColorNamedValueSwizzle(this, ColorSwizzle.R);

    public override IScalarValue G
      => new ColorNamedValueSwizzle(this, ColorSwizzle.G);

    public override IScalarValue B
      => new ColorNamedValueSwizzle(this, ColorSwizzle.B);
  }

  private class ColorOutput(TIdentifier identifier, IColorValue value)
      : BColorValue, IColorOutput<TIdentifier> {
    public TIdentifier Identifier { get; } = identifier;
    public IColorValue ColorValue { get; } = value;

    public override IScalarValue? Intensity => null;

    public override IScalarValue R
      => new ColorNamedValueSwizzle(this, ColorSwizzle.R);

    public override IScalarValue G
      => new ColorNamedValueSwizzle(this, ColorSwizzle.G);

    public override IScalarValue B
      => new ColorNamedValueSwizzle(this, ColorSwizzle.B);
  }


  private class ColorNamedValueSwizzle(
      IColorIdentifiedValue<TIdentifier> source,
      ColorSwizzle swizzleType)
      : BScalarValue,
        IColorNamedValueSwizzle<
            TIdentifier> {
    public IColorIdentifiedValue<TIdentifier> Source { get; } = source;
    public ColorSwizzle SwizzleType { get; } = swizzleType;
  }
}

public class ColorValueSwizzle(IColorValue source, ColorSwizzle swizzleType)
    : BScalarValue, IColorValueSwizzle {
  public IColorValue Source { get; } = source;
  public ColorSwizzle SwizzleType { get; } = swizzleType;
}

public class ColorExpression(IReadOnlyList<IColorValue> terms)
    : BColorValue, IColorExpression {
  public IReadOnlyList<IColorValue> Terms { get; } = terms;

  public IColorExpression Add(
      IColorValue term1,
      params IColorValue[] terms)
    => new ColorExpression(
        ListUtil.ReadonlyConcat(this.Terms, [term1], terms));

  public IColorExpression Subtract(
      IColorValue term1,
      params IColorValue[] terms)
    => new ColorExpression(
        ListUtil.ReadonlyConcat(this.Terms,
                                this.NegateTerms(term1, terms)));

  public IColorExpression Add(
      IScalarValue term1,
      params IScalarValue[] terms)
    => new ColorExpression(
        ListUtil.ReadonlyConcat(this.Terms,
                                this.ToColorValues(term1, terms)));

  public IColorExpression Subtract(
      IScalarValue term1,
      params IScalarValue[] terms)
    => new ColorExpression(
        ListUtil.ReadonlyConcat(this.Terms,
                                this.ToColorValues(
                                    this.NegateTerms(term1, terms))));

  public override IScalarValue? Intensity {
    get {
      var numeratorAs =
          this.Terms.Select(factor => factor.Intensity)
              .ToArray();

      if (numeratorAs.Any(a => a == null)) {
        return null;
      }

      return new ScalarExpression(numeratorAs.Select(a => a!).ToArray());
    }
  }

  public override IScalarValue R
    => new ScalarExpression(this.Terms.Select(factor => factor.R)
                                .ToArray());

  public override IScalarValue G
    => new ScalarExpression(this.Terms.Select(factor => factor.G)
                                .ToArray());

  public override IScalarValue B
    => new ScalarExpression(this.Terms.Select(factor => factor.B)
                                .ToArray());
}

public class ColorTerm : BColorValue, IColorTerm {
  public ColorTerm(IReadOnlyList<IColorValue> numeratorFactors) {
    this.NumeratorFactors = numeratorFactors;
  }

  public ColorTerm(
      IReadOnlyList<IColorValue> numeratorFactors,
      IReadOnlyList<IColorValue> denominatorFactors) {
    this.NumeratorFactors = numeratorFactors;
    this.DenominatorFactors = denominatorFactors;
  }

  public IReadOnlyList<IColorValue> NumeratorFactors { get; }
  public IReadOnlyList<IColorValue>? DenominatorFactors { get; }

  public IColorTerm Multiply(
      IColorValue factor1,
      params IColorValue[] factors)
    => new ColorTerm(ListUtil.ReadonlyConcat(
                         this.NumeratorFactors,
                         ListUtil.ReadonlyFrom(factor1, factors)));

  public IColorTerm Divide(
      IColorValue factor1,
      params IColorValue[] factors)
    => new ColorTerm(this.NumeratorFactors,
                     ListUtil.ReadonlyConcat(
                         this.DenominatorFactors,
                         ListUtil.ReadonlyFrom(factor1, factors)));

  public IColorTerm Multiply(
      IScalarValue factor1,
      params IScalarValue[] factors)
    => new ColorTerm(ListUtil.ReadonlyConcat(
                         this.NumeratorFactors,
                         this.ToColorValues(factor1, factors)));

  public IColorTerm Divide(
      IScalarValue factor1,
      params IScalarValue[] factors)
    => new ColorTerm(this.NumeratorFactors,
                     ListUtil.ReadonlyConcat(
                         this.DenominatorFactors,
                         this.ToColorValues(factor1, factors)));

  public override IScalarValue? Intensity {
    get {
      var numeratorAs =
          this.NumeratorFactors.Select(factor => factor.Intensity)
              .ToArray();
      var denominatorAs =
          this.DenominatorFactors?.Select(factor => factor.Intensity)
              .ToArray();

      if (numeratorAs.Any(a => a == null) ||
          (denominatorAs?.Any(a => a == null) ?? false)) {
        return null;
      }

      return new ScalarTerm(
          numeratorAs.Select(a => a!).ToArray(),
          denominatorAs?.Select(a => a!).ToArray());
    }
  }

  public override IScalarValue R
    => new ScalarTerm(
        this.NumeratorFactors.Select(factor => factor.R).ToArray(),
        this.DenominatorFactors?.Select(factor => factor.R)
            .ToArray());

  public override IScalarValue G
    => new ScalarTerm(
        this.NumeratorFactors.Select(factor => factor.G).ToArray(),
        this.DenominatorFactors?.Select(factor => factor.G)
            .ToArray());

  public override IScalarValue B
    => new ScalarTerm(
        this.NumeratorFactors.Select(factor => factor.B).ToArray(),
        this.DenominatorFactors?.Select(factor => factor.B)
            .ToArray());
}

public static class FixedFunctionUtils {
  public const float TOLERANCE = 1 / 255f;

  public static bool CompareColorConstants(double lhsR,
                                           double lhsG,
                                           double lhsB,
                                           double? lhsIntensity,
                                           double rhsR,
                                           double rhsG,
                                           double rhsB,
                                           double? rhsIntensity) {
    if (CompareScalarConstants(lhsIntensity, rhsIntensity)) {
      return true;
    }

    if (lhsIntensity == null && rhsIntensity == null) {
      return Math.Abs(lhsR - rhsR) < TOLERANCE &&
             Math.Abs(lhsG - rhsG) < TOLERANCE &&
             Math.Abs(lhsB - rhsB) < TOLERANCE;
    }

    return false;
  }

  public static bool CompareScalarConstants(double? lhsIntensity,
                                            double? rhsIntensity) {
    if (lhsIntensity != null && rhsIntensity != null) {
      return Math.Abs(lhsIntensity.Value - rhsIntensity.Value) < TOLERANCE;
    }

    return false;
  }
}

public class ColorConstant : BColorValue, IColorConstant {
  public static readonly ColorConstant NEGATIVE_ONE = new(-1);

  public ColorConstant(double r, double g, double b) {
    if (Math.Abs(r - g) < FixedFunctionUtils.TOLERANCE &&
        Math.Abs(r - b) < FixedFunctionUtils.TOLERANCE) {
      this.IntensityValue = r;
      this.Intensity = new ScalarConstant(r);
    }

    this.RValue = r;
    this.GValue = g;
    this.BValue = b;

    this.R = new ScalarConstant(r);
    this.G = new ScalarConstant(g);
    this.B = new ScalarConstant(b);
  }

  public ColorConstant(double intensity) {
    this.IntensityValue = intensity;
    this.RValue = intensity;
    this.GValue = intensity;
    this.BValue = intensity;

    this.Intensity = new ScalarConstant(intensity);
    this.R = new ScalarConstant(intensity);
    this.G = new ScalarConstant(intensity);
    this.B = new ScalarConstant(intensity);
  }


  public double? IntensityValue { get; }
  public double RValue { get; }
  public double GValue { get; }
  public double BValue { get; }

  public override IScalarValue? Intensity { get; }
  public override IScalarValue R { get; }
  public override IScalarValue G { get; }
  public override IScalarValue B { get; }

  public override string ToString() => $"<{R}, {G}, {B}>";

  public override bool Equals(object? other) {
    if (Object.ReferenceEquals(this, other)) {
      return true;
    }

    if (other is IScalarConstant otherScalar) {
      return FixedFunctionUtils.CompareScalarConstants(
          this.IntensityValue,
          otherScalar.Value);
    }

    if (other is IColorConstant otherColor) {
      return FixedFunctionUtils.CompareColorConstants(
          this.RValue,
          this.GValue,
          this.BValue,
          this.IntensityValue,
          otherColor.RValue,
          otherColor.GValue,
          otherColor.BValue,
          otherColor.IntensityValue);
    }

    if (other is ColorWrapper colorWrapper) {
      if (colorWrapper.Intensity is IScalarConstant intensityWrapper) {
        return FixedFunctionUtils.CompareScalarConstants(
            this.IntensityValue,
            intensityWrapper.Value);
      }
    }

    return false;
  }
}

public class ColorWrapper(
    IScalarValue r,
    IScalarValue g,
    IScalarValue b)
    : BColorValue, IColorFactor {
  public ColorWrapper(IScalarValue intensity) : this(intensity, intensity, intensity) {
    this.Intensity = intensity;
  }

  public override IScalarValue? Intensity { get; }
  public override IScalarValue R { get; } = r;
  public override IScalarValue G { get; } = g;
  public override IScalarValue B { get; } = b;

  public override string ToString()
    => this.Intensity != null ? $"{this.Intensity}" : $"<{R}, {G}, {B}>";

  public override bool Equals(object? other) {
    if (Object.ReferenceEquals(this, other)) {
      return true;
    }

    if (other is IScalarConstant otherScalar) {
      return FixedFunctionUtils.CompareScalarConstants(
          (this.Intensity as IScalarConstant)?.Value,
          otherScalar.Value);
    }

    return false;
  }
}

public class ColorValueTernaryOperator : BColorValue,
                                         IColorValueTernaryOperator {
  public BoolComparisonType ComparisonType { get; set; }
  public IScalarValue Lhs { get; set; }
  public IScalarValue Rhs { get; set; }
  public IColorValue TrueValue { get; set; }
  public IColorValue FalseValue { get; set; }

  public override IScalarValue? Intensity { get; }

  public override IScalarValue R
    => new ColorValueSwizzle(this, ColorSwizzle.R);

  public override IScalarValue G
    => new ColorValueSwizzle(this, ColorSwizzle.G);

  public override IScalarValue B
    => new ColorValueSwizzle(this, ColorSwizzle.B);
}