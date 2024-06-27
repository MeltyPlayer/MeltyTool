namespace fin.language.equations.fixedFunction; 
/*
public class FixedFunctionEquationsGlslPrinterTest {
  [Test]
  public void TestScalarColor() {
    var equations = new FixedFunctionEquations<FixedFunctionSource>();

    var sc = equations.CreateScalarConstant(1);
    var colSc =
        equations.CreateColorOutput(FixedFunctionSource.OUTPUT_COLOR, equations.CreateColor(sc));

    this.AssertEquals_(equations,
                       @"#version 130

out vec4 fragColor;

void main() {
fragColor = rgb(1.0,1.0,1.0);

if (fragColor.a < .95) {
  discard;
}
}"
    );
  }

  [Test]
  public void TestColorSwizzleIn() {
    var equations = new FixedFunctionEquations<FixedFunctionSource>();

    var colRgb =
        equations.CreateColorInput(FixedFunctionSource.VERTEX_COLOR_0,
                                   equations.CreateColorConstant(1, 2, 3));
    var colGbr =
        equations.CreateColorOutput(FixedFunctionSource.OUTPUT_COLOR,
                                    equations.CreateColor(
                                        colRgb.G,
                                        colRgb.B,
                                        colRgb.R));

    this.AssertEquals_(equations,
                       "Scalar inputs:",
                       "",
                       "Color inputs:",
                       "colRgb: rgb<1,2,3>",
                       "",
                       "",
                       "Scalar outputs:",
                       "",
                       "Color outputs:",
                       "colGbr: rgb<<colRgb>.G,<colRgb>.B,<colRgb>.R>"
    );
  }

  private void AssertEquals_(
      IFixedFunctionEquations<FixedFunctionSource> equations,
      params string[] expectedLines) {
    var actualText =
        new FixedFunctionEquationsGlslPrinter(null).Print(
            null);

    var actualLines = StringUtil.SplitNewlines(actualText);
    actualLines = actualLines.Take(actualLines.Length - 1).ToArray();

    var expectedText = string.Join('\n', expectedLines);
    actualText = string.Join('\n', actualLines);

    Assert.AreEqual(expectedText, actualText);
  }
}*/