using System.Numerics;

using f3dzex2.image;

using sm64.LevelInfo;
using sm64.memory;

namespace sm64.Scripts {
  public class LevelScripts {
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

    public static int parse(IN64Hardware<ISm64Memory> sm64Hardware,
                            ref Level lvl,
                            byte seg,
                            uint off) {
      var sm64Memory = sm64Hardware.Memory;

      if (seg == 0) return -1;
      ROM rom = ROM.Instance;
      byte[] data = rom.getSegment(seg, null);
      bool end = false;
      int endCmd = 0;
      while (!end) {
        //Stopwatch stopWatch = new Stopwatch();
        //stopWatch.Start();
        byte cmdLen = data[off + 1];
        byte[] cmd = rom.getSubArray_safe(data, off, cmdLen);
        //Console.WriteLine(rom.decodeSegmentAddress_safe(seg, off, null).ToString("X8"));
        //rom.printArray(cmd, cmdLen);
        string desc = "Unknown command";
        bool alreadyAdded = false;
        switch (cmd[0]) {
          case 0x00:
          case 0x01:
            CMD_00(sm64Hardware, ref lvl, ref desc, cmd, seg, off);
            alreadyAdded = true;
            break;
          case 0x02:
            endCmd = 2;
            desc = "End level script";
            end = true;
            break;
          case 0x03:
          case 0x04:
            desc = "Delay frames";
            break;
          case 0x05:
            endCmd = CMD_05(sm64Hardware, ref lvl, ref desc, cmd, seg, off);
            alreadyAdded = true;
            end = true;
            break;
          case 0x06:
            if (CMD_06(sm64Hardware, ref lvl, ref desc, cmd, seg, off) == 0x02) {
              end = true;
              endCmd = 2;
            }
            alreadyAdded = true;
            break;
          case 0x07:
            end = true;
            desc = "Pop script stack and return back";
            endCmd = 0x07;
            break;
          case 0x08:
            desc = "Push script stack and a 16-bit parameter onto stack";
            break;
          case 0x09:
            desc = "Pops script stack and parameter";
            break;
          case 0x0A:
            desc =
                "Push next level command on script stack and param 0x00000000 onto stack";
            break;
          case 0x0B:
            CMD_0B(ref lvl, ref desc, cmd, seg, off, sm64Memory.AreaId);
            alreadyAdded = true;
            break;
          case 0x0C:
            CMD_0C(sm64Hardware, ref lvl, ref desc, cmd, seg, off);
            alreadyAdded = true;
            break;
          case 0x0D:
            CMD_0D(ref lvl, ref desc, cmd, seg, off, sm64Memory.AreaId);
            alreadyAdded = true;
            break;
          case 0x0E:
            CMD_0E(ref lvl, ref desc, cmd, seg, off, sm64Memory.AreaId);
            alreadyAdded = true;
            break;
          case 0x0F:
            desc = "Skip following 0x10 (Do nothing) commands";
            break;
          case 0x10:
            desc = "Do nothing";
            break;
          case 0x11:
            desc = "Call ASM function and set level accumulator (script_accum)";
            break;
          case 0x12:
            desc =
                "Call ASM function loop and set level accumulator (script_accum)";
            break;
          case 0x13:
            desc = "Set level accumulator (script_accum) as 0x" +
                   bytesToInt(cmd, 2, 2).ToString("X4");
            break;
          case 0x14:
            desc = "Call PushPoolState() function";
            break;
          case 0x15:
            desc = "Call PopPoolState() function";
            break;
          case 0x16:
            desc = "Copy bytes from ROM (0x" +
                   bytesToInt(cmd, 8, 4).ToString("X8") + " to 0x" +
                   bytesToInt(cmd, 12, 4).ToString("X8") +
                   ") to RAM address 0x" + bytesToInt(cmd, 4, 4).ToString("X8");
            break;
          case 0x17:
            CMD_17(ref lvl, ref desc, cmd);
            break;
          case 0x18:
          case 0x1A:
            CMD_18(ref lvl, ref desc, cmd);
            break;
          case 0x19:
            desc = "Create Mario head demo (";
            switch (cmd[3]) {
              case 1:
                desc += "No face";
                break;
              case 2:
                desc += "Regular face";
                break;
              case 3:
                desc += "Game over face";
                break;
            }
            desc += ")";
            break;
          case 0x1B:
            desc = "Start loading sequence";
            break;
          case 0x1C:
            desc = "Level & Memory cleanup";
            break;
          case 0x1D:
            desc = "End loading sequence";
            break;
          case 0x1E:
            desc = "Allocate space for level data from pool";
            break;
          case 0x1F: {
            //Globals.DEBUG_PLG = true;                       
            CMD_1F(sm64Hardware, ref lvl, ref desc, cmd, data);
            break;
          }
          case 0x20: {
            desc = "End of area " + lvl.CurrentAreaID;
            sm64Memory.AreaId = null;
            break;
          }
          case 0x21:
            CMD_21(sm64Hardware, ref lvl, ref desc, cmd);
            break;
          case 0x22:
            //Globals.DEBUG_PLG = false;
            CMD_22(sm64Hardware, ref lvl, ref desc, cmd);
            break;
          case 0x24:
            CMD_24(sm64Memory, ref lvl, ref desc, cmd, seg, off);
            break;
          case 0x25:
            desc = "Setup Mario object";
            break;
          case 0x26:
            CMD_26(ref lvl, ref desc, cmd, seg, off, sm64Memory.AreaId);
            break;
          case 0x27:
            CMD_27(ref lvl, ref desc, cmd, seg, off, sm64Memory.AreaId);
            break;
          case 0x28:
            CMD_28(ref lvl, ref desc, cmd, seg, off, sm64Memory.AreaId);
            break;
          case 0x2B: {
            desc = "Mario's default pos = (" +
                   (short) bytesToInt(cmd, 6, 2) + "," +
                   (short) bytesToInt(cmd, 8, 2) + "," +
                   (short) bytesToInt(cmd, 10, 2) + "), Y-Rot = " +
                   (short) bytesToInt(cmd, 4, 2) + ", start in area " + cmd[2];
          }
            break;
          case 0x2E:
            CMD_2E(ref lvl, ref desc, cmd, sm64Memory.AreaId);
            break;
          case 0x2F:
            CMD_2F(ref lvl, ref desc, cmd, sm64Memory.AreaId);
            break;
          case 0x30:
            desc = "Show dialog message when level starts; dialog ID = 0x" +
                   cmd[3].ToString("X2");
            break;
          case 0x31: {
            var defaultTerrainType = cmd[3];

            // TODO: Does this need to be remembered for future areas?
            var area =
                lvl.Areas.Single(area => area.AreaID == sm64Memory.AreaId);
            area.DefaultTerrainType = defaultTerrainType;

            switch (defaultTerrainType) {
              case 0:
                desc = "Set default terrain to \"Normal\"";
                break;
              case 1:
                desc = "Set default terrain to \"Normal B\"";
                break;
              case 2:
                desc = "Set default terrain to \"Snow\"";
                break;
              case 3:
                desc = "Set default terrain to \"Sand\"";
                break;
              case 4:
                desc = "Set default terrain to \"Haunted House\"";
                break;
              case 5:
                desc = "Set default terrain to \"Water levels\"";
                break;
              case 6:
                desc = "Set default terrain to \"Slippery Slide\"";
                break;
            }
            break;
          }
          case 0x32:
            desc = "Do nothing";
            break;
          case 0x33:
            if (cmd[2] == 0x01)
              desc = "Fade screen with color (R = " + cmd[4] + ", G = " +
                     cmd[5] + ", B = " + cmd[6] + ", duration = " + cmd[3] +
                     " frames)";
            else
              desc = "Disable screen fade";
            break;
          case 0x34:
            if (cmd[2] == 0x00)
              desc = "Cancel blackout";
            else
              desc = "Blackout screen";
            break;
          case 0x36:
            desc = "Set music (Seq = 0x" + cmd[5].ToString("X2") + ")";
            break;
          case 0x37:
            desc = "Set music (Seq = 0x" + cmd[3].ToString("X2") + ")";
            break;
          case 0x39:
            CMD_39(sm64Memory, ref lvl, ref desc, cmd);
            break;
          case 0x3B:
            desc = "Add jet stream; Position = (" +
                   (short) bytesToInt(cmd, 4, 2) + "," +
                   (short) bytesToInt(cmd, 6, 2) + "," +
                   (short) bytesToInt(cmd, 8, 2) +
                   "), Intensity = " + (short) bytesToInt(cmd, 10, 2);
            break;
        }
        if (!alreadyAdded)
          addLSCommandToDump(ref lvl, cmd, seg, off, desc, sm64Memory.AreaId);
        //stopWatch.Stop();
        // if(stopWatch.Elapsed.Milliseconds > 1)
        //    Console.WriteLine("RunTime (CMD "+cmd[0].ToString("X2")+"): " + stopWatch.Elapsed.Milliseconds + "ms");
        off += cmdLen;
      }
      return endCmd;
    }

