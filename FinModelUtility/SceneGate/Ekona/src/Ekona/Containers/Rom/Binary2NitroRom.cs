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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using SceneGate.Ekona.Security;
using Yarhl.FileFormat;
using Yarhl.FileSystem;
using Yarhl.IO;

namespace SceneGate.Ekona.Containers.Rom
{
    /// <summary>
    /// Converter for binary formats into a NitroRom container.
    /// </summary>
    public class Binary2NitroRom : IConverter<IBinary, NitroRom>, IInitializer<DsiKeyStore>
    {
        private const int SecureAreaLength = 16 * 1024;
        private static readonly FileAddressOffsetComparer FileAddressComparer = new FileAddressOffsetComparer();

        private DsiKeyStore keyStore;

        private DataReader reader;
        private NitroRom rom;
        private RomHeader header;
        private FileAddress[] addresses;
        private List<FileAddress> addressesByOffset;

        /// <summary>
        /// Initializes the converter with the key store to validate signatures.
        /// </summary>
        /// <param name="parameters">The key store.</param>
        public void Initialize(DsiKeyStore parameters)
        {
            keyStore = parameters;
        }

        /// <summary>
        /// Read the internal info of a ROM file.
        /// </summary>
        /// <param name="source">Source binary format to read from.</param>
        /// <returns>The new node container.</returns>
        public NitroRom Convert(IBinary source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            source.Stream.Position = 0;
            reader = new DataReader(source.Stream);
            rom = new NitroRom();

            ReadHeader();
            ReadBanner();
            ReadFat();
            ReadPrograms();
            ReadProgramCodeParameters();
            ReadFileSystem();
            ValidateSignatures();

            return rom;
        }

        private void ReadHeader()
        {
            if (reader.Stream.Length < Binary2RomHeader.HeaderSizeOffset + 4) {
                throw new EndOfStreamException("Stream is smaller than the ROM header");
            }

            reader.Stream.Seek(Binary2RomHeader.HeaderSizeOffset, SeekOrigin.Begin);
            int headerSize = reader.ReadInt32();
            using var binaryHeader = new BinaryFormat(reader.Stream, 0, headerSize);
            header = (RomHeader)ConvertFormat.With<Binary2RomHeader>(binaryHeader);

            rom.System.Children["info"].ChangeFormat(header.ProgramInfo);

            var binaryStream = DataStreamFactory.FromArray(header.CopyrightLogo, 0, header.CopyrightLogo.Length);
            var binaryLogo = new BinaryFormat(binaryStream);
            rom.System.Children["copyright_logo"].ChangeFormat(binaryLogo);
        }

        private void ReadBanner()
        {
            if (header.SectionInfo.BannerOffset == 0) {
                return;
            }

            reader.Stream.Position = header.SectionInfo.BannerOffset;
            int bannerSize = Binary2Banner.GetSize(reader.Stream);
            var binaryBanner = new BinaryFormat(reader.Stream, header.SectionInfo.BannerOffset, bannerSize);
            rom.System.Children["banner"].ChangeFormat(binaryBanner);
            rom.System.Children["banner"].TransformWith<Binary2Banner>();
        }

        private void ReadFat()
        {
            int numFiles = (int)(header.SectionInfo.FatSize / 0x08);
            addresses = new FileAddress[numFiles];

            reader.Stream.Position = header.SectionInfo.FatOffset;
            for (int i = 0; i < numFiles; i++) {
                uint offset = reader.ReadUInt32();
                uint endOffset = reader.ReadUInt32();
                addresses[i] = new FileAddress {
                    Index = i,
                    Offset = offset,
                    Size = endOffset - offset,
                };
            }

            addressesByOffset = addresses.ToList();
            addressesByOffset.Sort(FileAddressComparer);
        }

