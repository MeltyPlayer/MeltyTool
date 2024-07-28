using System;
using System.Collections.Generic;

namespace KSoft.Phoenix.Phx
{
	public sealed class BProtoObjectVeterancy
		: IO.ITagElementStringNameStreamable
		, IComparable<BProtoObjectVeterancy>
		, IEqualityComparer<BProtoObjectVeterancy>
	{
		sealed class _EqualityComparer : IEqualityComparer<BProtoObjectVeterancy>
		{
			#region IEqualityComparer<BProtoObjectVeterancy> Members
			public bool Equals(BProtoObjectVeterancy x, BProtoObjectVeterancy y)
			{
				return x.XP == y.XP
					&& x.Damage == y.Damage
					&& x.Velocity == y.Velocity
					&& x.Accuracy == y.Accuracy
					&& x.WorkRate == y.WorkRate
					&& x.WeaponRange == y.WeaponRange
					&& x.DamageTaken == y.DamageTaken;
			}

			public int GetHashCode(BProtoObjectVeterancy obj)
			{
				return obj.XP.GetHashCode()
					^ obj.Damage.GetHashCode()
					^ obj.Velocity.GetHashCode()
					^ obj.Accuracy.GetHashCode()
					^ obj.WorkRate.GetHashCode()
					^ obj.WeaponRange.GetHashCode()
					^ obj.DamageTaken.GetHashCode();
			}
			#endregion
		};
		private static _EqualityComparer gEqualityComparer;
		public static IEqualityComparer<BProtoObjectVeterancy> EqualityComparer { get {
			if (gEqualityComparer == null)
				gEqualityComparer = new _EqualityComparer();

			return gEqualityComparer;
		} }

		#region Constants
		static readonly BProtoObjectVeterancy kInvalid = new BProtoObjectVeterancy(),
			kDefaultLevel1 = new BProtoObjectVeterancy()
			{
				mDamage = 1.15f, mVelocity = 1, mAccuracy = 1.6f, mWorkRate = 1.2f, mWeaponRange = 1f, mDamageTaken = 0.87f
			},
			kDefaultLevel2 = new BProtoObjectVeterancy()
			{
				mDamage = 1.15f, mVelocity = 1, mAccuracy = 1.7f, mWorkRate = 1.2f, mWeaponRange = 1f, mDamageTaken = 0.80f
			},
			kDefaultLevel3 = new BProtoObjectVeterancy()
			{
				mDamage = 1.15f, mVelocity = 1, mAccuracy = 1.8f, mWorkRate = 1.2f, mWeaponRange = 1f, mDamageTaken = 0.74f
			},
			kDefaultLevel4 = new BProtoObjectVeterancy()
			{
				mDamage = 2.00f, mVelocity = 1, mAccuracy = 1.1f, mWorkRate = 2.0f, mWeaponRange = 1f, mDamageTaken = 0.50f
			},
			kDefaultLevel5 = new BProtoObjectVeterancy()
			{
				mDamage = 2.00f, mVelocity = 1, mAccuracy = 1.2f, mWorkRate = 2.0f, mWeaponRange = 1f, mDamageTaken = 0.50f
			};

		public static IEnumerable<BProtoObjectVeterancy> GetLevelDefaults()
		{
			yield return kDefaultLevel1;
			yield return kDefaultLevel2;
			yield return kDefaultLevel3;
			yield return kDefaultLevel4;
			yield return kDefaultLevel5;
		}
		#endregion

		#region Xml constants
		public static readonly Collections.BListExplicitIndexParams<BProtoObjectVeterancy> kBListExplicitIndexParams = new
			Collections.BListExplicitIndexParams<BProtoObjectVeterancy>(5)
			{
				// We use a zero'd instance as the invalid format
				// Game considers Vets with XP = 0 as 'null' basically
				kTypeGetInvalid = () => kInvalid
			};
		public static readonly XML.BListExplicitIndexXmlParams<BProtoObjectVeterancy> kBListExplicitIndexXmlParams = new
			XML.BListExplicitIndexXmlParams<BProtoObjectVeterancy>("Veterancy", "Level");
		#endregion

		#region Properties
		float mXP;
		public float XP { get { return this.mXP; } }
		float mDamage = 1.0f;
		public float Damage { get { return this.mDamage; } }
		float mVelocity = 1.0f;
		public float Velocity { get { return this.mVelocity; } }
		float mAccuracy = 1.0f;
		public float Accuracy { get { return this.mAccuracy; } }
		float mWorkRate = 1.0f;
		public float WorkRate { get { return this.mWorkRate; } }
		float mWeaponRange = 1.0f;
		public float WeaponRange { get { return this.mWeaponRange; } }
		float mDamageTaken = 1.0f;
		public float DamageTaken { get { return this.mDamageTaken; } }
		#endregion

		public bool IsInvalid { get { return ReferenceEquals(this, kInvalid); } }
		public bool IsIgnored { get { return this.mXP == 0.0f; } }

		#region IComparable Members
		int IComparable<BProtoObjectVeterancy>.CompareTo(BProtoObjectVeterancy other)
		{
			if (this.XP != other.XP)
				return this.XP.CompareTo(other.XP);

			if (this.Damage != other.Damage)
				return this.Damage.CompareTo(other.Damage);

			if (this.Velocity != other.Velocity)
				return this.Velocity.CompareTo(other.Velocity);

			if (this.Accuracy != other.Accuracy)
				return this.Accuracy.CompareTo(other.Accuracy);

			if (this.WorkRate != other.WorkRate)
				return this.WorkRate.CompareTo(other.WorkRate);

			if (this.WeaponRange != other.WeaponRange)
				return this.WeaponRange.CompareTo(other.WeaponRange);

			if (this.DamageTaken != other.DamageTaken)
				return this.DamageTaken.CompareTo(other.DamageTaken);

			return 0;
		}
		#endregion

		#region IEqualityComparer<BProtoObjectVeterancy> Members
		public bool Equals(BProtoObjectVeterancy x, BProtoObjectVeterancy y)
		{
			return EqualityComparer.Equals(x, y);
		}

		public int GetHashCode(BProtoObjectVeterancy obj)
		{
			return EqualityComparer.GetHashCode(obj);
		}
		#endregion

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttributeOpt("XP", ref this.mXP, Predicates.IsNotZero);
			s.StreamAttributeOpt("Damage", ref this.mDamage, PhxPredicates.IsNotOne);
			s.StreamAttributeOpt("Velocity", ref this.mVelocity, PhxPredicates.IsNotOne);
			s.StreamAttributeOpt("Accuracy", ref this.mAccuracy, PhxPredicates.IsNotOne);
			s.StreamAttributeOpt("WorkRate", ref this.mWorkRate, PhxPredicates.IsNotOne);
			s.StreamAttributeOpt("WeaponRange", ref this.mWeaponRange, PhxPredicates.IsNotOne);
			s.StreamAttributeOpt("DamageTaken", ref this.mDamageTaken, PhxPredicates.IsNotOne);
		}
		#endregion
	};
}