    private static void addLSCommandToDump(ref Level lvl,
                                           byte[] cmd,
                                           byte seg,
                                           uint offset,
                                           string description,
                                           byte? areaID) {
      ScriptDumpCommandInfo info = new ScriptDumpCommandInfo();
      info.data = cmd;
      info.description = description;
      info.segAddress = (uint) (seg << 24) | offset;
      info.romAddress =
          ROM.Instance.decodeSegmentAddress_safe(seg, offset, areaID);
      lvl.LevelScriptCommands_ForDump.Add(info);
    }

    private static void CMD_00(
        IN64Hardware<ISm64Memory> sm64Hardware,
        ref Level lvl,
        ref string desc,
        byte[] cmd,
        byte org_seg,
        uint org_off) {
      ROM rom = ROM.Instance;
      byte seg = cmd[3];
      uint start = bytesToInt(cmd, 4, 4);
      uint end = bytesToInt(cmd, 8, 4);
      uint off = bytesToInt(cmd, 13, 3);
      desc = "Load segment 0x" + seg.ToString("X2") + " and jump to 0x" +
             seg.ToString("X2") + off.ToString("X6");
      rom.setSegment(seg, start, end, false, null);
      if (seg == 0x14) {
        desc +=
            " (This gets skipped in Quad64, since it's just the star select screen data)";
        addLSCommandToDump(ref lvl,
                           cmd,
                           org_seg,
                           org_off,
                           desc,
                           sm64Hardware.Memory.AreaId);
        return;
      }

      addLSCommandToDump(ref lvl,
                         cmd,
                         org_seg,
                         org_off,
                         desc,
                         sm64Hardware.Memory.AreaId);
      parse(sm64Hardware, ref lvl, seg, off);
    }

