using System.Linq;

using fin.util.strings;

using NUnit.Framework;

using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace fin.language.equations.fixedFunction;

public class FixedFunctionEquationsTests {
  [Test]
  public void TestInOutScalars() {
    var equations = new FixedFunctionEquations<string>();

    var fooIn = equations.CreateOrGetScalarInput("fooIn");
    var fooOut = equations.CreateScalarOutput("fooOut", fooIn);

    this.AssertEquals_(equations,
                       "Scalar inputs:",
                       "fooIn",
                       "",
                       "Color inputs:",
                       "",
                       "",
                       "Scalar outputs:",
                       "fooOut: {fooIn}",
                       "",
                       "Color outputs:"
    );
  }


  [Test]
  public void TestInOut() {
    var equations = new FixedFunctionEquations<string>();

    var scIn = equations.CreateOrGetScalarInput("scIn");
    var scOut = equations.CreateScalarOutput("scOut", scIn);

    var colSc =
        equations.CreateColorOutput("colSc", equations.CreateColor(scIn));

    this.AssertEquals_(equations,
                       "Scalar inputs:",
                       "scIn",
                       "",
                       "Color inputs:",
                       "",
                       "",
                       "Scalar outputs:",
                       "scOut: {scIn}",
                       "",
                       "Color outputs:",
                       "colSc: i<{scIn}>"
    );
  }

  [Test]
  public void TestColorMath() {
    var equations = new FixedFunctionEquations<string>();

    var colRgb1 =
        equations.CreateOrGetColorInput("colRgb1");
    var colRgb2 =
        equations.CreateOrGetColorInput("colRgb2");
    var colI1 =
        equations.CreateOrGetColorInput("colI1");
    var colI2 =
        equations.CreateOrGetColorInput("colI2");

    var colOutput =
        equations.CreateColorOutput("colOutput",
                                    (colRgb1.Add(colRgb2)).Divide(
                                        colI1.Subtract(colI2)));

    this.AssertEquals_(equations,
                       "Scalar inputs:",
                       "",
                       "Color inputs:",
                       "colRgb1",
                       "colRgb2",
                       "colI1",
                       "colI2",
                       "",
                       "",
                       "Scalar outputs:",
                       "",
                       "Color outputs:",
                       "colOutput: (<colRgb1> + <colRgb2>)/(<colI1> + i<-1>*<colI2>)"
    );
  }


  [Test]
  public void TestColorSwizzleOut() {
    var equations = new FixedFunctionEquations<string>();

    var colRgb1 =
        equations.CreateOrGetColorInput("colRgb1");
    var colRgb2 =
        equations.CreateOrGetColorInput("colRgb2");
    var colI1 =
        equations.CreateOrGetColorInput("colI1");
    var colI2 =
        equations.CreateOrGetColorInput("colI2");

    var scR1 = equations.CreateScalarOutput("scR1", colI1.R);
    var scR2 = equations.CreateScalarOutput("scR2", colRgb1.R);

    var colOutputExp = (colRgb1.Add(colRgb2)).Divide(
        colI1.Subtract(colI2));
    var colOutput =
        equations.CreateColorOutput("colOutput",
                                    colOutputExp);

    var scBExp = equations.CreateScalarOutput("scBExp", colOutputExp.B);
    var scBVar = equations.CreateScalarOutput("scBVar", colOutput.B);

    this.AssertEquals_(equations,
                       "Scalar inputs:",
                       "",
                       "Color inputs:",
                       "colRgb1",
                       "colRgb2",
                       "colI1",
                       "colI2",
                       "",
                       "",
                       "Scalar outputs:",
                       "scR1: <colI1>.R",
                       "scR2: <colRgb1>.R",
                       "scBExp: (<colRgb1>.B + <colRgb2>.B)/(<colI1>.B + -1*<colI2>.B)",
                       "scBVar: <colOutput>.B",
                       "",
                       "Color outputs:",
                       "colOutput: (<colRgb1> + <colRgb2>)/(<colI1> + i<-1>*<colI2>)"
    );
  }


  [Test]
  public void TestColorSwizzleIn() {
    var equations = new FixedFunctionEquations<string>();

    var colRgb =
        equations.CreateOrGetColorInput("colRgb");
    var colGbr =
        equations.CreateColorOutput("colGbr",
                                    equations.CreateColor(
                                        colRgb.G,
                                        colRgb.B,
                                        colRgb.R));

    this.AssertEquals_(equations,
                       "Scalar inputs:",
                       "",
                       "Color inputs:",
                       "colRgb",
                       "",
                       "",
                       "Scalar outputs:",
                       "",
                       "Color outputs:",
                       "colGbr: rgb<<colRgb>.G,<colRgb>.B,<colRgb>.R>"
    );
  }

  private void AssertEquals_<TIdentifier>(
      IFixedFunctionEquations<TIdentifier> equations,
      params string[] expectedLines) {
    var actualText =
        new FixedFunctionEquationsPrettyPrinter<TIdentifier>().Print(
            equations);

    var actualLines = actualText.SplitNewlines();
    actualLines = actualLines.Take(actualLines.Length - 1).ToArray();

    var expectedText = string.Join('\n', expectedLines);
    actualText = string.Join('\n', actualLines);

    Assert.AreEqual(expectedText, actualText);
  }
}