using fin.language.equations.fixedFunction;

namespace uni.ui.avalonia.registers;

public static class RegistersDesignerUtil {
  public static IFixedFunctionRegisters CreateStubRegisters() {
    var registers = new FixedFunctionRegisters();

    registers.GetOrCreateColorRegister(
        "color constant a",
        new ColorConstant(.3, .4, .5));
    registers.GetOrCreateScalarRegister(
        "scalar constant 1",
        new ScalarConstant(.3));

    return registers;
  }
}