    private static int CMD_05(IN64Hardware<ISm64Memory> sm64Hardware,
                              ref Level lvl,
                              ref string desc,
                              byte[] cmd,
                              byte currentSeg,
                              uint currentOff) {
      byte seg = cmd[4];
      uint off = bytesToInt(cmd, 5, 3);
      desc = "Jump to segment address 0x" + seg.ToString("X2") +
             off.ToString("X6");
      addLSCommandToDump(ref lvl,
                         cmd,
                         currentSeg,
                         currentOff,
                         desc,
                         sm64Hardware.Memory.AreaId);
      if (seg == currentSeg) {
        if ((long) off - (long) currentOff == -4) {
          Console.WriteLine("Infinite loop detected!");
          return 0x02;
        }
      }
      return parse(sm64Hardware, ref lvl, seg, off);
    }

    private static int CMD_06(IN64Hardware<ISm64Memory> sm64Hardware,
                              ref Level lvl,
                              ref string desc,
                              byte[] cmd,
                              byte org_seg,
                              uint org_off) {
      byte seg = cmd[4];
      uint off = bytesToInt(cmd, 5, 3);
      desc = "Push script stack and jump to address 0x" + seg.ToString("X2") +
             off.ToString("X6");
      addLSCommandToDump(ref lvl,
                         cmd,
                         org_seg,
                         org_off,
                         desc,
                         sm64Hardware.Memory.AreaId);
      return parse(sm64Hardware, ref lvl, seg, off);
    }

    private static string getCondition(byte operation,
                                       uint argument,
                                       bool inverse) {
      string appendInverse = (inverse ? "!" : "");
      switch (operation) {
        case 0:
          return "if (" + appendInverse + " script_accum & 0x" +
                 argument.ToString("X2") + ")";
        case 1:
          return "if (" + appendInverse + "!(script_accum & 0x" +
                 argument.ToString("X2") + "))";
        case 2:
          return "if (" + appendInverse + "script_accum == 0x" +
                 argument.ToString("X2") + ")";
        case 3:
          return "if (" + appendInverse + "script_accum != 0x" +
                 argument.ToString("X2") + ")";
        case 4:
          return "if (" + appendInverse + "script_accum < 0x" +
                 argument.ToString("X2") + ")";
        case 5:
          return "if (" + appendInverse + "script_accum <= 0x" +
                 argument.ToString("X2") + ")";
        case 6:
          return "if (" + appendInverse + "script_accum > 0x" +
                 argument.ToString("X2") + ")";
        case 7:
          return "if (" + appendInverse + "script_accum >= 0x" +
                 argument.ToString("X2") + ")";
        default:
          return "";
      }
    }


    private static void CMD_0B(ref Level lvl,
                               ref string desc,
                               byte[] cmd,
                               byte org_seg,
                               uint org_off,
                               byte? areaID) {
      var lvlcheck = bytesToInt(cmd, 4, 4);
      byte operation = cmd[2];
      desc = "Pop stack " + " " + getCondition(operation, lvlcheck, false);
      addLSCommandToDump(ref lvl, cmd, org_seg, org_off, desc, areaID);
    }

    private static void CMD_0C(IN64Hardware<ISm64Memory> sm64Hardware,
                               ref Level lvl,
                               ref string desc,
                               byte[] cmd,
                               byte org_seg,
                               uint org_off) {
      var lvlcheck = bytesToInt(cmd, 4, 4);
      byte operation = cmd[2];
      desc = "Jump to address 0x" + bytesToInt(cmd, 8, 4).ToString("X8") + " " +
             getCondition(operation, lvlcheck, false);
      addLSCommandToDump(ref lvl,
                         cmd,
                         org_seg,
                         org_off,
                         desc,
                         sm64Hardware.Memory.AreaId);
      if (org_seg == 0x15) {
        if (lvlcheck == lvl.LevelID) {
          byte seg = cmd[8];
          uint off = bytesToInt(cmd, 9, 3);
          parse(sm64Hardware, ref lvl, seg, off);
        }
      }
    }


    private static void CMD_0D(ref Level lvl,
                               ref string desc,
                               byte[] cmd,
                               byte org_seg,
                               uint org_off,
                               byte? areaID) {
      var lvlcheck = bytesToInt(cmd, 4, 4);
      byte operation = cmd[2];
      desc = "Push next command to stack and jump " + " " +
             getCondition(operation, lvlcheck, false);
      addLSCommandToDump(ref lvl, cmd, org_seg, org_off, desc, areaID);
    }

