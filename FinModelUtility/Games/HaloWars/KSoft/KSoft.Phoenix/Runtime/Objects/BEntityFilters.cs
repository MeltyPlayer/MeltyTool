using BEntityID = System.Int32;
using BPlayerID = System.Int32;
using BTeamID = System.Int32;
using BObjectTypeID = System.Int32;
using BProtoObjectID = System.Int32;
using BProtoSquadID = System.Int32;
using BRelationType = System.Byte;

namespace KSoft.Phoenix.Runtime
{
	abstract class BEntityFilterBase
		: IO.IEndianStreamSerializable
	{
		public byte Type;
		public bool IsInverted, AppliesToUnits, 
			AppliesToSquads, AppliesToEntities;

		#region IEndianStreamSerializable Members
		public virtual void Serialize(IO.EndianStream s)
		{
			s.Stream(ref Type);
			s.Stream(ref IsInverted); s.Stream(ref AppliesToUnits); 
			s.Stream(ref AppliesToSquads); s.Stream(ref AppliesToEntities);
		}
		#endregion

		internal static BEntityFilterBase FromType(int type)
		{
			switch (type)
			{
			case BEntityFilterIsAlive.kType: return new BEntityFilterIsAlive();
			case BEntityFilterIsIdle.kType: return new BEntityFilterIsIdle();
			case BEntityFilterEntities.kType: return new BEntityFilterEntities();
			case BEntityFilterPlayers.kType: return new BEntityFilterPlayers();
			case BEntityFilterTeams.kType: return new BEntityFilterTeams();
			case BEntityFilterProtoObjects.kType: return new BEntityFilterProtoObjects();
			case BEntityFilterProtoSquads.kType: return new BEntityFilterProtoSquads();
			case BEntityFilterObjectTypes.kType: return new BEntityFilterObjectTypes();
			case BEntityFilterRefCountTypes.kType: return new BEntityFilterRefCountTypes();
			case BEntityFilterDiplomacy.kType: return new BEntityFilterDiplomacy();
			case BEntityFilterMaxObjectType.kType: return new BEntityFilterMaxObjectType();
			case BEntityFilterIsSelected.kType: return new BEntityFilterIsSelected();
			case BEntityFilterCanChangeOwner.kType: return new BEntityFilterCanChangeOwner();
			case BEntityFilterJacking.kType: return new BEntityFilterJacking();

			default: throw new KSoft.Debug.UnreachableException(type.ToString());
			}
		}
	};
	sealed class BEntityFilterIsAlive
		: BEntityFilterBase
	{
		public const int kType = 0;
	};
	sealed class BEntityFilterIsIdle
		: BEntityFilterBase
	{
		public const int kType = 1;
	};
	sealed class BEntityFilterEntities
		: BEntityFilterBase
	{
		public const int kType = 2;

		public BEntityID[] EntityList;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			BSaveGame.StreamArray(s, ref EntityList);

			base.Serialize(s);
		}
		#endregion
	};
	sealed class BEntityFilterPlayers
		: BEntityFilterBase
	{
		public const int kType = 3;

		public BPlayerID[] Players;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			BSaveGame.StreamArray(s, ref Players);

			base.Serialize(s);
		}
		#endregion
	};
	sealed class BEntityFilterTeams
		: BEntityFilterBase
	{
		public const int kType = 4;

		public BTeamID[] Teams;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			BSaveGame.StreamArray(s, ref Teams);

			base.Serialize(s);
		}
		#endregion
	};
	sealed class BEntityFilterProtoObjects
		: BEntityFilterBase
	{
		public const int kType = 5;

		public BProtoObjectID[] ProtoObjects;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			BSaveGame.StreamArray16(s, ref ProtoObjects);

			base.Serialize(s);
		}
		#endregion
	};
	sealed class BEntityFilterProtoSquads
		: BEntityFilterBase
	{
		public const int kType = 6;

		public BProtoSquadID[] ProtoSquads;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			BSaveGame.StreamArray16(s, ref ProtoSquads);

			base.Serialize(s);
		}
		#endregion
	};
	sealed class BEntityFilterObjectTypes
		: BEntityFilterBase
	{
		public const int kType = 7;

		public BObjectTypeID[] ObjectTypes;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			BSaveGame.StreamArray16(s, ref ObjectTypes);

			base.Serialize(s);
		}
		#endregion
	};
	sealed class BEntityFilterRefCountTypes
		: BEntityFilterBase
	{
		public const int kType = 8;

		public int RefCountType, CompareType, Count;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			s.Stream(ref RefCountType); s.Stream(ref CompareType); s.Stream(ref Count);

			base.Serialize(s);
		}
		#endregion
	};
	sealed class BEntityFilterDiplomacy
		: BEntityFilterBase
	{
		public const int kType = 9;

		public BRelationType RelationType;
		public BTeamID TeamID;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			s.Stream(ref RelationType);
			s.Stream(ref TeamID);

			base.Serialize(s);
		}
		#endregion
	};
	sealed class BEntityFilterMaxObjectType
		: BEntityFilterBase
	{
		public const int kType = 10;

		public BObjectTypeID ObjectTypeID;
		public uint MaxCount;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			s.Stream(ref ObjectTypeID);
			s.Stream(ref MaxCount);

			base.Serialize(s);
		}
		#endregion
	};
	sealed class BEntityFilterIsSelected
		: BEntityFilterBase
	{
		public const int kType = 11;

		public BPlayerID PlayerID;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			s.Stream(ref PlayerID);

			base.Serialize(s);
		}
		#endregion
	};
	sealed class BEntityFilterCanChangeOwner
		: BEntityFilterBase
	{
		public const int kType = 12;
	};
	sealed class BEntityFilterJacking
		: BEntityFilterBase
	{
		public const int kType = 13;
	};

	struct BEntityFilter : IO.IEndianStreamSerializable
	{
		public byte Type;
		public BEntityFilterBase Filter;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref Type);
			int type = Type; // since this is a struct we have to declare a local copy for the lambda
			s.Stream(ref Filter,
				() => BEntityFilterBase.FromType(type));
		}
		#endregion
	};
	sealed class BEntityFilterSet
		: IO.IEndianStreamSerializable
	{
		public const int kMaxCount = 0x3E8;

		public BEntityFilter[] Filters;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			BSaveGame.StreamArray16(s, ref Filters, maxCount:kMaxCount);
		}
		#endregion
	};
}
