namespace sm64.src {
  class AssemblyReader {
    public List<JAL_CALL> findJALsInFunction(uint RAMFunc, uint RAMtoROM) {
      List<JAL_CALL> calls = [];
      List<Instruction> inst = this.ReadFunction(RAMFunc, RAMtoROM);
      uint a0 = 0, a1 = 0, a2 = 0, a3 = 0;
      uint jal_addr = 0;
      bool addNextTime = false;
      for (int i = 0; i < inst.Count; i++) {
        bool addJAL = addNextTime;
        switch (inst[i].opCode) {
          case OPCODE.LUI:
            if (inst[i].gp_dest == GP_REGISTER.A0)
              a0 = (uint) (inst[i].immediate << 16);
            else if (inst[i].gp_dest == GP_REGISTER.A1)
              a1 = (uint) (inst[i].immediate << 16);
            else if (inst[i].gp_dest == GP_REGISTER.A2)
              a2 = (uint) (inst[i].immediate << 16);
            else if (inst[i].gp_dest == GP_REGISTER.A3)
              a3 = (uint) (inst[i].immediate << 16);
            break;
          case OPCODE.ADDIU:
            if (inst[i].gp_dest == GP_REGISTER.A0 &&
                inst[i].gp_1 == GP_REGISTER.A0)
              a0 += (uint) inst[i].immediate;
            else if (inst[i].gp_dest == GP_REGISTER.A1 &&
                     inst[i].gp_1 == GP_REGISTER.A1)
              a1 += (uint) inst[i].immediate;
            else if (inst[i].gp_dest == GP_REGISTER.A2 &&
                     inst[i].gp_1 == GP_REGISTER.A2)
              a2 += (uint) inst[i].immediate;
            else if (inst[i].gp_dest == GP_REGISTER.A3 &&
                     inst[i].gp_1 == GP_REGISTER.A3)
              a3 += (uint) inst[i].immediate;
            else if (inst[i].gp_dest == GP_REGISTER.A0 &&
                     inst[i].gp_1 == GP_REGISTER.R0)
              a0 = (uint) inst[i].immediate;
            else if (inst[i].gp_dest == GP_REGISTER.A1 &&
                     inst[i].gp_1 == GP_REGISTER.R0)
              a1 = (uint) inst[i].immediate;
            else if (inst[i].gp_dest == GP_REGISTER.A2 &&
                     inst[i].gp_1 == GP_REGISTER.R0)
              a2 = (uint) inst[i].immediate;
            else if (inst[i].gp_dest == GP_REGISTER.A3 &&
                     inst[i].gp_1 == GP_REGISTER.R0)
              a3 = (uint) inst[i].immediate;
            else {
              if (inst[i].gp_dest == GP_REGISTER.A0)
                a0 = (uint) (inst[i].immediate +
                             this.gp_register_values[(int) inst[i].gp_1]);
              else if (inst[i].gp_dest == GP_REGISTER.A1)
                a1 = (uint) (inst[i].immediate +
                             this.gp_register_values[(int) inst[i].gp_1]);
              else if (inst[i].gp_dest == GP_REGISTER.A2)
                a2 = (uint) (inst[i].immediate +
                             this.gp_register_values[(int) inst[i].gp_1]);
              else if (inst[i].gp_dest == GP_REGISTER.A3)
                a3 = (uint) (inst[i].immediate +
                             this.gp_register_values[(int) inst[i].gp_1]);
            }

            break;
          case OPCODE.ORI:
            if (inst[i].gp_dest == GP_REGISTER.A0 &&
                inst[i].gp_1 == GP_REGISTER.A0)
              a0 |= (ushort) inst[i].immediate;
            else if (inst[i].gp_dest == GP_REGISTER.A1 &&
                     inst[i].gp_1 == GP_REGISTER.A1)
              a1 |= (ushort) inst[i].immediate;
            else if (inst[i].gp_dest == GP_REGISTER.A2 &&
                     inst[i].gp_1 == GP_REGISTER.A2)
              a2 |= (ushort) inst[i].immediate;
            else if (inst[i].gp_dest == GP_REGISTER.A3 &&
                     inst[i].gp_1 == GP_REGISTER.A3)
              a3 |= (ushort) inst[i].immediate;
            else if (inst[i].gp_dest == GP_REGISTER.A0 &&
                     inst[i].gp_1 == GP_REGISTER.R0)
              a0 = (uint) inst[i].immediate;
            else if (inst[i].gp_dest == GP_REGISTER.A1 &&
                     inst[i].gp_1 == GP_REGISTER.R0)
              a1 = (uint) inst[i].immediate;
            else if (inst[i].gp_dest == GP_REGISTER.A2 &&
                     inst[i].gp_1 == GP_REGISTER.R0)
              a2 = (uint) inst[i].immediate;
            else if (inst[i].gp_dest == GP_REGISTER.A3 &&
                     inst[i].gp_1 == GP_REGISTER.R0)
              a3 = (uint) inst[i].immediate;
            else {
              if (inst[i].gp_dest == GP_REGISTER.A0) {
                uint immediate_unsigned = (uint) inst[i].immediate;
                a0 = (uint) (immediate_unsigned |
                             this.gp_register_values[(int) inst[i].gp_1]);
              } else if (inst[i].gp_dest == GP_REGISTER.A1) {
                uint immediate_unsigned = (uint) inst[i].immediate;
                a1 = (uint) (immediate_unsigned |
                             this.gp_register_values[(int) inst[i].gp_1]);
              } else if (inst[i].gp_dest == GP_REGISTER.A2) {
                uint immediate_unsigned = (uint) inst[i].immediate;
                a2 = (uint) (immediate_unsigned |
                             this.gp_register_values[(int) inst[i].gp_1]);
              } else if (inst[i].gp_dest == GP_REGISTER.A3) {
                uint immediate_unsigned = (uint) inst[i].immediate;
                a3 = (uint) (immediate_unsigned |
                             this.gp_register_values[(int) inst[i].gp_1]);
              }
            }
            break;
          case OPCODE.JAL:
            jal_addr = inst[i].jump_to_func;
            addNextTime = true;
            break;
        }

        if (addJAL) {
          JAL_CALL newCall = new JAL_CALL();
          newCall.a0 = a0;
          newCall.a1 = a1;
          newCall.a2 = a2;
          newCall.a3 = a3;
          newCall.JAL_ADDRESS = jal_addr;
          calls.Add(newCall);
          //Console.WriteLine(newCall.ToString());
          addNextTime = false;
        }
      }

      return calls;
    }

