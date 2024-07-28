using System.Collections.Generic;
using System.IO;

namespace KSoft.Phoenix.Xmb
{
	public struct BinaryDataTreePackedNode
		: IO.IEndianStreamSerializable
	{
		public const int kSizeOf = 2+2+2+1+1;

		public ushort ParentIndex;// = ushort.MaxValue;
		public ushort ChildNodeIndex;// = ushort.MaxValue;
		public ushort NameValueOffset;
		public byte NameValuesCount;
		public byte ChildNodesCount;

		public bool IsRootNode { get { return this.ParentIndex == ushort.MaxValue; } }
		public bool HasNameValuesCountOverflow { get { return this.NameValuesCount == byte.MaxValue; } }
		public bool HasChildNodesCountOverflow { get { return this.ChildNodesCount == byte.MaxValue; } }

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref this.ParentIndex);
			s.Stream(ref this.ChildNodeIndex);
			s.Stream(ref this.NameValueOffset);
			s.Stream(ref this.NameValuesCount);
			s.Stream(ref this.ChildNodesCount);
		}
		#endregion
	};

	public sealed class BinaryDataTreeBuildNode
	{
		public BinaryDataTreeBuildNode Parent;
		public List<BinaryDataTreeBuildNode> Children;
		// First entry should be the element's name and text
		// Remaining entries are the attribute names and values
		public List<BinaryDataTreeBuildNameValue> NameValues;

		public string NodeName { get {
			var name_value = this.NameValues[0];
			return name_value.Name;
		} }
		public BinaryDataTreeVariantData NodeVariant { get {
			var name_value = this.NameValues[0];
			return name_value.Variant;
		} }

		internal void SetParent(BinaryDataTreeDecompiler decompiler, BinaryDataTreePackedNode packedNode)
		{
			if (packedNode.IsRootNode)
			{
				decompiler.RootNode = this;
				this.Parent = null;
			}
			else
			{
				this.Parent = decompiler.Nodes[packedNode.ParentIndex];
			}
		}

		internal void SetChildren(BinaryDataTreeDecompiler decompiler, BinaryDataTreePackedNode packedNode, int numChildNodes)
		{
			this.Children = new List<BinaryDataTreeBuildNode>(numChildNodes);
			for (int x = 0; x < numChildNodes; x++)
				this.Children.Add(decompiler.Nodes[packedNode.ChildNodeIndex + x]);
		}

		internal void SetNameValues(BinaryDataTreeDecompiler decompiler, BinaryDataTreePackedNode packedNode, int numNameValues)
		{
			this.NameValues = new List<BinaryDataTreeBuildNameValue>(numNameValues);
			for (int y = 0; y < numNameValues; y++)
				this.NameValues.Add(new BinaryDataTreeBuildNameValue());

			for (int x = 0; x < this.NameValues.Count; x++)
			{
				int packed_name_value_index = packedNode.NameValueOffset + x;
				var packed_name_value = decompiler.NameValues[packed_name_value_index];
				var build_name_value = this.NameValues[x];

				if (x == (this.NameValues.Count-1))
				{
					if (!packed_name_value.IsLastNameValue)
						throw new InvalidDataException("Expected IsLastNameValue");
				}

				build_name_value.Name = decompiler.ReadName(packed_name_value.NameOffset);
				build_name_value.Variant.Read(decompiler.ValueDataPool, packed_name_value);

				if (packed_name_value.HasUnicodeData)
					decompiler.HasUnicodeStrings = true;
			}
		}

		internal void ToXml(BinaryDataTree tree, IO.XmlElementStream s)
		{
			this.AttributesToXml(tree, s);
			this.ChildrenToXml(tree, s);
			this.InnerTextToXml(s);
		}
		void AttributesToXml(BinaryDataTree tree, IO.XmlElementStream s)
		{
			if (this.NameValues == null || this.NameValues.Count <= 1)
				return;

			if (tree.DecompileAttributesWithTypeData)
			{
				using (s.EnterCursorBookmark("Attributes"))
				{
					for (int x = 1; x < this.NameValues.Count; x++)
					{
						var name_value = this.NameValues[x];

						using (s.EnterCursorBookmark(name_value.Name))
							name_value.Variant.ToStream(s);
					}
				}
			}
			else
			{
				for (int x = 1; x < this.NameValues.Count; x++)
				{
					var name_value = this.NameValues[x];
					name_value.Variant.ToStreamAsAttribute(name_value.Name, s);
				}
			}
		}
		void ChildrenToXml(BinaryDataTree tree, IO.XmlElementStream s)
		{
			if (this.Children == null || this.Children.Count == 0)
				return;

			foreach (var child in this.Children)
			{
				using (s.EnterCursorBookmark(child.NodeName))
					child.ToXml(tree, s);
			}
		}
		void InnerTextToXml(IO.XmlElementStream s)
		{
			var inner_text_variant = this.NodeVariant;
			if (inner_text_variant.Type == BinaryDataTreeVariantType.Null)
				return;

			inner_text_variant.ToStream(s);
		}
	};
}