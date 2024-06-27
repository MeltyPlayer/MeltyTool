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
using System.IO;
using System.Text;
using SceneGate.Ekona.Security;
using Texim.Colors;
using Texim.Images;
using Texim.Palettes;
using Texim.Pixels;
using Yarhl.FileFormat;
using Yarhl.FileSystem;
using Yarhl.IO;

namespace SceneGate.Ekona.Containers.Rom
{
    /// <summary>
    /// Convert a binary format into a container with the banner information.
    /// </summary>
    /// <remarks>
    /// <para>Supported versions: 0.1, 0.2, 0.3 and 1.3 (except animated icons).</para>
    /// <para>The new container hierarchy is:</para>
    /// <list type="table">
    /// <item><term>/info</term><description>Program banner content.</description></item>
    /// <item><term>/icon</term><description>Program icon.</description></item>
    /// </list>
    /// </remarks>
    public class Binary2Banner : IConverter<IBinary, NodeContainerFormat>
    {
        private const int IconWidth = 32;
        private const int IconHeight = 32;

        /// <summary>
        /// Gets the maximum number of animated images.
        /// </summary>
        public static int NumAnimatedImages => 8;

        /// <summary>
        /// Gets the serialized size of the banner including padding.
        /// </summary>
        /// <param name="stream">The stream to analyze.</param>
        /// <remarks>
        /// The stream must be in the start of the banner.
        /// The position is restored to the start of the banner.
        /// </remarks>
        /// <returns>The expected size of the binary banner.</returns>
        public static int GetSize(Stream stream)
        {
            byte minor = (byte)stream.ReadByte();
            byte major = (byte)stream.ReadByte();
            stream.Position -= 2;

            var version = new Version(major, minor);
            return GetSize(version);
        }

        /// <summary>
        /// Gets the serialized size of the banner including padding.
        /// </summary>
        /// <param name="version">Version of the banner.</param>
        /// <returns>The expected size of the binary banner.</returns>
        public static int GetSize(Version version) =>
            version switch {
                { Major: 0, Minor: 1 } => 0x0840,
                { Major: 0, Minor: 2 } => 0x0940,
                { Major: 0, Minor: 3 } => 0x0A40,
                { Major: 1, Minor: 3 } => 0x23C0,
                _ => throw new NotSupportedException(),
            };

        /// <summary>
        /// Read a banner from a binary format.
        /// </summary>
        /// <param name="source">Source binary to read from.</param>
        /// <returns>The new container with the banner.</returns>
        public NodeContainerFormat Convert(IBinary source)
        {
            source.Stream.Position = 0;
            var reader = new DataReader(source.Stream) {
                DefaultEncoding = Encoding.Unicode,
            };

            Banner banner = ReadHeader(reader);
            IndexedPaletteImage icon = ReadIcon(reader);
            ReadTitles(reader, banner);
            Node animated = ReadAnimatedIcon(reader, banner);

            var container = new NodeContainerFormat();
            container.Root.Add(new Node("info", banner));
            container.Root.Add(new Node("icon", icon));
            container.Root.Add(animated);

            return container;
        }

        private static Banner ReadHeader(DataReader reader)
        {
            var banner = new Banner();
            ushort versionData = reader.ReadUInt16();
            banner.Version = new Version(versionData >> 8, versionData & 0xFF);

            var crcGen = new NitroCrcGenerator();
            banner.ChecksumBase = reader.ReadCrc16();
            banner.ChecksumBase.Validate(crcGen.GenerateCrc16(reader.Stream, 0x20, 0x820));

            if (banner.Version.Minor > 1) {
                banner.ChecksumChinese = reader.ReadCrc16();
                banner.ChecksumChinese.Validate(crcGen.GenerateCrc16(reader.Stream, 0x20, 0x920));
            } else {
                reader.Stream.Position += 2;
            }

            if (banner.Version.Minor > 2) {
                banner.ChecksumKorean = reader.ReadCrc16();
                banner.ChecksumKorean.Validate(crcGen.GenerateCrc16(reader.Stream, 0x20, 0xA20));
            } else {
                reader.Stream.Position += 2;
            }

            if (banner.Version.Major > 0) {
                banner.ChecksumAnimatedIcon = reader.ReadCrc16();
                banner.ChecksumAnimatedIcon.Validate(crcGen.GenerateCrc16(reader.Stream, 0x1240, 0x1180));
            } else {
                reader.Stream.Position += 2;
            }

            reader.Stream.Position += 0x16; // reserved
            return banner;
        }

