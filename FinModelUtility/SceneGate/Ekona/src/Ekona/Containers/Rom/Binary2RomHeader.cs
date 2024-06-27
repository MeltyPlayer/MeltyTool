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

using System;
using SceneGate.Ekona.Security;
using Yarhl.FileFormat;
using Yarhl.IO;

namespace SceneGate.Ekona.Containers.Rom
{
    /// <summary>
    /// Converter for binary ROM header into an object.
    /// </summary>
    public class Binary2RomHeader :
        IInitializer<DsiKeyStore>,
        IConverter<IBinary, RomHeader>
    {
        private const int SecureAreaLength = 16 * 1024;
        private DsiKeyStore keyStore;

        /// <summary>
        /// Gets the offset in the header containing the header size value.
        /// </summary>
        public static int HeaderSizeOffset => 0x84;

        /// <summary>
        /// Initialize the converter with the key store to enable additional features.
        /// </summary>
        /// <remarks>
        /// The key store is used to verify a special token and set the value of `DisableSecureArea`.
        /// Otherwise, it will always be `false`. The required key is `BlowfishDsKey`.
        /// </remarks>
        /// <param name="parameters">The converter parameters.</param>
        /// <exception cref="ArgumentNullException">The argument is null.</exception>
        public void Initialize(DsiKeyStore parameters)
        {
            keyStore = parameters ?? throw new ArgumentNullException(nameof(parameters));
        }

        /// <summary>
        /// Convert a binary format into a ROM header object.
        /// </summary>
        /// <param name="source">The stream to read.</param>
        /// <returns>The new ROM header.</returns>
        public RomHeader Convert(IBinary source)
        {
            ArgumentNullException.ThrowIfNull(source);

            source.Stream.Position = 0;
            var reader = new DataReader(source.Stream);
            var header = new RomHeader();

            ReadDsFields(reader, header);

            if (header.ProgramInfo.UnitCode != DeviceUnitKind.DS) {
                ReadDsiFields(reader, header);
            }

            ValidateChecksums(reader.Stream, header.ProgramInfo);

            return header;
        }

        private static void ValidateChecksums(DataStream stream, ProgramInfo info)
        {
            // We don't validate the checksum of the secure area as it's outside the header. Same for HMAC.
            var crcGen = new NitroCrcGenerator();
            info.ChecksumLogo.Validate(crcGen.GenerateCrc16(stream, 0xC0, 0x9C));
            info.ChecksumHeader.Validate(crcGen.GenerateCrc16(stream, 0x00, 0x15E));
        }

        private static void ReadDsiFields(DataReader reader, RomHeader header)
        {
            DsiProgramInfo info = header.ProgramInfo.DsiInfo;
            RomSectionInfo sections = header.SectionInfo;

            // Pos: 0x180
            reader.Stream.Position = 0x180;
            info.GlobalMemoryBanks = ReadGlobalMemoryBankSettings(reader);

            // Pos: 0x194
            info.LocalMemoryBanksArm9 = ReadLocalMemoryBankSettings(reader);

            // Pos: 0x1A0
            info.LocalMemoryBanksArm7 = ReadLocalMemoryBankSettings(reader);
            info.GlobalMbk9Settings = reader.ReadInt24();
            info.GlobalWramCntSettings = reader.ReadByte();

            // Pos: 0x1B0
            header.ProgramInfo.Region = (ProgramRegions)reader.ReadUInt32();
            info.AccessControl = (TwilightAccessControl)reader.ReadUInt32();
            info.ScfgExtendedArm7 = (ScfgExtendedFeaturesArm7)reader.ReadUInt32();
            reader.Stream.Position += 4; // ProgramFeatures flags read with DS fields

            // Pos: 0x1C0
            sections.Arm9iOffset = reader.ReadUInt32();
            reader.Stream.Position += 4; // reserved
            info.Arm9iRamAddress = reader.ReadUInt32();
            sections.Arm9iSize = reader.ReadUInt32();

            // Pos: 0x1D0
            sections.Arm7iOffset = reader.ReadUInt32();
            info.StorageDeviceListArm7RamAddress = reader.ReadUInt32();
            info.Arm7iRamAddress = reader.ReadUInt32();
            sections.Arm7iSize = reader.ReadUInt32();

            // Pos: 0x1E0
            sections.DigestNitroOffset = reader.ReadUInt32();
            sections.DigestNitroLength = reader.ReadUInt32();
            sections.DigestTwilightOffset = reader.ReadUInt32();
            sections.DigestTwilightLength = reader.ReadUInt32();

            // Pos: 0x1F0
            sections.DigestSectorHashtableOffset = reader.ReadUInt32();
            sections.DigestSectorHashtableLength = reader.ReadUInt32();
            sections.DigestBlockHashtableOffset = reader.ReadUInt32();
            sections.DigestBlockHashtableLength = reader.ReadUInt32();

            // Pos: 0x200
            sections.DigestSectorSize = reader.ReadUInt32();
            sections.DigestBlockSectorCount = reader.ReadUInt32();
            sections.BannerLength = reader.ReadUInt32();
            info.StorageShared20Length = reader.ReadByte();
            info.StorageShared21Length = reader.ReadByte();
            info.EulaVersion = reader.ReadByte();
            info.UseRatings = reader.ReadByte();

            // Pos: 0x210
            sections.DsiRomLength = reader.ReadUInt32();
            info.StorageShared22Length = reader.ReadByte();
            info.StorageShared23Length = reader.ReadByte();
            info.StorageShared24Length = reader.ReadByte();
            info.StorageShared25Length = reader.ReadByte();
            info.Arm9iParametersTableOffset = reader.ReadUInt32();
            info.Arm7iParametersTableOffset = reader.ReadUInt32();

            // Pos: 0x220
            sections.ModcryptArea1Offset = reader.ReadUInt32();
            sections.ModcryptArea1Length = reader.ReadUInt32();
            info.ModcryptArea1Target = GetModcryptTarget(
                sections.ModcryptArea1Offset,
                sections.ModcryptArea1Length,
                sections);

            sections.ModcryptArea2Offset = reader.ReadUInt32();
            sections.ModcryptArea2Length = reader.ReadUInt32();
            info.ModcryptArea2Target = GetModcryptTarget(
                sections.ModcryptArea2Offset,
                sections.ModcryptArea2Length,
                sections);

            // Pos: 0x230
            info.TitleId = reader.ReadUInt64();
            info.StoragePublicSaveLength = reader.ReadUInt32();
            info.StoragePrivateSaveLength = reader.ReadUInt32();

            // Pos: 0x2F0
            reader.Stream.Position = 0x2F0;
            info.AgeRatingCero = DeserializeAgeRating(reader.ReadByte());
            info.AgeRatingEsrb = DeserializeAgeRating(reader.ReadByte());
            reader.Stream.Position++; // reserved
            info.AgeRatingUsk = DeserializeAgeRating(reader.ReadByte());
            info.AgeRatingPegiEurope = DeserializeAgeRating(reader.ReadByte());
            reader.Stream.Position++; // reserved
            info.AgeRatingPegiPortugal = DeserializeAgeRating(reader.ReadByte());
            info.AgeRatingPegiUk = DeserializeAgeRating(reader.ReadByte());
            info.AgeRatingAgcb = DeserializeAgeRating(reader.ReadByte());
            info.AgeRatingGrb = DeserializeAgeRating(reader.ReadByte());
            reader.Stream.Position += 6; // reserved

            // Pos: 0x300
            info.Arm9SecureMac = reader.ReadHmacSha1();
            info.Arm7Mac = reader.ReadHmacSha1();
            info.DigestMain = reader.ReadHmacSha1();
            reader.Stream.Position += 0x14; // Banner HMAC already read with DS fields
            info.Arm9iMac = reader.ReadHmacSha1();
            info.Arm7iMac = reader.ReadHmacSha1();
            reader.Stream.Position += 0x28; // HMAC for whitelist DS games
            info.Arm9Mac = reader.ReadHmacSha1();
        }

        private static GlobalMemoryBankSettings[] ReadGlobalMemoryBankSettings(DataReader reader)
        {
            var settings = new GlobalMemoryBankSettings[5 * 4];
            for (int i = 0; i < settings.Length; i++) {
                byte data = reader.ReadByte();
                settings[i] = new GlobalMemoryBankSettings {
                    Processor = (MemoryBankProcessor)(data & 0x3),
                    OffsetSlot = (byte)((data >> 2) & 0x7),
                    Enabled = (data >> 7) == 1,
                };
            }

            return settings;
        }

        private static LocalMemoryBankSettings[] ReadLocalMemoryBankSettings(DataReader reader)
        {
            var settings = new LocalMemoryBankSettings[3];

            // MBK6 - WRAM A
            uint mbk6 = reader.ReadUInt32();
            settings[0] = new LocalMemoryBankSettings {
                StartAddressSlot = (int)((mbk6 >> 4) & 0xFF),
                ImageSize = (int)((mbk6 >> 12) & 0x3),
                EndAddressSlot = (int)((mbk6 >> 20) & 0x1FF),
            };

            // MBK 7 and 8 - WRAM B and C
            for (int i = 0; i < 2; i++) {
                uint data = reader.ReadUInt32();
                settings[i + 1] = new LocalMemoryBankSettings {
                    StartAddressSlot = (int)((data >> 3) & 0x1FF),
                    ImageSize = (int)((data >> 12) & 0x3),
                    EndAddressSlot = (int)((data >> 19) & 0x3FF),
                };
            }

            return settings;
        }

        private static ModcryptTargetKind GetModcryptTarget(uint offset, uint length, RomSectionInfo info)
        {
            if (offset == 0)
                return ModcryptTargetKind.None;
            if (offset == info.Arm9Offset)
                return ModcryptTargetKind.Arm9;
            if (offset == info.Arm7Offset)
                return ModcryptTargetKind.Arm7;
            if (offset == info.Arm9iOffset && length == SecureAreaLength)
                return ModcryptTargetKind.Arm9iSecureArea;
            if (offset == info.Arm9iOffset)
                return ModcryptTargetKind.Arm9i;
            if (offset == info.Arm7iOffset)
                return ModcryptTargetKind.Arm7i;

            return ModcryptTargetKind.Unknown;
        }

        private static AgeRating DeserializeAgeRating(byte value) =>
            new AgeRating {
                Enabled = (value >> 7) == 1,
                Prohibited = (value >> 6) == 1,
                Age = value & 0x1F,
            };

    private void ReadDsFields(DataReader reader, RomHeader header)
        {
            // Pos: 0x00
            header.ProgramInfo.GameTitle = reader.ReadString(12).Replace("\0", string.Empty);
            header.ProgramInfo.GameCode = reader.ReadString(4);

            // Pos: 0x10
            header.ProgramInfo.MakerCode = reader.ReadString(2);
            header.ProgramInfo.UnitCode = (DeviceUnitKind)reader.ReadByte();
            header.ProgramInfo.EncryptionSeed = reader.ReadByte();
            header.ProgramInfo.CartridgeSize = ProgramInfo.MinimumCartridgeSize * (1 << reader.ReadByte());
            reader.Stream.Position += 7; // reserved
            header.ProgramInfo.DsiCryptoFlags = (DsiCryptoMode)reader.ReadByte();

            if (header.ProgramInfo.UnitCode == DeviceUnitKind.DS) {
                byte region = reader.ReadByte();
                header.ProgramInfo.Region = ProgramRegions.NitroBase;

                if ((region & 0x80) != 0) {
                    header.ProgramInfo.Region |= ProgramRegions.China;
                }

                if ((region & 0x40) != 0) {
                    header.ProgramInfo.Region |= ProgramRegions.Korea;
                }
            } else {
                header.ProgramInfo.DsiInfo.StartJumpKind = (ProgramStartJumpKind)reader.ReadByte();
            }

            header.ProgramInfo.Version = reader.ReadByte();
            header.ProgramInfo.AutoStartFlag = reader.ReadByte();

            // Pos: 0x20
            header.SectionInfo.Arm9Offset = reader.ReadUInt32();
            header.ProgramInfo.Arm9EntryAddress = reader.ReadUInt32();
            header.ProgramInfo.Arm9RamAddress = reader.ReadUInt32();
            header.SectionInfo.Arm9Size = reader.ReadUInt32();

            // Pos: 0x30
            header.SectionInfo.Arm7Offset = reader.ReadUInt32();
            header.ProgramInfo.Arm7EntryAddress = reader.ReadUInt32();
            header.ProgramInfo.Arm7RamAddress = reader.ReadUInt32();
            header.SectionInfo.Arm7Size = reader.ReadUInt32();

            // Pos: 0x40
            header.SectionInfo.FntOffset = reader.ReadUInt32();
            header.SectionInfo.FntSize = reader.ReadUInt32();
            header.SectionInfo.FatOffset = reader.ReadUInt32();
            header.SectionInfo.FatSize = reader.ReadUInt32();

            // Pos: 0x50
            header.SectionInfo.Overlay9TableOffset = reader.ReadUInt32();
            header.SectionInfo.Overlay9TableSize = reader.ReadInt32();
            header.SectionInfo.Overlay7TableOffset = reader.ReadUInt32();
            header.SectionInfo.Overlay7TableSize = reader.ReadInt32();

            // Pos: 0x60
            header.ProgramInfo.FlagsRead = reader.ReadUInt32();
            header.ProgramInfo.FlagsInit = reader.ReadUInt32();
            header.SectionInfo.BannerOffset = reader.ReadUInt32();
            header.ProgramInfo.ChecksumSecureArea = reader.ReadCrc16();
            header.ProgramInfo.SecureAreaDelay = reader.ReadUInt16();

            // Pos: 0x70
            header.ProgramInfo.Arm9Autoload = reader.ReadUInt32();
            header.ProgramInfo.Arm7Autoload = reader.ReadUInt32();

            if (keyStore is { BlowfishDsKey: { Length: > 0 } }) {
                var encryption = new NitroKey1Encryption(header.ProgramInfo.GameCode, keyStore);
                byte[] disableToken = reader.ReadBytes(8);
                header.ProgramInfo.DisableSecureArea = encryption.HasDisabledSecureArea(disableToken);
            } else {
                reader.Stream.Position += 8;
                header.ProgramInfo.DisableSecureArea = false;
            }

            // Pos: 0x80
            header.SectionInfo.RomSize = reader.ReadUInt32();
            header.SectionInfo.HeaderSize = reader.ReadUInt32();
            header.ProgramInfo.Arm9ParametersTableOffset = reader.ReadUInt32();
            header.ProgramInfo.Arm7ParametersTableOffset = reader.ReadUInt32();

            // Pos: 0x90
            header.SectionInfo.NitroRegionEnd = reader.ReadUInt16();
            header.SectionInfo.TwilightRegionStart = reader.ReadUInt16();

            // Pos: 0xC0
            reader.Stream.Position = 0xC0;
            header.CopyrightLogo = reader.ReadBytes(156);
            header.ProgramInfo.ChecksumLogo = reader.ReadCrc16();
            header.ProgramInfo.ChecksumHeader = reader.ReadCrc16();

            // Pos: 0x160
            header.ProgramInfo.DebugRomOffset = reader.ReadUInt32();
            header.ProgramInfo.DebugSize = reader.ReadUInt32();
            header.ProgramInfo.DebugRamAddress = reader.ReadUInt32();

            // Pos: 0x1BF
            reader.Stream.Position = 0x1BF;
            header.ProgramInfo.ProgramFeatures = (DsiRomFeatures)reader.ReadByte();

            // Pos: 0x33C
            reader.Stream.Position = 0x33C;
            header.ProgramInfo.BannerMac = reader.ReadHmacSha1();

            // Pos: 0x378
            reader.Stream.Position = 0x378;
            header.ProgramInfo.NitroProgramMac = reader.ReadHmacSha1();
            header.ProgramInfo.NitroOverlaysMac = reader.ReadHmacSha1();

            // Pos: 0xF80
            reader.Stream.Position = 0xF80;
            header.ProgramInfo.Signature = reader.ReadSignatureSha1Rsa();
        }
    }
}
