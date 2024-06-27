using System.IO;
using System.Text;

using fin.util.asserts;

namespace fin.language.equations.fixedFunction;

public class FixedFunctionEquationsPrettyPrinter<TIdentifier> {
  public string Print(IFixedFunctionEquations<TIdentifier> equations) {
    var sb = new StringBuilder();

    using var os = new StringWriter(sb);
    this.Print(os, equations);

    return sb.ToString();
  }

  public string Print(IColorValue colorValue) {
    var sb = new StringBuilder();

    using var os = new StringWriter(sb);
    this.PrintColorValue_(os, colorValue);

    return sb.ToString();
  }

  public string Print(IScalarValue scalarValue) {
    var sb = new StringBuilder();

    using var os = new StringWriter(sb);
    this.PrintScalarValue_(os, scalarValue);

    return sb.ToString();
  }

  public void Print(
      StringWriter os,
      IFixedFunctionEquations<TIdentifier> equations) {
    os.WriteLine("Scalar inputs:");
    foreach (var (id, _) in equations.ScalarInputs) {
      os.WriteLine(id);
    }

    os.WriteLine();
    os.WriteLine("Color inputs:");
    foreach (var (id, _) in equations.ColorInputs) {
      os.WriteLine(id);
    }


    os.WriteLine();
    os.WriteLine();
    os.WriteLine("Scalar outputs:");
    foreach (var (id, output) in equations.ScalarOutputs) {
      os.Write(id);
      os.Write(": ");
      this.PrintScalarValue_(os, output.ScalarValue);
      os.Write("\n");
    }

    os.WriteLine();
    os.WriteLine("Color outputs:");
    foreach (var (id, output) in equations.ColorOutputs) {
      os.Write(id);
      os.Write(": ");
      this.PrintColorValue_(os, output.ColorValue);
      os.Write("\n");
    }
  }


  private void PrintScalarValue_(
      StringWriter os,
      IScalarValue value,
      bool wrapExpressions = false) {
    if (value is IScalarExpression expression) {
      if (wrapExpressions) {
        os.Write("(");
      }
      this.PrintScalarExpression_(os, expression);
      if (wrapExpressions) {
        os.Write(")");
      }
    } else if (value is IScalarTerm term) {
      this.PrintScalarTerm_(os, term);
    } else if (value is IScalarFactor factor) {
      this.PrintScalarFactor_(os, factor);
    } else {
      Asserts.Fail("Unsupported value type!");
    }
  }

  private void PrintScalarExpression_(
      StringWriter os,
      IScalarExpression expression) {
    var terms = expression.Terms;

    for (var i = 0; i < terms.Count; ++i) {
      var term = terms[i];

      if (i > 0) {
        os.Write(" + ");
      }
      this.PrintScalarValue_(os, term);
    }
  }

  private void PrintScalarTerm_(
      StringWriter os,
      IScalarTerm scalarTerm) {
    var numerators = scalarTerm.NumeratorFactors;
    var denominators = scalarTerm.DenominatorFactors;

    if (numerators.Count > 0) {
      for (var i = 0; i < numerators.Count; ++i) {
        var numerator = numerators[i];

        if (i > 0) {
          os.Write("*");
        }

        this.PrintScalarValue_(os, numerator, true);
      }
    } else {
      os.Write(1);
    }

    if (denominators != null) {
      for (var i = 0; i < denominators.Count; ++i) {
        var denominator = denominators[i];

        os.Write("/");

        this.PrintScalarValue_(os, denominator, true);
      }
    }
  }

  private void PrintScalarFactor_(
      StringWriter os,
      IScalarFactor factor) {
    if (factor is IScalarIdentifiedValue<TIdentifier> identifiedValue) {
      this.PrintScalarIdentifiedValue_(os, identifiedValue);
    } else if (factor is IScalarNamedValue namedValue) {
      this.PrintScalarNamedValue_(os, namedValue);
    } else if (factor is IScalarConstant constant) {
      this.PrintScalarConstant_(os, constant);
    } else if (factor is IColorNamedValueSwizzle<TIdentifier> namedSwizzle) {
      this.PrintColorNamedValueSwizzle_(os, namedSwizzle);
    } else if (factor is IColorValueSwizzle swizzle) {
      this.PrintColorValueSwizzle_(os, swizzle);
    } else {
      Asserts.Fail("Unsupported factor type!");
    }
  }

  private void PrintScalarIdentifiedValue_(
      StringWriter os,
      IScalarIdentifiedValue<TIdentifier> identifiedValue)
    => os.Write("{" + identifiedValue.Identifier + "}");

  private void PrintScalarNamedValue_(
      StringWriter os,
      IScalarNamedValue namedValue)
    => os.Write("{" + namedValue.Name + "}");

  private void PrintScalarConstant_(
      StringWriter os,
      IScalarConstant constant)
    => os.Write(constant.Value);


  private enum WrapType {
    NEVER,
    EXPRESSIONS,
    ALWAYS
  }

