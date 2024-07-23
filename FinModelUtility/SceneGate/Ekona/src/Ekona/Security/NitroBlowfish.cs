// Copyright (c) 2022 SceneGate

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
#nullable enable
using System;
using System.IO;
using Yarhl.IO;

namespace SceneGate.Ekona.Security;

/// <summary>
/// Encrypt and decrypt with the blowfish based algorithm used in DS.
/// </summary>
public class NitroBlowfish
{
    private readonly uint[] keyBuffer = new uint[KeyLength / 4];

    /// <summary>
    /// Gets the required length of the key.
    /// </summary>
    public static int KeyLength => (0x12 + 0x400) * 4;

    /// <summary>
    /// Initialize the algorithm.
    /// </summary>
    /// <param name="idText">The ID code to initialize the key.</param>
    /// <param name="level">The level of key initialization.</param>
    /// <param name="modulo">The modulo to initialize the key.</param>
    /// <param name="key">The key data of 0x1048 bytes.</param>
    public void Initialize(string idText, int level, int modulo, byte[] key)
    {
        ArgumentNullException.ThrowIfNull(idText);
        ArgumentNullException.ThrowIfNull(key);
        if (idText.Length != 4)
            throw new ArgumentException("Length must be 4", nameof(idText));
        if (key.Length != KeyLength)
            throw new ArgumentException($"Length must be {KeyLength}", nameof(key));
        if (modulo is not 4 and not 8 and not 12)
            throw new ArgumentOutOfRangeException(nameof(modulo), "Must be 4, 8 or 12");
        if (level is < 1 or > 3)
            throw new ArgumentOutOfRangeException(nameof(level), "Must be 1, 2 or 3");

        for (int i = 0; i < keyBuffer.Length; i++) {
            keyBuffer[i] = BitConverter.ToUInt32(key, i * 4);
        }

        uint idCode = (uint)(((byte)idText[3] << 24)
            | ((byte)idText[2] << 16)
            | ((byte)idText[1] << 8)
            | ((byte)idText[0] << 0));
        uint[] keyCode = [idCode, idCode >> 1, idCode << 1];

        ApplyKeyCode(modulo, keyCode); // level 1 (always)
        if (level >= 2) {
            ApplyKeyCode(modulo, keyCode);
        }

        keyCode[1] <<= 1;
        keyCode[2] >>= 1;

        if (level >= 3) {
            ApplyKeyCode(modulo, keyCode);
        }
    }

    /// <summary>
    /// Encrypt a 64-bits value as two pair of 64-bits.
    /// </summary>
    /// <param name="data0">The first pair of 32-bits to encrypt and store result.</param>
    /// <param name="data1">The second pair of 32-bits to encrypt and store result.</param>
    public void Encrypt(ref uint data0, ref uint data1)
    {
        uint x = data1;
        uint y = data0;

        for (int i = 0; i < 0x10; i++) {
            uint z = keyBuffer[i] ^ x;
            x = GetMixer(z) ^ y;
            y = z;
        }

        data0 = x ^ keyBuffer[0x10];
        data1 = y ^ keyBuffer[0x11];
    }

    /// <summary>
    /// Encrypt a complete stream over itself.
    /// </summary>
    /// <param name="stream">The stream with the decrypted the data that will be overwritten encrypted.</param>
    public void Encrypt(Stream stream) => StreamEncryption(true, stream);

    /// <summary>
    /// Encrypt an array of bytes.
    /// </summary>
    /// <param name="data">Data to encrypt.</param>
    /// <returns>A new array with the encrypted data.</returns>
    public byte[] Encrypt(ReadOnlySpan<byte> data) => ArrayEncryption(true, data);