    private static void CMD_0E(ref Level lvl,
                               ref string desc,
                               byte[] cmd,
                               byte org_seg,
                               uint org_off,
                               byte? areaID) {
      var lvlcheck = bytesToInt(cmd, 4, 4);
      byte operation = cmd[2];
      desc = "Skip following 0x10 and 0x0F levelscript commands if " + " " +
             getCondition(operation, lvlcheck, true);
      addLSCommandToDump(ref lvl, cmd, org_seg, org_off, desc, areaID);
    }

    private static void CMD_17(ref Level lvl, ref string desc, byte[] cmd) {
      ROM rom = ROM.Instance;
      byte seg = cmd[3];
      uint start = bytesToInt(cmd, 4, 4);
      uint end = bytesToInt(cmd, 8, 4);
      desc = "Load Segment 0x" + seg.ToString("X2") + " from ROM 0x" +
             start.ToString("X8") + " to 0x" + end.ToString("X8");
      rom.setSegment(seg, start, end, false, null);
    }

    private static void CMD_18(ref Level lvl, ref string desc, byte[] cmd) {
      ROM rom = ROM.Instance;
      byte seg = cmd[3];
      uint start = bytesToInt(cmd, 4, 4);
      uint end = bytesToInt(cmd, 8, 4);
      desc = "Load Segment 0x" + seg.ToString("X2") +
             " from compressed MIO0 at ROM addr 0x" + start.ToString("X8") +
             " to 0x" + end.ToString("X8");
      byte[] MIO0_header = rom.getSubArray_safe(rom.Bytes, start, 0x10);
      if (bytesToInt(MIO0_header, 0, 4) == 0x4D494F30) // Check MIO0 signature
      {
        int compressedOffset = (int) bytesToInt(MIO0_header, 0x8, 4);
        int uncompressedOffset = (int) bytesToInt(MIO0_header, 0xC, 4);
        bool isFakeMIO0 =
            rom.testIfMIO0IsFake(start, compressedOffset, uncompressedOffset);
        rom.setSegment(seg, start, end, true, isFakeMIO0,
                       (uint) uncompressedOffset, null);
      }
    }

    private static void CMD_1F(IN64Hardware<ISm64Memory> sm64Hardware,
                               ref Level lvl,
                               ref string desc,
                               byte[] cmd,
                               byte[] data) {
      byte areaID = cmd[2];
      byte seg = cmd[4];
      uint off = bytesToInt(cmd, 5, 3);

      sm64Hardware.Memory.AreaId = areaID;

      setAreaSegmented0xE(areaID, data);

      desc = "Start area " + areaID + "; Load area geo layout from 0x" +
             seg.ToString("X2") + off.ToString("X6");

      Area newArea = new Area(sm64Hardware, areaID, bytesToInt(cmd, 4, 4), lvl);

      // Globals.DEBUG_PARSING_LEVEL_AREA = true;
      // Stopwatch stopWatch = new Stopwatch();
      // stopWatch.Start();
      new GeoScriptsWrapper().parse(sm64Hardware.Memory,
                                    newArea.AreaModel,
                                    ref lvl,
                                    seg,
                                    off);
      lvl.setAreaBackgroundInfo(ref newArea);
      lvl.Areas.Add(newArea);
      lvl.CurrentAreaID = areaID;
      // stopWatch.Stop();
      // Console.WriteLine("RunTime (GeoScripts.parse): " + stopWatch.Elapsed.Milliseconds + "ms");

      //stopWatch = new Stopwatch();
      // stopWatch.Start();
      //if(areaID == 1) newArea.AreaModel.dumpModelToOBJ(1.0f/500.0f);
      //stopWatch.Stop();
      //Console.WriteLine("RunTime (newArea.AreaModel.buildBuffers): " + stopWatch.Elapsed.Milliseconds + "ms");
      //Globals.DEBUG_PARSING_LEVEL_AREA = false;
      // newArea.AreaModel.outputTextureAtlasToPng("Area_"+areaID+"_TexAtlus.png");
    }

    private static void CMD_21(
        IN64Hardware<ISm64Memory> sm64Hardware,
        ref Level lvl,
        ref string desc,
        byte[] cmd) {
      ROM rom = ROM.Instance;
      byte modelID = cmd[3];
      var address = bytesToInt(cmd, 4, 4);
      byte seg = cmd[4];
      uint off = bytesToInt(cmd, 5, 3);

      desc = "Define Model ID 0x" + modelID.ToString("X2") +
             "; Load Fast3D from 0x" + seg.ToString("X2") + off.ToString("X6");

      Model3DLods newModel = new Model3DLods(sm64Hardware);

      if (rom.getSegment(seg, sm64Hardware.Memory.AreaId) != null) {
        newModel.AddDl(address);
      }

      if (lvl.ModelIDs.ContainsKey(modelID))
        lvl.ModelIDs.Remove(modelID);
      lvl.ModelIDs.Add(modelID, newModel);
    }