        private void ReadPrograms()
        {
            var arm9Stream = GetOrModcrypt(
                reader.Stream,
                header.SectionInfo.Arm9Offset,
                header.SectionInfo.Arm9Size,
                ModcryptTargetKind.Arm9);
            var arm9 = new BinaryFormat(arm9Stream);
            rom.System.Children["arm9"].ChangeFormat(arm9);
            ReadOverlayTable(
                header.ProgramInfo.Overlays9Info,
                header.SectionInfo.Overlay9TableOffset,
                header.SectionInfo.Overlay9TableSize);

            var arm7Stream = GetOrModcrypt(
                reader.Stream,
                header.SectionInfo.Arm7Offset,
                header.SectionInfo.Arm7Size,
                ModcryptTargetKind.Arm7);
            var arm7 = new BinaryFormat(arm7Stream);
            rom.System.Children["arm7"].ChangeFormat(arm7);
            ReadOverlayTable(
                header.ProgramInfo.Overlays7Info,
                header.SectionInfo.Overlay7TableOffset,
                header.SectionInfo.Overlay7TableSize);

            if (header.ProgramInfo.UnitCode != DeviceUnitKind.DS) {
                var arm9iStream = GetOrModcrypt(
                    reader.Stream,
                    header.SectionInfo.Arm9iOffset,
                    header.SectionInfo.Arm9iSize,
                    ModcryptTargetKind.Arm9i,
                    ModcryptTargetKind.Arm9iSecureArea);
                var arm9i = new BinaryFormat(arm9iStream);
                rom.System.Add(new Node("arm9i", arm9i));

                var arm7iStream = GetOrModcrypt(
                    reader.Stream,
                    header.SectionInfo.Arm7iOffset,
                    header.SectionInfo.Arm7iSize,
                    ModcryptTargetKind.Arm7i);
                var arm7i = new BinaryFormat(arm7iStream);
                rom.System.Add(new Node("arm7i", arm7i));
            }
        }

        private DataStream GetOrModcrypt(Stream source, long offset, long length, params ModcryptTargetKind[] kinds)
        {
            if (header.ProgramInfo.UnitCode == DeviceUnitKind.DS) {
                return new DataStream(source, offset, length);
            }

            int area;
            uint modcryptLength;
            if (kinds.Any(k => header.ProgramInfo.DsiInfo.ModcryptArea1Target == k)) {
                area = 1;
                modcryptLength = header.SectionInfo.ModcryptArea1Length;
            } else if (kinds.Any(k => header.ProgramInfo.DsiInfo.ModcryptArea2Target == k)) {
                area = 2;
                modcryptLength = header.SectionInfo.ModcryptArea2Length;
            } else {
                return new DataStream(source, offset, length);
            }

            var modcrypt = Modcrypt.Create(header.ProgramInfo, area);
            using var input = new DataStream(source, offset, modcryptLength);
            var output = modcrypt.Transform(input);

            // For instance if only the secure area of ARM9i is modcrypted. Copy the rest of ARM9i program.
            if (length > modcryptLength) {
                long remaining = length - modcryptLength;
                using var remainingInput = new DataStream(source, offset + modcryptLength, remaining);
                remainingInput.WriteTo(output);
            }

            return output;
        }

        private void ReadProgramCodeParameters()
        {
            var programParams = header.ProgramInfo.ProgramCodeParameters;
            uint paramsOffset = header.ProgramInfo.Arm9ParametersTableOffset;

            reader.Stream.Position = header.SectionInfo.Arm9Offset + header.SectionInfo.Arm9Size;
            if (reader.ReadUInt32() == NitroRom.NitroCode) {
                programParams.ProgramParameterOffset = reader.ReadUInt32();
                programParams.NitroOverlayHMacOffset = reader.ReadUInt32();
                paramsOffset = programParams.ProgramParameterOffset;
            }

            if (paramsOffset == 0) {
                return;
            }

            reader.Stream.Position = header.SectionInfo.Arm9Offset + paramsOffset;
            programParams.ItcmBlockInfoOffset = reader.ReadUInt32();
            programParams.ItcmBlockInfoEndOffset = reader.ReadUInt32();
            programParams.ItcmInputDataOffset = reader.ReadUInt32();
            programParams.BssOffset = reader.ReadUInt32();
            programParams.BssEndOffset = reader.ReadUInt32();
            programParams.CompressedLength = reader.ReadUInt32() - header.ProgramInfo.Arm9RamAddress;

            ushort build = reader.ReadUInt16();
            byte minor = reader.ReadByte();
            byte major = reader.ReadByte();
            programParams.SdkVersion = new Version(major, minor, build);
        }