    /// <summary>
    /// Decrypt a 64-bits value as two pair of 32-bits.
    /// </summary>
    /// <param name="data0">The first pair of 32-bits to decrypt and store result.</param>
    /// <param name="data1">The second pair of 32-bits to decrypt and store result.</param>
    public void Decrypt(ref uint data0, ref uint data1)
    {
        uint x = data1;
        uint y = data0;

        for (int i = 0x11; i >= 0x2; i--) {
            uint z = keyBuffer[i] ^ x;
            x = GetMixer(z) ^ y;
            y = z;
        }

        data0 = x ^ keyBuffer[0x01];
        data1 = y ^ keyBuffer[0x00];
    }

    /// <summary>
    /// Decrypt a complete stream over itself.
    /// </summary>
    /// <param name="stream">The stream with the decrypted the data that will be overwritten encrypted.</param>
    public void Decrypt(Stream stream) => StreamEncryption(false, stream);

    /// <summary>
    /// Decrypt an array of bytes.
    /// </summary>
    /// <param name="data">Data to decrypt.</param>
    /// <returns>A new array with the decrypted data.</returns>
    public byte[] Decrypt(ReadOnlySpan<byte> data) => ArrayEncryption(false, data);

    private void StreamEncryption(bool encrypt, Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        var reader = new DataReader(stream);
        var writer = new DataWriter(stream);

        stream.Position = 0;
        while (stream.Position + 8 <= stream.Length) {
            uint data0 = reader.ReadUInt32();
            uint data1 = reader.ReadUInt32();

            if (encrypt) {
                Encrypt(ref data0, ref data1);
            } else {
                Decrypt(ref data0, ref data1);
            }

            stream.Position -= 8;
            writer.Write(data0);
            writer.Write(data1);
        }
    }

    private byte[] ArrayEncryption(bool encrypt, ReadOnlySpan<byte> data)
    {
        byte[] output = new byte[data.Length];
        for (int i = 0; i + 8 <= data.Length; i += 8) {
            uint data0 = BitConverter.ToUInt32(data.Slice(i, 4));
            uint data1 = BitConverter.ToUInt32(data.Slice(i + 4, 4));

            if (encrypt) {
                Encrypt(ref data0, ref data1);
            } else {
                Decrypt(ref data0, ref data1);
            }

            output[i] = (byte)data0;
            output[i + 1] = (byte)(data0 >> 8);
            output[i + 2] = (byte)(data0 >> 16);
            output[i + 3] = (byte)(data0 >> 24);

            output[i + 4] = (byte)data1;
            output[i + 5] = (byte)(data1 >> 8);
            output[i + 6] = (byte)(data1 >> 16);
            output[i + 7] = (byte)(data1 >> 24);
        }

        return output;
    }

    private void ApplyKeyCode(int modulo, uint[] keyCode)
    {
        uint ReverseEndianness(uint value) =>
            (uint)((byte)(value >> 24)
                | ((byte)(value >> 16) << 8)
                | ((byte)(value >> 8) << 16)
                | ((byte)value << 24));

        Encrypt(ref keyCode[1], ref keyCode[2]);
        Encrypt(ref keyCode[0], ref keyCode[1]);

        modulo /= 4;
        for (int i = 0; i <= 0x44 / 4; i++) {
            uint xorValue = ReverseEndianness(keyCode[i % modulo]);
            keyBuffer[i] ^= xorValue;
        }

        uint scratch0 = 0;
        uint scratch1 = 0;
        for (int i = 0; i <= 0x1040 / 4; i += 2) {
            Encrypt(ref scratch0, ref scratch1);
            keyBuffer[i] = scratch1;
            keyBuffer[i + 1] = scratch0;
        }
    }

    private uint GetMixer(uint index)
    {
        uint value = keyBuffer[0x12 + ((index >> 24) & 0xFF)];
        value += keyBuffer[0x112 + ((index >> 16) & 0xFF)];
        value ^= keyBuffer[0x212 + ((index >> 8) & 0xFF)];
        value += keyBuffer[0x312 + (index & 0xFF)];
        return value;
    }
}