    public List<Instruction> ReadFunction(uint RAMAddr, uint RAMtoROM) {
      List<Instruction> instructions = [];
      ROM rom = ROM.Instance;
      uint ROM_OFFSET = RAMAddr - RAMtoROM;
      uint offset = ROM_OFFSET;
      int end = 0x8000;
      while (end > 0) {
        Instruction cur = this.parseInstruction(rom.readWordUnsigned(offset));
        offset += 4;
        end--;
        if (cur.opCode == OPCODE.JR && cur.gp_1 == GP_REGISTER.RA)
          end = 1;
        instructions.Add(cur);
      }
      return instructions;
    }

    private Instruction parseInstruction(uint data) {
      Instruction inst = new Instruction();
      if (data == 0) {
        inst.opCode = OPCODE.NOP;
        return inst;
      }

      uint opCode = data >> 26, func = 0;
      switch (opCode) {
        case 0x00:
          func = data & 0x3F;
          switch (func) {
            case 0x08:
              inst.opCode = OPCODE.JR;
              inst.gp_1 = (GP_REGISTER) ((data >> 21) & 0x1F);
              break;
            default:
              inst.opCode = OPCODE.DO_NOT_CARE;
              break;
          }
          break;
        case 0x03:
          inst.opCode = OPCODE.JAL;
          inst.jump_to_func = 0x80000000 + ((data & 0x3FFFFFF) << 2);
          break;
        case 0x09:
          inst.opCode = OPCODE.ADDIU;
          inst.gp_dest = (GP_REGISTER) ((data >> 16) & 0x1F);
          inst.gp_1 = (GP_REGISTER) ((data >> 21) & 0x1F);
          inst.immediate = (short) (data & 0xFFFF);
          this.gp_register_values[(int) inst.gp_dest] = this.gp_register_values[(int) inst.gp_1] + inst.immediate;
          break;
        case 0x0D:
          inst.opCode = OPCODE.ORI;
          inst.gp_dest = (GP_REGISTER) ((data >> 16) & 0x1F);
          inst.gp_1 = (GP_REGISTER) ((data >> 21) & 0x1F);
          inst.immediate = (short) (data & 0xFFFF);

          this.gp_register_values[(int) inst.gp_dest] = this.gp_register_values[(int) inst.gp_1] | (long) inst.immediate;
          break;
        case 0x0F:
          inst.opCode = OPCODE.LUI;
          inst.gp_dest = (GP_REGISTER) ((data >> 16) & 0x1F);
          inst.immediate = (short) (data & 0xFFFF);
          this.gp_register_values[(int) inst.gp_dest] = (long) inst.immediate << 16;
          break;
        default:
          inst.opCode = OPCODE.DO_NOT_CARE;
          break;
      }

      return inst;
    }