    private static void CMD_22(IN64Hardware<ISm64Memory> sm64Hardware,
                               ref Level lvl,
                               ref string desc,
                               byte[] cmd) {
      ROM rom = ROM.Instance;
      byte modelID = cmd[3];
      byte seg = cmd[4];
      uint off = bytesToInt(cmd, 5, 3);

      if (modelID == 0x7A)
        Globals.DEBUG_PARSING_DL = true;

      desc = "Define Model ID 0x" + modelID.ToString("X2") +
             "; Load Geometry layout from 0x" + seg.ToString("X2") +
             off.ToString("X6");

      //Console.WriteLine("Size of seg 0x"+seg.ToString("X2")+" = " + rom.getSegment(seg).Length);
      Model3DLods newModel = new Model3DLods(sm64Hardware);
      if (rom.getSegment(seg, sm64Hardware.Memory.AreaId) != null) {
        try {
          new GeoScriptsWrapper().parse(sm64Hardware.Memory,
                                        newModel,
                                        ref lvl,
                                        seg,
                                        off);
        } catch (Exception e) {
          Console.WriteLine(e.Message);
          Console.WriteLine(e.StackTrace);
        }
      }

      var geoDataSegAddress = bytesToInt(cmd, 4, 4);
      lvl.AddObjectCombos(modelID, geoDataSegAddress);

      if (lvl.ModelIDs.ContainsKey(modelID))
        lvl.ModelIDs.Remove(modelID);
      lvl.ModelIDs.Add(modelID, newModel);

      if (modelID == 0x7A)
        Globals.DEBUG_PARSING_DL = false;
    }

    private static void CMD_24(
        IReadOnlySm64Memory n64Memory,
        ref Level lvl,
        ref string desc,
        byte[] cmd,
        byte seg,
        uint off) {
      ROM rom = ROM.Instance;
      Object3D newObj = new Object3D();
      if (rom.isSegmentMIO0(seg, n64Memory.AreaId)) {
        newObj.MakeReadOnly();
        newObj.Address = "N/A";
      } else {
        newObj.Address =
            "0x" + rom.decodeSegmentAddress(seg, off, n64Memory.AreaId).ToString("X");
      }

      byte actMask = cmd[2];
      newObj.AllActs = (actMask == 0x1F);
      newObj.Act1 = ((actMask & 0x1) == 0x1);
      newObj.Act2 = ((actMask & 0x2) == 0x2);
      newObj.Act3 = ((actMask & 0x4) == 0x4);
      newObj.Act4 = ((actMask & 0x8) == 0x8);
      newObj.Act5 = ((actMask & 0x10) == 0x10);
      newObj.Act6 = ((actMask & 0x20) == 0x20);
      newObj.ShowHideActs(newObj.AllActs);
      newObj.ModelID = cmd[3];
      newObj.xPos = (short) bytesToInt(cmd, 0x4, 2);
      newObj.yPos = (short) bytesToInt(cmd, 0x6, 2);
      newObj.zPos = (short) bytesToInt(cmd, 0x8, 2);
      newObj.xRot = (short) bytesToInt(cmd, 0xA, 2);
      newObj.yRot = (short) bytesToInt(cmd, 0xC, 2);
      newObj.zRot = (short) bytesToInt(cmd, 0xE, 2);
      newObj.MakeBehaviorReadOnly(false);
      newObj.MakeModelIDReadOnly(false);
      newObj.setBehaviorFromAddress(bytesToInt(cmd, 0x14, 4));
      newObj.BehaviorParameter1 = cmd[0x10];
      newObj.BehaviorParameter2 = cmd[0x11];
      newObj.BehaviorParameter3 = cmd[0x12];
      newObj.BehaviorParameter4 = cmd[0x13];
      newObj.createdFromLevelScriptCommand = Object3D.FROM_LS_CMD.CMD_24;
      newObj.level = lvl;
      lvl.getCurrentArea().Objects.Add(newObj);

      desc = "Place Object at pos (" + newObj.xPos + "," + newObj.yPos + "," +
             newObj.zPos + ")";
    }


    private static void CMD_26(ref Level lvl,
                               ref string desc,
                               byte[] cmd,
                               byte seg,
                               uint off,
                               byte? areaID) {
      ROM rom = ROM.Instance;
      Warp warp = new Warp(false);
      if (rom.isSegmentMIO0(seg, areaID)) {
        warp.MakeReadOnly();
        warp.Address = "N/A";
      } else {
        warp.Address =
            "0x" + rom.decodeSegmentAddress(seg, off, areaID).ToString("X");
      }
      warp.WarpFrom_ID = cmd[2];
      warp.WarpTo_LevelID = cmd[3];
      warp.WarpTo_AreaID = cmd[4];
      warp.WarpTo_WarpID = cmd[5];
      lvl.getCurrentArea().Warps.Add(warp);
      desc = "Define warp (0x" + warp.WarpFrom_ID.ToString("X2") + " -> ";
      if (warp.WarpTo_LevelID == lvl.LevelID) {
        if (warp.WarpTo_AreaID == lvl.getCurrentArea().AreaID)
          desc += "0x" + warp.WarpTo_WarpID.ToString("X2") + ")";
        else
          desc += "0x" + warp.WarpTo_WarpID.ToString("X2") + " in area " +
                  warp.WarpTo_AreaID + ")";
      } else {
        desc += "0x" + warp.WarpTo_WarpID.ToString("X2") + " at level 0x" +
                warp.WarpTo_LevelID.ToString("X2") + " in area " +
                warp.WarpTo_AreaID + ")";
      }
    }

