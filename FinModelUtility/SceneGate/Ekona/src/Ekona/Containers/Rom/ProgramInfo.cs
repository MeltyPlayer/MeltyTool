// Copyright(c) 2021 SceneGate
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.Collections.ObjectModel;
using SceneGate.Ekona.Security;
using Yarhl.FileFormat;

namespace SceneGate.Ekona.Containers.Rom
{
    /// <summary>
    /// Information about the content of the program.
    /// </summary>
    public class ProgramInfo : IFormat
    {
        /// <summary>
        /// Gets the minimum cartridge size.
        /// </summary>
        public static long MinimumCartridgeSize => 1 << 17;

        /// <summary>
        /// Gets or sets the short game title.
        /// </summary>
        public string GameTitle { get; set; }

        /// <summary>
        /// Gets or sets the game code.
        /// </summary>
        public string GameCode { get; set; }

        /// <summary>
        /// Gets or sets the maker code.
        /// </summary>
        public string MakerCode { get; set; }

        /// <summary>
        /// Gets or sets the unit code.
        /// </summary>
        public DeviceUnitKind UnitCode { get; set; }

        /// <summary>
        /// Gets or sets the index to encryption seed byte for KEY2.
        /// </summary>
        public byte EncryptionSeed { get; set; }

        /// <summary>
        /// Gets or sets the size of the ROM cartridge.
        /// </summary>
        public long CartridgeSize { get; set; }

        /// <summary>
        /// Gets or sets the modcrypt flags for DSi.
        /// </summary>
        public DsiCryptoMode DsiCryptoFlags { get; set; }

        /// <summary>
        /// Gets or sets the supported region.
        /// </summary>
        public ProgramRegions Region { get; set; }

        /// <summary>
        /// Gets or sets the version of the program.
        /// </summary>
        public byte Version { get; set; }

        /// <summary>
        /// Gets or sets the flags to skip parts of the autostart.
        /// </summary>
        public byte AutoStartFlag { get; set; }

        /// <summary>
        /// Gets or sets the entry address of the ARM-9.
        /// </summary>
        public uint Arm9EntryAddress { get; set; }

        /// <summary>
        /// Gets or sets the address of the ARM-9 in the RAM.
        /// </summary>
        public uint Arm9RamAddress { get; set; }

        /// <summary>
        /// Gets or sets the entry address of the ARM-7.
        /// </summary>
        public uint Arm7EntryAddress { get; set; }

        /// <summary>
        /// Gets or sets the address of the ARM-7 in the RAM.
        /// </summary>
        public uint Arm7RamAddress { get; set; }

        /// <summary>
        /// Gets or sets the control register flags for read.
        /// </summary>
        public uint FlagsRead { get; set; }

        /// <summary>
        /// Gets or sets the control register flags for init.
        /// </summary>
        public uint FlagsInit { get; set; }

        /// <summary>
        /// Gets or sets the checksum of the ARM-9 secure area.
        /// </summary>
        public HashInfo ChecksumSecureArea { get; set; }

        /// <summary>
        /// Gets or sets the secure area delay in 131kHz units.
        /// </summary>
        public ushort SecureAreaDelay { get; set; }

        /// <summary>
        /// Gets or sets the auto-load address of the ARM-9.
        /// </summary>
        public uint Arm9Autoload { get; set; }

        /// <summary>
        /// Gets or sets the auto-load address of the ARM-7.
        /// </summary>
        public uint Arm7Autoload { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the ARM9 secure are encryption is disabled.
        /// </summary>
        public bool DisableSecureArea { get; set; }

        /// <summary>
        /// Gets or sets the checksum of the copyright logo.
        /// </summary>
        public HashInfo ChecksumLogo { get; set; }

        /// <summary>
        /// Gets or sets the checksum of the header.
        /// </summary>
        public HashInfo ChecksumHeader { get; set; }

        /// <summary>
        /// Gets or sets the debug ROM offset.
        /// </summary>
        public uint DebugRomOffset { get; set; }

        /// <summary>
        /// Gets or sets the debug ROM size.
        /// </summary>
        public uint DebugSize { get; set; }

        /// <summary>
        /// Gets or sets the debug ROM location in the RAM.
        /// </summary>
        public uint DebugRamAddress { get; set; }

        /// <summary>
        /// Gets or sets the offset in the ARM9 pointing to some parameter values.
        /// Relative to the base RAM address for ARM9.
        /// </summary>
        /// <remarks>
        /// Only for DSi games and DS games released after the DSi. Otherwise 0.
        /// </remarks>
        public uint Arm9ParametersTableOffset { get; set; }

        /// <summary>
        /// Gets or sets the offset in the ARM7 pointing to some parameter values.
        /// Relative to the base RAM address for ARM7.
        /// </summary>
        /// <remarks>
        /// Only for DSi games and DS games released after the DSi. Otherwise 0.
        /// </remarks>
        public uint Arm7ParametersTableOffset { get; set; }

        /// <summary>
        /// Gets or sets flags related to ROM features.
        /// </summary>
        /// <remarks>
        /// Only for DSi games and DS games released after the DSi. Otherwise 0.
        /// </remarks>
        public DsiRomFeatures ProgramFeatures { get; set; }

        /// <summary>
        /// Gets or sets the SHA-1 HMAC of the banner.
        /// </summary>
        /// <remarks>
        /// Only for DSi games and DS games released after the DSi. Otherwise null.
        /// </remarks>
        public HashInfo BannerMac { get; set; }

        /// <summary>
        /// Gets or sets the SHA-1 HMAC of the header, encrypted ARM9 and ARM7 programs.
        /// </summary>
        /// <remarks>
        /// Only for DS games released after the DSi. Otherwise null.
        /// </remarks>
        public HashInfo NitroProgramMac { get; set; }

        /// <summary>
        /// Gets or sets the SHA-1 HMAC of the overlays of the ARM9.
        /// </summary>
        /// <remarks>
        /// Only for DS games released after the DSi. Otherwise null.
        /// </remarks>
        public HashInfo NitroOverlaysMac { get; set; }

        /// <summary>
        /// Gets or sets the RSA SHA-1 signature of the ROM header.
        /// </summary>
        /// <remarks>
        /// Only for DSi games and DS games released after the DSi. Otherwise null.
        /// </remarks>
        public HashInfo Signature { get; set; }

        /// <summary>
        /// Gets or sets the information exclusive to DSi programs.
        /// </summary>
        public DsiProgramInfo DsiInfo { get; set; } = new DsiProgramInfo();

        /// <summary>
        /// Gets or sets the program code (arm9) parameters.
        /// </summary>
        public NitroProgramCodeParameters ProgramCodeParameters { get; set; } = new NitroProgramCodeParameters();

        /// <summary>
        /// Gets or sets the collection of information of overlays for ARM-9.
        /// </summary>
        public Collection<OverlayInfo> Overlays9Info { get; set; } = [];

        /// <summary>
        /// Gets or sets the collection of information of overlays for ARM-7.
        /// </summary>
        public Collection<OverlayInfo> Overlays7Info { get; set; } = [];
    }
}