    long[] gp_register_values = new long[0x32];

    // General-Purpose Registers
    public enum GP_REGISTER {
      R0, // Constant 0
      AT, // Used for psuedo-instructions
      V0,
      V1, // Function returns
      A0,
      A1,
      A2,
      A3, // Function Arguments
      T0,
      T1,
      T2,
      T3,
      T4,
      T5,
      T6,
      T7, // Temporary
      S0,
      S1,
      S2,
      S3,
      S4,
      S5,
      S6,
      S7, // Saved
      T8,
      T9, // More temporary
      K0,
      K1, // Reserved for Kernal (do not use)
      GP, // Global area pointer
      SP, // Stack pointer
      FP, // Frame pointer
      RA // Return address
    }

    // Floating-Point Registers
    public enum FP_REGISTER {
      F0,
      F1,
      F2,
      F3, // Function returns
      F4,
      F5,
      F6,
      F7,
      F8,
      F9,
      F10,
      F11, // Temporary
      F12,
      F13,
      F14,
      F15, // Function arguments
      F16,
      F17,
      F18,
      F19, // More Temporary
      F20,
      F21,
      F22,
      F23,
      F24,
      F25,
      F26,
      F27,
      F28,
      F29,
      F30,
      F31 // Saved
    }

    public enum OPCODE {
      LUI,
      ADDIU,
      ORI,
      JAL,
      JR,
      NOP,
      DO_NOT_CARE
    }

    public class JAL_CALL {
      public uint JAL_ADDRESS = 0, a0 = 0, a1 = 0, a2 = 0, a3 = 0;

      public override string ToString() {
        return this.JAL_ADDRESS.ToString("X8") + "("
                                               +
                                               this.a0.ToString("X8") + ", "
                                               +
                                               this.a1.ToString("X8") + ", "
                                               +
                                               this.a2.ToString("X8") + ", "
                                               +
                                               this.a3.ToString("X8") + ")";
      }
    }

    public class Instruction {
      public OPCODE opCode;
      public GP_REGISTER gp_1, gp_dest;
      public short immediate = 0;
      public uint jump_to_func = 0;

      public override string ToString() {
        switch (this.opCode) {
          case OPCODE.JR:
            return "JR " + this.gp_1.ToString();
          case OPCODE.LUI:
            return "LUI " +
                   this.gp_dest.ToString() + " 0x" +
                   this.immediate.ToString("X4");
          case OPCODE.ADDIU:
            return "ADDIU " +
                   this.gp_dest.ToString() + " " +
                   this.gp_1.ToString() +
                   " 0x" +
                   this.immediate.ToString("X4");
          case OPCODE.JAL:
            return "JAL 0x" + this.jump_to_func.ToString("X8");
        }
        return this.opCode.ToString();
      }
    }
  }
}