 namespace UoT.hacks.displaylist;

 public class LinkChildDisplayListHack {
   /// <summary>
   ///   The offset of the display list to render for Link's left hand. For
   ///   the debug rom, this is indirectly stored in RDRAM at 0x224714. This
   ///   value seems to be a pointer that points to another RDRAM location,
   ///   which points to a standard address w/ segment + offset.
   /// </summary>
   public enum LeftHandDisplayList {
     // Stored in RDRAM at 0x125e0c
     EMPTY = 0x013cb0,

     // Stored in RDRAM @ 0x125e3c
     SWORD = 0x013f38,

     // Stored in RDRAM @ 0x125efc
     BOOMERANG = 0x014660,
   }

   /// <summary>
   ///   The offset of the display list to render for Link's right hand. For
   ///   the debug rom, this is indirectly stored in RDRAM at 0x224710. This
   ///   value seems to be a pointer that points to another RDRAM location,
   ///   which points to a standard address w/ segment + offset.
   /// </summary>
   public enum RightHandDisplayList {
     // Stored in RDRAM @ 0x125e4c
     EMPTY = 0x0141c0, // TODO: Is this right?

     // Stored in RDRAM @ 0x125e6c
     SLINGSHOT = 0x015DF0,

     // Stored in RDRAM @ 0x125ec4
     FAIRY_OCARINA = 0x0,

     // Stored in RDRAM @ 0x125ecc
     OCARINA_OF_TIME = 0x0,
   }

   /// <summary>
   ///   The offset of the display list to render for Link's right hand. For
   ///   the debug rom, this is indirectly stored in RDRAM at 0x224718. This
   ///   value seems to be a pointer that points to another RDRAM location,
   ///   which points to a standard address w/ segment + offset.
   /// </summary>
   public enum SheathState {
     // Stored in RDRAM @ 0x125d2c
     WITH_SWORD = 0x015248,

     // Stored in RDRAM @ 0x125d8c
     WITHOUT_SWORD = 0x015408,
   }

   public uint MapDisplayListAddress(uint originalAddress) {
      return originalAddress;
    }
 }