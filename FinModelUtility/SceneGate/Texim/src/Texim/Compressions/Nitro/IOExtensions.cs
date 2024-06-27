// Copyright (c) 2021 SceneGate

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
namespace Texim.Compressions.Nitro
{
    using System.Collections.Generic;
    using Yarhl.IO;

    public static class IOExtensions
    {
        public static MapInfo ReadMapInfo(this DataReader reader)
        {
            return new MapInfo(reader.ReadInt16());
        }

        public static MapInfo[] ReadMapInfos(this DataReader reader, int numMaps)
        {
            var infos = new MapInfo[numMaps];
            for (int i = 0; i < numMaps; i++) {
                infos[i] = new MapInfo(reader.ReadInt16());
            }

            return infos;
        }

        public static void Write(this DataWriter writer, MapInfo info)
        {
            writer.Write(info.ToInt16());
        }

        public static void Write(this DataWriter writer, IEnumerable<MapInfo> infos)
        {
            foreach (var info in infos) {
                writer.Write(info.ToInt16());
            }
        }
    }
}