    private static void CMD_27(ref Level lvl,
                               ref string desc,
                               byte[] cmd,
                               byte seg,
                               uint off,
                               byte? areaID) {
      ROM rom = ROM.Instance;
      Warp warp = new Warp(true);
      if (rom.isSegmentMIO0(seg, areaID)) {
        warp.MakeReadOnly();
        warp.Address = "N/A";
      } else {
        warp.Address =
            "0x" + rom.decodeSegmentAddress(seg, off, areaID).ToString("X");
      }
      warp.WarpFrom_ID = cmd[2];
      warp.WarpTo_LevelID = cmd[3];
      warp.WarpTo_AreaID = cmd[4];
      warp.WarpTo_WarpID = cmd[5];
      lvl.getCurrentArea().PaintingWarps.Add(warp);
      desc = "Define painting warp (0x" + warp.WarpFrom_ID.ToString("X2") +
             " -> ";
      if (warp.WarpTo_LevelID == lvl.LevelID) {
        if (warp.WarpTo_AreaID == lvl.getCurrentArea().AreaID)
          desc += "0x" + warp.WarpTo_WarpID.ToString("X2") + ")";
        else
          desc += "0x" + warp.WarpTo_WarpID.ToString("X2") + " in area " +
                  warp.WarpTo_AreaID + ")";
      } else {
        desc += "0x" + warp.WarpTo_WarpID.ToString("X2") + " at level 0x" +
                warp.WarpTo_LevelID.ToString("X2") + " in area " +
                warp.WarpTo_AreaID + ")";
      }
    }

    private static void CMD_28(ref Level lvl,
                               ref string desc,
                               byte[] cmd,
                               byte seg,
                               uint off,
                               byte? areaID) {
      ROM rom = ROM.Instance;
      WarpInstant warp = new WarpInstant();
      if (rom.isSegmentMIO0(seg, areaID)) {
        warp.MakeReadOnly();
        warp.Address = "N/A";
      } else {
        warp.Address =
            "0x" + rom.decodeSegmentAddress(seg, off, areaID).ToString("X");
      }
      warp.TriggerID = cmd[2];
      warp.AreaID = cmd[3];
      warp.TeleX = (short) bytesToInt(cmd, 4, 2);
      warp.TeleY = (short) bytesToInt(cmd, 6, 2);
      warp.TeleZ = (short) bytesToInt(cmd, 8, 2);
      lvl.getCurrentArea().InstantWarps.Add(warp);
      desc = "Define Instant warp (";
      if (lvl.getCurrentArea().AreaID != warp.AreaID)
        desc += "To area " + warp.AreaID + ",";

      desc += "Teleport (" + warp.TeleX + "," + warp.TeleY + "," + warp.TeleZ +
              "), Trigger = collision id 0x" +
              (warp.TriggerID + 0x1B).ToString("X2") + ")";
    }