        private void ReadOverlayTable(Collection<OverlayInfo> infos, uint offset, int size)
        {
            reader.Stream.Position = offset;
            int numFiles = size / 0x20;
            for (int i = 0; i < numFiles; i++) {
                var overlayInfo = new OverlayInfo();
                overlayInfo.OverlayId = reader.ReadUInt32();
                overlayInfo.RamAddress = reader.ReadUInt32();
                overlayInfo.RamSize = reader.ReadUInt32();
                overlayInfo.BssSize = reader.ReadUInt32();
                overlayInfo.StaticInitStart = reader.ReadUInt32();
                overlayInfo.StaticInitEnd = reader.ReadUInt32();
                overlayInfo.OverlayId = reader.ReadUInt32();
                uint encodingInfo = reader.ReadUInt32();
                overlayInfo.CompressedSize = encodingInfo & 0x00FFFFFF;
                overlayInfo.IsCompressed = ((encodingInfo >> 24) & 0x01) == 1;
                overlayInfo.IsSigned = ((encodingInfo >> 24) & 0x02) == 2;

                infos.Add(overlayInfo);

                var fileInfo = addresses[overlayInfo.OverlayId];
                var binaryOverlay = new BinaryFormat(reader.Stream, fileInfo.Offset, fileInfo.Size);
                var overlay = new Node($"overlay_{i}", binaryOverlay);
                overlay.Tags.Add("scenegate.ekona.id", overlayInfo.OverlayId);
                rom.System.Children["overlays9"].Add(overlay);
            }
        }

        private void ReadFileSystem()
        {
            var encoding = Encoding.GetEncoding("shift_jis");
            var directoryQueue = new Queue<(int Id, Node Current)>();

            rom.Data.Tags["scenegate.ekona.id"] = 0xF000;
            directoryQueue.Enqueue((0xF000, rom.Data));

            while (directoryQueue.Count > 0) {
                var info = directoryQueue.Dequeue();

                // Read directory sub-entries definition
                reader.Stream.Position = header.SectionInfo.FntOffset + ((info.Id & 0xFFF) * 8);
                uint entriesOffset = reader.ReadUInt32();
                int fileId = reader.ReadUInt16();
                reader.ReadUInt16(); // parent ID

                // Read each entry data
                reader.Stream.Position = header.SectionInfo.FntOffset + entriesOffset;
                byte nodeType = reader.ReadByte();
                while (nodeType != 0) {
                    bool isFile = (nodeType & 0x80) == 0;
                    int nameLength = nodeType & 0x7F;
                    string name = reader.ReadString(nameLength, encoding);

                    Node node;
                    int id;
                    if (isFile) {
                        id = fileId++;
                        FileAddress fileInfo = addresses[id];
                        var fileData = new BinaryFormat(reader.Stream, fileInfo.Offset, fileInfo.Size);
                        node = new Node(name, fileData);
                        node.Tags["scenegate.ekona.physical_id"] = addressesByOffset.BinarySearch(fileInfo, FileAddressComparer);
                    } else {
                        id = reader.ReadUInt16();
                        node = NodeFactory.CreateContainer(name);
                        directoryQueue.Enqueue((id, node));
                    }

                    node.Tags["scenegate.ekona.id"] = id;
                    info.Current.Add(node);

                    nodeType = reader.ReadByte();
                }
            }
        }

