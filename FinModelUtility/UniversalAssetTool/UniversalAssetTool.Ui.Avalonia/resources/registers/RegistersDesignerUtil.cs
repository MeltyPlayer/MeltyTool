using fin.language.equations.fixedFunction;

namespace uni.ui.avalonia.resources.registers;

public static class RegistersDesignerUtil {
  public static IFixedFunctionRegisters CreateStubRegisters() {
    var registers = new FixedFunctionRegisters();

    registers.GetOrCreateColorRegister(
        "color constant a",
        new ColorConstant(.3f, .4f, .5f));
    registers.GetOrCreateScalarRegister(
        "scalar constant 1",
        new ScalarConstant(.3f));

    return registers;
  }
}