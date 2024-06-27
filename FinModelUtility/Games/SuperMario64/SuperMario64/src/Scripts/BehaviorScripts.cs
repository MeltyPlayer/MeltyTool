namespace sm64.Scripts;

public enum BehaviorCommand : byte {
  BEGIN = 0x00,
  DELAY,
  CALL,
  RETURN,
  GOTO,
  BEGIN_REPEAT,
  end_repeat,
  end_repeat_continue,
  begin_loop,
  end_loop,
  BREAK = 0x0A,
  break_unused,
  call_native,
  add_float,
  set_float,
  add_int,
  set_int,
  or_int,
  bit_clear,
  set_int_rand_rshift,
  set_random_float,
  set_random_int,
  add_random_float,
  add_int_rand_rshift,
  nop_1,
  nop_2,
  nop_3,
  set_model,
  spawn_child,
  deactivate,
  drop_to_floor,
  sum_float,
  sum_int = 0x20,
  billboard,
  hide,
  set_hitbox,
  nop_4,
  delay_var,
  begin_repeat_unused,
  load_animations,
  animate,
  spawn_child_with_param,
  load_collision_data,
  set_hitbox_with_offset,
  spawn_obj,
  set_home,
  set_hurtbox,
  set_interact_type,
  set_obj_physics,
  set_interact_subtype,
  SCALE = 0x32,
  parent_bit_clear,
  animate_texture,
  disable_rendering,
  set_int_unused,
  spawn_water_droplet,
}

class BehaviorScripts {
  private static uint bytesToInt(byte[] b, int offset, int length) {
      switch (length) {
        case 1: return b[0 + offset];
        case 2: return (uint) (b[0 + offset] << 8 | b[1 + offset]);
        case 3:
          return (uint) (b[0 + offset] << 16 | b[1 + offset] << 8 |
                         b[2 + offset]);
        default:
          return (uint) (b[0 + offset] << 24 | b[1 + offset] << 16 |
                         b[2 + offset] << 8 | b[3 + offset]);
      }
    }

  public static void parse(ref List<ScriptDumpCommandInfo> dump,
                           uint segOffset) {
      parse(ref dump, (byte) (segOffset >> 24), segOffset & 0x00FFFFFF);
    }