    /* Process collision map, Special Objects, and waterboxes. */
    private static void CMD_2E(ref Level lvl,
                               ref string desc,
                               byte[] cmd,
                               byte? areaID) {
      ROM rom = ROM.Instance;
      if (cmd.Length < 8)
        return;
      desc = "Load collision, and place special objects from address 0x" +
             bytesToInt(cmd, 4, 4).ToString("X8");
      byte segment = cmd[4];
      uint off = bytesToInt(cmd, 5, 3);
      byte[] data = rom.getSegment(segment, areaID)!;
      var sub_cmd = (CollisionMapReader.CollisionSubCommand) bytesToInt(data, (int) off, 2);

      // Check if the data is actually collision data.
      if (data[off] != 0x00 || data[off + 1] != 0x40)
        return;

      CollisionMap cmap = lvl.getCurrentArea().collision;
      uint num_verts = (ushort) bytesToInt(data, (int) off + 2, 2);

      off += 4;
      for (int i = 0; i < num_verts; i++) {
        short x = (short) bytesToInt(data, (int) off + 0, 2);
        short y = (short) bytesToInt(data, (int) off + 2, 2);
        short z = (short) bytesToInt(data, (int) off + 4, 2);
        cmap.AddVertex(new Vector3(x, y, z));
        off += 6;
      }

      while (sub_cmd != CollisionMapReader.CollisionSubCommand.TERRAIN_LOAD_CONTINUE) {
        sub_cmd = (CollisionMapReader.CollisionSubCommand) bytesToInt(data, (int) off, 2);
        //Console.WriteLine(sub_cmd.ToString("X8"));
        if (sub_cmd == CollisionMapReader.CollisionSubCommand.TERRAIN_LOAD_CONTINUE) break;
        //rom.printArraySection(data, (int)off, 4 + (int)collisionLength(sub_cmd));
        cmap.NewTriangleList((int) bytesToInt(data, (int) off, 2));
        uint num_tri = (ushort) bytesToInt(data, (int) off + 2, 2);
        uint col_len = CollisionMapReader.GetLengthOfSubCommand(sub_cmd);
        off += 4;
        for (int i = 0; i < num_tri; i++) {
          uint a = bytesToInt(data, (int) off + 0, 2);
          uint b = bytesToInt(data, (int) off + 2, 2);
          uint c = bytesToInt(data, (int) off + 4, 2);
          cmap.AddTriangle(a, b, c);
          off += col_len;
        }
      }
      cmap.buildCollisionMap();
      off += 2;
      bool end = false;
      while (!end) {
        sub_cmd = (CollisionMapReader.CollisionSubCommand) bytesToInt(data, (int) off, 2);
        switch (sub_cmd) {
          case CollisionMapReader.CollisionSubCommand.TERRAIN_LOAD_END:
            end = true;
            break;
          case CollisionMapReader.CollisionSubCommand.TERRAIN_LOAD_OBJECTS:
            uint num_obj = (ushort) bytesToInt(data, (int) off + 2, 2);
            off += 4;
            for (int i = 0; i < num_obj; i++) {
              ushort obj_id = (ushort) bytesToInt(data, (int) off, 2);
              byte[] entry = getSpecialObjectEntry((byte) obj_id);
              uint obj_len = getSpecialObjectLength(obj_id);
              Object3D newObj = new Object3D();
              if (rom.isSegmentMIO0(segment, areaID)) {
                newObj.MakeReadOnly();
                newObj.Address = "N/A";
              } else {
                newObj.Address = "0x" + rom
                                        .decodeSegmentAddress(
                                            segment, off, areaID)
                                        .ToString("X");
              }
              newObj.setPresetID(obj_id);
              newObj.level = lvl;
              newObj.HideProperty(Object3D.FLAGS.ROTATION_X);
              newObj.HideProperty(Object3D.FLAGS.ROTATION_Z);
              newObj.HideProperty(Object3D.FLAGS.BPARAM_3);
              newObj.HideProperty(Object3D.FLAGS.BPARAM_4);
              newObj.xPos = (short) bytesToInt(data, (int) off + 2, 2);
              newObj.yPos = (short) bytesToInt(data, (int) off + 4, 2);
              newObj.zPos = (short) bytesToInt(data, (int) off + 6, 2);
              newObj.BehaviorParameter1 = entry[1];
              newObj.BehaviorParameter2 = entry[2];
              newObj.MakeBehaviorReadOnly(true);
              newObj.MakeModelIDReadOnly(true);
              if (obj_len > 8) {
                newObj.yRot =
                    (short) (bytesToInt(data, (int) off + 8, 2) * 1.40625);
                if (obj_len > 10) {
                  newObj.BehaviorParameter1 = data[off + 10];
                  newObj.BehaviorParameter2 = data[off + 11];
                  newObj.createdFromLevelScriptCommand =
                      Object3D.FROM_LS_CMD.CMD_2E_12;
                  lvl.AddSpecialObjectPreset_12(obj_id, entry[3],
                                                bytesToInt(entry, 4, 4),
                                                data[off + 10], data[off + 11]);
                } else {
                  lvl.AddSpecialObjectPreset_10(obj_id, entry[3],
                                                bytesToInt(entry, 4, 4));
                  newObj.HideProperty(Object3D.FLAGS.BPARAM_1);
                  newObj.HideProperty(Object3D.FLAGS.BPARAM_2);
                  newObj.createdFromLevelScriptCommand =
                      Object3D.FROM_LS_CMD.CMD_2E_10;
                }
              } else {
                lvl.AddSpecialObjectPreset_8(obj_id, entry[3],
                                             bytesToInt(entry, 4, 4));
                newObj.HideProperty(Object3D.FLAGS.BPARAM_1);
                newObj.HideProperty(Object3D.FLAGS.BPARAM_2);
                newObj.HideProperty(Object3D.FLAGS.ROTATION_Y);
                newObj.createdFromLevelScriptCommand =
                    Object3D.FROM_LS_CMD.CMD_2E_8;
              }
              newObj.ModelID = entry[3];
              uint behavior = bytesToInt(entry, 4, 4);
              newObj.setBehaviorFromAddress(behavior);
              newObj.DontShowActs();
              if (behavior != 0)
                lvl.getCurrentArea().SpecialObjects.Add(newObj);
              off += obj_len;
            }
            break;
          case CollisionMapReader.CollisionSubCommand.TERRAIN_LOAD_ENVIRONMENT:
            // Also skipping water boxes. Will come back to it later.
            uint num_boxes = (ushort) bytesToInt(data, (int) off + 2, 2);
            off += 4 + (num_boxes * 0xC);
            break;
        }
      }
    }

    private static void CMD_2F(ref Level lvl,
                               ref string desc,
                               byte[] cmd,
                               byte? areaID) {
      ROM rom = ROM.Instance;
      Console.WriteLine(bytesToInt(cmd, 4, 4).ToString("X8"));
      byte seg = cmd[4];
      uint off = bytesToInt(cmd, 5, 3);
      uint len = lvl.getCurrentArea().collision.getTriangleCount();
      //byte[] data = rom.getSubArray_safe(rom.getSegment(seg, areaID), off, len);
      //Console.WriteLine("Num triangles = 0x" + len.ToString("X8") + ": ");
      //rom.printArray(data);
    }

