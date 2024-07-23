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

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using SceneGate.Ekona.Containers.Rom;
using Yarhl.IO;

namespace SceneGate.Ekona.Security;

/// <summary>
/// Twilight modcrypt encryption (AES-CTR).
/// </summary>
public class Modcrypt
{
    private readonly Aes aes;
    private readonly byte[] counter;
    private readonly byte[] key;

    /// <summary>
    /// Initializes a new instance of the <see cref="Modcrypt" /> class.
    /// </summary>
    /// <param name="iv">The initialization value.</param>
    /// <param name="key">The key.</param>
    public Modcrypt(byte[] iv, byte[] key)
    {
        aes = Aes.Create();
        aes.Mode = CipherMode.ECB;
        aes.Padding = PaddingMode.None;

        this.key = key;

        counter = new byte[16];
        Array.Copy(iv, counter, counter.Length);
    }

    /// <summary>
    /// Create a <see cref="Modcrypt" /> for a twilight program.
    /// </summary>
    /// <param name="programInfo">Information of the program to initialize encryption.</param>
    /// <param name="area">Modcrypt area 1 or 2.</param>
    /// <returns>The initialized encryption.</returns>
    /// <exception cref="ArgumentException">Area is not 1/2 or the program flags disable modcrypt.</exception>
    public static Modcrypt Create(ProgramInfo programInfo, int area)
    {
        ArgumentNullException.ThrowIfNull(programInfo);
        ArgumentNullException.ThrowIfNull(programInfo.DsiInfo);
        if (area is not 1 and not 2) {
            throw new ArgumentException("Area must be 1 or 2", nameof(area));
        }

        if (!programInfo.DsiCryptoFlags.HasFlag(DsiCryptoMode.Modcrypted)) {
            throw new ArgumentException("Program flags indicates modcrypt is disabled.");
        }

        bool useKeyDebug = programInfo.DsiCryptoFlags.HasFlag(DsiCryptoMode.ModcryptKeyDebug);
        bool devApp = programInfo.ProgramFeatures.HasFlag(DsiRomFeatures.DeveloperApp);
        if (useKeyDebug || devApp) {
            return CreateDebug(programInfo, area);
        }

        return CreateRetail(programInfo, area);
    }

    /// <summary>
    /// Encrypt or decrypt (same operation) the input into a new stream.
    /// </summary>
    /// <param name="input">The input data.</param>
    /// <returns>The decrypted or encrypted data.</returns>
    [SuppressMessage("", "S3329", Justification = "Implement must match device.")]
    public DataStream Transform(Stream input)
    {
        var output = new DataStream();

        int blockSize = aes.BlockSize / 8;
        var zeroIv = new byte[blockSize];
        using ICryptoTransform encryptor = aes.CreateEncryptor(key, zeroIv);

        byte[] buffer = new byte[blockSize];
        byte[] xorMask = new byte[blockSize];
        while (input.Position < input.Length) {
            encryptor.TransformBlock(counter, 0, counter.Length, xorMask, 0);
            IncrementCounter();

            int read = input.Read(buffer);
            for (int i = 0; i < read; i++) {
                buffer[i] ^= xorMask[blockSize - 1 - i];
            }

            output.Write(buffer, 0, read);
        }

        return output;
    }

    [SuppressMessage("", "S1117", Justification = "Static methods can't access instance fields")]
    private static Modcrypt CreateDebug(ProgramInfo programInfo, int area)
    {
        byte[] iv = (area == 1)
            ? programInfo.DsiInfo.Arm9SecureMac.Hash[0..16]
            : programInfo.DsiInfo.Arm7Mac.Hash[0..16];

        // Debug KEY[0..F]: First 16 bytes of the header
        byte[] key = new byte[16];
        Array.Copy(Encoding.ASCII.GetBytes(programInfo.GameTitle), key, 12);
        Array.Copy(Encoding.ASCII.GetBytes(programInfo.GameCode), 0, key, 12, 4);

        Array.Reverse(iv);
        Array.Reverse(key);
        return new Modcrypt(iv, key);
    }

    [SuppressMessage("", "S1117", Justification = "Static methods can't access instance fields")]
    private static Modcrypt CreateRetail(ProgramInfo programInfo, int area)
    {
        byte[] iv = (area == 1)
            ? programInfo.DsiInfo.Arm9SecureMac.Hash[0..16]
            : programInfo.DsiInfo.Arm7Mac.Hash[0..16];

        // KEY_X[0..7] = Decimal obfuscation of the forbidden fruit.
        // KEY_X[8..B] = gamecode forwards
        // KEY_X[C..F] = gamecode backwards because originality has its limits.
        byte[] keyX =
        [
            78,
            105,
            110,
            116,
            101,
            110,
            100,
            111,
            (byte)programInfo.GameCode[0],
            (byte)programInfo.GameCode[1],
            (byte)programInfo.GameCode[2],
            (byte)programInfo.GameCode[3],
            (byte)programInfo.GameCode[3],
            (byte)programInfo.GameCode[2],
            (byte)programInfo.GameCode[1],
            (byte)programInfo.GameCode[0],
        ];

        // KEY_Y[0..F]: First 16 bytes of the ARM9i SHA1-HMAC
        byte[] keyY = programInfo.DsiInfo.Arm9iMac.Hash[0..16];

        // Key = ((Key_X XOR Key_Y) + scrambler) ROL 42
        byte[] scrambler = [0x79, 0x3E, 0x4F, 0x1A, 0x5F, 0x0F, 0x68, 0x2A, 0x58, 0x02, 0x59, 0x29, 0x4E, 0xFB, 0xFE, 0xFF
        ];
        var bigX = new BigInteger(keyX);
        var bigY = new BigInteger(keyY);
        var bigScrambler = new BigInteger(scrambler);
        var keyNum = ((bigX ^ bigY) + bigScrambler) << 42;
        keyNum |= keyNum >> 128; // move to overflow 128-bits into lower part (ROL 42)
        byte[] key = keyNum.ToByteArray()[0..16]; // Get first 128-bits (like an AND)

        // Reverse everything
        Array.Reverse(iv);
        Array.Reverse(key);
        return new Modcrypt(iv, key);
    }

    private void IncrementCounter()
    {
        // Increment the count as it were a big little endian number.
        for (var i = counter.Length - 1; i >= 0; i--)
        {
            counter[i]++;

            // If it's 0, then keep iterating as we overflew this byte.
            if (counter[i] != 0)
            {
                break;
            }
        }
    }
}