  public static void parse(ref List<ScriptDumpCommandInfo> dump,
                           byte seg,
                           uint off) {
      if (seg == 0) return;
      ROM rom = ROM.Instance;
      byte[] data = rom.getSegment(seg, null);
      bool end = false;
      while (!end) {
        byte cmdLen = getCmdLength(data[off]);
        byte[] cmd = rom.getSubArray_safe(data, off, cmdLen);
        //rom.printArray(cmd, cmdLen);
        string desc = "Unknown command";
        bool alreadyAdded = false;
        switch (cmd[0]) {
          case 0x00:
            desc = "Start Behavior (Object type = " + cmd[1] + ")";
            break;
          case 0x02:
            desc = "Jump & link to address 0x" +
                   bytesToInt(cmd, 4, 4).ToString("X8");
            alreadyAdded = true;
            addBehCommandToDump(ref dump, cmd, seg, off, desc);
            parse(ref dump, cmd[4], bytesToInt(cmd, 5, 3));
            break;
          case 0x03:
            desc = "Return back from jump & link";
            end = true;
            break;
          case 0x04:
            desc = "Jump to address 0x" + bytesToInt(cmd, 4, 4).ToString("X8");
            alreadyAdded = true;
            addBehCommandToDump(ref dump, cmd, seg, off, desc);
            parse(ref dump, cmd[4], bytesToInt(cmd, 5, 3));
            end = true;
            break;
          case 0x05:
            desc = "Loop " + bytesToInt(cmd, 2, 2) + " times";
            break;
          case 0x07:
            desc = "Infinite loop (jump back 4 bytes)";
            break;
          case 0x08:
            desc = "Start of loop";
            break;
          case 0x06:
          case 0x09:
            desc = "End of loop";
            end = true;
            break;
          case 0x0A:
            desc = "End behavior script";
            end = true;
            break;
          case 0x0B:
            desc = "End behavior script";
            end = true;
            break;
          case 0x0C:
            desc = "Call ASM function 0x" +
                   bytesToInt(cmd, 4, 4).ToString("X8");
            break;
          case 0x0D:
            desc = "Add (float)" + bytesToInt(cmd, 2, 2) +
                   " to the value at obj->_0x" +
                   (cmd[1] * 4 + 0x88).ToString("X2");
            break;
          case 0x0E:
            desc = "(Set value) obj->_0x" + (cmd[1] * 4 + 0x88).ToString("X2") +
                   " = (float)" + bytesToInt(cmd, 2, 2);
            break;
          case 0x0F:
            desc = "Add " + (short) (bytesToInt(cmd, 2, 2) & 0xFFFF) +
                   " to the value at obj->_0x" +
                   (cmd[1] * 4 + 0x88).ToString("X2");
            break;
          case 0x10:
            desc = "(Set value) obj->_0x" + (cmd[1] * 4 + 0x88).ToString("X2") +
                   " = " + (short) (bytesToInt(cmd, 2, 2) & 0xFFFF);
            break;
          case 0x11:
            desc = "(Set bits) obj->_0x" + (cmd[1] * 4 + 0x88).ToString("X2") +
                   " |= 0x" + bytesToInt(cmd, 2, 2).ToString("X4");
            break;
          case 0x12:
            desc = "(Clear bits) obj->_0x" +
                   (cmd[1] * 4 + 0x88).ToString("X2") + " &= (0x" +
                   bytesToInt(cmd, 2, 2).ToString("X4") + " ^ 0xFFFF)";
            break;
          case 0x13:
            desc = "(Set RNG) obj->_0x" + (cmd[1] * 4 + 0x88).ToString("X2") +
                   " = " + bytesToInt(cmd, 2, 2) + " + (randU16() >> " +
                   bytesToInt(cmd, 4, 2) + ")";
            break;
          case 0x14:
            desc = "(Set float from multi) obj->_0x" +
                   (cmd[1] * 4 + 0x88).ToString("X2") + " = (float)" +
                   bytesToInt(cmd, 2, 2) + " * (float)" +
                   bytesToInt(cmd, 4, 2) + ")";
            break;
          case 0x15:
            desc = "(Set float RNG from multi) obj->_0x" +
                   (cmd[1] * 4 + 0x88).ToString("X2") + " = (float)" +
                   bytesToInt(cmd, 2, 2) + " + (randFloat() * (float)" +
                   bytesToInt(cmd, 4, 2) + ")";
            break;
          case 0x16:
            desc = "(Set float RNG from add) obj->_0x" +
                   (cmd[1] * 4 + 0x88).ToString("X2") + " = (float)" +
                   bytesToInt(cmd, 2, 2) + " + randFloat() + (float)" +
                   bytesToInt(cmd, 4, 2);
            break;
          case 0x17:
            desc = "(Set RNG) obj->_0x" + (cmd[1] * 4 + 0x88).ToString("X2") +
                   " = (obj->_0x" + (cmd[1] * 4 + 0x88).ToString("X2") + " + " +
                   bytesToInt(cmd, 2, 2) + ") + (randU16() >> " +
                   bytesToInt(cmd, 4, 2) + ")";
            break;
          case 0x18:
          case 0x19:
          case 0x1A:
            desc = "Do nothing";
            break;
          case 0x1B:
            desc = "Set Model ID to 0x" + bytesToInt(cmd, 2, 2).ToString("X4");
            break;
          case 0x1C:
            desc = "Spawn child object (Model ID = 0x" +
                   bytesToInt(cmd, 4, 4).ToString("X4") + ", Behavior = 0x" +
                   bytesToInt(cmd, 8, 4).ToString("X8");
            break;
          case 0x1D:
            desc = "Deactivate object";
            break;
          case 0x1E:
            desc = "Drop object to ground, and object->_0xEC |= 0x2";
            break;
          case 0x1F:
            desc = "(Add floats) obj->_0x" + (cmd[1] * 4 + 0x88).ToString("X2")
                                           + " = (float)obj->_0x" +
                                           (cmd[2] * 4 + 0x88).ToString("X2")
                                           + " + (float)obj->_0x" +
                                           (cmd[3] * 4 + 0x88).ToString("X2");
            break;
          case 0x20:
            desc = "(Add values) obj->_0x" + (cmd[1] * 4 + 0x88).ToString("X2")
                                           + " = obj->_0x" +
                                           (cmd[2] * 4 + 0x88).ToString("X2")
                                           + " + obj->_0x" +
                                           (cmd[3] * 4 + 0x88).ToString("X2");
            break;
          case 0x21:
            desc = "Set billboarding flag in graph flags (obj->_0x02 |= 0x04)";
            break;
          case 0x22:
            desc = "Set 0x10 flag in graph flags (obj->_0x02 |= 0x10)";
            break;
          case 0x23:
            desc = "Set Collision sphere size (XZ radius = " +
                   bytesToInt(cmd, 4, 2) + ", Y radius = " +
                   bytesToInt(cmd, 6, 2) + ")";
            break;
          case 0x24:
            desc = "Do nothing. (Unused function)";
            break;
          case 0x25:
            desc = "obj->_0x1F4 = ((obj->_0x1F4 < (obj->_0x" +
                   (cmd[1] * 4 + 0x88).ToString("X2") +
                   " - 1)) ? obj->_0x1F4 + 1 : 0)";
            break;
          case 0x26:
            desc = "Loop " + cmd[1] + " times";
            break;
          case 0x27:
            desc = "(Set word) obj->_0x" + (cmd[1] * 4 + 0x88).ToString("X2") +
                   " = 0x" + bytesToInt(cmd, 4, 4).ToString("X8");
            break;
          case 0x28:
            desc = "Set obj->_0x3C from (obj->_0x120 + 0x" +
                   (cmd[1] * 4).ToString("X") + ")";
            break;
          case 0x29:
            desc = "Spawn child object (Model ID = 0x" +
                   bytesToInt(cmd, 4, 4).ToString("X4") + ", Behavior = 0x" +
                   bytesToInt(cmd, 8, 4).ToString("X8") + ", B.Param = 0x" +
                   bytesToInt(cmd, 2, 2).ToString("X4");
            break;
          case 0x2A:
            desc = "Set collision address (obj->_0x218) from address 0x" +
                   bytesToInt(cmd, 4, 4).ToString("X8");
            ;
            break;
          case 0x2B:
            desc = "Set Collision sphere size (XZ radius = " +
                   bytesToInt(cmd, 4, 2) + ", Y radius = " +
                   bytesToInt(cmd, 6, 2) + ", obj->_0x208 = " +
                   bytesToInt(cmd, 8, 2) + ")";
            break;
          case 0x2C:
            desc = "(Spawn child object) obj->_0x6C = (Model ID = 0x" +
                   bytesToInt(cmd, 4, 4).ToString("X4") + ", Behavior = 0x" +
                   bytesToInt(cmd, 8, 4).ToString("X8");
            break;
          case 0x2D:
            desc =
                "Set inital position. (Used in Dorrie, Fly guys, etc. to determine in which range they can move)";
            break;
          case 0x2E:
            desc = "obj->_0x200 = (float)" +
                   bytesToInt(cmd, 4, 2).ToString("X4") +
                   ", obj->_0x204 = (float)" +
                   bytesToInt(cmd, 6, 2).ToString("X4");
            break;
          case 0x2F:
            desc = "(Set interaction value) obj->_0x130 = 0x" +
                   bytesToInt(cmd, 4, 4).ToString("X8");
            break;
          case 0x30:
            desc =
                "(Set gravity) obj->_0x128 = " + bytesToInt(cmd, 4, 2)
                                               + "f, obj->_0xE8 = " +
                                               (bytesToInt(cmd, 6, 2) / 100.0f)
                                               + "f, obj->_0x158 = " +
                                               (bytesToInt(cmd, 8, 2) / 100.0f)
                                               + "f, obj->_0x12C = " +
                                               (bytesToInt(cmd, 10, 2) / 100.0f)
                                               + "f, obj->_0x170 = " +
                                               (bytesToInt(cmd, 12, 2) / 100.0f)
                                               + "f, obj->_0x174 = " +
                                               (bytesToInt(cmd, 14, 2) /
                                                100.0f) + "f";
            break;
          case 0x31:
            desc = "obj->_0x190 = 0x" + bytesToInt(cmd, 4, 4).ToString("X8");
            break;
          case 0x32:
            desc = "Set uniform scaling to " + (bytesToInt(cmd, 2, 2) / 100f) +
                   "%";
            break;
          case 0x33:
            desc = "(Clear flags in child) obj->_0x68->_0x" +
                   (cmd[1] * 4 + 0x88).ToString("X2") + " &= ~0x" +
                   bytesToInt(cmd, 4, 4).ToString("X8");
            break;
          case 0x34:
            desc = "(Increment texture animate rate) if((*0x8032D5D4) / " +
                   bytesToInt(cmd, 2, 2) + " == 0) obj->_0x" +
                   (cmd[1] * 4 + 0x88).ToString("X2") + "++";
            break;
          case 0x35:
            desc = "(Clear lsb of graph flags) obj->_0x2 &= 0xFFFE";
            break;
          case 0x36:
            desc = "obj->_0x" + (cmd[1] * 4 + 0x88).ToString("X2") + " = " +
                   ((short) bytesToInt(cmd, 4, 4)).ToString("X4");
            break;
        }
        if (!alreadyAdded)
          addBehCommandToDump(ref dump, cmd, seg, off, desc);
        off += cmdLen;
      }
    }

  private static void addBehCommandToDump(
      ref List<ScriptDumpCommandInfo> dump,
      byte[] cmd,
      byte seg,
      uint offset,
      string description) {
      ScriptDumpCommandInfo info = new ScriptDumpCommandInfo();
      info.data = cmd;
      info.description = description;
      info.segAddress = (uint) (seg << 24) | offset;
      info.romAddress =
          ROM.Instance.decodeSegmentAddress_safe(seg, offset, null);
      dump.Add(info);
    }

  private static byte getCmdLength(byte cmd) {
      switch (cmd) {
        case 0x02:
        case 0x04:
        case 0x0C:
        case 0x13:
        case 0x16:
        case 0x17:
        case 0x23:
        case 0x27:
        case 0x2A:
        case 0x2E:
        case 0x2F:
        case 0x36:
        case 0x37:
          return 0x08;
        case 0x1C:
        case 0x29:
        case 0x2B:
        case 0x2C:
          return 0x0C;
        case 0x30:
          return 0x14;
        default:
          return 0x04;
      }
    }
}