        private void ValidateSignatures()
        {
            if (keyStore is null) {
                return;
            }

            ProgramInfo programInfo = header.ProgramInfo;
            bool isDsi = programInfo.UnitCode != DeviceUnitKind.DS;
            var key1Encryption = new NitroKey1Encryption(programInfo.GameCode, keyStore);
            var hashGenerator = new TwilightHMacGenerator(keyStore);
            var crcGenerator = new NitroCrcGenerator();

            // Get ARM9 encrypted (or encrypt it) since it's used for several CRC / hashes
            DataStream arm9 = rom.System.Children["arm9"].Stream!;
            using DataStream encryptedArm9 = key1Encryption.HasEncryptedArm9(arm9)
                ? new DataStream(arm9)
                : key1Encryption.EncryptArm9(arm9);

            // Header secure area CRC
            byte[] secureAreaCrc = crcGenerator.GenerateCrc16(encryptedArm9, 0, SecureAreaLength);
            programInfo.ChecksumSecureArea.Validate(secureAreaCrc);

            // HMAC for phase 1 and 2 of DS games
            bool checkPhase12 = programInfo.ProgramFeatures.HasFlag(DsiRomFeatures.NitroProgramSigned);
            if (checkPhase12 && keyStore.HMacKeyWhitelist12 is { Length: > 0 }) {
                byte[] phase1Hash = hashGenerator.GeneratePhase1Hmac(reader.Stream, encryptedArm9, header.SectionInfo);
                programInfo.NitroProgramMac.Validate(phase1Hash);

                byte[] phase2Hash = hashGenerator.GeneratePhase2Hmac(reader.Stream, header.SectionInfo);
                programInfo.NitroOverlaysMac.Validate(phase2Hash);
            }

            // HMAC for banner of DS (phase 3) and DSi games
            byte[] bannerKey = isDsi ? keyStore.HMacKeyDSiGames : keyStore.HMacKeyWhitelist34;
            bool checkBannerHmac = isDsi || programInfo.ProgramFeatures.HasFlag(DsiRomFeatures.NitroBannerSigned);
            if (checkBannerHmac && bannerKey is { Length: > 0 }) {
                byte[] actualHash = hashGenerator.GeneratePhase3Hmac(reader.Stream, programInfo, header.SectionInfo);
                programInfo.BannerMac.Validate(actualHash);
            }

            // ROM signature of DS and DSi games
            bool checkSignature = isDsi || programInfo.ProgramFeatures.HasFlag(DsiRomFeatures.NitroProgramSigned);
            if (checkSignature && keyStore.PublicModulusRetailGames is { Length: > 0 }) {
                var signer = new TwilightSigner(keyStore.PublicModulusRetailGames);
                programInfo.Signature.Status = signer.VerifySignature(programInfo.Signature.Hash, reader.Stream);
            }

            if (isDsi && keyStore.HMacKeyDSiGames is { Length: > 0 }) {
                byte[] arm9EncryptedHash = hashGenerator.GenerateHmac(encryptedArm9);
                programInfo.DsiInfo.Arm9SecureMac.Validate(arm9EncryptedHash);

                byte[] arm7Hash = hashGenerator.GenerateHmac(rom.System.Children["arm7"].Stream);
                programInfo.DsiInfo.Arm7Mac.Validate(arm7Hash);

                byte[] digestHash = hashGenerator.GenerateDigestBlockHmac(reader.Stream, header.SectionInfo);
                programInfo.DsiInfo.DigestMain.Validate(digestHash);

                byte[] arm9iHash = hashGenerator.GenerateHmac(rom.System.Children["arm9i"].Stream);
                programInfo.DsiInfo.Arm9iMac.Validate(arm9iHash);

                byte[] arm7iHash = hashGenerator.GenerateHmac(rom.System.Children["arm7i"].Stream);
                programInfo.DsiInfo.Arm7iMac.Validate(arm7iHash);

                byte[] arm9Hash = hashGenerator.GenerateArm9NoSecureAreaHmac(reader.Stream, header.SectionInfo);
                programInfo.DsiInfo.Arm9Mac.Validate(arm9Hash);

                HashStatus digestBlockStatus = hashGenerator.VerifyDigestBlock(reader.Stream, header.SectionInfo);
                if (digestBlockStatus == HashStatus.Valid) {
                    digestBlockStatus = hashGenerator.VerifyDigestSectionContent(
                        reader.Stream,
                        encryptedArm9,
                        rom.System,
                        header.SectionInfo);
                }

                programInfo.DsiInfo.DigestHashesStatus = digestBlockStatus;
            }
        }

        private struct FileAddress
        {
            public uint Offset { get; init; }

            public uint Size { get; init; }

            public int Index { get; init; }
        }

        private sealed class FileAddressOffsetComparer : IComparer<FileAddress>
        {
            public int Compare(FileAddress x, FileAddress y)
            {
                int offsetComparison = x.Offset.CompareTo(y.Offset);
                if (offsetComparison != 0) return offsetComparison;
                return x.Index.CompareTo(y.Index);
            }
        }
    }
}
