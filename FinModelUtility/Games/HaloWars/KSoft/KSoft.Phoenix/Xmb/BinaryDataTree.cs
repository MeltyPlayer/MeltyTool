using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Phoenix.Xmb
{
	// #TODO refactor these APIs

	public sealed class BinaryDataTree
		: IO.IEndianStreamSerializable
	{
		public const string kBinaryFileExtension = ".binary_data_tree";
		public const string kTextFileExtension = ".binary_data_tree_xml";

		BinaryDataTreeHeader mHeader;

		internal BinaryDataTreeDecompiler Decompiler { get; private set; }
		public bool DecompileAttributesWithTypeData { get; set; }

		public bool ValidateData { get; set; } = true;

		public Shell.EndianFormat Endian
		{
			get { return this.mHeader.SignatureAsEndianFormat; }
			set { this.mHeader.SignatureAsEndianFormat = value; }
		}

		#region IEndianStreamable Members
		public void Serialize(IO.EndianStream s)
		{
			if (s.IsReading)
				this.Endian = BinaryDataTreeHeader.PeekSignatureAsEndianFormat(s.Reader);

			using (s.BeginEndianSwitch(this.Endian))
			{
				if (s.IsReading)
					this.ReadInternal(s);
				else if (s.IsWriting)
					this.WriteInternal(s);
			}
		}

	private void ReadInternal(IO.EndianStream s)
		{
			long stream_length = s.BaseStream.Length - s.BaseStream.Position;

			#region Header
			if (this.ValidateData)
			{
				if (stream_length < BinaryDataTreeHeader.kSizeOf)
					throw new InvalidDataException("Expected more bytes for header data");
			}

			this.mHeader.Serialize(s);

			if (this.ValidateData)
			{
				this.mHeader.Validate();

				long min_expected_bytes_remaining =
					BinaryDataTreeHeader.kSizeOf +
					(BinaryDataTreeSectionHeader.kSizeOf * this.mHeader.UserSectionCount);
				if (stream_length < min_expected_bytes_remaining)
					throw new InvalidDataException("Expected more bytes for header and user sections data");
			}
			#endregion

			#region Data
			long data_position = s.BaseStream.Position;
			if (this.ValidateData)
			{
				long total_size = BinaryDataTreeHeader.kSizeOf + this.mHeader.DataSize;
				if (stream_length < total_size)
					throw new InvalidDataException("Expected more bytes for header and payload data");

				uint actual_data_crc = this.GetDataCrc32(s.BaseStream);
				if (this.mHeader.DataCrc32 != actual_data_crc)
					throw new InvalidDataException(string.Format("Invalid Data CRC 0x{0}, expected 0x{1}",
						actual_data_crc.ToString("X8"),
						this.mHeader.DataCrc32.ToString("X8")));
			}
			#endregion

			#region Sections
			var section_headers = new BinaryDataTreeSectionHeader[this.mHeader.UserSectionCount];
			s.StreamArray(section_headers);

			if (this.ValidateData)
			{
				foreach (var header in section_headers)
				{
					if (stream_length < (header.Offset + header.Size))
						throw new InvalidDataException("Expected more bytes for section data");
				}
			}

			long offset_cursor = data_position;

			uint nodes_size = this.mHeader[BinaryDataTreeSectionID.NodeSectionIndex];
			long nodes_offset = nodes_size > 0
				? offset_cursor
				: 0;
			offset_cursor += nodes_size;

			uint name_values_size = this.mHeader[BinaryDataTreeSectionID.NameValueSectionIndex];
			long name_values_offset = name_values_size > 0
				? offset_cursor
				: 0;
			offset_cursor += name_values_size;

			uint name_data_size = this.mHeader[BinaryDataTreeSectionID.NameDataSectionIndex];
			long name_data_offset = name_data_size > 0
				? offset_cursor
				: 0;
			offset_cursor += name_data_size;

			if (this.mHeader[BinaryDataTreeSectionID.ValueDataSectionIndex] > 0)
				offset_cursor = IntegerMath.Align(IntegerMath.k16ByteAlignmentBit, offset_cursor);
			uint value_data_size = this.mHeader[BinaryDataTreeSectionID.ValueDataSectionIndex];
			long value_data_offset = value_data_size > 0
				? offset_cursor
				: 0;
			offset_cursor += value_data_size;

			if (this.ValidateData)
			{
				if (stream_length < offset_cursor)
					throw new InvalidDataException("Expected more bytes for section data");
			}
			#endregion

			#region Decompiler

			this.Decompiler = new BinaryDataTreeDecompiler();

			s.Seek(name_data_offset);
			this.Decompiler.NameData = new byte[name_data_size];
			s.Stream(this.Decompiler.NameData);

			s.Seek(value_data_offset);
			this.Decompiler.ValueData = new byte[value_data_size];
			s.Stream(this.Decompiler.ValueData);

			s.Seek(nodes_offset);
			this.Decompiler.PackedNodes = new BinaryDataTreePackedNode[nodes_size / BinaryDataTreePackedNode.kSizeOf];
			s.StreamArray(this.Decompiler.PackedNodes);

			s.Seek(name_values_offset);
			this.Decompiler.NameValues = new BinaryDataTreeNameValue[name_values_size / BinaryDataTreeNameValue.kSizeOf];
			s.StreamArray(this.Decompiler.NameValues);

			this.Decompiler.Decompile();
			#endregion
		}

		private void WriteInternal(IO.EndianStream s)
		{
			long headerPosition = s.BaseStream.Position;
			this.mHeader.Serialize(s);

			// #TODO

			s.Seek(headerPosition);
			this.mHeader.Serialize(s);

			throw new NotImplementedException();
		}

		private uint GetDataCrc32(Stream s)
		{
			var crc_hasher = new Security.Cryptography.CrcHash32(PhxUtil.kCrc32Definition);
			var stream_crc_hash_computer = new Security.Cryptography.StreamHashComputer<Security.Cryptography.CrcHash32>(crc_hasher, s, restorePosition: true);
			stream_crc_hash_computer.SetRangeAtCurrentOffset(this.mHeader.DataSize);
			uint actual_data_crc = stream_crc_hash_computer.Compute().Hash32;
			return actual_data_crc;
		}
		#endregion

		#region ToXml
		public XmlDocument ToXmlDocument()
		{
			if (this.Decompiler != null)
				return this.Decompiler.ToXmlDocument(this);

			throw new InvalidOperationException();
		}

		public void ToXml(string file)
		{
			Contract.Requires(!string.IsNullOrEmpty(file));

			using (var fs = File.Create(file))
			{
				this.ToXml(fs);
			}
		}
		public void ToXml(Stream stream)
		{
			Contract.Requires(stream != null);

			var doc = this.ToXmlDocument();

			var encoding = this.Decompiler.HasUnicodeStrings
				? System.Text.Encoding.UTF8
				: System.Text.Encoding.ASCII;
			var xml_writer_settings = new XmlWriterSettings()
			{
				Indent = true,
				IndentChars = "\t",
				CloseOutput = false,
				Encoding = encoding,
			};
			using (var xml = XmlWriter.Create(stream, xml_writer_settings))
			{
				doc.Save(xml);
			}
		}
		#endregion
	};

	sealed class BinaryDataTreeDecompiler
	{
		public List<BinaryDataTreeBuildNode> Nodes;
		public BinaryDataTreeBuildNode RootNode;
		public bool HasUnicodeStrings;

		public BinaryDataTreePackedNode[] PackedNodes;
		public BinaryDataTreeNameValue[] NameValues;
		public byte[] NameData;
		public byte[] ValueData;

		public IO.EndianReader NameDataReader;
		public BinaryDataTreeMemoryPool ValueDataPool;

		public void Decompile()
		{
			this.NameDataReader = new IO.EndianReader(
				new MemoryStream(this.NameData, writable: false),
				Shell.EndianFormat.Little,
				name: "NameDataReader");

			this.ValueDataPool = new BinaryDataTreeMemoryPool(this.ValueData);

			this.Nodes = new List<BinaryDataTreeBuildNode>(this.PackedNodes.Length);
			for (int x = 0; x < this.PackedNodes.Length; x++)
				this.Nodes.Add(new BinaryDataTreeBuildNode());

			for (int x = 0; x < this.PackedNodes.Length; x++)
			{
				var packed_node = this.PackedNodes[x];
				var build_node = this.Nodes[x];

				build_node.SetParent(this, packed_node);

				int num_child_nodes;
				this.CalculateChildNodesCount(x, out num_child_nodes);
				build_node.SetChildren(this, packed_node, num_child_nodes);

				int num_name_values;
				this.CalculateNameValuesCount(x, out num_name_values);
				if (num_name_values == 0)
					throw new InvalidDataException("No name-values: #" + x);
				build_node.SetNameValues(this, packed_node, num_name_values);
			}

			Util.DisposeAndNull(ref this.NameDataReader);
		}

		public string ReadName(int nameOffset)
		{
			if (this.NameData == null || nameOffset >= this.NameData.Length)
				throw new InvalidOperationException(nameOffset.ToString("X8"));

			this.NameDataReader.Seek(nameOffset);
			return this.NameDataReader.ReadString(Memory.Strings.StringStorage.CStringAscii);
		}

		private void CalculateChildNodesCount(int nodeIndex, out int numChildNodes)
		{
			var packed_node = this.PackedNodes[nodeIndex];

			numChildNodes = packed_node.ChildNodesCount;
			if (!packed_node.HasChildNodesCountOverflow)
				return;

			for (int childNodeIndex = packed_node.ChildNodeIndex; ; numChildNodes++)
			{
				if ((childNodeIndex + numChildNodes) > this.Nodes.Count)
					throw new InvalidDataException();
				else if ((childNodeIndex + numChildNodes) == this.Nodes.Count)
					break;

				var childNode = this.PackedNodes[childNodeIndex + numChildNodes];
				if (childNode.ParentIndex != nodeIndex)
					break;
			}
		}

		private void CalculateNameValuesCount(int nodeIndex, out int numNameValues)
		{
			var packed_node = this.PackedNodes[nodeIndex];

			numNameValues = packed_node.NameValuesCount;
			if (!packed_node.HasNameValuesCountOverflow)
				return;

			for (int nameValueIndex = packed_node.NameValueOffset; ; numNameValues++)
			{
				if ((nameValueIndex + numNameValues) >= numNameValues)
					throw new InvalidDataException();

				var nameValue = this.NameValues[nameValueIndex + numNameValues];
				if (nameValue.IsLastNameValue)
					break;
			}
		}

		public XmlDocument ToXmlDocument(BinaryDataTree tree)
		{
			Contract.Requires(this.RootNode != null);
			Contract.Ensures(Contract.Result<XmlDocument>() != null);

			string root_name = this.RootNode.NodeName;
			var s = IO.XmlElementStream.CreateForWrite(root_name);
			this.RootNode.ToXml(tree, s);

			return s.Document;
		}
	};
}