        private static IndexedPaletteImage ReadIcon(DataReader reader)
        {
            IndexedPixel[] pixels = reader.ReadPixels<Indexed4Bpp>(IconWidth * IconHeight);
            var swizzling = new TileSwizzling<IndexedPixel>(IconWidth);
            pixels = swizzling.Unswizzle(pixels);

            var palette = new Palette(reader.ReadColors<Bgr555>(16));

            var icon = new IndexedPaletteImage {
                Height = IconHeight,
                Width = IconWidth,
                Pixels = pixels,
            };
            icon.Palettes.Add(palette);

            return icon;
        }

        private static void ReadTitles(DataReader reader, Banner banner)
        {
            banner.JapaneseTitle = reader.ReadString(0x100).Replace("\0", string.Empty);
            banner.EnglishTitle = reader.ReadString(0x100).Replace("\0", string.Empty);
            banner.FrenchTitle = reader.ReadString(0x100).Replace("\0", string.Empty);
            banner.GermanTitle = reader.ReadString(0x100).Replace("\0", string.Empty);
            banner.ItalianTitle = reader.ReadString(0x100).Replace("\0", string.Empty);
            banner.SpanishTitle = reader.ReadString(0x100).Replace("\0", string.Empty);

            if (banner.Version.Minor > 1) {
                banner.ChineseTitle = reader.ReadString(0x100).Replace("\0", string.Empty);
            }

            if (banner.Version.Minor > 2) {
                banner.KoreanTitle = reader.ReadString(0x100).Replace("\0", string.Empty);
            }
        }

        private static Node ReadAnimatedIcon(DataReader reader, Banner banner)
        {
            Node container = NodeFactory.CreateContainer("animated");

            if (banner.Version is { Major: < 1 } or { Major: 1, Minor: < 3 }) {
                return container;
            }

            reader.Stream.Position = 0x1240;

            var swizzling = new TileSwizzling<IndexedPixel>(IconWidth);
            for (int i = 0; i < NumAnimatedImages; i++) {
                IndexedPixel[] pixels = reader.ReadPixels<Indexed4Bpp>(IconWidth * IconHeight);
                pixels = swizzling.Unswizzle(pixels);

                var bitmap = new IndexedImage(IconWidth, IconHeight, pixels);
                container.Add(new Node($"bitmap{i}", bitmap));
            }

            var palettes = new PaletteCollection();
            container.Add(new Node("palettes", palettes));
            for (int i = 0; i < NumAnimatedImages; i++) {
                var palette = new Palette(reader.ReadColors<Bgr555>(16));
                palettes.Palettes.Add(palette);
            }

            var animation = new IconAnimationSequence();
            container.Add(new Node("animation", animation));
            for (int i = 0; i < 64; i++) {
                ushort aniData = reader.ReadUInt16();
                if (aniData == 0x00) {
                    break;
                }

                var frame = new IconAnimationFrame {
                    Duration = (int)((aniData & 0xFF) * 1000.0 / 60),
                    BitmapIndex = (aniData >> 8) & 0x3,
                    PaletteIndex = (aniData >> 11) & 0x3,
                    FlipHorizontal = ((aniData >> 14) & 0x1) != 0,
                    FlipVertical = ((aniData >> 15) & 0x1) != 0,
                };
                animation.Frames.Add(frame);
            }

            return container;
        }
    }
}
