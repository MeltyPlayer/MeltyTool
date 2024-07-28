using System;
using System.Collections.Generic;

namespace KSoft.Phoenix.Phx
{
	public sealed class BWeaponModifier
		: IO.ITagElementStringNameStreamable
		, IComparable<BWeaponModifier>
		, IEqualityComparer<BWeaponModifier>
	{
		#region Xml constants
		public static readonly Collections.BTypeValuesParams<BWeaponModifier> kBListParams = new
			Collections.BTypeValuesParams<BWeaponModifier>(db => db.DamageTypes);
		public static readonly XML.BTypeValuesXmlParams<BWeaponModifier> kBListXmlParams = new
			XML.BTypeValuesXmlParams<BWeaponModifier>("DamageModifier", "type");
		#endregion

		#region Rating
		float mRating = 1.0f;
		public float Rating
		{
			get { return this.mRating; }
			set { this.mRating = value; }
		}
		#endregion

		#region DamagePercentage
		float mDamagePercentage = 1.0f;
		public float DamagePercentage
		{
			get { return this.mDamagePercentage; }
		}
		#endregion

		#region ReflectDamageFactor
		float mReflectDamageFactor;
		public float ReflectDamageFactor
		{
			get { return this.mReflectDamageFactor; }
			set { this.mReflectDamageFactor = value; }
		}
		#endregion

		#region Bowlable
		bool mBowlable;
		public bool Bowlable
		{
			get { return this.mBowlable; }
			set { this.mBowlable = value; }
		}
		#endregion

		#region Rammable
		bool mRammable;
		public bool Rammable
		{
			get { return this.mRammable; }
			set { this.mRammable = value; }
		}
		#endregion

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttributeOpt("rating", ref this.mRating, PhxPredicates.IsNotOne);
			s.StreamCursor(ref this.mDamagePercentage);
			s.StreamAttributeOpt("reflectDamageFactor", ref this.mReflectDamageFactor, Predicates.IsNotZero);
			s.StreamAttributeOpt("bowlable", ref this.mBowlable, Predicates.IsTrue);
			s.StreamAttributeOpt("rammable", ref this.mRammable, Predicates.IsTrue);
		}
		#endregion

		#region IComparable<BDamageModifier> Members
		public int CompareTo(BWeaponModifier other)
		{
			if (this.Rating != other.Rating)
				return this.Rating.CompareTo(other.Rating);

			return this.DamagePercentage.CompareTo(other.DamagePercentage);
		}
		#endregion

		#region IEqualityComparer<BDamageModifier> Members
		public bool Equals(BWeaponModifier x, BWeaponModifier y)
		{
			return x.Rating == y.Rating
				&& x.DamagePercentage == y.DamagePercentage;
		}

		public int GetHashCode(BWeaponModifier obj)
		{
			return obj.Rating.GetHashCode() ^ obj.DamagePercentage.GetHashCode();
		}
		#endregion
	};
}