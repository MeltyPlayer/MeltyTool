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
using Yarhl.IO;

namespace SceneGate.Ekona.Containers.Rom
{
    /// <summary>
    /// Find the constant with the decoded size of the ARM.
    /// </summary>
    public static class ArmEncodeFieldFinder
    {
        private const int SecureAreaSize = 0x4000;

        // Number of bytes from the header to the first instruction in DecoderOps
        private const int DecoderShift = 0x0C;

        // First ARM instructions of the BLZ decoder
        private static readonly uint[] DecoderOps = [
            0xE9100006, 0xE0802002, 0xE0403C21, 0xE3C114FF,
            0xE0401001, 0xE1A04002
        ];

        /// <summary>
        /// Searchs the encoded size address.
        /// </summary>
        /// <param name="arm">The ARM file to analyze.</param>
        /// <param name="info">The information of the program.</param>
        /// <returns>The encoded size address. 0 if not found.</returns>
        public static int SearchEncodedSizeAddress(IBinary arm, ProgramInfo info)
        {
            // Steps to find the ARM9 size address that we need to change
            // in order to fix the BLZ decoded error.
            // 1º Get ARM9 entry address.
            // 2º From that point and while we're in the secure zone,
            //    search the decode_BLZ routine.
            // 3º Search previous BL (jump) instruction that call the decoder.
            // 4º Search instructions before it that loads R0 (parameter of decode_BLZ).
            DataReader reader = new DataReader(arm.Stream);

            // 1º
            uint entryAddress = info.Arm9EntryAddress - info.Arm9RamAddress;

            // 2º
            arm.Stream.Seek(entryAddress, SeekOrigin.Begin);
            uint decoderAddress = SearchDecoder(arm.Stream);
            if (decoderAddress == 0x00) {
                throw new FormatException("Invalid decoder address");
            }

            // 3º & 4º
            arm.Stream.Seek(entryAddress, SeekOrigin.Begin);
            uint baseOffset = SearchBaseOffset(arm.Stream, decoderAddress);
            if (baseOffset == 0x00) {
                throw new FormatException("Invalid base offset");
            }

            // Get relative address (not RAM address)
            arm.Stream.Seek(baseOffset, SeekOrigin.Begin);
            uint sizeAddress = reader.ReadUInt32() + 0x14; // Size is at 0x14 from that address
            sizeAddress -= info.Arm9RamAddress;

            return (int)sizeAddress;
        }

        private static uint SearchDecoder(DataStream stream)
        {
            DataReader reader = new DataReader(stream);
            long startPosition = stream.Position;

            uint decoderAddress = 0x00;
            while (stream.Position - startPosition < SecureAreaSize && decoderAddress == 0x00)
            {
                long loopPosition = stream.Position;

                // Compare instructions to see if it's the routing we want
                bool found = true;
                for (int i = 0; i < DecoderOps.Length && found; i++) {
                    if (reader.ReadUInt32() != DecoderOps[i]) {
                            found = false;
                    }
                }

                if (found) {
                    decoderAddress = (uint)loopPosition - DecoderShift; // Get start of routine
                } else {
                    stream.Seek(loopPosition + 4, SeekOrigin.Begin);  // Go to next instruction
                }
            }

            return decoderAddress;
        }

        private static uint SearchBaseOffset(DataStream stream, uint decoderAddress)
        {
            DataReader reader = new DataReader(stream);
            uint instr;

            // Search the instruction: BL DecoderAddress
            // Where DecoderAddress=(PC+8+nn*4)
            bool found = false;
            while (stream.Position < decoderAddress && !found)
            {
                instr = reader.ReadUInt32();
                if ((instr & 0xFF000000) == 0xEB000000) {
                    uint shift = instr & 0x00FFFFFF;
                    shift = 4 + (shift * 4);

                    // Check if that jump goes to the correct routine
                    if (stream.Position + shift == decoderAddress)
                        found = true;
                }
            }

            // Search for the Load instruction, btw LDR R1=[PC+ZZ].
            // Usually two instruction before.
            stream.Seek(-0x0C, SeekOrigin.Current);
            uint baseOffset = 0x00;
            instr = reader.ReadUInt32();
            if ((instr & 0xFFFF0000) == 0xE59F0000) {
                baseOffset = (uint)stream.Position + (instr & 0xFFF) + 4;
            }

            // If not found... Should we continue looking above instructions?
            // I run a test with > 500 games and at the moment it is always there
            return baseOffset;
        }
    }
}