  private void PrintColorValue_(
      StringWriter os,
      IColorValue value,
      WrapType wrapType = WrapType.NEVER) {
    if (value is IColorExpression expression) {
      var wrapExpressions =
          wrapType is WrapType.EXPRESSIONS or WrapType.ALWAYS;
      if (wrapExpressions) {
        os.Write("(");
      }
      this.PrintColorExpression_(os, expression);
      if (wrapExpressions) {
        os.Write(")");
      }
    } else if (value is IColorTerm term) {
      var wrapTerms = wrapType == WrapType.ALWAYS;
      if (wrapTerms) {
        os.Write("(");
      }
      this.PrintColorTerm_(os, term);
      if (wrapTerms) {
        os.Write(")");
      }
    } else if (value is IColorFactor factor) {
      var wrapFactors = wrapType == WrapType.ALWAYS;
      if (wrapFactors) {
        os.Write("(");
      }
      this.PrintColorFactor_(os, factor);
      if (wrapFactors) {
        os.Write(")");
      }
    } else if (value is IColorValueTernaryOperator ternaryOperator) {
      this.PrintColorTernaryOperator_(os, ternaryOperator);
    } else {
      Asserts.Fail("Unsupported value type!");
    }
  }

  private void PrintColorExpression_(
      StringWriter os,
      IColorExpression expression) {
    var terms = expression.Terms;

    for (var i = 0; i < terms.Count; ++i) {
      var term = terms[i];

      if (i > 0) {
        os.Write(" + ");
      }
      this.PrintColorValue_(os, term);
    }
  }

  private void PrintColorTerm_(
      StringWriter os,
      IColorTerm scalarTerm) {
    var numerators = scalarTerm.NumeratorFactors;
    var denominators = scalarTerm.DenominatorFactors;

    if (numerators.Count > 0) {
      for (var i = 0; i < numerators.Count; ++i) {
        var numerator = numerators[i];

        if (i > 0) {
          os.Write("*");
        }

        this.PrintColorValue_(os, numerator, WrapType.EXPRESSIONS);
      }
    } else {
      os.Write(1);
    }

    if (denominators != null) {
      for (var i = 0; i < denominators.Count; ++i) {
        var denominator = denominators[i];

        os.Write("/");

        this.PrintColorValue_(os, denominator, WrapType.EXPRESSIONS);
      }
    }
  }

  private void PrintColorFactor_(
      StringWriter os,
      IColorFactor factor) {
    if (factor is IColorIdentifiedValue<TIdentifier> identifiedValue) {
      this.PrintColorIdentifiedValue_(os, identifiedValue);
    } else if (factor is IColorNamedValue namedValue) {
      this.PrintColorNamedValue_(os, namedValue);
    } else {
      var useIntensity = factor.Intensity != null;

      if (!useIntensity) {
        os.Write("rgb<");
        this.PrintScalarValue_(os, factor.R);
        os.Write(",");
        this.PrintScalarValue_(os, factor.G);
        os.Write(",");
        this.PrintScalarValue_(os, factor.B);
        os.Write(">");
      } else {
        os.Write("i<");
        this.PrintScalarValue_(os, factor.Intensity!);
        os.Write(">");
      }
    }
  }

  private void PrintColorIdentifiedValue_(
      StringWriter os,
      IColorIdentifiedValue<TIdentifier> identifiedValue)
    => os.Write("<" + identifiedValue.Identifier + ">");

  private void PrintColorNamedValue_(
      StringWriter os,
      IColorNamedValue namedValue)
    => os.Write("<" + namedValue.Name + ">");

  private void PrintColorTernaryOperator_(
      StringWriter os,
      IColorValueTernaryOperator ternaryOperator) {
    os.Write('(');
    this.PrintScalarValue_(os, ternaryOperator.Lhs);
    os.Write(ternaryOperator.ComparisonType switch {
        BoolComparisonType.EQUAL_TO                 => " == ",
        BoolComparisonType.NOT_EQUAL_TO             => " != ",
        BoolComparisonType.GREATER_THAN             => " > ",
        BoolComparisonType.GREATER_THAN_OR_EQUAL_TO => " >= ",
        BoolComparisonType.LESS_THAN                => " < ",
        BoolComparisonType.LESS_THAN_OR_EQUAL_TO    => " <= ",
    });
    this.PrintScalarValue_(os, ternaryOperator.Rhs);
    os.Write(" ? ");
    this.PrintColorValue_(os, ternaryOperator.TrueValue);
    os.Write(" : ");
    this.PrintColorValue_(os, ternaryOperator.FalseValue);
    os.Write(')');
  }

  private void PrintColorNamedValueSwizzle_(
      StringWriter os,
      IColorNamedValueSwizzle<TIdentifier> swizzle) {
    this.PrintColorIdentifiedValue_(os, swizzle.Source);
    os.Write(".");
    os.Write(swizzle.SwizzleType);
  }

  private void PrintColorValueSwizzle_(
      StringWriter os,
      IColorValueSwizzle swizzle) {
    this.PrintColorValue_(os, swizzle.Source, WrapType.ALWAYS);
    os.Write(".");
    os.Write(swizzle.SwizzleType);
  }
}