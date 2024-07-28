using System.Collections.Generic;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Phoenix.Runtime
{
	public interface ICondensedListItem
	{
		int Index { get; }

		void Serialize(IO.EndianStream s, CondensedListInfo parentListInfo);
	};

	public struct CondensedListItem8<T>
		: ICondensedListItem
		where T : class, IO.IEndianStreamSerializable, new()
	{
		public sbyte mIndex;
		public int Index { get { return this.mIndex; } }
		public T Value;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s, CondensedListInfo parentListInfo)
		{
			s.Stream(ref this.mIndex);
			if (this.Index != parentListInfo.DoneIndex)
			{
				if (s.IsReading)
					this.Value = new T();
				s.Stream(this.Value);
			}
		}
		#endregion
	};
	public struct CondensedListItemValue8<T>
		: ICondensedListItem
		where T : struct, IO.IEndianStreamSerializable
	{
		public sbyte mIndex;
		public int Index { get { return this.mIndex; } }
		public T Value;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s, CondensedListInfo parentListInfo)
		{
			s.Stream(ref this.mIndex);
			if (this.Index != parentListInfo.DoneIndex)
				s.Stream(ref this.Value);
		}
		#endregion
	};

	public struct CondensedListItem16<T>
		: ICondensedListItem
		where T : class, IO.IEndianStreamSerializable, new()
	{
		public short mIndex;
		public int Index { get { return this.mIndex; } }
		public T Value;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s, CondensedListInfo parentListInfo)
		{
			s.Stream(ref this.mIndex);
			if (this.Index != parentListInfo.DoneIndex)
			{
				if (s.IsReading)
					this.Value = new T();
				s.Stream(this.Value);
			}
		}
		#endregion
	};
	public struct CondensedListItemValue16<T>
		: ICondensedListItem
		where T : struct, IO.IEndianStreamSerializable
	{
		public short mIndex;
		public int Index { get { return this.mIndex; } }
		public T Value;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s, CondensedListInfo parentListInfo)
		{
			s.Stream(ref this.mIndex);
			if (this.Index != parentListInfo.DoneIndex)
				s.Stream(ref this.Value);
		}
		#endregion
	};

	public struct CondensedListItem32<T>
		: ICondensedListItem
		where T : class, IO.IEndianStreamSerializable, new()
	{
		public int mIndex;
		public int Index { get { return this.mIndex; } }
		public T Value;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s, CondensedListInfo parentListInfo)
		{
			s.Stream(ref this.mIndex);
			if (this.Index != parentListInfo.DoneIndex)
			{
				if (s.IsReading)
					this.Value = new T();
				s.Stream(this.Value);
			}
		}
		#endregion
	};
	public struct CondensedListItemValue32<T> : ICondensedListItem
			where T : struct, IO.IEndianStreamSerializable
	{
		public int mIndex;
		public int Index { get { return this.mIndex; } }
		public T Value;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s, CondensedListInfo parentListInfo)
		{
			s.Stream(ref this.mIndex);
			if (this.Index != parentListInfo.DoneIndex)
				s.Stream(ref this.Value);
		}
		#endregion
	};

	partial class BSaveGame
	{
		public static IO.EndianStream StreamList<T>(IO.EndianStream s, List<T> list,
			CondensedListInfo info)
			// constraining it to struct instead of 'new()' generates less complex 'new T' code and avoids boxing ops
			where T : struct, ICondensedListItem
		{
			int capacity = list.Capacity;
			info.StreamCapacity(s, ref capacity);

			if (s.IsReading)
			{
				if (info.SerializeCapacity)
				{
					Contract.Assert(capacity <= info.MaxCount);
					list.Capacity = capacity;
				}

				var item = new T();
				for (item.Serialize(s, info); item.Index != info.DoneIndex; item.Serialize(s, info))
				{
					Contract.Assert(list.Count <= info.MaxCount);
					list.Add(item);
				}
			}
			else if (s.IsWriting)
			{
				foreach (var obj in list)
					obj.Serialize(s, info);

				info.StreamDoneIndex(s);
			}

			return s;
		}

		public static IO.EndianStream StreamFreeList<T>(IO.EndianStream s, List<T> list,
			FreeListInfo info)
			where T : struct, ICondensedListItem
		{
			int capacity = list.Capacity;
			info.StreamCapacity(s, ref capacity); // highWaterMark
			int count = list.Count;
			info.StreamCapacity(s, ref count); // numAllocated

			if (s.IsReading)
			{
				Contract.Assert(capacity <= info.MaxCount && count < info.MaxCount);
				list.Capacity = capacity;

				for (int x = 0; x < count; x++)
				{
					var item = new T();
					item.Serialize(s, info);

					list.Add(item);
				}
			}
			else if (s.IsWriting)
			{
				foreach (var obj in list)
					obj.Serialize(s, info);
			}

			info.StreamSaveMarker(s);

			return s;
		}
	};
}