    private static byte[] getSpecialObjectEntry(byte presetID) {
      ROM rom = ROM.Instance;
      byte[] data = new byte[8];
      uint offset = Globals.MemoryConstants.SpecialPresetTable;
      byte got = rom.Bytes[offset];
      while (got != 0xFF) {
        if (got == presetID) {
          Array.Copy(rom.Bytes, offset, data, 0, 8);
          break;
        }
        offset += 8;
        got = rom.Bytes[offset];
      }

      return data;
    }

    private static uint getSpecialObjectLength(int obj) {
      if (obj > 0x64 && obj < 0x79) return 10;
      else if (obj > 0x78 && obj < 0x7E) return 8;
      else if (obj > 0x7D && obj < 0x83) return 10;
      else if (obj > 0x88 && obj < 0x8E) return 10;
      else if (obj > 0x82 && obj < 0x8A) return 12;
      else if (obj == 0x40) return 10;
      else if (obj == 0x64) return 12;
      else if (obj == 0xC0) return 8;
      else if (obj == 0xE0) return 12;
      else if (obj == 0xCD) return 12;
      else if (obj == 0x00) return 10;
      return 8;
    }

    private static void CMD_39(
        IReadOnlySm64Memory sm64Memory,
        ref Level lvl,
        ref string desc,
        byte[] cmd) {
      if (cmd.Length < 8)
        return;
      uint pos = bytesToInt(cmd, 4, 4);

      desc = "Place macro objects loaded from address 0x" + pos.ToString("X8");

      ROM rom = ROM.Instance;
      using var br = sm64Memory.OpenAtSegmentedAddress(pos);

      lvl.getCurrentArea().MacroObjects.Clear();
      bool endList = false;
      while (!endList) {
        //rom.printArray(data, 10);
        var firstAndSecond = br.ReadUInt16();
        uint id = (uint) (firstAndSecond & 0x1FF);
        if (id == 0 || id == 0x1E) break;
        Object3D newObj = new Object3D();
        if (rom.isSegmentMIO0(cmd[4], sm64Memory.AreaId)) {
          newObj.MakeReadOnly();
          newObj.Address = "N/A";
        } else {
          newObj.Address =
              "0x" + rom.decodeSegmentAddress(pos, sm64Memory.AreaId)
                        .ToString("X");
        }

        uint table_off = (id - 0x1F) * 8;
        byte[] entryData =
            rom.getSubArray_safe(rom.Bytes,
                                 Globals.MemoryConstants.MacroPresetTable + table_off, 8);
        newObj.level = lvl;
        newObj.createdFromLevelScriptCommand = Object3D.FROM_LS_CMD.CMD_39;
        newObj.setBehaviorFromAddress(bytesToInt(entryData, 0, 4));
        newObj.HideProperty(Object3D.FLAGS.ROTATION_X);
        newObj.HideProperty(Object3D.FLAGS.ROTATION_Z);
        newObj.HideProperty(Object3D.FLAGS.BPARAM_3);
        newObj.HideProperty(Object3D.FLAGS.BPARAM_4);
        newObj.ModelID = entryData[5];
        newObj.setPresetID((ushort) id);
        newObj.yRot = (short) ((firstAndSecond >> 9) * 2.8125);
        newObj.xPos = br.ReadInt16();
        newObj.yPos = br.ReadInt16();
        newObj.zPos = br.ReadInt16();
        newObj.DontShowActs();
        newObj.MakeBehaviorReadOnly(true);
        newObj.MakeModelIDReadOnly(true);

        var bp1 = br.ReadByte();
        if (bp1 != 0)
          newObj.BehaviorParameter1 = bp1;
        else
          newObj.BehaviorParameter1 = entryData[6];

        var bp2 = br.ReadByte();
        if (bp2 != 0)
          newObj.BehaviorParameter2 = bp2;
        else
          newObj.BehaviorParameter2 = entryData[7];

        lvl.getCurrentArea().MacroObjects.Add(newObj);
      }

      //uint end = bytesToInt(cmd, 8, 4);
      //rom.setSegment(seg, start, end, false);
    }

    private static bool isPerAreaBank0E(byte[] segData) {
      if (segData.Length < 0x6000) return false;
      uint offset = 0x5FFC;
      return ((segData[0 + offset] << 24 | segData[1 + offset] << 16 |
               segData[2 + offset] << 8 | segData[3 + offset]) == 0x4BC9189A);
    }

    private static void setAreaSegmented0xE(byte areaID, byte[] segData) {
      if (!isPerAreaBank0E(segData))
        return;

      uint start, end;

      uint offset = 0x5F00 + (uint) areaID * 0x10;
      start = (uint) ((segData[offset] << 24) | (segData[offset + 1] << 16) |
                      (segData[offset + 2] << 8) | segData[offset + 3]);

      offset += 4;
      end = (uint) ((segData[offset] << 24) | (segData[offset + 1] << 16) |
                    (segData[offset + 2] << 8) | segData[offset + 3]);

      ROM.Instance.setSegment(0xE, start, end, false, areaID);
    }
  }
}