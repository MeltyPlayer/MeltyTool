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

using Yarhl.FileSystem;

namespace SceneGate.Ekona.Containers.Rom
{
    /// <summary>
    /// NDS cartridge file system.
    /// </summary>
    /// <remarks>
    /// <para>The container hierarchy is:</para>
    /// <list type="table">
    /// <listheader><term>Node path</term><description>Description</description></listheader>
    /// <item><term>/system</term><description>ROM information and program files.</description></item>
    /// <item><term>/system/info</term><description>Program information.</description></item>
    /// <item><term>/system/copyright_logo</term><description>Copyright logo.</description></item>
    /// <item><term>/system/banner/</term><description>Program banner.</description></item>
    /// <item><term>/system/banner/info</term><description>Program banner content.</description></item>
    /// <item><term>/system/banner/icon</term><description>Program icon.</description></item>
    /// <item><term>/system/banner/animated</term><description>Animated program icon.</description></item>
    /// <item><term>/system/banner/animated/bitmapX</term><description>Bitmap X (0-7) for the animated icon.</description></item>
    /// <item><term>/system/banner/animated/palettes</term><description>Palettes (0-7) for the animated icon.</description></item>
    /// <item><term>/system/banner/animated/animation</term><description>Animation icon information.</description></item>
    /// <item><term>/system/arm9</term><description>Program executable for ARM9 CPU.</description></item>
    /// <item><term>/system/overlays9</term><description>Overlay libraries for ARM9 CPU.</description></item>
    /// <item><term>/system/overlays9/overlay_X</term><description>Overlay X for ARM9 CPU.</description></item>
    /// <item><term>/system/arm7</term><description>Program executable for ARM7 CPU.</description></item>
    /// <item><term>/system/overlays7</term><description>Overlay libraries for ARM7 CPU.</description></item>
    /// <item><term>/system/overlays7/overlay7_X</term><description>Overlay X for ARM7 CPU.</description></item>
    /// <item><term>/data</term><description>Program data files.</description></item>
    /// </list>
    /// </remarks>
    public class NitroRom : NodeContainerFormat
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NitroRom"/> class.
        /// </summary>
        public NitroRom()
        {
            Node system = NodeFactory.CreateContainer("system");
            system.Add(new Node("info", new ProgramInfo()));
            system.Add(new Node("copyright_logo"));
            system.Add(NodeFactory.CreateContainer("banner"));
            system.Add(new Node("arm9"));
            system.Add(NodeFactory.CreateContainer("overlays9"));
            system.Add(new Node("arm7"));
            system.Add(NodeFactory.CreateContainer("overlays7"));
            Root.Add(system);

            Node data = NodeFactory.CreateContainer("data");
            Root.Add(data);
        }

        /// <summary>
        /// Gets the Nitro constant in little endian: 2-10-6 (NiToRo in Japanese numbers) + 0xCODE.
        /// It's a marker for the program code to find constants.
        /// </summary>
        public static uint NitroCode => 0xDEC00621;

        /// <summary>
        /// Gets the container with the system files of the program.
        /// </summary>
        public Node System => Root.Children["system"];

        /// <summary>
        /// Gets the container with the program data files.
        /// </summary>
        public Node Data => Root.Children["data"];

        /// <summary>
        /// Gets the information of the program.
        /// </summary>
        public ProgramInfo Information => System?.Children["info"]?.GetFormatAs<ProgramInfo>();

        /// <summary>
        /// Gets the banner of the program.
        /// </summary>
        public Banner Banner => System?.Children["banner"]?.Children["info"]?.GetFormatAs<Banner>();
    }
}
