using Avalonia.Headless.NUnit;

using fin.language.equations.fixedFunction;
using fin.util.asserts;

using uni.ui.avalonia.helpers;

namespace uni.ui.avalonia.resources.registers;

public class RegistersPanelTests {
  [AvaloniaTest]
  public void TestDisplaysExpectedListsOfRegisters() {
    var registers = new FixedFunctionRegisters();
    registers.GetOrCreateColorRegister("foo", ColorConstant.ONE);
    registers.GetOrCreateColorRegister("bar", ColorConstant.ONE);
    registers.GetOrCreateScalarRegister("item 1", ScalarConstant.ONE);
    registers.GetOrCreateScalarRegister("item 10", ScalarConstant.ONE);
    registers.GetOrCreateScalarRegister("item 2", ScalarConstant.ONE);

    var registersPanel = RegistersPanel.Bootstrap(new RegistersPanelViewModel {
        Registers = registers
    });

    Asserts.SequenceEqual(
        ["ambientLightColor", "bar", "foo"],
        registersPanel.GetColorRegisterNames());
    Asserts.SequenceEqual(
        ["ambientLightAmount", "item 1", "item 2", "item 10"],
        registersPanel.GetScalarRegisterNames());
  }
}