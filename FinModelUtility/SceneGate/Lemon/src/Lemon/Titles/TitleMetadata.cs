// Copyright (c) 2020 SceneGate

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
namespace SceneGate.Lemon.Titles
{
    using System.Collections.ObjectModel;
    using Yarhl.FileFormat;

    /// <summary>
    /// E-shop title metadata.
    /// </summary>
    public class TitleMetadata : IFormat
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TitleMetadata" /> class.
        /// </summary>
        public TitleMetadata()
        {
            InfoRecords = [];
            Chunks = [];
        }

        /// <summary>
        /// Gets or sets the signature type.
        /// </summary>
        public uint SignType { get; set; }

        /// <summary>
        /// Gets or sets the signature.
        /// </summary>
        public byte[] Signature { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the title metadata has a
        /// valid signature.
        /// </summary>
        public bool ValidSignature { get; set; }

        /// <summary>
        /// Gets or sets the signature issuer.
        /// </summary>
        public string SignatureIssuer { get; set; }

        /// <summary>
        /// Gets or sets the version of the title metadata.
        /// </summary>
        public byte Version { get; set; }

        /// <summary>
        /// Gets or sets the version of the CA CRL.
        /// </summary>
        public byte CaCrlVersion { get; set; }

        /// <summary>
        /// Gets or sets the version of the signer CRL.
        /// </summary>
        public byte SignerCrlVersion { get; set; }

        /// <summary>
        /// Gets or sets the system's version.
        /// </summary>
        public long SystemVersion { get; set; }

        /// <summary>
        /// Gets or sets the ID of the title.
        /// </summary>
        public long TitleId { get; set; }

        /// <summary>
        /// Gets or sets the type of title.
        /// </summary>
        public int TitleType { get; set; }

        /// <summary>
        /// Gets or sets the ID of the group.
        /// </summary>
        public short GroupId { get; set; }

        /// <summary>
        /// Gets or sets the size of the save file data.
        /// </summary>
        public int SaveSize { get; set; }

        /// <summary>
        /// Gets or sets the size of the private data of a SRL (DSi) save file.
        /// </summary>
        public int SrlPrivateSaveSize { get; set; }

        /// <summary>
        /// Gets or sets the SRL (DSi) flags.
        /// </summary>
        public byte SrlFlag { get; set; }

        /// <summary>
        /// Gets or sets the access rights.
        /// </summary>
        public int AccessRights { get; set; }

        /// <summary>
        /// Gets or sets the version of the title.
        /// </summary>
        public short TitleVersion { get; set; }

        /// <summary>
        /// Gets or sets the index of the bootable content.
        /// </summary>
        public int BootContent { get; set; }

        /// <summary>
        /// Gets or sets the hash of the Content Info Records.
        /// </summary>
        public byte[] Hash { get; set; }

        /// <summary>
        /// Gets a collection of content info records.
        /// </summary>
        public Collection<ContentInfoRecord> InfoRecords { get; private set; }

        /// <summary>
        /// Gets a collection of content chunks.
        /// </summary>
        public Collection<ContentChunkRecord> Chunks { get; private set; }
    }
}
